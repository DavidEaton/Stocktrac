using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Employees;

namespace Stocktrac.Tests.Features.Unit.Employees;

public class RoleAssignmentShould
{
    private static readonly DateOnly Start = new(2025, 1, 15);

    [Fact]
    public void PreserveComponents_On_Create_WhenInputsAreValid()
    {
        var range = CreateRange(Start, Start.AddDays(5));
        var role = CreateRole(range);

        var result = RoleAssignment.Create(role, range);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Role.ShouldBeSameAs(role);
        result.Value.PeriodAssigned.ShouldBe(range);
    }

    [Theory]
    [InlineData("Role")]
    [InlineData("Period")]
    public void ReturnFailureResult_On_Create_WhenRequiredValueIsNull(string component)
    {
        var range = CreateRange(Start, Start.AddDays(5));
        var role = CreateRole(range);

        var result = RoleAssignment.Create(component == "Role" ? null! : role, component == "Period" ? null! : range);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(RoleAssignment.RequiredMessage);
    }

    [Fact]
    public void ReturnFailureResult_On_Create_WhenRoleIsEmpty()
    {
        var result = RoleAssignment.Create(EmploymentRole.Empty, CreateRange(Start, Start.AddDays(5)));

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(RoleAssignment.InvalidRoleMessage);
    }

    [Theory]
    [InlineData(-1, 5, false)]
    [InlineData(0, 6, false)]
    [InlineData(0, 5, true)]
    [InlineData(1, 4, true)]
    public void ValidateAssignmentPeriod_On_Create_WhenComparedWithRoleValidity(int startOffset, int endOffset, bool expectedSuccess)
    {
        var role = CreateRole(CreateRange(Start, Start.AddDays(5)));
        var period = CreateRange(Start.AddDays(startOffset), Start.AddDays(endOffset));

        var result = RoleAssignment.Create(role, period);

        result.IsSuccess.ShouldBe(expectedSuccess);
        if (!expectedSuccess)
            result.Error.ShouldBe(RoleAssignment.InvalidPeriodMessage);
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(5, true)]
    [InlineData(6, false)]
    public void EvaluateActivity_On_IsActive_UsingSuppliedDate(int days, bool expectedActive)
    {
        var range = CreateRange(Start, Start.AddDays(5));
        var assignment = RoleAssignment.Create(CreateRole(range), range).Value;
        var date = Start.AddDays(days);

        assignment.IsActive(date).ShouldBe(expectedActive);
        if (expectedActive)
            assignment.Role.IsActive(date).ShouldBeTrue();
    }

    [Fact]
    public void CompareByValue_On_Equals_WhenComponentsMatch()
    {
        var role = CreateRole(CreateRange(Start, Start.AddDays(5)));
        var first = RoleAssignment.Create(role, CreateRange(Start, Start.AddDays(5))).Value;
        var second = RoleAssignment.Create(role, CreateRange(Start, Start.AddDays(5))).Value;

        first.ShouldNotBeSameAs(second);
        (first == second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        new HashSet<RoleAssignment> { first, second }.Count.ShouldBe(1);
    }

    [Fact]
    public void RemainUnequal_On_Equals_WhenRolesHaveDifferentIdentity()
    {
        var range = CreateRange(Start, Start.AddDays(5));
        var first = RoleAssignment.Create(CreateRole(range), range).Value;
        var second = RoleAssignment.Create(CreateRole(range), range).Value;

        (first == second).ShouldBeFalse();
    }

    [Fact]
    public void RemainUnequal_On_Equals_WhenPeriodsDiffer()
    {
        var range = CreateRange(Start, Start.AddDays(5));
        var role = CreateRole(range);
        var first = RoleAssignment.Create(role, range).Value;
        var second = RoleAssignment.Create(role, CreateRange(Start, Start.AddDays(4))).Value;

        (first == second).ShouldBeFalse();
    }

    [Fact]
    public void ReturnNewValueAndPreserveOriginal_On_ReplaceRole_WhenRoleIsValid()
    {
        var range = CreateRange(Start, Start.AddDays(5));
        var originalRole = CreateRole(range);
        var replacement = CreateRole(range);
        var assignment = RoleAssignment.Create(originalRole, range).Value;

        var result = assignment.ReplaceRole(replacement);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeSameAs(assignment);
        result.Value.Role.ShouldBeSameAs(replacement);
        result.Value.PeriodAssigned.ShouldBe(range);
        assignment.Role.ShouldBeSameAs(originalRole);
    }

    [Fact]
    public void ReturnFailureAndPreserveOriginal_On_ReplaceRole_WhenRoleDoesNotCoverPeriod()
    {
        var range = CreateRange(Start, Start.AddDays(5));
        var originalRole = CreateRole(range);
        var assignment = RoleAssignment.Create(originalRole, range).Value;

        var result = assignment.ReplaceRole(CreateRole(CreateRange(Start, Start.AddDays(4))));

        result.IsFailure.ShouldBeTrue();
        assignment.Role.ShouldBeSameAs(originalRole);
        assignment.PeriodAssigned.ShouldBe(range);
    }

    [Fact]
    public void ReturnNewValueAndPreserveOriginal_On_ReplacePeriodAssigned_WhenPeriodIsValid()
    {
        var range = CreateRange(Start, Start.AddDays(5));
        var assignment = RoleAssignment.Create(CreateRole(range), range).Value;
        var replacement = CreateRange(Start.AddDays(1), Start.AddDays(4));

        var result = assignment.ReplacePeriodAssigned(replacement);

        result.IsSuccess.ShouldBeTrue();
        result.Value.PeriodAssigned.ShouldBe(replacement);
        result.Value.Role.ShouldBeSameAs(assignment.Role);
        assignment.PeriodAssigned.ShouldBe(range);
    }

    [Fact]
    public void ReturnFailureAndPreserveOriginal_On_ReplacePeriodAssigned_WhenPeriodExceedsRoleValidity()
    {
        var range = CreateRange(Start, Start.AddDays(5));
        var assignment = RoleAssignment.Create(CreateRole(range), range).Value;

        var result = assignment.ReplacePeriodAssigned(CreateRange(Start, Start.AddDays(6)));

        result.IsFailure.ShouldBeTrue();
        assignment.PeriodAssigned.ShouldBe(range);
    }

    private static EmploymentRole CreateRole(DateRange range) =>
        EmploymentRole.Create(
            NonEmptyString.Create("Role").Value,
            NonEmptyString.Create("Description").Value,
            range,
            [],
            EmploymentRole.Empty).Value;

    private static DateRange CreateRange(DateOnly start, DateOnly end) =>
        DateRange.Create(start, end).Value;
}
