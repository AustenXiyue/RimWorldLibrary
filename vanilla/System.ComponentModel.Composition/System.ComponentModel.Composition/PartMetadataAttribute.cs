namespace System.ComponentModel.Composition;

/// <summary>Specifies metadata for a part.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class PartMetadataAttribute : Attribute
{
	/// <summary>Gets the name of the metadata value.</summary>
	/// <returns> A string that contains the name of the metadata value.</returns>
	public string Name { get; private set; }

	/// <summary>Gets the metadata value.</summary>
	/// <returns> An object that contains the metadata value.</returns>
	public object Value { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.PartMetadataAttribute" /> class with the specified name and metadata value.</summary>
	/// <param name="name">A string that contains the name of the metadata value or null to use an empty string ("").</param>
	/// <param name="value">An object that contains the metadata value. This can be null.</param>
	public PartMetadataAttribute(string name, object value)
	{
		Name = name ?? string.Empty;
		Value = value;
	}
}
