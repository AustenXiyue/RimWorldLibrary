namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
internal sealed class _003C22576685_002D8ec8_002D4022_002D94e7_002Db5a630de7c65_003ENotNullIfNotNullAttribute : Attribute
{
	public string ParameterName { get; }

	public _003C22576685_002D8ec8_002D4022_002D94e7_002Db5a630de7c65_003ENotNullIfNotNullAttribute(string parameterName)
	{
		ParameterName = parameterName;
	}
}
