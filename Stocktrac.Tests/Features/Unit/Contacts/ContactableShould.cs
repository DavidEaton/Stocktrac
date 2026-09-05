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
        person.Notes.Value.Length.ShouldBe(Contactable.NoteMaximumLength);
        person.Notes.Value.ShouldNotStartWith(" ");
    }

    [Fact]
    public void SetAndClearAddress_WhenAddressIsValidAndRejectNullAddress()
    {
        var person = CreatePerson();
        var address = CreateAddress("123 Main St", "Anytown", State.NY, "12345");

        person.SetAddress(address).IsSuccess.ShouldBe(true);
        person.Address.ShouldBe(address);
        person.Address.ShouldBe(address);
        person.ClearAddress().IsSuccess.ShouldBe(true);
        person.Address.HasValue.ShouldBe(false);
    }

    [Fact]
    public void ReplacePhones_WhenRequestedCollectionIsValid()
    {
        var person = CreatePerson(
            phones: [CreatePhone("555-111-1111", PhoneType.Mobile, true)]);
        var replacements = new[]
        {
            CreatePhone("555-222-2222", PhoneType.Home, true),
            CreatePhone("555-333-3333", PhoneType.Work, false)
        };

        var result = person.ReplacePhones(replacements);

        result.IsSuccess.ShouldBeTrue();
        person.Phones.ShouldBe(replacements);
    }

    [Fact]
    public void ReplaceEmails_WhenRequestedCollectionIsValid()
    {
        var person = CreatePerson(
            emails: [CreateEmail("old@example.com", true)]);
        var replacements = new[]
        {
            CreateEmail("primary@example.com", true),
            CreateEmail("other@example.com", false)
        };

        var result = person.ReplaceEmails(replacements);

        result.IsSuccess.ShouldBeTrue();
        person.Emails.ShouldBe(replacements);
    }

    [Fact]
    public void ClearContacts_WhenRequestedCollectionsAreEmpty()
    {
        var person = CreatePerson(
            phones: [CreatePhone("555-111-1111", PhoneType.Mobile, true)],
            emails: [CreateEmail("person@example.com", true)]);

        person.ReplacePhones([]).IsSuccess.ShouldBeTrue();
        person.ReplaceEmails([]).IsSuccess.ShouldBeTrue();

        person.Phones.ShouldBeEmpty();
        person.Emails.ShouldBeEmpty();
    }

    [Fact]
    public void PreservePhones_WhenReplacementContainsDuplicateNumbers()
    {
        var original = CreatePhone("555-111-1111", PhoneType.Mobile, true);
        var person = CreatePerson(phones: [original]);

        var result = person.ReplacePhones(
        [
            CreatePhone("555-222-2222", PhoneType.Mobile, false),
            CreatePhone("555-222-2222", PhoneType.Home, false)
        ]);

        result.Error.ShouldBe(Contactable.NonuniqueMessage);
        person.Phones.ShouldBe([original]);
    }

    [Fact]
    public void PreserveEmails_WhenReplacementContainsMultiplePrimaries()
    {
        var original = CreateEmail("original@example.com", true);
        var person = CreatePerson(emails: [original]);

        var result = person.ReplaceEmails(
        [
            CreateEmail("first@example.com", true),
            CreateEmail("second@example.com", true)
        ]);

        result.Error.ShouldBe(Contactable.PrimaryExistsMessage);
        person.Emails.ShouldBe([original]);
    }

    [Fact]
    public void RejectNullReplacementCollections()
    {
        var person = CreatePerson();

        person.ReplacePhones(null!).Error.ShouldBe(Contactable.RequiredMessage);
        person.ReplaceEmails(null!).Error.ShouldBe(Contactable.RequiredMessage);
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

    private static Address CreateAddress(string line, string city, State state, string postalCode) =>
        Address.Create(
            AddressLine.Create(line).Value,
            City.Create(city).Value,
            state,
            PostalCode.Create(postalCode).Value,
            Maybe<AddressLine>.None).Value;

    private static void SetId(Entity entity, long? id)
    {
        if (id is not null)
            typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(entity, id.Value);
    }
}
