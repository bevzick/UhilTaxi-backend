"""Exercise the real ASP.NET CORS pipeline without a database or an existing API.

Build the API first, then run: python3 tests/cors_integration.py
Requires dotnet on PATH. Each test host uses an ephemeral port and signing key.
"""
import json
import os
from pathlib import Path
import secrets
import socket
import subprocess
import tempfile
import time
import urllib.error
import urllib.request

ROOT = Path(__file__).resolve().parents[1]
API = ROOT / "src/UhilTaxi.Api"
DLL = API / "bin/Debug/net10.0/UhilTaxi.Api.dll"
checks = 0


def check(condition, message):
    global checks
    assert condition, message
    checks += 1


def request(base, path, origin=None, method="GET", body=None, preflight=False):
    headers = {"Origin": origin} if origin else {}
    if preflight:
        headers.update({"Access-Control-Request-Method": "POST",
                        "Access-Control-Request-Headers": "content-type,authorization"})
    if body is not None:
        headers["Content-Type"] = "application/json"
    req = urllib.request.Request(base + path, headers=headers, method=method,
                                 data=json.dumps(body).encode() if body is not None else None)
    try:
        return urllib.request.urlopen(req, timeout=2)
    except urllib.error.HTTPError as error:
        return error


def scenario(environment, allowed=None):
    with socket.socket() as sock:
        sock.bind(("127.0.0.1", 0))
        port = sock.getsockname()[1]
    base = f"http://127.0.0.1:{port}"
    env = os.environ.copy()
    env.update({"ASPNETCORE_ENVIRONMENT": environment, "ASPNETCORE_URLS": base,
                "Jwt__Key": secrets.token_urlsafe(48), "AdminSeed__Enabled": "false",
                "ConnectionStrings__Default": "Server=127.0.0.1;Database=unused;User=unused;Password=unused;"})
    if allowed is not None:
        env["Cors__AllowedOrigins"] = allowed
    else:
        env.pop("Cors__AllowedOrigins", None)
    with tempfile.TemporaryFile() as log:
        process = subprocess.Popen(["dotnet", str(DLL)], cwd=API, env=env,
                                   stdout=log, stderr=subprocess.STDOUT)
        try:
            for _ in range(100):
                if process.poll() is not None:
                    raise AssertionError("Test API exited before becoming ready")
                try:
                    # OPTIONS is handled before production HTTPS redirection.
                    with request(base, "/api/v1/auth/login", "http://localhost:5173",
                                 "OPTIONS", preflight=True):
                        break
                except urllib.error.URLError:
                    time.sleep(0.1)
            else:
                raise AssertionError("Test API did not become ready")

            origins = ("http://localhost:5173", "http://127.0.0.1:5173",
                       "https://frontend.example.test", "https://untrusted.example.test",
                       "http://localhost:5174", "null")
            expected = set(allowed.split(',')) if allowed is not None else (
                set(origins[:2]) if environment == "Development" else set())
            for origin in origins:
                for path in ("/api/v1/auth/login", "/api/v1/auth/register", "/api/v1/me"):
                    with request(base, path, origin, "OPTIONS", preflight=True) as response:
                        check(response.status == 204, "Preflight must bypass authorization and HTTPS redirection")
                        check(response.headers.get("Access-Control-Allow-Origin") ==
                              (origin if origin in expected else None), f"Unexpected origin permission: {origin}")
                        if origin in expected:
                            check(response.headers.get("Access-Control-Allow-Credentials") == "true", "Cookies must be permitted")
                            check("POST" in response.headers.get("Access-Control-Allow-Methods", ""), "POST must be permitted")
                            check("authorization" in response.headers.get("Access-Control-Allow-Headers", "").lower(), "Bearer header must be permitted")
                            if len(expected) > 1:
                                check("origin" in response.headers.get("Vary", "").lower(), "Multiple allowed origins must vary by Origin")
            if environment == "Development":
                for path, method, body, status in (("/health", "GET", None, 200),
                                                   ("/api/v1/me", "GET", None, 401),
                                                   ("/api/v1/auth/register", "POST", {}, 400)):
                    with request(base, path, origins[0], method, body) as response:
                        check(response.status == status, f"Unexpected status for {path}")
                        check(response.headers.get("Access-Control-Allow-Origin") ==
                              (origins[0] if origins[0] in expected else None), "CORS missing on actual response")
                with request(base, "/health") as response:
                    check(response.headers.get("Access-Control-Allow-Origin") is None, "Same-origin requests do not need CORS headers")
        finally:
            process.terminate()
            try:
                process.wait(timeout=5)
            except subprocess.TimeoutExpired:
                process.kill()
                process.wait()


if __name__ == "__main__":
    if not DLL.exists():
        raise SystemExit("Build first: dotnet build src/UhilTaxi.Api/UhilTaxi.Api.csproj")
    scenario("Development")
    scenario("Development", "https://frontend.example.test")
    scenario("Development", "")
    scenario("Production", "")
    scenario("Production", "https://frontend.example.test")
    print(f"CORS integration: {checks} checks passed")
