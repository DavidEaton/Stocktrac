using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Domain.Features.Customers;

// Aggregate factories reject default or null-case values before using this union.
public union CustomerEntity(Person, Business)
{
    internal Contactable Contactable => this switch
    {
        Person person => person,
        Business business => business
    };

    public string Name => this switch
    {
        Person person => person.ToString(),
        Business business => business.ToString()
    };
}
