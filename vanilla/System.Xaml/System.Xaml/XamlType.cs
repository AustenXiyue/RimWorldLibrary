using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Markup;
using System.Xaml.MS.Impl;
using System.Xaml.Schema;
using MS.Internal.Xaml.Parser;

namespace System.Xaml;

/// <summary>Reports information about XAML types as part of the overall XAML system that is implemented in .NET Framework XAML Services.</summary>
public class XamlType : IEquatable<XamlType>
{
	internal static class EmptyList<T>
	{
		public static readonly ReadOnlyCollection<T> Value = new ReadOnlyCollection<T>(Array.Empty<T>());
	}

	private readonly string _name;

	private XamlSchemaContext _schemaContext;

	private readonly IList<XamlType> _typeArguments;

	private TypeReflector _reflector;

	private NullableReference<Type> _underlyingType;

	private ReadOnlyCollection<string> _namespaces;

	private ThreeValuedBool _isNameValid;

	/// <summary>Gets the <see cref="T:System.Xaml.XamlType" /> for the immediate base type of this XAML type. Determination of this value is based on the underlying type of this <see cref="T:System.Xaml.XamlType" /> and schema context.</summary>
	/// <returns>The <see cref="T:System.Xaml.XamlType" /> for the immediate base type of this XAML type.</returns>
	public XamlType BaseType
	{
		get
		{
			EnsureReflector();
			if (!_reflector.BaseTypeIsSet)
			{
				_reflector.BaseType = LookupBaseType();
			}
			return _reflector.BaseType;
		}
	}

	/// <summary>Gets the <see cref="T:System.Xaml.Schema.XamlTypeInvoker" /> implementation that is associated with this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>The <see cref="T:System.Xaml.Schema.XamlTypeInvoker" /> implementation that is associated with this <see cref="T:System.Xaml.XamlType" />.</returns>
	public XamlTypeInvoker Invoker
	{
		get
		{
			EnsureReflector();
			if (_reflector.Invoker == null)
			{
				_reflector.Invoker = LookupInvoker() ?? XamlTypeInvoker.UnknownInvoker;
			}
			return _reflector.Invoker;
		}
	}

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> is initialized by using a valid xamlName string as its <see cref="P:System.Xaml.XamlType.Name" />.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> is initialized by using a valid xamlName string; otherwise, false.</returns>
	public bool IsNameValid
	{
		get
		{
			if (_isNameValid == ThreeValuedBool.NotSet)
			{
				_isNameValid = ((!XamlName.IsValidXamlName(_name)) ? ThreeValuedBool.False : ThreeValuedBool.True);
			}
			return _isNameValid == ThreeValuedBool.True;
		}
	}

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> represents a type that cannot be resolved in the underlying type system.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> represents an unresolvable type; otherwise, false.</returns>
	public bool IsUnknown
	{
		get
		{
			EnsureReflector();
			return _reflector.IsUnknown;
		}
	}

	/// <summary>Gets the string name of the type that this <see cref="T:System.Xaml.XamlType" /> represents.</summary>
	/// <returns>The string name of this XAML type.</returns>
	public string Name => _name;

	/// <summary>Gets the single XAML namespace that is the primary XAML namespace for this <see cref="T:System.Xaml.XamlType" />. </summary>
	/// <returns>The identifier, as a string, of the primary XAML namespace for this XAML type.</returns>
	public string PreferredXamlNamespace
	{
		get
		{
			IList<string> xamlNamespaces = GetXamlNamespaces();
			if (xamlNamespaces.Count > 0)
			{
				return xamlNamespaces[0];
			}
			return null;
		}
	}

	/// <summary>Gets a list of type arguments for cases where this <see cref="T:System.Xaml.XamlType" /> represents a generic.</summary>
	/// <returns>A list of type argument types; otherwise, null, if this <see cref="T:System.Xaml.XamlType" /> does not represent a generic.</returns>
	public IList<XamlType> TypeArguments => _typeArguments;

	/// <summary>Gets the CLR <see cref="T:System.Type" /> that underlies this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>The CLR <see cref="T:System.Type" /> that underlies this <see cref="T:System.Xaml.XamlType" />.</returns>
	public Type UnderlyingType
	{
		get
		{
			if (!_underlyingType.IsSet)
			{
				_underlyingType.SetIfNull(LookupUnderlyingType());
			}
			return _underlyingType.Value;
		}
	}

	internal NullableReference<Type> UnderlyingTypeInternal => _underlyingType;

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> must have arguments (generic constraints through x:TypeArguments, initialization text, or other XAML techniques) to construct a valid instance of the type.</summary>
	/// <returns>true if construction of an instance requires some argument value; otherwise, false. </returns>
	public bool ConstructionRequiresArguments => GetFlag(BoolTypeBits.ConstructionRequiresArguments);

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> represents an array.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> represents an array; otherwise, false.</returns>
	public bool IsArray => GetCollectionKind() == XamlCollectionKind.Array;

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> represents a collection.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> represents a collection; otherwise, false.</returns>
	public bool IsCollection => GetCollectionKind() == XamlCollectionKind.Collection;

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> represents a constructible type, as per the XAML definition.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> represents a constructible type; otherwise, false.</returns>
	public bool IsConstructible => GetFlag(BoolTypeBits.Constructible);

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> represents a dictionary, as per the XAML definition.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> represents a dictionary; otherwise, false.</returns>
	public bool IsDictionary => GetCollectionKind() == XamlCollectionKind.Dictionary;

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> represents a generic type.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> represents a generic type; otherwise, false.</returns>
	public bool IsGeneric => TypeArguments != null;

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> represents a markup extension.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> represents a markup extension; otherwise, false.</returns>
	public bool IsMarkupExtension => GetFlag(BoolTypeBits.MarkupExtension);

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> represents a XAML namescope, as per the XAML definition.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> represents a XAML namescope; otherwise, false.</returns>
	public bool IsNameScope => GetFlag(BoolTypeBits.NameScope);

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> represents a nullable type, as per the XAML definition.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> represents a nullable type; otherwise, false.</returns>
	public bool IsNullable => GetFlag(BoolTypeBits.Nullable);

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> represents a public type in the relevant type system.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> represents a public type; otherwise, false.</returns>
	public bool IsPublic => GetFlag(BoolTypeBits.Public);

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> is built top-down during XAML initialization.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> is built top-down during XAML initialization; otherwise, false. The default is false.</returns>
	public bool IsUsableDuringInitialization => GetFlag(BoolTypeBits.UsableDuringInitialization);

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> represents a whitespace significant collection, as per the XML definition.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> represents a whitespace significant collection; otherwise, false.</returns>
	public bool IsWhitespaceSignificantCollection => GetFlag(BoolTypeBits.WhitespaceSignificantCollection);

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> represents XML XDATA, as per the XAML definition.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> represents XDATA; otherwise, false.</returns>
	public bool IsXData => GetFlag(BoolTypeBits.XmlData);

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> has whitespace handling behavior for serialization that trims the surrounding whitespace in its content.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> represents a type that uses whitespace trimming; otherwise, false.</returns>
	public bool TrimSurroundingWhitespace => GetFlag(BoolTypeBits.TrimSurroundingWhitespace);

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> represents an ambient type, as per the XAML definition.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> represents an ambient type; otherwise, false.</returns>
	public bool IsAmbient => GetFlag(BoolTypeBits.Ambient);

	/// <summary>Gets a value that provides the type information for the key property of this <see cref="T:System.Xaml.XamlType" />, if the <see cref="T:System.Xaml.XamlType" /> represents a dictionary.</summary>
	/// <returns>A <see cref="T:System.Xaml.XamlType" /> object for the type of the key for dictionary usage, otherwise, null, if this <see cref="T:System.Xaml.XamlType" /> does not represent a dictionary.</returns>
	public XamlType KeyType
	{
		get
		{
			if (!IsDictionary)
			{
				return null;
			}
			if (_reflector.KeyType == null)
			{
				_reflector.KeyType = LookupKeyType() ?? XamlLanguage.Object;
			}
			return _reflector.KeyType;
		}
	}

	/// <summary>Gets a value that provides the type information for the Items property of this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>A <see cref="T:System.Xaml.XamlType" /> object for the type of the items in the collection; otherwise, null, if this <see cref="T:System.Xaml.XamlType" /> does not represent a collection.</returns>
	public XamlType ItemType
	{
		get
		{
			if (GetCollectionKind() == XamlCollectionKind.None)
			{
				return null;
			}
			if (_reflector.ItemType == null)
			{
				_reflector.ItemType = LookupItemType() ?? XamlLanguage.Object;
			}
			return _reflector.ItemType;
		}
	}

	/// <summary>Gets a read-only collection of the types that are usable as the <see cref="P:System.Xaml.XamlType.ContentProperty" /> value for this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>A read-only collection of possible content types.</returns>
	public IList<XamlType> AllowedContentTypes
	{
		get
		{
			XamlCollectionKind collectionKind = GetCollectionKind();
			if (collectionKind != XamlCollectionKind.Collection && collectionKind != XamlCollectionKind.Dictionary)
			{
				return null;
			}
			if (_reflector.AllowedContentTypes == null)
			{
				_reflector.AllowedContentTypes = LookupAllowedContentTypes() ?? EmptyList<XamlType>.Value;
			}
			return _reflector.AllowedContentTypes;
		}
	}

	/// <summary>Gets the types that are used to wrap content for a content property when it is not a strict type match, such as strings in a strongly typed Collection&lt;T&gt;.</summary>
	/// <returns>A read-only collection of possible content wrapper types; otherwise, null. See Remarks.</returns>
	public IList<XamlType> ContentWrappers
	{
		get
		{
			if (!IsCollection)
			{
				return null;
			}
			if (_reflector.ContentWrappers == null)
			{
				_reflector.ContentWrappers = LookupContentWrappers() ?? EmptyList<XamlType>.Value;
			}
			return _reflector.ContentWrappers;
		}
	}

	/// <summary>Gets a <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> with <see cref="T:System.ComponentModel.TypeConverter" /> constraint that represents type conversion behavior for values of this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>A <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> with <see cref="T:System.ComponentModel.TypeConverter" /> constraint that represents type conversion behavior for values of this <see cref="T:System.Xaml.XamlType" />.</returns>
	public XamlValueConverter<TypeConverter> TypeConverter
	{
		get
		{
			EnsureReflector();
			if (!_reflector.TypeConverterIsSet)
			{
				_reflector.TypeConverter = LookupTypeConverter();
			}
			return _reflector.TypeConverter;
		}
	}

	/// <summary>Gets a <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> with <see cref="T:System.Windows.Markup.ValueSerializer" /> constraint that represents value serialization behavior for values of this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>A <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> with <see cref="T:System.Windows.Markup.ValueSerializer" /> constraint that represents value serialization behavior for values of this <see cref="T:System.Xaml.XamlType" />; otherwise, null.</returns>
	public XamlValueConverter<ValueSerializer> ValueSerializer
	{
		get
		{
			EnsureReflector();
			if (!_reflector.ValueSerializerIsSet)
			{
				_reflector.ValueSerializer = LookupValueSerializer();
			}
			return _reflector.ValueSerializer;
		}
	}

	/// <summary>Gets the <see cref="T:System.Xaml.XamlMember" /> information for the content property of this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>
	///   <see cref="T:System.Xaml.XamlMember" /> information for the content property of this <see cref="T:System.Xaml.XamlType" />. May be null if no content property exists.</returns>
	public XamlMember ContentProperty
	{
		get
		{
			EnsureReflector();
			if (!_reflector.ContentPropertyIsSet)
			{
				_reflector.ContentProperty = LookupContentProperty();
			}
			return _reflector.ContentProperty;
		}
	}

	/// <summary>Gets the <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> that represents the deferred loading conversion behavior for this type.</summary>
	/// <returns>The <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> with <see cref="T:System.Xaml.XamlDeferringLoader" /> constraint that represents the deferred loading behavior for this type.</returns>
	public XamlValueConverter<XamlDeferringLoader> DeferringLoader
	{
		get
		{
			EnsureReflector();
			if (!_reflector.DeferringLoaderIsSet)
			{
				_reflector.DeferringLoader = LookupDeferringLoader();
			}
			return _reflector.DeferringLoader;
		}
	}

	/// <summary>Gets a value that provides the type information for the returned ProvideValue of this <see cref="T:System.Xaml.XamlType" />, if it represents a markup extension.</summary>
	/// <returns>A <see cref="T:System.Xaml.XamlType" /> object for the return type for markup extension usage; otherwise, null, if this <see cref="T:System.Xaml.XamlType" /> does not represent a markup extension.</returns>
	public XamlType MarkupExtensionReturnType
	{
		get
		{
			if (!IsMarkupExtension)
			{
				return null;
			}
			if (_reflector.MarkupExtensionReturnType == null)
			{
				_reflector.MarkupExtensionReturnType = LookupMarkupExtensionReturnType() ?? XamlLanguage.Object;
			}
			return _reflector.MarkupExtensionReturnType;
		}
	}

	/// <summary>Gets the active <see cref="T:System.Xaml.XamlSchemaContext" /> for processing this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>The active <see cref="T:System.Xaml.XamlSchemaContext" /> for processing this <see cref="T:System.Xaml.XamlType" />.</returns>
	public XamlSchemaContext SchemaContext => _schemaContext;

	internal bool IsUsableAsReadOnly
	{
		get
		{
			XamlCollectionKind collectionKind = GetCollectionKind();
			if (collectionKind != XamlCollectionKind.Collection && collectionKind != XamlCollectionKind.Dictionary)
			{
				return IsXData;
			}
			return true;
		}
	}

	internal MethodInfo IsReadOnlyMethod
	{
		get
		{
			if (ItemType == null || UnderlyingType == null)
			{
				return null;
			}
			if (!_reflector.IsReadOnlyMethodIsSet)
			{
				if (UnderlyingType != null && ItemType.UnderlyingType != null)
				{
					_reflector.IsReadOnlyMethod = CollectionReflector.GetIsReadOnlyMethod(UnderlyingType, ItemType.UnderlyingType);
				}
				else
				{
					_reflector.IsReadOnlyMethod = null;
				}
			}
			return _reflector.IsReadOnlyMethod;
		}
	}

	internal EventHandler<XamlSetMarkupExtensionEventArgs> SetMarkupExtensionHandler
	{
		get
		{
			if (!_reflector.XamlSetMarkupExtensionHandlerIsSet)
			{
				_reflector.XamlSetMarkupExtensionHandler = LookupSetMarkupExtensionHandler();
			}
			return _reflector.XamlSetMarkupExtensionHandler;
		}
	}

	internal EventHandler<XamlSetTypeConverterEventArgs> SetTypeConverterHandler
	{
		get
		{
			EnsureReflector();
			if (!_reflector.XamlSetTypeConverterHandlerIsSet)
			{
				_reflector.XamlSetTypeConverterHandler = LookupSetTypeConverterHandler();
			}
			return _reflector.XamlSetTypeConverterHandler;
		}
	}

	internal MethodInfo AddMethod
	{
		get
		{
			if (UnderlyingType == null)
			{
				return null;
			}
			EnsureReflector();
			if (!_reflector.AddMethodIsSet)
			{
				XamlCollectionKind collectionKind = GetCollectionKind();
				_reflector.AddMethod = CollectionReflector.LookupAddMethod(UnderlyingType, collectionKind);
			}
			return _reflector.AddMethod;
		}
	}

	internal MethodInfo GetEnumeratorMethod
	{
		get
		{
			if (GetCollectionKind() == XamlCollectionKind.None || UnderlyingType == null)
			{
				return null;
			}
			if (!_reflector.GetEnumeratorMethodIsSet)
			{
				_reflector.GetEnumeratorMethod = CollectionReflector.GetEnumeratorMethod(UnderlyingType);
			}
			return _reflector.GetEnumeratorMethod;
		}
	}

	private bool AreAttributesAvailable
	{
		get
		{
			EnsureReflector();
			if (!_reflector.CustomAttributeProviderIsSet)
			{
				_reflector.CustomAttributeProvider = LookupCustomAttributeProvider();
			}
			if (_reflector.CustomAttributeProvider == null)
			{
				return UnderlyingTypeInternal.Value != null;
			}
			return true;
		}
	}

	private BindingFlags ConstructorBindingFlags
	{
		get
		{
			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
			if (!IsPublic)
			{
				bindingFlags |= BindingFlags.NonPublic;
			}
			return bindingFlags;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlType" /> class based on a string name for the type.</summary>
	/// <param name="typeName">The name of the type to create.</param>
	/// <param name="typeArguments">The type arguments for a <see cref="T:System.Xaml.XamlType" /> that represents a generic type. Can be (and often is) null, which indicates that the represented type is not a generic type.</param>
	/// <param name="schemaContext">XAML schema context for XAML readers and XAML writers.</param>
	/// <exception cref="T:System.ArgumentNullException">One or more of <paramref name="typeName" /> or <paramref name="schemaContext" /> are null.</exception>
	protected XamlType(string typeName, IList<XamlType> typeArguments, XamlSchemaContext schemaContext)
	{
		_name = typeName ?? throw new ArgumentNullException("typeName");
		_schemaContext = schemaContext ?? throw new ArgumentNullException("schemaContext");
		_typeArguments = GetTypeArguments(typeArguments);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlType" /> class based on the XAML namespace and a string name for the type. This constructor is exclusively for analysis and XAML-node recording of type usages that are known to not have backing in the supporting type system and XAML schema context.</summary>
	/// <param name="unknownTypeNamespace">The XAML namespace for the type, as a string.</param>
	/// <param name="unknownTypeName">The name of the type in the provided <paramref name="unknownTypeNamespace" /> XAML namespace.</param>
	/// <param name="typeArguments">The type arguments for a <see cref="T:System.Xaml.XamlType" /> that represents a generic type. Can be (and often is) null, which indicates that the represented type is not a generic type.</param>
	/// <param name="schemaContext">XAML schema context for XAML readers or XAML writers.</param>
	/// <exception cref="T:System.ArgumentNullException">One or more of <paramref name="unknownTypeNamespace" />, <paramref name="unknownTypeName" />, or <paramref name="schemaContext" /> are null.</exception>
	public XamlType(string unknownTypeNamespace, string unknownTypeName, IList<XamlType> typeArguments, XamlSchemaContext schemaContext)
	{
		ArgumentNullException.ThrowIfNull(unknownTypeNamespace, "unknownTypeNamespace");
		_name = unknownTypeName ?? throw new ArgumentNullException("unknownTypeName");
		_namespaces = new ReadOnlyCollection<string>(new string[1] { unknownTypeNamespace });
		_schemaContext = schemaContext ?? throw new ArgumentNullException("schemaContext");
		_typeArguments = GetTypeArguments(typeArguments);
		_reflector = TypeReflector.UnknownReflector;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlType" /> class based on the underlying CLR type information.</summary>
	/// <param name="underlyingType">The underlying CLR <see cref="T:System.Type" /> for the XAML type to construct.</param>
	/// <param name="schemaContext">XAML schema context for XAML readers or XAML writers.</param>
	/// <exception cref="T:System.ArgumentNullException">One or more of <paramref name="underlyingType" /> or <paramref name="schemaContext" /> are null.</exception>
	public XamlType(Type underlyingType, XamlSchemaContext schemaContext)
		: this(underlyingType, schemaContext, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlType" /> class based on underlying type information and a <see cref="T:System.Xaml.Schema.XamlTypeInvoker" /> implementation.</summary>
	/// <param name="underlyingType">The underlying type for the XAML type to construct.</param>
	/// <param name="schemaContext">XAML schema context for the XAML reader.</param>
	/// <param name="invoker">The <see cref="T:System.Xaml.Schema.XamlTypeInvoker" /> implementation that handles run-time reflection calls against the <see cref="T:System.Xaml.XamlType" />.</param>
	/// <exception cref="T:System.ArgumentNullException">One or more of <paramref name="underlyingType" /> or <paramref name="schemaContext" /> are null.</exception>
	public XamlType(Type underlyingType, XamlSchemaContext schemaContext, XamlTypeInvoker invoker)
		: this(null, underlyingType, schemaContext, invoker, null)
	{
	}

	internal XamlType(string alias, Type underlyingType, XamlSchemaContext schemaContext, XamlTypeInvoker invoker, TypeReflector reflector)
	{
		ArgumentNullException.ThrowIfNull(underlyingType, "underlyingType");
		_reflector = reflector ?? new TypeReflector(underlyingType);
		_name = alias ?? GetTypeName(underlyingType);
		_schemaContext = schemaContext ?? throw new ArgumentNullException("schemaContext");
		_typeArguments = GetTypeArguments(underlyingType, schemaContext);
		_underlyingType.Value = underlyingType;
		_reflector.Invoker = invoker;
	}

	/// <summary>Returns a <see cref="T:System.Xaml.XamlMember" /> for a specific named member from this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>The <see cref="T:System.Xaml.XamlMember" /> information for the member, if such a member was found; otherwise, null.</returns>
	/// <param name="name">The name of the member to get (as a string).</param>
	public XamlMember GetMember(string name)
	{
		EnsureReflector();
		if (!_reflector.Members.TryGetValue(name, out var member) && !_reflector.Members.IsComplete)
		{
			member = LookupMember(name, skipReadOnlyCheck: false);
			return _reflector.Members.TryAdd(name, member);
		}
		return member;
	}

	/// <summary>Returns a collection that contains all the members that are exposed by this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>A collection that contains zero or more <see cref="T:System.Xaml.XamlMember" /> values.</returns>
	public ICollection<XamlMember> GetAllMembers()
	{
		EnsureReflector();
		if (!_reflector.Members.IsComplete)
		{
			IEnumerable<XamlMember> enumerable = LookupAllMembers();
			if (enumerable != null)
			{
				foreach (XamlMember item in enumerable)
				{
					_reflector.Members.TryAdd(item.Name, item);
				}
				_reflector.Members.IsComplete = true;
			}
		}
		return _reflector.Members.Values;
	}

	/// <summary>Returns the XAML member that is aliased to a XAML directive by this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>The aliased member, if found; otherwise, null.</returns>
	/// <param name="directive">The directive for which to find the aliased member.</param>
	public XamlMember GetAliasedProperty(XamlDirective directive)
	{
		EnsureReflector();
		if (!_reflector.TryGetAliasedProperty(directive, out var member))
		{
			member = LookupAliasedProperty(directive);
			_reflector.TryAddAliasedProperty(directive, member);
		}
		return member;
	}

	/// <summary>Returns a <see cref="T:System.Xaml.XamlMember" /> representing a specific named attachable member of this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>A <see cref="T:System.Xaml.XamlMember" /> object for the requested attachable member; otherwise, null, if no attachable member by that name exists.</returns>
	/// <param name="name">The name of the attachable member to get, in ownerTypeName.MemberName form.</param>
	public XamlMember GetAttachableMember(string name)
	{
		EnsureReflector();
		if (!_reflector.AttachableMembers.TryGetValue(name, out var member) && !_reflector.AttachableMembers.IsComplete)
		{
			member = LookupAttachableMember(name);
			return _reflector.AttachableMembers.TryAdd(name, member);
		}
		return member;
	}

	/// <summary>Returns a collection that contains all the attachable properties that are exposed by this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>A collection that contains zero or more <see cref="T:System.Xaml.XamlMember" /> values.</returns>
	public ICollection<XamlMember> GetAllAttachableMembers()
	{
		EnsureReflector();
		if (!_reflector.AttachableMembers.IsComplete)
		{
			IEnumerable<XamlMember> enumerable = LookupAllAttachableMembers();
			if (enumerable != null)
			{
				foreach (XamlMember item in enumerable)
				{
					_reflector.AttachableMembers.TryAdd(item.Name, item);
				}
			}
			_reflector.AttachableMembers.IsComplete = true;
		}
		return _reflector.AttachableMembers.Values;
	}

	/// <summary>Returns a value that indicates whether an instance of this <see cref="T:System.Xaml.XamlType" /> has the specified <see cref="T:System.Xaml.XamlType" /> in its list of assignable types.</summary>
	/// <returns>true if <paramref name="xamlType" /> is in the assignable types list; otherwise, false.</returns>
	/// <param name="xamlType">The type to check against the current <see cref="T:System.Xaml.XamlType" /> .</param>
	public virtual bool CanAssignTo(XamlType xamlType)
	{
		if ((object)xamlType == null)
		{
			return false;
		}
		Type underlyingType = xamlType.UnderlyingType;
		XamlType xamlType2 = this;
		do
		{
			Type underlyingType2 = xamlType2.UnderlyingType;
			if (underlyingType != null && underlyingType2 != null)
			{
				if (underlyingType2.Assembly.ReflectionOnly && underlyingType.Assembly == typeof(XamlType).Assembly)
				{
					return LooseTypeExtensions.IsAssemblyQualifiedNameAssignableFrom(underlyingType, underlyingType2);
				}
				return underlyingType.IsAssignableFrom(underlyingType2);
			}
			if (xamlType2 == xamlType)
			{
				return true;
			}
			xamlType2 = xamlType2.BaseType;
		}
		while (xamlType2 != null);
		return false;
	}

	/// <summary>For markup extension types, returns the types of the positional parameters that are supported in a specific markup extension usage for this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>A list of <see cref="T:System.Xaml.XamlType" /> values, where each <see cref="T:System.Xaml.XamlType" /> is the type for that position in the syntax. You must specify the types in the same order when you supply markup input for the markup extension.</returns>
	/// <param name="parameterCount">The count (arity) of the particular syntax or constructor mode that you want information about.</param>
	public IList<XamlType> GetPositionalParameters(int parameterCount)
	{
		EnsureReflector();
		if (!_reflector.TryGetPositionalParameters(parameterCount, out var result))
		{
			result = LookupPositionalParameters(parameterCount);
			return _reflector.TryAddPositionalParameters(parameterCount, result);
		}
		return result;
	}

	/// <summary>Returns a list of string identifiers for XAML namespaces that the type is included in.</summary>
	/// <returns>A list of string values, where each string is the URI identifier for a XAML namespace.</returns>
	public virtual IList<string> GetXamlNamespaces()
	{
		if (_namespaces == null)
		{
			_namespaces = _schemaContext.GetXamlNamespaces(this);
			if (_namespaces == null)
			{
				_namespaces = new ReadOnlyCollection<string>(new string[1] { string.Empty });
			}
		}
		return _namespaces;
	}

	/// <summary>Returns a string representation of this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>A string representation of this <see cref="T:System.Xaml.XamlType" />.</returns>
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		AppendTypeName(stringBuilder, forceNsInitialization: false);
		return stringBuilder.ToString();
	}

	internal string GetQualifiedName()
	{
		StringBuilder stringBuilder = new StringBuilder();
		AppendTypeName(stringBuilder, forceNsInitialization: true);
		return stringBuilder.ToString();
	}

	internal bool IsVisibleTo(Assembly accessingAssembly)
	{
		if (IsPublic)
		{
			return true;
		}
		Type underlyingType = UnderlyingType;
		if (accessingAssembly != null && underlyingType != null)
		{
			return TypeReflector.IsVisibleTo(underlyingType, accessingAssembly, SchemaContext);
		}
		return false;
	}

	internal ICollection<XamlMember> GetAllExcludedReadOnlyMembers()
	{
		EnsureReflector();
		if (_reflector.ExcludedReadOnlyMembers == null)
		{
			_reflector.ExcludedReadOnlyMembers = LookupAllExcludedReadOnlyMembers() ?? EmptyList<XamlMember>.Value;
		}
		return _reflector.ExcludedReadOnlyMembers;
	}

	internal IEnumerable<ConstructorInfo> GetConstructors()
	{
		if (UnderlyingType == null)
		{
			return EmptyList<ConstructorInfo>.Value;
		}
		if (IsPublic)
		{
			return UnderlyingType.GetConstructors();
		}
		return GetPublicAndInternalConstructors();
	}

	internal ConstructorInfo GetConstructor(Type[] paramTypes)
	{
		if (UnderlyingType == null)
		{
			return null;
		}
		IEnumerable<ConstructorInfo> constructors = GetConstructors();
		ConstructorInfo[] array = constructors as ConstructorInfo[];
		if (array == null)
		{
			array = new List<ConstructorInfo>(constructors).ToArray();
		}
		Binder defaultBinder = Type.DefaultBinder;
		BindingFlags constructorBindingFlags = ConstructorBindingFlags;
		MethodBase[] match = array;
		return (ConstructorInfo)defaultBinder.SelectMethod(constructorBindingFlags, match, paramTypes, null);
	}

	/// <summary>Returns the XAML member that is aliased to a XAML directive by this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>The aliased member, if found; otherwise, null.</returns>
	/// <param name="directive">The directive for which to find the aliased member.</param>
	protected virtual XamlMember LookupAliasedProperty(XamlDirective directive)
	{
		if (AreAttributesAvailable)
		{
			Type type = null;
			bool skipReadOnlyCheck = false;
			if (directive == XamlLanguage.Key)
			{
				type = typeof(DictionaryKeyPropertyAttribute);
				skipReadOnlyCheck = true;
			}
			else if (directive == XamlLanguage.Name)
			{
				type = typeof(RuntimeNamePropertyAttribute);
			}
			else if (directive == XamlLanguage.Uid)
			{
				type = typeof(UidPropertyAttribute);
			}
			else if (directive == XamlLanguage.Lang)
			{
				type = typeof(XmlLangPropertyAttribute);
			}
			if (type != null && TryGetAttributeString(type, out var result))
			{
				if (string.IsNullOrEmpty(result))
				{
					return null;
				}
				return GetPropertyOrUnknown(result, skipReadOnlyCheck);
			}
		}
		if (BaseType != null)
		{
			return BaseType.GetAliasedProperty(directive);
		}
		return null;
	}

	/// <summary>Returns a list of the types that are usable as the <see cref="P:System.Xaml.XamlType.ContentProperty" /> value for this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>A list of possible content types.</returns>
	protected virtual IList<XamlType> LookupAllowedContentTypes()
	{
		IList<XamlType> obj = ContentWrappers ?? EmptyList<XamlType>.Value;
		List<XamlType> list = new List<XamlType>(obj.Count + 1) { ItemType };
		foreach (XamlType item in obj)
		{
			if (item.ContentProperty != null && !item.ContentProperty.IsUnknown)
			{
				XamlType type = item.ContentProperty.Type;
				if (!list.Contains(type))
				{
					list.Add(type);
				}
			}
		}
		return list.AsReadOnly();
	}

	/// <summary>Returns the <see cref="T:System.Xaml.XamlType" /> for the immediate base type of this XAML type. Determination of this value is based on the underlying type of this <see cref="T:System.Xaml.XamlType" /> and schema context.</summary>
	/// <returns>The <see cref="T:System.Xaml.XamlType" /> for the immediate base type of this XAML type.</returns>
	protected virtual XamlType LookupBaseType()
	{
		Type underlyingType = UnderlyingType;
		if (underlyingType == null)
		{
			return XamlLanguage.Object;
		}
		if (underlyingType.BaseType != null)
		{
			return SchemaContext.GetXamlType(underlyingType.BaseType);
		}
		return null;
	}

	/// <summary>Returns a value of the <see cref="T:System.Xaml.Schema.XamlCollectionKind" /> enumeration that declares which specific collection type this <see cref="T:System.Xaml.XamlType" /> uses.</summary>
	/// <returns>A value of the <see cref="T:System.Xaml.Schema.XamlCollectionKind" /> enumeration. </returns>
	protected virtual XamlCollectionKind LookupCollectionKind()
	{
		if (UnderlyingType == null)
		{
			if (!(BaseType != null))
			{
				return XamlCollectionKind.None;
			}
			return BaseType.GetCollectionKind();
		}
		MethodInfo addMethod = null;
		XamlCollectionKind result = CollectionReflector.LookupCollectionKind(UnderlyingType, out addMethod);
		if (addMethod != null)
		{
			_reflector.AddMethod = addMethod;
		}
		return result;
	}

	/// <summary>Returns a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> must have arguments (generic constraints through x:TypeArguments, initialization text, or other XAML techniques) to construct a valid instance of the type.</summary>
	/// <returns>true if construction of an instance requires some argument value; otherwise, false. </returns>
	protected virtual bool LookupConstructionRequiresArguments()
	{
		Type underlyingType = UnderlyingType;
		if (underlyingType == null)
		{
			return GetDefaultFlag(BoolTypeBits.ConstructionRequiresArguments);
		}
		if (underlyingType.IsValueType)
		{
			return false;
		}
		ConstructorInfo constructor = underlyingType.GetConstructor(ConstructorBindingFlags, null, Type.EmptyTypes, null);
		if (!(constructor == null))
		{
			return !TypeReflector.IsPublicOrInternal(constructor);
		}
		return true;
	}

	/// <summary>Returns <see cref="T:System.Xaml.XamlMember" /> information for the content property of this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>
	///   <see cref="T:System.Xaml.XamlMember" /> information for the content property of this <see cref="T:System.Xaml.XamlType" />. May be null.</returns>
	protected virtual XamlMember LookupContentProperty()
	{
		if (TryGetAttributeString(typeof(ContentPropertyAttribute), out var result))
		{
			if (string.IsNullOrEmpty(result))
			{
				return null;
			}
			return GetPropertyOrUnknown(result, skipReadOnlyCheck: false);
		}
		if (BaseType != null)
		{
			return BaseType.ContentProperty;
		}
		return null;
	}

	/// <summary>Gets a list of <see cref="T:System.Xaml.XamlType" /> values that represent the content wrappers for this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>A list of <see cref="T:System.Xaml.XamlType" /> values that represent the content wrappers for this <see cref="T:System.Xaml.XamlType" />.</returns>
	protected virtual IList<XamlType> LookupContentWrappers()
	{
		List<XamlType> list = null;
		if (AreAttributesAvailable)
		{
			List<Type> allAttributeContents = _reflector.GetAllAttributeContents<Type>(typeof(ContentWrapperAttribute));
			if (allAttributeContents != null)
			{
				list = new List<XamlType>(allAttributeContents.Count);
				foreach (Type item in allAttributeContents)
				{
					list.Add(SchemaContext.GetXamlType(item));
				}
			}
		}
		if (BaseType != null)
		{
			IList<XamlType> contentWrappers = BaseType.ContentWrappers;
			if (list == null)
			{
				return contentWrappers;
			}
			if (contentWrappers != null)
			{
				list.AddRange(contentWrappers);
			}
		}
		return GetReadOnly(list);
	}

	/// <summary>When implemented in a derived class, returns an <see cref="T:System.Reflection.ICustomAttributeProvider" /> implementation.</summary>
	/// <returns>An <see cref="T:System.Reflection.ICustomAttributeProvider" /> implementation.</returns>
	protected virtual ICustomAttributeProvider LookupCustomAttributeProvider()
	{
		return null;
	}

	/// <summary>Returns a <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> object, which is used for deferred loading of XAML-declared objects.</summary>
	/// <returns>A <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> that has a <see cref="T:System.Xaml.XamlDeferringLoader" /> constraint on the generic.</returns>
	protected virtual XamlValueConverter<XamlDeferringLoader> LookupDeferringLoader()
	{
		if (AreAttributesAvailable)
		{
			Type[] attributeTypes = _reflector.GetAttributeTypes(typeof(XamlDeferLoadAttribute), 2);
			if (attributeTypes != null)
			{
				return SchemaContext.GetValueConverter<XamlDeferringLoader>(attributeTypes[0], null);
			}
		}
		if (BaseType != null)
		{
			return BaseType.DeferringLoader;
		}
		return null;
	}

	/// <summary>Returns a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> represents a constructible type, as per the XAML definition.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> represents a constructible type; otherwise, false.</returns>
	protected virtual bool LookupIsConstructible()
	{
		Type underlyingType = UnderlyingType;
		if (underlyingType == null)
		{
			return GetDefaultFlag(BoolTypeBits.Constructible);
		}
		if (underlyingType.IsAbstract || underlyingType.IsInterface || underlyingType.IsNested || underlyingType.IsGenericParameter || underlyingType.IsGenericTypeDefinition)
		{
			return false;
		}
		if (underlyingType.IsValueType)
		{
			return true;
		}
		if (!ConstructionRequiresArguments)
		{
			return true;
		}
		using (IEnumerator<ConstructorInfo> enumerator = GetConstructors().GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				_ = enumerator.Current;
				return true;
			}
		}
		return false;
	}

	/// <summary>Returns a <see cref="T:System.Xaml.Schema.XamlTypeInvoker" /> that is associated with this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>The <see cref="T:System.Xaml.Schema.XamlTypeInvoker" /> information for this <see cref="T:System.Xaml.XamlType" />; otherwise, null.</returns>
	protected virtual XamlTypeInvoker LookupInvoker()
	{
		if (!(UnderlyingType != null))
		{
			return null;
		}
		return new XamlTypeInvoker(this);
	}

	/// <summary>Returns a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> represents a markup extension.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> represents a markup extension; otherwise, false.</returns>
	protected virtual bool LookupIsMarkupExtension()
	{
		return CanAssignTo(XamlLanguage.MarkupExtension);
	}

	/// <summary>Returns a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> represents a XAML namescope, as per the XAML definition.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> represents a XAML namescope; otherwise, false.</returns>
	protected virtual bool LookupIsNameScope()
	{
		return CanAssignTo(XamlLanguage.INameScope);
	}

	/// <summary>Returns a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> represents a nullable type, as per the XAML definition.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> represents a nullable type; otherwise, false.</returns>
	protected virtual bool LookupIsNullable()
	{
		if (UnderlyingType != null)
		{
			if (UnderlyingType.IsValueType)
			{
				return IsNullableGeneric();
			}
			return true;
		}
		return GetDefaultFlag(BoolTypeBits.Nullable);
	}

	/// <summary>Returns a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> represents a type that cannot be resolved in the underlying type system.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> represents a nonresolvable type; otherwise, false.</returns>
	protected virtual bool LookupIsUnknown()
	{
		if (_reflector != null)
		{
			return _reflector.IsUnknown;
		}
		return UnderlyingType == null;
	}

	/// <summary>Returns a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> represents a whitespace significant collection, as per the XML definition.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> represents a white-space significant collection; otherwise, false.</returns>
	protected virtual bool LookupIsWhitespaceSignificantCollection()
	{
		if (AreAttributesAvailable && _reflector.IsAttributePresent(typeof(WhitespaceSignificantCollectionAttribute)))
		{
			return true;
		}
		if (BaseType != null)
		{
			return BaseType.IsWhitespaceSignificantCollection;
		}
		if (IsUnknown)
		{
			return _reflector.GetFlag(BoolTypeBits.WhitespaceSignificantCollection).Value;
		}
		return GetDefaultFlag(BoolTypeBits.WhitespaceSignificantCollection);
	}

	/// <summary>Returns a value that provides the type information for the key property of this <see cref="T:System.Xaml.XamlType" />, if the <see cref="T:System.Xaml.XamlType" /> represents a dictionary.</summary>
	/// <returns>A <see cref="T:System.Xaml.XamlType" /> object for the type of the key for dictionary usage, or null if this <see cref="T:System.Xaml.XamlType" /> does not represent a dictionary,</returns>
	protected virtual XamlType LookupKeyType()
	{
		MethodInfo addMethod = AddMethod;
		if (addMethod != null)
		{
			ParameterInfo[] parameters = addMethod.GetParameters();
			if (parameters.Length == 2)
			{
				return SchemaContext.GetXamlType(parameters[0].ParameterType);
			}
		}
		else if (UnderlyingType == null && BaseType != null)
		{
			return BaseType.KeyType;
		}
		return null;
	}

	/// <summary>Returns a value that provides the type information for the Items property of this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>A <see cref="T:System.Xaml.XamlType" /> object for the type of the items in the collection; otherwise, null if this <see cref="T:System.Xaml.XamlType" /> does not represent a collection.</returns>
	protected virtual XamlType LookupItemType()
	{
		Type type = null;
		MethodInfo addMethod = AddMethod;
		if (addMethod != null)
		{
			ParameterInfo[] parameters = addMethod.GetParameters();
			if (parameters.Length == 2)
			{
				type = parameters[1].ParameterType;
			}
			else if (parameters.Length == 1)
			{
				type = parameters[0].ParameterType;
			}
		}
		else if (UnderlyingType != null)
		{
			if (UnderlyingType.IsArray)
			{
				type = UnderlyingType.GetElementType();
			}
		}
		else if (BaseType != null)
		{
			return BaseType.ItemType;
		}
		if (!(type != null))
		{
			return null;
		}
		return SchemaContext.GetXamlType(type);
	}

	/// <summary>Returns a value that provides the type information for the returned ProvideValue of this <see cref="T:System.Xaml.XamlType" />, if it represents a markup extension.</summary>
	/// <returns>A <see cref="T:System.Xaml.XamlType" /> object for the return type for markup extension usage; otherwise, null, if this <see cref="T:System.Xaml.XamlType" /> does not represent a markup extension.</returns>
	protected virtual XamlType LookupMarkupExtensionReturnType()
	{
		if (AreAttributesAvailable)
		{
			Type attributeType = _reflector.GetAttributeType(typeof(MarkupExtensionReturnTypeAttribute));
			if (attributeType != null)
			{
				return SchemaContext.GetXamlType(attributeType);
			}
		}
		if (BaseType != null)
		{
			return BaseType.MarkupExtensionReturnType;
		}
		return null;
	}

	/// <summary>Returns an enumerable set that contains all attachable properties that are exposed by this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>An enumerable set that contains zero or more <see cref="T:System.Xaml.XamlMember" /> values; otherwise, null.</returns>
	protected virtual IEnumerable<XamlMember> LookupAllAttachableMembers()
	{
		if (UnderlyingType == null)
		{
			if (!(BaseType != null))
			{
				return null;
			}
			return BaseType.GetAllAttachableMembers();
		}
		EnsureReflector();
		return _reflector.LookupAllAttachableMembers(SchemaContext);
	}

	/// <summary>Returns an enumerable set that contains all the members that are exposed by this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>An enumerable set that contains zero or more <see cref="T:System.Xaml.XamlMember" /> values.</returns>
	protected virtual IEnumerable<XamlMember> LookupAllMembers()
	{
		if (UnderlyingType == null)
		{
			if (!(BaseType != null))
			{
				return null;
			}
			return BaseType.GetAllMembers();
		}
		EnsureReflector();
		_reflector.LookupAllMembers(out var newProperties, out var newEvents, out var knownMembers);
		if (newProperties != null)
		{
			foreach (PropertyInfo item in newProperties)
			{
				XamlMember property = SchemaContext.GetProperty(item);
				if (!property.IsReadOnly || property.Type.IsUsableAsReadOnly)
				{
					knownMembers.Add(property);
				}
			}
		}
		if (newEvents != null)
		{
			foreach (EventInfo item2 in newEvents)
			{
				XamlMember @event = SchemaContext.GetEvent(item2);
				knownMembers.Add(@event);
			}
		}
		return knownMembers;
	}

	/// <summary>Returns the <see cref="T:System.Xaml.XamlMember" /> for a specific named member from this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>The <see cref="T:System.Xaml.XamlMember" /> information for the member, if a member was found; otherwise, null.</returns>
	/// <param name="name">The name of the member to get (as a string).</param>
	/// <param name="skipReadOnlyCheck">true to return a member even if that member has a true value for <see cref="P:System.Xaml.XamlMember.IsReadOnly" />; false to not return a <see cref="P:System.Xaml.XamlMember.IsReadOnly" /> member. The default is false.</param>
	protected virtual XamlMember LookupMember(string name, bool skipReadOnlyCheck)
	{
		if (UnderlyingType == null)
		{
			if (BaseType != null)
			{
				if (!skipReadOnlyCheck)
				{
					return BaseType.GetMember(name);
				}
				return BaseType.LookupMember(name, skipReadOnlyCheck: true);
			}
			return null;
		}
		EnsureReflector();
		PropertyInfo propertyInfo = _reflector.LookupProperty(name);
		if (propertyInfo != null)
		{
			XamlMember property = SchemaContext.GetProperty(propertyInfo);
			if (!skipReadOnlyCheck && property.IsReadOnly && !property.Type.IsUsableAsReadOnly)
			{
				return null;
			}
			return property;
		}
		EventInfo eventInfo = _reflector.LookupEvent(name);
		if (eventInfo != null)
		{
			return SchemaContext.GetEvent(eventInfo);
		}
		return null;
	}

	/// <summary>Returns a <see cref="T:System.Xaml.XamlMember" /> for a specific named attachable from this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>A <see cref="T:System.Xaml.XamlMember" /> object for the requested attachable member; otherwise, null, if no attachable member by that name exists.</returns>
	/// <param name="name">The name of the attachable member to get, in ownerTypeName.MemberName form.</param>
	protected virtual XamlMember LookupAttachableMember(string name)
	{
		if (UnderlyingType == null)
		{
			if (!(BaseType != null))
			{
				return null;
			}
			return BaseType.GetAttachableMember(name);
		}
		EnsureReflector();
		if (_reflector.LookupAttachableProperty(name, out var getter, out var setter))
		{
			XamlMember attachableProperty = SchemaContext.GetAttachableProperty(name, getter, setter);
			if (attachableProperty.IsReadOnly && !attachableProperty.Type.IsUsableAsReadOnly)
			{
				return null;
			}
			return attachableProperty;
		}
		setter = _reflector.LookupAttachableEvent(name);
		if (setter != null)
		{
			return SchemaContext.GetAttachableEvent(name, setter);
		}
		return null;
	}

	/// <summary>For markup extension types, returns the types of the positional parameters that are supported in a specific markup extension usage for this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>A list of <see cref="T:System.Xaml.XamlType" /> values where each such <see cref="T:System.Xaml.XamlType" /> is the type for that position in the syntax. You must specify those types in the same order when supplying markup input for the markup extension.</returns>
	/// <param name="parameterCount">The count (arity) of the particular syntax or constructor mode that you want information about.</param>
	protected virtual IList<XamlType> LookupPositionalParameters(int parameterCount)
	{
		if (UnderlyingType == null)
		{
			return null;
		}
		EnsureReflector();
		if (_reflector.ReflectedPositionalParameters == null)
		{
			_reflector.ReflectedPositionalParameters = LookupAllPositionalParameters();
		}
		_reflector.ReflectedPositionalParameters.TryGetValue(parameterCount, out var value);
		return value;
	}

	/// <summary>Returns the CLR <see cref="T:System.Type" /> that underlies this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>The CLR <see cref="T:System.Type" /> that underlies this <see cref="T:System.Xaml.XamlType" />; otherwise, null.</returns>
	protected virtual Type LookupUnderlyingType()
	{
		return UnderlyingTypeInternal.Value;
	}

	/// <summary>Returns a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> represents a public type in the relevant type system.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> represents a public type; otherwise, false.</returns>
	protected virtual bool LookupIsPublic()
	{
		Type underlyingType = UnderlyingType;
		if (underlyingType == null)
		{
			return GetDefaultFlag(BoolTypeBits.Public);
		}
		return underlyingType.IsVisible;
	}

	/// <summary>Returns a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> represents XML XDATA, as per the XAML definition.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> represents XDATA; otherwise, false.</returns>
	protected virtual bool LookupIsXData()
	{
		return CanAssignTo(XamlLanguage.IXmlSerializable);
	}

	/// <summary>Returns a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> represents an ambient type, as per the XAML definition.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> represents an ambient type; otherwise, false.</returns>
	protected virtual bool LookupIsAmbient()
	{
		if (AreAttributesAvailable && _reflector.IsAttributePresent(typeof(AmbientAttribute)))
		{
			return true;
		}
		if (BaseType != null)
		{
			return BaseType.IsAmbient;
		}
		if (IsUnknown)
		{
			return _reflector.GetFlag(BoolTypeBits.Ambient).Value;
		}
		return GetDefaultFlag(BoolTypeBits.Ambient);
	}

	/// <summary>Returns a <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> that has a <see cref="T:System.ComponentModel.TypeConverter" /> constraint, which represents type-conversion behavior for values of this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>A <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> with <see cref="T:System.ComponentModel.TypeConverter" /> constraint that represents type-conversion behavior for values of this <see cref="T:System.Xaml.XamlType" />; otherwise, null.</returns>
	protected virtual XamlValueConverter<TypeConverter> LookupTypeConverter()
	{
		if (AreAttributesAvailable)
		{
			Type attributeType = _reflector.GetAttributeType(typeof(TypeConverterAttribute));
			if (attributeType != null)
			{
				return SchemaContext.GetValueConverter<TypeConverter>(attributeType, null);
			}
		}
		if (BaseType != null)
		{
			XamlValueConverter<TypeConverter> typeConverter = BaseType.TypeConverter;
			if (typeConverter != null && typeConverter.TargetType != XamlLanguage.Object)
			{
				return typeConverter;
			}
		}
		Type underlyingType = UnderlyingType;
		if (underlyingType != null)
		{
			if (underlyingType.IsEnum)
			{
				return SchemaContext.GetValueConverter<TypeConverter>(typeof(EnumConverter), this);
			}
			XamlValueConverter<TypeConverter> typeConverter2 = BuiltInValueConverter.GetTypeConverter(underlyingType);
			if (typeConverter2 != null)
			{
				return typeConverter2;
			}
			if (IsNullableGeneric())
			{
				Type[] genericArguments = underlyingType.GetGenericArguments();
				return SchemaContext.GetXamlType(genericArguments[0]).TypeConverter;
			}
		}
		return null;
	}

	/// <summary>Returns a <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> that has a <see cref="T:System.Windows.Markup.ValueSerializer" /> constraint, which represents value serialization behavior for values of this <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <returns>A <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> with <see cref="T:System.Windows.Markup.ValueSerializer" /> constraint that represents value serialization behavior for values of this <see cref="T:System.Xaml.XamlType" />; otherwise, null.</returns>
	protected virtual XamlValueConverter<ValueSerializer> LookupValueSerializer()
	{
		if (AreAttributesAvailable)
		{
			Type attributeType = _reflector.GetAttributeType(typeof(ValueSerializerAttribute));
			if (attributeType != null)
			{
				return SchemaContext.GetValueConverter<ValueSerializer>(attributeType, null);
			}
		}
		if (BaseType != null)
		{
			XamlValueConverter<ValueSerializer> valueSerializer = BaseType.ValueSerializer;
			if (valueSerializer != null)
			{
				return valueSerializer;
			}
		}
		Type underlyingType = UnderlyingType;
		if (underlyingType != null)
		{
			XamlValueConverter<ValueSerializer> valueSerializer2 = BuiltInValueConverter.GetValueSerializer(underlyingType);
			if (valueSerializer2 != null)
			{
				return valueSerializer2;
			}
			if (IsNullableGeneric())
			{
				Type[] genericArguments = underlyingType.GetGenericArguments();
				return SchemaContext.GetXamlType(genericArguments[0]).ValueSerializer;
			}
		}
		return null;
	}

	/// <summary>Returns a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> should be serialized using a mode that  trims surrounding whitespace.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> should be serialized in a mode that trims surrounding whitespace; otherwise, false.</returns>
	protected virtual bool LookupTrimSurroundingWhitespace()
	{
		if (AreAttributesAvailable && _reflector.IsAttributePresent(typeof(TrimSurroundingWhitespaceAttribute)))
		{
			return true;
		}
		if (BaseType != null)
		{
			return BaseType.TrimSurroundingWhitespace;
		}
		return GetDefaultFlag(BoolTypeBits.TrimSurroundingWhitespace);
	}

	/// <summary>Returns a value that indicates whether this <see cref="T:System.Xaml.XamlType" /> is built top-down during XAML initialization.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlType" /> is built top-down during XAML initialization; otherwise, false.</returns>
	protected virtual bool LookupUsableDuringInitialization()
	{
		if (AreAttributesAvailable)
		{
			bool? attributeValue = _reflector.GetAttributeValue<bool>(typeof(UsableDuringInitializationAttribute));
			if (attributeValue.HasValue)
			{
				return attributeValue.Value;
			}
		}
		if (BaseType != null)
		{
			return BaseType.IsUsableDuringInitialization;
		}
		return GetDefaultFlag(BoolTypeBits.UsableDuringInitialization);
	}

	/// <summary>Returns a handler callback to use for the set operations of markup extensions.</summary>
	/// <returns>A handler callback to use for the set operations of markup extensions.</returns>
	protected virtual EventHandler<XamlSetMarkupExtensionEventArgs> LookupSetMarkupExtensionHandler()
	{
		if (UnderlyingType != null && TryGetAttributeString(typeof(XamlSetMarkupExtensionAttribute), out var result))
		{
			if (string.IsNullOrEmpty(result))
			{
				return null;
			}
			return (EventHandler<XamlSetMarkupExtensionEventArgs>)Delegate.CreateDelegate(typeof(EventHandler<XamlSetMarkupExtensionEventArgs>), UnderlyingType, result);
		}
		if (BaseType != null)
		{
			return BaseType.SetMarkupExtensionHandler;
		}
		return null;
	}

	/// <summary>Returns a handler to use for type converter setting cases.</summary>
	/// <returns>A handler to use for type converter setting cases.</returns>
	protected virtual EventHandler<XamlSetTypeConverterEventArgs> LookupSetTypeConverterHandler()
	{
		if (UnderlyingType != null && TryGetAttributeString(typeof(XamlSetTypeConverterAttribute), out var result))
		{
			if (string.IsNullOrEmpty(result))
			{
				return null;
			}
			return (EventHandler<XamlSetTypeConverterEventArgs>)Delegate.CreateDelegate(typeof(EventHandler<XamlSetTypeConverterEventArgs>), UnderlyingType, result);
		}
		if (BaseType != null)
		{
			return BaseType.SetTypeConverterHandler;
		}
		return null;
	}

	private void AppendTypeName(StringBuilder sb, bool forceNsInitialization)
	{
		string value = null;
		if (forceNsInitialization)
		{
			value = PreferredXamlNamespace;
		}
		else if (_namespaces != null && _namespaces.Count > 0)
		{
			value = _namespaces[0];
		}
		if (!string.IsNullOrEmpty(value))
		{
			sb.Append('{');
			sb.Append(PreferredXamlNamespace);
			sb.Append('}');
		}
		else if (UnderlyingTypeInternal.Value != null)
		{
			sb.Append(UnderlyingTypeInternal.Value.Namespace);
			sb.Append('.');
		}
		sb.Append(Name);
		if (!IsGeneric)
		{
			return;
		}
		sb.Append('(');
		for (int i = 0; i < TypeArguments.Count; i++)
		{
			TypeArguments[i].AppendTypeName(sb, forceNsInitialization);
			if (i < TypeArguments.Count - 1)
			{
				sb.Append(", ");
			}
		}
		sb.Append(')');
	}

	private void CreateReflector()
	{
		Interlocked.CompareExchange(value: (!LookupIsUnknown()) ? new TypeReflector(UnderlyingType) : TypeReflector.UnknownReflector, location1: ref _reflector, comparand: null);
	}

	private void EnsureReflector()
	{
		if (_reflector == null)
		{
			CreateReflector();
		}
	}

	private XamlCollectionKind GetCollectionKind()
	{
		EnsureReflector();
		if (!_reflector.CollectionKindIsSet)
		{
			_reflector.CollectionKind = LookupCollectionKind();
		}
		return _reflector.CollectionKind;
	}

	private bool GetFlag(BoolTypeBits flagBit)
	{
		EnsureReflector();
		bool? flag = _reflector.GetFlag(flagBit);
		if (!flag.HasValue)
		{
			flag = LookupBooleanValue(flagBit);
			_reflector.SetFlag(flagBit, flag.Value);
		}
		return flag.Value;
	}

	private XamlMember GetPropertyOrUnknown(string propertyName, bool skipReadOnlyCheck)
	{
		XamlMember xamlMember = (skipReadOnlyCheck ? LookupMember(propertyName, skipReadOnlyCheck: true) : GetMember(propertyName));
		if (xamlMember == null)
		{
			xamlMember = new XamlMember(propertyName, this, isAttachable: false);
		}
		return xamlMember;
	}

	private static bool GetDefaultFlag(BoolTypeBits flagBit)
	{
		return (BoolTypeBits.Default & flagBit) == flagBit;
	}

	private IEnumerable<ConstructorInfo> GetPublicAndInternalConstructors()
	{
		ConstructorInfo[] constructors = UnderlyingType.GetConstructors(ConstructorBindingFlags);
		foreach (ConstructorInfo constructorInfo in constructors)
		{
			if (TypeReflector.IsPublicOrInternal(constructorInfo))
			{
				yield return constructorInfo;
			}
		}
	}

	internal static ReadOnlyCollection<T> GetReadOnly<T>(IList<T> list)
	{
		if (list == null)
		{
			return null;
		}
		if (list.Count > 0)
		{
			return new ReadOnlyCollection<T>(list);
		}
		return EmptyList<T>.Value;
	}

	private static ReadOnlyCollection<XamlType> GetTypeArguments(IList<XamlType> typeArguments)
	{
		if (typeArguments == null || typeArguments.Count == 0)
		{
			return null;
		}
		foreach (XamlType typeArgument in typeArguments)
		{
			if (typeArgument == null)
			{
				throw new ArgumentException(System.SR.Format(System.SR.CollectionCannotContainNulls, "typeArguments"));
			}
		}
		return new List<XamlType>(typeArguments).AsReadOnly();
	}

	private static ReadOnlyCollection<XamlType> GetTypeArguments(Type type, XamlSchemaContext schemaContext)
	{
		Type type2 = type;
		while (type2.IsArray)
		{
			type2 = type2.GetElementType();
		}
		if (!type2.IsGenericType)
		{
			return null;
		}
		Type[] genericArguments = type2.GetGenericArguments();
		XamlType[] array = new XamlType[genericArguments.Length];
		for (int i = 0; i < genericArguments.Length; i++)
		{
			array[i] = schemaContext.GetXamlType(genericArguments[i]);
		}
		return GetReadOnly(array);
	}

	private static string GetTypeName(Type type)
	{
		string text = type.Name;
		int num = text.IndexOf('`');
		if (num >= 0)
		{
			text = GenericTypeNameScanner.StripSubscript(text, out var subscript);
			text = string.Concat(text.AsSpan(0, num), subscript);
		}
		if (type.IsNested)
		{
			text = GetTypeName(type.DeclaringType) + "+" + text;
		}
		return text;
	}

	private bool IsNullableGeneric()
	{
		if (UnderlyingType != null)
		{
			if (KS.Eq(UnderlyingType.Name, "Nullable`1") && UnderlyingType.Assembly == typeof(Nullable<>).Assembly)
			{
				return UnderlyingType.Namespace == typeof(Nullable<>).Namespace;
			}
			return false;
		}
		return false;
	}

	private ICollection<XamlMember> LookupAllExcludedReadOnlyMembers()
	{
		if (UnderlyingType == null)
		{
			return null;
		}
		GetAllMembers();
		IList<PropertyInfo> list = _reflector.LookupRemainingProperties();
		if (list == null)
		{
			return null;
		}
		List<XamlMember> list2 = new List<XamlMember>(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			XamlMember xamlMember = new XamlMember(list[i], SchemaContext);
			if (xamlMember.IsReadOnly && !xamlMember.Type.IsUsableAsReadOnly)
			{
				list2.Add(xamlMember);
			}
		}
		return new ReadOnlyCollection<XamlMember>(list2);
	}

	private Dictionary<int, IList<XamlType>> LookupAllPositionalParameters()
	{
		if (UnderlyingType == XamlLanguage.Type.UnderlyingType)
		{
			Dictionary<int, IList<XamlType>> dictionary = new Dictionary<int, IList<XamlType>>();
			XamlType xamlType = SchemaContext.GetXamlType(typeof(Type));
			XamlType[] list = new XamlType[1] { xamlType };
			dictionary.Add(1, GetReadOnly(list));
			return dictionary;
		}
		Dictionary<int, IList<XamlType>> dictionary2 = new Dictionary<int, IList<XamlType>>();
		foreach (ConstructorInfo constructor in GetConstructors())
		{
			ParameterInfo[] parameters = constructor.GetParameters();
			XamlType[] array = new XamlType[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				Type parameterType = parameters[i].ParameterType;
				XamlType xamlType2 = SchemaContext.GetXamlType(parameterType);
				array[i] = xamlType2;
			}
			if (dictionary2.ContainsKey(array.Length))
			{
				if (!SchemaContext.SupportMarkupExtensionsWithDuplicateArity)
				{
					throw new XamlSchemaException(System.SR.Format(System.SR.MarkupExtensionWithDuplicateArity, UnderlyingType, array.Length));
				}
			}
			else
			{
				dictionary2.Add(array.Length, GetReadOnly(array));
			}
		}
		return dictionary2;
	}

	private bool LookupBooleanValue(BoolTypeBits typeBit)
	{
		bool flag;
		switch (typeBit)
		{
		case BoolTypeBits.Constructible:
			flag = LookupIsConstructible();
			break;
		case BoolTypeBits.ConstructionRequiresArguments:
			flag = LookupConstructionRequiresArguments();
			break;
		case BoolTypeBits.MarkupExtension:
			flag = LookupIsMarkupExtension();
			break;
		case BoolTypeBits.Nullable:
			flag = LookupIsNullable();
			break;
		case BoolTypeBits.NameScope:
			flag = LookupIsNameScope();
			break;
		case BoolTypeBits.Public:
			flag = LookupIsPublic();
			break;
		case BoolTypeBits.TrimSurroundingWhitespace:
			flag = LookupTrimSurroundingWhitespace();
			break;
		case BoolTypeBits.UsableDuringInitialization:
			flag = LookupUsableDuringInitialization();
			if (flag && IsMarkupExtension)
			{
				throw new XamlSchemaException(System.SR.Format(System.SR.UsableDuringInitializationOnME, this));
			}
			break;
		case BoolTypeBits.WhitespaceSignificantCollection:
			flag = LookupIsWhitespaceSignificantCollection();
			break;
		case BoolTypeBits.XmlData:
			flag = LookupIsXData();
			break;
		case BoolTypeBits.Ambient:
			flag = LookupIsAmbient();
			break;
		default:
			flag = GetDefaultFlag(typeBit);
			break;
		}
		return flag;
	}

	private bool TryGetAttributeString(Type attributeType, out string result)
	{
		if (!AreAttributesAvailable)
		{
			result = null;
			return false;
		}
		result = _reflector.GetAttributeString(attributeType, out var checkedInherited);
		if (checkedInherited || result != null)
		{
			return true;
		}
		XamlType baseType = BaseType;
		if (baseType != null)
		{
			return baseType.TryGetAttributeString(attributeType, out result);
		}
		return true;
	}

	/// <summary>Indicates whether the current object is equal to another object.</summary>
	/// <returns>true if the current object is equal to the <paramref name="obj" /> parameter; otherwise, false.</returns>
	/// <param name="obj">The object to compare with this object.</param>
	public override bool Equals(object obj)
	{
		XamlType xamlType = obj as XamlType;
		return this == xamlType;
	}

	/// <summary>Returns the hash code for this object.</summary>
	/// <returns>An integer hash code.</returns>
	public override int GetHashCode()
	{
		if (IsUnknown)
		{
			int num = _name.GetHashCode();
			if (_namespaces != null && _namespaces.Count > 0)
			{
				num ^= _namespaces[0].GetHashCode();
			}
			if (_typeArguments != null && _typeArguments.Count > 0)
			{
				foreach (XamlType typeArgument in _typeArguments)
				{
					num ^= typeArgument.GetHashCode();
				}
			}
			return num;
		}
		if (UnderlyingType != null)
		{
			return UnderlyingType.GetHashCode() ^ 8;
		}
		return base.GetHashCode();
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
	/// <param name="other">An object to compare with this object.</param>
	public bool Equals(XamlType other)
	{
		return this == other;
	}

	/// <summary>Determines whether two specified <see cref="T:System.Xaml.XamlType" /> objects have the same value.</summary>
	/// <returns>true if the value of <paramref name="xamlType1" /> is the same as the value of <paramref name="xamlType2" />; otherwise, false.</returns>
	/// <param name="xamlType1">A <see cref="T:System.Xaml.XamlType" /> or null.</param>
	/// <param name="xamlType2">A <see cref="T:System.Xaml.XamlType" /> or null.</param>
	public static bool operator ==(XamlType xamlType1, XamlType xamlType2)
	{
		if ((object)xamlType1 == xamlType2)
		{
			return true;
		}
		if ((object)xamlType1 == null || (object)xamlType2 == null)
		{
			return false;
		}
		if (xamlType1.IsUnknown)
		{
			if (xamlType2.IsUnknown)
			{
				if (xamlType1._namespaces != null)
				{
					if (xamlType2._namespaces == null || xamlType1._namespaces[0] != xamlType2._namespaces[0])
					{
						return false;
					}
				}
				else if (xamlType2._namespaces != null)
				{
					return false;
				}
				if (xamlType1._name == xamlType2._name)
				{
					return TypeArgumentsAreEqual(xamlType1, xamlType2);
				}
				return false;
			}
			return false;
		}
		if (xamlType2.IsUnknown)
		{
			return false;
		}
		return xamlType1.UnderlyingType == xamlType2.UnderlyingType;
	}

	/// <summary>Determines whether two specified <see cref="T:System.Xaml.XamlType" /> objects have different values.</summary>
	/// <returns>true if the value of <paramref name="xamlType1" /> is different from the value of <paramref name="xamlType2" />; otherwise, false.</returns>
	/// <param name="xamlType1">A <see cref="T:System.Xaml.XamlType" /> or null.</param>
	/// <param name="xamlType2">A <see cref="T:System.Xaml.XamlType" /> or null.</param>
	public static bool operator !=(XamlType xamlType1, XamlType xamlType2)
	{
		return !(xamlType1 == xamlType2);
	}

	private static bool TypeArgumentsAreEqual(XamlType xamlType1, XamlType xamlType2)
	{
		if (!xamlType1.IsGeneric)
		{
			return !xamlType2.IsGeneric;
		}
		if (!xamlType2.IsGeneric)
		{
			return false;
		}
		if (xamlType1._typeArguments.Count != xamlType2._typeArguments.Count)
		{
			return false;
		}
		for (int i = 0; i < xamlType1._typeArguments.Count; i++)
		{
			if (xamlType1._typeArguments[i] != xamlType2._typeArguments[i])
			{
				return false;
			}
		}
		return true;
	}
}
