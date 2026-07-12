using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Domain.Features.Customers;

public interface ICustomerEntity : IContactable
{
    EntityType EntityType { get; }
}