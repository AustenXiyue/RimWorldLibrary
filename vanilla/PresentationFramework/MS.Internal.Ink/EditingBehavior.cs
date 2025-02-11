using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MS.Internal.Ink;

internal abstract class EditingBehavior
{
	private InkCanvas _inkCanvas;

	private EditingCoordinator _editingCoordinator;

	private Cursor _cachedCursor;

	public Cursor Cursor
	{
		get
		{
			if (_cachedCursor == null || !EditingCoordinator.IsCursorValid(this))
			{
				_cachedCursor = GetCurrentCursor();
			}
			return _cachedCursor;
		}
	}

	protected InkCanvas InkCanvas => _inkCanvas;

	protected EditingCoordinator EditingCoordinator => _editingCoordinator;

	internal EditingBehavior(EditingCoordinator editingCoordinator, InkCanvas inkCanvas)
	{
		if (inkCanvas == null)
		{
			throw new ArgumentNullException("inkCanvas");
		}
		if (editingCoordinator == null)
		{
			throw new ArgumentNullException("editingCoordinator");
		}
		_inkCanvas = inkCanvas;
		_editingCoordinator = editingCoordinator;
	}

	public void Activate()
	{
		OnActivate();
	}

	public void Deactivate()
	{
		OnDeactivate();
	}

	public void Commit(bool commit)
	{
		OnCommit(commit);
	}

	public void UpdateTransform()
	{
		if (!EditingCoordinator.IsTransformValid(this))
		{
			OnTransformChanged();
		}
	}

	protected abstract void OnActivate();

	protected abstract void OnDeactivate();

	protected abstract void OnCommit(bool commit);

	protected abstract Cursor GetCurrentCursor();

	protected void SelfDeactivate()
	{
		EditingCoordinator.DeactivateDynamicBehavior();
	}

	protected Matrix GetElementTransformMatrix()
	{
		Transform layoutTransform = InkCanvas.LayoutTransform;
		Transform renderTransform = InkCanvas.RenderTransform;
		return layoutTransform.Value * renderTransform.Value;
	}

	protected virtual void OnTransformChanged()
	{
	}
}
