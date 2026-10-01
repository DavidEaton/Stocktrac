namespace Stocktrac.Domain.Features
{
    public sealed record NonEmptyString
    {
        internal NonEmptyString(string value) => Value = value;

        public string Value { get; }

        public override string ToString() => Value;
    }
}