namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
internal sealed class _003C22576685_002D8ec8_002D4022_002D94e7_002Db5a630de7c65_003EMemberNotNullWhenAttribute : Attribute
{
	public bool ReturnValue { get; }

	public string[] Members { get; }

	public _003C22576685_002D8ec8_002D4022_002D94e7_002Db5a630de7c65_003EMemberNotNullWhenAttribute(bool returnValue, string member)
	{
		ReturnValue = returnValue;
		Members = new string[1] { member };
	}

	public _003C22576685_002D8ec8_002D4022_002D94e7_002Db5a630de7c65_003EMemberNotNullWhenAttribute(bool returnValue, params string[] members)
	{
		ReturnValue = returnValue;
		Members = members;
	}
}
