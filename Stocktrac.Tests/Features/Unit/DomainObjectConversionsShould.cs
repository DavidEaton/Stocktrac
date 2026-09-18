using Shouldly;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Customers;
using Stocktrac.Domain.Features.Financial;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Tests.Features.Unit;

public class DomainObjectConversionsShould
{
    [Fact]
    public void ConvertStringBackedValuesToAndFromStrings()
    {
        AssertStringConversion("123 Main Street", value => (AddressLine)value, value => value);
        AssertStringConversion("Acme Automotive", value => (BusinessName)value, value => value);
        AssertStringConversion("Springfield", value => (City)value, value => value);
        AssertStringConversion("D123456", value => (DriversLicenseNumber)value, value => value);
        AssertStringConversion("owner@example.com", value => (EmailAddress)value, value => value);
        AssertStringConversion("Remember this", value => (Note)value, value => value);
        AssertStringConversion("15551234567", value => (PhoneNumber)value, value => value);
        AssertStringConversion("90210", value => (PostalCode)value, value => value);
        AssertStringConversion("CUST-100", value => (CustomerCode)value, value => value);
        AssertStringConversion("Visa", value => (CreditCardName)value, value => value);
        AssertStringConversion("CAD", value => (CurrencyCode)value, value => value);
        AssertStringConversion("123456789", value => (SSN)value, value => value);
    }

    [Fact]
    public void ConvertAmountToAndFromDecimal()
    {
        var amount = (Amount)123.45m;
        decimal value = amount;

        value.ShouldBe(123.45m);
    }

    [Fact]
    public void ConvertBirthdayToAndFromDateTime()
    {
        var date = new DateTime(1990, 6, 15);
        var birthday = (Birthday)date;
        DateTime value = birthday;

        value.ShouldBe(date);
    }

    private static void AssertStringConversion<T>(
        string expected,
        Func<string, T> fromString,
        Func<T, string> toString)
    {
        var domainValue = fromString(expected);
        string value = toString(domainValue);

        value.ShouldBe(expected);
    }
}
