# IDMS Backend

**Identity & Domain Management System** — A RESTful backend API for managing student identity, attendance tracking, event management, and role-based access control across institutional domains.

---

## Table of Contents

- [Overview](#overview)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
  - [1. Clone the Repository](#1-clone-the-repository)
  - [2. Start the Database](#2-start-the-database)
  - [3. Configure the Application](#3-configure-the-application)
  - [4. Run the Application](#4-run-the-application)
- [Configuration Reference](#configuration-reference)
- [API Reference](#api-reference)
  - [Users](#users)
  - [Students](#students)
  - [Student Cards](#student-cards)
  - [Roles](#roles)
  - [Domains](#domains)
  - [Events](#events)
  - [User Domain Roles](#user-domain-roles)
  - [Health](#health)
- [Data Models](#data-models)
- [Architecture](#architecture)
  - [Response Format](#response-format)
  - [Background Services](#background-services)
  - [Database Seeding](#database-seeding)
  - [Audit Logging](#audit-logging)
- [Development Tools](#development-tools)

---

## Overview

IDMS Backend provides a centralized API for institutions to:

- **Manage users** (students and faculty) with role-based permissions
- **Track events** across different campus domains (Church, Cafeteria, Library, Dormitories, etc.)
- **Synchronize student records** from an external university system automatically
- **Issue and manage student cards** for identification purposes
- **Assign domain-scoped roles** to control access and responsibilities

---

## Technology Stack

| Component        | Technology                          |
|-----------------|-------------------------------------|
| Framework       | .NET 9.0 ASP.NET Core Web API       |
| Database        | PostgreSQL 16                       |
| ORM             | Entity Framework Core 9.0 (Npgsql)  |
| Password Hashing| BCrypt.Net-Next 4.1.0               |
| API Docs        | Scalar (OpenAPI) — development only |
| Containerization| Docker & Docker Compose             |
| Health Checks   | AspNetCore.HealthChecks.NpgSql      |

---

## Project Structure

```
IDMSBackend/
├── BackgroundServices/
│   └── StudentSyncWorker.cs       # Scheduled student data sync job
├── Controllers/
│   ├── DomainController.cs        # Domain management endpoints
│   ├── EventController.cs         # Event management endpoints
│   ├── HealthController.cs        # Health check redirect
│   ├── RoleContoller.cs           # Role management endpoints
│   ├── StudentCardController.cs   # Student card endpoints
│   ├── StudentController.cs       # Student management endpoints
│   ├── UserController.cs          # User management endpoints
│   └── UserDomainRoleController.cs # Role-domain assignment endpoints
├── Data/
│   ├── AppDbContext.cs            # EF Core database context with audit logging
│   └── DbInitializer.cs          # Database seeder (domains, roles, sample students)
├── Docker/
│   └── docker-compose.yml        # PostgreSQL container configuration
├── DTOs/
│   ├── DomainsDtos.cs            # Domain data transfer objects
│   ├── EventsDtos.cs             # Event data transfer objects
│   ├── RoleDtos.cs               # Role data transfer objects
│   ├── StudentCardsDtos.cs       # Student card data transfer objects
│   ├── StudentDtos.cs            # Student data transfer objects
│   ├── UserDomainRoleDtos.cs     # Role-domain assignment DTOs
│   └── UsersDtos.cs             # User data transfer objects
├── Models/
│   ├── AuditLog.cs              # Audit trail model
│   ├── Domains.cs               # Campus domain entity
│   ├── Events.cs                # Campus event entity
│   ├── EventsRecurrenceRules.cs # Event recurrence configuration
│   ├── Roles.cs                 # System role entity
│   ├── StudentCards.cs          # Student identification card entity
│   ├── Students.cs              # Student record entity
│   ├── User.cs                  # Application user entity
│   └── UserDomainRole.cs        # User ↔ Role ↔ Domain junction entity
├── Services/
│   ├── Implementations/         # Concrete service implementations
│   │   ├── DomainServices.cs
│   │   ├── EventServices.cs
│   │   ├── RoleServices.cs
│   │   ├── StudentCardServices.cs
│   │   ├── StudentServices.cs
│   │   ├── UserDomainRoleServices.cs
│   │   └── UserServices.cs
│   └── Interfaces/              # Service contracts
│       ├── IDomainsServices.cs
│       ├── IEventServices.cs
│       ├── IRoleServices.cs
│       ├── IStudentCards.cs
│       ├── IStudentServices.cs
│       ├── IUserDomainRole.cs
│       └── IUserService.cs
├── Wrappers/
│   └── ApiResponse.cs           # Standardized API response wrapper
├── Program.cs                   # Application entry point and DI configuration
├── appsettings.json             # Application configuration
└── IDMSBackend.csproj           # Project file and dependencies
```

---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started) (for running PostgreSQL)
- [Docker Compose](https://docs.docker.com/compose/)

---

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/mainavict/IDMSBackend.git
cd IDMSBackend
```

### 2. Start the Database

Start the PostgreSQL container using Docker Compose:

```bash
cd Docker
docker-compose up -d
cd ..
```

This starts a PostgreSQL 16 instance with:
- **Host**: `localhost`
- **Port**: `5432`
- **Database**: `appdb`
- **User**: `appuser`
- **Password**: `strongpassword`

### 3. Configure the Application

Update `appsettings.json` (or `appsettings.Development.json`) to match your environment:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=appdb;Username=appuser;Password=strongpassword"
  },
  "SyncSettings": {
    "UniversityBaseUrl": "http://your-university-api-url",
    "RunHour": 12,
    "RunMinute": 10
  }
}
```

### 4. Run the Application

```bash
dotnet restore
dotnet run
```

The API will be available at:
- **API Base URL**: `http://localhost:5004/api`
- **Health Check**: `http://localhost:5004/health`
- **API Documentation** (development only): `http://localhost:5004/scalar`

> In development mode, the database is automatically seeded with sample domains, roles, and students on first launch.

---

## Configuration Reference

| Key | Description | Default |
|-----|-------------|---------|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string | `Host=localhost;Port=5432;Database=appdb;Username=appuser;Password=strongpassword` |
| `SyncSettings:UniversityBaseUrl` | Base URL for the external university student API | `http://localhost:8000` |
| `SyncSettings:RunHour` | Hour (0–23) when the daily student sync runs | `12` |
| `SyncSettings:RunMinute` | Minute (0–59) when the daily student sync runs | `10` |

---

## API Reference

All endpoints return a standardized JSON response:

```json
{
  "success": true,
  "message": "Description of the result",
  "data": { ... },
  "statusCode": 200
}
```

---

### Users

Base path: `/api/user`

| Method | Endpoint | Description | Request Body |
|--------|----------|-------------|--------------|
| `POST` | `/api/user` | Create a new user | `UserCreateDto` |
| `GET` | `/api/user` | Get all users | — |
| `GET` | `/api/user/{userId}` | Get a user by ID | — |
| `PUT` | `/api/user/{userId}` | Update user details | `UserUpdateDto` |
| `POST` | `/api/user/{userId}/change-password` | Change user password | `ChangePasswordDto` |
| `POST` | `/api/user/{userId}/change-status` | Change user status | `ChangeUserStatusDto` |

**`UserCreateDto`**
```json
{
  "schoolId": "STU12345",
  "facultyId": null,
  "userType": "student",
  "firstName": "Jane",
  "lastName": "Doe",
  "email": "jane.doe@example.com",
  "password": "SecurePassword123"
}
```

**`UserUpdateDto`**
```json
{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@example.com"
}
```

**`ChangePasswordDto`**
```json
{
  "currentPassword": "OldPassword123",
  "newPassword": "NewPassword456"
}
```

**`ChangeUserStatusDto`**
```json
{
  "newStatus": "suspended"
}
```

User statuses: `active`, `suspended`, `deleted`

User types: `student`, `faculty`

---

### Students

Base path: `/api/student`

| Method | Endpoint | Description | Request Body |
|--------|----------|-------------|--------------|
| `GET` | `/api/student` | Get all students | — |
| `GET` | `/api/student/{studentId}` | Get a student by school ID | — |
| `POST` | `/api/student/sync` | Sync students from external data | `List<StudentSyncDto>` |

**`StudentSyncDto`** (array)
```json
[
  {
    "schoolId": "STU001",
    "fullName": "John Doe",
    "email": "john.doe@example.com",
    "yearOfStudy": "3",
    "residence": "Men's Dorm A",
    "academicStatus": "Active"
  }
]
```

---

### Student Cards

Base path: `/api/studentcard`

| Method | Endpoint | Description | Request Body |
|--------|----------|-------------|--------------|
| `POST` | `/api/studentcard` | Create a student card | `CreateStudentCardsDto` |
| `GET` | `/api/studentcard/{schoolId}` | Get a student card by school ID | — |

**`CreateStudentCardsDto`**
```json
{
  "schoolId": "STU001",
  "cardNumber": "CARD-2024-001"
}
```

---

### Roles

Base path: `/api/role`

| Method | Endpoint | Description | Request Body |
|--------|----------|-------------|--------------|
| `POST` | `/api/role` | Create a new role | `CreateRoleDto` |
| `GET` | `/api/role` | Get all roles | — |
| `DELETE` | `/api/role/{roleId}` | Delete a role by ID | — |

**`CreateRoleDto`**
```json
{
  "name": "Manager"
}
```

Predefined roles (seeded on startup): `Admin`, `Manager`, `Scanner`

---

### Domains

Base path: `/api/domain`

| Method | Endpoint | Description | Request Body |
|--------|----------|-------------|--------------|
| `POST` | `/api/domain` | Create a new domain | `domainName` (query param) |
| `GET` | `/api/domain` | Get all domains | — |

Example: `POST /api/domain?domainName=Library`

Predefined domains (seeded on startup): `Church`, `Cafeteria`, `Men's-dorm`, `Library`, `Lady's-dorm`

---

### Events

Base path: `/api/event`

| Method | Endpoint | Description | Headers |
|--------|----------|-------------|---------|
| `POST` | `/api/event` | Create an event | `creatorId: <userId>` |
| `GET` | `/api/event/{domainId}` | Get active events for a domain | — |
| `GET` | `/api/event/domain/{domainId}` | Get all events for a domain | — |
| `GET` | `/api/event/scanner/{userId}` | Get current active events for a scanner user | — |
| `PUT` | `/api/event/{eventId}` | Update an event | `updaterId: <userId>` |
| `DELETE` | `/api/event/{eventId}` | Delete an event | — |

**`CreateEventDto`**
```json
{
  "name": "Morning Chapel",
  "eventType": "church",
  "isCritical": true,
  "activeFrom": "2024-08-01T00:00:00Z",
  "activeUntil": "2025-05-31T00:00:00Z",
  "startTime": "07:00",
  "endTime": "08:30",
  "scanStartOffset": 15,
  "description": "Daily morning chapel service",
  "domainId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "isRecurring": true,
  "frequency": "daily",
  "dayOfWeek": null,
  "dayOfMonth": 0,
  "monthOfYear": 0
}
```

Event types: `cafeteria`, `residence`, `church`, `assembly`, `exams`

Recurrence frequencies: `daily`, `weekly`, `monthly`, `yearly`

---

### User Domain Roles

Base path: `/api/userdomainrole`

| Method | Endpoint | Description | Request Body |
|--------|----------|-------------|--------------|
| `POST` | `/api/userdomainrole/assign-role-domain` | Assign a role to a user in a domain | `AllocateRoleToUserDtos` |
| `GET` | `/api/userdomainrole` | Get roles and domains for a user | `userId` (query param) |
| `DELETE` | `/api/userdomainrole/remove-role-domain` | Remove a role from a user in a domain | `RemoveUserRoleDomainDtos` |
| `GET` | `/api/userdomainrole/get-users-by-domain` | Get all users in a domain | `domainName` (query param) |

**`AllocateRoleToUserDtos`**
```json
{
  "userId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "role": "Scanner",
  "domain": "Cafeteria"
}
```

**`RemoveUserRoleDomainDtos`**
```json
{
  "userId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "role": "Scanner",
  "domain": "Cafeteria"
}
```

---

### Health

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/health` | Returns system health status (PostgreSQL connectivity) |
| `GET` | `/api/health` | Redirects to `/health` |

**Example response:**
```json
{
  "status": "Healthy",
  "checks": [
    {
      "component": "PostgreSQL",
      "status": "Healthy",
      "description": null
    }
  ],
  "duration": "00:00:00.0123456"
}
```

---

## Data Models

### User

| Field | Type | Description |
|-------|------|-------------|
| `Id` | `Guid` | Primary key |
| `SchoolId` | `string?` | Student school ID (unique) |
| `FacultyId` | `string?` | Faculty ID (unique) |
| `UserType` | `enum` | `Student` or `Faculty` |
| `FirstName` | `string` | First name |
| `LastName` | `string` | Last name |
| `Email` | `string` | Email address |
| `PasswordHash` | `string` | BCrypt password hash |
| `Status` | `enum` | `Active`, `Suspended`, or `Deleted` |
| `CreatedAt` | `DateTime` | Creation timestamp (UTC) |
| `UpdatedAt` | `DateTime` | Last update timestamp (UTC) |

### Student

| Field | Type | Description |
|-------|------|-------------|
| `Id` | `Guid` | Primary key |
| `SchoolId` | `string` | Unique school identifier |
| `FullName` | `string` | Student's full name |
| `Email` | `string` | Email address |
| `YearOfStudy` | `string` | Academic year |
| `Residence` | `string` | Campus residence/dormitory |
| `AcademicStatus` | `string` | Enrollment status |
| `LastSyncDate` | `DateTime` | Last sync from external system |

### Event

| Field | Type | Description |
|-------|------|-------------|
| `Id` | `Guid` | Primary key |
| `Name` | `string` | Event name |
| `EventType` | `enum` | `Cafeteria`, `Residence`, `Church`, `Assembly`, `Exams` |
| `IsCritical` | `bool` | Whether attendance is mandatory |
| `ActiveFrom` | `DateTime` | Event season start date |
| `ActiveUntil` | `DateTime` | Event season end date |
| `StartTime` | `TimeOnly` | Daily start time |
| `EndTime` | `TimeOnly` | Daily end time |
| `IsRecurring` | `bool` | Whether event repeats |
| `ScanStartOffset` | `int` | Minutes before start when scanning opens |
| `DomainId` | `Guid` | Owning domain |

### EventsRecurrenceRules

| Field | Type | Description |
|-------|------|-------------|
| `EventId` | `Guid` | FK to Event |
| `Frequency` | `enum` | `Daily`, `Weekly`, `Monthly`, `Yearly` |
| `Interval` | `int` | Recurrence interval (default: 1) |
| `DayOfWeek` | `DayOfWeek?` | For weekly recurrence |
| `DayOfMonth` | `int` | For monthly recurrence |
| `MonthOfYear` | `int` | For yearly recurrence |

### UserDomainRole

Junction table linking Users, Roles, and Domains (many-to-many).

| Field | Type | Description |
|-------|------|-------------|
| `UserId` | `Guid` | FK to User |
| `RoleId` | `Guid` | FK to Role |
| `DomainId` | `Guid` | FK to Domain |
| `AssignedAt` | `DateTime` | Assignment timestamp |

---

## Architecture

### Response Format

All API responses use the `ApiResponse<T>` wrapper:

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public int StatusCode { get; set; }
}
```

### Background Services

**`StudentSyncWorker`** runs on a configurable daily schedule to automatically sync student records from an external university API. It:

1. Calculates the next scheduled run time based on `SyncSettings:RunHour` and `SyncSettings:RunMinute`
2. Fetches all student data from the external API
3. Processes records in batches of 50
4. Detects changes (only updates records where data has actually changed)
5. Reports sync statistics: records added, updated, and execution time

### Database Seeding

In development mode, `DbInitializer.Seed()` populates the database with:

- **5 Domains**: Church, Cafeteria, Men's-dorm, Library, Lady's-dorm
- **3 Roles**: Admin, Manager, Scanner
- **10 Sample Students** for testing

### Audit Logging

`AppDbContext` automatically records every database change to the `AuditLog` table, capturing:
- Entity type and action (Create/Update/Delete)
- User identity and IP address
- Request trace ID and user agent
- Timestamp

---

## Development Tools

When running in **Development** mode, the interactive **Scalar API Reference** is available at:

```
http://localhost:5004/scalar
```

It is configured with the **Moon** theme and supports C# `HttpClient` code generation.

The raw OpenAPI document is served at:

```
http://localhost:5004/openapi/v1.json
```
