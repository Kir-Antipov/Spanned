namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class StringSyntaxAttribute : Attribute
{
    public StringSyntaxAttribute(string syntax)
    {
        Syntax = syntax;
        Arguments = Array.Empty<object?>();
    }

    public StringSyntaxAttribute(string syntax, params object?[] arguments)
    {
        Syntax = syntax;
        Arguments = arguments;
    }

    public string Syntax { get; }

    public object?[] Arguments { get; }

    public const string CompositeFormat = nameof(CompositeFormat);
}
