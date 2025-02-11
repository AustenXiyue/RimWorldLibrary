using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Diagnostics;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xaml;
using MS.Internal.WindowsBase;

namespace System.Windows.Baml2006;

/// <summary>Processes XAML in optimized BAML form and produces a XAML node stream.</summary>
public class Baml2006Reader : System.Xaml.XamlReader, IXamlLineInfo, IFreezeFreezables
{
	private Baml2006ReaderSettings _settings;

	private bool _isBinaryProvider;

	private bool _isEof;

	private int _lookingForAKeyOnAMarkupExtensionInADictionaryDepth;

	private XamlNodeList _lookingForAKeyOnAMarkupExtensionInADictionaryNodeList;

	private BamlBinaryReader _binaryReader;

	private Baml2006ReaderContext _context;

	private XamlNodeQueue _xamlMainNodeQueue;

	private XamlNodeList _xamlTemplateNodeList;

	private System.Xaml.XamlReader _xamlNodesReader;

	private System.Xaml.XamlWriter _xamlNodesWriter;

	private Stack<System.Xaml.XamlWriter> _xamlWriterStack = new Stack<System.Xaml.XamlWriter>();

	private Dictionary<int, TypeConverter> _typeConverterMap = new Dictionary<int, TypeConverter>();

	private Dictionary<Type, TypeConverter> _enumTypeConverterMap = new Dictionary<Type, TypeConverter>();

	private Dictionary<string, Freezable> _freezeCache;

	private const short ExtensionIdMask = 4095;

	private const short TypeExtensionValueMask = 16384;

	private const short StaticExtensionValueMask = 8192;

	private const sbyte ReaderFlags_AddedToTree = 2;

	private object _root;

	/// <summary>Gets the type of the current node.</summary>
	/// <returns>A value of the enumeration.</returns>
	public override System.Xaml.XamlNodeType NodeType => _xamlNodesReader.NodeType;

	/// <summary>Gets a value that reports whether the reader position is at the end of file.</summary>
	/// <returns>true if the reader position is at the end of the file; otherwise, false.</returns>
	public override bool IsEof => _isEof;

	/// <summary>Gets the XAML namespace from the current node.</summary>
	/// <returns>The XAML namespace if available, otherwise null.</returns>
	public override NamespaceDeclaration Namespace => _xamlNodesReader.Namespace;

	/// <summary>Gets an object that provides schema context information for the information set.</summary>
	/// <returns>An object that provides schema context information for the information set.</returns>
	public override XamlSchemaContext SchemaContext => _xamlNodesReader.SchemaContext;

	/// <summary>Gets the <see cref="T:System.Xaml.XamlType" /> of the current node.</summary>
	/// <returns>The <see cref="T:System.Xaml.XamlType" /> of the current node, or null if the position is not on an object.</returns>
	public override XamlType Type => _xamlNodesReader.Type;

	/// <summary>Gets the value of the current node.</summary>
	/// <returns>The value of the current node, or null if the position is not on a <see cref="F:System.Xaml.XamlNodeType.Value" /> node type.</returns>
	public override object Value => _xamlNodesReader.Value;

	/// <summary>Gets the current member at the reader position, if the reader position is on a <see cref="F:System.Xaml.XamlNodeType.StartMember" />.</summary>
	/// <returns>The current member, or null if the position is not on a member.</returns>
	public override XamlMember Member => _xamlNodesReader.Member;

	/// <summary>See <see cref="P:System.Xaml.IXamlLineInfo.HasLineInfo" />.</summary>
	/// <returns>true if line information is available; otherwise, false.</returns>
	bool IXamlLineInfo.HasLineInfo => _context.CurrentFrame != null;

	/// <summary>See <see cref="P:System.Xaml.IXamlLineInfo.LineNumber" />.</summary>
	/// <returns>The line number to report.</returns>
	int IXamlLineInfo.LineNumber => _context.LineNumber;

	/// <summary>See <see cref="P:System.Xaml.IXamlLineInfo.LinePosition" />.</summary>
	/// <returns>The line position to report.</returns>
	int IXamlLineInfo.LinePosition => _context.LineOffset;

	internal bool FreezeFreezables
	{
		get
		{
			return _context.CurrentFrame.FreezeFreezables;
		}
		set
		{
			_context.CurrentFrame.FreezeFreezables = value;
		}
	}

	bool IFreezeFreezables.FreezeFreezables => _context.CurrentFrame.FreezeFreezables;

	private Baml2006SchemaContext BamlSchemaContext => (Baml2006SchemaContext)SchemaContext;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Baml2006.Baml2006Reader" /> class, based on the file name of a local file to read.</summary>
	/// <param name="fileName">String that declares a file path to the file that contains BAML to read.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="fileName" /> is null.</exception>
	public Baml2006Reader(string fileName)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
		Baml2006SchemaContext schemaContext = new Baml2006SchemaContext(null);
		Baml2006ReaderSettings baml2006ReaderSettings = System.Windows.Markup.XamlReader.CreateBamlReaderSettings();
		baml2006ReaderSettings.OwnsStream = true;
		Initialize(stream, schemaContext, baml2006ReaderSettings);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Baml2006.Baml2006Reader" /> class based on an input stream.</summary>
	/// <param name="stream">Input stream of source BAML.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null.</exception>
	public Baml2006Reader(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		Baml2006SchemaContext schemaContext = new Baml2006SchemaContext(null);
		Baml2006ReaderSettings settings = new Baml2006ReaderSettings();
		Initialize(stream, schemaContext, settings);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Baml2006.Baml2006Reader" /> class based on an input stream and reader settings.</summary>
	/// <param name="stream">Input stream of source BAML.</param>
	/// <param name="xamlReaderSettings">Reader settings. See Remarks.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> or <paramref name="xamlReaderSettings" /> is null.</exception>
	public Baml2006Reader(Stream stream, XamlReaderSettings xamlReaderSettings)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (xamlReaderSettings == null)
		{
			throw new ArgumentNullException("xamlReaderSettings");
		}
		Baml2006SchemaContext schemaContext = ((!xamlReaderSettings.ValuesMustBeString) ? new Baml2006SchemaContext(xamlReaderSettings.LocalAssembly) : new Baml2006SchemaContext(xamlReaderSettings.LocalAssembly, System.Windows.Markup.XamlReader.XamlV3SharedSchemaContext));
		Baml2006ReaderSettings settings = new Baml2006ReaderSettings(xamlReaderSettings);
		Initialize(stream, schemaContext, settings);
	}

	internal Baml2006Reader(Stream stream, Baml2006SchemaContext schemaContext, Baml2006ReaderSettings settings)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (schemaContext == null)
		{
			throw new ArgumentNullException("schemaContext");
		}
		Initialize(stream, schemaContext, settings ?? new Baml2006ReaderSettings());
	}

	internal Baml2006Reader(Stream stream, Baml2006SchemaContext baml2006SchemaContext, Baml2006ReaderSettings baml2006ReaderSettings, object root)
		: this(stream, baml2006SchemaContext, baml2006ReaderSettings)
	{
		_root = root;
	}

	private void Initialize(Stream stream, Baml2006SchemaContext schemaContext, Baml2006ReaderSettings settings)
	{
		schemaContext.Settings = settings;
		_settings = settings;
		_context = new Baml2006ReaderContext(schemaContext);
		_xamlMainNodeQueue = new XamlNodeQueue(schemaContext);
		_xamlNodesReader = _xamlMainNodeQueue.Reader;
		_xamlNodesWriter = _xamlMainNodeQueue.Writer;
		_lookingForAKeyOnAMarkupExtensionInADictionaryDepth = -1;
		_isBinaryProvider = !settings.ValuesMustBeString;
		if (_settings.OwnsStream)
		{
			stream = new SharedStream(stream);
		}
		_binaryReader = new BamlBinaryReader(stream);
		_context.TemplateStartDepth = -1;
		if (!_settings.IsBamlFragment)
		{
			Process_Header();
		}
	}

	/// <summary>Provides the next XAML node from the source BAML, if a node is available. </summary>
	/// <returns>true if a node is available; otherwise, false.</returns>
	/// <exception cref="T:System.ObjectDisposedException">Reader was disposed during traversal.</exception>
	public override bool Read()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException("Baml2006Reader");
		}
		if (IsEof)
		{
			return false;
		}
		while (!_xamlNodesReader.Read())
		{
			if (!Process_BamlRecords())
			{
				_isEof = true;
				return false;
			}
		}
		if (_binaryReader.BaseStream.Length == _binaryReader.BaseStream.Position && _xamlNodesReader.NodeType != System.Xaml.XamlNodeType.EndObject)
		{
			_isEof = true;
			return false;
		}
		return true;
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Windows.Baml2006.Baml2006Reader" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release the managed resources; otherwise, false.</param>
	protected override void Dispose(bool disposing)
	{
		if (_binaryReader != null)
		{
			if (_settings.OwnsStream && _binaryReader.BaseStream is SharedStream { SharedCount: <1 })
			{
				_binaryReader.Close();
			}
			_binaryReader = null;
			_context = null;
		}
	}

	internal List<KeyRecord> ReadKeys()
	{
		_context.KeyList = new List<KeyRecord>();
		_context.CurrentFrame.IsDeferredContent = true;
		bool flag = true;
		while (flag)
		{
			switch (Read_RecordType())
			{
			case Baml2006RecordType.KeyElementStart:
				Process_KeyElementStart();
				while (true)
				{
					Baml2006RecordType baml2006RecordType = Read_RecordType();
					if (baml2006RecordType == Baml2006RecordType.KeyElementEnd)
					{
						break;
					}
					_binaryReader.BaseStream.Seek(-1L, SeekOrigin.Current);
					Process_OneBamlRecord();
				}
				Process_KeyElementEnd();
				break;
			case Baml2006RecordType.StaticResourceStart:
				Process_StaticResourceStart();
				while (true)
				{
					Baml2006RecordType baml2006RecordType = Read_RecordType();
					if (baml2006RecordType == Baml2006RecordType.StaticResourceEnd)
					{
						break;
					}
					_binaryReader.BaseStream.Seek(-1L, SeekOrigin.Current);
					Process_OneBamlRecord();
				}
				Process_StaticResourceEnd();
				break;
			case Baml2006RecordType.DefAttributeKeyType:
				Process_DefAttributeKeyType();
				break;
			case Baml2006RecordType.DefAttributeKeyString:
				Process_DefAttributeKeyString();
				break;
			case Baml2006RecordType.OptimizedStaticResource:
				Process_OptimizedStaticResource();
				break;
			default:
				flag = false;
				_binaryReader.BaseStream.Seek(-1L, SeekOrigin.Current);
				break;
			}
			if (_binaryReader.BaseStream.Length == _binaryReader.BaseStream.Position)
			{
				flag = false;
				break;
			}
		}
		KeyRecord keyRecord = null;
		long position = _binaryReader.BaseStream.Position;
		foreach (KeyRecord key in _context.KeyList)
		{
			key.ValuePosition += position;
			if (keyRecord != null)
			{
				keyRecord.ValueSize = (int)(key.ValuePosition - keyRecord.ValuePosition);
			}
			keyRecord = key;
		}
		keyRecord.ValueSize = (int)(_binaryReader.BaseStream.Length - keyRecord.ValuePosition);
		flag = false;
		return _context.KeyList;
	}

	internal System.Xaml.XamlReader ReadObject(KeyRecord record)
	{
		if (record.ValuePosition == _binaryReader.BaseStream.Length)
		{
			return null;
		}
		_binaryReader.BaseStream.Seek(record.ValuePosition, SeekOrigin.Begin);
		_context.CurrentKey = _context.KeyList.IndexOf(record);
		if (_xamlMainNodeQueue.Count > 0)
		{
			throw new System.Xaml.XamlParseException();
		}
		if (Read_RecordType() != Baml2006RecordType.ElementStart)
		{
			throw new System.Xaml.XamlParseException();
		}
		System.Xaml.XamlWriter xamlNodesWriter = _xamlNodesWriter;
		int num = ((record.ValueSize < 800) ? ((int)((double)record.ValueSize / 2.2)) : ((int)((double)record.ValueSize / 4.25)));
		num = ((num < 8) ? 8 : num);
		XamlNodeList xamlNodeList = new XamlNodeList(_xamlNodesReader.SchemaContext, num);
		_xamlNodesWriter = xamlNodeList.Writer;
		Baml2006ReaderFrame currentFrame = _context.CurrentFrame;
		Process_ElementStart();
		while (currentFrame != _context.CurrentFrame)
		{
			Process_OneBamlRecord();
		}
		_xamlNodesWriter.Close();
		_xamlNodesWriter = xamlNodesWriter;
		return xamlNodeList.GetReader();
	}

	internal Type GetTypeOfFirstStartObject(KeyRecord record)
	{
		_context.CurrentKey = _context.KeyList.IndexOf(record);
		if (record.ValuePosition == _binaryReader.BaseStream.Length)
		{
			return null;
		}
		_binaryReader.BaseStream.Seek(record.ValuePosition, SeekOrigin.Begin);
		if (Read_RecordType() != Baml2006RecordType.ElementStart)
		{
			throw new System.Xaml.XamlParseException();
		}
		return BamlSchemaContext.GetClrType(_binaryReader.ReadInt16());
	}

	private bool Process_BamlRecords()
	{
		int count = _xamlMainNodeQueue.Count;
		while (Process_OneBamlRecord())
		{
			if (_xamlMainNodeQueue.Count > count)
			{
				return true;
			}
		}
		return false;
	}

	private bool Process_OneBamlRecord()
	{
		if (_binaryReader.BaseStream.Position == _binaryReader.BaseStream.Length)
		{
			_isEof = true;
			return false;
		}
		Baml2006RecordType baml2006RecordType = Read_RecordType();
		switch (baml2006RecordType)
		{
		case Baml2006RecordType.DocumentStart:
			SkipBytes(6L);
			break;
		case Baml2006RecordType.DocumentEnd:
			return false;
		case Baml2006RecordType.ElementStart:
			Process_ElementStart();
			break;
		case Baml2006RecordType.ElementEnd:
			Process_ElementEnd();
			break;
		case Baml2006RecordType.NamedElementStart:
			throw new System.Xaml.XamlParseException();
		case Baml2006RecordType.KeyElementStart:
			Process_KeyElementStart();
			break;
		case Baml2006RecordType.KeyElementEnd:
			Process_KeyElementEnd();
			break;
		case Baml2006RecordType.XmlnsProperty:
			throw new System.Xaml.XamlParseException("Found unexpected Xmlns BAML record");
		case Baml2006RecordType.Property:
			Process_Property();
			break;
		case Baml2006RecordType.PropertyCustom:
			Process_PropertyCustom();
			break;
		case Baml2006RecordType.PropertyWithConverter:
			Process_PropertyWithConverter();
			break;
		case Baml2006RecordType.PropertyWithExtension:
			Process_PropertyWithExtension();
			break;
		case Baml2006RecordType.PropertyTypeReference:
			Process_PropertyTypeReference();
			break;
		case Baml2006RecordType.PropertyStringReference:
			Process_PropertyStringReference();
			break;
		case Baml2006RecordType.PropertyWithStaticResourceId:
			Process_PropertyWithStaticResourceId();
			break;
		case Baml2006RecordType.ContentProperty:
			Process_ContentProperty();
			break;
		case Baml2006RecordType.RoutedEvent:
			Process_RoutedEvent();
			break;
		case Baml2006RecordType.ClrEvent:
			Process_ClrEvent();
			break;
		case Baml2006RecordType.ConstructorParametersStart:
			Process_ConstructorParametersStart();
			break;
		case Baml2006RecordType.ConstructorParameterType:
			Process_ConstructorParameterType();
			break;
		case Baml2006RecordType.ConstructorParametersEnd:
			Process_ConstructorParametersEnd();
			break;
		case Baml2006RecordType.PropertyComplexStart:
			Process_PropertyComplexStart();
			break;
		case Baml2006RecordType.PropertyArrayStart:
		case Baml2006RecordType.PropertyIListStart:
			Process_PropertyArrayStart();
			break;
		case Baml2006RecordType.PropertyIDictionaryStart:
			Process_PropertyIDictionaryStart();
			break;
		case Baml2006RecordType.PropertyComplexEnd:
		case Baml2006RecordType.PropertyArrayEnd:
		case Baml2006RecordType.PropertyIListEnd:
			Process_PropertyEnd();
			break;
		case Baml2006RecordType.PropertyIDictionaryEnd:
			Process_PropertyIDictionaryEnd();
			break;
		case Baml2006RecordType.StaticResourceId:
			Process_StaticResourceId();
			break;
		case Baml2006RecordType.StaticResourceStart:
			Process_StaticResourceStart();
			break;
		case Baml2006RecordType.StaticResourceEnd:
			Process_StaticResourceEnd();
			break;
		case Baml2006RecordType.OptimizedStaticResource:
			Process_OptimizedStaticResource();
			break;
		case Baml2006RecordType.Text:
			Process_Text();
			break;
		case Baml2006RecordType.TextWithConverter:
			Process_TextWithConverter();
			break;
		case Baml2006RecordType.TextWithId:
			Process_TextWithId();
			break;
		case Baml2006RecordType.LiteralContent:
			Process_LiteralContent();
			break;
		case Baml2006RecordType.DefAttribute:
			Process_DefAttribute();
			break;
		case Baml2006RecordType.DefAttributeKeyString:
			Process_DefAttributeKeyString();
			break;
		case Baml2006RecordType.DefAttributeKeyType:
			Process_DefAttributeKeyType();
			break;
		case Baml2006RecordType.DefTag:
			Process_DefTag();
			break;
		case Baml2006RecordType.DeferableContentStart:
			Process_DeferableContentStart();
			break;
		case Baml2006RecordType.EndAttributes:
			Process_EndAttributes();
			break;
		case Baml2006RecordType.XmlAttribute:
			Process_XmlAttribute();
			break;
		case Baml2006RecordType.PresentationOptionsAttribute:
			Process_PresentationOptionsAttribute();
			break;
		case Baml2006RecordType.ProcessingInstruction:
			Process_ProcessingInstruction();
			break;
		case Baml2006RecordType.PIMapping:
			Process_PIMapping();
			break;
		case Baml2006RecordType.AssemblyInfo:
			Process_AssemblyInfo();
			break;
		case Baml2006RecordType.TypeInfo:
			Process_TypeInfo();
			break;
		case Baml2006RecordType.TypeSerializerInfo:
			Process_TypeSerializerInfo();
			break;
		case Baml2006RecordType.AttributeInfo:
			Process_AttributeInfo();
			break;
		case Baml2006RecordType.StringInfo:
			Process_StringInfo();
			break;
		case Baml2006RecordType.LinePosition:
			Process_LinePosition();
			break;
		case Baml2006RecordType.LineNumberAndPosition:
			Process_LineNumberAndPosition();
			break;
		case Baml2006RecordType.Comment:
			Process_Comment();
			break;
		case Baml2006RecordType.ConnectionId:
			Process_ConnectionId();
			break;
		default:
			throw new System.Xaml.XamlParseException(string.Format(CultureInfo.CurrentCulture, SR.Format(SR.UnknownBamlRecord, baml2006RecordType)));
		}
		return true;
	}

	private void Process_ProcessingInstruction()
	{
		throw new NotImplementedException();
	}

	private void Process_DefTag()
	{
		throw new NotImplementedException();
	}

	private void Process_EndAttributes()
	{
		throw new NotImplementedException();
	}

	private void Process_XmlAttribute()
	{
		throw new NotImplementedException();
	}

	private void Process_PresentationOptionsAttribute()
	{
		Common_Process_Property();
		Read_RecordSize();
		string value = _binaryReader.ReadString();
		_context.SchemaContext.GetString(_binaryReader.ReadInt16());
		if (_context.TemplateStartDepth < 0)
		{
			_xamlNodesWriter.WriteStartMember(XamlReaderHelper.Freeze);
			_xamlNodesWriter.WriteValue(value);
			_xamlNodesWriter.WriteEndMember();
		}
	}

	private void Process_Comment()
	{
		throw new NotImplementedException();
	}

	private void Process_LiteralContent()
	{
		Read_RecordSize();
		string text = _binaryReader.ReadString();
		_binaryReader.ReadInt32();
		_binaryReader.ReadInt32();
		bool num = _context.CurrentFrame.Member == null;
		if (num)
		{
			if (!(_context.CurrentFrame.XamlType.ContentProperty != null))
			{
				throw new NotImplementedException();
			}
			Common_Process_Property();
			_xamlNodesWriter.WriteStartMember(_context.CurrentFrame.XamlType.ContentProperty);
		}
		if (!_isBinaryProvider)
		{
			_xamlNodesWriter.WriteStartObject(XamlLanguage.XData);
			XamlMember member = XamlLanguage.XData.GetMember("Text");
			_xamlNodesWriter.WriteStartMember(member);
			_xamlNodesWriter.WriteValue(text);
			_xamlNodesWriter.WriteEndMember();
			_xamlNodesWriter.WriteEndObject();
		}
		else
		{
			XData value = new XData
			{
				Text = text
			};
			_xamlNodesWriter.WriteValue(value);
		}
		if (num)
		{
			_xamlNodesWriter.WriteEndMember();
		}
	}

	private void Process_TextWithConverter()
	{
		Read_RecordSize();
		string value = _binaryReader.ReadString();
		_binaryReader.ReadInt16();
		bool num = _context.CurrentFrame.Member == null;
		if (num)
		{
			Common_Process_Property();
			_xamlNodesWriter.WriteStartMember(XamlLanguage.Initialization);
		}
		_xamlNodesWriter.WriteValue(value);
		if (num)
		{
			_xamlNodesWriter.WriteEndMember();
		}
	}

	private void Process_StaticResourceEnd()
	{
		System.Xaml.XamlWriter writer = GetLastStaticResource().ResourceNodeList.Writer;
		writer.WriteEndObject();
		writer.Close();
		_context.InsideStaticResource = false;
		_xamlNodesWriter = _xamlWriterStack.Pop();
		_context.PopScope();
	}

	private void Process_StaticResourceStart()
	{
		XamlType xamlType = BamlSchemaContext.GetXamlType(_binaryReader.ReadInt16());
		_binaryReader.ReadByte();
		StaticResource staticResource = new StaticResource(xamlType, BamlSchemaContext);
		_context.LastKey.StaticResources.Add(staticResource);
		_context.InsideStaticResource = true;
		_xamlWriterStack.Push(_xamlNodesWriter);
		_xamlNodesWriter = staticResource.ResourceNodeList.Writer;
		_context.PushScope();
		_context.CurrentFrame.XamlType = xamlType;
	}

	private void Process_StaticResourceId()
	{
		InjectPropertyAndFrameIfNeeded(_context.SchemaContext.GetXamlType(typeof(StaticResourceExtension)), 0);
		short index = _binaryReader.ReadInt16();
		object obj = _context.KeyList[_context.CurrentKey - 1].StaticResources[index];
		if (obj is StaticResource staticResource)
		{
			XamlServices.Transform(staticResource.ResourceNodeList.GetReader(), _xamlNodesWriter, closeWriter: false);
		}
		else
		{
			_xamlNodesWriter.WriteValue(obj);
		}
	}

	private void Process_ClrEvent()
	{
		throw new NotImplementedException();
	}

	private void Process_RoutedEvent()
	{
		throw new NotImplementedException();
	}

	private void Process_PropertyStringReference()
	{
		throw new NotImplementedException();
	}

	private void Process_OptimizedStaticResource()
	{
		byte flags = _binaryReader.ReadByte();
		short num = _binaryReader.ReadInt16();
		OptimizedStaticResource optimizedStaticResource = new OptimizedStaticResource(flags, num);
		if (_isBinaryProvider)
		{
			if (optimizedStaticResource.IsKeyTypeExtension)
			{
				XamlType xamlType = BamlSchemaContext.GetXamlType(num);
				optimizedStaticResource.KeyValue = xamlType.UnderlyingType;
			}
			else if (optimizedStaticResource.IsKeyStaticExtension)
			{
				Type memberType;
				object providedValue;
				string staticExtensionValue = GetStaticExtensionValue(num, out memberType, out providedValue);
				if (providedValue == null)
				{
					providedValue = new StaticExtension(staticExtensionValue)
					{
						MemberType = memberType
					}.ProvideValue(null);
				}
				optimizedStaticResource.KeyValue = providedValue;
			}
			else
			{
				optimizedStaticResource.KeyValue = _context.SchemaContext.GetString(num);
			}
		}
		_context.LastKey.StaticResources.Add(optimizedStaticResource);
	}

	private void Process_DeferableContentStart()
	{
		int num = _binaryReader.ReadInt32();
		if (_isBinaryProvider && num > 0)
		{
			object obj = null;
			if (_settings.OwnsStream)
			{
				long position = _binaryReader.BaseStream.Position;
				obj = new SharedStream(_binaryReader.BaseStream, position, num);
				_binaryReader.BaseStream.Seek(position + num, SeekOrigin.Begin);
			}
			else
			{
				obj = new MemoryStream(_binaryReader.ReadBytes(num));
			}
			Common_Process_Property();
			_xamlNodesWriter.WriteStartMember(BamlSchemaContext.ResourceDictionaryDeferredContentProperty);
			_xamlNodesWriter.WriteValue(obj);
			_xamlNodesWriter.WriteEndMember();
		}
		else
		{
			_context.KeyList = new List<KeyRecord>();
			_context.CurrentKey = 0;
			_context.CurrentFrame.IsDeferredContent = true;
		}
	}

	private void Process_DefAttribute()
	{
		Read_RecordSize();
		string text = _binaryReader.ReadString();
		short stringId = _binaryReader.ReadInt16();
		XamlMember xamlDirective = BamlSchemaContext.GetXamlDirective("http://schemas.microsoft.com/winfx/2006/xaml", BamlSchemaContext.GetString(stringId));
		if (xamlDirective == XamlLanguage.Key)
		{
			_context.CurrentFrame.Key = new KeyRecord(shared: false, sharedSet: false, 0, text);
			return;
		}
		Common_Process_Property();
		_xamlNodesWriter.WriteStartMember(xamlDirective);
		_xamlNodesWriter.WriteValue(text);
		_xamlNodesWriter.WriteEndMember();
	}

	private void Process_DefAttributeKeyString()
	{
		Read_RecordSize();
		short stringId = _binaryReader.ReadInt16();
		int valuePosition = _binaryReader.ReadInt32();
		bool shared = _binaryReader.ReadBoolean();
		bool sharedSet = _binaryReader.ReadBoolean();
		string @string = _context.SchemaContext.GetString(stringId);
		KeyRecord keyRecord = new KeyRecord(shared, sharedSet, valuePosition, @string);
		if (_context.CurrentFrame.IsDeferredContent)
		{
			_context.KeyList.Add(keyRecord);
		}
		else
		{
			_context.CurrentFrame.Key = keyRecord;
		}
	}

	private void Process_DefAttributeKeyType()
	{
		short typeId = _binaryReader.ReadInt16();
		_binaryReader.ReadByte();
		int valuePosition = _binaryReader.ReadInt32();
		bool shared = _binaryReader.ReadBoolean();
		bool sharedSet = _binaryReader.ReadBoolean();
		Type type = Baml2006SchemaContext.KnownTypes.GetKnownType(typeId);
		if (type == null)
		{
			type = BamlSchemaContext.GetClrType(typeId);
		}
		KeyRecord keyRecord = new KeyRecord(shared, sharedSet, valuePosition, type);
		if (_context.CurrentFrame.IsDeferredContent)
		{
			_context.KeyList.Add(keyRecord);
		}
		else
		{
			_context.CurrentFrame.Key = keyRecord;
		}
	}

	private bool IsStringOnlyWhiteSpace(string value)
	{
		for (int i = 0; i < value.Length; i++)
		{
			if (!char.IsWhiteSpace(value[i]))
			{
				return false;
			}
		}
		return true;
	}

	private void Process_Text()
	{
		Read_RecordSize();
		string stringValue = _binaryReader.ReadString();
		Process_Text_Helper(stringValue);
	}

	private void Process_TextWithId()
	{
		Read_RecordSize();
		short stringId = _binaryReader.ReadInt16();
		string @string = BamlSchemaContext.GetString(stringId);
		Process_Text_Helper(@string);
	}

	private void Process_Text_Helper(string stringValue)
	{
		if (!_context.InsideKeyRecord && !_context.InsideStaticResource)
		{
			InjectPropertyAndFrameIfNeeded(_context.SchemaContext.GetXamlType(typeof(string)), 0);
		}
		if (IsStringOnlyWhiteSpace(stringValue) && _context.CurrentFrame.Member != XamlLanguage.PositionalParameters)
		{
			if (_context.CurrentFrame.XamlType != null && _context.CurrentFrame.XamlType.IsCollection)
			{
				if (!_context.CurrentFrame.XamlType.IsWhitespaceSignificantCollection)
				{
					return;
				}
			}
			else if (_context.CurrentFrame.Member.Type != null && !_context.CurrentFrame.Member.Type.UnderlyingType.IsAssignableFrom(typeof(string)))
			{
				return;
			}
		}
		_xamlNodesWriter.WriteValue(stringValue);
	}

	private void Process_ConstructorParametersEnd()
	{
		_xamlNodesWriter.WriteEndMember();
		_context.CurrentFrame.Member = null;
	}

	private void Process_ConstructorParametersStart()
	{
		Common_Process_Property();
		_xamlNodesWriter.WriteStartMember(XamlLanguage.PositionalParameters);
		_context.CurrentFrame.Member = XamlLanguage.PositionalParameters;
	}

	private void Process_ConstructorParameterType()
	{
		short typeId = _binaryReader.ReadInt16();
		if (_isBinaryProvider)
		{
			_xamlNodesWriter.WriteValue(BamlSchemaContext.GetXamlType(typeId).UnderlyingType);
			return;
		}
		_xamlNodesWriter.WriteStartObject(XamlLanguage.Type);
		_xamlNodesWriter.WriteStartMember(XamlLanguage.PositionalParameters);
		_xamlNodesWriter.WriteValue(Logic_GetFullyQualifiedNameForType(BamlSchemaContext.GetXamlType(typeId)));
		_xamlNodesWriter.WriteEndMember();
		_xamlNodesWriter.WriteEndObject();
	}

	private void Process_Header()
	{
		int num = _binaryReader.ReadInt32() + 12;
		Stream baseStream = _binaryReader.BaseStream;
		if (baseStream.CanSeek)
		{
			baseStream.Position += num;
			return;
		}
		byte[] array = ArrayPool<byte>.Shared.Rent(num);
		int num2;
		for (int i = 0; i < num; i += num2)
		{
			if ((num2 = baseStream.Read(array, 0, num - i)) <= 0)
			{
				break;
			}
		}
		ArrayPool<byte>.Shared.Return(array);
	}

	private void Process_ElementStart()
	{
		short typeId = _binaryReader.ReadInt16();
		XamlType xamlType;
		if (_root != null && _context.CurrentFrame.Depth == 0)
		{
			Type type = _root.GetType();
			xamlType = BamlSchemaContext.GetXamlType(type);
		}
		else
		{
			xamlType = BamlSchemaContext.GetXamlType(typeId);
		}
		sbyte b = _binaryReader.ReadSByte();
		if (b < 0 || b > 3)
		{
			throw new System.Xaml.XamlParseException();
		}
		InjectPropertyAndFrameIfNeeded(xamlType, b);
		_context.PushScope();
		_context.CurrentFrame.XamlType = xamlType;
		bool flag = true;
		do
		{
			switch (Read_RecordType())
			{
			case Baml2006RecordType.XmlnsProperty:
				Process_XmlnsProperty();
				break;
			case Baml2006RecordType.AssemblyInfo:
				Process_AssemblyInfo();
				break;
			case Baml2006RecordType.LinePosition:
				Process_LinePosition();
				break;
			case Baml2006RecordType.LineNumberAndPosition:
				Process_LineNumberAndPosition();
				break;
			default:
				SkipBytes(-1L);
				flag = false;
				break;
			}
		}
		while (flag);
		if ((b & 2) > 0)
		{
			_xamlNodesWriter.WriteGetObject();
		}
		else
		{
			_xamlNodesWriter.WriteStartObject(_context.CurrentFrame.XamlType);
		}
		if (_context.CurrentFrame.Depth == 1 && _settings.BaseUri != null && !string.IsNullOrEmpty(_settings.BaseUri.ToString()))
		{
			_xamlNodesWriter.WriteStartMember(XamlLanguage.Base);
			_xamlNodesWriter.WriteValue(_settings.BaseUri.ToString());
			_xamlNodesWriter.WriteEndMember();
		}
		if (!_context.PreviousFrame.IsDeferredContent || _context.InsideStaticResource)
		{
			return;
		}
		if (!_isBinaryProvider)
		{
			_xamlNodesWriter.WriteStartMember(XamlLanguage.Key);
			KeyRecord keyRecord = _context.KeyList[_context.CurrentKey];
			if (!string.IsNullOrEmpty(keyRecord.KeyString))
			{
				_xamlNodesWriter.WriteValue(keyRecord.KeyString);
			}
			else if (keyRecord.KeyType != null)
			{
				_xamlNodesWriter.WriteStartObject(XamlLanguage.Type);
				_xamlNodesWriter.WriteStartMember(XamlLanguage.PositionalParameters);
				_xamlNodesWriter.WriteValue(Logic_GetFullyQualifiedNameForType(SchemaContext.GetXamlType(keyRecord.KeyType)));
				_xamlNodesWriter.WriteEndMember();
				_xamlNodesWriter.WriteEndObject();
			}
			else
			{
				XamlServices.Transform(keyRecord.KeyNodeList.GetReader(), _xamlNodesWriter, closeWriter: false);
			}
			_xamlNodesWriter.WriteEndMember();
		}
		_context.CurrentKey++;
	}

	private void Process_ElementEnd()
	{
		RemoveImplicitFrame();
		if (_context.CurrentFrame.Key != null)
		{
			_xamlNodesWriter.WriteStartMember(XamlLanguage.Key);
			KeyRecord key = _context.CurrentFrame.Key;
			if (key.KeyType != null)
			{
				if (_isBinaryProvider)
				{
					_xamlNodesWriter.WriteValue(key.KeyType);
				}
				else
				{
					_xamlNodesWriter.WriteStartObject(XamlLanguage.Type);
					_xamlNodesWriter.WriteStartMember(XamlLanguage.PositionalParameters);
					_xamlNodesWriter.WriteValue(Logic_GetFullyQualifiedNameForType(SchemaContext.GetXamlType(key.KeyType)));
					_xamlNodesWriter.WriteEndMember();
					_xamlNodesWriter.WriteEndObject();
				}
			}
			else if (key.KeyNodeList != null)
			{
				XamlServices.Transform(key.KeyNodeList.GetReader(), _xamlNodesWriter, closeWriter: false);
			}
			else
			{
				_xamlNodesWriter.WriteValue(key.KeyString);
			}
			_xamlNodesWriter.WriteEndMember();
			_context.CurrentFrame.Key = null;
		}
		if (_context.CurrentFrame.DelayedConnectionId != -1)
		{
			_xamlNodesWriter.WriteStartMember(XamlLanguage.ConnectionId);
			if (_isBinaryProvider)
			{
				_xamlNodesWriter.WriteValue(_context.CurrentFrame.DelayedConnectionId);
			}
			else
			{
				_xamlNodesWriter.WriteValue(_context.CurrentFrame.DelayedConnectionId.ToString(System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS));
			}
			_xamlNodesWriter.WriteEndMember();
		}
		_xamlNodesWriter.WriteEndObject();
		if (_context.CurrentFrame.IsDeferredContent)
		{
			_context.KeyList = null;
		}
		_context.PopScope();
	}

	private void Process_KeyElementStart()
	{
		short typeId = _binaryReader.ReadInt16();
		byte flags = _binaryReader.ReadByte();
		int valuePosition = _binaryReader.ReadInt32();
		bool shared = _binaryReader.ReadBoolean();
		bool sharedSet = _binaryReader.ReadBoolean();
		XamlType xamlType = _context.SchemaContext.GetXamlType(typeId);
		_context.PushScope();
		_context.CurrentFrame.XamlType = xamlType;
		KeyRecord keyRecord = new KeyRecord(shared, sharedSet, valuePosition, _context.SchemaContext);
		keyRecord.Flags = flags;
		keyRecord.KeyNodeList.Writer.WriteStartObject(xamlType);
		_context.InsideKeyRecord = true;
		_xamlWriterStack.Push(_xamlNodesWriter);
		_xamlNodesWriter = keyRecord.KeyNodeList.Writer;
		if (_context.PreviousFrame.IsDeferredContent)
		{
			_context.KeyList.Add(keyRecord);
		}
		else
		{
			_context.PreviousFrame.Key = keyRecord;
		}
	}

	private void Process_KeyElementEnd()
	{
		KeyRecord keyRecord = null;
		keyRecord = ((!_context.PreviousFrame.IsDeferredContent) ? _context.PreviousFrame.Key : _context.LastKey);
		keyRecord.KeyNodeList.Writer.WriteEndObject();
		keyRecord.KeyNodeList.Writer.Close();
		_xamlNodesWriter = _xamlWriterStack.Pop();
		_context.InsideKeyRecord = false;
		_context.PopScope();
	}

	private void Process_Property()
	{
		Common_Process_Property();
		Read_RecordSize();
		if (_context.CurrentFrame.XamlType.UnderlyingType == typeof(EventSetter))
		{
			_xamlNodesWriter.WriteStartMember(_context.SchemaContext.EventSetterEventProperty);
			XamlMember property = GetProperty(_binaryReader.ReadInt16(), isAttached: false);
			Type type = property.DeclaringType.UnderlyingType;
			while (null != type)
			{
				SecurityHelper.RunClassConstructor(type);
				type = type.BaseType;
			}
			RoutedEvent routedEventFromName = EventManager.GetRoutedEventFromName(property.Name, property.DeclaringType.UnderlyingType);
			_xamlNodesWriter.WriteValue(routedEventFromName);
			_xamlNodesWriter.WriteEndMember();
			_xamlNodesWriter.WriteStartMember(_context.SchemaContext.EventSetterHandlerProperty);
			_xamlNodesWriter.WriteValue(_binaryReader.ReadString());
			_xamlNodesWriter.WriteEndMember();
		}
		else
		{
			XamlMember property2 = GetProperty(_binaryReader.ReadInt16(), _context.CurrentFrame.XamlType);
			_xamlNodesWriter.WriteStartMember(property2);
			_xamlNodesWriter.WriteValue(_binaryReader.ReadString());
			_xamlNodesWriter.WriteEndMember();
		}
	}

	private void Common_Process_Property()
	{
		if (!_context.InsideKeyRecord && !_context.InsideStaticResource)
		{
			RemoveImplicitFrame();
			if (_context.CurrentFrame.XamlType == null)
			{
				throw new System.Xaml.XamlParseException(SR.PropertyFoundOutsideStartElement);
			}
			if (_context.CurrentFrame.Member != null)
			{
				throw new System.Xaml.XamlParseException(SR.Format(SR.PropertyOutOfOrder, _context.CurrentFrame.Member));
			}
		}
	}

	private Int32Collection GetInt32Collection()
	{
		BinaryReader binaryReader = new BinaryReader(_binaryReader.BaseStream);
		XamlInt32CollectionSerializer.IntegerCollectionType integerCollectionType = (XamlInt32CollectionSerializer.IntegerCollectionType)binaryReader.ReadByte();
		int num = binaryReader.ReadInt32();
		if (num < 0)
		{
			throw new ArgumentException(SR.Format(SR.IntegerCollectionLengthLessThanZero));
		}
		Int32Collection int32Collection = new Int32Collection(num);
		switch (integerCollectionType)
		{
		case XamlInt32CollectionSerializer.IntegerCollectionType.Byte:
		{
			for (int k = 0; k < num; k++)
			{
				int32Collection.Add(binaryReader.ReadByte());
			}
			return int32Collection;
		}
		case XamlInt32CollectionSerializer.IntegerCollectionType.UShort:
		{
			for (int j = 0; j < num; j++)
			{
				int32Collection.Add(binaryReader.ReadUInt16());
			}
			return int32Collection;
		}
		case XamlInt32CollectionSerializer.IntegerCollectionType.Integer:
		{
			for (int l = 0; l < num; l++)
			{
				int value = binaryReader.ReadInt32();
				int32Collection.Add(value);
			}
			return int32Collection;
		}
		case XamlInt32CollectionSerializer.IntegerCollectionType.Consecutive:
		{
			int num2 = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				int32Collection.Add(num2 + i);
			}
			return int32Collection;
		}
		default:
			throw new InvalidOperationException(SR.UnableToConvertInt32);
		}
	}

	private XamlMember GetProperty(short propertyId, XamlType parentType)
	{
		return BamlSchemaContext.GetProperty(propertyId, parentType);
	}

	private XamlMember GetProperty(short propertyId, bool isAttached)
	{
		return BamlSchemaContext.GetProperty(propertyId, isAttached);
	}

	private void Process_PropertyCustom()
	{
		Common_Process_Property();
		int num = Read_RecordSize();
		XamlMember property = GetProperty(_binaryReader.ReadInt16(), _context.CurrentFrame.XamlType);
		_xamlNodesWriter.WriteStartMember(property);
		short num2 = _binaryReader.ReadInt16();
		if ((num2 & 0x4000) == 16384)
		{
			num2 &= -16385;
		}
		if (_isBinaryProvider)
		{
			WriteTypeConvertedInstance(num2, num - 5);
		}
		else
		{
			_xamlNodesWriter.WriteValue(GetTextFromBinary(_binaryReader.ReadBytes(num - 5), num2, property, _context.CurrentFrame.XamlType));
		}
		_xamlNodesWriter.WriteEndMember();
	}

	private bool WriteTypeConvertedInstance(short converterId, int dataByteSize)
	{
		switch (converterId)
		{
		case 745:
			_xamlNodesWriter.WriteValue(GetInt32Collection());
			break;
		case 195:
		{
			TypeConverter typeConverter = new EnumConverter(_context.CurrentFrame.XamlType.UnderlyingType);
			_xamlNodesWriter.WriteValue(typeConverter.ConvertFrom(_binaryReader.ReadBytes(dataByteSize)));
			break;
		}
		case 46:
			_xamlNodesWriter.WriteValue(_binaryReader.ReadBytes(1)[0] != 0);
			break;
		case 615:
			_xamlNodesWriter.WriteValue(_binaryReader.ReadString());
			break;
		case 137:
		{
			DependencyProperty dependencyProperty = null;
			if (dataByteSize == 2)
			{
				dependencyProperty = BamlSchemaContext.GetDependencyProperty(_binaryReader.ReadInt16());
			}
			else
			{
				Type underlyingType = BamlSchemaContext.GetXamlType(_binaryReader.ReadInt16()).UnderlyingType;
				dependencyProperty = DependencyProperty.FromName(_binaryReader.ReadString(), underlyingType);
			}
			_xamlNodesWriter.WriteValue(dependencyProperty);
			break;
		}
		case 744:
		case 746:
		case 747:
		case 748:
		case 752:
		{
			DeferredBinaryDeserializerExtension value = new DeferredBinaryDeserializerExtension(this, _binaryReader, converterId, dataByteSize);
			_xamlNodesWriter.WriteValue(value);
			break;
		}
		default:
			throw new NotImplementedException();
		}
		return true;
	}

	private void Process_PropertyWithConverter()
	{
		Common_Process_Property();
		Read_RecordSize();
		XamlMember property = GetProperty(_binaryReader.ReadInt16(), _context.CurrentFrame.XamlType);
		_xamlNodesWriter.WriteStartMember(property);
		object obj = _binaryReader.ReadString();
		short num = _binaryReader.ReadInt16();
		if (_isBinaryProvider && num < 0 && -num != 615)
		{
			TypeConverter value = null;
			if (-num == 195)
			{
				Type underlyingType = property.Type.UnderlyingType;
				if (underlyingType.IsEnum && !_enumTypeConverterMap.TryGetValue(underlyingType, out value))
				{
					value = new EnumConverter(underlyingType);
					_enumTypeConverterMap[underlyingType] = value;
				}
			}
			else if (!_typeConverterMap.TryGetValue(num, out value))
			{
				value = Baml2006SchemaContext.KnownTypes.CreateKnownTypeConverter(num);
				_typeConverterMap[num] = value;
			}
			if (value != null)
			{
				obj = CreateTypeConverterMarkupExtension(property, value, obj, _settings);
			}
		}
		_xamlNodesWriter.WriteValue(obj);
		_xamlNodesWriter.WriteEndMember();
	}

	internal virtual object CreateTypeConverterMarkupExtension(XamlMember property, TypeConverter converter, object propertyValue, Baml2006ReaderSettings settings)
	{
		return new TypeConverterMarkupExtension(converter, propertyValue);
	}

	private void Process_PropertyWithExtension()
	{
		Common_Process_Property();
		short propertyId = _binaryReader.ReadInt16();
		short num = _binaryReader.ReadInt16();
		short num2 = _binaryReader.ReadInt16();
		XamlMember property = GetProperty(propertyId, _context.CurrentFrame.XamlType);
		short num3 = (short)(num & 0xFFF);
		XamlType xamlType = BamlSchemaContext.GetXamlType((short)(-num3));
		bool flag = (num & 0x4000) == 16384;
		bool flag2 = (num & 0x2000) == 8192;
		Type type = null;
		Type memberType = null;
		object value = null;
		_xamlNodesWriter.WriteStartMember(property);
		bool flag3 = false;
		if (_isBinaryProvider)
		{
			object obj = null;
			object providedValue = null;
			if (!flag2)
			{
				obj = (flag ? BamlSchemaContext.GetXamlType(num2).UnderlyingType : (num3 switch
				{
					634 => _context.SchemaContext.GetDependencyProperty(num2), 
					602 => GetStaticExtensionValue(num2, out memberType, out providedValue), 
					691 => BamlSchemaContext.GetXamlType(num2).UnderlyingType, 
					_ => BamlSchemaContext.GetString(num2), 
				}));
			}
			else
			{
				Type memberType2 = null;
				string staticExtensionValue = GetStaticExtensionValue(num2, out memberType2, out providedValue);
				obj = ((providedValue == null) ? new StaticExtension(staticExtensionValue)
				{
					MemberType = memberType2
				}.ProvideValue(null) : providedValue);
			}
			switch (num3)
			{
			case 189:
				value = new DynamicResourceExtension(obj);
				flag3 = true;
				break;
			case 603:
				value = new StaticResourceExtension(obj);
				flag3 = true;
				break;
			case 634:
				value = new TemplateBindingExtension((DependencyProperty)obj);
				flag3 = true;
				break;
			case 691:
				value = obj;
				flag3 = true;
				break;
			case 602:
				value = ((providedValue == null) ? new StaticExtension((string)obj)
				{
					MemberType = memberType
				} : providedValue);
				flag3 = true;
				break;
			}
			if (flag3)
			{
				_xamlNodesWriter.WriteValue(value);
				_xamlNodesWriter.WriteEndMember();
				return;
			}
		}
		if (!flag3)
		{
			_xamlNodesWriter.WriteStartObject(xamlType);
			_xamlNodesWriter.WriteStartMember(XamlLanguage.PositionalParameters);
			if (flag2)
			{
				Type memberType3 = null;
				value = GetStaticExtensionValue(num2, out memberType3, out var providedValue2);
				if (providedValue2 != null)
				{
					_xamlNodesWriter.WriteValue(providedValue2);
				}
				else
				{
					_xamlNodesWriter.WriteStartObject(XamlLanguage.Static);
					_xamlNodesWriter.WriteStartMember(XamlLanguage.PositionalParameters);
					_xamlNodesWriter.WriteValue(value);
					_xamlNodesWriter.WriteEndMember();
					if (memberType3 != null)
					{
						_xamlNodesWriter.WriteStartMember(BamlSchemaContext.StaticExtensionMemberTypeProperty);
						_xamlNodesWriter.WriteValue(memberType3);
						_xamlNodesWriter.WriteEndMember();
					}
					_xamlNodesWriter.WriteEndObject();
				}
			}
			else if (flag)
			{
				_xamlNodesWriter.WriteStartObject(XamlLanguage.Type);
				_xamlNodesWriter.WriteStartMember(BamlSchemaContext.TypeExtensionTypeProperty);
				type = BamlSchemaContext.GetXamlType(num2).UnderlyingType;
				if (_isBinaryProvider)
				{
					_xamlNodesWriter.WriteValue(type);
				}
				else
				{
					_xamlNodesWriter.WriteValue(Logic_GetFullyQualifiedNameForType(BamlSchemaContext.GetXamlType(num2)));
				}
				_xamlNodesWriter.WriteEndMember();
				_xamlNodesWriter.WriteEndObject();
			}
			else
			{
				value = num3 switch
				{
					634 => (!_isBinaryProvider) ? ((object)Logic_GetFullyQualifiedNameForMember(num2)) : ((object)BitConverter.GetBytes(num2)), 
					602 => GetStaticExtensionValue(num2, out memberType, out var _), 
					691 => BamlSchemaContext.GetXamlType(num2).UnderlyingType, 
					_ => BamlSchemaContext.GetString(num2), 
				};
				_xamlNodesWriter.WriteValue(value);
			}
			_xamlNodesWriter.WriteEndMember();
			if (memberType != null)
			{
				_xamlNodesWriter.WriteStartMember(BamlSchemaContext.StaticExtensionMemberTypeProperty);
				_xamlNodesWriter.WriteValue(memberType);
				_xamlNodesWriter.WriteEndMember();
			}
		}
		_xamlNodesWriter.WriteEndObject();
		_xamlNodesWriter.WriteEndMember();
	}

	private void Process_PropertyTypeReference()
	{
		Common_Process_Property();
		XamlMember property = GetProperty(_binaryReader.ReadInt16(), _context.CurrentFrame.XamlType);
		XamlType xamlType = BamlSchemaContext.GetXamlType(_binaryReader.ReadInt16());
		_xamlNodesWriter.WriteStartMember(property);
		if (_isBinaryProvider)
		{
			_xamlNodesWriter.WriteValue(xamlType.UnderlyingType);
		}
		else
		{
			_xamlNodesWriter.WriteStartObject(XamlLanguage.Type);
			_xamlNodesWriter.WriteStartMember(XamlLanguage.PositionalParameters);
			_xamlNodesWriter.WriteValue(Logic_GetFullyQualifiedNameForType(xamlType));
			_xamlNodesWriter.WriteEndMember();
			_xamlNodesWriter.WriteEndObject();
		}
		_xamlNodesWriter.WriteEndMember();
	}

	private void Process_PropertyWithStaticResourceId()
	{
		Common_Process_Property();
		short propertyId = _binaryReader.ReadInt16();
		short index = _binaryReader.ReadInt16();
		XamlMember property = _context.SchemaContext.GetProperty(propertyId, _context.CurrentFrame.XamlType);
		object obj = _context.KeyList[_context.CurrentKey - 1].StaticResources[index];
		if (obj is StaticResourceHolder)
		{
			_xamlNodesWriter.WriteStartMember(property);
			_xamlNodesWriter.WriteValue(obj);
			_xamlNodesWriter.WriteEndMember();
			return;
		}
		_xamlNodesWriter.WriteStartMember(property);
		_xamlNodesWriter.WriteStartObject(BamlSchemaContext.StaticResourceExtensionType);
		_xamlNodesWriter.WriteStartMember(XamlLanguage.PositionalParameters);
		if (obj is OptimizedStaticResource optimizedStaticResource)
		{
			if (optimizedStaticResource.IsKeyStaticExtension)
			{
				Type memberType = null;
				object providedValue;
				string staticExtensionValue = GetStaticExtensionValue(optimizedStaticResource.KeyId, out memberType, out providedValue);
				if (providedValue != null)
				{
					_xamlNodesWriter.WriteValue(providedValue);
				}
				else
				{
					_xamlNodesWriter.WriteStartObject(XamlLanguage.Static);
					_xamlNodesWriter.WriteStartMember(XamlLanguage.PositionalParameters);
					_xamlNodesWriter.WriteValue(staticExtensionValue);
					_xamlNodesWriter.WriteEndMember();
					if (memberType != null)
					{
						_xamlNodesWriter.WriteStartMember(BamlSchemaContext.StaticExtensionMemberTypeProperty);
						_xamlNodesWriter.WriteValue(memberType);
						_xamlNodesWriter.WriteEndMember();
					}
					_xamlNodesWriter.WriteEndObject();
				}
			}
			else if (optimizedStaticResource.IsKeyTypeExtension)
			{
				if (_isBinaryProvider)
				{
					XamlType xamlType = BamlSchemaContext.GetXamlType(optimizedStaticResource.KeyId);
					_xamlNodesWriter.WriteValue(xamlType.UnderlyingType);
				}
				else
				{
					_xamlNodesWriter.WriteStartObject(XamlLanguage.Type);
					_xamlNodesWriter.WriteStartMember(XamlLanguage.PositionalParameters);
					_xamlNodesWriter.WriteValue(Logic_GetFullyQualifiedNameForType(BamlSchemaContext.GetXamlType(optimizedStaticResource.KeyId)));
					_xamlNodesWriter.WriteEndMember();
					_xamlNodesWriter.WriteEndObject();
				}
			}
			else
			{
				string @string = _context.SchemaContext.GetString(optimizedStaticResource.KeyId);
				_xamlNodesWriter.WriteValue(@string);
			}
		}
		else
		{
			XamlServices.Transform((obj as StaticResource).ResourceNodeList.GetReader(), _xamlNodesWriter, closeWriter: false);
		}
		_xamlNodesWriter.WriteEndMember();
		_xamlNodesWriter.WriteEndObject();
		_xamlNodesWriter.WriteEndMember();
	}

	private void Process_PropertyComplexStart()
	{
		Common_Process_Property();
		XamlMember property = GetProperty(_binaryReader.ReadInt16(), _context.CurrentFrame.XamlType);
		_context.CurrentFrame.Member = property;
		_xamlNodesWriter.WriteStartMember(property);
	}

	private void Process_PropertyArrayStart()
	{
		Common_Process_Property();
		XamlMember property = GetProperty(_binaryReader.ReadInt16(), _context.CurrentFrame.XamlType);
		_context.CurrentFrame.Member = property;
		_xamlNodesWriter.WriteStartMember(property);
	}

	private void Process_PropertyIDictionaryStart()
	{
		Common_Process_Property();
		XamlMember property = GetProperty(_binaryReader.ReadInt16(), _context.CurrentFrame.XamlType);
		_context.CurrentFrame.Member = property;
		_xamlNodesWriter.WriteStartMember(property);
	}

	private void Process_PropertyEnd()
	{
		RemoveImplicitFrame();
		_context.CurrentFrame.Member = null;
		_xamlNodesWriter.WriteEndMember();
	}

	private void Process_PropertyIDictionaryEnd()
	{
		if (_lookingForAKeyOnAMarkupExtensionInADictionaryDepth == _context.CurrentFrame.Depth)
		{
			RestoreSavedFirstItemInDictionary();
		}
		RemoveImplicitFrame();
		_context.CurrentFrame.Member = null;
		_xamlNodesWriter.WriteEndMember();
	}

	private string Logic_GetFullyQualifiedNameForMember(short propertyId)
	{
		return Logic_GetFullyQualifiedNameForType(BamlSchemaContext.GetPropertyDeclaringType(propertyId)) + "." + BamlSchemaContext.GetPropertyName(propertyId, fullName: false);
	}

	private string Logic_GetFullyQualifiedNameForType(XamlType type)
	{
		Baml2006ReaderFrame baml2006ReaderFrame = _context.CurrentFrame;
		IList<string> xamlNamespaces = type.GetXamlNamespaces();
		while (baml2006ReaderFrame != null)
		{
			foreach (string item in xamlNamespaces)
			{
				string prefix = null;
				if (baml2006ReaderFrame.TryGetPrefixByNamespace(item, out prefix))
				{
					if (string.IsNullOrEmpty(prefix))
					{
						return type.Name;
					}
					return prefix + ":" + type.Name;
				}
			}
			baml2006ReaderFrame = (Baml2006ReaderFrame)baml2006ReaderFrame.Previous;
		}
		throw new InvalidOperationException("Could not find prefix for type: " + type.Name);
	}

	private string Logic_GetFullXmlns(string uriInput)
	{
		int num = uriInput.IndexOf(':');
		if (num != -1 && MemoryExtensions.Equals(uriInput.AsSpan(0, num), "clr-namespace", StringComparison.Ordinal))
		{
			int num2 = uriInput.IndexOf(';');
			if (-1 == num2)
			{
				return uriInput + ((_settings.LocalAssembly != null) ? string.Concat(";assembly=", GetAssemblyNameForNamespace(_settings.LocalAssembly)) : string.Empty);
			}
			int num3 = num2 + 1;
			int num4 = uriInput.IndexOf('=');
			if (-1 == num4)
			{
				throw new ArgumentException(SR.Format(SR.MissingTagInNamespace, "=", uriInput));
			}
			if (!MemoryExtensions.Equals(uriInput.AsSpan(num3, num4 - num3), "assembly", StringComparison.Ordinal))
			{
				throw new ArgumentException(SR.Format(SR.AssemblyTagMissing, "assembly", uriInput));
			}
			if (uriInput.AsSpan(num4 + 1).TrimStart().IsEmpty)
			{
				return string.Concat(uriInput, GetAssemblyNameForNamespace(_settings.LocalAssembly));
			}
		}
		return uriInput;
	}

	internal virtual ReadOnlySpan<char> GetAssemblyNameForNamespace(Assembly assembly)
	{
		string fullName = assembly.FullName;
		return fullName.AsSpan(0, fullName.IndexOf(','));
	}

	private void Process_XmlnsProperty()
	{
		Read_RecordSize();
		string prefix = _binaryReader.ReadString();
		string uriInput = _binaryReader.ReadString();
		uriInput = Logic_GetFullXmlns(uriInput);
		_context.CurrentFrame.AddNamespace(prefix, uriInput);
		NamespaceDeclaration namespaceDeclaration = new NamespaceDeclaration(uriInput, prefix);
		_xamlNodesWriter.WriteNamespace(namespaceDeclaration);
		short num = _binaryReader.ReadInt16();
		if (uriInput.StartsWith("clr-namespace:", StringComparison.Ordinal))
		{
			SkipBytes(num * 2);
		}
		else if (num > 0)
		{
			short[] array = new short[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = _binaryReader.ReadInt16();
			}
			BamlSchemaContext.AddXmlnsMapping(uriInput, array);
		}
	}

	private void Process_LinePosition()
	{
		_context.LineOffset = _binaryReader.ReadInt32();
		if (_xamlNodesWriter is IXamlLineInfoConsumer xamlLineInfoConsumer)
		{
			xamlLineInfoConsumer.SetLineInfo(_context.LineNumber, _context.LineOffset);
		}
	}

	private void Process_LineNumberAndPosition()
	{
		_context.LineNumber = _binaryReader.ReadInt32();
		_context.LineOffset = _binaryReader.ReadInt32();
		if (_xamlNodesWriter is IXamlLineInfoConsumer xamlLineInfoConsumer)
		{
			xamlLineInfoConsumer.SetLineInfo(_context.LineNumber, _context.LineOffset);
		}
	}

	private void Process_PIMapping()
	{
		Read_RecordSize();
		_binaryReader.ReadString();
		_binaryReader.ReadString();
		_binaryReader.ReadInt16();
	}

	private void Process_AssemblyInfo()
	{
		Read_RecordSize();
		short assemblyId = _binaryReader.ReadInt16();
		string assemblyName = _binaryReader.ReadString();
		BamlSchemaContext.AddAssembly(assemblyId, assemblyName);
	}

	private void Process_TypeInfo()
	{
		Read_RecordSize();
		short typeId = _binaryReader.ReadInt16();
		short num = _binaryReader.ReadInt16();
		string typeName = _binaryReader.ReadString();
		Baml2006SchemaContext.TypeInfoFlags flags = (Baml2006SchemaContext.TypeInfoFlags)(num >> 12);
		num &= 0xFFF;
		BamlSchemaContext.AddXamlType(typeId, num, typeName, flags);
	}

	private void Process_TypeSerializerInfo()
	{
		Read_RecordSize();
		short typeId = _binaryReader.ReadInt16();
		short num = _binaryReader.ReadInt16();
		string typeName = _binaryReader.ReadString();
		_binaryReader.ReadInt16();
		Baml2006SchemaContext.TypeInfoFlags flags = (Baml2006SchemaContext.TypeInfoFlags)(num >> 12);
		num &= 0xFFF;
		BamlSchemaContext.AddXamlType(typeId, num, typeName, flags);
	}

	private void Process_AttributeInfo()
	{
		Read_RecordSize();
		short propertyId = _binaryReader.ReadInt16();
		short declaringTypeId = _binaryReader.ReadInt16();
		_binaryReader.ReadByte();
		string propertyName = _binaryReader.ReadString();
		BamlSchemaContext.AddProperty(propertyId, declaringTypeId, propertyName);
	}

	private void Process_StringInfo()
	{
		Read_RecordSize();
		short stringId = _binaryReader.ReadInt16();
		string value = _binaryReader.ReadString();
		BamlSchemaContext.AddString(stringId, value);
	}

	private void Process_ContentProperty()
	{
		short num = _binaryReader.ReadInt16();
		if (num != -174)
		{
			XamlMember xamlMember = GetProperty(num, isAttached: false);
			WpfXamlMember wpfXamlMember = xamlMember as WpfXamlMember;
			if (wpfXamlMember != null)
			{
				xamlMember = wpfXamlMember.AsContentProperty;
			}
			_context.CurrentFrame.ContentProperty = xamlMember;
		}
	}

	private void Process_ConnectionId()
	{
		int num = _binaryReader.ReadInt32();
		if (_context.CurrentFrame.Member != null)
		{
			Baml2006ReaderFrame baml2006ReaderFrame = _context.CurrentFrame;
			if (baml2006ReaderFrame.Flags == Baml2006ReaderFrameFlags.IsImplict)
			{
				baml2006ReaderFrame = _context.PreviousFrame;
			}
			baml2006ReaderFrame.DelayedConnectionId = num;
			return;
		}
		Common_Process_Property();
		_xamlNodesWriter.WriteStartMember(XamlLanguage.ConnectionId);
		if (_isBinaryProvider)
		{
			_xamlNodesWriter.WriteValue(num);
		}
		else
		{
			_xamlNodesWriter.WriteValue(num.ToString(System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS));
		}
		_xamlNodesWriter.WriteEndMember();
	}

	private Baml2006RecordType Read_RecordType()
	{
		byte b = _binaryReader.ReadByte();
		if (b < 0)
		{
			return Baml2006RecordType.DocumentEnd;
		}
		return (Baml2006RecordType)b;
	}

	private int Read_RecordSize()
	{
		long position = _binaryReader.BaseStream.Position;
		int num = _binaryReader.Read7BitEncodedInt();
		int num2 = (int)(_binaryReader.BaseStream.Position - position);
		if (num2 == 1)
		{
			return num;
		}
		return num - num2 + 1;
	}

	private void SkipBytes(long offset)
	{
		_binaryReader.BaseStream.Seek(offset, SeekOrigin.Current);
	}

	private void RemoveImplicitFrame()
	{
		if (_context.CurrentFrame.Flags == Baml2006ReaderFrameFlags.IsImplict)
		{
			_xamlNodesWriter.WriteEndMember();
			_xamlNodesWriter.WriteEndObject();
			_context.PopScope();
		}
		if (_context.CurrentFrame.Flags == Baml2006ReaderFrameFlags.HasImplicitProperty)
		{
			if (_context.CurrentFrame.Depth == _context.TemplateStartDepth)
			{
				_xamlNodesWriter.Close();
				_xamlNodesWriter = _xamlWriterStack.Pop();
				_xamlNodesWriter.WriteValue(_xamlTemplateNodeList);
				_xamlTemplateNodeList = null;
				_context.TemplateStartDepth = -1;
			}
			_xamlNodesWriter.WriteEndMember();
			_context.CurrentFrame.Member = null;
			_context.CurrentFrame.Flags = Baml2006ReaderFrameFlags.None;
		}
	}

	private void InjectPropertyAndFrameIfNeeded(XamlType elementType, sbyte flags)
	{
		if (_lookingForAKeyOnAMarkupExtensionInADictionaryDepth == _context.CurrentFrame.Depth)
		{
			RestoreSavedFirstItemInDictionary();
		}
		XamlType xamlType = _context.CurrentFrame.XamlType;
		XamlMember xamlMember = _context.CurrentFrame.Member;
		if (!(xamlType != null))
		{
			return;
		}
		if (xamlMember == null)
		{
			if (_context.CurrentFrame.ContentProperty != null)
			{
				xamlMember = (_context.CurrentFrame.Member = _context.CurrentFrame.ContentProperty);
			}
			else if (xamlType.ContentProperty != null)
			{
				xamlMember = (_context.CurrentFrame.Member = xamlType.ContentProperty);
			}
			else if (xamlType.IsCollection || xamlType.IsDictionary)
			{
				xamlMember = (_context.CurrentFrame.Member = XamlLanguage.Items);
			}
			else
			{
				if (!(xamlType.TypeConverter != null))
				{
					throw new System.Xaml.XamlParseException(SR.Format(SR.RecordOutOfOrder, xamlType.Name));
				}
				xamlMember = (_context.CurrentFrame.Member = XamlLanguage.Initialization);
			}
			_context.CurrentFrame.Flags = Baml2006ReaderFrameFlags.HasImplicitProperty;
			_xamlNodesWriter.WriteStartMember(xamlMember);
			if (_context.TemplateStartDepth < 0 && _isBinaryProvider && xamlMember == BamlSchemaContext.FrameworkTemplateTemplateProperty)
			{
				_context.TemplateStartDepth = _context.CurrentFrame.Depth;
				_xamlTemplateNodeList = new XamlNodeList(_xamlNodesWriter.SchemaContext);
				_xamlWriterStack.Push(_xamlNodesWriter);
				_xamlNodesWriter = _xamlTemplateNodeList.Writer;
				if (XamlSourceInfoHelper.IsXamlSourceInfoEnabled && _xamlNodesWriter is IXamlLineInfoConsumer xamlLineInfoConsumer)
				{
					xamlLineInfoConsumer.SetLineInfo(_context.LineNumber, _context.LineOffset);
				}
			}
		}
		XamlType type = xamlMember.Type;
		if (!(type != null) || (!type.IsCollection && !type.IsDictionary) || xamlMember.IsDirective || (flags & 2) != 0)
		{
			return;
		}
		bool flag = false;
		if (xamlMember.IsReadOnly)
		{
			flag = true;
		}
		else if (!elementType.CanAssignTo(type))
		{
			if (!elementType.IsMarkupExtension)
			{
				flag = true;
			}
			else if (_context.CurrentFrame.Flags == Baml2006ReaderFrameFlags.HasImplicitProperty)
			{
				flag = true;
			}
			else if (elementType == XamlLanguage.Array)
			{
				flag = true;
			}
		}
		if (flag)
		{
			EmitGoItemsPreamble(type);
		}
		if (!flag && type.IsDictionary && elementType.IsMarkupExtension)
		{
			StartSavingFirstItemInDictionary();
		}
	}

	private void StartSavingFirstItemInDictionary()
	{
		_lookingForAKeyOnAMarkupExtensionInADictionaryDepth = _context.CurrentFrame.Depth;
		_lookingForAKeyOnAMarkupExtensionInADictionaryNodeList = new XamlNodeList(_xamlNodesWriter.SchemaContext);
		_xamlWriterStack.Push(_xamlNodesWriter);
		_xamlNodesWriter = _lookingForAKeyOnAMarkupExtensionInADictionaryNodeList.Writer;
	}

	private void RestoreSavedFirstItemInDictionary()
	{
		_xamlNodesWriter.Close();
		_xamlNodesWriter = _xamlWriterStack.Pop();
		if (NodeListHasAKeySetOnTheRoot(_lookingForAKeyOnAMarkupExtensionInADictionaryNodeList.GetReader()))
		{
			EmitGoItemsPreamble(_context.CurrentFrame.Member.Type);
		}
		XamlServices.Transform(_lookingForAKeyOnAMarkupExtensionInADictionaryNodeList.GetReader(), _xamlNodesWriter, closeWriter: false);
		_lookingForAKeyOnAMarkupExtensionInADictionaryDepth = -1;
	}

	private void EmitGoItemsPreamble(XamlType parentPropertyType)
	{
		_context.PushScope();
		_context.CurrentFrame.XamlType = parentPropertyType;
		_xamlNodesWriter.WriteGetObject();
		_context.CurrentFrame.Flags = Baml2006ReaderFrameFlags.IsImplict;
		_context.CurrentFrame.Member = XamlLanguage.Items;
		_xamlNodesWriter.WriteStartMember(_context.CurrentFrame.Member);
	}

	private StaticResource GetLastStaticResource()
	{
		return _context.LastKey.LastStaticResource;
	}

	private string GetTextFromBinary(byte[] bytes, short serializerId, XamlMember property, XamlType type)
	{
		switch (serializerId)
		{
		case 46:
			if (bytes[0] != 0)
			{
				return true.ToString();
			}
			return false.ToString();
		case 195:
			return Enum.ToObject(type.UnderlyingType, bytes).ToString();
		case 744:
		{
			using MemoryStream input6 = new MemoryStream(bytes);
			using BinaryReader reader6 = new BinaryReader(input6);
			return (SolidColorBrush.DeserializeFrom(reader6) as SolidColorBrush).ToString();
		}
		case 746:
		{
			using MemoryStream input5 = new MemoryStream(bytes);
			using BinaryReader reader5 = new BinaryReader(input5);
			return new XamlPathDataSerializer().ConvertCustomBinaryToObject(reader5).ToString();
		}
		case 747:
		{
			using MemoryStream input4 = new MemoryStream(bytes);
			using BinaryReader reader4 = new BinaryReader(input4);
			return new XamlPoint3DCollectionSerializer().ConvertCustomBinaryToObject(reader4).ToString();
		}
		case 752:
		{
			using MemoryStream input3 = new MemoryStream(bytes);
			using BinaryReader reader3 = new BinaryReader(input3);
			return new XamlVector3DCollectionSerializer().ConvertCustomBinaryToObject(reader3).ToString();
		}
		case 748:
		{
			using MemoryStream input2 = new MemoryStream(bytes);
			using BinaryReader reader2 = new BinaryReader(input2);
			return new XamlPointCollectionSerializer().ConvertCustomBinaryToObject(reader2).ToString();
		}
		case 745:
		{
			using MemoryStream input = new MemoryStream(bytes);
			using BinaryReader reader = new BinaryReader(input);
			return new XamlInt32CollectionSerializer().ConvertCustomBinaryToObject(reader).ToString();
		}
		case 0:
		case 137:
		{
			if (bytes.Length == 2)
			{
				short propertyId = (short)(bytes[0] | (bytes[1] << 8));
				return Logic_GetFullyQualifiedNameForMember(propertyId);
			}
			using BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes));
			XamlType xamlType = BamlSchemaContext.GetXamlType(binaryReader.ReadInt16());
			string text = binaryReader.ReadString();
			return Logic_GetFullyQualifiedNameForType(xamlType) + "." + text;
		}
		default:
			throw new NotImplementedException();
		}
	}

	private string GetStaticExtensionValue(short valueId, out Type memberType, out object providedValue)
	{
		string text = "";
		memberType = null;
		providedValue = null;
		if (valueId < 0)
		{
			valueId = (short)(-valueId);
			bool isKey = true;
			valueId = SystemResourceKey.GetSystemResourceKeyIdFromBamlId(valueId, out isKey);
			if (valueId <= 0 || valueId >= 236)
			{
				throw new InvalidOperationException(SR.BamlBadExtensionValue);
			}
			if (_isBinaryProvider)
			{
				if (isKey)
				{
					providedValue = SystemResourceKey.GetResourceKey(valueId);
				}
				else
				{
					providedValue = SystemResourceKey.GetResource(valueId);
				}
			}
			else
			{
				SystemResourceKeyID id = (SystemResourceKeyID)valueId;
				XamlType xamlType = _context.SchemaContext.GetXamlType(SystemKeyConverter.GetSystemClassType(id));
				text = Logic_GetFullyQualifiedNameForType(xamlType) + ".";
				text = ((!isKey) ? (text + SystemKeyConverter.GetSystemPropertyName(id)) : (text + SystemKeyConverter.GetSystemKeyName(id)));
			}
		}
		else if (_isBinaryProvider)
		{
			memberType = BamlSchemaContext.GetPropertyDeclaringType(valueId).UnderlyingType;
			text = BamlSchemaContext.GetPropertyName(valueId, fullName: false);
			providedValue = CommandConverter.GetKnownControlCommand(memberType, text);
		}
		else
		{
			text = Logic_GetFullyQualifiedNameForMember(valueId);
		}
		return text;
	}

	private bool NodeListHasAKeySetOnTheRoot(System.Xaml.XamlReader reader)
	{
		int num = 0;
		while (reader.Read())
		{
			switch (reader.NodeType)
			{
			case System.Xaml.XamlNodeType.StartObject:
				num++;
				break;
			case System.Xaml.XamlNodeType.EndObject:
				num--;
				break;
			case System.Xaml.XamlNodeType.StartMember:
				if (reader.Member == XamlLanguage.Key && num == 1)
				{
					return true;
				}
				break;
			}
		}
		return false;
	}

	bool IFreezeFreezables.TryFreeze(string value, Freezable freezable)
	{
		if (freezable.CanFreeze)
		{
			if (!freezable.IsFrozen)
			{
				freezable.Freeze();
			}
			if (_freezeCache == null)
			{
				_freezeCache = new Dictionary<string, Freezable>();
			}
			_freezeCache.Add(value, freezable);
			return true;
		}
		return false;
	}

	Freezable IFreezeFreezables.TryGetFreezable(string value)
	{
		Freezable value2 = null;
		if (_freezeCache != null)
		{
			_freezeCache.TryGetValue(value, out value2);
		}
		return value2;
	}
}
