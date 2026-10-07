# C# 15 union refactoring readiness

## Decision

The union refactoring is intentionally **not attempted** in the current environment.
The repository requests `net11.0` and `LangVersion` `preview`, but no .NET SDK or C#
compiler is installed. Consequently, this environment cannot verify that the compiler
used by the repository accepts the C# 15 `union` syntax. Project configuration alone is
not evidence that a particular preview compiler implements a proposal.

Installing an SDK was also unavailable: downloading the official `dotnet-install.sh`
bootstrap script returned HTTP 403. No union syntax, hand-written union emulation, or
third-party discriminated-union package has been introduced as a substitute.

## Scope inspected

Before making this decision, the review covered:

- all tracked C# files in `Stocktrac.Domain`;
- all tracked C# tests in `Stocktrac.Tests`;
- the Domain, Tests, API, and Contracts project files;
- compiler and language-version configuration available in the repository;
- domain-type usages in the API and Contracts projects; and
- persistence configuration in the API project.

## Preliminary candidate assessment

These candidates should be revisited only after the actual repository compiler passes a
minimal compile probe using the supported, documented C# 15 union syntax.

1. **Customer entity — high value.** `Customer` stores an `ICustomerEntity`, while the
   implementation repeatedly switches between the closed `Person` and `Business`
   implementations and retains an `EntityType` discriminator. A domain union with
   person-customer and business-customer cases could remove unsupported implementations,
   the duplicated discriminator, catch-all branches, and the “unsupported entity”
   exception. Persistence mapping must be designed explicitly before migration.
2. **Vehicle kind — high value, higher migration risk.** `Vehicle` combines
   `NonTraditionalVehicle` with an optional VIN and conditionally validates make/model.
   Traditional and non-traditional vehicle cases could make “traditional without VIN”
   and mode transitions that invalidate VIN requirements unrepresentable. Shared mutable
   lifecycle and registration data need careful placement, so this should not be changed
   without compiler-backed tests and an API/persistence migration plan.
3. **Employment lifecycle — possible value.** `Employee` combines `Hired` with optional
   `Exited` and enforces ordering procedurally. Active and exited-employment cases could
   couple the exit date to the exited case. This needs further analysis because absence of
   an exit date may be genuine lifecycle optionality, and a union may add more complexity
   than it removes.

The following were deliberately rejected during the preliminary review:

- `Maybe<T>` and `Result<T>` remain the appropriate existing abstractions for optionality
  and operation outcomes.
- `ContactPreferences` flags are independent permissions, not mutually exclusive cases.
- primary-contact flags describe a contact's role within a collection; replacing them
  with a union would not by itself enforce the collection-wide single-primary invariant.
- `PhoneType`, `EmploymentRole`, `EmployeeExpenseCategory`, `CreditCardFeeType`, and
  `CustomerType` currently carry no case-specific data. Replacing plain closed labels
  with payload-free unions would not clearly reduce domain complexity.
- `DateRange`, addresses, names, money, and other validated value objects are products of
  their constituent values rather than alternative forms.

## Verification status

- **Compiler support:** unverified because neither `dotnet`, `csc`, nor `mcs` exists in
  the environment.
- **Build:** not run; `dotnet` is unavailable.
- **Tests:** not run; `dotnet` is unavailable.
- **Unions introduced:** none.
- **Domain APIs removed or changed:** none.
- **Remaining invalid-state risks:** unchanged, including acceptance of an arbitrary
  `ICustomerEntity` implementation and vehicle/employment lifecycle invariants whose
  correctness still depends on procedural validation rather than their type shapes.

The required next step is to provide the repository's intended .NET 11/C# 15 preview SDK
and successfully compile a minimal native-union probe. Only then should candidate design,
incremental migration, persistence adaptation, and full solution verification proceed.
