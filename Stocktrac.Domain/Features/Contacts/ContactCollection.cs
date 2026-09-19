using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

internal sealed class ContactCollection<TContact, TIdentity>
    where TContact : class, IHasPrimary
    where TIdentity : notnull
{
    private readonly List<TContact> contacts;
    private readonly Func<TContact, TIdentity> getIdentity;

    internal ContactCollection(List<TContact> contacts, Func<TContact, TIdentity> getIdentity)
    {
        this.contacts = contacts;
        this.getIdentity = getIdentity;
    }

    internal IReadOnlyList<TContact> Items => [.. contacts];

    internal Result<TContact> Add(TContact? contact) =>
        contact is null
            ? Result.Failure<TContact>(Contactable.RequiredMessage)
            : Result.Success(contact)
                .Ensure(
                    requestedContact => !Contains(getIdentity(requestedContact)),
                    Contactable.NonuniqueMessage)
                .Ensure(
                    requestedContact => !requestedContact.IsPrimary || !HasPrimary,
                    Contactable.PrimaryExistsMessage)
                .Tap(contacts.Add);

    internal Result<TContact> Remove(TContact? contact) =>
        contact is null
            ? Result.Failure<TContact>(Contactable.RequiredMessage)
            : Result.Success(contact)
                .Bind(RemoveByIdentity);

    internal Result Replace(IReadOnlyList<TContact>? requestedContacts) =>
        Validate(requestedContacts, getIdentity)
            .Bind(validContacts =>
            {
                contacts.Clear();
                contacts.AddRange(validContacts);

                return Result.Success();
            });

    internal bool Contains(TIdentity identity) =>
        contacts.Any(contact => HasIdentity(contact, identity));

    internal bool HasPrimary => contacts.Any(contact => contact.IsPrimary);

    internal static Result<IReadOnlyList<TContact>> Validate(
        IReadOnlyList<TContact>? contacts,
        Func<TContact, TIdentity> getIdentity) =>
        contacts is null
            ? Result.Failure<IReadOnlyList<TContact>>(Contactable.RequiredMessage)
            : contacts.Any(contact => contact is null)
                ? Result.Failure<IReadOnlyList<TContact>>(Contactable.RequiredMessage)
                : Result.Success(contacts)
                    .Ensure(
                        contactList =>
                            contactList
                                .GroupBy(getIdentity)
                                .All(group => group.Count() == 1),
                        Contactable.NonuniqueMessage)
                    .Ensure(
                        contactList => contactList.Count(contact => contact.IsPrimary) <= 1,
                        Contactable.MultiplePrimariesMessage);

    private bool HasIdentity(TContact contact, TIdentity identity) =>
        EqualityComparer<TIdentity>.Default.Equals(getIdentity(contact), identity);

    private Result<TContact> RemoveByIdentity(TContact requestedContact)
    {
        var identity = getIdentity(requestedContact);
        var existingContact = contacts.FirstOrDefault(contact => HasIdentity(contact, identity));

        if (existingContact is null)
        {
            return Result.Failure<TContact>(Contactable.NotFoundMessage);
        }

        contacts.Remove(existingContact);
        return Result.Success(existingContact);
    }
}
