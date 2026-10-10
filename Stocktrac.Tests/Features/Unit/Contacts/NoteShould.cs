using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class NoteShould
{
    [Theory]
    [InlineData(1)]
    [InlineData(Note.MaximumLength)]
    public void PreserveValue_On_Create_WhenLengthIsAtBoundary(int length)
    {
        var value = NonEmptyString.Create(new string('a', length)).Value;

        var result = Note.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBeSameAs(value);
    }

    [Fact]
    public void Create_WhenGivenANonEmptyString()
    {
        var value = NonEmptyString.Create("A useful note.").Value;

        var result = Note.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
        result.Value.ToString().ShouldBe("A useful note.");
    }

    [Fact]
    public void ReturnMaximumLengthError_WhenValueExceedsMaximumLength()
    {
        var value = NonEmptyString.Create(new string('a', Note.MaximumLength + 1)).Value;

        var result = Note.Create(value);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Note.MaximumLengthMessage);
    }
}
