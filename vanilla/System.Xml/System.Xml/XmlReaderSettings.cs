using System.ComponentModel;
using System.IO;
using System.Security.Permissions;
using System.Xml.Schema;
using System.Xml.XmlConfiguration;

namespace System.Xml;

/// <summary>Specifies a set of features to support on the <see cref="T:System.Xml.XmlReader" /> object created by the <see cref="Overload:System.Xml.XmlReader.Create" /> method. </summary>
[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
public sealed class XmlReaderSettings
{
	private bool useAsync;

	private XmlNameTable nameTable;

	private XmlResolver xmlResolver;

	private int lineNumberOffset;

	private int linePositionOffset;

	private ConformanceLevel conformanceLevel;

	private bool checkCharacters;

	private long maxCharactersInDocument;

	private long maxCharactersFromEntities;

	private bool ignoreWhitespace;

	private bool ignorePIs;

	private bool ignoreComments;

	private DtdProcessing dtdProcessing;

	private ValidationType validationType;

	private XmlSchemaValidationFlags validationFlags;

	private XmlSchemaSet schemas;

	private ValidationEventHandler valEventHandler;

	private bool closeInput;

	private bool isReadOnly;

	private static bool? s_enableLegacyXmlSettings;

	/// <summary>Gets or sets whether asynchronous <see cref="T:System.Xml.XmlReader" /> methods can be used on a particular <see cref="T:System.Xml.XmlReader" /> instance.</summary>
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

	/// <summary>Gets or sets the <see cref="T:System.Xml.XmlNameTable" /> used for atomized string comparisons.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlNameTable" /> that stores all the atomized strings used by all <see cref="T:System.Xml.XmlReader" /> instances created using this <see cref="T:System.Xml.XmlReaderSettings" /> object.The default is null. The created <see cref="T:System.Xml.XmlReader" /> instance will use a new empty <see cref="T:System.Xml.NameTable" /> if this value is null.</returns>
	public XmlNameTable NameTable
	{
		get
		{
			return nameTable;
		}
		set
		{
			CheckReadOnly("NameTable");
			nameTable = value;
		}
	}

	internal bool IsXmlResolverSet { get; set; }

	/// <summary>Sets the <see cref="T:System.Xml.XmlResolver" /> used to access external documents.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlResolver" /> used to access external documents. If set to null, an <see cref="T:System.Xml.XmlException" /> is thrown when the <see cref="T:System.Xml.XmlReader" /> tries to access an external resource. The default is a new <see cref="T:System.Xml.XmlUrlResolver" /> with no credentials.</returns>
	public XmlResolver XmlResolver
	{
		set
		{
			CheckReadOnly("XmlResolver");
			xmlResolver = value;
			IsXmlResolverSet = true;
		}
	}

	/// <summary>Gets or sets line number offset of the <see cref="T:System.Xml.XmlReader" /> object.</summary>
	/// <returns>The line number offset. The default is 0.</returns>
	public int LineNumberOffset
	{
		get
		{
			return lineNumberOffset;
		}
		set
		{
			CheckReadOnly("LineNumberOffset");
			lineNumberOffset = value;
		}
	}

	/// <summary>Gets or sets line position offset of the <see cref="T:System.Xml.XmlReader" /> object.</summary>
	/// <returns>The line position offset. The default is 0.</returns>
	public int LinePositionOffset
	{
		get
		{
			return linePositionOffset;
		}
		set
		{
			CheckReadOnly("LinePositionOffset");
			linePositionOffset = value;
		}
	}

	/// <summary>Gets or sets the level of conformance which the <see cref="T:System.Xml.XmlReader" /> will comply.</summary>
	/// <returns>One of the <see cref="T:System.Xml.ConformanceLevel" /> values that specifies the level of conformance which the <see cref="T:System.Xml.XmlReader" /> will comply. The default is ConformanceLevel.Document.</returns>
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
	/// <returns>true to do character checking; otherwise false. The default is true.NoteIf the <see cref="T:System.Xml.XmlReader" /> is processing text data, it always checks that the XML names and text content are valid, regardless of the property setting. Setting <see cref="P:System.Xml.XmlReaderSettings.CheckCharacters" /> to false turns off character checking for character entity references.</returns>
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

	/// <summary>Gets or sets a value indicating the maximum allowable number of characters XML document. A zero (0) value means no limits on the size of the XML document. A non-zero value specifies the maximum size, in characters.</summary>
	/// <returns>The maximum allowable number of characters in an XML document. The default is 0.</returns>
	public long MaxCharactersInDocument
	{
		get
		{
			return maxCharactersInDocument;
		}
		set
		{
			CheckReadOnly("MaxCharactersInDocument");
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			maxCharactersInDocument = value;
		}
	}

	/// <summary>Gets or sets a value indicating the maximum allowable number of characters in a document that result from expanding entities.</summary>
	/// <returns>The maximum allowable number of characters from expanded entities. The default is 0.</returns>
	public long MaxCharactersFromEntities
	{
		get
		{
			return maxCharactersFromEntities;
		}
		set
		{
			CheckReadOnly("MaxCharactersFromEntities");
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			maxCharactersFromEntities = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether to ignore insignificant white space.</summary>
	/// <returns>true to ignore white space; otherwise false. The default is false.</returns>
	public bool IgnoreWhitespace
	{
		get
		{
			return ignoreWhitespace;
		}
		set
		{
			CheckReadOnly("IgnoreWhitespace");
			ignoreWhitespace = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether to ignore processing instructions.</summary>
	/// <returns>true to ignore processing instructions; otherwise false. The default is false.</returns>
	public bool IgnoreProcessingInstructions
	{
		get
		{
			return ignorePIs;
		}
		set
		{
			CheckReadOnly("IgnoreProcessingInstructions");
			ignorePIs = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether to ignore comments.</summary>
	/// <returns>true to ignore comments; otherwise false. The default is false.</returns>
	public bool IgnoreComments
	{
		get
		{
			return ignoreComments;
		}
		set
		{
			CheckReadOnly("IgnoreComments");
			ignoreComments = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether to prohibit document type definition (DTD) processing. This property is obsolete. Use <see cref="P:System.Xml.XmlTextReader.DtdProcessing" /> instead.</summary>
	/// <returns>true to prohibit DTD processing; otherwise false. The default is true.</returns>
	[Obsolete("Use XmlReaderSettings.DtdProcessing property instead.")]
	public bool ProhibitDtd
	{
		get
		{
			return dtdProcessing == DtdProcessing.Prohibit;
		}
		set
		{
			CheckReadOnly("ProhibitDtd");
			dtdProcessing = ((!value) ? DtdProcessing.Parse : DtdProcessing.Prohibit);
		}
	}

	/// <summary>Gets or sets a value that determines the processing of DTDs.</summary>
	/// <returns>One of the values of the <see cref="T:System.Xml.DtdProcessing" /> enumeration that determines the processing of DTDs.</returns>
	public DtdProcessing DtdProcessing
	{
		get
		{
			return dtdProcessing;
		}
		set
		{
			CheckReadOnly("DtdProcessing");
			if ((uint)value > 2u)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			dtdProcessing = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether the underlying stream or <see cref="T:System.IO.TextReader" /> should be closed when the reader is closed.</summary>
	/// <returns>true to close the underlying stream or <see cref="T:System.IO.TextReader" /> when the reader is closed; otherwise false. The default is false.</returns>
	public bool CloseInput
	{
		get
		{
			return closeInput;
		}
		set
		{
			CheckReadOnly("CloseInput");
			closeInput = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether the <see cref="T:System.Xml.XmlReader" /> will perform validation or type assignment when reading.</summary>
	/// <returns>One of the <see cref="T:System.Xml.ValidationType" /> values that indicates whether XmlReader will perform validation or type assignment when reading. The default is ValidationType.None.</returns>
	public ValidationType ValidationType
	{
		get
		{
			return validationType;
		}
		set
		{
			CheckReadOnly("ValidationType");
			if ((uint)value > 4u)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			validationType = value;
		}
	}

	/// <summary>Gets or sets a value indicating the schema validation settings. This setting applies to schema validating <see cref="T:System.Xml.XmlReader" /> objects (<see cref="P:System.Xml.XmlReaderSettings.ValidationType" /> property set to ValidationType.Schema).</summary>
	/// <returns>A set of <see cref="T:System.Xml.Schema.XmlSchemaValidationFlags" /> values. <see cref="F:System.Xml.Schema.XmlSchemaValidationFlags.ProcessIdentityConstraints" /> and <see cref="F:System.Xml.Schema.XmlSchemaValidationFlags.AllowXmlAttributes" /> are enabled by default. <see cref="F:System.Xml.Schema.XmlSchemaValidationFlags.ProcessInlineSchema" />, <see cref="F:System.Xml.Schema.XmlSchemaValidationFlags.ProcessSchemaLocation" />, and <see cref="F:System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings" /> are disabled by default.</returns>
	public XmlSchemaValidationFlags ValidationFlags
	{
		get
		{
			return validationFlags;
		}
		set
		{
			CheckReadOnly("ValidationFlags");
			if ((uint)value > 31u)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			validationFlags = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Xml.Schema.XmlSchemaSet" /> to use when performing schema validation.</summary>
	/// <returns>The <see cref="T:System.Xml.Schema.XmlSchemaSet" /> to use when performing schema validation. The default is an empty <see cref="T:System.Xml.Schema.XmlSchemaSet" /> object.</returns>
	public XmlSchemaSet Schemas
	{
		get
		{
			if (schemas == null)
			{
				schemas = new XmlSchemaSet();
			}
			return schemas;
		}
		set
		{
			CheckReadOnly("Schemas");
			schemas = value;
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

	/// <summary>Occurs when the reader encounters validation errors.</summary>
	public event ValidationEventHandler ValidationEventHandler
	{
		add
		{
			CheckReadOnly("ValidationEventHandler");
			valEventHandler = (ValidationEventHandler)Delegate.Combine(valEventHandler, value);
		}
		remove
		{
			CheckReadOnly("ValidationEventHandler");
			valEventHandler = (ValidationEventHandler)Delegate.Remove(valEventHandler, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlReaderSettings" /> class.</summary>
	public XmlReaderSettings()
	{
		Initialize();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlReaderSettings" /> class.</summary>
	/// <param name="resolver">The XML resolver.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
	public XmlReaderSettings(XmlResolver resolver)
	{
		Initialize(resolver);
	}

	internal XmlResolver GetXmlResolver()
	{
		return xmlResolver;
	}

	internal XmlResolver GetXmlResolver_CheckConfig()
	{
		if (XmlReaderSection.ProhibitDefaultUrlResolver && !IsXmlResolverSet)
		{
			return null;
		}
		return xmlResolver;
	}

	/// <summary>Resets the members of the settings class to their default values.</summary>
	public void Reset()
	{
		CheckReadOnly("Reset");
		Initialize();
	}

	/// <summary>Creates a copy of the <see cref="T:System.Xml.XmlReaderSettings" /> instance.</summary>
	/// <returns>The cloned <see cref="T:System.Xml.XmlReaderSettings" /> object.</returns>
	public XmlReaderSettings Clone()
	{
		XmlReaderSettings obj = MemberwiseClone() as XmlReaderSettings;
		obj.ReadOnly = false;
		return obj;
	}

	internal ValidationEventHandler GetEventHandler()
	{
		return valEventHandler;
	}

	internal XmlReader CreateReader(string inputUri, XmlParserContext inputContext)
	{
		if (inputUri == null)
		{
			throw new ArgumentNullException("inputUri");
		}
		if (inputUri.Length == 0)
		{
			throw new ArgumentException(Res.GetString("The string was not recognized as a valid Uri."), "inputUri");
		}
		XmlResolver xmlResolver = GetXmlResolver();
		if (xmlResolver == null)
		{
			xmlResolver = CreateDefaultResolver();
		}
		XmlReader xmlReader = new XmlTextReaderImpl(inputUri, this, inputContext, xmlResolver);
		if (ValidationType != 0)
		{
			xmlReader = AddValidation(xmlReader);
		}
		if (useAsync)
		{
			xmlReader = XmlAsyncCheckReader.CreateAsyncCheckWrapper(xmlReader);
		}
		return xmlReader;
	}

	internal XmlReader CreateReader(Stream input, Uri baseUri, string baseUriString, XmlParserContext inputContext)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (baseUriString == null)
		{
			baseUriString = ((!(baseUri == null)) ? baseUri.ToString() : string.Empty);
		}
		XmlReader xmlReader = new XmlTextReaderImpl(input, null, 0, this, baseUri, baseUriString, inputContext, closeInput);
		if (ValidationType != 0)
		{
			xmlReader = AddValidation(xmlReader);
		}
		if (useAsync)
		{
			xmlReader = XmlAsyncCheckReader.CreateAsyncCheckWrapper(xmlReader);
		}
		return xmlReader;
	}

	internal XmlReader CreateReader(TextReader input, string baseUriString, XmlParserContext inputContext)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (baseUriString == null)
		{
			baseUriString = string.Empty;
		}
		XmlReader xmlReader = new XmlTextReaderImpl(input, this, baseUriString, inputContext);
		if (ValidationType != 0)
		{
			xmlReader = AddValidation(xmlReader);
		}
		if (useAsync)
		{
			xmlReader = XmlAsyncCheckReader.CreateAsyncCheckWrapper(xmlReader);
		}
		return xmlReader;
	}

	internal XmlReader CreateReader(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		return AddValidationAndConformanceWrapper(reader);
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
		Initialize(null);
	}

	private void Initialize(XmlResolver resolver)
	{
		nameTable = null;
		if (!EnableLegacyXmlSettings())
		{
			xmlResolver = resolver;
			maxCharactersFromEntities = 10000000L;
		}
		else
		{
			xmlResolver = ((resolver == null) ? CreateDefaultResolver() : resolver);
			maxCharactersFromEntities = 0L;
		}
		lineNumberOffset = 0;
		linePositionOffset = 0;
		checkCharacters = true;
		conformanceLevel = ConformanceLevel.Document;
		ignoreWhitespace = false;
		ignorePIs = false;
		ignoreComments = false;
		dtdProcessing = DtdProcessing.Prohibit;
		closeInput = false;
		maxCharactersInDocument = 0L;
		schemas = null;
		validationType = ValidationType.None;
		validationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints;
		validationFlags |= XmlSchemaValidationFlags.AllowXmlAttributes;
		useAsync = false;
		isReadOnly = false;
		IsXmlResolverSet = false;
	}

	private static XmlResolver CreateDefaultResolver()
	{
		return new XmlUrlResolver();
	}

	internal XmlReader AddValidation(XmlReader reader)
	{
		if (validationType == ValidationType.Schema)
		{
			XmlResolver xmlResolver = GetXmlResolver_CheckConfig();
			if (xmlResolver == null && !IsXmlResolverSet && !EnableLegacyXmlSettings())
			{
				xmlResolver = new XmlUrlResolver();
			}
			reader = new XsdValidatingReader(reader, xmlResolver, this);
		}
		else if (validationType == ValidationType.DTD)
		{
			reader = CreateDtdValidatingReader(reader);
		}
		return reader;
	}

	private XmlReader AddValidationAndConformanceWrapper(XmlReader reader)
	{
		if (validationType == ValidationType.DTD)
		{
			reader = CreateDtdValidatingReader(reader);
		}
		reader = AddConformanceWrapper(reader);
		if (validationType == ValidationType.Schema)
		{
			reader = new XsdValidatingReader(reader, GetXmlResolver_CheckConfig(), this);
		}
		return reader;
	}

	private XmlValidatingReaderImpl CreateDtdValidatingReader(XmlReader baseReader)
	{
		return new XmlValidatingReaderImpl(baseReader, GetEventHandler(), (ValidationFlags & XmlSchemaValidationFlags.ProcessIdentityConstraints) != 0);
	}

	internal XmlReader AddConformanceWrapper(XmlReader baseReader)
	{
		XmlReaderSettings settings = baseReader.Settings;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool ignorePis = false;
		DtdProcessing dtdProcessing = (DtdProcessing)(-1);
		bool flag4 = false;
		if (settings == null)
		{
			if (conformanceLevel != 0 && conformanceLevel != XmlReader.GetV1ConformanceLevel(baseReader))
			{
				throw new InvalidOperationException(Res.GetString("Cannot change conformance checking to {0}. Make sure the ConformanceLevel in XmlReaderSettings is set to Auto for wrapping scenarios.", conformanceLevel.ToString()));
			}
			XmlTextReader xmlTextReader = baseReader as XmlTextReader;
			if (xmlTextReader == null && baseReader is XmlValidatingReader xmlValidatingReader)
			{
				xmlTextReader = (XmlTextReader)xmlValidatingReader.Reader;
			}
			if (ignoreWhitespace)
			{
				WhitespaceHandling whitespaceHandling = WhitespaceHandling.All;
				if (xmlTextReader != null)
				{
					whitespaceHandling = xmlTextReader.WhitespaceHandling;
				}
				if (whitespaceHandling == WhitespaceHandling.All)
				{
					flag2 = true;
					flag4 = true;
				}
			}
			if (ignoreComments)
			{
				flag3 = true;
				flag4 = true;
			}
			if (ignorePIs)
			{
				ignorePis = true;
				flag4 = true;
			}
			DtdProcessing dtdProcessing2 = DtdProcessing.Parse;
			if (xmlTextReader != null)
			{
				dtdProcessing2 = xmlTextReader.DtdProcessing;
			}
			if ((this.dtdProcessing == DtdProcessing.Prohibit && dtdProcessing2 != 0) || (this.dtdProcessing == DtdProcessing.Ignore && dtdProcessing2 == DtdProcessing.Parse))
			{
				dtdProcessing = this.dtdProcessing;
				flag4 = true;
			}
		}
		else
		{
			if (conformanceLevel != settings.ConformanceLevel && conformanceLevel != 0)
			{
				throw new InvalidOperationException(Res.GetString("Cannot change conformance checking to {0}. Make sure the ConformanceLevel in XmlReaderSettings is set to Auto for wrapping scenarios.", conformanceLevel.ToString()));
			}
			if (checkCharacters && !settings.CheckCharacters)
			{
				flag = true;
				flag4 = true;
			}
			if (ignoreWhitespace && !settings.IgnoreWhitespace)
			{
				flag2 = true;
				flag4 = true;
			}
			if (ignoreComments && !settings.IgnoreComments)
			{
				flag3 = true;
				flag4 = true;
			}
			if (ignorePIs && !settings.IgnoreProcessingInstructions)
			{
				ignorePis = true;
				flag4 = true;
			}
			if ((this.dtdProcessing == DtdProcessing.Prohibit && settings.DtdProcessing != 0) || (this.dtdProcessing == DtdProcessing.Ignore && settings.DtdProcessing == DtdProcessing.Parse))
			{
				dtdProcessing = this.dtdProcessing;
				flag4 = true;
			}
		}
		if (flag4)
		{
			if (baseReader is IXmlNamespaceResolver readerAsNSResolver)
			{
				return new XmlCharCheckingReaderWithNS(baseReader, readerAsNSResolver, flag, flag2, flag3, ignorePis, dtdProcessing);
			}
			return new XmlCharCheckingReader(baseReader, flag, flag2, flag3, ignorePis, dtdProcessing);
		}
		return baseReader;
	}

	internal static bool EnableLegacyXmlSettings()
	{
		if (s_enableLegacyXmlSettings.HasValue)
		{
			return s_enableLegacyXmlSettings.Value;
		}
		if (!BinaryCompatibility.TargetsAtLeast_Desktop_V4_5_2)
		{
			s_enableLegacyXmlSettings = true;
			return s_enableLegacyXmlSettings.Value;
		}
		s_enableLegacyXmlSettings = false;
		return s_enableLegacyXmlSettings.Value;
	}
}
