namespace Stocktrac.Domain.Features.Contacts;

public abstract partial class Contactable
{
    protected sealed class ValidatedContactCollections
    {
        public IReadOnlyList<Phone> Phones { get; }
        public IReadOnlyList<Email> Emails { get; }

        internal  ValidatedContactCollections(
            IReadOnlyList<Phone> phones,
            IReadOnlyList<Email> emails) =>
            (Phones, Emails) = (phones, emails);
    }
}
