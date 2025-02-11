using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace MS.Internal.Ink;

internal sealed class InkCollectionBehavior : StylusEditingBehavior
{
	private bool _resetDynamicRenderer;

	private StylusPointCollection _stylusPoints;

	private bool _userInitiated;

	private DrawingAttributes _strokeDrawingAttributes;

	private DrawingAttributes _cursorDrawingAttributes;

	private Cursor _cachedPenCursor;

	private Cursor PenCursor
	{
		get
		{
			if (_cachedPenCursor == null || _cursorDrawingAttributes != base.InkCanvas.DefaultDrawingAttributes)
			{
				Matrix elementTransformMatrix = GetElementTransformMatrix();
				DrawingAttributes drawingAttributes = base.InkCanvas.DefaultDrawingAttributes;
				if (!elementTransformMatrix.IsIdentity)
				{
					elementTransformMatrix *= drawingAttributes.StylusTipTransform;
					elementTransformMatrix.OffsetX = 0.0;
					elementTransformMatrix.OffsetY = 0.0;
					if (elementTransformMatrix.HasInverse)
					{
						drawingAttributes = drawingAttributes.Clone();
						drawingAttributes.StylusTipTransform = elementTransformMatrix;
					}
				}
				_cursorDrawingAttributes = base.InkCanvas.DefaultDrawingAttributes.Clone();
				DpiScale dpi = base.InkCanvas.GetDpi();
				_cachedPenCursor = PenCursorManager.GetPenCursor(drawingAttributes, isHollow: false, base.InkCanvas.FlowDirection == FlowDirection.RightToLeft, dpi.DpiScaleX, dpi.DpiScaleY);
			}
			return _cachedPenCursor;
		}
	}

	internal InkCollectionBehavior(EditingCoordinator editingCoordinator, InkCanvas inkCanvas)
		: base(editingCoordinator, inkCanvas)
	{
		_stylusPoints = null;
		_userInitiated = false;
	}

	internal void ResetDynamicRenderer()
	{
		_resetDynamicRenderer = true;
	}

	protected override void OnSwitchToMode(InkCanvasEditingMode mode)
	{
		switch (mode)
		{
		case InkCanvasEditingMode.Ink:
		case InkCanvasEditingMode.GestureOnly:
		case InkCanvasEditingMode.InkAndGesture:
			base.InkCanvas.RaiseActiveEditingModeChanged(new RoutedEventArgs(InkCanvas.ActiveEditingModeChangedEvent, base.InkCanvas));
			break;
		case InkCanvasEditingMode.EraseByPoint:
		case InkCanvasEditingMode.EraseByStroke:
			Commit(commit: false);
			base.EditingCoordinator.ChangeStylusEditingMode(this, mode);
			break;
		case InkCanvasEditingMode.Select:
		{
			StylusPointCollection stylusPointCollection = ((_stylusPoints != null) ? _stylusPoints.Clone() : null);
			Commit(commit: false);
			IStylusEditing stylusEditing = base.EditingCoordinator.ChangeStylusEditingMode(this, mode);
			if (stylusPointCollection != null)
			{
				stylusEditing?.AddStylusPoints(stylusPointCollection, userInitiated: false);
			}
			break;
		}
		case InkCanvasEditingMode.None:
			Commit(commit: false);
			base.EditingCoordinator.ChangeStylusEditingMode(this, mode);
			break;
		}
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		if (base.InkCanvas.InternalDynamicRenderer != null)
		{
			base.InkCanvas.InternalDynamicRenderer.Enabled = true;
			base.InkCanvas.UpdateDynamicRenderer();
		}
		_resetDynamicRenderer = base.EditingCoordinator.StylusOrMouseIsDown;
	}

	protected override void OnDeactivate()
	{
		base.OnDeactivate();
		if (base.InkCanvas.InternalDynamicRenderer != null)
		{
			base.InkCanvas.InternalDynamicRenderer.Enabled = false;
			base.InkCanvas.UpdateDynamicRenderer();
		}
	}

	protected override Cursor GetCurrentCursor()
	{
		if (base.EditingCoordinator.UserIsEditing)
		{
			return Cursors.None;
		}
		return PenCursor;
	}

	protected override void StylusInputBegin(StylusPointCollection stylusPoints, bool userInitiated)
	{
		_userInitiated = false;
		if (userInitiated)
		{
			_userInitiated = true;
		}
		_stylusPoints = new StylusPointCollection(stylusPoints.Description, 100);
		_stylusPoints.Add(stylusPoints);
		_strokeDrawingAttributes = base.InkCanvas.DefaultDrawingAttributes.Clone();
		if (_resetDynamicRenderer)
		{
			InputDevice inputDeviceForReset = base.EditingCoordinator.GetInputDeviceForReset();
			if (base.InkCanvas.InternalDynamicRenderer != null && inputDeviceForReset != null)
			{
				StylusDevice stylusDevice = inputDeviceForReset as StylusDevice;
				base.InkCanvas.InternalDynamicRenderer.Reset(stylusDevice, stylusPoints);
			}
			_resetDynamicRenderer = false;
		}
		base.EditingCoordinator.InvalidateBehaviorCursor(this);
	}

	protected override void StylusInputContinue(StylusPointCollection stylusPoints, bool userInitiated)
	{
		if (!userInitiated)
		{
			_userInitiated = false;
		}
		_stylusPoints.Add(stylusPoints);
	}

	protected override void StylusInputEnd(bool commit)
	{
		try
		{
			if (commit && _stylusPoints != null)
			{
				InkCanvasStrokeCollectedEventArgs e = new InkCanvasStrokeCollectedEventArgs(new Stroke(_stylusPoints, _strokeDrawingAttributes));
				base.InkCanvas.RaiseGestureOrStrokeCollected(e, _userInitiated);
			}
		}
		finally
		{
			_stylusPoints = null;
			_strokeDrawingAttributes = null;
			_userInitiated = false;
			base.EditingCoordinator.InvalidateBehaviorCursor(this);
		}
	}

	protected override void OnTransformChanged()
	{
		_cachedPenCursor = null;
	}
}
