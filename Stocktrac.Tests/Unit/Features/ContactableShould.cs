using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Contact;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Tests.Unit.Features;

public class ContactableShould
{
    [Fact]
    public void Update_Phones_By_Adding_Modifying_And_Removing_Items()
    {
        var existingPrimaryPhone = CreatePhone("555-111-1111", PhoneType.Mobile, true, 1);
        var existingPhoneToRemove = CreatePhone("555-222-2222", PhoneType.Home, false, 2);
        var person = CreatePerson(phones: [existingPrimaryPhone, existingPhoneToRemove]);
        var requestedPrimaryPhone = CreatePhone("555-333-3333", PhoneType.Work, false, 1);
        var requestedNewPhone = CreatePhone("555-444-4444", PhoneType.Home, true);
        var contactDetails = ContactDetails.Create(
            phones: [requestedPrimaryPhone, requestedNewPhone],
            emails: [],
            address: Maybe<Address>.None).Value;

        person.UpdateContactDetails(contactDetails);

        person.Phones.Count.ShouldBe(2);
        person.Phones.ShouldContain(phone => phone.Id == 1 &&
                                             phone.Number == "555-333-3333" &&
                                             phone.PhoneType == PhoneType.Work &&
                                             phone.IsPrimary == false);
        person.Phones.ShouldContain(phone => phone.Id == 0 &&
                                             phone.Number == "555-444-4444" &&
                                             phone.IsPrimary);
        person.Phones.ShouldNotContain(phone => phone.Id == 2);
    }

    [Fact]
    public void Update_Emails_By_Adding_Modifying_And_Removing_Items()
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
        person.Emails.ShouldContain(email => email.Id == 1 &&
                                             email.Address == "updated@example.com" &&
                                             email.IsPrimary == false);
        person.Emails.ShouldContain(email => email.Id == 0 &&
                                             email.Address == "new@example.com" &&
                                             email.IsPrimary);
        person.Emails.ShouldNotContain(email => email.Id == 2);
    }

    [Fact]
    public void Remove_All_Phones_And_Emails_When_Contact_Details_Are_Empty()
    {
        var person = CreatePerson(
            phones: [CreatePhone("555-111-1111", PhoneType.Mobile, true, 1)],
            emails: [CreateEmail("primary@example.com", true, 1)]);
        var contactDetails = ContactDetails.Create(
            phones: [],
            emails: [],
            address: Maybe<Address>.None).Value;

        person.UpdateContactDetails(contactDetails);

        person.Phones.ShouldBeEmpty();
        person.Emails.ShouldBeEmpty();
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
