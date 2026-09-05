using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Domain.Features.Employees
{
    public class RoleAssignment : Entity
    {
        public static readonly string RequiredMessage = $"Please include all required items.";
        public EmploymentRole Role { get; private set; }
        public DateTimeRange PeriodAssigned { get; private set; }
        public bool IsActive =>
            DateTime.Today.InRange(PeriodAssigned);

        private RoleAssignment(EmploymentRole role, DateTimeRange periodAssigned) =>
            (Role, PeriodAssigned) = (role, periodAssigned);

        public static Result<RoleAssignment> Create(EmploymentRole role, DateTimeRange periodAssigned) =>
            Result.Success((Role: role, PeriodAssigned: periodAssigned))
                .Ensure(
                    values => Enum.IsDefined(values.Role) && values.PeriodAssigned is not null,
                    RequiredMessage)
                .Map(values => new RoleAssignment(values.Role, values.PeriodAssigned));

        public Result<EmploymentRole> SetRole(EmploymentRole role)
        {
            return
                !Enum.IsDefined(role)
                ? Result.Failure<EmploymentRole>(RequiredMessage)
                : Result.Success(Role = role);
        }

        public Result<DateTimeRange> StartRoleAssignmentPeriod(DateTime? startDate = null)
        {
            startDate ??= DateTime.Now;
            return Result.Success(PeriodAssigned =
                DateTimeRange.Create(
                    startDate.Value,
                    PeriodAssigned.End)
                .Value);
        }

        public Result<DateTimeRange> EndRoleAssignmentPeriod(DateTime? endDate = null)
        {
            endDate ??= DateTime.Today;
            return Result.Success(PeriodAssigned = 
                DateTimeRange.Create(
                    PeriodAssigned.Start,
                    endDate.Value)
                .Value);
        }

        // EF requires a parameterless constructor
        protected RoleAssignment()
        {
            Role = EmploymentRole.Inspector;
            PeriodAssigned = DateTimeRange.Create(
                DateTime.Now,
                DateTime.Now.AddDays(1))
            .Value;
        }
    }
}
