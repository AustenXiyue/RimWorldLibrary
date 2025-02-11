namespace System.Xaml;

/// <summary>Describes the type of the node that is currently being processed by a XAML reader.</summary>
public enum XamlNodeType : byte
{
	/// <summary>The reader is not positioned at a true node (for example, the reader might be at end-of-file).</summary>
	None,
	/// <summary>The reader is at the start of an object node.</summary>
	StartObject,
	/// <summary>The reader is within an object node and writing a default or implicit value, instead of being a specified object value.</summary>
	GetObject,
	/// <summary>The reader is at the end of an object node.</summary>
	EndObject,
	/// <summary>The reader is at the start of a member node.</summary>
	StartMember,
	/// <summary>The reader is at the end of a member node.</summary>
	EndMember,
	/// <summary>The reader is within a node and processing a value.</summary>
	Value,
	/// <summary>The reader is within an XML namespace declaration.</summary>
	NamespaceDeclaration
}
