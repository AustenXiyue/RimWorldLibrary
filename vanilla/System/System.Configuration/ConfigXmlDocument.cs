using System.Configuration.Internal;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Xml;

namespace System.Configuration;

/// <summary>Wraps the corresponding <see cref="T:System.Xml.XmlDocument" /> type and also carries the necessary information for reporting file-name and line numbers. </summary>
/// <filterpriority>2</filterpriority>
[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
public sealed class ConfigXmlDocument : XmlDocument, System.Configuration.IConfigXmlNode, IConfigErrorInfo
{
	private class ConfigXmlAttribute : XmlAttribute, System.Configuration.IConfigXmlNode, IConfigErrorInfo
	{
		private string fileName;

		private int lineNumber;

		public string Filename
		{
			get
			{
				if (fileName != null && fileName.Length > 0 && SecurityManager.SecurityEnabled)
				{
					new FileIOPermission(FileIOPermissionAccess.PathDiscovery, fileName).Demand();
				}
				return fileName;
			}
		}

		public int LineNumber => lineNumber;

		public ConfigXmlAttribute(ConfigXmlDocument document, string prefix, string localName, string namespaceUri)
			: base(prefix, localName, namespaceUri, document)
		{
			fileName = document.fileName;
			lineNumber = document.LineNumber;
		}
	}

	private class ConfigXmlCDataSection : XmlCDataSection, System.Configuration.IConfigXmlNode, IConfigErrorInfo
	{
		private string fileName;

		private int lineNumber;

		public string Filename
		{
			get
			{
				if (fileName != null && fileName.Length > 0 && SecurityManager.SecurityEnabled)
				{
					new FileIOPermission(FileIOPermissionAccess.PathDiscovery, fileName).Demand();
				}
				return fileName;
			}
		}

		public int LineNumber => lineNumber;

		public ConfigXmlCDataSection(ConfigXmlDocument document, string data)
			: base(data, document)
		{
			fileName = document.fileName;
			lineNumber = document.LineNumber;
		}
	}

	private class ConfigXmlComment : XmlComment, System.Configuration.IConfigXmlNode
	{
		private string fileName;

		private int lineNumber;

		public string Filename
		{
			get
			{
				if (fileName != null && fileName.Length > 0 && SecurityManager.SecurityEnabled)
				{
					new FileIOPermission(FileIOPermissionAccess.PathDiscovery, fileName).Demand();
				}
				return fileName;
			}
		}

		public int LineNumber => lineNumber;

		public ConfigXmlComment(ConfigXmlDocument document, string comment)
			: base(comment, document)
		{
			fileName = document.fileName;
			lineNumber = document.LineNumber;
		}
	}

	private class ConfigXmlElement : XmlElement, System.Configuration.IConfigXmlNode, IConfigErrorInfo
	{
		private string fileName;

		private int lineNumber;

		public string Filename
		{
			get
			{
				if (fileName != null && fileName.Length > 0 && SecurityManager.SecurityEnabled)
				{
					new FileIOPermission(FileIOPermissionAccess.PathDiscovery, fileName).Demand();
				}
				return fileName;
			}
		}

		public int LineNumber => lineNumber;

		public ConfigXmlElement(ConfigXmlDocument document, string prefix, string localName, string namespaceUri)
			: base(prefix, localName, namespaceUri, document)
		{
			fileName = document.fileName;
			lineNumber = document.LineNumber;
		}
	}

	private class ConfigXmlText : XmlText, System.Configuration.IConfigXmlNode, IConfigErrorInfo
	{
		private string fileName;

		private int lineNumber;

		public string Filename
		{
			get
			{
				if (fileName != null && fileName.Length > 0 && SecurityManager.SecurityEnabled)
				{
					new FileIOPermission(FileIOPermissionAccess.PathDiscovery, fileName).Demand();
				}
				return fileName;
			}
		}

		public int LineNumber => lineNumber;

		public ConfigXmlText(ConfigXmlDocument document, string data)
			: base(data, document)
		{
			fileName = document.fileName;
			lineNumber = document.LineNumber;
		}
	}

	private XmlTextReader reader;

	private string fileName;

	private int lineNumber;

	/// <summary>Gets the configuration file name.</summary>
	/// <returns>The configuration file name.</returns>
	/// <filterpriority>2</filterpriority>
	public string Filename
	{
		get
		{
			if (fileName != null && fileName.Length > 0 && SecurityManager.SecurityEnabled)
			{
				new FileIOPermission(FileIOPermissionAccess.PathDiscovery, fileName).Demand();
			}
			return fileName;
		}
	}

	/// <summary>Gets the current node line number.</summary>
	/// <returns>The line number for the current node.</returns>
	/// <filterpriority>2</filterpriority>
	public int LineNumber => lineNumber;

	/// <summary>Gets the configuration file name.</summary>
	/// <returns>The file name.</returns>
	string IConfigErrorInfo.Filename => Filename;

	/// <summary>Gets the configuration line number.</summary>
	/// <returns>The line number.</returns>
	int IConfigErrorInfo.LineNumber => LineNumber;

	string System.Configuration.IConfigXmlNode.Filename => Filename;

	int System.Configuration.IConfigXmlNode.LineNumber => LineNumber;

	/// <summary>Creates a configuration element attribute.</summary>
	/// <returns>The <see cref="P:System.Xml.Serialization.XmlAttributes.XmlAttribute" /> attribute.</returns>
	/// <param name="prefix">The prefix definition.</param>
	/// <param name="localName">The name that is used locally.</param>
	/// <param name="namespaceUri">The URL that is assigned to the namespace.</param>
	/// <filterpriority>2</filterpriority>
	public override XmlAttribute CreateAttribute(string prefix, string localName, string namespaceUri)
	{
		return new ConfigXmlAttribute(this, prefix, localName, namespaceUri);
	}

	/// <summary>Creates an XML CData section.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlCDataSection" /> value.</returns>
	/// <param name="data">The data to use.</param>
	/// <filterpriority>2</filterpriority>
	public override XmlCDataSection CreateCDataSection(string data)
	{
		return new ConfigXmlCDataSection(this, data);
	}

	/// <summary>Create an XML comment.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlComment" /> value.</returns>
	/// <param name="data">The comment data.</param>
	/// <filterpriority>2</filterpriority>
	public override XmlComment CreateComment(string data)
	{
		return new ConfigXmlComment(this, data);
	}

	/// <summary>Creates a configuration element.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlElement" /> value.</returns>
	/// <param name="prefix">The prefix definition.</param>
	/// <param name="localName">The name used locally.</param>
	/// <param name="namespaceUri">The namespace for the URL.</param>
	/// <filterpriority>2</filterpriority>
	public override XmlElement CreateElement(string prefix, string localName, string namespaceUri)
	{
		return new ConfigXmlElement(this, prefix, localName, namespaceUri);
	}

	/// <summary>Creates white spaces.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlSignificantWhitespace" /> value.</returns>
	/// <param name="data">The data to use.</param>
	/// <filterpriority>2</filterpriority>
	public override XmlSignificantWhitespace CreateSignificantWhitespace(string data)
	{
		return base.CreateSignificantWhitespace(data);
	}

	/// <summary>Create a text node.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlText" /> value.</returns>
	/// <param name="text">The text to use.</param>
	/// <filterpriority>2</filterpriority>
	public override XmlText CreateTextNode(string text)
	{
		return new ConfigXmlText(this, text);
	}

	/// <summary>Creates white space.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlWhitespace" /> value.</returns>
	/// <param name="data">The data to use.</param>
	/// <filterpriority>2</filterpriority>
	public override XmlWhitespace CreateWhitespace(string data)
	{
		return base.CreateWhitespace(data);
	}

	/// <summary>Loads the configuration file.</summary>
	/// <param name="filename">The name of the file.</param>
	/// <filterpriority>2</filterpriority>
	public override void Load(string filename)
	{
		XmlTextReader xmlTextReader = new XmlTextReader(filename);
		try
		{
			xmlTextReader.MoveToContent();
			LoadSingleElement(filename, xmlTextReader);
		}
		finally
		{
			xmlTextReader.Close();
		}
	}

	/// <summary>Loads a single configuration element.</summary>
	/// <param name="filename">The name of the file.</param>
	/// <param name="sourceReader">The source for the reader.</param>
	/// <filterpriority>2</filterpriority>
	public void LoadSingleElement(string filename, XmlTextReader sourceReader)
	{
		fileName = filename;
		lineNumber = sourceReader.LineNumber;
		string s = sourceReader.ReadOuterXml();
		reader = new XmlTextReader(new StringReader(s), sourceReader.NameTable);
		Load(reader);
		reader.Close();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigXmlDocument" /> class. </summary>
	public ConfigXmlDocument()
	{
	}
}
