using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MS.Internal.Ink;

internal sealed class SelectionEditingBehavior : EditingBehavior
{
	private const double MinimumHeightWidthSize = 16.0;

	private Point _previousLocation;

	private Rect _previousRect;

	private Rect _selectionRect;

	private InkCanvasSelectionHitResult _hitResult;

	private bool _actionStarted;

	internal SelectionEditingBehavior(EditingCoordinator editingCoordinator, InkCanvas inkCanvas)
		: base(editingCoordinator, inkCanvas)
	{
	}

	protected override void OnActivate()
	{
		_actionStarted = false;
		InitializeCapture();
		MouseDevice primaryDevice = Mouse.PrimaryDevice;
		_hitResult = base.InkCanvas.SelectionAdorner.SelectionHandleHitTest(primaryDevice.GetPosition(base.InkCanvas.SelectionAdorner));
		base.EditingCoordinator.InvalidateBehaviorCursor(this);
		_selectionRect = base.InkCanvas.GetSelectionBounds();
		_previousLocation = primaryDevice.GetPosition(base.InkCanvas.SelectionAdorner);
		_previousRect = _selectionRect;
		base.InkCanvas.InkCanvasSelection.StartFeedbackAdorner(_selectionRect, _hitResult);
		base.InkCanvas.SelectionAdorner.AddHandler(Mouse.MouseUpEvent, new MouseButtonEventHandler(OnMouseUp));
		base.InkCanvas.SelectionAdorner.AddHandler(Mouse.MouseMoveEvent, new MouseEventHandler(OnMouseMove));
		base.InkCanvas.SelectionAdorner.AddHandler(UIElement.LostMouseCaptureEvent, new MouseEventHandler(OnLostMouseCapture));
	}

	protected override void OnDeactivate()
	{
		base.InkCanvas.SelectionAdorner.RemoveHandler(Mouse.MouseUpEvent, new MouseButtonEventHandler(OnMouseUp));
		base.InkCanvas.SelectionAdorner.RemoveHandler(Mouse.MouseMoveEvent, new MouseEventHandler(OnMouseMove));
		base.InkCanvas.SelectionAdorner.RemoveHandler(UIElement.LostMouseCaptureEvent, new MouseEventHandler(OnLostMouseCapture));
	}

	protected override void OnCommit(bool commit)
	{
		ReleaseCapture(releaseDevice: true, commit);
	}

	protected override Cursor GetCurrentCursor()
	{
		return PenCursorManager.GetSelectionCursor(_hitResult, base.InkCanvas.FlowDirection == FlowDirection.RightToLeft);
	}

	private void OnMouseMove(object sender, MouseEventArgs args)
	{
		Point position = args.GetPosition(base.InkCanvas.SelectionAdorner);
		if (!DoubleUtil.AreClose(position.X, _previousLocation.X) || !DoubleUtil.AreClose(position.Y, _previousLocation.Y))
		{
			if (!_actionStarted)
			{
				_actionStarted = true;
			}
			Rect rect = ChangeFeedbackRectangle(position);
			base.InkCanvas.InkCanvasSelection.UpdateFeedbackAdorner(rect);
			_previousRect = rect;
		}
	}

	private void OnMouseUp(object sender, MouseButtonEventArgs args)
	{
		if (_actionStarted)
		{
			_previousRect = ChangeFeedbackRectangle(args.GetPosition(base.InkCanvas.SelectionAdorner));
		}
		Commit(commit: true);
	}

	private void OnLostMouseCapture(object sender, MouseEventArgs args)
	{
		if (base.EditingCoordinator.UserIsEditing)
		{
			ReleaseCapture(releaseDevice: false, commit: true);
		}
	}

	private Rect ChangeFeedbackRectangle(Point newPoint)
	{
		if ((_hitResult == InkCanvasSelectionHitResult.TopLeft || _hitResult == InkCanvasSelectionHitResult.BottomLeft || _hitResult == InkCanvasSelectionHitResult.Left) && newPoint.X > _selectionRect.Right - 16.0)
		{
			newPoint.X = _selectionRect.Right - 16.0;
		}
		if ((_hitResult == InkCanvasSelectionHitResult.TopRight || _hitResult == InkCanvasSelectionHitResult.BottomRight || _hitResult == InkCanvasSelectionHitResult.Right) && newPoint.X < _selectionRect.Left + 16.0)
		{
			newPoint.X = _selectionRect.Left + 16.0;
		}
		if ((_hitResult == InkCanvasSelectionHitResult.TopLeft || _hitResult == InkCanvasSelectionHitResult.TopRight || _hitResult == InkCanvasSelectionHitResult.Top) && newPoint.Y > _selectionRect.Bottom - 16.0)
		{
			newPoint.Y = _selectionRect.Bottom - 16.0;
		}
		if ((_hitResult == InkCanvasSelectionHitResult.BottomLeft || _hitResult == InkCanvasSelectionHitResult.BottomRight || _hitResult == InkCanvasSelectionHitResult.Bottom) && newPoint.Y < _selectionRect.Top + 16.0)
		{
			newPoint.Y = _selectionRect.Top + 16.0;
		}
		Rect result = CalculateRect(newPoint.X - _previousLocation.X, newPoint.Y - _previousLocation.Y);
		if (_hitResult == InkCanvasSelectionHitResult.BottomRight || _hitResult == InkCanvasSelectionHitResult.BottomLeft || _hitResult == InkCanvasSelectionHitResult.TopRight || _hitResult == InkCanvasSelectionHitResult.TopLeft || _hitResult == InkCanvasSelectionHitResult.Selection)
		{
			_previousLocation.X = newPoint.X;
			_previousLocation.Y = newPoint.Y;
			return result;
		}
		if (_hitResult == InkCanvasSelectionHitResult.Left || _hitResult == InkCanvasSelectionHitResult.Right)
		{
			_previousLocation.X = newPoint.X;
			return result;
		}
		if (_hitResult == InkCanvasSelectionHitResult.Top || _hitResult == InkCanvasSelectionHitResult.Bottom)
		{
			_previousLocation.Y = newPoint.Y;
		}
		return result;
	}

	private Rect CalculateRect(double x, double y)
	{
		Rect rect = _previousRect;
		switch (_hitResult)
		{
		case InkCanvasSelectionHitResult.BottomRight:
			rect = ExtendSelectionRight(rect, x);
			rect = ExtendSelectionBottom(rect, y);
			break;
		case InkCanvasSelectionHitResult.Bottom:
			rect = ExtendSelectionBottom(rect, y);
			break;
		case InkCanvasSelectionHitResult.BottomLeft:
			rect = ExtendSelectionLeft(rect, x);
			rect = ExtendSelectionBottom(rect, y);
			break;
		case InkCanvasSelectionHitResult.TopRight:
			rect = ExtendSelectionTop(rect, y);
			rect = ExtendSelectionRight(rect, x);
			break;
		case InkCanvasSelectionHitResult.Top:
			rect = ExtendSelectionTop(rect, y);
			break;
		case InkCanvasSelectionHitResult.TopLeft:
			rect = ExtendSelectionTop(rect, y);
			rect = ExtendSelectionLeft(rect, x);
			break;
		case InkCanvasSelectionHitResult.Left:
			rect = ExtendSelectionLeft(rect, x);
			break;
		case InkCanvasSelectionHitResult.Right:
			rect = ExtendSelectionRight(rect, x);
			break;
		case InkCanvasSelectionHitResult.Selection:
			rect.Offset(x, y);
			break;
		}
		return rect;
	}

	private static Rect ExtendSelectionLeft(Rect rect, double extendBy)
	{
		Rect result = rect;
		result.X += extendBy;
		result.Width -= extendBy;
		return result;
	}

	private static Rect ExtendSelectionTop(Rect rect, double extendBy)
	{
		Rect result = rect;
		result.Y += extendBy;
		result.Height -= extendBy;
		return result;
	}

	private static Rect ExtendSelectionRight(Rect rect, double extendBy)
	{
		Rect result = rect;
		result.Width += extendBy;
		return result;
	}

	private static Rect ExtendSelectionBottom(Rect rect, double extendBy)
	{
		Rect result = rect;
		result.Height += extendBy;
		return result;
	}

	private void InitializeCapture()
	{
		base.EditingCoordinator.UserIsEditing = true;
		base.InkCanvas.SelectionAdorner.CaptureMouse();
	}

	private void ReleaseCapture(bool releaseDevice, bool commit)
	{
		if (base.EditingCoordinator.UserIsEditing)
		{
			base.EditingCoordinator.UserIsEditing = false;
			if (releaseDevice)
			{
				base.InkCanvas.SelectionAdorner.ReleaseMouseCapture();
			}
			SelfDeactivate();
			base.InkCanvas.InkCanvasSelection.EndFeedbackAdorner(commit ? _previousRect : _selectionRect);
		}
	}
}
