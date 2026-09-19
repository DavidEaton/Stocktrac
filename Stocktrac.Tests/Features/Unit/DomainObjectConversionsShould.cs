using CSharpFunctionalExtensions;
using Shouldly;

using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Customers;
using Stocktrac.Domain.Features.Financial;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Tests.Features.Unit;

public class DomainObjectConversionsShould
{
    [Fact]
    public void CreateStringBackedValuesFromValidStrings()
    {
        AssertStringValue("123 Main Street", AddressLine.Create);
        AssertStringValue("Acme Automotive", BusinessName.Create);
        AssertStringValue("Springfield", City.Create);
        AssertStringValue("D123456", DriversLicenseNumber.Create);
        AssertStringValue("owner@example.com", EmailAddress.Create);
        AssertStringValue("Remember this", Note.Create);
        AssertStringValue("15551234567", PhoneNumber.Create);
        AssertStringValue("90210", PostalCode.Create);
        AssertStringValue("CUST-100", CustomerCode.Create);
        AssertStringValue("Visa", CreditCardName.Create);
        AssertStringValue("CAD", CurrencyCode.Create);
        AssertStringValue("123456789", SSN.Create);
    }

    [Fact]
    public void CreateAmountFromDecimalAndConvertBack()
    {
        var result = Amount.FromDecimal(123.45m);

        decimal value = result.Value;
        value.ShouldBe(123.45m);
    }

    [Fact]
    public void CreateBirthdayFromDateTimeAndConvertBack()
    {
        var date = new DateTime(1990, 6, 15);

        var result = Birthday.Create(date);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEquivalentTo(date);
    }

    private static void AssertStringValue<T>(
        string expected,
        Func<string, Result<T>> factory)
    {
        var result = factory(expected);

        result.IsSuccess.ShouldBeTrue();
        result!.Value!.ToString().ShouldBe(expected);
    }
}