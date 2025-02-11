using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Markup;

namespace System.Windows;

/// <summary>Represents a trigger that applies property values or performs actions when the bound data meets a specified condition.</summary>
[ContentProperty("Setters")]
[XamlSetMarkupExtension("ReceiveMarkupExtension")]
public class DataTrigger : TriggerBase, IAddChild
{
	private BindingBase _binding;

	private object _value = DependencyProperty.UnsetValue;

	private SetterBaseCollection _setters;

	/// <summary>Gets or sets the binding that produces the property value of the data object.</summary>
	/// <returns>The default value is null.</returns>
	[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
	public BindingBase Binding
	{
		get
		{
			VerifyAccess();
			return _binding;
		}
		set
		{
			VerifyAccess();
			if (base.IsSealed)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "DataTrigger"));
			}
			_binding = value;
		}
	}

	/// <summary>Gets or sets the value to be compared with the property value of the data object.</summary>
	/// <returns>The default value is null. See also the Exceptions section.</returns>
	/// <exception cref="T:System.ArgumentException">Only load-time <see cref="T:System.Windows.Markup.MarkupExtension" />s are supported.</exception>
	/// <exception cref="T:System.ArgumentException">Expressions are not supported. Bindings are not supported.</exception>
	[DependsOn("Binding")]
	[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
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
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "DataTrigger"));
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

	/// <summary>Gets a collection of <see cref="T:System.Windows.Setter" /> objects, which describe the property values to apply when the data item meets the specified condition.</summary>
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
		Setters.Add(Trigger.CheckChildIsSetter(value));
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="text">The text to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		VerifyAccess();
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
	}

	internal sealed override void Seal()
	{
		if (base.IsSealed)
		{
			return;
		}
		ProcessSettersCollection(_setters);
		StyleHelper.SealIfSealable(_value);
		base.TriggerConditions = new TriggerCondition[1]
		{
			new TriggerCondition(_binding, LogicalOp.Equals, _value)
		};
		for (int i = 0; i < PropertyValues.Count; i++)
		{
			PropertyValue value = PropertyValues[i];
			value.Conditions = base.TriggerConditions;
			switch (value.ValueType)
			{
			case PropertyValueType.Trigger:
				value.ValueType = PropertyValueType.DataTrigger;
				break;
			case PropertyValueType.PropertyTriggerResource:
				value.ValueType = PropertyValueType.DataTriggerResource;
				break;
			default:
				throw new InvalidOperationException(SR.Format(SR.UnexpectedValueTypeForDataTrigger, value.ValueType));
			}
			PropertyValues[i] = value;
		}
		base.Seal();
	}

	internal override bool GetCurrentState(DependencyObject container, UncommonField<HybridDictionary[]> dataField)
	{
		return base.TriggerConditions[0].ConvertAndMatch(StyleHelper.GetDataTriggerValue(dataField, container, base.TriggerConditions[0].Binding));
	}

	/// <summary>Handles cases where a markup extension provides a value for a property of a <see cref="T:System.Windows.DataTrigger" /> object.</summary>
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
		if (targetObject is DataTrigger dataTrigger && eventArgs.Member.Name == "Binding" && eventArgs.MarkupExtension is BindingBase)
		{
			dataTrigger.Binding = eventArgs.MarkupExtension as BindingBase;
			eventArgs.Handled = true;
		}
		else
		{
			eventArgs.CallBase();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.DataTrigger" /> class.</summary>
	public DataTrigger()
	{
	}
}
