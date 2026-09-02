using Shouldly;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class BusinessNameShould
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ReturnRequiredError_On_Create_WhenNameIsBlank(string? name)
    {
        var result = BusinessName.Create(name!);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(BusinessName.RequiredMessage);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(256)]
    public void ReturnLengthError_On_Create_WhenTrimmedNameIsOutsideBounds(int length)
    {
        var result = BusinessName.Create($"  {new string('x', length)}  ");

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe($"{BusinessName.InvalidLengthMessage} You entered {length} character(s).");
    }

    [Theory]
    [InlineData(2)]
    [InlineData(255)]
    public void ReturnNormalizedBusinessName_On_Create_WhenNameIsAtBoundary(int length)
    {
        var name = new string('x', length);

        var result = BusinessName.Create($"  {name}  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe(name);
        result.Value.ToString().ShouldBe(name);
    }

    [Fact]
    public void ApplySameValidation_On_NewBusinessName_WhenNameIsProvided()
    {
        BusinessName.NewBusinessName(" x ").Error.ShouldContain("1 character(s)");
        BusinessName.NewBusinessName("  Acme  ").Value.Name.ShouldBe("Acme");
    }

    [Fact]
    public void BeEqualAndHaveMatchingHashCodes_WhenNamesAreEqual()
    {
        var first = BusinessName.Create("Acme").Value;
        var second = BusinessName.Create("Acme").Value;

        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.ShouldNotBe(BusinessName.Create("Other").Value);
    }
}