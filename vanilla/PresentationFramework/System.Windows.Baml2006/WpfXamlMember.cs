using System.Reflection;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;

namespace System.Windows.Baml2006;

internal class WpfXamlMember : XamlMember, IProvideValueTarget
{
	[Flags]
	private enum BoolMemberBits
	{
		UseV3Rules = 1,
		BamlMember = 2,
		UnderlyingMemberIsKnown = 4,
		ApplyGetterFallback = 8
	}

	private byte _bitField;

	private XamlMember _baseUnderlyingMember;

	private WpfXamlMember _asContentProperty;

	public DependencyProperty DependencyProperty { get; set; }

	public RoutedEvent RoutedEvent { get; set; }

	internal bool ApplyGetterFallback
	{
		get
		{
			return WpfXamlType.GetFlag(ref _bitField, 8);
		}
		private set
		{
			WpfXamlType.SetFlag(ref _bitField, 8, value);
		}
	}

	internal WpfXamlMember AsContentProperty
	{
		get
		{
			if (_asContentProperty == null)
			{
				_asContentProperty = GetAsContentProperty();
			}
			return _asContentProperty;
		}
	}

	private bool _useV3Rules
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

	private bool _isBamlMember
	{
		get
		{
			return WpfXamlType.GetFlag(ref _bitField, 2);
		}
		set
		{
			WpfXamlType.SetFlag(ref _bitField, 2, value);
		}
	}

	private bool _underlyingMemberIsKnown
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

	object IProvideValueTarget.TargetObject
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	object IProvideValueTarget.TargetProperty
	{
		get
		{
			if (DependencyProperty != null)
			{
				return DependencyProperty;
			}
			return base.UnderlyingMember;
		}
	}

	private XamlMember BaseUnderlyingMember
	{
		get
		{
			if (_baseUnderlyingMember == null)
			{
				WpfXamlType wpfXamlType = base.DeclaringType as WpfXamlType;
				_baseUnderlyingMember = wpfXamlType.FindBaseXamlMember(base.Name, base.IsAttachable);
				if (_baseUnderlyingMember == null)
				{
					_baseUnderlyingMember = wpfXamlType.FindBaseXamlMember(base.Name, !base.IsAttachable);
				}
			}
			return _baseUnderlyingMember;
		}
	}

	public WpfXamlMember(DependencyProperty dp, bool isAttachable)
		: base(dp.Name, System.Windows.Markup.XamlReader.BamlSharedSchemaContext.GetXamlType(dp.OwnerType), isAttachable)
	{
		DependencyProperty = dp;
		_useV3Rules = true;
		_isBamlMember = true;
		_underlyingMemberIsKnown = false;
	}

	public WpfXamlMember(RoutedEvent re, bool isAttachable)
		: base(re.Name, System.Windows.Markup.XamlReader.BamlSharedSchemaContext.GetXamlType(re.OwnerType), isAttachable)
	{
		RoutedEvent = re;
		_useV3Rules = true;
		_isBamlMember = true;
		_underlyingMemberIsKnown = false;
	}

	public WpfXamlMember(DependencyProperty dp, MethodInfo getter, MethodInfo setter, XamlSchemaContext schemaContext, bool useV3Rules)
		: base(dp.Name, getter, setter, schemaContext)
	{
		DependencyProperty = dp;
		_useV3Rules = useV3Rules;
		_underlyingMemberIsKnown = true;
	}

	public WpfXamlMember(DependencyProperty dp, PropertyInfo property, XamlSchemaContext schemaContext, bool useV3Rules)
		: base(property, schemaContext)
	{
		DependencyProperty = dp;
		_useV3Rules = useV3Rules;
		_underlyingMemberIsKnown = true;
	}

	public WpfXamlMember(RoutedEvent re, MethodInfo setter, XamlSchemaContext schemaContext, bool useV3Rules)
		: base(re.Name, setter, schemaContext)
	{
		RoutedEvent = re;
		_useV3Rules = useV3Rules;
		_underlyingMemberIsKnown = true;
	}

	public WpfXamlMember(RoutedEvent re, EventInfo eventInfo, XamlSchemaContext schemaContext, bool useV3Rules)
		: base(eventInfo, schemaContext)
	{
		RoutedEvent = re;
		_useV3Rules = useV3Rules;
		_underlyingMemberIsKnown = true;
	}

	protected WpfXamlMember(string name, XamlType declaringType, bool isAttachable)
		: base(name, declaringType, isAttachable)
	{
		_useV3Rules = true;
		_isBamlMember = true;
		_underlyingMemberIsKnown = false;
	}

	protected virtual WpfXamlMember GetAsContentProperty()
	{
		if (DependencyProperty == null)
		{
			return this;
		}
		WpfXamlMember wpfXamlMember = null;
		if (_underlyingMemberIsKnown)
		{
			PropertyInfo propertyInfo = base.UnderlyingMember as PropertyInfo;
			if (propertyInfo == null)
			{
				return this;
			}
			wpfXamlMember = new WpfXamlMember(DependencyProperty, propertyInfo, base.DeclaringType.SchemaContext, _useV3Rules);
		}
		else
		{
			wpfXamlMember = new WpfXamlMember(DependencyProperty, isAttachable: false);
		}
		wpfXamlMember.ApplyGetterFallback = true;
		return wpfXamlMember;
	}

	protected override XamlType LookupType()
	{
		if (DependencyProperty != null)
		{
			if (_isBamlMember)
			{
				return System.Windows.Markup.XamlReader.BamlSharedSchemaContext.GetXamlType(DependencyProperty.PropertyType);
			}
			return System.Windows.Markup.XamlReader.GetWpfSchemaContext().GetXamlType(DependencyProperty.PropertyType);
		}
		if (RoutedEvent != null)
		{
			if (_isBamlMember)
			{
				return System.Windows.Markup.XamlReader.BamlSharedSchemaContext.GetXamlType(RoutedEvent.HandlerType);
			}
			return System.Windows.Markup.XamlReader.GetWpfSchemaContext().GetXamlType(RoutedEvent.HandlerType);
		}
		return base.LookupType();
	}

	protected override MemberInfo LookupUnderlyingMember()
	{
		MemberInfo memberInfo = base.LookupUnderlyingMember();
		if (memberInfo == null && BaseUnderlyingMember != null)
		{
			memberInfo = BaseUnderlyingMember.UnderlyingMember;
		}
		_underlyingMemberIsKnown = true;
		return memberInfo;
	}

	protected override MethodInfo LookupUnderlyingSetter()
	{
		MethodInfo methodInfo = base.LookupUnderlyingSetter();
		if (methodInfo == null && BaseUnderlyingMember != null)
		{
			methodInfo = BaseUnderlyingMember.Invoker.UnderlyingSetter;
		}
		_underlyingMemberIsKnown = true;
		return methodInfo;
	}

	protected override MethodInfo LookupUnderlyingGetter()
	{
		MethodInfo methodInfo = base.LookupUnderlyingGetter();
		if (methodInfo == null && BaseUnderlyingMember != null)
		{
			methodInfo = BaseUnderlyingMember.Invoker.UnderlyingGetter;
		}
		_underlyingMemberIsKnown = true;
		return methodInfo;
	}

	protected override bool LookupIsReadOnly()
	{
		if (DependencyProperty != null)
		{
			return DependencyProperty.ReadOnly;
		}
		return base.LookupIsReadOnly();
	}

	protected override bool LookupIsEvent()
	{
		if (RoutedEvent != null)
		{
			return true;
		}
		return false;
	}

	protected override XamlMemberInvoker LookupInvoker()
	{
		return new WpfMemberInvoker(this);
	}

	protected override bool LookupIsUnknown()
	{
		return false;
	}

	protected override XamlValueConverter<XamlDeferringLoader> LookupDeferringLoader()
	{
		if (_useV3Rules)
		{
			return null;
		}
		return base.LookupDeferringLoader();
	}
}
