namespace System.Windows;

/// <summary>Represents the base class for value setters. </summary>
[Localizability(LocalizationCategory.Ignore)]
public abstract class SetterBase
{
	private bool _sealed;

	/// <summary>Gets a value that indicates whether this object is in an immutable state.</summary>
	/// <returns>true if this object is in an immutable state; otherwise, false.</returns>
	public bool IsSealed => _sealed;

	internal SetterBase()
	{
	}

	internal virtual void Seal()
	{
		_sealed = true;
	}

	/// <summary>Checks whether this object is read-only and cannot be changed.</summary>
	protected void CheckSealed()
	{
		if (_sealed)
		{
			throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "SetterBase"));
		}
	}
}
