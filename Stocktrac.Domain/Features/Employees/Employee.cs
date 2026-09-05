using CSharpFunctionalExtensions;
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
    public static readonly string BenefitLoadMessage = $"Benefit load must be between {MinimumBenefitLoad} and {MaximumBenefitLoad}";
    public static string InvalidMaximumLengthMessage(int max) => $"Value must be less than {max} characters in length";
    public Person PersonEmployed { get; private set; }
    public IReadOnlyList<RoleAssignment> RoleAssignments => [.. roleAssignments];
    private readonly List<RoleAssignment> roleAssignments = [];
    public string? Notes { get; private set; }
    public SSN SSN { get; private set; }
    public string? CertificationNumber { get; private set; } // TODO: This should be defined and probably a value object 
    public DateTime? Hired { get; private set; } = null;
    public DateTime? Exited { get; private set; }
    public bool Active => Hired.HasValue && !Exited.HasValue;
    public string? PrintedName { get; private set; } // TTODO: his should be defined and probably a value object
    public EmployeeExpenseCategory ExpenseCategory { get; private set; } = EmployeeExpenseCategory.CostOfDirectLabor;
    public double BenefitLoad { get; private set; } = 0.0; // TODO: This should be defined and probably a value object 

    private Employee(Person personEmployed,
        List<RoleAssignment> roleAssignments,
        SSN ssn,
        DateTime hired,
        string? notes,
        string? certificationNumber,
        string? printedName,
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

        if (roleAssignments is not null)
            foreach (var assignment in roleAssignments)
                AddRoleAssignment(assignment);
    }

    public Result<RoleAssignment> AddRoleAssignment(RoleAssignment assignment)
    {
        if (assignment is null)
            return Result.Failure<RoleAssignment>(RequiredMessage);

        roleAssignments.Add(assignment);
        return Result.Success(assignment);
    }

    public static Result<Employee> Create(
        Person hiredPerson,
        List<RoleAssignment> roleAssignments,
        SSN ssn,
        DateTime hired,
        string? notes = null,
        string? certificationNumber = null,
        string? printedName = null,
        EmployeeExpenseCategory expenseCategory = EmployeeExpenseCategory.CostOfDirectLabor,
        double benefitLoad = 0.0)
        => Result.Success((
                Notes: (notes ?? string.Empty).Trim().Truncate(MaximumNoteLength),
                CertificationNumber: certificationNumber?.Trim() ?? string.Empty,
                PrintedName: printedName?.Trim() ?? string.Empty))
            .Ensure(_ => hiredPerson is not null, RequiredMessage)
            .Ensure(_ => hired >= StartDateMinimum && hired <= EndDateMaximum, DateRangeMessage)
            .Ensure(
                values => ValidateCertificationNumber(values.CertificationNumber).IsSuccess,
                InvalidMaximumLengthMessage(MaximumCertificationNumberLength))
            .Ensure(
                values => ValidatePrintedName(values.PrintedName).IsSuccess,
                InvalidMaximumLengthMessage(MaximumPrintedNameLength))
            .Ensure(_ => ValidateExpenseCategory(expenseCategory).IsSuccess, InvalidExpenseCategoryMessage)
            .Ensure(_ => ValidateBenefitLoad(benefitLoad).IsSuccess, BenefitLoadMessage)
            .Map(values => new Employee(
                hiredPerson,
                roleAssignments,
                ssn,
                hired,
                values.Notes,
                values.CertificationNumber,
                values.PrintedName,
                expenseCategory,
                benefitLoad));

    private static Result ValidateCertificationNumber(string? certificationNumber)
    {
        if (string.IsNullOrWhiteSpace(certificationNumber))
            return Result.Success();

        return certificationNumber.Trim().Length <= MaximumCertificationNumberLength
            ? Result.Success()
            : Result.Failure<string>(InvalidMaximumLengthMessage(MaximumCertificationNumberLength));
    }

    private static Result ValidatePrintedName(string? printedName)
    {
        if (string.IsNullOrWhiteSpace(printedName))
            return Result.Success();

        return printedName.Trim().Length <= MaximumPrintedNameLength
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

    public Result<DateTime> SetExited(DateTime exited)
    {
        if (!IsEmploymentDateWithinAllowedRange(exited))
        {
            return Result.Failure<DateTime>(DateRangeMessage);
        }

        if (!Hired.HasValue || exited < Hired.Value)
        {
            return Result.Failure<DateTime>(DateRangeMessage);
        }

        Exited = exited;
        return Result.Success(exited);
    }

    private static bool IsEmploymentDateWithinAllowedRange(DateTime employmentDate) =>
        employmentDate >= StartDateMinimum &&
        employmentDate <= EndDateMaximum;

    public Result<string> SetNotes(string notes) =>
        Result.Success(Notes = notes
            .Trim()
            .Truncate(MaximumNoteLength));

    public Result<SSN> SetSSN(SSN ssn) =>
        Result.Success(SSN = ssn);

    public Result SetCertificationNumber(string? certificationNumber)
    {
        certificationNumber = certificationNumber?.Trim() ?? string.Empty;

        return certificationNumber.Length > MaximumCertificationNumberLength
            ? Result.Failure(InvalidMaximumLengthMessage(MaximumCertificationNumberLength))
            : Result.Success(CertificationNumber = certificationNumber);
    }

    public Result<string> SetPrintedName(string printedName)
    {
        printedName = printedName?.Trim() ?? string.Empty;

        return printedName.Length <= MaximumPrintedNameLength
            ? Result.Success(PrintedName = printedName)
            : Result.Failure<string>(InvalidMaximumLengthMessage(MaximumPrintedNameLength));
    }

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
        PersonEmployed = Person.Create(personName, string.Empty).Value;
        SSN = SSN.Create(string.Empty).Value;
        Hired = DateTime.Today;
        Notes = string.Empty;
        CertificationNumber = string.Empty;
        PrintedName = string.Empty;
    }
}
