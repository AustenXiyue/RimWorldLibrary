namespace System.Xml.Linq;

/// <summary>Specifies whether to omit duplicate namespaces when loading an <see cref="T:System.Xml.Linq.XDocument" /> with an <see cref="T:System.Xml.XmlReader" />.</summary>
[Flags]
public enum ReaderOptions
{
	/// <summary>No reader options specified.</summary>
	None = 0,
	/// <summary>Omit duplicate namespaces when loading the <see cref="T:System.Xml.Linq.XDocument" />.</summary>
	OmitDuplicateNamespaces = 1
}
