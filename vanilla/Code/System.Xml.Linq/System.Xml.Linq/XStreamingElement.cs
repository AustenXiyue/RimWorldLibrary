using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace System.Xml.Linq;

/// <summary>Represents elements in an XML tree that supports deferred streaming output.</summary>
public class XStreamingElement
{
	internal XName name;

	internal object content;

	/// <summary>Gets or sets the name of this streaming element.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XName" /> that contains the name of this streaming element.</returns>
	public XName Name
	{
		get
		{
			return name;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			name = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XElement" /> class from the specified <see cref="T:System.Xml.Linq.XName" />.</summary>
	/// <param name="name">An <see cref="T:System.Xml.Linq.XName" /> that contains the name of the element.</param>
	public XStreamingElement(XName name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		this.name = name;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XStreamingElement" /> class with the specified name and content.</summary>
	/// <param name="name">An <see cref="T:System.Xml.Linq.XName" /> that contains the element name.</param>
	/// <param name="content">The contents of the element.</param>
	public XStreamingElement(XName name, object content)
		: this(name)
	{
		this.content = ((!(content is List<object>)) ? content : new object[1] { content });
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XStreamingElement" /> class with the specified name and content.</summary>
	/// <param name="name">An <see cref="T:System.Xml.Linq.XName" /> that contains the element name.</param>
	/// <param name="content">The contents of the element.</param>
	public XStreamingElement(XName name, params object[] content)
		: this(name)
	{
		this.content = content;
	}

	/// <summary>Adds the specified content as children to this <see cref="T:System.Xml.Linq.XStreamingElement" />.</summary>
	/// <param name="content">Content to be added to the streaming element.</param>
	public void Add(object content)
	{
		if (content == null)
		{
			return;
		}
		List<object> list = this.content as List<object>;
		if (list == null)
		{
			list = new List<object>();
			if (this.content != null)
			{
				list.Add(this.content);
			}
			this.content = list;
		}
		list.Add(content);
	}

	/// <summary>Adds the specified content as children to this <see cref="T:System.Xml.Linq.XStreamingElement" />.</summary>
	/// <param name="content">Content to be added to the streaming element.</param>
	public void Add(params object[] content)
	{
		Add((object)content);
	}

	/// <summary>Serialize this streaming element to a file.</summary>
	/// <param name="fileName">A <see cref="T:System.String" /> that contains the name of the file.</param>
	public void Save(string fileName)
	{
		Save(fileName, SaveOptions.None);
	}

	/// <summary>Serialize this streaming element to a file, optionally disabling formatting.</summary>
	/// <param name="fileName">A <see cref="T:System.String" /> that contains the name of the file.</param>
	/// <param name="options">A <see cref="T:System.Xml.Linq.SaveOptions" /> object that specifies formatting behavior.</param>
	public void Save(string fileName, SaveOptions options)
	{
		XmlWriterSettings xmlWriterSettings = XNode.GetXmlWriterSettings(options);
		using XmlWriter writer = XmlWriter.Create(fileName, xmlWriterSettings);
		Save(writer);
	}

	/// <summary>Outputs this <see cref="T:System.Xml.Linq.XStreamingElement" /> to the specified <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="stream">The stream to output this <see cref="T:System.Xml.Linq.XDocument" /> to.</param>
	public void Save(Stream stream)
	{
		Save(stream, SaveOptions.None);
	}

	/// <summary>Outputs this <see cref="T:System.Xml.Linq.XStreamingElement" /> to the specified <see cref="T:System.IO.Stream" />, optionally specifying formatting behavior.</summary>
	/// <param name="stream">The stream to output this <see cref="T:System.Xml.Linq.XDocument" /> to.</param>
	/// <param name="options">A <see cref="T:System.Xml.Linq.SaveOptions" /> object that specifies formatting behavior.</param>
	public void Save(Stream stream, SaveOptions options)
	{
		XmlWriterSettings xmlWriterSettings = XNode.GetXmlWriterSettings(options);
		using XmlWriter writer = XmlWriter.Create(stream, xmlWriterSettings);
		Save(writer);
	}

	/// <summary>Serialize this streaming element to a <see cref="T:System.IO.TextWriter" />.</summary>
	/// <param name="textWriter">A <see cref="T:System.IO.TextWriter" /> that the <see cref="T:System.Xml.Linq.XStreamingElement" /> will be written to.</param>
	public void Save(TextWriter textWriter)
	{
		Save(textWriter, SaveOptions.None);
	}

	/// <summary>Serialize this streaming element to a <see cref="T:System.IO.TextWriter" />, optionally disabling formatting.</summary>
	/// <param name="textWriter">The <see cref="T:System.IO.TextWriter" /> to output the XML to.</param>
	/// <param name="options">A <see cref="T:System.Xml.Linq.SaveOptions" /> that specifies formatting behavior.</param>
	public void Save(TextWriter textWriter, SaveOptions options)
	{
		XmlWriterSettings xmlWriterSettings = XNode.GetXmlWriterSettings(options);
		using XmlWriter writer = XmlWriter.Create(textWriter, xmlWriterSettings);
		Save(writer);
	}

	/// <summary>Serialize this streaming element to an <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="writer">A <see cref="T:System.Xml.XmlWriter" /> that the <see cref="T:System.Xml.Linq.XElement" /> will be written to.</param>
	public void Save(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.WriteStartDocument();
		WriteTo(writer);
		writer.WriteEndDocument();
	}

	/// <summary>Returns the formatted (indented) XML for this streaming element.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the indented XML.</returns>
	public override string ToString()
	{
		return GetXmlString(SaveOptions.None);
	}

	/// <summary>Returns the XML for this streaming element, optionally disabling formatting.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the XML.</returns>
	/// <param name="options">A <see cref="T:System.Xml.Linq.SaveOptions" /> that specifies formatting behavior.</param>
	public string ToString(SaveOptions options)
	{
		return GetXmlString(options);
	}

	/// <summary>Writes this streaming element to an <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="writer">An <see cref="T:System.Xml.XmlWriter" /> into which this method will write.</param>
	/// <filterpriority>2</filterpriority>
	public void WriteTo(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		new StreamingElementWriter(writer).WriteStreamingElement(this);
	}

	private string GetXmlString(SaveOptions o)
	{
		using StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.OmitXmlDeclaration = true;
		if ((o & SaveOptions.DisableFormatting) == 0)
		{
			xmlWriterSettings.Indent = true;
		}
		if ((o & SaveOptions.OmitDuplicateNamespaces) != 0)
		{
			xmlWriterSettings.NamespaceHandling |= NamespaceHandling.OmitDuplicates;
		}
		using (XmlWriter writer = XmlWriter.Create(stringWriter, xmlWriterSettings))
		{
			WriteTo(writer);
		}
		return stringWriter.ToString();
	}
}
