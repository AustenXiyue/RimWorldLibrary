namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
internal sealed class _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMemberNotNullAttribute : Attribute
{
	public string[] Members { get; }

	public _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMemberNotNullAttribute(string member)
	{
		Members = new string[1] { member };
	}

	public _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMemberNotNullAttribute(params string[] members)
	{
		Members = members;
	}
}
