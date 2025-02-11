using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MS.Internal.Ink;

internal class SelectionEditor : EditingBehavior
{
	private InkCanvasSelectionHitResult _hitResult;

	internal SelectionEditor(EditingCoordinator editingCoordinator, InkCanvas inkCanvas)
		: base(editingCoordinator, inkCanvas)
	{
	}

	internal void OnInkCanvasSelectionChanged()
	{
		Point position = Mouse.PrimaryDevice.GetPosition(base.InkCanvas.SelectionAdorner);
		UpdateSelectionCursor(position);
	}

	protected override void OnActivate()
	{
		base.InkCanvas.SelectionAdorner.AddHandler(Mouse.MouseDownEvent, new MouseButtonEventHandler(OnAdornerMouseButtonDownEvent));
		base.InkCanvas.SelectionAdorner.AddHandler(Mouse.MouseMoveEvent, new MouseEventHandler(OnAdornerMouseMoveEvent));
		base.InkCanvas.SelectionAdorner.AddHandler(Mouse.MouseEnterEvent, new MouseEventHandler(OnAdornerMouseMoveEvent));
		base.InkCanvas.SelectionAdorner.AddHandler(Mouse.MouseLeaveEvent, new MouseEventHandler(OnAdornerMouseLeaveEvent));
		Point position = Mouse.PrimaryDevice.GetPosition(base.InkCanvas.SelectionAdorner);
		UpdateSelectionCursor(position);
	}

	protected override void OnDeactivate()
	{
		base.InkCanvas.SelectionAdorner.RemoveHandler(Mouse.MouseDownEvent, new MouseButtonEventHandler(OnAdornerMouseButtonDownEvent));
		base.InkCanvas.SelectionAdorner.RemoveHandler(Mouse.MouseMoveEvent, new MouseEventHandler(OnAdornerMouseMoveEvent));
		base.InkCanvas.SelectionAdorner.RemoveHandler(Mouse.MouseEnterEvent, new MouseEventHandler(OnAdornerMouseMoveEvent));
		base.InkCanvas.SelectionAdorner.RemoveHandler(Mouse.MouseLeaveEvent, new MouseEventHandler(OnAdornerMouseLeaveEvent));
	}

	protected override void OnCommit(bool commit)
	{
	}

	protected override Cursor GetCurrentCursor()
	{
		if (base.InkCanvas.SelectionAdorner.IsMouseOver)
		{
			return PenCursorManager.GetSelectionCursor(_hitResult, base.InkCanvas.FlowDirection == FlowDirection.RightToLeft);
		}
		return null;
	}

	private void OnAdornerMouseButtonDownEvent(object sender, MouseButtonEventArgs args)
	{
		if (args.StylusDevice != null || args.LeftButton == MouseButtonState.Pressed)
		{
			Point position = args.GetPosition(base.InkCanvas.SelectionAdorner);
			if (HitTestOnSelectionAdorner(position) != 0)
			{
				base.EditingCoordinator.ActivateDynamicBehavior(base.EditingCoordinator.SelectionEditingBehavior, args.Device);
			}
			else
			{
				base.EditingCoordinator.ActivateDynamicBehavior(base.EditingCoordinator.LassoSelectionBehavior, (args.StylusDevice != null) ? args.StylusDevice : args.Device);
			}
		}
	}

	private void OnAdornerMouseMoveEvent(object sender, MouseEventArgs args)
	{
		Point position = args.GetPosition(base.InkCanvas.SelectionAdorner);
		UpdateSelectionCursor(position);
	}

	private void OnAdornerMouseLeaveEvent(object sender, MouseEventArgs args)
	{
		base.EditingCoordinator.InvalidateBehaviorCursor(this);
	}

	private InkCanvasSelectionHitResult HitTestOnSelectionAdorner(Point position)
	{
		InkCanvasSelectionHitResult inkCanvasSelectionHitResult = InkCanvasSelectionHitResult.None;
		if (base.InkCanvas.InkCanvasSelection.HasSelection)
		{
			inkCanvasSelectionHitResult = base.InkCanvas.SelectionAdorner.SelectionHandleHitTest(position);
			if (inkCanvasSelectionHitResult >= InkCanvasSelectionHitResult.TopLeft && inkCanvasSelectionHitResult <= InkCanvasSelectionHitResult.Left)
			{
				inkCanvasSelectionHitResult = (base.InkCanvas.ResizeEnabled ? inkCanvasSelectionHitResult : InkCanvasSelectionHitResult.None);
			}
			else if (inkCanvasSelectionHitResult == InkCanvasSelectionHitResult.Selection)
			{
				inkCanvasSelectionHitResult = (base.InkCanvas.MoveEnabled ? inkCanvasSelectionHitResult : InkCanvasSelectionHitResult.None);
			}
		}
		return inkCanvasSelectionHitResult;
	}

	private void UpdateSelectionCursor(Point hitPoint)
	{
		InkCanvasSelectionHitResult inkCanvasSelectionHitResult = HitTestOnSelectionAdorner(hitPoint);
		if (_hitResult != inkCanvasSelectionHitResult)
		{
			_hitResult = inkCanvasSelectionHitResult;
			base.EditingCoordinator.InvalidateBehaviorCursor(this);
		}
	}
}
