using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Domain.Features.Employees
{
    public class RoleAssignment : Entity
    {
        public const string RequiredMessage = "Please include all required items.";
        public EmploymentRole Role { get; private set; }
        public DateRange PeriodAssigned { get; private set; }
        public bool IsActive =>
            DateOnly.FromDateTime(DateTime.Today).InRange(PeriodAssigned);

        private RoleAssignment(EmploymentRole role, DateRange periodAssigned) =>
            (Role, PeriodAssigned) = (role, periodAssigned);

        public static Result<RoleAssignment> Create(EmploymentRole role, DateRange periodAssigned) =>
            Result.Combine(
                Environment.NewLine,
                Result.FailureIf(periodAssigned is null, RequiredMessage),
                Result.FailureIf(!Enum.IsDefined(role), RequiredMessage))
                .Map(() => new RoleAssignment(role, periodAssigned!));

        public Result<EmploymentRole> SetRole(EmploymentRole role)
        {
            return
                !Enum.IsDefined(role)
                ? Result.Failure<EmploymentRole>(RequiredMessage)
                : Result.Success(Role = role);
        }

        public Result<DateRange> StartRoleAssignmentPeriod() =>
            StartRoleAssignmentPeriod(DateOnly.FromDateTime(DateTime.Today));

        public Result<DateRange> StartRoleAssignmentPeriod(DateOnly startDate) =>
            DateRange.Create(startDate, PeriodAssigned.End)
                .Tap(period => PeriodAssigned = period);

        public Result<DateRange> EndRoleAssignmentPeriod() =>
            EndRoleAssignmentPeriod(DateOnly.FromDateTime(DateTime.Today));

        public Result<DateRange> EndRoleAssignmentPeriod(DateOnly endDate) =>
            DateRange.Create(PeriodAssigned.Start, endDate)
                .Tap(period => PeriodAssigned = period);

        // EF requires a parameterless constructor
        protected RoleAssignment()
        {
            Role = EmploymentRole.Inspector;
            PeriodAssigned = DateRange.Create(
                DateOnly.FromDateTime(DateTime.Today),
                DateOnly.FromDateTime(DateTime.Today).AddDays(1))
            .Value;
        }
    }
}
