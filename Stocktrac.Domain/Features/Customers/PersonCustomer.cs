using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Domain.Features.Customers;

public sealed class PersonCustomer
{
    public Person Person { get; }
    internal CustomerState State { get; }

    internal PersonCustomer(Person person, CustomerState state) =>
        (Person, State) = (person, state);
}
