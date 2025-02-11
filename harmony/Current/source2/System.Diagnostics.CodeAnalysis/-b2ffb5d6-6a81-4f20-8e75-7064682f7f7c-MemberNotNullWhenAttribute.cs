namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
internal sealed class _003Cb2ffb5d6_002D6a81_002D4f20_002D8e75_002D7064682f7f7c_003EMemberNotNullWhenAttribute : Attribute
{
	public bool ReturnValue { get; }

	public string[] Members { get; }

	public _003Cb2ffb5d6_002D6a81_002D4f20_002D8e75_002D7064682f7f7c_003EMemberNotNullWhenAttribute(bool returnValue, string member)
	{
		ReturnValue = returnValue;
		Members = new string[1] { member };
	}

	public _003Cb2ffb5d6_002D6a81_002D4f20_002D8e75_002D7064682f7f7c_003EMemberNotNullWhenAttribute(bool returnValue, params string[] members)
	{
		ReturnValue = returnValue;
		Members = members;
	}
}
