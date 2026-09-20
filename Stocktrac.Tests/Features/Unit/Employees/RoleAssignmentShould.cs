using Shouldly;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Employees;

namespace Stocktrac.Tests.Features.Unit.Employees;

public class RoleAssignmentShould
{
    private static readonly DateOnly Start = new(2025, 1, 15);

    [Fact]
    public void PreserveComponents_On_Create_WhenInputsAreValid()
    {
        var range = CreateRange(Start, Start.AddDays(5));

        var result = RoleAssignment.Create(EmploymentRole.Inspector, range);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Role.ShouldBe(EmploymentRole.Inspector);
        result.Value.PeriodAssigned.ShouldBe(range);
    }

    [Fact]
    public void ReturnRequiredError_On_Create_WhenRangeIsNull() =>
        RoleAssignment.Create(EmploymentRole.Inspector, null!).Error
            .ShouldBe(RoleAssignment.RequiredMessage);

    [Fact]
    public void ReplaceStart_On_StartRoleAssignmentPeriod_WhenDatePrecedesEnd()
    {
        var assignment = CreateAssignment();
        var replacement = Start.AddDays(1);

        var result = assignment.StartRoleAssignmentPeriod(replacement);

        result.IsSuccess.ShouldBeTrue();
        assignment.PeriodAssigned.Start.ShouldBe(replacement);
        assignment.PeriodAssigned.End.ShouldBe(Start.AddDays(5));
    }

    [Fact]
    public void PreservePeriod_On_StartRoleAssignmentPeriod_WhenDateDoesNotPrecedeEnd()
    {
        var assignment = CreateAssignment();
        var original = assignment.PeriodAssigned;

        var result = assignment.StartRoleAssignmentPeriod(original.End);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DateTimeRange.EndBeforeStartMessage);
        assignment.PeriodAssigned.ShouldBe(original);
    }

    [Fact]
    public void ReplaceEnd_On_EndRoleAssignmentPeriod_WhenDateFollowsStart()
    {
        var assignment = CreateAssignment();
        var replacement = Start.AddDays(10);

        var result = assignment.EndRoleAssignmentPeriod(replacement);

        result.IsSuccess.ShouldBeTrue();
        assignment.PeriodAssigned.Start.ShouldBe(Start);
        assignment.PeriodAssigned.End.ShouldBe(replacement);
    }

    [Fact]
    public void PreservePeriod_On_EndRoleAssignmentPeriod_WhenDateDoesNotFollowStart()
    {
        var assignment = CreateAssignment();
        var original = assignment.PeriodAssigned;

        var result = assignment.EndRoleAssignmentPeriod(original.Start);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DateTimeRange.EndBeforeStartMessage);
        assignment.PeriodAssigned.ShouldBe(original);
    }

    private static RoleAssignment CreateAssignment() =>
        RoleAssignment.Create(
            EmploymentRole.Inspector,
            CreateRange(Start, Start.AddDays(5))).Value;

    private static DateTimeRange CreateRange(DateOnly start, DateOnly end) =>
        DateTimeRange.Create(start, end).Value;
}
