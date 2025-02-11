using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Input;
using System.Windows.Media.Animation;
using MS.Internal;
using MS.Internal.Interop;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows;

/// <summary>Provides a WPF core-level base class for content elements. Content elements are designed for flow-style presentation, using an intuitive markup-oriented layout model and a deliberately simple object model. </summary>
public class ContentElement : DependencyObject, IAnimatable, IInputElement
{
	private static readonly Type _typeofThis;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewMouseDown" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewMouseDown" /> routed event.</returns>
	public static readonly RoutedEvent PreviewMouseDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.MouseDown" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.MouseDown" /> routed event.</returns>
	public static readonly RoutedEvent MouseDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewMouseUp" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewMouseUp" /> routed event.</returns>
	public static readonly RoutedEvent PreviewMouseUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.MouseUp" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.MouseUp" /> routed event.</returns>
	public static readonly RoutedEvent MouseUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewMouseLeftButtonDown" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewMouseLeftButtonDown" /> routed event.</returns>
	public static readonly RoutedEvent PreviewMouseLeftButtonDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.MouseLeftButtonDown" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.MouseLeftButtonDown" /> routed event.</returns>
	public static readonly RoutedEvent MouseLeftButtonDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewMouseLeftButtonUp" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewMouseLeftButtonUp" /> routed event.</returns>
	public static readonly RoutedEvent PreviewMouseLeftButtonUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.MouseLeftButtonUp" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.MouseLeftButtonUp" /> routed event.</returns>
	public static readonly RoutedEvent MouseLeftButtonUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewMouseRightButtonDown" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewMouseRightButtonDown" /> routed event.</returns>
	public static readonly RoutedEvent PreviewMouseRightButtonDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.MouseRightButtonDown" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.MouseRightButtonDown" /> routed event.</returns>
	public static readonly RoutedEvent MouseRightButtonDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewMouseRightButtonUp" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewMouseRightButtonUp" /> routed event.</returns>
	public static readonly RoutedEvent PreviewMouseRightButtonUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.MouseRightButtonUp" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.MouseRightButtonUp" /> routed event.</returns>
	public static readonly RoutedEvent MouseRightButtonUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewMouseMove" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewMouseMove" /> routed event.</returns>
	public static readonly RoutedEvent PreviewMouseMoveEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.MouseMove" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.MouseMove" /> routed event.</returns>
	public static readonly RoutedEvent MouseMoveEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewMouseWheel" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewMouseWheel" /> routed event.</returns>
	public static readonly RoutedEvent PreviewMouseWheelEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.MouseWheel" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.MouseWheel" /> routed event.</returns>
	public static readonly RoutedEvent MouseWheelEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.MouseEnter" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.MouseEnter" /> routed event.</returns>
	public static readonly RoutedEvent MouseEnterEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.MouseLeave" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.MouseLeave" /> routed event.</returns>
	public static readonly RoutedEvent MouseLeaveEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.GotMouseCapture" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.GotMouseCapture" /> routed event.</returns>
	public static readonly RoutedEvent GotMouseCaptureEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.LostMouseCapture" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.LostMouseCapture" /> routed event.</returns>
	public static readonly RoutedEvent LostMouseCaptureEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.QueryCursor" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.QueryCursor" /> routed event.</returns>
	public static readonly RoutedEvent QueryCursorEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewStylusDown" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewStylusDown" /> routed event.</returns>
	public static readonly RoutedEvent PreviewStylusDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.StylusDown" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.StylusDown" /> routed event.</returns>
	public static readonly RoutedEvent StylusDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewStylusUp" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewStylusUp" /> routed event. </returns>
	public static readonly RoutedEvent PreviewStylusUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.StylusUp" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.StylusUp" /> routed event.</returns>
	public static readonly RoutedEvent StylusUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewStylusMove" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewStylusMove" /> routed event.</returns>
	public static readonly RoutedEvent PreviewStylusMoveEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.StylusMove" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.StylusMove" /> routed event.</returns>
	public static readonly RoutedEvent StylusMoveEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewStylusInAirMove" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewStylusInAirMove" /> routed event.</returns>
	public static readonly RoutedEvent PreviewStylusInAirMoveEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.StylusInAirMove" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.StylusInAirMove" /> routed event.</returns>
	public static readonly RoutedEvent StylusInAirMoveEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.StylusEnter" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.StylusEnter" /> routed event.</returns>
	public static readonly RoutedEvent StylusEnterEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.StylusLeave" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.StylusLeave" /> routed event.</returns>
	public static readonly RoutedEvent StylusLeaveEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewStylusInRange" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewStylusInRange" /> routed event.</returns>
	public static readonly RoutedEvent PreviewStylusInRangeEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.StylusInRange" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.StylusInRange" /> routed event.</returns>
	public static readonly RoutedEvent StylusInRangeEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewStylusOutOfRange" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewStylusOutOfRange" /> routed event.</returns>
	public static readonly RoutedEvent PreviewStylusOutOfRangeEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.StylusOutOfRange" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.StylusOutOfRange" /> routed event.</returns>
	public static readonly RoutedEvent StylusOutOfRangeEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewStylusSystemGesture" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewStylusSystemGesture" /> routed event.</returns>
	public static readonly RoutedEvent PreviewStylusSystemGestureEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.StylusSystemGesture" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.StylusSystemGesture" /> routed event.</returns>
	public static readonly RoutedEvent StylusSystemGestureEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.GotStylusCapture" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.GotStylusCapture" /> routed event.</returns>
	public static readonly RoutedEvent GotStylusCaptureEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.LostStylusCapture" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.LostStylusCapture" /> routed event.</returns>
	public static readonly RoutedEvent LostStylusCaptureEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.StylusButtonDown" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.StylusButtonDown" /> routed event.</returns>
	public static readonly RoutedEvent StylusButtonDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.StylusButtonUp" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.StylusButtonUp" /> routed event.</returns>
	public static readonly RoutedEvent StylusButtonUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewStylusButtonDown" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewStylusButtonDown" /> routed event.</returns>
	public static readonly RoutedEvent PreviewStylusButtonDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewStylusButtonUp" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewStylusButtonUp" /> routed event.</returns>
	public static readonly RoutedEvent PreviewStylusButtonUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewKeyDown" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewKeyDown" /> routed event.</returns>
	public static readonly RoutedEvent PreviewKeyDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.KeyDown" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.KeyDown" /> routed event.</returns>
	public static readonly RoutedEvent KeyDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewKeyUp" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewKeyUp" /> routed event.</returns>
	public static readonly RoutedEvent PreviewKeyUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.KeyUp" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.KeyUp" /> routed event.</returns>
	public static readonly RoutedEvent KeyUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewGotKeyboardFocus" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewGotKeyboardFocus" /> routed event.</returns>
	public static readonly RoutedEvent PreviewGotKeyboardFocusEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.GotKeyboardFocus" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.GotKeyboardFocus" /> routed event.</returns>
	public static readonly RoutedEvent GotKeyboardFocusEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewLostKeyboardFocus" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewLostKeyboardFocus" /> routed event.</returns>
	public static readonly RoutedEvent PreviewLostKeyboardFocusEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.LostKeyboardFocus" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.LostKeyboardFocus" /> routed event.</returns>
	public static readonly RoutedEvent LostKeyboardFocusEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewTextInput" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewTextInput" /> routed event.</returns>
	public static readonly RoutedEvent PreviewTextInputEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.TextInput" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.TextInput" /> routed event.</returns>
	public static readonly RoutedEvent TextInputEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewQueryContinueDrag" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewQueryContinueDrag" /> routed event.</returns>
	public static readonly RoutedEvent PreviewQueryContinueDragEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.QueryContinueDrag" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.QueryContinueDrag" /> routed event.</returns>
	public static readonly RoutedEvent QueryContinueDragEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewGiveFeedback" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewGiveFeedback" /> routed event.</returns>
	public static readonly RoutedEvent PreviewGiveFeedbackEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.GiveFeedback" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.GiveFeedback" /> routed event.</returns>
	public static readonly RoutedEvent GiveFeedbackEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewDragEnter" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewDragEnter" /> routed event.</returns>
	public static readonly RoutedEvent PreviewDragEnterEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.DragEnter" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.DragEnter" /> routed event.</returns>
	public static readonly RoutedEvent DragEnterEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewDragOver" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewDragOver" /> routed event.</returns>
	public static readonly RoutedEvent PreviewDragOverEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.DragOver" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.DragOver" /> routed event.</returns>
	public static readonly RoutedEvent DragOverEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewDragLeave" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewDragLeave" /> routed event.</returns>
	public static readonly RoutedEvent PreviewDragLeaveEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.DragLeave" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.DragLeave" /> routed event.</returns>
	public static readonly RoutedEvent DragLeaveEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewDrop" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewDrop" /> routed event.</returns>
	public static readonly RoutedEvent PreviewDropEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.Drop" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.Drop" /> routed event.</returns>
	public static readonly RoutedEvent DropEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewTouchDown" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewTouchDown" /> routed event.</returns>
	public static readonly RoutedEvent PreviewTouchDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.TouchDown" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.TouchDown" /> routed event.</returns>
	public static readonly RoutedEvent TouchDownEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewTouchMove" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewTouchMove" /> routed event.</returns>
	public static readonly RoutedEvent PreviewTouchMoveEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.TouchMove" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.TouchMove" /> routed event.</returns>
	public static readonly RoutedEvent TouchMoveEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.PreviewTouchUp" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.PreviewTouchUp" /> dependency property.</returns>
	public static readonly RoutedEvent PreviewTouchUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.TouchUp" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.TouchUp" /> dependency property.</returns>
	public static readonly RoutedEvent TouchUpEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.GotTouchCapture" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.GotTouchCapture" /> routed event.</returns>
	public static readonly RoutedEvent GotTouchCaptureEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.LostTouchCapture" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.LostTouchCapture" /> routed event.</returns>
	public static readonly RoutedEvent LostTouchCaptureEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.TouchEnter" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.TouchEnter" /> routed event.</returns>
	public static readonly RoutedEvent TouchEnterEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.TouchLeave" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.TouchLeave" /> routed event.</returns>
	public static readonly RoutedEvent TouchLeaveEvent;

	/// <summary>Identifies the <see cref="P:System.Windows.ContentElement.IsMouseDirectlyOver" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.ContentElement.IsMouseDirectlyOver" /> dependency property.</returns>
	public static readonly DependencyProperty IsMouseDirectlyOverProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.ContentElement.IsMouseOver" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.ContentElement.IsMouseOver" /> dependency property identifier.</returns>
	public static readonly DependencyProperty IsMouseOverProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.ContentElement.IsStylusOver" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.ContentElement.IsStylusOver" /> dependency property identifier.</returns>
	public static readonly DependencyProperty IsStylusOverProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.ContentElement.IsKeyboardFocusWithin" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.ContentElement.IsKeyboardFocusWithin" /> dependency property identifier.</returns>
	public static readonly DependencyProperty IsKeyboardFocusWithinProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.ContentElement.IsMouseCaptured" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.ContentElement.IsMouseCaptured" /> dependency property.</returns>
	public static readonly DependencyProperty IsMouseCapturedProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.ContentElement.IsMouseCaptureWithin" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.ContentElement.IsMouseCaptureWithin" /> dependency property.</returns>
	public static readonly DependencyProperty IsMouseCaptureWithinProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.ContentElement.IsStylusDirectlyOver" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.ContentElement.IsStylusDirectlyOver" /> dependency property.</returns>
	public static readonly DependencyProperty IsStylusDirectlyOverProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.ContentElement.IsStylusCaptured" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.ContentElement.IsStylusCaptured" /> dependency property.</returns>
	public static readonly DependencyProperty IsStylusCapturedProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.ContentElement.IsStylusCaptureWithin" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.ContentElement.IsStylusCaptureWithin" /> dependency property.</returns>
	public static readonly DependencyProperty IsStylusCaptureWithinProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.ContentElement.IsKeyboardFocused" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.ContentElement.IsKeyboardFocused" /> dependency property.</returns>
	public static readonly DependencyProperty IsKeyboardFocusedProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.ContentElement.AreAnyTouchesDirectlyOver" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.ContentElement.AreAnyTouchesDirectlyOver" /> dependency property.</returns>
	public static readonly DependencyProperty AreAnyTouchesDirectlyOverProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.ContentElement.AreAnyTouchesOver" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.ContentElement.AreAnyTouchesOver" /> dependency property.</returns>
	public static readonly DependencyProperty AreAnyTouchesOverProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.ContentElement.AreAnyTouchesCaptured" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.ContentElement.AreAnyTouchesCaptured" /> dependency property.</returns>
	public static readonly DependencyProperty AreAnyTouchesCapturedProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.ContentElement.AreAnyTouchesCapturedWithin" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.ContentElement.AreAnyTouchesCapturedWithin" /> dependency property.</returns>
	public static readonly DependencyProperty AreAnyTouchesCapturedWithinProperty;

	private CoreFlags _flags;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.GotFocus" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.GotFocus" /> routed event.</returns>
	public static readonly RoutedEvent GotFocusEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.ContentElement.LostFocus" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.ContentElement.LostFocus" /> routed event.</returns>
	public static readonly RoutedEvent LostFocusEvent;

	/// <summary>Identifies the <see cref="P:System.Windows.ContentElement.IsFocused" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.ContentElement.IsFocused" /> dependency property identifier.</returns>
	public static readonly DependencyProperty IsFocusedProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.ContentElement.IsEnabled" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.ContentElement.IsEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty IsEnabledProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.ContentElement.Focusable" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.ContentElement.Focusable" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty FocusableProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.ContentElement.AllowDrop" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.ContentElement.AllowDrop" /> dependency property.</returns>
	public static readonly DependencyProperty AllowDropProperty;

	internal DependencyObject _parent;

	internal static readonly UncommonField<EventHandlersStore> EventHandlersStoreField;

	internal static readonly UncommonField<InputBindingCollection> InputBindingCollectionField;

	internal static readonly UncommonField<CommandBindingCollection> CommandBindingCollectionField;

	private static readonly UncommonField<AutomationPeer> AutomationPeerField;

	private static DependencyObjectType ContentElementType;

	/// <summary>Gets a value that indicates whether this element has any animated properties. </summary>
	/// <returns>true if this element has animations attached to any of its properties; otherwise, false.</returns>
	public bool HasAnimatedProperties
	{
		get
		{
			VerifyAccess();
			return base.IAnimatable_HasAnimatedProperties;
		}
	}

	/// <summary>Gets the collection of input bindings that are associated with this element. </summary>
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

	/// <summary>Gets a collection of <see cref="T:System.Windows.Input.CommandBinding" /> objects that are associated with this element. </summary>
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

	internal DependencyObject Parent => _parent;

	/// <summary>Gets a value that indicates whether the position of the mouse pointer corresponds to hit test results, which take element compositing into account.  </summary>
	/// <returns>true if the mouse pointer is over the same element result as a hit test; otherwise, false. The default is false.</returns>
	public bool IsMouseDirectlyOver => IsMouseDirectlyOver_ComputeValue();

	/// <summary>Gets a value that indicates whether the mouse pointer is located over this element (including visual child elements, or its control compositing). </summary>
	/// <returns>true if mouse pointer is over the element or its child elements; otherwise, false. The default is false.</returns>
	public bool IsMouseOver => ReadFlag(CoreFlags.IsMouseOverCache);

	/// <summary>Gets a value that indicates whether the stylus is located over this element (including visual child elements).  </summary>
	/// <returns>true if the stylus is over the element or its child elements; otherwise, false. The default is false.</returns>
	public bool IsStylusOver => ReadFlag(CoreFlags.IsStylusOverCache);

	/// <summary>Gets a value that indicates whether keyboard focus is anywhere within the element or child elements.  </summary>
	/// <returns>true if keyboard focus is on the element or its child elements; otherwise, false. The default is false.</returns>
	public bool IsKeyboardFocusWithin => ReadFlag(CoreFlags.IsKeyboardFocusWithinCache);

	/// <summary>Gets a value that indicates whether the mouse is captured by this element.  </summary>
	/// <returns>true if the element has mouse capture; otherwise, false. The default is false.</returns>
	public bool IsMouseCaptured => (bool)GetValue(IsMouseCapturedProperty);

	/// <summary>Gets a value that determines whether mouse capture is held by this element or by child elements in its element tree. </summary>
	/// <returns>true if this element or a contained element has mouse capture; otherwise, false. The default is false.</returns>
	public bool IsMouseCaptureWithin => ReadFlag(CoreFlags.IsMouseCaptureWithinCache);

	/// <summary>Gets a value that indicates whether the stylus position corresponds to hit test results, which take element compositing into account.  </summary>
	/// <returns>true if the stylus is over the same element as a hit test; otherwise, false. The default is false.</returns>
	public bool IsStylusDirectlyOver => IsStylusDirectlyOver_ComputeValue();

	/// <summary>Gets a value that indicates whether the stylus is captured to this element.  </summary>
	/// <returns>true if the element has stylus capture; otherwise, false. The default is false.</returns>
	public bool IsStylusCaptured => (bool)GetValue(IsStylusCapturedProperty);

	/// <summary>Gets a value that determines whether stylus capture is held by this element, including child elements and control compositing. </summary>
	/// <returns>true if stylus capture is held within this element; otherwise, false.</returns>
	public bool IsStylusCaptureWithin => ReadFlag(CoreFlags.IsStylusCaptureWithinCache);

	/// <summary>Gets a value that indicates whether this element has keyboard focus.  </summary>
	/// <returns>true if this element has keyboard focus; otherwise, false. The default is false.</returns>
	public bool IsKeyboardFocused => IsKeyboardFocused_ComputeValue();

	/// <summary>Gets a value that determines whether this element has logical focus.  </summary>
	/// <returns>true if this element has logical focus; otherwise, false.</returns>
	public bool IsFocused => (bool)GetValue(IsFocusedProperty);

	/// <summary>Gets or sets a value that indicates whether this element is enabled in the user interface (UI).  </summary>
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

	/// <summary>Gets a value that becomes the return value of <see cref="P:System.Windows.ContentElement.IsEnabled" /> in derived classes. </summary>
	/// <returns>true if the element is enabled; otherwise, false.</returns>
	protected virtual bool IsEnabledCore => true;

	/// <summary>Gets or sets a value that indicates whether the element can receive focus.  </summary>
	/// <returns>true if the element is focusable; otherwise false. The default is false; however, see Remarks.</returns>
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

	/// <summary>Gets a value that indicates whether an input method system, such as an Input Method Editor (IME), is enabled for processing the input to this element. </summary>
	/// <returns>true if an input method is active; otherwise, false. The default value of the underlying attached property is true; however, this value is influenced by the state of input methods at runtime.</returns>
	public bool IsInputMethodEnabled => (bool)GetValue(InputMethod.IsInputMethodEnabledProperty);

	/// <summary>Gets or sets a value that indicates whether this element can be used as the target of a drag-and-drop operation.  </summary>
	/// <returns>true if this element can be used as the target of a drag-and-drop operation; otherwise, false. The default value is false.</returns>
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
			AddHandler(UIElement.PreviewMouseLeftButtonDownEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(UIElement.PreviewMouseLeftButtonDownEvent, value);
		}
	}

	/// <summary>Occurs when the left mouse button is pressed while the mouse pointer is over this element. </summary>
	public event MouseButtonEventHandler MouseLeftButtonDown
	{
		add
		{
			AddHandler(UIElement.MouseLeftButtonDownEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(UIElement.MouseLeftButtonDownEvent, value);
		}
	}

	/// <summary>Occurs when the left mouse button is released while the mouse pointer is over this element. </summary>
	public event MouseButtonEventHandler PreviewMouseLeftButtonUp
	{
		add
		{
			AddHandler(UIElement.PreviewMouseLeftButtonUpEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(UIElement.PreviewMouseLeftButtonUpEvent, value);
		}
	}

	/// <summary>Occurs when the left mouse button is released while the mouse pointer is over this element. </summary>
	public event MouseButtonEventHandler MouseLeftButtonUp
	{
		add
		{
			AddHandler(UIElement.MouseLeftButtonUpEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(UIElement.MouseLeftButtonUpEvent, value);
		}
	}

	/// <summary>Occurs when the right mouse button is pressed while the mouse pointer is over this element. </summary>
	public event MouseButtonEventHandler PreviewMouseRightButtonDown
	{
		add
		{
			AddHandler(UIElement.PreviewMouseRightButtonDownEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(UIElement.PreviewMouseRightButtonDownEvent, value);
		}
	}

	/// <summary>Occurs when the right mouse button is pressed while the mouse pointer is over this element. </summary>
	public event MouseButtonEventHandler MouseRightButtonDown
	{
		add
		{
			AddHandler(UIElement.MouseRightButtonDownEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(UIElement.MouseRightButtonDownEvent, value);
		}
	}

	/// <summary>Occurs when the right mouse button is released while the mouse pointer is over this element. </summary>
	public event MouseButtonEventHandler PreviewMouseRightButtonUp
	{
		add
		{
			AddHandler(UIElement.PreviewMouseRightButtonUpEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(UIElement.PreviewMouseRightButtonUpEvent, value);
		}
	}

	/// <summary>Occurs when the right mouse button is released while the mouse pointer is over this element. </summary>
	public event MouseButtonEventHandler MouseRightButtonUp
	{
		add
		{
			AddHandler(UIElement.MouseRightButtonUpEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(UIElement.MouseRightButtonUpEvent, value);
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

	/// <summary>Occurs when the stylus moves while over the element. The stylus must move while being detected by the  digitizer  to raise this event, otherwise, <see cref="E:System.Windows.ContentElement.PreviewStylusInAirMove" /> is raised instead.</summary>
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

	/// <summary>Occurs when the stylus moves over this element. The stylus must move while on the digitizer to raise this event. Otherwise, <see cref="E:System.Windows.ContentElement.StylusInAirMove" /> is raised instead.</summary>
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

	/// <summary>Occurs when a key is pressed while the keyboard is focused on this element. </summary>
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

	/// <summary>Occurs when a key is released while the keyboard is focused on this element. </summary>
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

	/// <summary>Occurs when the keyboard is no longer focused on this element. </summary>
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

	/// <summary>Occurs when the value of the <see cref="P:System.Windows.ContentElement.IsMouseDirectlyOver" /> property changes on this element. </summary>
	public event DependencyPropertyChangedEventHandler IsMouseDirectlyOverChanged
	{
		add
		{
			EventHandlersStoreAdd(UIElement.IsMouseDirectlyOverChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(UIElement.IsMouseDirectlyOverChangedKey, value);
		}
	}

	/// <summary>Occurs when the value of the <see cref="E:System.Windows.ContentElement.IsKeyboardFocusWithinChanged" /> property changes on this element. </summary>
	public event DependencyPropertyChangedEventHandler IsKeyboardFocusWithinChanged
	{
		add
		{
			EventHandlersStoreAdd(UIElement.IsKeyboardFocusWithinChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(UIElement.IsKeyboardFocusWithinChangedKey, value);
		}
	}

	/// <summary>Occurs when the value of the <see cref="P:System.Windows.ContentElement.IsMouseCaptured" /> property changes on this element. </summary>
	public event DependencyPropertyChangedEventHandler IsMouseCapturedChanged
	{
		add
		{
			EventHandlersStoreAdd(UIElement.IsMouseCapturedChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(UIElement.IsMouseCapturedChangedKey, value);
		}
	}

	/// <summary>Occurs when the value of the <see cref="F:System.Windows.ContentElement.IsMouseCaptureWithinProperty" /> changes on this element.</summary>
	public event DependencyPropertyChangedEventHandler IsMouseCaptureWithinChanged
	{
		add
		{
			EventHandlersStoreAdd(UIElement.IsMouseCaptureWithinChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(UIElement.IsMouseCaptureWithinChangedKey, value);
		}
	}

	/// <summary>Occurs when the value of the <see cref="P:System.Windows.ContentElement.IsStylusDirectlyOver" /> property changes on this element. </summary>
	public event DependencyPropertyChangedEventHandler IsStylusDirectlyOverChanged
	{
		add
		{
			EventHandlersStoreAdd(UIElement.IsStylusDirectlyOverChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(UIElement.IsStylusDirectlyOverChangedKey, value);
		}
	}

	/// <summary>Occurs when the value of the <see cref="P:System.Windows.ContentElement.IsStylusCaptured" /> property changes on this element. </summary>
	public event DependencyPropertyChangedEventHandler IsStylusCapturedChanged
	{
		add
		{
			EventHandlersStoreAdd(UIElement.IsStylusCapturedChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(UIElement.IsStylusCapturedChangedKey, value);
		}
	}

	/// <summary>Occurs when the value of the <see cref="P:System.Windows.ContentElement.IsStylusCaptureWithin" /> property changes on this element. </summary>
	public event DependencyPropertyChangedEventHandler IsStylusCaptureWithinChanged
	{
		add
		{
			EventHandlersStoreAdd(UIElement.IsStylusCaptureWithinChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(UIElement.IsStylusCaptureWithinChangedKey, value);
		}
	}

	/// <summary>Occurs when the value of the <see cref="P:System.Windows.ContentElement.IsKeyboardFocused" /> property changes on this element. </summary>
	public event DependencyPropertyChangedEventHandler IsKeyboardFocusedChanged
	{
		add
		{
			EventHandlersStoreAdd(UIElement.IsKeyboardFocusedChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(UIElement.IsKeyboardFocusedChangedKey, value);
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

	/// <summary>Occurs when the value of the <see cref="P:System.Windows.ContentElement.IsEnabled" /> property on this element changes. </summary>
	public event DependencyPropertyChangedEventHandler IsEnabledChanged
	{
		add
		{
			EventHandlersStoreAdd(UIElement.IsEnabledChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(UIElement.IsEnabledChangedKey, value);
		}
	}

	/// <summary>Occurs when the value of the <see cref="P:System.Windows.ContentElement.Focusable" /> property changes.</summary>
	public event DependencyPropertyChangedEventHandler FocusableChanged
	{
		add
		{
			EventHandlersStoreAdd(UIElement.FocusableChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(UIElement.FocusableChangedKey, value);
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
	/// <param name="dp">The.dependency property to check.</param>
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

	/// <summary>Returns whether serialization processes should serialize the contents of the <see cref="P:System.Windows.ContentElement.InputBindings" /> property on instances of this class.</summary>
	/// <returns>true if the <see cref="P:System.Windows.ContentElement.InputBindings" /> property value should be serialized; otherwise, false.</returns>
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

	/// <summary>Returns whether serialization processes should serialize the contents of the <see cref="P:System.Windows.ContentElement.CommandBindings" /> property on instances of this class.</summary>
	/// <returns>true if the <see cref="P:System.Windows.ContentElement.CommandBindings" /> property value should be serialized; otherwise, false.</returns>
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
		UIElement.BuildRouteHelper(this, route, args);
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
		UIElement.RaiseEventImpl(this, e);
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
		UIElement.RaiseEventImpl(this, args);
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
			UIElement.RaiseEventImpl(this, args);
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
	/// <param name="routedEvent">An identifier for the routed event to be handled.</param>
	/// <param name="handler">A reference to the handler implementation.</param>
	public void AddHandler(RoutedEvent routedEvent, Delegate handler)
	{
		AddHandler(routedEvent, handler, handledEventsToo: false);
	}

	/// <summary>Adds a routed event handler for a specified routed event, adding the handler to the handler collection on the current element. Specify <paramref name="handledEventsToo" /> as true to have the provided handler be invoked for routed event that had already been marked as handled by another element along the event route. </summary>
	/// <param name="routedEvent">An identifier for the.routed event to be handled.</param>
	/// <param name="handler">A reference to the handler implementation.</param>
	/// <param name="handledEventsToo">true to register the handler such that it is invoked even when the routed event is marked handled in its event data; false to register the handler with the default condition that it will not be invoked if the routed event is already marked handled. The default is false.Do not routinely ask to rehandle a routed event. For more information, see Remarks.</param>
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

	/// <summary>Removes the specified routed event handler from this element. </summary>
	/// <param name="routedEvent">The identifier of the.routed event for which the handler is attached.</param>
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

	/// <summary>Adds handlers to the specified <see cref="T:System.Windows.EventRoute" /> for the current <see cref="T:System.Windows.ContentElement" /> event handler collection.</summary>
	/// <param name="route">The event route that handlers are added to.</param>
	/// <param name="e">The event data that is used to add the handlers. This method uses the <see cref="P:System.Windows.RoutedEventArgs.RoutedEvent" /> property of the arguments to create the handlers. </param>
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

	private static void RegisterProperties()
	{
		UIElement.IsMouseDirectlyOverPropertyKey.OverrideMetadata(_typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox, IsMouseDirectlyOver_Changed));
		UIElement.IsMouseOverPropertyKey.OverrideMetadata(_typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox));
		UIElement.IsStylusOverPropertyKey.OverrideMetadata(_typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox));
		UIElement.IsKeyboardFocusWithinPropertyKey.OverrideMetadata(_typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox));
		UIElement.IsMouseCapturedPropertyKey.OverrideMetadata(_typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox, IsMouseCaptured_Changed));
		UIElement.IsMouseCaptureWithinPropertyKey.OverrideMetadata(_typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox));
		UIElement.IsStylusDirectlyOverPropertyKey.OverrideMetadata(_typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox, IsStylusDirectlyOver_Changed));
		UIElement.IsStylusCapturedPropertyKey.OverrideMetadata(_typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox, IsStylusCaptured_Changed));
		UIElement.IsStylusCaptureWithinPropertyKey.OverrideMetadata(_typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox));
		UIElement.IsKeyboardFocusedPropertyKey.OverrideMetadata(_typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox, IsKeyboardFocused_Changed));
		UIElement.AreAnyTouchesDirectlyOverPropertyKey.OverrideMetadata(_typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox));
		UIElement.AreAnyTouchesOverPropertyKey.OverrideMetadata(_typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox));
		UIElement.AreAnyTouchesCapturedPropertyKey.OverrideMetadata(_typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox));
		UIElement.AreAnyTouchesCapturedWithinPropertyKey.OverrideMetadata(_typeofThis, new PropertyMetadata(BooleanBoxes.FalseBox));
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.PreviewMouseDown" /> attached routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that one or more mouse buttons were pressed.</param>
	protected internal virtual void OnPreviewMouseDown(MouseButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseDown" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. This event data reports details about the mouse button that was pressed and the handled state.</param>
	protected internal virtual void OnMouseDown(MouseButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.PreviewMouseUp" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.</summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that one or more mouse buttons were released.</param>
	protected internal virtual void OnPreviewMouseUp(MouseButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseUp" /> routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that the mouse button was released.</param>
	protected internal virtual void OnMouseUp(MouseButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.ContentElement.PreviewMouseLeftButtonDown" /> routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.</summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that the left mouse button was pressed.</param>
	protected internal virtual void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.ContentElement.MouseLeftButtonDown" /> routed event is raised on this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that the left mouse button was pressed.</param>
	protected internal virtual void OnMouseLeftButtonDown(MouseButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.ContentElement.PreviewMouseLeftButtonUp" /> routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.</summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that the left mouse button was released.</param>
	protected internal virtual void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.ContentElement.MouseLeftButtonUp" /> routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that the left mouse button was released.</param>
	protected internal virtual void OnMouseLeftButtonUp(MouseButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.ContentElement.PreviewMouseRightButtonDown" /> routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.</summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that the right mouse button was pressed.</param>
	protected internal virtual void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.ContentElement.MouseRightButtonDown" /> routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that the right mouse button was pressed.</param>
	protected internal virtual void OnMouseRightButtonDown(MouseButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.ContentElement.PreviewMouseRightButtonUp" /> routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.</summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that the right mouse button was released.</param>
	protected internal virtual void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.ContentElement.MouseRightButtonUp" /> routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that the right mouse button was released.</param>
	protected internal virtual void OnMouseRightButtonUp(MouseButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.PreviewMouseMove" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewMouseMove(MouseEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseMove" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnMouseMove(MouseEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.PreviewMouseWheel" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseWheelEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewMouseWheel(MouseWheelEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseWheel" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseWheelEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnMouseWheel(MouseWheelEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseEnter" /> attached event is raised on this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnMouseEnter(MouseEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseLeave" /> attached event is raised on this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnMouseLeave(MouseEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.GotMouseCapture" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnGotMouseCapture(MouseEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.LostMouseCapture" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains event data.</param>
	protected internal virtual void OnLostMouseCapture(MouseEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.QueryCursor" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.QueryCursorEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnQueryCursor(QueryCursorEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.PreviewStylusDown" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.</summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusDownEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewStylusDown(StylusDownEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.StylusDown" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusDownEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnStylusDown(StylusDownEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.PreviewStylusUp" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewStylusUp(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.StylusUp" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnStylusUp(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.PreviewStylusMove" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewStylusMove(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.StylusMove" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnStylusMove(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.PreviewStylusInAirMove" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewStylusInAirMove(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.StylusInAirMove" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnStylusInAirMove(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.StylusEnter" /> attached event is raised by this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnStylusEnter(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.StylusLeave" /> attached event is raised by this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnStylusLeave(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.PreviewStylusInRange" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewStylusInRange(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.StylusInRange" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnStylusInRange(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.PreviewStylusOutOfRange" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewStylusOutOfRange(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.StylusOutOfRange" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnStylusOutOfRange(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.PreviewStylusSystemGesture" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusSystemGestureEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewStylusSystemGesture(StylusSystemGestureEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.StylusSystemGesture" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusSystemGestureEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnStylusSystemGesture(StylusSystemGestureEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.GotStylusCapture" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnGotStylusCapture(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.LostStylusCapture" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusEventArgs" /> that contains event data.</param>
	protected internal virtual void OnLostStylusCapture(StylusEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.StylusButtonDown" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusButtonEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnStylusButtonDown(StylusButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.StylusButtonUp" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusButtonEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnStylusButtonUp(StylusButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.PreviewStylusButtonDown" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusButtonEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewStylusButtonDown(StylusButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Stylus.PreviewStylusButtonUp" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.StylusButtonEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewStylusButtonUp(StylusButtonEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Keyboard.PreviewKeyDown" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.KeyEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewKeyDown(KeyEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Keyboard.KeyDown" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.KeyEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnKeyDown(KeyEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Keyboard.PreviewKeyUp" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.KeyEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewKeyUp(KeyEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Keyboard.KeyUp" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.KeyEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnKeyUp(KeyEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Keyboard.PreviewGotKeyboardFocus" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.KeyboardFocusChangedEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Keyboard.GotKeyboardFocus" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.KeyboardFocusChangedEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Keyboard.PreviewKeyDown" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.KeyboardFocusChangedEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Keyboard.LostKeyboardFocus" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.KeyboardFocusChangedEventArgs" /> that contains event data.</param>
	protected internal virtual void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.TextCompositionManager.PreviewTextInput" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.TextCompositionEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewTextInput(TextCompositionEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.TextCompositionManager.TextInput" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.TextCompositionEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnTextInput(TextCompositionEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.DragDrop.PreviewQueryContinueDrag" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.QueryContinueDragEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewQueryContinueDrag(QueryContinueDragEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.DragDrop.QueryContinueDrag" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.QueryContinueDragEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnQueryContinueDrag(QueryContinueDragEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.DragDrop.PreviewGiveFeedback" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.GiveFeedbackEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewGiveFeedback(GiveFeedbackEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.DragDrop.GiveFeedback" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.GiveFeedbackEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnGiveFeedback(GiveFeedbackEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.DragDrop.PreviewDragEnter" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DragEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewDragEnter(DragEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.DragDrop.DragEnter" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DragEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnDragEnter(DragEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.DragDrop.PreviewDragOver" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DragEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewDragOver(DragEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.DragDrop.DragOver" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DragEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnDragOver(DragEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.DragDrop.PreviewDragLeave" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DragEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewDragLeave(DragEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.DragDrop.DragLeave" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DragEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnDragLeave(DragEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.DragDrop.PreviewDrop" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DragEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewDrop(DragEventArgs e)
	{
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.DragDrop.DragEnter" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DragEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnDrop(DragEventArgs e)
	{
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.ContentElement.PreviewTouchDown" /> routed event that occurs when a touch presses this element. </summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.TouchEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewTouchDown(TouchEventArgs e)
	{
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.ContentElement.TouchDown" /> routed event that occurs when a touch presses inside this element.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.TouchEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnTouchDown(TouchEventArgs e)
	{
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.ContentElement.PreviewTouchMove" /> routed event that occurs when a touch moves while inside this element.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.TouchEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewTouchMove(TouchEventArgs e)
	{
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.ContentElement.TouchMove" /> routed event that occurs when a touch moves while inside this element.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.TouchEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnTouchMove(TouchEventArgs e)
	{
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.ContentElement.PreviewTouchUp" /> routed event that occurs when a touch is released inside this element. </summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.TouchEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnPreviewTouchUp(TouchEventArgs e)
	{
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.ContentElement.TouchUp" /> routed event that occurs when a touch is released inside this element.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.TouchEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnTouchUp(TouchEventArgs e)
	{
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.ContentElement.GotTouchCapture" /> routed event that occurs when a touch is captured to this element.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.TouchEventArgs" /> that contains the event data. </param>
	protected internal virtual void OnGotTouchCapture(TouchEventArgs e)
	{
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.ContentElement.LostTouchCapture" /> routed event that occurs when this element loses a touch capture.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.TouchEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnLostTouchCapture(TouchEventArgs e)
	{
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.ContentElement.TouchEnter" /> routed event that occurs when a touch moves from outside to inside the bounds of this element. </summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.TouchEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnTouchEnter(TouchEventArgs e)
	{
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.ContentElement.TouchLeave" /> routed event that occurs when a touch moves from inside to outside the bounds of this element. </summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.TouchEventArgs" /> that contains the event data.</param>
	protected internal virtual void OnTouchLeave(TouchEventArgs e)
	{
	}

	private static void IsMouseDirectlyOver_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ContentElement)d).RaiseIsMouseDirectlyOverChanged(e);
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.ContentElement.IsMouseDirectlyOverChanged" /> event is raised on this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> that contains the event data.</param>
	protected virtual void OnIsMouseDirectlyOverChanged(DependencyPropertyChangedEventArgs e)
	{
	}

	private void RaiseIsMouseDirectlyOverChanged(DependencyPropertyChangedEventArgs args)
	{
		OnIsMouseDirectlyOverChanged(args);
		RaiseDependencyPropertyChanged(UIElement.IsMouseDirectlyOverChangedKey, args);
	}

	/// <summary>Invoked just before the <see cref="E:System.Windows.ContentElement.IsKeyboardFocusWithinChanged" /> event is raised by this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">A <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> that contains the event data.</param>
	protected virtual void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
	{
	}

	internal void RaiseIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs args)
	{
		OnIsKeyboardFocusWithinChanged(args);
		RaiseDependencyPropertyChanged(UIElement.IsKeyboardFocusWithinChangedKey, args);
	}

	private static void IsMouseCaptured_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ContentElement)d).RaiseIsMouseCapturedChanged(e);
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.ContentElement.IsMouseCapturedChanged" /> event is raised on this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> that contains the event data.</param>
	protected virtual void OnIsMouseCapturedChanged(DependencyPropertyChangedEventArgs e)
	{
	}

	private void RaiseIsMouseCapturedChanged(DependencyPropertyChangedEventArgs args)
	{
		OnIsMouseCapturedChanged(args);
		RaiseDependencyPropertyChanged(UIElement.IsMouseCapturedChangedKey, args);
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.ContentElement.IsMouseCaptureWithinChanged" /> event is raised on this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">A <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> that contains the event data.</param>
	protected virtual void OnIsMouseCaptureWithinChanged(DependencyPropertyChangedEventArgs e)
	{
	}

	internal void RaiseIsMouseCaptureWithinChanged(DependencyPropertyChangedEventArgs args)
	{
		OnIsMouseCaptureWithinChanged(args);
		RaiseDependencyPropertyChanged(UIElement.IsMouseCaptureWithinChangedKey, args);
	}

	private static void IsStylusDirectlyOver_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ContentElement)d).RaiseIsStylusDirectlyOverChanged(e);
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.ContentElement.IsStylusDirectlyOverChanged" /> event is raised on this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> that contains the event data.</param>
	protected virtual void OnIsStylusDirectlyOverChanged(DependencyPropertyChangedEventArgs e)
	{
	}

	private void RaiseIsStylusDirectlyOverChanged(DependencyPropertyChangedEventArgs args)
	{
		OnIsStylusDirectlyOverChanged(args);
		RaiseDependencyPropertyChanged(UIElement.IsStylusDirectlyOverChangedKey, args);
	}

	private static void IsStylusCaptured_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ContentElement)d).RaiseIsStylusCapturedChanged(e);
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.ContentElement.IsStylusCapturedChanged" /> event is raised on this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">A <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> that contains the event data.</param>
	protected virtual void OnIsStylusCapturedChanged(DependencyPropertyChangedEventArgs e)
	{
	}

	private void RaiseIsStylusCapturedChanged(DependencyPropertyChangedEventArgs args)
	{
		OnIsStylusCapturedChanged(args);
		RaiseDependencyPropertyChanged(UIElement.IsStylusCapturedChangedKey, args);
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.ContentElement.IsStylusCaptureWithinChanged" /> event is raised on this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> that contains the event data.</param>
	protected virtual void OnIsStylusCaptureWithinChanged(DependencyPropertyChangedEventArgs e)
	{
	}

	internal void RaiseIsStylusCaptureWithinChanged(DependencyPropertyChangedEventArgs args)
	{
		OnIsStylusCaptureWithinChanged(args);
		RaiseDependencyPropertyChanged(UIElement.IsStylusCaptureWithinChangedKey, args);
	}

	private static void IsKeyboardFocused_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ContentElement)d).RaiseIsKeyboardFocusedChanged(e);
	}

	/// <summary>Invoked when an unhandled <see cref="E:System.Windows.ContentElement.IsKeyboardFocusedChanged" /> event is raised on this element. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> that contains the event data.</param>
	protected virtual void OnIsKeyboardFocusedChanged(DependencyPropertyChangedEventArgs e)
	{
	}

	private void RaiseIsKeyboardFocusedChanged(DependencyPropertyChangedEventArgs args)
	{
		OnIsKeyboardFocusedChanged(args);
		RaiseDependencyPropertyChanged(UIElement.IsKeyboardFocusedChangedKey, args);
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

	static ContentElement()
	{
		_typeofThis = typeof(ContentElement);
		PreviewMouseDownEvent = Mouse.PreviewMouseDownEvent.AddOwner(_typeofThis);
		MouseDownEvent = Mouse.MouseDownEvent.AddOwner(_typeofThis);
		PreviewMouseUpEvent = Mouse.PreviewMouseUpEvent.AddOwner(_typeofThis);
		MouseUpEvent = Mouse.MouseUpEvent.AddOwner(_typeofThis);
		PreviewMouseLeftButtonDownEvent = UIElement.PreviewMouseLeftButtonDownEvent.AddOwner(_typeofThis);
		MouseLeftButtonDownEvent = UIElement.MouseLeftButtonDownEvent.AddOwner(_typeofThis);
		PreviewMouseLeftButtonUpEvent = UIElement.PreviewMouseLeftButtonUpEvent.AddOwner(_typeofThis);
		MouseLeftButtonUpEvent = UIElement.MouseLeftButtonUpEvent.AddOwner(_typeofThis);
		PreviewMouseRightButtonDownEvent = UIElement.PreviewMouseRightButtonDownEvent.AddOwner(_typeofThis);
		MouseRightButtonDownEvent = UIElement.MouseRightButtonDownEvent.AddOwner(_typeofThis);
		PreviewMouseRightButtonUpEvent = UIElement.PreviewMouseRightButtonUpEvent.AddOwner(_typeofThis);
		MouseRightButtonUpEvent = UIElement.MouseRightButtonUpEvent.AddOwner(_typeofThis);
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
		IsMouseDirectlyOverProperty = UIElement.IsMouseDirectlyOverProperty.AddOwner(_typeofThis);
		IsMouseOverProperty = UIElement.IsMouseOverProperty.AddOwner(_typeofThis);
		IsStylusOverProperty = UIElement.IsStylusOverProperty.AddOwner(_typeofThis);
		IsKeyboardFocusWithinProperty = UIElement.IsKeyboardFocusWithinProperty.AddOwner(_typeofThis);
		IsMouseCapturedProperty = UIElement.IsMouseCapturedProperty.AddOwner(_typeofThis);
		IsMouseCaptureWithinProperty = UIElement.IsMouseCaptureWithinProperty.AddOwner(_typeofThis);
		IsStylusDirectlyOverProperty = UIElement.IsStylusDirectlyOverProperty.AddOwner(_typeofThis);
		IsStylusCapturedProperty = UIElement.IsStylusCapturedProperty.AddOwner(_typeofThis);
		IsStylusCaptureWithinProperty = UIElement.IsStylusCaptureWithinProperty.AddOwner(_typeofThis);
		IsKeyboardFocusedProperty = UIElement.IsKeyboardFocusedProperty.AddOwner(_typeofThis);
		AreAnyTouchesDirectlyOverProperty = UIElement.AreAnyTouchesDirectlyOverProperty.AddOwner(_typeofThis);
		AreAnyTouchesOverProperty = UIElement.AreAnyTouchesOverProperty.AddOwner(_typeofThis);
		AreAnyTouchesCapturedProperty = UIElement.AreAnyTouchesCapturedProperty.AddOwner(_typeofThis);
		AreAnyTouchesCapturedWithinProperty = UIElement.AreAnyTouchesCapturedWithinProperty.AddOwner(_typeofThis);
		GotFocusEvent = FocusManager.GotFocusEvent.AddOwner(typeof(ContentElement));
		LostFocusEvent = FocusManager.LostFocusEvent.AddOwner(typeof(ContentElement));
		IsFocusedProperty = UIElement.IsFocusedProperty.AddOwner(typeof(ContentElement));
		IsEnabledProperty = UIElement.IsEnabledProperty.AddOwner(typeof(ContentElement), new UIPropertyMetadata(BooleanBoxes.TrueBox, OnIsEnabledChanged, CoerceIsEnabled));
		FocusableProperty = UIElement.FocusableProperty.AddOwner(typeof(ContentElement), new UIPropertyMetadata(BooleanBoxes.FalseBox, OnFocusableChanged));
		AllowDropProperty = UIElement.AllowDropProperty.AddOwner(typeof(ContentElement), new PropertyMetadata(BooleanBoxes.FalseBox));
		EventHandlersStoreField = UIElement.EventHandlersStoreField;
		InputBindingCollectionField = UIElement.InputBindingCollectionField;
		CommandBindingCollectionField = UIElement.CommandBindingCollectionField;
		AutomationPeerField = new UncommonField<AutomationPeer>();
		ContentElementType = DependencyObjectType.FromSystemTypeInternal(typeof(ContentElement));
		UIElement.RegisterEvents(typeof(ContentElement));
		RegisterProperties();
		UIElement.IsFocusedPropertyKey.OverrideMetadata(typeof(ContentElement), new PropertyMetadata(BooleanBoxes.FalseBox, IsFocused_Changed));
	}

	internal DependencyObject GetUIParent()
	{
		return GetUIParent(continuePastVisualTree: false);
	}

	internal DependencyObject GetUIParent(bool continuePastVisualTree)
	{
		DependencyObject dependencyObject = null;
		dependencyObject = InputElement.GetContainingInputElement(_parent) as DependencyObject;
		if (dependencyObject == null && continuePastVisualTree)
		{
			dependencyObject = InputElement.GetContainingInputElement(GetUIParentCore()) as DependencyObject;
		}
		return dependencyObject;
	}

	/// <summary>When overridden in a derived class, returns an alternative user interface (UI) parent for this element if no visual parent exists. </summary>
	/// <returns>An object, if implementation of a derived class has an alternate parent connection to report.</returns>
	protected internal virtual DependencyObject GetUIParentCore()
	{
		return null;
	}

	[FriendAccessAllowed]
	internal virtual void OnContentParentChanged(DependencyObject oldParent)
	{
		SynchronizeReverseInheritPropertyFlags(oldParent, isCoreParent: true);
	}

	/// <summary>Returns class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementations for the Windows Presentation Foundation (WPF) infrastructure.</summary>
	/// <returns>The type-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation.</returns>
	protected virtual AutomationPeer OnCreateAutomationPeer()
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
			automationPeer = OnCreateAutomationPeer();
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

	internal void AddSynchronizedInputPreOpportunityHandler(EventRoute route, RoutedEventArgs args)
	{
		if (InputManager.IsSynchronizedInput && SynchronizedInputHelper.IsListening(this, args))
		{
			RoutedEventHandler eventHandler = SynchronizedInputPreOpportunityHandler;
			SynchronizedInputHelper.AddHandlerToRoute(this, route, eventHandler, handledToo: false);
		}
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
		if (!args.Handled)
		{
			SynchronizedInputHelper.PreOpportunityHandler(sender, args);
		}
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

	/// <summary>Releases the mouse capture, if this element held the capture.</summary>
	public void ReleaseMouseCapture()
	{
		Mouse.Capture(null);
	}

	private bool IsStylusDirectlyOver_ComputeValue()
	{
		return Stylus.DirectlyOver == this;
	}

	/// <summary>Attempts to force capture of the stylus to this element. </summary>
	/// <returns>true if the stylus is successfully captured; otherwise, false. </returns>
	public bool CaptureStylus()
	{
		return Stylus.Capture(this);
	}

	/// <summary>Releases the stylus device capture, if this element held the capture. </summary>
	public void ReleaseStylusCapture()
	{
		Stylus.Capture(null);
	}

	private bool IsKeyboardFocused_ComputeValue()
	{
		return Keyboard.FocusedElement == this;
	}

	/// <summary>Attempts to set focus to this element. </summary>
	/// <returns>true if keyboard focus could be set to this element; false if this method call did not force focus.</returns>
	public bool Focus()
	{
		if (Keyboard.Focus(this) == this)
		{
			TipTsfHelper.Show(this);
			return true;
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
	/// <returns>The element that would have received focus if <see cref="M:System.Windows.ContentElement.MoveFocus(System.Windows.Input.TraversalRequest)" /> were actually invoked.</returns>
	/// <param name="direction">The direction of the requested focus traversal.</param>
	public virtual DependencyObject PredictFocus(FocusNavigationDirection direction)
	{
		return null;
	}

	private static void IsFocused_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ContentElement contentElement = (ContentElement)d;
		if ((bool)e.NewValue)
		{
			contentElement.OnGotFocus(new RoutedEventArgs(GotFocusEvent, contentElement));
		}
		else
		{
			contentElement.OnLostFocus(new RoutedEventArgs(LostFocusEvent, contentElement));
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.ContentElement.GotFocus" /> routed event by using the event data provided. </summary>
	/// <param name="e">A <see cref="T:System.Windows.RoutedEventArgs" /> that contains event data. This event data must contain the identifier for the <see cref="E:System.Windows.ContentElement.GotFocus" /> event.</param>
	protected virtual void OnGotFocus(RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.ContentElement.LostFocus" /> routed event by using the event data that is provided. </summary>
	/// <param name="e">A <see cref="T:System.Windows.RoutedEventArgs" /> that contains event data. This event data must contain the identifier for the <see cref="E:System.Windows.ContentElement.LostFocus" /> event.</param>
	protected virtual void OnLostFocus(RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	private static object CoerceIsEnabled(DependencyObject d, object value)
	{
		ContentElement contentElement = (ContentElement)d;
		if ((bool)value)
		{
			DependencyObject uIParentCore = contentElement.GetUIParentCore();
			if (uIParentCore == null || (bool)uIParentCore.GetValue(IsEnabledProperty))
			{
				return BooleanBoxes.Box(contentElement.IsEnabledCore);
			}
			return BooleanBoxes.FalseBox;
		}
		return BooleanBoxes.FalseBox;
	}

	private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ContentElement obj = (ContentElement)d;
		obj.RaiseDependencyPropertyChanged(UIElement.IsEnabledChangedKey, e);
		obj.InvalidateForceInheritPropertyOnChildren(e.Property);
		InputManager.SafeCurrentNotifyHitTestInvalidated();
	}

	private static void OnFocusableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ContentElement)d).RaiseDependencyPropertyChanged(UIElement.FocusableChangedKey, e);
	}

	private void RaiseMouseButtonEvent(EventPrivateKey key, MouseButtonEventArgs e)
	{
		EventHandlersStore eventHandlersStore = EventHandlersStore;
		if (eventHandlersStore != null)
		{
			Delegate @delegate = eventHandlersStore.Get(key);
			if ((object)@delegate != null)
			{
				((MouseButtonEventHandler)@delegate)(this, e);
			}
		}
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

	internal virtual void InvalidateForceInheritPropertyOnChildren(DependencyProperty property)
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.ContentElement" /> class. </summary>
	public ContentElement()
	{
	}
}
