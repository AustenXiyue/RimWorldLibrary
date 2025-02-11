using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace MS.Internal.Controls;

internal class ValidationRuleCollection : Collection<ValidationRule>
{
	protected override void InsertItem(int index, ValidationRule item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		base.InsertItem(index, item);
	}

	protected override void SetItem(int index, ValidationRule item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		base.SetItem(index, item);
	}
}
