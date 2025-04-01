namespace System.CodeDom;

/// <summary>Specifies how the code type reference is to be resolved.</summary>
[Flags]
public enum CodeTypeReferenceOptions
{
	/// <summary>Resolve the type from the root namespace.</summary>
	GlobalReference = 1,
	/// <summary>Resolve the type from the type parameter.</summary>
	GenericTypeParameter = 2
}
