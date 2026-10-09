using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Employees;

public sealed record RoleAssignment
{
    public const string RequiredMessage = "Please include all required items.";
    public const string InvalidRoleMessage = "An assignment requires an employment role.";
    public const string InvalidPeriodMessage = "The assigned period must be within the role's valid date range.";

    public EmploymentRole Role { get; }
    public DateRange PeriodAssigned { get; }

    private RoleAssignment(EmploymentRole role, DateRange periodAssigned) =>
        (Role, PeriodAssigned) = (role, periodAssigned);

    public bool IsActive(DateTime date) =>
        DateOnly.FromDateTime(date).InRange(PeriodAssigned) && Role.IsActive(date);

    public static Result<RoleAssignment> Create(EmploymentRole role, DateRange periodAssigned)
    {
        if (role is null || periodAssigned is null)
            return Result.Failure<RoleAssignment>(RequiredMessage);

        if (ReferenceEquals(role, EmploymentRole.Empty))
            return Result.Failure<RoleAssignment>(InvalidRoleMessage);

        return Result.Success(role)
            .Ensure(
                value => periodAssigned.Start >= value.ValidDateRange.Start &&
                         periodAssigned.End <= value.ValidDateRange.End,
                InvalidPeriodMessage)
            .Map(value => new RoleAssignment(value, periodAssigned));
    }

    public Result<RoleAssignment> ReplaceRole(EmploymentRole role) => Create(role, PeriodAssigned);

    public Result<RoleAssignment> ReplacePeriodAssigned(DateRange periodAssigned) => Create(Role, periodAssigned);
}
