using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Customers;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Tests.Features.Unit.Customers;

public class BusinessShould
{
    [Theory]
    [InlineData("Phones")]
    [InlineData("Emails")]
    public void ReturnRequiredError_On_Create_WhenContactCollectionIsNull(string collection)
    {
        var result = Business.Create(
            BusinessName.Create(NonEmptyString.Create("Acme").Value).Value,
            Maybe<Address>.None,
            Note.Create(NonEmptyString.Create("Notes").Value).Value,
            Maybe<Person>.None,
            collection == "Emails" ? null! : [],
            collection == "Phones" ? null! : []);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Contactable.RequiredMessage);
    }

    [Fact]
    public void ReturnDuplicateError_On_Create_WhenPhoneNumbersRepeat()
    {
        var number = PhoneNumber.Create(NonEmptyString.Create("5551234567").Value).Value;
        var first = ContactPhone.Create(number, PhoneType.Home, false).Value;
        var second = ContactPhone.Create(number, PhoneType.Mobile, false).Value;

        var result = Business.Create(
            BusinessName.Create(NonEmptyString.Create("Acme").Value).Value,
            Maybe<Address>.None,
            Note.Create(NonEmptyString.Create("Notes").Value).Value,
            Maybe<Person>.None, [], [first, second]);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Contactable.NonuniqueMessage);
    }

    [Fact]
    public void ReturnPrimaryCardinalityError_On_Create_WhenEmailsHaveMultiplePrimaries()
    {
        var first = ContactEmail.Create(
            EmailAddress.Create(NonEmptyString.Create("first@example.com").Value).Value, true).Value;
        var second = ContactEmail.Create(
            EmailAddress.Create(NonEmptyString.Create("second@example.com").Value).Value, true).Value;

        var result = Business.Create(
            BusinessName.Create(NonEmptyString.Create("Acme").Value).Value,
            Maybe<Address>.None,
            Note.Create(NonEmptyString.Create("Notes").Value).Value,
            Maybe<Person>.None, [first, second], []);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Contactable.MultiplePrimariesMessage);
    }

    [Fact]
    public void ReplaceContact_On_UpdateContact_WhenContactIsValid()
    {
        var business = CreateBusiness();
        var contact = CreatePerson();

        var result = business.UpdateContact(contact);

        result.IsSuccess.ShouldBeTrue();
        business.Contact.Value.ShouldBe(contact);
    }

    [Fact]
    public void RemoveContact_On_RemoveContact_WhenContactExists()
    {
        var business = CreateBusiness(CreatePerson());

        business.RemoveContact();

        business.Contact.HasNoValue.ShouldBeTrue();
    }

    private static Business CreateBusiness(Maybe<Person> contact = default) =>
        Business.Create(
            BusinessName.Create(NonEmptyString.Create("Acme Repair").Value).Value,
            Maybe<Address>.None,
            Note.Create(NonEmptyString.Create("No notes").Value).Value,
            contact,
            [],
            []).Value;

    private static Person CreatePerson() =>
        Person.Create(
            PersonName.Create(
                NonEmptyString.Create("Doe").Value,
                NonEmptyString.Create("Jane").Value).Value,
            Note.Create(NonEmptyString.Create("No notes").Value).Value,
            [],
            [],
            Maybe<Birthday>.None,
            Maybe<DriversLicense>.None,
            Maybe<Address>.None).Value;
}
