using System.Collections.ObjectModel;

namespace System.Windows;

/// <summary>Represents a collection of <see cref="T:System.Windows.SetterBase" /> objects.</summary>
public sealed class SetterBaseCollection : Collection<SetterBase>
{
	private bool _sealed;

	/// <summary>Gets a value that indicates whether this object is in a read-only state.</summary>
	/// <returns>true if this object is in a read-only state and cannot be changed; otherwise, false.</returns>
	public bool IsSealed => _sealed;

	protected override void ClearItems()
	{
		CheckSealed();
		base.ClearItems();
	}

	protected override void InsertItem(int index, SetterBase item)
	{
		CheckSealed();
		SetterBaseValidation(item);
		base.InsertItem(index, item);
	}

	protected override void RemoveItem(int index)
	{
		CheckSealed();
		base.RemoveItem(index);
	}

	protected override void SetItem(int index, SetterBase item)
	{
		CheckSealed();
		SetterBaseValidation(item);
		base.SetItem(index, item);
	}

	internal void Seal()
	{
		_sealed = true;
		for (int i = 0; i < base.Count; i++)
		{
			base[i].Seal();
		}
	}

	private void CheckSealed()
	{
		if (_sealed)
		{
			throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "SetterBaseCollection"));
		}
	}

	private void SetterBaseValidation(SetterBase setterBase)
	{
		if (setterBase == null)
		{
			throw new ArgumentNullException("setterBase");
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.SetterBaseCollection" /> class.</summary>
	public SetterBaseCollection()
	{
	}
}
