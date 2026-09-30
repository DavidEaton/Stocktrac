using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Customers;
using Stocktrac.Domain.Features.Employees;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Tests.Features.Unit;

public class OptionalAggregateStateShould
{
    [Fact]
    public void ExplicitlyUpdateAndRemoveBusinessContact()
    {
        var business = Business.Create(
            BusinessName.Create(NonEmptyString.Create("Acme Repair").Value).Value,
            Maybe<Address>.None,
            Note.Create(NonEmptyString.Create("No notes").Value).Value,
            Maybe<Person>.None,
            [],
            []).Value;
        var contact = CreatePerson();

        business.UpdateContact(contact);
        business.Contact.Value.ShouldBe(contact);

        business.RemoveContact();
        business.Contact.HasNoValue.ShouldBeTrue();
    }

    [Fact]
    public void ExplicitlyUpdateAndRemoveEmployeeExitDate()
    {
        var hired = DateTime.Today.AddDays(-1);
        var employee = Employee.Create(
            CreatePerson(),
            [],
            SSN.Create(NonEmptyString.Create("123-45-6789").Value).Value,
            hired,
            Note.Create(NonEmptyString.Create("No notes").Value).Value).Value;

        employee.UpdateExited(DateTime.Today).IsSuccess.ShouldBeTrue();
        employee.Exited.Value.ShouldBe(DateTime.Today);
        employee.Active.ShouldBeFalse();

        employee.RemoveExited();
        employee.Exited.HasNoValue.ShouldBeTrue();
        employee.Active.ShouldBeTrue();
    }

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
