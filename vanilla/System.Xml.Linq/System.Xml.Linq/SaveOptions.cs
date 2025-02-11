namespace System.Xml.Linq;

/// <summary>Specifies serialization options.</summary>
[Flags]
public enum SaveOptions
{
	/// <summary>Format (indent) the XML while serializing.</summary>
	None = 0,
	/// <summary>Preserve all insignificant white space while serializing.</summary>
	DisableFormatting = 1,
	/// <summary>Remove the duplicate namespace declarations while serializing.</summary>
	OmitDuplicateNamespaces = 2
}
