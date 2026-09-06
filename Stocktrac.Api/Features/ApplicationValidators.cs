using CSharpFunctionalExtensions;
using FluentValidation;
using Stocktrac.Domain.Features;

namespace Stocktrac.Api.Features;

public static class ApplicationValidators
{
    public static IRuleBuilderOptionsConditions<T, TElement>
        MustSatisfyFactory<T, TElement, TResult>(
            this IRuleBuilder<T, TElement> ruleBuilder,
            Func<TElement, Result<TResult>> factoryMethod) =>
        ruleBuilder.Custom((value, context) =>
        {
            var result = factoryMethod(value);

            if (result.IsFailure)
                context.AddFailure(result.Error);
        });

    public static IRuleBuilderOptionsConditions<T, TElement>
        MustBeEntity<T, TElement, TEntity>(
            this IRuleBuilder<T, TElement> ruleBuilder,
            Func<TElement, Result<TEntity>> factoryMethod)
        where TEntity : Domain.Features.Entity =>
        ruleBuilder.MustSatisfyFactory(factoryMethod);

    public static IRuleBuilderOptions<T, IList<TElement>>
        ListHasAtMostOnePrimary<T, TElement>(
            this IRuleBuilder<T, IList<TElement>> ruleBuilder)
        where TElement : IHasPrimary =>
        ruleBuilder
            .Must(items => items.Count(item => item.IsPrimary) <= 1)
            .WithMessage("Only one primary item is allowed in the list.");

    private static bool HasAtMostOnePrimary(IList<IHasPrimary> items) =>
        items.Count(item => item.IsPrimary) <= 1;

}
