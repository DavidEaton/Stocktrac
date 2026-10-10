using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Employees;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Tests.Features.Unit.Employees;

public class EmployeeShould
{
    private static readonly DateTime ReferenceDate = new(2025, 1, 15);

    [Fact]
    public void StoreExitDate_On_UpdateExited_WhenDateIsValid()
    {
        var employee = CreateEmployee();
        var exitDate = ReferenceDate;

        var result = employee.UpdateExited(exitDate, ReferenceDate);

        result.IsSuccess.ShouldBeTrue();
        employee.Exited.Value.ShouldBe(exitDate);
    }

    [Fact]
    public void BecomeInactive_On_UpdateExited_WhenDateIsValid()
    {
        var employee = CreateEmployee();

        employee.UpdateExited(ReferenceDate, ReferenceDate);

        employee.Active.ShouldBeFalse();
    }

    [Fact]
    public void RemoveExitDate_On_RemoveExited_WhenExitDateExists()
    {
        var employee = CreateEmployee();
        employee.UpdateExited(ReferenceDate, ReferenceDate);

        employee.RemoveExited(ReferenceDate).IsSuccess.ShouldBeTrue();

        employee.Exited.HasNoValue.ShouldBeTrue();
    }

    [Fact]
    public void BecomeActive_On_RemoveExited_WhenExitDateExists()
    {
        var employee = CreateEmployee();
        employee.UpdateExited(ReferenceDate, ReferenceDate);

        employee.RemoveExited(ReferenceDate).IsSuccess.ShouldBeTrue();

        employee.Active.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ReturnFailureAndPreserveAbsence_On_UpdateCertificationNumber_WhenValueIsMissing(string? value)
    {
        var employee = CreateEmployee();

        var result = employee.UpdateCertificationNumber(value!);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Employee.OptionalTextRequiredMessage);
        employee.CertificationNumber.HasNoValue.ShouldBeTrue();
    }

    [Fact]
    public void StoreTrimmedValue_On_UpdatePrintedName_WhenValueIsPresent()
    {
        var employee = CreateEmployee();

        var result = employee.UpdatePrintedName("  Jane Doe  ");

        result.IsSuccess.ShouldBeTrue();
        employee.PrintedName.Value.ShouldBe("Jane Doe");
    }

    [Fact]
    public void ReturnFailureAndPreserveNotes_On_UpdateNotes_WhenValueIsNull()
    {
        var employee = CreateEmployee();
        var original = employee.Notes;

        var result = employee.UpdateNotes(null!);

        result.IsFailure.ShouldBeTrue();
        employee.Notes.ShouldBe(original);
    }

    [Theory]
    [InlineData("Null")]
    [InlineData("NullMember")]
    [InlineData("Empty")]
    [InlineData("Expired")]
    [InlineData("Future")]
    [InlineData("Duplicate")]
    public void ReturnFailureResult_On_Create_WhenAssignmentsAreInvalid(string scenario)
    {
        var assignment = CreateAssignment();
        IReadOnlyList<RoleAssignment> assignments = scenario switch
        {
            "Null" => null!,
            "NullMember" => [null!],
            "Empty" => [],
            "Expired" => [CreateAssignment(ReferenceDate.AddDays(-10), ReferenceDate.AddDays(-1))],
            "Future" => [CreateAssignment(ReferenceDate.AddDays(1), ReferenceDate.AddDays(10))],
            "Duplicate" => [assignment, assignment.ReplacePeriodAssigned(assignment.PeriodAssigned).Value],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };

        var result = CreateEmployeeResult(assignments);

        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void PreserveActiveAssignment_On_Create_WhenOtherAssignmentsAreInactive()
    {
        var active = CreateAssignment();
        var expired = CreateAssignment(ReferenceDate.AddDays(-10), ReferenceDate.AddDays(-1));

        var result = CreateEmployeeResult([expired, active]);

        result.IsSuccess.ShouldBeTrue();
        result.Value.RoleAssignments.ShouldBe([expired, active]);
        result.Value.RoleAssignments.Any(assignment => assignment.IsActive(ReferenceDate)).ShouldBeTrue();
    }

    [Fact]
    public void CopyAssignments_On_Create_WhenCallerChangesInput()
    {
        var active = CreateAssignment();
        List<RoleAssignment> input = [active];
        var employee = CreateEmployeeResult(input).Value;

        input.Clear();

        employee.RoleAssignments.ShouldBe([active]);
    }

    [Fact]
    public void PreserveAssignments_On_RoleAssignments_WhenSnapshotIsChanged()
    {
        var employee = CreateEmployee();
        var original = employee.RoleAssignments.Single();
        var snapshot = employee.RoleAssignments;

        if (snapshot is IList<RoleAssignment> list && !list.IsReadOnly)
            list.Clear();

        employee.RoleAssignments.ShouldBe([original]);
    }

    [Theory]
    [InlineData("Null")]
    [InlineData("Duplicate")]
    public void ReturnFailureAndPreserveAssignments_On_AddRoleAssignment_WhenAssignmentIsInvalid(string scenario)
    {
        var employee = CreateEmployee();
        var original = employee.RoleAssignments.Single();
        var assignment = scenario == "Null" ? null! : original.ReplaceRole(original.Role).Value;

        var result = employee.AddRoleAssignment(assignment, ReferenceDate);

        result.IsFailure.ShouldBeTrue();
        employee.RoleAssignments.ShouldBe([original]);
    }

    [Fact]
    public void StoreAssignment_On_AddRoleAssignment_WhenAssignmentIsValid()
    {
        var employee = CreateEmployee();
        var assignment = CreateAssignment();

        var result = employee.AddRoleAssignment(assignment, ReferenceDate);

        result.IsSuccess.ShouldBeTrue();
        employee.RoleAssignments.ShouldContain(assignment);
        employee.RoleAssignments.Count.ShouldBe(2);
    }

    [Theory]
    [InlineData("Empty")]
    [InlineData("Expired")]
    [InlineData("Null")]
    [InlineData("NullMember")]
    [InlineData("Duplicate")]
    public void ReturnFailureAndPreserveAssignments_On_ReplaceRoleAssignments_WhenReplacementIsInvalid(string scenario)
    {
        var employee = CreateEmployee();
        var original = employee.RoleAssignments.Single();
        IReadOnlyList<RoleAssignment> replacement = scenario switch
        {
            "Empty" => [],
            "Expired" => [CreateAssignment(ReferenceDate.AddDays(-10), ReferenceDate.AddDays(-1))],
            "Null" => null!,
            "NullMember" => [null!],
            "Duplicate" => [original, original],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };

        var result = employee.ReplaceRoleAssignments(replacement, ReferenceDate);

        result.IsFailure.ShouldBeTrue();
        employee.RoleAssignments.ShouldBe([original]);
    }

    [Fact]
    public void ReplaceAssignments_On_ReplaceRoleAssignments_WhenReplacementIsValid()
    {
        var employee = CreateEmployee();
        var replacement = CreateAssignment();

        var result = employee.ReplaceRoleAssignments([replacement], ReferenceDate);

        result.IsSuccess.ShouldBeTrue();
        employee.RoleAssignments.ShouldBe([replacement]);
    }

    [Fact]
    public void ReturnFailureAndPreserveAssignments_On_RemoveRoleAssignment_WhenLastActiveAssignmentIsRemoved()
    {
        var employee = CreateEmployee();
        var active = employee.RoleAssignments.Single();
        var expired = CreateAssignment(ReferenceDate.AddDays(-10), ReferenceDate.AddDays(-1));
        employee.AddRoleAssignment(expired, ReferenceDate).IsSuccess.ShouldBeTrue();

        var result = employee.RemoveRoleAssignment(active.ReplaceRole(active.Role).Value, ReferenceDate);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Employee.ActiveRoleAssignmentRequiredMessage);
        employee.RoleAssignments.ShouldBe([active, expired]);
    }

    [Fact]
    public void RemoveAssignment_On_RemoveRoleAssignment_WhenAnotherActiveAssignmentRemains()
    {
        var employee = CreateEmployee();
        var original = employee.RoleAssignments.Single();
        var replacement = CreateAssignment();
        employee.AddRoleAssignment(replacement, ReferenceDate).IsSuccess.ShouldBeTrue();

        var result = employee.RemoveRoleAssignment(original.ReplaceRole(original.Role).Value, ReferenceDate);

        result.IsSuccess.ShouldBeTrue();
        employee.RoleAssignments.ShouldBe([replacement]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ReturnFailureAndPreserveAssignments_On_RemoveRoleAssignment_WhenAssignmentIsMissing(bool isNull)
    {
        var employee = CreateEmployee();
        var original = employee.RoleAssignments.Single();

        var result = employee.RemoveRoleAssignment(isNull ? null! : CreateAssignment(), ReferenceDate);

        result.IsFailure.ShouldBeTrue();
        employee.RoleAssignments.ShouldBe([original]);
    }

    [Fact]
    public void RemoveLastAssignment_On_RemoveRoleAssignment_WhenEmployeeIsInactive()
    {
        var employee = CreateEmployee();
        employee.UpdateExited(ReferenceDate, ReferenceDate).IsSuccess.ShouldBeTrue();

        var result = employee.RemoveRoleAssignment(employee.RoleAssignments.Single(), ReferenceDate);

        result.IsSuccess.ShouldBeTrue();
        employee.RoleAssignments.ShouldBeEmpty();
        employee.Active.ShouldBeFalse();
    }

    [Fact]
    public void AllowEmptyAssignments_On_ReplaceRoleAssignments_WhenEmployeeIsInactive()
    {
        var employee = CreateEmployee();
        employee.UpdateExited(ReferenceDate, ReferenceDate).IsSuccess.ShouldBeTrue();

        var result = employee.ReplaceRoleAssignments([], ReferenceDate);

        result.IsSuccess.ShouldBeTrue();
        employee.RoleAssignments.ShouldBeEmpty();
        employee.ValidateRoleAssignments(ReferenceDate).IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ReturnFailureAndPreserveExitDate_On_RemoveExited_WhenNoActiveAssignmentExists()
    {
        var employee = CreateEmployee();
        employee.UpdateExited(ReferenceDate, ReferenceDate).IsSuccess.ShouldBeTrue();
        employee.ReplaceRoleAssignments([], ReferenceDate).IsSuccess.ShouldBeTrue();

        var result = employee.RemoveExited(ReferenceDate);

        result.IsFailure.ShouldBeTrue();
        employee.Exited.Value.ShouldBe(ReferenceDate);
        employee.Active.ShouldBeFalse();
    }

    [Fact]
    public void ReturnFailureAndPreserveExitDate_On_RemoveExited_WhenAssignmentsHaveExpired()
    {
        var employee = CreateEmployee();
        employee.UpdateExited(ReferenceDate, ReferenceDate).IsSuccess.ShouldBeTrue();

        var result = employee.RemoveExited(ReferenceDate.AddDays(11));

        result.IsFailure.ShouldBeTrue();
        employee.Exited.Value.ShouldBe(ReferenceDate);
    }

    [Fact]
    public void BecomeActive_On_RemoveExited_AfterAddingActiveAssignment()
    {
        var employee = CreateEmployee();
        employee.UpdateExited(ReferenceDate, ReferenceDate).IsSuccess.ShouldBeTrue();
        employee.ReplaceRoleAssignments([], ReferenceDate).IsSuccess.ShouldBeTrue();
        employee.AddRoleAssignment(CreateAssignment(), ReferenceDate).IsSuccess.ShouldBeTrue();

        var result = employee.RemoveExited(ReferenceDate);

        result.IsSuccess.ShouldBeTrue();
        employee.Active.ShouldBeTrue();
        employee.ValidateRoleAssignments(ReferenceDate).IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ReturnFailureResult_On_ValidateRoleAssignments_WhenAssignmentsExpireAtSuppliedDate()
    {
        var employee = CreateEmployee();

        employee.ValidateRoleAssignments(ReferenceDate.AddDays(10)).IsSuccess.ShouldBeTrue();
        employee.ValidateRoleAssignments(ReferenceDate.AddDays(11)).IsFailure.ShouldBeTrue();
    }

    [Theory]
    [InlineData(-50, 0, true)]
    [InlineData(-50, -1, false)]
    [InlineData(1, 0, true)]
    [InlineData(1, 1, false)]
    public void ValidateHireDate_On_Create_UsingSuppliedDateBoundaries(int years, int days, bool expectedSuccess)
    {
        var hired = ReferenceDate.AddYears(years).AddDays(days);

        var result = CreateEmployeeResult([CreateAssignment()], hired);

        result.IsSuccess.ShouldBe(expectedSuccess);
    }

    [Theory]
    [InlineData(-50, 0, true)]
    [InlineData(-50, -1, false)]
    [InlineData(1, 0, true)]
    [InlineData(1, 1, false)]
    public void ValidateHireDateAndPreserveState_On_UpdateHired_UsingSuppliedDateBoundaries(int years, int days, bool expectedSuccess)
    {
        var employee = CreateEmployee();
        var original = employee.Hired;
        var hired = ReferenceDate.AddYears(years).AddDays(days);

        var result = employee.UpdateHired(hired, ReferenceDate);

        result.IsSuccess.ShouldBe(expectedSuccess);
        employee.Hired.ShouldBe(expectedSuccess ? hired : original);
    }

    [Fact]
    public void ReturnFailureAndPreserveHireDate_On_UpdateHired_WhenAssignmentsHaveExpired()
    {
        var employee = CreateEmployee();
        var original = employee.Hired;

        var result = employee.UpdateHired(ReferenceDate, ReferenceDate.AddDays(11));

        result.IsFailure.ShouldBeTrue();
        employee.Hired.ShouldBe(original);
    }

    [Fact]
    public void ReturnFailureAndPreserveHireDate_On_UpdateHired_WhenHireDateFollowsExit()
    {
        var employee = CreateEmployee();
        var original = employee.Hired;
        employee.UpdateExited(ReferenceDate, ReferenceDate).IsSuccess.ShouldBeTrue();

        var result = employee.UpdateHired(ReferenceDate.AddDays(1), ReferenceDate);

        result.IsFailure.ShouldBeTrue();
        employee.Hired.ShouldBe(original);
    }

    [Fact]
    public void ReturnFailureAndPreserveExitDate_On_UpdateExited_WhenNewDateIsInvalid()
    {
        var employee = CreateEmployee();
        employee.UpdateExited(ReferenceDate, ReferenceDate).IsSuccess.ShouldBeTrue();

        var result = employee.UpdateExited(ReferenceDate.AddDays(-2), ReferenceDate);

        result.IsFailure.ShouldBeTrue();
        employee.Exited.Value.ShouldBe(ReferenceDate);
    }

    [Fact]
    public void UseCalendarDateBounds_On_Create_WhenEvaluationDateIncludesTime()
    {
        var result = Employee.Create(
            CreatePerson(),
            [CreateAssignment()],
            SSN.Create(NonEmptyString.Create("123-45-6789").Value).Value,
            ReferenceDate.AddYears(-50),
            Note.Create(NonEmptyString.Create("No notes").Value).Value,
            ReferenceDate.AddHours(23));

        result.IsSuccess.ShouldBeTrue();
    }

    [Theory]
    [InlineData(-2, false)]
    [InlineData(-1, true)]
    [InlineData(0, true)]
    [InlineData(365, true)]
    [InlineData(366, false)]
    public void ValidateExitDateAndPreserveState_On_UpdateExited_UsingSuppliedDateBoundaries(int days, bool expectedSuccess)
    {
        var employee = CreateEmployee();
        var exited = ReferenceDate.AddDays(days);

        var result = employee.UpdateExited(exited, ReferenceDate);

        result.IsSuccess.ShouldBe(expectedSuccess);
        employee.Exited.HasValue.ShouldBe(expectedSuccess);
        if (expectedSuccess)
            employee.Exited.Value.ShouldBe(exited);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(9999)]
    public void ReturnSupportedDateBounds_On_StartDateMinimumAndEndDateMaximum_WhenReferenceDateIsExtreme(int year)
    {
        var date = new DateTime(year, 1, 1);

        Employee.StartDateMinimum(date).ShouldBeLessThanOrEqualTo(date);
        Employee.EndDateMaximum(date).ShouldBeGreaterThanOrEqualTo(date);
    }

    private static RoleAssignment CreateAssignment(DateTime? start = null, DateTime? end = null)
    {
        var range = DateRange.Create(
            DateOnly.FromDateTime(start ?? ReferenceDate.AddDays(-10)),
            DateOnly.FromDateTime(end ?? ReferenceDate.AddDays(10))).Value;
        var role = EmploymentRole.Create(
            NonEmptyString.Create("Technician").Value,
            NonEmptyString.Create("Services vehicles").Value,
            range,
            [],
            EmploymentRole.Empty).Value;
        return RoleAssignment.Create(role, range).Value;
    }

    private static Result<Employee> CreateEmployeeResult(
        IReadOnlyList<RoleAssignment> assignments, DateTime? hired = null) =>
        Employee.Create(
            CreatePerson(),
            assignments,
            SSN.Create(NonEmptyString.Create("123-45-6789").Value).Value,
            hired ?? ReferenceDate.AddDays(-1),
            Note.Create(NonEmptyString.Create("No notes").Value).Value,
            ReferenceDate);

    private static Employee CreateEmployee() => CreateEmployeeResult([CreateAssignment()]).Value;

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
