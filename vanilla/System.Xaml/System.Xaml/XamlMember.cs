using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Windows.Markup;
using System.Xaml.Schema;
using MS.Internal.Xaml.Parser;

namespace System.Xaml;

/// <summary>Provides the XAML type system identifier for members of XAML types. The identifier is used by XAML readers and XAML writers during processing of member nodes (when the XAML reader is positioned on a <see cref="F:System.Xaml.XamlNodeType.StartMember" />) and also for general XAML type system logic.</summary>
public class XamlMember : IEquatable<XamlMember>
{
	private enum MemberType : byte
	{
		Instance,
		Attachable,
		Directive
	}

	private string _name;

	private XamlType _declaringType;

	private readonly MemberType _memberType;

	private ThreeValuedBool _isNameValid;

	private MemberReflector _reflector;

	private NullableReference<MemberInfo> _underlyingMember;

	/// <summary>Gets the <see cref="T:System.Xaml.XamlType" /> for the type that declares the member that is associated with this <see cref="T:System.Xaml.XamlMember" />.</summary>
	/// <returns>The <see cref="T:System.Xaml.XamlType" /> for the type that declares the member that is associated with this <see cref="T:System.Xaml.XamlMember" />.</returns>
	public XamlType DeclaringType => _declaringType;

	/// <summary>Gets the <see cref="T:System.Xaml.Schema.XamlMemberInvoker" /> implementation that is associated with this <see cref="T:System.Xaml.XamlMember" />.</summary>
	/// <returns>The <see cref="T:System.Xaml.Schema.XamlMemberInvoker" /> implementation that is associated with this <see cref="T:System.Xaml.XamlMember" />.</returns>
	public XamlMemberInvoker Invoker
	{
		get
		{
			EnsureReflector();
			if (_reflector.Invoker == null)
			{
				_reflector.Invoker = LookupInvoker() ?? XamlMemberInvoker.UnknownInvoker;
			}
			return _reflector.Invoker;
		}
	}

	/// <summary>Gets a value that indicates whether the member is not resolvable by the backing system that is used for type and member resolution.</summary>
	/// <returns>true if the member is not resolvable; false if the member is resolvable.</returns>
	public bool IsUnknown
	{
		get
		{
			EnsureReflector();
			return _reflector.IsUnknown;
		}
	}

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlMember" /> represents a member with a callable public get accessor.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlMember" /> represents a callable public get accessor; otherwise, false.</returns>
	public bool IsReadPublic
	{
		get
		{
			if (IsReadPublicIgnoringType)
			{
				if (!(_declaringType == null))
				{
					return _declaringType.IsPublic;
				}
				return true;
			}
			return false;
		}
	}

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlMember" /> represents a member that has a callable public set accessor.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlMember" /> represents a callable public set accessor; otherwise, false.</returns>
	public bool IsWritePublic
	{
		get
		{
			if (IsWritePublicIgnoringType)
			{
				if (!(_declaringType == null))
				{
					return _declaringType.IsPublic;
				}
				return true;
			}
			return false;
		}
	}

	/// <summary>Gets the xamlName name string that declares this <see cref="T:System.Xaml.XamlMember" />.</summary>
	/// <returns>The xamlName name string that declares this <see cref="T:System.Xaml.XamlMember" />.</returns>
	public string Name => _name;

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlMember" /> is initialized with a valid xamlName string as its <see cref="P:System.Xaml.XamlMember.Name" />.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlMember" /> is initialized with a valid xamlName string; otherwise, false.</returns>
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

	/// <summary>Gets the single XAML namespace URI that identifies the primary XAML namespace for this <see cref="T:System.Xaml.XamlMember" />. </summary>
	/// <returns>The identifier for the primary XAML namespace for this <see cref="T:System.Xaml.XamlMember" />, as a string.</returns>
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

	/// <summary>Gets the <see cref="T:System.Xaml.XamlType" /> of the type where the <see cref="T:System.Xaml.XamlMember" /> can exist.</summary>
	/// <returns>The type where the <see cref="T:System.Xaml.XamlMember" /> can exist. See Remarks.</returns>
	public XamlType TargetType
	{
		get
		{
			if (!IsAttachable)
			{
				return _declaringType;
			}
			EnsureReflector();
			if (_reflector.TargetType == null)
			{
				if (_reflector.IsUnknown)
				{
					return XamlLanguage.Object;
				}
				_reflector.TargetType = LookupTargetType() ?? XamlLanguage.Object;
			}
			return _reflector.TargetType;
		}
	}

	/// <summary>Gets the <see cref="T:System.Xaml.XamlType" /> of the type that is used by the member.</summary>
	/// <returns>The <see cref="T:System.Xaml.XamlType" /> of the type that is used by the member. See Remarks.</returns>
	public XamlType Type
	{
		get
		{
			EnsureReflector();
			if (_reflector.Type == null)
			{
				_reflector.Type = LookupType() ?? XamlLanguage.Object;
			}
			return _reflector.Type;
		}
	}

	/// <summary>Gets a <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> object, which can be used for type conversion construction of XAML declared objects.</summary>
	/// <returns>A <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> value, with a <see cref="T:System.ComponentModel.TypeConverter" /> constraint on the generic. See Remarks.</returns>
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

	/// <summary>Gets a <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> object, which is used for value serialization of XAML declared objects.</summary>
	/// <returns>A <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> with <see cref="T:System.Windows.Markup.ValueSerializer" /> constraint on the generic.</returns>
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

	/// <summary>Gets a <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> object, which is used for deferred loading of XAML declared objects.</summary>
	/// <returns>A <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> with <see cref="T:System.Xaml.XamlDeferringLoader" /> constraint on the generic.</returns>
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

	/// <summary>Gets the CLR type system <see cref="T:System.Reflection.MemberInfo" /> that is available for a member that is constructed by <see cref="T:System.Reflection.PropertyInfo" />, <see cref="T:System.Reflection.MethodInfo" />, or <see cref="T:System.Reflection.EventInfo" />.</summary>
	/// <returns>CLR type system <see cref="T:System.Reflection.MemberInfo" /> information, as cast from the initial constructor parameters. A <see cref="T:System.Xaml.XamlMember" /> that is constructed with the <see cref="M:System.Xaml.XamlMember.#ctor(System.String,System.Xaml.XamlType,System.Boolean)" /> signature returns null.</returns>
	public MemberInfo UnderlyingMember
	{
		get
		{
			if (!_underlyingMember.IsSet)
			{
				_underlyingMember.SetIfNull(LookupUnderlyingMember());
			}
			return _underlyingMember.Value;
		}
	}

	internal NullableReference<MemberInfo> UnderlyingMemberInternal => _underlyingMember;

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlMember" /> represents a read-only member.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlMember" /> represents a read-only member; otherwise, false.</returns>
	public bool IsReadOnly => GetFlag(BoolMemberBits.ReadOnly);

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlMember" /> represents a write-only member.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlMember" /> represents a write-only member; otherwise, false.</returns>
	public bool IsWriteOnly => GetFlag(BoolMemberBits.WriteOnly);

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlMember" /> is an attachable member.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlMember" /> is an attachable member; otherwise, false.</returns>
	public bool IsAttachable => _memberType == MemberType.Attachable;

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlMember" /> represents an event member.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlMember" /> represents an event; otherwise, false.</returns>
	public bool IsEvent => GetFlag(BoolMemberBits.Event);

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlMember" /> is a XAML directive.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlMember" /> is a XAML directive; otherwise, false.</returns>
	public bool IsDirective => _memberType == MemberType.Directive;

	/// <summary>Gets a list of <see cref="T:System.Xaml.XamlMember" /> objects. These report the members where dependency relationships for initialization order exist relative to this <see cref="T:System.Xaml.XamlMember" />.</summary>
	/// <returns>A list of <see cref="T:System.Xaml.XamlMember" /> objects.</returns>
	public IList<XamlMember> DependsOn
	{
		get
		{
			EnsureReflector();
			if (_reflector.DependsOn == null)
			{
				_reflector.DependsOn = LookupDependsOn() ?? XamlType.EmptyList<XamlMember>.Value;
			}
			return _reflector.DependsOn;
		}
	}

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xaml.XamlMember" /> is reported as an ambient property.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlMember" /> is reported as an ambient property; otherwise, false.</returns>
	public bool IsAmbient => GetFlag(BoolMemberBits.Ambient);

	/// <summary>Gets a <see cref="T:System.ComponentModel.DesignerSerializationVisibility" /> value, which indicates how a visual designer should process the member.</summary>
	/// <returns>A value of the <see cref="T:System.ComponentModel.DesignerSerializationVisibility" /> enumeration. The default is <see cref="F:System.ComponentModel.DesignerSerializationVisibility.Visible" />.</returns>
	public DesignerSerializationVisibility SerializationVisibility
	{
		get
		{
			EnsureReflector();
			if (!_reflector.DesignerSerializationVisibilityIsSet)
			{
				_reflector.SerializationVisibility = LookupSerializationVisibility();
			}
			return _reflector.SerializationVisibility ?? DesignerSerializationVisibility.Visible;
		}
	}

	public IReadOnlyDictionary<char, char> MarkupExtensionBracketCharacters
	{
		get
		{
			EnsureReflector();
			if (!_reflector.MarkupExtensionBracketCharactersArgumentIsSet)
			{
				_reflector.MarkupExtensionBracketCharactersArgument = LookupMarkupExtensionBracketCharacters();
				_reflector.MarkupExtensionBracketCharactersArgumentIsSet = true;
			}
			return _reflector.MarkupExtensionBracketCharactersArgument;
		}
	}

	internal string ConstructorArgument
	{
		get
		{
			EnsureReflector();
			if (!_reflector.ConstructorArgumentIsSet)
			{
				_reflector.ConstructorArgument = LookupConstructorArgument();
			}
			return _reflector.ConstructorArgument;
		}
	}

	internal object DefaultValue
	{
		get
		{
			EnsureDefaultValue();
			return _reflector.DefaultValue;
		}
	}

	internal MethodInfo Getter
	{
		get
		{
			EnsureReflector();
			if (!_reflector.GetterIsSet)
			{
				_reflector.Getter = LookupUnderlyingGetter();
			}
			return _reflector.Getter;
		}
	}

	internal bool HasDefaultValue
	{
		get
		{
			EnsureDefaultValue();
			return !_reflector.DefaultValueIsNotPresent;
		}
	}

	internal bool HasSerializationVisibility
	{
		get
		{
			EnsureReflector();
			if (!_reflector.DesignerSerializationVisibilityIsSet)
			{
				_reflector.SerializationVisibility = LookupSerializationVisibility();
			}
			return _reflector.SerializationVisibility.HasValue;
		}
	}

	internal MethodInfo Setter
	{
		get
		{
			EnsureReflector();
			if (!_reflector.SetterIsSet)
			{
				_reflector.Setter = LookupUnderlyingSetter();
			}
			return _reflector.Setter;
		}
	}

	private bool IsReadPublicIgnoringType
	{
		get
		{
			EnsureReflector();
			bool? flag = _reflector.GetFlag(BoolMemberBits.ReadPublic);
			if (!flag.HasValue)
			{
				flag = LookupIsReadPublic();
				_reflector.SetFlag(BoolMemberBits.ReadPublic, flag.Value);
			}
			return flag.Value;
		}
	}

	private bool IsWritePublicIgnoringType
	{
		get
		{
			EnsureReflector();
			bool? flag = _reflector.GetFlag(BoolMemberBits.WritePublic);
			if (!flag.HasValue)
			{
				flag = LookupIsWritePublic();
				_reflector.SetFlag(BoolMemberBits.WritePublic, flag.Value);
			}
			return flag.Value;
		}
	}

	private bool AreAttributesAvailable
	{
		get
		{
			EnsureReflector();
			if (!_reflector.CustomAttributeProviderIsSetVolatile)
			{
				ICustomAttributeProvider customAttributeProvider = LookupCustomAttributeProvider();
				if (customAttributeProvider == null)
				{
					_reflector.UnderlyingMember = UnderlyingMember;
				}
				_reflector.SetCustomAttributeProviderVolatile(customAttributeProvider);
			}
			if (_reflector.CustomAttributeProvider == null)
			{
				return UnderlyingMemberInternal.Value != null;
			}
			return true;
		}
	}

	private XamlSchemaContext SchemaContext => _declaringType.SchemaContext;

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlMember" /> class using a string name and declaring <see cref="T:System.Xaml.XamlType" /> information. A <see cref="T:System.Xaml.XamlMember" /> that is constructed with this signature has significant limitations; see Remarks.</summary>
	/// <param name="name">The string name of the member.</param>
	/// <param name="declaringType">The <see cref="T:System.Xaml.XamlType" /> information for the declaring type.</param>
	/// <param name="isAttachable">true to indicate that the member is attachable; otherwise, false.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> or <paramref name="declaringType" /> is null.</exception>
	public XamlMember(string name, XamlType declaringType, bool isAttachable)
	{
		_name = name ?? throw new ArgumentNullException("name");
		_declaringType = declaringType ?? throw new ArgumentNullException("declaringType");
		_memberType = (isAttachable ? MemberType.Attachable : MemberType.Instance);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlMember" /> class using CLR type system <see cref="T:System.Reflection.PropertyInfo" /> and a <see cref="T:System.Xaml.XamlSchemaContext" />.</summary>
	/// <param name="propertyInfo">The CLR type system <see cref="T:System.Reflection.PropertyInfo" /> that represents the property member.</param>
	/// <param name="schemaContext">The <see cref="T:System.Xaml.XamlSchemaContext" /> context that qualifies the member.</param>
	public XamlMember(PropertyInfo propertyInfo, XamlSchemaContext schemaContext)
		: this(propertyInfo, schemaContext, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlMember" /> class using reflection <see cref="T:System.Reflection.PropertyInfo" /> and a <see cref="T:System.Xaml.XamlSchemaContext" />, including <see cref="T:System.Xaml.Schema.XamlMemberInvoker" /> information.</summary>
	/// <param name="propertyInfo">The CLR type system <see cref="T:System.Reflection.PropertyInfo" /> that represents the property member.</param>
	/// <param name="schemaContext">The <see cref="T:System.Xaml.XamlSchemaContext" /> context that qualifies the member.</param>
	/// <param name="invoker">The <see cref="T:System.Xaml.Schema.XamlMemberInvoker" /> implementation that handles run-time invocation calls against the <see cref="T:System.Xaml.XamlMember" />.</param>
	public XamlMember(PropertyInfo propertyInfo, XamlSchemaContext schemaContext, XamlMemberInvoker invoker)
		: this(propertyInfo, schemaContext, invoker, new MemberReflector(isEvent: false))
	{
	}

	internal XamlMember(PropertyInfo propertyInfo, XamlSchemaContext schemaContext, XamlMemberInvoker invoker, MemberReflector reflector)
	{
		ArgumentNullException.ThrowIfNull(propertyInfo, "propertyInfo");
		ArgumentNullException.ThrowIfNull(schemaContext, "schemaContext");
		_name = propertyInfo.Name;
		_declaringType = schemaContext.GetXamlType(propertyInfo.DeclaringType);
		_memberType = MemberType.Instance;
		_reflector = reflector;
		_reflector.Invoker = invoker;
		_underlyingMember.Value = propertyInfo;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlMember" /> class using CLR type system <see cref="T:System.Reflection.EventInfo" /> and a <see cref="T:System.Xaml.XamlSchemaContext" />.</summary>
	/// <param name="eventInfo">The CLR type system <see cref="T:System.Reflection.EventInfo" /> that represents the event member.</param>
	/// <param name="schemaContext">The <see cref="T:System.Xaml.XamlSchemaContext" /> context that qualifies the member.</param>
	public XamlMember(EventInfo eventInfo, XamlSchemaContext schemaContext)
		: this(eventInfo, schemaContext, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlMember" /> class using CLR type system <see cref="T:System.Reflection.EventInfo" /> and a <see cref="T:System.Xaml.XamlSchemaContext" />, including <see cref="T:System.Xaml.Schema.XamlMemberInvoker" /> information.</summary>
	/// <param name="eventInfo">The CLR type system <see cref="T:System.Reflection.EventInfo" /> that represents the event member.</param>
	/// <param name="schemaContext">The <see cref="T:System.Xaml.XamlSchemaContext" /> context that qualifies the member.</param>
	/// <param name="invoker">The <see cref="T:System.Xaml.Schema.XamlMemberInvoker" /> implementation that handles run-time reflection calls against the <see cref="T:System.Xaml.XamlMember" />.</param>
	public XamlMember(EventInfo eventInfo, XamlSchemaContext schemaContext, XamlMemberInvoker invoker)
		: this(eventInfo, schemaContext, invoker, new MemberReflector(isEvent: true))
	{
	}

	internal XamlMember(EventInfo eventInfo, XamlSchemaContext schemaContext, XamlMemberInvoker invoker, MemberReflector reflector)
	{
		ArgumentNullException.ThrowIfNull(eventInfo, "eventInfo");
		ArgumentNullException.ThrowIfNull(schemaContext, "schemaContext");
		_name = eventInfo.Name;
		_declaringType = schemaContext.GetXamlType(eventInfo.DeclaringType);
		_memberType = MemberType.Instance;
		_reflector = reflector;
		_reflector.Invoker = invoker;
		_underlyingMember.Value = eventInfo;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlMember" /> class for a <see cref="T:System.Xaml.XamlMember" /> that represents an attachable property.</summary>
	/// <param name="attachablePropertyName">The string name of the attachable property.</param>
	/// <param name="getter">The CLR type system <see cref="T:System.Reflection.MethodInfo" /> for the get accessor of the attachable member's backing implementation.</param>
	/// <param name="setter">The CLR type system <see cref="T:System.Reflection.MethodInfo" /> for the set accessor of the attachable member's backing implementation.</param>
	/// <param name="schemaContext">The <see cref="T:System.Xaml.XamlSchemaContext" /> context that qualifies the member.</param>
	public XamlMember(string attachablePropertyName, MethodInfo getter, MethodInfo setter, XamlSchemaContext schemaContext)
		: this(attachablePropertyName, getter, setter, schemaContext, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlMember" /> class for a <see cref="T:System.Xaml.XamlMember" /> that represents an attachable property, including <see cref="T:System.Xaml.Schema.XamlMemberInvoker" /> information.</summary>
	/// <param name="attachablePropertyName">The string name of the attachable property.</param>
	/// <param name="getter">The CLR type system <see cref="T:System.Reflection.MethodInfo" /> for the get accessor of the attachable member's backing implementation.</param>
	/// <param name="setter">The CLR type system <see cref="T:System.Reflection.MethodInfo" /> for the set accessor of the attachable member's backing implementation.</param>
	/// <param name="schemaContext">The <see cref="T:System.Xaml.XamlSchemaContext" /> context that qualifies the member.</param>
	/// <param name="invoker">The <see cref="T:System.Xaml.Schema.XamlMemberInvoker" /> implementation that handles run-time invocation calls against the <see cref="T:System.Xaml.XamlMember" />.</param>
	public XamlMember(string attachablePropertyName, MethodInfo getter, MethodInfo setter, XamlSchemaContext schemaContext, XamlMemberInvoker invoker)
		: this(attachablePropertyName, getter, setter, schemaContext, invoker, new MemberReflector(getter, setter, isEvent: false))
	{
	}

	internal XamlMember(string attachablePropertyName, MethodInfo getter, MethodInfo setter, XamlSchemaContext schemaContext, XamlMemberInvoker invoker, MemberReflector reflector)
	{
		ArgumentNullException.ThrowIfNull(schemaContext, "schemaContext");
		MethodInfo methodInfo = getter ?? setter;
		if (methodInfo == null)
		{
			throw new ArgumentNullException(System.SR.GetterOrSetterRequired, (Exception?)null);
		}
		_name = attachablePropertyName ?? throw new ArgumentNullException("attachablePropertyName");
		ValidateGetter(getter, "getter");
		ValidateSetter(setter, "setter");
		_declaringType = schemaContext.GetXamlType(methodInfo.DeclaringType);
		_reflector = reflector;
		_memberType = MemberType.Attachable;
		_reflector.Invoker = invoker;
		_underlyingMember.Value = getter ?? setter;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlMember" /> class for a <see cref="T:System.Xaml.XamlMember" /> that represents an attachable event.</summary>
	/// <param name="attachableEventName">The string name of the attachable event.</param>
	/// <param name="adder">The CLR type system <see cref="T:System.Reflection.MethodInfo" /> for the handler Add method of the attachable member's backing implementation.</param>
	/// <param name="schemaContext">The <see cref="T:System.Xaml.XamlSchemaContext" /> context that qualifies the member.</param>
	public XamlMember(string attachableEventName, MethodInfo adder, XamlSchemaContext schemaContext)
		: this(attachableEventName, adder, schemaContext, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlMember" /> class for a <see cref="T:System.Xaml.XamlMember" /> that represents an attachable event, including <see cref="T:System.Xaml.Schema.XamlMemberInvoker" /> information.</summary>
	/// <param name="attachableEventName">The string name of the attachable event.</param>
	/// <param name="adder">The CLR type system <see cref="T:System.Reflection.MethodInfo" /> for the handler Add method of the attachable member's backing implementation.</param>
	/// <param name="schemaContext">The <see cref="T:System.Xaml.XamlSchemaContext" /> context that qualifies the member.</param>
	/// <param name="invoker">The <see cref="T:System.Xaml.Schema.XamlMemberInvoker" /> implementation that handles run-time invocation calls against the <see cref="T:System.Xaml.XamlMember" />.</param>
	public XamlMember(string attachableEventName, MethodInfo adder, XamlSchemaContext schemaContext, XamlMemberInvoker invoker)
		: this(attachableEventName, adder, schemaContext, invoker, new MemberReflector(null, adder, isEvent: true))
	{
	}

	internal XamlMember(string attachableEventName, MethodInfo adder, XamlSchemaContext schemaContext, XamlMemberInvoker invoker, MemberReflector reflector)
	{
		ArgumentNullException.ThrowIfNull(adder, "adder");
		ArgumentNullException.ThrowIfNull(schemaContext, "schemaContext");
		ValidateSetter(adder, "adder");
		_name = attachableEventName ?? throw new ArgumentNullException("attachableEventName");
		_declaringType = schemaContext.GetXamlType(adder.DeclaringType);
		_reflector = reflector;
		_memberType = MemberType.Attachable;
		_reflector.Invoker = invoker;
		_underlyingMember.Value = adder;
	}

	internal XamlMember(string name, MemberReflector reflector)
	{
		_name = name;
		_declaringType = null;
		_reflector = reflector ?? MemberReflector.UnknownReflector;
		_memberType = MemberType.Directive;
	}

	/// <summary>Returns a list of XAML namespaces where this XAML member can exist. </summary>
	/// <returns>A list of XAML namespace identifiers as strings. </returns>
	public virtual IList<string> GetXamlNamespaces()
	{
		return DeclaringType.GetXamlNamespaces();
	}

	/// <summary>Returns a string representation of this <see cref="T:System.Xaml.XamlMember" />.</summary>
	/// <returns>A string representation of this <see cref="T:System.Xaml.XamlMember" />.</returns>
	public override string ToString()
	{
		return _declaringType.ToString() + "." + Name;
	}

	internal bool IsReadVisibleTo(Assembly accessingAssembly, Type accessingType)
	{
		if (IsReadPublicIgnoringType)
		{
			return true;
		}
		MethodInfo getter = Getter;
		if (getter != null)
		{
			if (MemberReflector.GenericArgumentsAreVisibleTo(getter, accessingAssembly, SchemaContext))
			{
				if (!MemberReflector.IsInternalVisibleTo(getter, accessingAssembly, SchemaContext))
				{
					return MemberReflector.IsProtectedVisibleTo(getter, accessingType, SchemaContext);
				}
				return true;
			}
			return false;
		}
		return false;
	}

	internal bool IsWriteVisibleTo(Assembly accessingAssembly, Type accessingType)
	{
		if (IsWritePublicIgnoringType)
		{
			return true;
		}
		MethodInfo setter = Setter;
		if (setter != null)
		{
			if (MemberReflector.GenericArgumentsAreVisibleTo(setter, accessingAssembly, SchemaContext))
			{
				if (!MemberReflector.IsInternalVisibleTo(setter, accessingAssembly, SchemaContext))
				{
					return MemberReflector.IsProtectedVisibleTo(setter, accessingType, SchemaContext);
				}
				return true;
			}
			return false;
		}
		return false;
	}

	/// <summary>Returns a <see cref="T:System.Xaml.Schema.XamlMemberInvoker" /> that is associated with this <see cref="T:System.Xaml.XamlMember" />.</summary>
	/// <returns>The <see cref="T:System.Xaml.Schema.XamlMemberInvoker" /> information for this <see cref="T:System.Xaml.XamlMember" />; or null.</returns>
	protected virtual XamlMemberInvoker LookupInvoker()
	{
		if (UnderlyingMember != null)
		{
			return new XamlMemberInvoker(this);
		}
		return null;
	}

	/// <summary>When implemented in a derived class, returns an <see cref="T:System.Reflection.ICustomAttributeProvider" /> implementation.</summary>
	/// <returns>An <see cref="T:System.Reflection.ICustomAttributeProvider" /> implementation.</returns>
	protected virtual ICustomAttributeProvider LookupCustomAttributeProvider()
	{
		return null;
	}

	/// <summary>Returns a <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> object, which is used for deferred loading of XAML declared objects.</summary>
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
		if (Type != null)
		{
			return Type.DeferringLoader;
		}
		return null;
	}

	/// <summary>Returns a list of <see cref="T:System.Xaml.XamlMember" /> objects. Items in the list report the members where dependency relationships for initialization order exist relative to this <see cref="T:System.Xaml.XamlMember" />.</summary>
	/// <returns>A list of <see cref="T:System.Xaml.XamlMember" /> objects.</returns>
	protected virtual IList<XamlMember> LookupDependsOn()
	{
		if (!AreAttributesAvailable)
		{
			return null;
		}
		List<string> allAttributeContents = _reflector.GetAllAttributeContents<string>(typeof(DependsOnAttribute));
		if (allAttributeContents == null || allAttributeContents.Count == 0)
		{
			return null;
		}
		List<XamlMember> list = new List<XamlMember>();
		foreach (string item in allAttributeContents)
		{
			XamlMember member = _declaringType.GetMember(item);
			if (member != null)
			{
				list.Add(member);
			}
		}
		return XamlType.GetReadOnly(list);
	}

	private DesignerSerializationVisibility? LookupSerializationVisibility()
	{
		DesignerSerializationVisibility? result = null;
		if (AreAttributesAvailable)
		{
			return _reflector.GetAttributeValue<DesignerSerializationVisibility>(typeof(DesignerSerializationVisibilityAttribute));
		}
		return result;
	}

	/// <summary>Returns whether this <see cref="T:System.Xaml.XamlMember" /> is reported as an ambient property.</summary>
	/// <returns>true to report this <see cref="T:System.Xaml.XamlMember" /> as an ambient property; otherwise, false.</returns>
	protected virtual bool LookupIsAmbient()
	{
		if (AreAttributesAvailable)
		{
			return _reflector.IsAttributePresent(typeof(AmbientAttribute));
		}
		return GetDefaultFlag(BoolMemberBits.Ambient);
	}

	/// <summary>Returns whether this <see cref="T:System.Xaml.XamlMember" /> represents an event.</summary>
	/// <returns>true to report that this <see cref="T:System.Xaml.XamlMember" /> represents an event; otherwise, false.</returns>
	protected virtual bool LookupIsEvent()
	{
		return UnderlyingMember is EventInfo;
	}

	/// <summary>Returns whether this <see cref="T:System.Xaml.XamlMember" /> represents a property that has a public get accessor.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlMember" /> represents a property that has a public get accessor; otherwise, false.</returns>
	protected virtual bool LookupIsReadPublic()
	{
		MethodInfo getter = Getter;
		if (getter != null && !getter.IsPublic)
		{
			return false;
		}
		return !IsWriteOnly;
	}

	/// <summary>Returns whether this <see cref="T:System.Xaml.XamlMember" /> represents an intended read-only property.</summary>
	/// <returns>true to report this <see cref="T:System.Xaml.XamlMember" /> as an intended read-only property; otherwise, false.</returns>
	protected virtual bool LookupIsReadOnly()
	{
		if (UnderlyingMember != null)
		{
			return Setter == null;
		}
		return GetDefaultFlag(BoolMemberBits.ReadOnly);
	}

	/// <summary>Returns whether this <see cref="T:System.Xaml.XamlMember" /> represents a member that is not resolvable by the backing system that is used for type and member resolution.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlMember" /> represents a non-resolvable member; otherwise, false.</returns>
	protected virtual bool LookupIsUnknown()
	{
		if (_reflector != null)
		{
			return _reflector.IsUnknown;
		}
		return UnderlyingMember == null;
	}

	/// <summary>Returns whether this <see cref="T:System.Xaml.XamlMember" /> represents a member that has a public set accessor but not a public get accessor.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlMember" /> represents a write-only member; otherwise, false.</returns>
	protected virtual bool LookupIsWriteOnly()
	{
		if (UnderlyingMember != null)
		{
			return Getter == null;
		}
		return GetDefaultFlag(BoolMemberBits.WriteOnly);
	}

	/// <summary>Returns whether this <see cref="T:System.Xaml.XamlMember" /> represents a member that has a public set accessor.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlMember" /> represents a writable member; otherwise, false.</returns>
	protected virtual bool LookupIsWritePublic()
	{
		MethodInfo setter = Setter;
		if (setter != null && !setter.IsPublic)
		{
			return false;
		}
		return !IsReadOnly;
	}

	/// <summary>Returns the <see cref="T:System.Xaml.XamlType" /> of the type where the <see cref="T:System.Xaml.XamlMember" /> can exist.</summary>
	/// <returns>The type where the <see cref="T:System.Xaml.XamlMember" /> can exist. See Remarks.</returns>
	protected virtual XamlType LookupTargetType()
	{
		if (IsAttachable)
		{
			MethodInfo methodInfo = UnderlyingMember as MethodInfo;
			if (methodInfo != null)
			{
				ParameterInfo[] parameters = methodInfo.GetParameters();
				if (parameters.Length != 0)
				{
					Type parameterType = parameters[0].ParameterType;
					return SchemaContext.GetXamlType(parameterType);
				}
			}
			return XamlLanguage.Object;
		}
		return _declaringType;
	}

	/// <summary>Returns a type converter implementation that is associated with this <see cref="T:System.Xaml.XamlMember" />.</summary>
	/// <returns>A <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> instance with <see cref="T:System.ComponentModel.TypeConverter" /> constraint; or null.</returns>
	protected virtual XamlValueConverter<TypeConverter> LookupTypeConverter()
	{
		XamlValueConverter<TypeConverter> xamlValueConverter = null;
		if (AreAttributesAvailable)
		{
			Type attributeType = _reflector.GetAttributeType(typeof(TypeConverterAttribute));
			if (attributeType != null)
			{
				xamlValueConverter = SchemaContext.GetValueConverter<TypeConverter>(attributeType, null);
			}
		}
		if (xamlValueConverter == null && Type != null)
		{
			xamlValueConverter = Type.TypeConverter;
		}
		return xamlValueConverter;
	}

	/// <summary>Returns a value serializer implementation that is associated with this <see cref="T:System.Xaml.XamlMember" />.</summary>
	/// <returns>A <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> instance with <see cref="T:System.Windows.Markup.ValueSerializer" /> constraint, or null.</returns>
	protected virtual XamlValueConverter<ValueSerializer> LookupValueSerializer()
	{
		XamlValueConverter<ValueSerializer> xamlValueConverter = null;
		if (AreAttributesAvailable)
		{
			Type attributeType = _reflector.GetAttributeType(typeof(ValueSerializerAttribute));
			if (attributeType != null)
			{
				xamlValueConverter = SchemaContext.GetValueConverter<ValueSerializer>(attributeType, null);
			}
		}
		if (xamlValueConverter == null && Type != null)
		{
			xamlValueConverter = Type.ValueSerializer;
		}
		return xamlValueConverter;
	}

	protected virtual IReadOnlyDictionary<char, char> LookupMarkupExtensionBracketCharacters()
	{
		if (AreAttributesAvailable)
		{
			IReadOnlyDictionary<char, char> bracketCharacterAttributes = _reflector.GetBracketCharacterAttributes(typeof(MarkupExtensionBracketCharactersAttribute));
			if (bracketCharacterAttributes != null)
			{
				_reflector.MarkupExtensionBracketCharactersArgument = bracketCharacterAttributes;
			}
		}
		return _reflector.MarkupExtensionBracketCharactersArgument;
	}

	/// <summary>Returns the <see cref="T:System.Xaml.XamlType" /> of the type that is used by the member. See Remarks.</summary>
	/// <returns>The <see cref="T:System.Xaml.XamlType" /> of the type that is used by the member. See Remarks.</returns>
	protected virtual XamlType LookupType()
	{
		Type type = LookupSystemType();
		if (!(type != null))
		{
			return null;
		}
		return SchemaContext.GetXamlType(type);
	}

	/// <summary>Returns a get accessor that is associated with this <see cref="T:System.Xaml.XamlMember" />.</summary>
	/// <returns>The <see cref="T:System.Reflection.MethodInfo" /> for the associated get accessor; or null.</returns>
	protected virtual MethodInfo LookupUnderlyingGetter()
	{
		EnsureReflector();
		if (_reflector.Getter != null)
		{
			return _reflector.Getter;
		}
		PropertyInfo propertyInfo = UnderlyingMember as PropertyInfo;
		if (!(propertyInfo != null))
		{
			return null;
		}
		return propertyInfo.GetGetMethod(nonPublic: true);
	}

	/// <summary>Returns a set accessor that is associated with this <see cref="T:System.Xaml.XamlMember" />.</summary>
	/// <returns>The <see cref="T:System.Reflection.MethodInfo" /> for the associated set accessor; or null.</returns>
	protected virtual MethodInfo LookupUnderlyingSetter()
	{
		EnsureReflector();
		if (_reflector.Setter != null)
		{
			return _reflector.Setter;
		}
		PropertyInfo propertyInfo = UnderlyingMember as PropertyInfo;
		if (propertyInfo != null)
		{
			return propertyInfo.GetSetMethod(nonPublic: true);
		}
		EventInfo eventInfo = UnderlyingMember as EventInfo;
		if (!(eventInfo != null))
		{
			return null;
		}
		return eventInfo.GetAddMethod(nonPublic: true);
	}

	/// <summary>Returns a CLR type system <see cref="T:System.Reflection.MemberInfo" /> that is associated with this <see cref="T:System.Xaml.XamlMember" />.</summary>
	/// <returns>A CLR type system <see cref="T:System.Reflection.MemberInfo" /> object that is associated with this <see cref="T:System.Xaml.XamlMember" />; or null.</returns>
	protected virtual MemberInfo LookupUnderlyingMember()
	{
		return UnderlyingMemberInternal.Value;
	}

	private static void ValidateGetter(MethodInfo method, string argumentName)
	{
		if (method == null || (method.GetParameters().Length == 1 && !(method.ReturnType == typeof(void))))
		{
			return;
		}
		throw new ArgumentException(System.SR.IncorrectGetterParamNum, argumentName);
	}

	private static void ValidateSetter(MethodInfo method, string argumentName)
	{
		if (method != null && method.GetParameters().Length != 2)
		{
			throw new ArgumentException(System.SR.IncorrectSetterParamNum, argumentName);
		}
	}

	private static bool GetDefaultFlag(BoolMemberBits flagBit)
	{
		return (BoolMemberBits.Default & flagBit) == flagBit;
	}

	private void CreateReflector()
	{
		MemberReflector value = (LookupIsUnknown() ? MemberReflector.UnknownReflector : new MemberReflector());
		Interlocked.CompareExchange(ref _reflector, value, null);
	}

	private void EnsureDefaultValue()
	{
		EnsureReflector();
		if (_reflector.DefaultValueIsSet)
		{
			return;
		}
		DefaultValueAttribute defaultValueAttribute = null;
		if (AreAttributesAvailable)
		{
			object[] customAttributes = (_reflector.CustomAttributeProvider ?? UnderlyingMember).GetCustomAttributes(typeof(DefaultValueAttribute), inherit: true);
			if (customAttributes.Length != 0)
			{
				defaultValueAttribute = (DefaultValueAttribute)customAttributes[0];
			}
		}
		if (defaultValueAttribute != null)
		{
			_reflector.DefaultValue = defaultValueAttribute.Value;
		}
		else
		{
			_reflector.DefaultValueIsNotPresent = true;
		}
	}

	private void EnsureReflector()
	{
		if (_reflector == null)
		{
			CreateReflector();
		}
	}

	private bool GetFlag(BoolMemberBits flagBit)
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

	private bool LookupBooleanValue(BoolMemberBits flag)
	{
		return flag switch
		{
			BoolMemberBits.Ambient => LookupIsAmbient(), 
			BoolMemberBits.Event => LookupIsEvent(), 
			BoolMemberBits.ReadOnly => LookupIsReadOnly(), 
			BoolMemberBits.ReadPublic => LookupIsReadPublic(), 
			BoolMemberBits.WriteOnly => LookupIsWriteOnly(), 
			BoolMemberBits.WritePublic => LookupIsWritePublic(), 
			_ => GetDefaultFlag(flag), 
		};
	}

	private string LookupConstructorArgument()
	{
		string result = null;
		if (AreAttributesAvailable)
		{
			result = _reflector.GetAttributeString(typeof(ConstructorArgumentAttribute), out var _);
		}
		return result;
	}

	private Type LookupSystemType()
	{
		MemberInfo underlyingMember = UnderlyingMember;
		PropertyInfo propertyInfo = underlyingMember as PropertyInfo;
		if (propertyInfo != null)
		{
			return propertyInfo.PropertyType;
		}
		EventInfo eventInfo = underlyingMember as EventInfo;
		if (eventInfo != null)
		{
			return eventInfo.EventHandlerType;
		}
		MethodInfo methodInfo = underlyingMember as MethodInfo;
		if (methodInfo != null)
		{
			if (methodInfo.ReturnType != null && methodInfo.ReturnType != typeof(void))
			{
				return methodInfo.ReturnType;
			}
			ParameterInfo[] parameters = methodInfo.GetParameters();
			if (parameters.Length == 2)
			{
				return parameters[1].ParameterType;
			}
		}
		return null;
	}

	/// <summary>Indicates whether the current object is equal to another object.</summary>
	/// <returns>true if the current object is equal to the <paramref name="obj" /> parameter; otherwise, false.</returns>
	/// <param name="obj">The object to compare with this object.</param>
	public override bool Equals(object obj)
	{
		XamlMember xamlMember = obj as XamlMember;
		return this == xamlMember;
	}

	/// <summary>Returns the hash code for this object.</summary>
	/// <returns>An integer hash code.</returns>
	public override int GetHashCode()
	{
		return (int)((uint)((Name != null) ? Name.GetHashCode() : 0) ^ (uint)_memberType) ^ DeclaringType.GetHashCode();
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
	/// <param name="other">An object to compare with this object.</param>
	public bool Equals(XamlMember other)
	{
		return this == other;
	}

	/// <summary>Determines whether two specified <see cref="T:System.Xaml.XamlMember" /> objects have the same value.</summary>
	/// <returns>true if the value of <paramref name="xamlMember1" /> is the same as the value of <paramref name="xamlMember2" />; otherwise, false.</returns>
	/// <param name="xamlMember1">A <see cref="T:System.Xaml.XamlMember" /> or null.</param>
	/// <param name="xamlMember2">A <see cref="T:System.Xaml.XamlMember" /> or null.</param>
	public static bool operator ==(XamlMember xamlMember1, XamlMember xamlMember2)
	{
		if ((object)xamlMember1 == xamlMember2)
		{
			return true;
		}
		if ((object)xamlMember1 == null || (object)xamlMember2 == null)
		{
			return false;
		}
		if (xamlMember1._memberType != xamlMember2._memberType || xamlMember1.Name != xamlMember2.Name)
		{
			return false;
		}
		if (xamlMember1.IsDirective)
		{
			return XamlDirective.NamespacesAreEqual((XamlDirective)xamlMember1, (XamlDirective)xamlMember2);
		}
		if (xamlMember1.DeclaringType == xamlMember2.DeclaringType)
		{
			return xamlMember1.IsUnknown == xamlMember2.IsUnknown;
		}
		return false;
	}

	/// <summary>Determines whether two specified <see cref="T:System.Xaml.XamlMember" /> objects have different values.</summary>
	/// <returns>true if the value of <paramref name="xamlMember1" /> differs from the value of <paramref name="xamlMember2" />; otherwise, false.</returns>
	/// <param name="xamlMember1">A <see cref="T:System.Xaml.XamlMember" /> or null.</param>
	/// <param name="xamlMember2">A <see cref="T:System.Xaml.XamlMember" /> or null.</param>
	public static bool operator !=(XamlMember xamlMember1, XamlMember xamlMember2)
	{
		return !(xamlMember1 == xamlMember2);
	}
}
