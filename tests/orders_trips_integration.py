"""Real HTTP/MySQL checks. Run against a local development API and uhiltaxi-mysql.

Creates uniquely named fixtures and removes only those fixtures in finally.
Uses the container's existing database authentication; never prints tokens or passwords.
"""
import concurrent.futures
import json
import os
import secrets
import subprocess
import urllib.error
import urllib.request
import uuid
from datetime import datetime, timedelta, timezone
from decimal import Decimal

BASE = os.environ.get("UHILTAXI_TEST_URL", "http://127.0.0.1:8080")
CONTAINER = os.environ.get("UHILTAXI_MYSQL_CONTAINER", "uhiltaxi-mysql")
TAG = "orders-trips-" + uuid.uuid4().hex[:16]
PROMO = "OT" + uuid.uuid4().hex[:12].upper()
checks = 0


def check(condition, message):
    global checks
    if not condition:
        raise AssertionError(message)
    checks += 1


def sql(statement):
    return subprocess.run(
        ["docker", "exec", "-i", CONTAINER, "sh", "-c",
         'MYSQL_PWD="$MYSQL_ROOT_PASSWORD" exec mysql -uroot --batch --skip-column-names "$MYSQL_DATABASE"'],
        input=statement, text=True, capture_output=True, check=True,
    ).stdout.strip()


def request(method, path, token=None, body=None):
    headers = {"Content-Type": "application/json"}
    if token:
        headers["Authorization"] = "Bearer " + token
    req = urllib.request.Request(BASE + path, data=json.dumps(body).encode() if body is not None else None,
                                 headers=headers, method=method)
    try:
        response = urllib.request.urlopen(req, timeout=20)
    except urllib.error.HTTPError as error:
        response = error
    payload = response.read()
    return response.status, json.loads(payload) if payload else None


def api(method, path, token=None, body=None, expected=200, code=None):
    status, result = request(method, path, token, body)
    error_code = result.get("code") if isinstance(result, dict) else None
    check(status == expected, f"{method} {path}: expected {expected}, got {status}; code={error_code}")
    if code:
        check(result.get("code") == code, f"{path}: wrong ProblemDetails code")
    return result


def account(name, role="client"):
    password = secrets.token_urlsafe(20)
    phone = "+3809" + str(secrets.randbelow(900000000) + 100000000)
    registered = api("POST", "/api/v1/auth/register", body={
        "first_name": name, "last_name": "Integration", "phone": phone,
        "email": f"{name}.{TAG}@example.test", "password": password, "birth_date": None,
    }, expected=201)
    user_id = registered["user"]["id"]
    if role != "client":
        sql(f"DELETE FROM client_profiles WHERE user_id={user_id}; UPDATE users SET role='{role}' WHERE id={user_id};")
        if role == "driver":
            sql(f"INSERT INTO driver_profiles(user_id,license_number,hire_date) VALUES ({user_id},'OT{user_id}',UTC_DATE());")
        registered = api("POST", "/api/v1/auth/login", body={"phone": phone, "password": password})
    return user_id, registered["access_token"]


def cleanup():
    # TAG is generated locally and consists exclusively of alphanumerics/hyphens.
    ids = sql(f"SELECT id FROM users WHERE email LIKE '%.{TAG}@example.test';").splitlines()
    if ids:
        joined = ",".join(str(int(value)) for value in ids)
        sql(f"""
            START TRANSACTION;
            DELETE h FROM order_status_history h JOIN orders o ON o.id=h.order_id WHERE o.client_id IN ({joined});
            DELETE p FROM promocode_usages p JOIN orders o ON o.id=p.order_id WHERE o.client_id IN ({joined});
            DELETE t FROM trips t JOIN orders o ON o.id=t.order_id WHERE o.client_id IN ({joined});
            DELETE FROM orders WHERE client_id IN ({joined});
            DELETE FROM shifts WHERE driver_id IN ({joined});
            DELETE FROM audit_logs WHERE user_id IN ({joined});
            DELETE FROM refresh_tokens WHERE user_id IN ({joined});
            DELETE FROM driver_profiles WHERE user_id IN ({joined});
            DELETE FROM client_profiles WHERE user_id IN ({joined});
            DELETE FROM users WHERE id IN ({joined});
            COMMIT;
        """)
    sql(f"""
        DELETE c FROM cars c JOIN car_models m ON m.id=c.model_id WHERE m.model_name='{TAG}';
        DELETE FROM car_models WHERE model_name='{TAG}';
        DELETE FROM tariffs WHERE name='{TAG}';
        DELETE FROM promocodes WHERE code='{PROMO}';
    """)


def main():
    api("GET", "/health")
    swagger = api("GET", "/swagger/v1/swagger.json")
    check("/api/v1/driver/orders/{id}/complete" in swagger["paths"], "OpenAPI must expose trip commands")
    client_id, client = account("client")
    _, other = account("other")
    driver1_id, driver1 = account("driver1", "driver")
    driver2_id, driver2 = account("driver2", "driver")
    driver3_id, driver3 = account("driver3", "driver")
    admin_id, admin = account("admin", "admin")
    tariff = int(sql(f"INSERT INTO tariffs(name,service_class,base_fare,rate_per_km,rate_per_min) VALUES('{TAG}','standard',50,10,2); SELECT LAST_INSERT_ID();"))
    promo = api("POST", "/api/v1/admin/promocodes", admin, {
        "code": PROMO.lower(), "discount_value": 10, "discount_type": "percentage",
        "expiry_date": (datetime.now(timezone.utc).date() + timedelta(days=1)).isoformat(),
        "max_uses": 1, "min_order_amount": None, "max_discount_amount": None,
    }, expected=201)
    check(promo["code"] == PROMO and promo["discount_type"] == "percentage", "Promocode must normalize code and persist the enum as a lowercase string")
    check(promo["is_active"] and promo["created_at"].startswith(str(datetime.now(timezone.utc).year)), "New promocodes must be active with initialized timestamps")
    promo_id = promo["id"]
    api("GET", f"/api/v1/admin/promocodes/{promo_id}", admin)
    updated = api("PATCH", f"/api/v1/admin/promocodes/{promo_id}", admin, {"discount_type": "fixed", "discount_value": 15})
    check(updated["discount_type"] == "fixed", "Promocode update must accept the fixed enum value")
    api("PATCH", f"/api/v1/admin/promocodes/{promo_id}", admin, {"discount_type": "percentage", "discount_value": 10})
    shifts = {}
    for driver_id, category in [(driver1_id, "standard"), (driver2_id, "standard"), (driver3_id, "economy")]:
        model = int(sql(f"INSERT INTO car_models(brand,model_name,category,fuel_type) VALUES('OT{driver_id}','{TAG}','{category}','petrol'); SELECT LAST_INSERT_ID();"))
        vin = uuid.uuid4().hex[:17].upper()
        car = int(sql(f"INSERT INTO cars(model_id,license_plate,vin_code,year,color) VALUES({model},'OT{driver_id}','{vin}',2025,'white'); SELECT LAST_INSERT_ID();"))
        shifts[driver_id] = int(sql(f"INSERT INTO shifts(driver_id,car_id,start_time,start_mileage) VALUES({driver_id},{car},DATE_SUB(UTC_TIMESTAMP(),INTERVAL 1 HOUR),0); SELECT LAST_INSERT_ID();"))
    order_body = {"tariff_id": tariff, "pickup": {"address": "Pickup", "lat": 49.42, "lng": 26.98},
                  "destination": {"address": "Destination", "lat": 49.40, "lng": 27.01}}
    api("GET", "/api/v1/orders", expected=401)
    api("POST", "/api/v1/orders", driver1, order_body, expected=403)
    sql(f"UPDATE users SET status='blocked' WHERE id={client_id};")
    api("GET", "/api/v1/orders", client, expected=403)
    sql(f"UPDATE users SET status='active' WHERE id={client_id};")
    api("POST", "/api/v1/orders", client, {**order_body, "destination": order_body["pickup"]}, expected=400)
    api("POST", "/api/v1/orders", client, {**order_body, "pickup": {"address": "Pickup", "lat": 91, "lng": 26}}, expected=400)
    api("POST", "/api/v1/orders", client, {**order_body, "pickup": {"address": "Pickup", "lng": 26}}, expected=400)
    quote = api("POST", "/api/v1/orders/estimate", client, {**order_body, "promocode": PROMO})
    order = api("POST", "/api/v1/orders", client, {**order_body, "promocode": PROMO, "client_id": 999999}, expected=201)
    oid = order["id"]
    check(order["client_id"] == client_id, "Client identity must come from JWT")
    check(order["estimated_fare"] == quote["estimated_fare"], "Order must persist the server quote")
    api("GET", f"/api/v1/orders/{oid}", other, expected=404)
    api("POST", f"/api/v1/orders/{oid}/cancel", other, {"reason": "forbidden"}, expected=404)
    check(any(o["id"] == oid for o in api("GET", "/api/v1/driver/orders/available", driver2)["data"]), "Unassigned order must be available")
    assigned = api("POST", f"/api/v1/admin/orders/{oid}/assign-driver", admin, {"driver_id": driver1_id})
    check(assigned["status"] == "pending", "Assignment must retain pending state")
    check(all(o["id"] != oid for o in api("GET", "/api/v1/driver/orders/available", driver2)["data"]), "Assigned order must be hidden from other drivers")
    api("POST", f"/api/v1/driver/orders/{oid}/accept", driver2, expected=409, code="ORDER_NOT_PENDING")
    api("POST", f"/api/v1/driver/orders/{oid}/accept", driver1)
    api("POST", f"/api/v1/driver/orders/{oid}/start", driver1, {"shift_id": shifts[driver1_id]}, expected=409)
    api("POST", f"/api/v1/driver/orders/{oid}/arrived", driver2, expected=404)
    api("POST", f"/api/v1/driver/orders/{oid}/arrived", driver1)
    api("POST", f"/api/v1/driver/orders/{oid}/start", driver1, {"shift_id": shifts[driver2_id]}, expected=409)
    trip = api("POST", f"/api/v1/driver/orders/{oid}/start", driver1, {"shift_id": shifts[driver1_id]}, expected=201)
    tid = trip["id"]
    api("GET", f"/api/v1/trips/{tid}", other, expected=404)
    api("GET", f"/api/v1/driver/trips/{tid}", driver2, expected=404)
    api("POST", f"/api/v1/driver/orders/{oid}/start", driver1, {"shift_id": shifts[driver1_id]}, expected=409)
    check(sql(f"SELECT COUNT(*) FROM trips WHERE order_id={oid};") == "1", "Duplicate start must not create a second trip")
    sql(f"UPDATE tariffs SET base_fare=999,rate_per_km=999,rate_per_min=999 WHERE id={tariff};")
    api("POST", f"/api/v1/driver/orders/{oid}/complete", driver1, {"distance_km": -1}, expected=400)
    api("POST", f"/api/v1/driver/orders/{oid}/complete", driver1, {"distance_km": 1.001}, expected=400)
    api("POST", f"/api/v1/orders/{oid}/cancel", client, {"reason": "too late"}, expected=409)
    api("POST", f"/api/v1/driver/orders/{oid}/complete", driver2, {"distance_km": 2.5}, expected=404)
    completed = api("POST", f"/api/v1/driver/orders/{oid}/complete", driver1, {"distance_km": 2.5})
    amount = Decimal(50) + Decimal(10) * Decimal("2.5") + Decimal(2) * completed["duration_min"]
    expected = (amount * Decimal("0.90")).quantize(Decimal("0.01"))
    check(Decimal(str(completed["final_fare"])) == expected, "Actual fare must use tariff snapshot and promo")
    check(completed["actual_end_time"].endswith("Z"), "API timestamps must be UTC")
    api("POST", f"/api/v1/driver/orders/{oid}/complete", driver1, {"distance_km": 2.5}, expected=409)
    api("POST", f"/api/v1/orders/{oid}/cancel", client, {"reason": "too late"}, expected=409)
    history = api("GET", f"/api/v1/orders/{oid}/history", client)
    check([h["to_status"] for h in history] == ["pending", "pending", "accepted", "driver_arriving", "in_progress", "completed"], "History must match the committed lifecycle")
    api("POST", "/api/v1/orders", client, {**order_body, "promocode": PROMO}, expected=409, code="PROMOCODE_EXHAUSTED")
    check(sql(f"SELECT COUNT(*) FROM promocode_usages p JOIN orders o ON o.id=p.order_id WHERE o.id={oid};") == "1", "Promo must be reserved once")
    sql(f"UPDATE tariffs SET base_fare=50,rate_per_km=10,rate_per_min=2 WHERE id={tariff};")
    cancelled = api("POST", "/api/v1/orders", client, order_body, expected=201)["id"]
    api("POST", f"/api/v1/orders/{cancelled}/cancel", client, {"reason": "changed plans"})
    api("POST", f"/api/v1/orders/{cancelled}/cancel", client, {"reason": "again"}, expected=409)
    race_order = api("POST", "/api/v1/orders", client, order_body, expected=201)["id"]
    with concurrent.futures.ThreadPoolExecutor(2) as pool:
        outcomes = list(pool.map(lambda token: request("POST", f"/api/v1/driver/orders/{race_order}/accept", token)[0], [driver1, driver2]))
    check(sorted(outcomes) == [200, 409], "Two drivers accepting one order must have one winner")
    check(sql(f"SELECT COUNT(*) FROM order_status_history WHERE order_id={race_order} AND to_status='accepted';") == "1", "Race must commit one acceptance history row")
    api("POST", f"/api/v1/orders/{race_order}/cancel", client, {"reason": "race fixture"})
    two_orders = [api("POST", "/api/v1/orders", client, order_body, expected=201)["id"] for _ in range(2)]
    with concurrent.futures.ThreadPoolExecutor(2) as pool:
        outcomes = list(pool.map(lambda order_id: request("POST", f"/api/v1/driver/orders/{order_id}/accept", driver1)[0], two_orders))
    check(sorted(outcomes) == [200, 409], "One driver must not accept two active orders concurrently")
    for order_id in two_orders:
        api("POST", f"/api/v1/orders/{order_id}/cancel", client, {"reason": "race fixture"})
    wrong_class = api("POST", "/api/v1/orders", client, order_body, expected=201)["id"]
    api("POST", f"/api/v1/driver/orders/{wrong_class}/accept", driver3, expected=409, code="CAR_CLASS_MISMATCH")
    sql(f"UPDATE shifts SET status='closed',end_time=UTC_TIMESTAMP(),end_mileage=0 WHERE driver_id={driver3_id};")
    api("POST", f"/api/v1/driver/orders/{wrong_class}/accept", driver3, expected=409, code="OPEN_SHIFT_REQUIRED")
    page = api("GET", "/api/v1/admin/orders?page=1&limit=2&status=cancelled", admin)
    check(len(page["data"]) == 2 and page["pagination"]["total"] >= 4, "Pagination/filter must report full total")
    api("GET", "/api/v1/orders?page=0", client, expected=400)
    api("GET", "/api/v1/orders?sort=untrusted_column", client, expected=400)
    api("GET", "/api/v1/orders?status=unknown", client, expected=400)
    check(any(t["id"] == tid for t in api("GET", "/api/v1/driver/trips", driver1)["data"]), "Driver history must include trip")
    api("GET", f"/api/v1/admin/trips/{tid}", admin)
    managed = api("POST", "/api/v1/admin/orders", admin, {"client_id": client_id, "order": order_body}, expected=201)["id"]
    api("POST", f"/api/v1/admin/orders/{managed}/accept", client, {"driver_id": driver1_id}, expected=403)
    api("POST", f"/api/v1/admin/orders/{managed}/accept", admin, {"driver_id": driver1_id})
    api("POST", f"/api/v1/admin/orders/{managed}/arrived", admin)
    api("POST", f"/api/v1/admin/orders/{managed}/start", admin, {"shift_id": shifts[driver1_id]}, expected=201)
    api("POST", f"/api/v1/admin/orders/{managed}/complete", admin, {"distance_km": 1})
    managed_history = api("GET", f"/api/v1/admin/orders/{managed}/history", admin)
    check(all(h["changed_by_user_id"] == admin_id for h in managed_history), "Admin commands must record the admin actor")
    check(sql(f"SELECT COUNT(*) FROM audit_logs WHERE user_id={admin_id} AND entity_id={managed};") == "5", "Admin lifecycle must commit all audit records")
    api("POST", f"/api/v1/admin/orders/{wrong_class}/cancel", admin, {"reason": "admin fixture cleanup"})
    check(sql(f"SELECT COUNT(*) FROM audit_logs WHERE user_id={admin_id} AND entity_id={wrong_class} AND action='order.cancel';") == "1", "Admin cancellation must be audited")
    print(f"Passed {checks} HTTP/MySQL integration checks; lifecycle, RBAC, pricing, pagination, races, and admin audit verified.")


if __name__ == "__main__":
    try:
        main()
    finally:
        cleanup()
