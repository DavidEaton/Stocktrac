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
        var role = EmploymentRole.Create(
            NonEmptyString.Create("Role").Value,
            NonEmptyString.Create("Description").Value,
            range,
            null).Value;

        var result = RoleAssignment.Create(role, range);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Role.ShouldBe(role);
        result.Value.PeriodAssigned.ShouldBe(range);
    }

    private static DateRange CreateRange(DateOnly start, DateOnly end) =>
        DateRange.Create(start, end).Value;
}
