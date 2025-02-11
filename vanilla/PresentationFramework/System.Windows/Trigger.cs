using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Markup;

namespace System.Windows;

/// <summary>Represents a trigger that applies property values or performs actions conditionally.</summary>
[ContentProperty("Setters")]
[XamlSetTypeConverter("ReceiveTypeConverter")]
public class Trigger : TriggerBase, IAddChild, ISupportInitialize
{
	private DependencyProperty _property;

	private object _value = DependencyProperty.UnsetValue;

	private string _sourceName;

	private SetterBaseCollection _setters;

	private object _unresolvedProperty;

	private object _unresolvedValue;

	private ITypeDescriptorContext _serviceProvider;

	private CultureInfo _cultureInfoForTypeConverter;

	/// <summary>Gets or sets the property that returns the value that is compared with the <see cref="P:System.Windows.Trigger.Value" /> property of the trigger. The comparison is a reference equality check.</summary>
	/// <returns>A <see cref="T:System.Windows.DependencyProperty" /> that returns the property value of the element. The default value is null.</returns>
	/// <exception cref="T:System.ArgumentException">A <see cref="T:System.Windows.Style" /> cannot contain a <see cref="T:System.Windows.Trigger" /> that refers to the <see cref="T:System.Windows.Style" /> property.</exception>
	/// <exception cref="T:System.InvalidOperationException">After a <see cref="T:System.Windows.Trigger" /> is in use, it cannot be modified.</exception>
	[Ambient]
	[Localizability(LocalizationCategory.None, Modifiability = Modifiability.Unmodifiable, Readability = Readability.Unreadable)]
	public DependencyProperty Property
	{
		get
		{
			VerifyAccess();
			return _property;
		}
		set
		{
			VerifyAccess();
			if (base.IsSealed)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "Trigger"));
			}
			_property = value;
		}
	}

	/// <summary>Gets or sets the value to be compared with the property value of the element. The comparison is a reference equality check.</summary>
	/// <returns>The default value is null. See also the Exceptions section.</returns>
	/// <exception cref="T:System.ArgumentException">Only load-time <see cref="T:System.Windows.Markup.MarkupExtension" />s are supported.</exception>
	/// <exception cref="T:System.ArgumentException">Expressions such as bindings are not supported.</exception>
	/// <exception cref="T:System.InvalidOperationException">After a <see cref="T:System.Windows.Trigger" /> is in use, it cannot be modified.</exception>
	[DependsOn("Property")]
	[DependsOn("SourceName")]
	[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
	[TypeConverter(typeof(SetterTriggerConditionValueConverter))]
	public object Value
	{
		get
		{
			VerifyAccess();
			return _value;
		}
		set
		{
			VerifyAccess();
			if (base.IsSealed)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "Trigger"));
			}
			if (value is NullExtension)
			{
				value = null;
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

	/// <summary>Gets or sets the name of the object with the property that causes the associated setters to be applied.</summary>
	/// <returns>The default property is null. If this property is null, then the <see cref="P:System.Windows.Trigger.Property" /> property is evaluated with respect to the element this style or template is being applied to (the styled parent or the templated parent).</returns>
	/// <exception cref="T:System.InvalidOperationException">After a <see cref="T:System.Windows.Trigger" /> is in use, it cannot be modified.</exception>
	[DefaultValue(null)]
	[Ambient]
	public string SourceName
	{
		get
		{
			VerifyAccess();
			return _sourceName;
		}
		set
		{
			VerifyAccess();
			if (base.IsSealed)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "Trigger"));
			}
			_sourceName = value;
		}
	}

	/// <summary>Gets a collection of <see cref="T:System.Windows.Setter" /> objects, which describe the property values to apply when the specified condition has been met.</summary>
	/// <returns>The default value is null.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public SetterBaseCollection Setters
	{
		get
		{
			VerifyAccess();
			if (_setters == null)
			{
				_setters = new SetterBaseCollection();
			}
			return _setters;
		}
	}

	/// <summary>Adds a child object. </summary>
	/// <param name="value">The child object to add.</param>
	void IAddChild.AddChild(object value)
	{
		VerifyAccess();
		Setters.Add(CheckChildIsSetter(value));
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="text">The text to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		VerifyAccess();
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
	}

	internal static Setter CheckChildIsSetter(object o)
	{
		if (o == null)
		{
			throw new ArgumentNullException("o");
		}
		return (o as Setter) ?? throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, o.GetType(), typeof(Setter)), "o");
	}

	internal sealed override void Seal()
	{
		if (!base.IsSealed)
		{
			if (_property != null && !_property.IsValidValue(_value))
			{
				throw new InvalidOperationException(SR.Format(SR.InvalidPropertyValue, _value, _property.Name));
			}
			StyleHelper.SealIfSealable(_value);
			ProcessSettersCollection(_setters);
			base.TriggerConditions = new TriggerCondition[1]
			{
				new TriggerCondition(_property, LogicalOp.Equals, _value, (_sourceName != null) ? _sourceName : "~Self")
			};
			for (int i = 0; i < PropertyValues.Count; i++)
			{
				PropertyValue value = PropertyValues[i];
				value.Conditions = base.TriggerConditions;
				PropertyValues[i] = value;
			}
			base.Seal();
		}
	}

	internal override bool GetCurrentState(DependencyObject container, UncommonField<HybridDictionary[]> dataField)
	{
		return base.TriggerConditions[0].Match(container.GetValue(base.TriggerConditions[0].Property));
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

	/// <summary>Handles cases where a type converter provides a value for a property of a <see cref="T:System.Windows.Trigger" /> object.</summary>
	/// <param name="targetObject">The object where the type converter sets the value.</param>
	/// <param name="eventArgs">Data that is relevant for type converter processing.</param>
	public static void ReceiveTypeConverter(object targetObject, XamlSetTypeConverterEventArgs eventArgs)
	{
		if (!(targetObject is Trigger trigger))
		{
			throw new ArgumentNullException("targetObject");
		}
		if (eventArgs == null)
		{
			throw new ArgumentNullException("eventArgs");
		}
		if (eventArgs.Member.Name == "Property")
		{
			trigger._unresolvedProperty = eventArgs.Value;
			trigger._serviceProvider = eventArgs.ServiceProvider;
			trigger._cultureInfoForTypeConverter = eventArgs.CultureInfo;
			eventArgs.Handled = true;
		}
		else if (eventArgs.Member.Name == "Value")
		{
			trigger._unresolvedValue = eventArgs.Value;
			trigger._serviceProvider = eventArgs.ServiceProvider;
			trigger._cultureInfoForTypeConverter = eventArgs.CultureInfo;
			eventArgs.Handled = true;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Trigger" /> class.</summary>
	public Trigger()
	{
	}
}
