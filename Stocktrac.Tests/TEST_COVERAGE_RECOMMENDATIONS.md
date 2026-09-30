# Test coverage priorities

Existing tests cover email/contact behavior, financial value types, collection
invariants, atomic failures, formatting, and credit-card setters.

Add coverage in this order:

1. Contact primitives: `ContactPhone`, `Address`, `DateRange`,
   `DriversLicense`, and `BusinessName`.
2. Identity: `PersonName`, `Person`, and `SSN`.
3. Customers: `Customer`, `CustomerCode`, and `Business`.
4. Traditional and non-traditional `Vehicle` rules.
5. Employees, sale codes, company, and tenant validation.
6. API validators, claims, EF configuration, and health checks.

For each area, cover normalization, exact boundaries and errors, invalid enum
values, equality, and mutation atomicity. Test optional values in both states
and collections for duplicates and primary cardinality.

Use theories for boundaries, concrete aggregates for shared behavior, and a
clock for date-sensitive rules. Assert both the `Result` and retained state.
Use coverage reports to find gaps, not as a substitute for behavior-focused
assertions.
