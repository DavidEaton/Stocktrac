using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Employees
{
    public sealed record RoleAssignment
    {
        public const string RequiredMessage = "A valid Role is required.";
        public EmploymentRole Role { get; private set; }
        public DateRange PeriodAssigned { get; private set; }
        public bool IsActive =>
            DateOnly.FromDateTime(DateTime.Today).InRange(PeriodAssigned);

        private RoleAssignment(EmploymentRole role, DateRange periodAssigned) =>
            (Role, PeriodAssigned) = (role, periodAssigned);

        public static Result<RoleAssignment> Create(EmploymentRole role, DateRange periodAssigned) =>
            !Enum.IsDefined(role)
                ? Result.Failure<RoleAssignment>(RequiredMessage)
                : Result.Success(new RoleAssignment(role, periodAssigned));

        public Result<EmploymentRole> UpdateRole(EmploymentRole role) =>
            !Enum.IsDefined(role)
                ? Result.Failure<EmploymentRole>(RequiredMessage)
                : Result.Success(Role = role);

        // EF requires a parameterless constructor
        private RoleAssignment()
        {
            Role = EmploymentRole.Inspector;
            PeriodAssigned = DateRange.Create(
                DateOnly.FromDateTime(DateTime.Today),
                DateOnly.FromDateTime(DateTime.Today).AddDays(1))
            .Value;
        }
    }
}
