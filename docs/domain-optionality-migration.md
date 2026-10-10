# Domain optionality migration

Migration is complete at the current persistence seam:

- Optional properties use `Maybe<T>` with explicit set and clear operations.
  The employment-role superior uses the specifically required
  `EmploymentRole.Empty` sentinel, as documented in the policy and
  [employee-role migration](employee-role-migration.md).
- Required parameters are non-nullable; defensive null failures remain valid.
- Collections are non-null and validate invariants before mutation.
- `Result` is reserved for operations that can fail.
- `OptionalInput` normalizes transport values at the API boundary.
- `MaybeValueConverters` maps optional values to nullable database columns.

Tests cover present and absent values, validation, clearing, immutable updates,
default-struct safety, and converter round trips. Add database-backed aggregate
round trips when mapped roots and an integration-test provider are introduced.
