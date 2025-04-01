using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Xml.Xsl.Runtime;

namespace System.Xml;

/// <summary>Specifies a set of features to support on the <see cref="T:System.Xml.XmlWriter" /> object created by the <see cref="Overload:System.Xml.XmlWriter.Create" /> method.</summary>
[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
public sealed class XmlWriterSettings
{
	private bool useAsync;

	private Encoding encoding;

	private bool omitXmlDecl;

	private NewLineHandling newLineHandling;

	private string newLineChars;

	private TriState indent;

	private string indentChars;

	private bool newLineOnAttributes;

	private bool closeOutput;

	private NamespaceHandling namespaceHandling;

	private ConformanceLevel conformanceLevel;

	private bool checkCharacters;

	private bool writeEndDocumentOnClose;

	private XmlOutputMethod outputMethod;

	private List<XmlQualifiedName> cdataSections = new List<XmlQualifiedName>();

	private bool doNotEscapeUriAttributes;

	private bool mergeCDataSections;

	private string mediaType;

	private string docTypeSystem;

	private string docTypePublic;

	private XmlStandalone standalone;

	private bool autoXmlDecl;

	private bool isReadOnly;

	/// <summary>Gets or sets a value that indicates whether asynchronous <see cref="T:System.Xml.XmlWriter" /> methods can be used on a particular <see cref="T:System.Xml.XmlWriter" /> instance.</summary>
	/// <returns>true if asynchronous methods can be used; otherwise, false.</returns>
	public bool Async
	{
		get
		{
			return useAsync;
		}
		set
		{
			CheckReadOnly("Async");
			useAsync = value;
		}
	}

	/// <summary>Gets or sets the type of text encoding to use.</summary>
	/// <returns>The text encoding to use. The default is Encoding.UTF8.</returns>
	public Encoding Encoding
	{
		get
		{
			return encoding;
		}
		set
		{
			CheckReadOnly("Encoding");
			encoding = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether to omit an XML declaration.</summary>
	/// <returns>true to omit the XML declaration; otherwise, false. The default is false, an XML declaration is written.</returns>
	public bool OmitXmlDeclaration
	{
		get
		{
			return omitXmlDecl;
		}
		set
		{
			CheckReadOnly("OmitXmlDeclaration");
			omitXmlDecl = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether to normalize line breaks in the output.</summary>
	/// <returns>One of the <see cref="T:System.Xml.NewLineHandling" /> values. The default is <see cref="F:System.Xml.NewLineHandling.Replace" />.</returns>
	public NewLineHandling NewLineHandling
	{
		get
		{
			return newLineHandling;
		}
		set
		{
			CheckReadOnly("NewLineHandling");
			if ((uint)value > 2u)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			newLineHandling = value;
		}
	}

	/// <summary>Gets or sets the character string to use for line breaks.</summary>
	/// <returns>The character string to use for line breaks. This can be set to any string value. However, to ensure valid XML, you should specify only valid white space characters, such as space characters, tabs, carriage returns, or line feeds. The default is \r\n (carriage return, new line).</returns>
	/// <exception cref="T:System.ArgumentNullException">The value assigned to the <see cref="P:System.Xml.XmlWriterSettings.NewLineChars" /> is null.</exception>
	public string NewLineChars
	{
		get
		{
			return newLineChars;
		}
		set
		{
			CheckReadOnly("NewLineChars");
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			newLineChars = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether to indent elements.</summary>
	/// <returns>true to write individual elements on new lines and indent; otherwise, false. The default is false.</returns>
	public bool Indent
	{
		get
		{
			return indent == TriState.True;
		}
		set
		{
			CheckReadOnly("Indent");
			indent = (value ? TriState.True : TriState.False);
		}
	}

	/// <summary>Gets or sets the character string to use when indenting. This setting is used when the <see cref="P:System.Xml.XmlWriterSettings.Indent" /> property is set to true.</summary>
	/// <returns>The character string to use when indenting. This can be set to any string value. However, to ensure valid XML, you should specify only valid white space characters, such as space characters, tabs, carriage returns, or line feeds. The default is two spaces.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value assigned to the <see cref="P:System.Xml.XmlWriterSettings.IndentChars" /> is null.</exception>
	public string IndentChars
	{
		get
		{
			return indentChars;
		}
		set
		{
			CheckReadOnly("IndentChars");
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			indentChars = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether to write attributes on a new line.</summary>
	/// <returns>true to write attributes on individual lines; otherwise, false. The default is false.NoteThis setting has no effect when the <see cref="P:System.Xml.XmlWriterSettings.Indent" /> property value is false.When <see cref="P:System.Xml.XmlWriterSettings.NewLineOnAttributes" /> is set to true, each attribute is pre-pended with a new line and one extra level of indentation.</returns>
	public bool NewLineOnAttributes
	{
		get
		{
			return newLineOnAttributes;
		}
		set
		{
			CheckReadOnly("NewLineOnAttributes");
			newLineOnAttributes = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether the <see cref="T:System.Xml.XmlWriter" /> should also close the underlying stream or <see cref="T:System.IO.TextWriter" /> when the <see cref="M:System.Xml.XmlWriter.Close" /> method is called.</summary>
	/// <returns>true to also close the underlying stream or <see cref="T:System.IO.TextWriter" />; otherwise, false. The default is false.</returns>
	public bool CloseOutput
	{
		get
		{
			return closeOutput;
		}
		set
		{
			CheckReadOnly("CloseOutput");
			closeOutput = value;
		}
	}

	/// <summary>Gets or sets the level of conformance which the <see cref="T:System.Xml.XmlWriter" /> complies with.</summary>
	/// <returns>One of the <see cref="T:System.Xml.ConformanceLevel" /> values. The default is ConformanceLevel.Document.</returns>
	public ConformanceLevel ConformanceLevel
	{
		get
		{
			return conformanceLevel;
		}
		set
		{
			CheckReadOnly("ConformanceLevel");
			if ((uint)value > 2u)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			conformanceLevel = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether to do character checking.</summary>
	/// <returns>true to do character checking; otherwise, false. The default is true.</returns>
	public bool CheckCharacters
	{
		get
		{
			return checkCharacters;
		}
		set
		{
			CheckReadOnly("CheckCharacters");
			checkCharacters = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Xml.XmlWriter" /> should remove duplicate namespace declarations when writing XML content. The default behavior is for the writer to output all namespace declarations that are present in the writer's namespace resolver.</summary>
	/// <returns>The <see cref="T:System.Xml.NamespaceHandling" /> enumeration used to specify whether to remove duplicate namespace declarations in the <see cref="T:System.Xml.XmlWriter" />.</returns>
	public NamespaceHandling NamespaceHandling
	{
		get
		{
			return namespaceHandling;
		}
		set
		{
			CheckReadOnly("NamespaceHandling");
			if ((uint)value > 1u)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			namespaceHandling = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Xml.XmlWriter" /> will add closing tags to all unclosed element tags when the <see cref="M:System.Xml.XmlWriter.Close" /> method is called.</summary>
	/// <returns>true if all unclosed element tags will be closed out; otherwise, false. The default value is true. </returns>
	public bool WriteEndDocumentOnClose
	{
		get
		{
			return writeEndDocumentOnClose;
		}
		set
		{
			CheckReadOnly("WriteEndDocumentOnClose");
			writeEndDocumentOnClose = value;
		}
	}

	/// <summary>Gets the method used to serialize the <see cref="T:System.Xml.XmlWriter" /> output.</summary>
	/// <returns>One of the <see cref="T:System.Xml.XmlOutputMethod" /> values. The default is <see cref="F:System.Xml.XmlOutputMethod.Xml" />.</returns>
	public XmlOutputMethod OutputMethod
	{
		get
		{
			return outputMethod;
		}
		internal set
		{
			outputMethod = value;
		}
	}

	internal List<XmlQualifiedName> CDataSectionElements => cdataSections;

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Xml.XmlWriter" /> do not escape URI attributes.</summary>
	/// <returns>true if the <see cref="T:System.Xml.XmlWriter" /> do not escape URI attributes; otherwise, false.</returns>
	public bool DoNotEscapeUriAttributes
	{
		get
		{
			return doNotEscapeUriAttributes;
		}
		set
		{
			CheckReadOnly("DoNotEscapeUriAttributes");
			doNotEscapeUriAttributes = value;
		}
	}

	internal bool MergeCDataSections
	{
		get
		{
			return mergeCDataSections;
		}
		set
		{
			CheckReadOnly("MergeCDataSections");
			mergeCDataSections = value;
		}
	}

	internal string MediaType
	{
		get
		{
			return mediaType;
		}
		set
		{
			CheckReadOnly("MediaType");
			mediaType = value;
		}
	}

	internal string DocTypeSystem
	{
		get
		{
			return docTypeSystem;
		}
		set
		{
			CheckReadOnly("DocTypeSystem");
			docTypeSystem = value;
		}
	}

	internal string DocTypePublic
	{
		get
		{
			return docTypePublic;
		}
		set
		{
			CheckReadOnly("DocTypePublic");
			docTypePublic = value;
		}
	}

	internal XmlStandalone Standalone
	{
		get
		{
			return standalone;
		}
		set
		{
			CheckReadOnly("Standalone");
			standalone = value;
		}
	}

	internal bool AutoXmlDeclaration
	{
		get
		{
			return autoXmlDecl;
		}
		set
		{
			CheckReadOnly("AutoXmlDeclaration");
			autoXmlDecl = value;
		}
	}

	internal TriState IndentInternal
	{
		get
		{
			return indent;
		}
		set
		{
			indent = value;
		}
	}

	internal bool IsQuerySpecific
	{
		get
		{
			if (cdataSections.Count == 0 && docTypePublic == null && docTypeSystem == null)
			{
				return standalone == XmlStandalone.Yes;
			}
			return true;
		}
	}

	internal bool ReadOnly
	{
		get
		{
			return isReadOnly;
		}
		set
		{
			isReadOnly = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlWriterSettings" /> class.</summary>
	public XmlWriterSettings()
	{
		Initialize();
	}

	/// <summary>Resets the members of the settings class to their default values.</summary>
	public void Reset()
	{
		CheckReadOnly("Reset");
		Initialize();
	}

	/// <summary>Creates a copy of the <see cref="T:System.Xml.XmlWriterSettings" /> instance.</summary>
	/// <returns>The cloned <see cref="T:System.Xml.XmlWriterSettings" /> object.</returns>
	public XmlWriterSettings Clone()
	{
		XmlWriterSettings obj = MemberwiseClone() as XmlWriterSettings;
		obj.cdataSections = new List<XmlQualifiedName>(cdataSections);
		obj.isReadOnly = false;
		return obj;
	}

	internal XmlWriter CreateWriter(string outputFileName)
	{
		if (outputFileName == null)
		{
			throw new ArgumentNullException("outputFileName");
		}
		XmlWriterSettings xmlWriterSettings = this;
		if (!xmlWriterSettings.CloseOutput)
		{
			xmlWriterSettings = xmlWriterSettings.Clone();
			xmlWriterSettings.CloseOutput = true;
		}
		FileStream fileStream = null;
		try
		{
			fileStream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, useAsync);
			return xmlWriterSettings.CreateWriter(fileStream);
		}
		catch
		{
			fileStream?.Close();
			throw;
		}
	}

	internal XmlWriter CreateWriter(Stream output)
	{
		if (output == null)
		{
			throw new ArgumentNullException("output");
		}
		XmlWriter xmlWriter;
		if (Encoding.WebName == "utf-8")
		{
			switch (OutputMethod)
			{
			case XmlOutputMethod.Xml:
				xmlWriter = ((!Indent) ? new XmlUtf8RawTextWriter(output, this) : new XmlUtf8RawTextWriterIndent(output, this));
				break;
			case XmlOutputMethod.Html:
				xmlWriter = ((!Indent) ? new HtmlUtf8RawTextWriter(output, this) : new HtmlUtf8RawTextWriterIndent(output, this));
				break;
			case XmlOutputMethod.Text:
				xmlWriter = new TextUtf8RawTextWriter(output, this);
				break;
			case XmlOutputMethod.AutoDetect:
				xmlWriter = new XmlAutoDetectWriter(output, this);
				break;
			default:
				return null;
			}
		}
		else
		{
			switch (OutputMethod)
			{
			case XmlOutputMethod.Xml:
				xmlWriter = ((!Indent) ? new XmlEncodedRawTextWriter(output, this) : new XmlEncodedRawTextWriterIndent(output, this));
				break;
			case XmlOutputMethod.Html:
				xmlWriter = ((!Indent) ? new HtmlEncodedRawTextWriter(output, this) : new HtmlEncodedRawTextWriterIndent(output, this));
				break;
			case XmlOutputMethod.Text:
				xmlWriter = new TextEncodedRawTextWriter(output, this);
				break;
			case XmlOutputMethod.AutoDetect:
				xmlWriter = new XmlAutoDetectWriter(output, this);
				break;
			default:
				return null;
			}
		}
		if (OutputMethod != XmlOutputMethod.AutoDetect && IsQuerySpecific)
		{
			xmlWriter = new QueryOutputWriter((XmlRawWriter)xmlWriter, this);
		}
		xmlWriter = new XmlWellFormedWriter(xmlWriter, this);
		if (useAsync)
		{
			xmlWriter = new XmlAsyncCheckWriter(xmlWriter);
		}
		return xmlWriter;
	}

	internal XmlWriter CreateWriter(TextWriter output)
	{
		if (output == null)
		{
			throw new ArgumentNullException("output");
		}
		XmlWriter xmlWriter;
		switch (OutputMethod)
		{
		case XmlOutputMethod.Xml:
			xmlWriter = ((!Indent) ? new XmlEncodedRawTextWriter(output, this) : new XmlEncodedRawTextWriterIndent(output, this));
			break;
		case XmlOutputMethod.Html:
			xmlWriter = ((!Indent) ? new HtmlEncodedRawTextWriter(output, this) : new HtmlEncodedRawTextWriterIndent(output, this));
			break;
		case XmlOutputMethod.Text:
			xmlWriter = new TextEncodedRawTextWriter(output, this);
			break;
		case XmlOutputMethod.AutoDetect:
			xmlWriter = new XmlAutoDetectWriter(output, this);
			break;
		default:
			return null;
		}
		if (OutputMethod != XmlOutputMethod.AutoDetect && IsQuerySpecific)
		{
			xmlWriter = new QueryOutputWriter((XmlRawWriter)xmlWriter, this);
		}
		xmlWriter = new XmlWellFormedWriter(xmlWriter, this);
		if (useAsync)
		{
			xmlWriter = new XmlAsyncCheckWriter(xmlWriter);
		}
		return xmlWriter;
	}

	internal XmlWriter CreateWriter(XmlWriter output)
	{
		if (output == null)
		{
			throw new ArgumentNullException("output");
		}
		return AddConformanceWrapper(output);
	}

	private void CheckReadOnly(string propertyName)
	{
		if (isReadOnly)
		{
			throw new XmlException("The '{0}' property is read only and cannot be set.", GetType().Name + "." + propertyName);
		}
	}

	private void Initialize()
	{
		encoding = Encoding.UTF8;
		omitXmlDecl = false;
		newLineHandling = NewLineHandling.Replace;
		newLineChars = Environment.NewLine;
		indent = TriState.Unknown;
		indentChars = "  ";
		newLineOnAttributes = false;
		closeOutput = false;
		namespaceHandling = NamespaceHandling.Default;
		conformanceLevel = ConformanceLevel.Document;
		checkCharacters = true;
		writeEndDocumentOnClose = true;
		outputMethod = XmlOutputMethod.Xml;
		cdataSections.Clear();
		mergeCDataSections = false;
		mediaType = null;
		docTypeSystem = null;
		docTypePublic = null;
		standalone = XmlStandalone.Omit;
		doNotEscapeUriAttributes = false;
		useAsync = false;
		isReadOnly = false;
	}

	private XmlWriter AddConformanceWrapper(XmlWriter baseWriter)
	{
		ConformanceLevel conformanceLevel = ConformanceLevel.Auto;
		XmlWriterSettings settings = baseWriter.Settings;
		bool flag = false;
		bool checkNames = false;
		bool flag2 = false;
		bool flag3 = false;
		if (settings == null)
		{
			if (newLineHandling == NewLineHandling.Replace)
			{
				flag2 = true;
				flag3 = true;
			}
			if (checkCharacters)
			{
				flag = true;
				flag3 = true;
			}
		}
		else
		{
			if (this.conformanceLevel != settings.ConformanceLevel)
			{
				conformanceLevel = ConformanceLevel;
				flag3 = true;
			}
			if (checkCharacters && !settings.CheckCharacters)
			{
				flag = true;
				checkNames = conformanceLevel == ConformanceLevel.Auto;
				flag3 = true;
			}
			if (newLineHandling == NewLineHandling.Replace && settings.NewLineHandling == NewLineHandling.None)
			{
				flag2 = true;
				flag3 = true;
			}
		}
		XmlWriter xmlWriter = baseWriter;
		if (flag3)
		{
			if (conformanceLevel != 0)
			{
				xmlWriter = new XmlWellFormedWriter(xmlWriter, this);
			}
			if (flag || flag2)
			{
				xmlWriter = new XmlCharCheckingWriter(xmlWriter, flag, checkNames, flag2, NewLineChars);
			}
		}
		if (IsQuerySpecific && (settings == null || !settings.IsQuerySpecific))
		{
			xmlWriter = new QueryOutputWriterV1(xmlWriter, this);
		}
		return xmlWriter;
	}

	internal void GetObjectData(XmlQueryDataWriter writer)
	{
		writer.Write(Encoding.CodePage);
		writer.Write(OmitXmlDeclaration);
		writer.Write((sbyte)NewLineHandling);
		writer.WriteStringQ(NewLineChars);
		writer.Write((sbyte)IndentInternal);
		writer.WriteStringQ(IndentChars);
		writer.Write(NewLineOnAttributes);
		writer.Write(CloseOutput);
		writer.Write((sbyte)ConformanceLevel);
		writer.Write(CheckCharacters);
		writer.Write((sbyte)outputMethod);
		writer.Write(cdataSections.Count);
		foreach (XmlQualifiedName cdataSection in cdataSections)
		{
			writer.Write(cdataSection.Name);
			writer.Write(cdataSection.Namespace);
		}
		writer.Write(mergeCDataSections);
		writer.WriteStringQ(mediaType);
		writer.WriteStringQ(docTypeSystem);
		writer.WriteStringQ(docTypePublic);
		writer.Write((sbyte)standalone);
		writer.Write(autoXmlDecl);
		writer.Write(ReadOnly);
	}

	internal XmlWriterSettings(XmlQueryDataReader reader)
	{
		Encoding = Encoding.GetEncoding(reader.ReadInt32());
		OmitXmlDeclaration = reader.ReadBoolean();
		NewLineHandling = (NewLineHandling)reader.ReadSByte(0, 2);
		NewLineChars = reader.ReadStringQ();
		IndentInternal = (TriState)reader.ReadSByte(-1, 1);
		IndentChars = reader.ReadStringQ();
		NewLineOnAttributes = reader.ReadBoolean();
		CloseOutput = reader.ReadBoolean();
		ConformanceLevel = (ConformanceLevel)reader.ReadSByte(0, 2);
		CheckCharacters = reader.ReadBoolean();
		outputMethod = (XmlOutputMethod)reader.ReadSByte(0, 3);
		int num = reader.ReadInt32();
		cdataSections = new List<XmlQualifiedName>(num);
		for (int i = 0; i < num; i++)
		{
			cdataSections.Add(new XmlQualifiedName(reader.ReadString(), reader.ReadString()));
		}
		mergeCDataSections = reader.ReadBoolean();
		mediaType = reader.ReadStringQ();
		docTypeSystem = reader.ReadStringQ();
		docTypePublic = reader.ReadStringQ();
		Standalone = (XmlStandalone)reader.ReadSByte(0, 2);
		autoXmlDecl = reader.ReadBoolean();
		ReadOnly = reader.ReadBoolean();
	}
}
