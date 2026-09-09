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
            Result.Combine(
                    Environment.NewLine,
                    Result.FailureIf(!Enum.IsDefined(role), RequiredMessage))
                .Map(() => new RoleAssignment(role, periodAssigned));

        public Result<EmploymentRole> SetRole(EmploymentRole role)
        {
            return
                !Enum.IsDefined(role)
                ? Result.Failure<EmploymentRole>(RequiredMessage)
                : Result.Success(Role = role);
        }

        public Result<DateTimeRange> StartRoleAssignmentPeriod() =>
            StartRoleAssignmentPeriod(DateTime.Now);

        public Result<DateTimeRange> StartRoleAssignmentPeriod(DateTime startDate) =>
            DateTimeRange.Create(startDate, PeriodAssigned.End)
                .Tap(period => PeriodAssigned = period);

        public Result<DateTimeRange> EndRoleAssignmentPeriod() =>
            EndRoleAssignmentPeriod(DateTime.Today);

        public Result<DateTimeRange> EndRoleAssignmentPeriod(DateTime endDate) =>
            DateTimeRange.Create(PeriodAssigned.Start, endDate)
                .Tap(period => PeriodAssigned = period);

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
