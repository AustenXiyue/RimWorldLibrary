using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Markup;

namespace System.Windows;

/// <summary>Represents a trigger that applies property values or performs actions when a set of conditions are satisfied.</summary>
[ContentProperty("Setters")]
public sealed class MultiTrigger : TriggerBase, IAddChild
{
	private ConditionCollection _conditions = new ConditionCollection();

	private SetterBaseCollection _setters;

	/// <summary>Gets a collection of <see cref="T:System.Windows.Condition" /> objects. Changes to property values are applied when all of the conditions in the collection are met.</summary>
	/// <returns>The default is an empty collection.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public ConditionCollection Conditions
	{
		get
		{
			VerifyAccess();
			return _conditions;
		}
	}

	/// <summary>Gets a collection of <see cref="T:System.Windows.Setter" /> objects, which describe the property values to apply when all of the conditions of the <see cref="T:System.Windows.MultiTrigger" /> are met.</summary>
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

	internal override void Seal()
	{
		if (!base.IsSealed)
		{
			ProcessSettersCollection(_setters);
			if (_conditions.Count > 0)
			{
				_conditions.Seal(ValueLookupType.Trigger);
			}
			base.TriggerConditions = new TriggerCondition[_conditions.Count];
			for (int i = 0; i < base.TriggerConditions.Length; i++)
			{
				base.TriggerConditions[i] = new TriggerCondition(_conditions[i].Property, LogicalOp.Equals, _conditions[i].Value, (_conditions[i].SourceName != null) ? _conditions[i].SourceName : "~Self");
			}
			for (int j = 0; j < PropertyValues.Count; j++)
			{
				PropertyValue value = PropertyValues[j];
				value.Conditions = base.TriggerConditions;
				PropertyValues[j] = value;
			}
			base.Seal();
		}
	}

	internal override bool GetCurrentState(DependencyObject container, UncommonField<HybridDictionary[]> dataField)
	{
		bool flag = base.TriggerConditions.Length != 0;
		int num = 0;
		while (flag && num < base.TriggerConditions.Length)
		{
			flag = base.TriggerConditions[num].Match(container.GetValue(base.TriggerConditions[num].Property));
			num++;
		}
		return flag;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.MultiTrigger" /> class.</summary>
	public MultiTrigger()
	{
	}
}
