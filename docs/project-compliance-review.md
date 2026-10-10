# Project compliance review

This review checks each solution project against the domain optionality policy,
the contact-collection design, and the test-naming guidance.

## Stocktrac.Domain

- Required values remain non-nullable and optional state uses `Maybe<T>`, with
  the explicitly required `EmploymentRole.Empty` superior sentinel documented
  as a specific exception.
- Employee optional text is trimmed and validated before construction or
  mutation. A present blank value is rejected; callers must use the matching
  remove operation to represent absence.
- Employee mutations validate before assignment, preserving prior state after
  failure. The invalid Entity Framework placeholder constructor is removed.
  The [employee-role migration](employee-role-migration.md) introduces entity
  identity for EmploymentRole, immutable RoleAssignment values, validated
  subordinate relationships, and caller-supplied dates for employee validation.
- Employee now owns an immutable EmploymentPeriod with required hire and optional
  exit dates. Date validation belongs to that value; role-dependent changes remain
  on the aggregate. See [the employment-period migration](employee-employment-period.md).
- Tenant logo URLs follow the same explicit update/remove contract and cannot
  store a blank string as a present optional value.
- Vehicle plates, unit numbers, and colors use that contract as well; blank
  present values fail without mutation rather than becoming hidden absence.
- Contact collection replacement continues to validate complete input before
  mutation, as described in the collection design.
- Customer is one native union with PersonCustomer and BusinessCustomer cases.
  The former Customer class and CustomerEntity union are consolidated. Required
  Person/Business references and shared aggregate state are encapsulated. Case
  replacement returns a new Customer while preserving customer identity, code,
  classification, preferences, and vehicle membership. Customer.Validate and
  fallible operations reject default/null cases. See the
  [consolidation report](customer-union-consolidation.md).
- Vehicle kinds use a verified native C# union. Aggregate boundaries reject
  default/null-case unions; Vehicle case properties are immutable and validated before aggregate assignment. Optional
  non-traditional descriptions use `Maybe<string>` with explicit removal.
  Shared Vehicle identity, lifecycle, and registration state remain on the
  aggregate. See [the scoped migration report](customer-vehicle-union-migration.md)
  for API changes and the persistence plan.

## Stocktrac.Api

- `OptionalInput` owns transport normalization and keeps nullable request data
  outside the domain.
- `MaybeValueConverters` maps database nulls to absence. The string converter
  also treats legacy blank values as absent and trims present values, preventing
  invalid persistence data from leaking into domain state.
- Health checks and application validators do not introduce an alternative
  domain absence model.

## Stocktrac.Contracts

The project currently contains no contract types. It remains a separate
assembly boundary ready for transport DTOs; domain entities should not be
placed there because contracts may use nullable transport values.

## Stocktrac.Tests

- Tests cover present, absent, and invalid optional string persistence values.
- Employee and tenant tests assert both the failure result and unchanged state
  for invalid mutations.
- New and touched test names use the documented outcome-first naming pattern.
- Customer tests cover both cases, case replacement, aggregate identity, shared
  reference state, collection snapshots, empty unions, and atomic failures.
- Employee-role tests cover activity at fixed dates, value and entity equality,
  hierarchy consistency, cycles, collection validation, and failed reactivation.

Database-backed aggregate round trips remain deferred until aggregate mappings
and an integration-test provider exist, as recorded in the optionality audit.
