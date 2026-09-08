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
Construction and mutation APIs must validate required inputs before committing
state. A successfully created domain object must satisfy its invariants.

### Collections

Use an empty collection to represent zero related items. Domain collection
properties must not return `null`, and `Maybe<IReadOnlyList<T>>` must not be
used merely to distinguish an absent collection from an empty one unless the
domain explicitly assigns different meanings to those states.

### Fallible operations

Use `Result<T>` or `Result` to represent validation or operation failure.
`Maybe<T>` communicates presence or absence; it must not be used to hide an
error or discard a failure reason.

### Boundary normalization

APIs, serializers, ORMs, legacy callers, and other external systems may supply
`null`. Boundary code may therefore accept nullable parameters when that
accurately describes possible input. It must validate or normalize those
values before they become domain state.

For optional input, convert `null` to `Maybe<T>.None`. For required input,
return an appropriate failure. This conversion should happen in an application
mapper, factory, or other clearly identified boundary.

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

Public or boundary-facing methods may defensively check for `null`, including
when nullable-reference analysis says a parameter is non-nullable. Such checks
protect the domain from callers that bypass compiler warnings or originate in
languages and frameworks without equivalent nullability guarantees.

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
| External input may be missing | Nullable boundary input, then normalize |
| A framework contract specifies nullability | Follow the framework contract |

Do not nest optional representations such as `Maybe<T?>`. Select one absence
model at the domain boundary and use it consistently.

## Review checklist

When creating or reviewing domain code, verify that:

- [ ] Every public domain member has an intentional absence model.
- [ ] Optional domain members use `Maybe<T>` and explicit `None` semantics.
- [ ] Required domain members cannot be absent after successful construction.
- [ ] Collections are non-null and use an empty collection for zero items.
- [ ] Failures use `Result`, not `None`, when callers need an error reason.
- [ ] Nullable boundary input is validated or normalized before entering the
      domain model.
- [ ] Persistence and serialization concerns do not leak nullable state into
      the domain API.
- [ ] Remaining uses of `null` are framework contracts, defensive checks, or
      justified encapsulated implementation details.
- [ ] Tests cover both `Some` and `None` behavior for optional members.

## Migration approach

Apply this policy incrementally rather than mechanically replacing every `?`
or `null` token:

1. Inventory nullable domain properties and classify each as required,
   optional, collection, failure, boundary input, or implementation detail.
2. Convert genuine optional domain properties to `Maybe<T>`.
3. Add explicit set and clear operations where behavior changes domain state.
4. Normalize nullable input in factories and application-layer mappers.
5. Add persistence conversions where infrastructure cannot map `Maybe<T>`
   directly.
6. Update tests to assert presence, absence, validation, and round-trip
   persistence behavior.
7. Retain justified nullable signatures and document non-obvious exceptions.

Success means that consumers of valid domain objects can understand and handle
absence from the types alone. It does not mean that the repository contains no
uses of the `null` keyword.
