using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;

namespace System.Windows.Baml2006;

internal class WpfKnownType : WpfXamlType, ICustomAttributeProvider
{
	[Flags]
	private enum BoolTypeBits
	{
		Frozen = 4,
		WhitespaceSignificantCollection = 8,
		UsableDurintInit = 0x10,
		HasSpecialValueConverter = 0x20
	}

	private static Attribute[] s_EmptyAttributes;

	private short _bamlNumber;

	private string _name;

	private Type _underlyingType;

	private string _contentPropertyName;

	private string _runtimeNamePropertyName;

	private string _dictionaryKeyPropertyName;

	private string _xmlLangPropertyName;

	private string _uidPropertyName;

	private Func<object> _defaultConstructor;

	private Type _deferringLoader;

	private Type _typeConverterType;

	private XamlCollectionKind _collectionKind;

	private Dictionary<int, Baml6ConstructorInfo> _constructors;

	private bool Frozen
	{
		get
		{
			return WpfXamlType.GetFlag(ref _bitField, 4);
		}
		set
		{
			WpfXamlType.SetFlag(ref _bitField, 4, value);
		}
	}

	public bool WhitespaceSignificantCollection
	{
		get
		{
			return WpfXamlType.GetFlag(ref _bitField, 8);
		}
		set
		{
			CheckFrozen();
			WpfXamlType.SetFlag(ref _bitField, 8, value);
		}
	}

	public bool IsUsableDuringInit
	{
		get
		{
			return WpfXamlType.GetFlag(ref _bitField, 16);
		}
		set
		{
			CheckFrozen();
			WpfXamlType.SetFlag(ref _bitField, 16, value);
		}
	}

	public bool HasSpecialValueConverter
	{
		get
		{
			return WpfXamlType.GetFlag(ref _bitField, 32);
		}
		set
		{
			CheckFrozen();
			WpfXamlType.SetFlag(ref _bitField, 32, value);
		}
	}

	public short BamlNumber => _bamlNumber;

	public string ContentPropertyName
	{
		get
		{
			return _contentPropertyName;
		}
		set
		{
			CheckFrozen();
			_contentPropertyName = value;
		}
	}

	public string RuntimeNamePropertyName
	{
		get
		{
			return _runtimeNamePropertyName;
		}
		set
		{
			CheckFrozen();
			_runtimeNamePropertyName = value;
		}
	}

	public string XmlLangPropertyName
	{
		get
		{
			return _xmlLangPropertyName;
		}
		set
		{
			CheckFrozen();
			_xmlLangPropertyName = value;
		}
	}

	public string UidPropertyName
	{
		get
		{
			return _uidPropertyName;
		}
		set
		{
			CheckFrozen();
			_uidPropertyName = value;
		}
	}

	public string DictionaryKeyPropertyName
	{
		get
		{
			return _dictionaryKeyPropertyName;
		}
		set
		{
			CheckFrozen();
			_dictionaryKeyPropertyName = value;
		}
	}

	public XamlCollectionKind CollectionKind
	{
		get
		{
			return _collectionKind;
		}
		set
		{
			CheckFrozen();
			_collectionKind = value;
		}
	}

	public Func<object> DefaultConstructor
	{
		get
		{
			return _defaultConstructor;
		}
		set
		{
			CheckFrozen();
			_defaultConstructor = value;
		}
	}

	public Type TypeConverterType
	{
		get
		{
			return _typeConverterType;
		}
		set
		{
			CheckFrozen();
			_typeConverterType = value;
		}
	}

	public Type DeferringLoaderType
	{
		get
		{
			return _deferringLoader;
		}
		set
		{
			CheckFrozen();
			_deferringLoader = value;
		}
	}

	public Dictionary<int, Baml6ConstructorInfo> Constructors
	{
		get
		{
			if (_constructors == null)
			{
				_constructors = new Dictionary<int, Baml6ConstructorInfo>();
			}
			return _constructors;
		}
	}

	public WpfKnownType(XamlSchemaContext schema, int bamlNumber, string name, Type underlyingType)
		: this(schema, bamlNumber, name, underlyingType, isBamlType: true, useV3Rules: true)
	{
	}

	public WpfKnownType(XamlSchemaContext schema, int bamlNumber, string name, Type underlyingType, bool isBamlType, bool useV3Rules)
		: base(underlyingType, schema, isBamlType, useV3Rules)
	{
		_bamlNumber = (short)bamlNumber;
		_name = name;
		_underlyingType = underlyingType;
	}

	public void Freeze()
	{
		Frozen = true;
	}

	private void CheckFrozen()
	{
		if (Frozen)
		{
			throw new InvalidOperationException("Can't Assign to Known Type attributes");
		}
	}

	protected override XamlMember LookupContentProperty()
	{
		return CallGetMember(_contentPropertyName);
	}

	protected override XamlMember LookupAliasedProperty(XamlDirective directive)
	{
		if (directive == XamlLanguage.Name)
		{
			return CallGetMember(_runtimeNamePropertyName);
		}
		if (directive == XamlLanguage.Key && _dictionaryKeyPropertyName != null)
		{
			return LookupMember(_dictionaryKeyPropertyName, skipReadOnlyCheck: true);
		}
		if (directive == XamlLanguage.Lang)
		{
			return CallGetMember(_xmlLangPropertyName);
		}
		if (directive == XamlLanguage.Uid)
		{
			return CallGetMember(_uidPropertyName);
		}
		return null;
	}

	protected override XamlCollectionKind LookupCollectionKind()
	{
		return _collectionKind;
	}

	protected override bool LookupIsWhitespaceSignificantCollection()
	{
		return WhitespaceSignificantCollection;
	}

	protected override XamlValueConverter<TypeConverter> LookupTypeConverter()
	{
		WpfSharedBamlSchemaContext bamlSharedSchemaContext = System.Windows.Markup.XamlReader.BamlSharedSchemaContext;
		if (_typeConverterType != null)
		{
			return bamlSharedSchemaContext.GetTypeConverter(_typeConverterType);
		}
		if (HasSpecialValueConverter)
		{
			return base.LookupTypeConverter();
		}
		return null;
	}

	protected override XamlValueConverter<XamlDeferringLoader> LookupDeferringLoader()
	{
		if (_deferringLoader != null)
		{
			return System.Windows.Markup.XamlReader.BamlSharedSchemaContext.GetDeferringLoader(_deferringLoader);
		}
		return null;
	}

	protected override EventHandler<XamlSetMarkupExtensionEventArgs> LookupSetMarkupExtensionHandler()
	{
		if (typeof(Setter).IsAssignableFrom(_underlyingType))
		{
			return Setter.ReceiveMarkupExtension;
		}
		if (typeof(DataTrigger).IsAssignableFrom(_underlyingType))
		{
			return DataTrigger.ReceiveMarkupExtension;
		}
		if (typeof(Condition).IsAssignableFrom(_underlyingType))
		{
			return Condition.ReceiveMarkupExtension;
		}
		return null;
	}

	protected override EventHandler<XamlSetTypeConverterEventArgs> LookupSetTypeConverterHandler()
	{
		if (typeof(Setter).IsAssignableFrom(_underlyingType))
		{
			return Setter.ReceiveTypeConverter;
		}
		if (typeof(Trigger).IsAssignableFrom(_underlyingType))
		{
			return Trigger.ReceiveTypeConverter;
		}
		if (typeof(Condition).IsAssignableFrom(_underlyingType))
		{
			return Condition.ReceiveTypeConverter;
		}
		return null;
	}

	protected override bool LookupUsableDuringInitialization()
	{
		return IsUsableDuringInit;
	}

	protected override XamlTypeInvoker LookupInvoker()
	{
		return new WpfKnownTypeInvoker(this);
	}

	private XamlMember CallGetMember(string name)
	{
		if (name != null)
		{
			return GetMember(name);
		}
		return null;
	}

	protected override IList<XamlType> LookupPositionalParameters(int paramCount)
	{
		if (base.IsMarkupExtension)
		{
			List<XamlType> list = null;
			Baml6ConstructorInfo value = Constructors[paramCount];
			if (Constructors.TryGetValue(paramCount, out value))
			{
				list = new List<XamlType>();
				foreach (Type type in value.Types)
				{
					list.Add(base.SchemaContext.GetXamlType(type));
				}
			}
			return list;
		}
		return base.LookupPositionalParameters(paramCount);
	}

	protected override ICustomAttributeProvider LookupCustomAttributeProvider()
	{
		return this;
	}

	object[] ICustomAttributeProvider.GetCustomAttributes(bool inherit)
	{
		return base.UnderlyingType.GetCustomAttributes(inherit);
	}

	object[] ICustomAttributeProvider.GetCustomAttributes(Type attributeType, bool inherit)
	{
		if (TryGetCustomAttribute(attributeType, out var result))
		{
			if (result != null)
			{
				return new Attribute[1] { result };
			}
			if (s_EmptyAttributes == null)
			{
				s_EmptyAttributes = Array.Empty<Attribute>();
			}
			return s_EmptyAttributes;
		}
		return base.UnderlyingType.GetCustomAttributes(attributeType, inherit);
	}

	private bool TryGetCustomAttribute(Type attributeType, out Attribute result)
	{
		bool result2 = true;
		if (attributeType == typeof(ContentPropertyAttribute))
		{
			result = ((_contentPropertyName == null) ? null : new ContentPropertyAttribute(_contentPropertyName));
		}
		else if (attributeType == typeof(RuntimeNamePropertyAttribute))
		{
			result = ((_runtimeNamePropertyName == null) ? null : new RuntimeNamePropertyAttribute(_runtimeNamePropertyName));
		}
		else if (attributeType == typeof(DictionaryKeyPropertyAttribute))
		{
			result = ((_dictionaryKeyPropertyName == null) ? null : new DictionaryKeyPropertyAttribute(_dictionaryKeyPropertyName));
		}
		else if (attributeType == typeof(XmlLangPropertyAttribute))
		{
			result = ((_xmlLangPropertyName == null) ? null : new XmlLangPropertyAttribute(_xmlLangPropertyName));
		}
		else if (attributeType == typeof(UidPropertyAttribute))
		{
			result = ((_uidPropertyName == null) ? null : new UidPropertyAttribute(_uidPropertyName));
		}
		else
		{
			result = null;
			result2 = false;
		}
		return result2;
	}

	bool ICustomAttributeProvider.IsDefined(Type attributeType, bool inherit)
	{
		return base.UnderlyingType.IsDefined(attributeType, inherit);
	}
}
