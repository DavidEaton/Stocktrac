namespace Stocktrac.Domain.Features.Customers;

public sealed class BusinessCustomer
{
    public Business Business { get; }
    internal CustomerState State { get; }

    internal BusinessCustomer(Business business, CustomerState state) =>
        (Business, State) = (business, state);
}
