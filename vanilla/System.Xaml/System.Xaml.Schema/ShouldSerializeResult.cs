namespace System.Xaml.Schema;

/// <summary>Defines serialization behavior as reported by a <see cref="T:System.Xaml.Schema.XamlMemberInvoker" />.</summary>
public enum ShouldSerializeResult
{
	/// <summary>Unknown, defer to the type of the member.</summary>
	Default,
	/// <summary>Serialize the result.</summary>
	True,
	/// <summary>Do not serialize the result.</summary>
	False
}
