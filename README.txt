# SweetBite — Full Stack Bakery Website

Learning project built with HTML, CSS, JavaScript, ASP.NET Core Web API, Entity Framework Core, and SQL Server.

## Features
- Products are loaded from SQL Server through `GET /api/products`.
- Add products to a client-side shopping cart.
- Submit a demo order with customer name/email and selected items.
- Orders and order items are saved to SQL Server.
- Server recalculates totals using database product prices.
- Responsive layout for mobile and desktop.

## Requirements
- .NET 8 SDK
- SQL Server LocalDB (usually installed with Visual Studio's SQL Server/Data storage components), or a SQL Server instance you configure.

## Run in Visual Studio
1. Extract the ZIP.
2. Open `SweetBite.Api.csproj` in Visual Studio 2022.
3. Check `appsettings.json`. The default connection string uses `(localdb)\MSSQLLocalDB`.
4. Restore NuGet packages, then run the project (F5).
5. Open the URL shown by Visual Studio. The site and API are served from the same ASP.NET Core app.
6. The database `SweetBiteDb` and sample products are created on first run.

## API endpoints
- `GET /api/health` — check API status.
- `GET /api/products` — list available products.
- `GET /api/products/{id}` — get one available product.
- `POST /api/orders` — create a demo order.
- `GET /api/orders/{id}` — inspect an order.

## Important limitations
- This is a learning/demo project, not production-ready.
- There is no login/admin panel, online payment, inventory management, email sending, or deployment configured.
- Customer order details are stored in the local database. Do not use real customer information in a public demo.
- The project uses `EnsureCreated()` for easy local setup. For a real production app, use EF Core migrations, authentication, authorization, validation, HTTPS, secret management, and deployment configuration.
- Product photos and Google Fonts require an internet connection.
