using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Customers;

namespace Stocktrac.Tests.Features.Unit.Customers;

public class CustomerCodeShould
{
    [Theory]
    [InlineData(1, true)]
    [InlineData(CustomerCode.MaximumLength, true)]
    [InlineData(CustomerCode.MaximumLength + 1, false)]
    public void EnforceMaximumLength_On_Create_WhenLengthIsAtBoundary(int length, bool valid)
    {
        var value = NonEmptyString.Create(new string('a', length)).Value;

        var result = CustomerCode.Create(value);

        result.IsSuccess.ShouldBe(valid);
        if (valid)
            result.Value.Value.ShouldBeSameAs(value);
        else
            result.Error.ShouldBe(CustomerCode.InvalidLengthMessage);
    }
}
