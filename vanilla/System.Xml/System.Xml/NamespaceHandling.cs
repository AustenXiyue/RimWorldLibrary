namespace System.Xml;

/// <summary>Specifies whether to remove duplicate namespace declarations in the <see cref="T:System.Xml.XmlWriter" />. </summary>
[Flags]
public enum NamespaceHandling
{
	/// <summary>Specifies that duplicate namespace declarations will not be removed.</summary>
	Default = 0,
	/// <summary>Specifies that duplicate namespace declarations will be removed. For the duplicate namespace to be removed, the prefix and the namespace must match.</summary>
	OmitDuplicates = 1
}
