# Domain Optionality Policy

## Purpose

This policy defines how Stocktrac represents missing or optional values in the
domain project. Its goal is to ensure that valid domain objects do not expose
`null` as an ambiguous representation of domain state, while preserving
practical interoperability with .NET, persistence, serialization, and other
application boundaries.

## Policy statement

> Valid domain objects must not expose `null` as the representation of optional
> domain state. Use `Maybe<T>` for legitimate absence, collections for
> zero-or-more relationships, and `Result<T>` for operations that may fail.
> Nullable values may still be accepted or encountered at application,
> framework, serialization, and persistence boundaries.

Domain APIs declare required inputs as non-nullable so nullable-aware callers
receive compiler diagnostics when they might pass `null`. Domain objects may
still guard against `null` at runtime and return a validation failure; that
defensive behavior does not make `null` part of the accepted contract. Code at
an external boundary remains responsible for handling nullable input before it
calls the domain.

## Required practices

### Optional domain members

Use `Maybe<T>` when the absence of a value is a valid, meaningful state of a
domain object.

Examples include:

- a person's birthday or driver's license;
- a tenant's logo URL;
- a person's middle name;
- a customer's code;
- a business's contact person;
- a vehicle's year or plate jurisdiction; and
- the date a credit card was added to a deposit.

Expose these members as `Maybe<T>` and represent absence with
`Maybe<T>.None`. Do not require callers to infer absence from `null`, an empty
string, zero, a default date, or another sentinel value.

```csharp
public Maybe<Birthday> Birthday { get; private set; }

public void RemoveBirthday() =>
    Birthday = Maybe<Birthday>.None;
```

### Required domain members

Use a non-nullable type when a value is required for a valid domain object.
Domain construction and mutation APIs must declare required arguments as
non-nullable. They must validate domain rules before committing state, and may
also retain defensive null checks for callers that bypass nullable analysis.
Such checks are runtime safeguards, not permission to pass `null`. A
successfully created domain object must satisfy its invariants.

### Collections

Use an empty collection to represent zero related items. Domain collection
properties must not return `null`, and `Maybe<IReadOnlyList<T>>` must not be
used merely to distinguish an absent collection from an empty one unless the
domain explicitly assigns different meanings to those states.

Collection parameters and their element types are non-nullable unless their
signatures explicitly say otherwise. A `null` collection or member is therefore
a programming error. Factories and replacement methods must validate genuine
collection invariants, such as uniqueness and cardinality, before constructing
an aggregate or mutating its current collection. Constructor paths must accept
only collections that have passed the same validation, so a derived constructor
cannot bypass aggregate invariants.

### Fallible operations

Use `Result<T>` or `Result` to represent validation or operation failure.
`Maybe<T>` communicates presence or absence; it must not be used to hide an
error or discard a failure reason.

Use an ordinary method returning `void` when an operation cannot fail under its
non-nullable contract. Do not wrap an unconditional assignment or clear
operation in a success-only `Result`. A `Result` remains appropriate for such
operations as adding, replacing, or removing collection entries when duplicate,
primary-cardinality, or not-found outcomes are expected domain failures.

### Mutation method names

Use `Set...` and `Clear...` for entity methods that mutate the current instance.
These names make the state change explicit at the call site: `Set...` assigns a
value, while `Clear...` removes an optional value.

Reserve `With...` and `Without...` for immutable value-object operations that
return a new instance and leave the original unchanged. Using those names for
in-place entity mutation is misleading because callers commonly understand the
`With...` convention as copy-with behavior.

```csharp
// Entity: mutates this instance.
customer.SetAddress(address);
customer.ClearAddress();

// Value object: returns a changed copy.
DateTimeRange extended = period.WithEnd(newEnd);
```

### Boundary normalization

APIs, serializers, ORMs, legacy callers, and other external systems may supply
`null`. Boundary code may therefore accept nullable parameters when that
accurately describes possible input. It must validate or normalize those
values before calling a non-nullable domain API or allowing them to become
domain state.

For optional input, convert `null` to `Maybe<T>.None`. For required input,
return an appropriate boundary validation failure. This conversion should
happen in an application mapper, request validator, or other clearly identified
boundary—not inside a domain method whose signature already declares the value
to be required.

```csharp
Maybe<string> logoUrl = string.IsNullOrWhiteSpace(request.LogoUrl)
    ? Maybe<string>.None
    : Maybe<string>.From(request.LogoUrl.Trim());
```

## Permitted uses of nullable types and `null`

The policy does not prohibit all syntactic uses of nullable types or `null`.
They remain appropriate in the following situations.

### .NET and framework contracts

Honor platform contracts such as `Equals(object? obj)` and APIs whose defined
representation of missing data is `null`. Do not replace their signatures with
`Maybe<T>` when doing so would violate or obscure the contract.

### Input validation and defensive checks

Boundary-facing methods may declare nullable transport input because absence is
part of that boundary contract. Domain methods may also defensively check for
`null`, especially for callers from languages and frameworks without equivalent
nullability guarantees, but required domain parameters remain non-nullable.
This preserves compiler diagnostics for ordinary C# callers while keeping the
runtime guard.

### Persistence and serialization

Infrastructure code may use nullable storage representations when required by
the ORM, database schema, or serializer. Mapping must translate those values to
and from the domain's `Maybe<T>` representation without leaking nullable state
through the domain API.

### Encapsulated implementation details

An internal nullable field may be used as an implementation detail when all of
the following are true:

1. it cannot escape through the public domain API;
2. every observable domain value remains valid and unambiguous;
3. it does not weaken an invariant; and
4. replacing it with `Maybe<T>` would not improve clarity or safety.

This exception should be uncommon and documented where its purpose is not
obvious.

## Modeling guidance

Choose the type according to the meaning that callers need:

| Meaning | Representation |
| --- | --- |
| A value is required | Non-nullable `T` |
| A value may legitimately be absent | `Maybe<T>` |
| Zero or more values | Non-null collection of `T` |
| An operation may succeed or fail | `Result` or `Result<T>` |
| An operation is unconditional | Ordinary method, usually returning `void` |
| External input may be missing | Nullable boundary input, then normalize |
| A framework contract specifies nullability | Follow the framework contract |

Do not nest optional representations such as `Maybe<T?>`. Select one absence
model at the domain boundary and use it consistently.

## Review checklist

When creating or reviewing domain code, verify that:

- [ ] Every public domain member has an intentional absence model.
- [ ] Optional domain members use `Maybe<T>` and explicit `None` semantics.
- [ ] Required domain members cannot be absent after successful construction.
- [ ] Required domain parameters are non-nullable, even when their implementations
      retain defensive null guards.
- [ ] Collections are non-null and use an empty collection for zero items.
- [ ] Every constructor path receives collections that have already passed the
      aggregate's uniqueness and primary-cardinality validation.
- [ ] Failures use `Result`, not `None`, when callers need an error reason.
- [ ] Unconditional assignments and clear operations do not return success-only
      results.
- [ ] Entity mutations use `Set...`/`Clear...`; immutable copy operations use
      `With...`/`Without...`.
- [ ] Nullable boundary input is validated or normalized before entering the
      domain model.
- [ ] Persistence and serialization concerns do not leak nullable state into
      the domain API.
- [ ] Remaining uses of `null` are framework contracts, boundary checks, tests
      of programmer errors, or justified encapsulated implementation details.
- [ ] Tests cover both `Some` and `None` behavior for optional members.

## Migration approach

Apply this policy incrementally rather than mechanically replacing every `?`
or `null` token:

1. Inventory nullable domain properties and classify each as required,
   optional, collection, failure, boundary input, or implementation detail.
2. Convert genuine optional domain properties to `Maybe<T>`.
3. Add explicit set and clear operations where behavior changes domain state.
4. Normalize nullable input in application-layer mappers before invoking
   non-nullable domain APIs; keep any domain null checks strictly defensive.
5. Add persistence conversions where infrastructure cannot map `Maybe<T>`
   directly.
6. Update tests to assert presence, absence, validation, and round-trip
   persistence behavior.
7. Retain justified nullable signatures and document non-obvious exceptions.

Success means that consumers of valid domain objects can understand and handle
absence from the types alone. It does not mean that the repository contains no
uses of the `null` keyword.
