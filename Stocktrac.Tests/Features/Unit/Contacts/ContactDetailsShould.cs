using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class ContactDetailsShould
{
    [Fact]
    public void NormalizeNullCollectionsAndAddress_On_Create_WhenArgumentsAreNull()
    {
        var result = ContactDetails.Create(null, null, null);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Phones.ShouldBeEmpty();
        result.Value.Emails.ShouldBeEmpty();
        result.Value.Address.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void PreserveContactsAndAddress_On_Create_WhenArgumentsAreValid()
    {
        var phone = Phone.Create("555-123-4567", PhoneType.Mobile, true).Value;
        var email = Email.Create("person@example.com", true).Value;
        var address = Address.Create(
            AddressLine.Create("123 Main").Value,
            City.Create("City").Value,
            State.AL,
            PostalCode.Create("12345").Value).Value;

        var details = ContactDetails.Create([phone], [email], Maybe<Address>.From(address)).Value;

        details.Phones.ShouldBe([phone]);
        details.Emails.ShouldBe([email]);
        details.Address.Value.ShouldBe(address);
    }

    [Fact]
    public void SnapshotInputCollections_On_Create_WhenSourcesAreLaterChanged()
    {
        var phones = new List<Phone> { Phone.Create("555-123-4567", PhoneType.Mobile, false).Value };
        var emails = new List<Email> { Email.Create("person@example.com", false).Value };
        var details = ContactDetails.Create(phones, emails, Maybe<Address>.None).Value;

        phones.Clear();
        emails.Clear();

        details.Phones.Count.ShouldBe(1);
        details.Emails.Count.ShouldBe(1);
    }

    [Fact]
    public void ReturnRequiredError_On_Create_WhenAnyContactIsNull()
    {
        ContactDetails.Create([null!], [], Maybe<Address>.None).Error.ShouldBe(Contactable.RequiredMessage);
        ContactDetails.Create([], [null!], Maybe<Address>.None).Error.ShouldBe(Contactable.RequiredMessage);
    }

    [Fact]
    public void ReturnPrimaryExistsError_On_Create_WhenMultiplePrimaryPhonesExist()
    {
        var result = ContactDetails.Create(
            [PhoneOf("555-111-1111", true), PhoneOf("555-222-2222", true)], [], Maybe<Address>.None);

        result.Error.ShouldBe(Contactable.PrimaryExistsMessage);
    }

    [Fact]
    public void ReturnPrimaryExistsError_On_Create_WhenMultiplePrimaryEmailsExist()
    {
        var result = ContactDetails.Create([], [EmailOf("one@example.com", true), EmailOf("two@example.com", true)],
            Maybe<Address>.None);

        result.Error.ShouldBe(Contactable.PrimaryExistsMessage);
    }

    [Fact]
    public void ReturnNonuniqueError_On_Create_WhenPhoneNumbersAreDuplicated()
    {
        var result = ContactDetails.Create(
            [PhoneOf("555-111-1111", false), PhoneOf("555-111-1111", false)], [], Maybe<Address>.None);

        result.Error.ShouldBe(Contactable.NonuniqueMessage);
    }

    [Fact]
    public void ReturnNonuniqueError_On_Create_WhenEmailAddressesAreDuplicated()
    {
        var result = ContactDetails.Create([], [EmailOf("same@example.com", false), EmailOf("same@example.com", false)],
            Maybe<Address>.None);

        result.Error.ShouldBe(Contactable.NonuniqueMessage);
    }

    [Fact]
    public void AllowSameContactValueAcrossDifferentContactKinds_On_Create()
    {
        var result = ContactDetails.Create([PhoneOf("5551234", false)], [EmailOf("5551234@example.com", false)],
            Maybe<Address>.None);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void BeEqualAndHaveMatchingHashCodes_WhenOrderedComponentsAreSameInstances()
    {
        var phone = PhoneOf("555-111-1111", false);
        var email = EmailOf("same@example.com", false);
        var first = ContactDetails.Create([phone], [email], Maybe<Address>.None).Value;
        var second = ContactDetails.Create([phone], [email], Maybe<Address>.None).Value;

        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.ShouldNotBe(ContactDetails.Create([], [email], Maybe<Address>.None).Value);
    }

    private static Phone PhoneOf(string number, bool primary) => Phone.Create(number, PhoneType.Mobile, primary).Value;
    private static Email EmailOf(string address, bool primary) => Email.Create(address, primary).Value;
}
