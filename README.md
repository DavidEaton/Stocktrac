# Stocktrac

Stocktrac models the business concepts used to service vehicles and equipment:
customers, contacts, vehicles, employees, financial values, and sale codes. The
current solution provides a domain model and an API foundation; repair-order and
inventory workflows are not implemented.

## Projects

| Project | Purpose and problem it solves |
| --- | --- |
| [Stocktrac.Domain](Stocktrac.Domain) | Expresses business rules in domain types, keeping validation and behavior together and independent of HTTP and persistence. |
| [Stocktrac.Api](Stocktrac.Api) | Provides the ASP.NET Core host and application/persistence boundary helpers. Currently exposes liveness and readiness checks and development OpenAPI; business endpoints and aggregate persistence are not wired up. |
| [Stocktrac.Contracts](Stocktrac.Contracts) | Reserves a separate assembly for transport contracts so wire formats can evolve independently of domain types. Currently contains no DTOs and is not referenced by the other projects. |
| [Stocktrac.Tests](Stocktrac.Tests) | Verifies observable domain behavior and optional-value converters with xUnit and Shouldly. Currently has no database-backed aggregate integration tests. |

## Design priorities

Our first architectural priority is immutability. Prefer modern C# and its
functional capabilities to reduce complexity and bugs and make the code easier
to read: immutable values, pure transformations, explicit results, composition,
and exhaustive pattern matching. Replace procedural state management with
functional code when it makes the business rule clearer. Preserve encapsulation
and make invalid states unrepresentable where practical.

The implementation combines immutable value objects with encapsulated mutable
entities and aggregates. See the short [architecture guide](docs/architecture.md)
and [domain optionality policy](Stocktrac.Domain/Domain%20Optionality%20Policy.md)
for the conventions and current limits.

## Build and run

Domain, API, and Tests target `net11.0` with `LangVersion=preview` and use native
C# unions. Use an SDK that supports that syntax; SDK
`11.0.100-rc.1.26425.128` is verified with this repository. Contracts targets
`net10.0`. There is currently no `global.json` pin.

```sh
dotnet build Stocktrac.slnx
dotnet test Stocktrac.Tests/Stocktrac.Tests.csproj
dotnet run --project Stocktrac.Api
```

`/health/live` checks the process. `/health/ready` requires a nonblank
`ConnectionStrings:DefaultConnection` configuration value; it does not test
database connectivity. The checked-in settings do not supply that value. Set it
through configuration, for example `ConnectionStrings__DefaultConnection` in
the environment. OpenAPI is mapped only in Development.

## Documentation

Keep documentation small: project purpose, design intent, essential setup, and
constraints that are easy to miss in code. Use the code and behavior-focused
tests to learn APIs and detailed rules. Update these guides when their claims
change; avoid duplicate API inventories, migration reports, and coverage lists.
Test conventions live in [Stocktrac.Tests/README.md](Stocktrac.Tests/README.md).
