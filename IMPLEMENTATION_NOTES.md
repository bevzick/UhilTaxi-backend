# UhilTaxi — доповнення backend за TaxiPark Technical Specification v2.0

## Що додано в цьому пакеті

У наданому проєкті були реалізовані Authentication, Profile, Drivers, Clients, Tariffs, Promocodes, Orders і Trips. Цей пакет **не замінює** їх і додає відсутні API:

- `GET/POST/PATCH /api/v1/admin/car-models`;
- `GET/POST /api/v1/admin/cars`, `GET/PATCH /api/v1/admin/cars/{id}`, `PATCH /api/v1/admin/cars/{id}/status`;
- `GET/POST /api/v1/admin/cars/{carId}/maintenance`, `GET/PATCH /api/v1/admin/maintenance/{id}`;
- `POST/GET /api/v1/driver/shifts`, `GET /api/v1/driver/shifts/current`, `POST /api/v1/driver/shifts/{shiftId}/close`;
- `GET /api/v1/admin/shifts`, `GET /api/v1/admin/shifts/{id}`;
- `GET/POST /api/v1/driver/energy-logs`;
- `GET /api/v1/driver/violations`, `GET/POST /api/v1/admin/drivers/{id}/violations`;
- `GET /api/v1/admin/drivers/{id}/shifts`, `GET /api/v1/admin/drivers/{id}/trips`;
- `GET /api/v1/admin/dashboard`, `GET /api/v1/admin/reports/{revenue,trips,drivers,cars}`;
- `GET /api/v1/admin/payments`, `GET /api/v1/admin/payments/{id}`;
- `POST/GET /api/v1/trips/{tripId}/payments`, `POST/GET /api/v1/trips/{tripId}/reviews`.

Вирішено помилку із повторним визначенням `UhilTaxiDbContext.Promocodes`. Додано таблиці EF DbSet та мапінги існуючих таблиць `database/init.sql` (база SQL НЕ перезаписується).

## Важливі правила / обмеження

1. Унікальність номера авто/VIN і моделі, валідність типів пального, позитивність місць.
2. Shift перевіряє активність водія й авто, одну відкриту зміну водія/авто, пробіг; відкриття використовує транзакцію Serializable.
3. Заправки прив'язані до відкритої зміни, валідуються обсяг, одиниця, тип пального і ціна.
4. Відгук дозволено лише учаснику завершеної поїздки; унікальний reviewer/trip, рейтинг водія перераховується у транзакції.
5. Оплати перевіряють власність поїздки, відсутність попередньої успішної оплати й idempotency_key; **готівка** в навчальному режимі відразу має статус `succeeded`, `card`/`wallet` створюються `pending` без зовнішнього payment provider.
6. Не всі рекомендації специфікації повністю реалізовані: у нових маршрутах поки використовуються entity payloads замість окремих DTO, Application Service для нових модулів зосереджено в `Infrastructure/OperationsService.cs`; пагінація та аудит нових адмін-операцій ще потребують вирівнювання. Документ не слід вважати повністю реалізованим.

## Запуск (PowerShell)

```powershell
dotnet restore UhilTaxi.slnx
dotnet build UhilTaxi.slnx
dotnet test UhilTaxi.slnx
docker compose up -d --build --force-recreate api
docker compose logs --tail=100 api
```

Перед оновленням рекомендується зробити `git checkout -b feature/backend-remaining` на актуальній базовій гілці та зробити backup MySQL. Не видаляйте Docker volumes і НЕ копіюйте `.env` із чужого архіву.

Увага: архів згенеровано в середовищі **без dotnet SDK** — збірку та інтеграційні тести тут не виконано. У разі помилок збірки потрібні повідомлення компілятора.
