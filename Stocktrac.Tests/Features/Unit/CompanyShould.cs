using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Customers;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Tests.Features.Unit;

public class CompanyShould
{
    [Fact]
    public void ReturnFirstError_On_Create_WhenBusinessAndSeedAreInvalid()
    {
        var result = Company.Create(null!, 0);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Company.RequiredMessage);
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(long.MaxValue, true)]
    public void EnforcePositiveSeed_On_Create_WhenSeedIsAtBoundary(long seed, bool valid)
    {
        var business = Business.Create(
            BusinessName.Create(NonEmptyString.Create("Acme").Value).Value,
            Maybe<Address>.None,
            Note.Create(NonEmptyString.Create("Notes").Value).Value,
            Maybe<Person>.None, [], []).Value;

        var result = Company.Create(business, seed);

        result.IsSuccess.ShouldBe(valid);
        if (valid)
        {
            result.Value.Business.ShouldBeSameAs(business);
            result.Value.NextInvoiceNumberOrSeed.ShouldBe(seed);
        }
        else
            result.Error.ShouldBe(Company.MinimumValueMessage);
    }
}
