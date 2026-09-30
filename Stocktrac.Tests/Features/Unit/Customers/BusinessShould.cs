using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Customers;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Tests.Features.Unit.Customers;

public class BusinessShould
{
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
