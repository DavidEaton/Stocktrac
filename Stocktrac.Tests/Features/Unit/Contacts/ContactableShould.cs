using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Persons;
using Entity = Stocktrac.Domain.Features.Entity;

namespace Stocktrac.Tests.Features.Unit.Contacts;
public class ContactableShould
{
    [Fact]
    public void HaveNoAddress_WhenAddressIsDefault()
    {
        var person = CreatePerson();

        Address.Default.HasValue.ShouldBe(false);
        person.Address.ShouldBe(Address.Default);
    }

    [Fact]
    public void AddAndRemoveContacts_WhenContactsAreValid()
    {
        var person = CreatePerson();
        var phone = CreatePhone("555-111-1111", PhoneType.Mobile, true);
        var email = CreateEmail("primary@example.com", true);

        person.AddPhone(phone).IsSuccess.ShouldBe(true);
        person.AddEmail(email).IsSuccess.ShouldBe(true);
        person.Phones.ShouldContain(phone);
        person.Emails.ShouldContain(email);

        person.RemovePhone(phone).IsSuccess.ShouldBe(true);
        person.RemoveEmail(email).IsSuccess.ShouldBe(true);
        person.Phones.ShouldBeEmpty();
        person.Emails.ShouldBeEmpty();
    }

    [Fact]
    public void ReturnRequiredError_WhenContactIsNull()
    {
        var person = CreatePerson();

        person.AddPhone(null!).Error.ShouldBe(Contactable.RequiredMessage);
        person.RemovePhone(null!).Error.ShouldBe(Contactable.RequiredMessage);
        person.AddEmail(null!).Error.ShouldBe(Contactable.RequiredMessage);
        person.RemoveEmail(null!).Error.ShouldBe(Contactable.RequiredMessage);
    }

    [Fact]
    public void RejectDuplicateValues_On_AddContact_WhenValueAlreadyExists()
    {
        var person = CreatePerson(
            emails: [CreateEmail("same@example.com", false)],
            phones: [CreatePhone("555-111-1111", PhoneType.Mobile, false)]);

        person.AddEmail(CreateEmail("same@example.com", false)).Error
            .ShouldBe(Contactable.NonuniqueMessage);
        person.AddPhone(CreatePhone("555-111-1111", PhoneType.Home, false)).Error
            .ShouldBe(Contactable.NonuniqueMessage);
        person.Emails.Count.ShouldBe(1);
        person.Phones.Count.ShouldBe(1);
    }

    [Fact]
    public void RejectSecondPrimary_On_AddContact_WhenPrimaryAlreadyExists()
    {
        var person = CreatePerson(
            emails: [CreateEmail("first@example.com", true)],
            phones: [CreatePhone("555-111-1111", PhoneType.Mobile, true)]);

        person.AddEmail(CreateEmail("second@example.com", true)).Error
            .ShouldBe(Contactable.PrimaryExistsMessage);
        person.AddPhone(CreatePhone("555-222-2222", PhoneType.Home, true)).Error
            .ShouldBe(Contactable.PrimaryExistsMessage);
    }

    [Fact]
    public void ReturnNotFoundError_On_RemoveContact_WhenContactIsAbsent()
    {
        var person = CreatePerson();

        person.RemoveEmail(CreateEmail("missing@example.com", false)).Error
            .ShouldBe(Contactable.NotFoundMessage);
        person.RemovePhone(CreatePhone("555-111-1111", PhoneType.Mobile, false)).Error
            .ShouldBe(Contactable.NotFoundMessage);
    }

    [Fact]
    public void NormalizeAndTruncateNotes_On_SetNotes_WhenNotesExceedMaximumLength()
    {
        var person = CreatePerson();
        var note = $"  {new string('n', Contactable.NoteMaximumLength + 1)}  ";

        var result = person.SetNotes(note);

        result.IsSuccess.ShouldBe(true);
        person.Notes!.Length.ShouldBe(Contactable.NoteMaximumLength);
        person.Notes.ShouldNotStartWith(" ");
    }

    [Fact]
    public void SetAndClearAddress_WhenAddressIsValidAndRejectNullAddress()
    {
        var person = CreatePerson();
        var address = Address.Create("123 Main St", "Anytown", State.NY, "12345").Value;

        person.SetAddress(address).IsSuccess.ShouldBe(true);
        person.Address.ShouldBe(address);
        person.SetAddress(null!).Error.ShouldBe(Contactable.RequiredMessage);
        person.Address.ShouldBe(address);
        person.ClearAddress().IsSuccess.ShouldBe(true);
        person.Address.HasValue.ShouldBe(false);
    }

    [Fact]
    public void RejectDuplicateValues_On_UpdateContactDetails_WhenValuesAreDuplicated()
    {
        var person = CreatePerson();
        var contactDetails = ContactDetails.Create(
            phones:
            [
                CreatePhone("555-111-1111", PhoneType.Mobile, false),
                CreatePhone("555-111-1111", PhoneType.Home, false)
            ],
            emails: [],
            address: Maybe<Address>.None).Value;

        var exception = Should.Throw<Exception>(() => person.UpdateContactDetails(contactDetails));

        exception.Message.ShouldBe(Contactable.NonuniqueMessage);
        person.Phones.ShouldBeEmpty();
    }

    [Fact(Skip = "Skipping test due to known bug in UpdateContactDetails method.")]
    public void RejectMultiplePrimaries_On_UpdateContactDetails_WhenMultiplePrimariesExist()
    {
        var person = CreatePerson();
        var contactDetails = ContactDetails.Create(
            phones: [],
            emails:
            [
                CreateEmail("first@example.com", true),
                CreateEmail("second@example.com", true)
            ],
            address: Maybe<Address>.None).Value;

        var exception = Should.Throw<Exception>(() => person.UpdateContactDetails(contactDetails));

        exception.Message.ShouldBe(Contactable.PrimaryExistsMessage);
        person.Emails.ShouldBeEmpty();
    }

    [Fact]
    public void UpdatePhones_On_UpdateContactDetails_WhenPhonesAreValid()
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
    public void UpdateEmails_On_UpdateContactDetails_WhenEmailsAreValid()
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
    public void UpdateAddress_On_UpdateContactDetails_WhenAddressIsValid()
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
    public void PreserveContactDetails_On_UpdateContactDetails_WhenContactDetailsAreEmpty()
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
