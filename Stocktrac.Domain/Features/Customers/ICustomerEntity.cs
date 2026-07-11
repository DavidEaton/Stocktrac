using Stocktrac.Domain.Features.Contact;

namespace Stocktrac.Domain.Features.Customers;

public interface ICustomerEntity : IContactable
{
    string DisplayName { get; }
    EntityType EntityType { get; }
}