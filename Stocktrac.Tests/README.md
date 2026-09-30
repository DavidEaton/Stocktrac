# Test naming

Names should read as short, outcome-first sentences:

```text
{DomainType}Should.{ExpectedBehavior}_On_{Member}_{Scenario}
```

Example:

```text
EmailAddressShould.ReturnFailureResult_On_Create_WhenValueIsNull
```

## Rules

- Name the class `{DomainType}Should`.
- Put observable behavior before the member under test.
- Use PascalCase within phrases and underscores between phrases.
- Match the production member name when practical.
- Prefer a precise `When...` scenario; omit it if none adds value.
- Use a clearer connector such as `After` when needed.
- Test concrete types, not abstract base classes.
- Keep one behavior per test and avoid implementation details.

Read the full name aloud before committing. If it is not a clear sentence,
shorten or restructure it.
