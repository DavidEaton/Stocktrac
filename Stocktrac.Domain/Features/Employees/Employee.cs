using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Domain.Features.Employees;

public sealed class Employee : Entity
{
    public const int MaximumNoteLength = 10000;
    public const int MaximumSSNLength = 12;
    public const int MaximumCertificationNumberLength = 20;
    public const int MaximumPrintedNameLength = 50;
    public static readonly double MinimumBenefitLoad = 0.0;
    public static readonly double MaximumBenefitLoad = 100.0;
    public const string RequiredMessage = "Please include all required items.";
    public const string OptionalTextRequiredMessage = "Use the remove operation to clear an optional value.";
    public const string ActiveRoleAssignmentRequiredMessage = "An active employee must have at least one active role assignment.";
    public const string DuplicateRoleAssignmentMessage = "Each role assignment must be unique.";
    public const string RoleAssignmentNotFoundMessage = "Role assignment not found.";
    public const string InvalidExpenseCategoryMessage = "Expense category is invalid.";
    public static readonly string BenefitLoadMessage = $"Benefit load must be between {MinimumBenefitLoad} and {MaximumBenefitLoad}.";
    public static string InvalidMaximumLengthMessage(int max) => $"Value must be less than {max} characters in length.";
    public Person EmployedPerson { get; private set; }
    public IReadOnlyList<RoleAssignment> RoleAssignments => [.. roleAssignments];
    private readonly List<RoleAssignment> roleAssignments = [];
    public Maybe<Note> Notes { get; private set; }
    public SSN SSN { get; private set; }
    public Maybe<string> CertificationNumber { get; private set; } // TODO: This should be defined and probably a value object
    public EmploymentPeriod PeriodEmployed { get; private set; }
    public bool Active => PeriodEmployed.Active;
    public Maybe<string> PrintedName { get; private set; } // TTODO: his should be defined and probably a value object
    public EmployeeExpenseCategory ExpenseCategory { get; private set; } = EmployeeExpenseCategory.CostOfDirectLabor;
    public double BenefitLoad { get; private set; } = 0.0; // TODO: This should be defined and probably a value object 

    private Employee(Person personEmployed,
        IReadOnlyList<RoleAssignment> roleAssignments,
        SSN ssn,
        EmploymentPeriod periodEmployed,
        Note notes,
        Maybe<string> certificationNumber,
        Maybe<string> printedName,
        EmployeeExpenseCategory expenseCategory,
        double benefitLoad)
    {
        EmployedPerson = personEmployed;
        SSN = ssn;
        PeriodEmployed = periodEmployed;
        Notes = notes;
        CertificationNumber = certificationNumber;
        PrintedName = printedName;
        ExpenseCategory = expenseCategory;
        BenefitLoad = benefitLoad;

        this.roleAssignments.AddRange(roleAssignments);
    }

    public Result<RoleAssignment> AddRoleAssignment(RoleAssignment assignment, DateOnly date)
    {
        if (assignment is null)
            return Result.Failure<RoleAssignment>(RequiredMessage);

        return ReplaceRoleAssignments([.. roleAssignments, assignment], date)
            .Map(() => assignment);
    }

    public Result ReplaceRoleAssignments(IReadOnlyList<RoleAssignment> assignments, DateOnly date) =>
        ValidateRoleAssignments(assignments, Active, date)
            .Tap(() =>
            {
                var replacement = assignments.ToArray();
                roleAssignments.Clear();
                roleAssignments.AddRange(replacement);
            });

    public Result<RoleAssignment> RemoveRoleAssignment(RoleAssignment assignment, DateOnly date)
    {
        if (assignment is null)
            return Result.Failure<RoleAssignment>(RequiredMessage);

        if (!roleAssignments.Contains(assignment))
            return Result.Failure<RoleAssignment>(RoleAssignmentNotFoundMessage);

        return ReplaceRoleAssignments(roleAssignments.Where(value => value != assignment).ToArray(), date)
            .Map(() => assignment);
    }

    public Result ValidateRoleAssignments(DateOnly date) =>
        ValidateRoleAssignments(roleAssignments, Active, date);

    private static Result ValidateRoleAssignments(
        IReadOnlyList<RoleAssignment> assignments, bool active, DateOnly date)
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
        DateOnly hired,
        Note notes,
        DateOnly date,
        Maybe<string> certificationNumber = default,
        Maybe<string> printedName = default,
        EmployeeExpenseCategory expenseCategory = EmployeeExpenseCategory.CostOfDirectLabor,
        double benefitLoad = 0.0)
    {
        var normalizedCertificationNumber = certificationNumber.Map(value => value.Trim());
        var normalizedPrintedName = printedName.Map(value => value.Trim());

        return Result.Success()
            .Ensure(() => hiredPerson is not null, RequiredMessage)
            .Bind(() => ValidateRoleAssignments(roleAssignments, true, date))
            .Ensure(() => ssn is not null, RequiredMessage)
            .Ensure(() => notes is not null, RequiredMessage)
            .Bind(() => EmploymentPeriod.Create(hired, date))
            .Bind(period => ValidateOptionalText(normalizedCertificationNumber, MaximumCertificationNumberLength)
                .Bind(() => ValidateOptionalText(normalizedPrintedName, MaximumPrintedNameLength))
                .Bind(() => ValidateExpenseCategory(expenseCategory))
                .Bind(() => ValidateBenefitLoad(benefitLoad))
                .Map(() => new Employee(
                    hiredPerson!,
                    roleAssignments!,
                    ssn!,
                    period,
                    notes!,
                    normalizedCertificationNumber,
                    normalizedPrintedName,
                    expenseCategory,
                    benefitLoad)));
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

    public Result<DateOnly> ReplaceHired(DateOnly hired, DateOnly date) =>
        PeriodEmployed.ReplaceHired(hired, date)
            .Bind(period => ValidateRoleAssignments(date).Map(() => period))
            .Tap(period => PeriodEmployed = period)
            .Map(period => period.Hired);

    public Result<DateOnly> ReplaceExited(DateOnly exited, DateOnly date) =>
        PeriodEmployed.ReplaceExited(exited, date)
            .Tap(period => PeriodEmployed = period)
            .Map(period => period.Exited.Value);

    public Result RemoveExited(DateOnly date) =>
        ValidateRoleAssignments(roleAssignments, true, date)
            .Tap(() => PeriodEmployed = PeriodEmployed.RemoveExited());

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
