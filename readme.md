# Inventory API

A robust ASP.NET Core Web API for managing products, warehouses, inventory levels, and user authentication. Designed for businesses to track stock, handle transfers, and monitor sales across multiple warehouses.

[**Try Here**](https://inventory-viewer-lliam.vercel.app/) – Live demo of the Inventory API frontend in React.
## Features

- **Product Management:** Create, update, delete, and list products.
- **Warehouse Management:** Manage multiple warehouse locations.
- **Inventory Tracking:** Monitor stock levels, restock, deplete, and transfer inventory between warehouses.
- **Stock Logging:** Detailed logs for all inventory changes (restock, sale, transfer).
- **User Authentication:** Secure JWT-based login, registration, and role-based access (Admin, Warehouse).
- **Health Checks:** API endpoint for service status.
- **Error Handling:** Global exception middleware for consistent error responses.
- **CORS Support:** Configured for frontend integration.
- **Database Seeding:** Automatic creation of base data for development.

## Technologies & Skills Used

- **C# / .NET 9**
- **ASP.NET Core Web API**
- **Entity Framework Core (EF Core)**
- **SQL Server (Azure & Local)**
- **JWT Authentication**
- **Role-Based Authorization**
- **Middleware (Custom JWT & Exception Handling)**
- **RESTful API Design**
- **Unit & Integration Testing (REST Client)**
- **Azure Web App Deployment (GitHub Actions)**
- **JSON Serialization**
- **LINQ & Async Programming**
- **Configuration Management**

## Getting Started

1. **Clone the repository:**
   ```sh
   git clone https://github.com/lliamsymonds04/inventory-api.git
   cd inventory-api
   ```

2. **Configure secrets:**
   - Set database and JWT secrets in `appsettings.Development.json` or via environment variables.

3. **Run locally:**
   ```sh
   dotnet run
   ```

4. **Seed the database (optional):**
   ```sh
   dotnet run --seed
   ```

5. **API Documentation:**
   - OpenAPI/Swagger available in development mode at `/swagger`.

## API Endpoints

- `/api/Product` - Product CRUD
- `/api/Warehouse` - Warehouse CRUD
- `/api/Inventory` - Inventory operations
- `/api/StockLog` - Stock change logs
- `/api/Auth` - User authentication

## Deployment

- Automated CI/CD via GitHub Actions to Azure Web App.
- See `.github/workflows/` for build and deploy pipelines.

## License

MIT License © 2025 Lliam

---

**Skills demonstrated:** backend development, authentication, database design, cloud deployment, API documentation, error handling, and modern C# best practices.
