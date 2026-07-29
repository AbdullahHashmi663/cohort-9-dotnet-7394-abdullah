# Task Management Tool

A full-stack web application for managing tasks with user authentication, role-based access, and real-time dashboard.

## Technology Stack

| Layer | Technology |
|-------|-----------|
| **Backend** | ASP.NET Core 10 (Web API) |
| **Frontend** | React 19 + Vite |
| **Database** | SQL Server + Entity Framework Core |
| **Auth** | JWT Bearer Authentication + BCrypt |
| **Logging** | Serilog (Console + File) |
| **Testing** | xUnit + Moq + EF InMemory |
| **Code Quality** | SonarQube |
| **Version Control** | Git + GitHub |

## Project Structure

```
├── TaskManager.API/          # ASP.NET Core Web API
│   ├── Controllers/          # API endpoints (Auth, Tasks, User)
│   ├── DTOs/                 # Data Transfer Objects
│   ├── Data/                 # EF DbContext
│   ├── Middleware/           # Global exception handling
│   ├── Models/               # Entity models (User, TaskItem)
│   └── Services/             # Business logic layer
├── TaskManager.UI/           # React + Vite frontend
│   └── src/
│       ├── api/              # Axios instance with JWT interceptor
│       ├── components/       # Layout, ProtectedRoute
│       ├── context/          # AuthContext (state management)
│       └── pages/            # All application screens
├── TaskManager.Tests/        # xUnit unit tests
│   ├── Helpers/              # Test utilities
│   ├── Middleware/           # Middleware tests
│   └── Services/             # Service layer tests
└── sonar-project.properties  # SonarQube configuration
```

## Features

- **User Authentication & Authorization** — Registration, login with JWT, role-based access (Admin/User)
- **Task CRUD** — Create, read, update, soft-delete tasks with priority, status, category, and due dates
- **Dashboard** — Task counts by status (Pending, InProgress, Completed)
- **Role-Based Access** — Admin sees all tasks; regular users see only their own
- **Global Exception Handling** — Centralized error handling with structured JSON responses
- **Serilog Logging** — Console + rolling file logging throughout the application
- **Search & Filter** — Client-side search by title/description/category, filter by status and priority
- **Soft Delete** — Tasks are marked as deleted rather than permanently removed

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server) (LocalDB or Express)

### Backend Setup

```bash
cd TaskManager.API

# Update connection string in appsettings.json if needed

# Apply migrations
dotnet ef database update

# Run the API
dotnet run
```

The API will run on `https://localhost:7001`.

### Frontend Setup

```bash
cd TaskManager.UI

# Install dependencies
npm install

# Start dev server
npm run dev
```

The UI will run on `http://localhost:5173`.

### Running Tests

```bash
dotnet test TaskManager.Tests/TaskManager.Tests.csproj
```

### SonarQube Analysis

```bash
# Install SonarScanner
dotnet tool install --global dotnet-sonarscanner

# Start analysis
dotnet sonarscanner begin /k:"TaskManager" /d:sonar.host.url="http://localhost:9000" /d:sonar.token="YOUR_TOKEN"
dotnet build
dotnet sonarscanner end /d:sonar.token="YOUR_TOKEN"
```

## API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | No | Register a new user |
| POST | `/api/auth/login` | No | Login and get JWT token |
| GET | `/api/tasks` | Yes | Get all tasks (user-scoped) |
| GET | `/api/tasks/{id}` | Yes | Get task by ID |
| GET | `/api/tasks/dashboard` | Yes | Get task counts by status |
| POST | `/api/tasks` | Yes | Create a new task |
| PUT | `/api/tasks/{id}` | Yes | Update a task |
| DELETE | `/api/tasks/{id}` | Yes | Soft-delete a task |
| GET | `/api/user/profile` | Yes | Get current user profile |

## Application Screens

- **Login / Signup** — User authentication
- **Dashboard** — Task status overview with counts
- **Task List** — Searchable, filterable task list
- **Task Detail** — Full task information view
- **New / Edit Task** — Create or update tasks
- **User Profile** — User info and logout
