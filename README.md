# TicketBookingSystem

A small ticket booking system built for a Zempler Bank technical exercise. Users can view ticket availability for an event, reserve a ticket, and purchase a reserved ticket — with race conditions on the last available ticket handled correctly.

## Tech stack

**Backend** — `TicketBookingSystemApi/`
- .NET 10 / ASP.NET Core Web API 
- Entity Framework Core 10 with SQLite (`tickets.db`)
- Swagger / Swashbuckle for API docs
- Layered as Controller → Service → Repository, with interfaces for each

**Frontend** — `ticketing-ui/`
- React 19 + TypeScript, built with Vite
- MUI (Material UI) for components

**Tests** — `TicketBookingSystemApi.Tests/`
- xUnit, using `WebApplicationFactory` for integration tests (event lookup, reserve, purchase, and concurrency behavior)

## Architecture notes

Design decisions are documented as short ADRs in the repo root:
- [0001-ResponseChoices.md](0001-ResponseChoices.md) — why event responses return ticket counts rather than the full ticket list
- [0002-MinimalApi-to-ControllerPattern-Justification.md](0002-MinimalApi-to-ControllerPattern-Justification.md) — why controllers were chosen over Minimal APIs
- [0003-Decision-to-go-with-optimistic-approach.md](0003-Decision-to-go-with-optimistic-approach.md) — how concurrent reservations for the last ticket are resolved, and what a higher-throughput solution would look like

## API overview

| Method | Route | Description |
|---|---|---|
| GET | `/api/events/{id}` | Ticket availability summary for an event (available / reserved / sold) |
| POST | `/api/events/{id}/reserve` | Reserve one available ticket under a holder name |
| POST | `/api/tickets/{id}/purchase` | Convert a reserved ticket to purchased, if reserved by the same holder |

Reservations expire after 10 minutes and become available again. Full request/response contracts are available via Swagger when running the API locally.

## Running the backend

Requirements: [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

```bash
cd TicketBookingSystemApi
dotnet run
```

The API starts at `http://localhost:5278` and applies migrations / seeds sample data on startup. In development, Swagger UI is available at `http://localhost:5278/swagger`.

To run the tests:

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
