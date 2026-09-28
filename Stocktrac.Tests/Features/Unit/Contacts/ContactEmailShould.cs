using Stocktrac.Domain.Features;
using Shouldly;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class ContactEmailShould
{
    [Fact]
    public void PreserveComponents_On_Create()
    {
        var address = CreateAddress("john@doe.com");

        var result = ContactEmail.Create(address, true);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Address.ShouldBe(address);
        result.Value.IsPrimary.ShouldBeTrue();
    }

    [Fact]
    public void ExposeImmutableProperties()
    {
        typeof(ContactEmail).GetProperty(nameof(ContactEmail.Address))!.SetMethod.ShouldBeNull();
        typeof(ContactEmail).GetProperty(nameof(ContactEmail.IsPrimary))!.SetMethod.ShouldBeNull();
    }

    [Fact]
    public void ReturnUpdatedCopy_On_ReplaceAddress()
    {
        var email = CreateValidPrimaryEmail();
        var replacement = CreateAddress("updated@address.com");

        var result = email.ReplaceAddress(replacement);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Address.ShouldBe(replacement);
        result.Value.IsPrimary.ShouldBe(email.IsPrimary);
        result.Value.ShouldNotBeSameAs(email);
        email.Address.ShouldBe(CreateAddress("email@email.com"));
    }

    [Fact]
    public void ReturnUpdatedCopy_On_ReplaceIsPrimary()
    {
        var email = CreateValidPrimaryEmail();

        var result = email.ReplaceIsPrimary(false);

        result.IsSuccess.ShouldBeTrue();
        result.Value.IsPrimary.ShouldBeFalse();
        result.Value.Address.ShouldBe(email.Address);
        result.Value.ShouldNotBeSameAs(email);
        email.IsPrimary.ShouldBeTrue();
    }

    [Fact]
    public void ReturnEquivalentDistinctCopy_On_ReplaceIsPrimary_WhenValueDoesNotChange()
    {
        var email = CreateValidPrimaryEmail();

        var result = email.ReplaceIsPrimary(true);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(email);
        result.Value.GetHashCode().ShouldBe(email.GetHashCode());
        result.Value.ShouldNotBeSameAs(email);
    }

    [Fact]
    public void NotEquateInstances_WhenOnlyPrimaryStatusDiffers()
    {
        var primary = CreateValidPrimaryEmail();
        var secondary = ContactEmail.Create(primary.Address, false).Value;

        primary.ShouldNotBe(secondary);
    }

    internal static ContactEmail CreateValidPrimaryEmail() =>
        ContactEmail.Create(CreateAddress("email@email.com"), true).Value;

    private static EmailAddress CreateAddress(string value) =>
        EmailAddress.Create(NonEmptyString.Create(value).Value).Value;
}
