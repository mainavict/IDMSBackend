# IDMS Backend

**Identity & Domain Management System** — A RESTful backend API for managing student identity,
attendance tracking, event management, and role-based access control across institutional domains.

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
  - [Standard Response Envelope](#standard-response-envelope)
  - [Users](#users)
  - [Students](#students)
  - [Student Cards](#student-cards)
  - [Roles](#roles)
  - [Domains](#domains)
  - [Events](#events)
  - [User Domain Roles](#user-domain-roles)
  - [Health](#health)
- [Business Logic and Validation Rules](#business-logic-and-validation-rules)
  - [User Creation Rules](#user-creation-rules)
  - [Event Creation and Detection Rules](#event-creation-and-detection-rules)
  - [Student Sync Algorithm](#student-sync-algorithm)
  - [Student Card Rules](#student-card-rules)
  - [Role and Domain Name Normalization](#role-and-domain-name-normalization)
- [Database Schema](#database-schema)
  - [Entity-Relationship Diagram](#entity-relationship-diagram)
  - [Tables and Columns](#tables-and-columns)
  - [Indexes and Constraints](#indexes-and-constraints)
  - [Foreign-Key Cascade Rules](#foreign-key-cascade-rules)
- [Seeded Reference Data](#seeded-reference-data)
- [Architecture](#architecture)
  - [Dependency Injection Map](#dependency-injection-map)
  - [Request Pipeline](#request-pipeline)
  - [Response Format](#response-format)
  - [Audit Logging](#audit-logging)
  - [Background Services](#background-services)
- [Common Use-Case Flows](#common-use-case-flows)
  - [Onboarding a New Scanner User](#onboarding-a-new-scanner-user)
  - [Creating a Recurring Event](#creating-a-recurring-event)
  - [Scanner App Getting Active Events](#scanner-app-getting-active-events)
- [Error Handling Reference](#error-handling-reference)
- [Development Tools](#development-tools)

---

## Overview

IDMS Backend provides a centralized API for institutions to:

- **Manage users** (students and faculty) with role-based permissions and soft-delete support
- **Track events** across different campus domains (Church, Cafeteria, Library, Dormitories, etc.)
- **Synchronize student records** automatically from an external university system on a daily schedule
- **Issue and manage student ID cards** linked to student records via a unique card UUID
- **Assign domain-scoped roles** (Admin, Manager, Scanner) to control access and responsibilities per location

The system is designed around the concept of **Domains** — physical or logical campus locations — and
**Events** that take place within them. A **Scanner** user assigned to a domain can query which events
are currently active and proceed to scan student cards for attendance.

---

## Technology Stack

| Component         | Technology                              | Version  |
|------------------|-----------------------------------------|----------|
| Framework        | ASP.NET Core Web API                    | .NET 9.0 |
| Database         | PostgreSQL                              | 16       |
| ORM              | Entity Framework Core (Npgsql provider) | 9.0.0    |
| Password Hashing | BCrypt.Net-Next                         | 4.1.0    |
| API Docs         | Scalar (OpenAPI) — development only     | 2.12.41  |
| Containerization | Docker & Docker Compose                 | —        |
| Health Checks    | AspNetCore.HealthChecks.NpgSql          | 9.0.0    |

---

## Project Structure

```
IDMSBackend/
├── BackgroundServices/
│   └── StudentSyncWorker.cs          # Hosted service: daily scheduled student sync
├── Controllers/
│   ├── DomainController.cs           # POST /api/domain, GET /api/domain
│   ├── EventController.cs            # Full event CRUD + scanner query
│   ├── HealthController.cs           # GET /api/health → redirects to /health
│   ├── RoleContoller.cs              # POST/GET/DELETE /api/role
│   ├── StudentCardController.cs      # POST/GET /api/studentcard
│   ├── StudentController.cs          # GET/POST(sync) /api/student
│   ├── UserController.cs             # Full user management /api/user
│   └── UserDomainRoleController.cs   # Role-domain assignment /api/userdomainrole
├── Data/
│   ├── AppDbContext.cs               # EF Core context + automatic audit logging
│   └── DbInitializer.cs             # Seeds domains, roles, and 10 sample students
├── Docker/
│   └── docker-compose.yml           # PostgreSQL 16 container definition
├── DTOs/
│   ├── DomainsDtos.cs               # CreateDomainsDtos, ResponseDomainsDtos
│   ├── EventsDtos.cs                # CreateEventDto, UpdateEventDto, EventDetailsDto, ActiveScanEventDto
│   ├── RoleDtos.cs                  # CreateRoleDto, RoleResponseDto
│   ├── StudentCardsDtos.cs          # CreateStudentCardsDto, StudentCardsDto
│   ├── StudentDtos.cs               # StudentSyncDto, StudentResponseDtos, SyncResultDto
│   ├── UserDomainRoleDtos.cs        # AllocateRoleToUserDtos, UserDomainRoleDtos, RemoveUserRoleDomainDtos
│   └── UsersDtos.cs                 # UserCreateDto, UserUpdateDto, UserResponseDto, ChangePasswordDto, ChangeUserStatusDto
├── Models/
│   ├── AuditLog.cs                  # Audit trail entity
│   ├── Domains.cs                   # Campus domain entity
│   ├── Events.cs                    # Campus event entity (with EventTypes enum)
│   ├── EventsRecurrenceRules.cs     # Recurrence rule entity (with RecurrenceFrequency enum)
│   ├── Roles.cs                     # System role entity
│   ├── StudentCards.cs              # Student ID card entity
│   ├── Students.cs                  # Student record entity (synced from university system)
│   ├── User.cs                      # Application user entity (with UserType, UserStatus enums)
│   └── UserDomainRole.cs            # Junction: User <-> Role <-> Domain
├── Services/
│   ├── Implementations/
│   │   ├── DomainServices.cs
│   │   ├── EventServices.cs
│   │   ├── RoleServices.cs
│   │   ├── StudentCardServices.cs
│   │   ├── StudentServices.cs
│   │   ├── UserDomainRoleServices.cs
│   │   └── UserServices.cs
│   └── Interfaces/
│       ├── IDomainsServices.cs
│       ├── IEventServices.cs
│       ├── IRoleServices.cs
│       ├── IStudentCards.cs
│       ├── IStudentServices.cs
│       ├── IUserDomainRole.cs
│       └── IUserService.cs
├── Wrappers/
│   └── ApiResponse.cs               # Generic API response wrapper
├── Program.cs                        # App startup, DI registration, middleware pipeline
├── appsettings.json                  # Production/shared configuration
├── appsettings.Development.json      # Development overrides
└── IDMSBackend.csproj                # Project file and NuGet dependencies
```

---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started) with [Docker Compose](https://docs.docker.com/compose/) (for PostgreSQL)

---

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/mainavict/IDMSBackend.git
cd IDMSBackend
```

### 2. Start the Database

```bash
cd Docker
docker-compose up -d
cd ..
```

This starts a PostgreSQL 16 container with:

| Setting  | Value          |
|----------|----------------|
| Host     | `localhost`    |
| Port     | `5432`         |
| Database | `appdb`        |
| Username | `appuser`      |
| Password | `strongpassword` |

Data is persisted in a Docker volume named `pgdata`.

### 3. Configure the Application

Edit `appsettings.Development.json` for local development:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=appdb;Username=appuser;Password=strongpassword"
  },
  "SyncSettings": {
    "UniversityBaseUrl": "http://your-university-api-host",
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

URLs available after startup:

| Purpose                       | URL                                      |
|-------------------------------|------------------------------------------|
| API base                      | `http://localhost:5004/api`              |
| Health check                  | `http://localhost:5004/health`           |
| Interactive API docs (dev only)| `http://localhost:5004/scalar`          |
| Raw OpenAPI document (dev only)| `http://localhost:5004/openapi/v1.json` |

> **Development mode auto-seeding**: On first launch, `DbInitializer.Seed()` runs database migrations and
> populates the database with 5 domains, 3 roles, and 10 sample student records if they do not already exist.

---

## Configuration Reference

| Key | Description | Default |
|-----|-------------|---------|
| `ConnectionStrings:DefaultConnection` | Full Npgsql connection string for PostgreSQL | `Host=localhost;Port=5432;...` |
| `SyncSettings:UniversityBaseUrl` | Base URL of the external university API that serves student records | `http://localhost:8000` |
| `SyncSettings:RunHour` | Hour of day (0–23, local server time) when the automatic sync runs | `12` |
| `SyncSettings:RunMinute` | Minute (0–59) within that hour when the sync runs | `10` |
| `Logging:LogLevel:Default` | Minimum log level for all components | `Information` |
| `Logging:LogLevel:Microsoft.AspNetCore` | Minimum log level for ASP.NET Core framework messages | `Warning` |

The sync worker reads `RunHour` and `RunMinute` inside its loop, so you can change them in
`appsettings.json` without restarting the application.

---

## API Reference

### Standard Response Envelope

Every endpoint returns the same JSON wrapper, regardless of success or failure:

```json
{
  "success": true,
  "message": "Human-readable description",
  "data": { },
  "statusCode": 200
}
```

| Field       | Type      | Description                                                      |
|-------------|-----------|------------------------------------------------------------------|
| `success`   | `boolean` | `true` on success, `false` on any error                          |
| `message`   | `string`  | Short description of the outcome                                 |
| `data`      | `object?` | Payload on success; `null` on error                              |
| `statusCode`| `integer` | Mirrors the HTTP status code (200, 201, 400, 404, 500, etc.)    |

---

### Users

Base path: `/api/user`

#### Endpoints

| Method   | Endpoint                          | Description                       |
|----------|-----------------------------------|-----------------------------------|
| `POST`   | `/api/user`                       | Create a new user                 |
| `GET`    | `/api/user`                       | Get all active/suspended users    |
| `GET`    | `/api/user/{userId}`              | Get a single user by GUID         |
| `PUT`    | `/api/user/{userId}`              | Update name or email              |
| `POST`   | `/api/user/{userId}/change-password` | Change password (requires current) |
| `POST`   | `/api/user/{userId}/change-status`   | Set status (active/suspended/deleted) |

#### POST `/api/user` — Create User

**Request body:**

```json
{
  "schoolId": "SJOHND 2311",
  "facultyId": null,
  "userType": "student",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@univ.edu",
  "password": "SecurePass123"
}
```

| Field       | Type     | Required For      | Description                               |
|-------------|----------|-------------------|-------------------------------------------|
| `schoolId`  | `string` | Student users     | Must match an existing `Students.SchoolId`|
| `facultyId` | `string` | Faculty users     | Unique identifier for the faculty member  |
| `userType`  | `string` | Always            | `"student"` or `"faculty"`               |
| `firstName` | `string` | Faculty users     | For students, taken from the Students table |
| `lastName`  | `string` | Faculty users     | For students, taken from the Students table |
| `email`     | `string` | Faculty users     | For students, taken from the Students table |
| `password`  | `string` | Always            | Plain text; stored as BCrypt hash         |

**Successful response (201 — new user):**

```json
{
  "success": true,
  "message": "User created successfully for student",
  "data": {
    "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "schoolId": "SJOHND 2311",
    "facultyId": null,
    "firstName": "John Doe",
    "lastName": "John Doe",
    "userType": "student",
    "status": "active",
    "email": "john.doe@univ.edu",
    "roles": []
  },
  "statusCode": 201
}
```

> **Note:** For student users, both `firstName` and `lastName` are populated with the student's
> `FullName` from the `Students` table (e.g., both will be `"John Doe"`). This is by design in the
> current implementation — the `Students` table stores a single `FullName` field, so both user name
> fields receive the same value.
```

**Successful response (200 — soft-deleted user reactivated):**

```json
{
  "success": true,
  "message": "User reactivated successfully",
  "data": { ... },
  "statusCode": 200
}
```

**Error responses:**

| HTTP | `message`                                          | Cause                                          |
|------|----------------------------------------------------|------------------------------------------------|
| 400  | `"User with this SchoolId already exists"`         | Active/suspended user with same school ID      |
| 400  | `"User with this Email already exists"`            | Active/suspended user with same email          |
| 400  | `"Student with this SchoolId does not exists"`     | No matching record in the Students table       |
| 400  | `"FirstName, LastName, Email, Password and FacultyId are required fields for faculty users"` | Missing faculty fields |
| 400  | `"Invalid user type specified"`                    | `userType` is neither student nor faculty      |
| 500  | `"An error occurred while creating the user..."`   | Unexpected server error                        |

#### GET `/api/user` — Get All Users

Returns all users whose status is `Active` or `Suspended`. Soft-deleted (`Deleted`) users are excluded.

**Successful response (200):**

```json
{
  "success": true,
  "message": "All users retrieved successfully",
  "data": [
    {
      "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "schoolId": "SJOHND 2311",
      "facultyId": null,
      "firstName": "John Doe",
      "lastName": "John Doe",
      "userType": "student",
      "status": "active",
      "email": "john.doe@univ.edu",
      "roles": [
        {
          "roleId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
          "roleName": "Scanner",
          "domainId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
          "domainName": "Cafeteria"
        }
      ]
    }
  ],
  "statusCode": 200
}
```

#### GET `/api/user/{userId}` — Get User by ID

**Error responses:**

| HTTP | `message`             | Cause              |
|------|-----------------------|--------------------|
| 404  | `"User not found"`    | No user with that GUID |
| 500  | `"An error occurred..."`| Unexpected error  |

#### PUT `/api/user/{userId}` — Update User

Only provided fields are updated; omitted or empty/whitespace fields are ignored.

**Request body:**

```json
{
  "firstName": "Jonathan",
  "lastName": "Doe",
  "email": "jonathan.doe@univ.edu"
}
```

#### POST `/api/user/{userId}/change-password` — Change Password

**Request body:**

```json
{
  "currentPassword": "OldPassword123",
  "newPassword": "NewSecurePass456"
}
```

| HTTP | `message`                          | Cause                          |
|------|------------------------------------|--------------------------------|
| 200  | `"Password changed successfully"`  | Success                        |
| 400  | `"Current password is incorrect"`  | BCrypt verification failed     |
| 404  | `"User not found"`                 | No user with that GUID         |

#### POST `/api/user/{userId}/change-status` — Change User Status

**Request body:**

```json
{
  "newStatus": "suspended"
}
```

Valid values for `newStatus`: `"active"`, `"suspended"`, `"deleted"` (case-insensitive via camelCase
JSON converter).

> Setting status to `"deleted"` is a **soft delete** — the user remains in the database but is
> excluded from all list queries. A subsequent `POST /api/user` with the same SchoolId or email
> will reactivate the account instead of creating a duplicate.

---

### Students

Base path: `/api/student`

Students are **read-only** from the API consumer's perspective; the only write path is the sync
endpoint (called by the background worker or manually).

#### Endpoints

| Method | Endpoint                 | Description                              |
|--------|--------------------------|------------------------------------------|
| `GET`  | `/api/student`           | Get all students                         |
| `GET`  | `/api/student/{schoolId}`| Get a student by their school ID string  |
| `POST` | `/api/student/sync`      | Bulk-upsert students from external data  |

#### GET `/api/student` — Get All Students

**Successful response (200):**

```json
{
  "success": true,
  "message": "Students retrieved successfully",
  "data": [
    {
      "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "name": "John Doe",
      "schoolId": "SJOHND 2311",
      "email": "john.doe@univ.edu",
      "yearOfStudy": "1",
      "residence": "Hall A",
      "academicStatus": "Active",
      "lastSyncDate": "2024-09-01T12:10:00Z"
    }
  ],
  "statusCode": 200
}
```

#### GET `/api/student/{schoolId}` — Get Student by School ID

The `{schoolId}` path segment is the student's string school ID (e.g., `SJOHND 2311`), **not** a GUID.

**Error responses:**

| HTTP | `message`              | Cause                       |
|------|------------------------|-----------------------------|
| 404  | `"Student not found"`  | No student with that school ID |

#### POST `/api/student/sync` — Sync Students

Accepts a JSON array of student records from the external university system. The service performs a
smart upsert — inserting new students and updating only records where data has changed.

**Request body** (JSON array, snake_case property names as returned by the university API):

```json
[
  {
    "school_id": "SJOHND 2311",
    "full_name": "John Doe",
    "email": "john.doe@univ.edu",
    "year_of_study": "1",
    "residence": "Hall A",
    "academic_status": "Active"
  },
  {
    "school_id": "SJANES 2312",
    "full_name": "Jane Smith",
    "email": "jane.smith@univ.edu",
    "year_of_study": "2",
    "residence": "Hall B",
    "academic_status": "Active"
  }
]
```

**Successful response (200):**

```json
{
  "success": true,
  "message": "Sync completed: 1 added, 1 updated.",
  "data": {
    "addedCount": 1,
    "updatedCount": 1,
    "unchangedCount": 0,
    "executionTimeMs": 142,
    "syncTimestamp": "2024-09-01T12:10:00Z"
  },
  "statusCode": 200
}
```

**Error responses:**

| HTTP | `message`                                          | Cause                          |
|------|----------------------------------------------------|--------------------------------|
| 400  | `"No student data provided for synchronization."`  | Empty or null request body     |

---

### Student Cards

Base path: `/api/studentcard`

Each student can have at most one card. Creating a card for a student who already has one **updates**
the existing card rather than creating a duplicate.

#### Endpoints

| Method | Endpoint                          | Description                              |
|--------|-----------------------------------|------------------------------------------|
| `POST` | `/api/studentcard`                | Create or update a student card          |
| `GET`  | `/api/studentcard/{schoolId}`     | Get the card for a student by school ID  |

#### POST `/api/studentcard` — Create or Update Card

**Request body:**

```json
{
  "schoolId": "SJOHND 2311",
  "cardUuid": "CARD-UUID-001-ABC",
  "isActive": true
}
```

| Field      | Type     | Required | Description                                          |
|------------|----------|----------|------------------------------------------------------|
| `schoolId` | `string` | Yes      | Must match an existing student in the Students table |
| `cardUuid` | `string` | Yes      | Unique identifier written to the physical card       |
| `isActive` | `bool`   | No       | Defaults to `true`                                   |

**Successful response — new card (201):**

```json
{
  "success": true,
  "message": "Student card created successfully",
  "data": {
    "schoolId": "SJOHND 2311",
    "fullName": "",
    "cardUuid": "CARD-UUID-001-ABC",
    "isActive": true,
    "issuedAt": "2024-09-01T10:00:00Z",
    "expiryDate": "2025-09-01T10:00:00Z",
    "revokedAt": "0001-01-01T00:00:00Z"
  },
  "statusCode": 201
}
```

> **Note:** `fullName` is empty (`""`) in the creation response because the service does not
> populate it on create. It is populated correctly when retrieving a card via
> `GET /api/studentcard/{schoolId}`.

**Successful response — updated card (201):**

```json
{
  "success": true,
  "message": "Student card updated successfully",
  ...
}
```

**Error responses:**

| HTTP | `message`               | Cause                                        |
|------|-------------------------|----------------------------------------------|
| 404  | `"Student not found"`   | No student with that school ID               |
| 500  | `"An error occurred..."` | Unexpected error                            |

#### GET `/api/studentcard/{schoolId}` — Get Card by School ID

**Successful response (200):**

```json
{
  "success": true,
  "message": "Student card retrieved successfully",
  "data": {
    "schoolId": "SJOHND 2311",
    "fullName": "John Doe",
    "cardUuid": "CARD-UUID-001-ABC",
    "isActive": true,
    "issuedAt": "2024-09-01T10:00:00Z",
    "expiryDate": "2025-09-01T10:00:00Z",
    "revokedAt": "0001-01-01T00:00:00Z"
  },
  "statusCode": 200
}
```

**Error responses:**

| HTTP | `message`                   | Cause                              |
|------|-----------------------------|------------------------------------|
| 404  | `"Student not found"`       | No student with that school ID     |
| 404  | `"Student card not found"`  | Student exists but has no card yet |

---

### Roles

Base path: `/api/role`

#### Endpoints

| Method   | Endpoint              | Description            |
|----------|-----------------------|------------------------|
| `POST`   | `/api/role`           | Create a new role      |
| `GET`    | `/api/role`           | Get all roles          |
| `DELETE` | `/api/role/{roleId}`  | Delete a role by GUID  |

#### POST `/api/role` — Create Role

Role names are automatically **title-cased** before saving (e.g., `"scanner"` → `"Scanner"`).
Duplicate names (after normalization) are rejected.

**Request body:**

```json
{
  "name": "Scanner",
  "description": "Restricted scanning access"
}
```

**Successful response (201):**

```json
{
  "success": true,
  "message": "Role created successfully.",
  "data": true,
  "statusCode": 201
}
```

**Error responses:**

| HTTP | `message`                     | Cause                         |
|------|-------------------------------|-------------------------------|
| 400  | `"Role name cannot be empty."` | Empty or whitespace name      |
| 400  | `"Role already exists."`       | Duplicate after normalization |

#### GET `/api/role` — Get All Roles

**Successful response (200):**

```json
{
  "success": true,
  "message": "Roles retrieved successfully.",
  "data": [
    { "id": "A1111111-1111-1111-1111-111111111111", "name": "Admin",   "description": "Full system access" },
    { "id": "B2222222-2222-2222-2222-222222222222", "name": "Manager", "description": "Event and Report management" },
    { "id": "C3333333-3333-3333-3333-333333333333", "name": "Scanner", "description": "Restricted scanning access" }
  ],
  "statusCode": 200
}
```

#### DELETE `/api/role/{roleId}` — Delete Role

**Error responses:**

| HTTP | `message`           | Cause                          |
|------|---------------------|--------------------------------|
| 400  | `"Role not found."` | No role with that GUID (returns success=false, HTTP 200 due to current implementation) |

---

### Domains

Base path: `/api/domain`

Domains represent physical or logical campus locations where events take place and users have roles.

#### Endpoints

| Method | Endpoint       | Description                              |
|--------|----------------|------------------------------------------|
| `POST` | `/api/domain`  | Create a new domain (query param)        |
| `GET`  | `/api/domain`  | Get all domains                          |

#### POST `/api/domain?domainName=Library` — Create Domain

The `domainName` is passed as a **query parameter**, not in the request body.

Domain names are automatically title-cased before saving (e.g., `"library"` → `"Library"`).
Duplicates (after normalization) are rejected.

**Example request:**

```
POST /api/domain?domainName=library
```

**Successful response (201):**

```json
{
  "success": true,
  "message": "Domain created successfully",
  "data": {
    "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "name": "Library"
  },
  "statusCode": 201
}
```

**Error responses:**

| HTTP | `message`                       | Cause                         |
|------|---------------------------------|-------------------------------|
| 400  | `"Domain name cannot be empty"` | Empty or whitespace input     |
| 400  | `"Domain already exists"`       | Duplicate after normalization |
| 500  | `"An error occurred..."`        | Unexpected server error       |

#### GET `/api/domain` — Get All Domains

**Successful response (200):**

```json
{
  "success": true,
  "message": "Domains retrieved successfully",
  "data": [
    { "id": "D1111111-1111-1111-1111-111111111111", "name": "Church" },
    { "id": "E2222222-2222-2222-2222-222222222222", "name": "Cafeteria" },
    { "id": "F3333333-3333-3333-3333-333333333333", "name": "Men's-dorm" },
    { "id": "A4444444-4444-4444-4444-444444444444", "name": "Library" },
    { "id": "B5555555-5555-5555-5555-555555555555", "name": "Lady's-dorm" }
  ],
  "statusCode": 200
}
```

---

### Events

Base path: `/api/event`

Events are tied to a specific Domain and optionally have a recurrence rule. Times are always stored in
UTC but compared against **East Africa Standard Time (UTC+3)** for active-event detection.

#### Endpoints

| Method   | Endpoint                           | Special Header            | Description                                    |
|----------|------------------------------------|---------------------------|------------------------------------------------|
| `POST`   | `/api/event`                       | `creatorId: <userGUID>`   | Create a new event                             |
| `GET`    | `/api/event/domain/{domainId}`     | —                         | Get ALL events for a domain (past and future)  |
| `GET`    | `/api/event/{domainId}`            | —                         | Get currently ACTIVE events for a domain       |
| `GET`    | `/api/event/scanner/{userId}`      | —                         | Get currently active events for a scanner user |
| `PUT`    | `/api/event/{eventId}`             | `updaterId: <userGUID>`   | Update an event                                |
| `DELETE` | `/api/event/{eventId}`             | —                         | Delete an event permanently                    |

#### POST `/api/event` — Create Event

The creator's user ID is passed in the **request header** (`creatorId`), not the body.

**Request body:**

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
  "description": "Daily morning chapel service for all students",
  "domainId": "D1111111-1111-1111-1111-111111111111",
  "isRecurring": true,
  "frequency": "daily",
  "dayOfWeek": null,
  "dayOfMonth": 0,
  "monthOfYear": 0
}
```

| Field             | Type       | Required | Description                                                    |
|-------------------|------------|----------|----------------------------------------------------------------|
| `name`            | `string`   | Yes      | Event name (max 100 chars)                                     |
| `eventType`       | `string`   | Yes      | `cafeteria`, `residence`, `church`, `assembly`, or `exams`     |
| `isCritical`      | `bool`     | Yes      | Whether attendance is mandatory                                |
| `activeFrom`      | `ISO8601`  | Yes      | Start of the event season (stored as UTC)                      |
| `activeUntil`     | `ISO8601`  | Yes      | End of the event season; must be after `activeFrom`            |
| `startTime`       | `"HH:mm"`  | Yes      | Daily start time (24-hour format)                              |
| `endTime`         | `"HH:mm"`  | Yes      | Daily end time                                                 |
| `scanStartOffset` | `int`      | No       | Minutes before `startTime` when scanning may begin. Default: 15|
| `description`     | `string`   | No       | Free-text description (max 500 chars)                          |
| `domainId`        | `GUID`     | Yes      | The domain this event belongs to                               |
| `isRecurring`     | `bool`     | Yes      | Whether a recurrence rule should be created                    |
| `frequency`       | `string`   | If recurring | `daily`, `weekly`, `monthly`, or `yearly`                 |
| `dayOfWeek`       | `int?`     | If weekly | 0=Sunday … 6=Saturday                                         |
| `dayOfMonth`      | `int`      | If monthly | Day of month (1–31)                                           |
| `monthOfYear`     | `int`      | If yearly  | Month number (1–12)                                           |

**Successful response (201):**

```json
{
  "success": true,
  "message": "Event created successfully.",
  "data": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "statusCode": 201
}
```

The `data` field contains the new event's GUID.

**Error responses:**

| HTTP | `message`                                    | Cause                                        |
|------|----------------------------------------------|----------------------------------------------|
| 404  | `"Creator not found."`                       | No user with the given `creatorId`           |
| 403  | `"Creator is not active."`                   | Creator's status is Suspended or Deleted     |
| 400  | `"ActiveFrom must be before ActiveUntil."`   | Invalid date range                           |
| 400  | (BadRequest from controller)                 | General validation failure                   |

#### GET `/api/event/domain/{domainId}` — All Events for Domain

Returns all events (past, present, future) for the specified domain, with full details including
recurrence information.

**Successful response (200):**

```json
{
  "success": true,
  "message": "Events retrieved successfully.",
  "data": [
    {
      "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "name": "Morning Chapel",
      "eventType": "church",
      "isCritical": true,
      "activeFrom": "2024-08-01T00:00:00Z",
      "activeUntil": "2025-05-31T00:00:00Z",
      "startTime": "07:00",
      "endTime": "08:30",
      "scanStartOffset": 15,
      "description": "Daily morning chapel service",
      "domainName": "Church",
      "isRecurring": true,
      "frequency": "daily",
      "dayOfWeek": null,
      "dayOfMonth": 0,
      "monthOfYear": 0
    }
  ],
  "statusCode": 200
}
```

#### GET `/api/event/{domainId}` — Active Events for Domain

Returns only events that are currently active: today falls within `[ActiveFrom, ActiveUntil]` and the
current East Africa time falls within `[StartTime, EndTime]`.

#### GET `/api/event/scanner/{userId}` — Active Events for Scanner

Returns events currently active (using UTC time comparison) in domains where the specified user holds
the **Scanner** role.

**Requirements:**
- User must exist and have `Status = Active`
- User must have at least one `UserDomainRole` with `Role.Name = "Scanner"`

**Successful response (200):**

```json
{
  "success": true,
  "message": "Active events retrieved successfully.",
  "data": [
    {
      "eventId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "name": "Lunch Service",
      "domainName": "Cafeteria",
      "startTime": "12:00",
      "endTime": "14:00",
      "isCritical": true
    }
  ],
  "statusCode": 200
}
```

#### PUT `/api/event/{eventId}` — Update Event

The updater's user ID is passed in the **request header** (`updaterId`).

Partial updates are supported: if `Name` or `Description` are empty/whitespace they are not changed.
If `isRecurring` changes from `true` to `false`, the associated recurrence rule record is deleted.

**Request body:** Same fields as `CreateEventDto` minus `domainId`.

**Error responses:**

| HTTP | `message`                                                           | Cause                               |
|------|---------------------------------------------------------------------|-------------------------------------|
| 404  | `"Event not found."`                                                | No event with that GUID             |
| 403  | `"Invalid or inactive updater."`                                    | Updater not found or not active     |
| 400  | `"ActiveFrom must be before ActiveUntil."`                          | Invalid date range                  |
| 400  | `"Frequency and DayOfWeek are required for recurring events."`      | `isRecurring=true` but no frequency |

#### DELETE `/api/event/{eventId}` — Delete Event

Permanently removes the event and its recurrence rule (via cascade). Returns HTTP 200 even if the
event is not found (check `success` field).

---

### User Domain Roles

Base path: `/api/userdomainrole`

Manages the many-to-many relationship between users, roles, and domains.

#### Endpoints

| Method   | Endpoint                                   | Description                                |
|----------|--------------------------------------------|--------------------------------------------|
| `POST`   | `/api/userdomainrole/assign-role-domain`   | Assign a role to a user in a domain       |
| `GET`    | `/api/userdomainrole?userId={guid}`        | Get all role-domain assignments for a user |
| `DELETE` | `/api/userdomainrole/remove-role-domain`   | Remove a specific role-domain assignment  |
| `GET`    | `/api/userdomainrole/get-users-by-domain?domainName={name}` | Get all users with roles in a domain |

#### POST `/api/userdomainrole/assign-role-domain` — Assign Role

Roles and domains are looked up by **name** (case-sensitive after creation, but domain names are
title-cased on creation).

**Request body:**

```json
{
  "userId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "role": "Scanner",
  "domain": "Cafeteria"
}
```

**Successful response (200):**

```json
{
  "success": true,
  "message": "Role assigned to user in domain successfully",
  "data": {
    "userId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "fullName": "John Doe",
    "roleId": "C3333333-3333-3333-3333-333333333333",
    "roleName": "Scanner",
    "domainId": "E2222222-2222-2222-2222-222222222222",
    "domainName": "Cafeteria",
    "assignedAt": "2024-09-01T08:00:00Z"
  },
  "statusCode": 200
}
```

**Error responses:**

| HTTP | `message`                                              | Cause                           |
|------|--------------------------------------------------------|---------------------------------|
| 400  | `"Input data cannot be null"`                          | Null request body               |
| 404  | `"User not found"`                                     | Invalid userId                  |
| 404  | `"Role not found"`                                     | Role name does not exist        |
| 404  | `"Domain not found"`                                   | Domain name does not exist      |
| 400  | `"User already has the role assigned in the domain"`   | Duplicate assignment            |

#### GET `/api/userdomainrole?userId={guid}` — Get User Roles

Returns all role-domain pairs for the given user.

**Successful response (200):**

```json
{
  "success": true,
  "message": "User roles and domains retrieved successfully",
  "data": [
    {
      "userId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "fullName": "",
      "roleId": "C3333333-3333-3333-3333-333333333333",
      "roleName": "Scanner",
      "domainId": "E2222222-2222-2222-2222-222222222222",
      "domainName": "Cafeteria",
      "assignedAt": "0001-01-01T00:00:00Z"
    }
  ],
  "statusCode": 200
}
```

> **Note:** `fullName` and `assignedAt` are not populated by `GetUserRolesAndDomainsAsync` — they
> appear as empty string and the default `DateTime` value (`0001-01-01`) respectively. These fields
> are fully populated in the assignment response (`POST assign-role-domain`) and in the
> `get-users-by-domain` query.

#### DELETE `/api/userdomainrole/remove-role-domain` — Remove Role Assignment

**Request body:**

```json
{
  "userId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "role": "Scanner",
  "domain": "Cafeteria"
}
```

#### GET `/api/userdomainrole/get-users-by-domain?domainName={name}` — Users by Domain

The `domainName` query parameter is automatically normalized (trimmed and title-cased) before lookup.

**Successful response (200):**

```json
{
  "success": true,
  "message": "Users with roles retrieved successfully for domain",
  "data": [
    {
      "userId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "fullName": "John Doe",
      "roleId": "C3333333-3333-3333-3333-333333333333",
      "roleName": "Scanner",
      "domainId": "E2222222-2222-2222-2222-222222222222",
      "domainName": "Cafeteria",
      "assignedAt": "2024-09-01T08:00:00Z"
    }
  ],
  "statusCode": 200
}
```

---

### Health

| Method | Endpoint       | Description                                        |
|--------|----------------|----------------------------------------------------|
| `GET`  | `/health`      | PostgreSQL connectivity check with JSON response   |
| `GET`  | `/api/health`  | Redirects (302) to `/health`                       |

**Successful response (200):**

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
  "duration": "00:00:00.0045123"
}
```

**Degraded/unhealthy response (503):**

```json
{
  "status": "Unhealthy",
  "checks": [
    {
      "component": "PostgreSQL",
      "status": "Unhealthy",
      "description": "An exception occurred while checking the NpgSql."
    }
  ],
  "duration": "00:00:05.0010000"
}
```

---

## Business Logic and Validation Rules

### User Creation Rules

The `CreateUserAsync` method enforces the following rules in order:

1. **Duplicate SchoolId check** (student users only): if a user with the same `SchoolId` already
   exists and is `Active` or `Suspended`, creation is rejected with HTTP 400. If the existing user
   is `Deleted`, it is **reactivated** with a new password hash and returned with HTTP 200.

2. **Duplicate email check** (all users): if a user with the same email already exists and is
   `Active` or `Suspended`, creation is rejected. If `Deleted`, the user is reactivated.

3. **Student must exist in Students table**: for `userType = "student"`, the `SchoolId` must match
   a record already in `Students`. The user's name and email are copied from the `Students` record,
   not from the request body.

4. **Faculty required fields**: for `userType = "faculty"`, `FirstName`, `LastName`, `Email`,
   `Password`, and `FacultyId` must all be non-empty.

5. **Password hashing**: passwords are never stored in plain text. BCrypt with the default work
   factor is applied via `BCrypt.Net.BCrypt.HashPassword()`.

### Event Creation and Detection Rules

**Creation validation:**
- The creator must exist and have `Status = Active`
- `ActiveFrom` must be strictly before `ActiveUntil`
- If `IsRecurring = true`, a `Frequency` value is required

**Active event detection (scanner query):**
- Today's UTC date must fall within `[Event.ActiveFrom.Date, Event.ActiveUntil.Date]`
- The current UTC time must fall within `[Event.StartTime, Event.EndTime]`
- The event's domain must be one where the querying user has the `Scanner` role

**Active event detection (domain query — `GET /api/event/{domainId}`):**
- `Event.ActiveFrom <= DateTime.UtcNow <= Event.ActiveUntil` (full date+time)
- The current **East Africa Standard Time** (UTC+3) must fall within `[StartTime, EndTime]`

**Update recurrence logic:**
- If the event is updated with `isRecurring = false` and it previously had a recurrence rule,
  the `EventsRecurrenceRules` record is permanently deleted.
- If updated with `isRecurring = true` and there is no existing rule, a new one is created.
- If there is already a rule, it is updated in place.

### Student Sync Algorithm

The sync endpoint (`POST /api/student/sync`) uses an optimized upsert strategy:

```
1. Load all existing students from the DB as a Dictionary<SchoolId, {Id, fields...}>
   (projection-only query to minimize memory)

2. For each incoming record from the external API:
   a. If SchoolId EXISTS in the dictionary:
      - Compare FullName, Email, Residence, YearOfStudy, AcademicStatus
      - If ANY field has changed: attach a stub entity, set only the changed fields,
        mark as Modified, increment updatedCount
      - If nothing changed: skip (no DB write)
   b. If SchoolId does NOT exist:
      - Add new Students entity, increment addedCount

3. Every 50 records: call SaveChangesAsync() and clear the ChangeTracker to keep
   memory usage constant regardless of dataset size.

4. After the loop: final SaveChangesAsync() for remaining records.

5. Return SyncResultDto with addedCount, updatedCount, and executionTimeMs.
```

This approach avoids loading full entity graphs into memory, uses O(1) dictionary lookups,
and prevents `DbUpdateConcurrencyException` from large batch operations.

### Student Card Rules

- A student card is always linked to a `Students` record via `StudentId` (FK).
- Only one card per student: if a card already exists, `CreateStudentCardAsync` updates it
  instead of inserting.
- Card expiry is automatically set to **1 year from the issue date**.
- The `CardUuid` must be unique across all student cards (unique index in the database).

### Role and Domain Name Normalization

Both `RoleServices.CreateRoleAsync` and `DomainServices.CreateDomainAsync` apply the same
normalization before saving or checking for duplicates:

```
normalizedName = Trim the input
normalizedName = UpperCase the first character + LowerCase the rest
```

Examples:
- `"SCANNER"` → `"Scanner"`
- `"  cafeteria  "` → `"Cafeteria"`

The `GetAllDomainusersByDomainNameAsync` method applies the same normalization to the
query parameter before searching.

---

## Database Schema

### Entity-Relationship Diagram

```
┌─────────────┐       ┌──────────────────────┐       ┌────────────┐
│    Users    │──1:N──│   UserDomainRoles    │──N:1──│   Roles    │
│─────────────│       │──────────────────────│       │────────────│
│ Id (PK)     │       │ Id (PK)              │       │ Id (PK)    │
│ SchoolId    │       │ UserId (FK→Users)    │       │ Name       │
│ FacultyId   │       │ RoleId (FK→Roles)    │       │ Description│
│ FirstName   │       │ DomainId (FK→Domains)│       │ CreatedAt  │
│ LastName    │       │ AssignedAt           │       └────────────┘
│ Email       │       └──────────────────────┘
│ PasswordHash│                 │N:1
│ UserType    │       ┌─────────▼──────────┐
│ Status      │       │      Domains       │
│ CreatedAt   │       │────────────────────│
│ UpdatedAt   │       │ Id (PK)            │
└──────┬──────┘       │ Name               │
       │              └─────────┬──────────┘
       │1:N                     │1:N
       ▼                        ▼
┌─────────────┐       ┌─────────────────────┐
│   Events    │──N:1──│                     │
│─────────────│(DomainId)                   │
│ Id (PK)     │       └─────────────────────┘
│ Name        │
│ EventType   │──1:1──┌──────────────────────────┐
│ IsCritical  │       │  EventsRecurrenceRules    │
│ ActiveFrom  │       │──────────────────────────│
│ ActiveUntil │       │ Id (PK)                  │
│ StartTime   │       │ EventId (FK→Events)      │
│ EndTime     │       │ Frequency                │
│ IsRecurring │       │ Interval                 │
│ ScanOffset  │       │ DayOfWeek                │
│ Description │       │ DayOfMonth               │
│ CreatedBy   │       │ MonthOfYear              │
│ DomainId    │       └──────────────────────────┘
│ UpdatedBy   │
└─────────────┘

┌──────────────┐──1:1──┌──────────────────┐
│   Students   │       │  StudentCards    │
│──────────────│       │──────────────────│
│ Id (PK)      │       │ Id (PK)          │
│ SchoolId     │       │ StudentId (FK)   │
│ FullName     │       │ CardUuid         │
│ Email        │       │ IsActive         │
│ YearOfStudy  │       │ IssuedAt         │
│ Residence    │       │ ExpiryDate       │
│ AcademicStatus│      │ RevokedAt        │
│ CreatedAt    │       └──────────────────┘
│ UpdatedAt    │
│ LastSyncDate │
└──────────────┘

┌───────────────────────────────────────────────────────┐
│                      AuditLogs                         │
│───────────────────────────────────────────────────────│
│ Id (PK)  UserId  Action  Details  IpAddress            │
│ UserAgent  TraceId  Timestamp                          │
└───────────────────────────────────────────────────────┘
```

### Tables and Columns

#### `Users`

| Column         | Type         | Nullable | Default              | Notes                    |
|----------------|--------------|----------|----------------------|--------------------------|
| `Id`           | `uuid`       | No       | `gen_random_uuid()`  | Primary key              |
| `SchoolId`     | `varchar(100)`| Yes     | —                    | Unique (conditional)     |
| `FacultyId`    | `varchar(100)`| Yes     | —                    | Unique (conditional)     |
| `userType`     | `varchar`    | No       | —                    | Column name is `userType` (camelCase); stored as string via EF Core `HasConversion<string>()` |
| `FirstName`    | `text`       | No       | —                    |                          |
| `LastName`     | `text`       | No       | —                    |                          |
| `Status`       | `varchar`    | No       | —                    | `Active`/`Suspended`/`Deleted` |
| `PasswordHash` | `text`       | No       | `''`                 | BCrypt hash              |
| `Email`        | `text`       | No       | `''`                 | Unique index             |
| `CreatedAt`    | `timestamptz`| No       | `CURRENT_TIMESTAMP`  |                          |
| `UpdatedAt`    | `timestamptz`| No       | `now()`              |                          |

#### `Students`

| Column           | Type          | Nullable | Notes            |
|------------------|---------------|----------|------------------|
| `Id`             | `uuid`        | No       | Primary key      |
| `SchoolId`       | `varchar(100)`| No       | Unique index     |
| `FullName`       | `varchar(200)`| No       |                  |
| `Email`          | `varchar(200)`| Yes      |                  |
| `YearOfStudy`    | `varchar(100)`| Yes      |                  |
| `Residence`      | `varchar(100)`| Yes      |                  |
| `AcademicStatus` | `varchar(100)`| Yes      |                  |
| `CreatedAt`      | `timestamptz` | No       |                  |
| `UpdatedAt`      | `timestamptz` | No       |                  |
| `LastSyncDate`   | `timestamptz` | Yes      |                  |

#### `StudentCards`

| Column       | Type         | Nullable | Notes                      |
|--------------|--------------|----------|----------------------------|
| `Id`         | `uuid`       | No       | Primary key                |
| `StudentId`  | `uuid`       | No       | FK → Students (Cascade)    |
| `CardUuid`   | `text`       | No       | Unique index               |
| `IsActive`   | `bool`       | No       |                            |
| `IssuedAt`   | `timestamptz`| No       | Default: `now()`           |
| `ExpiryDate` | `timestamptz`| No       | Set to +1 year on creation |
| `RevokedAt`  | `timestamptz`| No       |                            |

#### `Events`

| Column            | Type          | Nullable | Notes                          |
|-------------------|---------------|----------|--------------------------------|
| `Id`              | `uuid`        | No       | Primary key                    |
| `Name`            | `varchar(100)`| No       |                                |
| `EventType`       | `varchar`     | No       | Stored as string               |
| `IsCritical`      | `bool`        | No       | Default: `true`                |
| `ActiveFrom`      | `timestamptz` | No       |                                |
| `ActiveUntil`     | `timestamptz` | No       |                                |
| `StartTime`       | `time`        | No       |                                |
| `EndTime`         | `time`        | No       |                                |
| `IsRecurring`     | `bool`        | No       | Default: `false`               |
| `ScanStartOffset` | `int`         | No       | Default: `15`                  |
| `Description`     | `varchar(500)`| No       |                                |
| `CreatedBy`       | `uuid`        | No       | FK → Users (Cascade)           |
| `DomainId`        | `uuid`        | No       | FK → Domains (Restrict)        |
| `UpdatedBy`       | `uuid`        | Yes      | FK → Users (Restrict)          |
| `CreatedAt`       | `timestamptz` | No       |                                |
| `UpdatedAt`       | `timestamptz` | No       |                                |

#### `EventsRecurrenceRules`

| Column        | Type      | Nullable | Notes                    |
|---------------|-----------|----------|--------------------------|
| `Id`          | `uuid`    | No       | Primary key              |
| `EventId`     | `uuid`    | No       | FK → Events (Cascade)    |
| `Frequency`   | `varchar` | No       | Stored as string         |
| `Interval`    | `int`     | No       | Default: `1`             |
| `DayOfWeek`   | `int`     | Yes      | 0=Sunday … 6=Saturday    |
| `DayOfMonth`  | `int`     | No       |                          |
| `MonthOfYear` | `int`     | No       |                          |

#### `Roles`

| Column        | Type          | Nullable | Notes           |
|---------------|---------------|----------|-----------------|
| `Id`          | `uuid`        | No       | Primary key     |
| `Name`        | `varchar(50)` | No       | Unique index    |
| `Description` | `text`        | No       |                 |
| `CreatedAt`   | `timestamptz` | No       |                 |

#### `Domains`

| Column | Type   | Nullable | Notes       |
|--------|--------|----------|-------------|
| `Id`   | `uuid` | No       | Primary key |
| `Name` | `text` | No       |             |

#### `UserDomainRoles`

| Column       | Type         | Nullable | Notes                         |
|--------------|--------------|----------|-------------------------------|
| `Id`         | `uuid`       | No       | Primary key                   |
| `UserId`     | `uuid`       | No       | FK → Users (Cascade)          |
| `RoleId`     | `uuid`       | No       | FK → Roles (Cascade)          |
| `DomainId`   | `uuid`       | No       | FK → Domains (Cascade)        |
| `AssignedAt` | `timestamptz`| No       |                               |

#### `AuditLogs`

| Column      | Type         | Nullable | Description                          |
|-------------|--------------|----------|--------------------------------------|
| `Id`        | `uuid`       | No       | Primary key                          |
| `UserId`    | `text`       | Yes      | Identity name from HTTP context, or "System/Sync" |
| `Action`    | `text`       | No       | EF change state: `Added`, `Modified`, `Deleted` |
| `Details`   | `text`       | Yes      | `"Table: <TableName> | Action by <UserId>"` |
| `IpAddress` | `text`       | Yes      | Remote IP address of the request     |
| `UserAgent` | `text`       | Yes      | `User-Agent` request header value    |
| `TraceId`   | `text`       | Yes      | ASP.NET Core `TraceIdentifier`       |
| `Timestamp` | `timestamptz`| No       | UTC time the change was committed    |

### Indexes and Constraints

| Table            | Column(s)           | Type          | Condition                                     |
|------------------|---------------------|---------------|-----------------------------------------------|
| `Users`          | `SchoolId`          | Unique        | `WHERE "SchoolId" IS NOT NULL AND "SchoolId" <> ''` |
| `Users`          | `FacultyId`         | Unique        | `WHERE "FacultyId" IS NOT NULL AND "FacultyId" <> ''` |
| `Users`          | `Email`             | Unique        | —                                             |
| `Students`       | `SchoolId`          | Unique        | —                                             |
| `StudentCards`   | `CardUuid`          | Unique        | —                                             |
| `Roles`          | `Name`              | Unique        | —                                             |

### Foreign-Key Cascade Rules

| Relationship                           | On Delete     |
|----------------------------------------|---------------|
| `UserDomainRoles` → `Users`            | **Cascade**   |
| `UserDomainRoles` → `Roles`            | **Cascade**   |
| `UserDomainRoles` → `Domains`          | **Cascade**   |
| `Events.CreatedBy` → `Users`           | **Cascade**   |
| `Events.UpdatedBy` → `Users`           | **Restrict**  |
| `Events.DomainId` → `Domains`          | **Restrict**  |
| `EventsRecurrenceRules.EventId` → `Events` | **Cascade** |
| `StudentCards.StudentId` → `Students`  | **Cascade**   |

---

## Seeded Reference Data

In development mode (`ASPNETCORE_ENVIRONMENT=Development`), the following records are inserted once
(if they do not already exist) when the application starts:

### Domains

| ID (fixed)                               | Name          |
|------------------------------------------|---------------|
| `D1111111-1111-1111-1111-111111111111`   | Church        |
| `E2222222-2222-2222-2222-222222222222`   | Cafeteria     |
| `F3333333-3333-3333-3333-333333333333`   | Men's-dorm    |
| `A4444444-4444-4444-4444-444444444444`   | Library       |
| `B5555555-5555-5555-5555-555555555555`   | Lady's-dorm   |

### Roles

| ID (fixed)                               | Name    | Description                     |
|------------------------------------------|---------|---------------------------------|
| `A1111111-1111-1111-1111-111111111111`   | Admin   | Full system access              |
| `B2222222-2222-2222-2222-222222222222`   | Manager | Event and Report management     |
| `C3333333-3333-3333-3333-333333333333`   | Scanner | Restricted scanning access      |

### Sample Students (10 records, IDs generated at seed time)

| School ID       | Full Name         | Year | Residence    | Status    |
|-----------------|-------------------|------|--------------|-----------|
| SJOHND 2311     | John Doe          | 1    | Hall A       | Active    |
| SJANES 2312     | Jane Smith        | 2    | Hall B       | Active    |
| SMIKER 2313     | Mike Ross         | 1    | Off-Campus   | Active    |
| SRACHZ 2314     | Rachel Zane       | 3    | Hall A       | Active    |
| SHARVS 2315     | Harvey Specter    | 4    | Off-Campus   | Active    |
| SDONNP 2316     | Donna Paulsen     | 2    | Hall C       | Active    |
| SLOUIS 2317     | Louis Litt        | 3    | Hall B       | Probation |
| SJESSP 2318     | Jessica Pearson   | 4    | Off-Campus   | Active    |
| SKATRB 2319     | Katrina Bennett   | 1    | Hall C       | Active    |
| SALEXW 2320     | Alex Williams     | 2    | Hall A       | Suspended |

---

## Architecture

### Dependency Injection Map

All services are registered in `Program.cs` with a **Scoped** lifetime (one instance per HTTP request),
except for the background worker which is a **Hosted Service** (singleton process).

```
IStudentCards       → StudentCardServices
IUserService        → UserServices
IRoleServices       → RoleServices
IStudentServices    → StudentServices
IDomainsServices    → DomainServices
IUserDomainRole     → UserDomainRoleServices
IEventServices      → EventServices
BackgroundService   → StudentSyncWorker (singleton hosted service)
HttpClient("UniversityApi") → configured with BaseAddress from SyncSettings:UniversityBaseUrl
```

All service implementations receive `AppDbContext` and `ILogger<T>` via constructor injection.

### Request Pipeline

```
HTTP Request
    │
    ├─ HTTPS Redirection
    ├─ Authorization middleware (placeholder, not yet configured)
    ├─ Controller routing → [ApiController] → [Route("api/[controller]")]
    │
    ▼
Controller action
    │
    ├─ Calls Service (IXxxService)
    │       │
    │       ├─ Business logic & validation
    │       ├─ EF Core queries (AppDbContext)
    │       │       │
    │       │       └─ SaveChangesAsync()
    │       │               │
    │       │               └─ OnBeforeSaveChanges() → AuditLog entries created
    │       │
    │       └─ Returns ApiResponse<T>
    │
    └─ Controller returns StatusCode(response.StatusCode, response)
```

### Response Format

All API responses use the `ApiResponse<T>` generic wrapper (`Wrappers/ApiResponse.cs`):

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public int StatusCode { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Success", int statusCode = 200)
        => new() { Success = true, Data = data, Message = message, StatusCode = statusCode };

    public static ApiResponse<T> FailureResponse(string message, int statusCode = 400)
        => new() { Success = false, Message = message, StatusCode = statusCode };
}
```

Enum values in JSON are serialized as **camelCase strings** (configured via `JsonStringEnumConverter`
in `Program.cs`), so `UserStatus.Active` becomes `"active"` in the response.

### Audit Logging

Every call to `AppDbContext.SaveChangesAsync()` automatically records all pending entity changes to
the `AuditLogs` table **before** they are committed. This happens in `OnBeforeSaveChanges()`:

1. The method iterates over `ChangeTracker.Entries()` (snapshot to avoid mutation errors).
2. `AuditLog` entries and unchanged entities are skipped.
3. For each changed entity, an `AuditLog` record is created capturing:
   - **UserId**: from `HttpContext.User.Identity.Name`, or `"System/Sync"` for background jobs
   - **Action**: `"Added"`, `"Modified"`, or `"Deleted"` (from EF `EntityState`)
   - **IpAddress**: remote IP from `HttpContext.Connection.RemoteIpAddress`
   - **UserAgent**: `User-Agent` request header
   - **TraceId**: ASP.NET Core's `HttpContext.TraceIdentifier`
   - **Details**: `"Table: <TableName> | Action by <UserId>"`
   - **Timestamp**: UTC timestamp
4. All new `AuditLog` records are added to the context's `AuditLogs` set.
5. `base.SaveChangesAsync()` persists both the original changes and the audit entries in one transaction.

### Background Services

#### StudentSyncWorker

`BackgroundServices/StudentSyncWorker.cs` extends `BackgroundService` and runs as a hosted service.

**Scheduling logic:**

```
1. Read RunHour and RunMinute from configuration (re-read every iteration, so config changes
   take effect without restart).
2. Calculate nextRunTime = today at HH:MM (local server time).
3. If now > nextRunTime, add 1 day (schedule for tomorrow).
4. Wait (Task.Delay) until nextRunTime.
5. Execute sync.
6. Wait an additional 30 seconds to prevent double-triggering.
7. Loop back to step 1.
```

**Sync execution:**

```
1. Create a new DI scope (StudentSyncWorker is a singleton; services are scoped).
2. Resolve IStudentServices from the scope.
3. Call FetchExternalDataAsync():
   - Uses the named HttpClient "UniversityApi" (base URL: SyncSettings:UniversityBaseUrl)
   - Makes GET /students
   - Deserializes the response as List<StudentSyncDto>
4. If data is non-empty, call studentService.SyncStudentsAsync(data).
5. Log the result: Added count, Updated count.
```

**Error handling**: Any exception during sync (other than `OperationCanceledException`) is caught,
logged, and the worker continues to the next scheduled run.

**Timeout**: The `HttpClient` named `"UniversityApi"` has a 30-minute timeout to accommodate
downloading large datasets (5,000+ records) from the external system.

---

## Common Use-Case Flows

### Onboarding a New Scanner User

Goal: Create a user account for student `SJOHND 2311` and assign them as a Scanner in the Cafeteria.

**Step 1 — Verify the student exists:**
```
GET /api/student/SJOHND 2311
→ 200 OK: student record found
```

**Step 2 — Create the user account:**
```
POST /api/user
Body: { "schoolId": "SJOHND 2311", "userType": "student", "password": "Pass1234" }
→ 201 Created: user GUID returned in data.id
```

**Step 3 — Assign the Scanner role in Cafeteria:**
```
POST /api/userdomainrole/assign-role-domain
Body: {
  "userId": "<user GUID from step 2>",
  "role": "Scanner",
  "domain": "Cafeteria"
}
→ 200 OK: assignment confirmed
```

**Step 4 — Verify the assignment:**
```
GET /api/userdomainrole?userId=<user GUID>
→ 200 OK: shows Scanner role in Cafeteria domain
```

### Creating a Recurring Event

Goal: Create a daily lunch event in the Cafeteria domain, active for the academic year.

**Step 1 — Note the Cafeteria domain ID:** `E2222222-2222-2222-2222-222222222222`

**Step 2 — Create the event (as an Admin/Manager user):**
```
POST /api/event
Header: creatorId: <admin user GUID>
Body:
{
  "name": "Lunch Service",
  "eventType": "cafeteria",
  "isCritical": true,
  "activeFrom": "2024-08-01T00:00:00Z",
  "activeUntil": "2025-05-31T00:00:00Z",
  "startTime": "12:00",
  "endTime": "14:00",
  "scanStartOffset": 10,
  "description": "Daily student lunch service",
  "domainId": "E2222222-2222-2222-2222-222222222222",
  "isRecurring": true,
  "frequency": "daily",
  "dayOfWeek": null,
  "dayOfMonth": 0,
  "monthOfYear": 0
}
→ 201 Created: event GUID returned
```

**Step 3 — Verify all events for Cafeteria:**
```
GET /api/event/domain/E2222222-2222-2222-2222-222222222222
→ 200 OK: lists the new event with recurrence details
```

### Scanner App Getting Active Events

Goal: A scanner application queries which events are currently happening for the logged-in scanner user.

**Step 1 — Query active events for the scanner:**
```
GET /api/event/scanner/<scanner user GUID>
→ 200 OK (if within event time window):
{
  "data": [
    {
      "eventId": "...",
      "name": "Lunch Service",
      "domainName": "Cafeteria",
      "startTime": "12:00",
      "endTime": "14:00",
      "isCritical": true
    }
  ]
}
→ 200 OK (outside event time window):
{ "data": [] }
```

The scanner app should poll this endpoint at the start of a session to know which event(s) to scan for.

---

## Error Handling Reference

All services follow a consistent exception-handling pattern:

```csharp
try
{
    // business logic
}
catch (Exception ex)
{
    _logger.LogError(ex, "Context-specific message with {Params}", param);
    return ApiResponse<T>.FailureResponse("User-facing message", 500);
}
```

Common HTTP status codes returned:

| Status | Meaning                      | Common Causes                                                |
|--------|------------------------------|--------------------------------------------------------------|
| 200    | OK                           | Successful read or update                                    |
| 201    | Created                      | Successful creation of a new resource                        |
| 400    | Bad Request                  | Validation failure, duplicate, missing required field        |
| 403    | Forbidden                    | User is inactive or unauthorized                             |
| 404    | Not Found                    | Resource with the given ID/name does not exist               |
| 500    | Internal Server Error        | Unhandled exception (details logged server-side)             |

> Note: Some operations (e.g., Delete Role, Delete Event) return HTTP 200 with `success: false`
> instead of HTTP 404 when the resource is not found. Always check both the HTTP status and the
> `success` field in the response envelope.

---

## Development Tools

### Interactive API Reference (Scalar)

Available only in `Development` environment at:

```
http://localhost:5004/scalar
```

Configured in `Program.cs` with:
- **Theme**: Moon
- **Default HTTP client**: C# `HttpClient`
- **Title**: IDMS Backend API

### Raw OpenAPI Document

```
http://localhost:5004/openapi/v1.json
```

### Entity Framework Migrations

To add a new migration after model changes:

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

To revert to a specific migration:

```bash
dotnet ef database update <MigrationName>
```

### Health Check Endpoint

Use `/health` to verify the application and database are running:

```bash
curl http://localhost:5004/health
```

A `"Healthy"` response confirms PostgreSQL connectivity. A `"Unhealthy"` response indicates a
database connection problem — verify Docker is running and the connection string is correct.
