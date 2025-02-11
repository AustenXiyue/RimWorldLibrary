using System.Collections.ObjectModel;

namespace System.Windows;

/// <summary>Represents a collection of <see cref="T:System.Windows.Condition" /> objects.</summary>
public sealed class ConditionCollection : Collection<Condition>
{
	private bool _sealed;

	/// <summary>Gets a value that indicates whether this trigger is read-only and cannot be changed .</summary>
	/// <returns>true if the trigger is read-only; otherwise, false.</returns>
	public bool IsSealed => _sealed;

	protected override void ClearItems()
	{
		CheckSealed();
		base.ClearItems();
	}

	protected override void InsertItem(int index, Condition item)
	{
		CheckSealed();
		ConditionValidation(item);
		base.InsertItem(index, item);
	}

	protected override void RemoveItem(int index)
	{
		CheckSealed();
		base.RemoveItem(index);
	}

	protected override void SetItem(int index, Condition item)
	{
		CheckSealed();
		ConditionValidation(item);
		base.SetItem(index, item);
	}

	internal void Seal(ValueLookupType type)
	{
		_sealed = true;
		for (int i = 0; i < base.Count; i++)
		{
			base[i].Seal(type);
		}
	}

	private void CheckSealed()
	{
		if (_sealed)
		{
			throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "ConditionCollection"));
		}
	}

	private void ConditionValidation(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is Condition))
		{
			throw new ArgumentException(SR.MustBeCondition);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.ConditionCollection" /> class.</summary>
	public ConditionCollection()
	{
	}
}
