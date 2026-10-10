# Customer and Vehicle native-union migration

Customer has subsequently been consolidated into one public union. See
[the consolidation report](customer-union-consolidation.md) for the current
case APIs, identity semantics, and verification. Vehicle APIs remain as below.

## Scope and decisions

This refactor follows the preliminary assessment in
[csharp15-union-readiness.md](csharp15-union-readiness.md), the
[refactoring prompt](../Stocktrac.Domain/Refactoring%20prompt%20-%20C%20sharp%20union.md),
the domain optionality policy, contact-collection design, and test-naming rules.
Only Customer and Vehicle are modeled with new unions. Person and Business are
changed only to remove the customer interface/discriminator they no longer need.
No other aggregate, infrastructure, endpoint, or contract is redesigned.

| Union | Domain concept and cases | Invalid states eliminated | Complexity removed |
| --- | --- | --- | --- |
| `Customer(PersonCustomer, BusinessCustomer)` | The person or business that is a customer | Arbitrary customer implementations; a discriminator inconsistent with the actual entity; an unsupported implementation accepted by creation but rejected by mutations | Repeated case dispatch, discriminator validation, catch-all failures, unsupported-entity exception |
| `VehicleKind(TraditionalVehicleKind, NonTraditionalVehicleKind)` | A traditional road vehicle or non-traditional equipment being serviced | A successful traditional case without VIN, make, or model; a mode change independent of the data it requires; clearing the last non-traditional description | Parallel mode/VIN/make/model storage, boolean-controlled construction, procedural mode transitions, invalid EF placeholder construction |

Customer has the lower migration risk and is prioritized first. Vehicle retains
all shared entity state on the aggregate and replaces its immutable kind in one
assignment only after validation succeeds. Exhaustive switches contain no
fallback arm. Factory validation still checks textual values; the union encodes
which required fields belong together, rather than proving arbitrary strings valid.

## Important API changes

- `Customer.Create` overloads accept a Person or Business plus classification
  and optional code, returning the corresponding validated customer case.
  Customer itself is the union; the CustomerEntity type/property and
  UpdateCustomerEntity are removed. Use ReplacePerson or ReplaceBusiness and
  retain the returned Customer. Only validated PersonCustomer/BusinessCustomer
  cases implicitly convert to Customer. Customer.Validate and fallible
  operations reject default/null-case unions.
- `ICustomerEntity`, `EntityType`, and the discriminator properties on Customer,
  Person, and Business are removed. `UnsupportedEntityTypeMessage` and the
  unsupported-case branches/exception are removed. `CustomerType` is retained.
- `Customer.Notes` is `Note`, reflecting the required notes on both underlying
  cases. Address and code remain genuine optional values. Contact behavior keeps
  the existing duplicate and primary-contact validation and snapshot semantics.
- `Vehicle.Create` now takes a validated `VehicleKind` followed by the shared
  fields. Its VIN/make/model/boolean parameter combination is removed.
- `Vehicle.NonTraditionalVehicle` and `UpdateNonTraditionalVehicle` are removed.
  Use exhaustive matching over `Kind` for case-specific behavior and
  `ReplaceKind` for an atomic transition with complete valid data.
- Traditional cases expose required `string` VIN, make, and model. Non-traditional
  cases expose `Maybe<string>` for those fields and enforce at least one of make
  or model. Aggregate/common projections expose `Maybe<string>` consistently;
  `Vehicle.Make` and `Vehicle.Model` therefore change from string to Maybe.
- VIN, make, and model mutations construct a new validated case before assignment.
  `RemoveVin` still fails for traditional vehicles. New `RemoveMake` and
  `RemoveModel` fail for traditional vehicles or removal of the last description.
- Existing update-operation length rules (2–50 characters for make/model) remain
  in place for both kinds. Non-traditional creation retains the old flexible
  description lengths. Present blank descriptions fail; absence must be explicit.
- Vehicle formatting omits absent year/make/model values, avoiding zero or empty
  strings as visible absence. The invalid parameterless EF constructor is removed.

Example construction:

```csharp
var result = TraditionalVehicleKind.Create("1HGCM82633A004352", "Honda", "Accord")
    .Bind(kind => Vehicle.Create(kind, Maybe<int>.None,
        Maybe<string>.None, Maybe<State>.None,
        Maybe<string>.None, Maybe<string>.None));
```

## API and persistence migration plan

The current API has no Customer or Vehicle endpoints/DTOs. Contracts contains no
transport types. ApplicationDbContext has no active Customer/Vehicle DbSets,
aggregate configurations, or migrations. The generic EntityConfiguration is
empty. There is therefore no live database schema or wire contract to migrate
in this change, and no union can be assumed to map automatically through EF.

When these boundaries are introduced, use the following explicit mapping:

1. **Customer storage:** keep a storage-only person/business discriminator and
   mutually exclusive PersonId/BusinessId foreign keys. A database check must
   require exactly the key corresponding to the discriminator. Load and validate
   the referenced Person or Business, call the corresponding
   Customer.Create overload with classification and optional code, then validate
   the returned Customer. Persist the shared customer identity and state
   separately from the referenced Person/Business identity. Unknown tags,
   missing references, or inconsistent columns are mapping failures. Derive the tag and exclusive key from exhaustive union
   matching on writes; do not store a second discriminator in the domain.
2. **Vehicle storage:** use a storage-only kind tag (or legacy boolean), nullable
   VIN/make/model columns, and separate shared registration/lifecycle columns.
   Require VIN/make/model for traditional rows and at least one description for
   non-traditional rows. Trim and normalize legacy missing non-traditional
   descriptions to Maybe.None at the boundary; reject invalid traditional rows
   rather than silently reclassifying them. Invoke the appropriate case factory,
   then Vehicle.Create, propagating failures rather than reading Result.Value
   unconditionally. Writes exhaustively match the kind; absent non-traditional
   fields map to database nulls. Existing Maybe converters remain appropriate
   for shared optional values.
3. **Transport:** introduce tagged DTOs in Contracts when endpoints exist.
   Normalize nullable request values with OptionalInput in the API, construct
   the case via its Result-returning factory, then create/replace the domain
   kind. A transition request supplies all fields required for the target case.
   Never deserialize a raw native union or use reflection to populate an invalid
   domain object. Do not expose the generated object-valued Value as a wire format.
4. **Round trips:** before enabling aggregate mapping, add provider-backed tests
   for both cases, optional values present/absent, shared state, identity and
   customer vehicle membership. Add corrupt-tag/row tests and check database
   constraints. Keep mapping records and reconstruction in the persistence layer;
   do not restore the invalid parameterless Vehicle constructor for EF.

Database-backed aggregate round trips remain deferred, as already recorded in
the optionality audit, until mapped roots and an integration-test provider exist.

## Considered and deferred alternatives

Employment lifecycle is outside the requested scope and still requires an
ordering/lifecycle design. Maybe/Result, independent contact permissions,
collection primary flags, payload-free classification enums, and product value
objects are deliberately retained for the reasons in the readiness assessment.
No further unions are introduced during the final review.

## Remaining limits

Native union structs still allow default/null-case values. Customer.Validate
and fallible Customer operations reject them, as do Vehicle aggregate factories
and replacements; raw empty-union projections are not a supported domain operation. Nullable annotations can also be bypassed
in existing Person/Business/contact factories; their pre-existing validation gaps
are outside this refactor. Existing employment date ordering and other aggregate
invariants continue to rely on procedural checks. VIN validation still enforces
the existing 17-character rule, without adding checksum/character validation.
Year validity is checked against the current date when constructed or updated.

## Original union migration verification

These results describe the original migration before Customer consolidation.
Current Customer verification is in the linked consolidation report.

- Native declaration/implicit-conversion/exhaustive-match executable probe: passed.
- Separate consumer project matching both cases of the actual domain unions:
  compiled with zero warnings/errors.
- Omitting Business from a CustomerEntity match or NonTraditionalVehicleKind
  from a VehicleKind match: rejected with CS8509, with warnings treated as errors.
- Implicitly assigning an unsupported object to CustomerEntity: rejected with CS0266.
- Passing Maybe.None instead of the required string VIN to the traditional case:
  rejected with CS1503.
- Complete solution build: passed with zero warnings/errors.
- Complete automated suite: 501 passed, 0 failed, 0 skipped. Tests cover legitimate
  cases, normalized inputs, length boundaries, absent/invalid values, default and
  null-case union rejection, contact validation, mutation atomicity, kind changes,
  and preservation of shared Vehicle state.
- `git diff --check`: passed. No third-party union implementation or new package
  dependency was introduced.

Validation uses .NET SDK `11.0.100-rc.1.26425.128` / Roslyn
`5.11.0-1.26425.128` with the repository's preview language configuration.
The test runner required local socket access; standard .NET test execution passed
after that access was available. Preview SDK support and the default-struct
limitation are recorded in the readiness document.
