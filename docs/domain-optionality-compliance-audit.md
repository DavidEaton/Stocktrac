# Domain optionality audit

The review covered public state, construction, mutation, and collections under
`Stocktrac.Domain/Features`.

## Result

- Optional domain state uses `Maybe<T>`; collections are non-null snapshots.
- Required APIs stay non-nullable, with some defensive runtime null checks.
- Infallible mutations no longer return success-only `Result` values.
- Contact creation and replacement validate uniqueness and primary cardinality
  before mutation.
- The API normalizes nullable transport values; persistence converters handle
  nullable columns.
- Framework and private implementation exceptions remain where required, such
  as `Entity.Equals(object?)` and the safe default representation of `Note`.

Tests cover presence, absence, clearing, validation, atomic failure, and
converter round trips. Database-backed aggregate round trips remain deferred
until mapped aggregate roots and an integration-test provider exist.

See [the project compliance review](project-compliance-review.md) for the
project-by-project findings and the persistence-boundary safeguards.
