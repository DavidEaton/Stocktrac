using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Customers;
using Stocktrac.Domain.Features.Employees;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Tests.Features.Unit;

public class OptionalAggregateStateShould
{
    [Fact]
    public void ExplicitlySetAndClearBusinessContact()
    {
        var business = Business.Create(
            BusinessName.Create("Acme Repair").Value,
            Maybe<Address>.None,
            Note.Create(string.Empty).Value,
            Maybe<Person>.None,
            [],
            []).Value;
        var contact = CreatePerson();

        business.SetContact(contact);
        business.Contact.Value.ShouldBe(contact);

        business.ClearContact();
        business.Contact.HasNoValue.ShouldBeTrue();
    }

    [Fact]
    public void ExplicitlySetAndClearEmployeeExitDate()
    {
        var hired = DateTime.Today.AddDays(-1);
        var employee = Employee.Create(
            CreatePerson(),
            [],
            SSN.Create("123-45-6789").Value,
            hired,
            Note.Create(string.Empty).Value).Value;

        employee.SetExited(DateTime.Today).IsSuccess.ShouldBeTrue();
        employee.Exited.Value.ShouldBe(DateTime.Today);
        employee.Active.ShouldBeFalse();

        employee.ClearExited();
        employee.Exited.HasNoValue.ShouldBeTrue();
        employee.Active.ShouldBeTrue();
    }

    private static Person CreatePerson() =>
        Person.Create(
            PersonName.Create("Doe", "Jane").Value,
            Note.Create(string.Empty).Value,
            [],
            [],
            Maybe<Birthday>.None,
            Maybe<Address>.None,
            Maybe<DriversLicense>.None).Value;
}
