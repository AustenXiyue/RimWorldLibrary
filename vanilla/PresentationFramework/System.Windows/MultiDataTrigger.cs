using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Markup;

namespace System.Windows;

/// <summary>Represents a trigger that applies property values or performs actions when the bound data meet a set of conditions. </summary>
[ContentProperty("Setters")]
public sealed class MultiDataTrigger : TriggerBase, IAddChild
{
	private ConditionCollection _conditions = new ConditionCollection();

	private SetterBaseCollection _setters;

	/// <summary>Gets a collection of <see cref="T:System.Windows.Condition" /> objects. Changes to property values are applied when all the conditions in the collection are met.</summary>
	/// <returns>A collection of <see cref="T:System.Windows.Condition" /> objects. The default is an empty collection.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public ConditionCollection Conditions
	{
		get
		{
			VerifyAccess();
			return _conditions;
		}
	}

	/// <summary>Gets a collection of <see cref="T:System.Windows.Setter" /> objects that describe the property values to apply when all the conditions of the <see cref="T:System.Windows.MultiDataTrigger" /> are met.</summary>
	/// <returns>A collection of <see cref="T:System.Windows.Setter" /> objects. The default value is an empty collection.</returns>
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

	internal override void Seal()
	{
		if (base.IsSealed)
		{
			return;
		}
		ProcessSettersCollection(_setters);
		if (_conditions.Count > 0)
		{
			_conditions.Seal(ValueLookupType.DataTrigger);
		}
		base.TriggerConditions = new TriggerCondition[_conditions.Count];
		for (int i = 0; i < base.TriggerConditions.Length; i++)
		{
			if (_conditions[i].SourceName != null && _conditions[i].SourceName.Length > 0)
			{
				throw new InvalidOperationException(SR.SourceNameNotSupportedForDataTriggers);
			}
			base.TriggerConditions[i] = new TriggerCondition(_conditions[i].Binding, LogicalOp.Equals, _conditions[i].Value);
		}
		for (int j = 0; j < PropertyValues.Count; j++)
		{
			PropertyValue value = PropertyValues[j];
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
			PropertyValues[j] = value;
		}
		base.Seal();
	}

	internal override bool GetCurrentState(DependencyObject container, UncommonField<HybridDictionary[]> dataField)
	{
		bool flag = base.TriggerConditions.Length != 0;
		int num = 0;
		while (flag && num < base.TriggerConditions.Length)
		{
			flag = base.TriggerConditions[num].ConvertAndMatch(StyleHelper.GetDataTriggerValue(dataField, container, base.TriggerConditions[num].Binding));
			num++;
		}
		return flag;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.MultiDataTrigger" /> class.</summary>
	public MultiDataTrigger()
	{
	}
}
