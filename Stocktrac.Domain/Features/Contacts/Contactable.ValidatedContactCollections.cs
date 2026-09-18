namespace Stocktrac.Domain.Features.Contacts;

public abstract partial class Contactable
{
    protected sealed class ValidatedContactCollections
    {
        public IReadOnlyList<ContactPhone> Phones { get; }
        public IReadOnlyList<Email> Emails { get; }

        internal  ValidatedContactCollections(
            IReadOnlyList<ContactPhone> phones,
            IReadOnlyList<Email> emails) =>
            (Phones, Emails) = (phones, emails);
    }
}
