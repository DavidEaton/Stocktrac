# Domain optionality migration record

This record classifies the nullable and absence-bearing state addressed by the
incremental migration. It complements the original compliance audit and records
the result of applying its acceptance criteria.

| Classification | Members and APIs | Migration result |
| --- | --- | --- |
| Genuine optional state | customer code; tenant logo; employee notes, certification and printed name; credit-card deposit date; vehicle VIN/year/plate/jurisdiction/unit/color; address line 2; person middle name, birthday and driver's license | Domain state uses `Maybe<T>` and mutations have distinct set and clear operations. |
| Required state | company/business, customer entity, person name, employee/person/SSN, role period, credit-card name/fee, address components, sale-code supplies | Non-nullable signatures are trusted. Domain factories no longer turn null contract violations into validation failures in the migrated paths. |
| Collections | contacts, vehicles, employee roles and sale codes | Exposed collections remain non-null snapshots. Contact construction and replacement continue to validate uniqueness and primary cardinality before mutation. |
| Failures | enum/range/length/uniqueness/not-found rules and mode transitions | `Result` remains where a domain rule can fail; unconditional setters no longer manufacture success-only results. |
| Boundary input | nullable strings and nullable value types supplied by transports | `OptionalInput` converts nullable transport values to trimmed `Maybe<T>` values before domain invocation. |
| Persistence | nullable primitive columns | `MaybeValueConverters` translates optional strings, integers and dates to and from nullable storage values. |
| Implementation/framework exceptions | `Entity.Equals(object?)`, immutable copy helpers, `default(Note)`, EF constructors and nullable `DbContext` dependencies | Framework signatures are retained. `Note` documents its nullable backing-field exception and guarantees a non-null public value for the default struct. |

## Follow-up acceptance pass

- **Pass:** migrated public optional properties expose absence through `Maybe<T>`.
- **Pass:** migrated optional mutations provide explicit set and clear behavior.
- **Pass:** migrated non-nullable parameters do not produce failures solely for null input.
- **Pass:** unconditional preference, person, primary-flag, boolean, fee, and assignment operations do not return success-only `Result` values.
- **Pass:** contact collection construction and replacement validate uniqueness and primary cardinality before mutation.
- **Pass:** nullable transport normalization now has an application-boundary home rather than an entity helper.
- **Pass:** tests exercise presence, absence, validation, explicit clearing, default-struct safety, immutable optional-state updates, and converter round trips for both `Some` and `None`.
- **Deferred with reason:** provider-backed aggregate round trips require mapped aggregate roots and an integration-test database provider; neither exists in the current model. Converter-level round trips protect the current persistence seam, and live database tests must accompany each future aggregate mapping.
