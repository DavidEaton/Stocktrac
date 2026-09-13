# Domain optionality compliance audit

## Scope and method

This audit covers every entity, aggregate, value object, and domain service-like
type under `Stocktrac.Domain/Features`. It applies the checklist in
`Stocktrac.Domain/Domain Optionality Policy.md` to public state, construction,
mutation, collections, and the remaining uses of nullable types. Enums,
interfaces, and extension classes were inspected as supporting APIs but are not
listed as domain objects because they do not own domain state.

The status labels mean:

- **Compliant**: no policy violation was found.
- **Partial**: optional state is modeled correctly, but one or more APIs still
  conflict with the policy.
- **Non-compliant**: the type has a direct conflict with a required practice.

This is a point-in-time analysis. It deliberately records migrations rather
than changing many public signatures in one pass; those changes affect API
mappers, persistence configuration, and consumers that are not represented in
the domain project.

## Executive summary

The domain generally does **not** expose nullable properties. Genuine optional
state is already represented with `Maybe<T>` in the major aggregates, and all
aggregate collections return non-null, read-only snapshots. The principal
remaining issues are at the API level:

1. Required parameters use non-nullable signatures while several factories
   deliberately retain defensive runtime null validation.
2. Several optional mutations accept nullable values rather than `Maybe<T>` or
   separate set/clear operations.
3. A number of unconditional mutations return success-only `Result` values.
4. Some constructors accept unvalidated mutable lists or nullable primitives
   and perform boundary normalization inside the domain.
5. A few optional concepts still use empty strings as their public state.

No public domain property is declared as nullable. The nullable field in
`CurrencyCode` is an intentional, documented storage optimization and satisfies
the policy's encapsulated-implementation exception. Framework-required
nullability in `Entity.Equals(object?)` is also compliant. The nullable backing
field in `Note` likewise ensures that `default(Note)` exposes a non-null public
value.

## Aggregate and entity findings

| Domain object | Status | Findings and required migration |
| --- | --- | --- |
| `Company` | Compliant | `Business` is required and non-nullable; `Create` may retain a defensive null failure without weakening that caller contract. The invoice seed operations are genuinely fallible and correctly use `Result`. |
| `Contactable` | Compliant | `Address` uses `Maybe<Address>`, phone/email collections are always non-null, construction receives a validated collection wrapper, replacement validates before mutation, and unconditional note/address set/clear methods return `void`. Collection-member `null` remains a caller contract violation. |
| `Business` | Partial | Optional address and contact use `Maybe<T>`, and collections are validated before construction. Defensive validation of non-nullable `BusinessName` preserves a runtime safeguard. The unused nullable `ToMaybe(Person?)` helper is boundary conversion inside the domain and should be removed. |
| `Person` | Non-compliant | Optional birthday, address, and driver's license are modeled correctly and contact collections are validated. `SetName`, `SetBirthday`, and `SetDriversLicense` are unconditional assignments wrapped in success-only results; change them to `void` (clear methods already follow the policy). |
| `Customer` | Non-compliant | `Code` and address use `Maybe<T>` and vehicles/contacts are non-null collections. `Create` and `SetCode` accept nullable `CustomerCode` instead of `Maybe<CustomerCode>` or explicit set/clear APIs. Required entity/vehicle parameters are defensively checked for `null`. `RemoveVehicle` always succeeds even when absent and should either be `void` or return a not-found failure. |
| `Employee` | Non-compliant | Notes, certification number, exit date, and printed name use `Maybe<T>`, while role assignments are non-null. Construction accepts nullable optional strings and a concrete mutable `List<T>`; accept `Maybe<string>` and `IReadOnlyList<RoleAssignment>`, validate first, and copy once. Keep required-reference signatures non-nullable even if they retain defensive checks. Unconditional `SetNotes` and `SetSSN` should return `void`; optional mutations need explicit clear semantics. |
| `RoleAssignment` | Partial | Required state is non-nullable and `Create` defensively converts a null range to failure. The start/end methods use nullable arguments as “use current time” sentinels, force `.Value`, and hide possible range failures; provide explicit no-argument overloads and bind the `DateTimeRange` result. |
| `CreditCard` | Non-compliant | `AddedToDeposit` is correctly a `Maybe<DateTime>`, but creation accepts `DateTime?`, lacks an explicit `None` API, and defensively validates non-nullable `Name`. `WithFee` and `WithAddedToDeposit` cannot fail and should return the new value directly (or mutate with `void`); add an explicit clear operation. The private nullable copy parameters are an acceptable implementation detail but a dedicated copy expression would be clearer. |
| `SaleCode` | Partial | All exposed state is required and non-nullable. `Create` and `SetShopSupplies` defensively convert runtime `null` violations into domain failures without advertising nullable inputs. Other results represent real length, range, and uniqueness failures. The sales-code collection must remain non-null and should be copied if retained. |
| `SaleCodeShopSupplies` | Non-compliant | Numeric setters are legitimately fallible. Boolean setters are unconditional success-only results and should return `void`. |
| `Tenant` | Non-compliant | `LogoUrl` is correctly exposed as `Maybe<string>`, but constructors and mutations perform nullable boundary normalization in the entity. Required `Name` and `CompanyName` parameters are also nullable. Accept non-null required strings and `Maybe<string>` for the optional URL, with explicit set/clear operations; keep request normalization in API mapping. |
| `Vehicle` | Non-compliant | `Year` and plate jurisdiction use `Maybe<T>`, but APIs accept nullable values and convert them internally. Optional text fields (`Plate`, `UnitNumber`, `Color`, and VIN for non-traditional vehicles) expose empty-string sentinels rather than typed absence. `SetActive` is an unconditional success-only result. Changing the non-traditional flag can violate VIN/make/model invariants and must be validated before mutation. |
| `Email` | Non-compliant | Required address is non-null after creation. Required string parameters are nevertheless null-normalized. `SetIsPrimary` is an unconditional success-only result; making a contact primary may also need aggregate-level cardinality validation rather than an isolated entity mutation. |
| `Phone` | Partial | Required number is non-null and phone type validation is fallible. Required number inputs are null-normalized. `SetIsPrimary` is success-only, though primary-cardinality must ultimately be enforced by `Contactable`; the private nullable copy fields are justified implementation details. |

## Value-object findings

| Domain object | Status | Findings and required migration |
| --- | --- | --- |
| `Address` | Partial | Optional line 2 uses `Maybe<AddressLine>`. Required reference parameters are non-nullable and also defensively checked at runtime. `WithAddressLine2` only sets a value; add `WithoutAddressLine2` so absence is explicit. |
| `AddressLine` | Compliant | State is always a valid non-null string. Its non-nullable factory signature preserves caller diagnostics while its null normalization is a defensive runtime guard. |
| `BusinessName` | Compliant | State and input are non-nullable, while null normalization remains as a defensive runtime safeguard. |
| `City` | Compliant | Like `AddressLine`, its required factory input is non-nullable and its runtime null normalization is defensive; no nullable state escapes. |
| `ContactPreferences` | Non-compliant | The value is always complete, but creation and all three copy methods are infallible and wrapped in `Result<T>`. Return `ContactPreferences` directly. |
| `DateTimeRange` | Partial | Required values are non-null and fallible calculations retain error reasons. `WithoutEnd` is a success-only result and uses `DateTime.MaxValue` as an absence sentinel despite `End` being modeled as required. Either make end optional with `Maybe<DateTime>` or rename the operation to reflect an open-ended sentinel explicitly. |
| `DriversLicense` | Compliant | All state and required inputs are non-nullable; `Create` also retains defensive failures for callers that bypass nullable analysis. |
| `DriversLicenseNumber` | Compliant | No nullable state escapes; creation requires a non-nullable value while retaining its defensive required-value validation. |
| `Note` | Compliant | The factory requires a non-nullable input but defensively maps runtime null to a non-null string. Its nullable backing field gives `default(Note)` a non-null public value. Aggregates that model note absence continue to use `Maybe<Note>`. |
| `PostalCode` | Compliant | No nullable state escapes; creation requires a non-nullable value while retaining its defensive validation. |
| `Birthday` | Compliant | It owns required value state and returns failures for real date invariants. |
| `PersonName` | Partial | `MiddleName` correctly uses `Maybe<string>`, but the constructor and public APIs accept nullable strings and normalize them internally. Accept required first/last names and model the optional middle name with `Maybe<string>` plus an explicit clear operation. |
| `SSN` | Compliant | Stored state is non-null, the public factory requires a non-nullable value, and the nullable internal normalization result communicates parse failure rather than optional domain state. |
| `CustomerCode` | Compliant | No nullable state escapes; its non-nullable factory signature preserves compiler diagnostics while normalization remains defensive. |
| `Amount` | Compliant | It has no absent state; `Result` values represent arithmetic overflow. |
| `CreditCardName` | Compliant | No nullable state escapes; the factory requires a non-nullable name and retains defensive required-value validation. |
| `CurrencyCode` | Compliant | Its public value is always non-null. The documented nullable backing field never escapes and is a justified optimization for the default struct value. Creation requires a non-nullable code while retaining defensive parser validation. |
| `Fee` | Compliant | State and currency input are required and non-null; creation delegates defensive currency validation to `Money`. |
| `Money` | Compliant | Both typed and decimal/string factories expose non-nullable required inputs, and the latter retains defensive currency parsing. |

## Supporting types

- `Entity` correctly follows the .NET `Equals(object?)` contract. Its equality
  operators should declare nullable operands (`Entity?`) because they explicitly
  support null comparison; this is framework/operator behavior rather than
  optional domain state.
- `ValidatedContactCollections` is an internal invariant carrier, not optional
  state. Its lists are non-null and are copied by `Contactable`.
- `Iso4217CountryCurrencyCodes.GetStream()` uses `Maybe<Stream>` to represent an
  embedded resource that might be unavailable. This is a legitimate optional
  implementation outcome and does not leak nullable state.
- The enums, `IHasPrimary`, `IContactable`, and `ICustomerEntity` expose no
  optionality violation. `DateExtensions`, `StringExtensions`, and
  `MoneyExtensions` own no domain state.

## Recommended migration order

1. **Remove success-only results without semantic change.** Start with
   `ContactPreferences`, `Person`, `SaleCodeShopSupplies`, and simple boolean
   setters. Update call sites and tests in the same commit.
2. **Make absence explicit.** Add set/clear pairs and `Maybe<T>` inputs for
   customer code, tenant logo, employee optional text, credit-card deposit date,
   vehicle year/jurisdiction, address line 2, and person middle name.
3. **Keep required signatures non-nullable.** Defensive null validation may
   remain, but it must never be expressed as permission for callers to pass
   nullable values. Use `null!` only in tests that deliberately exercise the
   runtime safeguard.
4. **Resolve sentinel-backed optional text.** Decide whether vehicle plate,
   unit, color, and non-traditional VIN are genuinely optional. If so, expose
   `Maybe<string>`; otherwise enforce a non-empty invariant.
5. **Separate boundary normalization from domain construction.** Boundary code
   handles nullable transport values; domain factory inputs remain non-nullable
   even when factories retain defensive null guards.
6. **Harden aggregate mutations.** Preserve contact primary cardinality when
   changing phone/email primary state, validate vehicle mode transitions, and
   make removal failure semantics consistent.
7. **Verify persistence and serialization.** Add conversions for every new
   `Maybe<T>` member before removing transitional nullable mapper code.

## Acceptance criteria for a follow-up compliance pass

- Public domain properties represent legitimate absence only with `Maybe<T>`.
- Optional mutations offer explicit set and clear behavior.
- Required domain parameters are declared non-nullable, and any null validation
  is documented as a defensive runtime safeguard rather than an accepted input.
- Unconditional operations do not return `Result`.
- All collection construction and replacement paths validate uniqueness and
  primary cardinality before state changes.
- Boundary projects, rather than domain entities, normalize nullable transport
  values.
- Tests cover `Some`, `None`, invalid-domain-rule, and persistence round-trip
  behavior for each migrated optional member.
