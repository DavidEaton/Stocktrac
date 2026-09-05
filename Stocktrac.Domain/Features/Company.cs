using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Customers;

namespace Stocktrac.Domain.Features;

public class Company : Entity
{
    // TODO: Move these constants to user-configurable settings in the future.
    // For now, they are hard-coded to match the current validation rules in StockTrac.
    public static readonly long MinimumValue = 0;
    public static readonly string MinimumValueMessage = $"Invoice Number Starting value must be >= {MinimumValue}.";
    public static readonly string RequiredMessage = $"Please include all required items.";

    public Business Business { get; private set; }
    public long NextInvoiceNumberOrSeed { get; private set; } = 0;
    private Company(Business business, long invoiceNumberSeed) =>
        (Business, NextInvoiceNumberOrSeed) = (business, invoiceNumberSeed);

    public static Result<Company> Create(Business business, long seed) =>
        Result.Success((Business: business, Seed: seed))
            .Ensure(values => values.Business is not null, RequiredMessage)
            .Ensure(values => values.Seed > MinimumValue, MinimumValueMessage)
            .Map(values => new Company(values.Business, values.Seed));

    public Result<long> SetInvoiceNumberSeed(long seed) =>
        seed <= MinimumValue || seed > long.MaxValue
            ? Result.Failure<long>(MinimumValueMessage)
            : Result.Success(NextInvoiceNumberOrSeed = seed);

    // EF requires a parameterless constructor
    private Company() =>
        Business = Business.Create(
            BusinessName.Create("Business Name").Value,
            Maybe<Address>.None)
            .Value;
}
