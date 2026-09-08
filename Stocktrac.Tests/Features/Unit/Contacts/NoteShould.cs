using Shouldly;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class NoteShould
{
    [Fact]
    public void NormalizeWhitespace_WhenCreated()
    {
        var result = Note.Create("   ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBeEmpty();
    }

    [Fact]
    public void TrimValue_WhenCreated()
    {
        var result = Note.Create("  A useful note.  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("A useful note.");
    }
}
