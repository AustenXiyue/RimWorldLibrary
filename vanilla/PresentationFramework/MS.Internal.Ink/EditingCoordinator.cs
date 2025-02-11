using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MS.Internal.Ink;

internal class EditingCoordinator
{
	[Flags]
	private enum BehaviorValidFlag
	{
		InkCollectionBehaviorCursorValid = 1,
		EraserBehaviorCursorValid = 2,
		LassoSelectionBehaviorCursorValid = 4,
		SelectionEditingBehaviorCursorValid = 8,
		SelectionEditorCursorValid = 0x10,
		InkCollectionBehaviorTransformValid = 0x20,
		EraserBehaviorTransformValid = 0x40,
		LassoSelectionBehaviorTransformValid = 0x80,
		SelectionEditingBehaviorTransformValid = 0x100,
		SelectionEditorTransformValid = 0x200
	}

	private InkCanvas _inkCanvas;

	private Stack<EditingBehavior> _activationStack;

	private InkCollectionBehavior _inkCollectionBehavior;

	private EraserBehavior _eraserBehavior;

	private LassoSelectionBehavior _lassoSelectionBehavior;

	private SelectionEditingBehavior _selectionEditingBehavior;

	private SelectionEditor _selectionEditor;

	private bool _moveEnabled = true;

	private bool _resizeEnabled = true;

	private bool _userIsEditing;

	private bool _stylusIsInverted;

	private StylusPointDescription _commonDescription;

	private StylusDevice _capturedStylus;

	private MouseDevice _capturedMouse;

	private BehaviorValidFlag _behaviorValidFlag;

	internal bool MoveEnabled
	{
		get
		{
			return _moveEnabled;
		}
		set
		{
			_moveEnabled = value;
		}
	}

	internal bool UserIsEditing
	{
		get
		{
			return _userIsEditing;
		}
		set
		{
			_userIsEditing = value;
		}
	}

	internal bool StylusOrMouseIsDown
	{
		get
		{
			bool flag = false;
			StylusDevice currentStylusDevice = Stylus.CurrentStylusDevice;
			if (currentStylusDevice != null && _inkCanvas.IsStylusOver && !currentStylusDevice.InAir)
			{
				flag = true;
			}
			bool flag2 = _inkCanvas.IsMouseOver && Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed;
			if (flag || flag2)
			{
				return true;
			}
			return false;
		}
	}

	internal bool ResizeEnabled
	{
		get
		{
			return _resizeEnabled;
		}
		set
		{
			_resizeEnabled = value;
		}
	}

	internal LassoSelectionBehavior LassoSelectionBehavior
	{
		get
		{
			if (_lassoSelectionBehavior == null)
			{
				_lassoSelectionBehavior = new LassoSelectionBehavior(this, _inkCanvas);
			}
			return _lassoSelectionBehavior;
		}
	}

	internal SelectionEditingBehavior SelectionEditingBehavior
	{
		get
		{
			if (_selectionEditingBehavior == null)
			{
				_selectionEditingBehavior = new SelectionEditingBehavior(this, _inkCanvas);
			}
			return _selectionEditingBehavior;
		}
	}

	internal InkCanvasEditingMode ActiveEditingMode
	{
		get
		{
			if (_stylusIsInverted)
			{
				return _inkCanvas.EditingModeInverted;
			}
			return _inkCanvas.EditingMode;
		}
	}

	internal SelectionEditor SelectionEditor
	{
		get
		{
			if (_selectionEditor == null)
			{
				_selectionEditor = new SelectionEditor(this, _inkCanvas);
			}
			return _selectionEditor;
		}
	}

	internal bool IsInMidStroke
	{
		get
		{
			if (_capturedStylus == null)
			{
				return _capturedMouse != null;
			}
			return true;
		}
	}

	internal bool IsStylusInverted => _stylusIsInverted;

	private EditingBehavior ActiveEditingBehavior
	{
		get
		{
			EditingBehavior result = null;
			if (_activationStack.Count > 0)
			{
				result = _activationStack.Peek();
			}
			return result;
		}
	}

	internal InkCollectionBehavior InkCollectionBehavior
	{
		get
		{
			if (_inkCollectionBehavior == null)
			{
				_inkCollectionBehavior = new InkCollectionBehavior(this, _inkCanvas);
			}
			return _inkCollectionBehavior;
		}
	}

	private EraserBehavior EraserBehavior
	{
		get
		{
			if (_eraserBehavior == null)
			{
				_eraserBehavior = new EraserBehavior(this, _inkCanvas);
			}
			return _eraserBehavior;
		}
	}

	internal EditingCoordinator(InkCanvas inkCanvas)
	{
		if (inkCanvas == null)
		{
			throw new ArgumentNullException("inkCanvas");
		}
		_inkCanvas = inkCanvas;
		_activationStack = new Stack<EditingBehavior>(2);
		_inkCanvas.AddHandler(Stylus.StylusInRangeEvent, new StylusEventHandler(OnInkCanvasStylusInAirOrInRangeMove));
		_inkCanvas.AddHandler(Stylus.StylusInAirMoveEvent, new StylusEventHandler(OnInkCanvasStylusInAirOrInRangeMove));
		_inkCanvas.AddHandler(Stylus.StylusOutOfRangeEvent, new StylusEventHandler(OnInkCanvasStylusOutOfRange));
	}

	internal void ActivateDynamicBehavior(EditingBehavior dynamicBehavior, InputDevice inputDevice)
	{
		PushEditingBehavior(dynamicBehavior);
		if (dynamicBehavior == LassoSelectionBehavior)
		{
			bool flag = false;
			try
			{
				InitializeCapture(inputDevice, (IStylusEditing)dynamicBehavior, userInitiated: true, resetDynamicRenderer: false);
				flag = true;
			}
			finally
			{
				if (!flag)
				{
					ActiveEditingBehavior.Commit(commit: false);
					ReleaseCapture(releaseDevice: true);
				}
			}
		}
		_inkCanvas.RaiseActiveEditingModeChanged(new RoutedEventArgs(InkCanvas.ActiveEditingModeChangedEvent, _inkCanvas));
	}

	internal void DeactivateDynamicBehavior()
	{
		PopEditingBehavior();
	}

	internal void UpdateActiveEditingState()
	{
		UpdateEditingState(_stylusIsInverted);
	}

	internal void UpdateEditingState(bool inverted)
	{
		if (inverted != _stylusIsInverted)
		{
			return;
		}
		EditingBehavior activeEditingBehavior = ActiveEditingBehavior;
		EditingBehavior behavior = GetBehavior(ActiveEditingMode);
		if (UserIsEditing)
		{
			if (IsInMidStroke)
			{
				((StylusEditingBehavior)activeEditingBehavior).SwitchToMode(ActiveEditingMode);
			}
			else
			{
				if (activeEditingBehavior == SelectionEditingBehavior)
				{
					activeEditingBehavior.Commit(commit: true);
				}
				ChangeEditingBehavior(behavior);
			}
		}
		else if (IsInMidStroke)
		{
			((StylusEditingBehavior)activeEditingBehavior).SwitchToMode(ActiveEditingMode);
		}
		else
		{
			ChangeEditingBehavior(behavior);
		}
		_inkCanvas.UpdateCursor();
	}

	internal void UpdatePointEraserCursor()
	{
		if (ActiveEditingMode == InkCanvasEditingMode.EraseByPoint)
		{
			InvalidateBehaviorCursor(EraserBehavior);
		}
	}

	internal void InvalidateTransform()
	{
		SetTransformValid(InkCollectionBehavior, isValid: false);
		SetTransformValid(EraserBehavior, isValid: false);
	}

	internal void InvalidateBehaviorCursor(EditingBehavior behavior)
	{
		SetCursorValid(behavior, isValid: false);
		if (!GetTransformValid(behavior))
		{
			behavior.UpdateTransform();
			SetTransformValid(behavior, isValid: true);
		}
		if (behavior == ActiveEditingBehavior)
		{
			_inkCanvas.UpdateCursor();
		}
	}

	internal bool IsCursorValid(EditingBehavior behavior)
	{
		return GetCursorValid(behavior);
	}

	internal bool IsTransformValid(EditingBehavior behavior)
	{
		return GetTransformValid(behavior);
	}

	internal IStylusEditing ChangeStylusEditingMode(StylusEditingBehavior sourceBehavior, InkCanvasEditingMode newMode)
	{
		if (IsInMidStroke && ((sourceBehavior != LassoSelectionBehavior && sourceBehavior == ActiveEditingBehavior) || (sourceBehavior == LassoSelectionBehavior && SelectionEditor == ActiveEditingBehavior)))
		{
			PopEditingBehavior();
			EditingBehavior behavior = GetBehavior(ActiveEditingMode);
			if (behavior != null)
			{
				PushEditingBehavior(behavior);
				if (newMode == InkCanvasEditingMode.Select && behavior == SelectionEditor)
				{
					PushEditingBehavior(LassoSelectionBehavior);
				}
			}
			else
			{
				ReleaseCapture(releaseDevice: true);
			}
			_inkCanvas.RaiseActiveEditingModeChanged(new RoutedEventArgs(InkCanvas.ActiveEditingModeChangedEvent, _inkCanvas));
			return ActiveEditingBehavior as IStylusEditing;
		}
		return null;
	}

	[Conditional("DEBUG")]
	internal void DebugCheckActiveBehavior(EditingBehavior behavior)
	{
	}

	[Conditional("DEBUG")]
	private void DebugCheckDynamicBehavior(EditingBehavior behavior)
	{
	}

	[Conditional("DEBUG")]
	private void DebugCheckNonDynamicBehavior(EditingBehavior behavior)
	{
	}

	internal InputDevice GetInputDeviceForReset()
	{
		if (_capturedStylus != null && !_capturedStylus.InAir)
		{
			return _capturedStylus;
		}
		if (_capturedMouse != null && _capturedMouse.LeftButton == MouseButtonState.Pressed)
		{
			return _capturedMouse;
		}
		return null;
	}

	private EditingBehavior GetBehavior(InkCanvasEditingMode editingMode)
	{
		switch (editingMode)
		{
		case InkCanvasEditingMode.Ink:
		case InkCanvasEditingMode.GestureOnly:
		case InkCanvasEditingMode.InkAndGesture:
			return InkCollectionBehavior;
		case InkCanvasEditingMode.Select:
			return SelectionEditor;
		case InkCanvasEditingMode.EraseByPoint:
		case InkCanvasEditingMode.EraseByStroke:
			return EraserBehavior;
		default:
			return null;
		}
	}

	private void PushEditingBehavior(EditingBehavior newEditingBehavior)
	{
		ActiveEditingBehavior?.Deactivate();
		_activationStack.Push(newEditingBehavior);
		newEditingBehavior.Activate();
	}

	private void PopEditingBehavior()
	{
		EditingBehavior activeEditingBehavior = ActiveEditingBehavior;
		if (activeEditingBehavior != null)
		{
			activeEditingBehavior.Deactivate();
			_activationStack.Pop();
			activeEditingBehavior = ActiveEditingBehavior;
			if (ActiveEditingBehavior != null)
			{
				activeEditingBehavior.Activate();
			}
		}
	}

	private void OnInkCanvasStylusInAirOrInRangeMove(object sender, StylusEventArgs args)
	{
		if (_capturedMouse != null)
		{
			if (ActiveEditingBehavior == InkCollectionBehavior && _inkCanvas.InternalDynamicRenderer != null)
			{
				_inkCanvas.InternalDynamicRenderer.Enabled = false;
				_inkCanvas.InternalDynamicRenderer.Enabled = true;
			}
			ActiveEditingBehavior.Commit(commit: true);
			ReleaseCapture(releaseDevice: true);
		}
		UpdateInvertedState(args.StylusDevice, args.Inverted);
	}

	private void OnInkCanvasStylusOutOfRange(object sender, StylusEventArgs args)
	{
		UpdateInvertedState(args.StylusDevice, stylusIsInverted: false);
	}

	internal void OnInkCanvasDeviceDown(object sender, InputEventArgs args)
	{
		MouseButtonEventArgs mouseButtonEventArgs = args as MouseButtonEventArgs;
		bool resetDynamicRenderer = false;
		if (mouseButtonEventArgs != null)
		{
			if (_inkCanvas.Focus() && ActiveEditingMode != 0)
			{
				mouseButtonEventArgs.Handled = true;
			}
			if (mouseButtonEventArgs.ChangedButton != 0)
			{
				return;
			}
			if (mouseButtonEventArgs.StylusDevice != null)
			{
				UpdateInvertedState(mouseButtonEventArgs.StylusDevice, mouseButtonEventArgs.StylusDevice.Inverted);
			}
		}
		else
		{
			StylusEventArgs stylusEventArgs = args as StylusEventArgs;
			UpdateInvertedState(stylusEventArgs.StylusDevice, stylusEventArgs.Inverted);
		}
		IStylusEditing stylusEditing = ActiveEditingBehavior as IStylusEditing;
		if (IsInMidStroke || stylusEditing == null)
		{
			return;
		}
		bool flag = false;
		try
		{
			InputDevice inputDevice = null;
			if (mouseButtonEventArgs != null && mouseButtonEventArgs.StylusDevice != null)
			{
				inputDevice = mouseButtonEventArgs.StylusDevice;
				resetDynamicRenderer = true;
			}
			else
			{
				inputDevice = args.Device;
			}
			InitializeCapture(inputDevice, stylusEditing, args.UserInitiated, resetDynamicRenderer);
			flag = true;
		}
		finally
		{
			if (!flag)
			{
				ActiveEditingBehavior.Commit(commit: false);
				ReleaseCapture(IsInMidStroke);
			}
		}
	}

	private void OnInkCanvasDeviceMove<TEventArgs>(object sender, TEventArgs args) where TEventArgs : InputEventArgs
	{
		if (!IsInputDeviceCaptured(args.Device) || !(ActiveEditingBehavior is IStylusEditing stylusEditing))
		{
			return;
		}
		StylusPointCollection stylusPoints;
		if (_capturedStylus != null)
		{
			stylusPoints = _capturedStylus.GetStylusPoints(_inkCanvas, _commonDescription);
		}
		else
		{
			if (args is MouseEventArgs { StylusDevice: not null })
			{
				return;
			}
			stylusPoints = new StylusPointCollection(new Point[1] { _capturedMouse.GetPosition(_inkCanvas) });
		}
		bool flag = false;
		try
		{
			stylusEditing.AddStylusPoints(stylusPoints, args.UserInitiated);
			flag = true;
		}
		finally
		{
			if (!flag)
			{
				ActiveEditingBehavior.Commit(commit: false);
				ReleaseCapture(releaseDevice: true);
			}
		}
	}

	internal void OnInkCanvasDeviceUp(object sender, InputEventArgs args)
	{
		MouseButtonEventArgs mouseButtonEventArgs = args as MouseButtonEventArgs;
		StylusDevice stylusDevice = null;
		if (mouseButtonEventArgs != null)
		{
			stylusDevice = mouseButtonEventArgs.StylusDevice;
		}
		if ((!IsInputDeviceCaptured(args.Device) && (stylusDevice == null || !IsInputDeviceCaptured(stylusDevice))) || (_capturedMouse != null && mouseButtonEventArgs != null && mouseButtonEventArgs.ChangedButton != 0))
		{
			return;
		}
		try
		{
			if (ActiveEditingBehavior != null)
			{
				ActiveEditingBehavior.Commit(commit: true);
			}
		}
		finally
		{
			ReleaseCapture(releaseDevice: true);
		}
	}

	private void OnInkCanvasLostDeviceCapture<TEventArgs>(object sender, TEventArgs args) where TEventArgs : InputEventArgs
	{
		if (UserIsEditing)
		{
			ReleaseCapture(releaseDevice: false);
			if (ActiveEditingBehavior == InkCollectionBehavior && _inkCanvas.InternalDynamicRenderer != null)
			{
				_inkCanvas.InternalDynamicRenderer.Enabled = false;
				_inkCanvas.InternalDynamicRenderer.Enabled = true;
			}
			ActiveEditingBehavior.Commit(commit: true);
		}
	}

	private void InitializeCapture(InputDevice inputDevice, IStylusEditing stylusEditingBehavior, bool userInitiated, bool resetDynamicRenderer)
	{
		_capturedStylus = inputDevice as StylusDevice;
		_capturedMouse = inputDevice as MouseDevice;
		if (_capturedStylus != null)
		{
			StylusPointCollection stylusPoints = _capturedStylus.GetStylusPoints(_inkCanvas);
			_commonDescription = StylusPointDescription.GetCommonDescription(_inkCanvas.DefaultStylusPointDescription, stylusPoints.Description);
			StylusPointCollection stylusPoints2 = stylusPoints.Reformat(_commonDescription);
			if (resetDynamicRenderer && stylusEditingBehavior is InkCollectionBehavior inkCollectionBehavior)
			{
				inkCollectionBehavior.ResetDynamicRenderer();
			}
			stylusEditingBehavior.AddStylusPoints(stylusPoints2, userInitiated);
			_inkCanvas.CaptureStylus();
			if (_inkCanvas.IsStylusCaptured && ActiveEditingMode != 0)
			{
				_inkCanvas.AddHandler(Stylus.StylusMoveEvent, new StylusEventHandler(OnInkCanvasDeviceMove));
				_inkCanvas.AddHandler(UIElement.LostStylusCaptureEvent, new StylusEventHandler(OnInkCanvasLostDeviceCapture));
			}
			else
			{
				_capturedStylus = null;
			}
		}
		else
		{
			_commonDescription = null;
			StylusPointCollection stylusPoints2 = new StylusPointCollection(new Point[1] { _capturedMouse.GetPosition(_inkCanvas) });
			stylusEditingBehavior.AddStylusPoints(stylusPoints2, userInitiated);
			_inkCanvas.CaptureMouse();
			if (_inkCanvas.IsMouseCaptured && ActiveEditingMode != 0)
			{
				_inkCanvas.AddHandler(Mouse.MouseMoveEvent, new MouseEventHandler(OnInkCanvasDeviceMove));
				_inkCanvas.AddHandler(UIElement.LostMouseCaptureEvent, new MouseEventHandler(OnInkCanvasLostDeviceCapture));
			}
			else
			{
				_capturedMouse = null;
			}
		}
	}

	private void ReleaseCapture(bool releaseDevice)
	{
		if (_capturedStylus != null)
		{
			_commonDescription = null;
			_inkCanvas.RemoveHandler(Stylus.StylusMoveEvent, new StylusEventHandler(OnInkCanvasDeviceMove));
			_inkCanvas.RemoveHandler(UIElement.LostStylusCaptureEvent, new StylusEventHandler(OnInkCanvasLostDeviceCapture));
			_capturedStylus = null;
			if (releaseDevice)
			{
				_inkCanvas.ReleaseStylusCapture();
			}
		}
		else if (_capturedMouse != null)
		{
			_inkCanvas.RemoveHandler(Mouse.MouseMoveEvent, new MouseEventHandler(OnInkCanvasDeviceMove));
			_inkCanvas.RemoveHandler(UIElement.LostMouseCaptureEvent, new MouseEventHandler(OnInkCanvasLostDeviceCapture));
			_capturedMouse = null;
			if (releaseDevice)
			{
				_inkCanvas.ReleaseMouseCapture();
			}
		}
	}

	private bool IsInputDeviceCaptured(InputDevice inputDevice)
	{
		if (inputDevice != _capturedStylus || ((StylusDevice)inputDevice).Captured != _inkCanvas)
		{
			if (inputDevice == _capturedMouse)
			{
				return ((MouseDevice)inputDevice).Captured == _inkCanvas;
			}
			return false;
		}
		return true;
	}

	internal Cursor GetActiveBehaviorCursor()
	{
		EditingBehavior activeEditingBehavior = ActiveEditingBehavior;
		if (activeEditingBehavior == null)
		{
			return Cursors.Arrow;
		}
		Cursor cursor = activeEditingBehavior.Cursor;
		if (!GetCursorValid(activeEditingBehavior))
		{
			SetCursorValid(activeEditingBehavior, isValid: true);
		}
		return cursor;
	}

	private bool GetCursorValid(EditingBehavior behavior)
	{
		BehaviorValidFlag behaviorCursorFlag = GetBehaviorCursorFlag(behavior);
		return GetBitFlag(behaviorCursorFlag);
	}

	private void SetCursorValid(EditingBehavior behavior, bool isValid)
	{
		BehaviorValidFlag behaviorCursorFlag = GetBehaviorCursorFlag(behavior);
		SetBitFlag(behaviorCursorFlag, isValid);
	}

	private bool GetTransformValid(EditingBehavior behavior)
	{
		BehaviorValidFlag behaviorTransformFlag = GetBehaviorTransformFlag(behavior);
		return GetBitFlag(behaviorTransformFlag);
	}

	private void SetTransformValid(EditingBehavior behavior, bool isValid)
	{
		BehaviorValidFlag behaviorTransformFlag = GetBehaviorTransformFlag(behavior);
		SetBitFlag(behaviorTransformFlag, isValid);
	}

	private bool GetBitFlag(BehaviorValidFlag flag)
	{
		return (_behaviorValidFlag & flag) != 0;
	}

	private void SetBitFlag(BehaviorValidFlag flag, bool value)
	{
		if (value)
		{
			_behaviorValidFlag |= flag;
		}
		else
		{
			_behaviorValidFlag &= ~flag;
		}
	}

	private BehaviorValidFlag GetBehaviorCursorFlag(EditingBehavior behavior)
	{
		BehaviorValidFlag result = (BehaviorValidFlag)0;
		if (behavior == InkCollectionBehavior)
		{
			result = BehaviorValidFlag.InkCollectionBehaviorCursorValid;
		}
		else if (behavior == EraserBehavior)
		{
			result = BehaviorValidFlag.EraserBehaviorCursorValid;
		}
		else if (behavior == LassoSelectionBehavior)
		{
			result = BehaviorValidFlag.LassoSelectionBehaviorCursorValid;
		}
		else if (behavior == SelectionEditingBehavior)
		{
			result = BehaviorValidFlag.SelectionEditingBehaviorCursorValid;
		}
		else if (behavior == SelectionEditor)
		{
			result = BehaviorValidFlag.SelectionEditorCursorValid;
		}
		return result;
	}

	private BehaviorValidFlag GetBehaviorTransformFlag(EditingBehavior behavior)
	{
		BehaviorValidFlag result = (BehaviorValidFlag)0;
		if (behavior == InkCollectionBehavior)
		{
			result = BehaviorValidFlag.InkCollectionBehaviorTransformValid;
		}
		else if (behavior == EraserBehavior)
		{
			result = BehaviorValidFlag.EraserBehaviorTransformValid;
		}
		else if (behavior == LassoSelectionBehavior)
		{
			result = BehaviorValidFlag.LassoSelectionBehaviorTransformValid;
		}
		else if (behavior == SelectionEditingBehavior)
		{
			result = BehaviorValidFlag.SelectionEditingBehaviorTransformValid;
		}
		else if (behavior == SelectionEditor)
		{
			result = BehaviorValidFlag.SelectionEditorTransformValid;
		}
		return result;
	}

	private void ChangeEditingBehavior(EditingBehavior newBehavior)
	{
		try
		{
			_inkCanvas.ClearSelection(raiseSelectionChangedEvent: true);
		}
		finally
		{
			if (ActiveEditingBehavior != null)
			{
				PopEditingBehavior();
			}
			if (newBehavior != null)
			{
				PushEditingBehavior(newBehavior);
			}
			_inkCanvas.RaiseActiveEditingModeChanged(new RoutedEventArgs(InkCanvas.ActiveEditingModeChangedEvent, _inkCanvas));
		}
	}

	private bool UpdateInvertedState(StylusDevice stylusDevice, bool stylusIsInverted)
	{
		if ((!IsInMidStroke || (IsInMidStroke && IsInputDeviceCaptured(stylusDevice))) && stylusIsInverted != _stylusIsInverted)
		{
			_stylusIsInverted = stylusIsInverted;
			UpdateActiveEditingState();
			return true;
		}
		return false;
	}
}
