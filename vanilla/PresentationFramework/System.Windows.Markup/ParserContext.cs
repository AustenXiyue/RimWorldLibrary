using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using MS.Internal;
using MS.Internal.Xaml.Parser;

namespace System.Windows.Markup;

/// <summary>Provides context information required by a XAML parser. </summary>
public class ParserContext : IUriContext
{
	private struct FreezeStackFrame
	{
		private bool _freezeFreezables;

		private int _repeatCount;

		internal bool FreezeFreezables => _freezeFreezables;

		internal void IncrementRepeatCount()
		{
			_repeatCount++;
		}

		internal bool DecrementRepeatCount()
		{
			if (_repeatCount > 0)
			{
				_repeatCount--;
				return true;
			}
			return false;
		}

		internal void Reset(bool freezeFreezables)
		{
			_freezeFreezables = freezeFreezables;
			_repeatCount = 0;
		}
	}

	private XamlTypeMapper _xamlTypeMapper;

	private Uri _baseUri;

	private XmlnsDictionary _xmlnsDictionary;

	private string _xmlLang = string.Empty;

	private string _xmlSpace = string.Empty;

	private Stack _langSpaceStack;

	private int _repeat;

	private Type _targetType;

	private Dictionary<Type, Dictionary<string, MS.Internal.Xaml.Parser.SpecialBracketCharacters>> _masterBracketCharacterCache;

	private bool _skipJournaledProperties;

	private MS.Internal.SecurityCriticalDataForSet<Assembly> _streamCreatedAssembly;

	private bool _ownsBamlStream;

	private ProvideValueServiceProvider _provideValueServiceProvider;

	private IStyleConnector _styleConnector;

	private Stack _nameScopeStack;

	private List<object[]> _staticResourcesStack;

	private object _rootElement;

	private FreezeStackFrame _currentFreezeStackFrame;

	private Dictionary<string, Freezable> _freezeCache;

	private Stack _freezeStack;

	private int _lineNumber;

	private int _linePosition;

	private BamlMapTable _mapTable;

	private bool _isDebugBamlStream;

	/// <summary>Gets the XAML namespace dictionary for this XAML parser context.</summary>
	/// <returns>The XAML namespace dictionary.</returns>
	public XmlnsDictionary XmlnsDictionary
	{
		get
		{
			if (_xmlnsDictionary == null)
			{
				_xmlnsDictionary = new XmlnsDictionary();
			}
			return _xmlnsDictionary;
		}
	}

	/// <summary>Gets or sets the xml:lang string for this context.</summary>
	/// <returns>The xml:lang string value.</returns>
	public string XmlLang
	{
		get
		{
			return _xmlLang;
		}
		set
		{
			EndRepeat();
			_xmlLang = ((value == null) ? string.Empty : value);
		}
	}

	/// <summary>Gets or sets the character for xml:space or this context.</summary>
	/// <returns>The character for xml:space or this context.</returns>
	public string XmlSpace
	{
		get
		{
			return _xmlSpace;
		}
		set
		{
			EndRepeat();
			_xmlSpace = value;
		}
	}

	internal Type TargetType
	{
		get
		{
			return _targetType;
		}
		set
		{
			EndRepeat();
			_targetType = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Markup.XamlTypeMapper" /> to use with this <see cref="T:System.Windows.Markup.ParserContext" />.</summary>
	/// <returns>The type mapper to use when mapping XAML elements to CLR types. </returns>
	public XamlTypeMapper XamlTypeMapper
	{
		get
		{
			return _xamlTypeMapper;
		}
		set
		{
			if (_xamlTypeMapper != value)
			{
				_xamlTypeMapper = value;
				_mapTable = new BamlMapTable(value);
				_xamlTypeMapper.MapTable = _mapTable;
			}
		}
	}

	internal Stack NameScopeStack
	{
		get
		{
			if (_nameScopeStack == null)
			{
				_nameScopeStack = new Stack(2);
			}
			return _nameScopeStack;
		}
	}

	/// <summary>Gets or sets the base URI for this context.</summary>
	/// <returns>The base URI, as a string.</returns>
	public Uri BaseUri
	{
		get
		{
			return _baseUri;
		}
		set
		{
			_baseUri = value;
		}
	}

	internal bool SkipJournaledProperties
	{
		get
		{
			return _skipJournaledProperties;
		}
		set
		{
			_skipJournaledProperties = value;
		}
	}

	internal Assembly StreamCreatedAssembly
	{
		get
		{
			return _streamCreatedAssembly.Value;
		}
		set
		{
			_streamCreatedAssembly.Value = value;
		}
	}

	internal bool FromRestrictiveReader { get; set; }

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

	internal bool IsDebugBamlStream
	{
		get
		{
			return _isDebugBamlStream;
		}
		set
		{
			_isDebugBamlStream = value;
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

	internal bool OwnsBamlStream
	{
		get
		{
			return _ownsBamlStream;
		}
		set
		{
			_ownsBamlStream = value;
		}
	}

	internal BamlMapTable MapTable
	{
		get
		{
			return _mapTable;
		}
		set
		{
			if (_mapTable != value)
			{
				_mapTable = value;
				_xamlTypeMapper = _mapTable.XamlTypeMapper;
				_xamlTypeMapper.MapTable = _mapTable;
			}
		}
	}

	internal IStyleConnector StyleConnector
	{
		get
		{
			return _styleConnector;
		}
		set
		{
			_styleConnector = value;
		}
	}

	internal ProvideValueServiceProvider ProvideValueProvider
	{
		get
		{
			if (_provideValueServiceProvider == null)
			{
				_provideValueServiceProvider = new ProvideValueServiceProvider(this);
			}
			return _provideValueServiceProvider;
		}
	}

	internal List<object[]> StaticResourcesStack
	{
		get
		{
			if (_staticResourcesStack == null)
			{
				_staticResourcesStack = new List<object[]>();
			}
			return _staticResourcesStack;
		}
	}

	internal bool InDeferredSection
	{
		get
		{
			if (_staticResourcesStack != null)
			{
				return _staticResourcesStack.Count > 0;
			}
			return false;
		}
	}

	internal Dictionary<Type, Dictionary<string, MS.Internal.Xaml.Parser.SpecialBracketCharacters>> MasterBracketCharacterCache
	{
		get
		{
			if (_masterBracketCharacterCache == null)
			{
				_masterBracketCharacterCache = new Dictionary<Type, Dictionary<string, MS.Internal.Xaml.Parser.SpecialBracketCharacters>>();
			}
			return _masterBracketCharacterCache;
		}
	}

	internal bool FreezeFreezables
	{
		get
		{
			return _currentFreezeStackFrame.FreezeFreezables;
		}
		set
		{
			if (value != _currentFreezeStackFrame.FreezeFreezables)
			{
				_currentFreezeStackFrame.DecrementRepeatCount();
				if (_freezeStack == null)
				{
					_freezeStack = new Stack();
				}
				_freezeStack.Push(_currentFreezeStackFrame);
				_currentFreezeStackFrame.Reset(value);
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.ParserContext" /> class. </summary>
	public ParserContext()
	{
		Initialize();
	}

	internal void Initialize()
	{
		_xmlnsDictionary = null;
		_nameScopeStack = null;
		_xmlLang = string.Empty;
		_xmlSpace = string.Empty;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.ParserContext" /> class by using the specified <see cref="T:System.Xml.XmlParserContext" />.</summary>
	/// <param name="xmlParserContext">The XML processing context to base the new <see cref="T:System.Windows.Markup.ParserContext" /> on.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xmlParserContext" /> is null.</exception>
	public ParserContext(XmlParserContext xmlParserContext)
	{
		if (xmlParserContext == null)
		{
			throw new ArgumentNullException("xmlParserContext");
		}
		_xmlLang = xmlParserContext.XmlLang;
		TypeConverter converter = TypeDescriptor.GetConverter(typeof(XmlSpace));
		if (converter != null)
		{
			_xmlSpace = converter.ConvertToString(null, TypeConverterHelper.InvariantEnglishUS, xmlParserContext.XmlSpace);
		}
		else
		{
			_xmlSpace = string.Empty;
		}
		_xmlnsDictionary = new XmlnsDictionary();
		if (xmlParserContext.BaseURI != null && xmlParserContext.BaseURI.Length > 0)
		{
			_baseUri = new Uri(xmlParserContext.BaseURI, UriKind.RelativeOrAbsolute);
		}
		XmlNamespaceManager namespaceManager = xmlParserContext.NamespaceManager;
		if (namespaceManager == null)
		{
			return;
		}
		foreach (string item in namespaceManager)
		{
			_xmlnsDictionary.Add(item, namespaceManager.LookupNamespace(item));
		}
	}

	internal ParserContext(XmlReader xmlReader)
	{
		if (xmlReader.BaseURI != null && xmlReader.BaseURI.Length != 0)
		{
			BaseUri = new Uri(xmlReader.BaseURI);
		}
		XmlLang = xmlReader.XmlLang;
		if (xmlReader.XmlSpace != 0)
		{
			XmlSpace = xmlReader.XmlSpace.ToString();
		}
	}

	internal ParserContext(ParserContext parserContext)
	{
		_xmlLang = parserContext.XmlLang;
		_xmlSpace = parserContext.XmlSpace;
		_xamlTypeMapper = parserContext.XamlTypeMapper;
		_mapTable = parserContext.MapTable;
		_baseUri = parserContext.BaseUri;
		_masterBracketCharacterCache = parserContext.MasterBracketCharacterCache;
		_rootElement = parserContext._rootElement;
		if (parserContext._nameScopeStack != null)
		{
			_nameScopeStack = (Stack)parserContext._nameScopeStack.Clone();
		}
		else
		{
			_nameScopeStack = null;
		}
		_skipJournaledProperties = parserContext._skipJournaledProperties;
		_xmlnsDictionary = null;
		if (parserContext._xmlnsDictionary == null || parserContext._xmlnsDictionary.Count <= 0)
		{
			return;
		}
		_xmlnsDictionary = new XmlnsDictionary();
		XmlnsDictionary xmlnsDictionary = parserContext.XmlnsDictionary;
		if (xmlnsDictionary == null)
		{
			return;
		}
		foreach (string key in xmlnsDictionary.Keys)
		{
			_xmlnsDictionary[key] = xmlnsDictionary[key];
		}
	}

	internal Dictionary<string, MS.Internal.Xaml.Parser.SpecialBracketCharacters> InitBracketCharacterCacheForType(Type type)
	{
		if (!MasterBracketCharacterCache.ContainsKey(type))
		{
			Dictionary<string, MS.Internal.Xaml.Parser.SpecialBracketCharacters> value = BuildBracketCharacterCacheForType(type);
			MasterBracketCharacterCache.Add(type, value);
		}
		return MasterBracketCharacterCache[type];
	}

	internal void PushScope()
	{
		_repeat++;
		_currentFreezeStackFrame.IncrementRepeatCount();
		if (_xmlnsDictionary != null)
		{
			_xmlnsDictionary.PushScope();
		}
	}

	internal void PopScope()
	{
		if (_repeat > 0)
		{
			_repeat--;
		}
		else if (_langSpaceStack != null && _langSpaceStack.Count > 0)
		{
			_repeat = (int)_langSpaceStack.Pop();
			_targetType = (Type)_langSpaceStack.Pop();
			_xmlSpace = (string)_langSpaceStack.Pop();
			_xmlLang = (string)_langSpaceStack.Pop();
		}
		if (!_currentFreezeStackFrame.DecrementRepeatCount())
		{
			_currentFreezeStackFrame = (FreezeStackFrame)_freezeStack.Pop();
		}
		if (_xmlnsDictionary != null)
		{
			_xmlnsDictionary.PopScope();
		}
	}

	/// <summary>Converts a XAML <see cref="T:System.Windows.Markup.ParserContext" /> to an <see cref="T:System.Xml.XmlParserContext" />.</summary>
	/// <returns>The converted XML parser context.</returns>
	/// <param name="parserContext">The XAML parser context to convert to an <see cref="T:System.Xml.XmlParserContext" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="parserContext" /> is null.</exception>
	public static implicit operator XmlParserContext(ParserContext parserContext)
	{
		return ToXmlParserContext(parserContext);
	}

	/// <summary>Converts an <see cref="T:System.Windows.Markup.ParserContext" /> to an <see cref="T:System.Xml.XmlParserContext" />.</summary>
	/// <returns>The XML parser context.</returns>
	/// <param name="parserContext">The context to convert to an <see cref="T:System.Xml.XmlParserContext" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="parserContext" /> is null.</exception>
	public static XmlParserContext ToXmlParserContext(ParserContext parserContext)
	{
		if (parserContext == null)
		{
			throw new ArgumentNullException("parserContext");
		}
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(new NameTable());
		XmlSpace xmlSpace = System.Xml.XmlSpace.None;
		if (parserContext.XmlSpace != null && parserContext.XmlSpace.Length != 0)
		{
			TypeConverter converter = TypeDescriptor.GetConverter(typeof(XmlSpace));
			if (converter != null)
			{
				try
				{
					xmlSpace = (XmlSpace)converter.ConvertFromString(null, TypeConverterHelper.InvariantEnglishUS, parserContext.XmlSpace);
				}
				catch (FormatException)
				{
					xmlSpace = System.Xml.XmlSpace.None;
				}
			}
		}
		if (parserContext._xmlnsDictionary != null)
		{
			foreach (string key in parserContext._xmlnsDictionary.Keys)
			{
				xmlNamespaceManager.AddNamespace(key, parserContext._xmlnsDictionary[key]);
			}
		}
		XmlParserContext xmlParserContext = new XmlParserContext(null, xmlNamespaceManager, parserContext.XmlLang, xmlSpace);
		if (parserContext.BaseUri == null)
		{
			xmlParserContext.BaseURI = null;
		}
		else
		{
			string components = new Uri(parserContext.BaseUri.GetComponents(UriComponents.SerializationInfoString, UriFormat.SafeUnescaped)).GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped);
			xmlParserContext.BaseURI = components;
		}
		return xmlParserContext;
	}

	private void EndRepeat()
	{
		if (_repeat > 0)
		{
			if (_langSpaceStack == null)
			{
				_langSpaceStack = new Stack(1);
			}
			_langSpaceStack.Push(XmlLang);
			_langSpaceStack.Push(XmlSpace);
			_langSpaceStack.Push(TargetType);
			_langSpaceStack.Push(_repeat);
			_repeat = 0;
		}
	}

	internal ParserContext ScopedCopy()
	{
		return ScopedCopy(copyNameScopeStack: true);
	}

	internal ParserContext ScopedCopy(bool copyNameScopeStack)
	{
		ParserContext parserContext = new ParserContext();
		parserContext._baseUri = _baseUri;
		parserContext._skipJournaledProperties = _skipJournaledProperties;
		parserContext._xmlLang = _xmlLang;
		parserContext._xmlSpace = _xmlSpace;
		parserContext._repeat = _repeat;
		parserContext._lineNumber = _lineNumber;
		parserContext._linePosition = _linePosition;
		parserContext._isDebugBamlStream = _isDebugBamlStream;
		parserContext._mapTable = _mapTable;
		parserContext._xamlTypeMapper = _xamlTypeMapper;
		parserContext._targetType = _targetType;
		parserContext._streamCreatedAssembly.Value = _streamCreatedAssembly.Value;
		parserContext._rootElement = _rootElement;
		parserContext._styleConnector = _styleConnector;
		if (_nameScopeStack != null && copyNameScopeStack)
		{
			parserContext._nameScopeStack = ((_nameScopeStack != null) ? ((Stack)_nameScopeStack.Clone()) : null);
		}
		else
		{
			parserContext._nameScopeStack = null;
		}
		parserContext._langSpaceStack = ((_langSpaceStack != null) ? ((Stack)_langSpaceStack.Clone()) : null);
		if (_xmlnsDictionary != null)
		{
			parserContext._xmlnsDictionary = new XmlnsDictionary(_xmlnsDictionary);
		}
		else
		{
			parserContext._xmlnsDictionary = null;
		}
		parserContext._currentFreezeStackFrame = _currentFreezeStackFrame;
		parserContext._freezeStack = ((_freezeStack != null) ? ((Stack)_freezeStack.Clone()) : null);
		return parserContext;
	}

	internal void TrimState()
	{
		if (_nameScopeStack != null && _nameScopeStack.Count == 0)
		{
			_nameScopeStack = null;
		}
	}

	internal ParserContext Clone()
	{
		ParserContext parserContext = ScopedCopy();
		parserContext._mapTable = ((_mapTable != null) ? _mapTable.Clone() : null);
		parserContext._xamlTypeMapper = ((_xamlTypeMapper != null) ? _xamlTypeMapper.Clone() : null);
		parserContext._xamlTypeMapper.MapTable = parserContext._mapTable;
		parserContext._mapTable.XamlTypeMapper = parserContext._xamlTypeMapper;
		return parserContext;
	}

	internal bool TryCacheFreezable(string value, Freezable freezable)
	{
		if (FreezeFreezables && freezable.CanFreeze)
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

	internal Freezable TryGetFreezable(string value)
	{
		Freezable value2 = null;
		if (_freezeCache != null)
		{
			_freezeCache.TryGetValue(value, out value2);
		}
		return value2;
	}

	private Dictionary<string, MS.Internal.Xaml.Parser.SpecialBracketCharacters> BuildBracketCharacterCacheForType(Type extensionType)
	{
		Dictionary<string, MS.Internal.Xaml.Parser.SpecialBracketCharacters> dictionary = new Dictionary<string, MS.Internal.Xaml.Parser.SpecialBracketCharacters>(StringComparer.OrdinalIgnoreCase);
		PropertyInfo[] properties = extensionType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
		Type type = null;
		Type type2 = null;
		PropertyInfo[] array = properties;
		foreach (PropertyInfo obj in array)
		{
			string name = obj.Name;
			string text = null;
			IList<CustomAttributeData> customAttributes = CustomAttributeData.GetCustomAttributes(obj);
			MS.Internal.Xaml.Parser.SpecialBracketCharacters specialBracketCharacters = null;
			foreach (CustomAttributeData item in customAttributes)
			{
				Type attributeType = item.AttributeType;
				Assembly assembly = attributeType.Assembly;
				if (type == null || type2 == null)
				{
					type = assembly.GetType("System.Windows.Markup.ConstructorArgumentAttribute");
					type2 = assembly.GetType("System.Windows.Markup.MarkupExtensionBracketCharactersAttribute");
				}
				if (attributeType.IsAssignableFrom(type))
				{
					text = item.ConstructorArguments[0].Value as string;
				}
				else if (attributeType.IsAssignableFrom(type2))
				{
					if (specialBracketCharacters == null)
					{
						specialBracketCharacters = new MS.Internal.Xaml.Parser.SpecialBracketCharacters();
					}
					specialBracketCharacters.AddBracketCharacters((char)item.ConstructorArguments[0].Value, (char)item.ConstructorArguments[1].Value);
				}
			}
			if (specialBracketCharacters != null)
			{
				specialBracketCharacters.EndInit();
				dictionary.Add(name, specialBracketCharacters);
				if (text != null)
				{
					dictionary.Add(text, specialBracketCharacters);
				}
			}
		}
		if (dictionary.Count != 0)
		{
			return dictionary;
		}
		return null;
	}
}
