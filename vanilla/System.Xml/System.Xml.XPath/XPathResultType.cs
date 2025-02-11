namespace System.Xml.XPath;

/// <summary>Specifies the return type of the XPath expression.</summary>
public enum XPathResultType
{
	/// <summary>A numeric value.</summary>
	Number = 0,
	/// <summary>A <see cref="T:System.String" /> value.</summary>
	String = 1,
	/// <summary>A <see cref="T:System.Boolean" />true or false value.</summary>
	Boolean = 2,
	/// <summary>A node collection.</summary>
	NodeSet = 3,
	/// <summary>A tree fragment.</summary>
	Navigator = 1,
	/// <summary>Any of the XPath node types.</summary>
	Any = 5,
	/// <summary>The expression does not evaluate to the correct XPath type.</summary>
	Error = 6
}
