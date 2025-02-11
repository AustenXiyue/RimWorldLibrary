using System.Text;

namespace System.Xml;

/// <summary>Represents the XML declaration node &lt;?xml version='1.0'...?&gt;.</summary>
public class XmlDeclaration : XmlLinkedNode
{
	private const string YES = "yes";

	private const string NO = "no";

	private string version;

	private string encoding;

	private string standalone;

	/// <summary>Gets the XML version of the document.</summary>
	/// <returns>The value is always 1.0.</returns>
	public string Version
	{
		get
		{
			return version;
		}
		internal set
		{
			version = value;
		}
	}

	/// <summary>Gets or sets the encoding level of the XML document.</summary>
	/// <returns>The valid character encoding name. The most commonly supported character encoding names for XML are the following: Category Encoding Names Unicode UTF-8, UTF-16 ISO 10646 ISO-10646-UCS-2, ISO-10646-UCS-4 ISO 8859 ISO-8859-n (where "n" is a digit from 1 to 9) JIS X-0208-1997 ISO-2022-JP, Shift_JIS, EUC-JP This value is optional. If a value is not set, this property returns String.Empty.If an encoding attribute is not included, UTF-8 encoding is assumed when the document is written or saved out.</returns>
	public string Encoding
	{
		get
		{
			return encoding;
		}
		set
		{
			encoding = ((value == null) ? string.Empty : value);
		}
	}

	/// <summary>Gets or sets the value of the standalone attribute.</summary>
	/// <returns>Valid values are yes if all entity declarations required by the XML document are contained within the document or no if an external document type definition (DTD) is required. If a standalone attribute is not present in the XML declaration, this property returns String.Empty.</returns>
	public string Standalone
	{
		get
		{
			return standalone;
		}
		set
		{
			if (value == null)
			{
				standalone = string.Empty;
				return;
			}
			if (value.Length == 0 || value == "yes" || value == "no")
			{
				standalone = value;
				return;
			}
			throw new ArgumentException(Res.GetString("Wrong value for the XML declaration standalone attribute of '{0}'.", value));
		}
	}

	/// <summary>Gets or sets the value of the XmlDeclaration.</summary>
	/// <returns>The contents of the XmlDeclaration (that is, everything between &lt;?xml and ?&gt;).</returns>
	public override string Value
	{
		get
		{
			return InnerText;
		}
		set
		{
			InnerText = value;
		}
	}

	/// <summary>Gets or sets the concatenated values of the XmlDeclaration.</summary>
	/// <returns>The concatenated values of the XmlDeclaration (that is, everything between &lt;?xml and ?&gt;).</returns>
	public override string InnerText
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder("version=\"" + Version + "\"");
			if (Encoding.Length > 0)
			{
				stringBuilder.Append(" encoding=\"");
				stringBuilder.Append(Encoding);
				stringBuilder.Append("\"");
			}
			if (Standalone.Length > 0)
			{
				stringBuilder.Append(" standalone=\"");
				stringBuilder.Append(Standalone);
				stringBuilder.Append("\"");
			}
			return stringBuilder.ToString();
		}
		set
		{
			string text = null;
			string text2 = null;
			string text3 = null;
			string text4 = Encoding;
			string text5 = Standalone;
			string text6 = Version;
			XmlLoader.ParseXmlDeclarationValue(value, out text, out text2, out text3);
			try
			{
				if (text != null && !IsValidXmlVersion(text))
				{
					throw new ArgumentException(Res.GetString("Wrong XML version information. The XML must match production \"VersionNum ::= '1.' [0-9]+\"."));
				}
				Version = text;
				if (text2 != null)
				{
					Encoding = text2;
				}
				if (text3 != null)
				{
					Standalone = text3;
				}
			}
			catch
			{
				Encoding = text4;
				Standalone = text5;
				Version = text6;
				throw;
			}
		}
	}

	/// <summary>Gets the qualified name of the node.</summary>
	/// <returns>For XmlDeclaration nodes, the name is xml.</returns>
	public override string Name => "xml";

	/// <summary>Gets the local name of the node.</summary>
	/// <returns>For XmlDeclaration nodes, the local name is xml.</returns>
	public override string LocalName => Name;

	/// <summary>Gets the type of the current node.</summary>
	/// <returns>For XmlDeclaration nodes, this value is XmlNodeType.XmlDeclaration.</returns>
	public override XmlNodeType NodeType => XmlNodeType.XmlDeclaration;

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlDeclaration" /> class.</summary>
	/// <param name="version">The XML version; see the <see cref="P:System.Xml.XmlDeclaration.Version" /> property.</param>
	/// <param name="encoding">The encoding scheme; see the <see cref="P:System.Xml.XmlDeclaration.Encoding" /> property.</param>
	/// <param name="standalone">Indicates whether the XML document depends on an external DTD; see the <see cref="P:System.Xml.XmlDeclaration.Standalone" /> property.</param>
	/// <param name="doc">The parent XML document.</param>
	protected internal XmlDeclaration(string version, string encoding, string standalone, XmlDocument doc)
		: base(doc)
	{
		if (!IsValidXmlVersion(version))
		{
			throw new ArgumentException(Res.GetString("Wrong XML version information. The XML must match production \"VersionNum ::= '1.' [0-9]+\"."));
		}
		if (standalone != null && standalone.Length > 0 && standalone != "yes" && standalone != "no")
		{
			throw new ArgumentException(Res.GetString("Wrong value for the XML declaration standalone attribute of '{0}'.", standalone));
		}
		Encoding = encoding;
		Standalone = standalone;
		Version = version;
	}

	/// <summary>Creates a duplicate of this node.</summary>
	/// <returns>The cloned node.</returns>
	/// <param name="deep">true to recursively clone the subtree under the specified node; false to clone only the node itself. Because XmlDeclaration nodes do not have children, the cloned node always includes the data value, regardless of the parameter setting. </param>
	public override XmlNode CloneNode(bool deep)
	{
		return OwnerDocument.CreateXmlDeclaration(Version, Encoding, Standalone);
	}

	/// <summary>Saves the node to the specified <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="w">The XmlWriter to which you want to save. </param>
	public override void WriteTo(XmlWriter w)
	{
		w.WriteProcessingInstruction(Name, InnerText);
	}

	/// <summary>Saves the children of the node to the specified <see cref="T:System.Xml.XmlWriter" />. Because XmlDeclaration nodes do not have children, this method has no effect.</summary>
	/// <param name="w">The XmlWriter to which you want to save. </param>
	public override void WriteContentTo(XmlWriter w)
	{
	}

	private bool IsValidXmlVersion(string ver)
	{
		if (ver.Length >= 3 && ver[0] == '1' && ver[1] == '.')
		{
			return XmlCharType.IsOnlyDigits(ver, 2, ver.Length - 2);
		}
		return false;
	}
}
