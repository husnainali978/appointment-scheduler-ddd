# Appointment Scheduler (DDD)

A medical/service appointment scheduling API built to demonstrate Domain-Driven
Design's tactical patterns in practice, not just define them. Scheduling is a
good fit for this: "can this provider take this booking?" is a real invariant
with real edge cases (overlap, past bookings, re-cancelling), and it's very
easy to accidentally leave that logic scattered across services instead of
owned by the model. This project keeps it on the model. There are no public
setters standing in for business rules and no service class doing
`appointments.Any(a => a.Start < x && a.End > y)` against a flat list -
`Provider.CanSchedule(TimeSlot)` is where that rule lives.

## Architecture

Four projects, dependencies flow inward toward the domain:

```
AppointmentScheduler.Api              (ASP.NET Core Web API - controllers, DI wiring, HTTP concerns)
        -> AppointmentScheduler.Application  (thin orchestration: DTOs, use cases, event handlers)
                -> AppointmentScheduler.Domain    (aggregates, value objects, domain events - no EF Core reference)
AppointmentScheduler.Infrastructure    (EF Core + SQLite, references Domain and Application)
```

`Domain` has no package dependencies at all - it's plain C#. `Infrastructure`
is the only project that knows EF Core exists, via `IEntityTypeConfiguration<T>`
classes that map the domain model without polluting it with `[Column]`
attributes or navigation setters added just to please the ORM.

**Aggregates**

- `Provider` (`Domain/Providers/Provider.cs`) is the aggregate responsible for
  not double-booking itself. `CanSchedule(TimeSlot)` checks a candidate slot
  against every currently-scheduled appointment it knows about, and
  `ScheduleAppointment(...)` uses that check before ever constructing an
  `Appointment`. Conflict detection is the provider's job because only the
  provider has the full picture of its own calendar.
- `Appointment` (`Domain/Appointments/Appointment.cs`) is its own aggregate
  root with a private constructor - the only way to create one is
  `Appointment.Schedule(...)`, which enforces that it belongs to a real
  provider and isn't being booked in the past. State transitions
  (`Cancel`, `Complete`, `Reschedule`) are methods with their own guard
  clauses (e.g. you can't cancel an already-cancelled appointment, can't
  complete one that hasn't started yet), not property setters.

**Value objects**

- `TimeSlot` (`Domain/Appointments/TimeSlot.cs`) wraps a start/end pair,
  rejects an end before start at construction time, and owns
  `Overlaps(TimeSlot)` - the actual interval-overlap check.
- `PatientInfo` (`Domain/Appointments/PatientInfo.cs`) wraps name/email with
  its own validation.

Both are immutable C# `record`s with value equality, mapped by EF Core as
owned types (`OwnsOne`) so they're stored inline on the `Appointments` table
rather than needing their own table or identity.

**Domain events**

`AppointmentBookedEvent`, `AppointmentCancelledEvent`, and
`AppointmentRescheduledEvent` are raised by the aggregates themselves at the
point an invariant is satisfied and state actually changes. They sit
uncommitted on the aggregate (`AggregateRoot.DomainEvents`) until
`SchedulingDbContext.SaveChangesAsync` calls `base.SaveChangesAsync` first,
*then* hands them to a small in-process `DomainEventDispatcher`
(`Infrastructure/Events/DomainEventDispatcher.cs`), which resolves
`IDomainEventHandler<TEvent>` implementations from DI and invokes them. No
message bus, no outbox - just a guarantee that handlers only ever see events
for changes that actually made it to the database. The handlers
(`Application/EventHandlers/*`) currently log what they'd do (send a
confirmation, notify a cancellation); swapping in real email/SMS sending
later wouldn't touch the domain or application layers at all.

**Project boundaries**

- `Application` depends on `Domain` only. Its services
  (`AppointmentService`, `ProviderService`) fetch aggregates through
  repository interfaces, call methods on them, and save - they never inspect
  a `TimeSlot` or compare two appointments themselves.
- `Infrastructure` implements the repository/unit-of-work interfaces
  `Application` defines, using `SchedulingDbContext` (EF Core + SQLite).
- `Api` references all three layers for DI composition, but talks to the
  domain exclusively through `Application`'s service interfaces.

## Features

- Register providers (name + specialty).
- Book an appointment with a provider; rejected with `409 Conflict` if the
  requested slot overlaps one the provider already has, `400 Bad Request` if
  the slot is invalid or in the past.
- Cancel a scheduled appointment (frees its slot for rebooking).
- Mark an appointment complete.
- Reschedule an appointment to a new slot, re-checking conflicts against the
  provider's calendar first.
- List a provider's appointments.
- Domain events raised for booking, cancellation, and rescheduling, dispatched
  in-process after each successful save.
- Global exception-handling middleware that maps domain/application
  exceptions to the right HTTP status codes (`400` / `404` / `409`) as
  `application/problem+json`.

## How to run it

Requires the .NET 10 SDK.

```bash
cd appointment-scheduler-ddd
dotnet restore
dotnet run --project src/AppointmentScheduler.Api
```

The API listens on `http://localhost:5239` (see
`src/AppointmentScheduler.Api/Properties/launchSettings.json`). EF Core
migrations are applied automatically on startup (`Database.Migrate()` in
`Program.cs`), so the SQLite database (`appointments.db`) and schema are
created on first run with no separate migration step. To add a new migration
after changing the model:

```bash
dotnet tool restore
dotnet tool run dotnet-ef migrations add <Name> --project src/AppointmentScheduler.Infrastructure --startup-project src/AppointmentScheduler.Api
```

Open `src/AppointmentScheduler.Api/AppointmentScheduler.Api.http` (Visual
Studio, Rider, or the VS Code REST Client extension) to walk through the full
flow - creating a provider, booking an appointment, a conflicting booking
being rejected with `409`, a past/invalid booking rejected with `400`,
rescheduling, and cancellation.

## Tech stack

ASP.NET Core 10 Web API, EF Core 10 + SQLite, C# 13 - layered as
Domain / Application / Infrastructure / Api.
