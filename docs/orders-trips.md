# UhilTaxi: orders та trips

Гілка `feature/orders-trips` створена від `develop`. Реалізація використовує
наявну схему `database/init.sql`; зміна БД або міграція не потрібна.

## API

Усі запити потребують Bearer access token активного користувача.
Клієнт читає лише власні замовлення/поїздки, водій — призначені йому,
адміністратор — усі. Чужий ресурс повертає `404`.

| Роль | Метод та шлях | Результат |
| --- | --- | --- |
| client | `POST /api/v1/orders/estimate` | Оцінка маршруту, знижки та вартості |
| client | `POST /api/v1/orders` | Створення `pending`, `201` |
| client | `GET /api/v1/orders` | Власні замовлення |
| client | `GET /api/v1/orders/{id}` | Власне замовлення |
| client | `GET /api/v1/orders/{id}/history` | Історія статусів |
| client | `POST /api/v1/orders/{id}/cancel` | Скасування до початку поїздки |
| client | `GET /api/v1/trips` / `GET /api/v1/trips/{id}` | Власні поїздки |
| driver | `GET /api/v1/driver/orders/available` | Доступні замовлення сумісного класу |
| driver | `GET /api/v1/driver/orders/assigned` | Призначені замовлення |
| driver | `GET /api/v1/driver/orders/{id}` | Призначене замовлення |
| driver | `GET /api/v1/driver/orders/{id}/history` | Історія статусів |
| driver | `POST /api/v1/driver/orders/{id}/accept` | Прийняття замовлення |
| driver | `POST /api/v1/driver/orders/{id}/arrived` | Прибуття водія |
| driver | `POST /api/v1/driver/orders/{id}/start` | Створення trip, `201` |
| driver | `POST /api/v1/driver/orders/{id}/complete` | Завершення й фактична вартість |
| driver | `GET /api/v1/driver/trips` / `GET /api/v1/driver/trips/{id}` | Історія поїздок |
| admin | `GET /api/v1/admin/orders` / `GET /api/v1/admin/orders/{id}` | Усі замовлення |
| admin | `GET /api/v1/admin/orders/{id}/history` | Історія статусів |
| admin | `POST /api/v1/admin/orders/{id}/assign-driver` | Призначення водія без зміни `pending` |
| admin | `POST /api/v1/admin/orders/{id}/cancel` | Скасування до початку поїздки |
| admin | `POST /api/v1/admin/orders` | Створення замовлення для клієнта |
| admin | `POST /api/v1/admin/orders/estimate` | Оцінка вартості |
| admin | `GET /api/v1/admin/orders/available` | Усі pending-замовлення |
| admin | `POST /api/v1/admin/orders/{id}/accept` | Прийняття для вказаного `driver_id` |
| admin | `POST /api/v1/admin/orders/{id}/arrived` | Прибуття призначеного водія |
| admin | `POST /api/v1/admin/orders/{id}/start` | Старт для призначеного водія |
| admin | `POST /api/v1/admin/orders/{id}/complete` | Завершення для призначеного водія |
| admin | `GET /api/v1/admin/trips` / `GET /api/v1/admin/trips/{id}` | Усі поїздки |

Для списків: `page=1&limit=20&sort=-created_at`. Максимальний `limit` — 100.
`sort` допускає лише `created_at`, `-created_at`, `id`, `-id`.
Списки замовлень клієнта, адміністратора й призначених водієві підтримують `status`.

```json
{"data": [], "pagination": {"page": 1, "limit": 20, "total": 0, "pages": 0}}
```

## Тіла запитів

Estimate та create використовують однаковий контракт:

```json
{
  "tariff_id": 1,
  "pickup": {"address": "вул. Прикладна, 1", "lat": 49.42, "lng": 26.98},
  "destination": {"address": "вул. Тестова, 20", "lat": 49.40, "lng": 27.01},
  "promocode": "TAXI10"
}
```

`promocode` необов'язковий. `client_id` визначається з JWT; клієнт не передає
вартість, відстань або тривалість оцінки. Координати обов'язкові, мають не більше
7 знаків після коми; початкова й кінцева точки мають відрізнятися.

Окремі команди:

```json
{"reason": "Змінилися плани"}
```

```json
{"driver_id": 12}
```

```json
{"shift_id": 5}
```

```json
{"distance_km": 7.40}
```

Це тіла `cancel`, `assign-driver`, `start`, `complete` відповідно.
Admin create приймає `{"client_id": 123, "order": {...}}`, де `order` — звичайний
create request. Admin accept приймає `{"driver_id": 12}`. Адміністративні
команди записують ID адміністратора в історію та `audit_logs` у тій самій транзакції.
`accept` і `arrived` не потребують body. Тривалість визначає сервер за UTC-часом
початку/завершення, округлюючи неповну хвилину вгору.

## Правила

Дозволений цикл: `pending → accepted → driver_arriving → in_progress → completed`.
Скасування можливе з `pending`, `accepted`, `driver_arriving`. Інші переходи
повертають `409` зі стабільним `code` у ProblemDetails.

Прийняття потребує активного водія, однієї відкритої зміни, активного авто та
точного збігу класу авто з класом тарифу. Водій може мати одне активне замовлення.
Адміністративне призначення залишає `pending`; замовлення доступне лише обраному
водієві. Призначення й адміністративне скасування записуються до `audit_logs`.

Параметризовані `SELECT ... FOR UPDATE` у транзакції блокують замовлення та
профіль водія. Зміна статусу й історія зберігаються атомарно. Start атомарно
створює єдину trip і змінює статус; complete атомарно зберігає фактичні дані,
вартість та історію. Повторне виконання повертає `409`.

При старті trip зберігається snapshot тарифу. Подальша зміна тарифу не змінює
вартість цієї поїздки. Гроші рахуються як `decimal` з округленням
`MidpointRounding.AwayFromZero` до двох знаків.

Промокод перевіряється під час estimate/create, а використання резервується
у транзакції створення замовлення під блокуванням рядка промокоду. Завершення
використовує поточні параметри зарезервованого промокоду для фактичної суми;
закінчення строку дії після резервування не анулює його. Скасування не повертає
використання. Знижка не робить суму від'ємною й враховує minimum/cap.

## Межі реалізації

`MockMapsService` — дозволена ТЗ MVP-апроксимація: відстань по великому колу
помножена на 1.25, швидкість — 30 км/год. Це не реальна маршрутизація дорогами.
Її можна замінити реалізацією `IMapsService` без зміни orders API.

Відкриті зміни, авто, тарифи й промокоди мають створювати відповідні модулі.
Ця feature читає ці дані, але не реалізує CRUD автопарку/змін/промокодів,
платежі, відгуки або Vue frontend. Контролери керування змінами в `develop`
поки порожні; integration-тест створює необхідні fixtures у MySQL.

## Перевірка

Потрібні .NET 10 SDK, Docker з Compose та запущений development API із MySQL.
У хмарному середовищі:

```bash
cd /workspace/UhilTaxi-backend
source /workspace/.cloud-setup/uhiltaxi/environment.sh
bash /workspace/.cloud-setup/uhiltaxi/start.sh
dotnet test UhilTaxi.slnx
python3 tests/orders_trips_integration.py
```

Integration-тест перевіряє справжній HTTP/MySQL сценарій, ownership/RBAC,
переходи статусів, тарифний snapshot, промокод, пагінацію та обидві гонки:
два водії на одне замовлення й один водій на два замовлення. Fixtures унікальні;
тест прибирає лише створені ним дані у `finally`.

Для звичайного локального запуску використайте `.env` за прикладом
`.env.example`, узгодьте пароль `ConnectionStrings__Default` з `MYSQL_PASSWORD`,
задайте власний `JWT_KEY` та запустіть MySQL і `dotnet run` для API-проєкту.
Хмарний helper може використовувати тимчасовий ключ у пам'яті. Для стабільних
токенів задайте `Jwt__Key` або `JWT_KEY` у налаштуваннях середовища; не додавайте
значення до Git. Мінімальна довжина ключа — 32 UTF-8 байти.

## Збірка після інтеграції з модулем промокодів

`Promocode.DiscountType` має тип доменного enum `DiscountType`. EF Core зберігає
його як `fixed` / `percentage` через явний converter; ці самі значення приймає
та повертає JSON API. У `UhilTaxiDbContext` має бути єдина властивість `Promocodes`.
Нові промокоди активні за замовчуванням і мають ініціалізовані UTC timestamps.

Змішування старої string-моделі orders із enum-моделлю адміністративного CRUD
спричиняло `CS0019` / `CS0029` під час `dotnet publish` у Docker. Після оновлення
гілки перебудуйте API через `make up`; наявний том MySQL видаляти не потрібно.

Перевірка сумісності виконується у `tests/orders_trips_integration.py`: промокод
створюється через admin API, читається з MySQL, змінює тип з percentage на fixed
і назад, а потім застосовується до замовлення та завершеної поїздки.

У Codex cloud Docker Buildx потребує доступного для запису `DOCKER_CONFIG`.
Для отримання Microsoft .NET образів потрібні `mcr.microsoft.com` та його CDN;
у поточному середовищі використовується `westcentralus.data.mcr.microsoft.com`.
Мережева заборона CDN під час завантаження образу є окремою від помилок C#.
