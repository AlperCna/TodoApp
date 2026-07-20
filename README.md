# TodoApp

A **Todo application** built with **.NET 8**, following **Clean Architecture** principles with a clear separation of concerns across four layers.

## 🏗️ Architecture

| Layer | Responsibility |
| :--- | :--- |
| `TodoApp.Domain` | Core entities and business rules — no external dependencies |
| `TodoApp.Application` | Use cases, interfaces, DTOs, and application logic |
| `TodoApp.Infrastructure` | Data access & persistence (SQL Server) and external services |
| `TodoApp.WebApi` | REST API endpoints and dependency wiring |

## 🛠️ Tech Stack

- **.NET 8** · C#
- **Clean Architecture**
- **SQL Server** (T-SQL)
- RESTful Web API

## 🚀 Getting Started

```bash
dotnet restore
dotnet build
dotnet run --project TodoApp.WebApi
```

Update the connection string in the Web API project's `appsettings.json` to point to your SQL Server instance before running.

## 📄 License

Open source, for educational purposes.
