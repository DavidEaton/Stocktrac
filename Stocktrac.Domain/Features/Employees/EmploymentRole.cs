using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Employees
{
    public record EmploymentRole
    {
        public NonEmptyString Name { get; }
        public NonEmptyString Description { get; }
        public DateRange ValidDateRange { get; }
        public IReadOnlyList<EmploymentRole> SubordinateRoles => [.. subordinateRoles];
        private readonly List<EmploymentRole> subordinateRoles = [];
        public EmploymentRole SuperiorRole { get; }

        private EmploymentRole(
            NonEmptyString name,
            NonEmptyString description,
            DateRange dateRange,
            EmploymentRole superiorRole) =>
                (Name, Description, ValidDateRange, SuperiorRole) = (name, description, dateRange, superiorRole);

        public static Result<EmploymentRole> Create(
            NonEmptyString name,
            NonEmptyString description,
            DateRange dateRange,
            EmploymentRole superiorRole) =>
                Result.Success(new EmploymentRole(name, description, dateRange, superiorRole));
    }
}