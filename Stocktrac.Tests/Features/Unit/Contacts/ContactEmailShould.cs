using Shouldly;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class ContactEmailShould
{
    private const string InvalidStringOverMaximumLength = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in"; // 256 characters
    private const string InvalidStringZeroLength = "";
    public static TheoryData<string> InvalidAddresses =>
    [
        InvalidStringZeroLength,
        new string('a', EmailAddress.MinimumLength - 1),
        new string('a', EmailAddress.MaximumLength + 1),
        "invalid-email-address.com"
    ];

    [Fact]
    public void ReturnContactEmail_On_Create_WhenAddressIsValid()
    {
        var address = "john@doe.com";
        var primary = true;

        var emailOrError = ContactEmail.Create(address, primary);

        emailOrError.Value.Address.Value.ShouldBe(address);
        emailOrError.Value.IsPrimary.ShouldBe(primary);
        emailOrError.IsFailure.ShouldBe(false);
    }

    [Fact]
    public void ExposeImmutableProperties()
    {
        typeof(ContactEmail).GetProperty(nameof(ContactEmail.Address))!.SetMethod.ShouldBeNull();
        typeof(ContactEmail).GetProperty(nameof(ContactEmail.IsPrimary))!.SetMethod.ShouldBeNull();
    }

    [Fact]
    public void TrimAddress_On_Create_WhenAddressContainsSurroundingWhitespace()
    {
        var result = ContactEmail.Create("  john@doe.com  ", true);

        result.IsSuccess.ShouldBe(true);
        result.Value.Address.Value.ShouldBe("john@doe.com");
        result.Value.ToString().ShouldBe("john@doe.com");
    }

    [Theory]
    [InlineData("a@b", "Email address cannot be less than 5 character(s) in length.")]
    [InlineData("invalid-email-address.com", "Email address and/or its format is invalid.")]
    [InlineData("", "Email address cannot be empty.")]
    [InlineData("lorem ipsum dolor sit amet consectetur adipiscing elit non qui ad dolores cillum non nam qui est in est dolorum laborum vel imperdiet cupiditate sit facilis minim consequat est do et dolor lorem nulla pariatur id vero est velit est dolorem laborum aut tempor", "Email address cannot be greater than 254 characters in length.")]
    public void ReturnSpecificError_On_Create_WhenAddressIsInvalid(string address, string expectedError)
    {
        var result = ContactEmail.Create(address, true);

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(expectedError,
            StringCompareShould.IgnoreCase |
            StringCompareShould.IgnoreLineEndings);
    }

    [Fact]
    public void ReturnMaximumLengthError_On_Create_WhenAddressIsOversized()
    {
        var result = ContactEmail.Create($"{new string('a', EmailAddress.MaximumLength)}@x.com", true);

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(EmailAddress.MaximumLengthMessage);
    }

    [Fact]
    public void ReturnFailureResult_On_Create_WhenAddressIsNull()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var result = ContactEmail.Create(
            address: null,
            isPrimary: true);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(EmailAddress.EmptyMessage);
    }

    [Fact]
    public void ReturnFailureResult_On_Create_WhenAddressIsEmpty()
    {
        var result = ContactEmail.Create(
            address: string.Empty,
            isPrimary: true);

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(EmailAddress.EmptyMessage);
    }

    [Theory]
    [MemberData(nameof(InvalidAddresses))]
    public void ReturnFailureResult_On_Create_WhenAddressIsInvalid(string address)
    {
        var result = ContactEmail.Create(address, true);

        result.IsFailure.ShouldBe(true);
    }

    [Fact]
    public void EquateDistinctInstances_WhenValuesAreTheSame()
    {
        var primaryAddress = "john@doe.com";
        var primaryEmail = ContactEmail.Create(
            address: primaryAddress,
            isPrimary: true).Value;

        var secondaryAddress = primaryAddress;
        var secondaryEmail = ContactEmail.Create(
            address: secondaryAddress,
            isPrimary: true).Value;

        primaryEmail.ShouldBe(secondaryEmail);
        primaryEmail.ShouldNotBeSameAs(secondaryEmail);
    }

    [Fact]
    public void HaveDifferingProperties_WhenValuesDiffer()
    {
        var primaryAddress = "john@doe.com";
        var primaryEmail = ContactEmail.Create(
            address: primaryAddress,
            isPrimary: true).Value;

        var secondaryAddress = "jane@doe.com";
        var secondaryEmail = ContactEmail.Create(
            address: secondaryAddress,
            isPrimary: false).Value;

        primaryEmail.Address.ShouldNotBe(secondaryEmail.Address);
        primaryEmail.IsPrimary.ShouldNotBe(secondaryEmail.IsPrimary);
    }

    [Fact]
    public void ReturnFailureResult_On_WithAddress_WhenAddressIsNull()
    {
        var email = Create_Valid_Primary_Email();

        var result = email.WithAddress(null!);

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(EmailAddress.EmptyMessage);
    }

    [Fact]
    public void ReturnUpdatedCopy_On_WithAddress_WhenAddressIsValid()
    {
        var email = Create_Valid_Primary_Email();
        var updatedAddress = "updated@address.com";

        var result = email.WithAddress(updatedAddress);

        result.IsSuccess.ShouldBe(true);
        result.Value.Address.Value.ShouldBe(updatedAddress);
        result.Value.IsPrimary.ShouldBe(email.IsPrimary);
        result.Value.ShouldNotBeSameAs(email);
        email.Address.Value.ShouldBe("email@email.com");
    }

    [Fact]
    public void TrimAddress_On_WithAddress_WhenAddressContainsSurroundingWhitespace()
    {
        var email = Create_Valid_Primary_Email();

        var result = email.WithAddress("  updated@address.com  ");

        result.IsSuccess.ShouldBe(true);
        result.Value.Address.Value.ShouldBe("updated@address.com");
        email.Address.Value.ShouldBe("email@email.com");
    }

    [Theory]
    [MemberData(nameof(InvalidAddresses))]
    public void PreserveAddress_On_WithAddress_WhenAddressIsInvalid(string address)
    {
        var email = Create_Valid_Primary_Email();
        var originalAddress = email.Address;

        var result = email.WithAddress(address);

        result.IsFailure.ShouldBe(true);
        email.Address.ShouldBe(originalAddress);
    }

    [Theory]
    [InlineData("", EmailAddress.EmptyMessage)]
    [InlineData("a@b", "Email address cannot be less than 5 character(s) in length.")]
    [InlineData("invalid-email-address.com", EmailAddress.InvalidMessage)]
    public void ReturnSpecificError_On_WithAddress_WhenAddressIsInvalid(string address, string expectedError)
    {
        var email = Create_Valid_Primary_Email();

        var result = email.WithAddress(address);

        result.Error.ShouldBe(expectedError);
        email.Address.Value.ShouldBe("email@email.com");
    }

    [Fact]
    public void ReturnUpdatedCopy_On_WithIsPrimary_WhenValueChanges()
    {
        var email = Create_Valid_Primary_Email();

        email.IsPrimary.ShouldBe(true);
        var updated = email.WithIsPrimary(false);

        updated.IsPrimary.ShouldBe(false);
        updated.Address.ShouldBe(email.Address);
        updated.ShouldNotBeSameAs(email);
        email.IsPrimary.ShouldBe(true);
    }

    [Fact]
    public void ReturnEquivalentDistinctCopy_On_WithIsPrimary_WhenValueDoesNotChange()
    {
        var email = Create_Valid_Primary_Email();

        var updated = email.WithIsPrimary(true);

        updated.ShouldBe(email);
        updated.GetHashCode().ShouldBe(email.GetHashCode());
        updated.ShouldNotBeSameAs(email);
    }

    [Fact]
    public void NotEquateInstances_WhenOnlyPrimaryStatusDiffers()
    {
        var primary = Create_Valid_Primary_Email();
        var secondary = ContactEmail.Create(primary.Address.Value, false).Value;

        primary.ShouldNotBe(secondary);
    }

    internal static ContactEmail Create_Valid_Primary_Email()
    {
        return ContactEmail.Create(
            address: "email@email.com",
            isPrimary: true).Value;
    }
}
