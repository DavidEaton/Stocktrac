# Test coverage recommendations

## Review outcome

The existing suite exercises `Email`, the shared contact behavior through `Person`,
and the financial value types. The added tests fill the most important missing
branches in those classes: normalization, exact validation failures, mutation
atomicity, collection invariants, boundary values, every arithmetic operation,
overflow propagation, culture-independent formatting, and credit-card setters.

Phone and email replacement is covered as aggregate behavior on `Person`, including
successful replacement, clearing, validation failures, and mutation atomicity.

## Recommended next test classes

The domain contains many public behaviors with no dedicated tests yet. Add these
in small, feature-focused changes, in the following order.

1. **Contact primitives:** `Phone`, `Address`, `DateTimeRange`, `DriversLicense`,
   and `BusinessName`. Cover null/blank input, trimming, every
   minimum and maximum boundary, invalid enum values, equality, formatting, and
   the guarantee that a failed `New...`/setter call leaves the original unchanged.
2. **People and identity:** `PersonName`, `Person`, and `SSN`. Include today and
   future birthdays, the exact 120-year boundary, leap day, optional middle names,
   nine-digit and hyphenated SSNs, Unicode digits, masking, and `SSN.None` behavior.
3. **Customers:** `Customer`, `CustomerCode`, and `Business`. Exercise both person
   and business delegation paths, unsupported entity implementations, duplicate
   vehicles with persisted and transient IDs, removals of absent vehicles, code
   length boundaries, and notes truncation.
4. **Vehicles:** cover traditional versus non-traditional requirements, VIN length,
   nullable and boundary years, undefined states, all optional-field maximums,
   trimming, setters preserving state on failure, and `ToString` with no year.
5. **Employees, sale codes, company, and tenant:** build a validation matrix for
   required values, enum validity, numeric limits (including `NaN` and infinities
   where `double` is used), uniqueness/primary invariants, optional normalization,
   and setter atomicity.
6. **API layer:** add validator tests, `UserContext` tests for missing/malformed
   claims, EF model-configuration tests, and health-check outcomes for complete,
   missing, and invalid configuration.

## Test design guidance

- Prefer one theory row for each side of a boundary and separate assertions for
  the exact error contract.
- Assert both the returned `Result` and resulting object state; failure paths must
  not partially mutate an entity.
- Use concrete `Person` and `Business` instances to test `Contactable` behavior,
  rather than coupling tests to the abstract base class.
- Keep date assertions deterministic. Pass a clock into age/year policies before
  attempting exhaustive tests of behavior currently based on `DateTime.Today`.
- Add coverage collection in CI and treat the report as a discovery aid, not as a
  substitute for invariant- and boundary-focused assertions.
