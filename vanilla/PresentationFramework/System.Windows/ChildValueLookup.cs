namespace System.Windows;

internal struct ChildValueLookup
{
	internal ValueLookupType LookupType;

	internal TriggerCondition[] Conditions;

	internal DependencyProperty Property;

	internal object Value;

	public override bool Equals(object value)
	{
		if (value is ChildValueLookup childValueLookup && LookupType == childValueLookup.LookupType && Property == childValueLookup.Property && Value == childValueLookup.Value)
		{
			if (Conditions == null && childValueLookup.Conditions == null)
			{
				return true;
			}
			if (Conditions == null || childValueLookup.Conditions == null)
			{
				return false;
			}
			if (Conditions.Length == childValueLookup.Conditions.Length)
			{
				for (int i = 0; i < Conditions.Length; i++)
				{
					if (!Conditions[i].TypeSpecificEquals(childValueLookup.Conditions[i]))
					{
						return false;
					}
				}
				return true;
			}
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public static bool operator ==(ChildValueLookup value1, ChildValueLookup value2)
	{
		return value1.Equals(value2);
	}

	public static bool operator !=(ChildValueLookup value1, ChildValueLookup value2)
	{
		return !value1.Equals(value2);
	}
}
