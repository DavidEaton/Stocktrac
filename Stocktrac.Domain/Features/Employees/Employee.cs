using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Domain.Features.Employees;

public sealed class Employee : Entity
{
    // TODO: Move these constants to user-configurable settings in the future.
    // For now, they are hard-coded to match the current validation rules in StockTrac.
    public static DateTime StartDateMinimum(DateTime date) =>
        date.Year > 50 ? date.Date.AddYears(-50) : DateTime.MinValue;
    public static DateTime EndDateMaximum(DateTime date) =>
        date.Year < 9999 ? date.Date.AddYears(1) : DateTime.MaxValue.Date;
    public const int MaximumNoteLength = 10000;
    public const int MaximumSSNLength = 12;
    public const int MaximumCertificationNumberLength = 20;
    public const int MaximumPrintedNameLength = 50;
    public static readonly double MinimumBenefitLoad = 0.0;
    public static readonly double MaximumBenefitLoad = 100.0;
    public const string RequiredMessage = "Please include all required items.";
    public const string OptionalTextRequiredMessage = "Use the remove operation to clear an optional value.";
    public const string DateRangeMessage = "Employment date(s) invalid.";
    public const string ActiveRoleAssignmentRequiredMessage = "An active employee must have at least one active role assignment.";
    public const string DuplicateRoleAssignmentMessage = "Each role assignment must be unique.";
    public const string RoleAssignmentNotFoundMessage = "Role assignment not found.";
    public const string InvalidExpenseCategoryMessage = "Expense category is invalid.";
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

    public Result<RoleAssignment> AddRoleAssignment(RoleAssignment assignment, DateTime date)
    {
        if (assignment is null)
            return Result.Failure<RoleAssignment>(RequiredMessage);

        return ReplaceRoleAssignments([.. roleAssignments, assignment], date)
            .Map(() => assignment);
    }

    public Result ReplaceRoleAssignments(IReadOnlyList<RoleAssignment> assignments, DateTime date) =>
        ValidateRoleAssignments(assignments, Active, date)
            .Tap(() =>
            {
                var replacement = assignments.ToArray();
                roleAssignments.Clear();
                roleAssignments.AddRange(replacement);
            });

    public Result<RoleAssignment> RemoveRoleAssignment(RoleAssignment assignment, DateTime date)
    {
        if (assignment is null)
            return Result.Failure<RoleAssignment>(RequiredMessage);

        if (!roleAssignments.Contains(assignment))
            return Result.Failure<RoleAssignment>(RoleAssignmentNotFoundMessage);

        return ReplaceRoleAssignments(roleAssignments.Where(value => value != assignment).ToArray(), date)
            .Map(() => assignment);
    }

    public Result ValidateRoleAssignments(DateTime date) =>
        ValidateRoleAssignments(roleAssignments, Active, date);

    private static Result ValidateRoleAssignments(
        IReadOnlyList<RoleAssignment> assignments, bool active, DateTime date)
    {
        if (assignments is null || assignments.Any(assignment => assignment is null))
            return Result.Failure(RequiredMessage);

        if (assignments.Distinct().Count() != assignments.Count)
            return Result.Failure(DuplicateRoleAssignmentMessage);

        return Result.FailureIf(
            active && !assignments.Any(assignment => assignment.IsActive(date)),
            ActiveRoleAssignmentRequiredMessage);
    }

    public static Result<Employee> Create(
        Person hiredPerson,
        IReadOnlyList<RoleAssignment> roleAssignments,
        SSN ssn,
        DateTime hired,
        Note notes,
        DateTime date,
        Maybe<string> certificationNumber = default,
        Maybe<string> printedName = default,
        EmployeeExpenseCategory expenseCategory = EmployeeExpenseCategory.CostOfDirectLabor,
        double benefitLoad = 0.0)
    {
        var normalizedCertificationNumber = certificationNumber.Map(value => value.Trim());
        var normalizedPrintedName = printedName.Map(value => value.Trim());

        return Result.Combine(
                Environment.NewLine,
                Result.FailureIf(hiredPerson is null, RequiredMessage),
                ValidateRoleAssignments(roleAssignments, true, date),
                Result.FailureIf(ssn is null, RequiredMessage),
                Result.FailureIf(notes is null, RequiredMessage),
                Result.FailureIf(!IsEmploymentDateWithinAllowedRange(hired, date), DateRangeMessage),
                ValidateOptionalText(normalizedCertificationNumber, MaximumCertificationNumberLength),
                ValidateOptionalText(normalizedPrintedName, MaximumPrintedNameLength),
                ValidateExpenseCategory(expenseCategory),
                ValidateBenefitLoad(benefitLoad))
            .Map(() => new Employee(
                hiredPerson!,
                roleAssignments!,
                ssn!,
                hired,
                notes!,
                normalizedCertificationNumber,
                normalizedPrintedName,
                expenseCategory,
                benefitLoad));
    }

    private static Result ValidateOptionalText(Maybe<string> value, int maximumLength)
    {
        if (value.HasNoValue)
            return Result.Success();

        if (string.IsNullOrWhiteSpace(value.Value))
            return Result.Failure(OptionalTextRequiredMessage);

        return value.Value.Length <= maximumLength
            ? Result.Success()
            : Result.Failure(InvalidMaximumLengthMessage(maximumLength));
    }

    private static Result ValidateExpenseCategory(EmployeeExpenseCategory expenseCategory)
    {
        return Enum.IsDefined(expenseCategory)
            ? Result.Success()
            : Result.Failure<EmployeeExpenseCategory>(InvalidExpenseCategoryMessage);
    }

    private static Result ValidateBenefitLoad(double benefitLoad)
    {
        return double.IsFinite(benefitLoad) && benefitLoad >= MinimumBenefitLoad && benefitLoad <= MaximumBenefitLoad
            ? Result.Success()
            : Result.Failure<double>(BenefitLoadMessage);
    }

    public Result<DateTime> UpdateHired(DateTime hired, DateTime date)
    {
        if (!IsEmploymentDateWithinAllowedRange(hired, date))
        {
            return Result.Failure<DateTime>(DateRangeMessage);
        }

        if (Exited.HasValue && hired > Exited.Value)
        {
            return Result.Failure<DateTime>(DateRangeMessage);
        }

        return ValidateRoleAssignments(date)
            .Map(() => Hired = hired);
    }

    public Result<DateTime> UpdateExited(DateTime exited, DateTime date) =>
        Result.Success(exited)
            .Ensure(value => IsEmploymentDateWithinAllowedRange(value, date), DateRangeMessage)
            .Ensure(value => value >= Hired, DateRangeMessage)
            .Tap(value => Exited = value);

    public Result RemoveExited(DateTime date) =>
        ValidateRoleAssignments(roleAssignments, true, date)
            .Tap(() => Exited = Maybe<DateTime>.None);

    private static bool IsEmploymentDateWithinAllowedRange(DateTime employmentDate, DateTime date) =>
        employmentDate >= StartDateMinimum(date) &&
        employmentDate <= EndDateMaximum(date);

    public Result UpdateNotes(Note notes) =>
        notes is null
            ? Result.Failure(RequiredMessage)
            : Result.Success().Tap(() => Notes = notes);

    public void RemoveNotes() => Notes = Maybe<Note>.None;

    public Result UpdateSSN(SSN ssn) =>
        ssn is null ? Result.Failure(RequiredMessage) : Result.Success().Tap(() => SSN = ssn);

    public Result UpdateCertificationNumber(string certificationNumber)
    {
        certificationNumber = certificationNumber?.Trim() ?? string.Empty;

        return ValidateOptionalText(certificationNumber, MaximumCertificationNumberLength)
            .Tap(() => CertificationNumber = certificationNumber);
    }

    public void RemoveCertificationNumber() => CertificationNumber = Maybe<string>.None;

    public Result<Maybe<string>> UpdatePrintedName(string printedName)
    {
        printedName = printedName?.Trim() ?? string.Empty;

        return ValidateOptionalText(printedName, MaximumPrintedNameLength)
            .Map(() => PrintedName = printedName);
    }

    public void RemovePrintedName() => PrintedName = Maybe<string>.None;

    public Result<EmployeeExpenseCategory> UpdateExpenseCategory(EmployeeExpenseCategory expenseCategory) =>
        Enum.IsDefined(expenseCategory)
            ? Result.Success(ExpenseCategory = expenseCategory)
            : Result.Failure<EmployeeExpenseCategory>(InvalidExpenseCategoryMessage);

    public Result<double> UpdateBenefitLoad(double benefitLoad) =>
        double.IsFinite(benefitLoad) && benefitLoad >= MinimumBenefitLoad && benefitLoad <= MaximumBenefitLoad
            ? Result.Success(BenefitLoad = benefitLoad)
            : Result.Failure<double>(BenefitLoadMessage);
}
