using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

namespace System.Windows.Markup;

internal class BamlReader
{
	internal class BamlNodeInfo
	{
		private BamlRecordType _recordType;

		private string _assemblyName;

		private string _prefix;

		private string _xmlNamespace;

		private string _clrNamespace;

		private string _name;

		private string _localName;

		private BamlAttributeUsage _attributeUsage;

		internal BamlRecordType RecordType
		{
			get
			{
				return _recordType;
			}
			set
			{
				_recordType = value;
			}
		}

		internal string AssemblyName
		{
			get
			{
				return _assemblyName;
			}
			set
			{
				_assemblyName = value;
			}
		}

		internal string Prefix
		{
			get
			{
				return _prefix;
			}
			set
			{
				_prefix = value;
			}
		}

		internal string XmlNamespace
		{
			get
			{
				return _xmlNamespace;
			}
			set
			{
				_xmlNamespace = value;
			}
		}

		internal string ClrNamespace
		{
			get
			{
				return _clrNamespace;
			}
			set
			{
				_clrNamespace = value;
			}
		}

		internal string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}

		internal string LocalName
		{
			get
			{
				return _localName;
			}
			set
			{
				_localName = value;
			}
		}

		internal BamlAttributeUsage AttributeUsage
		{
			get
			{
				return _attributeUsage;
			}
			set
			{
				_attributeUsage = value;
			}
		}

		internal BamlNodeInfo()
		{
		}
	}

	internal class BamlPropertyInfo : BamlNodeInfo
	{
		private string _value;

		internal string Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		internal BamlPropertyInfo()
		{
		}
	}

	internal class BamlContentPropertyInfo : BamlNodeInfo
	{
	}

	[DebuggerDisplay("{_offset}")]
	internal class BamlKeyInfo : BamlPropertyInfo
	{
		private int _offset;

		private List<List<BamlRecord>> _staticResources;

		internal int Offset
		{
			get
			{
				return _offset;
			}
			set
			{
				_offset = value;
			}
		}

		internal List<List<BamlRecord>> StaticResources
		{
			get
			{
				if (_staticResources == null)
				{
					_staticResources = new List<List<BamlRecord>>();
				}
				return _staticResources;
			}
		}

		internal BamlKeyInfo()
		{
		}
	}

	private BamlRecordReader _bamlRecordReader;

	private XmlnsDictionary _prefixDictionary;

	private BamlRecord _currentBamlRecord;

	private bool _haveUnprocessedRecord;

	private int _deferableContentBlockDepth;

	private long _deferableContentPosition;

	private List<BamlKeyInfo> _deferKeys;

	private BamlKeyInfo _currentKeyInfo;

	private List<BamlRecord> _currentStaticResourceRecords;

	private int _currentStaticResourceRecordIndex;

	private BamlNodeType _bamlNodeType;

	private ReadState _readState;

	private string _assemblyName;

	private string _prefix;

	private string _xmlNamespace;

	private string _clrNamespace;

	private string _value;

	private string _name;

	private string _localName;

	private string _ownerTypeName;

	private ArrayList _properties;

	private DependencyProperty _propertyDP;

	private int _propertiesIndex;

	private int _connectionId;

	private string _contentPropertyName;

	private BamlAttributeUsage _attributeUsage;

	private Stack _nodeStack;

	private ParserContext _parserContext;

	private bool _isInjected;

	private bool _useTypeConverter;

	private string _typeConverterAssemblyName;

	private string _typeConverterName;

	private Dictionary<string, List<string>> _reverseXmlnsTable;

	public int PropertyCount => _properties.Count;

	public bool HasProperties => PropertyCount > 0;

	public int ConnectionId => _connectionId;

	public BamlAttributeUsage AttributeUsage => _attributeUsage;

	public BamlNodeType NodeType => _bamlNodeType;

	public string Name => _name;

	public string LocalName => _localName;

	public string Prefix => _prefix;

	public string AssemblyName => _assemblyName;

	public string XmlNamespace => _xmlNamespace;

	public string ClrNamespace => _clrNamespace;

	public string Value => _value;

	public bool IsInjected => _isInjected;

	public bool CreateUsingTypeConverter => _useTypeConverter;

	public string TypeConverterName => _typeConverterName;

	public string TypeConverterAssemblyName => _typeConverterAssemblyName;

	private BamlNodeType NodeTypeInternal
	{
		set
		{
			_bamlNodeType = value;
		}
	}

	private BamlMapTable MapTable => _parserContext.MapTable;

	public BamlReader(Stream bamlStream)
	{
		_parserContext = new ParserContext();
		_parserContext.XamlTypeMapper = XmlParserDefaults.DefaultMapper;
		_bamlRecordReader = new BamlRecordReader(bamlStream, _parserContext, loadMapper: false);
		_readState = ReadState.Initial;
		_bamlNodeType = BamlNodeType.None;
		_prefixDictionary = new XmlnsDictionary();
		_value = string.Empty;
		_assemblyName = string.Empty;
		_prefix = string.Empty;
		_xmlNamespace = string.Empty;
		_clrNamespace = string.Empty;
		_name = string.Empty;
		_localName = string.Empty;
		_ownerTypeName = string.Empty;
		_properties = new ArrayList();
		_haveUnprocessedRecord = false;
		_deferableContentBlockDepth = -1;
		_nodeStack = new Stack();
		_reverseXmlnsTable = new Dictionary<string, List<string>>();
	}

	public bool Read()
	{
		if (_readState == ReadState.EndOfFile || _readState == ReadState.Closed)
		{
			throw new InvalidOperationException(SR.BamlReaderClosed);
		}
		ReadNextRecord();
		return _readState != ReadState.EndOfFile;
	}

	private void AddToPropertyInfoCollection(object info)
	{
		_properties.Add(info);
	}

	public void Close()
	{
		if (_readState != ReadState.Closed)
		{
			_bamlRecordReader.Close();
			_currentBamlRecord = null;
			_bamlRecordReader = null;
			_readState = ReadState.Closed;
		}
	}

	public bool MoveToFirstProperty()
	{
		if (HasProperties)
		{
			_propertiesIndex = -1;
			return MoveToNextProperty();
		}
		return false;
	}

	public bool MoveToNextProperty()
	{
		if (_propertiesIndex < _properties.Count - 1)
		{
			_propertiesIndex++;
			object obj = _properties[_propertiesIndex];
			if (obj is BamlPropertyInfo bamlPropertyInfo)
			{
				_name = bamlPropertyInfo.Name;
				_localName = bamlPropertyInfo.LocalName;
				int num = bamlPropertyInfo.Name.LastIndexOf('.');
				if (num > 0)
				{
					_ownerTypeName = bamlPropertyInfo.Name.Substring(0, num);
				}
				else
				{
					_ownerTypeName = string.Empty;
				}
				_value = bamlPropertyInfo.Value;
				_assemblyName = bamlPropertyInfo.AssemblyName;
				_prefix = bamlPropertyInfo.Prefix;
				_xmlNamespace = bamlPropertyInfo.XmlNamespace;
				_clrNamespace = bamlPropertyInfo.ClrNamespace;
				_connectionId = 0;
				_contentPropertyName = string.Empty;
				_attributeUsage = bamlPropertyInfo.AttributeUsage;
				if (bamlPropertyInfo.RecordType == BamlRecordType.XmlnsProperty)
				{
					NodeTypeInternal = BamlNodeType.XmlnsProperty;
				}
				else if (bamlPropertyInfo.RecordType == BamlRecordType.DefAttribute)
				{
					NodeTypeInternal = BamlNodeType.DefAttribute;
				}
				else if (bamlPropertyInfo.RecordType == BamlRecordType.PresentationOptionsAttribute)
				{
					NodeTypeInternal = BamlNodeType.PresentationOptionsAttribute;
				}
				else
				{
					NodeTypeInternal = BamlNodeType.Property;
				}
				return true;
			}
			if (obj is BamlContentPropertyInfo bamlContentPropertyInfo)
			{
				_contentPropertyName = bamlContentPropertyInfo.LocalName;
				_connectionId = 0;
				_prefix = string.Empty;
				_name = bamlContentPropertyInfo.Name;
				int num2 = bamlContentPropertyInfo.Name.LastIndexOf('.');
				if (num2 > 0)
				{
					_ownerTypeName = bamlContentPropertyInfo.Name.Substring(0, num2);
				}
				_localName = bamlContentPropertyInfo.LocalName;
				_ownerTypeName = string.Empty;
				_assemblyName = bamlContentPropertyInfo.AssemblyName;
				_xmlNamespace = string.Empty;
				_clrNamespace = string.Empty;
				_attributeUsage = BamlAttributeUsage.Default;
				_value = bamlContentPropertyInfo.LocalName;
				NodeTypeInternal = BamlNodeType.ContentProperty;
				return true;
			}
			_connectionId = (int)obj;
			_contentPropertyName = string.Empty;
			_prefix = string.Empty;
			_name = string.Empty;
			_localName = string.Empty;
			_ownerTypeName = string.Empty;
			_assemblyName = string.Empty;
			_xmlNamespace = string.Empty;
			_clrNamespace = string.Empty;
			_attributeUsage = BamlAttributeUsage.Default;
			_value = _connectionId.ToString(CultureInfo.CurrentCulture);
			NodeTypeInternal = BamlNodeType.ConnectionId;
			return true;
		}
		return false;
	}

	private void GetNextRecord()
	{
		if (_currentStaticResourceRecords != null)
		{
			_currentBamlRecord = _currentStaticResourceRecords[_currentStaticResourceRecordIndex++];
			if (_currentStaticResourceRecordIndex == _currentStaticResourceRecords.Count)
			{
				_currentStaticResourceRecords = null;
				_currentStaticResourceRecordIndex = -1;
			}
		}
		else
		{
			_currentBamlRecord = _bamlRecordReader.GetNextRecord();
		}
	}

	private void ReadNextRecord()
	{
		if (_readState == ReadState.Initial)
		{
			_bamlRecordReader.ReadVersionHeader();
		}
		bool flag = true;
		while (flag)
		{
			if (_haveUnprocessedRecord)
			{
				_haveUnprocessedRecord = false;
			}
			else
			{
				GetNextRecord();
			}
			if (_currentBamlRecord == null)
			{
				NodeTypeInternal = BamlNodeType.None;
				_readState = ReadState.EndOfFile;
				ClearProperties();
				break;
			}
			_readState = ReadState.Interactive;
			flag = false;
			switch (_currentBamlRecord.RecordType)
			{
			case BamlRecordType.AssemblyInfo:
				ReadAssemblyInfoRecord();
				flag = true;
				break;
			case BamlRecordType.TypeInfo:
			case BamlRecordType.TypeSerializerInfo:
				MapTable.LoadTypeInfoRecord((BamlTypeInfoRecord)_currentBamlRecord);
				flag = true;
				break;
			case BamlRecordType.AttributeInfo:
				MapTable.LoadAttributeInfoRecord((BamlAttributeInfoRecord)_currentBamlRecord);
				flag = true;
				break;
			case BamlRecordType.StringInfo:
				MapTable.LoadStringInfoRecord((BamlStringInfoRecord)_currentBamlRecord);
				flag = true;
				break;
			case BamlRecordType.ContentProperty:
				ReadContentPropertyRecord();
				flag = true;
				break;
			case BamlRecordType.DocumentStart:
				ReadDocumentStartRecord();
				break;
			case BamlRecordType.DocumentEnd:
				ReadDocumentEndRecord();
				break;
			case BamlRecordType.PIMapping:
				ReadPIMappingRecord();
				break;
			case BamlRecordType.LiteralContent:
				ReadLiteralContentRecord();
				break;
			case BamlRecordType.ElementStart:
			case BamlRecordType.StaticResourceStart:
				ReadElementStartRecord();
				break;
			case BamlRecordType.ElementEnd:
			case BamlRecordType.StaticResourceEnd:
				ReadElementEndRecord();
				break;
			case BamlRecordType.PropertyComplexStart:
			case BamlRecordType.PropertyArrayStart:
			case BamlRecordType.PropertyIListStart:
			case BamlRecordType.PropertyIDictionaryStart:
				ReadPropertyComplexStartRecord();
				break;
			case BamlRecordType.PropertyComplexEnd:
			case BamlRecordType.PropertyArrayEnd:
			case BamlRecordType.PropertyIListEnd:
			case BamlRecordType.PropertyIDictionaryEnd:
				ReadPropertyComplexEndRecord();
				break;
			case BamlRecordType.Text:
			case BamlRecordType.TextWithConverter:
			case BamlRecordType.TextWithId:
				ReadTextRecord();
				break;
			case BamlRecordType.DeferableContentStart:
				ReadDeferableContentRecord();
				flag = true;
				break;
			case BamlRecordType.ConstructorParametersStart:
				ReadConstructorStart();
				break;
			case BamlRecordType.ConstructorParametersEnd:
				ReadConstructorEnd();
				break;
			case BamlRecordType.ConnectionId:
				ReadConnectionIdRecord();
				break;
			case BamlRecordType.StaticResourceId:
				ReadStaticResourceId();
				flag = true;
				break;
			default:
				throw new InvalidOperationException(SR.Format(SR.ParserUnknownBaml, ((int)_currentBamlRecord.RecordType).ToString(CultureInfo.CurrentCulture)));
			}
		}
	}

	private void ReadProperties()
	{
		while (!_haveUnprocessedRecord)
		{
			GetNextRecord();
			ProcessPropertyRecord();
		}
	}

	private void ProcessPropertyRecord()
	{
		switch (_currentBamlRecord.RecordType)
		{
		case BamlRecordType.AssemblyInfo:
			ReadAssemblyInfoRecord();
			break;
		case BamlRecordType.TypeInfo:
		case BamlRecordType.TypeSerializerInfo:
			MapTable.LoadTypeInfoRecord((BamlTypeInfoRecord)_currentBamlRecord);
			break;
		case BamlRecordType.AttributeInfo:
			MapTable.LoadAttributeInfoRecord((BamlAttributeInfoRecord)_currentBamlRecord);
			break;
		case BamlRecordType.StringInfo:
			MapTable.LoadStringInfoRecord((BamlStringInfoRecord)_currentBamlRecord);
			break;
		case BamlRecordType.XmlnsProperty:
			ReadXmlnsPropertyRecord();
			break;
		case BamlRecordType.ConnectionId:
			ReadConnectionIdRecord();
			break;
		case BamlRecordType.Property:
		case BamlRecordType.PropertyWithConverter:
			ReadPropertyRecord();
			break;
		case BamlRecordType.ContentProperty:
			ReadContentPropertyRecord();
			break;
		case BamlRecordType.PropertyStringReference:
			ReadPropertyStringRecord();
			break;
		case BamlRecordType.PropertyTypeReference:
			ReadPropertyTypeRecord();
			break;
		case BamlRecordType.PropertyWithExtension:
			ReadPropertyWithExtensionRecord();
			break;
		case BamlRecordType.PropertyWithStaticResourceId:
			ReadPropertyWithStaticResourceIdRecord();
			break;
		case BamlRecordType.PropertyCustom:
			ReadPropertyCustomRecord();
			break;
		case BamlRecordType.DefAttribute:
			ReadDefAttributeRecord();
			break;
		case BamlRecordType.PresentationOptionsAttribute:
			ReadPresentationOptionsAttributeRecord();
			break;
		case BamlRecordType.DefAttributeKeyType:
			ReadDefAttributeKeyTypeRecord();
			break;
		case BamlRecordType.RoutedEvent:
			ReadRoutedEventRecord();
			break;
		case BamlRecordType.ClrEvent:
			ReadClrEventRecord();
			break;
		case BamlRecordType.KeyElementStart:
		{
			BamlKeyInfo info = ProcessKeyTree();
			AddToPropertyInfoCollection(info);
			break;
		}
		default:
			_haveUnprocessedRecord = true;
			break;
		}
	}

	private void ReadXmlnsPropertyRecord()
	{
		BamlXmlnsPropertyRecord bamlXmlnsPropertyRecord = (BamlXmlnsPropertyRecord)_currentBamlRecord;
		_parserContext.XmlnsDictionary[bamlXmlnsPropertyRecord.Prefix] = bamlXmlnsPropertyRecord.XmlNamespace;
		_prefixDictionary[bamlXmlnsPropertyRecord.XmlNamespace] = bamlXmlnsPropertyRecord.Prefix;
		BamlPropertyInfo bamlPropertyInfo = new BamlPropertyInfo();
		bamlPropertyInfo.Value = bamlXmlnsPropertyRecord.XmlNamespace;
		bamlPropertyInfo.XmlNamespace = string.Empty;
		bamlPropertyInfo.ClrNamespace = string.Empty;
		bamlPropertyInfo.AssemblyName = string.Empty;
		bamlPropertyInfo.Prefix = "xmlns";
		bamlPropertyInfo.LocalName = ((bamlXmlnsPropertyRecord.Prefix == null) ? string.Empty : bamlXmlnsPropertyRecord.Prefix);
		bamlPropertyInfo.Name = ((bamlXmlnsPropertyRecord.Prefix == null || bamlXmlnsPropertyRecord.Prefix == string.Empty) ? "xmlns" : ("xmlns:" + bamlXmlnsPropertyRecord.Prefix));
		bamlPropertyInfo.RecordType = BamlRecordType.XmlnsProperty;
		AddToPropertyInfoCollection(bamlPropertyInfo);
	}

	private void ReadPropertyRecord()
	{
		string value = ((BamlPropertyRecord)_currentBamlRecord).Value;
		value = MarkupExtensionParser.AddEscapeToLiteralString(value);
		AddToPropertyInfoCollection(ReadPropertyRecordCore(value));
	}

	private void ReadContentPropertyRecord()
	{
		BamlContentPropertyInfo bamlContentPropertyInfo = new BamlContentPropertyInfo();
		BamlContentPropertyRecord bamlContentPropertyRecord = (BamlContentPropertyRecord)_currentBamlRecord;
		SetCommonPropertyInfo(bamlContentPropertyInfo, bamlContentPropertyRecord.AttributeId);
		bamlContentPropertyInfo.RecordType = _currentBamlRecord.RecordType;
		AddToPropertyInfoCollection(bamlContentPropertyInfo);
	}

	private void ReadPropertyStringRecord()
	{
		string stringFromStringId = MapTable.GetStringFromStringId(((BamlPropertyStringReferenceRecord)_currentBamlRecord).StringId);
		AddToPropertyInfoCollection(ReadPropertyRecordCore(stringFromStringId));
	}

	private void ReadPropertyTypeRecord()
	{
		BamlPropertyInfo bamlPropertyInfo = new BamlPropertyInfo();
		SetCommonPropertyInfo(bamlPropertyInfo, ((BamlPropertyTypeReferenceRecord)_currentBamlRecord).AttributeId);
		bamlPropertyInfo.RecordType = _currentBamlRecord.RecordType;
		bamlPropertyInfo.Value = GetTypeValueString(((BamlPropertyTypeReferenceRecord)_currentBamlRecord).TypeId);
		bamlPropertyInfo.AttributeUsage = BamlAttributeUsage.Default;
		AddToPropertyInfoCollection(bamlPropertyInfo);
	}

	private void ReadPropertyWithExtensionRecord()
	{
		BamlPropertyInfo bamlPropertyInfo = new BamlPropertyInfo();
		SetCommonPropertyInfo(bamlPropertyInfo, ((BamlPropertyWithExtensionRecord)_currentBamlRecord).AttributeId);
		bamlPropertyInfo.RecordType = _currentBamlRecord.RecordType;
		bamlPropertyInfo.Value = GetExtensionValueString((IOptimizedMarkupExtension)_currentBamlRecord);
		bamlPropertyInfo.AttributeUsage = BamlAttributeUsage.Default;
		AddToPropertyInfoCollection(bamlPropertyInfo);
	}

	private void ReadPropertyWithStaticResourceIdRecord()
	{
		BamlPropertyWithStaticResourceIdRecord bamlPropertyWithStaticResourceIdRecord = (BamlPropertyWithStaticResourceIdRecord)_currentBamlRecord;
		BamlPropertyInfo bamlPropertyInfo = new BamlPropertyInfo();
		SetCommonPropertyInfo(bamlPropertyInfo, bamlPropertyWithStaticResourceIdRecord.AttributeId);
		bamlPropertyInfo.RecordType = _currentBamlRecord.RecordType;
		BamlOptimizedStaticResourceRecord optimizedMarkupExtensionRecord = (BamlOptimizedStaticResourceRecord)_currentKeyInfo.StaticResources[bamlPropertyWithStaticResourceIdRecord.StaticResourceId][0];
		bamlPropertyInfo.Value = GetExtensionValueString(optimizedMarkupExtensionRecord);
		bamlPropertyInfo.AttributeUsage = BamlAttributeUsage.Default;
		AddToPropertyInfoCollection(bamlPropertyInfo);
	}

	private BamlPropertyInfo ReadPropertyRecordCore(string value)
	{
		BamlPropertyInfo bamlPropertyInfo = new BamlPropertyInfo();
		SetCommonPropertyInfo(bamlPropertyInfo, ((BamlPropertyRecord)_currentBamlRecord).AttributeId);
		bamlPropertyInfo.RecordType = _currentBamlRecord.RecordType;
		bamlPropertyInfo.Value = value;
		return bamlPropertyInfo;
	}

	private void ReadPropertyCustomRecord()
	{
		BamlPropertyInfo propertyCustomRecordInfo = GetPropertyCustomRecordInfo();
		AddToPropertyInfoCollection(propertyCustomRecordInfo);
	}

	private BamlPropertyInfo GetPropertyCustomRecordInfo()
	{
		BamlPropertyInfo bamlPropertyInfo = new BamlPropertyInfo();
		BamlAttributeInfoRecord bamlAttributeInfoRecord = SetCommonPropertyInfo(bamlPropertyInfo, ((BamlPropertyCustomRecord)_currentBamlRecord).AttributeId);
		bamlPropertyInfo.RecordType = _currentBamlRecord.RecordType;
		bamlPropertyInfo.AttributeUsage = BamlAttributeUsage.Default;
		BamlPropertyCustomRecord bamlPropertyCustomRecord = (BamlPropertyCustomRecord)_currentBamlRecord;
		if (bamlAttributeInfoRecord.DP == null && bamlAttributeInfoRecord.PropInfo == null)
		{
			bamlAttributeInfoRecord.DP = MapTable.GetDependencyProperty(bamlAttributeInfoRecord);
			if (bamlAttributeInfoRecord.OwnerType == null)
			{
				throw new InvalidOperationException(SR.Format(SR.BamlReaderNoOwnerType, bamlAttributeInfoRecord.Name, AssemblyName));
			}
			if (bamlAttributeInfoRecord.DP == null)
			{
				try
				{
					bamlAttributeInfoRecord.PropInfo = bamlAttributeInfoRecord.OwnerType.GetProperty(bamlAttributeInfoRecord.Name, BindingFlags.Instance | BindingFlags.Public);
				}
				catch (AmbiguousMatchException)
				{
					PropertyInfo[] properties = bamlAttributeInfoRecord.OwnerType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
					for (int i = 0; i < properties.Length; i++)
					{
						if (properties[i].Name == bamlAttributeInfoRecord.Name)
						{
							bamlAttributeInfoRecord.PropInfo = properties[i];
							break;
						}
					}
				}
				if (bamlAttributeInfoRecord.PropInfo == null)
				{
					throw new InvalidOperationException(SR.Format(SR.ParserCantGetDPOrPi, bamlPropertyInfo.Name));
				}
			}
		}
		Type propertyType = bamlAttributeInfoRecord.GetPropertyType();
		string name = bamlAttributeInfoRecord.Name;
		if (bamlPropertyCustomRecord.SerializerTypeId == 137)
		{
			Type declaringType = null;
			_propertyDP = _bamlRecordReader.GetCustomDependencyPropertyValue(bamlPropertyCustomRecord, out declaringType);
			declaringType = ((declaringType == null) ? _propertyDP.OwnerType : declaringType);
			bamlPropertyInfo.Value = declaringType.Name + "." + _propertyDP.Name;
			string xmlNamespace = _parserContext.XamlTypeMapper.GetXmlNamespace(declaringType.Namespace, declaringType.Assembly.FullName);
			string xmlnsPrefix = GetXmlnsPrefix(xmlNamespace);
			if (xmlnsPrefix != string.Empty)
			{
				bamlPropertyInfo.Value = xmlnsPrefix + ":" + bamlPropertyInfo.Value;
			}
			if (!_propertyDP.PropertyType.IsEnum)
			{
				_propertyDP = null;
			}
		}
		else
		{
			if (_propertyDP != null)
			{
				propertyType = _propertyDP.PropertyType;
				name = _propertyDP.Name;
				_propertyDP = null;
			}
			object customValue = _bamlRecordReader.GetCustomValue(bamlPropertyCustomRecord, propertyType, name);
			TypeConverter converter = TypeDescriptor.GetConverter(customValue.GetType());
			bamlPropertyInfo.Value = converter.ConvertToString(null, TypeConverterHelper.InvariantEnglishUS, customValue);
		}
		return bamlPropertyInfo;
	}

	private void ReadDefAttributeRecord()
	{
		BamlDefAttributeRecord bamlDefAttributeRecord = (BamlDefAttributeRecord)_currentBamlRecord;
		bamlDefAttributeRecord.Name = MapTable.GetStringFromStringId(bamlDefAttributeRecord.NameId);
		BamlPropertyInfo bamlPropertyInfo = new BamlPropertyInfo();
		bamlPropertyInfo.Value = bamlDefAttributeRecord.Value;
		bamlPropertyInfo.AssemblyName = string.Empty;
		bamlPropertyInfo.Prefix = _prefixDictionary["http://schemas.microsoft.com/winfx/2006/xaml"];
		bamlPropertyInfo.XmlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
		bamlPropertyInfo.ClrNamespace = string.Empty;
		bamlPropertyInfo.Name = bamlDefAttributeRecord.Name;
		bamlPropertyInfo.LocalName = bamlPropertyInfo.Name;
		bamlPropertyInfo.RecordType = BamlRecordType.DefAttribute;
		AddToPropertyInfoCollection(bamlPropertyInfo);
	}

	private void ReadPresentationOptionsAttributeRecord()
	{
		BamlPresentationOptionsAttributeRecord bamlPresentationOptionsAttributeRecord = (BamlPresentationOptionsAttributeRecord)_currentBamlRecord;
		bamlPresentationOptionsAttributeRecord.Name = MapTable.GetStringFromStringId(bamlPresentationOptionsAttributeRecord.NameId);
		BamlPropertyInfo bamlPropertyInfo = new BamlPropertyInfo();
		bamlPropertyInfo.Value = bamlPresentationOptionsAttributeRecord.Value;
		bamlPropertyInfo.AssemblyName = string.Empty;
		bamlPropertyInfo.Prefix = _prefixDictionary["http://schemas.microsoft.com/winfx/2006/xaml/presentation/options"];
		bamlPropertyInfo.XmlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation/options";
		bamlPropertyInfo.ClrNamespace = string.Empty;
		bamlPropertyInfo.Name = bamlPresentationOptionsAttributeRecord.Name;
		bamlPropertyInfo.LocalName = bamlPropertyInfo.Name;
		bamlPropertyInfo.RecordType = BamlRecordType.PresentationOptionsAttribute;
		AddToPropertyInfoCollection(bamlPropertyInfo);
	}

	private void ReadDefAttributeKeyTypeRecord()
	{
		BamlDefAttributeKeyTypeRecord bamlDefAttributeKeyTypeRecord = (BamlDefAttributeKeyTypeRecord)_currentBamlRecord;
		BamlPropertyInfo bamlPropertyInfo = new BamlPropertyInfo();
		bamlPropertyInfo.Value = GetTypeValueString(bamlDefAttributeKeyTypeRecord.TypeId);
		bamlPropertyInfo.AssemblyName = string.Empty;
		bamlPropertyInfo.Prefix = _prefixDictionary["http://schemas.microsoft.com/winfx/2006/xaml"];
		bamlPropertyInfo.XmlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
		bamlPropertyInfo.ClrNamespace = string.Empty;
		bamlPropertyInfo.Name = "Key";
		bamlPropertyInfo.LocalName = bamlPropertyInfo.Name;
		bamlPropertyInfo.RecordType = BamlRecordType.DefAttribute;
		AddToPropertyInfoCollection(bamlPropertyInfo);
	}

	private void ReadDeferableContentRecord()
	{
		_deferableContentBlockDepth = _nodeStack.Count;
		_deferableContentPosition = ReadDeferKeys();
	}

	private long ReadDeferKeys()
	{
		long result = -1L;
		_deferKeys = new List<BamlKeyInfo>();
		while (!_haveUnprocessedRecord)
		{
			GetNextRecord();
			ProcessDeferKey();
			if (!_haveUnprocessedRecord)
			{
				result = _bamlRecordReader.StreamPosition;
			}
		}
		return result;
	}

	private void ProcessDeferKey()
	{
		switch (_currentBamlRecord.RecordType)
		{
		case BamlRecordType.DefAttributeKeyString:
			if (_currentBamlRecord is BamlDefAttributeKeyStringRecord bamlDefAttributeKeyStringRecord)
			{
				BamlKeyInfo bamlKeyInfo2 = CheckForSharedness();
				if (bamlKeyInfo2 != null)
				{
					_deferKeys.Add(bamlKeyInfo2);
				}
				bamlDefAttributeKeyStringRecord.Value = MapTable.GetStringFromStringId(bamlDefAttributeKeyStringRecord.ValueId);
				bamlKeyInfo2 = new BamlKeyInfo();
				bamlKeyInfo2.Value = bamlDefAttributeKeyStringRecord.Value;
				bamlKeyInfo2.AssemblyName = string.Empty;
				bamlKeyInfo2.Prefix = _prefixDictionary["http://schemas.microsoft.com/winfx/2006/xaml"];
				bamlKeyInfo2.XmlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
				bamlKeyInfo2.ClrNamespace = string.Empty;
				bamlKeyInfo2.Name = "Key";
				bamlKeyInfo2.LocalName = bamlKeyInfo2.Name;
				bamlKeyInfo2.RecordType = BamlRecordType.DefAttribute;
				bamlKeyInfo2.Offset = ((IBamlDictionaryKey)bamlDefAttributeKeyStringRecord).ValuePosition;
				_deferKeys.Add(bamlKeyInfo2);
			}
			break;
		case BamlRecordType.DefAttributeKeyType:
			if (_currentBamlRecord is BamlDefAttributeKeyTypeRecord bamlDefAttributeKeyTypeRecord)
			{
				string text = _prefixDictionary["http://schemas.microsoft.com/winfx/2006/xaml"];
				string text2 = ((!(text != string.Empty)) ? "{Type " : ("{" + text + ":Type "));
				BamlTypeInfoRecord typeInfoFromId = MapTable.GetTypeInfoFromId(bamlDefAttributeKeyTypeRecord.TypeId);
				string typeFullName = typeInfoFromId.TypeFullName;
				typeFullName = typeFullName.Substring(typeFullName.LastIndexOf('.') + 1);
				GetAssemblyAndPrefixAndXmlns(typeInfoFromId, out var _, out var prefix, out var _);
				typeFullName = ((!(prefix != string.Empty)) ? (text2 + typeFullName + "}") : (text2 + prefix + ":" + typeFullName + "}"));
				BamlKeyInfo bamlKeyInfo = new BamlKeyInfo();
				bamlKeyInfo.Value = typeFullName;
				bamlKeyInfo.AssemblyName = string.Empty;
				bamlKeyInfo.Prefix = text;
				bamlKeyInfo.XmlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
				bamlKeyInfo.ClrNamespace = string.Empty;
				bamlKeyInfo.Name = "Key";
				bamlKeyInfo.LocalName = bamlKeyInfo.Name;
				bamlKeyInfo.RecordType = BamlRecordType.DefAttribute;
				bamlKeyInfo.Offset = ((IBamlDictionaryKey)bamlDefAttributeKeyTypeRecord).ValuePosition;
				_deferKeys.Add(bamlKeyInfo);
			}
			break;
		case BamlRecordType.KeyElementStart:
		{
			BamlKeyInfo bamlKeyInfo3 = CheckForSharedness();
			if (bamlKeyInfo3 != null)
			{
				_deferKeys.Add(bamlKeyInfo3);
			}
			bamlKeyInfo3 = ProcessKeyTree();
			_deferKeys.Add(bamlKeyInfo3);
			break;
		}
		case BamlRecordType.StaticResourceStart:
		case BamlRecordType.OptimizedStaticResource:
		{
			List<BamlRecord> list = new List<BamlRecord>();
			_currentBamlRecord.Pin();
			list.Add(_currentBamlRecord);
			if (_currentBamlRecord.RecordType == BamlRecordType.StaticResourceStart)
			{
				ProcessStaticResourceTree(list);
			}
			_deferKeys[_deferKeys.Count - 1].StaticResources.Add(list);
			break;
		}
		default:
			_haveUnprocessedRecord = true;
			break;
		}
	}

	private BamlKeyInfo CheckForSharedness()
	{
		IBamlDictionaryKey bamlDictionaryKey = (IBamlDictionaryKey)_currentBamlRecord;
		if (!bamlDictionaryKey.SharedSet)
		{
			return null;
		}
		BamlKeyInfo obj = new BamlKeyInfo
		{
			Value = bamlDictionaryKey.Shared.ToString(),
			AssemblyName = string.Empty,
			Prefix = _prefixDictionary["http://schemas.microsoft.com/winfx/2006/xaml"],
			XmlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml",
			ClrNamespace = string.Empty,
			Name = "Shared"
		};
		obj.LocalName = obj.Name;
		obj.RecordType = BamlRecordType.DefAttribute;
		obj.Offset = bamlDictionaryKey.ValuePosition;
		return obj;
	}

	private BamlKeyInfo ProcessKeyTree()
	{
		BamlKeyElementStartRecord bamlKeyElementStartRecord = _currentBamlRecord as BamlKeyElementStartRecord;
		BamlTypeInfoRecord typeInfoFromId = MapTable.GetTypeInfoFromId(bamlKeyElementStartRecord.TypeId);
		string typeFullName = typeInfoFromId.TypeFullName;
		typeFullName = typeFullName.Substring(typeFullName.LastIndexOf('.') + 1);
		GetAssemblyAndPrefixAndXmlns(typeInfoFromId, out var assemblyFullName, out var prefix, out var xmlns);
		typeFullName = ((!(prefix != string.Empty)) ? ("{" + typeFullName + " ") : ("{" + prefix + ":" + typeFullName + " "));
		bool flag = true;
		Stack stack = new Stack();
		Stack stack2 = new Stack();
		Stack stack3 = new Stack();
		stack.Push(false);
		stack2.Push(false);
		stack3.Push(false);
		while (flag)
		{
			if (!_haveUnprocessedRecord)
			{
				GetNextRecord();
			}
			else
			{
				_haveUnprocessedRecord = false;
			}
			switch (_currentBamlRecord.RecordType)
			{
			case BamlRecordType.AssemblyInfo:
				ReadAssemblyInfoRecord();
				break;
			case BamlRecordType.TypeInfo:
			case BamlRecordType.TypeSerializerInfo:
				MapTable.LoadTypeInfoRecord((BamlTypeInfoRecord)_currentBamlRecord);
				break;
			case BamlRecordType.AttributeInfo:
				MapTable.LoadAttributeInfoRecord((BamlAttributeInfoRecord)_currentBamlRecord);
				break;
			case BamlRecordType.StringInfo:
				MapTable.LoadStringInfoRecord((BamlStringInfoRecord)_currentBamlRecord);
				break;
			case BamlRecordType.PropertyComplexStart:
			{
				ReadPropertyComplexStartRecord();
				BamlNodeInfo bamlNodeInfo = (BamlNodeInfo)_nodeStack.Pop();
				if ((bool)stack.Pop())
				{
					typeFullName += ", ";
				}
				typeFullName = typeFullName + bamlNodeInfo.LocalName + "=";
				stack.Push(true);
				break;
			}
			case BamlRecordType.Text:
			case BamlRecordType.TextWithId:
			{
				if (_currentBamlRecord is BamlTextWithIdRecord bamlTextWithIdRecord)
				{
					bamlTextWithIdRecord.Value = MapTable.GetStringFromStringId(bamlTextWithIdRecord.ValueId);
				}
				string text = EscapeString(((BamlTextRecord)_currentBamlRecord).Value);
				if ((bool)stack3.Peek())
				{
					typeFullName += ", ";
				}
				typeFullName += text;
				if ((bool)stack2.Peek())
				{
					stack3.Pop();
					stack3.Push(true);
				}
				break;
			}
			case BamlRecordType.ElementStart:
			{
				if ((bool)stack3.Peek())
				{
					typeFullName += ", ";
				}
				if ((bool)stack2.Peek())
				{
					stack3.Pop();
					stack3.Push(true);
				}
				stack.Push(false);
				stack2.Push(false);
				stack3.Push(false);
				BamlElementStartRecord bamlElementStartRecord = _currentBamlRecord as BamlElementStartRecord;
				BamlTypeInfoRecord typeInfoFromId2 = MapTable.GetTypeInfoFromId(bamlElementStartRecord.TypeId);
				string typeFullName2 = typeInfoFromId2.TypeFullName;
				typeFullName2 = typeFullName2.Substring(typeFullName2.LastIndexOf('.') + 1);
				GetAssemblyAndPrefixAndXmlns(typeInfoFromId2, out assemblyFullName, out prefix, out xmlns);
				typeFullName = ((!(prefix != string.Empty)) ? ("{" + typeFullName2 + " ") : (typeFullName + "{" + prefix + ":" + typeFullName2 + " "));
				break;
			}
			case BamlRecordType.ElementEnd:
				stack.Pop();
				stack2.Pop();
				stack3.Pop();
				typeFullName += "}";
				break;
			case BamlRecordType.ConstructorParametersStart:
				stack2.Pop();
				stack2.Push(true);
				break;
			case BamlRecordType.ConstructorParametersEnd:
				stack2.Pop();
				stack2.Push(false);
				stack3.Pop();
				stack3.Push(false);
				break;
			case BamlRecordType.ConstructorParameterType:
			{
				if ((bool)stack3.Peek())
				{
					typeFullName += ", ";
				}
				if ((bool)stack2.Peek())
				{
					stack3.Pop();
					stack3.Push(true);
				}
				BamlConstructorParameterTypeRecord bamlConstructorParameterTypeRecord = _currentBamlRecord as BamlConstructorParameterTypeRecord;
				typeFullName += GetTypeValueString(bamlConstructorParameterTypeRecord.TypeId);
				break;
			}
			case BamlRecordType.Property:
			case BamlRecordType.PropertyWithConverter:
			{
				string value = ((BamlPropertyRecord)_currentBamlRecord).Value;
				BamlPropertyInfo bamlPropertyInfo2 = ReadPropertyRecordCore(value);
				if ((bool)stack.Pop())
				{
					typeFullName += ", ";
				}
				typeFullName = typeFullName + bamlPropertyInfo2.LocalName + "=" + bamlPropertyInfo2.Value;
				stack.Push(true);
				break;
			}
			case BamlRecordType.PropertyCustom:
			{
				BamlPropertyInfo propertyCustomRecordInfo = GetPropertyCustomRecordInfo();
				if ((bool)stack.Pop())
				{
					typeFullName += ", ";
				}
				typeFullName = typeFullName + propertyCustomRecordInfo.LocalName + "=" + propertyCustomRecordInfo.Value;
				stack.Push(true);
				break;
			}
			case BamlRecordType.PropertyStringReference:
			{
				string stringFromStringId = MapTable.GetStringFromStringId(((BamlPropertyStringReferenceRecord)_currentBamlRecord).StringId);
				BamlPropertyInfo bamlPropertyInfo = ReadPropertyRecordCore(stringFromStringId);
				if ((bool)stack.Pop())
				{
					typeFullName += ", ";
				}
				typeFullName = typeFullName + bamlPropertyInfo.LocalName + "=" + bamlPropertyInfo.Value;
				stack.Push(true);
				break;
			}
			case BamlRecordType.PropertyTypeReference:
			{
				string typeValueString = GetTypeValueString(((BamlPropertyTypeReferenceRecord)_currentBamlRecord).TypeId);
				string attributeNameFromId2 = MapTable.GetAttributeNameFromId(((BamlPropertyTypeReferenceRecord)_currentBamlRecord).AttributeId);
				if ((bool)stack.Pop())
				{
					typeFullName += ", ";
				}
				typeFullName = typeFullName + attributeNameFromId2 + "=" + typeValueString;
				stack.Push(true);
				break;
			}
			case BamlRecordType.PropertyWithExtension:
			{
				string extensionValueString = GetExtensionValueString((BamlPropertyWithExtensionRecord)_currentBamlRecord);
				string attributeNameFromId = MapTable.GetAttributeNameFromId(((BamlPropertyWithExtensionRecord)_currentBamlRecord).AttributeId);
				if ((bool)stack.Pop())
				{
					typeFullName += ", ";
				}
				typeFullName = typeFullName + attributeNameFromId + "=" + extensionValueString;
				stack.Push(true);
				break;
			}
			case BamlRecordType.KeyElementEnd:
				typeFullName += "}";
				flag = false;
				_haveUnprocessedRecord = false;
				break;
			default:
				throw new InvalidOperationException(SR.Format(SR.ParserUnknownBaml, ((int)_currentBamlRecord.RecordType).ToString(CultureInfo.CurrentCulture)));
			case BamlRecordType.PropertyComplexEnd:
				break;
			}
		}
		BamlKeyInfo obj = new BamlKeyInfo
		{
			Value = typeFullName,
			AssemblyName = string.Empty,
			Prefix = _prefixDictionary["http://schemas.microsoft.com/winfx/2006/xaml"],
			XmlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml",
			ClrNamespace = string.Empty,
			Name = "Key"
		};
		obj.LocalName = obj.Name;
		obj.RecordType = BamlRecordType.DefAttribute;
		obj.Offset = ((IBamlDictionaryKey)bamlKeyElementStartRecord).ValuePosition;
		return obj;
	}

	private void ProcessStaticResourceTree(List<BamlRecord> srRecords)
	{
		bool flag = true;
		while (flag)
		{
			if (_haveUnprocessedRecord)
			{
				_haveUnprocessedRecord = false;
			}
			else
			{
				GetNextRecord();
			}
			_currentBamlRecord.Pin();
			srRecords.Add(_currentBamlRecord);
			if (_currentBamlRecord.RecordType == BamlRecordType.StaticResourceEnd)
			{
				flag = false;
			}
		}
	}

	private void ReadStaticResourceId()
	{
		BamlStaticResourceIdRecord bamlStaticResourceIdRecord = (BamlStaticResourceIdRecord)_currentBamlRecord;
		_currentStaticResourceRecords = _currentKeyInfo.StaticResources[bamlStaticResourceIdRecord.StaticResourceId];
		_currentStaticResourceRecordIndex = 0;
	}

	private string EscapeString(string value)
	{
		StringBuilder stringBuilder = null;
		for (int i = 0; i < value.Length; i++)
		{
			if (value[i] == '{' || value[i] == '}')
			{
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder(value.Length + 2);
					stringBuilder.Append(value, 0, i);
				}
				stringBuilder.Append('\\');
			}
			stringBuilder?.Append(value[i]);
		}
		if (stringBuilder == null)
		{
			return value;
		}
		return stringBuilder.ToString();
	}

	private void ReadRoutedEventRecord()
	{
		throw new InvalidOperationException(SR.Format(SR.ParserBamlEvent, string.Empty));
	}

	private void ReadClrEventRecord()
	{
		throw new InvalidOperationException(SR.Format(SR.ParserBamlEvent, string.Empty));
	}

	private void ReadDocumentStartRecord()
	{
		ClearProperties();
		NodeTypeInternal = BamlNodeType.StartDocument;
		BamlDocumentStartRecord bamlDocumentStartRecord = (BamlDocumentStartRecord)_currentBamlRecord;
		_parserContext.IsDebugBamlStream = bamlDocumentStartRecord.DebugBaml;
		BamlNodeInfo bamlNodeInfo = new BamlNodeInfo();
		bamlNodeInfo.RecordType = BamlRecordType.DocumentStart;
		_nodeStack.Push(bamlNodeInfo);
	}

	private void ReadDocumentEndRecord()
	{
		BamlNodeInfo bamlNodeInfo = (BamlNodeInfo)_nodeStack.Pop();
		if (bamlNodeInfo.RecordType != BamlRecordType.DocumentStart)
		{
			throw new InvalidOperationException(SR.Format(SR.BamlScopeError, bamlNodeInfo.RecordType.ToString(), "DocumentEnd"));
		}
		ClearProperties();
		NodeTypeInternal = BamlNodeType.EndDocument;
	}

	private void ReadAssemblyInfoRecord()
	{
		BamlAssemblyInfoRecord bamlAssemblyInfoRecord = (BamlAssemblyInfoRecord)_currentBamlRecord;
		MapTable.LoadAssemblyInfoRecord(bamlAssemblyInfoRecord);
		Assembly assembly = Assembly.Load(bamlAssemblyInfoRecord.AssemblyFullName);
		object[] customAttributes = assembly.GetCustomAttributes(typeof(XmlnsDefinitionAttribute), inherit: true);
		for (int i = 0; i < customAttributes.Length; i++)
		{
			XmlnsDefinitionAttribute xmlnsDefinitionAttribute = (XmlnsDefinitionAttribute)customAttributes[i];
			SetXmlNamespace(xmlnsDefinitionAttribute.ClrNamespace, assembly.FullName, xmlnsDefinitionAttribute.XmlNamespace);
		}
	}

	private void ReadPIMappingRecord()
	{
		BamlPIMappingRecord bamlPIMappingRecord = (BamlPIMappingRecord)_currentBamlRecord;
		BamlAssemblyInfoRecord assemblyInfoFromId = MapTable.GetAssemblyInfoFromId(bamlPIMappingRecord.AssemblyId);
		if (assemblyInfoFromId == null)
		{
			throw new InvalidOperationException(SR.ParserMapPIMissingAssembly);
		}
		if (!_parserContext.XamlTypeMapper.PITable.Contains(bamlPIMappingRecord.XmlNamespace))
		{
			_parserContext.XamlTypeMapper.AddMappingProcessingInstruction(bamlPIMappingRecord.XmlNamespace, bamlPIMappingRecord.ClrNamespace, assemblyInfoFromId.AssemblyFullName);
		}
		ClearProperties();
		NodeTypeInternal = BamlNodeType.PIMapping;
		_name = "Mapping";
		_localName = _name;
		_ownerTypeName = string.Empty;
		_xmlNamespace = bamlPIMappingRecord.XmlNamespace;
		_clrNamespace = bamlPIMappingRecord.ClrNamespace;
		_assemblyName = assemblyInfoFromId.AssemblyFullName;
		IFormatProvider formatProvider = null;
		IFormatProvider provider = formatProvider;
		Span<char> initialBuffer = stackalloc char[100];
		DefaultInterpolatedStringHandler handler = new DefaultInterpolatedStringHandler(43, 3, formatProvider, initialBuffer);
		handler.AppendLiteral("XmlNamespace=\"");
		handler.AppendFormatted(_xmlNamespace);
		handler.AppendLiteral("\" ClrNamespace=\"");
		handler.AppendFormatted(_clrNamespace);
		handler.AppendLiteral("\" Assembly=\"");
		handler.AppendFormatted(_assemblyName);
		handler.AppendLiteral("\"");
		_value = string.Create(provider, initialBuffer, ref handler);
	}

	private void ReadLiteralContentRecord()
	{
		ClearProperties();
		BamlLiteralContentRecord bamlLiteralContentRecord = (BamlLiteralContentRecord)_currentBamlRecord;
		NodeTypeInternal = BamlNodeType.LiteralContent;
		_value = bamlLiteralContentRecord.Value;
	}

	private void ReadConnectionIdRecord()
	{
		BamlConnectionIdRecord bamlConnectionIdRecord = (BamlConnectionIdRecord)_currentBamlRecord;
		AddToPropertyInfoCollection(bamlConnectionIdRecord.ConnectionId);
	}

	private void ReadElementStartRecord()
	{
		ClearProperties();
		_propertyDP = null;
		_parserContext.PushScope();
		_prefixDictionary.PushScope();
		BamlElementStartRecord bamlElementStartRecord = (BamlElementStartRecord)_currentBamlRecord;
		BamlTypeInfoRecord typeInfoFromId = MapTable.GetTypeInfoFromId(bamlElementStartRecord.TypeId);
		NodeTypeInternal = BamlNodeType.StartElement;
		_name = typeInfoFromId.TypeFullName;
		_localName = _name.Substring(_name.LastIndexOf('.') + 1);
		_ownerTypeName = string.Empty;
		_clrNamespace = typeInfoFromId.ClrNamespace;
		GetAssemblyAndPrefixAndXmlns(typeInfoFromId, out _assemblyName, out _prefix, out _xmlNamespace);
		BamlNodeInfo bamlNodeInfo = new BamlNodeInfo();
		bamlNodeInfo.Name = _name;
		bamlNodeInfo.LocalName = _localName;
		bamlNodeInfo.AssemblyName = _assemblyName;
		bamlNodeInfo.Prefix = _prefix;
		bamlNodeInfo.ClrNamespace = _clrNamespace;
		bamlNodeInfo.XmlNamespace = _xmlNamespace;
		bamlNodeInfo.RecordType = BamlRecordType.ElementStart;
		_useTypeConverter = bamlElementStartRecord.CreateUsingTypeConverter;
		_isInjected = bamlElementStartRecord.IsInjected;
		if (_deferableContentBlockDepth == _nodeStack.Count)
		{
			int num = (int)(_bamlRecordReader.StreamPosition - _deferableContentPosition);
			num -= bamlElementStartRecord.RecordSize + 1;
			if (BamlRecordHelper.HasDebugExtensionRecord(_parserContext.IsDebugBamlStream, bamlElementStartRecord))
			{
				BamlRecord next = bamlElementStartRecord.Next;
				num -= next.RecordSize + 1;
			}
			InsertDeferedKey(num);
		}
		_nodeStack.Push(bamlNodeInfo);
		ReadProperties();
	}

	private void ReadElementEndRecord()
	{
		if (_deferableContentBlockDepth == _nodeStack.Count)
		{
			_deferableContentBlockDepth = -1;
			_deferableContentPosition = -1L;
		}
		BamlNodeInfo bamlNodeInfo = (BamlNodeInfo)_nodeStack.Pop();
		if (bamlNodeInfo.RecordType != BamlRecordType.ElementStart)
		{
			throw new InvalidOperationException(SR.Format(SR.BamlScopeError, _currentBamlRecord.RecordType.ToString(), BamlRecordType.ElementEnd.ToString()));
		}
		ClearProperties();
		NodeTypeInternal = BamlNodeType.EndElement;
		_name = bamlNodeInfo.Name;
		_localName = bamlNodeInfo.LocalName;
		_ownerTypeName = string.Empty;
		_assemblyName = bamlNodeInfo.AssemblyName;
		_prefix = bamlNodeInfo.Prefix;
		_xmlNamespace = bamlNodeInfo.XmlNamespace;
		_clrNamespace = bamlNodeInfo.ClrNamespace;
		_parserContext.PopScope();
		_prefixDictionary.PopScope();
		ReadProperties();
	}

	private void ReadPropertyComplexStartRecord()
	{
		ClearProperties();
		_parserContext.PushScope();
		_prefixDictionary.PushScope();
		BamlNodeInfo bamlNodeInfo = new BamlNodeInfo();
		SetCommonPropertyInfo(bamlNodeInfo, ((BamlPropertyComplexStartRecord)_currentBamlRecord).AttributeId);
		NodeTypeInternal = BamlNodeType.StartComplexProperty;
		_localName = bamlNodeInfo.LocalName;
		int num = bamlNodeInfo.Name.LastIndexOf('.');
		if (num > 0)
		{
			_ownerTypeName = bamlNodeInfo.Name.Substring(0, num);
		}
		else
		{
			_ownerTypeName = string.Empty;
		}
		_name = bamlNodeInfo.Name;
		_clrNamespace = bamlNodeInfo.ClrNamespace;
		_assemblyName = bamlNodeInfo.AssemblyName;
		_prefix = bamlNodeInfo.Prefix;
		_xmlNamespace = bamlNodeInfo.XmlNamespace;
		bamlNodeInfo.RecordType = _currentBamlRecord.RecordType;
		_nodeStack.Push(bamlNodeInfo);
		ReadProperties();
	}

	private void ReadPropertyComplexEndRecord()
	{
		BamlNodeInfo bamlNodeInfo = (BamlNodeInfo)_nodeStack.Pop();
		BamlRecordType bamlRecordType = bamlNodeInfo.RecordType switch
		{
			BamlRecordType.PropertyComplexStart => BamlRecordType.PropertyComplexEnd, 
			BamlRecordType.PropertyArrayStart => BamlRecordType.PropertyArrayEnd, 
			BamlRecordType.PropertyIListStart => BamlRecordType.PropertyIListEnd, 
			BamlRecordType.PropertyIDictionaryStart => BamlRecordType.PropertyIDictionaryEnd, 
			_ => BamlRecordType.Unknown, 
		};
		if (_currentBamlRecord.RecordType != bamlRecordType)
		{
			throw new InvalidOperationException(SR.Format(SR.BamlScopeError, _currentBamlRecord.RecordType.ToString(), bamlRecordType.ToString()));
		}
		ClearProperties();
		NodeTypeInternal = BamlNodeType.EndComplexProperty;
		_name = bamlNodeInfo.Name;
		_localName = bamlNodeInfo.LocalName;
		int num = bamlNodeInfo.Name.LastIndexOf('.');
		if (num > 0)
		{
			_ownerTypeName = bamlNodeInfo.Name.Substring(0, num);
		}
		else
		{
			_ownerTypeName = string.Empty;
		}
		_assemblyName = bamlNodeInfo.AssemblyName;
		_prefix = bamlNodeInfo.Prefix;
		_xmlNamespace = bamlNodeInfo.XmlNamespace;
		_clrNamespace = bamlNodeInfo.ClrNamespace;
		_parserContext.PopScope();
		_prefixDictionary.PopScope();
		ReadProperties();
	}

	private void ReadTextRecord()
	{
		ClearProperties();
		if (_currentBamlRecord is BamlTextWithIdRecord bamlTextWithIdRecord)
		{
			bamlTextWithIdRecord.Value = MapTable.GetStringFromStringId(bamlTextWithIdRecord.ValueId);
		}
		if (_currentBamlRecord is BamlTextWithConverterRecord { ConverterTypeId: var converterTypeId })
		{
			Type typeFromId = MapTable.GetTypeFromId(converterTypeId);
			_typeConverterAssemblyName = typeFromId.Assembly.FullName;
			_typeConverterName = typeFromId.FullName;
		}
		NodeTypeInternal = BamlNodeType.Text;
		_prefix = string.Empty;
		_value = ((BamlTextRecord)_currentBamlRecord).Value;
	}

	private void ReadConstructorStart()
	{
		ClearProperties();
		NodeTypeInternal = BamlNodeType.StartConstructor;
		BamlNodeInfo bamlNodeInfo = new BamlNodeInfo();
		bamlNodeInfo.RecordType = BamlRecordType.ConstructorParametersStart;
		_nodeStack.Push(bamlNodeInfo);
	}

	private void ReadConstructorEnd()
	{
		ClearProperties();
		NodeTypeInternal = BamlNodeType.EndConstructor;
		if (((BamlNodeInfo)_nodeStack.Pop()).RecordType != BamlRecordType.ConstructorParametersStart)
		{
			throw new InvalidOperationException(SR.Format(SR.BamlScopeError, _currentBamlRecord.RecordType.ToString(), BamlRecordType.ConstructorParametersEnd.ToString()));
		}
		ReadProperties();
	}

	private void InsertDeferedKey(int valueOffset)
	{
		if (_deferKeys == null)
		{
			return;
		}
		BamlKeyInfo bamlKeyInfo = _deferKeys[0];
		while (bamlKeyInfo.Offset == valueOffset)
		{
			_currentKeyInfo = bamlKeyInfo;
			BamlPropertyInfo bamlPropertyInfo = new BamlPropertyInfo();
			bamlPropertyInfo.Value = bamlKeyInfo.Value;
			bamlPropertyInfo.AssemblyName = string.Empty;
			bamlPropertyInfo.Prefix = _prefixDictionary["http://schemas.microsoft.com/winfx/2006/xaml"];
			bamlPropertyInfo.XmlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
			bamlPropertyInfo.ClrNamespace = string.Empty;
			bamlPropertyInfo.Name = bamlKeyInfo.Name;
			bamlPropertyInfo.LocalName = bamlPropertyInfo.Name;
			bamlPropertyInfo.RecordType = BamlRecordType.DefAttribute;
			AddToPropertyInfoCollection(bamlPropertyInfo);
			_deferKeys.RemoveAt(0);
			if (_deferKeys.Count > 0)
			{
				bamlKeyInfo = _deferKeys[0];
				continue;
			}
			break;
		}
	}

	private void ClearProperties()
	{
		_value = string.Empty;
		_prefix = string.Empty;
		_name = string.Empty;
		_localName = string.Empty;
		_ownerTypeName = string.Empty;
		_assemblyName = string.Empty;
		_xmlNamespace = string.Empty;
		_clrNamespace = string.Empty;
		_connectionId = 0;
		_contentPropertyName = string.Empty;
		_attributeUsage = BamlAttributeUsage.Default;
		_typeConverterAssemblyName = string.Empty;
		_typeConverterName = string.Empty;
		_properties.Clear();
	}

	private BamlAttributeInfoRecord SetCommonPropertyInfo(BamlNodeInfo nodeInfo, short attrId)
	{
		BamlAttributeInfoRecord attributeInfoFromId = MapTable.GetAttributeInfoFromId(attrId);
		BamlTypeInfoRecord typeInfoFromId = MapTable.GetTypeInfoFromId(attributeInfoFromId.OwnerTypeId);
		nodeInfo.LocalName = attributeInfoFromId.Name;
		nodeInfo.Name = typeInfoFromId.TypeFullName + "." + nodeInfo.LocalName;
		GetAssemblyAndPrefixAndXmlns(typeInfoFromId, out var assemblyFullName, out var prefix, out var xmlns);
		nodeInfo.AssemblyName = assemblyFullName;
		nodeInfo.Prefix = prefix;
		nodeInfo.XmlNamespace = xmlns;
		nodeInfo.ClrNamespace = typeInfoFromId.ClrNamespace;
		nodeInfo.AttributeUsage = attributeInfoFromId.AttributeUsage;
		return attributeInfoFromId;
	}

	private string GetTemplateBindingExtensionValueString(short memberId)
	{
		string empty = string.Empty;
		string prefix = null;
		string text = null;
		string text2 = null;
		if (memberId < 0)
		{
			memberId = (short)(-memberId);
			DependencyProperty dependencyProperty = null;
			if (memberId < 137)
			{
				dependencyProperty = KnownTypes.GetKnownDependencyPropertyFromId((KnownProperties)memberId);
			}
			if (dependencyProperty == null)
			{
				throw new InvalidOperationException(SR.BamlBadExtensionValue);
			}
			text = dependencyProperty.OwnerType.Name;
			text2 = dependencyProperty.Name;
			object obj = _prefixDictionary["http://schemas.microsoft.com/winfx/2006/xaml/presentation"];
			prefix = ((obj == null) ? string.Empty : ((string)obj));
		}
		else
		{
			BamlAttributeInfoRecord attributeInfoFromId = MapTable.GetAttributeInfoFromId(memberId);
			BamlTypeInfoRecord typeInfoFromId = MapTable.GetTypeInfoFromId(attributeInfoFromId.OwnerTypeId);
			GetAssemblyAndPrefixAndXmlns(typeInfoFromId, out var _, out prefix, out var _);
			text = typeInfoFromId.TypeFullName;
			text = text.Substring(text.LastIndexOf('.') + 1);
			text2 = attributeInfoFromId.Name;
		}
		empty = ((!(prefix == string.Empty)) ? (empty + prefix + ":" + text) : (empty + text));
		return empty + "." + text2 + "}";
	}

	private string GetStaticExtensionValueString(short memberId)
	{
		string empty = string.Empty;
		string prefix = null;
		string text = null;
		string text2 = null;
		string text3 = _prefixDictionary["http://schemas.microsoft.com/winfx/2006/xaml"];
		empty = ((!(text3 != string.Empty)) ? "{Static " : ("{" + text3 + ":Static "));
		if (memberId < 0)
		{
			memberId = (short)(-memberId);
			bool isKey = true;
			memberId = SystemResourceKey.GetSystemResourceKeyIdFromBamlId(memberId, out isKey);
			if (!Enum.IsDefined(typeof(SystemResourceKeyID), (int)memberId))
			{
				throw new InvalidOperationException(SR.BamlBadExtensionValue);
			}
			SystemResourceKeyID id = (SystemResourceKeyID)memberId;
			text = SystemKeyConverter.GetSystemClassName(id);
			text2 = ((!isKey) ? SystemKeyConverter.GetSystemPropertyName(id) : SystemKeyConverter.GetSystemKeyName(id));
			object obj = _prefixDictionary["http://schemas.microsoft.com/winfx/2006/xaml/presentation"];
			prefix = ((obj == null) ? string.Empty : ((string)obj));
		}
		else
		{
			BamlAttributeInfoRecord attributeInfoFromId = MapTable.GetAttributeInfoFromId(memberId);
			BamlTypeInfoRecord typeInfoFromId = MapTable.GetTypeInfoFromId(attributeInfoFromId.OwnerTypeId);
			GetAssemblyAndPrefixAndXmlns(typeInfoFromId, out var _, out prefix, out var _);
			text = typeInfoFromId.TypeFullName;
			text = text.Substring(text.LastIndexOf('.') + 1);
			text2 = attributeInfoFromId.Name;
		}
		empty = ((!(prefix == string.Empty)) ? (empty + prefix + ":" + text) : (empty + text));
		return empty + "." + text2 + "}";
	}

	private string GetExtensionPrefixString(string extensionName)
	{
		string empty = string.Empty;
		string text = _prefixDictionary["http://schemas.microsoft.com/winfx/2006/xaml/presentation"];
		if (!string.IsNullOrEmpty(text))
		{
			return "{" + text + ":" + extensionName + " ";
		}
		return "{" + extensionName + " ";
	}

	private string GetInnerExtensionValueString(IOptimizedMarkupExtension optimizedMarkupExtensionRecord)
	{
		string empty = string.Empty;
		short valueId = optimizedMarkupExtensionRecord.ValueId;
		empty = (optimizedMarkupExtensionRecord.IsValueTypeExtension ? GetTypeValueString(valueId) : ((!optimizedMarkupExtensionRecord.IsValueStaticExtension) ? MapTable.GetStringFromStringId(valueId) : GetStaticExtensionValueString(valueId)));
		return empty + "}";
	}

	private string GetExtensionValueString(IOptimizedMarkupExtension optimizedMarkupExtensionRecord)
	{
		string result = string.Empty;
		short valueId = optimizedMarkupExtensionRecord.ValueId;
		switch (optimizedMarkupExtensionRecord.ExtensionTypeId)
		{
		case 602:
			result = GetStaticExtensionValueString(valueId);
			break;
		case 634:
			result = GetExtensionPrefixString("TemplateBinding");
			result += GetTemplateBindingExtensionValueString(valueId);
			break;
		case 189:
			result = GetExtensionPrefixString("DynamicResource");
			result += GetInnerExtensionValueString(optimizedMarkupExtensionRecord);
			break;
		case 603:
			result = GetExtensionPrefixString("StaticResource");
			result += GetInnerExtensionValueString(optimizedMarkupExtensionRecord);
			break;
		}
		return result;
	}

	private string GetTypeValueString(short typeId)
	{
		string text = _prefixDictionary["http://schemas.microsoft.com/winfx/2006/xaml"];
		string text2 = ((!(text != string.Empty)) ? "{Type " : ("{" + text + ":Type "));
		BamlTypeInfoRecord typeInfoFromId = MapTable.GetTypeInfoFromId(typeId);
		GetAssemblyAndPrefixAndXmlns(typeInfoFromId, out var _, out var prefix, out var _);
		string typeFullName = typeInfoFromId.TypeFullName;
		typeFullName = typeFullName.Substring(typeFullName.LastIndexOf('.') + 1);
		text2 = ((!(prefix == string.Empty)) ? (text2 + prefix + ":" + typeFullName) : (text2 + typeFullName));
		return text2 + "}";
	}

	private void GetAssemblyAndPrefixAndXmlns(BamlTypeInfoRecord typeInfo, out string assemblyFullName, out string prefix, out string xmlns)
	{
		if (typeInfo.AssemblyId >= 0 || typeInfo.Type == null)
		{
			BamlAssemblyInfoRecord assemblyInfoFromId = MapTable.GetAssemblyInfoFromId(typeInfo.AssemblyId);
			assemblyFullName = assemblyInfoFromId.AssemblyFullName;
		}
		else
		{
			Assembly assembly = typeInfo.Type.Assembly;
			assemblyFullName = assembly.FullName;
		}
		if (typeInfo.ClrNamespace == "System.Windows.Markup" && (assemblyFullName.StartsWith("PresentationFramework", StringComparison.Ordinal) || assemblyFullName.StartsWith("System.Xaml", StringComparison.Ordinal)))
		{
			xmlns = "http://schemas.microsoft.com/winfx/2006/xaml";
		}
		else
		{
			xmlns = _parserContext.XamlTypeMapper.GetXmlNamespace(typeInfo.ClrNamespace, assemblyFullName);
			if (string.IsNullOrEmpty(xmlns))
			{
				List<string> xmlNamespaceList = GetXmlNamespaceList(typeInfo.ClrNamespace, assemblyFullName);
				prefix = GetXmlnsPrefix(xmlNamespaceList);
				return;
			}
		}
		prefix = GetXmlnsPrefix(xmlns);
	}

	private void SetXmlNamespace(string clrNamespace, string assemblyFullName, string xmlNs)
	{
		string key = clrNamespace + "#" + assemblyFullName;
		List<string> list;
		if (_reverseXmlnsTable.ContainsKey(key))
		{
			list = _reverseXmlnsTable[key];
		}
		else
		{
			list = new List<string>();
			_reverseXmlnsTable[key] = list;
		}
		list.Add(xmlNs);
	}

	private List<string> GetXmlNamespaceList(string clrNamespace, string assemblyFullName)
	{
		string key = clrNamespace + "#" + assemblyFullName;
		List<string> result = null;
		if (_reverseXmlnsTable.ContainsKey(key))
		{
			result = _reverseXmlnsTable[key];
		}
		return result;
	}

	internal string GetXmlnsPrefix(string xmlns)
	{
		string result = string.Empty;
		if (xmlns == string.Empty)
		{
			xmlns = _parserContext.XmlnsDictionary[string.Empty];
		}
		else
		{
			object obj = _prefixDictionary[xmlns];
			if (obj != null)
			{
				result = (string)obj;
			}
		}
		return result;
	}

	private string GetXmlnsPrefix(List<string> xmlnsList)
	{
		if (xmlnsList != null)
		{
			for (int i = 0; i < xmlnsList.Count; i++)
			{
				string prefix = xmlnsList[i];
				string text = _prefixDictionary[prefix];
				if (text != null)
				{
					return text;
				}
			}
		}
		return string.Empty;
	}
}
