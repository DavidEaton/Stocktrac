# Domain optionality

Use one explicit representation for each meaning:

| Meaning | Type |
| --- | --- |
| Required value | Non-nullable `T` |
| Optional value | `Maybe<T>` |
| Zero or more values | Non-null collection |
| Fallible operation | `Result` or `Result<T>` |
| Infallible operation | The new value or `void` |

These are conventions for new and changed code; existing success-only results
and runtime validation gaps do not imply universal compliance.

- Do not represent absence with null, blank strings, zero, or default dates;
  legitimate zero values remain valid, such as `Amount.FromDecimal(0)`.
- Keep required inputs non-nullable and reject invalid runtime inputs before
  construction. Nullable annotations alone do not enforce invariants.
- Use empty collections for none, subject to the collection's business rules.
  Validate members, uniqueness, and primary cardinality before changing state.
- Give optional values explicit set and clear operations. Prefer precise names
  such as `Replace...`, `AddOrReplace...`, and `Remove...`; existing `Update...`
  APIs remain in the code. Preserve state when a fallible change fails.
- Normalize nullable transport input at the API boundary; map database nulls to
  domain absence at the persistence boundary. Do not combine absence models
  such as `Maybe<T?>`.
- Respect framework contracts such as `Equals(object?)`. Private nullable
  implementation details must not escape or weaken domain invariants.

`EmploymentRole.SuperiorRole` is the explicit exception: absence uses the
non-null singleton `EmploymentRole.Empty`. It is inactive, has no subordinates,
rejects hierarchy changes and assignment creation, and must never be persisted.
Do not also use null or Maybe for this relationship. Other optional domain
values use `Maybe<T>`.

See [architecture](../docs/architecture.md) for union defaults, collection rules,
and persistence limits, and [test guidance](../Stocktrac.Tests/README.md) for
verification of present, absent, invalid, and unchanged-on-failure behavior.
