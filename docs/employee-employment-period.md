# Employee employment period

## Decision

Starting from fetched `origin/main` at `fdf8e1c`, Employee owns one required
`EmploymentPeriod PeriodEmployed` instead of separately storing Hired and Exited.
EmploymentPeriod is a sealed immutable record with a private constructor,
required DateOnly Hired, and optional Maybe<DateOnly> Exited. Its factory returns
Result<EmploymentPeriod>; successful values cannot contain an exit before hiring.
Same-day hiring and exit remain valid.

This is a product value, so ordinary composition expresses it directly. No union,
additional dependency, public setter, or second absence model is introduced.
The existing native Customer and Vehicle unions and compiler configuration remain
unchanged. A lifecycle union remains deferred: Active still means an exit is
absent, including when hire or exit is in the future.

## API and behavior

- Read `employee.PeriodEmployed.Hired` and `employee.PeriodEmployed.Exited`.
  Employee.Hired and Employee.Exited are removed.
- Employee.Create continues to accept a hire date and caller-supplied evaluation
  date; it creates an active EmploymentPeriod through its validated factory.
- ReplaceHired and ReplaceExited replace the former UpdateHired and UpdateExited
  methods, following the optionality policy's mutation naming convention.
  Employee methods retain Result<DateOnly>; value-object methods return
  Result<EmploymentPeriod> without changing the original value.
- Employee.RemoveExited still returns Result because reactivation requires an
  active assignment. EmploymentPeriod.RemoveExited is infallible and returns a
  new active period. Modifying a detached period never modifies an Employee.
- EmploymentPeriod owns StartDateMinimum, EndDateMaximum, and DateRangeMessage;
  their Employee counterparts are removed.

Creation validates both dates against the caller's inclusive 50-year lookback
and one-year lookahead, with supported-date clamping at the extremes. Date
replacement validates the changed date and ordering. An unchanged historical
date is not revalidated against a later evaluation date: an aging hire date must
not prevent an otherwise valid exit. No operation reads the system clock.

Employee retains aggregate rules: hire replacement validates current role
assignments, and exit removal validates assignments for reactivation. A failed
operation preserves the entire original period. Previously read periods remain
unchanged after successful aggregate mutations. Value equality and hashing use
both dates, including exit absence.

## Callers and persistence

The latest main commit migrated production employee and role APIs to DateOnly
but left DateTime test callers. Employee, employment-role, and assignment tests
now use fixed DateOnly values and calendar-date boundaries. The same main commit
removed assignment null/empty-role guards; these are restored to preserve the
documented required-role invariant and existing failure-result tests.

There are no active Employee mappings, endpoints, DTOs, or migrations. Future
storage should retain a required hire column and nullable exit column, map null
to Maybe.None at the boundary, and reconstruct through the validated factory.
Do not deserialize an unvalidated record or add an invalid EF placeholder.
Introduce database constraints for ordering and provider-backed round-trip and
corrupt-row tests when aggregate persistence is enabled, as required by the
existing optionality audit. No live schema migration is needed here.

## Verification

Tests cover presence, absence, same-day exit, ordering, inclusive date bounds,
immutable replacements, value equality, historical dates, failed aggregate
changes, and role-dependent reactivation.

- Complete solution build: passed with zero warnings and errors.
- Complete automated suite: 654 passed, zero failed or skipped.
- `git diff --check`: passed.

Validation used the documented .NET SDK `11.0.100-rc.1.26425.128`, including
compilation of the existing native unions. No compiler or project changes were
needed. Database-backed aggregate tests remain deferred until mappings exist.
