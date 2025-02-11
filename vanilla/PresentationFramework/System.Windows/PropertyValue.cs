namespace System.Windows;

internal struct PropertyValue
{
	internal PropertyValueType ValueType;

	internal TriggerCondition[] Conditions;

	internal string ChildName;

	internal DependencyProperty Property;

	internal object ValueInternal;

	internal object Value
	{
		get
		{
			if (ValueInternal is DeferredReference deferredReference)
			{
				ValueInternal = deferredReference.GetValue(BaseValueSourceInternal.Unknown);
			}
			return ValueInternal;
		}
	}
}
