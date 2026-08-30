# Stocktrac Test Naming Convention

This document explains how to name unit tests in Stocktrac. The goal is to make every fully qualified test name read like a short sentence that describes the expected behavior.

## The convention

Use an outcome-first name with this structure:

```text
{DomainType}Should.{ExpectedBehavior}_On_{MemberUnderTest}_{Scenario}
```

| Part | Purpose | Example |
| --- | --- | --- |
| `{DomainType}Should` | Names the test class and the domain type being tested. | `EmailShould` |
| `{ExpectedBehavior}` | States what the type should do. | `ReturnFailureResult` |
| `On` | Introduces the member or operation under test. | `On` |
| `{MemberUnderTest}` | Uses the exact method, property, or operation name when practical. | `Create` |
| `{Scenario}` | Describes the condition that causes the behavior. It usually starts with `When`. | `WhenAddressIsNull` |

For example:

```text
EmailShould.ReturnFailureResult_On_Create_WhenAddressIsNull
```

Read the name as a sentence:

> Email should return failure result on Create when address is null.

This order is intentional. We value the readability of the complete test name over alphabetical grouping by the member under test.

## Writing a test name step by step

Suppose you are testing this behavior:

> Calling `Email.Create` with a null address returns a failure result.

### 1. Identify the domain type

The domain type is `Email`, so name the test class:

```csharp
public class EmailShould
{
}
```

The word `Should` begins the sentence formed by the fully qualified test name.

### 2. State the expected behavior

Ask: **What should Email do?**

```text
ReturnFailureResult
```

Use an active verb phrase such as `Return`, `Reject`, `Preserve`, `Normalize`, `Contain`, or `Update`.

### 3. Identify the member under test

The method is `Create`, so add:

```text
_On_Create
```

`On` is the default connector. In this convention, it means "when this operation is performed" and keeps the member under test visually distinct.

### 4. Describe the scenario

Ask: **Under what condition should this happen?**

```text
_WhenAddressIsNull
```

Name the relevant value precisely. Here, the `address` argument is null—not the `Email` object—so `WhenAddressIsNull` is clearer than `WhenEmailIsNull`.

### 5. Combine the parts

```csharp
public class EmailShould
{
    [Fact]
    public void ReturnFailureResult_On_Create_WhenAddressIsNull()
    {
        var result = Email.Create(address: null!, isPrimary: true);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Email.EmptyMessage);
    }
}
```

The fully qualified name is:

```text
EmailShould.ReturnFailureResult_On_Create_WhenAddressIsNull
```

## Formatting rules

### Use PascalCase inside each phrase

Do:

```text
ReturnFailureResult_On_Create_WhenAddressIsNull
```

Do not separate every word:

```text
Return_Failure_Result_On_Create_With_Null_Address
```

### Use underscores to separate semantic sections

The underscores are structural markers, not word separators:

```text
ReturnFailureResult _ On _ Create _ WhenAddressIsNull
| expected behavior | member | scenario |
```

This makes the outcome, member, and scenario easy to recognize at a glance.

### Prefer a precise scenario beginning with `When`

Good scenarios explain the relevant state or condition:

```text
WhenAddressIsNull
WhenCurrenciesDiffer
WhenResultExceedsDecimalRange
WhenCurrentCultureUsesComma
```

Avoid vague scenarios when a more precise description is available:

```text
WhenInvalid       // What is invalid?
WhenItFails       // What causes the failure?
WhenCalled        // Usually adds no useful information.
```

### Keep the member name exact when practical

If the production member is `SetAddress`, use:

```text
_On_SetAddress_
```

Exact names help a reader connect the test to the production API.

## Examples

```text
EmailShould.ReturnFailureResult_On_Create_WhenAddressIsNull
EmailShould.TrimAddress_On_Create_WhenAddressContainsSurroundingWhitespace
EmailShould.UpdateAddress_On_SetAddress_WhenAddressIsValid
AmountShould.ContainExactValue_On_FromDecimal_WhenGivenAnyDecimal
AmountShould.ReturnOverflowFailure_On_Add_WhenResultExceedsDecimalRange
MoneyShould.ReturnCurrencyMismatchFailure_On_Add_WhenCurrenciesDiffer
MoneyShould.PreserveCurrency_On_Multiply_WhenResultIsInRange
```

## Adapt the sentence when necessary

`On` is the default connector, not an inflexible rule. A test name must remain accurate and natural. Replace or omit a section when forcing the template would make the name misleading.

### Tests of state after an operation

`After` may describe a post-operation state more accurately:

```text
EmailShould.ContainUpdatedAddress_After_SetAddress_WhenAddressIsValid
```

Use `After` only when the timing or resulting state matters. For a value returned directly by a method, `On` is usually clearer.

### Tests without a meaningful scenario

Do not add `WhenCalled` merely to fill the template:

```text
AmountShould.ReturnInvariantRepresentation_On_ToString
```

### Tests of a broader contract

Some tests cover a language or domain contract rather than one member. Omit the member section if naming one member would be artificial:

```text
AmountShould.BeEqualAndHaveMatchingHashCodes_WhenValuesAreEqual
```

### Choose a behavior-specific connector when it improves clarity

Occasionally `From`, `After`, or another connector produces a more accurate sentence. Use that alternative only when its meaning is clearer than `On` and the full test name still reads naturally.

## Common mistakes

### Starting with the member

Avoid:

```text
EmailShould.Create_WhenAddressIsNull_ReturnsFailureResult
```

Read as a sentence, this becomes "Email should Create when address is null returns failure result." Put the expected behavior immediately after `Should` instead.

### Repeating information

Avoid unnecessarily long outcomes:

```text
AmountShould.ReturnAmountContainingExactValue_On_FromDecimal_WhenGivenAnyDecimal
```

Prefer a shorter behavioral phrase:

```text
AmountShould.ContainExactValue_On_FromDecimal_WhenGivenAnyDecimal
```

### Describing implementation instead of behavior

Prefer what a caller can observe:

```text
EmailShould.ReturnFailureResult_On_Create_WhenAddressIsNull
```

Avoid internal details unless those details are themselves part of the required contract:

```text
EmailShould.CallValidationHelper_On_Create_WhenAddressIsNull
```

### Combining unrelated behaviors

A name containing several unrelated outcomes often indicates that the test should be split. Each test should normally explain one behavior under one scenario.

## Review checklist

Before committing a test, verify:

- [ ] The class is named `{DomainType}Should`.
- [ ] The fully qualified name begins as a sentence: "Domain type should..."
- [ ] The expected behavior comes before the member under test.
- [ ] The expected behavior uses a clear, active verb phrase.
- [ ] Underscores separate semantic sections, not individual words.
- [ ] The member name matches the production API when practical.
- [ ] The scenario identifies the relevant condition precisely.
- [ ] The full name is accurate when read aloud.
- [ ] Optional sections have been omitted or adapted when they would make the sentence awkward.
- [ ] The test checks the behavior promised by its name.

## Quick reference

Default template:

```text
{DomainType}Should.{ExpectedBehavior}_On_{MemberUnderTest}_{Scenario}
```

Typical example:

```text
EmailShould.ReturnFailureResult_On_Create_WhenAddressIsNull
```

The most important rule is simple: **read the fully qualified test name aloud. It should sound like a clear sentence describing the behavior.**
