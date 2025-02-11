using System.Collections.ObjectModel;
using MS.Internal;

namespace System.Windows;

/// <summary>Represents a collection of <see cref="T:System.Windows.TriggerBase" /> objects.</summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public sealed class TriggerCollection : Collection<TriggerBase>
{
	private bool _sealed;

	private FrameworkElement _owner;

	/// <summary>Gets a value that indicates whether this collection is read-only and cannot be changed.</summary>
	/// <returns>true if this collection is read-only; otherwise, false.</returns>
	public bool IsSealed => _sealed;

	internal FrameworkElement Owner => _owner;

	internal TriggerCollection()
		: this(null)
	{
	}

	internal TriggerCollection(FrameworkElement owner)
	{
		_sealed = false;
		_owner = owner;
	}

	protected override void ClearItems()
	{
		CheckSealed();
		OnClear();
		base.ClearItems();
	}

	protected override void InsertItem(int index, TriggerBase item)
	{
		CheckSealed();
		TriggerBaseValidation(item);
		OnAdd(item);
		base.InsertItem(index, item);
	}

	protected override void RemoveItem(int index)
	{
		CheckSealed();
		TriggerBase triggerBase = base[index];
		OnRemove(triggerBase);
		base.RemoveItem(index);
	}

	protected override void SetItem(int index, TriggerBase item)
	{
		CheckSealed();
		TriggerBaseValidation(item);
		OnAdd(item);
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
			throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "TriggerCollection"));
		}
	}

	private void TriggerBaseValidation(TriggerBase triggerBase)
	{
		if (triggerBase == null)
		{
			throw new ArgumentNullException("triggerBase");
		}
	}

	private void OnAdd(TriggerBase triggerBase)
	{
		if (Owner != null && Owner.IsInitialized)
		{
			EventTrigger.ProcessOneTrigger(Owner, triggerBase);
		}
		InheritanceContextHelper.ProvideContextForObject(Owner, triggerBase);
	}

	private void OnRemove(TriggerBase triggerBase)
	{
		if (Owner != null)
		{
			if (Owner.IsInitialized)
			{
				EventTrigger.DisconnectOneTrigger(Owner, triggerBase);
			}
			InheritanceContextHelper.RemoveContextFromObject(Owner, triggerBase);
		}
	}

	private void OnClear()
	{
		if (Owner != null)
		{
			if (Owner.IsInitialized)
			{
				EventTrigger.DisconnectAllTriggers(Owner);
			}
			for (int num = base.Count - 1; num >= 0; num--)
			{
				InheritanceContextHelper.RemoveContextFromObject(Owner, base[num]);
			}
		}
	}
}
