namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
internal sealed class _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMemberNotNullWhenAttribute : Attribute
{
	public bool ReturnValue { get; }

	public string[] Members { get; }

	public _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMemberNotNullWhenAttribute(bool returnValue, string member)
	{
		ReturnValue = returnValue;
		Members = new string[1] { member };
	}

	public _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMemberNotNullWhenAttribute(bool returnValue, params string[] members)
	{
		ReturnValue = returnValue;
		Members = members;
	}
}
