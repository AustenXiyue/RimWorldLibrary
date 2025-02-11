namespace System.Xml.Resolvers;

/// <summary>The <see cref="T:System.Xml.Resolvers.XmlKnownDtds" /> enumeration is used by the <see cref="T:System.Xml.Resolvers.XmlPreloadedResolver" /> and defines which well-known DTDs that the <see cref="T:System.Xml.Resolvers.XmlPreloadedResolver" /> recognizes.</summary>
[Flags]
public enum XmlKnownDtds
{
	/// <summary>Specifies that the <see cref="T:System.Xml.Resolvers.XmlPreloadedResolver" /> will not recognize any of the predefined DTDs.</summary>
	None = 0,
	/// <summary>Specifies that the <see cref="T:System.Xml.Resolvers.XmlPreloadedResolver" /> will recognize DTDs and entities that are defined in XHTML 1.0. </summary>
	Xhtml10 = 1,
	/// <summary>Specifies that the <see cref="T:System.Xml.Resolvers.XmlPreloadedResolver" /> will recognize DTDs and entities that are defined in RSS 0.91.</summary>
	Rss091 = 2,
	/// <summary>Specifies that the <see cref="T:System.Xml.Resolvers.XmlPreloadedResolver" /> will recognize all currently supported DTDs. This is the default behavior.</summary>
	All = 0xFFFF
}
