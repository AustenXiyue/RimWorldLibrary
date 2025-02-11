using System.ComponentModel;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;

namespace System.Windows.Baml2006;

internal class WpfKnownMember : WpfXamlMember
{
	[Flags]
	private enum BoolMemberBits
	{
		Frozen = 1,
		HasSpecialTypeConverter = 2,
		ReadOnly = 4,
		Ambient = 8,
		ReadPrivate = 0x10,
		WritePrivate = 0x20
	}

	private Action<object, object> _setDelegate;

	private Func<object, object> _getDelegate;

	private Type _deferringLoader;

	private Type _typeConverterType;

	private Type _type;

	private byte _bitField;

	private bool Frozen
	{
		get
		{
			return WpfXamlType.GetFlag(ref _bitField, 1);
		}
		set
		{
			WpfXamlType.SetFlag(ref _bitField, 1, value);
		}
	}

	private bool ReadOnly
	{
		get
		{
			return WpfXamlType.GetFlag(ref _bitField, 4);
		}
		set
		{
			CheckFrozen();
			WpfXamlType.SetFlag(ref _bitField, 4, value);
		}
	}

	public bool HasSpecialTypeConverter
	{
		get
		{
			return WpfXamlType.GetFlag(ref _bitField, 2);
		}
		set
		{
			CheckFrozen();
			WpfXamlType.SetFlag(ref _bitField, 2, value);
		}
	}

	public bool Ambient
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

	public bool IsReadPrivate
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

	public bool IsWritePrivate
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

	public Action<object, object> SetDelegate
	{
		get
		{
			return _setDelegate;
		}
		set
		{
			CheckFrozen();
			_setDelegate = value;
		}
	}

	public Func<object, object> GetDelegate
	{
		get
		{
			return _getDelegate;
		}
		set
		{
			CheckFrozen();
			_getDelegate = value;
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

	public WpfKnownMember(XamlSchemaContext schema, XamlType declaringType, string name, DependencyProperty dProperty, bool isReadOnly, bool isAttachable)
		: base(dProperty, isAttachable)
	{
		base.DependencyProperty = dProperty;
		ReadOnly = isReadOnly;
	}

	public WpfKnownMember(XamlSchemaContext schema, XamlType declaringType, string name, Type type, bool isReadOnly, bool isAttachable)
		: base(name, declaringType, isAttachable)
	{
		_type = type;
		ReadOnly = isReadOnly;
	}

	protected override bool LookupIsUnknown()
	{
		return false;
	}

	public void Freeze()
	{
		Frozen = true;
	}

	private void CheckFrozen()
	{
		if (Frozen)
		{
			throw new InvalidOperationException("Can't Assign to Known Member attributes");
		}
	}

	protected override XamlMemberInvoker LookupInvoker()
	{
		return new WpfKnownMemberInvoker(this);
	}

	protected override XamlValueConverter<TypeConverter> LookupTypeConverter()
	{
		WpfSharedBamlSchemaContext bamlSharedSchemaContext = System.Windows.Markup.XamlReader.BamlSharedSchemaContext;
		if (HasSpecialTypeConverter)
		{
			return bamlSharedSchemaContext.GetXamlType(_typeConverterType).TypeConverter;
		}
		if (_typeConverterType != null)
		{
			return bamlSharedSchemaContext.GetTypeConverter(_typeConverterType);
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

	protected override bool LookupIsReadOnly()
	{
		return ReadOnly;
	}

	protected override XamlType LookupType()
	{
		if (base.DependencyProperty != null)
		{
			return System.Windows.Markup.XamlReader.BamlSharedSchemaContext.GetXamlType(base.DependencyProperty.PropertyType);
		}
		return System.Windows.Markup.XamlReader.BamlSharedSchemaContext.GetXamlType(_type);
	}

	protected override MemberInfo LookupUnderlyingMember()
	{
		return base.LookupUnderlyingMember();
	}

	protected override bool LookupIsAmbient()
	{
		return Ambient;
	}

	protected override bool LookupIsWritePublic()
	{
		return !IsWritePrivate;
	}

	protected override bool LookupIsReadPublic()
	{
		return !IsReadPrivate;
	}

	protected override WpfXamlMember GetAsContentProperty()
	{
		return this;
	}
}
