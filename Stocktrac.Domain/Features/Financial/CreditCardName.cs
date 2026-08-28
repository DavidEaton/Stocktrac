using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Financial.Extensions;

namespace Stocktrac.Domain.Features.Financial;

public readonly record struct CreditCardName
{
    private string Value { get; init; }

    public static Result<CreditCardName> Create(string? name) =>
        CreditCardExtensions.ValidateName(name)
            .Map(validName => new CreditCardName { Value = validName });

}
