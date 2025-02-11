using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace System.Windows.Markup;

internal class BamlRecordWriter
{
	private class DeferRecord
	{
		private int _lineNumber;

		private int _linePosition;

		internal int LineNumber
		{
			get
			{
				return _lineNumber;
			}
			set
			{
				_lineNumber = value;
			}
		}

		internal int LinePosition
		{
			get
			{
				return _linePosition;
			}
			set
			{
				_linePosition = value;
			}
		}

		internal DeferRecord(int lineNumber, int linePosition)
		{
			_lineNumber = lineNumber;
			_linePosition = linePosition;
		}
	}

	private class ValueDeferRecord : DeferRecord
	{
		private bool _updateOffset;

		private BamlRecord _record;

		internal BamlRecord Record
		{
			get
			{
				return _record;
			}
			set
			{
				_record = value;
			}
		}

		internal bool UpdateOffset
		{
			get
			{
				return _updateOffset;
			}
			set
			{
				_updateOffset = value;
			}
		}

		internal ValueDeferRecord(BamlRecord record, int lineNumber, int linePosition)
			: base(lineNumber, linePosition)
		{
			_record = record;
			_updateOffset = false;
		}
	}

	private class KeyDeferRecord : DeferRecord
	{
		private BamlRecord _record;

		private ArrayList _recordList;

		private List<List<ValueDeferRecord>> _staticResourceRecordList;

		internal BamlRecord Record
		{
			get
			{
				return _record;
			}
			set
			{
				_record = value;
			}
		}

		internal ArrayList RecordList
		{
			get
			{
				return _recordList;
			}
			set
			{
				_recordList = value;
			}
		}

		internal List<List<ValueDeferRecord>> StaticResourceRecordList
		{
			get
			{
				if (_staticResourceRecordList == null)
				{
					_staticResourceRecordList = new List<List<ValueDeferRecord>>(1);
				}
				return _staticResourceRecordList;
			}
		}

		internal KeyDeferRecord(int lineNumber, int linePosition)
			: base(lineNumber, linePosition)
		{
		}
	}

	private XamlTypeMapper _xamlTypeMapper;

	private Stream _bamlStream;

	private BamlBinaryWriter _bamlBinaryWriter;

	private BamlDocumentStartRecord _startDocumentRecord;

	private ParserContext _parserContext;

	private BamlMapTable _bamlMapTable;

	private BamlRecordManager _bamlRecordManager;

	private ITypeDescriptorContext _typeConvertContext;

	private bool _deferLoadingSupport;

	private int _deferElementDepth;

	private bool _deferEndOfStartReached;

	private int _deferComplexPropertyDepth;

	private bool _deferKeyCollecting;

	private ArrayList _deferKeys;

	private ArrayList _deferValues;

	private ArrayList _deferElement;

	private short _staticResourceElementDepth;

	private short _dynamicResourceElementDepth;

	private List<ValueDeferRecord> _staticResourceRecordList;

	private bool _debugBamlStream;

	private int _lineNumber;

	private int _linePosition;

	private BamlLineAndPositionRecord _bamlLineAndPositionRecord;

	private BamlLinePositionRecord _bamlLinePositionRecord;

	internal virtual bool UpdateParentNodes => true;

	public bool DebugBamlStream => _debugBamlStream;

	internal BamlLineAndPositionRecord LineAndPositionRecord
	{
		get
		{
			if (_bamlLineAndPositionRecord == null)
			{
				_bamlLineAndPositionRecord = new BamlLineAndPositionRecord();
			}
			return _bamlLineAndPositionRecord;
		}
	}

	internal BamlLinePositionRecord LinePositionRecord
	{
		get
		{
			if (_bamlLinePositionRecord == null)
			{
				_bamlLinePositionRecord = new BamlLinePositionRecord();
			}
			return _bamlLinePositionRecord;
		}
	}

	public Stream BamlStream => _bamlStream;

	internal BamlBinaryWriter BinaryWriter => _bamlBinaryWriter;

	internal BamlMapTable MapTable => _bamlMapTable;

	internal ParserContext ParserContext => _parserContext;

	internal virtual BamlRecordManager BamlRecordManager => _bamlRecordManager;

	private BamlDocumentStartRecord DocumentStartRecord
	{
		get
		{
			return _startDocumentRecord;
		}
		set
		{
			_startDocumentRecord = value;
		}
	}

	private bool CollectingValues
	{
		get
		{
			if (_deferEndOfStartReached && !_deferKeyCollecting)
			{
				return _deferComplexPropertyDepth <= 0;
			}
			return false;
		}
	}

	private ITypeDescriptorContext TypeConvertContext
	{
		get
		{
			if (_typeConvertContext == null)
			{
				_typeConvertContext = new TypeConvertContext(_parserContext);
			}
			return _typeConvertContext;
		}
	}

	private bool InStaticResourceSection => _staticResourceElementDepth > 0;

	private bool InDynamicResourceSection => _dynamicResourceElementDepth > 0;

	public BamlRecordWriter(Stream stream, ParserContext parserContext, bool deferLoadingSupport)
	{
		_bamlStream = stream;
		_xamlTypeMapper = parserContext.XamlTypeMapper;
		_deferLoadingSupport = deferLoadingSupport;
		_bamlMapTable = parserContext.MapTable;
		_parserContext = parserContext;
		_debugBamlStream = false;
		_lineNumber = -1;
		_linePosition = -1;
		_bamlBinaryWriter = new BamlBinaryWriter(stream, new UTF8Encoding());
		_bamlRecordManager = new BamlRecordManager();
	}

	internal virtual void WriteBamlRecord(BamlRecord bamlRecord, int lineNumber, int linePosition)
	{
		try
		{
			bamlRecord.Write(BinaryWriter);
			if (DebugBamlStream && BamlRecordHelper.DoesRecordTypeHaveDebugExtension(bamlRecord.RecordType))
			{
				WriteDebugExtensionRecord(lineNumber, linePosition);
			}
		}
		catch (XamlParseException ex)
		{
			_xamlTypeMapper.ThrowExceptionWithLine(ex.Message, ex.InnerException);
		}
	}

	internal void SetParseMode(XamlParseMode xamlParseMode)
	{
		if (UpdateParentNodes && xamlParseMode == XamlParseMode.Asynchronous && DocumentStartRecord != null)
		{
			DocumentStartRecord.LoadAsync = true;
			DocumentStartRecord.UpdateWrite(BinaryWriter);
		}
	}

	internal virtual void SetMaxAsyncRecords(int maxAsyncRecords)
	{
		if (UpdateParentNodes && DocumentStartRecord != null)
		{
			DocumentStartRecord.MaxAsyncRecords = maxAsyncRecords;
			DocumentStartRecord.UpdateWrite(BinaryWriter);
		}
	}

	internal void WriteDebugExtensionRecord(int lineNumber, int linePosition)
	{
		if (lineNumber != _lineNumber)
		{
			BamlLineAndPositionRecord lineAndPositionRecord = LineAndPositionRecord;
			_lineNumber = lineNumber;
			lineAndPositionRecord.LineNumber = (uint)lineNumber;
			_linePosition = linePosition;
			lineAndPositionRecord.LinePosition = (uint)linePosition;
			lineAndPositionRecord.Write(BinaryWriter);
		}
		else if (linePosition != _linePosition)
		{
			_linePosition = linePosition;
			BamlLinePositionRecord linePositionRecord = LinePositionRecord;
			linePositionRecord.LinePosition = (uint)linePosition;
			linePositionRecord.Write(BinaryWriter);
		}
	}

	internal void WriteDocumentStart(XamlDocumentStartNode xamlDocumentNode)
	{
		new BamlVersionHeader().WriteVersion(BinaryWriter);
		DocumentStartRecord = (BamlDocumentStartRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.DocumentStart);
		DocumentStartRecord.DebugBaml = DebugBamlStream;
		WriteBamlRecord(DocumentStartRecord, xamlDocumentNode.LineNumber, xamlDocumentNode.LinePosition);
		BamlRecordManager.ReleaseWriteRecord(DocumentStartRecord);
	}

	internal void WriteDocumentEnd(XamlDocumentEndNode xamlDocumentEndNode)
	{
		BamlDocumentEndRecord bamlDocumentEndRecord = (BamlDocumentEndRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.DocumentEnd);
		WriteBamlRecord(bamlDocumentEndRecord, xamlDocumentEndNode.LineNumber, xamlDocumentEndNode.LinePosition);
		BamlRecordManager.ReleaseWriteRecord(bamlDocumentEndRecord);
	}

	internal void WriteConnectionId(int connectionId)
	{
		BamlConnectionIdRecord bamlConnectionIdRecord = (BamlConnectionIdRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.ConnectionId);
		bamlConnectionIdRecord.ConnectionId = connectionId;
		WriteAndReleaseRecord(bamlConnectionIdRecord, null);
	}

	internal void WriteElementStart(XamlElementStartNode xamlElementNode)
	{
		BamlElementStartRecord bamlElementStartRecord = (BamlElementStartRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.ElementStart);
		if (!MapTable.GetTypeInfoId(BinaryWriter, xamlElementNode.AssemblyName, xamlElementNode.TypeFullName, out var typeId))
		{
			string serializerAssemblyFullName = string.Empty;
			if (xamlElementNode.SerializerType != null)
			{
				serializerAssemblyFullName = xamlElementNode.SerializerType.Assembly.FullName;
			}
			typeId = MapTable.AddTypeInfoMap(BinaryWriter, xamlElementNode.AssemblyName, xamlElementNode.TypeFullName, xamlElementNode.ElementType, serializerAssemblyFullName, xamlElementNode.SerializerTypeFullName);
		}
		bamlElementStartRecord.TypeId = typeId;
		bamlElementStartRecord.CreateUsingTypeConverter = xamlElementNode.CreateUsingTypeConverter;
		bamlElementStartRecord.IsInjected = xamlElementNode.IsInjected;
		if (_deferLoadingSupport && _deferElementDepth > 0)
		{
			_deferElementDepth++;
			if (InStaticResourceSection)
			{
				_staticResourceElementDepth++;
				_staticResourceRecordList.Add(new ValueDeferRecord(bamlElementStartRecord, xamlElementNode.LineNumber, xamlElementNode.LinePosition));
				return;
			}
			if (CollectingValues && KnownTypes.Types[603] == xamlElementNode.ElementType)
			{
				_staticResourceElementDepth = 1;
				_staticResourceRecordList = new List<ValueDeferRecord>(5);
				_staticResourceRecordList.Add(new ValueDeferRecord(bamlElementStartRecord, xamlElementNode.LineNumber, xamlElementNode.LinePosition));
				return;
			}
			if (InDynamicResourceSection)
			{
				_dynamicResourceElementDepth++;
			}
			else if (CollectingValues && KnownTypes.Types[189] == xamlElementNode.ElementType)
			{
				_dynamicResourceElementDepth = 1;
			}
			ValueDeferRecord valueDeferRecord = new ValueDeferRecord(bamlElementStartRecord, xamlElementNode.LineNumber, xamlElementNode.LinePosition);
			if (_deferComplexPropertyDepth > 0)
			{
				_deferElement.Add(valueDeferRecord);
			}
			else if (_deferElementDepth == 2)
			{
				_deferKeys.Add(new KeyDeferRecord(xamlElementNode.LineNumber, xamlElementNode.LinePosition));
				valueDeferRecord.UpdateOffset = true;
				_deferValues.Add(valueDeferRecord);
			}
			else if (_deferKeyCollecting)
			{
				if (typeof(string).IsAssignableFrom(xamlElementNode.ElementType) || KnownTypes.Types[602].IsAssignableFrom(xamlElementNode.ElementType) || KnownTypes.Types[691].IsAssignableFrom(xamlElementNode.ElementType))
				{
					((KeyDeferRecord)_deferKeys[_deferKeys.Count - 1]).RecordList.Add(valueDeferRecord);
				}
				else
				{
					XamlParser.ThrowException("ParserBadKey", xamlElementNode.TypeFullName, xamlElementNode.LineNumber, xamlElementNode.LinePosition);
				}
			}
			else
			{
				_deferValues.Add(valueDeferRecord);
			}
		}
		else if (_deferLoadingSupport && KnownTypes.Types[524].IsAssignableFrom(xamlElementNode.ElementType))
		{
			_deferElementDepth = 1;
			_deferEndOfStartReached = false;
			_deferElement = new ArrayList(2);
			_deferKeys = new ArrayList(10);
			_deferValues = new ArrayList(100);
			_deferElement.Add(new ValueDeferRecord(bamlElementStartRecord, xamlElementNode.LineNumber, xamlElementNode.LinePosition));
		}
		else
		{
			WriteBamlRecord(bamlElementStartRecord, xamlElementNode.LineNumber, xamlElementNode.LinePosition);
			BamlRecordManager.ReleaseWriteRecord(bamlElementStartRecord);
		}
	}

	internal void WriteElementEnd(XamlElementEndNode xamlElementEndNode)
	{
		BamlElementEndRecord bamlRecord = (BamlElementEndRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.ElementEnd);
		if (_deferLoadingSupport && _deferElementDepth > 0 && _deferElementDepth-- == 1)
		{
			WriteDeferableContent(xamlElementEndNode);
			_deferKeys = null;
			_deferValues = null;
			_deferElement = null;
			return;
		}
		WriteAndReleaseRecord(bamlRecord, xamlElementEndNode);
		if (_deferLoadingSupport && _staticResourceElementDepth > 0 && _staticResourceElementDepth-- == 1)
		{
			WriteStaticResource();
			_staticResourceRecordList = null;
		}
		else if (_deferLoadingSupport && _dynamicResourceElementDepth > 0)
		{
			_dynamicResourceElementDepth--;
			_ = 1;
		}
	}

	internal void WriteEndAttributes(XamlEndAttributesNode xamlEndAttributesNode)
	{
		if (_deferLoadingSupport && _deferElementDepth > 0)
		{
			_deferEndOfStartReached = true;
		}
	}

	internal void WriteLiteralContent(XamlLiteralContentNode xamlLiteralContentNode)
	{
		BamlLiteralContentRecord bamlLiteralContentRecord = (BamlLiteralContentRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.LiteralContent);
		bamlLiteralContentRecord.Value = xamlLiteralContentNode.Content;
		WriteAndReleaseRecord(bamlLiteralContentRecord, xamlLiteralContentNode);
	}

	internal void WriteDefAttributeKeyType(XamlDefAttributeKeyTypeNode xamlDefNode)
	{
		if (!MapTable.GetTypeInfoId(BinaryWriter, xamlDefNode.AssemblyName, xamlDefNode.Value, out var typeId))
		{
			typeId = MapTable.AddTypeInfoMap(BinaryWriter, xamlDefNode.AssemblyName, xamlDefNode.Value, xamlDefNode.ValueType, string.Empty, string.Empty);
		}
		BamlDefAttributeKeyTypeRecord bamlDefAttributeKeyTypeRecord = BamlRecordManager.GetWriteRecord(BamlRecordType.DefAttributeKeyType) as BamlDefAttributeKeyTypeRecord;
		bamlDefAttributeKeyTypeRecord.TypeId = typeId;
		((IBamlDictionaryKey)bamlDefAttributeKeyTypeRecord).KeyObject = xamlDefNode.ValueType;
		if (_deferLoadingSupport && _deferElementDepth == 2)
		{
			KeyDeferRecord keyDeferRecord = (KeyDeferRecord)_deferKeys[_deferKeys.Count - 1];
			TransferOldSharedData(keyDeferRecord.Record as IBamlDictionaryKey, bamlDefAttributeKeyTypeRecord);
			keyDeferRecord.Record = bamlDefAttributeKeyTypeRecord;
			keyDeferRecord.LineNumber = xamlDefNode.LineNumber;
			keyDeferRecord.LinePosition = xamlDefNode.LinePosition;
		}
		else if (_deferLoadingSupport && _deferElementDepth > 0)
		{
			_deferValues.Add(new ValueDeferRecord(bamlDefAttributeKeyTypeRecord, xamlDefNode.LineNumber, xamlDefNode.LinePosition));
		}
		else
		{
			WriteBamlRecord(bamlDefAttributeKeyTypeRecord, xamlDefNode.LineNumber, xamlDefNode.LinePosition);
			BamlRecordManager.ReleaseWriteRecord(bamlDefAttributeKeyTypeRecord);
		}
	}

	private void TransferOldSharedData(IBamlDictionaryKey oldRecord, IBamlDictionaryKey newRecord)
	{
		if (oldRecord != null && newRecord != null)
		{
			newRecord.Shared = oldRecord.Shared;
			newRecord.SharedSet = oldRecord.SharedSet;
		}
	}

	private IBamlDictionaryKey FindBamlDictionaryKey(KeyDeferRecord record)
	{
		if (record != null)
		{
			if (record.RecordList != null)
			{
				for (int i = 0; i < record.RecordList.Count; i++)
				{
					if (((ValueDeferRecord)record.RecordList[i]).Record is IBamlDictionaryKey result)
					{
						return result;
					}
				}
			}
			return record.Record as IBamlDictionaryKey;
		}
		return null;
	}

	internal void WriteDefAttribute(XamlDefAttributeNode xamlDefNode)
	{
		if (_deferLoadingSupport && _deferElementDepth == 2 && xamlDefNode.Name == "Key")
		{
			KeyDeferRecord keyDeferRecord = (KeyDeferRecord)_deferKeys[_deferKeys.Count - 1];
			BamlDefAttributeKeyStringRecord bamlDefAttributeKeyStringRecord = keyDeferRecord.Record as BamlDefAttributeKeyStringRecord;
			if (bamlDefAttributeKeyStringRecord == null)
			{
				bamlDefAttributeKeyStringRecord = (BamlDefAttributeKeyStringRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.DefAttributeKeyString);
				TransferOldSharedData(keyDeferRecord.Record as IBamlDictionaryKey, bamlDefAttributeKeyStringRecord);
				keyDeferRecord.Record = bamlDefAttributeKeyStringRecord;
			}
			if (!MapTable.GetStringInfoId(xamlDefNode.Value, out var stringId))
			{
				stringId = MapTable.AddStringInfoMap(BinaryWriter, xamlDefNode.Value);
			}
			bamlDefAttributeKeyStringRecord.Value = xamlDefNode.Value;
			bamlDefAttributeKeyStringRecord.ValueId = stringId;
			keyDeferRecord.LineNumber = xamlDefNode.LineNumber;
			keyDeferRecord.LinePosition = xamlDefNode.LinePosition;
		}
		else if (_deferLoadingSupport && _deferElementDepth == 2 && xamlDefNode.Name == "Shared")
		{
			KeyDeferRecord keyDeferRecord2 = (KeyDeferRecord)_deferKeys[_deferKeys.Count - 1];
			IBamlDictionaryKey bamlDictionaryKey = FindBamlDictionaryKey(keyDeferRecord2);
			if (bamlDictionaryKey == null)
			{
				BamlDefAttributeKeyStringRecord bamlDefAttributeKeyStringRecord2 = (BamlDefAttributeKeyStringRecord)(keyDeferRecord2.Record = (BamlDefAttributeKeyStringRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.DefAttributeKeyString));
				bamlDictionaryKey = bamlDefAttributeKeyStringRecord2;
			}
			bamlDictionaryKey.Shared = bool.Parse(xamlDefNode.Value);
			bamlDictionaryKey.SharedSet = true;
			keyDeferRecord2.LineNumber = xamlDefNode.LineNumber;
			keyDeferRecord2.LinePosition = xamlDefNode.LinePosition;
		}
		else
		{
			if (!MapTable.GetStringInfoId(xamlDefNode.Name, out var stringId2))
			{
				stringId2 = MapTable.AddStringInfoMap(BinaryWriter, xamlDefNode.Name);
			}
			BamlDefAttributeRecord bamlDefAttributeRecord = (BamlDefAttributeRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.DefAttribute);
			bamlDefAttributeRecord.Value = xamlDefNode.Value;
			bamlDefAttributeRecord.Name = xamlDefNode.Name;
			bamlDefAttributeRecord.AttributeUsage = xamlDefNode.AttributeUsage;
			bamlDefAttributeRecord.NameId = stringId2;
			WriteAndReleaseRecord(bamlDefAttributeRecord, xamlDefNode);
		}
	}

	internal void WritePresentationOptionsAttribute(XamlPresentationOptionsAttributeNode xamlPresentationOptionsNode)
	{
		if (!MapTable.GetStringInfoId(xamlPresentationOptionsNode.Name, out var stringId))
		{
			stringId = MapTable.AddStringInfoMap(BinaryWriter, xamlPresentationOptionsNode.Name);
		}
		BamlPresentationOptionsAttributeRecord bamlPresentationOptionsAttributeRecord = (BamlPresentationOptionsAttributeRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.PresentationOptionsAttribute);
		bamlPresentationOptionsAttributeRecord.Value = xamlPresentationOptionsNode.Value;
		bamlPresentationOptionsAttributeRecord.Name = xamlPresentationOptionsNode.Name;
		bamlPresentationOptionsAttributeRecord.NameId = stringId;
		WriteAndReleaseRecord(bamlPresentationOptionsAttributeRecord, xamlPresentationOptionsNode);
	}

	internal void WriteNamespacePrefix(XamlXmlnsPropertyNode xamlXmlnsPropertyNode)
	{
		BamlXmlnsPropertyRecord bamlXmlnsPropertyRecord = (BamlXmlnsPropertyRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.XmlnsProperty);
		bamlXmlnsPropertyRecord.Prefix = xamlXmlnsPropertyNode.Prefix;
		bamlXmlnsPropertyRecord.XmlNamespace = xamlXmlnsPropertyNode.XmlNamespace;
		WriteAndReleaseRecord(bamlXmlnsPropertyRecord, xamlXmlnsPropertyNode);
	}

	internal void WritePIMapping(XamlPIMappingNode xamlPIMappingNode)
	{
		BamlPIMappingRecord bamlPIMappingRecord = (BamlPIMappingRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.PIMapping);
		BamlAssemblyInfoRecord bamlAssemblyInfoRecord = MapTable.AddAssemblyMap(BinaryWriter, xamlPIMappingNode.AssemblyName);
		bamlPIMappingRecord.XmlNamespace = xamlPIMappingNode.XmlNamespace;
		bamlPIMappingRecord.ClrNamespace = xamlPIMappingNode.ClrNamespace;
		bamlPIMappingRecord.AssemblyId = bamlAssemblyInfoRecord.AssemblyId;
		WriteBamlRecord(bamlPIMappingRecord, xamlPIMappingNode.LineNumber, xamlPIMappingNode.LinePosition);
		BamlRecordManager.ReleaseWriteRecord(bamlPIMappingRecord);
	}

	internal void WritePropertyComplexStart(XamlPropertyComplexStartNode xamlComplexPropertyNode)
	{
		BamlPropertyComplexStartRecord bamlPropertyComplexStartRecord = (BamlPropertyComplexStartRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.PropertyComplexStart);
		bamlPropertyComplexStartRecord.AttributeId = MapTable.AddAttributeInfoMap(BinaryWriter, xamlComplexPropertyNode.AssemblyName, xamlComplexPropertyNode.TypeFullName, xamlComplexPropertyNode.PropDeclaringType, xamlComplexPropertyNode.PropName, xamlComplexPropertyNode.PropValidType, BamlAttributeUsage.Default);
		WriteAndReleaseRecord(bamlPropertyComplexStartRecord, xamlComplexPropertyNode);
	}

	internal void WritePropertyComplexEnd(XamlPropertyComplexEndNode xamlPropertyComplexEnd)
	{
		BamlPropertyComplexEndRecord bamlRecord = (BamlPropertyComplexEndRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.PropertyComplexEnd);
		WriteAndReleaseRecord(bamlRecord, xamlPropertyComplexEnd);
	}

	public void WriteKeyElementStart(XamlElementStartNode xamlKeyElementNode)
	{
		if (!typeof(string).IsAssignableFrom(xamlKeyElementNode.ElementType) && !KnownTypes.Types[602].IsAssignableFrom(xamlKeyElementNode.ElementType) && !KnownTypes.Types[691].IsAssignableFrom(xamlKeyElementNode.ElementType) && !KnownTypes.Types[525].IsAssignableFrom(xamlKeyElementNode.ElementType))
		{
			XamlParser.ThrowException("ParserBadKey", xamlKeyElementNode.TypeFullName, xamlKeyElementNode.LineNumber, xamlKeyElementNode.LinePosition);
		}
		BamlKeyElementStartRecord bamlKeyElementStartRecord = (BamlKeyElementStartRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.KeyElementStart);
		if (!MapTable.GetTypeInfoId(BinaryWriter, xamlKeyElementNode.AssemblyName, xamlKeyElementNode.TypeFullName, out var typeId))
		{
			string serializerAssemblyFullName = string.Empty;
			if (xamlKeyElementNode.SerializerType != null)
			{
				serializerAssemblyFullName = xamlKeyElementNode.SerializerType.Assembly.FullName;
			}
			typeId = MapTable.AddTypeInfoMap(BinaryWriter, xamlKeyElementNode.AssemblyName, xamlKeyElementNode.TypeFullName, xamlKeyElementNode.ElementType, serializerAssemblyFullName, xamlKeyElementNode.SerializerTypeFullName);
		}
		bamlKeyElementStartRecord.TypeId = typeId;
		if (_deferLoadingSupport && _deferElementDepth == 2)
		{
			_deferElementDepth++;
			_deferKeyCollecting = true;
			KeyDeferRecord keyDeferRecord = (KeyDeferRecord)_deferKeys[_deferKeys.Count - 1];
			keyDeferRecord.RecordList = new ArrayList(5);
			keyDeferRecord.RecordList.Add(new ValueDeferRecord(bamlKeyElementStartRecord, xamlKeyElementNode.LineNumber, xamlKeyElementNode.LinePosition));
			if (keyDeferRecord.Record != null)
			{
				TransferOldSharedData(keyDeferRecord.Record as IBamlDictionaryKey, bamlKeyElementStartRecord);
				keyDeferRecord.Record = null;
			}
			keyDeferRecord.LineNumber = xamlKeyElementNode.LineNumber;
			keyDeferRecord.LinePosition = xamlKeyElementNode.LinePosition;
		}
		else
		{
			WriteAndReleaseRecord(bamlKeyElementStartRecord, xamlKeyElementNode);
		}
	}

	internal void WriteKeyElementEnd(XamlElementEndNode xamlKeyElementEnd)
	{
		BamlKeyElementEndRecord bamlRecord = (BamlKeyElementEndRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.KeyElementEnd);
		WriteAndReleaseRecord(bamlRecord, xamlKeyElementEnd);
		if (_deferLoadingSupport && _deferKeyCollecting)
		{
			_deferKeyCollecting = false;
			_deferElementDepth--;
		}
	}

	internal void WriteConstructorParametersStart(XamlConstructorParametersStartNode xamlConstructorParametersStartNode)
	{
		BamlConstructorParametersStartRecord bamlRecord = (BamlConstructorParametersStartRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.ConstructorParametersStart);
		WriteAndReleaseRecord(bamlRecord, xamlConstructorParametersStartNode);
	}

	internal void WriteConstructorParametersEnd(XamlConstructorParametersEndNode xamlConstructorParametersEndNode)
	{
		BamlConstructorParametersEndRecord bamlRecord = (BamlConstructorParametersEndRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.ConstructorParametersEnd);
		WriteAndReleaseRecord(bamlRecord, xamlConstructorParametersEndNode);
	}

	internal virtual void WriteContentProperty(XamlContentPropertyNode xamlContentPropertyNode)
	{
		BamlContentPropertyRecord bamlContentPropertyRecord = (BamlContentPropertyRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.ContentProperty);
		bamlContentPropertyRecord.AttributeId = MapTable.AddAttributeInfoMap(BinaryWriter, xamlContentPropertyNode.AssemblyName, xamlContentPropertyNode.TypeFullName, xamlContentPropertyNode.PropDeclaringType, xamlContentPropertyNode.PropName, xamlContentPropertyNode.PropValidType, BamlAttributeUsage.Default);
		WriteAndReleaseRecord(bamlContentPropertyRecord, xamlContentPropertyNode);
	}

	internal virtual void WriteProperty(XamlPropertyNode xamlProperty)
	{
		short attributeId = MapTable.AddAttributeInfoMap(BinaryWriter, xamlProperty.AssemblyName, xamlProperty.TypeFullName, xamlProperty.PropDeclaringType, xamlProperty.PropName, xamlProperty.PropValidType, xamlProperty.AttributeUsage);
		if (xamlProperty.AssemblyName != string.Empty && xamlProperty.TypeFullName != string.Empty)
		{
			short converterOrSerializerTypeId;
			Type converterOrSerializerType;
			bool customSerializerOrConverter = MapTable.GetCustomSerializerOrConverter(BinaryWriter, xamlProperty.ValueDeclaringType, xamlProperty.ValuePropertyType, xamlProperty.ValuePropertyMember, xamlProperty.ValuePropertyName, out converterOrSerializerTypeId, out converterOrSerializerType);
			if (converterOrSerializerType != null)
			{
				if (customSerializerOrConverter)
				{
					BamlPropertyCustomWriteInfoRecord bamlPropertyCustomWriteInfoRecord = (BamlPropertyCustomWriteInfoRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.PropertyCustom);
					bamlPropertyCustomWriteInfoRecord.AttributeId = attributeId;
					bamlPropertyCustomWriteInfoRecord.Value = xamlProperty.Value;
					bamlPropertyCustomWriteInfoRecord.ValueType = xamlProperty.ValuePropertyType;
					bamlPropertyCustomWriteInfoRecord.SerializerTypeId = converterOrSerializerTypeId;
					bamlPropertyCustomWriteInfoRecord.SerializerType = converterOrSerializerType;
					bamlPropertyCustomWriteInfoRecord.TypeContext = TypeConvertContext;
					if (converterOrSerializerTypeId == 137)
					{
						if (xamlProperty.HasValueId)
						{
							bamlPropertyCustomWriteInfoRecord.ValueId = xamlProperty.ValueId;
							bamlPropertyCustomWriteInfoRecord.ValueMemberName = xamlProperty.MemberName;
						}
						else
						{
							string memberName;
							Type dependencyPropertyOwnerAndName = _xamlTypeMapper.GetDependencyPropertyOwnerAndName(xamlProperty.Value, ParserContext, xamlProperty.DefaultTargetType, out memberName);
							short typeId;
							short attributeOrTypeId = MapTable.GetAttributeOrTypeId(BinaryWriter, dependencyPropertyOwnerAndName, memberName, out typeId);
							if (attributeOrTypeId < 0)
							{
								bamlPropertyCustomWriteInfoRecord.ValueId = attributeOrTypeId;
								bamlPropertyCustomWriteInfoRecord.ValueMemberName = null;
							}
							else
							{
								bamlPropertyCustomWriteInfoRecord.ValueId = typeId;
								bamlPropertyCustomWriteInfoRecord.ValueMemberName = memberName;
							}
						}
					}
					WriteAndReleaseRecord(bamlPropertyCustomWriteInfoRecord, xamlProperty);
				}
				else
				{
					BamlPropertyWithConverterRecord bamlPropertyWithConverterRecord = (BamlPropertyWithConverterRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.PropertyWithConverter);
					bamlPropertyWithConverterRecord.AttributeId = attributeId;
					bamlPropertyWithConverterRecord.Value = xamlProperty.Value;
					bamlPropertyWithConverterRecord.ConverterTypeId = converterOrSerializerTypeId;
					WriteAndReleaseRecord(bamlPropertyWithConverterRecord, xamlProperty);
				}
				return;
			}
		}
		BaseWriteProperty(xamlProperty);
	}

	internal virtual void WritePropertyWithExtension(XamlPropertyWithExtensionNode xamlPropertyNode)
	{
		short stringId = 0;
		short extensionTypeId = xamlPropertyNode.ExtensionTypeId;
		bool isValueTypeExtension = false;
		bool isValueStaticExtension = false;
		if (extensionTypeId == 189 || extensionTypeId == 603)
		{
			if (xamlPropertyNode.IsValueNestedExtension)
			{
				if (xamlPropertyNode.IsValueTypeExtension)
				{
					Type typeFromBaseString = _xamlTypeMapper.GetTypeFromBaseString(xamlPropertyNode.Value, ParserContext, throwOnError: true);
					if (!MapTable.GetTypeInfoId(BinaryWriter, typeFromBaseString.Assembly.FullName, typeFromBaseString.FullName, out stringId))
					{
						stringId = MapTable.AddTypeInfoMap(BinaryWriter, typeFromBaseString.Assembly.FullName, typeFromBaseString.FullName, typeFromBaseString, string.Empty, string.Empty);
					}
					isValueTypeExtension = true;
				}
				else
				{
					stringId = MapTable.GetStaticMemberId(BinaryWriter, ParserContext, 602, xamlPropertyNode.Value, xamlPropertyNode.DefaultTargetType);
					isValueStaticExtension = true;
				}
			}
			else if (!MapTable.GetStringInfoId(xamlPropertyNode.Value, out stringId))
			{
				stringId = MapTable.AddStringInfoMap(BinaryWriter, xamlPropertyNode.Value);
			}
		}
		else
		{
			stringId = MapTable.GetStaticMemberId(BinaryWriter, ParserContext, extensionTypeId, xamlPropertyNode.Value, xamlPropertyNode.DefaultTargetType);
		}
		short attributeId = MapTable.AddAttributeInfoMap(BinaryWriter, xamlPropertyNode.AssemblyName, xamlPropertyNode.TypeFullName, xamlPropertyNode.PropDeclaringType, xamlPropertyNode.PropName, xamlPropertyNode.PropValidType, BamlAttributeUsage.Default);
		if (_deferLoadingSupport && _deferElementDepth > 0 && CollectingValues && extensionTypeId == 603)
		{
			BamlOptimizedStaticResourceRecord bamlOptimizedStaticResourceRecord = (BamlOptimizedStaticResourceRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.OptimizedStaticResource);
			bamlOptimizedStaticResourceRecord.IsValueTypeExtension = isValueTypeExtension;
			bamlOptimizedStaticResourceRecord.IsValueStaticExtension = isValueStaticExtension;
			bamlOptimizedStaticResourceRecord.ValueId = stringId;
			_staticResourceRecordList = new List<ValueDeferRecord>(1);
			_staticResourceRecordList.Add(new ValueDeferRecord(bamlOptimizedStaticResourceRecord, xamlPropertyNode.LineNumber, xamlPropertyNode.LinePosition));
			KeyDeferRecord keyDeferRecord = (KeyDeferRecord)_deferKeys[_deferKeys.Count - 1];
			keyDeferRecord.StaticResourceRecordList.Add(_staticResourceRecordList);
			BamlPropertyWithStaticResourceIdRecord bamlPropertyWithStaticResourceIdRecord = (BamlPropertyWithStaticResourceIdRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.PropertyWithStaticResourceId);
			bamlPropertyWithStaticResourceIdRecord.AttributeId = attributeId;
			bamlPropertyWithStaticResourceIdRecord.StaticResourceId = (short)(keyDeferRecord.StaticResourceRecordList.Count - 1);
			_deferValues.Add(new ValueDeferRecord(bamlPropertyWithStaticResourceIdRecord, xamlPropertyNode.LineNumber, xamlPropertyNode.LinePosition));
			_staticResourceRecordList = null;
		}
		else
		{
			BamlPropertyWithExtensionRecord bamlPropertyWithExtensionRecord = (BamlPropertyWithExtensionRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.PropertyWithExtension);
			bamlPropertyWithExtensionRecord.AttributeId = attributeId;
			bamlPropertyWithExtensionRecord.ExtensionTypeId = extensionTypeId;
			bamlPropertyWithExtensionRecord.IsValueTypeExtension = isValueTypeExtension;
			bamlPropertyWithExtensionRecord.IsValueStaticExtension = isValueStaticExtension;
			bamlPropertyWithExtensionRecord.ValueId = stringId;
			WriteAndReleaseRecord(bamlPropertyWithExtensionRecord, xamlPropertyNode);
		}
	}

	internal virtual void WritePropertyWithType(XamlPropertyWithTypeNode xamlPropertyWithType)
	{
		short attributeId = MapTable.AddAttributeInfoMap(BinaryWriter, xamlPropertyWithType.AssemblyName, xamlPropertyWithType.TypeFullName, xamlPropertyWithType.PropDeclaringType, xamlPropertyWithType.PropName, xamlPropertyWithType.PropValidType, BamlAttributeUsage.Default);
		if (!MapTable.GetTypeInfoId(BinaryWriter, xamlPropertyWithType.ValueTypeAssemblyName, xamlPropertyWithType.ValueTypeFullName, out var typeId))
		{
			typeId = MapTable.AddTypeInfoMap(BinaryWriter, xamlPropertyWithType.ValueTypeAssemblyName, xamlPropertyWithType.ValueTypeFullName, xamlPropertyWithType.ValueElementType, xamlPropertyWithType.ValueSerializerTypeAssemblyName, xamlPropertyWithType.ValueSerializerTypeFullName);
		}
		BamlPropertyTypeReferenceRecord bamlPropertyTypeReferenceRecord = BamlRecordManager.GetWriteRecord(BamlRecordType.PropertyTypeReference) as BamlPropertyTypeReferenceRecord;
		bamlPropertyTypeReferenceRecord.AttributeId = attributeId;
		bamlPropertyTypeReferenceRecord.TypeId = typeId;
		WriteAndReleaseRecord(bamlPropertyTypeReferenceRecord, xamlPropertyWithType);
	}

	internal void BaseWriteProperty(XamlPropertyNode xamlProperty)
	{
		short attributeId = MapTable.AddAttributeInfoMap(BinaryWriter, xamlProperty.AssemblyName, xamlProperty.TypeFullName, xamlProperty.PropDeclaringType, xamlProperty.PropName, xamlProperty.PropValidType, xamlProperty.AttributeUsage);
		BamlPropertyRecord bamlPropertyRecord = (BamlPropertyRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.Property);
		bamlPropertyRecord.AttributeId = attributeId;
		bamlPropertyRecord.Value = xamlProperty.Value;
		WriteAndReleaseRecord(bamlPropertyRecord, xamlProperty);
	}

	internal void WriteClrEvent(XamlClrEventNode xamlClrEventNode)
	{
	}

	internal void WritePropertyArrayStart(XamlPropertyArrayStartNode xamlPropertyArrayStartNode)
	{
		BamlPropertyArrayStartRecord bamlPropertyArrayStartRecord = (BamlPropertyArrayStartRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.PropertyArrayStart);
		bamlPropertyArrayStartRecord.AttributeId = MapTable.AddAttributeInfoMap(BinaryWriter, xamlPropertyArrayStartNode.AssemblyName, xamlPropertyArrayStartNode.TypeFullName, xamlPropertyArrayStartNode.PropDeclaringType, xamlPropertyArrayStartNode.PropName, null, BamlAttributeUsage.Default);
		WriteAndReleaseRecord(bamlPropertyArrayStartRecord, xamlPropertyArrayStartNode);
	}

	internal virtual void WritePropertyArrayEnd(XamlPropertyArrayEndNode xamlPropertyArrayEndNode)
	{
		BamlPropertyArrayEndRecord bamlRecord = (BamlPropertyArrayEndRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.PropertyArrayEnd);
		WriteAndReleaseRecord(bamlRecord, xamlPropertyArrayEndNode);
	}

	internal void WritePropertyIListStart(XamlPropertyIListStartNode xamlPropertyIListStart)
	{
		BamlPropertyIListStartRecord bamlPropertyIListStartRecord = (BamlPropertyIListStartRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.PropertyIListStart);
		bamlPropertyIListStartRecord.AttributeId = MapTable.AddAttributeInfoMap(BinaryWriter, xamlPropertyIListStart.AssemblyName, xamlPropertyIListStart.TypeFullName, xamlPropertyIListStart.PropDeclaringType, xamlPropertyIListStart.PropName, null, BamlAttributeUsage.Default);
		WriteAndReleaseRecord(bamlPropertyIListStartRecord, xamlPropertyIListStart);
	}

	internal virtual void WritePropertyIListEnd(XamlPropertyIListEndNode xamlPropertyIListEndNode)
	{
		BamlPropertyIListEndRecord bamlRecord = (BamlPropertyIListEndRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.PropertyIListEnd);
		WriteAndReleaseRecord(bamlRecord, xamlPropertyIListEndNode);
	}

	internal void WritePropertyIDictionaryStart(XamlPropertyIDictionaryStartNode xamlPropertyIDictionaryStartNode)
	{
		BamlPropertyIDictionaryStartRecord bamlPropertyIDictionaryStartRecord = (BamlPropertyIDictionaryStartRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.PropertyIDictionaryStart);
		bamlPropertyIDictionaryStartRecord.AttributeId = MapTable.AddAttributeInfoMap(BinaryWriter, xamlPropertyIDictionaryStartNode.AssemblyName, xamlPropertyIDictionaryStartNode.TypeFullName, xamlPropertyIDictionaryStartNode.PropDeclaringType, xamlPropertyIDictionaryStartNode.PropName, null, BamlAttributeUsage.Default);
		WriteAndReleaseRecord(bamlPropertyIDictionaryStartRecord, xamlPropertyIDictionaryStartNode);
	}

	internal virtual void WritePropertyIDictionaryEnd(XamlPropertyIDictionaryEndNode xamlPropertyIDictionaryEndNode)
	{
		BamlPropertyIDictionaryEndRecord bamlRecord = (BamlPropertyIDictionaryEndRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.PropertyIDictionaryEnd);
		WriteAndReleaseRecord(bamlRecord, xamlPropertyIDictionaryEndNode);
	}

	internal void WriteRoutedEvent(XamlRoutedEventNode xamlRoutedEventNode)
	{
		BamlRoutedEventRecord bamlRoutedEventRecord = (BamlRoutedEventRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.RoutedEvent);
		MapTable.AddAttributeInfoMap(BinaryWriter, xamlRoutedEventNode.AssemblyName, xamlRoutedEventNode.TypeFullName, null, xamlRoutedEventNode.EventName, null, BamlAttributeUsage.Default, out var bamlAttributeInfoRecord);
		bamlAttributeInfoRecord.Event = xamlRoutedEventNode.Event;
		bamlRoutedEventRecord.AttributeId = bamlAttributeInfoRecord.AttributeId;
		bamlRoutedEventRecord.Value = xamlRoutedEventNode.Value;
		WriteAndReleaseRecord(bamlRoutedEventRecord, xamlRoutedEventNode);
	}

	internal void WriteText(XamlTextNode xamlTextNode)
	{
		BamlTextRecord bamlTextRecord;
		if (xamlTextNode.ConverterType == null)
		{
			if (!InStaticResourceSection && !InDynamicResourceSection)
			{
				bamlTextRecord = (BamlTextRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.Text);
			}
			else
			{
				bamlTextRecord = (BamlTextWithIdRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.TextWithId);
				if (!MapTable.GetStringInfoId(xamlTextNode.Text, out var stringId))
				{
					stringId = MapTable.AddStringInfoMap(BinaryWriter, xamlTextNode.Text);
				}
				((BamlTextWithIdRecord)bamlTextRecord).ValueId = stringId;
			}
		}
		else
		{
			bamlTextRecord = (BamlTextWithConverterRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.TextWithConverter);
			string fullName = xamlTextNode.ConverterType.Assembly.FullName;
			string fullName2 = xamlTextNode.ConverterType.FullName;
			if (!MapTable.GetTypeInfoId(BinaryWriter, fullName, fullName2, out var typeId))
			{
				typeId = MapTable.AddTypeInfoMap(BinaryWriter, fullName, fullName2, xamlTextNode.ConverterType, string.Empty, string.Empty);
			}
			((BamlTextWithConverterRecord)bamlTextRecord).ConverterTypeId = typeId;
		}
		bamlTextRecord.Value = xamlTextNode.Text;
		WriteAndReleaseRecord(bamlTextRecord, xamlTextNode);
	}

	private void WriteAndReleaseRecord(BamlRecord bamlRecord, XamlNode xamlNode)
	{
		int lineNumber = xamlNode?.LineNumber ?? 0;
		int linePosition = xamlNode?.LinePosition ?? 0;
		if (_deferLoadingSupport && _deferElementDepth > 0)
		{
			if (InStaticResourceSection)
			{
				_staticResourceRecordList.Add(new ValueDeferRecord(bamlRecord, lineNumber, linePosition));
				return;
			}
			ValueDeferRecord value = new ValueDeferRecord(bamlRecord, lineNumber, linePosition);
			if (_deferEndOfStartReached)
			{
				if (_deferElementDepth == 1 && xamlNode is XamlPropertyComplexStartNode)
				{
					_deferComplexPropertyDepth++;
				}
				if (_deferComplexPropertyDepth > 0)
				{
					_deferElement.Add(value);
					if (_deferElementDepth == 1 && xamlNode is XamlPropertyComplexEndNode)
					{
						_deferComplexPropertyDepth--;
					}
				}
				else if (_deferKeyCollecting)
				{
					((KeyDeferRecord)_deferKeys[_deferKeys.Count - 1]).RecordList.Add(value);
				}
				else
				{
					_deferValues.Add(value);
				}
			}
			else
			{
				_deferElement.Add(value);
			}
		}
		else
		{
			WriteBamlRecord(bamlRecord, lineNumber, linePosition);
			BamlRecordManager.ReleaseWriteRecord(bamlRecord);
		}
	}

	private void WriteDeferableContent(XamlElementEndNode xamlNode)
	{
		for (int i = 0; i < _deferElement.Count; i++)
		{
			ValueDeferRecord valueDeferRecord = (ValueDeferRecord)_deferElement[i];
			WriteBamlRecord(valueDeferRecord.Record, valueDeferRecord.LineNumber, valueDeferRecord.LinePosition);
		}
		BamlDeferableContentStartRecord bamlDeferableContentStartRecord = (BamlDeferableContentStartRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.DeferableContentStart);
		WriteBamlRecord(bamlDeferableContentStartRecord, xamlNode.LineNumber, xamlNode.LinePosition);
		long num = BinaryWriter.Seek(0, SeekOrigin.Current);
		for (int j = 0; j < _deferKeys.Count; j++)
		{
			KeyDeferRecord keyDeferRecord = (KeyDeferRecord)_deferKeys[j];
			if (keyDeferRecord.RecordList != null && keyDeferRecord.RecordList.Count > 0)
			{
				for (int k = 0; k < keyDeferRecord.RecordList.Count; k++)
				{
					ValueDeferRecord valueDeferRecord2 = (ValueDeferRecord)keyDeferRecord.RecordList[k];
					WriteBamlRecord(valueDeferRecord2.Record, valueDeferRecord2.LineNumber, valueDeferRecord2.LinePosition);
				}
			}
			else if (keyDeferRecord.Record == null)
			{
				XamlParser.ThrowException("ParserNoDictionaryKey", keyDeferRecord.LineNumber, keyDeferRecord.LinePosition);
			}
			else
			{
				WriteBamlRecord(keyDeferRecord.Record, keyDeferRecord.LineNumber, keyDeferRecord.LinePosition);
			}
			List<List<ValueDeferRecord>> staticResourceRecordList = keyDeferRecord.StaticResourceRecordList;
			if (staticResourceRecordList.Count <= 0)
			{
				continue;
			}
			for (int l = 0; l < staticResourceRecordList.Count; l++)
			{
				List<ValueDeferRecord> list = staticResourceRecordList[l];
				for (int m = 0; m < list.Count; m++)
				{
					ValueDeferRecord valueDeferRecord3 = list[m];
					WriteBamlRecord(valueDeferRecord3.Record, valueDeferRecord3.LineNumber, valueDeferRecord3.LinePosition);
				}
			}
		}
		long num2 = BinaryWriter.Seek(0, SeekOrigin.Current);
		int num3 = 0;
		for (int n = 0; n < _deferValues.Count; n++)
		{
			ValueDeferRecord valueDeferRecord4 = (ValueDeferRecord)_deferValues[n];
			if (valueDeferRecord4.UpdateOffset)
			{
				KeyDeferRecord keyDeferRecord2 = (KeyDeferRecord)_deferKeys[num3++];
				long num4 = BinaryWriter.Seek(0, SeekOrigin.Current);
				((keyDeferRecord2.RecordList == null || keyDeferRecord2.RecordList.Count <= 0) ? ((IBamlDictionaryKey)keyDeferRecord2.Record) : ((IBamlDictionaryKey)((ValueDeferRecord)keyDeferRecord2.RecordList[0]).Record))?.UpdateValuePosition((int)(num4 - num2), BinaryWriter);
			}
			WriteBamlRecord(valueDeferRecord4.Record, valueDeferRecord4.LineNumber, valueDeferRecord4.LinePosition);
		}
		long num5 = BinaryWriter.Seek(0, SeekOrigin.Current);
		bamlDeferableContentStartRecord.UpdateContentSize((int)(num5 - num), BinaryWriter);
		BamlElementEndRecord bamlElementEndRecord = (BamlElementEndRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.ElementEnd);
		WriteBamlRecord(bamlElementEndRecord, xamlNode.LineNumber, xamlNode.LinePosition);
		BamlRecordManager.ReleaseWriteRecord(bamlElementEndRecord);
	}

	private void WriteStaticResource()
	{
		ValueDeferRecord valueDeferRecord = _staticResourceRecordList[0];
		int lineNumber = valueDeferRecord.LineNumber;
		int linePosition = valueDeferRecord.LinePosition;
		BamlStaticResourceStartRecord bamlStaticResourceStartRecord = (BamlStaticResourceStartRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.StaticResourceStart);
		bamlStaticResourceStartRecord.TypeId = ((BamlElementStartRecord)valueDeferRecord.Record).TypeId;
		valueDeferRecord.Record = bamlStaticResourceStartRecord;
		valueDeferRecord = _staticResourceRecordList[_staticResourceRecordList.Count - 1];
		BamlStaticResourceEndRecord record = (BamlStaticResourceEndRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.StaticResourceEnd);
		valueDeferRecord.Record = record;
		KeyDeferRecord keyDeferRecord = (KeyDeferRecord)_deferKeys[_deferKeys.Count - 1];
		keyDeferRecord.StaticResourceRecordList.Add(_staticResourceRecordList);
		BamlStaticResourceIdRecord bamlStaticResourceIdRecord = (BamlStaticResourceIdRecord)BamlRecordManager.GetWriteRecord(BamlRecordType.StaticResourceId);
		bamlStaticResourceIdRecord.StaticResourceId = (short)(keyDeferRecord.StaticResourceRecordList.Count - 1);
		_deferValues.Add(new ValueDeferRecord(bamlStaticResourceIdRecord, lineNumber, linePosition));
	}
}
