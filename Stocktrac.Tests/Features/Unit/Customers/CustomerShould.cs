using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Customers;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Tests.Features.Unit.Customers;

public class CustomerShould
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExposeEntityDetails_On_Create_ForEitherCase(bool business)
    {
        var entity = CreateEntity(business);

        var result = Customer.Create(entity, CustomerType.Retail, Maybe<CustomerCode>.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.CustomerEntity.Value.ShouldBeSameAs(entity.Value);
        result.Value.Name.ShouldBe(entity.Name);
        result.Value.Notes.ShouldBe(CreateNote());
        result.Value.Address.HasNoValue.ShouldBeTrue();
        result.Value.Code.HasNoValue.ShouldBeTrue();
        result.Value.Phones.ShouldBeEmpty();
        result.Value.Emails.ShouldBeEmpty();
        result.Value.ContactPreferences.AllowMail.ShouldBeTrue();
        result.Value.ContactPreferences.AllowEmail.ShouldBeTrue();
        result.Value.ContactPreferences.AllowSms.ShouldBeTrue();
    }

    [Fact]
    public void ReturnFailure_On_Create_WhenEntityIsDefault() =>
        Customer.Create(default, CustomerType.Retail, Maybe<CustomerCode>.None)
            .Error.ShouldBe(Customer.RequiredMessage);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ReturnFailure_On_Create_WhenEntityCaseIsNull(bool business)
    {
        CustomerEntity entity = business
            ? new CustomerEntity((Business)null!)
            : new CustomerEntity((Person)null!);

        Customer.Create(entity, CustomerType.Retail, Maybe<CustomerCode>.None)
            .Error.ShouldBe(Customer.RequiredMessage);
    }

    [Fact]
    public void ReturnFailure_On_Create_WhenCustomerTypeIsUndefined() =>
        Customer.Create(CreateEntity(false), (CustomerType)999, Maybe<CustomerCode>.None)
            .Error.ShouldBe(Customer.UnknownCustomerTypeMessage);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ReplaceEntityAndPreserveAggregateState_On_UpdateCustomerEntity(bool business)
    {
        var code = CustomerCode.Create(NonEmptyString.Create("CUST-100").Value).Value;
        var customer = Customer.Create(CreateEntity(!business), CustomerType.Fleet, code).Value;
        var vehicle = Vehicle.Create(
            TraditionalVehicleKind.Create("1HGCM82633A004352", "Honda", "Accord").Value,
            Maybe<int>.None, Maybe<string>.None, Maybe<State>.None,
            Maybe<string>.None, Maybe<string>.None).Value;
        customer.AddVehicle(vehicle);
        var replacement = CreateEntity(business);
        var originalId = customer.Id;

        var result = customer.UpdateCustomerEntity(replacement);

        result.IsSuccess.ShouldBeTrue();
        customer.CustomerEntity.Value.ShouldBeSameAs(replacement.Value);
        customer.Name.ShouldBe(replacement.Name);
        customer.CustomerType.ShouldBe(CustomerType.Fleet);
        customer.Code.Value.ShouldBe(code);
        customer.Vehicles.ShouldBe(new[] { vehicle });
        customer.Id.ShouldBe(originalId);
    }

    [Fact]
    public void PreserveEntity_On_UpdateCustomerEntity_WhenEntityIsDefault()
    {
        var customer = CreateCustomer(false);
        var original = customer.CustomerEntity.Value;

        customer.UpdateCustomerEntity(default).Error.ShouldBe(Customer.RequiredMessage);

        customer.CustomerEntity.Value.ShouldBeSameAs(original);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PreserveEntity_On_UpdateCustomerEntity_WhenCaseIsNull(bool business)
    {
        var customer = CreateCustomer(false);
        var original = customer.CustomerEntity.Value;
        CustomerEntity entity = business
            ? new CustomerEntity((Business)null!)
            : new CustomerEntity((Person)null!);

        customer.UpdateCustomerEntity(entity).Error.ShouldBe(Customer.RequiredMessage);

        customer.CustomerEntity.Value.ShouldBeSameAs(original);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ReplaceAddress_On_UpdateAddress_ForEitherCase(bool business)
    {
        var customer = CreateCustomer(business);
        var address = CreateAddress();

        customer.UpdateAddress(address).IsSuccess.ShouldBeTrue();

        customer.Address.Value.ShouldBeSameAs(address);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PreserveAddress_On_UpdateAddress_WhenAddressIsNull(bool business)
    {
        var customer = CreateCustomer(business);
        var address = CreateAddress();
        customer.UpdateAddress(address);

        customer.UpdateAddress(null!).Error.ShouldBe(Customer.RequiredMessage);

        customer.Address.Value.ShouldBeSameAs(address);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ClearAddress_On_RemoveAddress_ForEitherCase(bool business)
    {
        var customer = CreateCustomer(business);
        customer.UpdateAddress(CreateAddress());

        customer.RemoveAddress();

        customer.Address.HasNoValue.ShouldBeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PreservePhones_On_AddPhone_WhenPhoneIsDuplicate(bool business)
    {
        var customer = CreateCustomer(business);
        var phone = CreatePhone("5551234567");
        customer.AddPhone(phone).IsSuccess.ShouldBeTrue();
        var snapshot = customer.Phones;

        customer.AddPhone(phone).Error.ShouldBe(Contactable.NonuniqueMessage);

        customer.Phones.ShouldBe(new[] { phone });
        customer.RemovePhone(phone).IsSuccess.ShouldBeTrue();
        customer.Phones.ShouldBeEmpty();
        snapshot.ShouldBe(new[] { phone });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PreservePhones_On_AddPhone_WhenPrimaryAlreadyExists(bool business)
    {
        var customer = CreateCustomer(business);
        var original = CreatePhone("5551234567");
        customer.AddPhone(original);

        customer.AddPhone(CreatePhone("5559876543")).Error.ShouldBe(Contactable.PrimaryExistsMessage);

        customer.Phones.ShouldBe(new[] { original });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PreserveEmails_On_AddEmail_WhenEmailIsDuplicate(bool business)
    {
        var customer = CreateCustomer(business);
        var email = CreateEmail("jane@example.com");
        customer.AddEmail(email).IsSuccess.ShouldBeTrue();
        var snapshot = customer.Emails;

        customer.AddEmail(email).Error.ShouldBe(Contactable.NonuniqueMessage);

        customer.Emails.ShouldBe(new[] { email });
        customer.RemoveEmail(email).IsSuccess.ShouldBeTrue();
        customer.Emails.ShouldBeEmpty();
        snapshot.ShouldBe(new[] { email });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PreserveEmails_On_AddEmail_WhenPrimaryAlreadyExists(bool business)
    {
        var customer = CreateCustomer(business);
        var original = CreateEmail("jane@example.com");
        customer.AddEmail(original);

        customer.AddEmail(CreateEmail("other@example.com")).Error.ShouldBe(Contactable.PrimaryExistsMessage);

        customer.Emails.ShouldBe(new[] { original });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ReturnFailure_On_ContactMutations_WhenContactIsNull(bool business)
    {
        var customer = CreateCustomer(business);

        customer.AddPhone(null!).Error.ShouldBe(Contactable.RequiredMessage);
        customer.RemovePhone(null!).Error.ShouldBe(Contactable.RequiredMessage);
        customer.AddEmail(null!).Error.ShouldBe(Contactable.RequiredMessage);
        customer.RemoveEmail(null!).Error.ShouldBe(Contactable.RequiredMessage);

        customer.Phones.ShouldBeEmpty();
        customer.Emails.ShouldBeEmpty();
    }

    private static Customer CreateCustomer(bool business) =>
        Customer.Create(CreateEntity(business), CustomerType.Retail, Maybe<CustomerCode>.None).Value;

    private static CustomerEntity CreateEntity(bool business) => business
        ? Business.Create(BusinessName.Create(NonEmptyString.Create("Acme Repair").Value).Value,
            Maybe<Address>.None, CreateNote(), Maybe<Person>.None, [], []).Value
        : Person.Create(PersonName.Create(NonEmptyString.Create("Doe").Value,
            NonEmptyString.Create("Jane").Value).Value, CreateNote(), [], []).Value;

    private static Note CreateNote() => Note.Create(NonEmptyString.Create("Some notes").Value).Value;

    private static Address CreateAddress() => Address.Create(
        AddressLine.Create(NonEmptyString.Create("123 Main St").Value).Value,
        City.Create(NonEmptyString.Create("Albany").Value).Value, State.NY,
        PostalCode.Create(NonEmptyString.Create("12345").Value).Value).Value;

    private static ContactPhone CreatePhone(string number) =>
        ContactPhone.Create(PhoneNumber.Create(NonEmptyString.Create(number).Value).Value, PhoneType.Mobile, true).Value;

    private static ContactEmail CreateEmail(string address) =>
        ContactEmail.Create(EmailAddress.Create(NonEmptyString.Create(address).Value).Value, true).Value;
}
