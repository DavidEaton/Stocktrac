using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Employees;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Tests.Features.Unit.Employees;

public class EmployeeShould
{
    [Fact]
    public void StoreExitDate_On_UpdateExited_WhenDateIsValid()
    {
        var employee = CreateEmployee();
        var exitDate = DateTime.Today;

        var result = employee.UpdateExited(exitDate);

        result.IsSuccess.ShouldBeTrue();
        employee.Exited.Value.ShouldBe(exitDate);
    }

    [Fact]
    public void BecomeInactive_On_UpdateExited_WhenDateIsValid()
    {
        var employee = CreateEmployee();

        employee.UpdateExited(DateTime.Today);

        employee.Active.ShouldBeFalse();
    }

    [Fact]
    public void RemoveExitDate_On_RemoveExited_WhenExitDateExists()
    {
        var employee = CreateEmployee();
        employee.UpdateExited(DateTime.Today);

        employee.RemoveExited();

        employee.Exited.HasNoValue.ShouldBeTrue();
    }

    [Fact]
    public void BecomeActive_On_RemoveExited_WhenExitDateExists()
    {
        var employee = CreateEmployee();
        employee.UpdateExited(DateTime.Today);

        employee.RemoveExited();

        employee.Active.ShouldBeTrue();
    }

    private static Employee CreateEmployee() =>
        Employee.Create(
            CreatePerson(),
            [],
            SSN.Create(NonEmptyString.Create("123-45-6789").Value).Value,
            DateTime.Today.AddDays(-1),
            Note.Create(NonEmptyString.Create("No notes").Value).Value).Value;

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
