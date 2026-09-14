using Shouldly;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Tests.Features.Unit.Persons;

public class PersonNameShould
{
    [Fact]
    public void ReturnUpdatedCopy_On_WithLastName_WhenNameIsValid()
    {
        var original = ValidName();

        var result = original.WithLastName("  Jones  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.LastName.ShouldBe("Jones");
        result.Value.FirstName.ShouldBe(original.FirstName);
        result.Value.MiddleName.ShouldBe(original.MiddleName);
        original.LastName.ShouldBe("Smith");
    }

    [Fact]
    public void ReturnUpdatedCopy_On_WithFirstName_WhenNameIsValid()
    {
        var original = ValidName();

        var result = original.WithFirstName("  Jane  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.FirstName.ShouldBe("Jane");
        result.Value.LastName.ShouldBe(original.LastName);
        result.Value.MiddleName.ShouldBe(original.MiddleName);
        original.FirstName.ShouldBe("John");
    }

    [Fact]
    public void ReturnUpdatedCopy_On_WithMiddleName_WhenNameIsValid()
    {
        var original = ValidName();

        var result = original.WithMiddleName("  Quinn  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.MiddleName.Value.ShouldBe("Quinn");
        result.Value.LastName.ShouldBe(original.LastName);
        result.Value.FirstName.ShouldBe(original.FirstName);
        original.MiddleName.Value.ShouldBe("Paul");
    }

    [Fact]
    public void ReturnFailure_On_WithLastName_WhenNameIsMissing()
    {
        var original = ValidName();

        var result = original.WithLastName("  ");

        result.Error.ShouldBe(PersonName.RequiredMessage);
        original.LastName.ShouldBe("Smith");
    }

    [Fact]
    public void ReturnCopyWithoutMiddleName_On_WithoutMiddleName_WhenMiddleNameExists()
    {
        var original = ValidName();

        var result = original.WithoutMiddleName();

        result.Value.MiddleName.HasNoValue.ShouldBeTrue();
        original.MiddleName.HasValue.ShouldBeTrue();
    }

    private static PersonName ValidName() =>
        PersonName.Create("Smith", "John", "Paul").Value;
}
