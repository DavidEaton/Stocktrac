using Shouldly;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Tests.Features.Unit.Persons;

public class PersonNameShould
{
    [Fact]
    public void ReturnUpdatedCopy_On_ReplaceLastName_WhenNameIsValid()
    {
        var original = ValidName();

        var result = original.ReplaceLastName("  Jones  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.LastName.ShouldBe("Jones");
        result.Value.FirstName.ShouldBe(original.FirstName);
        result.Value.MiddleName.ShouldBe(original.MiddleName);
        original.LastName.ShouldBe("Smith");
    }

    [Fact]
    public void ReturnUpdatedCopy_On_ReplaceFirstName_WhenNameIsValid()
    {
        var original = ValidName();

        var result = original.ReplaceFirstName("  Jane  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.FirstName.ShouldBe("Jane");
        result.Value.LastName.ShouldBe(original.LastName);
        result.Value.MiddleName.ShouldBe(original.MiddleName);
        original.FirstName.ShouldBe("John");
    }

    [Fact]
    public void ReturnUpdatedCopy_On_AddOrReplaceMiddleName_WhenNameIsValid()
    {
        var original = ValidName();

        var result = original.AddOrReplaceMiddleName("  Quinn  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.MiddleName.Value.ShouldBe("Quinn");
        result.Value.LastName.ShouldBe(original.LastName);
        result.Value.FirstName.ShouldBe(original.FirstName);
        original.MiddleName.Value.ShouldBe("Paul");
    }

    [Fact]
    public void ReturnFailure_On_ReplaceLastName_WhenNameIsMissing()
    {
        var original = ValidName();

        var result = original.ReplaceLastName("  ");

        result.Error.ShouldBe(PersonName.RequiredMessage);
        original.LastName.ShouldBe("Smith");
    }

    [Fact]
    public void ReturnCopyRemoveMiddleName_On_RemoveMiddleName_WhenMiddleNameExists()
    {
        var original = ValidName();

        var result = original.RemoveMiddleName();

        result.Value.MiddleName.HasNoValue.ShouldBeTrue();
        original.MiddleName.HasValue.ShouldBeTrue();
    }

    private static PersonName ValidName() =>
        PersonName.Create("Smith", "John", "Paul").Value;
}
