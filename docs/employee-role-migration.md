# Employee and employment-role migration

## Repository baseline

This change is based on fetched `origin/main` at
`6cf2c88`, which already exposes `SubordinateRoles` as `IReadOnlyList`.
It preserves that snapshot API and supplies validated construction and hierarchy
operations. `Person` already has neither `ICustomerEntity` nor `EntityType`;
Person, Business, and CustomerEntity are unchanged from this base revision.

## Model and invariants

`Employee` remains the aggregate root. It owns a non-null, defensively copied
`IReadOnlyList<RoleAssignment>`. `EmploymentRole` now derives from the existing
domain `Entity`: independently persisted roles compare by identity, including
the existing distinction between different unsaved entities. Roles referenced
by an employee's assignments are child entities, not nested value objects.

`RoleAssignment` is an immutable, sealed record containing a required `Role` and
`PeriodAssigned`. Equality uses the referenced role's entity identity and the
date range's value equality. `ReplaceRole` and `ReplacePeriodAssigned` return
new validated assignments; neither changes an assignment already owned by an
employee. Replace the employee's collection to apply those new values.
The old mutating `UpdateRole` and invalid parameterless constructor are removed.

Assignment periods must be contained within the role's immutable validity
period. This guarantees that whenever an assignment is active, its required
employment role is active too. Both date ranges include their start and end
dates. Activity is evaluated by `IsActive(DateTime date)`, using the supplied
calendar date, without reading the system clock.

Employee creation, role collection changes, hire-date changes, and reactivation
validate that an active employee has at least one active assignment at the
supplied date. Empty, null, null-member, and duplicate assignment collections
are rejected where they violate these rules. Inactive employees may have an
empty assignment collection. All failed changes preserve existing state.

`Active` retains the existing lifecycle meaning: no exit date has been set.
This change does not redefine future hire or exit dates as a new lifecycle
model. Date-dependent validity is evaluated at the caller's date; applications
must call `ValidateRoleAssignments(date)` when evaluating an existing employee
at a later date. Passage of time cannot mutate an entity automatically, and
finite assignments can expire. Supply the date in the relevant business time
zone consistently across a single operation.

## Role hierarchy

`SubordinateRoles` is a required, non-null `IReadOnlyList<EmploymentRole>`
snapshot backed by one private list. Factory input is copied. Creation and
`AddSubordinateRole` link both the subordinate collection and `SuperiorRole`;
`RemoveSubordinateRole` removes both links. To move a role, explicitly remove it
from its current superior before adding it to the next one.

Null roles, duplicate identities, self-reference, cycles, and assigning an
already attached subordinate to another superior fail before either side is
changed. Different unsaved roles with identical descriptive values are distinct
entities and may both belong to the same superior.

As explicitly required by this model, absent superior roles use
`EmploymentRole.Empty`, rather than `Maybe<EmploymentRole>` or null. The singleton
has no subordinates, points to itself as its own absent superior, is always
inactive, and rejects hierarchy mutations and assignment creation. Its named
metadata and broad date range are sentinel implementation details, not actual
employment data. It must never be persisted. This is a specific exception to
the normal optionality policy; other optional values continue to use `Maybe<T>`.

## API changes and example

- `EmploymentRole.Create` requires subordinate roles and an explicit superior;
  use `[]` and `EmploymentRole.Empty` for a root without children.
- `RoleAssignment.IsActive` changes from a property to `IsActive(DateTime date)`.
  `EmploymentRole.IsActive(DateTime date)` uses its own validity period.
- `Employee.Create` requires a `DateTime date` after `notes`, before its optional
  parameters. `StartDateMinimum` and `EndDateMaximum` now accept that date too.
  These bounds use its calendar date and clamp to supported dates at the
  extremes of the `DateTime` range.
- `UpdateHired`, `UpdateExited`, `AddRoleAssignment`, and `RemoveExited` require
  the evaluation date. `RemoveExited` now returns `Result`, because reactivation
  can fail when no active assignment exists.
- `ReplaceRoleAssignments`, `RemoveRoleAssignment`, and
  `ValidateRoleAssignments` expose validated collection operations.
- The invalid parameterless Employee constructor is removed. It previously
  constructed an active employee without assignments and read `DateTime.Today`.

```csharp
var date = new DateTime(2025, 1, 15);
var period = DateRange.Create(new(2025, 1, 1), new(2025, 12, 31)).Value;
var role = EmploymentRole.Create(
    NonEmptyString.Create("Technician").Value,
    NonEmptyString.Create("Services vehicles").Value,
    period, [], EmploymentRole.Empty).Value;
var assignment = RoleAssignment.Create(role, period).Value;
var employee = Employee.Create(person, [assignment], ssn,
    date.AddDays(-1), notes, date);
```

## Persistence and architectural scope

There are no active Employee/EmploymentRole mappings, DbSets, endpoints, DTOs,
or migrations in this repository. This change provides the domain identity and
relationships without introducing a database schema or transport contract.
Database-backed aggregate round trips remain deferred under the existing audit.

When persistence is introduced, store employment roles as entities with their
own keys and an optional superior foreign key. Map database null superior keys
to `EmploymentRole.Empty` at the boundary; never insert a row for the singleton.
Persist assignments as employee-owned values containing a role foreign key and
period, with no independent domain identity. Reconstruct and validate all values,
relationships, and active assignment rules before exposing an aggregate; do not
restore invalid EF placeholder constructors. Use an identity map for role
references. Add provider-backed tests for identity, owned assignment values,
root and subordinate roles, absent superior keys, and invalid persisted rows
before enabling these mappings.

No unions, enums, dependencies, compiler configuration, or unrelated aggregates
are introduced or changed. Entity relationships and immutable product values
express this model directly. A new employment-lifecycle union was considered
and remains deferred because this task does not redefine its states. Existing
Customer and Vehicle unions remain intact. The installed .NET 11 preview SDK
supports their native syntax; the complete solution build verifies compatibility.

## Verification

Tests use fixed caller-supplied dates and follow the project's outcome-first
naming rules. They cover required inputs, inclusive activity boundaries,
assignment-period containment, value versus entity equality, defensive copies,
hierarchy links and cycles, duplicate rejection, mutation atomicity, empty
inactive collections, reactivation, and employment date boundaries.

- Complete solution build: passed, zero warnings and errors.
- Complete automated suite: 596 passed, zero failed or skipped, including
  110 employee and role cases.
- No `DateTime.Today`, `DateTime.Now`, `DateTime.UtcNow`, nullable suppression,
  or mutable public collections remain in the affected employee domain files.
- `git diff --check`: passed.

Validation used .NET SDK `11.0.100-rc.1.26425.128` with the repository's existing
preview language configuration. Cached dependencies were reused; no package or
network restore was required. VSTest required local socket access, which was
enabled for the standard test command after the sandbox blocked it.

```sh
dotnet build Stocktrac.slnx --no-restore --disable-build-servers -m:1 -p:UseSharedCompilation=false
dotnet test Stocktrac.Tests/Stocktrac.Tests.csproj --no-build --no-restore --disable-build-servers -m:1
```
