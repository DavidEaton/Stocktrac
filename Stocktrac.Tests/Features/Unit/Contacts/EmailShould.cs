using Shouldly;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class EmailShould
{
    private const string InvalidStringOverMaximumLength = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in"; // 256 characters
    private const string InvalidStringZeroLength = "";
    public static TheoryData<string> InvalidAddresses =>
    [
        InvalidStringZeroLength,
        new string('a', Email.MinimumLength - 1),
        new string('a', Email.MaximumLength + 1),
        "invalid-email-address.com"
    ];

    [Fact]
    public void ReturnEmail_On_Create_WhenAddressIsValid()
    {
        var address = "john@doe.com";
        var primary = true;

        var emailOrError = Email.Create(address, primary);

        emailOrError.Value.Address.ShouldBe(address);
        emailOrError.Value.IsPrimary.ShouldBe(primary);
        emailOrError.IsFailure.ShouldBe(false);
    }

    [Fact]
    public void TrimAddress_On_Create_WhenAddressContainsSurroundingWhitespace()
    {
        var result = Email.Create("  john@doe.com  ", true);

        result.IsSuccess.ShouldBe(true);
        result.Value.Address.ShouldBe("john@doe.com");
        result.Value.ToString().ShouldBe("john@doe.com");
    }

    [Theory]
    [InlineData("a@b", "Email address cannot be less than 5 character(s) in length.")]
    [InlineData("invalid-email-address.com", "Email address and/or its format is invalid")]
    [InlineData("", "Email address cannot be empty.")]
    [InlineData("lorem ipsum dolor sit amet consectetur adipiscing elit non qui ad dolores cillum non nam qui est in est dolorum laborum vel imperdiet cupiditate sit facilis minim consequat est do et dolor lorem nulla pariatur id vero est velit est dolorem laborum aut tempor", "Email address cannot be greater than 254 characters in length.")]
    public void ReturnSpecificError_On_Create_WhenAddressIsInvalid(string address, string expectedError)
    {
        var result = Email.Create(address, true);

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(expectedError);
    }

    [Fact]
    public void ReturnMaximumLengthError_On_Create_WhenAddressIsOversized()
    {
        var result = Email.Create($"{new string('a', Email.MaximumLength)}@x.com", true);

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(Email.MaximumLengthMessage);
    }

    [Fact]
    public void ReturnFailureResult_On_Create_WhenAddressIsNull()
    {
        var result = Email.Create(
            address: null!,
            isPrimary: true);

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(Email.EmptyMessage);
    }

    [Fact]
    public void ReturnFailureResult_On_Create_WhenAddressIsEmpty()
    {
        var result = Email.Create(
            address: string.Empty,
            isPrimary: true);

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(Email.EmptyMessage);
    }

    [Theory]
    [MemberData(nameof(InvalidAddresses))]
    public void ReturnFailureResult_On_Create_WhenAddressIsInvalid(string address)
    {
        var result = Email.Create(address, true);

        result.IsFailure.ShouldBe(true);
    }

    [Fact]
    public void NotEquateDistinctInstances_WhenValuesAreTheSame()
    {
        var primaryAddress = "john@doe.com";
        var primaryEmail = Email.Create(
            address: primaryAddress,
            isPrimary: true).Value;

        var secondaryAddress = primaryAddress;
        var secondaryEmail = Email.Create(
            address: secondaryAddress,
            isPrimary: true).Value;

        primaryEmail.ShouldNotBe(secondaryEmail);
    }

    [Fact]
    public void HaveDifferingProperties_WhenValuesDiffer()
    {
        var primaryAddress = "john@doe.com";
        var primaryEmail = Email.Create(
            address: primaryAddress,
            isPrimary: true).Value;

        var secondaryAddress = "jane@doe.com";
        var secondaryEmail = Email.Create(
            address: secondaryAddress,
            isPrimary: false).Value;

        primaryEmail.Address.ShouldNotBe(secondaryEmail.Address);
        primaryEmail.IsPrimary.ShouldNotBe(secondaryEmail.IsPrimary);
    }

    [Fact]
    public void ReturnFailureResult_On_SetAddress_WhenAddressIsNull()
    {
        var email = Create_Valid_Primary_Email();

        var result = email.SetAddress(null!);

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(Email.EmptyMessage);
    }

    [Fact]
    public void ReturnSuccessAndUpdateAddress_On_SetAddress_WhenAddressIsValid()
    {
        var email = Create_Valid_Primary_Email();
        var updatedAddress = "updated@address.com";

        var result = email.SetAddress(updatedAddress);

        result.IsSuccess.ShouldBe(true);
        result.Value.ShouldBe(updatedAddress);
        email.Address.ShouldBe(updatedAddress);
    }

    [Fact]
    public void TrimAddress_On_SetAddress_WhenAddressContainsSurroundingWhitespace()
    {
        var email = Create_Valid_Primary_Email();

        var result = email.SetAddress("  updated@address.com  ");

        result.IsSuccess.ShouldBe(true);
        result.Value.ShouldBe("updated@address.com");
        email.Address.ShouldBe("updated@address.com");
    }

    [Theory]
    [MemberData(nameof(InvalidAddresses))]
    public void PreserveAddress_On_SetAddress_WhenAddressIsInvalid(string address)
    {
        var email = Create_Valid_Primary_Email();
        var originalAddress = email.Address;

        var result = email.SetAddress(address);

        result.IsFailure.ShouldBe(true);
        email.Address.ShouldBe(originalAddress);
    }

    [Fact]
    public void UpdatePrimaryStatus_On_SetIsPrimary_WhenValueChanges()
    {
        var email = Create_Valid_Primary_Email();

        email.IsPrimary.ShouldBe(true);
        email.SetIsPrimary(false);

        email.IsPrimary.ShouldBe(false);
    }

    internal static Email Create_Valid_Primary_Email()
    {
        return Email.Create(
            address: "email@email.com",
            isPrimary: true).Value;
    }
}
