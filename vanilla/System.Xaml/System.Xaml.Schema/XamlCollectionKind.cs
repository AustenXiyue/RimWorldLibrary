namespace System.Xaml.Schema;

/// <summary>Describes the collection metaphor (if any) used by a XAML member.</summary>
public enum XamlCollectionKind : byte
{
	/// <summary>XAML member does not support a collection.</summary>
	None,
	/// <summary>XAML member supports a list or a collection.</summary>
	Collection,
	/// <summary>XAML member supports a dictionary (key-value pairs).</summary>
	Dictionary,
	/// <summary>XAML member supports an array collection.</summary>
	Array
}
