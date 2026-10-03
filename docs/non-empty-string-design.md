# `NonEmptyString` design review

## Decision

`NonEmptyString` is a small, well-encapsulated value object for one invariant: a
successfully created instance contains a trimmed, non-blank string. Its private
constructor and `Create` factory make the supported construction path explicit,
and the sealed record supplies immutable value equality without exposing a
mutation API.

This is an appropriate design as long as `NonEmptyString` remains a structural
building block rather than becoming a substitute for domain concepts. For
example, a business name and a person's name may both contain non-empty text,
but they are not interchangeable merely because they share that invariant.

## Encapsulation boundary

The type guarantees the following for every non-null value obtained through its
public API:

- creation fails for `null`, empty, or whitespace-only input;
- surrounding whitespace is removed before the value is stored;
- the stored value cannot be reassigned; and
- equality is based on the normalized value.

Returning `Value` does not leak mutable state because `string` is immutable.
`ToString` is a representation convenience, not domain behavior.

The private constructor prevents ordinary callers from constructing an invalid
instance. It should not be described as an absolute runtime security boundary:
reflection and some serializers can bypass normal construction rules. Persistence
adapters must therefore be configured and tested to rehydrate the type safely.
Also, because this is a reference type, `null` remains possible at unguarded
interop, reflection, or `default` boundaries; nullable annotations protect
normal compiled C# call sites but do not make null impossible at runtime.

Record `with` expressions can copy an instance, but they cannot replace the
get-only `Value`. Consequently, the generated copy operation preserves the
invariant.

## Adding behavior

Domain-specific extension methods are useful when an operation:

- is a stateless convenience over any `NonEmptyString`;
- does not need privileged access to the type; and
- has an unambiguous meaning wherever its namespace is imported.

Extensions do not actually extend the type's contract. They are selected
statically, can conflict across namespaces, cannot enforce their use, and cannot
add interfaces or operators. Behavior that establishes a new invariant, gives
the value a distinct domain identity, or must always accompany the value belongs
on a dedicated value object instead. Repeated domain-specific extensions are a
signal to introduce such a type rather than grow a generic extension-method
surface.

## Compatibility considerations

The factory's normalization and error text are observable API behavior. Changing
trimming rules, comparison semantics, or the failure contract can affect equality,
dictionary keys, persistence, and callers that display or inspect the error.
Those changes require focused tests and a compatibility review even though the
constructor remains private.
