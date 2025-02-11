namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
internal sealed class _003Cb2ffb5d6_002D6a81_002D4f20_002D8e75_002D7064682f7f7c_003ENotNullWhenAttribute : Attribute
{
	public bool ReturnValue { get; }

	public _003Cb2ffb5d6_002D6a81_002D4f20_002D8e75_002D7064682f7f7c_003ENotNullWhenAttribute(bool returnValue)
	{
		ReturnValue = returnValue;
	}
}
