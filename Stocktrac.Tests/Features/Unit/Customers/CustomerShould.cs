using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Customers;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Tests.Features.Unit.Customers;

public class CustomerShould
{
    [Fact]
    public void ReturnFirstError_On_Create_WhenPersonAndTypeAreInvalid()
    {
        var result = Customer.Create((Person)null!, (CustomerType)(-1), Maybe<CustomerCode>.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Customer.RequiredMessage);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExposeCustomerDetails_On_Create_ForEitherCase(bool business)
    {
        var result = CreateCustomerResult(business, CustomerType.Retail, Maybe<CustomerCode>.None);

        result.IsSuccess.ShouldBeTrue();
        var customer = result.Value;
        customer.Validate().IsSuccess.ShouldBeTrue();
        var name = customer switch
        {
            PersonCustomer person => person.Person.ToString(),
            BusinessCustomer company => company.Business.ToString()
        };
        customer.Name.ShouldBe(name);
        customer.Notes.ShouldBe(CreateNote());
        customer.Address.HasNoValue.ShouldBeTrue();
        customer.Code.HasNoValue.ShouldBeTrue();
        customer.CustomerType.ShouldBe(CustomerType.Retail);
        customer.Phones.ShouldBeEmpty();
        customer.Emails.ShouldBeEmpty();
        customer.Vehicles.ShouldBeEmpty();
        customer.ContactPreferences.AllowMail.ShouldBeTrue();
        customer.ContactPreferences.AllowEmail.ShouldBeTrue();
        customer.ContactPreferences.AllowSms.ShouldBeTrue();
    }

    [Fact]
    public void ReturnFailure_On_Create_WhenPersonIsNull() =>
        Customer.Create((Person)null!, CustomerType.Retail, Maybe<CustomerCode>.None)
            .Error.ShouldBe(Customer.RequiredMessage);

    [Fact]
    public void ReturnFailure_On_Create_WhenBusinessIsNull() =>
        Customer.Create((Business)null!, CustomerType.Retail, Maybe<CustomerCode>.None)
            .Error.ShouldBe(Customer.RequiredMessage);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReturnFailure_On_Create_WhenCustomerTypeIsUndefined(bool business) =>
        CreateCustomerResult(business, (CustomerType)999, Maybe<CustomerCode>.None)
            .Error.ShouldBe(Customer.UnknownCustomerTypeMessage);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PreserveAggregateState_On_ReplacePerson_ForEitherCase(bool business)
    {
        var customer = CreateCustomerWithVehicle(business);
        var originalId = customer.Id;
        var person = CreatePerson();
        person.ReplaceAddress(CreateAddress());
        person.AddPhone(CreatePhone("5551234567"));
        person.AddEmail(CreateEmail("jane@example.com"));

        var result = customer.ReplacePerson(person);

        result.IsSuccess.ShouldBeTrue();
        var replacement = result.Value;
        (replacement switch
        {
            PersonCustomer individual => individual.Person,
            BusinessCustomer => throw new Xunit.Sdk.XunitException("Expected a person customer.")
        }).ShouldBeSameAs(person);
        replacement.Name.ShouldBe(person.ToString());
        replacement.Address.ShouldBe(person.Address);
        replacement.Phones.ShouldBe(person.Phones);
        replacement.Emails.ShouldBe(person.Emails);
        replacement.CustomerType.ShouldBe(CustomerType.Fleet);
        replacement.Code.ShouldBe(customer.Code);
        replacement.ContactPreferences.ShouldBe(customer.ContactPreferences);
        replacement.Vehicles.ShouldBe(customer.Vehicles);
        replacement.Id.ShouldBe(originalId);
        replacement.ShouldBe(customer);
        replacement.GetHashCode().ShouldBe(customer.GetHashCode());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PreserveAggregateState_On_ReplaceBusiness_ForEitherCase(bool business)
    {
        var customer = CreateCustomerWithVehicle(business);
        var company = CreateBusiness();
        company.ReplaceAddress(CreateAddress());
        company.AddPhone(CreatePhone("5551234567"));
        company.AddEmail(CreateEmail("acme@example.com"));

        var result = customer.ReplaceBusiness(company);

        result.IsSuccess.ShouldBeTrue();
        var replacement = result.Value;
        (replacement switch
        {
            PersonCustomer => throw new Xunit.Sdk.XunitException("Expected a business customer."),
            BusinessCustomer commercial => commercial.Business
        }).ShouldBeSameAs(company);
        replacement.Name.ShouldBe(company.ToString());
        replacement.Address.ShouldBe(company.Address);
        replacement.Phones.ShouldBe(company.Phones);
        replacement.Emails.ShouldBe(company.Emails);
        replacement.CustomerType.ShouldBe(CustomerType.Fleet);
        replacement.Code.ShouldBe(customer.Code);
        replacement.ContactPreferences.ShouldBe(customer.ContactPreferences);
        replacement.Vehicles.ShouldBe(customer.Vehicles);
        replacement.Id.ShouldBe(customer.Id);
        replacement.ShouldBe(customer);
        replacement.GetHashCode().ShouldBe(customer.GetHashCode());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PreserveCustomer_On_ReplacePerson_WhenPersonIsNull(bool business)
    {
        var customer = CreateCustomerWithVehicle(business);
        var original = customer.Value;
        var vehicles = customer.Vehicles;

        customer.ReplacePerson(null!).Error.ShouldBe(Customer.RequiredMessage);

        customer.Value.ShouldBeSameAs(original);
        customer.Vehicles.ShouldBe(vehicles);
        customer.CustomerType.ShouldBe(CustomerType.Fleet);
        customer.Code.HasValue.ShouldBeTrue();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PreserveCustomer_On_ReplaceBusiness_WhenBusinessIsNull(bool business)
    {
        var customer = CreateCustomerWithVehicle(business);
        var original = customer.Value;
        var vehicles = customer.Vehicles;

        customer.ReplaceBusiness(null!).Error.ShouldBe(Customer.RequiredMessage);

        customer.Value.ShouldBeSameAs(original);
        customer.Vehicles.ShouldBe(vehicles);
        customer.CustomerType.ShouldBe(CustomerType.Fleet);
        customer.Code.HasValue.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ReturnFailure_On_Validate_WhenCustomerIsEmpty(int representation) =>
        CreateEmptyCustomer(representation).Validate().Error.ShouldBe(Customer.RequiredMessage);

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ReturnFailure_On_Mutations_WhenCustomerIsEmpty(int representation)
    {
        var customer = CreateEmptyCustomer(representation);

        customer.ReplacePerson(CreatePerson()).Error.ShouldBe(Customer.RequiredMessage);
        customer.ReplaceBusiness(CreateBusiness()).Error.ShouldBe(Customer.RequiredMessage);
        customer.UpdateAddress(CreateAddress()).Error.ShouldBe(Customer.RequiredMessage);
        customer.UpdateCustomerType(CustomerType.Fleet).Error.ShouldBe(Customer.RequiredMessage);
        customer.UpdateCode(CreateCode()).Error.ShouldBe(Customer.RequiredMessage);
        customer.AddVehicle(CreateVehicle()).Error.ShouldBe(Customer.RequiredMessage);
        customer.RemoveVehicle(CreateVehicle()).Error.ShouldBe(Customer.RequiredMessage);
        customer.AddPhone(CreatePhone("5551234567")).Error.ShouldBe(Customer.RequiredMessage);
        customer.RemovePhone(CreatePhone("5551234567")).Error.ShouldBe(Customer.RequiredMessage);
        customer.AddEmail(CreateEmail("jane@example.com")).Error.ShouldBe(Customer.RequiredMessage);
        customer.RemoveEmail(CreateEmail("jane@example.com")).Error.ShouldBe(Customer.RequiredMessage);
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PreserveCustomerType_On_UpdateCustomerType_WhenTypeIsUndefined(bool business)
    {
        var customer = CreateCustomer(business);

        customer.UpdateCustomerType((CustomerType)999).Error.ShouldBe(Customer.UnknownCustomerTypeMessage);

        customer.CustomerType.ShouldBe(CustomerType.Retail);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReplaceCustomerType_On_UpdateCustomerType_WhenTypeIsValid(bool business)
    {
        var customer = CreateCustomer(business);

        customer.UpdateCustomerType(CustomerType.Fleet).IsSuccess.ShouldBeTrue();

        customer.CustomerType.ShouldBe(CustomerType.Fleet);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PreserveCode_On_UpdateCode_WhenCodeIsNull(bool business)
    {
        var customer = CreateCustomerWithVehicle(business);
        var original = customer.Code;

        customer.UpdateCode(null!).Error.ShouldBe(Customer.RequiredMessage);

        customer.Code.ShouldBe(original);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SetCode_On_UpdateCode_WhenCodeIsValid(bool business)
    {
        var customer = CreateCustomer(business);
        var code = CreateCode();

        customer.UpdateCode(code).IsSuccess.ShouldBeTrue();

        customer.Code.Value.ShouldBeSameAs(code);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ClearCode_On_RemoveCode_WhenCodeExists(bool business)
    {
        var customer = CreateCustomerWithVehicle(business);

        customer.RemoveCode();

        customer.Code.HasNoValue.ShouldBeTrue();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PreserveVehicles_On_AddVehicle_WhenVehicleIsDuplicate(bool business)
    {
        var customer = CreateCustomerWithVehicle(business);
        var vehicle = customer.Vehicles.Single();
        var snapshot = customer.Vehicles;

        customer.AddVehicle(vehicle).IsFailure.ShouldBeTrue();

        customer.Vehicles.ShouldBe(new[] { vehicle });
        customer.RemoveVehicle(vehicle).IsSuccess.ShouldBeTrue();
        customer.Vehicles.ShouldBeEmpty();
        snapshot.ShouldBe(new[] { vehicle });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PreserveVehicles_On_RemoveVehicle_WhenVehicleIsMissing(bool business)
    {
        var customer = CreateCustomerWithVehicle(business);
        var snapshot = customer.Vehicles;

        customer.RemoveVehicle(CreateVehicle()).Error.ShouldBe(Contactable.NotFoundMessage);

        customer.Vehicles.ShouldBe(snapshot);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PreserveVehicles_On_VehicleMutations_WhenVehicleIsNull(bool business)
    {
        var customer = CreateCustomerWithVehicle(business);
        var snapshot = customer.Vehicles;

        customer.AddVehicle(null!).Error.ShouldBe(Customer.RequiredMessage);
        customer.RemoveVehicle(null!).Error.ShouldBe(Customer.RequiredMessage);

        customer.Vehicles.ShouldBe(snapshot);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CompareByCustomerIdentity_On_Equals_WhenSubjectsHaveIdenticalValues(bool business)
    {
        var first = CreateCustomer(business);
        var second = CreateCustomer(business);
        var copy = first;

        first.Equals(second).ShouldBeFalse();
        (first == second).ShouldBeFalse();
        (first != second).ShouldBeTrue();
        first.Equals(copy).ShouldBeTrue();
        first.Equals((object)copy).ShouldBeTrue();
        first.Equals((object?)null).ShouldBeFalse();
        first.Equals(new object()).ShouldBeFalse();
        first.GetHashCode().ShouldBe(copy.GetHashCode());
    }

    [Fact]
    public void CompareByCustomerIdentity_On_Equals_WhenCustomersReferToTheSamePerson()
    {
        var person = CreatePerson();
        var first = Customer.Create(person, CustomerType.Retail, Maybe<CustomerCode>.None).Value;
        var second = Customer.Create(person, CustomerType.Retail, Maybe<CustomerCode>.None).Value;

        first.Equals(second).ShouldBeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void CompareSafely_On_Equals_WhenCustomerIsEmpty(int representation)
    {
        var empty = CreateEmptyCustomer(representation);
        var customer = CreateCustomer(false);

        empty.Equals(default(Customer)).ShouldBeTrue();
        empty.Equals(customer).ShouldBeFalse();
        customer.Equals(empty).ShouldBeFalse();
        empty.GetHashCode().ShouldBe(0);
    }

    [Fact]
    public void ShareAggregateMutations_On_ReplaceBusiness_WhilePreservingOriginalPersonCase()
    {
        var customer = CreateCustomerWithVehicle(false);
        var originalCase = customer.Value;
        var snapshot = customer.Vehicles;
        var replacement = customer.ReplaceBusiness(CreateBusiness()).Value;

        replacement.RemoveCode();
        replacement.UpdateCustomerType(CustomerType.Retail).IsSuccess.ShouldBeTrue();
        replacement.RemoveVehicle(snapshot.Single()).IsSuccess.ShouldBeTrue();

        customer.Value.ShouldBeSameAs(originalCase);
        customer.Code.HasNoValue.ShouldBeTrue();
        customer.CustomerType.ShouldBe(CustomerType.Retail);
        customer.Vehicles.ShouldBeEmpty();
        snapshot.Count.ShouldBe(1);
        replacement.ShouldBe(customer);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PreserveCustomer_On_ImplicitConversion_FromValidatedCase(bool business)
    {
        var customer = CreateCustomer(business);

        Customer converted = customer switch
        {
            PersonCustomer person => person,
            BusinessCustomer company => company
        };

        converted.Validate().IsSuccess.ShouldBeTrue();
        converted.ShouldBe(customer);
        converted.Value.ShouldBeSameAs(customer.Value);
    }

    private static Customer CreateCustomer(bool business) =>
        CreateCustomerResult(business, CustomerType.Retail, Maybe<CustomerCode>.None).Value;

    private static Result<Customer> CreateCustomerResult(
        bool business, CustomerType customerType, Maybe<CustomerCode> code) => business
        ? Customer.Create(CreateBusiness(), customerType, code)
        : Customer.Create(CreatePerson(), customerType, code);

    private static Customer CreateCustomerWithVehicle(bool business)
    {
        var customer = CreateCustomerResult(business, CustomerType.Fleet, CreateCode()).Value;
        customer.AddVehicle(CreateVehicle()).IsSuccess.ShouldBeTrue();
        return customer;
    }

    private static Customer CreateEmptyCustomer(int representation) => representation switch
    {
        1 => new Customer((PersonCustomer)null!),
        2 => new Customer((BusinessCustomer)null!),
        _ => default
    };

    private static Business CreateBusiness() =>
        Business.Create(BusinessName.Create(NonEmptyString.Create("Acme Repair").Value).Value,
            Maybe<Address>.None, CreateNote(), Maybe<Person>.None, [], []).Value;

    private static Person CreatePerson() =>
        Person.Create(PersonName.Create(NonEmptyString.Create("Doe").Value,
            NonEmptyString.Create("Jane").Value).Value, CreateNote(), [], []).Value;

    private static CustomerCode CreateCode() =>
        CustomerCode.Create(NonEmptyString.Create("CUST-100").Value).Value;

    private static Vehicle CreateVehicle() => Vehicle.Create(
        TraditionalVehicleKind.Create("1HGCM82633A004352", "Honda", "Accord").Value,
        Maybe<int>.None, Maybe<string>.None, Maybe<State>.None,
        Maybe<string>.None, Maybe<string>.None, new DateOnly(2025, 6, 15)).Value;

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
