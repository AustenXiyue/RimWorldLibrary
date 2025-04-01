using System.Text;

namespace System.Xml.Linq;

/// <summary>Represents an XML declaration.</summary>
/// <filterpriority>2</filterpriority>
public class XDeclaration
{
	private string version;

	private string encoding;

	private string standalone;

	/// <summary>Gets or sets the encoding for this document.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the code page name for this document.</returns>
	public string Encoding
	{
		get
		{
			return encoding;
		}
		set
		{
			encoding = value;
		}
	}

	/// <summary>Gets or sets the standalone property for this document.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the standalone property for this document.</returns>
	public string Standalone
	{
		get
		{
			return standalone;
		}
		set
		{
			standalone = value;
		}
	}

	/// <summary>Gets or sets the version property for this document.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the version property for this document.</returns>
	public string Version
	{
		get
		{
			return version;
		}
		set
		{
			version = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XDeclaration" /> class with the specified version, encoding, and standalone status.</summary>
	/// <param name="version">The version of the XML, usually "1.0".</param>
	/// <param name="encoding">The encoding for the XML document.</param>
	/// <param name="standalone">A string containing "yes" or "no" that specifies whether the XML is standalone or requires external entities to be resolved.</param>
	public XDeclaration(string version, string encoding, string standalone)
	{
		this.version = version;
		this.encoding = encoding;
		this.standalone = standalone;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XDeclaration" /> class from another <see cref="T:System.Xml.Linq.XDeclaration" /> object. </summary>
	/// <param name="other">The <see cref="T:System.Xml.Linq.XDeclaration" /> used to initialize this <see cref="T:System.Xml.Linq.XDeclaration" /> object.</param>
	public XDeclaration(XDeclaration other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		version = other.version;
		encoding = other.encoding;
		standalone = other.standalone;
	}

	internal XDeclaration(XmlReader r)
	{
		version = r.GetAttribute("version");
		encoding = r.GetAttribute("encoding");
		standalone = r.GetAttribute("standalone");
		r.Read();
	}

	/// <summary>Provides the declaration as a formatted string.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the formatted XML string.</returns>
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder("<?xml");
		if (version != null)
		{
			stringBuilder.Append(" version=\"");
			stringBuilder.Append(version);
			stringBuilder.Append("\"");
		}
		if (encoding != null)
		{
			stringBuilder.Append(" encoding=\"");
			stringBuilder.Append(encoding);
			stringBuilder.Append("\"");
		}
		if (standalone != null)
		{
			stringBuilder.Append(" standalone=\"");
			stringBuilder.Append(standalone);
			stringBuilder.Append("\"");
		}
		stringBuilder.Append("?>");
		return stringBuilder.ToString();
	}
}
