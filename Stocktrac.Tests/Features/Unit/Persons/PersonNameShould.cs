using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Tests.Features.Unit.Persons;

public class PersonNameShould
{
    [Fact]
    public void ReplaceOnlyLastName_On_ReplaceLastName_WhenNameIsValid()
    {
        var original = ValidName();

        var result = original.ReplaceLastName(NonEmptyString.Create("Jones").Value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.LastName.Value.ShouldBe("Jones");
        result.Value.FirstName.ShouldBe(original.FirstName);
        result.Value.MiddleName.ShouldBe(original.MiddleName);
        original.LastName.Value.ShouldBe("Smith");
    }

    [Fact]
    public void ReplaceOnlyFirstName_On_ReplaceFirstName_WhenNameIsValid()
    {
        var original = ValidName();

        var result = original.ReplaceFirstName(NonEmptyString.Create("Jane").Value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.FirstName.Value.ShouldBe("Jane");
        result.Value.LastName.ShouldBe(original.LastName);
        result.Value.MiddleName.ShouldBe(original.MiddleName);
        original.FirstName.Value.ShouldBe("John");
    }

    [Fact]
    public void ReplaceOnlyMiddleName_On_AddOrReplaceMiddleName_WhenNameIsValid()
    {
        var original = ValidName();

        var result = original.AddOrReplaceMiddleName(NonEmptyString.Create("Quinn").Value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.MiddleName.Value.Value.ShouldBe("Quinn");
        result.Value.LastName.ShouldBe(original.LastName);
        result.Value.FirstName.ShouldBe(original.FirstName);
        original.MiddleName.Value.Value.ShouldBe("Paul");
    }

    [Fact]
    public void ReturnFailure_On_NonEmptyStringCreate_WhenNameIsMissing()
    {
        var result = NonEmptyString.Create("  ");

        result.Error.ShouldBe(NonEmptyString.RequiredMessage);
    }

    [Fact]
    public void RemoveMiddleNameAndPreserveOriginal_On_RemoveMiddleName_WhenMiddleNameExists()
    {
        var original = ValidName();

        var result = original.RemoveMiddleName();

        result.Value.MiddleName.HasNoValue.ShouldBeTrue();
        original.MiddleName.HasValue.ShouldBeTrue();
    }

    private static PersonName ValidName() =>
        PersonName.Create(
            NonEmptyString.Create("Smith").Value,
            NonEmptyString.Create("John").Value,
            NonEmptyString.Create("Paul").Value
            ).Value;
}
