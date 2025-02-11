using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace System.Windows;

/// <summary>Represents a condition for the <see cref="T:System.Windows.MultiTrigger" /> and the <see cref="T:System.Windows.MultiDataTrigger" />, which apply changes to property values based on a set of conditions.</summary>
[XamlSetMarkupExtension("ReceiveMarkupExtension")]
[XamlSetTypeConverter("ReceiveTypeConverter")]
public sealed class Condition : ISupportInitialize
{
	private bool _sealed;

	private DependencyProperty _property;

	private BindingBase _binding;

	private object _value = DependencyProperty.UnsetValue;

	private string _sourceName;

	private object _unresolvedProperty;

	private object _unresolvedValue;

	private ITypeDescriptorContext _serviceProvider;

	private CultureInfo _cultureInfoForTypeConverter;

	/// <summary>Gets or sets the property of the condition. This is only applicable to <see cref="T:System.Windows.MultiTrigger" /> objects.</summary>
	/// <returns>A <see cref="T:System.Windows.DependencyProperty" /> that specifies the property of the condition. The default value is null.</returns>
	[Ambient]
	[DefaultValue(null)]
	public DependencyProperty Property
	{
		get
		{
			return _property;
		}
		set
		{
			if (_sealed)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "Condition"));
			}
			if (_binding != null)
			{
				throw new InvalidOperationException(SR.ConditionCannotUseBothPropertyAndBinding);
			}
			_property = value;
		}
	}

	/// <summary>Gets or sets the binding that specifies the property of the condition. This is only applicable to <see cref="T:System.Windows.MultiDataTrigger" /> objects.</summary>
	/// <returns>The default value is null.</returns>
	[DefaultValue(null)]
	public BindingBase Binding
	{
		get
		{
			return _binding;
		}
		set
		{
			if (_sealed)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "Condition"));
			}
			if (_property != null)
			{
				throw new InvalidOperationException(SR.ConditionCannotUseBothPropertyAndBinding);
			}
			_binding = value;
		}
	}

	/// <summary>Gets or sets the value of the condition.</summary>
	/// <returns>The <see cref="P:System.Windows.Condition.Value" /> property cannot be null for a given <see cref="T:System.Windows.Condition" />.See also the Exceptions section. The default value is null.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="T:System.Windows.Markup.MarkupExtension" />s are not supported.</exception>
	/// <exception cref="T:System.ArgumentException">Expressions are not supported.</exception>
	[TypeConverter(typeof(SetterTriggerConditionValueConverter))]
	public object Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (_sealed)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "Condition"));
			}
			if (value is MarkupExtension)
			{
				throw new ArgumentException(SR.Format(SR.ConditionValueOfMarkupExtensionNotSupported, value.GetType().Name));
			}
			if (value is Expression)
			{
				throw new ArgumentException(SR.ConditionValueOfExpressionNotSupported);
			}
			_value = value;
		}
	}

	/// <summary>Gets or sets the name of the object with the property that causes the associated setters to be applied. This is only applicable to <see cref="T:System.Windows.MultiTrigger" /> objects.</summary>
	/// <returns>The default property is null. If this property is null, then the property of the object being styled causes the associated setters to be applied.</returns>
	/// <exception cref="T:System.InvalidOperationException">After a <see cref="T:System.Windows.Condition" /> is in use, it cannot be modified.</exception>
	[DefaultValue(null)]
	public string SourceName
	{
		get
		{
			return _sourceName;
		}
		set
		{
			if (_sealed)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "Condition"));
			}
			_sourceName = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Condition" /> class. </summary>
	public Condition()
	{
		_property = null;
		_binding = null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Condition" /> class with the specified property and value. This constructor performs parameter validation. </summary>
	/// <param name="conditionProperty">The property of the condition.</param>
	/// <param name="conditionValue">The value of the condition.</param>
	public Condition(DependencyProperty conditionProperty, object conditionValue)
		: this(conditionProperty, conditionValue, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Condition" /> class with the specified property, value, and the name of the source object.</summary>
	/// <param name="conditionProperty">The property of the condition.</param>
	/// <param name="conditionValue">The value of the condition.</param>
	/// <param name="sourceName">x:Name of the object with the <paramref name="conditionProperty" />.</param>
	public Condition(DependencyProperty conditionProperty, object conditionValue, string sourceName)
	{
		if (conditionProperty == null)
		{
			throw new ArgumentNullException("conditionProperty");
		}
		if (!conditionProperty.IsValidValue(conditionValue))
		{
			throw new ArgumentException(SR.Format(SR.InvalidPropertyValue, conditionValue, conditionProperty.Name));
		}
		_property = conditionProperty;
		Value = conditionValue;
		_sourceName = sourceName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Condition" /> class. </summary>
	/// <param name="binding">The binding that specifies the property of the condition.</param>
	/// <param name="conditionValue">The value of the condition.</param>
	public Condition(BindingBase binding, object conditionValue)
	{
		if (binding == null)
		{
			throw new ArgumentNullException("binding");
		}
		Binding = binding;
		Value = conditionValue;
	}

	internal void Seal(ValueLookupType type)
	{
		if (_sealed)
		{
			return;
		}
		_sealed = true;
		if (_property != null && _binding != null)
		{
			throw new InvalidOperationException(SR.ConditionCannotUseBothPropertyAndBinding);
		}
		switch (type)
		{
		case ValueLookupType.Trigger:
		case ValueLookupType.PropertyTriggerResource:
			if (_property == null)
			{
				throw new InvalidOperationException(SR.Format(SR.NullPropertyIllegal, "Property"));
			}
			if (!_property.IsValidValue(_value))
			{
				throw new InvalidOperationException(SR.Format(SR.InvalidPropertyValue, _value, _property.Name));
			}
			break;
		case ValueLookupType.DataTrigger:
		case ValueLookupType.DataTriggerResource:
			if (_binding == null)
			{
				throw new InvalidOperationException(SR.Format(SR.NullPropertyIllegal, "Binding"));
			}
			break;
		default:
			throw new InvalidOperationException(SR.Format(SR.UnexpectedValueTypeForCondition, type));
		}
		StyleHelper.SealIfSealable(_value);
	}

	/// <summary>Signals the object that initialization is starting.</summary>
	void ISupportInitialize.BeginInit()
	{
	}

	/// <summary>Signals the object that initialization is complete.</summary>
	void ISupportInitialize.EndInit()
	{
		if (_unresolvedProperty != null)
		{
			try
			{
				Property = DependencyPropertyConverter.ResolveProperty(_serviceProvider, SourceName, _unresolvedProperty);
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

	/// <summary>Handles cases where a markup extension provides a value for a property of a <see cref="T:System.Windows.Condition" /> object</summary>
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
		if (targetObject is Condition condition && eventArgs.Member.Name == "Binding" && eventArgs.MarkupExtension is BindingBase)
		{
			condition.Binding = eventArgs.MarkupExtension as BindingBase;
			eventArgs.Handled = true;
		}
	}

	/// <summary>Handles cases where a type converter provides a value for a property of on a<see cref="T:System.Windows.Condition" /> object.</summary>
	/// <param name="targetObject">The object where the type converter sets the value.</param>
	/// <param name="eventArgs">Data that is relevant for type converter processing.</param>
	public static void ReceiveTypeConverter(object targetObject, XamlSetTypeConverterEventArgs eventArgs)
	{
		if (!(targetObject is Condition condition))
		{
			throw new ArgumentNullException("targetObject");
		}
		if (eventArgs == null)
		{
			throw new ArgumentNullException("eventArgs");
		}
		if (eventArgs.Member.Name == "Property")
		{
			condition._unresolvedProperty = eventArgs.Value;
			condition._serviceProvider = eventArgs.ServiceProvider;
			condition._cultureInfoForTypeConverter = eventArgs.CultureInfo;
			eventArgs.Handled = true;
		}
		else if (eventArgs.Member.Name == "Value")
		{
			condition._unresolvedValue = eventArgs.Value;
			condition._serviceProvider = eventArgs.ServiceProvider;
			condition._cultureInfoForTypeConverter = eventArgs.CultureInfo;
			eventArgs.Handled = true;
		}
	}
}
