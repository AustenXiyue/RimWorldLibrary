using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace System.Windows;

/// <summary>Represents a setter that applies a property value.</summary>
/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Windows.Setter.Property" /> property cannot be null.</exception>
/// <exception cref="T:System.ArgumentException">If the specified <see cref="P:System.Windows.Setter.Property" /> is a read-only property.</exception>
/// <exception cref="T:System.ArgumentException">If the specified <see cref="P:System.Windows.Setter.Value" /> is set to <see cref="F:System.Windows.DependencyProperty.UnsetValue" />.</exception>
[XamlSetMarkupExtension("ReceiveMarkupExtension")]
[XamlSetTypeConverter("ReceiveTypeConverter")]
public class Setter : SetterBase, ISupportInitialize
{
	private DependencyProperty _property;

	private object _value = DependencyProperty.UnsetValue;

	private string _target;

	private object _unresolvedProperty;

	private object _unresolvedValue;

	private ITypeDescriptorContext _serviceProvider;

	private CultureInfo _cultureInfoForTypeConverter;

	/// <summary>Gets or sets the property to which the <see cref="P:System.Windows.Setter.Value" /> will be applied.</summary>
	/// <returns>A <see cref="T:System.Windows.DependencyProperty" /> to which the <see cref="P:System.Windows.Setter.Value" /> will be applied. The default value is null.</returns>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Windows.Setter.Property" /> property cannot be null.</exception>
	/// <exception cref="T:System.ArgumentException">The specified <see cref="P:System.Windows.Setter.Property" /> property cannot be read-only.</exception>
	/// <exception cref="T:System.InvalidOperationException">If the specified <see cref="P:System.Windows.Setter.Value" /> is not valid for the type of the specified <see cref="P:System.Windows.Setter.Property" />.</exception>
	[Ambient]
	[DefaultValue(null)]
	[Localizability(LocalizationCategory.None, Modifiability = Modifiability.Unmodifiable, Readability = Readability.Unreadable)]
	public DependencyProperty Property
	{
		get
		{
			return _property;
		}
		set
		{
			CheckValidProperty(value);
			CheckSealed();
			_property = value;
		}
	}

	/// <summary>Gets or sets the value to apply to the property that is specified by this <see cref="T:System.Windows.Setter" />.</summary>
	/// <returns>The default value is <see cref="F:System.Windows.DependencyProperty.UnsetValue" />.</returns>
	/// <exception cref="T:System.ArgumentException">If the specified <see cref="P:System.Windows.Setter.Value" /> is set to <see cref="F:System.Windows.DependencyProperty.UnsetValue" />.</exception>
	[DependsOn("Property")]
	[DependsOn("TargetName")]
	[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
	[TypeConverter(typeof(SetterTriggerConditionValueConverter))]
	public object Value
	{
		get
		{
			if (_value is DeferredReference deferredReference)
			{
				_value = deferredReference.GetValue(BaseValueSourceInternal.Unknown);
			}
			return _value;
		}
		set
		{
			if (value == DependencyProperty.UnsetValue)
			{
				throw new ArgumentException(SR.SetterValueCannotBeUnset);
			}
			CheckSealed();
			if (value is Expression)
			{
				throw new ArgumentException(SR.StyleValueOfExpressionNotSupported);
			}
			_value = value;
		}
	}

	internal object ValueInternal => _value;

	/// <summary>Gets or sets the name of the object this <see cref="T:System.Windows.Setter" /> is intended for.</summary>
	/// <returns>The default value is null.</returns>
	[DefaultValue(null)]
	[Ambient]
	public string TargetName
	{
		get
		{
			return _target;
		}
		set
		{
			CheckSealed();
			_target = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Setter" /> class.</summary>
	public Setter()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Setter" /> class with the specified property and value.</summary>
	/// <param name="property">The <see cref="T:System.Windows.DependencyProperty" /> to apply the <see cref="P:System.Windows.Setter.Value" /> to.</param>
	/// <param name="value">The value to apply to the property.</param>
	public Setter(DependencyProperty property, object value)
	{
		Initialize(property, value, null);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Setter" /> class with the specified property, value, and target name.</summary>
	/// <param name="property">The <see cref="T:System.Windows.DependencyProperty" /> to apply the <see cref="P:System.Windows.Setter.Value" /> to.</param>
	/// <param name="value">The value to apply to the property.</param>
	/// <param name="targetName">The name of the child node this <see cref="T:System.Windows.Setter" /> is intended for.</param>
	public Setter(DependencyProperty property, object value, string targetName)
	{
		Initialize(property, value, targetName);
	}

	private void Initialize(DependencyProperty property, object value, string target)
	{
		if (value == DependencyProperty.UnsetValue)
		{
			throw new ArgumentException(SR.SetterValueCannotBeUnset);
		}
		CheckValidProperty(property);
		_property = property;
		_value = value;
		_target = target;
	}

	private void CheckValidProperty(DependencyProperty property)
	{
		if (property == null)
		{
			throw new ArgumentNullException("property");
		}
		if (property.ReadOnly)
		{
			throw new ArgumentException(SR.Format(SR.ReadOnlyPropertyNotAllowed, property.Name, GetType().Name));
		}
		if (property == FrameworkElement.NameProperty)
		{
			throw new InvalidOperationException(SR.Format(SR.CannotHavePropertyInStyle, FrameworkElement.NameProperty.Name));
		}
	}

	internal override void Seal()
	{
		DependencyProperty property = Property;
		object valueInternal = ValueInternal;
		if (property == null)
		{
			throw new ArgumentException(SR.Format(SR.NullPropertyIllegal, "Setter.Property"));
		}
		if (string.IsNullOrEmpty(TargetName) && property == FrameworkElement.StyleProperty)
		{
			throw new ArgumentException(SR.StylePropertyInStyleNotAllowed);
		}
		if (!property.IsValidValue(valueInternal))
		{
			if (valueInternal is MarkupExtension)
			{
				if (!(valueInternal is DynamicResourceExtension) && !(valueInternal is BindingBase))
				{
					throw new ArgumentException(SR.Format(SR.SetterValueOfMarkupExtensionNotSupported, valueInternal.GetType().Name));
				}
			}
			else if (!(valueInternal is DeferredReference))
			{
				throw new ArgumentException(SR.Format(SR.InvalidSetterValue, valueInternal, property.OwnerType, property.Name));
			}
		}
		StyleHelper.SealIfSealable(_value);
		base.Seal();
	}

	/// <summary>Handles cases where a markup extension provides a value for a property of <see cref="T:System.Windows.Setter" /> object.</summary>
	/// <param name="targetObject">The object where the markup extension sets the value.</param>
	/// <param name="eventArgs">Data that is relevant for markup extension processing.</param>
	public static void ReceiveMarkupExtension(object targetObject, XamlSetMarkupExtensionEventArgs eventArgs)
	{
		if (targetObject == null)
		{
			throw new ArgumentNullException("targetObject");
		}
		if (eventArgs == null)
		{
			throw new ArgumentNullException("eventArgs");
		}
		if (targetObject is Setter setter && !(eventArgs.Member.Name != "Value"))
		{
			MarkupExtension markupExtension = eventArgs.MarkupExtension;
			if (markupExtension is StaticResourceExtension)
			{
				StaticResourceExtension staticResourceExtension = markupExtension as StaticResourceExtension;
				setter.Value = staticResourceExtension.ProvideValueInternal(eventArgs.ServiceProvider, allowDeferredReference: true);
				eventArgs.Handled = true;
			}
			else if (markupExtension is DynamicResourceExtension || markupExtension is BindingBase)
			{
				setter.Value = markupExtension;
				eventArgs.Handled = true;
			}
		}
	}

	/// <summary>Handles cases where a type converter provides a value for a property of a <see cref="T:System.Windows.Setter" /> object.</summary>
	/// <param name="targetObject">The object where the type converter sets the value.</param>
	/// <param name="eventArgs">Data that is relevant for type converter processing.</param>
	public static void ReceiveTypeConverter(object targetObject, XamlSetTypeConverterEventArgs eventArgs)
	{
		if (!(targetObject is Setter setter))
		{
			throw new ArgumentNullException("targetObject");
		}
		if (eventArgs == null)
		{
			throw new ArgumentNullException("eventArgs");
		}
		if (eventArgs.Member.Name == "Property")
		{
			setter._unresolvedProperty = eventArgs.Value;
			setter._serviceProvider = eventArgs.ServiceProvider;
			setter._cultureInfoForTypeConverter = eventArgs.CultureInfo;
			eventArgs.Handled = true;
		}
		else if (eventArgs.Member.Name == "Value")
		{
			setter._unresolvedValue = eventArgs.Value;
			setter._serviceProvider = eventArgs.ServiceProvider;
			setter._cultureInfoForTypeConverter = eventArgs.CultureInfo;
			eventArgs.Handled = true;
		}
	}

	/// <summary>Signals the object that initialization is starting. </summary>
	void ISupportInitialize.BeginInit()
	{
	}

	/// <summary>Signals the object that initialization is complete. </summary>
	void ISupportInitialize.EndInit()
	{
		if (_unresolvedProperty != null)
		{
			try
			{
				Property = DependencyPropertyConverter.ResolveProperty(_serviceProvider, TargetName, _unresolvedProperty);
			}
			finally
			{
				_unresolvedProperty = null;
			}
		}
		if (_unresolvedValue != null)
		{
			try
			{
				Value = SetterTriggerConditionValueConverter.ResolveValue(_serviceProvider, Property, _cultureInfoForTypeConverter, _unresolvedValue);
			}
			finally
			{
				_unresolvedValue = null;
			}
		}
		_serviceProvider = null;
		_cultureInfoForTypeConverter = null;
	}
}
