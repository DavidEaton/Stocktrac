using Shouldly;
using Stocktrac.Domain.Features.Contact;

namespace Stocktrac.Tests.Unit.Features;

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
    public void Create_Email()
    {
        var address = "john@doe.com";
        var primary = true;

        var emailOrError = Email.Create(address, primary);

        emailOrError.Value.Address.ShouldBe(address);
        emailOrError.Value.IsPrimary.ShouldBe(primary);
        emailOrError.IsFailure.ShouldBe(false);
    }

    [Fact]
    public void Return_Failure_Result_With_Null_Address()
    {
        var result = Email.Create(
            address: null,
            isPrimary: true);

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(Email.EmptyMessage);
    }

    [Fact]
    public void Return_Failure_Result_With_Empty_Address()
    {
        var result = Email.Create(
            address: string.Empty,
            isPrimary: true);

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(Email.EmptyMessage);
    }

    [Theory]
    [MemberData(nameof(InvalidAddresses))]
    public void Return_Failure_Result_On_Create_With_Invalid_Address(string address)
    {
        var result = Email.Create(address, true);

        result.IsFailure.ShouldBe(true);
    }

    [Fact]
    public void Not_Equate_Email_Instances_Having_Same_Values()
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
    public void Not_Equate_Email_Instances_Having_Differing_Values()
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
    public void Return_Failure_Result_On_SetAddress_With_Null_Address()
    {
        var email = Create_Valid_Primary_Email();

        var result = email.SetAddress(null);

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(Email.EmptyMessage);
    }

    [Fact]
    public void Return_Success_And_Update_Email_On_SetAddress()
    {
        var email = Create_Valid_Primary_Email();
        var updatedAddress = "updated@address.com";

        var result = email.SetAddress(updatedAddress);

        result.IsSuccess.ShouldBe(true);
        result.Value.ShouldBe(updatedAddress);
        email.Address.ShouldBe(updatedAddress);
    }

    [Theory]
    [MemberData(nameof(InvalidAddresses))]
    public void Not_SetAddress_With_Invalid_Parameter(string address)
    {
        var email = Create_Valid_Primary_Email();

        var result = email.SetAddress(address);

        result.IsFailure.ShouldBe(true);
        email.Address.ShouldNotBe(address);
    }

    [Fact]
    public void Update_Email_On_SetIsPrimary()
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