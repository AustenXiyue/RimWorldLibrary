using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Data;
using System.Windows.Navigation;
using System.Xml;
using System.Xml.Serialization;
using MS.Internal;
using MS.Internal.IO.Packaging;
using MS.Internal.Utility;
using MS.Utility;

namespace System.Windows.Markup;

internal class BamlRecordReader
{
	private static Type NullableType = typeof(Nullable<>);

	private IComponentConnector _componentConnector;

	private object _rootElement;

	private bool _bamlAsForest;

	private bool _isRootAlreadyLoaded;

	private ArrayList _rootList;

	private ParserContext _parserContext;

	private TypeConvertContext _typeConvertContext;

	private int _persistId;

	private ParserStack _contextStack = new ParserStack();

	private XamlParseMode _parseMode = XamlParseMode.Synchronous;

	private int _maxAsyncRecords;

	private Stream _bamlStream;

	private ReaderStream _xamlReaderStream;

	private BamlBinaryReader _binaryReader;

	private BamlRecordManager _bamlRecordManager;

	private BamlRecord _preParsedBamlRecordsStart;

	private BamlRecord _preParsedIndexRecord;

	private bool _endOfDocument;

	private bool _buildTopDown = true;

	private BamlRecordReader _previousBamlRecordReader;

	private static List<ReaderContextStackData> _stackDataFactoryCache = new List<ReaderContextStackData>();

	internal ArrayList RootList
	{
		get
		{
			return _rootList;
		}
		set
		{
			_rootList = value;
		}
	}

	internal bool BuildTopDown
	{
		get
		{
			return _buildTopDown;
		}
		set
		{
			_buildTopDown = value;
		}
	}

	internal int BytesAvailible
	{
		get
		{
			Stream baseStream = BinaryReader.BaseStream;
			return (int)(baseStream.Length - baseStream.Position);
		}
	}

	internal BamlRecord PreParsedRecordsStart
	{
		get
		{
			return _preParsedBamlRecordsStart;
		}
		set
		{
			_preParsedBamlRecordsStart = value;
		}
	}

	internal BamlRecord PreParsedCurrentRecord
	{
		get
		{
			return _preParsedIndexRecord;
		}
		set
		{
			_preParsedIndexRecord = value;
		}
	}

	internal Stream BamlStream
	{
		get
		{
			return _bamlStream;
		}
		set
		{
			_bamlStream = value;
			if (_bamlStream is ReaderStream)
			{
				_xamlReaderStream = (ReaderStream)_bamlStream;
			}
			else
			{
				_xamlReaderStream = null;
			}
			if (BamlStream != null)
			{
				_binaryReader = new BamlBinaryReader(BamlStream, new UTF8Encoding());
			}
		}
	}

	internal BamlBinaryReader BinaryReader => _binaryReader;

	internal XamlTypeMapper XamlTypeMapper => ParserContext.XamlTypeMapper;

	internal ParserContext ParserContext
	{
		get
		{
			return _parserContext;
		}
		set
		{
			_parserContext = value;
			_typeConvertContext = null;
		}
	}

	internal TypeConvertContext TypeConvertContext
	{
		get
		{
			if (_typeConvertContext == null)
			{
				_typeConvertContext = new TypeConvertContext(ParserContext);
			}
			return _typeConvertContext;
		}
	}

	internal XamlParseMode XamlParseMode
	{
		get
		{
			return _parseMode;
		}
		set
		{
			_parseMode = value;
		}
	}

	internal int MaxAsyncRecords
	{
		get
		{
			return _maxAsyncRecords;
		}
		set
		{
			_maxAsyncRecords = value;
		}
	}

	internal BamlMapTable MapTable => ParserContext.MapTable;

	internal XmlnsDictionary XmlnsDictionary => ParserContext.XmlnsDictionary;

	internal ReaderContextStackData CurrentContext => (ReaderContextStackData)ReaderContextStack.CurrentContext;

	internal ReaderContextStackData ParentContext => (ReaderContextStackData)ReaderContextStack.ParentContext;

	internal object ParentObjectData => ParentContext?.ObjectData;

	internal ReaderContextStackData GrandParentContext => (ReaderContextStackData)ReaderContextStack.GrandParentContext;

	internal object GrandParentObjectData => GrandParentContext?.ObjectData;

	internal ReaderContextStackData GreatGrandParentContext => (ReaderContextStackData)ReaderContextStack.GreatGrandParentContext;

	internal ParserStack ReaderContextStack => _contextStack;

	internal BamlRecordManager BamlRecordManager
	{
		get
		{
			if (_bamlRecordManager == null)
			{
				_bamlRecordManager = new BamlRecordManager();
			}
			return _bamlRecordManager;
		}
	}

	internal bool EndOfDocument
	{
		get
		{
			return _endOfDocument;
		}
		set
		{
			_endOfDocument = value;
		}
	}

	internal object RootElement
	{
		get
		{
			return _rootElement;
		}
		set
		{
			_rootElement = value;
		}
	}

	internal IComponentConnector ComponentConnector
	{
		get
		{
			return _componentConnector;
		}
		set
		{
			_componentConnector = value;
		}
	}

	private ReaderStream XamlReaderStream => _xamlReaderStream;

	internal ParserStack ContextStack
	{
		get
		{
			return _contextStack;
		}
		set
		{
			_contextStack = value;
		}
	}

	internal int LineNumber
	{
		get
		{
			return ParserContext.LineNumber;
		}
		set
		{
			ParserContext.LineNumber = value;
		}
	}

	internal int LinePosition
	{
		get
		{
			return ParserContext.LinePosition;
		}
		set
		{
			ParserContext.LinePosition = value;
		}
	}

	internal bool IsDebugBamlStream
	{
		get
		{
			return ParserContext.IsDebugBamlStream;
		}
		set
		{
			ParserContext.IsDebugBamlStream = value;
		}
	}

	internal long StreamPosition => _bamlStream.Position;

	private long StreamLength => _bamlStream.Length;

	internal bool IsRootAlreadyLoaded
	{
		get
		{
			return _isRootAlreadyLoaded;
		}
		set
		{
			_isRootAlreadyLoaded = value;
		}
	}

	internal BamlRecordReader PreviousBamlRecordReader => _previousBamlRecordReader;

	internal BamlRecordReader(Stream bamlStream, ParserContext parserContext)
		: this(bamlStream, parserContext, loadMapper: true)
	{
		XamlParseMode = XamlParseMode.Synchronous;
	}

	internal BamlRecordReader(Stream bamlStream, ParserContext parserContext, object root)
	{
		ParserContext = parserContext;
		_rootElement = root;
		_bamlAsForest = root != null;
		if (_bamlAsForest)
		{
			ParserContext.RootElement = _rootElement;
		}
		_rootList = new ArrayList(1);
		BamlStream = bamlStream;
	}

	internal BamlRecordReader(Stream bamlStream, ParserContext parserContext, bool loadMapper)
	{
		ParserContext = parserContext;
		_rootList = new ArrayList(1);
		BamlStream = bamlStream;
		if (loadMapper)
		{
			ParserContext.XamlTypeMapper = XamlTypeMapper;
		}
	}

	protected internal BamlRecordReader()
	{
	}

	internal void Initialize()
	{
		MapTable.Initialize();
		XamlTypeMapper.Initialize();
		ParserContext.Initialize();
	}

	internal BamlRecord GetNextRecord()
	{
		BamlRecord bamlRecord = null;
		if (PreParsedRecordsStart == null)
		{
			Stream baseStream = BinaryReader.BaseStream;
			if (XamlReaderStream != null)
			{
				long position = baseStream.Position;
				long num = baseStream.Length - position;
				if (1 > num)
				{
					return null;
				}
				BamlRecordType recordType = (BamlRecordType)BinaryReader.ReadByte();
				num--;
				bamlRecord = ReadNextRecordWithDebugExtension(num, recordType);
				if (bamlRecord == null)
				{
					baseStream.Seek(position, SeekOrigin.Begin);
					return null;
				}
				XamlReaderStream.ReaderDoneWithFileUpToPosition(baseStream.Position - 1);
			}
			else
			{
				bool flag = true;
				while (flag)
				{
					if (BinaryReader.BaseStream.Length > BinaryReader.BaseStream.Position)
					{
						BamlRecordType recordType2 = (BamlRecordType)BinaryReader.ReadByte();
						bamlRecord = ReadNextRecordWithDebugExtension(long.MaxValue, recordType2);
						flag = false;
					}
					else
					{
						flag = false;
					}
				}
			}
		}
		else if (PreParsedCurrentRecord != null)
		{
			bamlRecord = PreParsedCurrentRecord;
			PreParsedCurrentRecord = PreParsedCurrentRecord.Next;
			if (BamlRecordHelper.HasDebugExtensionRecord(ParserContext.IsDebugBamlStream, bamlRecord))
			{
				ProcessDebugBamlRecord(PreParsedCurrentRecord);
				PreParsedCurrentRecord = PreParsedCurrentRecord.Next;
			}
		}
		return bamlRecord;
	}

	internal BamlRecord ReadNextRecordWithDebugExtension(long bytesAvailable, BamlRecordType recordType)
	{
		BamlRecord bamlRecord = BamlRecordManager.ReadNextRecord(BinaryReader, bytesAvailable, recordType);
		if (IsDebugBamlStream && BamlRecordHelper.DoesRecordTypeHaveDebugExtension(bamlRecord.RecordType))
		{
			BamlRecord next = ReadDebugExtensionRecord();
			bamlRecord.Next = next;
		}
		return bamlRecord;
	}

	internal BamlRecord ReadDebugExtensionRecord()
	{
		Stream baseStream = BinaryReader.BaseStream;
		long num = baseStream.Length - baseStream.Position;
		if (num == 0L)
		{
			return null;
		}
		BamlRecordType recordType = (BamlRecordType)BinaryReader.ReadByte();
		if (BamlRecordHelper.IsDebugBamlRecordType(recordType))
		{
			BamlRecord bamlRecord = BamlRecordManager.ReadNextRecord(BinaryReader, num, recordType);
			ProcessDebugBamlRecord(bamlRecord);
			return bamlRecord;
		}
		baseStream.Seek(-1L, SeekOrigin.Current);
		return null;
	}

	internal void ProcessDebugBamlRecord(BamlRecord bamlRecord)
	{
		if (bamlRecord.RecordType == BamlRecordType.LineNumberAndPosition)
		{
			BamlLineAndPositionRecord bamlLineAndPositionRecord = (BamlLineAndPositionRecord)bamlRecord;
			LineNumber = (int)bamlLineAndPositionRecord.LineNumber;
			LinePosition = (int)bamlLineAndPositionRecord.LinePosition;
		}
		else
		{
			BamlLinePositionRecord bamlLinePositionRecord = (BamlLinePositionRecord)bamlRecord;
			LinePosition = (int)bamlLinePositionRecord.LinePosition;
		}
	}

	internal BamlRecordType GetNextRecordType()
	{
		if (PreParsedRecordsStart == null)
		{
			return (BamlRecordType)BinaryReader.PeekChar();
		}
		return PreParsedCurrentRecord.RecordType;
	}

	internal void Close()
	{
		if (BamlStream != null)
		{
			BamlStream.Close();
		}
		EndOfDocument = true;
	}

	internal bool Read(bool singleRecord)
	{
		BamlRecord bamlRecord = null;
		bool flag = true;
		while (flag && (bamlRecord = GetNextRecord()) != null)
		{
			flag = ReadRecord(bamlRecord);
			if (singleRecord)
			{
				break;
			}
		}
		if (bamlRecord == null)
		{
			flag = false;
		}
		return flag;
	}

	internal bool Read()
	{
		return Read(singleRecord: false);
	}

	internal bool Read(BamlRecord bamlRecord, int lineNumber, int linePosition)
	{
		LineNumber = lineNumber;
		LinePosition = linePosition;
		return ReadRecord(bamlRecord);
	}

	internal void ReadVersionHeader()
	{
		new BamlVersionHeader().LoadVersion(BinaryReader);
	}

	internal object ReadElement(long startPosition, XamlObjectIds contextXamlObjectIds, object dictionaryKey)
	{
		BamlRecord bamlRecord = null;
		bool flag = true;
		BinaryReader.BaseStream.Position = startPosition;
		int num = 0;
		bool flag2 = false;
		PushContext(ReaderFlags.RealizeDeferContent, null, null, 0);
		CurrentContext.ElementNameOrPropertyName = contextXamlObjectIds.Name;
		CurrentContext.Uid = contextXamlObjectIds.Uid;
		CurrentContext.Key = dictionaryKey;
		while (flag && (bamlRecord = GetNextRecord()) != null)
		{
			if (bamlRecord is BamlElementStartRecord bamlElementStartRecord)
			{
				if (!MapTable.HasSerializerForTypeId(bamlElementStartRecord.TypeId))
				{
					num++;
				}
			}
			else if (bamlRecord is BamlElementEndRecord)
			{
				num--;
			}
			flag = ReadRecord(bamlRecord);
			if (!flag2)
			{
				CurrentContext.Key = dictionaryKey;
				flag2 = true;
			}
			if (num == 0)
			{
				break;
			}
		}
		object objectData = CurrentContext.ObjectData;
		CurrentContext.ObjectData = null;
		PopContext();
		MapTable.ClearConverterCache();
		return objectData;
	}

	protected virtual void ReadConnectionId(BamlConnectionIdRecord bamlConnectionIdRecord)
	{
		if (_componentConnector != null)
		{
			object currentObjectData = GetCurrentObjectData();
			_componentConnector.Connect(bamlConnectionIdRecord.ConnectionId, currentObjectData);
		}
	}

	private void ReadDocumentStartRecord(BamlDocumentStartRecord documentStartRecord)
	{
		IsDebugBamlStream = documentStartRecord.DebugBaml;
	}

	private void ReadDocumentEndRecord()
	{
		SetPropertyValueToParent(fromStartTag: false);
		ParserContext.RootElement = null;
		MapTable.ClearConverterCache();
		EndOfDocument = true;
	}

	internal virtual bool ReadRecord(BamlRecord bamlRecord)
	{
		bool result = true;
		try
		{
			switch (bamlRecord.RecordType)
			{
			case BamlRecordType.DocumentStart:
				ReadDocumentStartRecord((BamlDocumentStartRecord)bamlRecord);
				break;
			case BamlRecordType.DocumentEnd:
				ReadDocumentEndRecord();
				result = false;
				break;
			case BamlRecordType.XmlnsProperty:
				ReadXmlnsPropertyRecord((BamlXmlnsPropertyRecord)bamlRecord);
				break;
			case BamlRecordType.PIMapping:
			{
				BamlPIMappingRecord bamlPIMappingRecord = (BamlPIMappingRecord)bamlRecord;
				if (!XamlTypeMapper.PITable.Contains(bamlPIMappingRecord.XmlNamespace))
				{
					BamlAssemblyInfoRecord assemblyInfoFromId = MapTable.GetAssemblyInfoFromId(bamlPIMappingRecord.AssemblyId);
					ClrNamespaceAssemblyPair clrNamespaceAssemblyPair = new ClrNamespaceAssemblyPair(bamlPIMappingRecord.ClrNamespace, assemblyInfoFromId.AssemblyFullName);
					XamlTypeMapper.PITable.Add(bamlPIMappingRecord.XmlNamespace, clrNamespaceAssemblyPair);
				}
				break;
			}
			case BamlRecordType.AssemblyInfo:
				MapTable.LoadAssemblyInfoRecord((BamlAssemblyInfoRecord)bamlRecord);
				break;
			case BamlRecordType.TypeInfo:
			case BamlRecordType.TypeSerializerInfo:
				MapTable.LoadTypeInfoRecord((BamlTypeInfoRecord)bamlRecord);
				break;
			case BamlRecordType.AttributeInfo:
				MapTable.LoadAttributeInfoRecord((BamlAttributeInfoRecord)bamlRecord);
				break;
			case BamlRecordType.StringInfo:
				MapTable.LoadStringInfoRecord((BamlStringInfoRecord)bamlRecord);
				break;
			case BamlRecordType.LiteralContent:
				ReadLiteralContentRecord((BamlLiteralContentRecord)bamlRecord);
				break;
			case BamlRecordType.ElementStart:
			case BamlRecordType.StaticResourceStart:
				if (((BamlElementStartRecord)bamlRecord).IsInjected)
				{
					CurrentContext.SetFlag(ReaderFlags.InjectedElement);
				}
				else
				{
					ReadElementStartRecord((BamlElementStartRecord)bamlRecord);
				}
				break;
			case BamlRecordType.ConnectionId:
				ReadConnectionId((BamlConnectionIdRecord)bamlRecord);
				break;
			case BamlRecordType.ElementEnd:
			case BamlRecordType.StaticResourceEnd:
				if (CurrentContext.CheckFlag(ReaderFlags.InjectedElement))
				{
					CurrentContext.ClearFlag(ReaderFlags.InjectedElement);
				}
				else
				{
					ReadElementEndRecord(fromNestedBamlRecordReader: false);
				}
				break;
			case BamlRecordType.PropertyComplexStart:
				ReadPropertyComplexStartRecord((BamlPropertyComplexStartRecord)bamlRecord);
				break;
			case BamlRecordType.PropertyComplexEnd:
				ReadPropertyComplexEndRecord();
				break;
			case BamlRecordType.Property:
				ReadPropertyRecord((BamlPropertyRecord)bamlRecord);
				break;
			case BamlRecordType.PropertyStringReference:
				ReadPropertyStringRecord((BamlPropertyStringReferenceRecord)bamlRecord);
				break;
			case BamlRecordType.PropertyTypeReference:
				ReadPropertyTypeRecord((BamlPropertyTypeReferenceRecord)bamlRecord);
				break;
			case BamlRecordType.PropertyWithExtension:
				ReadPropertyWithExtensionRecord((BamlPropertyWithExtensionRecord)bamlRecord);
				break;
			case BamlRecordType.PropertyWithConverter:
				ReadPropertyConverterRecord((BamlPropertyWithConverterRecord)bamlRecord);
				break;
			case BamlRecordType.PropertyCustom:
				ReadPropertyCustomRecord((BamlPropertyCustomRecord)bamlRecord);
				break;
			case BamlRecordType.PropertyArrayStart:
				ReadPropertyArrayStartRecord((BamlPropertyArrayStartRecord)bamlRecord);
				break;
			case BamlRecordType.PropertyArrayEnd:
				ReadPropertyArrayEndRecord();
				break;
			case BamlRecordType.PropertyIListStart:
				ReadPropertyIListStartRecord((BamlPropertyIListStartRecord)bamlRecord);
				break;
			case BamlRecordType.PropertyIListEnd:
				ReadPropertyIListEndRecord();
				break;
			case BamlRecordType.PropertyIDictionaryStart:
				ReadPropertyIDictionaryStartRecord((BamlPropertyIDictionaryStartRecord)bamlRecord);
				break;
			case BamlRecordType.PropertyIDictionaryEnd:
				ReadPropertyIDictionaryEndRecord();
				break;
			case BamlRecordType.DefAttribute:
				ReadDefAttributeRecord((BamlDefAttributeRecord)bamlRecord);
				break;
			case BamlRecordType.DefAttributeKeyType:
				ReadDefAttributeKeyTypeRecord((BamlDefAttributeKeyTypeRecord)bamlRecord);
				break;
			case BamlRecordType.PresentationOptionsAttribute:
				ReadPresentationOptionsAttributeRecord((BamlPresentationOptionsAttributeRecord)bamlRecord);
				break;
			case BamlRecordType.RoutedEvent:
			{
				GetCurrentObjectData();
				BamlRoutedEventRecord bamlRoutedEventRecord = (BamlRoutedEventRecord)bamlRecord;
				ThrowException("ParserBamlEvent", bamlRoutedEventRecord.Value);
				break;
			}
			case BamlRecordType.Text:
			case BamlRecordType.TextWithConverter:
			case BamlRecordType.TextWithId:
				ReadTextRecord((BamlTextRecord)bamlRecord);
				break;
			case BamlRecordType.DeferableContentStart:
				ReadDeferableContentStart((BamlDeferableContentStartRecord)bamlRecord);
				break;
			case BamlRecordType.KeyElementStart:
				ReadKeyElementStartRecord((BamlKeyElementStartRecord)bamlRecord);
				break;
			case BamlRecordType.KeyElementEnd:
				ReadKeyElementEndRecord();
				break;
			case BamlRecordType.ConstructorParametersStart:
				ReadConstructorParametersStartRecord();
				break;
			case BamlRecordType.ConstructorParametersEnd:
				ReadConstructorParametersEndRecord();
				break;
			case BamlRecordType.ConstructorParameterType:
				ReadConstructorParameterTypeRecord((BamlConstructorParameterTypeRecord)bamlRecord);
				break;
			case BamlRecordType.ContentProperty:
				ReadContentPropertyRecord((BamlContentPropertyRecord)bamlRecord);
				break;
			case BamlRecordType.StaticResourceId:
				ReadStaticResourceIdRecord((BamlStaticResourceIdRecord)bamlRecord);
				break;
			case BamlRecordType.PropertyWithStaticResourceId:
				ReadPropertyWithStaticResourceIdRecord((BamlPropertyWithStaticResourceIdRecord)bamlRecord);
				break;
			default:
				ThrowException("ParserUnknownBaml", ((int)bamlRecord.RecordType).ToString(CultureInfo.CurrentCulture));
				break;
			case BamlRecordType.NamedElementStart:
				break;
			}
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex) || ex is XamlParseException)
			{
				throw;
			}
			XamlParseException.ThrowException(ParserContext, LineNumber, LinePosition, string.Empty, ex);
		}
		return result;
	}

	protected virtual void ReadXmlnsPropertyRecord(BamlXmlnsPropertyRecord xmlnsRecord)
	{
		if (ReaderFlags.DependencyObject == CurrentContext.ContextType || ReaderFlags.ClrObject == CurrentContext.ContextType || ReaderFlags.PropertyComplexClr == CurrentContext.ContextType || ReaderFlags.PropertyComplexDP == CurrentContext.ContextType)
		{
			XmlnsDictionary[xmlnsRecord.Prefix] = xmlnsRecord.XmlNamespace;
			XamlTypeMapper.SetUriToAssemblyNameMapping(xmlnsRecord.XmlNamespace, xmlnsRecord.AssemblyIds);
			if (ReaderFlags.DependencyObject == CurrentContext.ContextType)
			{
				SetXmlnsOnCurrentObject(xmlnsRecord);
			}
		}
	}

	private void GetElementAndFlags(BamlElementStartRecord bamlElementStartRecord, out object element, out ReaderFlags flags, out Type delayCreatedType, out short delayCreatedTypeId)
	{
		short typeId = bamlElementStartRecord.TypeId;
		Type typeFromId = MapTable.GetTypeFromId(typeId);
		element = null;
		delayCreatedType = null;
		delayCreatedTypeId = 0;
		flags = ReaderFlags.Unknown;
		if (!(null != typeFromId))
		{
			return;
		}
		if (bamlElementStartRecord.CreateUsingTypeConverter || typeof(MarkupExtension).IsAssignableFrom(typeFromId))
		{
			delayCreatedType = typeFromId;
			delayCreatedTypeId = typeId;
		}
		else
		{
			element = CreateInstanceFromType(typeFromId, typeId, throwOnFail: false);
			if (element == null)
			{
				ThrowException("ParserNoElementCreate2", typeFromId.FullName);
			}
		}
		flags = GetFlagsFromType(typeFromId);
	}

	protected ReaderFlags GetFlagsFromType(Type elementType)
	{
		ReaderFlags readerFlags = (typeof(DependencyObject).IsAssignableFrom(elementType) ? ReaderFlags.DependencyObject : ReaderFlags.ClrObject);
		if (typeof(IDictionary).IsAssignableFrom(elementType))
		{
			readerFlags |= ReaderFlags.IDictionary;
		}
		else if (typeof(IList).IsAssignableFrom(elementType))
		{
			readerFlags |= ReaderFlags.IList;
		}
		else if (typeof(ArrayExtension).IsAssignableFrom(elementType))
		{
			readerFlags |= ReaderFlags.ArrayExt;
		}
		else if (BamlRecordManager.TreatAsIAddChild(elementType))
		{
			readerFlags |= ReaderFlags.IAddChild;
		}
		return readerFlags;
	}

	internal static void CheckForTreeAdd(ref ReaderFlags flags, ReaderContextStackData context)
	{
		if (context == null || (context.ContextType != ReaderFlags.ConstructorParams && context.ContextType != ReaderFlags.RealizeDeferContent))
		{
			flags |= ReaderFlags.NeedToAddToTree;
		}
	}

	internal void SetDependencyValue(DependencyObject dependencyObject, DependencyProperty dependencyProperty, object value)
	{
		FrameworkPropertyMetadata frameworkPropertyMetadata = ((ParserContext != null && ParserContext.SkipJournaledProperties) ? (dependencyProperty.GetMetadata(dependencyObject.DependencyObjectType) as FrameworkPropertyMetadata) : null);
		if (frameworkPropertyMetadata == null || !frameworkPropertyMetadata.Journal || value is Expression)
		{
			SetDependencyValueCore(dependencyObject, dependencyProperty, value);
		}
	}

	internal virtual void SetDependencyValueCore(DependencyObject dependencyObject, DependencyProperty dependencyProperty, object value)
	{
		dependencyObject.SetValue(dependencyProperty, value);
	}

	internal object ProvideValueFromMarkupExtension(MarkupExtension markupExtension, object obj, object member)
	{
		object obj2 = null;
		ProvideValueServiceProvider provideValueProvider = ParserContext.ProvideValueProvider;
		provideValueProvider.SetData(obj, member);
		try
		{
			obj2 = markupExtension.ProvideValue(provideValueProvider);
			if (TraceMarkup.IsEnabled)
			{
				TraceMarkup.TraceActivityItem(TraceMarkup.ProvideValue, markupExtension, obj, member, obj2);
			}
		}
		finally
		{
			provideValueProvider.ClearData();
		}
		return obj2;
	}

	internal void BaseReadElementStartRecord(BamlElementStartRecord bamlElementRecord)
	{
		object element = null;
		Type delayCreatedType = null;
		short delayCreatedTypeId = 0;
		ReaderFlags flags = ReaderFlags.Unknown;
		ReaderContextStackData currentContext = CurrentContext;
		if (_bamlAsForest && currentContext == null)
		{
			element = _rootElement;
			flags = GetFlagsFromType(element.GetType());
		}
		else
		{
			if (currentContext != null && (ReaderFlags.PropertyComplexClr == currentContext.ContextType || ReaderFlags.PropertyComplexDP == currentContext.ContextType) && null == currentContext.ExpectedType)
			{
				string propNameFrom = GetPropNameFrom(currentContext.ObjectData);
				ThrowException("ParserNoComplexMulti", propNameFrom);
			}
			if (ParentContext == null)
			{
				SetPropertyValueToParent(fromStartTag: true);
			}
			GetElementAndFlags(bamlElementRecord, out element, out flags, out delayCreatedType, out delayCreatedTypeId);
		}
		Stream bamlStream = BamlStream;
		if (!_bamlAsForest && currentContext == null && element != null && bamlStream != null && !(bamlStream is ReaderStream) && StreamPosition == StreamLength)
		{
			ReadDocumentEndRecord();
			if (RootList.Count == 0)
			{
				RootList.Add(element);
			}
			IsRootAlreadyLoaded = true;
			return;
		}
		if (element != null)
		{
			string name = null;
			if (bamlElementRecord is BamlNamedElementStartRecord)
			{
				name = (bamlElementRecord as BamlNamedElementStartRecord).RuntimeName;
			}
			ElementInitialize(element, name);
		}
		CheckForTreeAdd(ref flags, currentContext);
		PushContext(flags, element, delayCreatedType, delayCreatedTypeId, bamlElementRecord.CreateUsingTypeConverter);
		if (BuildTopDown && element != null && (element is UIElement || element is ContentElement || element is UIElement3D))
		{
			SetPropertyValueToParent(fromStartTag: true);
		}
		else
		{
			if (!CurrentContext.CheckFlag(ReaderFlags.IDictionary))
			{
				return;
			}
			bool isMarkupExtension = false;
			if (CheckExplicitCollectionTag(ref isMarkupExtension))
			{
				CurrentContext.MarkAddedToTree();
				if (element is ResourceDictionary)
				{
					SetCollectionPropertyValue(ParentContext);
				}
			}
		}
	}

	protected virtual bool ReadElementStartRecord(BamlElementStartRecord bamlElementRecord)
	{
		if (MapTable.HasSerializerForTypeId(bamlElementRecord.TypeId))
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXamlBaml, EventTrace.Level.Verbose, EventTrace.Event.WClientParseRdrCrInstBegin);
			try
			{
				BamlTypeInfoRecord typeInfoFromId = MapTable.GetTypeInfoFromId(bamlElementRecord.TypeId);
				XamlSerializer xamlSerializer = CreateSerializer((BamlTypeInfoWithSerializerRecord)typeInfoFromId);
				if (ParserContext.RootElement == null)
				{
					ParserContext.RootElement = _rootElement;
				}
				if (ParserContext.StyleConnector == null)
				{
					ParserContext.StyleConnector = _rootElement as IStyleConnector;
				}
				xamlSerializer.ConvertBamlToObject(this, bamlElementRecord, ParserContext);
			}
			finally
			{
				EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXamlBaml, EventTrace.Level.Verbose, EventTrace.Event.WClientParseRdrCrInstEnd);
			}
			return true;
		}
		BaseReadElementStartRecord(bamlElementRecord);
		return false;
	}

	protected internal virtual void ReadElementEndRecord(bool fromNestedBamlRecordReader)
	{
		if (CurrentContext == null || (ReaderFlags.DependencyObject != CurrentContext.ContextType && ReaderFlags.ClrObject != CurrentContext.ContextType))
		{
			ThrowException("ParserUnexpectedEndEle");
		}
		object element = GetCurrentObjectData();
		ElementEndInit(ref element);
		SetPropertyValueToParent(fromStartTag: false);
		ReaderFlags contextFlags = CurrentContext.ContextFlags;
		FreezeIfRequired(element);
		PopContext();
		if ((contextFlags & ReaderFlags.AddedToTree) == 0 && CurrentContext != null)
		{
			switch (CurrentContext.ContextType)
			{
			case ReaderFlags.RealizeDeferContent:
				CurrentContext.ObjectData = element;
				break;
			case ReaderFlags.ConstructorParams:
				SetConstructorParameter(element);
				break;
			}
		}
	}

	internal virtual void ReadKeyElementStartRecord(BamlKeyElementStartRecord bamlElementRecord)
	{
		Type typeFromId = MapTable.GetTypeFromId(bamlElementRecord.TypeId);
		ReaderFlags contextFlags = (ReaderFlags)((typeFromId.IsAssignableFrom(typeof(DependencyObject)) ? 4096 : 8192) | 1);
		PushContext(contextFlags, null, typeFromId, bamlElementRecord.TypeId);
	}

	internal virtual void ReadKeyElementEndRecord()
	{
		object key = ProvideValueFromMarkupExtension((MarkupExtension)GetCurrentObjectData(), ParentObjectData, null);
		SetKeyOnContext(key, "Key", ParentContext, GrandParentContext);
		PopContext();
	}

	internal virtual void ReadConstructorParameterTypeRecord(BamlConstructorParameterTypeRecord constructorParameterType)
	{
		SetConstructorParameter(MapTable.GetTypeFromId(constructorParameterType.TypeId));
	}

	internal virtual void ReadContentPropertyRecord(BamlContentPropertyRecord bamlContentPropertyRecord)
	{
		object obj = null;
		short attributeId = bamlContentPropertyRecord.AttributeId;
		object currentObjectData = GetCurrentObjectData();
		if (currentObjectData != null)
		{
			short knownTypeIdFromType = BamlMapTable.GetKnownTypeIdFromType(currentObjectData.GetType());
			if (knownTypeIdFromType < 0)
			{
				obj = KnownTypes.GetCollectionForCPA(currentObjectData, (KnownElements)(-knownTypeIdFromType));
			}
		}
		if (obj == null)
		{
			WpfPropertyDefinition wpfPropertyDefinition = new WpfPropertyDefinition(this, attributeId, currentObjectData is DependencyObject);
			if (wpfPropertyDefinition.DependencyProperty != null)
			{
				obj = ((!typeof(IList).IsAssignableFrom(wpfPropertyDefinition.PropertyType)) ? ((object)wpfPropertyDefinition.DependencyProperty) : ((object)(((DependencyObject)currentObjectData).GetValue(wpfPropertyDefinition.DependencyProperty) as IList)));
			}
			if (obj == null && wpfPropertyDefinition.PropertyInfo != null)
			{
				if (wpfPropertyDefinition.IsInternal)
				{
					obj = XamlTypeMapper.GetInternalPropertyValue(ParserContext, ParserContext.RootElement, wpfPropertyDefinition.PropertyInfo, currentObjectData) as IList;
					if (obj == null)
					{
						bool allowProtected = ParserContext.RootElement is IComponentConnector && ParserContext.RootElement == currentObjectData;
						if (!XamlTypeMapper.IsAllowedPropertySet(wpfPropertyDefinition.PropertyInfo, allowProtected, out var _))
						{
							ThrowException("ParserCantSetContentProperty", wpfPropertyDefinition.Name, wpfPropertyDefinition.PropertyInfo.ReflectedType.Name);
						}
					}
				}
				else
				{
					obj = wpfPropertyDefinition.PropertyInfo.GetValue(currentObjectData, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy, null, null, TypeConverterHelper.InvariantEnglishUS) as IList;
				}
				if (obj == null)
				{
					obj = wpfPropertyDefinition.PropertyInfo;
				}
			}
		}
		if (obj == null)
		{
			ThrowException("ParserCantGetDPOrPi", GetPropertyNameFromAttributeId(attributeId));
		}
		CurrentContext.ContentProperty = obj;
	}

	internal virtual void ReadConstructorParametersStartRecord()
	{
		PushContext(ReaderFlags.ConstructorParams, null, null, 0);
	}

	internal virtual void ReadConstructorParametersEndRecord()
	{
		Type expectedType = ParentContext.ExpectedType;
		short num = (short)(-ParentContext.ExpectedTypeId);
		object obj = null;
		ArrayList arrayList = null;
		object obj2 = null;
		bool flag = false;
		if (TraceMarkup.IsEnabled)
		{
			TraceMarkup.Trace(TraceEventType.Start, TraceMarkup.CreateMarkupExtension, expectedType);
		}
		int num2;
		if (CurrentContext.CheckFlag(ReaderFlags.SingletonConstructorParam))
		{
			obj = CurrentContext.ObjectData;
			num2 = 1;
			switch (num)
			{
			case 691:
			{
				Type type = obj as Type;
				obj2 = ((!(type != null)) ? new TypeExtension((string)obj) : new TypeExtension(type));
				flag = true;
				break;
			}
			case 603:
				obj2 = new StaticResourceExtension(obj);
				flag = true;
				break;
			case 189:
				obj2 = new DynamicResourceExtension(obj);
				flag = true;
				break;
			case 602:
				obj2 = new StaticExtension((string)obj);
				flag = true;
				break;
			case 634:
			{
				DependencyProperty dependencyProperty = obj as DependencyProperty;
				if (dependencyProperty == null)
				{
					string text = obj as string;
					Type ownerType = ParserContext.TargetType;
					dependencyProperty = XamlTypeMapper.ParsePropertyName(ParserContext, text.Trim(), ref ownerType);
					if (dependencyProperty == null)
					{
						ThrowException("ParserNoDPOnOwner", text, ownerType.FullName);
					}
				}
				obj2 = new TemplateBindingExtension(dependencyProperty);
				flag = true;
				break;
			}
			}
		}
		else
		{
			arrayList = (ArrayList)CurrentContext.ObjectData;
			num2 = arrayList.Count;
		}
		if (!flag)
		{
			XamlTypeMapper.ConstructorData constructors = XamlTypeMapper.GetConstructors(expectedType);
			ConstructorInfo[] constructors2 = constructors.Constructors;
			for (int i = 0; i < constructors2.Length; i++)
			{
				ConstructorInfo constructorInfo = constructors2[i];
				ParameterInfo[] parameters = constructors.GetParameters(i);
				if (parameters.Length != num2)
				{
					continue;
				}
				object[] array = new object[parameters.Length];
				if (num2 == 1)
				{
					ProcessConstructorParameter(parameters[0], obj, ref array[0]);
					if (num == 516)
					{
						obj2 = new RelativeSource((RelativeSourceMode)array[0]);
						flag = true;
					}
				}
				else
				{
					for (int j = 0; j < parameters.Length; j++)
					{
						ProcessConstructorParameter(parameters[j], arrayList[j], ref array[j]);
					}
				}
				if (flag)
				{
					continue;
				}
				try
				{
					obj2 = constructorInfo.Invoke(array);
					flag = true;
				}
				catch (Exception innerException)
				{
					if (CriticalExceptions.IsCriticalException(innerException) || innerException is XamlParseException)
					{
						throw;
					}
					if (innerException is TargetInvocationException ex)
					{
						innerException = ex.InnerException;
					}
					ThrowExceptionWithLine(SR.Format(SR.ParserFailedToCreateFromConstructor, constructorInfo.DeclaringType.Name), innerException);
				}
			}
		}
		if (flag)
		{
			ParentContext.ObjectData = obj2;
			ParentContext.ExpectedType = null;
			PopContext();
		}
		else
		{
			ThrowException("ParserBadConstructorParams", expectedType.Name, num2.ToString(CultureInfo.CurrentCulture));
		}
		if (TraceMarkup.IsEnabled)
		{
			TraceMarkup.Trace(TraceEventType.Stop, TraceMarkup.CreateMarkupExtension, expectedType, obj2);
		}
	}

	private void ProcessConstructorParameter(ParameterInfo paramInfo, object param, ref object paramArrayItem)
	{
		if (param is MarkupExtension markupExtension)
		{
			param = ProvideValueFromMarkupExtension(markupExtension, null, null);
		}
		if (param != null && paramInfo.ParameterType != typeof(object) && paramInfo.ParameterType != param.GetType())
		{
			TypeConverter typeConverter = XamlTypeMapper.GetTypeConverter(paramInfo.ParameterType);
			if (TraceMarkup.IsEnabled)
			{
				TraceMarkup.Trace(TraceEventType.Start, TraceMarkup.ProcessConstructorParameter, paramInfo.ParameterType, typeConverter.GetType(), param);
			}
			try
			{
				if (param is string)
				{
					param = typeConverter.ConvertFromString(TypeConvertContext, TypeConverterHelper.InvariantEnglishUS, param as string);
				}
				else if (!paramInfo.ParameterType.IsAssignableFrom(param.GetType()))
				{
					param = typeConverter.ConvertTo(TypeConvertContext, TypeConverterHelper.InvariantEnglishUS, param, paramInfo.ParameterType);
				}
			}
			catch (Exception ex)
			{
				if (CriticalExceptions.IsCriticalException(ex) || ex is XamlParseException)
				{
					throw;
				}
				ThrowExceptionWithLine(SR.Format(SR.ParserCannotConvertString, param.ToString(), paramInfo.ParameterType.FullName), ex);
			}
			if (TraceMarkup.IsEnabled)
			{
				TraceMarkup.Trace(TraceEventType.Stop, TraceMarkup.ProcessConstructorParameter, paramInfo.ParameterType, typeConverter.GetType(), param);
			}
		}
		paramArrayItem = param;
	}

	internal virtual void ReadDeferableContentStart(BamlDeferableContentStartRecord bamlRecord)
	{
		if (!(GetDictionaryFromContext(CurrentContext, toInsert: true) is ResourceDictionary))
		{
			return;
		}
		Stream baseStream = BinaryReader.BaseStream;
		long position = baseStream.Position;
		if (baseStream.Length - position < bamlRecord.ContentSize)
		{
			ThrowException("ParserDeferContentAsync");
		}
		BaseReadDeferableContentStart(bamlRecord, out var _, out var _);
		long position2 = baseStream.Position;
		int num = (int)(bamlRecord.ContentSize - position2 + position);
		if (!ParserContext.OwnsBamlStream)
		{
			byte[] buffer = new byte[num];
			if (num > 0)
			{
				PackagingUtilities.ReliableRead(BinaryReader, buffer, 0, num);
			}
			throw new NotImplementedException();
		}
		throw new NotImplementedException();
	}

	internal void BaseReadDeferableContentStart(BamlDeferableContentStartRecord bamlRecord, out ArrayList defKeyList, out List<object[]> staticResourceValuesList)
	{
		defKeyList = new ArrayList(Math.Max(5, bamlRecord.ContentSize / 400));
		staticResourceValuesList = new List<object[]>(defKeyList.Capacity);
		ArrayList arrayList = new ArrayList();
		BamlRecordType nextRecordType = GetNextRecordType();
		while (nextRecordType == BamlRecordType.DefAttributeKeyString || nextRecordType == BamlRecordType.DefAttributeKeyType || nextRecordType == BamlRecordType.KeyElementStart)
		{
			BamlRecord nextRecord = GetNextRecord();
			IBamlDictionaryKey bamlDictionaryKey = nextRecord as IBamlDictionaryKey;
			if (nextRecordType == BamlRecordType.KeyElementStart)
			{
				ReadKeyElementStartRecord((BamlKeyElementStartRecord)nextRecord);
				defKeyList.Add(nextRecord);
				bool flag = true;
				BamlRecord nextRecord2;
				while (flag && (nextRecord2 = GetNextRecord()) != null)
				{
					if (nextRecord2 is BamlKeyElementEndRecord)
					{
						object obj = GetCurrentObjectData();
						if (obj is MarkupExtension markupExtension)
						{
							obj = ProvideValueFromMarkupExtension(markupExtension, GetParentObjectData(), null);
						}
						bamlDictionaryKey.KeyObject = obj;
						PopContext();
						flag = false;
					}
					else
					{
						flag = ReadRecord(nextRecord2);
					}
				}
			}
			else if (nextRecord is BamlDefAttributeKeyStringRecord bamlDefAttributeKeyStringRecord)
			{
				bamlDefAttributeKeyStringRecord.Value = MapTable.GetStringFromStringId(bamlDefAttributeKeyStringRecord.ValueId);
				bamlDictionaryKey.KeyObject = XamlTypeMapper.GetDictionaryKey(bamlDefAttributeKeyStringRecord.Value, ParserContext);
				defKeyList.Add(bamlDefAttributeKeyStringRecord);
			}
			else if (nextRecord is BamlDefAttributeKeyTypeRecord bamlDefAttributeKeyTypeRecord)
			{
				bamlDictionaryKey.KeyObject = MapTable.GetTypeFromId(bamlDefAttributeKeyTypeRecord.TypeId);
				defKeyList.Add(bamlDefAttributeKeyTypeRecord);
			}
			else
			{
				ThrowException("ParserUnexpInBAML", nextRecord.RecordType.ToString(CultureInfo.CurrentCulture));
			}
			nextRecordType = GetNextRecordType();
			if (!ParserContext.InDeferredSection)
			{
				while (nextRecordType == BamlRecordType.StaticResourceStart || nextRecordType == BamlRecordType.OptimizedStaticResource)
				{
					BamlRecord nextRecord3 = GetNextRecord();
					if (nextRecordType == BamlRecordType.StaticResourceStart)
					{
						BamlStaticResourceStartRecord bamlElementRecord = (BamlStaticResourceStartRecord)nextRecord3;
						BaseReadElementStartRecord(bamlElementRecord);
						bool flag2 = true;
						BamlRecord nextRecord4;
						while (flag2 && (nextRecord4 = GetNextRecord()) != null)
						{
							if (nextRecord4.RecordType == BamlRecordType.StaticResourceEnd)
							{
								StaticResourceExtension value = (StaticResourceExtension)GetCurrentObjectData();
								arrayList.Add(value);
								PopContext();
								flag2 = false;
							}
							else
							{
								flag2 = ReadRecord(nextRecord4);
							}
						}
					}
					else
					{
						StaticResourceExtension value2 = (StaticResourceExtension)GetExtensionValue((IOptimizedMarkupExtension)nextRecord3, null);
						arrayList.Add(value2);
					}
					nextRecordType = GetNextRecordType();
				}
			}
			else
			{
				object[] array = ParserContext.StaticResourcesStack[ParserContext.StaticResourcesStack.Count - 1];
				while (nextRecordType == BamlRecordType.StaticResourceId)
				{
					BamlStaticResourceIdRecord bamlStaticResourceIdRecord = (BamlStaticResourceIdRecord)GetNextRecord();
					DeferredResourceReference deferredResourceReference = (DeferredResourceReference)array[bamlStaticResourceIdRecord.StaticResourceId];
					arrayList.Add(new StaticResourceHolder(deferredResourceReference.Key, deferredResourceReference));
					nextRecordType = GetNextRecordType();
				}
			}
			staticResourceValuesList.Add(arrayList.ToArray());
			arrayList.Clear();
			nextRecordType = GetNextRecordType();
		}
	}

	protected virtual void ReadStaticResourceIdRecord(BamlStaticResourceIdRecord bamlStaticResourceIdRecord)
	{
		object staticResourceFromId = GetStaticResourceFromId(bamlStaticResourceIdRecord.StaticResourceId);
		PushContext((ReaderFlags)8193, staticResourceFromId, null, 0);
		ReadElementEndRecord(fromNestedBamlRecordReader: true);
	}

	protected virtual void ReadPropertyWithStaticResourceIdRecord(BamlPropertyWithStaticResourceIdRecord bamlPropertyWithStaticResourceIdRecord)
	{
		if (CurrentContext == null || (ReaderFlags.DependencyObject != CurrentContext.ContextType && ReaderFlags.ClrObject != CurrentContext.ContextType))
		{
			ThrowException("ParserUnexpInBAML", "Property");
		}
		short attributeId = bamlPropertyWithStaticResourceIdRecord.AttributeId;
		object currentObjectData = GetCurrentObjectData();
		WpfPropertyDefinition propertyDefinition = new WpfPropertyDefinition(this, attributeId, currentObjectData is DependencyObject);
		object staticResourceFromId = GetStaticResourceFromId(bamlPropertyWithStaticResourceIdRecord.StaticResourceId);
		BaseReadOptimizedMarkupExtension(currentObjectData, attributeId, propertyDefinition, staticResourceFromId);
	}

	internal StaticResourceHolder GetStaticResourceFromId(short staticResourceId)
	{
		DeferredResourceReference deferredResourceReference = (DeferredResourceReference)ParserContext.StaticResourcesStack[ParserContext.StaticResourcesStack.Count - 1][staticResourceId];
		return new StaticResourceHolder(deferredResourceReference.Key, deferredResourceReference);
	}

	internal virtual void ReadLiteralContentRecord(BamlLiteralContentRecord bamlLiteralContentRecord)
	{
		if (CurrentContext != null)
		{
			object obj = null;
			object obj2 = null;
			if (CurrentContext.ContentProperty != null)
			{
				obj = CurrentContext.ContentProperty;
				obj2 = CurrentContext.ObjectData;
			}
			else if (CurrentContext.ContextType == ReaderFlags.PropertyComplexClr || CurrentContext.ContextType == ReaderFlags.PropertyComplexDP)
			{
				obj = CurrentContext.ObjectData;
				obj2 = ParentContext.ObjectData;
			}
			IXmlSerializable xmlSerializable = null;
			PropertyInfo propertyInfo = obj as PropertyInfo;
			if (propertyInfo != null)
			{
				if (typeof(IXmlSerializable).IsAssignableFrom(propertyInfo.PropertyType))
				{
					xmlSerializable = propertyInfo.GetValue(obj2, null) as IXmlSerializable;
				}
			}
			else if (obj is DependencyProperty dependencyProperty && typeof(IXmlSerializable).IsAssignableFrom(dependencyProperty.PropertyType))
			{
				xmlSerializable = ((DependencyObject)obj2).GetValue(dependencyProperty) as IXmlSerializable;
			}
			if (xmlSerializable != null)
			{
				FilteredXmlReader reader = new FilteredXmlReader(bamlLiteralContentRecord.Value, XmlNodeType.Element, ParserContext);
				xmlSerializable.ReadXml(reader);
				return;
			}
		}
		ThrowException("ParserUnexpInBAML", "BamlLiteralContent");
	}

	protected virtual void ReadPropertyComplexStartRecord(BamlPropertyComplexStartRecord bamlPropertyRecord)
	{
		if (CurrentContext == null || (ReaderFlags.ClrObject != CurrentContext.ContextType && ReaderFlags.DependencyObject != CurrentContext.ContextType))
		{
			ThrowException("ParserUnexpInBAML", "PropertyComplexStart");
		}
		short attributeId = bamlPropertyRecord.AttributeId;
		WpfPropertyDefinition wpfPropertyDefinition = new WpfPropertyDefinition(this, attributeId, ReaderFlags.DependencyObject == CurrentContext.ContextType);
		if (wpfPropertyDefinition.DependencyProperty != null)
		{
			PushContext(ReaderFlags.PropertyComplexDP, wpfPropertyDefinition.AttributeInfo, wpfPropertyDefinition.PropertyType, 0);
		}
		else if (wpfPropertyDefinition.PropertyInfo != null)
		{
			PushContext(ReaderFlags.PropertyComplexClr, wpfPropertyDefinition.PropertyInfo, wpfPropertyDefinition.PropertyType, 0);
		}
		else if (wpfPropertyDefinition.AttachedPropertySetter != null)
		{
			PushContext(ReaderFlags.PropertyComplexClr, wpfPropertyDefinition.AttachedPropertySetter, wpfPropertyDefinition.PropertyType, 0);
		}
		else if (wpfPropertyDefinition.AttachedPropertyGetter != null)
		{
			PushContext(ReaderFlags.PropertyComplexClr, wpfPropertyDefinition.AttachedPropertyGetter, wpfPropertyDefinition.PropertyType, 0);
		}
		else
		{
			ThrowException("ParserCantGetDPOrPi", GetPropertyNameFromAttributeId(attributeId));
		}
		CurrentContext.ElementNameOrPropertyName = wpfPropertyDefinition.Name;
	}

	protected virtual void ReadPropertyComplexEndRecord()
	{
		PopContext();
	}

	internal DependencyProperty GetCustomDependencyPropertyValue(BamlPropertyCustomRecord bamlPropertyRecord)
	{
		Type declaringType = null;
		return GetCustomDependencyPropertyValue(bamlPropertyRecord, out declaringType);
	}

	internal DependencyProperty GetCustomDependencyPropertyValue(BamlPropertyCustomRecord bamlPropertyRecord, out Type declaringType)
	{
		declaringType = null;
		DependencyProperty dependencyProperty = null;
		_ = bamlPropertyRecord.SerializerTypeId;
		if (!bamlPropertyRecord.ValueObjectSet)
		{
			short memberId = BinaryReader.ReadInt16();
			string memberName = null;
			if (bamlPropertyRecord.IsValueTypeId)
			{
				memberName = BinaryReader.ReadString();
			}
			dependencyProperty = MapTable.GetDependencyPropertyValueFromId(memberId, memberName, out declaringType);
			if (dependencyProperty == null)
			{
				ThrowException("ParserCannotConvertPropertyValue", "Property", typeof(DependencyProperty).FullName);
			}
			bamlPropertyRecord.ValueObject = dependencyProperty;
			bamlPropertyRecord.ValueObjectSet = true;
		}
		else
		{
			dependencyProperty = (DependencyProperty)bamlPropertyRecord.ValueObject;
		}
		return dependencyProperty;
	}

	internal object GetCustomValue(BamlPropertyCustomRecord bamlPropertyRecord, Type propertyType, string propertyName)
	{
		object result = null;
		if (!bamlPropertyRecord.ValueObjectSet)
		{
			Exception innerException = null;
			short serializerTypeId = bamlPropertyRecord.SerializerTypeId;
			try
			{
				result = ((serializerTypeId != 137) ? bamlPropertyRecord.GetCustomValue(BinaryReader, propertyType, serializerTypeId, this) : GetCustomDependencyPropertyValue(bamlPropertyRecord));
			}
			catch (Exception ex)
			{
				if (CriticalExceptions.IsCriticalException(ex) || ex is XamlParseException)
				{
					throw;
				}
				innerException = ex;
			}
			if (!bamlPropertyRecord.ValueObjectSet && !bamlPropertyRecord.IsRawEnumValueSet)
			{
				string message = SR.Format(SR.ParserCannotConvertPropertyValue, propertyName, propertyType.FullName);
				ThrowExceptionWithLine(message, innerException);
			}
		}
		else
		{
			result = bamlPropertyRecord.ValueObject;
		}
		return result;
	}

	protected virtual void ReadPropertyCustomRecord(BamlPropertyCustomRecord bamlPropertyRecord)
	{
		if (CurrentContext == null || (ReaderFlags.DependencyObject != CurrentContext.ContextType && ReaderFlags.ClrObject != CurrentContext.ContextType))
		{
			ThrowException("ParserUnexpInBAML", "PropertyCustom");
		}
		object obj = null;
		object currentObjectData = GetCurrentObjectData();
		short attributeId = bamlPropertyRecord.AttributeId;
		WpfPropertyDefinition wpfPropertyDefinition = new WpfPropertyDefinition(this, attributeId, currentObjectData is DependencyObject);
		if (!bamlPropertyRecord.ValueObjectSet)
		{
			try
			{
				obj = GetCustomValue(bamlPropertyRecord, wpfPropertyDefinition.PropertyType, wpfPropertyDefinition.Name);
			}
			catch (Exception ex)
			{
				if (CriticalExceptions.IsCriticalException(ex) || ex is XamlParseException)
				{
					throw;
				}
				string message = SR.Format(SR.ParserCannotConvertPropertyValue, wpfPropertyDefinition.Name, wpfPropertyDefinition.PropertyType.FullName);
				ThrowExceptionWithLine(message, ex);
			}
		}
		else
		{
			obj = bamlPropertyRecord.ValueObject;
		}
		FreezeIfRequired(obj);
		if (wpfPropertyDefinition.DependencyProperty != null)
		{
			SetDependencyValue((DependencyObject)currentObjectData, wpfPropertyDefinition.DependencyProperty, obj);
		}
		else if (wpfPropertyDefinition.PropertyInfo != null)
		{
			if (wpfPropertyDefinition.IsInternal)
			{
				if (!XamlTypeMapper.SetInternalPropertyValue(ParserContext, ParserContext.RootElement, wpfPropertyDefinition.PropertyInfo, currentObjectData, obj))
				{
					ThrowException("ParserCantSetAttribute", "property", wpfPropertyDefinition.Name, "set");
				}
			}
			else
			{
				wpfPropertyDefinition.PropertyInfo.SetValue(currentObjectData, obj, BindingFlags.Default, null, null, TypeConverterHelper.InvariantEnglishUS);
			}
		}
		else if (wpfPropertyDefinition.AttachedPropertySetter != null)
		{
			wpfPropertyDefinition.AttachedPropertySetter.Invoke(null, new object[2] { currentObjectData, obj });
		}
		else
		{
			ThrowException("ParserCantGetDPOrPi", GetPropertyNameFromAttributeId(attributeId));
		}
	}

	protected virtual void ReadPropertyRecord(BamlPropertyRecord bamlPropertyRecord)
	{
		if (CurrentContext == null || (ReaderFlags.DependencyObject != CurrentContext.ContextType && ReaderFlags.ClrObject != CurrentContext.ContextType))
		{
			ThrowException("ParserUnexpInBAML", "Property");
		}
		ReadPropertyRecordBase(bamlPropertyRecord.Value, bamlPropertyRecord.AttributeId, 0);
	}

	protected virtual void ReadPropertyConverterRecord(BamlPropertyWithConverterRecord bamlPropertyRecord)
	{
		if (CurrentContext == null || (ReaderFlags.DependencyObject != CurrentContext.ContextType && ReaderFlags.ClrObject != CurrentContext.ContextType))
		{
			ThrowException("ParserUnexpInBAML", "Property");
		}
		ReadPropertyRecordBase(bamlPropertyRecord.Value, bamlPropertyRecord.AttributeId, bamlPropertyRecord.ConverterTypeId);
	}

	protected virtual void ReadPropertyStringRecord(BamlPropertyStringReferenceRecord bamlPropertyRecord)
	{
		if (CurrentContext == null || (ReaderFlags.DependencyObject != CurrentContext.ContextType && ReaderFlags.ClrObject != CurrentContext.ContextType))
		{
			ThrowException("ParserUnexpInBAML", "Property");
		}
		string propertyValueFromStringId = GetPropertyValueFromStringId(bamlPropertyRecord.StringId);
		ReadPropertyRecordBase(propertyValueFromStringId, bamlPropertyRecord.AttributeId, 0);
	}

	private object GetInnerExtensionValue(IOptimizedMarkupExtension optimizedMarkupExtensionRecord)
	{
		object obj = null;
		short valueId = optimizedMarkupExtensionRecord.ValueId;
		if (optimizedMarkupExtensionRecord.IsValueTypeExtension)
		{
			return MapTable.GetTypeFromId(valueId);
		}
		if (optimizedMarkupExtensionRecord.IsValueStaticExtension)
		{
			return GetStaticExtensionValue(valueId);
		}
		return MapTable.GetStringFromStringId(valueId);
	}

	private object GetStaticExtensionValue(short memberId)
	{
		object result = null;
		if (memberId < 0)
		{
			short bamlId = (short)(-memberId);
			bamlId = SystemResourceKey.GetSystemResourceKeyIdFromBamlId(bamlId, out var isKey);
			result = ((!isKey) ? SystemResourceKey.GetResource(bamlId) : SystemResourceKey.GetResourceKey(bamlId));
		}
		else
		{
			BamlAttributeInfoRecord attributeInfoFromId = MapTable.GetAttributeInfoFromId(memberId);
			if (attributeInfoFromId != null)
			{
				result = new StaticExtension
				{
					MemberType = MapTable.GetTypeFromId(attributeInfoFromId.OwnerTypeId),
					Member = attributeInfoFromId.Name
				}.ProvideValue(null);
			}
		}
		return result;
	}

	internal virtual object GetExtensionValue(IOptimizedMarkupExtension optimizedMarkupExtensionRecord, string propertyName)
	{
		object obj = null;
		short valueId = optimizedMarkupExtensionRecord.ValueId;
		short extensionTypeId = optimizedMarkupExtensionRecord.ExtensionTypeId;
		switch (extensionTypeId)
		{
		case 602:
			obj = GetStaticExtensionValue(valueId);
			break;
		case 189:
			obj = new DynamicResourceExtension(GetInnerExtensionValue(optimizedMarkupExtensionRecord));
			break;
		case 603:
			obj = new StaticResourceExtension(GetInnerExtensionValue(optimizedMarkupExtensionRecord));
			break;
		}
		if (obj == null)
		{
			string parameter = string.Empty;
			switch (extensionTypeId)
			{
			case 602:
				parameter = typeof(StaticExtension).FullName;
				break;
			case 189:
				parameter = typeof(DynamicResourceExtension).FullName;
				break;
			case 603:
				parameter = typeof(StaticResourceExtension).FullName;
				break;
			}
			ThrowException("ParserCannotConvertPropertyValue", propertyName, parameter);
		}
		return obj;
	}

	protected virtual void ReadPropertyWithExtensionRecord(BamlPropertyWithExtensionRecord bamlPropertyRecord)
	{
		if (CurrentContext == null || (ReaderFlags.DependencyObject != CurrentContext.ContextType && ReaderFlags.ClrObject != CurrentContext.ContextType))
		{
			ThrowException("ParserUnexpInBAML", "Property");
		}
		short attributeId = bamlPropertyRecord.AttributeId;
		object currentObjectData = GetCurrentObjectData();
		WpfPropertyDefinition propertyDefinition = new WpfPropertyDefinition(this, attributeId, currentObjectData is DependencyObject);
		object extensionValue = GetExtensionValue(bamlPropertyRecord, propertyDefinition.Name);
		BaseReadOptimizedMarkupExtension(currentObjectData, attributeId, propertyDefinition, extensionValue);
	}

	private void BaseReadOptimizedMarkupExtension(object element, short attributeId, WpfPropertyDefinition propertyDefinition, object value)
	{
		try
		{
			if (value is MarkupExtension markupExtension)
			{
				value = ProvideValueFromMarkupExtension(markupExtension, element, propertyDefinition.DpOrPiOrMi);
				if (TraceMarkup.IsEnabled)
				{
					TraceMarkup.TraceActivityItem(TraceMarkup.ProvideValue, markupExtension, element, propertyDefinition.DpOrPiOrMi, value);
				}
			}
			if (!SetPropertyValue(element, propertyDefinition, value))
			{
				ThrowException("ParserCantGetDPOrPi", GetPropertyNameFromAttributeId(attributeId));
			}
		}
		catch (Exception innerException)
		{
			if (CriticalExceptions.IsCriticalException(innerException) || innerException is XamlParseException)
			{
				throw;
			}
			if (innerException is TargetInvocationException ex)
			{
				innerException = ex.InnerException;
			}
			string message = SR.Format(SR.ParserCannotConvertPropertyValue, propertyDefinition.Name, propertyDefinition.PropertyType.FullName);
			ThrowExceptionWithLine(message, innerException);
		}
	}

	private bool SetPropertyValue(object o, WpfPropertyDefinition propertyDefinition, object value)
	{
		bool result = true;
		if (propertyDefinition.DependencyProperty != null)
		{
			if (TraceMarkup.IsEnabled)
			{
				TraceMarkup.Trace(TraceEventType.Start, TraceMarkup.SetPropertyValue, o, propertyDefinition.DependencyProperty.Name, value);
			}
			SetDependencyValue((DependencyObject)o, propertyDefinition.DependencyProperty, value);
			if (TraceMarkup.IsEnabled)
			{
				TraceMarkup.Trace(TraceEventType.Stop, TraceMarkup.SetPropertyValue, o, propertyDefinition.DependencyProperty.Name, value);
			}
		}
		else if (propertyDefinition.PropertyInfo != null)
		{
			if (TraceMarkup.IsEnabled)
			{
				TraceMarkup.Trace(TraceEventType.Start, TraceMarkup.SetPropertyValue, o, propertyDefinition.PropertyInfo.Name, value);
			}
			if (propertyDefinition.IsInternal)
			{
				if (!XamlTypeMapper.SetInternalPropertyValue(ParserContext, ParserContext.RootElement, propertyDefinition.PropertyInfo, o, value))
				{
					ThrowException("ParserCantSetAttribute", "property", propertyDefinition.Name, "set");
				}
			}
			else
			{
				propertyDefinition.PropertyInfo.SetValue(o, value, BindingFlags.Default, null, null, TypeConverterHelper.InvariantEnglishUS);
			}
			if (TraceMarkup.IsEnabled)
			{
				TraceMarkup.Trace(TraceEventType.Stop, TraceMarkup.SetPropertyValue, o, propertyDefinition.PropertyInfo.Name, value);
			}
		}
		else if (propertyDefinition.AttachedPropertySetter != null)
		{
			if (TraceMarkup.IsEnabled)
			{
				TraceMarkup.Trace(TraceEventType.Start, TraceMarkup.SetPropertyValue, o, propertyDefinition.AttachedPropertySetter.Name, value);
			}
			propertyDefinition.AttachedPropertySetter.Invoke(null, new object[2] { o, value });
			if (TraceMarkup.IsEnabled)
			{
				TraceMarkup.Trace(TraceEventType.Stop, TraceMarkup.SetPropertyValue, o, propertyDefinition.AttachedPropertySetter.Name, value);
			}
		}
		else
		{
			result = false;
		}
		return result;
	}

	protected virtual void ReadPropertyTypeRecord(BamlPropertyTypeReferenceRecord bamlPropertyRecord)
	{
		if (CurrentContext == null || (ReaderFlags.DependencyObject != CurrentContext.ContextType && ReaderFlags.ClrObject != CurrentContext.ContextType))
		{
			ThrowException("ParserUnexpInBAML", "Property");
		}
		short attributeId = bamlPropertyRecord.AttributeId;
		object currentObjectData = GetCurrentObjectData();
		Type typeFromId = MapTable.GetTypeFromId(bamlPropertyRecord.TypeId);
		WpfPropertyDefinition propertyDefinition = new WpfPropertyDefinition(this, attributeId, currentObjectData is DependencyObject);
		try
		{
			if (!SetPropertyValue(currentObjectData, propertyDefinition, typeFromId))
			{
				ThrowException("ParserCantGetDPOrPi", GetPropertyNameFromAttributeId(attributeId));
			}
		}
		catch (Exception innerException)
		{
			if (CriticalExceptions.IsCriticalException(innerException) || innerException is XamlParseException)
			{
				throw;
			}
			if (innerException is TargetInvocationException ex)
			{
				innerException = ex.InnerException;
			}
			ThrowExceptionWithLine(SR.Format(SR.ParserCannotSetValue, currentObjectData.GetType().FullName, propertyDefinition.Name, typeFromId.Name), innerException);
		}
	}

	private void ReadPropertyRecordBase(string attribValue, short attributeId, short converterTypeId)
	{
		if (CurrentContext.CreateUsingTypeConverter)
		{
			ParserContext.XmlSpace = attribValue;
			return;
		}
		object currentObjectData = GetCurrentObjectData();
		WpfPropertyDefinition propertyDefinition = new WpfPropertyDefinition(this, attributeId, currentObjectData is DependencyObject);
		try
		{
			switch (propertyDefinition.AttributeUsage)
			{
			case BamlAttributeUsage.RuntimeName:
				DoRegisterName(attribValue, currentObjectData);
				break;
			case BamlAttributeUsage.XmlLang:
				ParserContext.XmlLang = attribValue;
				break;
			case BamlAttributeUsage.XmlSpace:
				ParserContext.XmlSpace = attribValue;
				break;
			}
			if (propertyDefinition.DependencyProperty != null)
			{
				object obj = ParseProperty((DependencyObject)currentObjectData, propertyDefinition.PropertyType, propertyDefinition.Name, propertyDefinition.DependencyProperty, attribValue, converterTypeId);
				if (obj != DependencyProperty.UnsetValue)
				{
					SetPropertyValue(currentObjectData, propertyDefinition, obj);
				}
				return;
			}
			if (propertyDefinition.PropertyInfo != null)
			{
				object obj2 = ParseProperty(currentObjectData, propertyDefinition.PropertyType, propertyDefinition.Name, propertyDefinition.PropertyInfo, attribValue, converterTypeId);
				if (obj2 != DependencyProperty.UnsetValue)
				{
					SetPropertyValue(currentObjectData, propertyDefinition, obj2);
				}
				return;
			}
			if (propertyDefinition.AttachedPropertySetter != null)
			{
				object obj3 = ParseProperty(currentObjectData, propertyDefinition.PropertyType, propertyDefinition.Name, propertyDefinition.AttachedPropertySetter, attribValue, converterTypeId);
				if (obj3 != DependencyProperty.UnsetValue)
				{
					SetPropertyValue(currentObjectData, propertyDefinition, obj3);
				}
				return;
			}
			bool isRE = false;
			object obj4 = null;
			bool isInternal = false;
			if (_componentConnector != null && _rootElement != null)
			{
				obj4 = GetREOrEiFromAttributeId(attributeId, out isInternal, out isRE);
			}
			if (obj4 != null)
			{
				Delegate @delegate;
				if (isRE)
				{
					RoutedEvent routedEvent = obj4 as RoutedEvent;
					@delegate = XamlTypeMapper.CreateDelegate(ParserContext, routedEvent.HandlerType, ParserContext.RootElement, attribValue);
					if ((object)@delegate == null)
					{
						ThrowException("ParserCantCreateDelegate", routedEvent.HandlerType.Name, attribValue);
					}
					if (currentObjectData is UIElement uIElement)
					{
						uIElement.AddHandler(routedEvent, @delegate);
					}
					else if (currentObjectData is ContentElement contentElement)
					{
						contentElement.AddHandler(routedEvent, @delegate);
					}
					else
					{
						(currentObjectData as UIElement3D).AddHandler(routedEvent, @delegate);
					}
					return;
				}
				EventInfo eventInfo = obj4 as EventInfo;
				@delegate = XamlTypeMapper.CreateDelegate(ParserContext, eventInfo.EventHandlerType, ParserContext.RootElement, attribValue);
				if ((object)@delegate == null)
				{
					ThrowException("ParserCantCreateDelegate", eventInfo.EventHandlerType.Name, attribValue);
				}
				if (isInternal)
				{
					if (!XamlTypeMapper.AddInternalEventHandler(ParserContext, ParserContext.RootElement, eventInfo, currentObjectData, @delegate))
					{
						ThrowException("ParserCantSetAttribute", "event", eventInfo.Name, "add");
					}
				}
				else
				{
					eventInfo.AddEventHandler(currentObjectData, @delegate);
				}
			}
			else
			{
				ThrowException("ParserCantGetDPOrPi", propertyDefinition.Name);
			}
		}
		catch (Exception innerException)
		{
			if (CriticalExceptions.IsCriticalException(innerException) || innerException is XamlParseException)
			{
				throw;
			}
			if (innerException is TargetInvocationException ex)
			{
				innerException = ex.InnerException;
			}
			ThrowExceptionWithLine(SR.Format(SR.ParserCannotSetValue, currentObjectData.GetType().FullName, propertyDefinition.AttributeInfo.Name, attribValue), innerException);
		}
	}

	private void DoRegisterName(string name, object element)
	{
		if (CurrentContext != null)
		{
			CurrentContext.ElementNameOrPropertyName = name;
		}
		if (ParserContext == null || ParserContext.NameScopeStack == null || ParserContext.NameScopeStack.Count == 0)
		{
			return;
		}
		INameScope nameScope = ParserContext.NameScopeStack.Pop() as INameScope;
		if (NameScope.NameScopeFromObject(element) != null && ParserContext.NameScopeStack.Count != 0)
		{
			if (ParserContext.NameScopeStack.Peek() is INameScope nameScope2)
			{
				nameScope2.RegisterName(name, element);
			}
		}
		else
		{
			nameScope.RegisterName(name, element);
		}
		ParserContext.NameScopeStack.Push(nameScope);
	}

	protected void ReadPropertyArrayStartRecord(BamlPropertyArrayStartRecord bamlPropertyArrayStartRecord)
	{
		short attributeId = bamlPropertyArrayStartRecord.AttributeId;
		object currentObjectData = GetCurrentObjectData();
		BamlCollectionHolder bamlCollectionHolder = new BamlCollectionHolder(this, currentObjectData, attributeId, needDefault: false);
		if (!bamlCollectionHolder.PropertyType.IsArray)
		{
			ThrowException("ParserNoMatchingArray", GetPropertyNameFromAttributeId(attributeId));
		}
		PushContext((ReaderFlags)20488, bamlCollectionHolder, bamlCollectionHolder.PropertyType, 0);
		CurrentContext.ElementNameOrPropertyName = bamlCollectionHolder.AttributeName;
	}

	protected virtual void ReadPropertyArrayEndRecord()
	{
		BamlCollectionHolder bamlCollectionHolder = (BamlCollectionHolder)GetCurrentObjectData();
		if (bamlCollectionHolder.Collection == null)
		{
			InitPropertyCollection(bamlCollectionHolder, CurrentContext);
		}
		ArrayExtension arrayExt = bamlCollectionHolder.ArrayExt;
		bamlCollectionHolder.Collection = ProvideValueFromMarkupExtension(arrayExt, bamlCollectionHolder, null);
		bamlCollectionHolder.SetPropertyValue();
		PopContext();
	}

	protected virtual void ReadPropertyIListStartRecord(BamlPropertyIListStartRecord bamlPropertyIListStartRecord)
	{
		short attributeId = bamlPropertyIListStartRecord.AttributeId;
		object currentObjectData = GetCurrentObjectData();
		BamlCollectionHolder bamlCollectionHolder = new BamlCollectionHolder(this, currentObjectData, attributeId);
		Type type = bamlCollectionHolder.PropertyType;
		ReaderFlags readerFlags = ReaderFlags.Unknown;
		if (typeof(IList).IsAssignableFrom(type))
		{
			readerFlags = ReaderFlags.PropertyIList;
		}
		else if (BamlRecordManager.TreatAsIAddChild(type))
		{
			readerFlags = ReaderFlags.PropertyIAddChild;
			bamlCollectionHolder.Collection = bamlCollectionHolder.DefaultCollection;
			bamlCollectionHolder.ReadOnly = true;
		}
		else if (typeof(IEnumerable).IsAssignableFrom(type) && BamlRecordManager.AsIAddChild(GetCurrentObjectData()) != null)
		{
			readerFlags = ReaderFlags.PropertyIAddChild;
			bamlCollectionHolder.Collection = CurrentContext.ObjectData;
			bamlCollectionHolder.ReadOnly = true;
			type = CurrentContext.ObjectData.GetType();
		}
		else
		{
			ThrowException("ParserReadOnlyProp", bamlCollectionHolder.PropertyDefinition.Name);
		}
		PushContext(readerFlags | ReaderFlags.CollectionHolder, bamlCollectionHolder, type, 0);
		CurrentContext.ElementNameOrPropertyName = bamlCollectionHolder.AttributeName;
	}

	protected virtual void ReadPropertyIListEndRecord()
	{
		SetCollectionPropertyValue(CurrentContext);
		PopContext();
	}

	protected virtual void ReadPropertyIDictionaryStartRecord(BamlPropertyIDictionaryStartRecord bamlPropertyIDictionaryStartRecord)
	{
		short attributeId = bamlPropertyIDictionaryStartRecord.AttributeId;
		object currentObjectData = GetCurrentObjectData();
		BamlCollectionHolder bamlCollectionHolder = new BamlCollectionHolder(this, currentObjectData, attributeId);
		PushContext((ReaderFlags)28680, bamlCollectionHolder, bamlCollectionHolder.PropertyType, 0);
		CurrentContext.ElementNameOrPropertyName = bamlCollectionHolder.AttributeName;
	}

	protected virtual void ReadPropertyIDictionaryEndRecord()
	{
		SetCollectionPropertyValue(CurrentContext);
		PopContext();
	}

	private void SetCollectionPropertyValue(ReaderContextStackData context)
	{
		BamlCollectionHolder bamlCollectionHolder = (BamlCollectionHolder)context.ObjectData;
		if (bamlCollectionHolder.Collection == null)
		{
			InitPropertyCollection(bamlCollectionHolder, context);
		}
		if (!bamlCollectionHolder.ReadOnly && bamlCollectionHolder.Collection != bamlCollectionHolder.DefaultCollection)
		{
			bamlCollectionHolder.SetPropertyValue();
		}
	}

	private void InitPropertyCollection(BamlCollectionHolder holder, ReaderContextStackData context)
	{
		if (context.ContextType == ReaderFlags.PropertyArray)
		{
			ArrayExtension arrayExtension = new ArrayExtension();
			arrayExtension.Type = context.ExpectedType.GetElementType();
			holder.Collection = arrayExtension;
		}
		else if (holder.DefaultCollection != null)
		{
			holder.Collection = holder.DefaultCollection;
		}
		else
		{
			ThrowException("ParserNullPropertyCollection", holder.PropertyDefinition.Name);
		}
		context.ExpectedType = null;
	}

	private BamlCollectionHolder GetCollectionHolderFromContext(ReaderContextStackData context, bool toInsert)
	{
		BamlCollectionHolder bamlCollectionHolder = (BamlCollectionHolder)context.ObjectData;
		if (bamlCollectionHolder.Collection == null && toInsert)
		{
			InitPropertyCollection(bamlCollectionHolder, context);
		}
		if (toInsert && bamlCollectionHolder.IsClosed)
		{
			ThrowException("ParserPropertyCollectionClosed", bamlCollectionHolder.PropertyDefinition.Name);
		}
		return bamlCollectionHolder;
	}

	protected IDictionary GetDictionaryFromContext(ReaderContextStackData context, bool toInsert)
	{
		IDictionary result = null;
		if (context != null)
		{
			if (context.CheckFlag(ReaderFlags.IDictionary))
			{
				result = (IDictionary)GetObjectDataFromContext(context);
			}
			else if (context.ContextType == ReaderFlags.PropertyIDictionary)
			{
				result = GetCollectionHolderFromContext(context, toInsert).Dictionary;
			}
		}
		return result;
	}

	private IList GetListFromContext(ReaderContextStackData context)
	{
		IList result = null;
		if (context != null)
		{
			if (context.CheckFlag(ReaderFlags.IList))
			{
				result = (IList)GetObjectDataFromContext(context);
			}
			else if (context.ContextType == ReaderFlags.PropertyIList)
			{
				result = GetCollectionHolderFromContext(context, toInsert: true).List;
			}
		}
		return result;
	}

	private IAddChild GetIAddChildFromContext(ReaderContextStackData context)
	{
		IAddChild result = null;
		if (context != null)
		{
			if (context.CheckFlag(ReaderFlags.IAddChild))
			{
				result = BamlRecordManager.AsIAddChild(context.ObjectData);
			}
			else if (context.ContextType == ReaderFlags.PropertyIAddChild)
			{
				result = BamlRecordManager.AsIAddChild(GetCollectionHolderFromContext(context, toInsert: false).Collection);
			}
		}
		return result;
	}

	private ArrayExtension GetArrayExtensionFromContext(ReaderContextStackData context)
	{
		ArrayExtension result = null;
		if (context != null)
		{
			result = context.ObjectData as ArrayExtension;
			if (context.CheckFlag(ReaderFlags.ArrayExt))
			{
				result = (ArrayExtension)context.ObjectData;
			}
			else if (context.ContextType == ReaderFlags.PropertyArray)
			{
				result = GetCollectionHolderFromContext(context, toInsert: true).ArrayExt;
			}
		}
		return result;
	}

	protected virtual void ReadDefAttributeRecord(BamlDefAttributeRecord bamlDefAttributeRecord)
	{
		bamlDefAttributeRecord.Name = MapTable.GetStringFromStringId(bamlDefAttributeRecord.NameId);
		if (bamlDefAttributeRecord.Name == "Key")
		{
			object dictionaryKey = XamlTypeMapper.GetDictionaryKey(bamlDefAttributeRecord.Value, ParserContext);
			if (dictionaryKey == null)
			{
				ThrowException("ParserNoResource", bamlDefAttributeRecord.Value);
			}
			SetKeyOnContext(dictionaryKey, bamlDefAttributeRecord.Value, CurrentContext, ParentContext);
		}
		else if (bamlDefAttributeRecord.Name == "Uid" || bamlDefAttributeRecord.NameId == BamlMapTable.UidStringId)
		{
			if (CurrentContext != null)
			{
				CurrentContext.Uid = bamlDefAttributeRecord.Value;
				if (CurrentContext.ObjectData is UIElement dependencyObject)
				{
					SetDependencyValue(dependencyObject, UIElement.UidProperty, bamlDefAttributeRecord.Value);
				}
			}
		}
		else if (bamlDefAttributeRecord.Name == "Shared")
		{
			ThrowException("ParserDefSharedOnlyInCompiled");
		}
		else if (bamlDefAttributeRecord.Name == "Name")
		{
			object currentObjectData = GetCurrentObjectData();
			if (currentObjectData != null)
			{
				DoRegisterName(bamlDefAttributeRecord.Value, currentObjectData);
			}
		}
		else
		{
			ThrowException("ParserUnknownDefAttribute", bamlDefAttributeRecord.Name);
		}
	}

	protected virtual void ReadDefAttributeKeyTypeRecord(BamlDefAttributeKeyTypeRecord bamlDefAttributeRecord)
	{
		Type typeFromId = MapTable.GetTypeFromId(bamlDefAttributeRecord.TypeId);
		if (typeFromId == null)
		{
			ThrowException("ParserNoResource", "Key");
		}
		SetKeyOnContext(typeFromId, "Key", CurrentContext, ParentContext);
	}

	private void SetKeyOnContext(object key, string attributeName, ReaderContextStackData context, ReaderContextStackData parentContext)
	{
		try
		{
			GetDictionaryFromContext(parentContext, toInsert: true);
		}
		catch (XamlParseException innerException)
		{
			if (parentContext.CheckFlag(ReaderFlags.CollectionHolder))
			{
				BamlCollectionHolder bamlCollectionHolder = (BamlCollectionHolder)parentContext.ObjectData;
				object objectData = context.ObjectData;
				if (objectData != null && objectData == bamlCollectionHolder.Dictionary)
				{
					ThrowExceptionWithLine(SR.Format(SR.ParserKeyOnExplicitDictionary, attributeName, objectData.GetType().ToString(), bamlCollectionHolder.PropertyDefinition.Name), innerException);
				}
			}
			ThrowExceptionWithLine(SR.Format(SR.ParserNoMatchingIDictionary, attributeName), innerException);
		}
		context.Key = key;
	}

	protected virtual void ReadTextRecord(BamlTextRecord bamlTextRecord)
	{
		if (bamlTextRecord is BamlTextWithIdRecord bamlTextWithIdRecord)
		{
			bamlTextWithIdRecord.Value = MapTable.GetStringFromStringId(bamlTextWithIdRecord.ValueId);
		}
		if (CurrentContext == null)
		{
			_componentConnector = null;
			_rootElement = null;
			RootList.Add(bamlTextRecord.Value);
			return;
		}
		short converterTypeId = 0;
		if (bamlTextRecord is BamlTextWithConverterRecord bamlTextWithConverterRecord)
		{
			converterTypeId = bamlTextWithConverterRecord.ConverterTypeId;
		}
		switch (CurrentContext.ContextType)
		{
		case ReaderFlags.DependencyObject:
		case ReaderFlags.ClrObject:
		{
			if (CurrentContext.CreateUsingTypeConverter)
			{
				object objectFromString2 = GetObjectFromString(CurrentContext.ExpectedType, bamlTextRecord.Value, converterTypeId);
				if (DependencyProperty.UnsetValue != objectFromString2)
				{
					CurrentContext.ObjectData = objectFromString2;
					CurrentContext.ExpectedType = null;
				}
				else
				{
					ThrowException("ParserCannotConvertString", bamlTextRecord.Value, CurrentContext.ExpectedType.FullName);
				}
				break;
			}
			object currentObjectData = GetCurrentObjectData();
			if (currentObjectData == null)
			{
				ThrowException("ParserCantCreateInstanceType", CurrentContext.ExpectedType.FullName);
			}
			IAddChild iAddChildFromContext = GetIAddChildFromContext(CurrentContext);
			if (iAddChildFromContext != null)
			{
				iAddChildFromContext.AddText(bamlTextRecord.Value);
			}
			else if (CurrentContext.ContentProperty != null)
			{
				AddToContentProperty(currentObjectData, CurrentContext.ContentProperty, bamlTextRecord.Value);
			}
			else
			{
				ThrowException("ParserIAddChildText", currentObjectData.GetType().FullName, bamlTextRecord.Value);
			}
			break;
		}
		case ReaderFlags.PropertyComplexDP:
		{
			if (null == CurrentContext.ExpectedType)
			{
				ThrowException("ParserNoComplexMulti", GetPropNameFrom(CurrentContext.ObjectData));
			}
			BamlAttributeInfoRecord bamlAttributeInfoRecord = CurrentContext.ObjectData as BamlAttributeInfoRecord;
			object obj = ParseProperty((DependencyObject)GetParentObjectData(), bamlAttributeInfoRecord.DP.PropertyType, bamlAttributeInfoRecord.DP.Name, bamlAttributeInfoRecord.DP, bamlTextRecord.Value, converterTypeId);
			if (DependencyProperty.UnsetValue != obj)
			{
				SetDependencyComplexProperty(obj);
			}
			else
			{
				ThrowException("ParserCantCreateTextComplexProp", bamlAttributeInfoRecord.OwnerType.FullName, bamlTextRecord.Value);
			}
			break;
		}
		case ReaderFlags.PropertyComplexClr:
		{
			if (null == CurrentContext.ExpectedType)
			{
				ThrowException("ParserNoComplexMulti", GetPropNameFrom(CurrentContext.ObjectData));
			}
			object objectFromString = GetObjectFromString(CurrentContext.ExpectedType, bamlTextRecord.Value, converterTypeId);
			if (DependencyProperty.UnsetValue != objectFromString)
			{
				SetClrComplexProperty(objectFromString);
			}
			else
			{
				ThrowException("ParserCantCreateTextComplexProp", CurrentContext.ExpectedType.FullName, bamlTextRecord.Value);
			}
			break;
		}
		case ReaderFlags.PropertyIAddChild:
		{
			IAddChild addChild = BamlRecordManager.AsIAddChild(GetCollectionHolderFromContext(CurrentContext, toInsert: true).Collection);
			if (addChild == null)
			{
				ThrowException("ParserNoMatchingIList", "?");
			}
			addChild.AddText(bamlTextRecord.Value);
			break;
		}
		case ReaderFlags.PropertyIList:
		{
			BamlCollectionHolder collectionHolderFromContext = GetCollectionHolderFromContext(CurrentContext, toInsert: true);
			if (collectionHolderFromContext.List == null)
			{
				ThrowException("ParserNoMatchingIList", "?");
			}
			collectionHolderFromContext.List.Add(bamlTextRecord.Value);
			break;
		}
		case ReaderFlags.ConstructorParams:
			SetConstructorParameter(bamlTextRecord.Value);
			break;
		default:
			ThrowException("ParserUnexpInBAML", "Text");
			break;
		}
	}

	protected virtual void ReadPresentationOptionsAttributeRecord(BamlPresentationOptionsAttributeRecord bamlPresentationOptionsAttributeRecord)
	{
		bamlPresentationOptionsAttributeRecord.Name = MapTable.GetStringFromStringId(bamlPresentationOptionsAttributeRecord.NameId);
		if (bamlPresentationOptionsAttributeRecord.Name == "Freeze")
		{
			bool freezeFreezables = bool.Parse(bamlPresentationOptionsAttributeRecord.Value);
			_parserContext.FreezeFreezables = freezeFreezables;
		}
		else
		{
			ThrowException("ParserUnknownPresentationOptionsAttribute", bamlPresentationOptionsAttributeRecord.Name);
		}
	}

	private void SetDependencyComplexProperty(object o)
	{
		object parentObjectData = GetParentObjectData();
		BamlAttributeInfoRecord attribInfo = (BamlAttributeInfoRecord)GetCurrentObjectData();
		SetDependencyComplexProperty(parentObjectData, attribInfo, o);
	}

	private void SetDependencyComplexProperty(object currentTarget, BamlAttributeInfoRecord attribInfo, object o)
	{
		DependencyProperty dependencyProperty = ((currentTarget is DependencyObject) ? attribInfo.DP : null);
		PropertyInfo propInfo = attribInfo.PropInfo;
		MethodInfo methodInfo = null;
		try
		{
			if (o is MarkupExtension markupExtension)
			{
				o = ProvideValueFromMarkupExtension(markupExtension, currentTarget, dependencyProperty);
			}
			Type propertyType = null;
			if (dependencyProperty != null)
			{
				propertyType = dependencyProperty.PropertyType;
			}
			else if (propInfo != null)
			{
				propertyType = propInfo.PropertyType;
			}
			else
			{
				if (attribInfo.AttachedPropertySetter == null)
				{
					XamlTypeMapper.UpdateAttachedPropertySetter(attribInfo);
				}
				methodInfo = attribInfo.AttachedPropertySetter;
				if (methodInfo != null)
				{
					propertyType = methodInfo.GetParameters()[1].ParameterType;
				}
			}
			o = OptionallyMakeNullable(propertyType, o, attribInfo.Name);
			if (dependencyProperty != null)
			{
				SetDependencyValue((DependencyObject)currentTarget, dependencyProperty, o);
			}
			else if (propInfo != null)
			{
				propInfo.SetValue(currentTarget, o, BindingFlags.Default, null, null, TypeConverterHelper.InvariantEnglishUS);
			}
			else if (methodInfo != null)
			{
				methodInfo.Invoke(null, new object[2] { currentTarget, o });
			}
		}
		catch (Exception innerException)
		{
			if (CriticalExceptions.IsCriticalException(innerException) || innerException is XamlParseException)
			{
				throw;
			}
			if (innerException is TargetInvocationException ex)
			{
				innerException = ex.InnerException;
			}
			ThrowExceptionWithLine(SR.Format(SR.ParserCannotSetValue, currentTarget.GetType().FullName, attribInfo.Name, o), innerException);
		}
		CurrentContext.ExpectedType = null;
	}

	internal static bool IsNullable(Type t)
	{
		if (t.IsGenericType)
		{
			return t.GetGenericTypeDefinition() == NullableType;
		}
		return false;
	}

	internal object OptionallyMakeNullable(Type propertyType, object o, string propName)
	{
		object o2 = o;
		if (!TryOptionallyMakeNullable(propertyType, propName, ref o2))
		{
			ThrowException("ParserBadNullableType", propName, propertyType.GetGenericArguments()[0].Name, o.GetType().FullName);
		}
		return o2;
	}

	internal static bool TryOptionallyMakeNullable(Type propertyType, string propName, ref object o)
	{
		if (o != null && IsNullable(propertyType) && !(o is Expression) && !(o is MarkupExtension) && propertyType.GetGenericArguments()[0] != o.GetType())
		{
			return false;
		}
		return true;
	}

	internal virtual void SetClrComplexPropertyCore(object parentObject, object value, MemberInfo memberInfo)
	{
		if (value is MarkupExtension markupExtension)
		{
			value = ProvideValueFromMarkupExtension(markupExtension, parentObject, memberInfo);
		}
		if (memberInfo is PropertyInfo)
		{
			PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
			value = OptionallyMakeNullable(propertyInfo.PropertyType, value, propertyInfo.Name);
			propertyInfo.SetValue(parentObject, value, BindingFlags.Default, null, null, TypeConverterHelper.InvariantEnglishUS);
		}
		else
		{
			MethodInfo methodInfo = (MethodInfo)memberInfo;
			value = OptionallyMakeNullable(methodInfo.GetParameters()[1].ParameterType, value, methodInfo.Name.Substring("Set".Length));
			methodInfo.Invoke(null, new object[2] { parentObject, value });
		}
	}

	private void SetClrComplexProperty(object o)
	{
		MemberInfo memberInfo = (MemberInfo)GetCurrentObjectData();
		object parentObjectData = GetParentObjectData();
		SetClrComplexProperty(parentObjectData, memberInfo, o);
	}

	private void SetClrComplexProperty(object parentObject, MemberInfo memberInfo, object o)
	{
		try
		{
			SetClrComplexPropertyCore(parentObject, o, memberInfo);
		}
		catch (Exception innerException)
		{
			if (CriticalExceptions.IsCriticalException(innerException) || innerException is XamlParseException)
			{
				throw;
			}
			if (innerException is TargetInvocationException ex)
			{
				innerException = ex.InnerException;
			}
			ThrowExceptionWithLine(SR.Format(SR.ParserCannotSetValue, parentObject.GetType().FullName, memberInfo.Name, o), innerException);
		}
		CurrentContext.ExpectedType = null;
	}

	private void SetConstructorParameter(object o)
	{
		if (o is MarkupExtension markupExtension)
		{
			o = ProvideValueFromMarkupExtension(markupExtension, null, null);
		}
		if (CurrentContext.ObjectData == null)
		{
			CurrentContext.ObjectData = o;
			CurrentContext.SetFlag(ReaderFlags.SingletonConstructorParam);
		}
		else if (CurrentContext.CheckFlag(ReaderFlags.SingletonConstructorParam))
		{
			ArrayList arrayList = new ArrayList(2);
			arrayList.Add(CurrentContext.ObjectData);
			arrayList.Add(o);
			CurrentContext.ObjectData = arrayList;
			CurrentContext.ClearFlag(ReaderFlags.SingletonConstructorParam);
		}
		else
		{
			((ArrayList)CurrentContext.ObjectData).Add(o);
		}
	}

	protected void SetXmlnsOnCurrentObject(BamlXmlnsPropertyRecord xmlnsRecord)
	{
		if (CurrentContext.ObjectData is DependencyObject dependencyObject)
		{
			XmlnsDictionary xmlnsDictionary = XmlAttributeProperties.GetXmlnsDictionary(dependencyObject);
			if (xmlnsDictionary != null)
			{
				xmlnsDictionary.Unseal();
				xmlnsDictionary[xmlnsRecord.Prefix] = xmlnsRecord.XmlNamespace;
				xmlnsDictionary.Seal();
			}
			else
			{
				xmlnsDictionary = new XmlnsDictionary();
				xmlnsDictionary[xmlnsRecord.Prefix] = xmlnsRecord.XmlNamespace;
				xmlnsDictionary.Seal();
				XmlAttributeProperties.SetXmlnsDictionary(dependencyObject, xmlnsDictionary);
			}
		}
	}

	internal object ParseProperty(object element, Type propertyType, string propertyName, object dpOrPi, string attribValue, short converterTypeId)
	{
		object obj = null;
		try
		{
			obj = XamlTypeMapper.ParseProperty(element, propertyType, propertyName, dpOrPi, TypeConvertContext, ParserContext, attribValue, converterTypeId);
			FreezeIfRequired(obj);
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex) || ex is XamlParseException)
			{
				throw;
			}
			ThrowPropertyParseError(ex, propertyName, attribValue, element, propertyType);
		}
		if (DependencyProperty.UnsetValue == obj)
		{
			ThrowException("ParserNullReturned", propertyName, attribValue);
		}
		return obj;
	}

	private void ThrowPropertyParseError(Exception e, string propertyName, string attribValue, object element, Type propertyType)
	{
		string empty = string.Empty;
		empty = ((FindResourceInParserStack(attribValue.Trim(), allowDeferredResourceReference: false, mustReturnDeferredResourceReference: false) != DependencyProperty.UnsetValue) ? SR.Format(SR.ParserErrorParsingAttribType, propertyName, attribValue) : ((!(propertyType == typeof(Type))) ? SR.Format(SR.ParserErrorParsingAttrib, propertyName, attribValue, propertyType.Name) : SR.Format(SR.ParserErrorParsingAttribType, propertyName, attribValue)));
		ThrowExceptionWithLine(empty, e);
	}

	private object GetObjectFromString(Type type, string s, short converterTypeId)
	{
		_ = DependencyProperty.UnsetValue;
		return ParserContext.XamlTypeMapper.ParseProperty(null, type, string.Empty, null, TypeConvertContext, ParserContext, s, converterTypeId);
	}

	private static object Lookup(IDictionary dictionary, object key, bool allowDeferredResourceReference, bool mustReturnDeferredResourceReference)
	{
		if (allowDeferredResourceReference && dictionary is ResourceDictionary resourceDictionary)
		{
			bool canCache;
			return resourceDictionary.FetchResource(key, allowDeferredResourceReference, mustReturnDeferredResourceReference, out canCache);
		}
		if (!mustReturnDeferredResourceReference)
		{
			return dictionary[key];
		}
		return new DeferredResourceReferenceHolder(key, dictionary[key]);
	}

	internal object FindResourceInParserStack(object resourceNameObject, bool allowDeferredResourceReference, bool mustReturnDeferredResourceReference)
	{
		object obj = DependencyProperty.UnsetValue;
		ParserStack parserStack = ReaderContextStack;
		BamlRecordReader bamlRecordReader = this;
		while (parserStack != null)
		{
			for (int num = parserStack.Count - 1; num >= 0; num--)
			{
				ReaderContextStackData readerContextStackData = (ReaderContextStackData)parserStack[num];
				IDictionary dictionaryFromContext = GetDictionaryFromContext(readerContextStackData, toInsert: false);
				if (dictionaryFromContext != null && dictionaryFromContext.Contains(resourceNameObject))
				{
					obj = Lookup(dictionaryFromContext, resourceNameObject, allowDeferredResourceReference, mustReturnDeferredResourceReference);
				}
				else if (readerContextStackData.ContextType == ReaderFlags.DependencyObject)
				{
					Helper.DowncastToFEorFCE((DependencyObject)readerContextStackData.ObjectData, out var fe, out var fce, throwIfNeither: false);
					if (fe != null)
					{
						obj = fe.FindResourceOnSelf(resourceNameObject, allowDeferredResourceReference, mustReturnDeferredResourceReference);
					}
					else if (fce != null)
					{
						obj = fce.FindResourceOnSelf(resourceNameObject, allowDeferredResourceReference, mustReturnDeferredResourceReference);
					}
				}
				else if (readerContextStackData.CheckFlag(ReaderFlags.StyleObject))
				{
					obj = ((Style)readerContextStackData.ObjectData).FindResource(resourceNameObject, allowDeferredResourceReference, mustReturnDeferredResourceReference);
				}
				else if (readerContextStackData.CheckFlag(ReaderFlags.FrameworkTemplateObject))
				{
					obj = ((FrameworkTemplate)readerContextStackData.ObjectData).FindResource(resourceNameObject, allowDeferredResourceReference, mustReturnDeferredResourceReference);
				}
				if (obj != DependencyProperty.UnsetValue)
				{
					return obj;
				}
			}
			bool flag = false;
			while (bamlRecordReader._previousBamlRecordReader != null)
			{
				bamlRecordReader = bamlRecordReader._previousBamlRecordReader;
				if (bamlRecordReader.ReaderContextStack != parserStack)
				{
					parserStack = bamlRecordReader.ReaderContextStack;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				parserStack = null;
			}
		}
		return DependencyProperty.UnsetValue;
	}

	private object FindResourceInRootOrAppOrTheme(object resourceNameObject, bool allowDeferredResourceReference, bool mustReturnDeferredResourceReference)
	{
		object source;
		object obj = (SystemResources.IsSystemResourcesParsing ? SystemResources.FindResourceInternal(resourceNameObject, allowDeferredResourceReference, mustReturnDeferredResourceReference) : FrameworkElement.FindResourceFromAppOrSystem(resourceNameObject, out source, disableThrowOnResourceNotFound: false, allowDeferredResourceReference, mustReturnDeferredResourceReference));
		if (obj != null)
		{
			return obj;
		}
		return DependencyProperty.UnsetValue;
	}

	internal object FindResourceInParentChain(object resourceNameObject, bool allowDeferredResourceReference, bool mustReturnDeferredResourceReference)
	{
		object obj = FindResourceInParserStack(resourceNameObject, allowDeferredResourceReference, mustReturnDeferredResourceReference);
		if (obj == DependencyProperty.UnsetValue)
		{
			obj = FindResourceInRootOrAppOrTheme(resourceNameObject, allowDeferredResourceReference, mustReturnDeferredResourceReference);
		}
		if (obj == DependencyProperty.UnsetValue && mustReturnDeferredResourceReference)
		{
			obj = new DeferredResourceReferenceHolder(resourceNameObject, DependencyProperty.UnsetValue);
		}
		return obj;
	}

	internal object LoadResource(string resourceNameString)
	{
		string keyString = resourceNameString.Substring(1, resourceNameString.Length - 2);
		object dictionaryKey = XamlTypeMapper.GetDictionaryKey(keyString, ParserContext);
		if (dictionaryKey == null)
		{
			ThrowException("ParserNoResource", resourceNameString);
		}
		object obj = FindResourceInParentChain(dictionaryKey, allowDeferredResourceReference: false, mustReturnDeferredResourceReference: false);
		if (obj == DependencyProperty.UnsetValue)
		{
			ThrowException("ParserNoResource", "{" + dictionaryKey.ToString() + "}");
		}
		return obj;
	}

	private object GetObjectDataFromContext(ReaderContextStackData context)
	{
		if (context.ObjectData == null && null != context.ExpectedType)
		{
			context.ObjectData = CreateInstanceFromType(context.ExpectedType, context.ExpectedTypeId, throwOnFail: true);
			if (context.ObjectData == null)
			{
				ThrowException("ParserCantCreateInstanceType", context.ExpectedType.FullName);
			}
			context.ExpectedType = null;
			ElementInitialize(context.ObjectData, null);
		}
		return context.ObjectData;
	}

	internal object GetCurrentObjectData()
	{
		return GetObjectDataFromContext(CurrentContext);
	}

	protected object GetParentObjectData()
	{
		return GetObjectDataFromContext(ParentContext);
	}

	internal void PushContext(ReaderFlags contextFlags, object contextData, Type expectedType, short expectedTypeId)
	{
		PushContext(contextFlags, contextData, expectedType, expectedTypeId, createUsingTypeConverter: false);
	}

	internal void PushContext(ReaderFlags contextFlags, object contextData, Type expectedType, short expectedTypeId, bool createUsingTypeConverter)
	{
		ReaderContextStackData readerContextStackData;
		lock (_stackDataFactoryCache)
		{
			if (_stackDataFactoryCache.Count == 0)
			{
				readerContextStackData = new ReaderContextStackData();
			}
			else
			{
				readerContextStackData = _stackDataFactoryCache[_stackDataFactoryCache.Count - 1];
				_stackDataFactoryCache.RemoveAt(_stackDataFactoryCache.Count - 1);
			}
		}
		readerContextStackData.ContextFlags = contextFlags;
		readerContextStackData.ObjectData = contextData;
		readerContextStackData.ExpectedType = expectedType;
		readerContextStackData.ExpectedTypeId = expectedTypeId;
		readerContextStackData.CreateUsingTypeConverter = createUsingTypeConverter;
		ReaderContextStack.Push(readerContextStackData);
		ParserContext.PushScope();
		INameScope nameScope = NameScope.NameScopeFromObject(contextData);
		if (nameScope != null)
		{
			ParserContext.NameScopeStack.Push(nameScope);
		}
	}

	internal void PopContext()
	{
		ReaderContextStackData readerContextStackData = (ReaderContextStackData)ReaderContextStack.Pop();
		if (NameScope.NameScopeFromObject(readerContextStackData.ObjectData) != null)
		{
			ParserContext.NameScopeStack.Pop();
		}
		ParserContext.PopScope();
		readerContextStackData.ClearData();
		lock (_stackDataFactoryCache)
		{
			_stackDataFactoryCache.Add(readerContextStackData);
		}
	}

	private Uri GetBaseUri()
	{
		Uri uri = ParserContext.BaseUri;
		if (uri == null)
		{
			uri = MS.Internal.Utility.BindUriHelper.BaseUri;
		}
		else if (!uri.IsAbsoluteUri)
		{
			uri = new Uri(MS.Internal.Utility.BindUriHelper.BaseUri, uri);
		}
		return uri;
	}

	private bool ElementInitialize(object element, string name)
	{
		bool result = false;
		if (element is ISupportInitialize supportInitialize)
		{
			if (TraceMarkup.IsEnabled)
			{
				TraceMarkup.Trace(TraceEventType.Start, TraceMarkup.BeginInit, supportInitialize);
			}
			supportInitialize.BeginInit();
			result = true;
			if (TraceMarkup.IsEnabled)
			{
				TraceMarkup.Trace(TraceEventType.Stop, TraceMarkup.BeginInit, supportInitialize);
			}
		}
		if (name != null)
		{
			DoRegisterName(name, element);
		}
		if (element is IUriContext uriContext)
		{
			uriContext.BaseUri = GetBaseUri();
		}
		else if (element is Application)
		{
			((Application)element).ApplicationMarkupBaseUri = GetBaseUri();
		}
		if (element is UIElement uIElement)
		{
			uIElement.SetPersistId(++_persistId);
		}
		if (CurrentContext == null)
		{
			IComponentConnector componentConnector = null;
			if (_componentConnector == null)
			{
				componentConnector = (_componentConnector = element as IComponentConnector);
				if (_componentConnector != null)
				{
					if (ParserContext.RootElement == null)
					{
						ParserContext.RootElement = element;
					}
					_componentConnector.Connect(0, element);
				}
			}
			_rootElement = element;
			DependencyObject dependencyObject = element as DependencyObject;
			if (!(element is INameScope) && ParserContext.NameScopeStack.Count == 0 && dependencyObject != null)
			{
				NameScope nameScope = null;
				if (componentConnector != null)
				{
					nameScope = NameScope.GetNameScope(dependencyObject) as NameScope;
				}
				if (nameScope == null)
				{
					nameScope = new NameScope();
					NameScope.SetNameScope(dependencyObject, nameScope);
				}
			}
			if (dependencyObject != null)
			{
				Uri baseUri = GetBaseUri();
				SetDependencyValue(dependencyObject, BaseUriHelper.BaseUriProperty, baseUri);
			}
		}
		return result;
	}

	private void ElementEndInit(ref object element)
	{
		try
		{
			if (element is ISupportInitialize supportInitialize)
			{
				if (TraceMarkup.IsEnabled)
				{
					TraceMarkup.Trace(TraceEventType.Start, TraceMarkup.EndInit, supportInitialize);
				}
				supportInitialize.EndInit();
				if (TraceMarkup.IsEnabled)
				{
					TraceMarkup.Trace(TraceEventType.Stop, TraceMarkup.EndInit, supportInitialize);
				}
			}
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex) || ex is XamlParseException)
			{
				throw;
			}
			ReaderContextStackData parentContext = ParentContext;
			ReaderFlags readerFlags = parentContext?.ContextType ?? ReaderFlags.Unknown;
			if ((readerFlags == ReaderFlags.PropertyComplexClr || readerFlags == ReaderFlags.PropertyComplexDP || readerFlags == ReaderFlags.PropertyIList || readerFlags == ReaderFlags.PropertyIDictionary || readerFlags == ReaderFlags.PropertyArray || readerFlags == ReaderFlags.PropertyIAddChild) && GrandParentObjectData is IProvidePropertyFallback providePropertyFallback)
			{
				string elementNameOrPropertyName = parentContext.ElementNameOrPropertyName;
				if (providePropertyFallback.CanProvidePropertyFallback(elementNameOrPropertyName))
				{
					element = providePropertyFallback.ProvidePropertyFallback(elementNameOrPropertyName, ex);
					CurrentContext.ObjectData = element;
					return;
				}
			}
			ThrowExceptionWithLine(SR.ParserFailedEndInit, ex);
		}
	}

	private void SetPropertyValueToParent(bool fromStartTag)
	{
		SetPropertyValueToParent(fromStartTag, out var _);
	}

	private void SetPropertyValueToParent(bool fromStartTag, out bool isMarkupExtension)
	{
		isMarkupExtension = false;
		object p = null;
		ReaderContextStackData currentContext = CurrentContext;
		ReaderContextStackData parentContext = ParentContext;
		if (currentContext == null || !currentContext.NeedToAddToTree || (ReaderFlags.DependencyObject != currentContext.ContextType && ReaderFlags.ClrObject != currentContext.ContextType))
		{
			return;
		}
		object obj = null;
		try
		{
			obj = GetCurrentObjectData();
			FreezeIfRequired(obj);
			if (parentContext == null)
			{
				if (RootList.Count == 0)
				{
					RootList.Add(obj);
				}
				currentContext.MarkAddedToTree();
				return;
			}
			if (CheckExplicitCollectionTag(ref isMarkupExtension))
			{
				currentContext.MarkAddedToTree();
				return;
			}
			object parentObjectData = GetParentObjectData();
			IDictionary dictionaryFromContext = GetDictionaryFromContext(parentContext, toInsert: true);
			if (dictionaryFromContext != null)
			{
				if (!fromStartTag)
				{
					obj = GetElementValue(obj, dictionaryFromContext, null, ref isMarkupExtension);
					if (currentContext.Key == null)
					{
						ThrowException("ParserNoDictionaryKey");
					}
					dictionaryFromContext.Add(currentContext.Key, obj);
					currentContext.MarkAddedToTree();
				}
				return;
			}
			IList listFromContext = GetListFromContext(parentContext);
			if (listFromContext != null)
			{
				obj = GetElementValue(obj, listFromContext, null, ref isMarkupExtension);
				listFromContext.Add(obj);
				currentContext.MarkAddedToTree();
				return;
			}
			ArrayExtension arrayExtensionFromContext = GetArrayExtensionFromContext(parentContext);
			if (arrayExtensionFromContext != null)
			{
				obj = GetElementValue(obj, arrayExtensionFromContext, null, ref isMarkupExtension);
				arrayExtensionFromContext.AddChild(obj);
				if (TraceMarkup.IsEnabled)
				{
					TraceMarkup.Trace(TraceEventType.Stop, TraceMarkup.AddValueToArray, p, parentContext.ElementNameOrPropertyName, obj);
				}
				currentContext.MarkAddedToTree();
				return;
			}
			IAddChild iAddChildFromContext = GetIAddChildFromContext(parentContext);
			if (iAddChildFromContext != null)
			{
				obj = GetElementValue(obj, iAddChildFromContext, null, ref isMarkupExtension);
				if (obj is string text)
				{
					iAddChildFromContext.AddText(text);
				}
				else
				{
					iAddChildFromContext.AddChild(obj);
				}
				if (TraceMarkup.IsEnabled)
				{
					TraceMarkup.Trace(TraceEventType.Stop, TraceMarkup.AddValueToAddChild, p, obj);
				}
				currentContext.MarkAddedToTree();
				return;
			}
			object contentProperty = parentContext.ContentProperty;
			if (contentProperty != null)
			{
				obj = GetElementValue(obj, parentContext.ObjectData, contentProperty, ref isMarkupExtension);
				AddToContentProperty(parentObjectData, contentProperty, obj);
				currentContext.MarkAddedToTree();
				return;
			}
			if (parentContext.ContextType == ReaderFlags.PropertyComplexClr)
			{
				object objectDataFromContext = GetObjectDataFromContext(GrandParentContext);
				MemberInfo memberInfo = (MemberInfo)GetParentObjectData();
				SetClrComplexProperty(objectDataFromContext, memberInfo, obj);
				currentContext.MarkAddedToTree();
				return;
			}
			if (parentContext.ContextType == ReaderFlags.PropertyComplexDP)
			{
				object objectDataFromContext2 = GetObjectDataFromContext(GrandParentContext);
				BamlAttributeInfoRecord attribInfo = (BamlAttributeInfoRecord)GetParentObjectData();
				SetDependencyComplexProperty(objectDataFromContext2, attribInfo, obj);
				currentContext.MarkAddedToTree();
				return;
			}
			Type parentType = GetParentType();
			string text2 = ((parentType == null) ? string.Empty : parentType.FullName);
			if (obj == null)
			{
				ThrowException("ParserCannotAddAnyChildren", text2);
			}
			else
			{
				ThrowException("ParserCannotAddAnyChildren2", text2, obj.GetType().FullName);
			}
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex) || ex is XamlParseException)
			{
				throw;
			}
			Type parentType2 = GetParentType();
			string text3 = ((parentType2 == null) ? string.Empty : parentType2.FullName);
			if (obj == null)
			{
				ThrowException("ParserCannotAddAnyChildren", text3);
			}
			else
			{
				ThrowException("ParserCannotAddAnyChildren2", text3, obj.GetType().FullName);
			}
		}
	}

	private Type GetParentType()
	{
		ReaderContextStackData parentContext = ParentContext;
		object obj = GetParentObjectData();
		if (parentContext.CheckFlag(ReaderFlags.CollectionHolder))
		{
			obj = ((BamlCollectionHolder)obj).Collection;
		}
		if (obj != null)
		{
			return obj.GetType();
		}
		if (parentContext.ExpectedType != null)
		{
			return parentContext.ExpectedType;
		}
		return null;
	}

	private object GetElementValue(object element, object parent, object contentProperty, ref bool isMarkupExtension)
	{
		if (element is MarkupExtension markupExtension)
		{
			isMarkupExtension = true;
			element = ProvideValueFromMarkupExtension(markupExtension, parent, contentProperty);
			CurrentContext.ObjectData = element;
		}
		return element;
	}

	private bool CheckExplicitCollectionTag(ref bool isMarkupExtension)
	{
		bool result = false;
		ReaderContextStackData parentContext = ParentContext;
		if (parentContext != null && parentContext.CheckFlag(ReaderFlags.CollectionHolder) && parentContext.ExpectedType != null)
		{
			BamlCollectionHolder bamlCollectionHolder = (BamlCollectionHolder)parentContext.ObjectData;
			if (!bamlCollectionHolder.IsClosed && !bamlCollectionHolder.ReadOnly)
			{
				ReaderContextStackData currentContext = CurrentContext;
				object obj = currentContext.ObjectData;
				Type c;
				if (currentContext.CheckFlag(ReaderFlags.ArrayExt))
				{
					c = ((ArrayExtension)obj).Type.MakeArrayType();
					isMarkupExtension = false;
				}
				else
				{
					obj = GetElementValue(obj, GrandParentObjectData, bamlCollectionHolder.PropertyDefinition.DependencyProperty, ref isMarkupExtension);
					c = obj?.GetType();
				}
				if (isMarkupExtension || parentContext.ExpectedType.IsAssignableFrom(c))
				{
					bamlCollectionHolder.Collection = obj;
					bamlCollectionHolder.IsClosed = true;
					parentContext.ExpectedType = null;
					result = true;
				}
			}
		}
		return result;
	}

	private void AddToContentProperty(object container, object contentProperty, object value)
	{
		IList list = contentProperty as IList;
		object p = null;
		try
		{
			if (list != null)
			{
				if (TraceMarkup.IsEnabled)
				{
					TraceMarkup.Trace(TraceEventType.Start, TraceMarkup.AddValueToList, p, string.Empty, value);
				}
				list.Add(value);
				if (TraceMarkup.IsEnabled)
				{
					TraceMarkup.Trace(TraceEventType.Stop, TraceMarkup.AddValueToList, p, string.Empty, value);
				}
			}
			else if (contentProperty is DependencyProperty dependencyProperty)
			{
				DependencyObject dependencyObject = container as DependencyObject;
				if (dependencyObject == null)
				{
					ThrowException("ParserParentDO", value.ToString());
				}
				if (TraceMarkup.IsEnabled)
				{
					TraceMarkup.Trace(TraceEventType.Start, TraceMarkup.SetPropertyValue, p, dependencyProperty.Name, value);
				}
				SetDependencyValue(dependencyObject, dependencyProperty, value);
				if (TraceMarkup.IsEnabled)
				{
					TraceMarkup.Trace(TraceEventType.Stop, TraceMarkup.SetPropertyValue, p, dependencyProperty.Name, value);
				}
			}
			else
			{
				PropertyInfo propertyInfo = contentProperty as PropertyInfo;
				if (propertyInfo != null)
				{
					if (TraceMarkup.IsEnabled)
					{
						TraceMarkup.Trace(TraceEventType.Start, TraceMarkup.SetPropertyValue, p, propertyInfo.Name, value);
					}
					if (!XamlTypeMapper.SetInternalPropertyValue(ParserContext, ParserContext.RootElement, propertyInfo, container, value))
					{
						ThrowException("ParserCantSetContentProperty", propertyInfo.Name, propertyInfo.ReflectedType.Name);
					}
					if (TraceMarkup.IsEnabled)
					{
						TraceMarkup.Trace(TraceEventType.Stop, TraceMarkup.SetPropertyValue, p, propertyInfo.Name, value);
					}
				}
			}
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex) || ex is XamlParseException)
			{
				throw;
			}
			ThrowExceptionWithLine(SR.Format(SR.ParserCannotAddChild, value.GetType().Name, container.GetType().Name), ex);
		}
		if (TraceMarkup.IsEnabled)
		{
			TraceMarkup.Trace(TraceEventType.Stop, TraceMarkup.SetCPA, p, value);
		}
	}

	internal string GetPropertyNameFromAttributeId(short id)
	{
		if (MapTable != null)
		{
			return MapTable.GetAttributeNameFromId(id);
		}
		return null;
	}

	internal string GetPropertyValueFromStringId(short id)
	{
		string result = null;
		if (MapTable != null)
		{
			result = MapTable.GetStringFromStringId(id);
		}
		return result;
	}

	private XamlSerializer CreateSerializer(BamlTypeInfoWithSerializerRecord typeWithSerializerInfo)
	{
		if (typeWithSerializerInfo.SerializerTypeId < 0)
		{
			return (XamlSerializer)MapTable.CreateKnownTypeFromId(typeWithSerializerInfo.SerializerTypeId);
		}
		if (typeWithSerializerInfo.SerializerType == null)
		{
			typeWithSerializerInfo.SerializerType = MapTable.GetTypeFromId(typeWithSerializerInfo.SerializerTypeId);
		}
		return (XamlSerializer)CreateInstanceFromType(typeWithSerializerInfo.SerializerType, typeWithSerializerInfo.SerializerTypeId, throwOnFail: false);
	}

	internal object GetREOrEiFromAttributeId(short id, out bool isInternal, out bool isRE)
	{
		object obj = null;
		isRE = true;
		isInternal = false;
		BamlAttributeInfoRecord bamlAttributeInfoRecord = null;
		if (MapTable != null)
		{
			bamlAttributeInfoRecord = MapTable.GetAttributeInfoFromId(id);
			if (bamlAttributeInfoRecord != null)
			{
				obj = bamlAttributeInfoRecord.Event;
				if (obj == null)
				{
					obj = bamlAttributeInfoRecord.EventInfo;
					if (obj == null)
					{
						bamlAttributeInfoRecord.Event = MapTable.GetRoutedEvent(bamlAttributeInfoRecord);
						obj = bamlAttributeInfoRecord.Event;
						if (obj == null)
						{
							Type type = GetCurrentObjectData().GetType();
							if (ReflectionHelper.IsPublicType(type))
							{
								bamlAttributeInfoRecord.EventInfo = ParserContext.XamlTypeMapper.GetClrEventInfo(type, bamlAttributeInfoRecord.Name);
							}
							if (bamlAttributeInfoRecord.EventInfo == null)
							{
								bamlAttributeInfoRecord.EventInfo = type.GetEvent(bamlAttributeInfoRecord.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
								if (bamlAttributeInfoRecord.EventInfo != null)
								{
									bamlAttributeInfoRecord.IsInternal = true;
								}
							}
							obj = bamlAttributeInfoRecord.EventInfo;
							isRE = false;
						}
					}
					else
					{
						isRE = false;
					}
				}
			}
		}
		if (bamlAttributeInfoRecord != null)
		{
			isInternal = bamlAttributeInfoRecord.IsInternal;
		}
		return obj;
	}

	private string GetPropNameFrom(object PiOrAttribInfo)
	{
		if (PiOrAttribInfo is BamlAttributeInfoRecord bamlAttributeInfoRecord)
		{
			return bamlAttributeInfoRecord.OwnerType.Name + "." + bamlAttributeInfoRecord.Name;
		}
		PropertyInfo propertyInfo = PiOrAttribInfo as PropertyInfo;
		if (propertyInfo != null)
		{
			return propertyInfo.DeclaringType.Name + "." + propertyInfo.Name;
		}
		return string.Empty;
	}

	protected void ThrowException(string id)
	{
		ThrowExceptionWithLine(SR.GetResourceString(id), null);
	}

	protected internal void ThrowException(string id, string parameter)
	{
		ThrowExceptionWithLine(SR.Format(SR.GetResourceString(id), parameter), null);
	}

	protected void ThrowException(string id, string parameter1, string parameter2)
	{
		ThrowExceptionWithLine(SR.Format(SR.GetResourceString(id), parameter1, parameter2), null);
	}

	protected void ThrowException(string id, string parameter1, string parameter2, string parameter3)
	{
		ThrowExceptionWithLine(SR.Format(SR.GetResourceString(id), parameter1, parameter2, parameter3), null);
	}

	internal void ThrowExceptionWithLine(string message, Exception innerException)
	{
		XamlParseException.ThrowException(ParserContext, LineNumber, LinePosition, message, innerException);
	}

	internal object CreateInstanceFromType(Type type, short typeId, bool throwOnFail)
	{
		bool flag = true;
		BamlTypeInfoRecord bamlTypeInfoRecord = null;
		if (typeId >= 0)
		{
			bamlTypeInfoRecord = MapTable.GetTypeInfoFromId(typeId);
			if (bamlTypeInfoRecord != null)
			{
				flag = !bamlTypeInfoRecord.IsInternalType;
			}
		}
		if (flag)
		{
			if (!ReflectionHelper.IsPublicType(type))
			{
				ThrowException("ParserNotMarkedPublic", type.Name);
			}
		}
		else if (!ReflectionHelper.IsInternalType(type))
		{
			ThrowException("ParserNotAllowedInternalType", type.Name);
		}
		try
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXamlBaml, EventTrace.Level.Verbose, EventTrace.Event.WClientParseRdrCrInFTypBegin);
			object obj = null;
			try
			{
				if (TraceMarkup.IsEnabled)
				{
					TraceMarkup.Trace(TraceEventType.Start, TraceMarkup.CreateObject, type);
				}
				if (type != typeof(string))
				{
					if (typeId < 0)
					{
						obj = MapTable.CreateKnownTypeFromId(typeId);
					}
					else if (flag)
					{
						obj = Activator.CreateInstance(type);
					}
					else
					{
						obj = XamlTypeMapper.CreateInternalInstance(ParserContext, type);
						if (obj == null && throwOnFail)
						{
							ThrowException("ParserNotAllowedInternalType", type.Name);
						}
					}
				}
				if (TraceMarkup.IsEnabled)
				{
					TraceMarkup.Trace(TraceEventType.Stop, TraceMarkup.CreateObject, type, obj);
				}
			}
			finally
			{
				EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXamlBaml, EventTrace.Level.Verbose, EventTrace.Event.WClientParseRdrCrInFTypEnd);
			}
			return obj;
		}
		catch (MissingMethodException innerException)
		{
			if (throwOnFail)
			{
				if (ParentContext != null && ParentContext.ContextType == ReaderFlags.PropertyComplexDP)
				{
					BamlAttributeInfoRecord bamlAttributeInfoRecord = GetParentObjectData() as BamlAttributeInfoRecord;
					ThrowException("ParserNoDefaultPropConstructor", type.Name, bamlAttributeInfoRecord.DP.Name);
				}
				else
				{
					ThrowExceptionWithLine(SR.Format(SR.ParserNoDefaultConstructor, type.Name), innerException);
				}
			}
			return null;
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex) || ex is XamlParseException)
			{
				throw;
			}
			ThrowExceptionWithLine(SR.Format(SR.ParserErrorCreatingInstance, type.Name, type.Assembly.FullName), ex);
			return null;
		}
	}

	internal void FreezeIfRequired(object element)
	{
		if (_parserContext.FreezeFreezables && element is Freezable freezable)
		{
			freezable.Freeze();
		}
	}

	internal void PreParsedBamlReset()
	{
		PreParsedCurrentRecord = PreParsedRecordsStart;
	}

	protected internal void SetPreviousBamlRecordReader(BamlRecordReader previousBamlRecordReader)
	{
		_previousBamlRecordReader = previousBamlRecordReader;
	}
}
