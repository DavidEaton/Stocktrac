You are refactoring a large C# domain model designed according to Domain-Driven Design principles.

The highest architectural priorities are:

1. Make invalid domain states unrepresentable.
2. Minimize accidental complexity.
3. Prefer functional programming techniques where they improve clarity and correctness.
4. Preserve strong encapsulation of all domain objects and invariants.
5. Use the C# 15 `union` language feature wherever it genuinely produces a simpler, more expressive, more type-safe domain model.
6. Prefer compile-time correctness over runtime validation whenever possible.
7. Preserve domain terminology and make the code read as closely as practical to the ubiquitous language.

Do NOT mechanically replace existing types with unions. A union should be introduced only when the domain concept represents a closed set of mutually exclusive alternatives.

Before modifying anything, inspect the entire domain project, its tests, project files, compiler/language-version configuration, and all usages of the affected domain types.

First verify that this repository's compiler and language version actually support the C# 15 `union` syntax. Do not invent syntax, emulate unions manually, or silently substitute a third-party discriminated-union implementation. If the compiler does not support the required feature, clearly report that before attempting the refactoring.

## Primary objective

Refactor the domain model to exploit C# 15 unions where doing so:

* eliminates invalid states;
* eliminates nullable properties that exist only for particular states;
* eliminates Boolean combinations representing mutually exclusive states;
* eliminates `enum` + optional-data combinations;
* eliminates discriminator properties followed by switching on that discriminator;
* replaces closed inheritance hierarchies whose primary purpose is representing alternatives;
* eliminates casts, `is` checks, defensive `default` branches, or "should never happen" exceptions;
* improves exhaustive pattern matching;
* reduces branching and state-management complexity;
* more accurately represents the domain;
* allows the compiler to enforce invariants currently enforced procedurally.

Look especially for structures resembling:

&#x20;   enum Type;
    SomeValue? ValueForTypeA;
    OtherValue? ValueForTypeB;


or:

&#x20;   bool IsFoo;
    bool IsBar;


or:

&#x20;   Status Status;
    Maybe<X> X;
    Maybe<Y> Y;


where only particular combinations are valid.

Also look for abstract classes or interfaces with a known, closed set of implementations where callers repeatedly pattern-match or switch over the implementations.

Those are strong candidates for unions.

For example, prefer modeling:

&#x20;   Payment = CashPayment | CreditCardPayment | CheckPayment


over a representation where one `Payment` object contains a payment-type enum plus several nullable properties.

## Domain modeling rules

Treat every aggregate, entity, value object, and union as part of a domain model, not merely a data structure.

Preserve aggregate boundaries.

Do not expose mutable state merely to make the refactoring easier.

Do not weaken existing invariants.

Do not add public setters.

Prefer immutable records/value objects when appropriate.

Construction must leave every domain object valid.

If construction can fail because input is invalid, continue using an explicit functional result such as `Result<T>` rather than constructing an invalid object and validating it afterward.

Do not use exceptions for ordinary domain validation or expected business-rule failures.

Exceptions should represent genuinely exceptional or programmer-error conditions.

Do not introduce `null` as an alternative-state mechanism.

Existing `Maybe<T>` should continue to represent genuine optionality.

Existing `Result<T>` should continue to represent an operation that can succeed or fail.

Do NOT replace `Maybe<T>` or `Result<T>` with a union merely because both can conceptually be described as alternatives.

Distinguish these concepts carefully:

* `Maybe<T>` = a value may legitimately be absent.
* `Result<T>` = an operation may succeed or fail.
* domain union = a domain value has one of a closed set of legitimate forms.

Use each abstraction for the problem it actually represents.

## Functional-programming goals

Prefer:

* immutable data;
* pure transformations;
* expression-oriented code;
* total functions;
* explicit data flow;
* exhaustive pattern matching;
* composition;
* small functions;
* value objects;
* `Result<T>` / `Maybe<T>` pipelines where already appropriate;
* transformations that return new valid values rather than partially mutating existing objects.

Avoid:

* hidden mutation;
* partially initialized objects;
* temporal coupling;
* state flags;
* Boolean parameters controlling unrelated behavior;
* null-driven control flow;
* downcasting;
* reflection-based domain behavior;
* catch-all `default` cases that hide missing domain cases;
* unnecessary inheritance;
* primitive obsession.

Do not pursue functional style at the expense of readability. A simple direct expression is preferable to an elaborate functional pipeline that obscures intent.

## Union design rules

When introducing a union:

1. Give each case a domain-specific name.
2. Put data that belongs exclusively to a case on that case.
3. Do not place case-specific nullable properties on the containing union.
4. Use exhaustive pattern matching.
5. Do not add `\_ => throw ...` merely to silence exhaustiveness checking when all cases are known.
6. Expose common domain behavior through the union when doing so simplifies callers.
7. Keep case-specific behavior on the case when it belongs there.
8. Do not expose the internal representation unnecessarily.
9. Do not create a union when ordinary composition or a single value object expresses the concept more clearly.
10. Prefer two or more semantically meaningful cases; do not create ceremonial one-case abstractions.

Where appropriate, move logic like this:

&#x20;   if (type == ...)
    else if (type == ...)
    else ...


or:

&#x20;   switch (someEnum)


into exhaustive matching over the domain union itself.

The goal is not fewer lines of code. The goal is fewer possible states and fewer concepts the programmer must keep synchronized mentally.

## Invalid-state analysis

For every candidate refactoring, explicitly ask:

"What invalid states can currently be constructed?"

Examples include:

* a discriminator says A while B-specific data is populated;
* a required property for the selected state is missing;
* two mutually exclusive flags are simultaneously true;
* no mutually exclusive state is selected;
* properties that only make sense in one lifecycle state are available in another;
* an enum can have a value without the data required by that value.

Where a union can structurally eliminate such states, prefer that representation.

The strongest desired outcome is code where those states cannot compile or cannot be constructed.

## Existing domain types

Respect the project's existing functional abstractions and conventions.

In particular, if the project already uses constructs such as:

&#x20;   Result<T>
    Maybe<T>
    Ensure(...)
    Map(...)
    Bind(...)
    Tap(...)


preserve them when they remain appropriate.

Do not introduce a second competing functional abstraction without a compelling reason.

Likewise, retain existing strongly typed value objects instead of replacing them with primitives merely to make union declarations shorter.

For example, preserve concepts such as:

&#x20;   EmailAddress
    PhoneNumber
    PostalCode
    Address
    DateTimeRange
    CurrencyCode


when those types enforce meaningful invariants.

## Refactoring process

Work systematically.

### Phase 1 — Analyze

Inventory the domain model.

Identify:

* aggregates;
* entities;
* value objects;
* inheritance hierarchies;
* enums;
* state machines;
* nullable or optional state-dependent properties;
* Boolean state flags;
* discriminated structures;
* Result/Maybe usage;
* switches and pattern matching;
* places where callers must understand internal state combinations.

Produce a concise list of union candidates.

For each candidate explain:

* current representation;
* invalid states currently representable;
* proposed union and cases;
* complexity removed;
* whether the change should or should not be made.

Do not modify code yet during this analysis.

### Phase 2 — Prioritize

Rank candidates according to:

1. invalid states eliminated;
2. complexity removed;
3. improvement in domain expressiveness;
4. improvement in exhaustive compiler checking;
5. reduction in caller knowledge/coupling;
6. refactoring risk.

Reject candidates where a union would merely make the implementation more fashionable rather than simpler.

### Phase 3 — Refactor

Perform the refactoring incrementally.

For each union:

* introduce the union and its cases;
* migrate construction;
* migrate behavior;
* migrate callers;
* replace discriminator-based control flow with exhaustive matching;
* remove obsolete states/properties/enums/classes;
* update validation;
* update tests.

Do not leave both the old representation and new union representation in parallel unless temporarily required during a compilable intermediate step.

### Phase 4 — Simplify

After each migration, look for code that has become unnecessary:

* null checks;
* guards;
* enum validation;
* defensive exceptions;
* state synchronization;
* duplicate validation;
* casts;
* redundant interfaces;
* redundant factory overloads;
* Boolean state checks;
* impossible-case branches.

Remove complexity made obsolete by the type system.

Do not merely layer unions on top of the old design.

### Phase 5 — Verify

Build the complete solution.

Run the complete automated test suite.

Fix all compilation errors and test failures caused by the refactoring.

Add or revise tests to demonstrate:

* each legitimate union case;
* domain behavior for each case;
* construction invariants;
* previously representable invalid states that are now impossible;
* exhaustive handling where appropriate.

Prefer testing observable domain behavior over implementation details.

## Architectural restraint

Do not refactor unrelated infrastructure, persistence, application services, endpoints, or user-interface code except where compilation or adaptation to the changed domain API requires it.

Do not change established domain terminology without a strong reason.

Do not create abstractions solely to reduce line count.

Do not introduce generic "Union<T1,T2>" wrappers when a named domain union communicates the concept better.

Prefer:

&#x20;   Tax
    Payment
    ContactMethod
    EmploymentStatus


over:

&#x20;   Union<A, B>


when the union itself represents a domain concept.

## Persistence considerations

Inspect how affected domain objects are persisted.

Do not assume the object-relational mapper, serializer, or database layer automatically understands C# unions.

Keep persistence concerns outside the domain model where possible.

If persistence requires mapping between a domain union and a storage representation, create an explicit mapping at the persistence boundary rather than contaminating the domain model with persistence-specific state.

Do not weaken the domain model merely to accommodate persistence.

## API compatibility

Preserve public APIs where reasonable, but do not preserve a poor API when doing so defeats the purpose of making invalid states unrepresentable.

When an API-breaking change materially improves the domain model, make the change and update callers.

Document significant breaking changes in the final report.

## Complexity criterion

For every proposed abstraction, evaluate:

&#x20;   Does this reduce the number of states, branches, rules, or relationships that a programmer must hold in their head?


If not, do not introduce it.

The desired result is not "maximum use of unions."

The desired result is:

&#x20;   maximum domain expressiveness
    + maximum compile-time correctness
    + minimum representable invalid state
    + minimum accidental complexity.


## Final review

When the refactoring is complete, perform another pass over the entire domain project and look specifically for opportunities exposed by the new design.

Ask:

* Can another enum + data structure now become a union?
* Can an impossible-state guard now disappear?
* Can a nullable property disappear?
* Can an inheritance hierarchy become simpler?
* Can a caller stop knowing which concrete case it received?
* Can duplicated branching move into the union?
* Can pattern matching become exhaustive?
* Can a factory become simpler?
* Can a domain rule move from runtime validation into the type system?
* Did any union increase complexity rather than reduce it?

Revert or redesign any union that made the model harder to understand.

## Final deliverable

At completion, provide a report containing:

1. Every union introduced and its cases.
2. The domain concept each union represents.
3. The invalid states eliminated by each change.
4. Major classes/enums/properties removed.
5. Important API changes.
6. Any union candidates considered but deliberately rejected, and why.
7. Remaining areas where invalid states can still be represented.
8. Build result.
9. Test result.
10. Any compiler/language-version limitations encountered.

Most importantly: use judgment.

Do not optimize for the number of unions introduced.

Optimize for a domain model in which the compiler carries as much of the correctness burden as reasonably possible and developers have as little state-management complexity as possible.

First verify that this repository's compiler and language version actually support the C# 15 `union` syntax. Do not invent syntax, emulate unions manually, or silently substitute a third-party discriminated-union implementation. If the compiler does not support the required feature, clearly report that before attempting the refactoring.

