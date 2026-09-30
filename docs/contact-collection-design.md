# Contact collections

`Contactable` directly owns phones and emails. `ReplacePhones` and
`ReplaceEmails` treat input as the complete new state:

- empty input fails and preserves the collection;
- null members, duplicates, and multiple primary contacts fail; and
- validation completes before mutation, so failure preserves current state.

Removing every item in a collection is deliberately separate from replacement.
Call `RemovePhones` or `RemoveEmails` when removing every contact is intended;
like the other mutation operations, each returns a result.

The internal `ContactCollection<TContact, TIdentity>` shares identity,
uniqueness, and primary-contact rules. The domain does not perform an ID-based
diff; EF tracks the resulting membership.

Address mutations follow the same explicit style, using operations such as
`ReplaceAddressLine1` and `RemoveAddressLine2`.
