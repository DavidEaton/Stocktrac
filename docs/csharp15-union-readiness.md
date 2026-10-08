# C# 15 union refactoring readiness

## Decision

Native union support is now verified. The earlier compiler blocker is resolved by
.NET SDK `11.0.100-rc.1.26425.128` and Roslyn `5.11.0-1.26425.128`
(commit `3551975be08744f0418857c5bed8ab1545c5dd47`). Domain, API, and Tests
continue to target `net11.0` with `LangVersion` `preview`; Contracts remains
unchanged at `net10.0`.

A minimal executable using the documented `union Subject(Person, Business);`
syntax compiled with warnings as errors and ran, printing `Person, Business`.
The probe used implicit conversions and exhaustive matching without a fallback.
The syntax follows the [C# union proposal](https://github.com/dotnet/csharplang/blob/main/proposals/csharp-15.0/unions.md).
No handwritten union emulation or additional union package is used.

The scoped refactoring implements only the Customer and Vehicle candidates below.
See [the migration report](customer-vehicle-union-migration.md) for APIs,
invalid-state analysis, persistence design, and verification.

## Scope inspected

The review covered the domain model, existing tests, all project files,
compiler/language configuration, affected usages, the API's persistence and
transport seams, and all tracked project documentation, including
[the refactoring prompt](../Stocktrac.Domain/Refactoring%20prompt%20-%20C%20sharp%20union.md).

## Preliminary candidate assessment

1. **Customer entity — high value; implemented.** The old `ICustomerEntity`
   accepted arbitrary implementations, while behavior repeatedly matched Person
   and Business and checked `EntityType`. `CustomerEntity(Person, Business)`
   closes that set and centralizes shared behavior. The redundant interface,
   discriminator enum and properties, unsupported-case branches, and exception
   are removed. Customer classification (`CustomerType`) is independent and
   stays an enum. The aggregate boundary rejects default or null-case unions.
2. **Vehicle kind — high value, higher migration risk; implemented.**
   `VehicleKind(TraditionalVehicleKind, NonTraditionalVehicleKind)` owns the
   mutually exclusive forms. Traditional cases require a validated VIN, make,
   and model. Non-traditional cases retain a genuine optional VIN and require at
   least one description. The aggregate retains identity, year, registration,
   and active state. Complete validated case replacement supersedes the boolean
   mode setter. Persistence and API migration are designed explicitly in the
   migration report; this repository has no active aggregate mappings or DTOs.
3. **Employment lifecycle — possible value; deferred.** Employee remains outside
   the requested scope. Hired/exited ordering still depends on validation; a
   separate lifecycle design would be needed to justify a union.

The following remain deliberately rejected:

- `Maybe<T>` and `Result<T>` model optionality and outcomes appropriately.
- Contact permissions are independent flags, not alternative forms.
- Primary-contact flags do not themselves express collection cardinality.
- `PhoneType`, `EmploymentRole`, `EmployeeExpenseCategory`, `CreditCardFeeType`,
  and `CustomerType` are labels without distinct case payloads.
- Dates, addresses, names, money, and other value objects are products, not sums.

## Compiler limitations

Native union declarations generate structs. `default(CustomerEntity)` and
`default(VehicleKind)` contain a null `Value`; callers can also bypass nullable
annotations and pass a null reference to a generated case constructor. This is
not a legitimate domain case. Aggregate factories and replacements reject those
values with a failure result before mutation. Successful Vehicle cases have
private constructors and immutable properties, so their required data cannot be
removed independently. Using a raw empty union's projections remains a
programmer error; the type system does not eliminate default struct values.

Preview configuration alone still does not establish compiler support. Use an
SDK implementing native union declarations and verify compilation when upgrading.
