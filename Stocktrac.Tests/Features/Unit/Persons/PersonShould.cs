using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Tests.Features.Unit.Persons;

public class PersonShould
{
    [Fact]
    public void PreserveBirthday_On_Create_WhenBirthdayIsProvided()
    {
        var birthday = Birthday.Create(new DateOnly(1990, 6, 15)).Value;

        var person = CreatePerson(birthday);

        person.Birthday.HasValue.ShouldBeTrue();
        person.Birthday.Value.ShouldBe(birthday);
    }

    [Fact]
    public void HaveNoBirthday_On_Create_WhenBirthdayIsAbsent()
    {
        var person = CreatePerson(Maybe<Birthday>.None);

        person.Birthday.HasNoValue.ShouldBeTrue();
    }

    [Fact]
    public void ReplaceBirthday_On_UpdateBirthday_WhenBirthdayIsValid()
    {
        var person = CreatePerson(Birthday.Create(new DateOnly(1990, 6, 15)).Value);
        var replacement = Birthday.Create(new DateOnly(1991, 7, 16)).Value;

        var result = person.UpdateBirthday(replacement);

        result.IsSuccess.ShouldBeTrue();
        person.Birthday.Value.ShouldBe(replacement);
    }

    [Fact]
    public void RemoveBirthday_On_RemoveBirthday_WhenBirthdayExists()
    {
        var person = CreatePerson(Birthday.Create(new DateOnly(1990, 6, 15)).Value);

        person.RemoveBirthday();

        person.Birthday.HasNoValue.ShouldBeTrue();
    }

    [Fact]
    public void ReturnName_On_ToString()
    {
        var person = CreatePerson(Maybe<Birthday>.None);

        person.ToString().ShouldBe(person.Name.ToString());
    }

    private static Person CreatePerson(Maybe<Birthday> birthday) =>
        Person.Create(
            PersonName.Create(
                NonEmptyString.Create("Doe").Value,
                NonEmptyString.Create("Jane").Value).Value,
            Note.Create(NonEmptyString.Create("Some notes.").Value).Value,
            [],
            [],
            birthday,
            Maybe<DriversLicense>.None,
            Maybe<Address>.None).Value;
}
