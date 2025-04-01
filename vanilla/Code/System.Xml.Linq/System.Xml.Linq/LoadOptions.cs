namespace System.Xml.Linq;

/// <summary>Specifies load options when parsing XML. </summary>
[Flags]
public enum LoadOptions
{
	/// <summary>Does not preserve insignificant white space or load base URI and line information.</summary>
	None = 0,
	/// <summary>Preserves insignificant white space while parsing.</summary>
	PreserveWhitespace = 1,
	/// <summary>Requests the base URI information from the <see cref="T:System.Xml.XmlReader" />, and makes it available via the <see cref="P:System.Xml.Linq.XObject.BaseUri" /> property.</summary>
	SetBaseUri = 2,
	/// <summary>Requests the line information from the <see cref="T:System.Xml.XmlReader" /> and makes it available via properties on <see cref="T:System.Xml.Linq.XObject" />.</summary>
	SetLineInfo = 4
}
