namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
internal sealed class _003C22576685_002D8ec8_002D4022_002D94e7_002Db5a630de7c65_003EMemberNotNullAttribute : Attribute
{
	public string[] Members { get; }

	public _003C22576685_002D8ec8_002D4022_002D94e7_002Db5a630de7c65_003EMemberNotNullAttribute(string member)
	{
		Members = new string[1] { member };
	}

	public _003C22576685_002D8ec8_002D4022_002D94e7_002Db5a630de7c65_003EMemberNotNullAttribute(params string[] members)
	{
		Members = members;
	}
}
