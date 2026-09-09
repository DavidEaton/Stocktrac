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
        var person = CreatePerson(emails: [], phones: []);

        Address.Default.HasValue.ShouldBe(false);
        person.Address.ShouldBe(Address.Default);
    }

    [Fact]
    public void AddAndRemoveContacts_WhenContactsAreValid()
    {
        var person = CreatePerson(emails: [], phones: []);
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
    public void RemoveContacts_ByTheirIdentifyingValues()
    {
        var person = CreatePerson(
            phones: [CreatePhone("555-111-1111", PhoneType.Mobile, false)],
            emails: [CreateEmail("person@example.com", false)]);

        var phoneWithDifferentDetails = CreatePhone(
            "555-111-1111",
            PhoneType.Home,
            true);
        var emailWithDifferentDetails = CreateEmail("person@example.com", true);

        person.RemovePhone(phoneWithDifferentDetails).IsSuccess.ShouldBeTrue();
        person.RemoveEmail(emailWithDifferentDetails).IsSuccess.ShouldBeTrue();
        person.Phones.ShouldBeEmpty();
        person.Emails.ShouldBeEmpty();
    }

    [Fact]
    public void IdentifyContacts_ByPhoneNumberAndEmailAddress()
    {
        var person = CreatePerson(
            phones: [CreatePhone("555-111-1111", PhoneType.Mobile, false)],
            emails: [CreateEmail("person@example.com", false)]);

        person.HasPhoneNumber("555-111-1111").ShouldBeTrue();
        person.HasPhoneNumber("555-222-2222").ShouldBeFalse();
        person.HasEmailAddress("person@example.com").ShouldBeTrue();
        person.HasEmailAddress("other@example.com").ShouldBeFalse();
    }

    [Fact]
    public void Throw_WhenContactIsNull()
    {
        var person = CreatePerson(emails: [], phones: []);

        Should.Throw<NullReferenceException>(() => person.AddPhone(null!));
        Should.Throw<NullReferenceException>(() => person.RemovePhone(null!));
        Should.Throw<NullReferenceException>(() => person.AddEmail(null!));
        Should.Throw<NullReferenceException>(() => person.RemoveEmail(null!));
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
        var person = CreatePerson(emails: [], phones: []);

        person.RemoveEmail(CreateEmail("missing@example.com", false)).Error
            .ShouldBe(Contactable.NotFoundMessage);
        person.RemovePhone(CreatePhone("555-111-1111", PhoneType.Mobile, false)).Error
            .ShouldBe(Contactable.NotFoundMessage);
    }

    [Fact]
    public void SetAndClearAddress_WhenAddressIsValid()
    {
        var person = CreatePerson(emails: [], phones: []);
        var address = CreateAddress("123 Main St", "Anytown", State.NY, "12345");

        person.SetAddress(address);
        person.Address.ShouldBe(address);
        person.Address.ShouldBe(address);
        person.ClearAddress();
        person.Address.HasValue.ShouldBe(false);
    }

    [Fact]
    public void ReplacePhones_WhenRequestedCollectionIsValid()
    {
        var person = CreatePerson(
            phones: [CreatePhone("555-111-1111", PhoneType.Mobile, true)],
            emails: []);
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
            emails: [CreateEmail("old@example.com", true)],
            phones: []);
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
        var person = CreatePerson(phones: [original], emails: []);

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
        var person = CreatePerson(emails: [original], phones: []);

        var result = person.ReplaceEmails(
        [
            CreateEmail("first@example.com", true),
            CreateEmail("second@example.com", true)
        ]);

        result.Error.ShouldBe(Contactable.MultiplePrimariesMessage);
        person.Emails.ShouldBe([original]);
    }

    [Fact]
    public void RejectInvalidCollections_BeforeConstructingPerson()
    {
        var result = Person.Create(
            PersonName.Create("Doe", "Jane").Value,
            Note.Create("Some notes.").Value,
            [
                CreateEmail("first@example.com", true),
                CreateEmail("second@example.com", true)
            ],
            [],
            Maybe<Birthday>.None,
            Maybe<Address>.None,
            Maybe<DriversLicense>.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Contactable.MultiplePrimariesMessage);
    }

    [Fact]
    public void Throw_WhenReplacementCollectionIsNull()
    {
        var person = CreatePerson(emails: [], phones: []);

        Should.Throw<ArgumentNullException>(() => person.ReplacePhones(null!));
        Should.Throw<ArgumentNullException>(() => person.ReplaceEmails(null!));
    }

    private static Person CreatePerson(
        IReadOnlyList<Email>? emails,
        IReadOnlyList<Phone>? phones) =>
        Person.Create(
            PersonName.Create("Doe", "Jane").Value,
            Note.Create("Some notes.").Value,
            emails ?? CreateEmails(),
            phones ?? CreatePhones(),
            Maybe<Birthday>.None,
            Maybe<Address>.None,
            Maybe<DriversLicense>.None
            ).Value;

    private static IReadOnlyList<Phone> CreatePhones() => [];

    private static IReadOnlyList<Email> CreateEmails() => [];

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
