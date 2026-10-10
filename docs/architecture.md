# Architecture and C# style

## Domain-driven design

Use the business vocabulary in types and operations. An entity has identity;
a value object is defined by its values. An aggregate owns related state and
protects its invariants through its public operations. Keep domain rules in
`Stocktrac.Domain`, with HTTP, serialization, and database concerns at the API
boundary. Prefer composition and meaningful value objects over extra inheritance
or primitive fields that callers must coordinate.

`Entity` implements identity equality: distinct unsaved entities remain distinct.
Records such as `PersonName`, `Address`, `Money`, `DateRange`, `EmploymentPeriod`,
and `RoleAssignment` express values. `RoleAssignment` contains an entity
reference, so its equality includes the employment role's identity.

## Immutability and functional code

Immutability is the primary design direction. Prefer validated immutable records
and values, get-only properties, and operations that return a new valid value.
Preserve the original on failure. Keep functions small and data flow explicit;
use expressions, composition, and pattern matching when they clarify intent.
A direct conditional or loop is appropriate when a pipeline would obscure the
rule. Do not add abstractions merely to make code look functional.

Use the existing `CSharpFunctionalExtensions` abstractions: `Ensure` checks a
rule, `Map` transforms success, `Bind` composes fallible work, and `Tap` performs
an explicit side effect after success. Handle failure before reading
`Result.Value`. Expected validation failures should be results rather than
exceptions. Use [Maybe and Result consistently](../Stocktrac.Domain/Domain%20Optionality%20Policy.md).

For example, this transformation creates a new employment period without
changing the original:

```csharp
var date = new DateOnly(2025, 1, 15);
var updated = EmploymentPeriod.Create(new DateOnly(2024, 1, 1), date)
    .Bind(period => period.ReplaceExited(date, date));
```

The current architecture is not fully immutable. `Employee`, `Vehicle`,
`Person`, and other entities mutate encapsulated state through domain operations.
`Employee` replaces its immutable `PeriodEmployed` only after validation; values
read earlier remain unchanged. Collection getters return membership snapshots,
but referenced entities can still change. Private setters, readonly structs,
and `Tap` pipelines do not by themselves make an object graph immutable. Some
older operations also return success-only results or assume non-null inputs;
the conventions describe the direction for new and changed code.

## Closed alternatives

Use native unions for meaningful, mutually exclusive domain forms and handle
all cases explicitly. Use ordinary composition for values whose fields belong
together; retain `Maybe<T>` for optionality and `Result<T>` for outcomes.

- `Customer(PersonCustomer, BusinessCustomer)` selects the customer form.
  Create it through `Customer.Create(Person, ...)` or `Customer.Create(Business, ...)`.
  Its internal `CustomerState` holds identity and shared metadata/vehicles;
  contacts belong to the selected Person or Business. `ReplacePerson` and
  `ReplaceBusiness` return a new union value with the same customer identity.
  Retain that returned value: earlier copies keep their selected subject, while
  shared aggregate mutations remain visible across copies. Customer is a
  readonly union struct, not an `Entity` subclass or an immutable object graph.
- `VehicleKind(TraditionalVehicleKind, NonTraditionalVehicleKind)` separates
  required VIN/make/model from equipment with optional VIN and at least one
  description (make or model). Its cases are immutable; `Vehicle` replaces a
  complete validated kind while retaining shared identity and registration data.

Native union structs can contain a default or null case. `Customer.Validate`
and its fallible operations reject empty customers; projections on them can
throw. `Vehicle.Create` and `ReplaceKind` reject empty kinds. These values are
not domain absence. Verify compiler support when changing the preview SDK.

## Collections and dates

Validate complete collection changes before mutating owned state. Contacts must
have unique phone numbers/email addresses and at most one primary per collection.
`ReplacePhones` and `ReplaceEmails` reject empty input; use `RemovePhones` or
`RemoveEmails` to clear all contacts. Creation permits empty contact collections.

Employment periods, role validity, and assignment periods use `DateOnly`.
Employment and role checks take a caller-supplied evaluation date; use the same
business date throughout an operation. `EmploymentPeriod.Active` means no exit
is set, including for future dates. Active employees need an active assignment
at the evaluation date; call `ValidateRoleAssignments(date)` when evaluating
later. Assignment periods must fit within the role's validity period. Role
hierarchy operations maintain both links and reject cycles. Vehicle year
validation still reads `DateTime.Today`; clock independence is not universal.

## Application and persistence boundaries

`OptionalInput` normalizes nullable transport values, and `MaybeValueConverters`
provides conversions to nullable database columns. The API has no business
endpoints, active aggregate DbSets/configurations, or migrations;
`ApplicationDbContext` is scaffolding and is not registered in `Program.cs`.
Native unions do not automatically provide ORM mappings or wire formats.

When persistence is introduced, map storage/transport representations explicitly
and reconstruct through validated factories. Customer storage needs its own key,
a case tag, and the corresponding Person/Business reference; Vehicle storage
needs its kind and case data. Keep discriminators outside the domain and do not
serialize the generated union `Value`. Preserve customer identity deliberately;
`CustomerState` is not another aggregate root. Store assignments as employee-owned
values referring to role keys; map an absent superior key to `EmploymentRole.Empty`
and never persist that sentinel. Validate required data, case consistency, and
date ordering, and add provider-backed round-trip and corrupt-row tests before
enabling aggregate persistence. Keep domain invariants intact when adapting EF.
