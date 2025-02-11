using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Input;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;
using MS.Internal;
using MS.Internal.Interop;
using MS.Internal.KnownBoxes;
using MS.Internal.Media;
using MS.Internal.PresentationCore;
using MS.Utility;
using MS.Win32;

namespace System.Windows;

/// <summary>
///   <see cref="T:System.Windows.UIElement" /> is a base class for WPF core level implementations building on Windows Presentation Foundation (WPF) elements and basic presentation characteristics. </summary>
[UidProperty("Uid")]
public class UIElement : Visual, IAnimatable, IInputElement
{
	private class InputHitTestResult
	{
		private HitTestResult _result;

		public DependencyObject Result
		{
			get
			{
				if (_result == null)
				{
					return null;
				}
				return _result.VisualHit;
			}
		}

		public HitTestResult HitTestResult => _result;

		public HitTestResultBehavior InputHitTestResultCallback(HitTestResult result)
		{
			_result = result;
			return HitTestResultBehavior.Stop;
		}
	}

	private static readonly Type _typeofThis;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewMouseDown" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewMouseDown" /> routed event.</returns>
	public static readonly RoutedEvent PreviewMouseDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.MouseDown" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.MouseDown" /> routed event.</returns>
	public static readonly RoutedEvent MouseDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewMouseUp" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewMouseUp" /> routed event.</returns>
	public static readonly RoutedEvent PreviewMouseUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.MouseUp" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.MouseUp" /> routed event.</returns>
	public static readonly RoutedEvent MouseUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewMouseLeftButtonDown" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewMouseLeftButtonDown" /> routed event.</returns>
	public static readonly RoutedEvent PreviewMouseLeftButtonDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.MouseLeftButtonDown" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.MouseLeftButtonDown" /> routed event.</returns>
	public static readonly RoutedEvent MouseLeftButtonDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewMouseLeftButtonUp" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewMouseLeftButtonUp" /> routed event.</returns>
	public static readonly RoutedEvent PreviewMouseLeftButtonUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.MouseLeftButtonUp" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.MouseLeftButtonUp" /> routed event.</returns>
	public static readonly RoutedEvent MouseLeftButtonUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewMouseRightButtonDown" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewMouseRightButtonDown" /> routed event.</returns>
	public static readonly RoutedEvent PreviewMouseRightButtonDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.MouseRightButtonDown" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.MouseRightButtonDown" /> routed event.</returns>
	public static readonly RoutedEvent MouseRightButtonDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewMouseRightButtonUp" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewMouseRightButtonUp" /> routed event.</returns>
	public static readonly RoutedEvent PreviewMouseRightButtonUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.MouseRightButtonUp" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.MouseRightButtonUp" /> routed event.</returns>
	public static readonly RoutedEvent MouseRightButtonUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewMouseMove" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewMouseMove" /> routed event.</returns>
	public static readonly RoutedEvent PreviewMouseMoveEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.MouseMove" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.MouseMove" /> routed event.</returns>
	public static readonly RoutedEvent MouseMoveEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewMouseWheel" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewMouseWheel" /> routed event.</returns>
	public static readonly RoutedEvent PreviewMouseWheelEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.MouseWheel" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.MouseWheel" /> routed event.</returns>
	public static readonly RoutedEvent MouseWheelEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.MouseEnter" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.MouseEnter" /> routed event.</returns>
	public static readonly RoutedEvent MouseEnterEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.MouseLeave" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.MouseLeave" /> routed event.</returns>
	public static readonly RoutedEvent MouseLeaveEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.GotMouseCapture" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.GotMouseCapture" /> routed event.</returns>
	public static readonly RoutedEvent GotMouseCaptureEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.LostMouseCapture" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.LostMouseCapture" /> routed event.</returns>
	public static readonly RoutedEvent LostMouseCaptureEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.QueryCursor" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.QueryCursor" /> routed event.</returns>
	public static readonly RoutedEvent QueryCursorEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewStylusDown" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewStylusDown" /> routed event.</returns>
	public static readonly RoutedEvent PreviewStylusDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.StylusDown" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.StylusDown" /> routed event.</returns>
	public static readonly RoutedEvent StylusDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewStylusUp" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewStylusUp" /> routed event. </returns>
	public static readonly RoutedEvent PreviewStylusUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.StylusUp" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.StylusUp" /> routed event.</returns>
	public static readonly RoutedEvent StylusUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewStylusMove" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewStylusMove" /> routed event.</returns>
	public static readonly RoutedEvent PreviewStylusMoveEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.StylusMove" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.StylusMove" /> routed event.</returns>
	public static readonly RoutedEvent StylusMoveEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewStylusInAirMove" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewStylusInAirMove" /> routed event.</returns>
	public static readonly RoutedEvent PreviewStylusInAirMoveEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.StylusInAirMove" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.StylusInAirMove" /> routed event.</returns>
	public static readonly RoutedEvent StylusInAirMoveEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.StylusEnter" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.StylusEnter" /> routed event.</returns>
	public static readonly RoutedEvent StylusEnterEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.StylusLeave" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.StylusLeave" /> routed event.</returns>
	public static readonly RoutedEvent StylusLeaveEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewStylusInRange" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewStylusInRange" /> routed event.</returns>
	public static readonly RoutedEvent PreviewStylusInRangeEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.StylusInRange" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.StylusInRange" /> routed event.</returns>
	public static readonly RoutedEvent StylusInRangeEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewStylusOutOfRange" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewStylusOutOfRange" /> routed event.</returns>
	public static readonly RoutedEvent PreviewStylusOutOfRangeEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.StylusOutOfRange" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.StylusOutOfRange" /> routed event.</returns>
	public static readonly RoutedEvent StylusOutOfRangeEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewStylusSystemGesture" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewStylusSystemGesture" /> routed event.</returns>
	public static readonly RoutedEvent PreviewStylusSystemGestureEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.StylusSystemGesture" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.StylusSystemGesture" /> routed event.</returns>
	public static readonly RoutedEvent StylusSystemGestureEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.GotStylusCapture" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.GotStylusCapture" /> routed event.</returns>
	public static readonly RoutedEvent GotStylusCaptureEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.LostStylusCapture" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.LostStylusCapture" /> routed event.</returns>
	public static readonly RoutedEvent LostStylusCaptureEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.StylusButtonDown" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.StylusButtonDown" /> routed event.</returns>
	public static readonly RoutedEvent StylusButtonDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.StylusButtonUp" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.StylusButtonUp" /> routed event.</returns>
	public static readonly RoutedEvent StylusButtonUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewStylusButtonDown" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewStylusButtonDown" /> routed event.</returns>
	public static readonly RoutedEvent PreviewStylusButtonDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewStylusButtonUp" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewStylusButtonUp" /> routed event.</returns>
	public static readonly RoutedEvent PreviewStylusButtonUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewKeyDown" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewKeyDown" /> routed event.</returns>
	public static readonly RoutedEvent PreviewKeyDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.KeyDown" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.KeyDown" /> routed event.</returns>
	public static readonly RoutedEvent KeyDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewKeyUp" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewKeyUp" /> routed event.</returns>
	public static readonly RoutedEvent PreviewKeyUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.KeyUp" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.KeyUp" /> routed event.</returns>
	public static readonly RoutedEvent KeyUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewGotKeyboardFocus" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewGotKeyboardFocus" /> routed event.</returns>
	public static readonly RoutedEvent PreviewGotKeyboardFocusEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.GotKeyboardFocus" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.GotKeyboardFocus" /> routed event.</returns>
	public static readonly RoutedEvent GotKeyboardFocusEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewLostKeyboardFocus" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewLostKeyboardFocus" /> routed event.</returns>
	public static readonly RoutedEvent PreviewLostKeyboardFocusEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.LostKeyboardFocus" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.LostKeyboardFocus" /> routed event.</returns>
	public static readonly RoutedEvent LostKeyboardFocusEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewTextInput" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewTextInput" /> routed event.</returns>
	public static readonly RoutedEvent PreviewTextInputEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.TextInput" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.TextInput" /> routed event.</returns>
	public static readonly RoutedEvent TextInputEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewQueryContinueDrag" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewQueryContinueDrag" /> routed event.</returns>
	public static readonly RoutedEvent PreviewQueryContinueDragEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.QueryContinueDrag" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.QueryContinueDrag" /> routed event.</returns>
	public static readonly RoutedEvent QueryContinueDragEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewGiveFeedback" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewGiveFeedback" /> routed event.</returns>
	public static readonly RoutedEvent PreviewGiveFeedbackEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.GiveFeedback" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.GiveFeedback" /> routed event.</returns>
	public static readonly RoutedEvent GiveFeedbackEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewDragEnter" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewDragEnter" /> routed event.</returns>
	public static readonly RoutedEvent PreviewDragEnterEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.DragEnter" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.DragEnter" /> routed event.</returns>
	public static readonly RoutedEvent DragEnterEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewDragOver" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewDragOver" /> routed event.</returns>
	public static readonly RoutedEvent PreviewDragOverEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.DragOver" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.DragOver" /> routed event.</returns>
	public static readonly RoutedEvent DragOverEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewDragLeave" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewDragLeave" /> routed event.</returns>
	public static readonly RoutedEvent PreviewDragLeaveEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.DragLeave" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.DragLeave" /> routed event.</returns>
	public static readonly RoutedEvent DragLeaveEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewDrop" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewDrop" /> routed event.</returns>
	public static readonly RoutedEvent PreviewDropEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.Drop" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.Drop" /> routed event.</returns>
	public static readonly RoutedEvent DropEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewTouchDown" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewTouchDown" /> routed event.</returns>
	public static readonly RoutedEvent PreviewTouchDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.TouchDown" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.TouchDown" /> routed event.</returns>
	public static readonly RoutedEvent TouchDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewTouchMove" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewTouchMove" /> routed event.</returns>
	public static readonly RoutedEvent PreviewTouchMoveEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.TouchMove" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.TouchMove" /> routed event.</returns>
	public static readonly RoutedEvent TouchMoveEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.PreviewTouchUp" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.PreviewTouchUp" /> routed event.</returns>
	public static readonly RoutedEvent PreviewTouchUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.TouchUp" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.TouchUp" /> routed event.</returns>
	public static readonly RoutedEvent TouchUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.GotTouchCapture" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.GotTouchCapture" /> routed event.</returns>
	public static readonly RoutedEvent GotTouchCaptureEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.LostTouchCapture" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.LostTouchCapture" /> routed event.</returns>
	public static readonly RoutedEvent LostTouchCaptureEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.TouchEnter" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.TouchEnter" /> routed event.</returns>
	public static readonly RoutedEvent TouchEnterEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.TouchLeave" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.TouchLeave" /> routed event.</returns>
	public static readonly RoutedEvent TouchLeaveEvent;

	internal static readonly DependencyPropertyKey IsMouseDirectlyOverPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.IsMouseDirectlyOver" /> dependency property.  </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.UIElement.IsMouseDirectlyOver" /> dependency property.</returns>
	public static readonly DependencyProperty IsMouseDirectlyOverProperty;

	internal static readonly EventPrivateKey IsMouseDirectlyOverChangedKey;

	internal static readonly DependencyPropertyKey IsMouseOverPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.IsMouseOver" /> dependency property.  </summary>
	/// <returns>The <see cref="P:System.Windows.UIElement.IsMouseOver" /> dependency property identifier.</returns>
	public static readonly DependencyProperty IsMouseOverProperty;

	internal static readonly DependencyPropertyKey IsStylusOverPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.IsStylusOver" /> dependency property.  </summary>
	/// <returns>The <see cref="P:System.Windows.UIElement.IsStylusOver" /> dependency property identifier.</returns>
	public static readonly DependencyProperty IsStylusOverProperty;

	internal static readonly DependencyPropertyKey IsKeyboardFocusWithinPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.IsKeyboardFocusWithin" /> dependency property.  </summary>
	/// <returns>The <see cref="P:System.Windows.UIElement.IsKeyboardFocusWithin" /> dependency property identifier.</returns>
	public static readonly DependencyProperty IsKeyboardFocusWithinProperty;

	internal static readonly EventPrivateKey IsKeyboardFocusWithinChangedKey;

	internal static readonly DependencyPropertyKey IsMouseCapturedPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.IsMouseCaptured" /> dependency property.  </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.UIElement.IsMouseCaptured" /> dependency property.</returns>
	public static readonly DependencyProperty IsMouseCapturedProperty;

	internal static readonly EventPrivateKey IsMouseCapturedChangedKey;

	internal static readonly DependencyPropertyKey IsMouseCaptureWithinPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.IsMouseCaptureWithin" /> dependency property.  </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.UIElement.IsMouseCaptureWithin" /> dependency property.</returns>
	public static readonly DependencyProperty IsMouseCaptureWithinProperty;

	internal static readonly EventPrivateKey IsMouseCaptureWithinChangedKey;

	internal static readonly DependencyPropertyKey IsStylusDirectlyOverPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.IsStylusDirectlyOver" /> dependency property.  </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.UIElement.IsStylusDirectlyOver" /> dependency property.</returns>
	public static readonly DependencyProperty IsStylusDirectlyOverProperty;

	internal static readonly EventPrivateKey IsStylusDirectlyOverChangedKey;

	internal static readonly DependencyPropertyKey IsStylusCapturedPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.IsStylusCaptured" /> dependency property.  </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.UIElement.IsStylusCaptured" /> dependency property.</returns>
	public static readonly DependencyProperty IsStylusCapturedProperty;

	internal static readonly EventPrivateKey IsStylusCapturedChangedKey;

	internal static readonly DependencyPropertyKey IsStylusCaptureWithinPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.IsStylusCaptureWithin" /> dependency property.  </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.UIElement.IsStylusCaptureWithin" /> dependency property.</returns>
	public static readonly DependencyProperty IsStylusCaptureWithinProperty;

	internal static readonly EventPrivateKey IsStylusCaptureWithinChangedKey;

	internal static readonly DependencyPropertyKey IsKeyboardFocusedPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.IsKeyboardFocused" /> dependency property.  </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.UIElement.IsKeyboardFocused" /> dependency property.</returns>
	public static readonly DependencyProperty IsKeyboardFocusedProperty;

	internal static readonly EventPrivateKey IsKeyboardFocusedChangedKey;

	internal static readonly DependencyPropertyKey AreAnyTouchesDirectlyOverPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.AreAnyTouchesDirectlyOver" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.UIElement.AreAnyTouchesDirectlyOver" /> dependency property.</returns>
	public static readonly DependencyProperty AreAnyTouchesDirectlyOverProperty;

	internal static readonly DependencyPropertyKey AreAnyTouchesOverPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.AreAnyTouchesOver" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.UIElement.AreAnyTouchesOver" /> dependency property.</returns>
	public static readonly DependencyProperty AreAnyTouchesOverProperty;

	internal static readonly DependencyPropertyKey AreAnyTouchesCapturedPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.AreAnyTouchesCaptured" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.UIElement.AreAnyTouchesCaptured" /> dependency property.</returns>
	public static readonly DependencyProperty AreAnyTouchesCapturedProperty;

	internal static readonly DependencyPropertyKey AreAnyTouchesCapturedWithinPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.AreAnyTouchesCapturedWithin" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.UIElement.AreAnyTouchesCapturedWithin" /> dependency property.</returns>
	public static readonly DependencyProperty AreAnyTouchesCapturedWithinProperty;

	private CoreFlags _flags;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.AllowDrop" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.UIElement.AllowDrop" /> dependency property.</returns>
	public static readonly DependencyProperty AllowDropProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.RenderTransform" /> dependency property.  </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.UIElement.RenderTransform" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty RenderTransformProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.RenderTransformOrigin" /> dependency property.  </summary>
	/// <returns>The <see cref="P:System.Windows.UIElement.RenderTransformOrigin" /> dependency property identifier.</returns>
	public static readonly DependencyProperty RenderTransformOriginProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.Opacity" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.UIElement.Opacity" /> dependency property.</returns>
	public static readonly DependencyProperty OpacityProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.OpacityMask" /> dependency property.  </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.UIElement.OpacityMask" /> dependency property.</returns>
	public static readonly DependencyProperty OpacityMaskProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.BitmapEffect" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.UIElement.BitmapEffect" /> dependency property identifier.</returns>
	public static readonly DependencyProperty BitmapEffectProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.Effect" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.UIElement.Effect" /> dependency property. </returns>
	public static readonly DependencyProperty EffectProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.BitmapEffectInput" /> dependency property.</summary>
	/// <returns>The <see cref="P:System.Windows.UIElement.BitmapEffectInput" /> dependency property identifier.</returns>
	public static readonly DependencyProperty BitmapEffectInputProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.CacheMode" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.UIElement.CacheMode" /> dependency property.</returns>
	public static readonly DependencyProperty CacheModeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.Uid" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.UIElement.Uid" /> dependency property.</returns>
	public static readonly DependencyProperty UidProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.Visibility" /> dependency property.  </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.UIElement.Visibility" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty VisibilityProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.ClipToBounds" /> dependency property.  </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.UIElement.ClipToBounds" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ClipToBoundsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.Clip" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.UIElement.Clip" /> dependency property.</returns>
	public static readonly DependencyProperty ClipProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.SnapsToDevicePixels" /> dependency property.  </summary>
	/// <returns>The <see cref="P:System.Windows.UIElement.SnapsToDevicePixels" /> dependency property identifier.</returns>
	public static readonly DependencyProperty SnapsToDevicePixelsProperty;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.GotFocus" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.GotFocus" /> routed event.</returns>
	public static readonly RoutedEvent GotFocusEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.LostFocus" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.LostFocus" /> routed event.</returns>
	public static readonly RoutedEvent LostFocusEvent;

	internal static readonly DependencyPropertyKey IsFocusedPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.IsFocused" /> dependency property.  </summary>
	/// <returns>The <see cref="P:System.Windows.UIElement.IsFocused" /> dependency property identifier.</returns>
	public static readonly DependencyProperty IsFocusedProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.IsEnabled" /> dependency property.  </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.UIElement.IsEnabled" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty IsEnabledProperty;

	internal static readonly EventPrivateKey IsEnabledChangedKey;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.IsHitTestVisible" />  dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.UIElement.IsHitTestVisible" /> dependency property identifier.</returns>
	public static readonly DependencyProperty IsHitTestVisibleProperty;

	internal static readonly EventPrivateKey IsHitTestVisibleChangedKey;

	private static PropertyMetadata _isVisibleMetadata;

	internal static readonly DependencyPropertyKey IsVisiblePropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.IsVisible" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.UIElement.IsVisible" /> dependency property identifier.</returns>
	public static readonly DependencyProperty IsVisibleProperty;

	internal static readonly EventPrivateKey IsVisibleChangedKey;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.Focusable" /> dependency property.  </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.UIElement.Focusable" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty FocusableProperty;

	internal static readonly EventPrivateKey FocusableChangedKey;

	/// <summary>Identifies the <see cref="P:System.Windows.UIElement.IsManipulationEnabled" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.UIElement.IsManipulationEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty IsManipulationEnabledProperty;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.ManipulationStarting" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.ManipulationStarting" /> routed event.</returns>
	public static readonly RoutedEvent ManipulationStartingEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.ManipulationStarted" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.ManipulationStarted" /> routed event.</returns>
	public static readonly RoutedEvent ManipulationStartedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.ManipulationDelta" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.ManipulationDelta" /> routed event.</returns>
	public static readonly RoutedEvent ManipulationDeltaEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.ManipulationInertiaStarting" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.ManipulationInertiaStarting" /> routed event.</returns>
	public static readonly RoutedEvent ManipulationInertiaStartingEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.ManipulationBoundaryFeedback" /> event.</summary>
	public static readonly RoutedEvent ManipulationBoundaryFeedbackEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.UIElement.ManipulationCompleted" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.UIElement.ManipulationCompleted" /> routed event.</returns>
	public static readonly RoutedEvent ManipulationCompletedEvent;

	private Rect _finalRect;

	private Size _desiredSize;

	private Size _previousAvailableSize;

	private IDrawingContent _drawingContent;

	internal ContextLayoutManager.LayoutQueue.Request MeasureRequest;

	internal ContextLayoutManager.LayoutQueue.Request ArrangeRequest;

	private int _persistId;

	internal static List<double> DpiScaleXValues;

	internal static List<double> DpiScaleYValues;

	internal static object DpiLock;

	private static double _dpiScaleX;

	private static double _dpiScaleY;

	private static bool _setDpi;

	internal static readonly UncommonField<EventHandlersStore> EventHandlersStoreField;

	internal static readonly UncommonField<InputBindingCollection> InputBindingCollectionField;

	internal static readonly UncommonField<CommandBindingCollection> CommandBindingCollectionField;

	private static readonly UncommonField<object> LayoutUpdatedListItemsField;

	private static readonly UncommonField<EventHandler> LayoutUpdatedHandlersField;

	private static readonly UncommonField<StylusPlugInCollection> StylusPlugInsField;

	private static readonly UncommonField<AutomationPeer> AutomationPeerField;

	private static readonly UncommonField<WeakReference<UIElement>> _positionAndSizeOfSetController;

	private static readonly UncommonField<bool> AutomationNotSupportedByDefaultField;

	internal SizeChangedInfo sizeChangedInfo;

	internal static readonly FocusWithinProperty FocusWithinProperty;

	internal static readonly MouseOverProperty MouseOverProperty;

	internal static readonly MouseCaptureWithinProperty MouseCaptureWithinProperty;

	internal static readonly StylusOverProperty StylusOverProperty;

	internal static readonly StylusCaptureWithinProperty StylusCaptureWithinProperty;

	internal static readonly TouchesOverProperty TouchesOverProperty;

	internal static readonly TouchesCapturedWithinProperty TouchesCapturedWithinProperty;

	private Size _size;

	internal const int MAX_ELEMENTS_IN_ROUTE = 4096;

	/// <summary>Gets a value indicating whether this element has any animated properties. </summary>
	/// <returns>true if this element has animations attached to any of its properties; otherwise, false.</returns>
	public bool HasAnimatedProperties
	{
		get
		{
			VerifyAccess();
			return base.IAnimatable_HasAnimatedProperties;
		}
	}

	/// <summary> Gets the collection of input bindings associated with this element. </summary>
	/// <returns>The collection of input bindings.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public InputBindingCollection InputBindings
	{
		get
		{
			VerifyAccess();
			InputBindingCollection inputBindingCollection = InputBindingCollectionField.GetValue(this);
			if (inputBindingCollection == null)
			{
				inputBindingCollection = new InputBindingCollection(this);
				InputBindingCollectionField.SetValue(this, inputBindingCollection);
			}
			return inputBindingCollection;
		}
	}

	internal InputBindingCollection InputBindingsInternal
	{
		get
		{
			VerifyAccess();
			return InputBindingCollectionField.GetValue(this);
		}
	}

	/// <summary>Gets a collection of <see cref="T:System.Windows.Input.CommandBinding" /> objects associated with this element. A <see cref="T:System.Windows.Input.CommandBinding" /> enables command handling for this element, and declares the linkage between a command, its events, and the handlers attached by this element.</summary>
	/// <returns>The collection of all <see cref="T:System.Windows.Input.CommandBinding" /> objects.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public CommandBindingCollection CommandBindings
	{
		get
		{
			VerifyAccess();
			CommandBindingCollection commandBindingCollection = CommandBindingCollectionField.GetValue(this);
			if (commandBindingCollection == null)
			{
				commandBindingCollection = new CommandBindingCollection();
				CommandBindingCollectionField.SetValue(this, commandBindingCollection);
			}
			return commandBindingCollection;
		}
	}

	internal CommandBindingCollection CommandBindingsInternal
	{
		get
		{
			VerifyAccess();
			return CommandBindingCollectionField.GetValue(this);
		}
	}

	internal EventHandlersStore EventHandlersStore
	{
		[FriendAccessAllowed]
		get
		{
			if (!ReadFlag(CoreFlags.ExistsEventHandlersStore))
			{
				return null;
			}
			return EventHandlersStoreField.GetValue(this);
		}
	}

	/// <summary>Gets or sets a value indicating whether this element can be used as the target of a drag-and-drop operation.  This is a dependency property.</summary>
	/// <returns>true if this element can be used as the target of a drag-and-drop operation; otherwise, false. The default value is false.     </returns>
	public bool AllowDrop
	{
		get
		{
			return (bool)GetValue(AllowDropProperty);
		}
		set
		{
			SetValue(AllowDropProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets a collection of all stylus plug-in (customization) objects associated with this element. </summary>
	/// <returns>The collection of stylus plug-ins, as a specialized collection.</returns>
	protected StylusPlugInCollection StylusPlugIns
	{
		get
		{
			StylusPlugInCollection stylusPlugInCollection = StylusPlugInsField.GetValue(this);
			if (stylusPlugInCollection == null)
			{
				stylusPlugInCollection = new StylusPlugInCollection(this);
				StylusPlugInsField.SetValue(this, stylusPlugInCollection);
			}
			return stylusPlugInCollection;
		}
	}

	/// <summary>Gets the size that this element computed during the measure pass of the layout process. </summary>
	/// <returns>The computed size, which becomes the desired size for the arrange pass.</returns>
	public Size DesiredSize
	{
		get
		{
			if (Visibility == Visibility.Collapsed)
			{
				return new Size(0.0, 0.0);
			}
			return _desiredSize;
		}
	}

	internal Size PreviousConstraint => _previousAvailableSize;

	/// <summary>Gets a value indicating whether the current size returned by layout measure is valid. </summary>
	/// <returns>true if the measure pass of layout returned a valid and current value; otherwise, false.</returns>
	public bool IsMeasureValid => !MeasureDirty;

	/// <summary>Gets a value indicating whether the computed size and position of child elements in this element's layout are valid. </summary>
	/// <returns>true if the size and position of layout are valid; otherwise, false.</returns>
	public bool IsArrangeValid => !ArrangeDirty;

	/// <summary>Gets (or sets, but see Remarks) the final render size of this element. </summary>
	/// <returns>The rendered size for this element.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Size RenderSize
	{
		get
		{
			if (Visibility == Visibility.Collapsed)
			{
				return default(Size);
			}
			return _size;
		}
		set
		{
			_size = value;
			InvalidateHitTestBounds();
		}
	}

	/// <summary>Gets or sets transform information that affects the rendering position of this element.  This is a dependency property.</summary>
	/// <returns>Describes the specifics of the desired render transform. The default is <see cref="P:System.Windows.Media.Transform.Identity" />.</returns>
	public Transform RenderTransform
	{
		get
		{
			return (Transform)GetValue(RenderTransformProperty);
		}
		set
		{
			SetValue(RenderTransformProperty, value);
		}
	}

	/// <summary>Gets or sets the center point of any possible render transform declared by <see cref="P:System.Windows.UIElement.RenderTransform" />, relative to the bounds of the element.  This is a dependency property.</summary>
	/// <returns>The value that declares the render transform. The default value is a <see cref="T:System.Windows.Point" /> with coordinates (0,0). </returns>
	public Point RenderTransformOrigin
	{
		get
		{
			return (Point)GetValue(RenderTransformOriginProperty);
		}
		set
		{
			SetValue(RenderTransformOriginProperty, value);
		}
	}

	/// <summary>Gets a value that indicates whether the position of the mouse pointer corresponds to hit test results, which take element compositing into account.  This is a dependency property.</summary>
	/// <returns>true if the mouse pointer is over the same element result as a hit test; otherwise, false. The default is false.</returns>
	public bool IsMouseDirectlyOver => IsMouseDirectlyOver_ComputeValue();

	/// <summary>Gets a value indicating whether the mouse pointer is located over this element (including child elements in the visual tree).  This is a dependency property.</summary>
	/// <returns>true if mouse pointer is over the element or its child elements; otherwise, false. The default is false.</returns>
	public bool IsMouseOver => ReadFlag(CoreFlags.IsMouseOverCache);

	/// <summary> Gets a value indicating whether the stylus cursor is located over this element (including visual child elements).  This is a dependency property.</summary>
	/// <returns>true if stylus cursor is over the element or its child elements; otherwise, false. The default is false.</returns>
	public bool IsStylusOver => ReadFlag(CoreFlags.IsStylusOverCache);

	/// <summary>Gets a value indicating whether keyboard focus is anywhere within the element or its visual tree child elements.  This is a dependency property.</summary>
	/// <returns>true if keyboard focus is on the element or its child elements; otherwise, false.</returns>
	public bool IsKeyboardFocusWithin => ReadFlag(CoreFlags.IsKeyboardFocusWithinCache);

	/// <summary>Gets a value indicating whether the mouse is captured to this element.  This is a dependency property.</summary>
	/// <returns>true if the element has mouse capture; otherwise, false. The default is false.</returns>
	public bool IsMouseCaptured => (bool)GetValue(IsMouseCapturedProperty);

	/// <summary>Gets a value that determines whether mouse capture is held by this element or by child elements in its visual tree. This is a dependency property.</summary>
	/// <returns>true if this element or a contained element has mouse capture; otherwise, false.</returns>
	public bool IsMouseCaptureWithin => ReadFlag(CoreFlags.IsMouseCaptureWithinCache);

	/// <summary>Gets a value that indicates whether the stylus position corresponds to hit test results, which take element compositing into account.  This is a dependency property.</summary>
	/// <returns>true if the stylus pointer is over the same element result as a hit test; otherwise, false. The default is false.</returns>
	public bool IsStylusDirectlyOver => IsStylusDirectlyOver_ComputeValue();

	/// <summary>Gets a value indicating whether the stylus is captured by this element.  This is a dependency property.</summary>
	/// <returns>true if the element has stylus capture; otherwise, false. The default is false.</returns>
	public bool IsStylusCaptured => (bool)GetValue(IsStylusCapturedProperty);

	/// <summary>Gets a value that determines whether stylus capture is held by this element, or an element within the element bounds and its visual tree. This is a dependency property.</summary>
	/// <returns>true if this element or a contained element has stylus capture; otherwise, false. The default is false.</returns>
	public bool IsStylusCaptureWithin => ReadFlag(CoreFlags.IsStylusCaptureWithinCache);

	/// <summary>Gets a value indicating whether this element has keyboard focus.  This is a dependency property.</summary>
	/// <returns>true if this element has keyboard focus; otherwise, false. The default is false.</returns>
	public bool IsKeyboardFocused => IsKeyboardFocused_ComputeValue();

	/// <summary>Gets a value indicating whether an input method system, such as an Input Method Editor (IME),  is enabled for processing the input to this element. </summary>
	/// <returns>true if an input method is active; otherwise, false. The default value of the underlying attached property is true; however, this will be influenced by the actual state of input methods at runtime.</returns>
	public bool IsInputMethodEnabled => (bool)GetValue(InputMethod.IsInputMethodEnabledProperty);

	/// <summary>Gets or sets the opacity factor applied to the entire <see cref="T:System.Windows.UIElement" /> when it is rendered in the user interface (UI).  This is a dependency property.</summary>
	/// <returns>The opacity factor. Default opacity is 1.0. Expected values are between 0.0 and 1.0.</returns>
	[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
	public double Opacity
	{
		get
		{
			return (double)GetValue(OpacityProperty);
		}
		set
		{
			SetValue(OpacityProperty, value);
		}
	}

	/// <summary>Gets or sets an opacity mask, as a <see cref="T:System.Windows.Media.Brush" /> implementation that is applied to any alpha-channel masking for the rendered content of this element.  This is a dependency property.</summary>
	/// <returns>The brush to use for opacity masking.</returns>
	public Brush OpacityMask
	{
		get
		{
			return (Brush)GetValue(OpacityMaskProperty);
		}
		set
		{
			SetValue(OpacityMaskProperty, value);
		}
	}

	/// <summary>Gets or sets a bitmap effect that applies directly to the rendered content for this element.  This is a dependency property.</summary>
	/// <returns>The bitmap effect to apply.</returns>
	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	public BitmapEffect BitmapEffect
	{
		get
		{
			return (BitmapEffect)GetValue(BitmapEffectProperty);
		}
		set
		{
			SetValue(BitmapEffectProperty, value);
		}
	}

	/// <summary>Gets or sets the bitmap effect to apply to the <see cref="T:System.Windows.UIElement" />. This is a dependency property.</summary>
	/// <returns>An <see cref="T:System.Windows.Media.Effects.Effect" /> that represents the bitmap effect. </returns>
	public Effect Effect
	{
		get
		{
			return (Effect)GetValue(EffectProperty);
		}
		set
		{
			SetValue(EffectProperty, value);
		}
	}

	/// <summary> Gets or sets an input source for the bitmap effect that applies directly to the rendered content for this element.  This is a dependency property.</summary>
	/// <returns>The source for bitmap effects.</returns>
	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	public BitmapEffectInput BitmapEffectInput
	{
		get
		{
			return (BitmapEffectInput)GetValue(BitmapEffectInputProperty);
		}
		set
		{
			SetValue(BitmapEffectInputProperty, value);
		}
	}

	/// <summary>Gets or sets a cached representation of the <see cref="T:System.Windows.UIElement" />. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.CacheMode" /> that holds a cached representation of the <see cref="T:System.Windows.UIElement" />.</returns>
	public CacheMode CacheMode
	{
		get
		{
			return (CacheMode)GetValue(CacheModeProperty);
		}
		set
		{
			SetValue(CacheModeProperty, value);
		}
	}

	/// <summary>Gets or sets the unique identifier (for localization) for this element. This is a dependency property. </summary>
	/// <returns>A string that is the unique identifier for this element.</returns>
	public string Uid
	{
		get
		{
			return (string)GetValue(UidProperty);
		}
		set
		{
			SetValue(UidProperty, value);
		}
	}

	/// <summary>Gets or sets the user interface (UI) visibility of this element.  This is a dependency property.</summary>
	/// <returns>A value of the enumeration. The default value is <see cref="F:System.Windows.Visibility.Visible" />.</returns>
	[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
	public Visibility Visibility
	{
		get
		{
			return VisibilityCache;
		}
		set
		{
			SetValue(VisibilityProperty, VisibilityBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets a value indicating whether to clip the content of this element (or content coming from the child elements of this element) to fit into the size of the containing element.   This is a dependency property.</summary>
	/// <returns>true if the content should be clipped; otherwise, false. The default value is false.</returns>
	public bool ClipToBounds
	{
		get
		{
			return ClipToBoundsCache;
		}
		set
		{
			SetValue(ClipToBoundsProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets the geometry used to define the outline of the contents of an element.  This is a dependency property.</summary>
	/// <returns>The geometry to be used for clipping area sizing. The default is a null <see cref="T:System.Windows.Media.Geometry" />.</returns>
	public Geometry Clip
	{
		get
		{
			return (Geometry)GetValue(ClipProperty);
		}
		set
		{
			SetValue(ClipProperty, value);
		}
	}

	/// <summary>Gets or sets a value that determines whether rendering for this element should use device-specific pixel settings during rendering.  This is a dependency property.</summary>
	/// <returns>true if the element should render in accordance to device pixels; otherwise, false. The default as declared on <see cref="T:System.Windows.UIElement" /> is false.</returns>
	public bool SnapsToDevicePixels
	{
		get
		{
			return SnapsToDevicePixelsCache;
		}
		set
		{
			SetValue(SnapsToDevicePixelsProperty, value);
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.UIElement" /> has focus.</summary>
	/// <returns>true if the <see cref="T:System.Windows.UIElement" /> has focus; otherwise, false.</returns>
	protected internal virtual bool HasEffectiveKeyboardFocus => IsKeyboardFocused;

	/// <summary>Gets a value that determines whether this element has logical focus.  This is a dependency property.</summary>
	/// <returns>true if this element has logical focus; otherwise, false.</returns>
	public bool IsFocused => (bool)GetValue(IsFocusedProperty);

	/// <summary>Gets or sets a value indicating whether this element is enabled in the user interface (UI).  This is a dependency property.</summary>
	/// <returns>true if the element is enabled; otherwise, false. The default value is true.</returns>
	public bool IsEnabled
	{
		get
		{
			return (bool)GetValue(IsEnabledProperty);
		}
		set
		{
			SetValue(IsEnabledProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets a value that becomes the return value of <see cref="P:System.Windows.UIElement.IsEnabled" /> in derived classes. </summary>
	/// <returns>true if the element is enabled; otherwise, false.</returns>
	protected virtual bool IsEnabledCore => true;

	/// <summary>Gets or sets a value that declares whether this element can possibly be returned as a hit test result from some portion of its rendered content. This is a dependency property.</summary>
	/// <returns>true if this element could be returned as a hit test result from at least one point; otherwise, false. The default value is true.</returns>
	public bool IsHitTestVisible
	{
		get
		{
			return (bool)GetValue(IsHitTestVisibleProperty);
		}
		set
		{
			SetValue(IsHitTestVisibleProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets a value indicating whether this element is visible in the user interface (UI).  This is a dependency property.</summary>
	/// <returns>true if the element is visible; otherwise, false.</returns>
	public bool IsVisible => ReadFlag(CoreFlags.IsVisibleCache);

	/// <summary>Gets or sets a value that indicates whether the element can receive focus.  This is a dependency property.</summary>
	/// <returns>true if the element is focusable; otherwise false. The default is false, but see Remarks.</returns>
	public bool Focusable
	{
		get
		{
			return (bool)GetValue(FocusableProperty);
		}
		set
		{
			SetValue(FocusableProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets a value that uniquely identifies this element. </summary>
	/// <returns>The unique identifier for this element.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Obsolete("PersistId is an obsolete property and may be removed in a future release.  The value of this property is not defined.")]
	public int PersistId => _persistId;

	internal Rect PreviousArrangeRect
	{
		[FriendAccessAllowed]
		get
		{
			return _finalRect;
		}
	}

	private Visibility VisibilityCache
	{
		get
		{
			if (CheckFlagsAnd(VisualFlags.VisibilityCache_Visible))
			{
				return Visibility.Visible;
			}
			if (CheckFlagsAnd(VisualFlags.VisibilityCache_TakesSpace))
			{
				return Visibility.Hidden;
			}
			return Visibility.Collapsed;
		}
		set
		{
			switch (value)
			{
			case Visibility.Visible:
				SetFlags(value: true, VisualFlags.VisibilityCache_Visible);
				SetFlags(value: false, VisualFlags.VisibilityCache_TakesSpace);
				break;
			case Visibility.Hidden:
				SetFlags(value: false, VisualFlags.VisibilityCache_Visible);
				SetFlags(value: true, VisualFlags.VisibilityCache_TakesSpace);
				break;
			case Visibility.Collapsed:
				SetFlags(value: false, VisualFlags.VisibilityCache_Visible);
				SetFlags(value: false, VisualFlags.VisibilityCache_TakesSpace);
				break;
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether manipulation events are enabled on this <see cref="T:System.Windows.UIElement" />.</summary>
	/// <returns>true if manipulation events are enabled on this <see cref="T:System.Windows.UIElement" />; otherwise, false. The default is false.</returns>
	[CustomCategory("Touch_Category")]
	public bool IsManipulationEnabled
	{
		get
		{
			return (bool)GetValue(IsManipulationEnabledProperty);
		}
		set
		{
			SetValue(IsManipulationEnabledProperty, value);
		}
	}

	/// <summary>Gets a value that indicates whether at least one touch is pressed over this element or any child elements in its visual tree. </summary>
	/// <returns>true if at least one touch is pressed over this element or any child elements in its visual tree; otherwise, false. </returns>
	public bool AreAnyTouchesOver => ReadFlag(CoreFlags.TouchesOverCache);

	/// <summary>Gets a value that indicates whether at least one touch is pressed over this element.</summary>
	/// <returns>true if at least one touch is pressed over this element; otherwise, false. </returns>
	public bool AreAnyTouchesDirectlyOver => (bool)GetValue(AreAnyTouchesDirectlyOverProperty);

	/// <summary>Gets a value that indicates whether at least one touch is captured to this element or to any child elements in its visual tree. </summary>
	/// <returns>true if at least one touch is captured to this element or any child elements in its visual tree; otherwise, false. </returns>
	public bool AreAnyTouchesCapturedWithin => ReadFlag(CoreFlags.TouchesCapturedWithinCache);

	/// <summary>Gets a value that indicates whether at least one touch is captured to this element. </summary>
	/// <returns>true if at least one touch is captured to this element; otherwise, false. </returns>
	public bool AreAnyTouchesCaptured => (bool)GetValue(AreAnyTouchesCapturedProperty);

	/// <summary>Gets all touch devices that are captured to this element.</summary>
	/// <returns>An enumeration of <see cref="T:System.Windows.Input.TouchDevice" /> objects that are captured to this element.</returns>
	public IEnumerable<TouchDevice> TouchesCaptured => TouchDevice.GetCapturedTouches(this, includeWithin: false);

	/// <summary>Gets all touch devices that are captured to this element or any child elements in its visual tree. </summary>
	/// <returns>An enumeration of <see cref="T:System.Windows.Input.TouchDevice" /> objects that are captured to this element or any child elements in its visual tree.</returns>
	public IEnumerable<TouchDevice> TouchesCapturedWithin => TouchDevice.GetCapturedTouches(this, includeWithin: true);

	/// <summary>Gets all touch devices that are over this element or any child elements in its visual tree.</summary>
	/// <returns>An enumeration of <see cref="T:System.Windows.Input.TouchDevice" /> objects that are over this element or any child elements in its visual tree.</returns>
	public IEnumerable<TouchDevice> TouchesOver => TouchDevice.GetTouchesOver(this, includeWithin: true);

	/// <summary>Gets all touch devices that are over this element.</summary>
	/// <returns>An enumeration of <see cref="T:System.Windows.Input.TouchDevice" /> objects that are over this element.</returns>
	public IEnumerable<TouchDevice> TouchesDirectlyOver => TouchDevice.GetTouchesOver(this, includeWithin: false);

	internal UIElement PositionAndSizeOfSetController
	{
		get
		{
			UIElement target = null;
			_positionAndSizeOfSetController.GetValue(this)?.TryGetTarget(out target);
			return target;
		}
		set
		{
			if (value != null)
			{
				_positionAndSizeOfSetController.SetValue(this, new WeakReference<UIElement>(value));
			}
			else
			{
				_positionAndSizeOfSetController.ClearValue(this);
			}
		}
	}

	internal bool HasAutomationPeer
	{
		get
		{
			return ReadFlag(CoreFlags.HasAutomationPeer);
		}
		set
		{
			WriteFlag(CoreFlags.HasAutomationPeer, value);
		}
	}

	private bool RenderingInvalidated
	{
		get
		{
			return ReadFlag(CoreFlags.RenderingInvalidated);
		}
		set
		{
			WriteFlag(CoreFlags.RenderingInvalidated, value);
		}
	}

	internal bool SnapsToDevicePixelsCache
	{
		get
		{
			return ReadFlag(CoreFlags.SnapsToDevicePixelsCache);
		}
		set
		{
			WriteFlag(CoreFlags.SnapsToDevicePixelsCache, value);
		}
	}

	internal bool ClipToBoundsCache
	{
		get
		{
			return ReadFlag(CoreFlags.ClipToBoundsCache);
		}
		set
		{
			WriteFlag(CoreFlags.ClipToBoundsCache, value);
		}
	}

	internal bool MeasureDirty
	{
		get
		{
			return ReadFlag(CoreFlags.MeasureDirty);
		}
		set
		{
			WriteFlag(CoreFlags.MeasureDirty, value);
		}
	}

	internal bool ArrangeDirty
	{
		get
		{
			return ReadFlag(CoreFlags.ArrangeDirty);
		}
		set
		{
			WriteFlag(CoreFlags.ArrangeDirty, value);
		}
	}

	internal bool MeasureInProgress
	{
		get
		{
			return ReadFlag(CoreFlags.MeasureInProgress);
		}
		set
		{
			WriteFlag(CoreFlags.MeasureInProgress, value);
		}
	}

	internal bool ArrangeInProgress
	{
		get
		{
			return ReadFlag(CoreFlags.ArrangeInProgress);
		}
		set
		{
			WriteFlag(CoreFlags.ArrangeInProgress, value);
		}
	}

	internal bool NeverMeasured
	{
		get
		{
			return ReadFlag(CoreFlags.NeverMeasured);
		}
		set
		{
			WriteFlag(CoreFlags.NeverMeasured, value);
		}
	}

	internal bool NeverArranged
	{
		get
		{
			return ReadFlag(CoreFlags.NeverArranged);
		}
		set
		{
			WriteFlag(CoreFlags.NeverArranged, value);
		}
	}

	internal bool MeasureDuringArrange
	{
		get
		{
			return ReadFlag(CoreFlags.MeasureDuringArrange);
		}
		set
		{
			WriteFlag(CoreFlags.MeasureDuringArrange, value);
		}
	}

	internal bool AreTransformsClean
	{
		get
		{
			return ReadFlag(CoreFlags.AreTransformsClean);
		}
		set
		{
			WriteFlag(CoreFlags.AreTransformsClean, value);
		}
	}

	/// <summary>Occurs when any mouse button is pressed while the pointer is over this element.</summary>
	public event MouseButtonEventHandler PreviewMouseDown
	{
		add
		{
			AddHandler(Mouse.PreviewMouseDownEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Mouse.PreviewMouseDownEvent, value);
		}
	}

	/// <summary>Occurs when any mouse button is pressed while the pointer is over this element.</summary>
	public event MouseButtonEventHandler MouseDown
	{
		add
		{
			AddHandler(Mouse.MouseDownEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Mouse.MouseDownEvent, value);
		}
	}

	/// <summary>Occurs when any mouse button is released while the mouse pointer is over this element.</summary>
	public event MouseButtonEventHandler PreviewMouseUp
	{
		add
		{
			AddHandler(Mouse.PreviewMouseUpEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Mouse.PreviewMouseUpEvent, value);
		}
	}

	/// <summary>Occurs when any mouse button is released over this element.</summary>
	public event MouseButtonEventHandler MouseUp
	{
		add
		{
			AddHandler(Mouse.MouseUpEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Mouse.MouseUpEvent, value);
		}
	}

	/// <summary>Occurs when the left mouse button is pressed while the mouse pointer is over this element. </summary>
	public event MouseButtonEventHandler PreviewMouseLeftButtonDown
	{
		add
		{
			AddHandler(PreviewMouseLeftButtonDownEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(PreviewMouseLeftButtonDownEvent, value);
		}
	}

	/// <summary>Occurs when the left mouse button is pressed while the mouse pointer is over this element. </summary>
	public event MouseButtonEventHandler MouseLeftButtonDown
	{
		add
		{
			AddHandler(MouseLeftButtonDownEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(MouseLeftButtonDownEvent, value);
		}
	}

	/// <summary>Occurs when the left mouse button is released while the mouse pointer is over this element. </summary>
	public event MouseButtonEventHandler PreviewMouseLeftButtonUp
	{
		add
		{
			AddHandler(PreviewMouseLeftButtonUpEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(PreviewMouseLeftButtonUpEvent, value);
		}
	}

	/// <summary>Occurs when the left mouse button is released while the mouse pointer is over this element. </summary>
	public event MouseButtonEventHandler MouseLeftButtonUp
	{
		add
		{
			AddHandler(MouseLeftButtonUpEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(MouseLeftButtonUpEvent, value);
		}
	}

	/// <summary>Occurs when the right mouse button is pressed while the mouse pointer is over this element. </summary>
	public event MouseButtonEventHandler PreviewMouseRightButtonDown
	{
		add
		{
			AddHandler(PreviewMouseRightButtonDownEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(PreviewMouseRightButtonDownEvent, value);
		}
	}

	/// <summary>Occurs when the right mouse button is pressed while the mouse pointer is over this element. </summary>
	public event MouseButtonEventHandler MouseRightButtonDown
	{
		add
		{
			AddHandler(MouseRightButtonDownEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(MouseRightButtonDownEvent, value);
		}
	}

	/// <summary>Occurs when the right mouse button is released while the mouse pointer is over this element. </summary>
	public event MouseButtonEventHandler PreviewMouseRightButtonUp
	{
		add
		{
			AddHandler(PreviewMouseRightButtonUpEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(PreviewMouseRightButtonUpEvent, value);
		}
	}

	/// <summary>Occurs when the right mouse button is released while the mouse pointer is over this element. </summary>
	public event MouseButtonEventHandler MouseRightButtonUp
	{
		add
		{
			AddHandler(MouseRightButtonUpEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(MouseRightButtonUpEvent, value);
		}
	}

	/// <summary>Occurs when the mouse pointer moves while the mouse pointer is over this element. </summary>
	public event MouseEventHandler PreviewMouseMove
	{
		add
		{
			AddHandler(Mouse.PreviewMouseMoveEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Mouse.PreviewMouseMoveEvent, value);
		}
	}

	/// <summary>Occurs when the mouse pointer moves while over this element. </summary>
	public event MouseEventHandler MouseMove
	{
		add
		{
			AddHandler(Mouse.MouseMoveEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Mouse.MouseMoveEvent, value);
		}
	}

	/// <summary>Occurs when the user rotates the mouse wheel while the mouse pointer is over this element. </summary>
	public event MouseWheelEventHandler PreviewMouseWheel
	{
		add
		{
			AddHandler(Mouse.PreviewMouseWheelEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Mouse.PreviewMouseWheelEvent, value);
		}
	}

	/// <summary>Occurs when the user rotates the mouse wheel while the mouse pointer is over this element. </summary>
	public event MouseWheelEventHandler MouseWheel
	{
		add
		{
			AddHandler(Mouse.MouseWheelEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Mouse.MouseWheelEvent, value);
		}
	}

	/// <summary>Occurs when the mouse pointer enters the bounds of this element. </summary>
	public event MouseEventHandler MouseEnter
	{
		add
		{
			AddHandler(Mouse.MouseEnterEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Mouse.MouseEnterEvent, value);
		}
	}

	/// <summary>Occurs when the mouse pointer leaves the bounds of this element. </summary>
	public event MouseEventHandler MouseLeave
	{
		add
		{
			AddHandler(Mouse.MouseLeaveEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Mouse.MouseLeaveEvent, value);
		}
	}

	/// <summary>Occurs when this element captures the mouse. </summary>
	public event MouseEventHandler GotMouseCapture
	{
		add
		{
			AddHandler(Mouse.GotMouseCaptureEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Mouse.GotMouseCaptureEvent, value);
		}
	}

	/// <summary>Occurs when this element loses mouse capture. </summary>
	public event MouseEventHandler LostMouseCapture
	{
		add
		{
			AddHandler(Mouse.LostMouseCaptureEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Mouse.LostMouseCaptureEvent, value);
		}
	}

	/// <summary>Occurs when the cursor is requested to display. This event is raised on an element each time that the mouse pointer moves to a new location, which means the cursor object might need to be changed based on its new position. </summary>
	public event QueryCursorEventHandler QueryCursor
	{
		add
		{
			AddHandler(Mouse.QueryCursorEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Mouse.QueryCursorEvent, value);
		}
	}

	/// <summary>Occurs when the stylus touches the digitizer while it is over this element. </summary>
	public event StylusDownEventHandler PreviewStylusDown
	{
		add
		{
			AddHandler(Stylus.PreviewStylusDownEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Stylus.PreviewStylusDownEvent, value);
		}
	}

	/// <summary>Occurs when the stylus touches the digitizer while the stylus is over this element. </summary>
	public event StylusDownEventHandler StylusDown
	{
		add
		{
			AddHandler(Stylus.StylusDownEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Stylus.StylusDownEvent, value);
		}
	}

	/// <summary>Occurs when the user raises the stylus off the digitizer while the stylus is over this element. </summary>
	public event StylusEventHandler PreviewStylusUp
	{
		add
		{
			AddHandler(Stylus.PreviewStylusUpEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Stylus.PreviewStylusUpEvent, value);
		}
	}

	/// <summary>Occurs when the user raises the stylus off the digitizer while it is over this element. </summary>
	public event StylusEventHandler StylusUp
	{
		add
		{
			AddHandler(Stylus.StylusUpEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Stylus.StylusUpEvent, value);
		}
	}

	/// <summary>Occurs when the stylus moves while over the element. The stylus must move while being detected by the digitizer to raise this event, otherwise, <see cref="E:System.Windows.UIElement.PreviewStylusInAirMove" /> is raised instead.</summary>
	public event StylusEventHandler PreviewStylusMove
	{
		add
		{
			AddHandler(Stylus.PreviewStylusMoveEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Stylus.PreviewStylusMoveEvent, value);
		}
	}

	/// <summary>Occurs when the stylus moves over this element. The stylus must move while on the digitizer to raise this event. Otherwise, <see cref="E:System.Windows.UIElement.StylusInAirMove" /> is raised instead.</summary>
	public event StylusEventHandler StylusMove
	{
		add
		{
			AddHandler(Stylus.StylusMoveEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Stylus.StylusMoveEvent, value);
		}
	}

	/// <summary>Occurs when the stylus moves over an element without actually touching the digitizer. </summary>
	public event StylusEventHandler PreviewStylusInAirMove
	{
		add
		{
			AddHandler(Stylus.PreviewStylusInAirMoveEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Stylus.PreviewStylusInAirMoveEvent, value);
		}
	}

	/// <summary>Occurs when the stylus moves over an element without actually touching the digitizer. </summary>
	public event StylusEventHandler StylusInAirMove
	{
		add
		{
			AddHandler(Stylus.StylusInAirMoveEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Stylus.StylusInAirMoveEvent, value);
		}
	}

	/// <summary>Occurs when the stylus enters the bounds of this element. </summary>
	public event StylusEventHandler StylusEnter
	{
		add
		{
			AddHandler(Stylus.StylusEnterEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Stylus.StylusEnterEvent, value);
		}
	}

	/// <summary>Occurs when the stylus leaves the bounds of the element. </summary>
	public event StylusEventHandler StylusLeave
	{
		add
		{
			AddHandler(Stylus.StylusLeaveEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Stylus.StylusLeaveEvent, value);
		}
	}

	/// <summary>Occurs when the stylus is close enough to the digitizer to be detected, while over this element. </summary>
	public event StylusEventHandler PreviewStylusInRange
	{
		add
		{
			AddHandler(Stylus.PreviewStylusInRangeEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Stylus.PreviewStylusInRangeEvent, value);
		}
	}

	/// <summary>Occurs when the stylus is close enough to the digitizer to be detected, while over this element. </summary>
	public event StylusEventHandler StylusInRange
	{
		add
		{
			AddHandler(Stylus.StylusInRangeEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Stylus.StylusInRangeEvent, value);
		}
	}

	/// <summary>Occurs when the stylus is too far from the digitizer to be detected. </summary>
	public event StylusEventHandler PreviewStylusOutOfRange
	{
		add
		{
			AddHandler(Stylus.PreviewStylusOutOfRangeEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Stylus.PreviewStylusOutOfRangeEvent, value);
		}
	}

	/// <summary>Occurs when the stylus is too far from the digitizer to be detected, while over this element. </summary>
	public event StylusEventHandler StylusOutOfRange
	{
		add
		{
			AddHandler(Stylus.StylusOutOfRangeEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Stylus.StylusOutOfRangeEvent, value);
		}
	}

	/// <summary>Occurs when a user performs one of several stylus gestures.</summary>
	public event StylusSystemGestureEventHandler PreviewStylusSystemGesture
	{
		add
		{
			AddHandler(Stylus.PreviewStylusSystemGestureEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Stylus.PreviewStylusSystemGestureEvent, value);
		}
	}

	/// <summary>Occurs when a user performs one of several stylus gestures.</summary>
	public event StylusSystemGestureEventHandler StylusSystemGesture
	{
		add
		{
			AddHandler(Stylus.StylusSystemGestureEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Stylus.StylusSystemGestureEvent, value);
		}
	}

	/// <summary>Occurs when this element captures the stylus. </summary>
	public event StylusEventHandler GotStylusCapture
	{
		add
		{
			AddHandler(Stylus.GotStylusCaptureEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Stylus.GotStylusCaptureEvent, value);
		}
	}

	/// <summary>Occurs when this element loses stylus capture. </summary>
	public event StylusEventHandler LostStylusCapture
	{
		add
		{
			AddHandler(Stylus.LostStylusCaptureEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Stylus.LostStylusCaptureEvent, value);
		}
	}

	/// <summary>Occurs when the stylus button is pressed while the pointer is over this element.</summary>
	public event StylusButtonEventHandler StylusButtonDown
	{
		add
		{
			AddHandler(Stylus.StylusButtonDownEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Stylus.StylusButtonDownEvent, value);
		}
	}

	/// <summary>Occurs when the stylus button is released while the pointer is over this element.</summary>
	public event StylusButtonEventHandler StylusButtonUp
	{
		add
		{
			AddHandler(Stylus.StylusButtonUpEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Stylus.StylusButtonUpEvent, value);
		}
	}

	/// <summary>Occurs when the stylus button is pressed while the pointer is over this element.</summary>
	public event StylusButtonEventHandler PreviewStylusButtonDown
	{
		add
		{
			AddHandler(Stylus.PreviewStylusButtonDownEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Stylus.PreviewStylusButtonDownEvent, value);
		}
	}

	/// <summary>Occurs when the stylus button is released while the pointer is over this element.</summary>
	public event StylusButtonEventHandler PreviewStylusButtonUp
	{
		add
		{
			AddHandler(Stylus.PreviewStylusButtonUpEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Stylus.PreviewStylusButtonUpEvent, value);
		}
	}

	/// <summary>Occurs when a key is pressed while focus is on this element. </summary>
	public event KeyEventHandler PreviewKeyDown
	{
		add
		{
			AddHandler(Keyboard.PreviewKeyDownEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Keyboard.PreviewKeyDownEvent, value);
		}
	}

	/// <summary>Occurs when a key is pressed while focus is on this element. </summary>
	public event KeyEventHandler KeyDown
	{
		add
		{
			AddHandler(Keyboard.KeyDownEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Keyboard.KeyDownEvent, value);
		}
	}

	/// <summary>Occurs when a key is released while focus is on this element. </summary>
	public event KeyEventHandler PreviewKeyUp
	{
		add
		{
			AddHandler(Keyboard.PreviewKeyUpEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Keyboard.PreviewKeyUpEvent, value);
		}
	}

	/// <summary>Occurs when a key is released while focus is on this element. </summary>
	public event KeyEventHandler KeyUp
	{
		add
		{
			AddHandler(Keyboard.KeyUpEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Keyboard.KeyUpEvent, value);
		}
	}

	/// <summary>Occurs when the keyboard is focused on this element. </summary>
	public event KeyboardFocusChangedEventHandler PreviewGotKeyboardFocus
	{
		add
		{
			AddHandler(Keyboard.PreviewGotKeyboardFocusEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Keyboard.PreviewGotKeyboardFocusEvent, value);
		}
	}

	/// <summary>Occurs when the keyboard is focused on this element. </summary>
	public event KeyboardFocusChangedEventHandler GotKeyboardFocus
	{
		add
		{
			AddHandler(Keyboard.GotKeyboardFocusEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Keyboard.GotKeyboardFocusEvent, value);
		}
	}

	/// <summary>Occurs when the keyboard is no longer focused on this element. </summary>
	public event KeyboardFocusChangedEventHandler PreviewLostKeyboardFocus
	{
		add
		{
			AddHandler(Keyboard.PreviewLostKeyboardFocusEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Keyboard.PreviewLostKeyboardFocusEvent, value);
		}
	}

	/// <summary>Occurs when the keyboard is no longer focused on this element,. </summary>
	public event KeyboardFocusChangedEventHandler LostKeyboardFocus
	{
		add
		{
			AddHandler(Keyboard.LostKeyboardFocusEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Keyboard.LostKeyboardFocusEvent, value);
		}
	}

	/// <summary>Occurs when this element gets text in a device-independent manner. </summary>
	public event TextCompositionEventHandler PreviewTextInput
	{
		add
		{
			AddHandler(TextCompositionManager.PreviewTextInputEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(TextCompositionManager.PreviewTextInputEvent, value);
		}
	}

	/// <summary>Occurs when this element gets text in a device-independent manner. </summary>
	public event TextCompositionEventHandler TextInput
	{
		add
		{
			AddHandler(TextCompositionManager.TextInputEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(TextCompositionManager.TextInputEvent, value);
		}
	}

	/// <summary>Occurs when there is a change in the keyboard or mouse button state during a drag-and-drop operation. </summary>
	public event QueryContinueDragEventHandler PreviewQueryContinueDrag
	{
		add
		{
			AddHandler(DragDrop.PreviewQueryContinueDragEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(DragDrop.PreviewQueryContinueDragEvent, value);
		}
	}

	/// <summary>Occurs when there is a change in the keyboard or mouse button state during a drag-and-drop operation. </summary>
	public event QueryContinueDragEventHandler QueryContinueDrag
	{
		add
		{
			AddHandler(DragDrop.QueryContinueDragEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(DragDrop.QueryContinueDragEvent, value);
		}
	}

	/// <summary>Occurs when a drag-and-drop operation is started. </summary>
	public event GiveFeedbackEventHandler PreviewGiveFeedback
	{
		add
		{
			AddHandler(DragDrop.PreviewGiveFeedbackEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(DragDrop.PreviewGiveFeedbackEvent, value);
		}
	}

	/// <summary>Occurs when the input system reports an underlying drag-and-drop event that involves this element. </summary>
	public event GiveFeedbackEventHandler GiveFeedback
	{
		add
		{
			AddHandler(DragDrop.GiveFeedbackEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(DragDrop.GiveFeedbackEvent, value);
		}
	}

	/// <summary>Occurs when the input system reports an underlying drag event with this element as the drag target. </summary>
	public event DragEventHandler PreviewDragEnter
	{
		add
		{
			AddHandler(DragDrop.PreviewDragEnterEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(DragDrop.PreviewDragEnterEvent, value);
		}
	}

	/// <summary>Occurs when the input system reports an underlying drag event with this element as the drag target. </summary>
	public event DragEventHandler DragEnter
	{
		add
		{
			AddHandler(DragDrop.DragEnterEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(DragDrop.DragEnterEvent, value);
		}
	}

	/// <summary>Occurs when the input system reports an underlying drag event with this element as the potential drop target. </summary>
	public event DragEventHandler PreviewDragOver
	{
		add
		{
			AddHandler(DragDrop.PreviewDragOverEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(DragDrop.PreviewDragOverEvent, value);
		}
	}

	/// <summary>Occurs when the input system reports an underlying drag event with this element as the potential drop target. </summary>
	public event DragEventHandler DragOver
	{
		add
		{
			AddHandler(DragDrop.DragOverEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(DragDrop.DragOverEvent, value);
		}
	}

	/// <summary>Occurs when the input system reports an underlying drag event with this element as the drag origin. </summary>
	public event DragEventHandler PreviewDragLeave
	{
		add
		{
			AddHandler(DragDrop.PreviewDragLeaveEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(DragDrop.PreviewDragLeaveEvent, value);
		}
	}

	/// <summary>Occurs when the input system reports an underlying drag event with this element as the drag origin. </summary>
	public event DragEventHandler DragLeave
	{
		add
		{
			AddHandler(DragDrop.DragLeaveEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(DragDrop.DragLeaveEvent, value);
		}
	}

	/// <summary>Occurs when the input system reports an underlying drop event with this element as the drop target. </summary>
	public event DragEventHandler PreviewDrop
	{
		add
		{
			AddHandler(DragDrop.PreviewDropEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(DragDrop.PreviewDropEvent, value);
		}
	}

	/// <summary>Occurs when the input system reports an underlying drop event with this element as the drop target. </summary>
	public event DragEventHandler Drop
	{
		add
		{
			AddHandler(DragDrop.DropEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(DragDrop.DropEvent, value);
		}
	}

	/// <summary>Occurs when a finger touches the screen while the finger is over this element. </summary>
	[CustomCategory("Touch_Category")]
	public event EventHandler<TouchEventArgs> PreviewTouchDown
	{
		add
		{
			AddHandler(Touch.PreviewTouchDownEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Touch.PreviewTouchDownEvent, value);
		}
	}

	/// <summary>Occurs when a finger touches the screen while the finger is over this element. </summary>
	[CustomCategory("Touch_Category")]
	public event EventHandler<TouchEventArgs> TouchDown
	{
		add
		{
			AddHandler(Touch.TouchDownEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Touch.TouchDownEvent, value);
		}
	}

	/// <summary>Occurs when a finger moves on the screen while the finger is over this element.</summary>
	[CustomCategory("Touch_Category")]
	public event EventHandler<TouchEventArgs> PreviewTouchMove
	{
		add
		{
			AddHandler(Touch.PreviewTouchMoveEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Touch.PreviewTouchMoveEvent, value);
		}
	}

	/// <summary>Occurs when a finger moves on the screen while the finger is over this element.</summary>
	[CustomCategory("Touch_Category")]
	public event EventHandler<TouchEventArgs> TouchMove
	{
		add
		{
			AddHandler(Touch.TouchMoveEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Touch.TouchMoveEvent, value);
		}
	}

	/// <summary>Occurs when a finger is raised off of the screen while the finger is over this element. </summary>
	[CustomCategory("Touch_Category")]
	public event EventHandler<TouchEventArgs> PreviewTouchUp
	{
		add
		{
			AddHandler(Touch.PreviewTouchUpEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Touch.PreviewTouchUpEvent, value);
		}
	}

	/// <summary>Occurs when a finger is raised off of the screen while the finger is over this element. </summary>
	[CustomCategory("Touch_Category")]
	public event EventHandler<TouchEventArgs> TouchUp
	{
		add
		{
			AddHandler(Touch.TouchUpEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Touch.TouchUpEvent, value);
		}
	}

	/// <summary>Occurs when a touch is captured to this element. </summary>
	[CustomCategory("Touch_Category")]
	public event EventHandler<TouchEventArgs> GotTouchCapture
	{
		add
		{
			AddHandler(Touch.GotTouchCaptureEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Touch.GotTouchCaptureEvent, value);
		}
	}

	/// <summary>Occurs when this element loses a touch capture. </summary>
	[CustomCategory("Touch_Category")]
	public event EventHandler<TouchEventArgs> LostTouchCapture
	{
		add
		{
			AddHandler(Touch.LostTouchCaptureEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Touch.LostTouchCaptureEvent, value);
		}
	}

	/// <summary>Occurs when a touch moves from outside to inside the bounds of this element. </summary>
	[CustomCategory("Touch_Category")]
	public event EventHandler<TouchEventArgs> TouchEnter
	{
		add
		{
			AddHandler(Touch.TouchEnterEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Touch.TouchEnterEvent, value);
		}
	}

	/// <summary>Occurs when a touch moves from inside to outside the bounds of this element. </summary>
	[CustomCategory("Touch_Category")]
	public event EventHandler<TouchEventArgs> TouchLeave
	{
		add
		{
			AddHandler(Touch.TouchLeaveEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(Touch.TouchLeaveEvent, value);
		}
	}

	/// <summary>Occurs when the value of the <see cref="P:System.Windows.UIElement.IsMouseDirectlyOver" /> property changes on this element. </summary>
	public event DependencyPropertyChangedEventHandler IsMouseDirectlyOverChanged
	{
		add
		{
			EventHandlersStoreAdd(IsMouseDirectlyOverChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(IsMouseDirectlyOverChangedKey, value);
		}
	}

	/// <summary>Occurs when the value of the <see cref="E:System.Windows.UIElement.IsKeyboardFocusWithinChanged" /> property changes on this element. </summary>
	public event DependencyPropertyChangedEventHandler IsKeyboardFocusWithinChanged
	{
		add
		{
			EventHandlersStoreAdd(IsKeyboardFocusWithinChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(IsKeyboardFocusWithinChangedKey, value);
		}
	}

	/// <summary>Occurs when the value of the <see cref="P:System.Windows.UIElement.IsMouseCaptured" /> property changes on this element. </summary>
	public event DependencyPropertyChangedEventHandler IsMouseCapturedChanged
	{
		add
		{
			EventHandlersStoreAdd(IsMouseCapturedChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(IsMouseCapturedChangedKey, value);
		}
	}

	/// <summary>Occurs when the value of the <see cref="F:System.Windows.UIElement.IsMouseCaptureWithinProperty" /> changes on this element.</summary>
	public event DependencyPropertyChangedEventHandler IsMouseCaptureWithinChanged
	{
		add
		{
			EventHandlersStoreAdd(IsMouseCaptureWithinChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(IsMouseCaptureWithinChangedKey, value);
		}
	}

	/// <summary>Occurs when the value of the <see cref="P:System.Windows.UIElement.IsStylusDirectlyOver" /> property changes on this element. </summary>
	public event DependencyPropertyChangedEventHandler IsStylusDirectlyOverChanged
	{
		add
		{
			EventHandlersStoreAdd(IsStylusDirectlyOverChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(IsStylusDirectlyOverChangedKey, value);
		}
	}

	/// <summary>Occurs when the value of the <see cref="P:System.Windows.UIElement.IsStylusCaptured" /> property changes on this element. </summary>
	public event DependencyPropertyChangedEventHandler IsStylusCapturedChanged
	{
		add
		{
			EventHandlersStoreAdd(IsStylusCapturedChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(IsStylusCapturedChangedKey, value);
		}
	}

	/// <summary>Occurs when the value of the <see cref="P:System.Windows.UIElement.IsStylusCaptureWithin" /> property changes on this element. </summary>
	public event DependencyPropertyChangedEventHandler IsStylusCaptureWithinChanged
	{
		add
		{
			EventHandlersStoreAdd(IsStylusCaptureWithinChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(IsStylusCaptureWithinChangedKey, value);
		}
	}

	/// <summary>Occurs when the value of the <see cref="P:System.Windows.UIElement.IsKeyboardFocused" /> property changes on this element. </summary>
	public event DependencyPropertyChangedEventHandler IsKeyboardFocusedChanged
	{
		add
		{
			EventHandlersStoreAdd(IsKeyboardFocusedChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(IsKeyboardFocusedChangedKey, value);
		}
	}

	/// <summary>Occurs when the layout of the various visual elements associated with the current <see cref="T:System.Windows.Threading.Dispatcher" /> changes. </summary>
	public event EventHandler LayoutUpdated
	{
		add
		{
			LayoutEventList.ListItem layoutUpdatedHandler = getLayoutUpdatedHandler(value);
			if (layoutUpdatedHandler == null)
			{
				layoutUpdatedHandler = ContextLayoutManager.From(base.Dispatcher).LayoutEvents.Add(value);
				addLayoutUpdatedHandler(value, layoutUpdatedHandler);
			}
		}
		remove
		{
			LayoutEventList.ListItem layoutUpdatedHandler = getLayoutUpdatedHandler(value);
			if (layoutUpdatedHandler != null)
			{
				removeLayoutUpdatedHandler(value);
				ContextLayoutManager.From(base.Dispatcher).LayoutEvents.Remove(layoutUpdatedHandler);
			}
		}
	}

	/// <summary>Occurs when this element gets logical focus. </summary>
	public event RoutedEventHandler GotFocus
	{
		add
		{
			AddHandler(GotFocusEvent, value);
		}
		remove
		{
			RemoveHandler(GotFocusEvent, value);
		}
	}

	/// <summary>Occurs when this element loses logical focus. </summary>
	public event RoutedEventHandler LostFocus
	{
		add
		{
			AddHandler(LostFocusEvent, value);
		}
		remove
		{
			RemoveHandler(LostFocusEvent, value);
		}
	}

	/// <summary>Occurs when the value of the <see cref="P:System.Windows.UIElement.IsEnabled" /> property on this element changes. </summary>
	public event DependencyPropertyChangedEventHandler IsEnabledChanged
	{
		add
		{
			EventHandlersStoreAdd(IsEnabledChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(IsEnabledChangedKey, value);
		}
	}

	/// <summary>Occurs when the value of the <see cref="P:System.Windows.UIElement.IsHitTestVisible" /> dependency property changes on this element.</summary>
	public event DependencyPropertyChangedEventHandler IsHitTestVisibleChanged
	{
		add
		{
			EventHandlersStoreAdd(IsHitTestVisibleChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(IsHitTestVisibleChangedKey, value);
		}
	}

	/// <summary>Occurs when the value of the <see cref="P:System.Windows.UIElement.IsVisible" /> property changes on this element. </summary>
	public event DependencyPropertyChangedEventHandler IsVisibleChanged
	{
		add
		{
			EventHandlersStoreAdd(IsVisibleChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(IsVisibleChangedKey, value);
		}
	}

	/// <summary>Occurs when the value of the <see cref="P:System.Windows.UIElement.Focusable" /> property changes.</summary>
	public event DependencyPropertyChangedEventHandler FocusableChanged
	{
		add
		{
			EventHandlersStoreAdd(FocusableChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(FocusableChangedKey, value);
		}
	}

	/// <summary>Occurs when the manipulation processor is first created. </summary>
	[CustomCategory("Touch_Category")]
	public event EventHandler<ManipulationStartingEventArgs> ManipulationStarting
	{
		add
		{
			AddHandler(ManipulationStartingEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(ManipulationStartingEvent, value);
		}
	}

	/// <summary>Occurs when an input device begins a manipulation on the <see cref="T:System.Windows.UIElement" /> object.</summary>
	[CustomCategory("Touch_Category")]
	public event EventHandler<ManipulationStartedEventArgs> ManipulationStarted
	{
		add
		{
			AddHandler(ManipulationStartedEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(ManipulationStartedEvent, value);
		}
	}

	/// <summary>Occurs when the input device changes position during a manipulation. </summary>
	[CustomCategory("Touch_Category")]
	public event EventHandler<ManipulationDeltaEventArgs> ManipulationDelta
	{
		add
		{
			AddHandler(ManipulationDeltaEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(ManipulationDeltaEvent, value);
		}
	}

	/// <summary>Occurs when the input device loses contact with the <see cref="T:System.Windows.UIElement" /> object during a manipulation and inertia begins.</summary>
	[CustomCategory("Touch_Category")]
	public event EventHandler<ManipulationInertiaStartingEventArgs> ManipulationInertiaStarting
	{
		add
		{
			AddHandler(ManipulationInertiaStartingEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(ManipulationInertiaStartingEvent, value);
		}
	}

	/// <summary>Occurs when the manipulation encounters a boundary.</summary>
	[CustomCategory("Touch_Category")]
	public event EventHandler<ManipulationBoundaryFeedbackEventArgs> ManipulationBoundaryFeedback
	{
		add
		{
			AddHandler(ManipulationBoundaryFeedbackEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(ManipulationBoundaryFeedbackEvent, value);
		}
	}

	/// <summary>Occurs when a manipulation and inertia on the <see cref="T:System.Windows.UIElement" /> object is complete.</summary>
	[CustomCategory("Touch_Category")]
	public event EventHandler<ManipulationCompletedEventArgs> ManipulationCompleted
	{
		add
		{
			AddHandler(ManipulationCompletedEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(ManipulationCompletedEvent, value);
		}
	}

	/// <summary>Applies an animation to a specified dependency property on this element. Any existing animations are stopped and replaced with the new animation.</summary>
	/// <param name="dp">The identifier for the property to animate.</param>
	/// <param name="clock">The animation clock that controls and declares the animation.</param>
	public void ApplyAnimationClock(DependencyProperty dp, AnimationClock clock)
	{
		ApplyAnimationClock(dp, clock, HandoffBehavior.SnapshotAndReplace);
	}

	/// <summary>Applies an animation to a specified dependency property on this element, with the ability to specify what happens if the property already has a running animation.</summary>
	/// <param name="dp">The property to animate.</param>
	/// <param name="clock">The animation clock that controls and declares the animation.</param>
	/// <param name="handoffBehavior">A value of the enumeration. The default is <see cref="F:System.Windows.Media.Animation.HandoffBehavior.SnapshotAndReplace" />, which will stop any existing animation and replace with the new one.</param>
	public void ApplyAnimationClock(DependencyProperty dp, AnimationClock clock, HandoffBehavior handoffBehavior)
	{
		if (dp == null)
		{
			throw new ArgumentNullException("dp");
		}
		if (!AnimationStorage.IsPropertyAnimatable(this, dp))
		{
			throw new ArgumentException(SR.Format(SR.Animation_DependencyPropertyIsNotAnimatable, dp.Name, GetType()), "dp");
		}
		if (clock != null && !AnimationStorage.IsAnimationValid(dp, clock.Timeline))
		{
			throw new ArgumentException(SR.Format(SR.Animation_AnimationTimelineTypeMismatch, clock.Timeline.GetType(), dp.Name, dp.PropertyType), "clock");
		}
		if (!HandoffBehaviorEnum.IsDefined(handoffBehavior))
		{
			throw new ArgumentException(SR.Animation_UnrecognizedHandoffBehavior);
		}
		if (base.IsSealed)
		{
			throw new InvalidOperationException(SR.Format(SR.IAnimatable_CantAnimateSealedDO, dp, GetType()));
		}
		AnimationStorage.ApplyAnimationClock(this, dp, clock, handoffBehavior);
	}

	/// <summary>Starts an animation for a specified animated property on this element. </summary>
	/// <param name="dp">The property to animate, which is specified as a dependency property identifier.</param>
	/// <param name="animation">The timeline of the animation to start.</param>
	public void BeginAnimation(DependencyProperty dp, AnimationTimeline animation)
	{
		BeginAnimation(dp, animation, HandoffBehavior.SnapshotAndReplace);
	}

	/// <summary>Starts a specific animation for a specified animated property on this element, with the option of specifying what happens if the property already has a running animation. </summary>
	/// <param name="dp">The property to animate, which is specified as the dependency property identifier.</param>
	/// <param name="animation">The timeline of the animation to be applied.</param>
	/// <param name="handoffBehavior">A value of the enumeration that specifies how the new animation interacts with any current (running) animations that are already affecting the property value.</param>
	public void BeginAnimation(DependencyProperty dp, AnimationTimeline animation, HandoffBehavior handoffBehavior)
	{
		if (dp == null)
		{
			throw new ArgumentNullException("dp");
		}
		if (!AnimationStorage.IsPropertyAnimatable(this, dp))
		{
			throw new ArgumentException(SR.Format(SR.Animation_DependencyPropertyIsNotAnimatable, dp.Name, GetType()), "dp");
		}
		if (animation != null && !AnimationStorage.IsAnimationValid(dp, animation))
		{
			throw new ArgumentException(SR.Format(SR.Animation_AnimationTimelineTypeMismatch, animation.GetType(), dp.Name, dp.PropertyType), "animation");
		}
		if (!HandoffBehaviorEnum.IsDefined(handoffBehavior))
		{
			throw new ArgumentException(SR.Animation_UnrecognizedHandoffBehavior);
		}
		if (base.IsSealed)
		{
			throw new InvalidOperationException(SR.Format(SR.IAnimatable_CantAnimateSealedDO, dp, GetType()));
		}
		AnimationStorage.BeginAnimation(this, dp, animation, handoffBehavior);
	}

	/// <summary>Returns the base property value for the specified property on this element, disregarding any possible animated value from a running or stopped animation. </summary>
	/// <returns>The property value as if no animations are attached to the specified dependency property. </returns>
	/// <param name="dp">The dependency property to check.</param>
	public object GetAnimationBaseValue(DependencyProperty dp)
	{
		if (dp == null)
		{
			throw new ArgumentNullException("dp");
		}
		return GetValueEntry(LookupEntry(dp.GlobalIndex), dp, null, RequestFlags.AnimationBaseValue).Value;
	}

	internal sealed override void EvaluateAnimatedValueCore(DependencyProperty dp, PropertyMetadata metadata, ref EffectiveValueEntry entry)
	{
		if (base.IAnimatable_HasAnimatedProperties)
		{
			AnimationStorage.GetStorage(this, dp)?.EvaluateAnimatedValue(metadata, ref entry);
		}
	}

	/// <summary>Returns whether serialization processes should serialize the contents of the <see cref="P:System.Windows.UIElement.InputBindings" /> property on instances of this class.</summary>
	/// <returns>true if the <see cref="P:System.Windows.UIElement.InputBindings" /> property value should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeInputBindings()
	{
		InputBindingCollection value = InputBindingCollectionField.GetValue(this);
		if (value != null && value.Count > 0)
		{
			return true;
		}
		return false;
	}

	/// <summary>Returns whether serialization processes should serialize the contents of the <see cref="P:System.Windows.UIElement.CommandBindings" /> property on instances of this class.</summary>
	/// <returns>true if the <see cref="P:System.Windows.UIElement.CommandBindings" /> property value should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeCommandBindings()
	{
		CommandBindingCollection value = CommandBindingCollectionField.GetValue(this);
		if (value != null && value.Count > 0)
		{
			return true;
		}
		return false;
	}

	internal virtual bool BuildRouteCore(EventRoute route, RoutedEventArgs args)
	{
		return false;
	}

	internal void BuildRoute(EventRoute route, RoutedEventArgs args)
	{
		BuildRouteHelper(this, route, args);
	}

	/// <summary>Raises a specific routed event. The <see cref="T:System.Windows.RoutedEvent" /> to be raised is identified within the <see cref="T:System.Windows.RoutedEventArgs" /> instance that is provided (as the <see cref="P:System.Windows.RoutedEventArgs.RoutedEvent" /> property of that event data). </summary>
	/// <param name="e">A <see cref="T:System.Windows.RoutedEventArgs" /> that contains the event data and also identifies the event to raise. </param>
	public void RaiseEvent(RoutedEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		e.ClearUserInitiated();
		RaiseEventImpl(this, e);
	}

	internal void RaiseEvent(RoutedEventArgs args, bool trusted)
	{
		if (args == null)
		{
			throw new ArgumentNullException("args");
		}
		if (trusted)
		{
			RaiseTrustedEvent(args);
			return;
		}
		args.ClearUserInitiated();
		RaiseEventImpl(this, args);
	}

	internal void RaiseTrustedEvent(RoutedEventArgs args)
	{
		if (args == null)
		{
			throw new ArgumentNullException("args");
		}
		args.MarkAsUserInitiated();
		try
		{
			RaiseEventImpl(this, args);
		}
		finally
		{
			args.ClearUserInitiated();
		}
	}

	internal virtual object AdjustEventSource(RoutedEventArgs args)
	{
		return null;
	}

	/// <summary>Adds a routed event handler for a specified routed event, adding the handler to the handler collection on the current element. </summary>
	/// <param name="routedEvent">An identifier for the routed event to be handled.</param>
	/// <param name="handler">A reference to the handler implementation.</param>
	public void AddHandler(RoutedEvent routedEvent, Delegate handler)
	{
		AddHandler(routedEvent, handler, handledEventsToo: false);
	}

	/// <summary>Adds a routed event handler for a specified routed event, adding the handler to the handler collection on the current element. Specify <paramref name="handledEventsToo" /> as true to have the provided handler be invoked for routed event that had already been marked as handled by another element along the event route. </summary>
	/// <param name="routedEvent">An identifier for the routed event to be handled.</param>
	/// <param name="handler">A reference to the handler implementation.</param>
	/// <param name="handledEventsToo">true to register the handler such that it is invoked even when  the routed event is marked handled in its event data; false to register the handler with the default condition that it will not be invoked if the routed event is already marked handled. The default is false.Do not routinely ask to rehandle a routed event. For more information, see Remarks.</param>
	public void AddHandler(RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
	{
		if (routedEvent == null)
		{
			throw new ArgumentNullException("routedEvent");
		}
		if ((object)handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		if (!routedEvent.IsLegalHandler(handler))
		{
			throw new ArgumentException(SR.HandlerTypeIllegal);
		}
		EnsureEventHandlersStore();
		EventHandlersStore.AddRoutedEventHandler(routedEvent, handler, handledEventsToo);
		OnAddHandler(routedEvent, handler);
	}

	internal virtual void OnAddHandler(RoutedEvent routedEvent, Delegate handler)
	{
	}

	/// <summary> Removes the specified routed event handler from this element. </summary>
	/// <param name="routedEvent">The identifier of the routed event for which the handler is attached.</param>
	/// <param name="handler">The specific handler implementation to remove from the event handler collection on this element.</param>
	public void RemoveHandler(RoutedEvent routedEvent, Delegate handler)
	{
		if (routedEvent == null)
		{
			throw new ArgumentNullException("routedEvent");
		}
		if ((object)handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		if (!routedEvent.IsLegalHandler(handler))
		{
			throw new ArgumentException(SR.HandlerTypeIllegal);
		}
		EventHandlersStore eventHandlersStore = EventHandlersStore;
		if (eventHandlersStore != null)
		{
			eventHandlersStore.RemoveRoutedEventHandler(routedEvent, handler);
			OnRemoveHandler(routedEvent, handler);
			if (eventHandlersStore.Count == 0)
			{
				EventHandlersStoreField.ClearValue(this);
				WriteFlag(CoreFlags.ExistsEventHandlersStore, value: false);
			}
		}
	}

	internal virtual void OnRemoveHandler(RoutedEvent routedEvent, Delegate handler)
	{
	}

	private void EventHandlersStoreAdd(EventPrivateKey key, Delegate handler)
	{
		EnsureEventHandlersStore();
		EventHandlersStore.Add(key, handler);
	}

	private void EventHandlersStoreRemove(EventPrivateKey key, Delegate handler)
	{
		EventHandlersStore eventHandlersStore = EventHandlersStore;
		if (eventHandlersStore != null)
		{
			eventHandlersStore.Remove(key, handler);
			if (eventHandlersStore.Count == 0)
			{
				EventHandlersStoreField.ClearValue(this);
				WriteFlag(CoreFlags.ExistsEventHandlersStore, value: false);
			}
		}
	}

	/// <summary>Adds handlers to the specified <see cref="T:System.Windows.EventRoute" /> for the current <see cref="T:System.Windows.UIElement" /> event handler collection.</summary>
	/// <param name="route">The event route that handlers are added to.</param>
	/// <param name="e">The event data that is used to add the handlers. This method uses the <see cref="P:System.Windows.RoutedEventArgs.RoutedEvent" /> property of the event data to create the handlers. </param>
	public void AddToEventRoute(EventRoute route, RoutedEventArgs e)
	{
		if (route == null)
		{
			throw new ArgumentNullException("route");
		}
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		for (RoutedEventHandlerInfoList routedEventHandlerInfoList = GlobalEventManager.GetDTypedClassListeners(base.DependencyObjectType, e.RoutedEvent); routedEventHandlerInfoList != null; routedEventHandlerInfoList = routedEventHandlerInfoList.Next)
		{
			for (int i = 0; i < routedEventHandlerInfoList.Handlers.Length; i++)
			{
				route.Add(this, routedEventHandlerInfoList.Handlers[i].Handler, routedEventHandlerInfoList.Handlers[i].InvokeHandledEventsToo);
			}
		}
		FrugalObjectList<RoutedEventHandlerInfo> frugalObjectList = null;
		EventHandlersStore eventHandlersStore = EventHandlersStore;
		if (eventHandlersStore != null)
		{
			frugalObjectList = eventHandlersStore[e.RoutedEvent];
			if (frugalObjectList != null)
			{
				for (int j = 0; j < frugalObjectList.Count; j++)
				{
					route.Add(this, frugalObjectList[j].Handler, frugalObjectList[j].InvokeHandledEventsToo);
				}
			}
		}
		AddToEventRouteCore(route, e);
	}

	internal virtual void AddToEventRouteCore(EventRoute route, RoutedEventArgs args)
	{
	}

	[FriendAccessAllowed]
	internal void EnsureEventHandlersStore()
	{
		if (EventHandlersStore == null)
		{
			EventHandlersStoreField.SetValue(this, new EventHandlersStore());
			WriteFlag(CoreFlags.ExistsEventHandlersStore, value: true);
		}
	}

	internal virtual bool InvalidateAutomationAncestorsCore(Stack<DependencyObject> branchNodeStack, out bool continuePastVisualTree)
	{
		continuePastVisualTree = false;
		return true;
	}

	internal static void RegisterEvents(Type type)
	{
		EventManager.RegisterClassHandler(type, Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler(OnPreviewMouseDownThunk), handledEventsToo: true);
		EventManager.RegisterClassHandler(type, Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseDownThunk), handledEventsToo: true);
		EventManager.RegisterClassHandler(type, Mouse.PreviewMouseUpEvent, new MouseButtonEventHandler(OnPreviewMouseUpThunk), handledEventsToo: true);
		EventManager.RegisterClassHandler(type, Mouse.MouseUpEvent, new MouseButtonEventHandler(OnMouseUpThunk), handledEventsToo: true);
		EventManager.RegisterClassHandler(type, PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(OnPreviewMouseLeftButtonDownThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnMouseLeftButtonDownThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, PreviewMouseLeftButtonUpEvent, new MouseButtonEventHandler(OnPreviewMouseLeftButtonUpThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnMouseLeftButtonUpThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, PreviewMouseRightButtonDownEvent, new MouseButtonEventHandler(OnPreviewMouseRightButtonDownThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, MouseRightButtonDownEvent, new MouseButtonEventHandler(OnMouseRightButtonDownThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, PreviewMouseRightButtonUpEvent, new MouseButtonEventHandler(OnPreviewMouseRightButtonUpThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, MouseRightButtonUpEvent, new MouseButtonEventHandler(OnMouseRightButtonUpThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Mouse.PreviewMouseMoveEvent, new MouseEventHandler(OnPreviewMouseMoveThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Mouse.MouseMoveEvent, new MouseEventHandler(OnMouseMoveThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Mouse.PreviewMouseWheelEvent, new MouseWheelEventHandler(OnPreviewMouseWheelThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Mouse.MouseWheelEvent, new MouseWheelEventHandler(OnMouseWheelThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Mouse.MouseEnterEvent, new MouseEventHandler(OnMouseEnterThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Mouse.MouseLeaveEvent, new MouseEventHandler(OnMouseLeaveThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Mouse.GotMouseCaptureEvent, new MouseEventHandler(OnGotMouseCaptureThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Mouse.LostMouseCaptureEvent, new MouseEventHandler(OnLostMouseCaptureThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Mouse.QueryCursorEvent, new QueryCursorEventHandler(OnQueryCursorThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Stylus.PreviewStylusDownEvent, new StylusDownEventHandler(OnPreviewStylusDownThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Stylus.StylusDownEvent, new StylusDownEventHandler(OnStylusDownThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Stylus.PreviewStylusUpEvent, new StylusEventHandler(OnPreviewStylusUpThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Stylus.StylusUpEvent, new StylusEventHandler(OnStylusUpThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Stylus.PreviewStylusMoveEvent, new StylusEventHandler(OnPreviewStylusMoveThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Stylus.StylusMoveEvent, new StylusEventHandler(OnStylusMoveThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Stylus.PreviewStylusInAirMoveEvent, new StylusEventHandler(OnPreviewStylusInAirMoveThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Stylus.StylusInAirMoveEvent, new StylusEventHandler(OnStylusInAirMoveThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Stylus.StylusEnterEvent, new StylusEventHandler(OnStylusEnterThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Stylus.StylusLeaveEvent, new StylusEventHandler(OnStylusLeaveThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Stylus.PreviewStylusInRangeEvent, new StylusEventHandler(OnPreviewStylusInRangeThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Stylus.StylusInRangeEvent, new StylusEventHandler(OnStylusInRangeThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Stylus.PreviewStylusOutOfRangeEvent, new StylusEventHandler(OnPreviewStylusOutOfRangeThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Stylus.StylusOutOfRangeEvent, new StylusEventHandler(OnStylusOutOfRangeThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Stylus.PreviewStylusSystemGestureEvent, new StylusSystemGestureEventHandler(OnPreviewStylusSystemGestureThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Stylus.StylusSystemGestureEvent, new StylusSystemGestureEventHandler(OnStylusSystemGestureThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Stylus.GotStylusCaptureEvent, new StylusEventHandler(OnGotStylusCaptureThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Stylus.LostStylusCaptureEvent, new StylusEventHandler(OnLostStylusCaptureThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Stylus.StylusButtonDownEvent, new StylusButtonEventHandler(OnStylusButtonDownThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Stylus.StylusButtonUpEvent, new StylusButtonEventHandler(OnStylusButtonUpThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Stylus.PreviewStylusButtonDownEvent, new StylusButtonEventHandler(OnPreviewStylusButtonDownThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Stylus.PreviewStylusButtonUpEvent, new StylusButtonEventHandler(OnPreviewStylusButtonUpThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Keyboard.PreviewKeyDownEvent, new KeyEventHandler(OnPreviewKeyDownThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Keyboard.KeyDownEvent, new KeyEventHandler(OnKeyDownThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Keyboard.PreviewKeyUpEvent, new KeyEventHandler(OnPreviewKeyUpThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Keyboard.KeyUpEvent, new KeyEventHandler(OnKeyUpThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Keyboard.PreviewGotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnPreviewGotKeyboardFocusThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Keyboard.GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnGotKeyboardFocusThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Keyboard.PreviewLostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnPreviewLostKeyboardFocusThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Keyboard.LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnLostKeyboardFocusThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, TextCompositionManager.PreviewTextInputEvent, new TextCompositionEventHandler(OnPreviewTextInputThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, TextCompositionManager.TextInputEvent, new TextCompositionEventHandler(OnTextInputThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, CommandManager.PreviewExecutedEvent, new ExecutedRoutedEventHandler(OnPreviewExecutedThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, CommandManager.ExecutedEvent, new ExecutedRoutedEventHandler(OnExecutedThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, CommandManager.PreviewCanExecuteEvent, new CanExecuteRoutedEventHandler(OnPreviewCanExecuteThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, CommandManager.CanExecuteEvent, new CanExecuteRoutedEventHandler(OnCanExecuteThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, CommandDevice.CommandDeviceEvent, new CommandDeviceEventHandler(OnCommandDeviceThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, DragDrop.PreviewQueryContinueDragEvent, new QueryContinueDragEventHandler(OnPreviewQueryContinueDragThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, DragDrop.QueryContinueDragEvent, new QueryContinueDragEventHandler(OnQueryContinueDragThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, DragDrop.PreviewGiveFeedbackEvent, new GiveFeedbackEventHandler(OnPreviewGiveFeedbackThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, DragDrop.GiveFeedbackEvent, new GiveFeedbackEventHandler(OnGiveFeedbackThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, DragDrop.PreviewDragEnterEvent, new DragEventHandler(OnPreviewDragEnterThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, DragDrop.DragEnterEvent, new DragEventHandler(OnDragEnterThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, DragDrop.PreviewDragOverEvent, new DragEventHandler(OnPreviewDragOverThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, DragDrop.DragOverEvent, new DragEventHandler(OnDragOverThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, DragDrop.PreviewDragLeaveEvent, new DragEventHandler(OnPreviewDragLeaveThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, DragDrop.DragLeaveEvent, new DragEventHandler(OnDragLeaveThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, DragDrop.PreviewDropEvent, new DragEventHandler(OnPreviewDropThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, DragDrop.DropEvent, new DragEventHandler(OnDropThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Touch.PreviewTouchDownEvent, new EventHandler<TouchEventArgs>(OnPreviewTouchDownThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Touch.TouchDownEvent, new EventHandler<TouchEventArgs>(OnTouchDownThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Touch.PreviewTouchMoveEvent, new EventHandler<TouchEventArgs>(OnPreviewTouchMoveThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Touch.TouchMoveEvent, new EventHandler<TouchEventArgs>(OnTouchMoveThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Touch.PreviewTouchUpEvent, new EventHandler<TouchEventArgs>(OnPreviewTouchUpThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Touch.TouchUpEvent, new EventHandler<TouchEventArgs>(OnTouchUpThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Touch.GotTouchCaptureEvent, new EventHandler<TouchEventArgs>(OnGotTouchCaptureThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Touch.LostTouchCaptureEvent, new EventHandler<TouchEventArgs>(OnLostTouchCaptureThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Touch.TouchEnterEvent, new EventHandler<TouchEventArgs>(OnTouchEnterThunk), handledEventsToo: false);
		EventManager.RegisterClassHandler(type, Touch.TouchLeaveEvent, new EventHandler<TouchEventArgs>(OnTouchLeaveThunk), handledEventsToo: false);
	}

	private static void OnPreviewMouseDownThunk(object sender, MouseButtonEventArgs e)
	{
		if (!e.Handled)
		{
			if (sender is UIElement uIElement)
			{
				uIElement.OnPreviewMouseDown(e);
			}
			else if (sender is ContentElement contentElement)
			{
				contentElement.OnPreviewMouseDown(e);
			}
			else
			{
				((UIElement3D)sender).OnPreviewMouseDown(e);
			}
		}
		CrackMouseButtonEventAndReRaiseEvent((DependencyObject)sender, e);
	}

	private static void OnMouseDownThunk(object sender, MouseButtonEventArgs e)
	{
		if (!e.Handled)
		{
			CommandManager.TranslateInput((IInputElement)sender, e);
		}
		if (!e.Handled)
		{
			if (sender is UIElement uIElement)
			{
				uIElement.OnMouseDown(e);
			}
			else if (sender is ContentElement contentElement)
			{
				contentElement.OnMouseDown(e);
			}
			else
			{
				((UIElement3D)sender).OnMouseDown(e);
			}
		}
		CrackMouseButtonEventAndReRaiseEvent((DependencyObject)sender, e);
	}

	private static void OnPreviewMouseUpThunk(object sender, MouseButtonEventArgs e)
	{
		if (!e.Handled)
		{
			if (sender is UIElement uIElement)
			{
				uIElement.OnPreviewMouseUp(e);
			}
			else if (sender is ContentElement contentElement)
			{
				contentElement.OnPreviewMouseUp(e);
			}
			else
			{
				((UIElement3D)sender).OnPreviewMouseUp(e);
			}
		}
		CrackMouseButtonEventAndReRaiseEvent((DependencyObject)sender, e);
	}

	private static void OnMouseUpThunk(object sender, MouseButtonEventArgs e)
	{
		if (!e.Handled)
		{
			if (sender is UIElement uIElement)
			{
				uIElement.OnMouseUp(e);
			}
			else if (sender is ContentElement contentElement)
			{
				contentElement.OnMouseUp(e);
			}
			else
			{
				((UIElement3D)sender).OnMouseUp(e);
			}
		}
		CrackMouseButtonEventAndReRaiseEvent((DependencyObject)sender, e);
	}

	private static void OnPreviewMouseLeftButtonDownThunk(object sender, MouseButtonEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewMouseLeftButtonDown(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewMouseLeftButtonDown(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewMouseLeftButtonDown(e);
		}
	}

	private static void OnMouseLeftButtonDownThunk(object sender, MouseButtonEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnMouseLeftButtonDown(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnMouseLeftButtonDown(e);
		}
		else
		{
			((UIElement3D)sender).OnMouseLeftButtonDown(e);
		}
	}

	private static void OnPreviewMouseLeftButtonUpThunk(object sender, MouseButtonEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewMouseLeftButtonUp(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewMouseLeftButtonUp(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewMouseLeftButtonUp(e);
		}
	}

	private static void OnMouseLeftButtonUpThunk(object sender, MouseButtonEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnMouseLeftButtonUp(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnMouseLeftButtonUp(e);
		}
		else
		{
			((UIElement3D)sender).OnMouseLeftButtonUp(e);
		}
	}

	private static void OnPreviewMouseRightButtonDownThunk(object sender, MouseButtonEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewMouseRightButtonDown(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewMouseRightButtonDown(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewMouseRightButtonDown(e);
		}
	}

	private static void OnMouseRightButtonDownThunk(object sender, MouseButtonEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnMouseRightButtonDown(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnMouseRightButtonDown(e);
		}
		else
		{
			((UIElement3D)sender).OnMouseRightButtonDown(e);
		}
	}

	private static void OnPreviewMouseRightButtonUpThunk(object sender, MouseButtonEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewMouseRightButtonUp(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewMouseRightButtonUp(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewMouseRightButtonUp(e);
		}
	}

	private static void OnMouseRightButtonUpThunk(object sender, MouseButtonEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnMouseRightButtonUp(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnMouseRightButtonUp(e);
		}
		else
		{
			((UIElement3D)sender).OnMouseRightButtonUp(e);
		}
	}

	private static void OnPreviewMouseMoveThunk(object sender, MouseEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewMouseMove(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewMouseMove(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewMouseMove(e);
		}
	}

	private static void OnMouseMoveThunk(object sender, MouseEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnMouseMove(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnMouseMove(e);
		}
		else
		{
			((UIElement3D)sender).OnMouseMove(e);
		}
	}

	private static void OnPreviewMouseWheelThunk(object sender, MouseWheelEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewMouseWheel(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewMouseWheel(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewMouseWheel(e);
		}
	}

	private static void OnMouseWheelThunk(object sender, MouseWheelEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		CommandManager.TranslateInput((IInputElement)sender, e);
		if (!e.Handled)
		{
			if (sender is UIElement uIElement)
			{
				uIElement.OnMouseWheel(e);
			}
			else if (sender is ContentElement contentElement)
			{
				contentElement.OnMouseWheel(e);
			}
			else
			{
				((UIElement3D)sender).OnMouseWheel(e);
			}
		}
	}

	private static void OnMouseEnterThunk(object sender, MouseEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnMouseEnter(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnMouseEnter(e);
		}
		else
		{
			((UIElement3D)sender).OnMouseEnter(e);
		}
	}

	private static void OnMouseLeaveThunk(object sender, MouseEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnMouseLeave(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnMouseLeave(e);
		}
		else
		{
			((UIElement3D)sender).OnMouseLeave(e);
		}
	}

	private static void OnGotMouseCaptureThunk(object sender, MouseEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnGotMouseCapture(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnGotMouseCapture(e);
		}
		else
		{
			((UIElement3D)sender).OnGotMouseCapture(e);
		}
	}

	private static void OnLostMouseCaptureThunk(object sender, MouseEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnLostMouseCapture(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnLostMouseCapture(e);
		}
		else
		{
			((UIElement3D)sender).OnLostMouseCapture(e);
		}
	}

	private static void OnQueryCursorThunk(object sender, QueryCursorEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnQueryCursor(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnQueryCursor(e);
		}
		else
		{
			((UIElement3D)sender).OnQueryCursor(e);
		}
	}

	private static void OnPreviewStylusDownThunk(object sender, StylusDownEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewStylusDown(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewStylusDown(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewStylusDown(e);
		}
	}

	private static void OnStylusDownThunk(object sender, StylusDownEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnStylusDown(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnStylusDown(e);
		}
		else
		{
			((UIElement3D)sender).OnStylusDown(e);
		}
	}

	private static void OnPreviewStylusUpThunk(object sender, StylusEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewStylusUp(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewStylusUp(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewStylusUp(e);
		}
	}

	private static void OnStylusUpThunk(object sender, StylusEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnStylusUp(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnStylusUp(e);
		}
		else
		{
			((UIElement3D)sender).OnStylusUp(e);
		}
	}

	private static void OnPreviewStylusMoveThunk(object sender, StylusEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewStylusMove(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewStylusMove(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewStylusMove(e);
		}
	}

	private static void OnStylusMoveThunk(object sender, StylusEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnStylusMove(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnStylusMove(e);
		}
		else
		{
			((UIElement3D)sender).OnStylusMove(e);
		}
	}

	private static void OnPreviewStylusInAirMoveThunk(object sender, StylusEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewStylusInAirMove(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewStylusInAirMove(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewStylusInAirMove(e);
		}
	}

	private static void OnStylusInAirMoveThunk(object sender, StylusEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnStylusInAirMove(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnStylusInAirMove(e);
		}
		else
		{
			((UIElement3D)sender).OnStylusInAirMove(e);
		}
	}

	private static void OnStylusEnterThunk(object sender, StylusEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnStylusEnter(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnStylusEnter(e);
		}
		else
		{
			((UIElement3D)sender).OnStylusEnter(e);
		}
	}

	private static void OnStylusLeaveThunk(object sender, StylusEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnStylusLeave(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnStylusLeave(e);
		}
		else
		{
			((UIElement3D)sender).OnStylusLeave(e);
		}
	}

	private static void OnPreviewStylusInRangeThunk(object sender, StylusEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewStylusInRange(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewStylusInRange(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewStylusInRange(e);
		}
	}

	private static void OnStylusInRangeThunk(object sender, StylusEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnStylusInRange(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnStylusInRange(e);
		}
		else
		{
			((UIElement3D)sender).OnStylusInRange(e);
		}
	}

	private static void OnPreviewStylusOutOfRangeThunk(object sender, StylusEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewStylusOutOfRange(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewStylusOutOfRange(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewStylusOutOfRange(e);
		}
	}

	private static void OnStylusOutOfRangeThunk(object sender, StylusEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnStylusOutOfRange(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnStylusOutOfRange(e);
		}
		else
		{
			((UIElement3D)sender).OnStylusOutOfRange(e);
		}
	}

	private static void OnPreviewStylusSystemGestureThunk(object sender, StylusSystemGestureEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewStylusSystemGesture(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewStylusSystemGesture(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewStylusSystemGesture(e);
		}
	}

	private static void OnStylusSystemGestureThunk(object sender, StylusSystemGestureEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnStylusSystemGesture(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnStylusSystemGesture(e);
		}
		else
		{
			((UIElement3D)sender).OnStylusSystemGesture(e);
		}
	}

	private static void OnGotStylusCaptureThunk(object sender, StylusEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnGotStylusCapture(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnGotStylusCapture(e);
		}
		else
		{
			((UIElement3D)sender).OnGotStylusCapture(e);
		}
	}

	private static void OnLostStylusCaptureThunk(object sender, StylusEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnLostStylusCapture(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnLostStylusCapture(e);
		}
		else
		{
			((UIElement3D)sender).OnLostStylusCapture(e);
		}
	}

	private static void OnStylusButtonDownThunk(object sender, StylusButtonEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnStylusButtonDown(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnStylusButtonDown(e);
		}
		else
		{
			((UIElement3D)sender).OnStylusButtonDown(e);
		}
	}

	private static void OnStylusButtonUpThunk(object sender, StylusButtonEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnStylusButtonUp(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnStylusButtonUp(e);
		}
		else
		{
			((UIElement3D)sender).OnStylusButtonUp(e);
		}
	}

	private static void OnPreviewStylusButtonDownThunk(object sender, StylusButtonEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewStylusButtonDown(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewStylusButtonDown(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewStylusButtonDown(e);
		}
	}

	private static void OnPreviewStylusButtonUpThunk(object sender, StylusButtonEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewStylusButtonUp(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewStylusButtonUp(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewStylusButtonUp(e);
		}
	}

	private static void OnPreviewKeyDownThunk(object sender, KeyEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewKeyDown(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewKeyDown(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewKeyDown(e);
		}
	}

	private static void OnKeyDownThunk(object sender, KeyEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		CommandManager.TranslateInput((IInputElement)sender, e);
		if (!e.Handled)
		{
			if (sender is UIElement uIElement)
			{
				uIElement.OnKeyDown(e);
			}
			else if (sender is ContentElement contentElement)
			{
				contentElement.OnKeyDown(e);
			}
			else
			{
				((UIElement3D)sender).OnKeyDown(e);
			}
		}
	}

	private static void OnPreviewKeyUpThunk(object sender, KeyEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewKeyUp(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewKeyUp(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewKeyUp(e);
		}
	}

	private static void OnKeyUpThunk(object sender, KeyEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnKeyUp(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnKeyUp(e);
		}
		else
		{
			((UIElement3D)sender).OnKeyUp(e);
		}
	}

	private static void OnPreviewGotKeyboardFocusThunk(object sender, KeyboardFocusChangedEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewGotKeyboardFocus(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewGotKeyboardFocus(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewGotKeyboardFocus(e);
		}
	}

	private static void OnGotKeyboardFocusThunk(object sender, KeyboardFocusChangedEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnGotKeyboardFocus(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnGotKeyboardFocus(e);
		}
		else
		{
			((UIElement3D)sender).OnGotKeyboardFocus(e);
		}
	}

	private static void OnPreviewLostKeyboardFocusThunk(object sender, KeyboardFocusChangedEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewLostKeyboardFocus(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewLostKeyboardFocus(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewLostKeyboardFocus(e);
		}
	}

	private static void OnLostKeyboardFocusThunk(object sender, KeyboardFocusChangedEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnLostKeyboardFocus(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnLostKeyboardFocus(e);
		}
		else
		{
			((UIElement3D)sender).OnLostKeyboardFocus(e);
		}
	}

	private static void OnPreviewTextInputThunk(object sender, TextCompositionEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewTextInput(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewTextInput(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewTextInput(e);
		}
	}

	private static void OnTextInputThunk(object sender, TextCompositionEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnTextInput(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnTextInput(e);
		}
		else
		{
			((UIElement3D)sender).OnTextInput(e);
		}
	}

	private static void OnPreviewExecutedThunk(object sender, ExecutedRoutedEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		CommandManager.OnExecuted(sender, e);
	}

	private static void OnExecutedThunk(object sender, ExecutedRoutedEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		CommandManager.OnExecuted(sender, e);
	}

	private static void OnPreviewCanExecuteThunk(object sender, CanExecuteRoutedEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		CommandManager.OnCanExecute(sender, e);
	}

	private static void OnCanExecuteThunk(object sender, CanExecuteRoutedEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		CommandManager.OnCanExecute(sender, e);
	}

	private static void OnCommandDeviceThunk(object sender, CommandDeviceEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		CommandManager.OnCommandDevice(sender, e);
	}

	private static void OnPreviewQueryContinueDragThunk(object sender, QueryContinueDragEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewQueryContinueDrag(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewQueryContinueDrag(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewQueryContinueDrag(e);
		}
	}

	private static void OnQueryContinueDragThunk(object sender, QueryContinueDragEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnQueryContinueDrag(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnQueryContinueDrag(e);
		}
		else
		{
			((UIElement3D)sender).OnQueryContinueDrag(e);
		}
	}

	private static void OnPreviewGiveFeedbackThunk(object sender, GiveFeedbackEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewGiveFeedback(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewGiveFeedback(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewGiveFeedback(e);
		}
	}

	private static void OnGiveFeedbackThunk(object sender, GiveFeedbackEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnGiveFeedback(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnGiveFeedback(e);
		}
		else
		{
			((UIElement3D)sender).OnGiveFeedback(e);
		}
	}

	private static void OnPreviewDragEnterThunk(object sender, DragEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewDragEnter(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewDragEnter(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewDragEnter(e);
		}
	}

	private static void OnDragEnterThunk(object sender, DragEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnDragEnter(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnDragEnter(e);
		}
		else
		{
			((UIElement3D)sender).OnDragEnter(e);
		}
	}

	private static void OnPreviewDragOverThunk(object sender, DragEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewDragOver(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewDragOver(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewDragOver(e);
		}
	}

	private static void OnDragOverThunk(object sender, DragEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnDragOver(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnDragOver(e);
		}
		else
		{
			((UIElement3D)sender).OnDragOver(e);
		}
	}

	private static void OnPreviewDragLeaveThunk(object sender, DragEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewDragLeave(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewDragLeave(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewDragLeave(e);
		}
	}

	private static void OnDragLeaveThunk(object sender, DragEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnDragLeave(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnDragLeave(e);
		}
		else
		{
			((UIElement3D)sender).OnDragLeave(e);
		}
	}

	private static void OnPreviewDropThunk(object sender, DragEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewDrop(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewDrop(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewDrop(e);
		}
	}

	private static void OnDropThunk(object sender, DragEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnDrop(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnDrop(e);
		}
		else
		{
			((UIElement3D)sender).OnDrop(e);
		}
	}

	private static void OnPreviewTouchDownThunk(object sender, TouchEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewTouchDown(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewTouchDown(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewTouchDown(e);
		}
	}

	private static void OnTouchDownThunk(object sender, TouchEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnTouchDown(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnTouchDown(e);
		}
		else
		{
			((UIElement3D)sender).OnTouchDown(e);
		}
	}

	private static void OnPreviewTouchMoveThunk(object sender, TouchEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewTouchMove(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewTouchMove(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewTouchMove(e);
		}
	}

	private static void OnTouchMoveThunk(object sender, TouchEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnTouchMove(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnTouchMove(e);
		}
		else
		{
			((UIElement3D)sender).OnTouchMove(e);
		}
	}

	private static void OnPreviewTouchUpThunk(object sender, TouchEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnPreviewTouchUp(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnPreviewTouchUp(e);
		}
		else
		{
			((UIElement3D)sender).OnPreviewTouchUp(e);
		}
	}

	private static void OnTouchUpThunk(object sender, TouchEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnTouchUp(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnTouchUp(e);
		}
		else
		{
			((UIElement3D)sender).OnTouchUp(e);
		}
	}

	private static void OnGotTouchCaptureThunk(object sender, TouchEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnGotTouchCapture(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnGotTouchCapture(e);
		}
		else
		{
			((UIElement3D)sender).OnGotTouchCapture(e);
		}
	}

	private static void OnLostTouchCaptureThunk(object sender, TouchEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnLostTouchCapture(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnLostTouchCapture(e);
		}
		else
		{
			((UIElement3D)sender).OnLostTouchCapture(e);
		}
	}

	private static void OnTouchEnterThunk(object sender, TouchEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnTouchEnter(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnTouchEnter(e);
		}
		else
		{
			((UIElement3D)sender).OnTouchEnter(e);
		}
	}

	private static void OnTouchLeaveThunk(object sender, TouchEventArgs e)
	{
		Invariant.Assert(!e.Handled, "Unexpected: Event has already been handled.");
		if (sender is UIElement uIElement)
		{
			uIElement.OnTouchLeave(e);
		}
		else if (sender is ContentElement contentElement)
		{
			contentElement.OnTouchLeave(e);
		}
		else
		{
			((UIElement3D)sender).OnTouchLeave(e);
		}
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.PreviewMouseDown" /> attached routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that one or more mouse buttons were pressed.</param>
	protected virtual void OnPreviewMouseDown(MouseButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseDown" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. This event data reports details about the mouse button that was pressed and the handled state.</param>
	protected virtual void OnMouseDown(MouseButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.PreviewMouseUp" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.</summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that one or more mouse buttons were released.</param>
	protected virtual void OnPreviewMouseUp(MouseButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseUp" /> routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that the mouse button was released.</param>
	protected virtual void OnMouseUp(MouseButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.UIElement.PreviewMouseLeftButtonDown" /> routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.</summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that the left mouse button was pressed.</param>
	protected virtual void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.UIElement.MouseLeftButtonDown" /> routed event is raised on this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that the left mouse button was pressed.</param>
	protected virtual void OnMouseLeftButtonDown(MouseButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.UIElement.PreviewMouseLeftButtonUp" /> routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.</summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that the left mouse button was released.</param>
	protected virtual void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.UIElement.MouseLeftButtonUp" /> routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that the left mouse button was released.</param>
	protected virtual void OnMouseLeftButtonUp(MouseButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.UIElement.PreviewMouseRightButtonDown" /> routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.</summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that the right mouse button was pressed.</param>
	protected virtual void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.UIElement.MouseRightButtonDown" /> routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that the right mouse button was pressed.</param>
	protected virtual void OnMouseRightButtonDown(MouseButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.UIElement.PreviewMouseRightButtonUp" /> routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.</summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that the right mouse button was released.</param>
	protected virtual void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.UIElement.MouseRightButtonUp" /> routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that the right mouse button was released.</param>
	protected virtual void OnMouseRightButtonUp(MouseButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.PreviewMouseMove" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewMouseMove(MouseEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseMove" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
	protected virtual void OnMouseMove(MouseEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.PreviewMouseWheel" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseWheelEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewMouseWheel(MouseWheelEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseWheel" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseWheelEventArgs" /> that contains the event data.</param>
	protected virtual void OnMouseWheel(MouseWheelEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseEnter" /> attached event is raised on this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
	protected virtual void OnMouseEnter(MouseEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseLeave" /> attached event is raised on this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
	protected virtual void OnMouseLeave(MouseEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.GotMouseCapture" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
	protected virtual void OnGotMouseCapture(MouseEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.LostMouseCapture" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains event data.</param>
	protected virtual void OnLostMouseCapture(MouseEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.QueryCursor" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.QueryCursorEventArgs" /> that contains the event data.</param>
	protected virtual void OnQueryCursor(QueryCursorEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.PreviewStylusDown" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.</summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusDownEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewStylusDown(StylusDownEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.StylusDown" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusDownEventArgs" /> that contains the event data.</param>
	protected virtual void OnStylusDown(StylusDownEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.PreviewStylusUp" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewStylusUp(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.StylusUp" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected virtual void OnStylusUp(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.PreviewStylusMove" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewStylusMove(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.StylusMove" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected virtual void OnStylusMove(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.PreviewStylusInAirMove" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewStylusInAirMove(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.StylusInAirMove" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected virtual void OnStylusInAirMove(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.StylusEnter" /> attached event is raised by this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected virtual void OnStylusEnter(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.StylusLeave" /> attached event is raised by this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected virtual void OnStylusLeave(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.PreviewStylusInRange" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewStylusInRange(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.StylusInRange" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected virtual void OnStylusInRange(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.PreviewStylusOutOfRange" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewStylusOutOfRange(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.StylusOutOfRange" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected virtual void OnStylusOutOfRange(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.PreviewStylusSystemGesture" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusSystemGestureEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewStylusSystemGesture(StylusSystemGestureEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.StylusSystemGesture" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusSystemGestureEventArgs" /> that contains the event data.</param>
	protected virtual void OnStylusSystemGesture(StylusSystemGestureEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.GotStylusCapture" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected virtual void OnGotStylusCapture(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.LostStylusCapture" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains event data.</param>
	protected virtual void OnLostStylusCapture(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.StylusButtonDown" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusButtonEventArgs" /> that contains the event data.</param>
	protected virtual void OnStylusButtonDown(StylusButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.StylusButtonUp" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusButtonEventArgs" /> that contains the event data.</param>
	protected virtual void OnStylusButtonUp(StylusButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.PreviewStylusButtonDown" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusButtonEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewStylusButtonDown(StylusButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.PreviewStylusButtonUp" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusButtonEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewStylusButtonUp(StylusButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Keyboard.PreviewKeyDown" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.KeyEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewKeyDown(KeyEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Keyboard.KeyDown" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.KeyEventArgs" /> that contains the event data.</param>
	protected virtual void OnKeyDown(KeyEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Keyboard.PreviewKeyUp" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.KeyEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewKeyUp(KeyEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Keyboard.KeyUp" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.KeyEventArgs" /> that contains the event data.</param>
	protected virtual void OnKeyUp(KeyEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Keyboard.PreviewGotKeyboardFocus" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.KeyboardFocusChangedEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Keyboard.GotKeyboardFocus" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.KeyboardFocusChangedEventArgs" /> that contains the event data.</param>
	protected virtual void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Keyboard.PreviewKeyDown" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.KeyboardFocusChangedEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Keyboard.LostKeyboardFocus" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.KeyboardFocusChangedEventArgs" /> that contains event data.</param>
	protected virtual void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.TextCompositionManager.PreviewTextInput" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.TextCompositionEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewTextInput(TextCompositionEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.TextCompositionManager.TextInput" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.TextCompositionEventArgs" /> that contains the event data.</param>
	protected virtual void OnTextInput(TextCompositionEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.DragDrop.PreviewQueryContinueDrag" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.QueryContinueDragEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewQueryContinueDrag(QueryContinueDragEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.DragDrop.QueryContinueDrag" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.QueryContinueDragEventArgs" /> that contains the event data.</param>
	protected virtual void OnQueryContinueDrag(QueryContinueDragEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.DragDrop.PreviewGiveFeedback" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.GiveFeedbackEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewGiveFeedback(GiveFeedbackEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.DragDrop.GiveFeedback" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.GiveFeedbackEventArgs" /> that contains the event data.</param>
	protected virtual void OnGiveFeedback(GiveFeedbackEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.DragDrop.PreviewDragEnter" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DragEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewDragEnter(DragEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.DragDrop.DragEnter" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DragEventArgs" /> that contains the event data.</param>
	protected virtual void OnDragEnter(DragEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.DragDrop.PreviewDragOver" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DragEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewDragOver(DragEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.DragDrop.DragOver" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DragEventArgs" /> that contains the event data.</param>
	protected virtual void OnDragOver(DragEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.DragDrop.PreviewDragLeave" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DragEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewDragLeave(DragEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.DragDrop.DragLeave" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DragEventArgs" /> that contains the event data.</param>
	protected virtual void OnDragLeave(DragEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.DragDrop.PreviewDrop" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DragEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewDrop(DragEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.DragDrop.DragEnter" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DragEventArgs" /> that contains the event data.</param>
	protected virtual void OnDrop(DragEventArgs e)
	{
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.PreviewTouchDown" /> routed event that occurs when a touch presses this element. </summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.TouchEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewTouchDown(TouchEventArgs e)
	{
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.TouchDown" /> routed event that occurs when a touch presses inside this element.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.TouchEventArgs" /> that contains the event data.</param>
	protected virtual void OnTouchDown(TouchEventArgs e)
	{
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.PreviewTouchMove" /> routed event that occurs when a touch moves while inside this element.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.TouchEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewTouchMove(TouchEventArgs e)
	{
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.TouchMove" /> routed event that occurs when a touch moves while inside this element.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.TouchEventArgs" /> that contains the event data.</param>
	protected virtual void OnTouchMove(TouchEventArgs e)
	{
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.PreviewTouchUp" /> routed event that occurs when a touch is released inside this element. </summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.TouchEventArgs" /> that contains the event data.</param>
	protected virtual void OnPreviewTouchUp(TouchEventArgs e)
	{
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.TouchUp" /> routed event that occurs when a touch is released inside this element.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.TouchEventArgs" /> that contains the event data.</param>
	protected virtual void OnTouchUp(TouchEventArgs e)
	{
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.GotTouchCapture" /> routed event that occurs when a touch is captured to this element.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.TouchEventArgs" /> that contains the event data. </param>
	protected virtual void OnGotTouchCapture(TouchEventArgs e)
	{
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.LostTouchCapture" /> routed event that occurs when this element loses a touch capture.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.TouchEventArgs" /> that contains the event data.</param>
	protected virtual void OnLostTouchCapture(TouchEventArgs e)
	{
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.TouchEnter" /> routed event that occurs when a touch moves from outside to inside the bounds of this element. </summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.TouchEventArgs" /> that contains the event data.</param>
	protected virtual void OnTouchEnter(TouchEventArgs e)
	{
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.TouchLeave" /> routed event that occurs when a touch moves from inside to outside the bounds of this <see cref="T:System.Windows.UIElement" />. </summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.TouchEventArgs" /> that contains the event data.</param>
	protected virtual void OnTouchLeave(TouchEventArgs e)
	{
	}

	private static void IsMouseDirectlyOver_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((UIElement)d).RaiseIsMouseDirectlyOverChanged(e);
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.UIElement.IsMouseDirectlyOverChanged" /> event is raised on this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> that contains the event data.</param>
	protected virtual void OnIsMouseDirectlyOverChanged(DependencyPropertyChangedEventArgs e)
	{
	}

	private void RaiseIsMouseDirectlyOverChanged(DependencyPropertyChangedEventArgs args)
	{
		OnIsMouseDirectlyOverChanged(args);
		RaiseDependencyPropertyChanged(IsMouseDirectlyOverChangedKey, args);
	}

	/// <summary>Invoked just before the <see cref="E:System.Windows.UIElement.IsKeyboardFocusWithinChanged" /> event is raised by this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">A <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> that contains the event data.</param>
	protected virtual void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
	{
	}

	internal void RaiseIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs args)
	{
		OnIsKeyboardFocusWithinChanged(args);
		RaiseDependencyPropertyChanged(IsKeyboardFocusWithinChangedKey, args);
	}

	private static void IsMouseCaptured_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((UIElement)d).RaiseIsMouseCapturedChanged(e);
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.UIElement.IsMouseCapturedChanged" /> event is raised on this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> that contains the event data.</param>
	protected virtual void OnIsMouseCapturedChanged(DependencyPropertyChangedEventArgs e)
	{
	}

	private void RaiseIsMouseCapturedChanged(DependencyPropertyChangedEventArgs args)
	{
		OnIsMouseCapturedChanged(args);
		RaiseDependencyPropertyChanged(IsMouseCapturedChangedKey, args);
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.UIElement.IsMouseCaptureWithinChanged" /> event is raised on this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">A <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> that contains the event data.</param>
	protected virtual void OnIsMouseCaptureWithinChanged(DependencyPropertyChangedEventArgs e)
	{
	}

	internal void RaiseIsMouseCaptureWithinChanged(DependencyPropertyChangedEventArgs args)
	{
		OnIsMouseCaptureWithinChanged(args);
		RaiseDependencyPropertyChanged(IsMouseCaptureWithinChangedKey, args);
	}

	private static void IsStylusDirectlyOver_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((UIElement)d).RaiseIsStylusDirectlyOverChanged(e);
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.UIElement.IsStylusDirectlyOverChanged" /> event is raised on this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> that contains the event data.</param>
	protected virtual void OnIsStylusDirectlyOverChanged(DependencyPropertyChangedEventArgs e)
	{
	}

	private void RaiseIsStylusDirectlyOverChanged(DependencyPropertyChangedEventArgs args)
	{
		OnIsStylusDirectlyOverChanged(args);
		RaiseDependencyPropertyChanged(IsStylusDirectlyOverChangedKey, args);
	}

	private static void IsStylusCaptured_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((UIElement)d).RaiseIsStylusCapturedChanged(e);
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.UIElement.IsStylusCapturedChanged" /> event is raised on this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">A <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> that contains the event data.</param>
	protected virtual void OnIsStylusCapturedChanged(DependencyPropertyChangedEventArgs e)
	{
	}

	private void RaiseIsStylusCapturedChanged(DependencyPropertyChangedEventArgs args)
	{
		OnIsStylusCapturedChanged(args);
		RaiseDependencyPropertyChanged(IsStylusCapturedChangedKey, args);
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.UIElement.IsStylusCaptureWithinChanged" /> event is raised on this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> that contains the event data.</param>
	protected virtual void OnIsStylusCaptureWithinChanged(DependencyPropertyChangedEventArgs e)
	{
	}

	internal void RaiseIsStylusCaptureWithinChanged(DependencyPropertyChangedEventArgs args)
	{
		OnIsStylusCaptureWithinChanged(args);
		RaiseDependencyPropertyChanged(IsStylusCaptureWithinChangedKey, args);
	}

	private static void IsKeyboardFocused_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((UIElement)d).RaiseIsKeyboardFocusedChanged(e);
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.UIElement.IsKeyboardFocusedChanged" /> event is raised on this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> that contains the event data.</param>
	protected virtual void OnIsKeyboardFocusedChanged(DependencyPropertyChangedEventArgs e)
	{
	}

	private void RaiseIsKeyboardFocusedChanged(DependencyPropertyChangedEventArgs args)
	{
		OnIsKeyboardFocusedChanged(args);
		RaiseDependencyPropertyChanged(IsKeyboardFocusedChangedKey, args);
	}

	internal bool ReadFlag(CoreFlags field)
	{
		return (_flags & field) != 0;
	}

	internal void WriteFlag(CoreFlags field, bool value)
	{
		if (value)
		{
			_flags |= field;
		}
		else
		{
			_flags &= ~field;
		}
	}

	static UIElement()
	{
		_typeofThis = typeof(UIElement);
		PreviewMouseDownEvent = Mouse.PreviewMouseDownEvent.AddOwner(_typeofThis);
		MouseDownEvent = Mouse.MouseDownEvent.AddOwner(_typeofThis);
		PreviewMouseUpEvent = Mouse.PreviewMouseUpEvent.AddOwner(_typeofThis);
		MouseUpEvent = Mouse.MouseUpEvent.AddOwner(_typeofThis);
		PreviewMouseLeftButtonDownEvent = EventManager.RegisterRoutedEvent("PreviewMouseLeftButtonDown", RoutingStrategy.Direct, typeof(MouseButtonEventHandler), _typeofThis);
		MouseLeftButtonDownEvent = EventManager.RegisterRoutedEvent("MouseLeftButtonDown", RoutingStrategy.Direct, typeof(MouseButtonEventHandler), _typeofThis);
		PreviewMouseLeftButtonUpEvent = EventManager.RegisterRoutedEvent("PreviewMouseLeftButtonUp", RoutingStrategy.Direct, typeof(MouseButtonEventHandler), _typeofThis);
		MouseLeftButtonUpEvent = EventManager.RegisterRoutedEvent("MouseLeftButtonUp", RoutingStrategy.Direct, typeof(MouseButtonEventHandler), _typeofThis);
		PreviewMouseRightButtonDownEvent = EventManager.RegisterRoutedEvent("PreviewMouseRightButtonDown", RoutingStrategy.Direct, typeof(MouseButtonEventHandler), _typeofThis);
		MouseRightButtonDownEvent = EventManager.RegisterRoutedEvent("MouseRightButtonDown", RoutingStrategy.Direct, typeof(MouseButtonEventHandler), _typeofThis);
		PreviewMouseRightButtonUpEvent = EventManager.RegisterRoutedEvent("PreviewMouseRightButtonUp", RoutingStrategy.Direct, typeof(MouseButtonEventHandler), _typeofThis);
		MouseRightButtonUpEvent = EventManager.RegisterRoutedEvent("MouseRightButtonUp", RoutingStrategy.Direct, typeof(MouseButtonEventHandler), _typeofThis);
		PreviewMouseMoveEvent = Mouse.PreviewMouseMoveEvent.AddOwner(_typeofThis);
		MouseMoveEvent = Mouse.MouseMoveEvent.AddOwner(_typeofThis);
		PreviewMouseWheelEvent = Mouse.PreviewMouseWheelEvent.AddOwner(_typeofThis);
		MouseWheelEvent = Mouse.MouseWheelEvent.AddOwner(_typeofThis);
		MouseEnterEvent = Mouse.MouseEnterEvent.AddOwner(_typeofThis);
		MouseLeaveEvent = Mouse.MouseLeaveEvent.AddOwner(_typeofThis);
		GotMouseCaptureEvent = Mouse.GotMouseCaptureEvent.AddOwner(_typeofThis);
		LostMouseCaptureEvent = Mouse.LostMouseCaptureEvent.AddOwner(_typeofThis);
		QueryCursorEvent = Mouse.QueryCursorEvent.AddOwner(_typeofThis);
		PreviewStylusDownEvent = Stylus.PreviewStylusDownEvent.AddOwner(_typeofThis);
		StylusDownEvent = Stylus.StylusDownEvent.AddOwner(_typeofThis);
		PreviewStylusUpEvent = Stylus.PreviewStylusUpEvent.AddOwner(_typeofThis);
		StylusUpEvent = Stylus.StylusUpEvent.AddOwner(_typeofThis);
		PreviewStylusMoveEvent = Stylus.PreviewStylusMoveEvent.AddOwner(_typeofThis);
		StylusMoveEvent = Stylus.StylusMoveEvent.AddOwner(_typeofThis);
		PreviewStylusInAirMoveEvent = Stylus.PreviewStylusInAirMoveEvent.AddOwner(_typeofThis);
		StylusInAirMoveEvent = Stylus.StylusInAirMoveEvent.AddOwner(_typeofThis);
		StylusEnterEvent = Stylus.StylusEnterEvent.AddOwner(_typeofThis);
		StylusLeaveEvent = Stylus.StylusLeaveEvent.AddOwner(_typeofThis);
		PreviewStylusInRangeEvent = Stylus.PreviewStylusInRangeEvent.AddOwner(_typeofThis);
		StylusInRangeEvent = Stylus.StylusInRangeEvent.AddOwner(_typeofThis);
		PreviewStylusOutOfRangeEvent = Stylus.PreviewStylusOutOfRangeEvent.AddOwner(_typeofThis);
		StylusOutOfRangeEvent = Stylus.StylusOutOfRangeEvent.AddOwner(_typeofThis);
		PreviewStylusSystemGestureEvent = Stylus.PreviewStylusSystemGestureEvent.AddOwner(_typeofThis);
		StylusSystemGestureEvent = Stylus.StylusSystemGestureEvent.AddOwner(_typeofThis);
		GotStylusCaptureEvent = Stylus.GotStylusCaptureEvent.AddOwner(_typeofThis);
		LostStylusCaptureEvent = Stylus.LostStylusCaptureEvent.AddOwner(_typeofThis);
		StylusButtonDownEvent = Stylus.StylusButtonDownEvent.AddOwner(_typeofThis);
		StylusButtonUpEvent = Stylus.StylusButtonUpEvent.AddOwner(_typeofThis);
		PreviewStylusButtonDownEvent = Stylus.PreviewStylusButtonDownEvent.AddOwner(_typeofThis);
		PreviewStylusButtonUpEvent = Stylus.PreviewStylusButtonUpEvent.AddOwner(_typeofThis);
		PreviewKeyDownEvent = Keyboard.PreviewKeyDownEvent.AddOwner(_typeofThis);
		KeyDownEvent = Keyboard.KeyDownEvent.AddOwner(_typeofThis);
		PreviewKeyUpEvent = Keyboard.PreviewKeyUpEvent.AddOwner(_typeofThis);
		KeyUpEvent = Keyboard.KeyUpEvent.AddOwner(_typeofThis);
		PreviewGotKeyboardFocusEvent = Keyboard.PreviewGotKeyboardFocusEvent.AddOwner(_typeofThis);
		GotKeyboardFocusEvent = Keyboard.GotKeyboardFocusEvent.AddOwner(_typeofThis);
		PreviewLostKeyboardFocusEvent = Keyboard.PreviewLostKeyboardFocusEvent.AddOwner(_typeofThis);
		LostKeyboardFocusEvent = Keyboard.LostKeyboardFocusEvent.AddOwner(_typeofThis);
		PreviewTextInputEvent = TextCompositionManager.PreviewTextInputEvent.AddOwner(_typeofThis);
		TextInputEvent = TextCompositionManager.TextInputEvent.AddOwner(_typeofThis);
		PreviewQueryContinueDragEvent = DragDrop.PreviewQueryContinueDragEvent.AddOwner(_typeofThis);
		QueryContinueDragEvent = DragDrop.QueryContinueDragEvent.AddOwner(_typeofThis);
		PreviewGiveFeedbackEvent = DragDrop.PreviewGiveFeedbackEvent.AddOwner(_typeofThis);
		GiveFeedbackEvent = DragDrop.GiveFeedbackEvent.AddOwner(_typeofThis);
		PreviewDragEnterEvent = DragDrop.PreviewDragEnterEvent.AddOwner(_typeofThis);
		DragEnterEvent = DragDrop.DragEnterEvent.AddOwner(_typeofThis);
		PreviewDragOverEvent = DragDrop.PreviewDragOverEvent.AddOwner(_typeofThis);
		DragOverEvent = DragDrop.DragOverEvent.AddOwner(_typeofThis);
		PreviewDragLeaveEvent = DragDrop.PreviewDragLeaveEvent.AddOwner(_typeofThis);
		DragLeaveEvent = DragDrop.DragLeaveEvent.AddOwner(_typeofThis);
		PreviewDropEvent = DragDrop.PreviewDropEvent.AddOwner(_typeofThis);
		DropEvent = DragDrop.DropEvent.AddOwner(_typeofThis);
		PreviewTouchDownEvent = Touch.PreviewTouchDownEvent.AddOwner(_typeofThis);
		TouchDownEvent = Touch.TouchDownEvent.AddOwner(_typeofThis);
		PreviewTouchMoveEvent = Touch.PreviewTouchMoveEvent.AddOwner(_typeofThis);
		TouchMoveEvent = Touch.TouchMoveEvent.AddOwner(_typeofThis);
		PreviewTouchUpEvent = Touch.PreviewTouchUpEvent.AddOwner(_typeofThis);
		TouchUpEvent = Touch.TouchUpEvent.AddOwner(_typeofThis);
		GotTouchCaptureEvent = Touch.GotTouchCaptureEvent.AddOwner(_typeofThis);
		LostTouchCaptureEvent = Touch.LostTouchCaptureEvent.AddOwner(_typeofThis);
		TouchEnterEvent = Touch.TouchEnterEvent.AddOwner(_typeofThis);
		TouchLeaveEvent = Touch.TouchLeaveEvent.AddOwner(_typeofThis);
		IsMouseDirectlyOverPropertyKey = DependencyProperty.RegisterReadOnly("IsMouseDirectlyOver", typeof(bool), _typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox, IsMouseDirectlyOver_Changed));
		IsMouseDirectlyOverProperty = IsMouseDirectlyOverPropertyKey.DependencyProperty;
		IsMouseDirectlyOverChangedKey = new EventPrivateKey();
		IsMouseOverPropertyKey = DependencyProperty.RegisterReadOnly("IsMouseOver", typeof(bool), _typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox));
		IsMouseOverProperty = IsMouseOverPropertyKey.DependencyProperty;
		IsStylusOverPropertyKey = DependencyProperty.RegisterReadOnly("IsStylusOver", typeof(bool), _typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox));
		IsStylusOverProperty = IsStylusOverPropertyKey.DependencyProperty;
		IsKeyboardFocusWithinPropertyKey = DependencyProperty.RegisterReadOnly("IsKeyboardFocusWithin", typeof(bool), _typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox));
		IsKeyboardFocusWithinProperty = IsKeyboardFocusWithinPropertyKey.DependencyProperty;
		IsKeyboardFocusWithinChangedKey = new EventPrivateKey();
		IsMouseCapturedPropertyKey = DependencyProperty.RegisterReadOnly("IsMouseCaptured", typeof(bool), _typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox, IsMouseCaptured_Changed));
		IsMouseCapturedProperty = IsMouseCapturedPropertyKey.DependencyProperty;
		IsMouseCapturedChangedKey = new EventPrivateKey();
		IsMouseCaptureWithinPropertyKey = DependencyProperty.RegisterReadOnly("IsMouseCaptureWithin", typeof(bool), _typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox));
		IsMouseCaptureWithinProperty = IsMouseCaptureWithinPropertyKey.DependencyProperty;
		IsMouseCaptureWithinChangedKey = new EventPrivateKey();
		IsStylusDirectlyOverPropertyKey = DependencyProperty.RegisterReadOnly("IsStylusDirectlyOver", typeof(bool), _typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox, IsStylusDirectlyOver_Changed));
		IsStylusDirectlyOverProperty = IsStylusDirectlyOverPropertyKey.DependencyProperty;
		IsStylusDirectlyOverChangedKey = new EventPrivateKey();
		IsStylusCapturedPropertyKey = DependencyProperty.RegisterReadOnly("IsStylusCaptured", typeof(bool), _typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox, IsStylusCaptured_Changed));
		IsStylusCapturedProperty = IsStylusCapturedPropertyKey.DependencyProperty;
		IsStylusCapturedChangedKey = new EventPrivateKey();
		IsStylusCaptureWithinPropertyKey = DependencyProperty.RegisterReadOnly("IsStylusCaptureWithin", typeof(bool), _typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox));
		IsStylusCaptureWithinProperty = IsStylusCaptureWithinPropertyKey.DependencyProperty;
		IsStylusCaptureWithinChangedKey = new EventPrivateKey();
		IsKeyboardFocusedPropertyKey = DependencyProperty.RegisterReadOnly("IsKeyboardFocused", typeof(bool), _typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox, IsKeyboardFocused_Changed));
		IsKeyboardFocusedProperty = IsKeyboardFocusedPropertyKey.DependencyProperty;
		IsKeyboardFocusedChangedKey = new EventPrivateKey();
		AreAnyTouchesDirectlyOverPropertyKey = DependencyProperty.RegisterReadOnly("AreAnyTouchesDirectlyOver", typeof(bool), _typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox));
		AreAnyTouchesDirectlyOverProperty = AreAnyTouchesDirectlyOverPropertyKey.DependencyProperty;
		AreAnyTouchesOverPropertyKey = DependencyProperty.RegisterReadOnly("AreAnyTouchesOver", typeof(bool), _typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox));
		AreAnyTouchesOverProperty = AreAnyTouchesOverPropertyKey.DependencyProperty;
		AreAnyTouchesCapturedPropertyKey = DependencyProperty.RegisterReadOnly("AreAnyTouchesCaptured", typeof(bool), _typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox));
		AreAnyTouchesCapturedProperty = AreAnyTouchesCapturedPropertyKey.DependencyProperty;
		AreAnyTouchesCapturedWithinPropertyKey = DependencyProperty.RegisterReadOnly("AreAnyTouchesCapturedWithin", typeof(bool), _typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox));
		AreAnyTouchesCapturedWithinProperty = AreAnyTouchesCapturedWithinPropertyKey.DependencyProperty;
		AllowDropProperty = DependencyProperty.Register("AllowDrop", typeof(bool), typeof(UIElement), new PropertyMetadata(BooleanBoxes.FalseBox));
		RenderTransformProperty = DependencyProperty.Register("RenderTransform", typeof(Transform), typeof(UIElement), new PropertyMetadata(Transform.Identity, RenderTransform_Changed));
		RenderTransformOriginProperty = DependencyProperty.Register("RenderTransformOrigin", typeof(Point), typeof(UIElement), new PropertyMetadata(new Point(0.0, 0.0), RenderTransformOrigin_Changed), IsRenderTransformOriginValid);
		OpacityProperty = DependencyProperty.Register("Opacity", typeof(double), typeof(UIElement), new UIPropertyMetadata(1.0, Opacity_Changed));
		OpacityMaskProperty = DependencyProperty.Register("OpacityMask", typeof(Brush), typeof(UIElement), new UIPropertyMetadata(OpacityMask_Changed));
		BitmapEffectProperty = DependencyProperty.Register("BitmapEffect", typeof(BitmapEffect), typeof(UIElement), new UIPropertyMetadata(OnBitmapEffectChanged));
		EffectProperty = DependencyProperty.Register("Effect", typeof(Effect), typeof(UIElement), new UIPropertyMetadata(OnEffectChanged));
		BitmapEffectInputProperty = DependencyProperty.Register("BitmapEffectInput", typeof(BitmapEffectInput), typeof(UIElement), new UIPropertyMetadata(OnBitmapEffectInputChanged));
		CacheModeProperty = DependencyProperty.Register("CacheMode", typeof(CacheMode), typeof(UIElement), new UIPropertyMetadata(OnCacheModeChanged));
		UidProperty = DependencyProperty.Register("Uid", typeof(string), typeof(UIElement), new UIPropertyMetadata(string.Empty));
		VisibilityProperty = DependencyProperty.Register("Visibility", typeof(Visibility), typeof(UIElement), new PropertyMetadata(VisibilityBoxes.VisibleBox, OnVisibilityChanged), ValidateVisibility);
		ClipToBoundsProperty = DependencyProperty.Register("ClipToBounds", typeof(bool), typeof(UIElement), new PropertyMetadata(BooleanBoxes.FalseBox, ClipToBounds_Changed));
		ClipProperty = DependencyProperty.Register("Clip", typeof(Geometry), typeof(UIElement), new PropertyMetadata(null, Clip_Changed));
		SnapsToDevicePixelsProperty = DependencyProperty.Register("SnapsToDevicePixels", typeof(bool), typeof(UIElement), new PropertyMetadata(BooleanBoxes.FalseBox, SnapsToDevicePixels_Changed));
		GotFocusEvent = FocusManager.GotFocusEvent.AddOwner(typeof(UIElement));
		LostFocusEvent = FocusManager.LostFocusEvent.AddOwner(typeof(UIElement));
		IsFocusedPropertyKey = DependencyProperty.RegisterReadOnly("IsFocused", typeof(bool), typeof(UIElement), new PropertyMetadata(BooleanBoxes.FalseBox, IsFocused_Changed));
		IsFocusedProperty = IsFocusedPropertyKey.DependencyProperty;
		IsEnabledProperty = DependencyProperty.Register("IsEnabled", typeof(bool), typeof(UIElement), new UIPropertyMetadata(BooleanBoxes.TrueBox, OnIsEnabledChanged, CoerceIsEnabled));
		IsEnabledChangedKey = new EventPrivateKey();
		IsHitTestVisibleProperty = DependencyProperty.Register("IsHitTestVisible", typeof(bool), typeof(UIElement), new UIPropertyMetadata(BooleanBoxes.TrueBox, OnIsHitTestVisibleChanged, CoerceIsHitTestVisible));
		IsHitTestVisibleChangedKey = new EventPrivateKey();
		_isVisibleMetadata = new ReadOnlyPropertyMetadata(BooleanBoxes.FalseBox, GetIsVisible, OnIsVisibleChanged);
		IsVisiblePropertyKey = DependencyProperty.RegisterReadOnly("IsVisible", typeof(bool), typeof(UIElement), _isVisibleMetadata);
		IsVisibleProperty = IsVisiblePropertyKey.DependencyProperty;
		IsVisibleChangedKey = new EventPrivateKey();
		FocusableProperty = DependencyProperty.Register("Focusable", typeof(bool), typeof(UIElement), new UIPropertyMetadata(BooleanBoxes.FalseBox, OnFocusableChanged));
		FocusableChangedKey = new EventPrivateKey();
		IsManipulationEnabledProperty = DependencyProperty.Register("IsManipulationEnabled", typeof(bool), typeof(UIElement), new PropertyMetadata(BooleanBoxes.FalseBox, OnIsManipulationEnabledChanged));
		ManipulationStartingEvent = Manipulation.ManipulationStartingEvent.AddOwner(typeof(UIElement));
		ManipulationStartedEvent = Manipulation.ManipulationStartedEvent.AddOwner(typeof(UIElement));
		ManipulationDeltaEvent = Manipulation.ManipulationDeltaEvent.AddOwner(typeof(UIElement));
		ManipulationInertiaStartingEvent = Manipulation.ManipulationInertiaStartingEvent.AddOwner(typeof(UIElement));
		ManipulationBoundaryFeedbackEvent = Manipulation.ManipulationBoundaryFeedbackEvent.AddOwner(typeof(UIElement));
		ManipulationCompletedEvent = Manipulation.ManipulationCompletedEvent.AddOwner(typeof(UIElement));
		DpiScaleXValues = new List<double>(3);
		DpiScaleYValues = new List<double>(3);
		DpiLock = new object();
		_dpiScaleX = 1.0;
		_dpiScaleY = 1.0;
		_setDpi = true;
		EventHandlersStoreField = new UncommonField<EventHandlersStore>();
		InputBindingCollectionField = new UncommonField<InputBindingCollection>();
		CommandBindingCollectionField = new UncommonField<CommandBindingCollection>();
		LayoutUpdatedListItemsField = new UncommonField<object>();
		LayoutUpdatedHandlersField = new UncommonField<EventHandler>();
		StylusPlugInsField = new UncommonField<StylusPlugInCollection>();
		AutomationPeerField = new UncommonField<AutomationPeer>();
		_positionAndSizeOfSetController = new UncommonField<WeakReference<UIElement>>();
		AutomationNotSupportedByDefaultField = new UncommonField<bool>();
		FocusWithinProperty = new FocusWithinProperty();
		MouseOverProperty = new MouseOverProperty();
		MouseCaptureWithinProperty = new MouseCaptureWithinProperty();
		StylusOverProperty = new StylusOverProperty();
		StylusCaptureWithinProperty = new StylusCaptureWithinProperty();
		TouchesOverProperty = new TouchesOverProperty();
		TouchesCapturedWithinProperty = new TouchesCapturedWithinProperty();
		RegisterEvents(typeof(UIElement));
		RenderOptions.EdgeModeProperty.OverrideMetadata(typeof(UIElement), new UIPropertyMetadata(EdgeMode_Changed));
		RenderOptions.BitmapScalingModeProperty.OverrideMetadata(typeof(UIElement), new UIPropertyMetadata(BitmapScalingMode_Changed));
		RenderOptions.ClearTypeHintProperty.OverrideMetadata(typeof(UIElement), new UIPropertyMetadata(ClearTypeHint_Changed));
		TextOptionsInternal.TextHintingModeProperty.OverrideMetadata(typeof(UIElement), new UIPropertyMetadata(TextHintingMode_Changed));
		EventManager.RegisterClassHandler(typeof(UIElement), ManipulationStartingEvent, new EventHandler<ManipulationStartingEventArgs>(OnManipulationStartingThunk));
		EventManager.RegisterClassHandler(typeof(UIElement), ManipulationStartedEvent, new EventHandler<ManipulationStartedEventArgs>(OnManipulationStartedThunk));
		EventManager.RegisterClassHandler(typeof(UIElement), ManipulationDeltaEvent, new EventHandler<ManipulationDeltaEventArgs>(OnManipulationDeltaThunk));
		EventManager.RegisterClassHandler(typeof(UIElement), ManipulationInertiaStartingEvent, new EventHandler<ManipulationInertiaStartingEventArgs>(OnManipulationInertiaStartingThunk));
		EventManager.RegisterClassHandler(typeof(UIElement), ManipulationBoundaryFeedbackEvent, new EventHandler<ManipulationBoundaryFeedbackEventArgs>(OnManipulationBoundaryFeedbackThunk));
		EventManager.RegisterClassHandler(typeof(UIElement), ManipulationCompletedEvent, new EventHandler<ManipulationCompletedEventArgs>(OnManipulationCompletedThunk));
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.UIElement" /> class. </summary>
	public UIElement()
	{
		Initialize();
	}

	private void Initialize()
	{
		BeginPropertyInitialization();
		NeverMeasured = true;
		NeverArranged = true;
		SnapsToDevicePixelsCache = (bool)SnapsToDevicePixelsProperty.GetDefaultValue(base.DependencyObjectType);
		ClipToBoundsCache = (bool)ClipToBoundsProperty.GetDefaultValue(base.DependencyObjectType);
		VisibilityCache = (Visibility)VisibilityProperty.GetDefaultValue(base.DependencyObjectType);
		SetFlags(value: true, VisualFlags.IsUIElement);
		if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Verbose))
		{
			PerfService.GetPerfElementID(this);
		}
	}

	private bool IsRenderable()
	{
		if (NeverMeasured || NeverArranged)
		{
			return false;
		}
		if (ReadFlag(CoreFlags.IsCollapsed))
		{
			return false;
		}
		if (IsMeasureValid)
		{
			return IsArrangeValid;
		}
		return false;
	}

	internal void InvalidateMeasureInternal()
	{
		MeasureDirty = true;
	}

	internal void InvalidateArrangeInternal()
	{
		ArrangeDirty = true;
	}

	/// <summary>Invalidates the measurement state (layout) for the element. </summary>
	public void InvalidateMeasure()
	{
		if (MeasureDirty || MeasureInProgress)
		{
			return;
		}
		if (!NeverMeasured)
		{
			ContextLayoutManager contextLayoutManager = ContextLayoutManager.From(base.Dispatcher);
			if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordLayout, EventTrace.Level.Verbose) && contextLayoutManager.MeasureQueue.IsEmpty)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientLayoutInvalidated, EventTrace.Keyword.KeywordLayout, EventTrace.Level.Verbose, PerfService.GetPerfElementID(this));
			}
			contextLayoutManager.MeasureQueue.Add(this);
		}
		MeasureDirty = true;
	}

	/// <summary>Invalidates the arrange state (layout) for the element. After the invalidation, the element will have its layout updated, which will occur asynchronously unless subsequently forced by <see cref="M:System.Windows.UIElement.UpdateLayout" />. </summary>
	public void InvalidateArrange()
	{
		if (!ArrangeDirty && !ArrangeInProgress)
		{
			if (!NeverArranged)
			{
				ContextLayoutManager.From(base.Dispatcher).ArrangeQueue.Add(this);
			}
			ArrangeDirty = true;
		}
	}

	/// <summary>Invalidates the rendering of the element, and forces a complete new layout pass. <see cref="M:System.Windows.UIElement.OnRender(System.Windows.Media.DrawingContext)" /> is called after the layout cycle is completed. </summary>
	public void InvalidateVisual()
	{
		InvalidateArrange();
		RenderingInvalidated = true;
	}

	/// <summary>Supports layout behavior when a child element is resized. </summary>
	/// <param name="child">The child element that is being resized.</param>
	protected virtual void OnChildDesiredSizeChanged(UIElement child)
	{
		if (IsMeasureValid)
		{
			InvalidateMeasure();
		}
	}

	private void addLayoutUpdatedHandler(EventHandler handler, LayoutEventList.ListItem item)
	{
		object value = LayoutUpdatedListItemsField.GetValue(this);
		if (value == null)
		{
			LayoutUpdatedListItemsField.SetValue(this, item);
			LayoutUpdatedHandlersField.SetValue(this, handler);
			return;
		}
		EventHandler value2 = LayoutUpdatedHandlersField.GetValue(this);
		if (value2 != null)
		{
			Hashtable hashtable = new Hashtable(2);
			hashtable.Add(value2, value);
			hashtable.Add(handler, item);
			LayoutUpdatedHandlersField.ClearValue(this);
			LayoutUpdatedListItemsField.SetValue(this, hashtable);
		}
		else
		{
			((Hashtable)value).Add(handler, item);
		}
	}

	private LayoutEventList.ListItem getLayoutUpdatedHandler(EventHandler d)
	{
		object value = LayoutUpdatedListItemsField.GetValue(this);
		if (value == null)
		{
			return null;
		}
		EventHandler value2 = LayoutUpdatedHandlersField.GetValue(this);
		if (value2 != null)
		{
			if (value2 == d)
			{
				return (LayoutEventList.ListItem)value;
			}
			return null;
		}
		return (LayoutEventList.ListItem)((Hashtable)value)[d];
	}

	private void removeLayoutUpdatedHandler(EventHandler d)
	{
		object value = LayoutUpdatedListItemsField.GetValue(this);
		EventHandler value2 = LayoutUpdatedHandlersField.GetValue(this);
		if (value2 != null)
		{
			if (value2 == d)
			{
				LayoutUpdatedListItemsField.ClearValue(this);
				LayoutUpdatedHandlersField.ClearValue(this);
			}
		}
		else
		{
			((Hashtable)value).Remove(d);
		}
	}

	internal static void PropagateSuspendLayout(Visual v)
	{
		if (v.CheckFlagsAnd(VisualFlags.IsLayoutIslandRoot) || v.CheckFlagsAnd(VisualFlags.IsLayoutSuspended))
		{
			return;
		}
		if (Invariant.Strict && v.CheckFlagsAnd(VisualFlags.IsUIElement))
		{
			UIElement uIElement = (UIElement)v;
			Invariant.Assert(!uIElement.MeasureInProgress && !uIElement.ArrangeInProgress);
		}
		v.SetFlags(value: true, VisualFlags.IsLayoutSuspended);
		v.TreeLevel = 0u;
		int internalVisualChildrenCount = v.InternalVisualChildrenCount;
		for (int i = 0; i < internalVisualChildrenCount; i++)
		{
			Visual visual = v.InternalGetVisualChild(i);
			if (visual != null)
			{
				PropagateSuspendLayout(visual);
			}
		}
	}

	internal static void PropagateResumeLayout(Visual parent, Visual v)
	{
		if (v.CheckFlagsAnd(VisualFlags.IsLayoutIslandRoot))
		{
			return;
		}
		bool num = parent?.CheckFlagsAnd(VisualFlags.IsLayoutSuspended) ?? false;
		uint num2 = parent?.TreeLevel ?? 0;
		if (num)
		{
			return;
		}
		v.SetFlags(value: false, VisualFlags.IsLayoutSuspended);
		v.TreeLevel = num2 + 1;
		if (v.CheckFlagsAnd(VisualFlags.IsUIElement))
		{
			UIElement uIElement = (UIElement)v;
			Invariant.Assert(!uIElement.MeasureInProgress && !uIElement.ArrangeInProgress);
			bool num3 = uIElement.MeasureDirty && !uIElement.NeverMeasured && uIElement.MeasureRequest == null;
			bool flag = uIElement.ArrangeDirty && !uIElement.NeverArranged && uIElement.ArrangeRequest == null;
			ContextLayoutManager contextLayoutManager = ((num3 || flag) ? ContextLayoutManager.From(uIElement.Dispatcher) : null);
			if (num3)
			{
				contextLayoutManager.MeasureQueue.Add(uIElement);
			}
			if (flag)
			{
				contextLayoutManager.ArrangeQueue.Add(uIElement);
			}
		}
		int internalVisualChildrenCount = v.InternalVisualChildrenCount;
		for (int i = 0; i < internalVisualChildrenCount; i++)
		{
			Visual visual = v.InternalGetVisualChild(i);
			if (visual != null)
			{
				PropagateResumeLayout(v, visual);
			}
		}
	}

	/// <summary>Updates the <see cref="P:System.Windows.UIElement.DesiredSize" /> of a <see cref="T:System.Windows.UIElement" />. Parent elements call this method from their own <see cref="M:System.Windows.UIElement.MeasureCore(System.Windows.Size)" /> implementations to form a recursive layout update. Calling this method constitutes the first pass (the "Measure" pass) of a layout update. </summary>
	/// <param name="availableSize">The available space that a parent element can allocate a child element. A child element can request a larger space than what is available; the provided size might be accommodated if scrolling is possible in the content model for the current element.</param>
	public void Measure(Size availableSize)
	{
		bool flag = false;
		long num = 0L;
		if (ContextLayoutManager.From(base.Dispatcher).AutomationEvents.Count != 0)
		{
			UIElementHelper.InvalidateAutomationAncestors(this);
		}
		if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordLayout, EventTrace.Level.Verbose))
		{
			num = PerfService.GetPerfElementID(this);
			flag = true;
			EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientMeasureElementBegin, EventTrace.Keyword.KeywordLayout, EventTrace.Level.Verbose, num, availableSize.Width, availableSize.Height);
		}
		try
		{
			using (base.Dispatcher.DisableProcessing())
			{
				if (double.IsNaN(availableSize.Width) || double.IsNaN(availableSize.Height))
				{
					throw new InvalidOperationException(SR.UIElement_Layout_NaNMeasure);
				}
				bool neverMeasured = NeverMeasured;
				if (neverMeasured)
				{
					switchVisibilityIfNeeded(Visibility);
					pushVisualEffects();
				}
				bool flag2 = DoubleUtil.AreClose(availableSize, _previousAvailableSize);
				if (Visibility == Visibility.Collapsed || CheckFlagsAnd(VisualFlags.IsLayoutSuspended))
				{
					if (MeasureRequest != null)
					{
						ContextLayoutManager.From(base.Dispatcher).MeasureQueue.Remove(this);
					}
					if (!flag2)
					{
						InvalidateMeasureInternal();
						_previousAvailableSize = availableSize;
					}
				}
				else
				{
					if (IsMeasureValid && !neverMeasured && flag2)
					{
						return;
					}
					NeverMeasured = false;
					Size desiredSize = _desiredSize;
					InvalidateArrange();
					MeasureInProgress = true;
					Size size = new Size(0.0, 0.0);
					ContextLayoutManager contextLayoutManager = ContextLayoutManager.From(base.Dispatcher);
					bool flag3 = true;
					try
					{
						contextLayoutManager.EnterMeasure();
						size = MeasureCore(availableSize);
						flag3 = false;
					}
					finally
					{
						MeasureInProgress = false;
						_previousAvailableSize = availableSize;
						contextLayoutManager.ExitMeasure();
						if (flag3 && contextLayoutManager.GetLastExceptionElement() == null)
						{
							contextLayoutManager.SetLastExceptionElement(this);
						}
					}
					if (double.IsPositiveInfinity(size.Width) || double.IsPositiveInfinity(size.Height))
					{
						throw new InvalidOperationException(SR.Format(SR.UIElement_Layout_PositiveInfinityReturned, GetType().FullName));
					}
					if (double.IsNaN(size.Width) || double.IsNaN(size.Height))
					{
						throw new InvalidOperationException(SR.Format(SR.UIElement_Layout_NaNReturned, GetType().FullName));
					}
					MeasureDirty = false;
					if (MeasureRequest != null)
					{
						ContextLayoutManager.From(base.Dispatcher).MeasureQueue.Remove(this);
					}
					_desiredSize = size;
					if (!MeasureDuringArrange && !DoubleUtil.AreClose(desiredSize, size))
					{
						GetUIParentOrICH(out var uiParent, out var ich);
						if (uiParent != null && !uiParent.MeasureInProgress)
						{
							uiParent.OnChildDesiredSizeChanged(this);
						}
						else
						{
							ich?.OnChildDesiredSizeChanged(this);
						}
					}
				}
			}
		}
		finally
		{
			if (flag)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientMeasureElementEnd, EventTrace.Keyword.KeywordLayout, EventTrace.Level.Verbose, num, _desiredSize.Width, _desiredSize.Height);
			}
		}
	}

	internal void GetUIParentOrICH(out UIElement uiParent, out IContentHost ich)
	{
		ich = null;
		uiParent = null;
		Visual visual = VisualTreeHelper.GetParent(this) as Visual;
		while (visual != null)
		{
			ich = visual as IContentHost;
			if (ich == null)
			{
				if (visual.CheckFlagsAnd(VisualFlags.IsUIElement))
				{
					uiParent = (UIElement)visual;
					break;
				}
				visual = VisualTreeHelper.GetParent(visual) as Visual;
				continue;
			}
			break;
		}
	}

	internal UIElement GetUIParentWithinLayoutIsland()
	{
		UIElement result = null;
		Visual visual = VisualTreeHelper.GetParent(this) as Visual;
		while (visual != null && !visual.CheckFlagsAnd(VisualFlags.IsLayoutIslandRoot))
		{
			if (visual.CheckFlagsAnd(VisualFlags.IsUIElement))
			{
				result = (UIElement)visual;
				break;
			}
			visual = VisualTreeHelper.GetParent(visual) as Visual;
		}
		return result;
	}

	/// <summary>Positions child elements and determines a size for a <see cref="T:System.Windows.UIElement" />. Parent elements call this method from their <see cref="M:System.Windows.UIElement.ArrangeCore(System.Windows.Rect)" /> implementation (or a WPF framework-level equivalent) to form a recursive layout update. This method constitutes the second pass of a layout update. </summary>
	/// <param name="finalRect">The final size that the parent computes for the child element, provided as a <see cref="T:System.Windows.Rect" /> instance.</param>
	public void Arrange(Rect finalRect)
	{
		bool flag = false;
		long num = 0L;
		if (ContextLayoutManager.From(base.Dispatcher).AutomationEvents.Count != 0)
		{
			UIElementHelper.InvalidateAutomationAncestors(this);
		}
		if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordLayout, EventTrace.Level.Verbose))
		{
			num = PerfService.GetPerfElementID(this);
			flag = true;
			EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientArrangeElementBegin, EventTrace.Keyword.KeywordLayout, EventTrace.Level.Verbose, num, finalRect.Top, finalRect.Left, finalRect.Width, finalRect.Height);
		}
		try
		{
			using (base.Dispatcher.DisableProcessing())
			{
				if (double.IsPositiveInfinity(finalRect.Width) || double.IsPositiveInfinity(finalRect.Height) || double.IsNaN(finalRect.Width) || double.IsNaN(finalRect.Height))
				{
					DependencyObject dependencyObject = GetUIParent() as UIElement;
					throw new InvalidOperationException(SR.Format(SR.UIElement_Layout_InfinityArrange, (dependencyObject == null) ? "" : dependencyObject.GetType().FullName, GetType().FullName));
				}
				if (Visibility == Visibility.Collapsed || CheckFlagsAnd(VisualFlags.IsLayoutSuspended))
				{
					if (ArrangeRequest != null)
					{
						ContextLayoutManager.From(base.Dispatcher).ArrangeQueue.Remove(this);
					}
					_finalRect = finalRect;
					return;
				}
				if (MeasureDirty || NeverMeasured)
				{
					try
					{
						MeasureDuringArrange = true;
						if (NeverMeasured)
						{
							Measure(finalRect.Size);
						}
						else
						{
							Measure(PreviousConstraint);
						}
					}
					finally
					{
						MeasureDuringArrange = false;
					}
				}
				if (IsArrangeValid && !NeverArranged && DoubleUtil.AreClose(finalRect, _finalRect))
				{
					return;
				}
				bool neverArranged = NeverArranged;
				NeverArranged = false;
				ArrangeInProgress = true;
				ContextLayoutManager contextLayoutManager = ContextLayoutManager.From(base.Dispatcher);
				Size renderSize = RenderSize;
				bool flag2 = false;
				bool flag3 = true;
				if (CheckFlagsAnd(VisualFlags.UseLayoutRounding))
				{
					DpiScale dpi = GetDpi();
					finalRect = RoundLayoutRect(finalRect, dpi.DpiScaleX, dpi.DpiScaleY);
				}
				try
				{
					contextLayoutManager.EnterArrange();
					ArrangeCore(finalRect);
					ensureClip(finalRect.Size);
					flag2 = markForSizeChangedIfNeeded(renderSize, RenderSize);
					flag3 = false;
				}
				finally
				{
					ArrangeInProgress = false;
					contextLayoutManager.ExitArrange();
					if (flag3 && contextLayoutManager.GetLastExceptionElement() == null)
					{
						contextLayoutManager.SetLastExceptionElement(this);
					}
				}
				_finalRect = finalRect;
				ArrangeDirty = false;
				if (ArrangeRequest != null)
				{
					ContextLayoutManager.From(base.Dispatcher).ArrangeQueue.Remove(this);
				}
				if ((flag2 || RenderingInvalidated || neverArranged) && IsRenderable())
				{
					DrawingContext drawingContext = RenderOpen();
					try
					{
						bool flag4 = EventTrace.IsEnabled(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordGraphics, EventTrace.Level.Verbose);
						if (flag4)
						{
							EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientOnRenderBegin, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordGraphics, EventTrace.Level.Verbose, num);
						}
						try
						{
							OnRender(drawingContext);
						}
						finally
						{
							if (flag4)
							{
								EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientOnRenderEnd, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordGraphics, EventTrace.Level.Verbose, num);
							}
						}
					}
					finally
					{
						drawingContext.Close();
						RenderingInvalidated = false;
					}
					updatePixelSnappingGuidelines();
				}
				if (neverArranged)
				{
					EndPropertyInitialization();
				}
			}
		}
		finally
		{
			if (flag)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientArrangeElementEnd, EventTrace.Keyword.KeywordLayout, EventTrace.Level.Verbose, num, finalRect.Top, finalRect.Left, finalRect.Width, finalRect.Height);
			}
		}
	}

	/// <summary>When overridden in a derived class, participates in rendering operations that are directed by the layout system. The rendering instructions for this element are not used directly when this method is invoked, and are instead preserved for later asynchronous use by layout and drawing. </summary>
	/// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
	protected virtual void OnRender(DrawingContext drawingContext)
	{
	}

	private void updatePixelSnappingGuidelines()
	{
		if (!SnapsToDevicePixels || _drawingContent == null)
		{
			DoubleCollection visualXSnappingGuidelines = (base.VisualYSnappingGuidelines = null);
			base.VisualXSnappingGuidelines = visualXSnappingGuidelines;
			return;
		}
		DoubleCollection visualXSnappingGuidelines2 = base.VisualXSnappingGuidelines;
		if (visualXSnappingGuidelines2 == null)
		{
			visualXSnappingGuidelines2 = new DoubleCollection();
			visualXSnappingGuidelines2.Add(0.0);
			visualXSnappingGuidelines2.Add(RenderSize.Width);
			base.VisualXSnappingGuidelines = visualXSnappingGuidelines2;
		}
		else
		{
			int index = visualXSnappingGuidelines2.Count - 1;
			if (!DoubleUtil.AreClose(visualXSnappingGuidelines2[index], RenderSize.Width))
			{
				visualXSnappingGuidelines2[index] = RenderSize.Width;
			}
		}
		DoubleCollection visualYSnappingGuidelines = base.VisualYSnappingGuidelines;
		if (visualYSnappingGuidelines == null)
		{
			visualYSnappingGuidelines = new DoubleCollection();
			visualYSnappingGuidelines.Add(0.0);
			visualYSnappingGuidelines.Add(RenderSize.Height);
			base.VisualYSnappingGuidelines = visualYSnappingGuidelines;
		}
		else
		{
			int index2 = visualYSnappingGuidelines.Count - 1;
			if (!DoubleUtil.AreClose(visualYSnappingGuidelines[index2], RenderSize.Height))
			{
				visualYSnappingGuidelines[index2] = RenderSize.Height;
			}
		}
	}

	private bool markForSizeChangedIfNeeded(Size oldSize, Size newSize)
	{
		bool flag = !DoubleUtil.AreClose(oldSize.Width, newSize.Width);
		bool flag2 = !DoubleUtil.AreClose(oldSize.Height, newSize.Height);
		SizeChangedInfo sizeChangedInfo = this.sizeChangedInfo;
		if (sizeChangedInfo != null)
		{
			sizeChangedInfo.Update(flag, flag2);
			return true;
		}
		if (flag || flag2)
		{
			sizeChangedInfo = (this.sizeChangedInfo = new SizeChangedInfo(this, oldSize, flag, flag2));
			ContextLayoutManager.From(base.Dispatcher).AddToSizeChangedChain(sizeChangedInfo);
			Visual.PropagateFlags(this, VisualFlags.IsSubtreeDirtyForPrecompute, VisualProxyFlags.IsSubtreeDirtyForRender);
			return true;
		}
		return false;
	}

	internal static Size RoundLayoutSize(Size size, double dpiScaleX, double dpiScaleY)
	{
		return new Size(RoundLayoutValue(size.Width, dpiScaleX), RoundLayoutValue(size.Height, dpiScaleY));
	}

	internal static double RoundLayoutValue(double value, double dpiScale)
	{
		double num;
		if (!DoubleUtil.AreClose(dpiScale, 1.0))
		{
			num = Math.Round(value * dpiScale) / dpiScale;
			if (double.IsNaN(num) || double.IsInfinity(num) || DoubleUtil.AreClose(num, double.MaxValue))
			{
				num = value;
			}
		}
		else
		{
			num = Math.Round(value);
		}
		return num;
	}

	internal static Rect RoundLayoutRect(Rect rect, double dpiScaleX, double dpiScaleY)
	{
		return new Rect(RoundLayoutValue(rect.X, dpiScaleX), RoundLayoutValue(rect.Y, dpiScaleY), RoundLayoutValue(rect.Width, dpiScaleX), RoundLayoutValue(rect.Height, dpiScaleY));
	}

	internal static DpiScale EnsureDpiScale()
	{
		if (_setDpi)
		{
			_setDpi = false;
			HandleRef hWnd = new HandleRef(null, IntPtr.Zero);
			nint dC = MS.Win32.UnsafeNativeMethods.GetDC(hWnd);
			if (dC == IntPtr.Zero)
			{
				throw new Win32Exception();
			}
			try
			{
				int deviceCaps = MS.Win32.UnsafeNativeMethods.GetDeviceCaps(new HandleRef(null, dC), 88);
				int deviceCaps2 = MS.Win32.UnsafeNativeMethods.GetDeviceCaps(new HandleRef(null, dC), 90);
				_dpiScaleX = (double)deviceCaps / 96.0;
				_dpiScaleY = (double)deviceCaps2 / 96.0;
			}
			finally
			{
				MS.Win32.UnsafeNativeMethods.ReleaseDC(hWnd, new HandleRef(null, dC));
			}
		}
		return new DpiScale(_dpiScaleX, _dpiScaleY);
	}

	/// <summary>When overridden in a derived class, participates in rendering operations that are directed by the layout system. This method is invoked after layout update, and before rendering, if the element's <see cref="P:System.Windows.UIElement.RenderSize" /> has changed as a result of layout update. </summary>
	/// <param name="info">The packaged parameters (<see cref="T:System.Windows.SizeChangedInfo" />), which includes old and new sizes, and which dimension actually changes.</param>
	protected internal virtual void OnRenderSizeChanged(SizeChangedInfo info)
	{
	}

	/// <summary>When overridden in a derived class, provides measurement logic for sizing this element properly, with consideration of the size of any child element content. </summary>
	/// <returns>The desired size of this element in layout.</returns>
	/// <param name="availableSize">The available size that the parent element can allocate for the child.</param>
	protected virtual Size MeasureCore(Size availableSize)
	{
		return new Size(0.0, 0.0);
	}

	/// <summary>Defines the template for WPF core-level arrange layout definition. </summary>
	/// <param name="finalRect">The final area within the parent that element should use to arrange itself and its child elements.</param>
	protected virtual void ArrangeCore(Rect finalRect)
	{
		RenderSize = finalRect.Size;
		Transform transform = RenderTransform;
		if (transform == Transform.Identity)
		{
			transform = null;
		}
		Vector visualOffset = base.VisualOffset;
		if (!DoubleUtil.AreClose(visualOffset.X, finalRect.X) || !DoubleUtil.AreClose(visualOffset.Y, finalRect.Y))
		{
			base.VisualOffset = new Vector(finalRect.X, finalRect.Y);
		}
		TransformGroup transformGroup;
		Point renderTransformOrigin;
		int num;
		if (transform != null)
		{
			transformGroup = new TransformGroup();
			renderTransformOrigin = RenderTransformOrigin;
			if (renderTransformOrigin.X == 0.0)
			{
				num = ((renderTransformOrigin.Y != 0.0) ? 1 : 0);
				if (num == 0)
				{
					goto IL_00d8;
				}
			}
			else
			{
				num = 1;
			}
			transformGroup.Children.Add(new TranslateTransform(0.0 - finalRect.Width * renderTransformOrigin.X, 0.0 - finalRect.Height * renderTransformOrigin.Y));
			goto IL_00d8;
		}
		base.VisualTransform = null;
		return;
		IL_00d8:
		transformGroup.Children.Add(transform);
		if (num != 0)
		{
			transformGroup.Children.Add(new TranslateTransform(finalRect.Width * renderTransformOrigin.X, finalRect.Height * renderTransformOrigin.Y));
		}
		base.VisualTransform = transformGroup;
	}

	internal override Rect GetHitTestBounds()
	{
		Rect result = new Rect(_size);
		if (_drawingContent != null)
		{
			MediaContext mediaContext = MediaContext.From(base.Dispatcher);
			BoundsDrawingContextWalker ctx = mediaContext.AcquireBoundsDrawingContextWalker();
			Rect contentBounds = _drawingContent.GetContentBounds(ctx);
			mediaContext.ReleaseBoundsDrawingContextWalker(ctx);
			result.Union(contentBounds);
		}
		return result;
	}

	private static void RenderTransform_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		UIElement uIElement = (UIElement)d;
		if (!uIElement.NeverMeasured && !uIElement.NeverArranged && !e.IsASubPropertyChange)
		{
			uIElement.InvalidateArrange();
			uIElement.AreTransformsClean = false;
		}
	}

	private static bool IsRenderTransformOriginValid(object value)
	{
		Point point = (Point)value;
		if (!double.IsNaN(point.X) && !double.IsPositiveInfinity(point.X) && !double.IsNegativeInfinity(point.X))
		{
			if (!double.IsNaN(point.Y) && !double.IsPositiveInfinity(point.Y))
			{
				return !double.IsNegativeInfinity(point.Y);
			}
			return false;
		}
		return false;
	}

	private static void RenderTransformOrigin_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		UIElement uIElement = (UIElement)d;
		if (!uIElement.NeverMeasured && !uIElement.NeverArranged)
		{
			uIElement.InvalidateArrange();
			uIElement.AreTransformsClean = false;
		}
	}

	/// <summary>Invoked when the parent element of this <see cref="T:System.Windows.UIElement" /> reports a change to its underlying visual parent.</summary>
	/// <param name="oldParent">The previous parent. This may be provided as null if the <see cref="T:System.Windows.DependencyObject" /> did not have a parent element previously.</param>
	protected internal override void OnVisualParentChanged(DependencyObject oldParent)
	{
		if (_parent != null)
		{
			DependencyObject dependencyObject = _parent;
			if (!(dependencyObject is UIElement) && !(dependencyObject is UIElement3D))
			{
				if (dependencyObject is Visual visual)
				{
					visual.VisualAncestorChanged += OnVisualAncestorChanged_ForceInherit;
					dependencyObject = InputElement.GetContainingUIElement(visual);
				}
				else if (dependencyObject is Visual3D visual3D)
				{
					visual3D.VisualAncestorChanged += OnVisualAncestorChanged_ForceInherit;
					dependencyObject = InputElement.GetContainingUIElement(visual3D);
				}
			}
			if (dependencyObject != null)
			{
				SynchronizeForceInheritProperties(this, null, null, dependencyObject);
			}
		}
		else
		{
			DependencyObject dependencyObject2 = oldParent;
			if (!(dependencyObject2 is UIElement) && !(dependencyObject2 is UIElement3D))
			{
				if (oldParent is Visual)
				{
					((Visual)oldParent).VisualAncestorChanged -= OnVisualAncestorChanged_ForceInherit;
				}
				else if (oldParent is Visual3D)
				{
					((Visual3D)oldParent).VisualAncestorChanged -= OnVisualAncestorChanged_ForceInherit;
				}
				dependencyObject2 = InputElement.GetContainingUIElement(oldParent);
			}
			if (dependencyObject2 != null)
			{
				SynchronizeForceInheritProperties(this, null, null, dependencyObject2);
			}
		}
		SynchronizeReverseInheritPropertyFlags(oldParent, isCoreParent: true);
	}

	private void OnVisualAncestorChanged_ForceInherit(object sender, AncestorChangedEventArgs e)
	{
		DependencyObject dependencyObject = null;
		if (e.OldParent == null)
		{
			dependencyObject = InputElement.GetContainingUIElement(_parent);
			if (dependencyObject != null && VisualTreeHelper.IsAncestorOf(e.Ancestor, dependencyObject))
			{
				dependencyObject = null;
			}
		}
		else
		{
			dependencyObject = InputElement.GetContainingUIElement(_parent);
			dependencyObject = ((dependencyObject == null) ? InputElement.GetContainingUIElement(e.OldParent) : null);
		}
		if (dependencyObject != null)
		{
			SynchronizeForceInheritProperties(this, null, null, dependencyObject);
		}
	}

	internal void OnVisualAncestorChanged(object sender, AncestorChangedEventArgs e)
	{
		if (sender is UIElement uie)
		{
			PresentationSource.OnVisualAncestorChanged(uie, e);
		}
	}

	internal DependencyObject GetUIParent()
	{
		return UIElementHelper.GetUIParent(this, continuePastVisualTree: false);
	}

	internal DependencyObject GetUIParent(bool continuePastVisualTree)
	{
		return UIElementHelper.GetUIParent(this, continuePastVisualTree);
	}

	internal DependencyObject GetUIParentNo3DTraversal()
	{
		return InputElement.GetContainingUIElement(base.InternalVisualParent, onlyTraverse2D: true);
	}

	/// <summary>When overridden in a derived class, returns an alternative user interface (UI) parent for this element if no visual parent exists. </summary>
	/// <returns>An object, if implementation of a derived class has an alternate parent connection to report.</returns>
	protected internal virtual DependencyObject GetUIParentCore()
	{
		return null;
	}

	/// <summary>Ensures that all visual child elements of this element are properly updated for layout. </summary>
	public void UpdateLayout()
	{
		ContextLayoutManager.From(base.Dispatcher).UpdateLayout();
	}

	internal static void BuildRouteHelper(DependencyObject e, EventRoute route, RoutedEventArgs args)
	{
		if (route == null)
		{
			throw new ArgumentNullException("route");
		}
		if (args == null)
		{
			throw new ArgumentNullException("args");
		}
		if (args.Source == null)
		{
			throw new ArgumentException(SR.SourceNotSet);
		}
		if (args.RoutedEvent != route.RoutedEvent)
		{
			throw new ArgumentException(SR.Mismatched_RoutedEvent);
		}
		if (args.RoutedEvent.RoutingStrategy == RoutingStrategy.Direct)
		{
			UIElement uIElement = e as UIElement;
			ContentElement contentElement = null;
			UIElement3D uIElement3D = null;
			if (uIElement == null)
			{
				contentElement = e as ContentElement;
				if (contentElement == null)
				{
					uIElement3D = e as UIElement3D;
				}
			}
			if (uIElement != null)
			{
				uIElement.AddToEventRoute(route, args);
			}
			else if (contentElement != null)
			{
				contentElement.AddToEventRoute(route, args);
			}
			else
			{
				uIElement3D?.AddToEventRoute(route, args);
			}
			return;
		}
		int num = 0;
		while (e != null)
		{
			UIElement uIElement2 = e as UIElement;
			ContentElement contentElement2 = null;
			UIElement3D uIElement3D2 = null;
			if (uIElement2 == null)
			{
				contentElement2 = e as ContentElement;
				if (contentElement2 == null)
				{
					uIElement3D2 = e as UIElement3D;
				}
			}
			if (num++ > 4096)
			{
				throw new InvalidOperationException(SR.TreeLoop);
			}
			object obj = null;
			if (uIElement2 != null)
			{
				obj = uIElement2.AdjustEventSource(args);
			}
			else if (contentElement2 != null)
			{
				obj = contentElement2.AdjustEventSource(args);
			}
			else if (uIElement3D2 != null)
			{
				obj = uIElement3D2.AdjustEventSource(args);
			}
			if (obj != null)
			{
				route.AddSource(obj);
			}
			bool flag = false;
			if (uIElement2 != null)
			{
				uIElement2.AddSynchronizedInputPreOpportunityHandler(route, args);
				flag = uIElement2.BuildRouteCore(route, args);
				uIElement2.AddToEventRoute(route, args);
				uIElement2.AddSynchronizedInputPostOpportunityHandler(route, args);
				e = uIElement2.GetUIParent(flag);
			}
			else if (contentElement2 != null)
			{
				contentElement2.AddSynchronizedInputPreOpportunityHandler(route, args);
				flag = contentElement2.BuildRouteCore(route, args);
				contentElement2.AddToEventRoute(route, args);
				contentElement2.AddSynchronizedInputPostOpportunityHandler(route, args);
				e = contentElement2.GetUIParent(flag);
			}
			else if (uIElement3D2 != null)
			{
				uIElement3D2.AddSynchronizedInputPreOpportunityHandler(route, args);
				flag = uIElement3D2.BuildRouteCore(route, args);
				uIElement3D2.AddToEventRoute(route, args);
				uIElement3D2.AddSynchronizedInputPostOpportunityHandler(route, args);
				e = uIElement3D2.GetUIParent(flag);
			}
			if (e == args.Source)
			{
				route.AddSource(e);
			}
		}
	}

	internal void AddSynchronizedInputPreOpportunityHandler(EventRoute route, RoutedEventArgs args)
	{
		if (InputManager.IsSynchronizedInput)
		{
			if (SynchronizedInputHelper.IsListening(this, args))
			{
				RoutedEventHandler eventHandler = SynchronizedInputPreOpportunityHandler;
				SynchronizedInputHelper.AddHandlerToRoute(this, route, eventHandler, handledToo: false);
			}
			else
			{
				AddSynchronizedInputPreOpportunityHandlerCore(route, args);
			}
		}
	}

	internal virtual void AddSynchronizedInputPreOpportunityHandlerCore(EventRoute route, RoutedEventArgs args)
	{
	}

	internal void AddSynchronizedInputPostOpportunityHandler(EventRoute route, RoutedEventArgs args)
	{
		if (InputManager.IsSynchronizedInput)
		{
			if (SynchronizedInputHelper.IsListening(this, args))
			{
				RoutedEventHandler eventHandler = SynchronizedInputPostOpportunityHandler;
				SynchronizedInputHelper.AddHandlerToRoute(this, route, eventHandler, handledToo: true);
			}
			else
			{
				SynchronizedInputHelper.AddParentPreOpportunityHandler(this, route, args);
			}
		}
	}

	internal void SynchronizedInputPreOpportunityHandler(object sender, RoutedEventArgs args)
	{
		SynchronizedInputHelper.PreOpportunityHandler(sender, args);
	}

	internal void SynchronizedInputPostOpportunityHandler(object sender, RoutedEventArgs args)
	{
		if (args.Handled && InputManager.SynchronizedInputState == SynchronizedInputStates.HadOpportunity)
		{
			SynchronizedInputHelper.PostOpportunityHandler(sender, args);
		}
	}

	internal bool StartListeningSynchronizedInput(SynchronizedInputType inputType)
	{
		if (InputManager.IsSynchronizedInput)
		{
			return false;
		}
		InputManager.StartListeningSynchronizedInput(this, inputType);
		return true;
	}

	internal void CancelSynchronizedInput()
	{
		InputManager.CancelSynchronizedInput();
	}

	[FriendAccessAllowed]
	internal static void AddHandler(DependencyObject d, RoutedEvent routedEvent, Delegate handler)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		if (d is UIElement uIElement)
		{
			uIElement.AddHandler(routedEvent, handler);
			return;
		}
		if (d is ContentElement contentElement)
		{
			contentElement.AddHandler(routedEvent, handler);
			return;
		}
		if (d is UIElement3D uIElement3D)
		{
			uIElement3D.AddHandler(routedEvent, handler);
			return;
		}
		throw new ArgumentException(SR.Format(SR.Invalid_IInputElement, d.GetType()));
	}

	[FriendAccessAllowed]
	internal static void RemoveHandler(DependencyObject d, RoutedEvent routedEvent, Delegate handler)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		if (d is UIElement uIElement)
		{
			uIElement.RemoveHandler(routedEvent, handler);
			return;
		}
		if (d is ContentElement contentElement)
		{
			contentElement.RemoveHandler(routedEvent, handler);
			return;
		}
		if (d is UIElement3D uIElement3D)
		{
			uIElement3D.RemoveHandler(routedEvent, handler);
			return;
		}
		throw new ArgumentException(SR.Format(SR.Invalid_IInputElement, d.GetType()));
	}

	internal virtual void OnPresentationSourceChanged(bool attached)
	{
		if (!attached && FocusManager.GetFocusedElement(this) != null)
		{
			FocusManager.SetFocusedElement(this, null);
		}
	}

	/// <summary>Translates a point relative to this element to coordinates that are relative to the specified element. </summary>
	/// <returns>A point value, now relative to the target element rather than this source element.</returns>
	/// <param name="point">The point value, as relative to this element.</param>
	/// <param name="relativeTo">The element to translate the given point into.</param>
	public Point TranslatePoint(Point point, UIElement relativeTo)
	{
		return InputElement.TranslatePoint(point, this, relativeTo);
	}

	/// <summary> Returns the input element within the current element that is at the specified coordinates, relative to the current element's origin. </summary>
	/// <returns>The element child that is located at the given position.</returns>
	/// <param name="point">The offset coordinates within this element.</param>
	public IInputElement InputHitTest(Point point)
	{
		InputHitTest(point, out var enabledHit, out var _);
		return enabledHit;
	}

	internal void InputHitTest(Point pt, out IInputElement enabledHit, out IInputElement rawHit)
	{
		InputHitTest(pt, out enabledHit, out rawHit, out var _);
	}

	internal void InputHitTest(Point pt, out IInputElement enabledHit, out IInputElement rawHit, out HitTestResult rawHitResult)
	{
		PointHitTestParameters hitTestParameters = new PointHitTestParameters(pt);
		InputHitTestResult inputHitTestResult = new InputHitTestResult();
		VisualTreeHelper.HitTest(this, InputHitTestFilterCallback, inputHitTestResult.InputHitTestResultCallback, hitTestParameters);
		DependencyObject dependencyObject = inputHitTestResult.Result;
		rawHit = dependencyObject as IInputElement;
		rawHitResult = inputHitTestResult.HitTestResult;
		enabledHit = null;
		while (dependencyObject != null)
		{
			if (dependencyObject is IContentHost contentHost && (bool)InputElement.GetContainingUIElement(dependencyObject).GetValue(IsEnabledProperty))
			{
				pt = InputElement.TranslatePoint(pt, this, dependencyObject);
				enabledHit = (rawHit = contentHost.InputHitTest(pt));
				rawHitResult = null;
				if (enabledHit != null)
				{
					break;
				}
			}
			if (dependencyObject is UIElement uIElement)
			{
				if (rawHit == null)
				{
					rawHit = uIElement;
					rawHitResult = null;
				}
				if (uIElement.IsEnabled)
				{
					enabledHit = uIElement;
					break;
				}
			}
			if (dependencyObject is UIElement3D uIElement3D)
			{
				if (rawHit == null)
				{
					rawHit = uIElement3D;
					rawHitResult = null;
				}
				if (uIElement3D.IsEnabled)
				{
					enabledHit = uIElement3D;
					break;
				}
			}
			if (dependencyObject != this)
			{
				dependencyObject = VisualTreeHelper.GetParentInternal(dependencyObject);
				continue;
			}
			break;
		}
	}

	private HitTestFilterBehavior InputHitTestFilterCallback(DependencyObject currentNode)
	{
		HitTestFilterBehavior result = HitTestFilterBehavior.Continue;
		if (UIElementHelper.IsUIElementOrUIElement3D(currentNode))
		{
			if (!UIElementHelper.IsVisible(currentNode))
			{
				result = HitTestFilterBehavior.ContinueSkipSelfAndChildren;
			}
			if (!UIElementHelper.IsHitTestVisible(currentNode))
			{
				result = HitTestFilterBehavior.ContinueSkipSelfAndChildren;
			}
		}
		else
		{
			result = HitTestFilterBehavior.Continue;
		}
		return result;
	}

	private static RoutedEvent CrackMouseButtonEvent(MouseButtonEventArgs e)
	{
		RoutedEvent result = null;
		switch (e.ChangedButton)
		{
		case MouseButton.Left:
			result = ((e.RoutedEvent != Mouse.PreviewMouseDownEvent) ? ((e.RoutedEvent != Mouse.MouseDownEvent) ? ((e.RoutedEvent != Mouse.PreviewMouseUpEvent) ? MouseLeftButtonUpEvent : PreviewMouseLeftButtonUpEvent) : MouseLeftButtonDownEvent) : PreviewMouseLeftButtonDownEvent);
			break;
		case MouseButton.Right:
			result = ((e.RoutedEvent != Mouse.PreviewMouseDownEvent) ? ((e.RoutedEvent != Mouse.MouseDownEvent) ? ((e.RoutedEvent != Mouse.PreviewMouseUpEvent) ? MouseRightButtonUpEvent : PreviewMouseRightButtonUpEvent) : MouseRightButtonDownEvent) : PreviewMouseRightButtonDownEvent);
			break;
		}
		return result;
	}

	private static void CrackMouseButtonEventAndReRaiseEvent(DependencyObject sender, MouseButtonEventArgs e)
	{
		RoutedEvent routedEvent = CrackMouseButtonEvent(e);
		if (routedEvent != null)
		{
			ReRaiseEventAs(sender, e, routedEvent);
		}
	}

	private static void ReRaiseEventAs(DependencyObject sender, RoutedEventArgs args, RoutedEvent newEvent)
	{
		RoutedEvent routedEvent = args.RoutedEvent;
		args.OverrideRoutedEvent(newEvent);
		object source = args.Source;
		EventRoute eventRoute = EventRouteFactory.FetchObject(args.RoutedEvent);
		if (TraceRoutedEvent.IsEnabled)
		{
			TraceRoutedEvent.Trace(TraceEventType.Start, TraceRoutedEvent.ReRaiseEventAs, args.RoutedEvent, sender, args, args.Handled);
		}
		try
		{
			BuildRouteHelper(sender, eventRoute, args);
			eventRoute.ReInvokeHandlers(sender, args);
			args.OverrideSource(source);
			args.OverrideRoutedEvent(routedEvent);
		}
		finally
		{
			if (TraceRoutedEvent.IsEnabled)
			{
				TraceRoutedEvent.Trace(TraceEventType.Stop, TraceRoutedEvent.ReRaiseEventAs, args.RoutedEvent, sender, args, args.Handled);
			}
		}
		EventRouteFactory.RecycleObject(eventRoute);
	}

	internal static void RaiseEventImpl(DependencyObject sender, RoutedEventArgs args)
	{
		EventRoute eventRoute = EventRouteFactory.FetchObject(args.RoutedEvent);
		if (TraceRoutedEvent.IsEnabled)
		{
			TraceRoutedEvent.Trace(TraceEventType.Start, TraceRoutedEvent.RaiseEvent, args.RoutedEvent, sender, args, args.Handled);
		}
		try
		{
			args.Source = sender;
			BuildRouteHelper(sender, eventRoute, args);
			eventRoute.InvokeHandlers(sender, args);
			args.Source = args.OriginalSource;
		}
		finally
		{
			if (TraceRoutedEvent.IsEnabled)
			{
				TraceRoutedEvent.Trace(TraceEventType.Stop, TraceRoutedEvent.RaiseEvent, args.RoutedEvent, sender, args, args.Handled);
			}
		}
		EventRouteFactory.RecycleObject(eventRoute);
	}

	private bool IsMouseDirectlyOver_ComputeValue()
	{
		return Mouse.DirectlyOver == this;
	}

	[FriendAccessAllowed]
	internal void SynchronizeReverseInheritPropertyFlags(DependencyObject oldParent, bool isCoreParent)
	{
		if (IsKeyboardFocusWithin)
		{
			Keyboard.PrimaryDevice.ReevaluateFocusAsync(this, oldParent, isCoreParent);
		}
		if (IsStylusOver)
		{
			StylusLogic.CurrentStylusLogicReevaluateStylusOver(this, oldParent, isCoreParent);
		}
		if (IsStylusCaptureWithin)
		{
			StylusLogic.CurrentStylusLogicReevaluateCapture(this, oldParent, isCoreParent);
		}
		if (IsMouseOver)
		{
			Mouse.PrimaryDevice.ReevaluateMouseOver(this, oldParent, isCoreParent);
		}
		if (IsMouseCaptureWithin)
		{
			Mouse.PrimaryDevice.ReevaluateCapture(this, oldParent, isCoreParent);
		}
		if (AreAnyTouchesOver)
		{
			TouchDevice.ReevaluateDirectlyOver(this, oldParent, isCoreParent);
		}
		if (AreAnyTouchesCapturedWithin)
		{
			TouchDevice.ReevaluateCapturedWithin(this, oldParent, isCoreParent);
		}
	}

	internal virtual bool BlockReverseInheritance()
	{
		return false;
	}

	/// <summary>Attempts to force capture of the mouse to this element. </summary>
	/// <returns>true if the mouse is successfully captured; otherwise, false.</returns>
	public bool CaptureMouse()
	{
		return Mouse.Capture(this);
	}

	/// <summary> Releases the mouse capture, if this element held the capture. </summary>
	public void ReleaseMouseCapture()
	{
		if (Mouse.Captured == this)
		{
			Mouse.Capture(null);
		}
	}

	private bool IsStylusDirectlyOver_ComputeValue()
	{
		return Stylus.DirectlyOver == this;
	}

	/// <summary> Attempts to force capture of the stylus to this element. </summary>
	/// <returns>true if the stylus was successfully captured; otherwise, false.</returns>
	public bool CaptureStylus()
	{
		return Stylus.Capture(this);
	}

	/// <summary> Releases the stylus device capture, if this element held the capture. </summary>
	public void ReleaseStylusCapture()
	{
		Stylus.Capture(null);
	}

	private bool IsKeyboardFocused_ComputeValue()
	{
		return Keyboard.FocusedElement == this;
	}

	/// <summary>Attempts to set focus to this element. </summary>
	/// <returns>true if keyboard focus and logical focus were set to this element; false if only logical focus was set to this element, or if the call to this method did not force the focus to change.</returns>
	public bool Focus()
	{
		if (Keyboard.Focus(this) == this)
		{
			TipTsfHelper.Show(this);
			return true;
		}
		if (Focusable && IsEnabled)
		{
			DependencyObject focusScope = FocusManager.GetFocusScope(this);
			if (FocusManager.GetFocusedElement(focusScope) == null)
			{
				FocusManager.SetFocusedElement(focusScope, this);
			}
		}
		return false;
	}

	/// <summary>Attempts to move focus from this element to another element. The direction to move focus is specified by a guidance direction, which is interpreted within the organization of the visual parent for this element. </summary>
	/// <returns>true if the requested traversal was performed; otherwise, false. </returns>
	/// <param name="request">A traversal request, which contains a property that indicates either a mode to traverse in existing tab order, or a direction to move visually.</param>
	public virtual bool MoveFocus(TraversalRequest request)
	{
		return false;
	}

	/// <summary>When overridden in a derived class, returns the element that would receive focus for a specified focus traversal direction, without actually moving focus to that element.</summary>
	/// <returns>The element that would have received focus if <see cref="M:System.Windows.UIElement.MoveFocus(System.Windows.Input.TraversalRequest)" /> were actually invoked.</returns>
	/// <param name="direction">The direction of the requested focus traversal.</param>
	public virtual DependencyObject PredictFocus(FocusNavigationDirection direction)
	{
		return null;
	}

	/// <summary> Provides class handling for when an access key that is meaningful for this element is invoked. </summary>
	/// <param name="e">The event data to the access key event. The event data reports which key was invoked, and indicate whether the <see cref="T:System.Windows.Input.AccessKeyManager" /> object that controls the sending of these events also sent this access key invocation to other elements.</param>
	protected virtual void OnAccessKey(AccessKeyEventArgs e)
	{
		Focus();
	}

	private static void Opacity_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((UIElement)d).pushOpacity();
	}

	private void pushOpacity()
	{
		if (Visibility == Visibility.Visible)
		{
			base.VisualOpacity = Opacity;
		}
	}

	private static void OpacityMask_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((UIElement)d).pushOpacityMask();
	}

	private void pushOpacityMask()
	{
		base.VisualOpacityMask = OpacityMask;
	}

	private static void OnBitmapEffectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((UIElement)d).pushBitmapEffect();
	}

	private void pushBitmapEffect()
	{
		base.VisualBitmapEffect = BitmapEffect;
	}

	private static void OnEffectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((UIElement)d).pushEffect();
	}

	private void pushEffect()
	{
		base.VisualEffect = Effect;
	}

	private static void OnBitmapEffectInputChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((UIElement)d).pushBitmapEffectInput((BitmapEffectInput)e.NewValue);
	}

	private void pushBitmapEffectInput(BitmapEffectInput newValue)
	{
		base.VisualBitmapEffectInput = newValue;
	}

	private static void EdgeMode_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((UIElement)d).pushEdgeMode();
	}

	private void pushEdgeMode()
	{
		base.VisualEdgeMode = RenderOptions.GetEdgeMode(this);
	}

	private static void BitmapScalingMode_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((UIElement)d).pushBitmapScalingMode();
	}

	private void pushBitmapScalingMode()
	{
		base.VisualBitmapScalingMode = RenderOptions.GetBitmapScalingMode(this);
	}

	private static void ClearTypeHint_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((UIElement)d).pushClearTypeHint();
	}

	private void pushClearTypeHint()
	{
		base.VisualClearTypeHint = RenderOptions.GetClearTypeHint(this);
	}

	private static void TextHintingMode_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((UIElement)d).pushTextHintingMode();
	}

	private void pushTextHintingMode()
	{
		base.VisualTextHintingMode = TextOptionsInternal.GetTextHintingMode(this);
	}

	private static void OnCacheModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((UIElement)d).pushCacheMode();
	}

	private void pushCacheMode()
	{
		base.VisualCacheMode = CacheMode;
	}

	private void pushVisualEffects()
	{
		pushCacheMode();
		pushOpacity();
		pushOpacityMask();
		pushBitmapEffect();
		pushEdgeMode();
		pushBitmapScalingMode();
		pushClearTypeHint();
		pushTextHintingMode();
	}

	private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		UIElement obj = (UIElement)d;
		Visibility visibility = (Visibility)e.NewValue;
		obj.VisibilityCache = visibility;
		obj.switchVisibilityIfNeeded(visibility);
		obj.UpdateIsVisibleCache();
	}

	private static bool ValidateVisibility(object o)
	{
		Visibility visibility = (Visibility)o;
		if (visibility != 0 && visibility != Visibility.Hidden)
		{
			return visibility == Visibility.Collapsed;
		}
		return true;
	}

	private void switchVisibilityIfNeeded(Visibility visibility)
	{
		switch (visibility)
		{
		case Visibility.Visible:
			ensureVisible();
			break;
		case Visibility.Hidden:
			ensureInvisible(collapsed: false);
			break;
		case Visibility.Collapsed:
			ensureInvisible(collapsed: true);
			break;
		}
	}

	private void ensureVisible()
	{
		if (ReadFlag(CoreFlags.IsOpacitySuppressed))
		{
			base.VisualOpacity = Opacity;
			if (ReadFlag(CoreFlags.IsCollapsed))
			{
				WriteFlag(CoreFlags.IsCollapsed, value: false);
				signalDesiredSizeChange();
				InvalidateVisual();
			}
			WriteFlag(CoreFlags.IsOpacitySuppressed, value: false);
		}
	}

	private void ensureInvisible(bool collapsed)
	{
		if (!ReadFlag(CoreFlags.IsOpacitySuppressed))
		{
			base.VisualOpacity = 0.0;
			WriteFlag(CoreFlags.IsOpacitySuppressed, value: true);
		}
		if (!ReadFlag(CoreFlags.IsCollapsed) && collapsed)
		{
			WriteFlag(CoreFlags.IsCollapsed, value: true);
			signalDesiredSizeChange();
		}
		else if (ReadFlag(CoreFlags.IsCollapsed) && !collapsed)
		{
			WriteFlag(CoreFlags.IsCollapsed, value: false);
			signalDesiredSizeChange();
		}
	}

	private void signalDesiredSizeChange()
	{
		GetUIParentOrICH(out var uiParent, out var ich);
		if (uiParent != null)
		{
			uiParent.OnChildDesiredSizeChanged(this);
		}
		else
		{
			ich?.OnChildDesiredSizeChanged(this);
		}
	}

	private void ensureClip(Size layoutSlotSize)
	{
		Geometry geometry = GetLayoutClip(layoutSlotSize);
		if (Clip != null)
		{
			geometry = ((geometry != null) ? new CombinedGeometry(GeometryCombineMode.Intersect, geometry, Clip) : Clip);
		}
		ChangeVisualClip(geometry, dontSetWhenClose: true);
	}

	/// <summary> Implements <see cref="M:System.Windows.Media.Visual.HitTestCore(System.Windows.Media.PointHitTestParameters)" /> to supply base element hit testing behavior (returning <see cref="T:System.Windows.Media.HitTestResult" />). </summary>
	/// <returns>Results of the test, including the evaluated point.</returns>
	/// <param name="hitTestParameters">Describes the hit test to perform, including the initial hit point.</param>
	protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
	{
		if (_drawingContent != null && _drawingContent.HitTestPoint(hitTestParameters.HitPoint))
		{
			return new PointHitTestResult(this, hitTestParameters.HitPoint);
		}
		return null;
	}

	/// <summary>Implements <see cref="M:System.Windows.Media.Visual.HitTestCore(System.Windows.Media.GeometryHitTestParameters)" /> to supply base element hit testing behavior (returning <see cref="T:System.Windows.Media.GeometryHitTestResult" />). </summary>
	/// <returns>Results of the test, including the evaluated geometry.</returns>
	/// <param name="hitTestParameters">Describes the hit test to perform, including the initial hit point.</param>
	protected override GeometryHitTestResult HitTestCore(GeometryHitTestParameters hitTestParameters)
	{
		if (_drawingContent != null && GetHitTestBounds().IntersectsWith(hitTestParameters.Bounds))
		{
			IntersectionDetail intersectionDetail = _drawingContent.HitTestGeometry(hitTestParameters.InternalHitGeometry);
			if (intersectionDetail != IntersectionDetail.Empty)
			{
				return new GeometryHitTestResult(this, intersectionDetail);
			}
		}
		return null;
	}

	[FriendAccessAllowed]
	internal DrawingContext RenderOpen()
	{
		return new VisualDrawingContext(this);
	}

	internal override void RenderClose(IDrawingContent newContent)
	{
		IDrawingContent drawingContent = _drawingContent;
		if (drawingContent != null || newContent != null)
		{
			_drawingContent = null;
			if (drawingContent != null)
			{
				drawingContent.PropagateChangedHandler(base.ContentsChangedHandler, adding: false);
				DisconnectAttachedResource(VisualProxyFlags.IsContentConnected, drawingContent);
			}
			newContent?.PropagateChangedHandler(base.ContentsChangedHandler, adding: true);
			_drawingContent = newContent;
			SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsContentDirty);
			Visual.PropagateFlags(this, VisualFlags.IsSubtreeDirtyForPrecompute, VisualProxyFlags.IsSubtreeDirtyForRender);
		}
	}

	internal override void FreeContent(DUCE.Channel channel)
	{
		if (_drawingContent != null && CheckFlagsAnd(channel, VisualProxyFlags.IsContentConnected))
		{
			DUCE.CompositionNode.SetContent(_proxy.GetHandle(channel), DUCE.ResourceHandle.Null, channel);
			_drawingContent.ReleaseOnChannel(channel);
			SetFlags(channel, value: false, VisualProxyFlags.IsContentConnected);
		}
		base.FreeContent(channel);
	}

	internal override Rect GetContentBounds()
	{
		if (_drawingContent != null)
		{
			_ = Rect.Empty;
			MediaContext mediaContext = MediaContext.From(base.Dispatcher);
			BoundsDrawingContextWalker ctx = mediaContext.AcquireBoundsDrawingContextWalker();
			Rect contentBounds = _drawingContent.GetContentBounds(ctx);
			mediaContext.ReleaseBoundsDrawingContextWalker(ctx);
			return contentBounds;
		}
		return Rect.Empty;
	}

	internal void WalkContent(DrawingContextWalker walker)
	{
		VerifyAPIReadOnly();
		if (_drawingContent != null)
		{
			_drawingContent.WalkContent(walker);
		}
	}

	internal override void RenderContent(RenderContext ctx, bool isOnChannel)
	{
		DUCE.Channel channel = ctx.Channel;
		if (_drawingContent != null)
		{
			DUCE.IResource drawingContent = _drawingContent;
			drawingContent.AddRefOnChannel(channel);
			DUCE.CompositionNode.SetContent(_proxy.GetHandle(channel), drawingContent.GetHandle(channel), channel);
			SetFlags(channel, value: true, VisualProxyFlags.IsContentConnected);
		}
		else if (isOnChannel)
		{
			DUCE.CompositionNode.SetContent(_proxy.GetHandle(channel), DUCE.ResourceHandle.Null, channel);
		}
	}

	internal override DrawingGroup GetDrawing()
	{
		VerifyAPIReadOnly();
		DrawingGroup result = null;
		if (_drawingContent != null)
		{
			result = DrawingServices.DrawingGroupFromRenderData((RenderData)_drawingContent);
		}
		return result;
	}

	/// <summary>Returns an alternative clipping geometry that represents the region that would be clipped if <see cref="P:System.Windows.UIElement.ClipToBounds" /> were set to true. </summary>
	/// <returns>The potential clipping geometry.</returns>
	/// <param name="layoutSlotSize">The available size provided by the element.</param>
	protected virtual Geometry GetLayoutClip(Size layoutSlotSize)
	{
		if (ClipToBounds)
		{
			RectangleGeometry rectangleGeometry = new RectangleGeometry(new Rect(RenderSize));
			rectangleGeometry.Freeze();
			return rectangleGeometry;
		}
		return null;
	}

	private static void ClipToBounds_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		UIElement uIElement = (UIElement)d;
		uIElement.ClipToBoundsCache = (bool)e.NewValue;
		if (!uIElement.NeverMeasured || !uIElement.NeverArranged)
		{
			uIElement.InvalidateArrange();
		}
	}

	private static void Clip_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		UIElement uIElement = (UIElement)d;
		if (!uIElement.NeverMeasured || !uIElement.NeverArranged)
		{
			uIElement.InvalidateArrange();
		}
	}

	private static void SnapsToDevicePixels_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		UIElement uIElement = (UIElement)d;
		uIElement.SnapsToDevicePixelsCache = (bool)e.NewValue;
		if (!uIElement.NeverMeasured || !uIElement.NeverArranged)
		{
			uIElement.InvalidateArrange();
		}
	}

	internal void InvokeAccessKey(AccessKeyEventArgs e)
	{
		OnAccessKey(e);
	}

	private static void IsFocused_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		UIElement uIElement = (UIElement)d;
		if ((bool)e.NewValue)
		{
			uIElement.OnGotFocus(new RoutedEventArgs(GotFocusEvent, uIElement));
		}
		else
		{
			uIElement.OnLostFocus(new RoutedEventArgs(LostFocusEvent, uIElement));
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.UIElement.GotFocus" /> routed event by using the event data provided. </summary>
	/// <param name="e">A <see cref="T:System.Windows.RoutedEventArgs" /> that contains event data. This event data must contain the identifier for the <see cref="E:System.Windows.UIElement.GotFocus" /> event.</param>
	protected virtual void OnGotFocus(RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.UIElement.LostFocus" /> routed event by using the event data that is provided. </summary>
	/// <param name="e">A <see cref="T:System.Windows.RoutedEventArgs" /> that contains event data. This event data must contain the identifier for the <see cref="E:System.Windows.UIElement.LostFocus" /> event.</param>
	protected virtual void OnLostFocus(RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	private static object CoerceIsEnabled(DependencyObject d, object value)
	{
		UIElement uIElement = (UIElement)d;
		if ((bool)value)
		{
			DependencyObject dependencyObject = uIElement.GetUIParentCore() as ContentElement;
			if (dependencyObject == null)
			{
				dependencyObject = InputElement.GetContainingUIElement(uIElement._parent);
			}
			if (dependencyObject == null || (bool)dependencyObject.GetValue(IsEnabledProperty))
			{
				return BooleanBoxes.Box(uIElement.IsEnabledCore);
			}
			return BooleanBoxes.FalseBox;
		}
		return BooleanBoxes.FalseBox;
	}

	private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		UIElement obj = (UIElement)d;
		obj.RaiseDependencyPropertyChanged(IsEnabledChangedKey, e);
		obj.InvalidateForceInheritPropertyOnChildren(e.Property);
		InputManager.SafeCurrentNotifyHitTestInvalidated();
		obj.GetAutomationPeer()?.InvalidatePeer();
	}

	private static object CoerceIsHitTestVisible(DependencyObject d, object value)
	{
		UIElement uIElement = (UIElement)d;
		if ((bool)value)
		{
			DependencyObject containingUIElement = InputElement.GetContainingUIElement(uIElement._parent);
			if (containingUIElement == null || UIElementHelper.IsHitTestVisible(containingUIElement))
			{
				return BooleanBoxes.TrueBox;
			}
			return BooleanBoxes.FalseBox;
		}
		return BooleanBoxes.FalseBox;
	}

	private static void OnIsHitTestVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		UIElement obj = (UIElement)d;
		obj.RaiseDependencyPropertyChanged(IsHitTestVisibleChangedKey, e);
		obj.InvalidateForceInheritPropertyOnChildren(e.Property);
		InputManager.SafeCurrentNotifyHitTestInvalidated();
	}

	private static object GetIsVisible(DependencyObject d, out BaseValueSourceInternal source)
	{
		source = BaseValueSourceInternal.Local;
		if (!((UIElement)d).IsVisible)
		{
			return BooleanBoxes.FalseBox;
		}
		return BooleanBoxes.TrueBox;
	}

	internal void UpdateIsVisibleCache()
	{
		bool flag = Visibility == Visibility.Visible;
		if (flag)
		{
			bool flag2 = false;
			DependencyObject containingUIElement = InputElement.GetContainingUIElement(_parent);
			if (containingUIElement != null)
			{
				flag2 = UIElementHelper.IsVisible(containingUIElement);
			}
			else if (PresentationSource.CriticalFromVisual(this) != null)
			{
				flag2 = true;
			}
			if (!flag2)
			{
				flag = false;
			}
		}
		if (flag != IsVisible)
		{
			WriteFlag(CoreFlags.IsVisibleCache, flag);
			NotifyPropertyChange(new DependencyPropertyChangedEventArgs(IsVisibleProperty, _isVisibleMetadata, BooleanBoxes.Box(!flag), BooleanBoxes.Box(flag)));
		}
	}

	private static void OnIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		UIElement obj = (UIElement)d;
		obj.RaiseDependencyPropertyChanged(IsVisibleChangedKey, e);
		obj.InvalidateForceInheritPropertyOnChildren(e.Property);
		InputManager.SafeCurrentNotifyHitTestInvalidated();
	}

	private static void OnFocusableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((UIElement)d).RaiseDependencyPropertyChanged(FocusableChangedKey, e);
	}

	/// <summary>Returns class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementations for the Windows Presentation Foundation (WPF) infrastructure.</summary>
	/// <returns>The type-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation.</returns>
	protected virtual AutomationPeer OnCreateAutomationPeer()
	{
		if (!AccessibilitySwitches.ItemsControlDoesNotSupportAutomation)
		{
			AutomationNotSupportedByDefaultField.SetValue(this, value: true);
		}
		return null;
	}

	internal virtual AutomationPeer OnCreateAutomationPeerInternal()
	{
		return null;
	}

	internal AutomationPeer CreateAutomationPeer()
	{
		VerifyAccess();
		AutomationPeer automationPeer = null;
		if (HasAutomationPeer)
		{
			automationPeer = AutomationPeerField.GetValue(this);
		}
		else
		{
			if (!AccessibilitySwitches.ItemsControlDoesNotSupportAutomation)
			{
				AutomationNotSupportedByDefaultField.ClearValue(this);
				automationPeer = OnCreateAutomationPeer();
				if (automationPeer == null && !AutomationNotSupportedByDefaultField.GetValue(this))
				{
					automationPeer = OnCreateAutomationPeerInternal();
				}
			}
			else
			{
				automationPeer = OnCreateAutomationPeer();
			}
			if (automationPeer != null)
			{
				AutomationPeerField.SetValue(this, automationPeer);
				HasAutomationPeer = true;
			}
		}
		return automationPeer;
	}

	internal AutomationPeer GetAutomationPeer()
	{
		VerifyAccess();
		if (HasAutomationPeer)
		{
			return AutomationPeerField.GetValue(this);
		}
		return null;
	}

	internal AutomationPeer CreateGenericRootAutomationPeer()
	{
		VerifyAccess();
		AutomationPeer automationPeer = null;
		if (HasAutomationPeer)
		{
			automationPeer = AutomationPeerField.GetValue(this);
		}
		else
		{
			automationPeer = new GenericRootAutomationPeer(this);
			AutomationPeerField.SetValue(this, automationPeer);
			HasAutomationPeer = true;
		}
		return automationPeer;
	}

	[FriendAccessAllowed]
	internal void SetPersistId(int value)
	{
		_persistId = value;
	}

	private void RaiseDependencyPropertyChanged(EventPrivateKey key, DependencyPropertyChangedEventArgs args)
	{
		EventHandlersStore eventHandlersStore = EventHandlersStore;
		if (eventHandlersStore != null)
		{
			Delegate @delegate = eventHandlersStore.Get(key);
			if ((object)@delegate != null)
			{
				((DependencyPropertyChangedEventHandler)@delegate)(this, args);
			}
		}
	}

	internal static void SynchronizeForceInheritProperties(UIElement uiElement, ContentElement contentElement, UIElement3D uiElement3D, DependencyObject parent)
	{
		if (uiElement != null || uiElement3D != null)
		{
			if (!(bool)parent.GetValue(IsEnabledProperty))
			{
				if (uiElement != null)
				{
					uiElement.CoerceValue(IsEnabledProperty);
				}
				else
				{
					uiElement3D.CoerceValue(IsEnabledProperty);
				}
			}
			if (!(bool)parent.GetValue(IsHitTestVisibleProperty))
			{
				if (uiElement != null)
				{
					uiElement.CoerceValue(IsHitTestVisibleProperty);
				}
				else
				{
					uiElement3D.CoerceValue(IsHitTestVisibleProperty);
				}
			}
			if ((bool)parent.GetValue(IsVisibleProperty))
			{
				if (uiElement != null)
				{
					uiElement.UpdateIsVisibleCache();
				}
				else
				{
					uiElement3D.UpdateIsVisibleCache();
				}
			}
		}
		else if (contentElement != null && !(bool)parent.GetValue(IsEnabledProperty))
		{
			contentElement.CoerceValue(IsEnabledProperty);
		}
	}

	internal static void InvalidateForceInheritPropertyOnChildren(Visual v, DependencyProperty property)
	{
		int internalVisual2DOr3DChildrenCount = v.InternalVisual2DOr3DChildrenCount;
		for (int i = 0; i < internalVisual2DOr3DChildrenCount; i++)
		{
			DependencyObject dependencyObject = v.InternalGet2DOr3DVisualChild(i);
			if (dependencyObject is Visual visual)
			{
				if (visual is UIElement uIElement)
				{
					if (property == IsVisibleProperty)
					{
						uIElement.UpdateIsVisibleCache();
					}
					else
					{
						uIElement.CoerceValue(property);
					}
				}
				else
				{
					visual.InvalidateForceInheritPropertyOnChildren(property);
				}
			}
			else
			{
				if (!(dependencyObject is Visual3D visual3D))
				{
					continue;
				}
				if (visual3D is UIElement3D uIElement3D)
				{
					if (property == IsVisibleProperty)
					{
						uIElement3D.UpdateIsVisibleCache();
					}
					else
					{
						uIElement3D.CoerceValue(property);
					}
				}
				else
				{
					visual3D.InvalidateForceInheritPropertyOnChildren(property);
				}
			}
		}
	}

	private static void OnIsManipulationEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if ((bool)e.NewValue)
		{
			((UIElement)d).CoerceStylusProperties();
		}
		else
		{
			Manipulation.TryCompleteManipulation((UIElement)d);
		}
	}

	private void CoerceStylusProperties()
	{
		if (IsDefaultValue(this, Stylus.IsFlicksEnabledProperty))
		{
			SetCurrentValueInternal(Stylus.IsFlicksEnabledProperty, BooleanBoxes.FalseBox);
		}
	}

	private static bool IsDefaultValue(DependencyObject dependencyObject, DependencyProperty dependencyProperty)
	{
		if (dependencyObject.GetValueSource(dependencyProperty, null, out var _, out var isExpression, out var isAnimated, out var isCoerced, out var _) == BaseValueSourceInternal.Default && !isExpression && !isAnimated)
		{
			return !isCoerced;
		}
		return false;
	}

	private static void OnManipulationStartingThunk(object sender, ManipulationStartingEventArgs e)
	{
		((UIElement)sender).OnManipulationStarting(e);
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.ManipulationStarting" /> routed event that occurs when the manipulation processor is first created. </summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.ManipulationStartingEventArgs" />  that contains the event data. </param>
	protected virtual void OnManipulationStarting(ManipulationStartingEventArgs e)
	{
	}

	private static void OnManipulationStartedThunk(object sender, ManipulationStartedEventArgs e)
	{
		((UIElement)sender).OnManipulationStarted(e);
	}

	/// <summary>Called when the <see cref="E:System.Windows.UIElement.ManipulationStarted" /> event occurs.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnManipulationStarted(ManipulationStartedEventArgs e)
	{
	}

	private static void OnManipulationDeltaThunk(object sender, ManipulationDeltaEventArgs e)
	{
		((UIElement)sender).OnManipulationDelta(e);
	}

	/// <summary>Called when the <see cref="E:System.Windows.UIElement.ManipulationDelta" /> event occurs.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnManipulationDelta(ManipulationDeltaEventArgs e)
	{
	}

	private static void OnManipulationInertiaStartingThunk(object sender, ManipulationInertiaStartingEventArgs e)
	{
		((UIElement)sender).OnManipulationInertiaStarting(e);
	}

	/// <summary>Called when the <see cref="E:System.Windows.UIElement.ManipulationInertiaStarting" /> event occurs.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnManipulationInertiaStarting(ManipulationInertiaStartingEventArgs e)
	{
	}

	private static void OnManipulationBoundaryFeedbackThunk(object sender, ManipulationBoundaryFeedbackEventArgs e)
	{
		((UIElement)sender).OnManipulationBoundaryFeedback(e);
	}

	/// <summary>Called when the <see cref="E:System.Windows.UIElement.ManipulationBoundaryFeedback" /> event occurs.</summary>
	/// <param name="e">The data for the event. </param>
	protected virtual void OnManipulationBoundaryFeedback(ManipulationBoundaryFeedbackEventArgs e)
	{
	}

	private static void OnManipulationCompletedThunk(object sender, ManipulationCompletedEventArgs e)
	{
		((UIElement)sender).OnManipulationCompleted(e);
	}

	/// <summary>Called when the <see cref="E:System.Windows.UIElement.ManipulationCompleted" /> event occurs.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnManipulationCompleted(ManipulationCompletedEventArgs e)
	{
	}

	/// <summary>Attempts to force capture of a touch to this element.</summary>
	/// <returns>true if the specified touch is captured to this element; otherwise, false.</returns>
	/// <param name="touchDevice">The device to capture.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="touchDevice" /> is null.</exception>
	public bool CaptureTouch(TouchDevice touchDevice)
	{
		if (touchDevice == null)
		{
			throw new ArgumentNullException("touchDevice");
		}
		return touchDevice.Capture(this);
	}

	/// <summary>Attempts to release the specified touch device from this element.</summary>
	/// <returns>true if the touch device is released; otherwise, false.</returns>
	/// <param name="touchDevice">The device to release. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="touchDevice" /> is null.</exception>
	public bool ReleaseTouchCapture(TouchDevice touchDevice)
	{
		if (touchDevice == null)
		{
			throw new ArgumentNullException("touchDevice");
		}
		if (touchDevice.Captured == this)
		{
			touchDevice.Capture(null);
			return true;
		}
		return false;
	}

	/// <summary>Releases all captured touch devices from this element.</summary>
	public void ReleaseAllTouchCaptures()
	{
		TouchDevice.ReleaseAllCaptures(this);
	}
}
