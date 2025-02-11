namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
internal sealed class _003Cb2ffb5d6_002D6a81_002D4f20_002D8e75_002D7064682f7f7c_003ENotNullIfNotNullAttribute : Attribute
{
	public string ParameterName { get; }

	public _003Cb2ffb5d6_002D6a81_002D4f20_002D8e75_002D7064682f7f7c_003ENotNullIfNotNullAttribute(string parameterName)
	{
		ParameterName = parameterName;
	}
}
