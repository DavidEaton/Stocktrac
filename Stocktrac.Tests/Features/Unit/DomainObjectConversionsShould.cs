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
        AssertStringValue("123 Main Street", AddressLine.Create, value => value.Value);
        AssertStringValue("Acme Automotive", BusinessName.Create, value => value.Name);
        AssertStringValue("Springfield", City.Create, value => value.Value);
        AssertStringValue("D123456", DriversLicenseNumber.Create, value => value.Number);
        AssertStringValue("owner@example.com", EmailAddress.Create, value => value.Value);
        AssertStringValue("Remember this", Note.Create, value => value.Value);
        AssertStringValue("15551234567", PhoneNumber.Create, value => value.Value);
        AssertStringValue("90210", PostalCode.Create, value => value.Value);
        AssertStringValue("CUST-100", CustomerCode.Create, value => value.Value);
        AssertStringValue("Visa", CreditCardName.Create, value => value.Value);
        AssertStringValue("CAD", CurrencyCode.Create, value => value.Value);
        AssertStringValue("123456789", SSN.Create, value => value.Value);
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
        result.Value.ShouldBeSameAs(date);
    }

    private static void AssertStringValue<T>(
        string expected,
        Func<string, Result<T>> factory,
        Func<T, string> getValue)
    {
        var result = factory(expected);

        result.IsSuccess.ShouldBeTrue();
        getValue(result.Value).ShouldBe(expected);
    }
}
