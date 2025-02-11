namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
internal sealed class _003C22576685_002D8ec8_002D4022_002D94e7_002Db5a630de7c65_003EDoesNotReturnIfAttribute : Attribute
{
	public bool ParameterValue { get; }

	public _003C22576685_002D8ec8_002D4022_002D94e7_002Db5a630de7c65_003EDoesNotReturnIfAttribute(bool parameterValue)
	{
		ParameterValue = parameterValue;
	}
}
