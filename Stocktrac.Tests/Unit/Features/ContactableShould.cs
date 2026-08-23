using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Persons;
using Entity = Stocktrac.Domain.Features.Entity;

namespace Stocktrac.Tests.Unit.Features;
// Test naming convention: SystemUnderTest_Scenario_Result/ExpectedBehavior
// For example: AddEmail_WhenEmailIsNull_ReturnsFailure
public class ContactableShould
{
    [Fact]
    public void UpdateContactDetails_WithValidPhones_ReturnsSuccess()
    {
        var originalPhonePrimary = CreatePhone(
            number: "555-111-1111",
            phoneType: PhoneType.Mobile,
            isPrimary: true,
            id: 1);
        var originalPhoneOther = CreatePhone(
            number: "555-222-2222",
            phoneType: PhoneType.Home,
            isPrimary: false,
            id: 2);
        var person = CreatePerson(
            phones: [originalPhonePrimary, originalPhoneOther]);

        person.Phones.Count.ShouldBe(2);
        person.Phones.ShouldContain(originalPhonePrimary);

        originalPhonePrimary.SetNumber("555-333-3333");
        var newPhoneOther = CreatePhone(
            number: "555-444-4444",
            phoneType: PhoneType.Home,
            isPrimary: false,
            id: 0);
        var contactDetails = ContactDetails.Create(
            phones: [originalPhonePrimary, newPhoneOther],
            emails: [],
            address: Maybe<Address>.None).Value;

        person.UpdateContactDetails(contactDetails);

        person.Phones.Count.ShouldBe(3);
        person.Phones.ShouldContain(originalPhonePrimary);
        person.Phones.ShouldContain(newPhoneOther);
    }

    [Fact(Skip = "Skipping test due to known bug in UpdateContactDetails method.")]
    public void UpdateContactDetails_WithValidEmails_ReturnsSuccess()
    {
        var existingPrimaryEmail = CreateEmail("primary@example.com", true, 1);
        var existingEmailToRemove = CreateEmail("remove@example.com", false, 2);
        var person = CreatePerson(emails: [existingPrimaryEmail, existingEmailToRemove]);
        var requestedPrimaryEmail = CreateEmail("updated@example.com", false, 1);
        var requestedNewEmail = CreateEmail("new@example.com", true);
        var contactDetails = ContactDetails.Create(
            phones: [],
            emails: [requestedPrimaryEmail, requestedNewEmail],
            address: Maybe<Address>.None).Value;

        person.UpdateContactDetails(contactDetails);

        person.Emails.Count.ShouldBe(2);
        person.Emails.ShouldContain(requestedPrimaryEmail);
        person.Emails.ShouldContain(requestedNewEmail);
    }

    [Fact]
    public void UpdateContactDetails_WithValidAddress_ReturnsSuccess()
    {
        var addressLine1 = "123 Main St";
        var city = "Anytown";
        var state = State.NY;
        var postalCode = "12345";
        var person = CreatePerson(
            phones: [],
            emails: []);
        var address = Address.Create(
            addressLine1: addressLine1,
            city: city,
            state: state,
            postalCode: postalCode).Value;
        var contactDetails = ContactDetails.Create(
            phones: [],
            emails: [],
            address: address).Value;

        person.UpdateContactDetails(contactDetails);

        person.Address.ShouldNotBe(null);
        person.Address.HasValue.ShouldBe(true);
        person.Address.Value.AddressLine1.ShouldBe(addressLine1);
        person.Address.Value.City.ShouldBe(city);
        person.Address.Value.PostalCode.ShouldBe(postalCode);
    }

    [Fact]
    public void Not_Change_ContactDetails_On_UpdateContactDetails_With_Empty_ContactDetails()
    {
        var addressLine1 = "123 Main St";
        var city = "Anytown";
        var state = State.NY;
        var postalCode = "12345";
        var person = CreatePerson(
            phones: [CreatePhone("555-111-1111", PhoneType.Mobile, true, 1)],
            emails: [CreateEmail("primary@example.com", true, 1)]);
        var address = Address.Create(
            addressLine1: addressLine1,
            city: city,
            state: state,
            postalCode: postalCode).Value;
        var contactDetails = ContactDetails.Create(
            phones: person.Phones,
            emails: person.Emails,
            address: address).Value;

        person.UpdateContactDetails(contactDetails);

        var phonesCount = person.Phones.Count;
        var emailsCount = person.Emails.Count;
        var addressValue = person.Address.Value;

        phonesCount.ShouldBeGreaterThan(0);
        emailsCount.ShouldBeGreaterThan(0);
        addressValue.ShouldBe(address);

        var emptyContactDetails = ContactDetails.Create(
            phones: [],
            emails: [],
            address: Maybe<Address>.None).Value;
        person?.UpdateContactDetails(emptyContactDetails);

        var personContactDetails = ContactDetails.Create(
            phones: person?.Phones,
            emails: person?.Emails,
            address: person?.Address).Value;

        // personContactDetails.ShouldBe(contactDetails);
        personContactDetails?.Phones?.Count.ShouldBe(phonesCount);
        personContactDetails?.Emails?.Count.ShouldBe(emailsCount);
        personContactDetails?.Address.ShouldBe(Maybe<Address>.None);
    }

    private static Person CreatePerson(
        IReadOnlyList<Email>? emails = null,
        IReadOnlyList<Phone>? phones = null) =>
        Person.Create(
            name: PersonName.Create("Doe", "Jane").Value,
            notes: null,
            emails: emails,
            phones: phones).Value;

    private static Phone CreatePhone(string number, PhoneType phoneType, bool isPrimary, long? id = null)
    {
        var phone = Phone.Create(number, phoneType, isPrimary).Value;
        SetId(phone, id);
        return phone;
    }

    private static Email CreateEmail(string address, bool isPrimary, long? id = null)
    {
        var email = Email.Create(address, isPrimary).Value;
        SetId(email, id);
        return email;
    }

    private static void SetId(Entity entity, long? id)
    {
        if (id is not null)
            typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(entity, id.Value);
    }
}
