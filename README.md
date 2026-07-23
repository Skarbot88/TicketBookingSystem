# TicketBookingSystem

A small ticket booking system built for a Zempler Bank technical exercise. Users can view ticket availability for an event, reserve a ticket, and purchase a reserved ticket — with race conditions on the last available ticket handled correctly.

## Tech stack

**Backend** — `TicketBookingSystemApi/`
- .NET 10 / ASP.NET Core Web API 
- Entity Framework Core 10 with SQL Server, run locally via Docker
- Pessimistic row locking (`UPDLOCK`/`ROWLOCK`/`READPAST`) to resolve concurrent reservations/purchases
- Swagger / Swashbuckle for API docs
- Layered as Controller → Service → Repository, with interfaces for each

**Frontend** — `ticketing-ui/`
- React 19 + TypeScript, built with Vite
- MUI (Material UI) for components

**Tests** — `TicketBookingSystemApi.Tests/`
- xUnit, using `WebApplicationFactory` for integration tests (event lookup, reserve, purchase, and concurrency behavior)
- Testcontainers spins up a real, throwaway SQL Server container for the test run — no manual setup, but Docker must be running

## Architecture notes

Design decisions are documented as short ADRs in the repo root:
- [0001-ResponseChoices.md](0001-ResponseChoices.md) — why event responses return ticket counts rather than the full ticket list
- [0002-MinimalApi-to-ControllerPattern-Justification.md](0002-MinimalApi-to-ControllerPattern-Justification.md) — why controllers were chosen over Minimal APIs
- [0003-Decision-to-go-with-optimistic-approach.md](0003-Decision-to-go-with-optimistic-approach.md) — how concurrent reservations for the last ticket are resolved using the SQLite db. 
- Need to add the ADR for this Postgres approach. 
## API overview

| Method | Route | Description |
|---|---|---|
| GET | `/api/events/{id}` | Ticket availability summary for an event (available / reserved / sold) |
| POST | `/api/events/{id}/reserve` | Reserve one available ticket under a holder name |
| POST | `/api/tickets/{id}/purchase` | Convert a reserved ticket to purchased, if reserved by the same holder |

Reservations expire after 10 minutes and become available again. Full request/response contracts are available via Swagger when running the API locally.

## Running with Docker (SQL Server)

Requirements: Docker Desktop (or a compatible Docker engine).

```bash
docker-compose up -d
```

This starts a SQL Server 2022 container on `localhost:1433`, matching the connection
string already configured in `appsettings.Development.json`. `docker-compose ps` will
report it `healthy` once it's ready to accept connections.

`docker-compose.yml` falls back to the same dev-only password used in
`appsettings.Development.json` if `MSSQL_SA_PASSWORD` isn't set, so this works with no
extra setup. To use a different password, copy `.env.example` to `.env` and edit it
(and update `appsettings.Development.json` to match).

## Running the backend

Requirements: [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), and the SQL Server container above running.

```bash
cd TicketBookingSystemApi
dotnet run
```

The API starts at `http://localhost:5278` and creates the schema / seeds sample data on startup. In development, Swagger UI is available at `http://localhost:5278/swagger`.

To run the tests (Docker must be running — Testcontainers provisions its own throwaway SQL Server instance, separate from the one above):

```bash
dotnet test
```

## Running the frontend

Requirements: Node.js (v20+ recommended)

```bash
cd ticketing-ui
npm install
npm run dev
```

The app starts at `http://localhost:5173` and expects the backend to be running at `http://localhost:5278` (CORS is already configured on the API for this origin).
