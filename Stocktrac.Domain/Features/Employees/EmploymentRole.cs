using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Employees;

public sealed class EmploymentRole : Entity
{
    public const string RequiredMessage = "Please include all required items.";
    public const string EmptyRoleMessage = "The empty role cannot participate in a role hierarchy.";
    public const string DuplicateRoleMessage = "Each subordinate role must be unique.";
    public const string AssignedSuperiorMessage = "Remove the existing superior before assigning a subordinate role.";
    public const string CyclicHierarchyMessage = "A role cannot be its own superior or subordinate.";
    public const string NotFoundMessage = "Subordinate role not found.";

    public static EmploymentRole Empty { get; } = new();

    public NonEmptyString Name { get; }
    public NonEmptyString Description { get; }
    public DateRange ValidDateRange { get; }
    public IReadOnlyList<EmploymentRole> SubordinateRoles => [.. subordinateRoles];
    private readonly List<EmploymentRole> subordinateRoles = [];
    public EmploymentRole SuperiorRole { get; private set; }

    private EmploymentRole(NonEmptyString name, NonEmptyString description, DateRange dateRange)
    {
        Name = name;
        Description = description;
        ValidDateRange = dateRange;
        SuperiorRole = Empty;
    }

    // The explicit absence object is never active, assigned, or persisted.
    private EmploymentRole()
    {
        Name = NonEmptyString.Create("Empty").Value;
        Description = NonEmptyString.Create("No employment role").Value;
        ValidDateRange = DateRange.Create(DateOnly.MinValue, DateOnly.MaxValue).Value;
        SuperiorRole = this;
    }

    public bool IsActive(DateOnly date) =>
        !ReferenceEquals(this, Empty) && date.InRange(ValidDateRange);

    public static Result<EmploymentRole> Create(
        NonEmptyString name,
        NonEmptyString description,
        DateRange dateRange,
        IReadOnlyList<EmploymentRole> subordinateRoles,
        EmploymentRole superiorRole) =>
        Result.Success()
            .Ensure(() => name is not null && description is not null && dateRange is not null &&
                superiorRole is not null && subordinateRoles is not null &&
                subordinateRoles.All(role => role is not null), RequiredMessage)
            .Ensure(() => subordinateRoles.All(role => !ReferenceEquals(role, Empty)), EmptyRoleMessage)
            .Ensure(() => subordinateRoles.Distinct().Count() == subordinateRoles.Count, DuplicateRoleMessage)
            .Ensure(() => subordinateRoles.All(role => ReferenceEquals(role.SuperiorRole, Empty)), AssignedSuperiorMessage)
            .Ensure(() => !HasAncestorAmong(superiorRole, subordinateRoles), CyclicHierarchyMessage)
            .Map(() => new EmploymentRole(name!, description!, dateRange!))
            .Tap(role =>
            {
                foreach (var subordinate in subordinateRoles)
                {
                    role.subordinateRoles.Add(subordinate);
                    subordinate.SuperiorRole = role;
                }

                if (!ReferenceEquals(superiorRole, Empty))
                {
                    superiorRole.subordinateRoles.Add(role);
                    role.SuperiorRole = superiorRole;
                }
            });

    private static bool HasAncestorAmong(
        EmploymentRole superiorRole,
        IReadOnlyList<EmploymentRole> roles)
    {
        for (var ancestor = superiorRole; !ReferenceEquals(ancestor, Empty); ancestor = ancestor.SuperiorRole)
        {
            if (roles.Contains(ancestor))
                return true;
        }

        return false;
    }

    public Result<EmploymentRole> AddSubordinateRole(EmploymentRole role)
    {
        if (role is null)
            return Result.Failure<EmploymentRole>(RequiredMessage);

        if (ReferenceEquals(this, Empty) || ReferenceEquals(role, Empty))
            return Result.Failure<EmploymentRole>(EmptyRoleMessage);

        if (subordinateRoles.Contains(role))
            return Result.Failure<EmploymentRole>(DuplicateRoleMessage);

        for (var ancestor = this; !ReferenceEquals(ancestor, Empty); ancestor = ancestor.SuperiorRole)
        {
            if (ancestor == role)
                return Result.Failure<EmploymentRole>(CyclicHierarchyMessage);
        }

        if (!ReferenceEquals(role.SuperiorRole, Empty))
            return Result.Failure<EmploymentRole>(AssignedSuperiorMessage);

        subordinateRoles.Add(role);
        role.SuperiorRole = this;
        return Result.Success(role);
    }

    public Result<EmploymentRole> RemoveSubordinateRole(EmploymentRole role)
    {
        if (role is null)
            return Result.Failure<EmploymentRole>(RequiredMessage);

        if (ReferenceEquals(this, Empty) || ReferenceEquals(role, Empty))
            return Result.Failure<EmploymentRole>(EmptyRoleMessage);

        var index = subordinateRoles.IndexOf(role);
        if (index < 0)
            return Result.Failure<EmploymentRole>(NotFoundMessage);

        var removed = subordinateRoles[index];
        subordinateRoles.RemoveAt(index);
        removed.SuperiorRole = Empty;
        return Result.Success(removed);
    }
}
