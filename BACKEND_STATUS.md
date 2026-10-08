# UhilTaxi — стан бекенду

Це **UhilTaxi**, без перейменування namespace, проєктів, Docker-сервісів або БД на TaxiPark.
Документ `TaxiPark_Technical_Specification_CSharp_Vue` використано **лише як функціональну специфікацію**.

## Реалізовані в кодовій базі підсистеми
- JWT access/refresh, логін, реєстрація клієнта, профіль і зміна пароля.
- Адміністратор seed (лише Development), клієнти, водії, тарифи, промокоди.
- Замовлення: estimate, create, списки, призначення, прийняття, прибуття, скасування.
- Поїздки: старт, завершення, збереження snapshot тарифу, історія статусів.
- Автопарк, моделі авто, зміни, технічне обслуговування, заправки, порушення.
- Навчальні платежі (cash), відгуки, адміністративна статистика і звіти.
- Централізована обробка AuthException як ProblemDetails.

## Виправлення цієї редакції
- Заповнені відсутні сутність AuditLog і EF Core configuration для audit_logs.
- Доданий DbSet<AuditLog> у DbContext.
- Вхідні DTO для CarModel, Car, Maintenance, Violation, EnergyLog — службові поля не приймаються з body.
- Аудит адміністративного керування авто/моделями/ТО і створення порушення.
- Додатковий захист від null VIN у валідації авто.

## Важливі обмеження / що треба перевірити перед захистом і production
- **У цьому середовищі немає .NET SDK, тому збірку, Docker runtime і HTTP інтеграційні тести НЕ перевірено.** Код не можна назвати гарантовано повністю протестованим.
- Реальний банківський еквайринг відсутній за межами MVP (картка/wallet = pending без провайдера).
- Карта/маршрут використовує MockMapsService (припустимо за MVP-специфікацією).
- Частина OperationsService залишається у Infrastructure, тоді як документація рекомендує бізнес-логіку в Application; потрібна подальша архітектурна уніфікація.
- Не всі admin-операції охоплені повним audit logging, не всі списки мають єдиний пагінований контракт.
- Адміністративні create-операції з аудитом іноді виконують 2 SaveChanges, тому потребують транзакційного доопрацювання для строго атомарного аудиту.
- Перевірити належну роботу refresh cookie в браузері з обраним frontend origin/CORS.

## Перевірки локально (Windows PowerShell)
```powershell
dotnet restore UhilTaxi.slnx
dotnet build UhilTaxi.slnx
dotnet test UhilTaxi.slnx
docker compose up -d --build --force-recreate api
docker compose ps -a
docker compose logs --tail=100 api
```
Swagger `http://localhost:8080/swagger`, health `http://localhost:8080/health`.

Не додавайте `.env` у Git і не замінюйте свою робочу `.env` з архіву.
