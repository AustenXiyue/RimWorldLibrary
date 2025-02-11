using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using MS.Internal;
using MS.Win32;

namespace System.Windows.Documents;

internal static class TextEditorDragDrop
{
	internal class _DragDropProcess
	{
		private TextEditor _textEditor;

		private ITextRange _dragSourceTextRange;

		private bool _dragStarted;

		private CaretElement _caretDragDrop;

		private Rect _dragRect;

		private ITextView TextView => _textEditor.TextView;

		internal _DragDropProcess(TextEditor textEditor)
		{
			Invariant.Assert(textEditor != null);
			_textEditor = textEditor;
		}

		internal bool SourceOnMouseLeftButtonDown(Point mouseDownPoint)
		{
			ITextSelection selection = _textEditor.Selection;
			if (_textEditor.UiScope is PasswordBox)
			{
				_dragStarted = false;
			}
			else
			{
				int num = (int)SystemParameters.MinimumHorizontalDragDistance;
				int num2 = (int)SystemParameters.MinimumVerticalDragDistance;
				_dragRect = new Rect(mouseDownPoint.X - (double)num, mouseDownPoint.Y - (double)num2, num * 2, num2 * 2);
				_dragStarted = selection.Contains(mouseDownPoint);
			}
			return _dragStarted;
		}

		internal void DoMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			if (!_dragStarted)
			{
				return;
			}
			if (TextView.IsValid)
			{
				Point position = e.GetPosition(_textEditor.TextView.RenderScope);
				ITextPointer textPositionFromPoint = TextView.GetTextPositionFromPoint(position, snapToText: true);
				if (textPositionFromPoint != null)
				{
					_textEditor.Selection.SetSelectionByMouse(textPositionFromPoint, position);
				}
			}
			_dragStarted = false;
		}

		internal bool SourceOnMouseMove(Point mouseMovePoint)
		{
			if (!_dragStarted)
			{
				return false;
			}
			if (!InitialThresholdCrossed(mouseMovePoint))
			{
				return true;
			}
			ITextSelection selection = _textEditor.Selection;
			_dragStarted = false;
			_dragSourceTextRange = new TextRange(selection.Start, selection.End);
			IDataObject dataObject = TextEditorCopyPaste._CreateDataObject(_textEditor, isDragDrop: true);
			if (dataObject != null)
			{
				SourceDoDragDrop(selection, dataObject);
				_textEditor.UiScope.ReleaseMouseCapture();
				return true;
			}
			return false;
		}

		private bool InitialThresholdCrossed(Point dragPoint)
		{
			return !_dragRect.Contains(dragPoint.X, dragPoint.Y);
		}

		private void SourceDoDragDrop(ITextSelection selection, IDataObject dataObject)
		{
			DragDropEffects dragDropEffects = DragDropEffects.Copy;
			if (!_textEditor.IsReadOnly)
			{
				dragDropEffects |= DragDropEffects.Move;
			}
			DragDropEffects dragDropEffects2 = DragDropEffects.None;
			try
			{
				dragDropEffects2 = DragDrop.DoDragDrop(_textEditor.UiScope, dataObject, dragDropEffects);
			}
			catch (COMException ex) when (ex.HResult == -2147418113)
			{
			}
			if (!_textEditor.IsReadOnly && dragDropEffects2 == DragDropEffects.Move && _dragSourceTextRange != null && !_dragSourceTextRange.IsEmpty)
			{
				using (selection.DeclareChangeBlock())
				{
					_dragSourceTextRange.Text = string.Empty;
				}
			}
			_dragSourceTextRange = null;
			if (!_textEditor.IsReadOnly)
			{
				BindingExpressionBase bindingExpressionBase = BindingOperations.GetBindingExpressionBase(_textEditor.UiScope, TextBox.TextProperty);
				if (bindingExpressionBase != null)
				{
					bindingExpressionBase.UpdateSource();
					bindingExpressionBase.UpdateTarget();
				}
			}
		}

		internal void TargetEnsureDropCaret()
		{
			if (_caretDragDrop == null)
			{
				_caretDragDrop = new CaretElement(_textEditor, isBlinkEnabled: false);
				_caretDragDrop.Hide();
			}
		}

		internal void TargetOnDragEnter(DragEventArgs e)
		{
			if (AllowDragDrop(e))
			{
				if ((e.AllowedEffects & DragDropEffects.Move) != 0)
				{
					e.Effects = DragDropEffects.Move;
				}
				if ((e.KeyStates & DragDropKeyStates.ControlKey) != 0)
				{
					e.Effects |= DragDropEffects.Copy;
				}
				TargetEnsureDropCaret();
			}
		}

		internal void TargetOnDragOver(DragEventArgs e)
		{
			if (!AllowDragDrop(e))
			{
				return;
			}
			if ((e.AllowedEffects & DragDropEffects.Move) != 0)
			{
				e.Effects = DragDropEffects.Move;
			}
			if ((e.KeyStates & DragDropKeyStates.ControlKey) != 0)
			{
				e.Effects |= DragDropEffects.Copy;
			}
			if (_caretDragDrop == null || !_textEditor.TextView.Validate(e.GetPosition(_textEditor.TextView.RenderScope)))
			{
				return;
			}
			FrameworkElement scroller = _textEditor._Scroller;
			if (scroller != null)
			{
				IScrollInfo scrollInfo = scroller as IScrollInfo;
				if (scrollInfo == null && scroller is ScrollViewer)
				{
					scrollInfo = ((ScrollViewer)scroller).ScrollInfo;
				}
				Invariant.Assert(scrollInfo != null);
				Point position = e.GetPosition(scroller);
				double num = (double)_textEditor.UiScope.GetValue(TextEditor.PageHeightProperty);
				double num2 = 16.0;
				if (position.Y < num2)
				{
					if (position.Y > num2 / 2.0)
					{
						scrollInfo.LineUp();
					}
					else
					{
						scrollInfo.PageUp();
					}
				}
				else if (position.Y > num - num2)
				{
					if (position.Y < num - num2 / 2.0)
					{
						scrollInfo.LineDown();
					}
					else
					{
						scrollInfo.PageDown();
					}
				}
			}
			_textEditor.TextView.RenderScope.UpdateLayout();
			if (_textEditor.TextView.IsValid)
			{
				ITextPointer dropPosition = GetDropPosition(_textEditor.TextView.RenderScope, e.GetPosition(_textEditor.TextView.RenderScope));
				if (dropPosition != null)
				{
					Rect rectangleFromTextPosition = TextView.GetRectangleFromTextPosition(dropPosition);
					object value = dropPosition.GetValue(TextElement.FontStyleProperty);
					bool italic = _textEditor.AcceptsRichContent && value != DependencyProperty.UnsetValue && (FontStyle)value == FontStyles.Italic;
					Brush caretBrush = TextSelection.GetCaretBrush(_textEditor);
					_caretDragDrop.Update(visible: true, rectangleFromTextPosition, caretBrush, 0.5, italic, CaretScrollMethod.None, double.NaN);
				}
			}
		}

		private ITextPointer GetDropPosition(Visual target, Point point)
		{
			Invariant.Assert(target != null);
			Invariant.Assert(_textEditor.TextView.IsValid);
			if (target != _textEditor.TextView.RenderScope && target != null && _textEditor.TextView.RenderScope.IsAncestorOf(target))
			{
				target.TransformToAncestor(_textEditor.TextView.RenderScope).TryTransform(point, out point);
			}
			ITextPointer textPointer = TextView.GetTextPositionFromPoint(point, snapToText: true);
			if (textPointer != null)
			{
				textPointer = textPointer.GetInsertionPosition(textPointer.LogicalDirection);
				if (_textEditor.AcceptsRichContent)
				{
					TextSegment normalizedLineRange = TextEditorSelection.GetNormalizedLineRange(TextView, textPointer);
					if (!normalizedLineRange.IsNull && textPointer.CompareTo(normalizedLineRange.End) < 0 && !TextPointerBase.IsAtWordBoundary(textPointer, LogicalDirection.Forward) && _dragSourceTextRange != null && TextPointerBase.IsAtWordBoundary(_dragSourceTextRange.Start, LogicalDirection.Forward) && TextPointerBase.IsAtWordBoundary(_dragSourceTextRange.End, LogicalDirection.Forward))
					{
						TextSegment wordRange = TextPointerBase.GetWordRange(textPointer);
						string textInternal = TextRangeBase.GetTextInternal(wordRange.Start, wordRange.End);
						textPointer = ((wordRange.Start.GetOffsetToPosition(textPointer) < textInternal.Length / 2) ? wordRange.Start : wordRange.End);
					}
				}
			}
			return textPointer;
		}

		internal void DeleteCaret()
		{
			if (_caretDragDrop != null)
			{
				AdornerLayer.GetAdornerLayer(TextView.RenderScope).Remove(_caretDragDrop);
				_caretDragDrop = null;
			}
		}

		internal void TargetOnDrop(DragEventArgs e)
		{
			if (!AllowDragDrop(e))
			{
				return;
			}
			ITextSelection selection = _textEditor.Selection;
			Invariant.Assert(selection != null);
			if (e.Data == null || e.AllowedEffects == DragDropEffects.None)
			{
				e.Effects = DragDropEffects.None;
				return;
			}
			if ((e.KeyStates & DragDropKeyStates.ControlKey) != 0)
			{
				e.Effects = DragDropEffects.Copy;
			}
			else if (e.Effects != DragDropEffects.Copy)
			{
				e.Effects = DragDropEffects.Move;
			}
			if (!_textEditor.TextView.Validate(e.GetPosition(_textEditor.TextView.RenderScope)))
			{
				e.Effects = DragDropEffects.None;
				return;
			}
			ITextPointer dropPosition = GetDropPosition(_textEditor.TextView.RenderScope, e.GetPosition(_textEditor.TextView.RenderScope));
			if (dropPosition == null)
			{
				return;
			}
			if (_dragSourceTextRange != null && _dragSourceTextRange.Start.TextContainer == selection.Start.TextContainer && !selection.IsEmpty && IsSelectionContainsDropPosition(selection, dropPosition))
			{
				selection.SetCaretToPosition(dropPosition, LogicalDirection.Backward, allowStopAtLineEnd: false, allowStopNearSpace: true);
				e.Effects = DragDropEffects.None;
				e.Handled = true;
			}
			else
			{
				using (selection.DeclareChangeBlock())
				{
					if ((e.Effects & DragDropEffects.Move) != 0 && _dragSourceTextRange != null && _dragSourceTextRange.Start.TextContainer == selection.Start.TextContainer)
					{
						_dragSourceTextRange.Text = string.Empty;
					}
					selection.SetCaretToPosition(dropPosition, LogicalDirection.Backward, allowStopAtLineEnd: false, allowStopNearSpace: true);
					e.Handled = TextEditorCopyPaste._DoPaste(_textEditor, e.Data, isDragDrop: true);
				}
			}
			if (e.Handled)
			{
				Win32SetForegroundWindow();
				_textEditor.UiScope.Focus();
			}
			else
			{
				e.Effects = DragDropEffects.None;
			}
		}

		private bool IsSelectionContainsDropPosition(ITextSelection selection, ITextPointer dropPosition)
		{
			bool flag = selection.Contains(dropPosition);
			if (flag && selection.IsTableCellRange)
			{
				for (int i = 0; i < selection.TextSegments.Count; i++)
				{
					if (dropPosition.CompareTo(selection._TextSegments[i].End) == 0)
					{
						flag = false;
						break;
					}
				}
			}
			return flag;
		}

		private bool AllowDragDrop(DragEventArgs e)
		{
			if (!_textEditor.IsReadOnly && _textEditor.TextView != null && _textEditor.TextView.RenderScope != null)
			{
				Window window = Window.GetWindow(_textEditor.TextView.RenderScope);
				if (window == null)
				{
					return true;
				}
				WindowInteropHelper windowInteropHelper = new WindowInteropHelper(window);
				if (SafeNativeMethods.IsWindowEnabled(new HandleRef(null, windowInteropHelper.Handle)))
				{
					return true;
				}
			}
			e.Effects = DragDropEffects.None;
			return false;
		}

		private void Win32SetForegroundWindow()
		{
			PresentationSource presentationSource = null;
			nint num = IntPtr.Zero;
			presentationSource = PresentationSource.CriticalFromVisual(_textEditor.UiScope);
			if (presentationSource != null)
			{
				num = (presentationSource as IWin32Window).Handle;
			}
			if (num != IntPtr.Zero)
			{
				MS.Win32.UnsafeNativeMethods.SetForegroundWindow(new HandleRef(null, num));
			}
		}
	}

	internal static void _RegisterClassHandlers(Type controlType, bool readOnly, bool registerEventListeners)
	{
		if (!readOnly)
		{
			EventManager.RegisterClassHandler(controlType, DragDrop.DropEvent, new DragEventHandler(OnClearState), handledEventsToo: true);
			EventManager.RegisterClassHandler(controlType, DragDrop.DragLeaveEvent, new DragEventHandler(OnClearState), handledEventsToo: true);
		}
		if (registerEventListeners)
		{
			EventManager.RegisterClassHandler(controlType, DragDrop.QueryContinueDragEvent, new QueryContinueDragEventHandler(OnQueryContinueDrag));
			EventManager.RegisterClassHandler(controlType, DragDrop.GiveFeedbackEvent, new GiveFeedbackEventHandler(OnGiveFeedback));
			EventManager.RegisterClassHandler(controlType, DragDrop.DragEnterEvent, new DragEventHandler(OnDragEnter));
			EventManager.RegisterClassHandler(controlType, DragDrop.DragOverEvent, new DragEventHandler(OnDragOver));
			EventManager.RegisterClassHandler(controlType, DragDrop.DragLeaveEvent, new DragEventHandler(OnDragLeave));
			if (!readOnly)
			{
				EventManager.RegisterClassHandler(controlType, DragDrop.DropEvent, new DragEventHandler(OnDrop));
			}
		}
	}

	internal static void OnQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor != null && textEditor._IsEnabled)
		{
			e.Handled = true;
			e.Action = DragAction.Continue;
			bool flag = (e.KeyStates & DragDropKeyStates.LeftMouseButton) == 0;
			if (e.EscapePressed)
			{
				e.Action = DragAction.Cancel;
			}
			else if (flag)
			{
				e.Action = DragAction.Drop;
			}
		}
	}

	internal static void OnGiveFeedback(object sender, GiveFeedbackEventArgs e)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor != null && textEditor._IsEnabled)
		{
			e.UseDefaultCursors = true;
			e.Handled = true;
		}
	}

	internal static void OnDragEnter(object sender, DragEventArgs e)
	{
		e.Handled = true;
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null)
		{
			e.Effects = DragDropEffects.None;
			return;
		}
		if (!textEditor._IsEnabled || textEditor.TextView == null || textEditor.TextView.RenderScope == null)
		{
			e.Effects = DragDropEffects.None;
			return;
		}
		if (e.Data == null)
		{
			e.Effects = DragDropEffects.None;
			return;
		}
		if (TextEditorCopyPaste.GetPasteApplyFormat(textEditor, e.Data) == string.Empty)
		{
			e.Effects = DragDropEffects.None;
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		if (!textEditor.TextView.Validate(e.GetPosition(textEditor.TextView.RenderScope)))
		{
			e.Effects = DragDropEffects.None;
		}
		else
		{
			textEditor._dragDropProcess.TargetOnDragEnter(e);
		}
	}

	internal static void OnDragOver(object sender, DragEventArgs e)
	{
		e.Handled = true;
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null)
		{
			e.Effects = DragDropEffects.None;
			return;
		}
		if (!textEditor._IsEnabled || textEditor.TextView == null || textEditor.TextView.RenderScope == null)
		{
			e.Effects = DragDropEffects.None;
			return;
		}
		if (e.Data == null)
		{
			e.Effects = DragDropEffects.None;
			return;
		}
		if (TextEditorCopyPaste.GetPasteApplyFormat(textEditor, e.Data) == string.Empty)
		{
			e.Effects = DragDropEffects.None;
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		if (!textEditor.TextView.Validate(e.GetPosition(textEditor.TextView.RenderScope)))
		{
			e.Effects = DragDropEffects.None;
		}
		else
		{
			textEditor._dragDropProcess.TargetOnDragOver(e);
		}
	}

	internal static void OnDragLeave(object sender, DragEventArgs e)
	{
		e.Handled = true;
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor != null)
		{
			if (!textEditor._IsEnabled)
			{
				e.Effects = DragDropEffects.None;
				return;
			}
			TextEditorTyping._FlushPendingInputItems(textEditor);
			textEditor.TextView.Validate(e.GetPosition(textEditor.TextView.RenderScope));
		}
	}

	internal static void OnDrop(object sender, DragEventArgs e)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor != null && textEditor._IsEnabled)
		{
			TextEditorTyping._FlushPendingInputItems(textEditor);
			if (textEditor.TextView.Validate(e.GetPosition(textEditor.TextView.RenderScope)))
			{
				textEditor._dragDropProcess.TargetOnDrop(e);
			}
		}
	}

	internal static void OnClearState(object sender, DragEventArgs e)
	{
		TextEditor._GetTextEditor(sender)?._dragDropProcess.DeleteCaret();
	}
}
