using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Baml2006;
using System.Xaml;
using MS.Internal;
using MS.Internal.WindowsBase;
using MS.Utility;

namespace System.Windows.Markup;

/// <summary>Maps a XAML element name to the appropriate CLR <see cref="T:System.Type" /> in assemblies.</summary>
public class XamlTypeMapper
{
	internal class ConstructorData
	{
		private ConstructorInfo[] _constructors;

		private ParameterInfo[][] _parameters;

		internal ConstructorInfo[] Constructors => _constructors;

		internal ConstructorData(ConstructorInfo[] constructors)
		{
			_constructors = constructors;
		}

		internal ParameterInfo[] GetParameters(int constructorIndex)
		{
			if (_parameters == null)
			{
				_parameters = new ParameterInfo[_constructors.Length][];
			}
			if (_parameters[constructorIndex] == null)
			{
				_parameters[constructorIndex] = _constructors[constructorIndex].GetParameters();
			}
			return _parameters[constructorIndex];
		}
	}

	internal class TypeInformationCacheData
	{
		private string _clrNamespace;

		private Type _baseType;

		private bool _trimSurroundingWhitespace;

		private Hashtable _dpLookupHashtable;

		private HybridDictionary _propertyConverters = new HybridDictionary();

		private bool _trimSurroundingWhitespaceSet;

		private TypeConverter _typeConverter;

		private Type _typeConverterType;

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

		internal Type BaseType => _baseType;

		internal TypeConverter Converter
		{
			get
			{
				return _typeConverter;
			}
			set
			{
				_typeConverter = value;
			}
		}

		internal Type TypeConverterType
		{
			get
			{
				return _typeConverterType;
			}
			set
			{
				_typeConverterType = value;
			}
		}

		internal bool TrimSurroundingWhitespace
		{
			get
			{
				return _trimSurroundingWhitespace;
			}
			set
			{
				_trimSurroundingWhitespace = value;
			}
		}

		internal bool TrimSurroundingWhitespaceSet
		{
			get
			{
				return _trimSurroundingWhitespaceSet;
			}
			set
			{
				_trimSurroundingWhitespaceSet = value;
			}
		}

		internal HybridDictionary PropertyConverters
		{
			get
			{
				if (_propertyConverters == null)
				{
					_propertyConverters = new HybridDictionary();
				}
				return _propertyConverters;
			}
		}

		internal TypeInformationCacheData(Type baseType)
		{
			_baseType = baseType;
		}

		internal PropertyAndType GetPropertyAndType(string dpName)
		{
			if (_dpLookupHashtable == null)
			{
				_dpLookupHashtable = new Hashtable();
				return null;
			}
			return _dpLookupHashtable[dpName] as PropertyAndType;
		}

		internal void SetPropertyAndType(string dpName, PropertyInfo dpInfo, Type ownerType, bool isInternal)
		{
			if (!(_dpLookupHashtable[dpName] is PropertyAndType propertyAndType))
			{
				_dpLookupHashtable[dpName] = new PropertyAndType(null, dpInfo, setterSet: false, propInfoSet: true, ownerType, isInternal);
				return;
			}
			propertyAndType.PropInfo = dpInfo;
			propertyAndType.PropInfoSet = true;
			propertyAndType.IsInternal = isInternal;
		}

		internal void SetPropertyConverter(object dpOrPi, TypeConverter converter)
		{
			_propertyConverters[dpOrPi] = converter;
		}
	}

	internal class PropertyAndType
	{
		public PropertyInfo PropInfo;

		public MethodInfo Setter;

		public Type OwnerType;

		public bool PropInfoSet;

		public bool SetterSet;

		public bool IsInternal;

		public PropertyAndType(MethodInfo dpSetter, PropertyInfo dpInfo, bool setterSet, bool propInfoSet, Type ot, bool isInternal)
		{
			Setter = dpSetter;
			PropInfo = dpInfo;
			OwnerType = ot;
			SetterSet = setterSet;
			PropInfoSet = propInfoSet;
			IsInternal = isInternal;
		}
	}

	internal class XamlTypeMapperSchemaContext : XamlSchemaContext
	{
		private Dictionary<string, FrugalObjectList<string>> _nsDefinitions;

		private XamlTypeMapper _typeMapper;

		private WpfSharedXamlSchemaContext _sharedSchemaContext;

		private readonly object syncObject = new object();

		private Dictionary<string, string> _piNamespaces;

		private IEnumerable<string> _allXamlNamespaces;

		private Dictionary<Type, XamlType> _allowedInternalTypes;

		private HashSet<string> _clrNamespaces;

		internal XamlTypeMapperSchemaContext(XamlTypeMapper typeMapper)
		{
			_typeMapper = typeMapper;
			_sharedSchemaContext = (WpfSharedXamlSchemaContext)XamlReader.GetWpfSchemaContext();
			if (typeMapper._namespaceMaps != null)
			{
				_nsDefinitions = new Dictionary<string, FrugalObjectList<string>>();
				NamespaceMapEntry[] namespaceMaps = _typeMapper._namespaceMaps;
				foreach (NamespaceMapEntry namespaceMapEntry in namespaceMaps)
				{
					if (!_nsDefinitions.TryGetValue(namespaceMapEntry.XmlNamespace, out var value))
					{
						value = new FrugalObjectList<string>(1);
						_nsDefinitions.Add(namespaceMapEntry.XmlNamespace, value);
					}
					string clrNsUri = GetClrNsUri(namespaceMapEntry.ClrNamespace, namespaceMapEntry.AssemblyName);
					value.Add(clrNsUri);
				}
			}
			if (typeMapper.PITable.Count > 0)
			{
				_piNamespaces = new Dictionary<string, string>(typeMapper.PITable.Count);
				foreach (DictionaryEntry item in typeMapper.PITable)
				{
					ClrNamespaceAssemblyPair clrNamespaceAssemblyPair = (ClrNamespaceAssemblyPair)item.Value;
					string clrNsUri2 = GetClrNsUri(clrNamespaceAssemblyPair.ClrNamespace, clrNamespaceAssemblyPair.AssemblyName);
					_piNamespaces.Add((string)item.Key, clrNsUri2);
				}
			}
			_clrNamespaces = new HashSet<string>();
		}

		public override IEnumerable<string> GetAllXamlNamespaces()
		{
			IEnumerable<string> enumerable = _allXamlNamespaces;
			if (enumerable == null)
			{
				lock (syncObject)
				{
					if (_nsDefinitions != null || _piNamespaces != null)
					{
						List<string> list = new List<string>(_sharedSchemaContext.GetAllXamlNamespaces());
						AddKnownNamespaces(list);
						enumerable = list.AsReadOnly();
					}
					else
					{
						enumerable = _sharedSchemaContext.GetAllXamlNamespaces();
					}
					_allXamlNamespaces = enumerable;
				}
			}
			return enumerable;
		}

		public override XamlType GetXamlType(Type type)
		{
			if (ReflectionHelper.IsPublicType(type))
			{
				return _sharedSchemaContext.GetXamlType(type);
			}
			return GetInternalType(type, null);
		}

		public override bool TryGetCompatibleXamlNamespace(string xamlNamespace, out string compatibleNamespace)
		{
			if (_sharedSchemaContext.TryGetCompatibleXamlNamespace(xamlNamespace, out compatibleNamespace))
			{
				return true;
			}
			if ((_nsDefinitions != null && _nsDefinitions.ContainsKey(xamlNamespace)) || (_piNamespaces != null && SyncContainsKey(_piNamespaces, xamlNamespace)))
			{
				compatibleNamespace = xamlNamespace;
				return true;
			}
			return false;
		}

		internal Hashtable GetNamespaceMapHashList()
		{
			Hashtable hashtable = new Hashtable();
			if (_typeMapper._namespaceMaps != null)
			{
				NamespaceMapEntry[] namespaceMaps = _typeMapper._namespaceMaps;
				foreach (NamespaceMapEntry namespaceMapEntry in namespaceMaps)
				{
					NamespaceMapEntry value = new NamespaceMapEntry
					{
						XmlNamespace = namespaceMapEntry.XmlNamespace,
						ClrNamespace = namespaceMapEntry.ClrNamespace,
						AssemblyName = namespaceMapEntry.AssemblyName,
						AssemblyPath = namespaceMapEntry.AssemblyPath
					};
					AddToMultiHashtable(hashtable, namespaceMapEntry.XmlNamespace, value);
				}
			}
			foreach (DictionaryEntry item in _typeMapper.PITable)
			{
				ClrNamespaceAssemblyPair clrNamespaceAssemblyPair = (ClrNamespaceAssemblyPair)item.Value;
				NamespaceMapEntry namespaceMapEntry2 = new NamespaceMapEntry
				{
					XmlNamespace = (string)item.Key,
					ClrNamespace = clrNamespaceAssemblyPair.ClrNamespace,
					AssemblyName = clrNamespaceAssemblyPair.AssemblyName,
					AssemblyPath = _typeMapper.AssemblyPathFor(clrNamespaceAssemblyPair.AssemblyName)
				};
				AddToMultiHashtable(hashtable, namespaceMapEntry2.XmlNamespace, namespaceMapEntry2);
			}
			lock (syncObject)
			{
				foreach (string clrNamespace2 in _clrNamespaces)
				{
					SplitClrNsUri(clrNamespace2, out var clrNamespace, out var assembly);
					if (!string.IsNullOrEmpty(assembly))
					{
						string text = _typeMapper.AssemblyPathFor(assembly);
						if (!string.IsNullOrEmpty(text))
						{
							NamespaceMapEntry namespaceMapEntry3 = new NamespaceMapEntry
							{
								XmlNamespace = clrNamespace2,
								ClrNamespace = clrNamespace,
								AssemblyName = assembly,
								AssemblyPath = text
							};
							AddToMultiHashtable(hashtable, namespaceMapEntry3.XmlNamespace, namespaceMapEntry3);
						}
					}
				}
			}
			object[] array = new object[hashtable.Count];
			hashtable.Keys.CopyTo(array, 0);
			object[] array2 = array;
			foreach (object key in array2)
			{
				List<NamespaceMapEntry> list = (List<NamespaceMapEntry>)hashtable[key];
				hashtable[key] = list.ToArray();
			}
			return hashtable;
		}

		internal void SetMappingProcessingInstruction(string xamlNamespace, ClrNamespaceAssemblyPair pair)
		{
			string clrNsUri = GetClrNsUri(pair.ClrNamespace, pair.AssemblyName);
			lock (syncObject)
			{
				if (_piNamespaces == null)
				{
					_piNamespaces = new Dictionary<string, string>();
				}
				_piNamespaces[xamlNamespace] = clrNsUri;
				_allXamlNamespaces = null;
			}
		}

		protected override XamlType GetXamlType(string xamlNamespace, string name, params XamlType[] typeArguments)
		{
			try
			{
				return LookupXamlType(xamlNamespace, name, typeArguments);
			}
			catch (Exception ex)
			{
				if (CriticalExceptions.IsCriticalException(ex))
				{
					throw;
				}
				if (_typeMapper.LoadReferenceAssemblies())
				{
					return LookupXamlType(xamlNamespace, name, typeArguments);
				}
				throw;
			}
		}

		protected override Assembly OnAssemblyResolve(string assemblyName)
		{
			string text = _typeMapper.AssemblyPathFor(assemblyName);
			if (!string.IsNullOrEmpty(text))
			{
				return ReflectionHelper.LoadAssembly(assemblyName, text);
			}
			return base.OnAssemblyResolve(assemblyName);
		}

		private static string GetClrNsUri(string clrNamespace, string assembly)
		{
			return "clr-namespace:" + clrNamespace + ";assembly=" + assembly;
		}

		private static void SplitClrNsUri(string xmlNamespace, out string clrNamespace, out string assembly)
		{
			clrNamespace = null;
			assembly = null;
			int num = xmlNamespace.IndexOf("clr-namespace:", StringComparison.Ordinal);
			if (num < 0)
			{
				return;
			}
			num += "clr-namespace:".Length;
			if (num <= xmlNamespace.Length)
			{
				return;
			}
			int num2 = xmlNamespace.IndexOf(";assembly=", StringComparison.Ordinal);
			if (num2 < num)
			{
				clrNamespace = xmlNamespace.Substring(num);
				return;
			}
			clrNamespace = xmlNamespace.Substring(num, num2 - num);
			num2 += ";assembly=".Length;
			if (num2 > xmlNamespace.Length)
			{
				assembly = xmlNamespace.Substring(num2);
			}
		}

		private void AddKnownNamespaces(List<string> nsList)
		{
			if (_nsDefinitions != null)
			{
				foreach (string key in _nsDefinitions.Keys)
				{
					if (!nsList.Contains(key))
					{
						nsList.Add(key);
					}
				}
			}
			if (_piNamespaces == null)
			{
				return;
			}
			foreach (string key2 in _piNamespaces.Keys)
			{
				if (!nsList.Contains(key2))
				{
					nsList.Add(key2);
				}
			}
		}

		private XamlType GetInternalType(Type type, XamlType sharedSchemaXamlType)
		{
			lock (syncObject)
			{
				if (_allowedInternalTypes == null)
				{
					_allowedInternalTypes = new Dictionary<Type, XamlType>();
				}
				if (!_allowedInternalTypes.TryGetValue(type, out var value))
				{
					WpfSharedXamlSchemaContext.RequireRuntimeType(type);
					value = ((!_typeMapper.IsInternalTypeAllowedInFullTrust(type)) ? (sharedSchemaXamlType ?? _sharedSchemaContext.GetXamlType(type)) : new VisibilityMaskingXamlType(type, _sharedSchemaContext));
					_allowedInternalTypes.Add(type, value);
				}
				return value;
			}
		}

		private XamlType LookupXamlType(string xamlNamespace, string name, XamlType[] typeArguments)
		{
			XamlType xamlType;
			if (_nsDefinitions != null && _nsDefinitions.TryGetValue(xamlNamespace, out var value))
			{
				for (int i = 0; i < value.Count; i++)
				{
					xamlType = base.GetXamlType(value[i], name, typeArguments);
					if (xamlType != null)
					{
						return xamlType;
					}
				}
			}
			if (_piNamespaces != null && SyncTryGetValue(_piNamespaces, xamlNamespace, out var value2))
			{
				return base.GetXamlType(value2, name, typeArguments);
			}
			if (xamlNamespace.StartsWith("clr-namespace:", StringComparison.Ordinal))
			{
				lock (syncObject)
				{
					if (!_clrNamespaces.Contains(xamlNamespace))
					{
						_clrNamespaces.Add(xamlNamespace);
					}
				}
				return base.GetXamlType(xamlNamespace, name, typeArguments);
			}
			xamlType = _sharedSchemaContext.GetXamlTypeInternal(xamlNamespace, name, typeArguments);
			if (!(xamlType == null) && !xamlType.IsPublic)
			{
				return GetInternalType(xamlType.UnderlyingType, xamlType);
			}
			return xamlType;
		}

		private bool SyncContainsKey<K, V>(IDictionary<K, V> dict, K key)
		{
			lock (syncObject)
			{
				return dict.ContainsKey(key);
			}
		}

		private bool SyncTryGetValue(IDictionary<string, string> dict, string key, out string value)
		{
			lock (syncObject)
			{
				return dict.TryGetValue(key, out value);
			}
		}

		private static void AddToMultiHashtable<K, V>(Hashtable hashtable, K key, V value)
		{
			List<V> list = (List<V>)hashtable[key];
			if (list == null)
			{
				list = new List<V>();
				hashtable.Add(key, list);
			}
			list.Add(value);
		}
	}

	private class VisibilityMaskingXamlType : XamlType
	{
		public VisibilityMaskingXamlType(Type underlyingType, XamlSchemaContext schemaContext)
			: base(underlyingType, schemaContext)
		{
		}

		protected override bool LookupIsPublic()
		{
			return true;
		}
	}

	internal const string MarkupExtensionTypeString = "Type ";

	internal const string MarkupExtensionStaticString = "Static ";

	internal const string MarkupExtensionDynamicResourceString = "DynamicResource ";

	internal const string PresentationFrameworkDllName = "PresentationFramework";

	internal const string GeneratedNamespace = "XamlGeneratedNamespace";

	internal const string GeneratedInternalTypeHelperClassName = "GeneratedInternalTypeHelper";

	internal const string MarkupExtensionTemplateBindingString = "TemplateBinding ";

	private BamlMapTable _mapTable;

	private string[] _assemblyNames;

	private NamespaceMapEntry[] _namespaceMaps;

	private Hashtable _typeLookupFromXmlHashtable = new Hashtable();

	private Hashtable _namespaceMapHashList = new Hashtable();

	private HybridDictionary _typeInformationCache = new HybridDictionary();

	private HybridDictionary _constructorInformationCache;

	private XamlTypeMapperSchemaContext _schemaContext;

	private HybridDictionary _piTable = new HybridDictionary();

	private Dictionary<string, string> _piReverseTable = new Dictionary<string, string>();

	private HybridDictionary _assemblyPathTable = new HybridDictionary();

	private bool _referenceAssembliesLoaded;

	private int _lineNumber;

	private int _linePosition;

	private static XmlnsCache _xmlnsCache;

	/// <summary>Gets an instance of the <see cref="T:System.Windows.Markup.XamlTypeMapper" /> to use if one has not been specified.</summary>
	/// <returns>The default type mapper.</returns>
	public static XamlTypeMapper DefaultMapper => XmlParserDefaults.DefaultMapper;

	internal HybridDictionary PITable => _piTable;

	internal BamlMapTable MapTable
	{
		get
		{
			return _mapTable;
		}
		set
		{
			_mapTable = value;
		}
	}

	internal int LineNumber
	{
		set
		{
			_lineNumber = value;
		}
	}

	internal int LinePosition
	{
		set
		{
			_linePosition = value;
		}
	}

	internal Hashtable NamespaceMapHashList => _namespaceMapHashList;

	internal XamlSchemaContext SchemaContext
	{
		get
		{
			if (_schemaContext == null)
			{
				_schemaContext = new XamlTypeMapperSchemaContext(this);
			}
			return _schemaContext;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XamlTypeMapper" /> class by specifying an array of assembly names that the <see cref="T:System.Windows.Markup.XamlTypeMapper" /> should use.</summary>
	/// <param name="assemblyNames">The array of assembly names the <see cref="T:System.Windows.Markup.XamlTypeMapper" /> should use.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="assemblyNames" /> is null.</exception>
	public XamlTypeMapper(string[] assemblyNames)
	{
		if (assemblyNames == null)
		{
			throw new ArgumentNullException("assemblyNames");
		}
		_assemblyNames = assemblyNames;
		_namespaceMaps = null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XamlTypeMapper" /> class, using the specified array of assembly names and the specified namespace maps.</summary>
	/// <param name="assemblyNames">The array of assembly names the <see cref="T:System.Windows.Markup.XamlTypeMapper" /> should use.</param>
	/// <param name="namespaceMaps">The array of namespace maps the <see cref="T:System.Windows.Markup.XamlTypeMapper" /> should use.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="assemblyNames" /> is null.</exception>
	public XamlTypeMapper(string[] assemblyNames, NamespaceMapEntry[] namespaceMaps)
	{
		if (assemblyNames == null)
		{
			throw new ArgumentNullException("assemblyNames");
		}
		_assemblyNames = assemblyNames;
		_namespaceMaps = namespaceMaps;
	}

	/// <summary>Gets the CLR <see cref="T:System.Type" /> that a given XAML element is mapped to, using the specified XML namespace prefix and element name.</summary>
	/// <returns>The <see cref="T:System.Type" /> for the object, or null if no mapping could be resolved.</returns>
	/// <param name="xmlNamespace">The specified XML namespace prefix.</param>
	/// <param name="localName">The "local" name of the XAML element to obtain the mapped <see cref="T:System.Type" /> for. Local in this context means as mapped versus the provided <paramref name="xmlNamespace" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xmlNamespace" /> is null-or-<paramref name="localName" /> is null.</exception>
	public Type GetType(string xmlNamespace, string localName)
	{
		if (xmlNamespace == null)
		{
			throw new ArgumentNullException("xmlNamespace");
		}
		if (localName == null)
		{
			throw new ArgumentNullException("localName");
		}
		return GetTypeOnly(xmlNamespace, localName)?.ObjectType;
	}

	/// <summary>Defines a mapping between an XML namespace and CLR namespaces in assemblies, and adds these to the <see cref="T:System.Windows.Markup.XamlTypeMapper" /> information.</summary>
	/// <param name="xmlNamespace">The prefix for the XML namespace..</param>
	/// <param name="clrNamespace">The CLR  namespace that contains the types to map.</param>
	/// <param name="assemblyName">The assembly that contains the CLR  namespace.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xmlNamespace" /> is null-or-<paramref name="clrNamespace" /> is null-or-<paramref name="assemblyName" /> is null.</exception>
	public void AddMappingProcessingInstruction(string xmlNamespace, string clrNamespace, string assemblyName)
	{
		if (xmlNamespace == null)
		{
			throw new ArgumentNullException("xmlNamespace");
		}
		if (clrNamespace == null)
		{
			throw new ArgumentNullException("clrNamespace");
		}
		if (assemblyName == null)
		{
			throw new ArgumentNullException("assemblyName");
		}
		ClrNamespaceAssemblyPair clrNamespaceAssemblyPair = new ClrNamespaceAssemblyPair(clrNamespace, assemblyName);
		PITable[xmlNamespace] = clrNamespaceAssemblyPair;
		string text = assemblyName.ToUpper(TypeConverterHelper.InvariantEnglishUS);
		string key = clrNamespace + "#" + text;
		_piReverseTable[key] = xmlNamespace;
		if (_schemaContext != null)
		{
			_schemaContext.SetMappingProcessingInstruction(xmlNamespace, clrNamespaceAssemblyPair);
		}
	}

	/// <summary>Specifies the path to use when loading an assembly.</summary>
	/// <param name="assemblyName">The short name of the assembly without an extension or path specified (equivalent to <see cref="P:System.Reflection.AssemblyName.Name" />).  </param>
	/// <param name="assemblyPath">The file path of the assembly. The assembly path must be a full file path containing a file extension.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="assemblyName" /> is null-or-<paramref name="assemblyPath" /> is null.</exception>
	/// <exception cref="T:System.Windows.Markup.XamlParseException">
	///   <paramref name="assemblyName" /> is <see cref="F:System.String.Empty" />-or-<paramref name="assemblyPath" /> is <see cref="F:System.String.Empty" />-or-<paramref name="assemblyPath" /> is not a full file path containing a file extension.</exception>
	public void SetAssemblyPath(string assemblyName, string assemblyPath)
	{
		if (assemblyName == null)
		{
			throw new ArgumentNullException("assemblyName");
		}
		if (assemblyPath == null)
		{
			throw new ArgumentNullException("assemblyPath");
		}
		if (assemblyPath == string.Empty)
		{
			_lineNumber = 0;
			ThrowException("ParserBadAssemblyPath");
		}
		if (assemblyName == string.Empty)
		{
			_lineNumber = 0;
			ThrowException("ParserBadAssemblyName");
		}
		string text = assemblyName.ToUpper(CultureInfo.InvariantCulture);
		lock (_assemblyPathTable)
		{
			_assemblyPathTable[text] = assemblyPath;
		}
		if (ReflectionHelper.GetAlreadyLoadedAssembly(text) != null)
		{
			ReflectionHelper.ResetCacheForAssembly(text);
			if (_schemaContext != null)
			{
				_schemaContext = null;
			}
		}
	}

	internal void Initialize()
	{
		_typeLookupFromXmlHashtable.Clear();
		_namespaceMapHashList.Clear();
		_piTable.Clear();
		_piReverseTable.Clear();
		lock (_assemblyPathTable)
		{
			_assemblyPathTable.Clear();
		}
		_referenceAssembliesLoaded = false;
	}

	internal XamlTypeMapper Clone()
	{
		return new XamlTypeMapper(_assemblyNames.Clone() as string[])
		{
			_mapTable = _mapTable,
			_referenceAssembliesLoaded = _referenceAssembliesLoaded,
			_lineNumber = _lineNumber,
			_linePosition = _linePosition,
			_namespaceMaps = (_namespaceMaps.Clone() as NamespaceMapEntry[]),
			_typeLookupFromXmlHashtable = (_typeLookupFromXmlHashtable.Clone() as Hashtable),
			_namespaceMapHashList = (_namespaceMapHashList.Clone() as Hashtable),
			_typeInformationCache = CloneHybridDictionary(_typeInformationCache),
			_piTable = CloneHybridDictionary(_piTable),
			_piReverseTable = CloneStringDictionary(_piReverseTable),
			_assemblyPathTable = CloneHybridDictionary(_assemblyPathTable)
		};
	}

	private HybridDictionary CloneHybridDictionary(HybridDictionary dict)
	{
		HybridDictionary hybridDictionary = new HybridDictionary(dict.Count);
		foreach (DictionaryEntry item in dict)
		{
			hybridDictionary.Add(item.Key, item.Value);
		}
		return hybridDictionary;
	}

	private Dictionary<string, string> CloneStringDictionary(Dictionary<string, string> dict)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (KeyValuePair<string, string> item in dict)
		{
			dictionary.Add(item.Key, item.Value);
		}
		return dictionary;
	}

	internal string AssemblyPathFor(string assemblyName)
	{
		string result = null;
		if (assemblyName != null)
		{
			lock (_assemblyPathTable)
			{
				result = _assemblyPathTable[assemblyName.ToUpper(CultureInfo.InvariantCulture)] as string;
			}
		}
		return result;
	}

	private bool LoadReferenceAssemblies()
	{
		if (!_referenceAssembliesLoaded)
		{
			_referenceAssembliesLoaded = true;
			foreach (DictionaryEntry item in _assemblyPathTable)
			{
				ReflectionHelper.LoadAssembly(item.Key as string, item.Value as string);
			}
			return true;
		}
		return false;
	}

	internal RoutedEvent GetRoutedEvent(Type owner, string xmlNamespace, string localName)
	{
		Type baseType = null;
		string dynamicObjectName = null;
		if (localName == null)
		{
			throw new ArgumentNullException("localName");
		}
		if (xmlNamespace == null)
		{
			throw new ArgumentNullException("xmlNamespace");
		}
		if (owner != null && !ReflectionHelper.IsPublicType(owner))
		{
			_lineNumber = 0;
			ThrowException("ParserOwnerEventMustBePublic", owner.FullName);
		}
		return GetDependencyObject(isEvent: true, owner, xmlNamespace, localName, ref baseType, ref dynamicObjectName) as RoutedEvent;
	}

	internal object ParseProperty(object targetObject, Type propType, string propName, object dpOrPiOrFi, ITypeDescriptorContext typeContext, ParserContext parserContext, string value, short converterTypeId)
	{
		_lineNumber = parserContext?.LineNumber ?? 0;
		_linePosition = parserContext?.LinePosition ?? 0;
		if (converterTypeId < 0 && (short)(-converterTypeId) == 615)
		{
			if (propType == typeof(object) || propType == typeof(string))
			{
				return value;
			}
			string message = SR.Format(SR.ParserCannotConvertPropertyValueString, value, propName, propType.FullName);
			XamlParseException.ThrowException(parserContext, _lineNumber, _linePosition, message, null);
		}
		object obj = null;
		TypeConverter typeConverter = ((converterTypeId == 0) ? GetPropertyConverter(propType, dpOrPiOrFi) : parserContext.MapTable.GetConverterFromId(converterTypeId, propType, parserContext));
		try
		{
			obj = typeConverter.ConvertFromString(typeContext, TypeConverterHelper.InvariantEnglishUS, value);
			if (TraceMarkup.IsEnabled)
			{
				TraceMarkup.TraceActivityItem(TraceMarkup.TypeConvert, typeConverter, value, obj);
			}
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex) || ex is XamlParseException)
			{
				throw;
			}
			if (targetObject is IProvidePropertyFallback providePropertyFallback && providePropertyFallback.CanProvidePropertyFallback(propName))
			{
				obj = providePropertyFallback.ProvidePropertyFallback(propName, ex);
				if (TraceMarkup.IsEnabled)
				{
					TraceMarkup.TraceActivityItem(TraceMarkup.TypeConvertFallback, typeConverter, value, obj);
				}
			}
			else if (typeConverter.GetType() == typeof(TypeConverter))
			{
				XamlParseException.ThrowException(message: (!(propName != string.Empty)) ? SR.Format(SR.ParserDefaultConverterElement, propType.FullName, value) : SR.Format(SR.ParserDefaultConverterProperty, propType.FullName, propName, value), parserContext: parserContext, lineNumber: _lineNumber, linePosition: _linePosition, innerException: null);
			}
			else
			{
				string message3 = TypeConverterFailure(value, propName, propType.FullName);
				XamlParseException.ThrowException(parserContext, _lineNumber, _linePosition, message3, ex);
			}
		}
		if (obj != null && !propType.IsAssignableFrom(obj.GetType()))
		{
			string message4 = TypeConverterFailure(value, propName, propType.FullName);
			XamlParseException.ThrowException(parserContext, _lineNumber, _linePosition, message4, null);
		}
		return obj;
	}

	private string TypeConverterFailure(string value, string propName, string propType)
	{
		if (propName != string.Empty)
		{
			return SR.Format(SR.ParserCannotConvertPropertyValueString, value, propName, propType);
		}
		return SR.Format(SR.ParserCannotConvertInitializationText, value, propType);
	}

	internal void ValidateNames(string value, int lineNumber, int linePosition)
	{
		_lineNumber = lineNumber;
		_linePosition = linePosition;
		if (value == string.Empty)
		{
			ThrowException("ParserBadName", value);
		}
		if (MarkupExtensionParser.LooksLikeAMarkupExtension(value))
		{
			throw new XamlParseException(string.Concat(SR.Format(SR.ParserBadUidOrNameME, value) + " ", SR.Format(SR.ParserLineAndOffset, lineNumber.ToString(CultureInfo.CurrentCulture), linePosition.ToString(CultureInfo.CurrentCulture))), lineNumber, linePosition);
		}
		if (!NameValidationHelper.IsValidIdentifierName(value))
		{
			ThrowException("ParserBadName", value);
		}
	}

	internal void ValidateEnums(string propName, Type propType, string attribValue)
	{
		if (!propType.IsEnum || !(attribValue != string.Empty))
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < attribValue.Length; i++)
		{
			if (char.IsWhiteSpace(attribValue[i]))
			{
				continue;
			}
			if (flag)
			{
				if (attribValue[i] == ',')
				{
					flag = false;
				}
			}
			else if (char.IsDigit(attribValue[i]))
			{
				ThrowException("ParserNoDigitEnums", propName, attribValue);
			}
			else
			{
				flag = true;
			}
		}
	}

	private MemberInfo GetCachedMemberInfo(Type owner, string propName, bool onlyPropInfo, out BamlAttributeInfoRecord infoRecord)
	{
		infoRecord = null;
		if (MapTable != null)
		{
			string ownerTypeName = (owner.IsGenericType ? (owner.Namespace + "." + owner.Name) : owner.FullName);
			object attributeInfoKey = MapTable.GetAttributeInfoKey(ownerTypeName, propName);
			infoRecord = MapTable.GetHashTableData(attributeInfoKey) as BamlAttributeInfoRecord;
			if (infoRecord != null)
			{
				return infoRecord.GetPropertyMember(onlyPropInfo) as MemberInfo;
			}
		}
		return null;
	}

	private void AddCachedAttributeInfo(Type ownerType, BamlAttributeInfoRecord infoRecord)
	{
		if (MapTable != null)
		{
			object attributeInfoKey = MapTable.GetAttributeInfoKey(ownerType.FullName, infoRecord.Name);
			MapTable.AddHashTableData(attributeInfoKey, infoRecord);
		}
	}

	internal void UpdateClrPropertyInfo(Type currentParentType, BamlAttributeInfoRecord attribInfo)
	{
		bool isInternal = false;
		string name = attribInfo.Name;
		attribInfo.PropInfo = GetCachedMemberInfo(currentParentType, name, onlyPropInfo: true, out var infoRecord) as PropertyInfo;
		if (attribInfo.PropInfo == null)
		{
			attribInfo.PropInfo = PropertyInfoFromName(name, currentParentType, !ReflectionHelper.IsPublicType(currentParentType), tryPublicOnly: false, out isInternal);
			attribInfo.IsInternal = isInternal;
			if (attribInfo.PropInfo != null)
			{
				if (infoRecord != null)
				{
					infoRecord.SetPropertyMember(attribInfo.PropInfo);
					infoRecord.IsInternal = attribInfo.IsInternal;
				}
				else
				{
					AddCachedAttributeInfo(currentParentType, attribInfo);
				}
			}
		}
		else
		{
			attribInfo.IsInternal = infoRecord.IsInternal;
		}
	}

	private void UpdateAttachedPropertyMethdodInfo(BamlAttributeInfoRecord attributeInfo, bool isSetter)
	{
		MethodInfo methodInfo = null;
		Type ownerType = attributeInfo.OwnerType;
		bool flag = !ReflectionHelper.IsPublicType(ownerType);
		string name = (isSetter ? "Set" : "Get") + attributeInfo.Name;
		BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy;
		try
		{
			if (!flag)
			{
				methodInfo = ownerType.GetMethod(name, bindingFlags);
			}
			if (methodInfo == null)
			{
				methodInfo = ownerType.GetMethod(name, bindingFlags | BindingFlags.NonPublic);
			}
		}
		catch (AmbiguousMatchException)
		{
		}
		int num = ((!isSetter) ? 1 : 2);
		if (methodInfo != null && methodInfo.GetParameters().Length == num)
		{
			if (isSetter)
			{
				attributeInfo.AttachedPropertySetter = methodInfo;
			}
			else
			{
				attributeInfo.AttachedPropertyGetter = methodInfo;
			}
		}
	}

	internal void UpdateAttachedPropertySetter(BamlAttributeInfoRecord attributeInfo)
	{
		if (attributeInfo.AttachedPropertySetter == null)
		{
			UpdateAttachedPropertyMethdodInfo(attributeInfo, isSetter: true);
		}
	}

	internal void UpdateAttachedPropertyGetter(BamlAttributeInfoRecord attributeInfo)
	{
		if (attributeInfo.AttachedPropertyGetter == null)
		{
			UpdateAttachedPropertyMethdodInfo(attributeInfo, isSetter: false);
		}
	}

	internal MemberInfo GetClrInfo(bool isEvent, Type owner, string xmlNamespace, string localName, ref string propName)
	{
		string globalClassName = null;
		int num = localName.LastIndexOf('.');
		if (-1 != num)
		{
			globalClassName = localName.Substring(0, num);
			localName = localName.Substring(num + 1);
		}
		return GetClrInfoForClass(isEvent, owner, xmlNamespace, localName, globalClassName, ref propName);
	}

	internal bool IsAllowedPropertySet(PropertyInfo pi)
	{
		MethodInfo setMethod = pi.GetSetMethod(nonPublic: true);
		if (setMethod != null)
		{
			return setMethod.IsPublic;
		}
		return false;
	}

	internal bool IsAllowedPropertyGet(PropertyInfo pi)
	{
		MethodInfo getMethod = pi.GetGetMethod(nonPublic: true);
		if (getMethod != null)
		{
			return getMethod.IsPublic;
		}
		return false;
	}

	internal static bool IsAllowedPropertySet(PropertyInfo pi, bool allowProtected, out bool isPublic)
	{
		MethodInfo setMethod = pi.GetSetMethod(nonPublic: true);
		bool flag = allowProtected && setMethod != null && setMethod.IsFamily;
		isPublic = setMethod != null && setMethod.IsPublic && ReflectionHelper.IsPublicType(setMethod.DeclaringType);
		if (setMethod != null)
		{
			return setMethod.IsPublic || setMethod.IsAssembly || setMethod.IsFamilyOrAssembly || flag;
		}
		return false;
	}

	private static bool IsAllowedPropertyGet(PropertyInfo pi, bool allowProtected, out bool isPublic)
	{
		MethodInfo getMethod = pi.GetGetMethod(nonPublic: true);
		bool flag = allowProtected && getMethod != null && getMethod.IsFamily;
		isPublic = getMethod != null && getMethod.IsPublic && ReflectionHelper.IsPublicType(getMethod.DeclaringType);
		if (getMethod != null)
		{
			return getMethod.IsPublic || getMethod.IsAssembly || getMethod.IsFamilyOrAssembly || flag;
		}
		return false;
	}

	private static bool IsAllowedEvent(EventInfo ei, bool allowProtected, out bool isPublic)
	{
		MethodInfo addMethod = ei.GetAddMethod(nonPublic: true);
		bool flag = allowProtected && addMethod != null && addMethod.IsFamily;
		isPublic = addMethod != null && addMethod.IsPublic && ReflectionHelper.IsPublicType(addMethod.DeclaringType);
		if (addMethod != null)
		{
			return addMethod.IsPublic || addMethod.IsAssembly || addMethod.IsFamilyOrAssembly || flag;
		}
		return false;
	}

	private static bool IsPublicEvent(EventInfo ei)
	{
		MethodInfo addMethod = ei.GetAddMethod(nonPublic: true);
		if (addMethod != null)
		{
			return addMethod.IsPublic;
		}
		return false;
	}

	/// <summary>Requests permission for a <see cref="T:System.Windows.Markup.XamlTypeMapper" /> derived type that is called under full trust to access a specific internal type.</summary>
	/// <returns>true if the internal type can be accessed; otherwise, false.</returns>
	/// <param name="type">The type to access.</param>
	protected virtual bool AllowInternalType(Type type)
	{
		return false;
	}

	private bool IsInternalTypeAllowedInFullTrust(Type type)
	{
		bool result = false;
		if (ReflectionHelper.IsInternalType(type))
		{
			result = AllowInternalType(type);
		}
		return result;
	}

	internal MemberInfo GetClrInfoForClass(bool isEvent, Type owner, string xmlNamespace, string localName, string globalClassName, ref string propName)
	{
		return GetClrInfoForClass(isEvent, owner, xmlNamespace, localName, globalClassName, tryInternal: false, ref propName);
	}

	private MemberInfo GetClrInfoForClass(bool isEvent, Type owner, string xmlNamespace, string localName, string globalClassName, bool tryInternal, ref string propName)
	{
		bool isInternal = false;
		MemberInfo memberInfo = null;
		BindingFlags bindingFlags = BindingFlags.Public;
		propName = null;
		ParameterInfo[] array = null;
		if (globalClassName != null)
		{
			TypeAndSerializer typeOnly = GetTypeOnly(xmlNamespace, globalClassName);
			if (typeOnly != null && typeOnly.ObjectType != null)
			{
				Type objectType = typeOnly.ObjectType;
				memberInfo = GetCachedMemberInfo(objectType, localName, onlyPropInfo: false, out var infoRecord);
				if (memberInfo == null)
				{
					if (isEvent)
					{
						memberInfo = objectType.GetMethod("Add" + localName + "Handler", bindingFlags | BindingFlags.Static | BindingFlags.FlattenHierarchy);
						if (memberInfo != null)
						{
							MethodInfo methodInfo = memberInfo as MethodInfo;
							if (methodInfo != null)
							{
								array = methodInfo.GetParameters();
								Type type = KnownTypes.Types[135];
								if (array == null || array.Length != 2 || !type.IsAssignableFrom(array[0].ParameterType))
								{
									memberInfo = null;
								}
							}
						}
						if (memberInfo == null)
						{
							memberInfo = objectType.GetEvent(localName, bindingFlags | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
							if (memberInfo != null)
							{
								EventInfo eventInfo = memberInfo as EventInfo;
								if (!ReflectionHelper.IsPublicType(eventInfo.EventHandlerType))
								{
									ThrowException("ParserEventDelegateTypeNotAccessible", eventInfo.EventHandlerType.FullName, objectType.Name + "." + localName);
								}
								if (!IsPublicEvent(eventInfo))
								{
									ThrowException("ParserCantSetAttribute", "event", objectType.Name + "." + localName, "add");
								}
							}
						}
					}
					else
					{
						memberInfo = objectType.GetMethod("Set" + localName, bindingFlags | BindingFlags.Static | BindingFlags.FlattenHierarchy);
						if (memberInfo != null && ((MethodInfo)memberInfo).GetParameters().Length != 2)
						{
							memberInfo = null;
						}
						if (memberInfo == null)
						{
							memberInfo = objectType.GetMethod("Get" + localName, bindingFlags | BindingFlags.Static | BindingFlags.FlattenHierarchy);
							if (memberInfo != null && ((MethodInfo)memberInfo).GetParameters().Length != 1)
							{
								memberInfo = null;
							}
						}
						if (memberInfo == null)
						{
							memberInfo = PropertyInfoFromName(localName, objectType, tryInternal, tryPublicOnly: true, out isInternal);
							if (memberInfo != null && owner != null && !objectType.IsAssignableFrom(owner))
							{
								ThrowException("ParserAttachedPropInheritError", string.Format(CultureInfo.CurrentCulture, "{0}.{1}", objectType.Name, localName), owner.Name);
							}
						}
						if (null != memberInfo && infoRecord != null)
						{
							if (infoRecord.DP == null)
							{
								infoRecord.DP = MapTable.GetDependencyProperty(infoRecord);
							}
							infoRecord.SetPropertyMember(memberInfo);
						}
					}
				}
			}
		}
		else if (null != owner)
		{
			if (null != owner)
			{
				memberInfo = GetCachedMemberInfo(owner, localName, onlyPropInfo: false, out var infoRecord2);
				if (memberInfo == null)
				{
					if (isEvent)
					{
						memberInfo = owner.GetMethod("Add" + localName + "Handler", bindingFlags | BindingFlags.Static | BindingFlags.FlattenHierarchy);
						if (memberInfo != null)
						{
							MethodInfo methodInfo2 = memberInfo as MethodInfo;
							if (methodInfo2 != null)
							{
								array = methodInfo2.GetParameters();
								Type type2 = KnownTypes.Types[135];
								if (array == null || array.Length != 2 || !type2.IsAssignableFrom(array[0].ParameterType))
								{
									memberInfo = null;
								}
							}
						}
						if (memberInfo == null)
						{
							memberInfo = owner.GetEvent(localName, BindingFlags.Instance | BindingFlags.FlattenHierarchy | bindingFlags);
							if (memberInfo != null)
							{
								EventInfo eventInfo2 = memberInfo as EventInfo;
								if (!ReflectionHelper.IsPublicType(eventInfo2.EventHandlerType))
								{
									ThrowException("ParserEventDelegateTypeNotAccessible", eventInfo2.EventHandlerType.FullName, owner.Name + "." + localName);
								}
								if (!IsPublicEvent(eventInfo2))
								{
									ThrowException("ParserCantSetAttribute", "event", owner.Name + "." + localName, "add");
								}
							}
						}
					}
					else
					{
						memberInfo = owner.GetMethod("Set" + localName, bindingFlags | BindingFlags.Static | BindingFlags.FlattenHierarchy);
						if (memberInfo != null && ((MethodInfo)memberInfo).GetParameters().Length != 2)
						{
							memberInfo = null;
						}
						if (memberInfo == null)
						{
							memberInfo = owner.GetMethod("Get" + localName, bindingFlags | BindingFlags.Static | BindingFlags.FlattenHierarchy);
							if (memberInfo != null && ((MethodInfo)memberInfo).GetParameters().Length != 1)
							{
								memberInfo = null;
							}
						}
						if (memberInfo == null)
						{
							memberInfo = PropertyInfoFromName(localName, owner, tryInternal, tryPublicOnly: true, out isInternal);
						}
						if (null != memberInfo && infoRecord2 != null)
						{
							if (infoRecord2.DP == null)
							{
								infoRecord2.DP = MapTable.GetDependencyProperty(infoRecord2);
							}
							infoRecord2.SetPropertyMember(memberInfo);
						}
					}
				}
			}
		}
		if (null != memberInfo)
		{
			propName = localName;
		}
		return memberInfo;
	}

	internal EventInfo GetClrEventInfo(Type owner, string eventName)
	{
		EventInfo eventInfo = null;
		while (owner != null)
		{
			eventInfo = owner.GetEvent(eventName, BindingFlags.Instance | BindingFlags.Public);
			if (eventInfo != null)
			{
				break;
			}
			owner = GetCachedBaseType(owner);
		}
		return eventInfo;
	}

	internal object GetDependencyObject(bool isEvent, Type owner, string xmlNamespace, string localName, ref Type baseType, ref string dynamicObjectName)
	{
		object obj = null;
		string text = null;
		dynamicObjectName = null;
		int num = localName.LastIndexOf('.');
		if (-1 != num)
		{
			text = localName.Substring(0, num);
			localName = localName.Substring(num + 1);
		}
		if (text != null)
		{
			TypeAndSerializer typeOnly = GetTypeOnly(xmlNamespace, text);
			if (typeOnly != null && typeOnly.ObjectType != null)
			{
				baseType = typeOnly.ObjectType;
				obj = ((!isEvent) ? ((object)DependencyProperty.FromName(localName, baseType)) : ((object)RoutedEventFromName(localName, baseType)));
				if (obj != null)
				{
					dynamicObjectName = localName;
				}
			}
		}
		else
		{
			NamespaceMapEntry[] namespaceMapEntries = GetNamespaceMapEntries(xmlNamespace);
			if (namespaceMapEntries == null)
			{
				return null;
			}
			baseType = owner;
			while (null != baseType)
			{
				bool flag = false;
				for (int i = 0; i < namespaceMapEntries.Length; i++)
				{
					if (flag)
					{
						break;
					}
					if (namespaceMapEntries[i].ClrNamespace == GetCachedNamespace(baseType))
					{
						flag = true;
					}
				}
				if (flag)
				{
					obj = ((!isEvent) ? ((object)DependencyProperty.FromName(localName, baseType)) : ((object)RoutedEventFromName(localName, baseType)));
				}
				if (obj != null || isEvent)
				{
					dynamicObjectName = localName;
					break;
				}
				baseType = GetCachedBaseType(baseType);
			}
		}
		return obj;
	}

	internal DependencyProperty DependencyPropertyFromName(string localName, string xmlNamespace, ref Type ownerType)
	{
		int num = localName.LastIndexOf('.');
		if (-1 != num)
		{
			string text = localName.Substring(0, num);
			localName = localName.Substring(num + 1);
			TypeAndSerializer typeOnly = GetTypeOnly(xmlNamespace, text);
			if (typeOnly == null || typeOnly.ObjectType == null)
			{
				ThrowException("ParserNoType", text);
			}
			ownerType = typeOnly.ObjectType;
		}
		if (null == ownerType)
		{
			throw new ArgumentNullException("ownerType");
		}
		return DependencyProperty.FromName(localName, ownerType);
	}

	internal PropertyInfo GetXmlLangProperty(string xmlNamespace, string localName)
	{
		TypeAndSerializer typeOnly = GetTypeOnly(xmlNamespace, localName);
		if (typeOnly == null || typeOnly.ObjectType == null)
		{
			return null;
		}
		if (typeOnly.XmlLangProperty == null)
		{
			BamlAssemblyInfoRecord assemblyInfoFromId = MapTable.GetAssemblyInfoFromId(-1);
			if (typeOnly.ObjectType.Assembly == assemblyInfoFromId.Assembly)
			{
				if (KnownTypes.Types[226].IsAssignableFrom(typeOnly.ObjectType) || KnownTypes.Types[225].IsAssignableFrom(typeOnly.ObjectType))
				{
					typeOnly.XmlLangProperty = KnownTypes.Types[226].GetProperty("Language", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
				}
			}
			else
			{
				string text = null;
				bool flag = false;
				AttributeCollection attributes = TypeDescriptor.GetAttributes(typeOnly.ObjectType);
				if (attributes != null && attributes[typeof(XmlLangPropertyAttribute)] is XmlLangPropertyAttribute xmlLangPropertyAttribute)
				{
					flag = true;
					text = xmlLangPropertyAttribute.Name;
				}
				if (flag)
				{
					if (text != null && text.Length > 0)
					{
						typeOnly.XmlLangProperty = typeOnly.ObjectType.GetProperty(text, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
					}
					if (typeOnly.XmlLangProperty == null)
					{
						ThrowException("ParserXmlLangPropertyValueInvalid");
					}
				}
			}
		}
		return typeOnly.XmlLangProperty;
	}

	private PropertyInfo PropertyInfoFromName(string localName, Type ownerType, bool tryInternal, bool tryPublicOnly, out bool isInternal)
	{
		PropertyInfo propertyInfo = null;
		isInternal = false;
		TypeInformationCacheData cachedInformationForType = GetCachedInformationForType(ownerType);
		PropertyAndType propertyAndType = cachedInformationForType.GetPropertyAndType(localName);
		if (propertyAndType == null || !propertyAndType.PropInfoSet)
		{
			try
			{
				BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
				if (!tryInternal)
				{
					propertyInfo = ownerType.GetProperty(localName, bindingFlags);
				}
				if (propertyInfo == null && !tryPublicOnly)
				{
					propertyInfo = ownerType.GetProperty(localName, bindingFlags | BindingFlags.NonPublic);
					if (propertyInfo != null)
					{
						isInternal = true;
					}
				}
			}
			catch (AmbiguousMatchException)
			{
				PropertyInfo[] properties = ownerType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
				for (int i = 0; i < properties.Length; i++)
				{
					if (properties[i].Name == localName)
					{
						propertyInfo = properties[i];
						break;
					}
				}
			}
			cachedInformationForType.SetPropertyAndType(localName, propertyInfo, ownerType, isInternal);
		}
		else
		{
			propertyInfo = propertyAndType.PropInfo;
			isInternal = propertyAndType.IsInternal;
		}
		return propertyInfo;
	}

	internal RoutedEvent RoutedEventFromName(string localName, Type ownerType)
	{
		Type type = ownerType;
		while (null != type)
		{
			MS.Internal.WindowsBase.SecurityHelper.RunClassConstructor(type);
			type = GetCachedBaseType(type);
		}
		return EventManager.GetRoutedEventFromName(localName, ownerType);
	}

	internal static Type GetPropertyType(object propertyMember)
	{
		GetPropertyType(propertyMember, out var propertyType, out var _);
		return propertyType;
	}

	internal static void GetPropertyType(object propertyMember, out Type propertyType, out bool propertyCanWrite)
	{
		if (propertyMember is DependencyProperty dependencyProperty)
		{
			propertyType = dependencyProperty.PropertyType;
			propertyCanWrite = !dependencyProperty.ReadOnly;
			return;
		}
		PropertyInfo propertyInfo = propertyMember as PropertyInfo;
		if (propertyInfo != null)
		{
			propertyType = propertyInfo.PropertyType;
			propertyCanWrite = propertyInfo.CanWrite;
			return;
		}
		MethodInfo methodInfo = propertyMember as MethodInfo;
		if (methodInfo != null)
		{
			ParameterInfo[] parameters = methodInfo.GetParameters();
			propertyType = ((parameters.Length == 1) ? methodInfo.ReturnType : parameters[1].ParameterType);
			propertyCanWrite = parameters.Length != 1;
		}
		else
		{
			propertyType = typeof(object);
			propertyCanWrite = false;
		}
	}

	internal static string GetPropertyName(object propertyMember)
	{
		if (propertyMember is DependencyProperty dependencyProperty)
		{
			return dependencyProperty.Name;
		}
		PropertyInfo propertyInfo = propertyMember as PropertyInfo;
		if (propertyInfo != null)
		{
			return propertyInfo.Name;
		}
		MethodInfo methodInfo = propertyMember as MethodInfo;
		if (methodInfo != null)
		{
			return methodInfo.Name.Substring("Get".Length);
		}
		return null;
	}

	internal static Type GetDeclaringType(object propertyMember)
	{
		Type type = null;
		MemberInfo memberInfo = propertyMember as MemberInfo;
		if (memberInfo != null)
		{
			return memberInfo.DeclaringType;
		}
		return ((DependencyProperty)propertyMember).OwnerType;
	}

	internal static Type GetTypeFromName(string typeName, DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (typeName == null)
		{
			throw new ArgumentNullException("typeName");
		}
		int num = typeName.IndexOf(':');
		string text = string.Empty;
		if (num > 0)
		{
			text = typeName.Substring(0, num);
			typeName = typeName.Substring(num + 1, typeName.Length - num - 1);
		}
		object obj = ((element.GetValue(XmlAttributeProperties.XmlnsDictionaryProperty) is XmlnsDictionary xmlnsDictionary) ? xmlnsDictionary[text] : null);
		NamespaceMapEntry[] array = ((element.GetValue(XmlAttributeProperties.XmlNamespaceMapsProperty) is Hashtable hashtable && obj != null) ? (hashtable[obj] as NamespaceMapEntry[]) : null);
		if (array == null)
		{
			if (text == string.Empty)
			{
				foreach (ClrNamespaceAssemblyPair item in GetClrNamespacePairFromCache("http://schemas.microsoft.com/winfx/2006/xaml/presentation"))
				{
					if (item.AssemblyName == null)
					{
						continue;
					}
					Assembly assembly = ReflectionHelper.LoadAssembly(item.AssemblyName, null);
					if (assembly != null)
					{
						string name = string.Format(TypeConverterHelper.InvariantEnglishUS, "{0}.{1}", item.ClrNamespace, typeName);
						Type type = assembly.GetType(name);
						if (type != null)
						{
							return type;
						}
					}
				}
			}
			return null;
		}
		for (int i = 0; i < array.Length; i++)
		{
			Assembly assembly2 = array[i].Assembly;
			if (assembly2 != null)
			{
				string name2 = string.Format(TypeConverterHelper.InvariantEnglishUS, "{0}.{1}", array[i].ClrNamespace, typeName);
				Type type2 = assembly2.GetType(name2);
				if (type2 != null)
				{
					return type2;
				}
			}
		}
		return null;
	}

	internal Type GetTargetTypeAndMember(string valueParam, ParserContext context, bool isTypeExpected, out string memberName)
	{
		string text = valueParam;
		string text2 = string.Empty;
		int num = text.IndexOf(':');
		if (num >= 0)
		{
			text2 = text.Substring(0, num);
			text = text.Substring(num + 1);
		}
		memberName = null;
		Type type = null;
		num = text.LastIndexOf('.');
		if (num >= 0)
		{
			memberName = text.Substring(num + 1);
			text = text.Substring(0, num);
			string xmlNamespace = context.XmlnsDictionary[text2];
			TypeAndSerializer typeOnly = GetTypeOnly(xmlNamespace, text);
			if (typeOnly != null)
			{
				type = typeOnly.ObjectType;
			}
			if (type == null)
			{
				ThrowException("ParserNoType", text);
			}
		}
		else if (!isTypeExpected && text2.Length == 0)
		{
			memberName = text;
		}
		else
		{
			ThrowException("ParserBadMemberReference", valueParam);
		}
		return type;
	}

	internal Type GetDependencyPropertyOwnerAndName(string memberValue, ParserContext context, Type defaultTargetType, out string memberName)
	{
		Type type = GetTargetTypeAndMember(memberValue, context, isTypeExpected: false, out memberName);
		if (type == null)
		{
			type = defaultTargetType;
			if (type == null)
			{
				ThrowException("ParserBadMemberReference", memberValue);
			}
		}
		string memberName2 = memberName + "Property";
		MemberInfo staticMemberInfo = GetStaticMemberInfo(type, memberName2, fieldInfoOnly: true);
		if (staticMemberInfo.DeclaringType != type)
		{
			type = staticMemberInfo.DeclaringType;
		}
		return type;
	}

	internal MemberInfo GetStaticMemberInfo(Type targetType, string memberName, bool fieldInfoOnly)
	{
		MemberInfo staticMemberInfo = GetStaticMemberInfo(targetType, memberName, fieldInfoOnly, tryInternal: false);
		if (staticMemberInfo == null)
		{
			ThrowException("ParserInvalidStaticMember", memberName, targetType.Name);
		}
		return staticMemberInfo;
	}

	private MemberInfo GetStaticMemberInfo(Type targetType, string memberName, bool fieldInfoOnly, bool tryInternal)
	{
		MemberInfo memberInfo = null;
		BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy;
		if (tryInternal)
		{
			bindingFlags |= BindingFlags.NonPublic;
		}
		if (!fieldInfoOnly)
		{
			memberInfo = targetType.GetProperty(memberName, bindingFlags);
		}
		if (memberInfo == null)
		{
			memberInfo = targetType.GetField(memberName, bindingFlags);
		}
		return memberInfo;
	}

	internal TypeAndSerializer GetTypeOnly(string xmlNamespace, string localName)
	{
		string key = xmlNamespace + ":" + localName;
		TypeAndSerializer typeAndSerializer = _typeLookupFromXmlHashtable[key] as TypeAndSerializer;
		if (typeAndSerializer == null && !_typeLookupFromXmlHashtable.Contains(key))
		{
			typeAndSerializer = CreateTypeAndSerializer(xmlNamespace, localName);
			_typeLookupFromXmlHashtable[key] = typeAndSerializer;
		}
		return typeAndSerializer;
	}

	internal TypeAndSerializer GetTypeAndSerializer(string xmlNamespace, string localName, object dpOrPiorMi)
	{
		string key = xmlNamespace + ":" + localName;
		TypeAndSerializer typeAndSerializer = _typeLookupFromXmlHashtable[key] as TypeAndSerializer;
		if (typeAndSerializer == null && !_typeLookupFromXmlHashtable.Contains(key))
		{
			typeAndSerializer = CreateTypeAndSerializer(xmlNamespace, localName);
			_typeLookupFromXmlHashtable[key] = typeAndSerializer;
		}
		if (typeAndSerializer != null && !typeAndSerializer.IsSerializerTypeSet)
		{
			typeAndSerializer.SerializerType = GetXamlSerializerForType(typeAndSerializer.ObjectType);
			typeAndSerializer.IsSerializerTypeSet = true;
		}
		return typeAndSerializer;
	}

	private TypeAndSerializer CreateTypeAndSerializer(string xmlNamespace, string localName)
	{
		TypeAndSerializer typeAndSerializer = null;
		NamespaceMapEntry[] namespaceMapEntries = GetNamespaceMapEntries(xmlNamespace);
		if (namespaceMapEntries != null)
		{
			bool flag = true;
			int num = 0;
			while (num < namespaceMapEntries.Length)
			{
				NamespaceMapEntry namespaceMapEntry = namespaceMapEntries[num];
				if (namespaceMapEntry != null)
				{
					Type objectType = GetObjectType(namespaceMapEntry, localName, flag);
					if (null != objectType)
					{
						if (!ReflectionHelper.IsPublicType(objectType) && !IsInternalTypeAllowedInFullTrust(objectType))
						{
							ThrowException("ParserPublicType", objectType.Name);
						}
						typeAndSerializer = new TypeAndSerializer();
						typeAndSerializer.ObjectType = objectType;
						break;
					}
				}
				num++;
				if (flag && num == namespaceMapEntries.Length)
				{
					flag = false;
					num = 0;
				}
			}
		}
		return typeAndSerializer;
	}

	private Type GetObjectType(NamespaceMapEntry namespaceMap, string localName, bool knownTypesOnly)
	{
		Type result = null;
		if (knownTypesOnly)
		{
			short knownTypeIdFromName = BamlMapTable.GetKnownTypeIdFromName(namespaceMap.AssemblyName, namespaceMap.ClrNamespace, localName);
			if (knownTypeIdFromName != 0)
			{
				result = BamlMapTable.GetKnownTypeFromId(knownTypeIdFromName);
			}
		}
		else
		{
			Assembly assembly = namespaceMap.Assembly;
			if (null != assembly)
			{
				string name = namespaceMap.ClrNamespace + "." + localName;
				try
				{
					result = assembly.GetType(name);
				}
				catch (Exception ex)
				{
					if (CriticalExceptions.IsCriticalException(ex))
					{
						throw;
					}
					if (LoadReferenceAssemblies())
					{
						try
						{
							result = assembly.GetType(name);
						}
						catch (ArgumentException)
						{
							result = null;
						}
					}
				}
			}
		}
		return result;
	}

	internal int GetCustomBamlSerializerIdForType(Type objectType)
	{
		if (objectType == KnownTypes.Types[52])
		{
			return 744;
		}
		if (objectType == KnownTypes.Types[237] || objectType == KnownTypes.Types[609])
		{
			return 746;
		}
		if (objectType == KnownTypes.Types[461])
		{
			return 747;
		}
		if (objectType == KnownTypes.Types[713])
		{
			return 752;
		}
		if (objectType == KnownTypes.Types[472])
		{
			return 748;
		}
		if (objectType == KnownTypes.Types[313])
		{
			return 745;
		}
		return 0;
	}

	internal Type GetXamlSerializerForType(Type objectType)
	{
		if (objectType == KnownTypes.Types[620])
		{
			return typeof(XamlStyleSerializer);
		}
		if (KnownTypes.Types[231].IsAssignableFrom(objectType))
		{
			return typeof(XamlTemplateSerializer);
		}
		return null;
	}

	internal static Type GetInternalTypeHelperTypeFromAssembly(ParserContext pc)
	{
		Assembly streamCreatedAssembly = pc.StreamCreatedAssembly;
		if (streamCreatedAssembly == null)
		{
			return null;
		}
		Type type = streamCreatedAssembly.GetType("XamlGeneratedNamespace.GeneratedInternalTypeHelper");
		if (type == null)
		{
			RootNamespaceAttribute rootNamespaceAttribute = (RootNamespaceAttribute)Attribute.GetCustomAttribute(streamCreatedAssembly, typeof(RootNamespaceAttribute));
			if (rootNamespaceAttribute != null)
			{
				string @namespace = rootNamespaceAttribute.Namespace;
				type = streamCreatedAssembly.GetType(@namespace + ".XamlGeneratedNamespace.GeneratedInternalTypeHelper");
			}
		}
		return type;
	}

	private static InternalTypeHelper GetInternalTypeHelperFromAssembly(ParserContext pc)
	{
		InternalTypeHelper result = null;
		Type internalTypeHelperTypeFromAssembly = GetInternalTypeHelperTypeFromAssembly(pc);
		if (internalTypeHelperTypeFromAssembly != null)
		{
			result = (InternalTypeHelper)Activator.CreateInstance(internalTypeHelperTypeFromAssembly);
		}
		return result;
	}

	internal static object CreateInternalInstance(ParserContext pc, Type type)
	{
		return Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, null, TypeConverterHelper.InvariantEnglishUS);
	}

	internal static object GetInternalPropertyValue(ParserContext pc, object rootElement, PropertyInfo pi, object target)
	{
		object result = null;
		bool isPublic = false;
		bool allowProtected = rootElement is IComponentConnector && rootElement == target;
		if (IsAllowedPropertyGet(pi, allowProtected, out isPublic))
		{
			result = pi.GetValue(target, BindingFlags.Default, null, null, TypeConverterHelper.InvariantEnglishUS);
		}
		return result;
	}

	internal static bool SetInternalPropertyValue(ParserContext pc, object rootElement, PropertyInfo pi, object target, object value)
	{
		bool isPublic = false;
		bool allowProtected = rootElement is IComponentConnector && rootElement == target;
		if (IsAllowedPropertySet(pi, allowProtected, out isPublic))
		{
			pi.SetValue(target, value, BindingFlags.Default, null, null, TypeConverterHelper.InvariantEnglishUS);
			return true;
		}
		return false;
	}

	internal static Delegate CreateDelegate(ParserContext pc, Type delegateType, object target, string handler)
	{
		Delegate result = null;
		if (ReflectionHelper.IsPublicType(delegateType) || ReflectionHelper.IsInternalType(delegateType))
		{
			result = Delegate.CreateDelegate(delegateType, target, handler);
		}
		return result;
	}

	internal static bool AddInternalEventHandler(ParserContext pc, object rootElement, EventInfo eventInfo, object target, Delegate handler)
	{
		bool isPublic = false;
		bool allowProtected = rootElement == target;
		if (IsAllowedEvent(eventInfo, allowProtected, out isPublic))
		{
			eventInfo.AddEventHandler(target, handler);
			return true;
		}
		return false;
	}

	internal bool IsLocalAssembly(string namespaceUri)
	{
		return false;
	}

	internal Type GetTypeFromBaseString(string typeString, ParserContext context, bool throwOnError)
	{
		string empty = string.Empty;
		Type type = null;
		int num = typeString.IndexOf(':');
		if (num == -1)
		{
			empty = context.XmlnsDictionary[string.Empty];
			if (empty == null)
			{
				ThrowException("ParserUndeclaredNS", string.Empty);
			}
		}
		else
		{
			string text = typeString.Substring(0, num);
			empty = context.XmlnsDictionary[text];
			if (empty == null)
			{
				ThrowException("ParserUndeclaredNS", text);
			}
			else
			{
				typeString = typeString.Substring(num + 1);
			}
		}
		if (string.Equals(empty, "http://schemas.microsoft.com/winfx/2006/xaml/presentation", StringComparison.Ordinal))
		{
			switch (typeString)
			{
			case "SystemParameters":
				type = typeof(SystemParameters);
				break;
			case "SystemColors":
				type = typeof(SystemColors);
				break;
			case "SystemFonts":
				type = typeof(SystemFonts);
				break;
			}
		}
		if (type == null)
		{
			type = GetType(empty, typeString);
		}
		if (type == null && throwOnError && !IsLocalAssembly(empty))
		{
			_lineNumber = context?.LineNumber ?? 0;
			_linePosition = context?.LinePosition ?? 0;
			ThrowException("ParserResourceKeyType", typeString);
		}
		return type;
	}

	private TypeInformationCacheData GetCachedInformationForType(Type type)
	{
		TypeInformationCacheData typeInformationCacheData = _typeInformationCache[type] as TypeInformationCacheData;
		if (typeInformationCacheData == null)
		{
			typeInformationCacheData = new TypeInformationCacheData(type.BaseType);
			typeInformationCacheData.ClrNamespace = type.Namespace;
			_typeInformationCache[type] = typeInformationCacheData;
		}
		return typeInformationCacheData;
	}

	private Type GetCachedBaseType(Type t)
	{
		return GetCachedInformationForType(t).BaseType;
	}

	internal static string ProcessNameString(ParserContext parserContext, ref string nameString)
	{
		int num = nameString.IndexOf(':');
		string text = string.Empty;
		if (num != -1)
		{
			text = nameString.Substring(0, num);
			nameString = nameString.Substring(num + 1);
		}
		string text2 = parserContext.XmlnsDictionary[text];
		if (text2 == null)
		{
			parserContext.XamlTypeMapper.ThrowException("ParserPrefixNSProperty", text, nameString);
		}
		return text2;
	}

	internal static DependencyProperty ParsePropertyName(ParserContext parserContext, string propertyName, ref Type ownerType)
	{
		string xmlNamespace = ProcessNameString(parserContext, ref propertyName);
		return parserContext.XamlTypeMapper.DependencyPropertyFromName(propertyName, xmlNamespace, ref ownerType);
	}

	internal static RoutedEvent ParseEventName(ParserContext parserContext, string eventName, Type ownerType)
	{
		string xmlNamespace = ProcessNameString(parserContext, ref eventName);
		return parserContext.XamlTypeMapper.GetRoutedEvent(ownerType, xmlNamespace, eventName);
	}

	internal object CreateInstance(Type t)
	{
		object obj = null;
		short knownTypeIdFromType = BamlMapTable.GetKnownTypeIdFromType(t);
		if (knownTypeIdFromType < 0)
		{
			return MapTable.CreateKnownTypeFromId(knownTypeIdFromType);
		}
		return Activator.CreateInstance(t, BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, null, null, TypeConverterHelper.InvariantEnglishUS);
	}

	internal bool IsXmlNamespaceKnown(string xmlNamespace, out string newXmlNamespace)
	{
		bool result;
		if (string.IsNullOrEmpty(xmlNamespace))
		{
			result = false;
			newXmlNamespace = null;
		}
		else
		{
			NamespaceMapEntry[] namespaceMapEntries = GetNamespaceMapEntries(xmlNamespace);
			if (_xmlnsCache == null)
			{
				_xmlnsCache = new XmlnsCache();
			}
			newXmlNamespace = _xmlnsCache.GetNewXmlnamespace(xmlNamespace);
			result = (namespaceMapEntries != null && namespaceMapEntries.Length != 0) || !string.IsNullOrEmpty(newXmlNamespace);
		}
		return result;
	}

	internal void SetUriToAssemblyNameMapping(string xmlNamespace, short[] assemblyIds)
	{
		if (xmlNamespace.StartsWith("clr-namespace:", StringComparison.Ordinal))
		{
			return;
		}
		if (_xmlnsCache == null)
		{
			_xmlnsCache = new XmlnsCache();
		}
		string[] array = null;
		if (assemblyIds != null && assemblyIds.Length != 0)
		{
			array = new string[assemblyIds.Length];
			for (int i = 0; i < assemblyIds.Length; i++)
			{
				BamlAssemblyInfoRecord assemblyInfoFromId = MapTable.GetAssemblyInfoFromId(assemblyIds[i]);
				array[i] = assemblyInfoFromId.AssemblyFullName;
			}
		}
		_xmlnsCache.SetUriToAssemblyNameMapping(xmlNamespace, array);
	}

	internal NamespaceMapEntry[] GetNamespaceMapEntries(string xmlNamespace)
	{
		NamespaceMapEntry[] array = null;
		array = _namespaceMapHashList[xmlNamespace] as NamespaceMapEntry[];
		if (array == null)
		{
			ArrayList arrayList = new ArrayList(6);
			if (_namespaceMaps != null)
			{
				for (int i = 0; i < _namespaceMaps.Length; i++)
				{
					NamespaceMapEntry namespaceMapEntry = _namespaceMaps[i];
					if (namespaceMapEntry.XmlNamespace == xmlNamespace)
					{
						arrayList.Add(namespaceMapEntry);
					}
				}
			}
			List<ClrNamespaceAssemblyPair> list;
			if (PITable.Contains(xmlNamespace))
			{
				list = new List<ClrNamespaceAssemblyPair>(1);
				list.Add((ClrNamespaceAssemblyPair)PITable[xmlNamespace]);
			}
			else
			{
				list = GetClrNamespacePairFromCache(xmlNamespace);
			}
			if (list != null)
			{
				for (int j = 0; j < list.Count; j++)
				{
					ClrNamespaceAssemblyPair clrNamespaceAssemblyPair = list[j];
					string text = null;
					string assemblyPath = AssemblyPathFor(clrNamespaceAssemblyPair.AssemblyName);
					if (!string.IsNullOrEmpty(clrNamespaceAssemblyPair.AssemblyName) && !string.IsNullOrEmpty(clrNamespaceAssemblyPair.ClrNamespace))
					{
						text = clrNamespaceAssemblyPair.AssemblyName;
						NamespaceMapEntry value = new NamespaceMapEntry(xmlNamespace, text, clrNamespaceAssemblyPair.ClrNamespace, assemblyPath);
						arrayList.Add(value);
					}
					if (string.IsNullOrEmpty(clrNamespaceAssemblyPair.ClrNamespace))
					{
						continue;
					}
					for (int k = 0; k < _assemblyNames.Length; k++)
					{
						if (text == null)
						{
							arrayList.Add(new NamespaceMapEntry(xmlNamespace, _assemblyNames[k], clrNamespaceAssemblyPair.ClrNamespace, assemblyPath));
							continue;
						}
						int num = _assemblyNames[k].LastIndexOf('\\');
						if (num > 0 && _assemblyNames[k].Substring(num + 1) == text)
						{
							arrayList.Add(new NamespaceMapEntry(xmlNamespace, _assemblyNames[k], clrNamespaceAssemblyPair.ClrNamespace, assemblyPath));
						}
					}
				}
			}
			array = (NamespaceMapEntry[])arrayList.ToArray(typeof(NamespaceMapEntry));
			if (array != null)
			{
				_namespaceMapHashList.Add(xmlNamespace, array);
			}
		}
		return array;
	}

	internal string GetXmlNamespace(string clrNamespaceFullName, string assemblyFullName)
	{
		string text = assemblyFullName.ToUpper(TypeConverterHelper.InvariantEnglishUS);
		string key = clrNamespaceFullName + "#" + text;
		if (_piReverseTable.TryGetValue(key, out var value) && value != null)
		{
			return value;
		}
		return string.Empty;
	}

	private string GetCachedNamespace(Type t)
	{
		return GetCachedInformationForType(t).ClrNamespace;
	}

	internal static List<ClrNamespaceAssemblyPair> GetClrNamespacePairFromCache(string namespaceUri)
	{
		if (_xmlnsCache == null)
		{
			_xmlnsCache = new XmlnsCache();
		}
		return _xmlnsCache.GetMappingArray(namespaceUri);
	}

	internal Type GetTypeConverterType(Type type)
	{
		TypeInformationCacheData cachedInformationForType = GetCachedInformationForType(type);
		Type type2 = null;
		if (null != cachedInformationForType.TypeConverterType)
		{
			return cachedInformationForType.TypeConverterType;
		}
		type2 = MapTable.GetKnownConverterTypeFromType(type);
		if (type2 == null)
		{
			type2 = TypeConverterHelper.GetConverterType(type);
			if (type2 == null)
			{
				type2 = TypeConverterHelper.GetCoreConverterTypeFromCustomType(type);
			}
		}
		cachedInformationForType.TypeConverterType = type2;
		return type2;
	}

	internal TypeConverter GetTypeConverter(Type type)
	{
		TypeInformationCacheData cachedInformationForType = GetCachedInformationForType(type);
		TypeConverter typeConverter = null;
		if (cachedInformationForType.Converter != null)
		{
			return cachedInformationForType.Converter;
		}
		typeConverter = MapTable.GetKnownConverterFromType(type);
		if (typeConverter == null)
		{
			Type converterType = TypeConverterHelper.GetConverterType(type);
			typeConverter = ((!(converterType == null)) ? (CreateInstance(converterType) as TypeConverter) : TypeConverterHelper.GetCoreConverterFromCustomType(type));
		}
		cachedInformationForType.Converter = typeConverter;
		if (typeConverter == null)
		{
			ThrowException("ParserNoTypeConv", type.Name);
		}
		return typeConverter;
	}

	internal Type GetPropertyConverterType(Type propType, object dpOrPiOrMi)
	{
		Type result = null;
		if (dpOrPiOrMi != null)
		{
			MemberInfo memberInfoForPropertyConverter = TypeConverterHelper.GetMemberInfoForPropertyConverter(dpOrPiOrMi);
			if (memberInfoForPropertyConverter != null)
			{
				result = TypeConverterHelper.GetConverterType(memberInfoForPropertyConverter);
			}
		}
		return result;
	}

	internal TypeConverter GetPropertyConverter(Type propType, object dpOrPiOrMi)
	{
		TypeConverter typeConverter = null;
		TypeInformationCacheData cachedInformationForType = GetCachedInformationForType(propType);
		if (dpOrPiOrMi != null)
		{
			object obj = cachedInformationForType.PropertyConverters[dpOrPiOrMi];
			if (obj != null)
			{
				return (TypeConverter)obj;
			}
			MemberInfo memberInfoForPropertyConverter = TypeConverterHelper.GetMemberInfoForPropertyConverter(dpOrPiOrMi);
			if (memberInfoForPropertyConverter != null)
			{
				Type converterType = TypeConverterHelper.GetConverterType(memberInfoForPropertyConverter);
				if (converterType != null)
				{
					typeConverter = CreateInstance(converterType) as TypeConverter;
				}
			}
		}
		if (typeConverter == null)
		{
			typeConverter = GetTypeConverter(propType);
		}
		if (dpOrPiOrMi != null)
		{
			cachedInformationForType.SetPropertyConverter(dpOrPiOrMi, typeConverter);
		}
		return typeConverter;
	}

	internal object GetDictionaryKey(string keyString, ParserContext context)
	{
		if (keyString.Length > 0 && (char.IsWhiteSpace(keyString[0]) || char.IsWhiteSpace(keyString[keyString.Length - 1])))
		{
			keyString = keyString.Trim();
		}
		return keyString;
	}

	internal ConstructorData GetConstructors(Type type)
	{
		if (_constructorInformationCache == null)
		{
			_constructorInformationCache = new HybridDictionary(3);
		}
		if (!_constructorInformationCache.Contains(type))
		{
			_constructorInformationCache[type] = new ConstructorData(type.GetConstructors(BindingFlags.Instance | BindingFlags.Public));
		}
		return (ConstructorData)_constructorInformationCache[type];
	}

	internal bool GetCachedTrimSurroundingWhitespace(Type t)
	{
		TypeInformationCacheData cachedInformationForType = GetCachedInformationForType(t);
		if (!cachedInformationForType.TrimSurroundingWhitespaceSet)
		{
			cachedInformationForType.TrimSurroundingWhitespace = GetTrimSurroundingWhitespace(t);
			cachedInformationForType.TrimSurroundingWhitespaceSet = true;
		}
		return cachedInformationForType.TrimSurroundingWhitespace;
	}

	private bool GetTrimSurroundingWhitespace(Type type)
	{
		if (null != type && (type.GetCustomAttributes(typeof(TrimSurroundingWhitespaceAttribute), inherit: true) as TrimSurroundingWhitespaceAttribute[]).Length != 0)
		{
			return true;
		}
		return false;
	}

	private void ThrowException(string id)
	{
		ThrowExceptionWithLine(SR.GetResourceString(id), null);
	}

	internal void ThrowException(string id, string parameter)
	{
		ThrowExceptionWithLine(SR.Format(SR.GetResourceString(id), parameter), null);
	}

	private void ThrowException(string id, string parameter1, string parameter2)
	{
		ThrowExceptionWithLine(SR.Format(SR.GetResourceString(id), parameter1, parameter2), null);
	}

	private void ThrowException(string id, string parameter1, string parameter2, string parameter3)
	{
		ThrowExceptionWithLine(SR.Format(SR.GetResourceString(id), parameter1, parameter2, parameter3), null);
	}

	internal void ThrowExceptionWithLine(string message, Exception innerException)
	{
		XamlParseException.ThrowException(message, innerException, _lineNumber, _linePosition);
	}
}
