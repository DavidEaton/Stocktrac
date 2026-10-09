using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Employees
{
    public sealed record RoleAssignment
    {
        public EmploymentRole Role { get; private set; }
        public DateRange PeriodAssigned { get; private set; }
        public bool IsActive =>
            DateOnly.FromDateTime(DateTime.Today).InRange(PeriodAssigned);

        private RoleAssignment(EmploymentRole role, DateRange periodAssigned) =>
            (Role, PeriodAssigned) = (role, periodAssigned);

        public static Result<RoleAssignment> Create(EmploymentRole role, DateRange periodAssigned) =>
            Result.Success(new RoleAssignment(role, periodAssigned));

        public Result<EmploymentRole> UpdateRole(EmploymentRole role) =>
            Result.Success(Role = role);

        // EF requires a parameterless constructor
        private RoleAssignment()
        {
            Role = null!;
            PeriodAssigned = null!;
        }   
    }
}
