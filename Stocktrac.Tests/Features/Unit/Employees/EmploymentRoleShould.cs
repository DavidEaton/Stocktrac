using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Employees;

namespace Stocktrac.Tests.Features.Unit.Employees;

public class EmploymentRoleShould
{
    private static readonly DateRange Range = DateRange.Create(new(2025, 1, 15), new(2025, 1, 20)).Value;

    [Fact]
    public void PreserveComponents_On_Create_WhenInputsAreValid()
    {
        var name = NonEmptyString.Create("Manager").Value;
        var description = NonEmptyString.Create("Manages a team").Value;
        var child = CreateRole();
        var superior = CreateRole();

        var result = EmploymentRole.Create(name, description, Range, [child], superior);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe(name);
        result.Value.Description.ShouldBe(description);
        result.Value.ValidDateRange.ShouldBe(Range);
        result.Value.SubordinateRoles.ShouldBe([child]);
        result.Value.SuperiorRole.ShouldBeSameAs(superior);
        child.SuperiorRole.ShouldBeSameAs(result.Value);
        superior.SubordinateRoles.ShouldBe([result.Value]);
    }

    [Fact]
    public void RepresentNoSuperior_On_Create_WhenSuperiorIsEmpty()
    {
        var role = CreateRole();

        role.SuperiorRole.ShouldBeSameAs(EmploymentRole.Empty);
        role.SubordinateRoles.ShouldBeEmpty();
        EmploymentRole.Empty.SuperiorRole.ShouldBeSameAs(EmploymentRole.Empty);
        EmploymentRole.Empty.SubordinateRoles.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("Name")]
    [InlineData("Description")]
    [InlineData("Range")]
    [InlineData("Superior")]
    [InlineData("Subordinates")]
    [InlineData("SubordinateMember")]
    public void ReturnFailureResult_On_Create_WhenRequiredValueIsNull(string component)
    {
        var name = NonEmptyString.Create("Role").Value;
        var result = EmploymentRole.Create(
            component == "Name" ? null! : name,
            component == "Description" ? null! : name,
            component == "Range" ? null! : Range,
            component == "Subordinates" ? null! : component == "SubordinateMember" ? [null!] : [],
            component == "Superior" ? null! : EmploymentRole.Empty);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EmploymentRole.RequiredMessage);
    }

    [Theory]
    [InlineData("Empty")]
    [InlineData("Duplicate")]
    [InlineData("AlreadyAssigned")]
    public void ReturnFailureAndPreserveHierarchy_On_Create_WhenSubordinatesAreInvalid(string scenario)
    {
        var child = CreateRole();
        var existingParent = CreateRole();
        var requestedParent = CreateRole();
        if (scenario == "AlreadyAssigned")
            existingParent.AddSubordinateRole(child).IsSuccess.ShouldBeTrue();
        IReadOnlyList<EmploymentRole> children = scenario switch
        {
            "Empty" => [EmploymentRole.Empty],
            "Duplicate" => [child, child],
            "AlreadyAssigned" => [child],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };

        var result = EmploymentRole.Create(
            NonEmptyString.Create("Role").Value,
            NonEmptyString.Create("Description").Value,
            Range, children, requestedParent);

        result.IsFailure.ShouldBeTrue();
        requestedParent.SubordinateRoles.ShouldBeEmpty();
        child.SuperiorRole.ShouldBeSameAs(scenario == "AlreadyAssigned" ? existingParent : EmploymentRole.Empty);
    }

    [Fact]
    public void ReturnFailureAndPreserveHierarchy_On_Create_WhenSuperiorDescendsFromSubordinate()
    {
        var root = CreateRole();
        var descendant = CreateRole();
        root.AddSubordinateRole(descendant).IsSuccess.ShouldBeTrue();

        var result = EmploymentRole.Create(
            NonEmptyString.Create("Role").Value,
            NonEmptyString.Create("Description").Value,
            Range, [root], descendant);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EmploymentRole.CyclicHierarchyMessage);
        root.SuperiorRole.ShouldBeSameAs(EmploymentRole.Empty);
        root.SubordinateRoles.ShouldBe([descendant]);
        descendant.SubordinateRoles.ShouldBeEmpty();
    }

    [Fact]
    public void CopySubordinates_On_Create_WhenCallerChangesInput()
    {
        var child = CreateRole();
        List<EmploymentRole> input = [child];
        var role = EmploymentRole.Create(
            NonEmptyString.Create("Role").Value,
            NonEmptyString.Create("Description").Value,
            Range, input, EmploymentRole.Empty).Value;

        input.Clear();

        role.SubordinateRoles.ShouldBe([child]);
        child.SuperiorRole.ShouldBeSameAs(role);
    }

    [Fact]
    public void PreserveSubordinates_On_SubordinateRoles_WhenSnapshotIsChanged()
    {
        var parent = CreateRole();
        var child = CreateRole();
        parent.AddSubordinateRole(child).IsSuccess.ShouldBeTrue();
        var snapshot = parent.SubordinateRoles;

        if (snapshot is IList<EmploymentRole> list && !list.IsReadOnly)
            list.Clear();

        parent.SubordinateRoles.ShouldBe([child]);
        child.SuperiorRole.ShouldBeSameAs(parent);
    }

    [Fact]
    public void LinkBothRoles_On_AddSubordinateRole_WhenRoleIsValid()
    {
        var parent = CreateRole();
        var child = CreateRole();

        var result = parent.AddSubordinateRole(child);

        result.IsSuccess.ShouldBeTrue();
        parent.SubordinateRoles.ShouldBe([child]);
        child.SuperiorRole.ShouldBeSameAs(parent);
    }

    [Fact]
    public void ReturnFailureAndPreserveHierarchy_On_AddSubordinateRole_WhenRoleIsDuplicate()
    {
        var parent = CreateRole();
        var child = CreateRole();
        parent.AddSubordinateRole(child).IsSuccess.ShouldBeTrue();

        var result = parent.AddSubordinateRole(child);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EmploymentRole.DuplicateRoleMessage);
        parent.SubordinateRoles.ShouldBe([child]);
        child.SuperiorRole.ShouldBeSameAs(parent);
    }

    [Fact]
    public void ReturnFailureAndPreserveHierarchy_On_AddSubordinateRole_WhenRoleAlreadyHasSuperior()
    {
        var parent = CreateRole();
        var child = CreateRole();
        var otherParent = CreateRole();
        parent.AddSubordinateRole(child).IsSuccess.ShouldBeTrue();

        var result = otherParent.AddSubordinateRole(child);

        result.IsFailure.ShouldBeTrue();
        otherParent.SubordinateRoles.ShouldBeEmpty();
        parent.SubordinateRoles.ShouldBe([child]);
        child.SuperiorRole.ShouldBeSameAs(parent);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReturnFailureAndPreserveHierarchy_On_AddSubordinateRole_WhenHierarchyWouldCycle(bool indirect)
    {
        var ancestor = CreateRole();
        var descendant = ancestor;
        if (indirect)
        {
            descendant = CreateRole();
            ancestor.AddSubordinateRole(descendant).IsSuccess.ShouldBeTrue();
        }

        var result = descendant.AddSubordinateRole(ancestor);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EmploymentRole.CyclicHierarchyMessage);
        ancestor.SuperiorRole.ShouldBeSameAs(EmploymentRole.Empty);
        descendant.SubordinateRoles.ShouldBeEmpty();
    }

    [Fact]
    public void UnlinkBothRoles_On_RemoveSubordinateRole_WhenRoleIsAssigned()
    {
        var parent = CreateRole();
        var child = CreateRole();
        parent.AddSubordinateRole(child).IsSuccess.ShouldBeTrue();

        var result = parent.RemoveSubordinateRole(child);

        result.IsSuccess.ShouldBeTrue();
        parent.SubordinateRoles.ShouldBeEmpty();
        child.SuperiorRole.ShouldBeSameAs(EmploymentRole.Empty);
    }

    [Fact]
    public void AllowNewSuperior_On_AddSubordinateRole_AfterRemovalFromPreviousSuperior()
    {
        var parent = CreateRole();
        var child = CreateRole();
        var newParent = CreateRole();
        parent.AddSubordinateRole(child).IsSuccess.ShouldBeTrue();
        parent.RemoveSubordinateRole(child).IsSuccess.ShouldBeTrue();

        var result = newParent.AddSubordinateRole(child);

        result.IsSuccess.ShouldBeTrue();
        parent.SubordinateRoles.ShouldBeEmpty();
        newParent.SubordinateRoles.ShouldBe([child]);
        child.SuperiorRole.ShouldBeSameAs(newParent);
    }

    [Theory]
    [InlineData("Null")]
    [InlineData("Empty")]
    [InlineData("Unknown")]
    public void ReturnFailureAndPreserveHierarchy_On_RemoveSubordinateRole_WhenRoleIsInvalid(string scenario)
    {
        var parent = CreateRole();
        var child = CreateRole();
        parent.AddSubordinateRole(child).IsSuccess.ShouldBeTrue();
        var requested = scenario == "Null" ? null! : scenario == "Empty" ? EmploymentRole.Empty : CreateRole();

        var result = parent.RemoveSubordinateRole(requested);

        result.IsFailure.ShouldBeTrue();
        parent.SubordinateRoles.ShouldBe([child]);
        child.SuperiorRole.ShouldBeSameAs(parent);
    }

    [Theory]
    [InlineData("Null")]
    [InlineData("Empty")]
    public void ReturnFailureAndPreserveHierarchy_On_AddSubordinateRole_WhenRoleIsMissing(string scenario)
    {
        var parent = CreateRole();

        var result = parent.AddSubordinateRole(scenario == "Null" ? null! : EmploymentRole.Empty);

        result.IsFailure.ShouldBeTrue();
        parent.SubordinateRoles.ShouldBeEmpty();
    }

    [Fact]
    public void ReturnFailureAndPreserveAbsence_On_AddSubordinateRole_WhenSuperiorIsEmpty()
    {
        var child = CreateRole();

        var result = EmploymentRole.Empty.AddSubordinateRole(child);

        result.IsFailure.ShouldBeTrue();
        EmploymentRole.Empty.SubordinateRoles.ShouldBeEmpty();
        child.SuperiorRole.ShouldBeSameAs(EmploymentRole.Empty);
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(5, true)]
    [InlineData(6, false)]
    public void EvaluateActivity_On_IsActive_UsingSuppliedDate(int days, bool expectedActive)
    {
        var role = CreateRole();

        role.IsActive(Range.Start.AddDays(days)).ShouldBe(expectedActive);
    }

    [Fact]
    public void RemainInactive_On_IsActive_WhenRoleIsEmpty()
    {
        EmploymentRole.Empty.IsActive(new DateOnly(2025, 1, 15)).ShouldBeFalse();
        EmploymentRole.Empty.IsActive(DateOnly.MinValue).ShouldBeFalse();
        EmploymentRole.Empty.IsActive(DateOnly.MaxValue).ShouldBeFalse();
    }

    [Fact]
    public void CompareByIdentity_On_Equals_WhenDifferentRolesHaveIdenticalValues()
    {
        var first = CreateRole();
        var second = CreateRole();

        first.ShouldBeAssignableTo<Entity>();
        first.Equals(second).ShouldBeFalse();
        (first == second).ShouldBeFalse();
        first.Equals(first).ShouldBeTrue();
    }

    [Fact]
    public void PreserveValueEquality_On_Equals_WhenReferencedRoleHierarchyChanges()
    {
        var role = CreateRole();
        var first = RoleAssignment.Create(role, Range).Value;
        var second = RoleAssignment.Create(role, Range).Value;
        var originalHash = first.GetHashCode();

        role.AddSubordinateRole(CreateRole()).IsSuccess.ShouldBeTrue();

        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(originalHash);
    }

    private static EmploymentRole CreateRole() =>
        EmploymentRole.Create(
            NonEmptyString.Create("Role").Value,
            NonEmptyString.Create("Description").Value,
            Range,
            [],
            EmploymentRole.Empty).Value;
}
