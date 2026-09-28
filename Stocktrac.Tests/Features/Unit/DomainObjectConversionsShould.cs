using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features;
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
        AssertStringValue(
            "Acme Automotive",
            input => NonEmptyString.Create(input).Bind(BusinessName.Create),
            value => value.Name.Value);

        AssertStringValue(
            "D123456",
            input => NonEmptyString.Create(input).Bind(DriversLicenseNumber.Create),
            value => value.Number.Value);

        AssertStringValue(
            "123 Main Street",
            input => NonEmptyString.Create(input).Bind(AddressLine.Create),
            value => value.Value.ToString());

        AssertStringValue(
            "Springfield",
            input => NonEmptyString.Create(input).Bind(City.Create),
            value => value.Value.ToString());

        AssertStringValue(
            "90210",
            input => NonEmptyString.Create(input).Bind(PostalCode.Create),
            value => value.Value.ToString());

        AssertStringValue("owner@example.com", EmailAddress.Create, value => value.Value);
        AssertStringValue("Remember this", Note.Create, value => value.Value);
        AssertStringValue("15551234567", PhoneNumber.Create, value => value.Value);
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
    public void CreateBirthdayFromDateOnlyAndConvertBack()
    {
        var date = new DateOnly(1990, 6, 15);

        var result = Birthday.Create(date);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(date);
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
