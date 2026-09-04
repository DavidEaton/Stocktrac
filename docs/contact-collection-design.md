# Contact collection design

## Decision

Phones and emails belong directly to a `Contactable` aggregate. There is no
separate `ContactDetails` domain value object. Address changes continue to use
`SetAddress` and `ClearAddress`, while phone and email collections use
`ReplacePhones` and `ReplaceEmails`.

## Collection replacement contract

Each replacement method treats its argument as the complete requested state:

- an empty collection clears the existing collection;
- a null collection or null member is rejected;
- duplicate phone numbers or email addresses are rejected;
- more than one primary item is rejected; and
- validation happens before mutation, so a failed replacement preserves the
  existing collection.

This contract removes the previous ID-based add/update inference. Callers no
longer need to construct an intermediate object or depend on persistence IDs to
express a collection edit. A request handler can map its input to domain `Phone`
and `Email` instances, call the corresponding replacement method, and return the
domain result directly.

## Why replacement rather than synchronization

The aggregate owns the collection invariant, but it does not need to own a
generic diff engine. Entity Framework's change tracker can observe the resulting
collection membership. Keeping persistence-specific reconciliation outside the
domain leaves the domain operation small, deterministic, and easy to test.
