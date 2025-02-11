using System.Windows.Controls;
using System.Windows.Input;

namespace MS.Internal.Ink;

internal abstract class StylusEditingBehavior : EditingBehavior, IStylusEditing
{
	private bool _disableInput;

	internal StylusEditingBehavior(EditingCoordinator editingCoordinator, InkCanvas inkCanvas)
		: base(editingCoordinator, inkCanvas)
	{
	}

	internal void SwitchToMode(InkCanvasEditingMode mode)
	{
		_disableInput = true;
		try
		{
			OnSwitchToMode(mode);
		}
		finally
		{
			_disableInput = false;
		}
	}

	void IStylusEditing.AddStylusPoints(StylusPointCollection stylusPoints, bool userInitiated)
	{
		if (!_disableInput)
		{
			if (!base.EditingCoordinator.UserIsEditing)
			{
				base.EditingCoordinator.UserIsEditing = true;
				StylusInputBegin(stylusPoints, userInitiated);
			}
			else
			{
				StylusInputContinue(stylusPoints, userInitiated);
			}
		}
	}

	protected abstract void OnSwitchToMode(InkCanvasEditingMode mode);

	protected override void OnActivate()
	{
	}

	protected override void OnDeactivate()
	{
	}

	protected sealed override void OnCommit(bool commit)
	{
		if (base.EditingCoordinator.UserIsEditing)
		{
			base.EditingCoordinator.UserIsEditing = false;
			StylusInputEnd(commit);
		}
		else
		{
			OnCommitWithoutStylusInput(commit);
		}
	}

	protected virtual void StylusInputBegin(StylusPointCollection stylusPoints, bool userInitiated)
	{
	}

	protected virtual void StylusInputContinue(StylusPointCollection stylusPoints, bool userInitiated)
	{
	}

	protected virtual void StylusInputEnd(bool commit)
	{
	}

	protected virtual void OnCommitWithoutStylusInput(bool commit)
	{
	}
}
