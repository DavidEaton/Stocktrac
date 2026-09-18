namespace Stocktrac.Domain.Features.Contacts;

public abstract partial class Contactable
{
    protected sealed class ValidatedContactCollections
    {
        public IReadOnlyList<ContactPhone> Phones { get; }
        public IReadOnlyList<ContactEmail> Emails { get; }

        internal  ValidatedContactCollections(
            IReadOnlyList<ContactPhone> phones,
            IReadOnlyList<ContactEmail> emails) =>
            (Phones, Emails) = (phones, emails);
    }
}
