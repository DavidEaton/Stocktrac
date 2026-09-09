using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Domain.Features.Employees;

public class Employee : Entity
{
    // TODO: Move these constants to user-configurable settings in the future.
    // For now, they are hard-coded to match the current validation rules in StockTrac.
    public static DateTime StartDateMinimum => DateTime.Today.AddYears(-50);
    public static DateTime EndDateMaximum => DateTime.Today.AddYears(1);
    public const int MaximumNoteLength = 10000;
    public const int MaximumSSNLength = 12;
    public const int MaximumCertificationNumberLength = 20;
    public const int MaximumPrintedNameLength = 50;
    public static readonly double MinimumBenefitLoad = 0.0;
    public static readonly double MaximumBenefitLoad = 100.0;
    public static readonly string RequiredMessage = $"Please include all required items.";
    public static readonly string DateRangeMessage = $"Employment date(s) invalid.";
    public static readonly string InvalidExpenseCategoryMessage = $"Expense category is invalid.";
    public static readonly string BenefitLoadMessage = $"Benefit load must be between {MinimumBenefitLoad} and {MaximumBenefitLoad}.";
    public static string InvalidMaximumLengthMessage(int max) => $"Value must be less than {max} characters in length.";
    public Person PersonEmployed { get; private set; }
    public IReadOnlyList<RoleAssignment> RoleAssignments => [.. roleAssignments];
    private readonly List<RoleAssignment> roleAssignments = [];
    public Maybe<Note> Notes { get; private set; }
    public SSN SSN { get; private set; }
    public Maybe<string> CertificationNumber { get; private set; } // TODO: This should be defined and probably a value object
    public DateTime Hired { get; private set; }
    public Maybe<DateTime> Exited { get; private set; }
    public bool Active => !Exited.HasValue;
    public Maybe<string> PrintedName { get; private set; } // TTODO: his should be defined and probably a value object
    public EmployeeExpenseCategory ExpenseCategory { get; private set; } = EmployeeExpenseCategory.CostOfDirectLabor;
    public double BenefitLoad { get; private set; } = 0.0; // TODO: This should be defined and probably a value object 

    private Employee(Person personEmployed,
        IReadOnlyList<RoleAssignment> roleAssignments,
        SSN ssn,
        DateTime hired,
        Note notes,
        Maybe<string> certificationNumber,
        Maybe<string> printedName,
        EmployeeExpenseCategory expenseCategory,
        double benefitLoad)
    {
        PersonEmployed = personEmployed;
        SSN = ssn;
        Hired = hired;
        Notes = notes;
        CertificationNumber = certificationNumber;
        PrintedName = printedName;
        ExpenseCategory = expenseCategory;
        BenefitLoad = benefitLoad;

        this.roleAssignments.AddRange(roleAssignments);
    }

    public Result<RoleAssignment> AddRoleAssignment(RoleAssignment assignment)
    {
        roleAssignments.Add(assignment);
        return Result.Success(assignment);
    }

    public static Result<Employee> Create(
        Person hiredPerson,
        IReadOnlyList<RoleAssignment> roleAssignments,
        SSN ssn,
        DateTime hired,
        Note notes,
        Maybe<string> certificationNumber = default,
        Maybe<string> printedName = default,
        EmployeeExpenseCategory expenseCategory = EmployeeExpenseCategory.CostOfDirectLabor,
        double benefitLoad = 0.0)
    {
        return Result.Combine(
                Environment.NewLine,
                Result.FailureIf(hired < StartDateMinimum || hired > EndDateMaximum, DateRangeMessage),
                ValidateCertificationNumber(certificationNumber),
                ValidatePrintedName(printedName),
                ValidateExpenseCategory(expenseCategory),
                ValidateBenefitLoad(benefitLoad))
            .Map(() => new Employee(
                hiredPerson,
                roleAssignments,
                ssn,
                hired,
                notes,
                certificationNumber,
                printedName,
                expenseCategory,
                benefitLoad));
    }

    private static Result ValidateCertificationNumber(Maybe<string> certificationNumber)
    {
        if (certificationNumber.HasNoValue)
            return Result.Success();

        return certificationNumber.Value.Trim().Length <= MaximumCertificationNumberLength
            ? Result.Success()
            : Result.Failure<string>(InvalidMaximumLengthMessage(MaximumCertificationNumberLength));
    }

    private static Result ValidatePrintedName(Maybe<string> printedName)
    {
        if (printedName.HasNoValue)
            return Result.Success();

        return printedName.Value.Trim().Length <= MaximumPrintedNameLength
            ? Result.Success()
            : Result.Failure<string>(InvalidMaximumLengthMessage(MaximumPrintedNameLength));
    }

    private static Result ValidateExpenseCategory(EmployeeExpenseCategory expenseCategory)
    {
        return Enum.IsDefined(expenseCategory)
            ? Result.Success()
            : Result.Failure<EmployeeExpenseCategory>(InvalidExpenseCategoryMessage);
    }

    private static Result ValidateBenefitLoad(double benefitLoad)
    {
        return benefitLoad >= MinimumBenefitLoad && benefitLoad <= MaximumBenefitLoad
            ? Result.Success()
            : Result.Failure<double>(BenefitLoadMessage);
    }

    public Result<DateTime> SetHired(DateTime hired)
    {
        if (!IsEmploymentDateWithinAllowedRange(hired))
        {
            return Result.Failure<DateTime>(DateRangeMessage);
        }

        if (Exited.HasValue && hired > Exited.Value)
        {
            return Result.Failure<DateTime>(DateRangeMessage);
        }

        Hired = hired;
        return Result.Success(hired);
    }

    public Result<DateTime> SetExited(DateTime exited) =>
        Result.Success(exited)
            .Ensure(IsEmploymentDateWithinAllowedRange, DateRangeMessage)
            .Ensure(value => value >= Hired, DateRangeMessage)
            .Tap(value => Exited = value);

    private static bool IsEmploymentDateWithinAllowedRange(DateTime employmentDate) =>
        employmentDate >= StartDateMinimum &&
        employmentDate <= EndDateMaximum;

    public void SetNotes(Note notes) => Notes = notes;

    public void ClearNotes() => Notes = Maybe<Note>.None;

    public void SetSSN(SSN ssn) => SSN = ssn;

    public Result SetCertificationNumber(string certificationNumber)
    {
        certificationNumber = certificationNumber.Trim();

        return certificationNumber.Length > MaximumCertificationNumberLength
            ? Result.Failure(InvalidMaximumLengthMessage(MaximumCertificationNumberLength))
            : Result.Success(CertificationNumber = certificationNumber);
    }

    public void ClearCertificationNumber() => CertificationNumber = Maybe<string>.None;

    public Result<Maybe<string>> SetPrintedName(string printedName)
    {
        printedName = printedName.Trim();

        return printedName.Length <= MaximumPrintedNameLength
            ? Result.Success(PrintedName = printedName)
            : Result.Failure<Maybe<string>>(InvalidMaximumLengthMessage(MaximumPrintedNameLength));
    }

    public void ClearPrintedName() => PrintedName = Maybe<string>.None;

    public Result<EmployeeExpenseCategory> SetExpenseCategory(EmployeeExpenseCategory expenseCategory) =>
        Enum.IsDefined(expenseCategory)
            ? Result.Success(ExpenseCategory = expenseCategory)
            : Result.Failure<EmployeeExpenseCategory>(InvalidExpenseCategoryMessage);

    public Result<double> SetBenefitLoad(double benefitLoad) =>
        benefitLoad >= MinimumBenefitLoad && benefitLoad <= MaximumBenefitLoad
            ? Result.Success(BenefitLoad = benefitLoad)
            : Result.Failure<double>(BenefitLoadMessage);

    // EF requires a parameterless constructor
    private Employee()
    {
        roleAssignments = [];
        var personName = PersonName.Create("LastName", "FirstName").Value;
        PersonEmployed = Person.Create(personName, Note.Create(string.Empty).Value, [], [], Maybe<Birthday>.None, Maybe<Address>.None, Maybe<DriversLicense>.None).Value;
        SSN = SSN.Create(string.Empty).Value;
        Hired = DateTime.Today;
        Notes = Maybe<Note>.None;
        CertificationNumber = Maybe<string>.None;
        PrintedName = Maybe<string>.None;
    }
}
