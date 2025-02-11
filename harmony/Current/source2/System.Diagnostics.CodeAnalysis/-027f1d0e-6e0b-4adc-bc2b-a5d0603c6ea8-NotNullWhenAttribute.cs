namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
internal sealed class _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003ENotNullWhenAttribute : Attribute
{
	public bool ReturnValue { get; }

	public _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003ENotNullWhenAttribute(bool returnValue)
	{
		ReturnValue = returnValue;
	}
}
