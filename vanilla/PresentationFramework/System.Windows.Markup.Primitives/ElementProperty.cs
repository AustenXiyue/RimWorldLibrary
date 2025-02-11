using System.ComponentModel;

namespace System.Windows.Markup.Primitives;

internal class ElementProperty : ElementObjectPropertyBase
{
	private PropertyDescriptor _descriptor;

	private bool _isDependencyPropertyCached;

	private DependencyProperty _dependencyProperty;

	private bool _isAttached;

	public override string Name => _descriptor.Name;

	public override Type PropertyType => _descriptor.PropertyType;

	public override PropertyDescriptor PropertyDescriptor => _descriptor;

	public override bool IsAttached
	{
		get
		{
			UpdateDependencyProperty();
			return _isAttached;
		}
	}

	public override DependencyProperty DependencyProperty
	{
		get
		{
			UpdateDependencyProperty();
			return _dependencyProperty;
		}
	}

	public override object Value
	{
		get
		{
			DependencyProperty dependencyProperty = DependencyProperty;
			object obj;
			if (dependencyProperty == null)
			{
				obj = (((!(Name == "Template") && !(Name == "VisualTree")) || !(base.Context.Instance is FrameworkTemplate) || !(base.Context.Instance as FrameworkTemplate).HasContent) ? _descriptor.GetValue(_object.Instance) : (base.Context.Instance as FrameworkTemplate).LoadContent());
			}
			else
			{
				DependencyObject dependencyObject = _object.Instance as DependencyObject;
				obj = dependencyObject.ReadLocalValue(dependencyProperty);
				if (obj is Expression expression)
				{
					TypeConverter converter = TypeDescriptor.GetConverter(obj);
					obj = ((base.Manager.XamlWriterMode != 0 || !converter.CanConvertTo(typeof(MarkupExtension))) ? expression.GetValue(dependencyObject, dependencyProperty) : converter.ConvertTo(expression, typeof(MarkupExtension)));
				}
				if (obj == DependencyProperty.UnsetValue)
				{
					obj = dependencyProperty.GetDefaultValue(dependencyObject.DependencyObjectType);
				}
			}
			if (!(obj is MarkupExtension) && !CanConvertToString(obj))
			{
				obj = CheckForMarkupExtension(PropertyType, obj, base.Context, convertEnums: true);
			}
			return obj;
		}
	}

	public override AttributeCollection Attributes => _descriptor.Attributes;

	internal ElementProperty(ElementMarkupObject obj, PropertyDescriptor descriptor)
		: base(obj)
	{
		_descriptor = descriptor;
	}

	private void UpdateDependencyProperty()
	{
		if (!_isDependencyPropertyCached)
		{
			DependencyPropertyDescriptor dependencyPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(_descriptor);
			if (dependencyPropertyDescriptor != null)
			{
				_dependencyProperty = dependencyPropertyDescriptor.DependencyProperty;
				_isAttached = dependencyPropertyDescriptor.IsAttached;
			}
			_isDependencyPropertyCached = true;
		}
	}

	internal static object CheckForMarkupExtension(Type propertyType, object value, IValueSerializerContext context, bool convertEnums)
	{
		if (value == null)
		{
			return new NullExtension();
		}
		TypeConverter converter = TypeDescriptor.GetConverter(value);
		if (converter.CanConvertTo(context, typeof(MarkupExtension)))
		{
			return converter.ConvertTo(context, TypeConverterHelper.InvariantEnglishUS, value, typeof(MarkupExtension));
		}
		Type type = value as Type;
		if (type != null)
		{
			if (propertyType == typeof(Type))
			{
				return value;
			}
			return new TypeExtension(type);
		}
		if (convertEnums && value is Enum @enum)
		{
			return new StaticExtension(context.GetValueSerializerFor(typeof(Type)).ConvertToString(@enum.GetType(), context) + "." + @enum.ToString());
		}
		if (value is Array elements)
		{
			return new ArrayExtension(elements);
		}
		return value;
	}
}
