using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Domain.Features.Employees;

public class Employee : Entity
{
    // TODO: Move these constants to user-configurable settings in the future.
    // For now, they are hard-coded to match the current validation rules in StockTrac.
    public static readonly DateTime StartDateMinimum = DateTime.Today.AddYears(-50);
    public static readonly DateTime EndDateMaximum = DateTime.Today.AddYears(1);
    public static readonly int MaximumNoteLength = 10000;
    public static readonly int MaximumSSNLength = 12;
    public static readonly int MaximumCertificationNumberLength = 20;
    public static readonly int MaximumPrintedNameLength = 50;
    public static readonly double MinimumBenefitLoad = 0.0;
    public static readonly double MaximumBenefitLoad = 100.0;

    public static readonly string RequiredMessage = $"Please include all required items.";
    public static readonly string DateRangeMessage = $"Employment date(s) invalid.";
    public static readonly string InvalidExpenseCategoryMessage = $"Expense category is invalid.";
    public static readonly string BenefitLoadMessage = $"Benefit load must be between {MinimumBenefitLoad} and {MaximumBenefitLoad}";
    public static string InvalidMaximumLengthMessage(int max) => $"Value must be less than {max} characters in length";

    public Person PersonalDetails { get; private set; }
    public DateTime? Hired { get; private set; } = null;
    public DateTime? Exited { get; private set; }
    public string CompanyEmployeeId { get; set; }
    public IReadOnlyList<RoleAssignment> RoleAssignments => roleAssignments.ToList();
    private readonly List<RoleAssignment> roleAssignments = new();
    public string Notes { get; private set; }
    public SSN SSN { get; private set; }
    public string CertificationNumber { get; private set; }
    public bool Active { get; private set; } = true;
    public string PrintedName { get; private set; }
    public EmployeeExpenseCategory ExpenseCategory { get; private set; } = EmployeeExpenseCategory.CostOfDirectLabor;
    public double BenefitLoad { get; private set; } = 0.0;

    private Employee(Person personalDetails,
        List<RoleAssignment> roleAssignments,
        SSN ssn,
        DateTime hired,
        string notes,
        string certificationNumber,
        bool active,
        string printedName,
        EmployeeExpenseCategory expenseCategory,
        double benefitLoad)
    {
        PersonalDetails = personalDetails;
        SSN = ssn;
        Hired = hired;
        Notes = notes;
        CertificationNumber = certificationNumber;
        Active = active;
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

    public static Result<Employee> Create(Person hiredPerson,
        List<RoleAssignment> roleAssignments,
        SSN ssn,
        DateTime hired,
        string? notes = null,
        string? certificationNumber = null,
        bool active = true,
        string? printedName = null,
        EmployeeExpenseCategory expenseCategory = EmployeeExpenseCategory.CostOfDirectLabor,
        double benefitLoad = 0.0)
    {
        if (hiredPerson is null)
            return Result.Failure<Employee>(RequiredMessage);

        if (hired < StartDateMinimum || hired > EndDateMaximum)
            return Result.Failure<Employee>(DateRangeMessage);

        notes = (notes ?? string.Empty)
            .Trim()
            .Truncate(MaximumNoteLength);

        var certificationNumberValidationResult = ValidateCertificationNumber(certificationNumber);
        if (certificationNumberValidationResult.IsFailure)
            return Result.Failure<Employee>(certificationNumberValidationResult.Error);
        certificationNumber = certificationNumber?.Trim();

        var printedNameValidationResult = ValidatePrintedName(printedName);
        if (printedNameValidationResult.IsFailure)
            return Result.Failure<Employee>(printedNameValidationResult.Error);
        printedName = printedName?.Trim();

        var expenseCategoryValidationResult = ValidateExpenseCategory(expenseCategory);
        if (expenseCategoryValidationResult.IsFailure)
            return Result.Failure<Employee>(expenseCategoryValidationResult.Error);

        var benefitLoadValidationResult = ValidateBenefitLoad(benefitLoad);
        if (benefitLoadValidationResult.IsFailure)
            return Result.Failure<Employee>(benefitLoadValidationResult.Error);

        return Result.Success(new Employee(hiredPerson, roleAssignments, ssn, hired, notes.Trim(), certificationNumber!, active, printedName!, expenseCategory, benefitLoad));
    }

    private static Result ValidateSSN(string? ssn)
    {
        if (string.IsNullOrWhiteSpace(ssn))
            return Result.Success();

        return ssn.Trim().Length <= MaximumSSNLength
            ? Result.Success()
            : Result.Failure(InvalidMaximumLengthMessage(MaximumSSNLength));
    }

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
        return hired < StartDateMinimum
            ? Result.Failure<DateTime>(DateRangeMessage)
            : Result.Success((DateTime)(Hired = hired));
    }

    public Result<DateTime> SetExited(DateTime exited)
    {
        return exited > EndDateMaximum
            ? Result.Failure<DateTime>(DateRangeMessage)
            : Result.Success((DateTime)(Hired = exited));
    }

    public Result<string> SetNotes(string notes) =>
        Result.Success(Notes = notes
            .Trim()
            .Truncate(MaximumNoteLength));

    public Result<SSN> SetSSN(SSN ssn) =>
        Result.Success(SSN = ssn);

    public Result<string> SetCertificationNumber(string certificationNumber)
    {
        if (string.IsNullOrWhiteSpace(certificationNumber))
        {
            return Result.Success(CertificationNumber = string.Empty);
        }

        return certificationNumber.Length <= MaximumCertificationNumberLength
            ? Result.Success(CertificationNumber = certificationNumber)
            : Result.Failure<string>(InvalidMaximumLengthMessage(MaximumCertificationNumberLength));
    }

    public Result<bool> SetActive(bool active = true)
    {
        return Result.Success(Active = active);
    }

    public Result<string> SetPrintedName(string printedName)
    {
        if (string.IsNullOrWhiteSpace(printedName))
        {
            return Result.Success(PrintedName = string.Empty);
        }

        return printedName.Length <= MaximumPrintedNameLength
            ? Result.Success(PrintedName = printedName)
            : Result.Failure<string>(InvalidMaximumLengthMessage(MaximumPrintedNameLength));
    }

    public Result<EmployeeExpenseCategory> SetExpenseCategory(EmployeeExpenseCategory expenseCategory)
    {
        return Enum.IsDefined(typeof(EmployeeExpenseCategory), expenseCategory)
            ? Result.Success(ExpenseCategory = expenseCategory)
            : Result.Failure<EmployeeExpenseCategory>(InvalidExpenseCategoryMessage);
    }

    public Result<double> SetBenefitLoad(double benefitLoad)
    {
        return benefitLoad >= MinimumBenefitLoad && benefitLoad <= MaximumBenefitLoad
            ? Result.Success(BenefitLoad = benefitLoad)
            : Result.Failure<double>(BenefitLoadMessage);
    }

    // EF requires a parameterless constructor
    protected Employee()
    {
        roleAssignments = [];
    }
}
