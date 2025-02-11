using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MS.Internal;

namespace System.Windows.Documents;

internal static class TextEditorMouse
{
	private static bool _selectionChanged;

	internal static void _RegisterClassHandlers(Type controlType, bool registerEventListeners)
	{
		if (registerEventListeners)
		{
			EventManager.RegisterClassHandler(controlType, Mouse.QueryCursorEvent, new QueryCursorEventHandler(OnQueryCursor));
			EventManager.RegisterClassHandler(controlType, Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseDown));
			EventManager.RegisterClassHandler(controlType, Mouse.MouseMoveEvent, new MouseEventHandler(OnMouseMove));
			EventManager.RegisterClassHandler(controlType, Mouse.MouseUpEvent, new MouseButtonEventHandler(OnMouseUp));
		}
	}

	internal static void SetCaretPositionOnMouseEvent(TextEditor This, Point mouseDownPoint, MouseButton changedButton, int clickCount)
	{
		ITextPointer textPositionFromPoint = This.TextView.GetTextPositionFromPoint(mouseDownPoint, snapToText: true);
		if (textPositionFromPoint == null)
		{
			MoveFocusToUiScope(This);
			return;
		}
		TextEditorSelection._ClearSuggestedX(This);
		TextEditorTyping._BreakTypingSequence(This);
		if (This.Selection is TextSelection)
		{
			((TextSelection)This.Selection).ClearSpringloadFormatting();
		}
		This._forceWordSelection = false;
		This._forceParagraphSelection = false;
		if (changedButton == MouseButton.Right || clickCount == 1)
		{
			if (changedButton != 0 || !This._dragDropProcess.SourceOnMouseLeftButtonDown(mouseDownPoint))
			{
				This.Selection.SetSelectionByMouse(textPositionFromPoint, mouseDownPoint);
			}
		}
		else if (clickCount == 2 && (Keyboard.Modifiers & ModifierKeys.Shift) == 0 && This.Selection.IsEmpty)
		{
			This._forceWordSelection = true;
			This._forceParagraphSelection = false;
			This.Selection.SelectWord(textPositionFromPoint);
		}
		else if (clickCount == 3 && (Keyboard.Modifiers & ModifierKeys.Shift) == 0 && This.AcceptsRichContent)
		{
			This._forceParagraphSelection = true;
			This._forceWordSelection = false;
			This.Selection.SelectParagraph(textPositionFromPoint);
		}
	}

	internal static bool IsPointWithinInteractiveArea(TextEditor textEditor, Point point)
	{
		bool flag = IsPointWithinRenderScope(textEditor, point);
		if (flag)
		{
			flag = textEditor.TextView.IsValid;
			if (flag)
			{
				textEditor.UiScope.TransformToDescendant(textEditor.TextView.RenderScope)?.TryTransform(point, out point);
				flag = textEditor.TextView.GetTextPositionFromPoint(point, snapToText: true) != null;
			}
		}
		return flag;
	}

	internal static void OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null)
		{
			return;
		}
		textEditor.CloseToolTip();
		if (!textEditor._IsEnabled || !textEditor.UiScope.Focusable || e.ButtonState == MouseButtonState.Released)
		{
			return;
		}
		e.Handled = true;
		MoveFocusToUiScope(textEditor);
		if (textEditor.UiScope != Keyboard.FocusedElement || e.ChangedButton != 0 || textEditor.TextView == null)
		{
			return;
		}
		textEditor.CompleteComposition();
		if (!textEditor.TextView.IsValid)
		{
			textEditor.TextView.RenderScope.UpdateLayout();
			if (textEditor.TextView == null || !textEditor.TextView.IsValid)
			{
				return;
			}
		}
		if (!IsPointWithinInteractiveArea(textEditor, e.GetPosition(textEditor.UiScope)))
		{
			return;
		}
		textEditor.TextView.ThrottleBackgroundTasksForUserInput();
		Point position = e.GetPosition(textEditor.TextView.RenderScope);
		if (TextEditor.IsTableEditingEnabled && TextRangeEditTables.TableBorderHitTest(textEditor.TextView, position))
		{
			textEditor._tableColResizeInfo = TextRangeEditTables.StartColumnResize(textEditor.TextView, position);
			Invariant.Assert(textEditor._tableColResizeInfo != null);
			textEditor._mouseCapturingInProgress = true;
			try
			{
				textEditor.UiScope.CaptureMouse();
				return;
			}
			finally
			{
				textEditor._mouseCapturingInProgress = false;
			}
		}
		textEditor.Selection.BeginChange();
		try
		{
			SetCaretPositionOnMouseEvent(textEditor, position, e.ChangedButton, e.ClickCount);
			textEditor._mouseCapturingInProgress = true;
			textEditor.UiScope.CaptureMouse();
		}
		finally
		{
			textEditor._mouseCapturingInProgress = false;
			textEditor.Selection.EndChange();
		}
	}

	internal static void OnMouseMove(object sender, MouseEventArgs e)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor != null && textEditor._IsEnabled && textEditor.TextView != null && textEditor.TextView.IsValid)
		{
			if (textEditor.UiScope.IsKeyboardFocused)
			{
				OnMouseMoveWithFocus(textEditor, e);
			}
			else
			{
				OnMouseMoveWithoutFocus(textEditor, e);
			}
		}
	}

	internal static void OnMouseUp(object sender, MouseButtonEventArgs e)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (e.ChangedButton != 0 || e.RightButton != 0 || textEditor == null || !textEditor._IsEnabled || textEditor.TextView == null || !textEditor.TextView.IsValid || !textEditor.UiScope.IsMouseCaptured)
		{
			return;
		}
		e.Handled = true;
		textEditor.CancelExtendSelection();
		Point position = e.GetPosition(textEditor.TextView.RenderScope);
		UpdateCursor(textEditor, position);
		if (textEditor._tableColResizeInfo != null)
		{
			using (textEditor.Selection.DeclareChangeBlock())
			{
				textEditor._tableColResizeInfo.ResizeColumn(position);
				textEditor._tableColResizeInfo = null;
			}
		}
		else
		{
			using (textEditor.Selection.DeclareChangeBlock())
			{
				textEditor._dragDropProcess.DoMouseLeftButtonUp(e);
				textEditor._forceWordSelection = false;
				textEditor._forceParagraphSelection = false;
			}
		}
		textEditor._mouseCapturingInProgress = true;
		try
		{
			textEditor.UiScope.ReleaseMouseCapture();
		}
		finally
		{
			textEditor._mouseCapturingInProgress = false;
		}
	}

	internal static void OnQueryCursor(object sender, QueryCursorEventArgs e)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor != null && textEditor.TextView != null && IsPointWithinInteractiveArea(textEditor, Mouse.GetPosition(textEditor.UiScope)))
		{
			e.Cursor = textEditor._cursor;
			e.Handled = true;
		}
	}

	private static void OnMouseMoveWithoutFocus(TextEditor This, MouseEventArgs e)
	{
		UpdateCursor(This, e.GetPosition(This.TextView.RenderScope));
	}

	private static void OnMouseMoveWithFocus(TextEditor This, MouseEventArgs e)
	{
		if (This._mouseCapturingInProgress)
		{
			return;
		}
		TextEditor._ThreadLocalStore.PureControlShift = false;
		Point position = e.GetPosition(This.TextView.RenderScope);
		UpdateCursor(This, position);
		Invariant.Assert(This.Selection != null);
		if (e.LeftButton != MouseButtonState.Pressed || !This.UiScope.IsMouseCaptured)
		{
			return;
		}
		This.TextView.ThrottleBackgroundTasksForUserInput();
		if (This._tableColResizeInfo != null)
		{
			This._tableColResizeInfo.UpdateAdorner(position);
			return;
		}
		e.Handled = true;
		Invariant.Assert(This.Selection != null);
		ITextPointer textPointer = This.TextView.GetTextPositionFromPoint(position, snapToText: true);
		Invariant.Assert(This.Selection != null);
		if (textPointer == null)
		{
			This.RequestExtendSelection(position);
			return;
		}
		This.CancelExtendSelection();
		Invariant.Assert(This.Selection != null);
		if (This._dragDropProcess.SourceOnMouseMove(position))
		{
			return;
		}
		FrameworkElement scroller = This._Scroller;
		if (scroller != null && This.UiScope is TextBoxBase)
		{
			ITextPointer textPointer2 = null;
			Point point = new Point(position.X, position.Y);
			Point position2 = e.GetPosition(scroller);
			double num = ((TextBoxBase)This.UiScope).ViewportHeight;
			double num2 = 16.0;
			if (position2.Y < 0.0 - num2)
			{
				Rect rectangleFromTextPosition = This.TextView.GetRectangleFromTextPosition(textPointer);
				point = new Point(point.X, rectangleFromTextPosition.Bottom - num);
				textPointer2 = This.TextView.GetTextPositionFromPoint(point, snapToText: true);
			}
			else if (position2.Y > num + num2)
			{
				Rect rectangleFromTextPosition2 = This.TextView.GetRectangleFromTextPosition(textPointer);
				point = new Point(point.X, rectangleFromTextPosition2.Top + num);
				textPointer2 = This.TextView.GetTextPositionFromPoint(point, snapToText: true);
			}
			double num3 = ((TextBoxBase)This.UiScope).ViewportWidth;
			if (position2.X < 0.0)
			{
				point = new Point(point.X - num2, point.Y);
				textPointer2 = This.TextView.GetTextPositionFromPoint(point, snapToText: true);
			}
			else if (position2.X > num3)
			{
				point = new Point(point.X + num2, point.Y);
				textPointer2 = This.TextView.GetTextPositionFromPoint(point, snapToText: true);
			}
			if (textPointer2 != null)
			{
				textPointer = textPointer2;
			}
		}
		using (This.Selection.DeclareChangeBlock())
		{
			if (textPointer.GetNextInsertionPosition(LogicalDirection.Forward) == null && textPointer.ParentType != null)
			{
				Rect characterRect = textPointer.GetCharacterRect(LogicalDirection.Backward);
				if (position.X > characterRect.X + characterRect.Width)
				{
					textPointer = This.TextContainer.End;
				}
			}
			This.Selection.ExtendSelectionByMouse(textPointer, This._forceWordSelection, This._forceParagraphSelection);
		}
	}

	private static bool MoveFocusToUiScope(TextEditor This)
	{
		long contentChangeCounter = This._ContentChangeCounter;
		Visual visual = VisualTreeHelper.GetParent(This.UiScope) as Visual;
		while (visual != null && !(visual is ScrollViewer))
		{
			visual = VisualTreeHelper.GetParent(visual) as Visual;
		}
		if (visual != null)
		{
			((ScrollViewer)visual).AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(OnScrollChangedDuringGotFocus));
		}
		ITextSelection selection = This.Selection;
		try
		{
			selection.Changed += OnSelectionChangedDuringGotFocus;
			_selectionChanged = false;
			This.UiScope.Focus();
		}
		finally
		{
			selection.Changed -= OnSelectionChangedDuringGotFocus;
			if (visual != null)
			{
				((ScrollViewer)visual).RemoveHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(OnScrollChangedDuringGotFocus));
			}
		}
		if (This.UiScope == Keyboard.FocusedElement && contentChangeCounter == This._ContentChangeCounter)
		{
			return !_selectionChanged;
		}
		return false;
	}

	private static void OnSelectionChangedDuringGotFocus(object sender, EventArgs e)
	{
		_selectionChanged = true;
	}

	private static void OnScrollChangedDuringGotFocus(object sender, ScrollChangedEventArgs e)
	{
		if (e.OriginalSource is ScrollViewer scrollViewer)
		{
			scrollViewer.RemoveHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(OnScrollChangedDuringGotFocus));
			scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.HorizontalChange);
			scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.VerticalChange);
		}
	}

	private static void UpdateCursor(TextEditor This, Point mouseMovePoint)
	{
		Invariant.Assert(This.TextView != null && This.TextView.IsValid);
		Cursor cursor = Cursors.IBeam;
		if (TextEditor.IsTableEditingEnabled && TextRangeEditTables.TableBorderHitTest(This.TextView, mouseMovePoint))
		{
			cursor = Cursors.SizeWE;
		}
		else if (This.Selection != null && !This.UiScope.IsMouseCaptured)
		{
			if (This.Selection.IsEmpty)
			{
				UIElement uIElementWhenMouseOver = GetUIElementWhenMouseOver(This, mouseMovePoint);
				if (uIElementWhenMouseOver != null && uIElementWhenMouseOver.IsEnabled)
				{
					cursor = Cursors.Arrow;
				}
			}
			else if (This.UiScope.IsFocused && This.Selection.Contains(mouseMovePoint))
			{
				cursor = Cursors.Arrow;
			}
		}
		if (cursor != This._cursor)
		{
			This._cursor = cursor;
			Mouse.UpdateCursor();
		}
	}

	private static UIElement GetUIElementWhenMouseOver(TextEditor This, Point mouseMovePoint)
	{
		ITextPointer textPositionFromPoint = This.TextView.GetTextPositionFromPoint(mouseMovePoint, snapToText: false);
		if (textPositionFromPoint == null)
		{
			return null;
		}
		if (textPositionFromPoint.GetPointerContext(textPositionFromPoint.LogicalDirection) != TextPointerContext.EmbeddedElement)
		{
			return null;
		}
		ITextPointer nextContextPosition = textPositionFromPoint.GetNextContextPosition(textPositionFromPoint.LogicalDirection);
		LogicalDirection gravity = ((textPositionFromPoint.LogicalDirection != LogicalDirection.Forward) ? LogicalDirection.Forward : LogicalDirection.Backward);
		nextContextPosition = nextContextPosition.CreatePointer(0, gravity);
		Rect rectangleFromTextPosition = This.TextView.GetRectangleFromTextPosition(textPositionFromPoint);
		Rect rectangleFromTextPosition2 = This.TextView.GetRectangleFromTextPosition(nextContextPosition);
		Rect rect = rectangleFromTextPosition;
		rect.Union(rectangleFromTextPosition2);
		if (!rect.Contains(mouseMovePoint))
		{
			return null;
		}
		return textPositionFromPoint.GetAdjacentElement(textPositionFromPoint.LogicalDirection) as UIElement;
	}

	private static bool IsPointWithinRenderScope(TextEditor textEditor, Point point)
	{
		DependencyObject parent = textEditor.TextContainer.Parent;
		UIElement renderScope = textEditor.TextView.RenderScope;
		CaretElement caretElement = textEditor.Selection.CaretElement;
		HitTestResult hitTestResult = VisualTreeHelper.HitTest(textEditor.UiScope, point);
		if (hitTestResult != null)
		{
			bool flag = false;
			if (hitTestResult.VisualHit is Visual)
			{
				flag = ((Visual)hitTestResult.VisualHit).IsDescendantOf(renderScope);
			}
			if (hitTestResult.VisualHit is Visual3D)
			{
				flag = ((Visual3D)hitTestResult.VisualHit).IsDescendantOf(renderScope);
			}
			if (hitTestResult.VisualHit == renderScope || flag || hitTestResult.VisualHit == caretElement)
			{
				return true;
			}
		}
		for (DependencyObject dependencyObject = textEditor.UiScope.InputHitTest(point) as DependencyObject; dependencyObject != null; dependencyObject = ((dependencyObject is FrameworkElement && ((FrameworkElement)dependencyObject).TemplatedParent == textEditor.UiScope) ? null : ((!(dependencyObject is Visual)) ? ((!(dependencyObject is FrameworkContentElement)) ? null : ((FrameworkContentElement)dependencyObject).Parent) : VisualTreeHelper.GetParent(dependencyObject))))
		{
			if (dependencyObject == parent || dependencyObject == renderScope || dependencyObject == caretElement)
			{
				return true;
			}
		}
		return false;
	}
}
