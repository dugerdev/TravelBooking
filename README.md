# TravelBooking

Full-stack travel reservation platform built with **ASP.NET Core 9** and **Clean Architecture**. Supports flight search & booking, hotel reservations, car rentals, tour packages, and an admin dashboard.

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [API Documentation](#api-documentation)
- [Admin Panel](#admin-panel)
- [Architecture](#architecture)
- [Contributing](#contributing)
- [License](#license)

---

## Features

### Customer-Facing (Web UI)
- **Flight Search & Booking** – Search external flight APIs, view results, complete multi-passenger bookings with seat selection.
- **Hotel Listings & Reservations** – Browse hotels with amenity filters (pool, restaurant, spa, Wi-Fi, parking), view room details, and book.
- **Car Rentals** – Search available vehicles, view specs (transmission, fuel type, capacity), and rent with optional insurance add-ons.
- **Tour Packages** – Explore curated tours with pricing, itineraries, and direct booking.
- **User Accounts** – Registration, login, email verification, password reset, profile management, reservation history.
- **Stripe Payments** – Secure checkout via Stripe integration.
- **Multi-Currency Support** – Dynamic currency conversion and display.
- **Responsive Design** – Mobile-first UI with Bootstrap 5.

### Admin Dashboard
- **User Management** – View, edit, and manage registered users and roles.
- **Reservation Management** – Monitor all bookings across flights, hotels, and cars.
- **Flight Management** – Create flights, sync with external APIs.
- **Statistics Dashboard** – Revenue, booking counts, user activity metrics.
- **Contact Messages** – View and manage customer inquiries.
- **News & Testimonials** – Content management for the public site.

### API
- **Clean Architecture** – Domain, Application, Infrastructure, and Presentation layers.
- **JWT Authentication** – Access + Refresh token flow.
- **Role-Based Authorization** – Admin and User roles with policy-based access.
- **Rate Limiting** – Protects auth endpoints (20 req/min).
- **Background Services** – Periodic token cleanup and flight data sync.
- **Health Checks** – `/health`, `/health/ready`, `/health/live` endpoints.
- **Swagger/OpenAPI** – Full interactive API documentation.
- **Caching** – In-memory caching for flights and airport data.
- **Polly Resilience** – Retry and circuit-breaker policies for external HTTP calls.

---

## Tech Stack

| Layer | Technology |
|---|---|
| **API** | ASP.NET Core 9, C# 13 |
| **Web UI** | ASP.NET Core MVC 9, Razor Views |
| **Database** | SQL Server (LocalDB / SQL Server) |
| **ORM** | Entity Framework Core 9 |
| **Auth** | ASP.NET Core Identity, JWT Bearer |
| **Validation** | FluentValidation |
| **Mapping** | AutoMapper |
| **API Docs** | Swashbuckle (Swagger) |
| **Payments** | Stripe |
| **Resilience** | Polly (retry, circuit-breaker) |
| **Front-End** | Bootstrap 5, jQuery, Swiper.js |
| **Admin UI** | SB Admin 2 (Bootstrap) |

---

## Project Structure

```
TravelBooking/
├── API/
│   └── TravelBooking/
│       ├── TravelBooking.API.sln
│       ├── TravelBooking.Api/              # Presentation Layer
│       │   ├── Controllers/                # REST API endpoints
│       │   ├── Authorization/              # Policies & handlers
│       │   ├── Services/                   # API-level services
│       │   ├── DbSeeder.cs                 # Initial data seeding
│       │   └── appsettings.json            # API configuration
│       ├── TravelBooking.Application/      # Application Layer
│       │   ├── Contracts/                  # Service interfaces
│       │   ├── Services/                   # Business logic
│       │   ├── Dtos/                       # Data Transfer Objects
│       │   └── Validators/                 # FluentValidation rules
│       ├── TravelBooking.Domain/           # Domain Layer
│       │   ├── Entities/                   # Domain models
│       │   ├── Events/                     # Domain events
│       │   └── Enums/                      # Enumerations
│       └── TravelBooking.Infrastructure/   # Infrastructure Layer
│           ├── Data/                       # DbContext & migrations
│           ├── Repositories/               # Repository implementations
│           └── External/                   # Email, external APIs
│
├── UI/
│   └── TravelBooking.Web/
│       ├── TravelBooking.Web.sln
│       └── TravelBooking.Web/
│           ├── Controllers/                # MVC controllers
│           ├── Views/                      # Razor views
│           ├── Areas/Admin/                # Admin dashboard area
│           ├── Services/                   # API client services
│           ├── DTOs/                       # View-level DTOs
│           ├── ViewModels/                 # View models
│           ├── Helpers/                    # Image URL, currency helpers
│           ├── Middleware/                 # Custom middleware
│           ├── wwwroot/                    # Static assets (CSS, JS, images)
│           └── appsettings.json            # Web configuration
│
├── .gitignore
└── README.md
```

---

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB included with Visual Studio, or any SQL Server instance)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (recommended) or [VS Code](https://code.visualstudio.com/) with C# Dev Kit
- (Optional) [Postman](https://www.postman.com/) for API testing

---

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd TravelBooking
```

### 2. Restore Packages

```bash
# API
dotnet restore API/TravelBooking/TravelBooking.API.sln

# Web UI
dotnet restore UI/TravelBooking.Web/TravelBooking.Web.sln
```

### 3. Apply Database Migrations

```bash
cd API/TravelBooking/TravelBooking.Api
dotnet ef database update
```

The database is also auto-migrated on first application startup.

### 4. Run Both Projects

**Terminal 1 – API:**
```bash
dotnet run --project API/TravelBooking/TravelBooking.Api
```

**Terminal 2 – Web UI:**
```bash
dotnet run --project UI/TravelBooking.Web/TravelBooking.Web
```

### 5. Access the Application

| Service | URL |
|---|---|
| **Web UI** | `https://localhost:7208` |
| **API (Swagger)** | `https://localhost:7283/swagger` |
| **Health Check** | `https://localhost:7283/health` |

---

## Configuration

### API (`API/TravelBooking/TravelBooking.Api/appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TravelBookingDb;Trusted_Connection=true;TrustServerCertificate=true"
  },
  "JWT": {
    "Secret": "<min-32-char-secret>",
    "ValidIssuer": "travelbooking.api",
    "ValidAudience": "travelbooking.client"
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "noreply@travelbooking.com",
    "SenderName": "TravelBooking",
    "Username": "<your-email>",
    "Password": "<your-app-password>"
  }
}
```

### Web UI (`UI/TravelBooking.Web/TravelBooking.Web/appsettings.json`)

Ensure the `ApiSettings:BaseUrl` points to the running API instance:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7283"
  }
}
```

> **Security Note:** Never commit production secrets. Use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for development and environment variables for production.

---

## API Documentation

### Core Endpoints

| Module | Endpoint | Description |
|---|---|---|
| **Auth** | `POST /api/auth/signup` | User registration |
| | `POST /api/auth/login` | Login (returns JWT) |
| | `POST /api/auth/refresh` | Refresh access token |
| | `POST /api/auth/forgot-password` | Request password reset |
| | `POST /api/auth/verify-email` | Verify email address |
| **Flights** | `GET /api/flights` | List flights (paginated) |
| | `GET /api/flights/{id}` | Flight details |
| | `GET /api/flights/search-external` | Search external flight APIs |
| | `POST /api/flights` | Create flight (admin) |
| **Hotels** | `GET /api/hotels` | List hotels |
| | `GET /api/hotels/{id}` | Hotel details with rooms |
| **Cars** | `GET /api/cars` | List rental cars |
| | `GET /api/cars/{id}` | Car details |
| **Tours** | `GET /api/tours` | List tour packages |
| **Reservations** | `POST /api/reservations` | Create reservation |
| | `GET /api/reservations/pnr/{pnr}` | Lookup by PNR |
| **Account** | `GET /api/account/profile` | User profile |
| | `GET /api/account/reservations` | User's reservations |
| **Admin** | `GET /api/admin/statistics` | Dashboard stats |
| | `GET /api/admin/users` | Manage users |
| | `GET /api/admin/reservations` | All reservations |

Full interactive documentation is available at `/swagger` when the API is running in Development mode.

### Postman Collection

Import the included Postman files for ready-to-use API requests:
- `API/TravelBooking/TravelBooking.postman_collection.json`
- `API/TravelBooking/TravelBooking.postman_environment.json`

---

## Admin Panel

### Default Credentials (Development Only)

| Field | Value |
|---|---|
| **Email** | `admin@travelbooking.com` |
| **Username** | `admin` |
| **Password** | `Admin123!ChangeMe` |

> **Warning:** Change these credentials immediately in production environments.

Access the admin panel by logging in with admin credentials. The dashboard is available under the `/Admin` area.

---

## Architecture

The API follows **Clean Architecture** principles with four distinct layers:

```
┌─────────────────────────────────────────────┐
│              TravelBooking.Api               │  Presentation
│         (Controllers, Middleware)             │  Layer
├─────────────────────────────────────────────┤
│          TravelBooking.Application           │  Application
│      (Services, DTOs, Validators)            │  Layer
├─────────────────────────────────────────────┤
│            TravelBooking.Domain              │  Domain
│        (Entities, Events, Enums)             │  Layer
├─────────────────────────────────────────────┤
│         TravelBooking.Infrastructure         │  Infrastructure
│    (EF Core, Repositories, Email, APIs)      │  Layer
└─────────────────────────────────────────────┘
```

**Key architectural decisions:**
- **Dependency Inversion** – All layers depend on abstractions (interfaces), not implementations.
- **CQRS-lite** – Separation of read and write concerns through service contracts.
- **Repository Pattern** – Data access abstracted behind repository interfaces.
- **Domain Events** – Decoupled event handling for cross-cutting concerns.
- **AutoMapper Profiles** – Centralized DTO mapping configuration.
- **FluentValidation** – Request validation as a cross-cutting concern.

The Web UI acts as a **BFF (Backend for Frontend)**, consuming the API through typed HTTP clients with Polly resilience policies.

---

## Health Checks

| Endpoint | Purpose |
|---|---|
| `GET /health` | Overall application health |
| `GET /health/ready` | Readiness probe (DB connectivity) |
| `GET /health/live` | Liveness probe |

---

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/your-feature`)
3. Commit your changes (`git commit -m 'Add your feature'`)
4. Push to the branch (`git push origin feature/your-feature`)
5. Open a Pull Request

---

## License

This project is proprietary software. All rights reserved.
#   T r a v e l B o o k i n g  
 