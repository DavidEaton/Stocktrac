using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Domain.Features.Customers;

// Aggregate factories reject default or null-case values before using this union.
public readonly union CustomerEntity(Person, Business)
{
    internal readonly Contactable Contactable => this switch
    {
        Person person => person,
        Business business => business,
        _ => throw new InvalidOperationException("CustomerEntity must be either a Person or a Business.")
    };

    public readonly string Name => this switch
    {
        Person person => person.ToString(),
        Business business => business.ToString(),
        _ => throw new InvalidOperationException("CustomerEntity must be either a Person or a Business.")
    };
}
