# Customer union consolidation

## Decision and scope

This change starts from fetched `origin/main` at `73ad46e`. The Customer class
and its nested CustomerEntity union previously made callers distinguish the
customer aggregate from the person/business alternative under a second customer
name. They are replaced by one native C# 15 union:

```csharp
public readonly union Customer(PersonCustomer, BusinessCustomer)
```

PersonCustomer contains a required Person; BusinessCustomer contains a required
Business. Their constructors are internal: public creation goes through the
Result-returning Customer.Create overloads. Both cases retain customer-specific
identity, classification, optional code, contact preferences, and vehicles in
one internal CustomerState entity. CustomerState is an implementation detail,
not a second public customer type, and contains no alternative/discriminator.
The union exposes common projections and behavior directly. The two cases are
mutually exclusive and can be matched exhaustively by consumers.

Person and Business remain reusable domain entities. A bare Person/Business
cannot implicitly become a customer with missing customer metadata. Validated
customer cases implicitly convert to Customer through the native compiler
feature. No handwritten union, additional package, public setters, nullable
case-specific fields, discriminator enum, or generic wrapper is introduced.

## Invariants and complexity

The existing closed-set guarantee is retained: arbitrary Contactable/interface
implementations cannot be Customer cases. Public factories reject null subjects
and undefined CustomerType values before constructing aggregate state. There
is no longer a Customer.CustomerEntity property or another union for callers
to construct or unwrap. PersonCustomer and BusinessCustomer are meaningful
customer forms, with their required subject stored only on the corresponding
case. Classification is independent of person/business form and remains an enum.

Customer delegates contacts to the case's Person/Business, preserving required
Note, optional Address, contact duplicate/primary rules, validation before
mutation, and collection snapshots. Code remains Maybe<CustomerCode>, with
explicit update and removal. Vehicles remain a non-null snapshot; duplicate,
missing, and null vehicle failures preserve membership.

Identity and common mutable state are retained by reference, including through
case replacement. Union equality, object equality, hashing, and operators use
that customer identity via the existing Entity semantics. Separate unsaved
customers remain distinct even when they share the same Person or have identical
metadata. Neither the subject's identity nor the selected case defines the
customer's identity. This preserves the original distinction between customer
and person/business keys without retaining a public Customer entity class.

## Breaking API changes

- Customer is a readonly native union struct, so it no longer inherits Entity
  and cannot be used with generic APIs constrained to Entity or EF DbSet<T>.
  Id remains a common read-only projection. Code relying on reference identity
  or null Customer values must use customer equality and explicit optionality.
- CustomerEntity and Customer.CustomerEntity are removed. Customer.Create now
  overloads on Person or Business, alongside CustomerType and Maybe<CustomerCode>.
  To inspect the subject, match Customer directly on PersonCustomer/BusinessCustomer.
- UpdateCustomerEntity is removed. ReplacePerson and ReplaceBusiness return
  Result<Customer>, preserving customer identity and shared metadata/vehicles.
  Retain the returned value to select the new case. The original union value
  keeps its original person/business reference; copies and replacements share
  common aggregate mutations. Case-specific contact operations use the subject
  selected by that union value. Previously stored values do not silently change
  their case when another value is replaced.
- Customer.Validate returns a failure for a default or null-case union. Use it
  when accepting a Customer at an application/persistence boundary. All fallible
  Customer operations also reject empty unions with RequiredMessage. Projections
  and infallible removal on an empty union are programmer errors and throw;
  an empty native struct is not a domain absence value.

```csharp
var created = Customer.Create(person, CustomerType.Retail, Maybe<CustomerCode>.None);
var replaced = created.Bind(customer => customer.ReplaceBusiness(business));
// Retain replaced.Value only after handling failure.

string Describe(Customer customer) => customer switch
{
    PersonCustomer individual => individual.Person.ToString(),
    BusinessCustomer commercial => commercial.Business.ToString()
};
```

## Persistence and transport

There are no active Customer mappings, DbSets, migrations, endpoints, or DTOs.
The existing customer DbSet placeholder comment now points to explicit mapping
rather than suggesting that EF can accept a native union struct.

Future persistence must keep a customer key and shared customer fields separate
from the Person/Business foreign key. Use a storage-only tag with exclusive
foreign keys and database constraints. Reconstruct through the corresponding
Customer.Create overload, restore identity through a deliberately designed
persistence boundary, and validate before exposing the result. Writes match the
Customer cases to derive the tag and referenced key; never serialize generated
Value as the transport/storage format or persist CustomerState as another root.
Native unions do not automatically supply ORM mapping or identity restoration.
Provider-backed aggregate round trips and corrupt-row tests remain deferred
until mappings and an integration provider exist, as in the optionality audit.

## Alternatives and remaining limits

A Customer(Person, Business) union was considered but rejected: those reusable
entities do not own customer identity, classification, code, preferences, or
vehicles. Adding aggregate fields to the union itself would let generated
constructors/implicit conversions bypass their initialization. The validated
customer cases retain the aggregate boundary without adding another public
customer abstraction. A new public customer base class was also unnecessary;
shared state is kept internal through composition.

Maybe, Result, classification enums, independent contact flags, and Vehicle's
existing kind union remain appropriate and unchanged. No new union candidates
were introduced in unrelated domain, employee, financial, or infrastructure code.
Pre-existing Person/Business factory validation gaps remain outside this scope.
Native unions still allow default structs and null-case references when nullable
annotations are bypassed. These values cannot become valid customers through
Validate or any fallible operation; the preview compiler cannot remove the
language-level default-struct limitation.

## Verification

Validation uses the existing .NET SDK `11.0.100-rc.1.26425.128`, preview language
version, and cached dependencies. A native-union executable probe confirms
syntax, implicit conversions, exhaustive matching, and custom identity equality.
The complete solution and automated suite are required; VSTest needs local
socket access in this environment.

- Complete solution build: passed with zero warnings/errors.
- Complete automated suite: 630 passed, zero failed or skipped. Tests cover both
  cases, all replacement directions, retained aggregate identity/state, contacts,
  optional code, vehicle snapshots and failures, implicit case conversions,
  independent customer identities, shared mutations, and empty union rejection.
- Separate consumer project: both cases matched exhaustively and implicitly
  converted to Customer with zero warnings/errors.
- Omitting BusinessCustomer from a match: rejected with CS8509 with warnings as
  errors. Implicitly converting a bare Person to Customer: rejected with CS0029.
  Constructing an unvalidated PersonCustomer externally: rejected with CS1729.
- Native executable probe: passed, including custom union equality.
- `git diff --check`: passed. No new dependencies or language configuration.

```sh
dotnet build Stocktrac.slnx --no-restore --disable-build-servers -m:1 -p:UseSharedCompilation=false
dotnet test Stocktrac.Tests/Stocktrac.Tests.csproj --no-build --no-restore --disable-build-servers -m:1
```

Consumer probes used cached packages with NuGet audit disabled only for the
scratch consumer project to avoid network access during its restore. Project
package references and warning policies are unchanged.
