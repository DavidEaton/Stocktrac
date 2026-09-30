# Domain optionality

Domain types must make absence explicit:

| Meaning | Type |
| --- | --- |
| Required value | Non-nullable `T` |
| Optional value | `Maybe<T>` |
| Zero or more values | Non-null collection |
| Fallible operation | `Result` or `Result<T>` |
| Infallible mutation | `void` or the new value |

## Rules

- Never expose `null`, empty strings, zero, or default dates as domain absence.
- Keep required parameters non-nullable. Defensive null checks may still reject
  callers that bypass C# nullability.
- Use empty collections for “none.” Validate elements, uniqueness, and primary
  cardinality before construction or mutation.
- Give optional values explicit set and clear operations.
- Name mutations precisely: `Replace...`, `AddOrReplace...`, and `Remove...`.
- Normalize nullable transport data at the API boundary. Persistence converters
  may map `Maybe<T>` to nullable columns.
- Follow framework contracts such as `Equals(object?)`. A private nullable field
  is acceptable only when it cannot escape or weaken an invariant.
- Do not combine absence models (for example, `Maybe<T?>`).

## Review checklist

- [ ] Every public member has one clear absence model.
- [ ] Successful construction leaves all required values and invariants valid.
- [ ] Failed mutations leave existing state unchanged.
- [ ] `Result` reports real failures; unconditional work does not return it.
- [ ] Boundary and persistence code prevent nullable state from leaking inward.
- [ ] Tests cover present, absent, invalid, and persistence round-trip cases.
