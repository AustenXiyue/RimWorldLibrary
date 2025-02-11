namespace System.Windows;

/// <summary>Specifies the locations where theme resource dictionaries are located.</summary>
public enum ResourceDictionaryLocation
{
	/// <summary>No theme dictionaries exist.</summary>
	None,
	/// <summary>Theme dictionaries exist in the assembly that defines the types being themed.</summary>
	SourceAssembly,
	/// <summary>Theme dictionaries exist in assemblies external to the one defining the types being themed.</summary>
	ExternalAssembly
}
