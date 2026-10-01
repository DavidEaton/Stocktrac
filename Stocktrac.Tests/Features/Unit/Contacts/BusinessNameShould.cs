using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Contacts;
using static Stocktrac.Domain.Features.NonEmptyStringConstruction;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class BusinessNameShould
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ReturnRequiredError_On_Create_WhenNameIsBlank(string? name)
    {
        var result = NonEmptyString.Create(name!);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(RequiredMessage);
    }

    [Fact]
    public void ReturnLengthError_On_Create_WhenNameExceedsMaximumLength()
    {
        var name = NonEmptyString.Create(new string('x', BusinessName.MaximumLength + 1)).Value;

        var result = BusinessName.Create(name);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(BusinessName.InvalidLengthMessage);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(255)]
    public void ReturnBusinessName_On_Create_WhenNameIsAtBoundary(int length)
    {
        var name = new string('x', length);

        var result = BusinessName.Create(NonEmptyString.Create(name).Value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.Value.ShouldBe(name);
        result.Value.ToString().ShouldBe(name);
    }

    [Fact]
    public void BeEqualAndHaveMatchingHashCodes_WhenNamesAreEqual()
    {
        var first = Create("Acme");
        var second = Create("Acme");

        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.ShouldNotBe(Create("Other"));
    }

    private static BusinessName Create(string name) =>
        BusinessName.Create(NonEmptyString.Create(name).Value).Value;
}
