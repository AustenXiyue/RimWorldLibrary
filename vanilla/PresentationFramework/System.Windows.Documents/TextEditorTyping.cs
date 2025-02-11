using System.Collections;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Commands;
using MS.Internal.Documents;
using MS.Internal.Interop;
using MS.Win32;

namespace System.Windows.Documents;

internal static class TextEditorTyping
{
	private abstract class InputItem
	{
		private TextEditor _textEditor;

		protected TextEditor TextEditor => _textEditor;

		internal InputItem(TextEditor textEditor)
		{
			_textEditor = textEditor;
		}

		internal abstract void Do();
	}

	private class TextInputItem : InputItem
	{
		private readonly string _text;

		private readonly bool _isInsertKeyToggled;

		internal TextInputItem(TextEditor textEditor, string text, bool isInsertKeyToggled)
			: base(textEditor)
		{
			_text = text;
			_isInsertKeyToggled = isInsertKeyToggled;
		}

		internal override void Do()
		{
			if (base.TextEditor.UiScope != null)
			{
				DoTextInput(base.TextEditor, _text, _isInsertKeyToggled, acceptControlCharacters: false);
			}
		}
	}

	private class KeyUpInputItem : InputItem
	{
		private readonly Key _key;

		private readonly ModifierKeys _modifiers;

		internal KeyUpInputItem(TextEditor textEditor, Key key, ModifierKeys modifiers)
			: base(textEditor)
		{
			_key = key;
			_modifiers = modifiers;
		}

		internal override void Do()
		{
			if (base.TextEditor.UiScope == null)
			{
				return;
			}
			switch (_key)
			{
			case Key.RightShift:
				if (TextSelection.IsBidiInputLanguageInstalled())
				{
					OnFlowDirectionCommand(base.TextEditor, _key);
				}
				break;
			case Key.LeftShift:
				OnFlowDirectionCommand(base.TextEditor, _key);
				break;
			default:
				Invariant.Assert(condition: false, "Unexpected key value!");
				break;
			}
		}
	}

	private const string KeyBackspace = "Backspace";

	private const string KeyDelete = "Delete";

	private const string KeyDeleteNextWord = "Ctrl+Delete";

	private const string KeyDeletePreviousWord = "Ctrl+Backspace";

	private const string KeyEnterLineBreak = "Shift+Enter";

	private const string KeyEnterParagraphBreak = "Enter";

	private const string KeyShiftBackspace = "Shift+Backspace";

	private const string KeyShiftSpace = "Shift+Space";

	private const string KeySpace = "Space";

	private const string KeyTabBackward = "Shift+Tab";

	private const string KeyTabForward = "Tab";

	private const string KeyToggleInsert = "Insert";

	internal static void _RegisterClassHandlers(Type controlType, bool registerEventListeners)
	{
		if (registerEventListeners)
		{
			EventManager.RegisterClassHandler(controlType, Keyboard.PreviewKeyDownEvent, new KeyEventHandler(OnPreviewKeyDown));
			EventManager.RegisterClassHandler(controlType, Keyboard.KeyDownEvent, new KeyEventHandler(OnKeyDown));
			EventManager.RegisterClassHandler(controlType, Keyboard.KeyUpEvent, new KeyEventHandler(OnKeyUp));
			EventManager.RegisterClassHandler(controlType, TextCompositionManager.TextInputEvent, new TextCompositionEventHandler(OnTextInput));
		}
		ExecutedRoutedEventHandler executedRoutedEventHandler = OnEnterBreak;
		ExecutedRoutedEventHandler executedRoutedEventHandler2 = OnSpace;
		CanExecuteRoutedEventHandler canExecuteRoutedEventHandler = OnQueryStatusNYI;
		CanExecuteRoutedEventHandler canExecuteRoutedEventHandler2 = OnQueryStatusEnterBreak;
		EventManager.RegisterClassHandler(controlType, Mouse.MouseMoveEvent, new MouseEventHandler(OnMouseMove), handledEventsToo: true);
		EventManager.RegisterClassHandler(controlType, Mouse.MouseLeaveEvent, new MouseEventHandler(OnMouseLeave), handledEventsToo: true);
		CommandHelpers.RegisterCommandHandler(controlType, ApplicationCommands.CorrectionList, OnCorrectionList, OnQueryStatusCorrectionList, "KeyCorrectionList", "KeyCorrectionListDisplayString");
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.ToggleInsert, OnToggleInsert, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Insert", "KeyToggleInsertDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.Delete, OnDelete, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Delete", "KeyDeleteDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.DeleteNextWord, OnDeleteNextWord, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Ctrl+Delete", "KeyDeleteNextWordDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.DeletePreviousWord, OnDeletePreviousWord, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Ctrl+Backspace", "KeyDeletePreviousWordDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.EnterParagraphBreak, executedRoutedEventHandler, canExecuteRoutedEventHandler2, KeyGesture.CreateFromResourceStrings("Enter", "KeyEnterParagraphBreakDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.EnterLineBreak, executedRoutedEventHandler, canExecuteRoutedEventHandler2, KeyGesture.CreateFromResourceStrings("Shift+Enter", "KeyEnterLineBreakDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.TabForward, OnTabForward, OnQueryStatusTabForward, KeyGesture.CreateFromResourceStrings("Tab", "KeyTabForwardDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.TabBackward, OnTabBackward, OnQueryStatusTabBackward, KeyGesture.CreateFromResourceStrings("Shift+Tab", "KeyTabBackwardDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.Space, executedRoutedEventHandler2, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Space", "KeySpaceDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.ShiftSpace, executedRoutedEventHandler2, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Shift+Space", "KeyShiftSpaceDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.Backspace, OnBackspace, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Backspace", SR.KeyBackspaceDisplayString), KeyGesture.CreateFromResourceStrings("Shift+Backspace", SR.KeyShiftBackspaceDisplayString));
	}

	internal static void _AddInputLanguageChangedEventHandler(TextEditor This)
	{
		Invariant.Assert(This._dispatcher == null);
		This._dispatcher = Dispatcher.CurrentDispatcher;
		Invariant.Assert(This._dispatcher != null);
		TextEditorThreadLocalStore threadLocalStore = TextEditor._ThreadLocalStore;
		if (threadLocalStore.InputLanguageChangeEventHandlerCount == 0)
		{
			InputLanguageManager.Current.InputLanguageChanged += OnInputLanguageChanged;
			Dispatcher.CurrentDispatcher.ShutdownFinished += OnDispatcherShutdownFinished;
		}
		threadLocalStore.InputLanguageChangeEventHandlerCount++;
	}

	internal static void _RemoveInputLanguageChangedEventHandler(TextEditor This)
	{
		TextEditorThreadLocalStore threadLocalStore = TextEditor._ThreadLocalStore;
		threadLocalStore.InputLanguageChangeEventHandlerCount--;
		if (threadLocalStore.InputLanguageChangeEventHandlerCount == 0)
		{
			InputLanguageManager.Current.InputLanguageChanged -= OnInputLanguageChanged;
			Dispatcher.CurrentDispatcher.ShutdownFinished -= OnDispatcherShutdownFinished;
		}
	}

	internal static void _BreakTypingSequence(TextEditor This)
	{
		This._typingUndoUnit = null;
	}

	internal static void _FlushPendingInputItems(TextEditor This)
	{
		if (This.TextView != null)
		{
			This.TextView.ThrottleBackgroundTasksForUserInput();
		}
		TextEditorThreadLocalStore threadLocalStore = TextEditor._ThreadLocalStore;
		if (threadLocalStore.PendingInputItems != null)
		{
			try
			{
				for (int i = 0; i < threadLocalStore.PendingInputItems.Count; i++)
				{
					((InputItem)threadLocalStore.PendingInputItems[i]).Do();
					threadLocalStore.PureControlShift = false;
				}
			}
			finally
			{
				threadLocalStore.PendingInputItems.Clear();
			}
		}
		threadLocalStore.PureControlShift = false;
	}

	internal static void _ShowCursor()
	{
		if (TextEditor._ThreadLocalStore.HideCursor)
		{
			TextEditor._ThreadLocalStore.HideCursor = false;
			SafeNativeMethods.ShowCursor(show: true);
		}
	}

	internal static void OnPreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.ImeProcessed && sender is RichTextBox { TextEditor: { _IsEnabled: not false, IsReadOnly: false } textEditor } richTextBox && textEditor._IsSourceInScope(e.OriginalSource) && !e.IsRepeat && textEditor.TextStore != null && !textEditor.TextStore.IsComposing && !richTextBox.Selection.IsEmpty)
		{
			textEditor.SetText(textEditor.Selection, string.Empty, InputLanguageManager.Current.CurrentInputLanguage);
		}
	}

	internal static void OnKeyDown(object sender, KeyEventArgs e)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null || !textEditor._IsEnabled || (textEditor.IsReadOnly && !textEditor.IsReadOnlyCaretVisible) || !textEditor._IsSourceInScope(e.OriginalSource) || e.IsRepeat)
		{
			return;
		}
		textEditor.CloseToolTip();
		TextEditorThreadLocalStore threadLocalStore = TextEditor._ThreadLocalStore;
		threadLocalStore.PureControlShift = false;
		if (textEditor.TextView != null && !textEditor.UiScope.IsMouseCaptured)
		{
			if ((e.Key == Key.RightShift || e.Key == Key.LeftShift) && (e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0 && (e.KeyboardDevice.Modifiers & ModifierKeys.Alt) == 0)
			{
				threadLocalStore.PureControlShift = true;
			}
			else if ((e.Key == Key.RightCtrl || e.Key == Key.LeftCtrl) && (e.KeyboardDevice.Modifiers & ModifierKeys.Shift) != 0 && (e.KeyboardDevice.Modifiers & ModifierKeys.Alt) == 0)
			{
				threadLocalStore.PureControlShift = true;
			}
			else if (e.Key == Key.RightCtrl || e.Key == Key.LeftCtrl)
			{
				UpdateHyperlinkCursor(textEditor);
			}
		}
	}

	internal static void OnKeyUp(object sender, KeyEventArgs e)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null || !textEditor._IsEnabled || (textEditor.IsReadOnly && !textEditor.IsReadOnlyCaretVisible) || !textEditor._IsSourceInScope(e.OriginalSource))
		{
			return;
		}
		switch (e.Key)
		{
		case Key.LeftShift:
		case Key.RightShift:
			if (TextEditor._ThreadLocalStore.PureControlShift && (e.KeyboardDevice.Modifiers & ModifierKeys.Alt) == 0)
			{
				ScheduleInput(textEditor, new KeyUpInputItem(textEditor, e.Key, e.KeyboardDevice.Modifiers));
			}
			break;
		case Key.LeftCtrl:
		case Key.RightCtrl:
			UpdateHyperlinkCursor(textEditor);
			break;
		}
	}

	internal static void OnTextInput(object sender, TextCompositionEventArgs e)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null || !textEditor._IsEnabled || textEditor.IsReadOnly || !textEditor._IsSourceInScope(e.OriginalSource))
		{
			return;
		}
		FrameworkTextComposition frameworkTextComposition = e.TextComposition as FrameworkTextComposition;
		if (frameworkTextComposition == null && (e.Text == null || e.Text.Length == 0))
		{
			return;
		}
		e.Handled = true;
		if (textEditor.TextView != null)
		{
			textEditor.TextView.ThrottleBackgroundTasksForUserInput();
		}
		if (frameworkTextComposition != null)
		{
			if (frameworkTextComposition.Owner == textEditor.TextStore)
			{
				textEditor.TextStore.UpdateCompositionText(frameworkTextComposition);
			}
			else if (frameworkTextComposition.Owner == textEditor.ImmComposition)
			{
				textEditor.ImmComposition.UpdateCompositionText(frameworkTextComposition);
			}
		}
		else
		{
			KeyboardDevice keyboardDevice = e.Device as KeyboardDevice;
			ScheduleInput(textEditor, new TextInputItem(textEditor, e.Text, keyboardDevice?.IsKeyToggled(Key.Insert) ?? false));
		}
	}

	private static void OnQueryStatusCorrectionList(object target, CanExecuteRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null)
		{
			if (textEditor.TextStore != null)
			{
				args.CanExecute = textEditor.TextStore.QueryRangeOrReconvertSelection(fDoReconvert: false);
			}
			else
			{
				args.CanExecute = false;
			}
		}
	}

	private static void OnCorrectionList(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null && textEditor.TextStore != null)
		{
			textEditor.TextStore.QueryRangeOrReconvertSelection(fDoReconvert: true);
		}
	}

	private static void OnToggleInsert(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor == null || !textEditor._IsEnabled || textEditor.IsReadOnly)
		{
			return;
		}
		textEditor._OvertypeMode = !textEditor._OvertypeMode;
		if (!TextServicesLoader.ServicesInstalled || textEditor.TextStore == null || TextServicesHost.Current == null)
		{
			return;
		}
		if (textEditor._OvertypeMode)
		{
			if (target is IInputElement element)
			{
				PresentationSource.AddSourceChangedHandler(element, OnSourceChanged);
			}
			TextServicesHost.StartTransitoryExtension(textEditor.TextStore);
		}
		else
		{
			if (target is IInputElement e)
			{
				PresentationSource.RemoveSourceChangedHandler(e, OnSourceChanged);
			}
			TextServicesHost.StopTransitoryExtension(textEditor.TextStore);
		}
	}

	private static void OnSourceChanged(object sender, SourceChangedEventArgs args)
	{
		OnToggleInsert(sender, null);
	}

	private static void OnDelete(object sender, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null || !textEditor._IsEnabled || textEditor.IsReadOnly || !textEditor._IsSourceInScope(args.Source))
		{
			return;
		}
		_FlushPendingInputItems(textEditor);
		((TextSelection)textEditor.Selection).ClearSpringloadFormatting();
		TextEditorSelection._ClearSuggestedX(textEditor);
		using (textEditor.Selection.DeclareChangeBlock())
		{
			ITextPointer end = textEditor.Selection.End;
			if (textEditor.Selection.IsEmpty)
			{
				ITextPointer nextInsertionPosition = end.GetNextInsertionPosition(LogicalDirection.Forward);
				if (nextInsertionPosition == null || TextPointerBase.IsAtRowEnd(nextInsertionPosition) || (end is TextPointer && !IsAtListItemStart(nextInsertionPosition) && HandleDeleteWhenStructuralBoundaryIsCrossed(textEditor, (TextPointer)end, (TextPointer)nextInsertionPosition)))
				{
					return;
				}
				textEditor.Selection.ExtendToNextInsertionPosition(LogicalDirection.Forward);
			}
			textEditor.Selection.Text = string.Empty;
		}
	}

	private static void OnBackspace(object sender, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null || !textEditor._IsEnabled || textEditor.IsReadOnly || !textEditor._IsSourceInScope(args.Source))
		{
			return;
		}
		_FlushPendingInputItems(textEditor);
		TextEditorSelection._ClearSuggestedX(textEditor);
		using (textEditor.Selection.DeclareChangeBlock())
		{
			ITextPointer textPointer = textEditor.Selection.Start;
			ITextPointer textPointer2 = null;
			if (textEditor.Selection.IsEmpty)
			{
				if (textEditor.AcceptsRichContent && IsAtListItemStart(textPointer))
				{
					TextRangeEditLists.ConvertListItemsToParagraphs((TextRange)textEditor.Selection);
				}
				else if (textEditor.AcceptsRichContent && (IsAtListItemChildStart(textPointer, emptyChildOnly: false) || IsAtIndentedParagraphOrBlockUIContainerStart(textEditor.Selection.Start)))
				{
					TextEditorLists.DecreaseIndentation(textEditor);
				}
				else
				{
					ITextPointer nextInsertionPosition = textPointer.GetNextInsertionPosition(LogicalDirection.Backward);
					if (nextInsertionPosition == null)
					{
						((TextSelection)textEditor.Selection).ClearSpringloadFormatting();
						return;
					}
					if (TextPointerBase.IsAtRowEnd(nextInsertionPosition))
					{
						((TextSelection)textEditor.Selection).ClearSpringloadFormatting();
						return;
					}
					if (textPointer is TextPointer && HandleDeleteWhenStructuralBoundaryIsCrossed(textEditor, (TextPointer)textPointer, (TextPointer)nextInsertionPosition))
					{
						return;
					}
					textPointer = textPointer.GetFrozenPointer(LogicalDirection.Backward);
					if (textEditor.TextView != null && textPointer.HasValidLayout && textPointer.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.Text)
					{
						textPointer2 = textEditor.TextView.GetBackspaceCaretUnitPosition(textPointer);
						Invariant.Assert(textPointer2 != null);
						if (textPointer2.CompareTo(textPointer) == 0)
						{
							textEditor.Selection.ExtendToNextInsertionPosition(LogicalDirection.Backward);
							textPointer2 = null;
						}
						else if (textPointer2.GetPointerContext(LogicalDirection.Backward) != TextPointerContext.Text)
						{
							textEditor.Selection.Select(textEditor.Selection.End, textPointer2);
							textPointer2 = null;
						}
					}
					else
					{
						textEditor.Selection.ExtendToNextInsertionPosition(LogicalDirection.Backward);
					}
				}
			}
			if (textEditor.AcceptsRichContent)
			{
				((TextSelection)textEditor.Selection).ClearSpringloadFormatting();
				((TextSelection)textEditor.Selection).SpringloadCurrentFormatting();
			}
			if (textPointer2 != null)
			{
				Invariant.Assert(textPointer2.CompareTo(textPointer) < 0);
				textPointer2.DeleteContentToPosition(textPointer);
			}
			else
			{
				textEditor.Selection.Text = string.Empty;
				textPointer = textEditor.Selection.Start;
			}
			textEditor.Selection.SetCaretToPosition(textPointer, LogicalDirection.Backward, allowStopAtLineEnd: false, allowStopNearSpace: true);
		}
	}

	private static bool HandleDeleteWhenStructuralBoundaryIsCrossed(TextEditor This, TextPointer position, TextPointer deletePosition)
	{
		if (!TextRangeEditTables.IsTableStructureCrossed(position, deletePosition) && !IsBlockUIContainerBoundaryCrossed(position, deletePosition) && !TextPointerBase.IsAtRowEnd(position))
		{
			return false;
		}
		LogicalDirection logicalDirection = ((position.CompareTo(deletePosition) < 0) ? LogicalDirection.Forward : LogicalDirection.Backward);
		Block paragraphOrBlockUIContainer = position.ParagraphOrBlockUIContainer;
		if (paragraphOrBlockUIContainer != null)
		{
			if (logicalDirection == LogicalDirection.Forward)
			{
				if ((paragraphOrBlockUIContainer.NextBlock != null && paragraphOrBlockUIContainer is Paragraph && Paragraph.HasNoTextContent((Paragraph)paragraphOrBlockUIContainer)) || (paragraphOrBlockUIContainer is BlockUIContainer && paragraphOrBlockUIContainer.IsEmpty))
				{
					paragraphOrBlockUIContainer.RepositionWithContent(null);
				}
			}
			else if ((paragraphOrBlockUIContainer.PreviousBlock != null && paragraphOrBlockUIContainer is Paragraph && Paragraph.HasNoTextContent((Paragraph)paragraphOrBlockUIContainer)) || (paragraphOrBlockUIContainer is BlockUIContainer && paragraphOrBlockUIContainer.IsEmpty))
			{
				paragraphOrBlockUIContainer.RepositionWithContent(null);
			}
		}
		This.Selection.SetCaretToPosition(deletePosition, logicalDirection, allowStopAtLineEnd: false, allowStopNearSpace: true);
		if (logicalDirection == LogicalDirection.Backward)
		{
			((TextSelection)This.Selection).ClearSpringloadFormatting();
		}
		return true;
	}

	private static bool IsAtIndentedParagraphOrBlockUIContainerStart(ITextPointer position)
	{
		if (position is TextPointer && TextPointerBase.IsAtParagraphOrBlockUIContainerStart(position))
		{
			Block paragraphOrBlockUIContainer = ((TextPointer)position).ParagraphOrBlockUIContainer;
			if (paragraphOrBlockUIContainer != null)
			{
				FlowDirection flowDirection = paragraphOrBlockUIContainer.FlowDirection;
				Thickness margin = paragraphOrBlockUIContainer.Margin;
				if ((flowDirection != 0 || !(margin.Left > 0.0)) && (flowDirection != FlowDirection.RightToLeft || !(margin.Right > 0.0)))
				{
					if (paragraphOrBlockUIContainer is Paragraph)
					{
						return ((Paragraph)paragraphOrBlockUIContainer).TextIndent > 0.0;
					}
					return false;
				}
				return true;
			}
		}
		return false;
	}

	private static bool IsAtListItemStart(ITextPointer position)
	{
		if (typeof(ListItem).IsAssignableFrom(position.ParentType) && position.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart && position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd)
		{
			return true;
		}
		while (position.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart)
		{
			Type parentType = position.ParentType;
			if (TextSchema.IsBlock(parentType))
			{
				if (TextSchema.IsParagraphOrBlockUIContainer(parentType))
				{
					position = position.GetNextContextPosition(LogicalDirection.Backward);
					if (position.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart && typeof(ListItem).IsAssignableFrom(position.ParentType))
					{
						return true;
					}
				}
				return false;
			}
			position = position.GetNextContextPosition(LogicalDirection.Backward);
		}
		return false;
	}

	private static bool IsAtListItemChildStart(ITextPointer position, bool emptyChildOnly)
	{
		if (position.GetPointerContext(LogicalDirection.Backward) != TextPointerContext.ElementStart)
		{
			return false;
		}
		if (emptyChildOnly && position.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.ElementEnd)
		{
			return false;
		}
		ITextPointer textPointer = position.CreatePointer();
		while (textPointer.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart && typeof(Inline).IsAssignableFrom(textPointer.ParentType))
		{
			textPointer.MoveToElementEdge(ElementEdge.BeforeStart);
		}
		if (textPointer.GetPointerContext(LogicalDirection.Backward) != TextPointerContext.ElementStart || !TextSchema.IsParagraphOrBlockUIContainer(textPointer.ParentType))
		{
			return false;
		}
		textPointer.MoveToElementEdge(ElementEdge.BeforeStart);
		return typeof(ListItem).IsAssignableFrom(textPointer.ParentType);
	}

	private static bool IsBlockUIContainerBoundaryCrossed(TextPointer position1, TextPointer position2)
	{
		if (position1.Parent is BlockUIContainer || position2.Parent is BlockUIContainer)
		{
			return position1.Parent != position2.Parent;
		}
		return false;
	}

	private static void OnDeleteNextWord(object sender, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null || !textEditor._IsEnabled || textEditor.IsReadOnly || textEditor.Selection.IsTableCellRange)
		{
			return;
		}
		_FlushPendingInputItems(textEditor);
		ITextPointer textPointer = textEditor.Selection.End.CreatePointer();
		if (textEditor.Selection.IsEmpty)
		{
			TextPointerBase.MoveToNextWordBoundary(textPointer, LogicalDirection.Forward);
		}
		if (TextRangeEditTables.IsTableStructureCrossed(textEditor.Selection.Start, textPointer))
		{
			return;
		}
		ITextRange textRange = new TextRange(textEditor.Selection.Start, textPointer);
		if (textRange.IsTableCellRange || textRange.IsEmpty)
		{
			return;
		}
		using (textEditor.Selection.DeclareChangeBlock())
		{
			if (textEditor.AcceptsRichContent)
			{
				((TextSelection)textEditor.Selection).ClearSpringloadFormatting();
			}
			textEditor.Selection.Select(textRange.Start, textRange.End);
			textEditor.Selection.Text = string.Empty;
		}
	}

	private static void OnDeletePreviousWord(object sender, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null || !textEditor._IsEnabled || textEditor.IsReadOnly || textEditor.Selection.IsTableCellRange)
		{
			return;
		}
		_FlushPendingInputItems(textEditor);
		ITextPointer textPointer = textEditor.Selection.Start.CreatePointer();
		if (textEditor.Selection.IsEmpty)
		{
			TextPointerBase.MoveToNextWordBoundary(textPointer, LogicalDirection.Backward);
		}
		if (TextRangeEditTables.IsTableStructureCrossed(textPointer, textEditor.Selection.Start))
		{
			return;
		}
		ITextRange textRange = new TextRange(textPointer, textEditor.Selection.End);
		if (textRange.IsTableCellRange || textRange.IsEmpty)
		{
			return;
		}
		using (textEditor.Selection.DeclareChangeBlock())
		{
			if (textEditor.AcceptsRichContent)
			{
				((TextSelection)textEditor.Selection).ClearSpringloadFormatting();
				textEditor.Selection.Select(textRange.Start, textRange.End);
				((TextSelection)textEditor.Selection).SpringloadCurrentFormatting();
			}
			else
			{
				textEditor.Selection.Select(textRange.Start, textRange.End);
			}
			textEditor.Selection.Text = string.Empty;
		}
	}

	private static void OnQueryStatusEnterBreak(object sender, CanExecuteRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null || !textEditor._IsEnabled || textEditor.IsReadOnly)
		{
			args.ContinueRouting = true;
		}
		else if (textEditor.Selection.IsTableCellRange || !textEditor.AcceptsReturn)
		{
			args.ContinueRouting = true;
		}
		else
		{
			args.CanExecute = true;
		}
	}

	private static void OnEnterBreak(object sender, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null || !textEditor._IsEnabled || textEditor.IsReadOnly || textEditor.Selection.IsTableCellRange || !textEditor.AcceptsReturn || !textEditor.UiScope.IsKeyboardFocused)
		{
			return;
		}
		_FlushPendingInputItems(textEditor);
		using (textEditor.Selection.DeclareChangeBlock())
		{
			if ((!textEditor.AcceptsRichContent || !(textEditor.Selection.Start is TextPointer)) ? HandleEnterBreakForPlainText(textEditor) : HandleEnterBreakForRichText(textEditor, args.Command))
			{
				textEditor.Selection.SetCaretToPosition(textEditor.Selection.End, LogicalDirection.Forward, allowStopAtLineEnd: false, allowStopNearSpace: false);
				TextEditorSelection._ClearSuggestedX(textEditor);
			}
		}
	}

	private static bool HandleEnterBreakForRichText(TextEditor This, ICommand command)
	{
		bool flag = true;
		((TextSelection)This.Selection).SpringloadCurrentFormatting();
		if (!This.Selection.IsEmpty)
		{
			This.Selection.Text = string.Empty;
		}
		if (!HandleEnterBreakWhenStructuralBoundaryIsCrossed(This, command))
		{
			TextPointer textPointer = ((TextSelection)This.Selection).End;
			if (command == EditingCommands.EnterParagraphBreak)
			{
				if (textPointer.HasNonMergeableInlineAncestor && !TextPointerBase.IsPositionAtNonMergeableInlineBoundary(textPointer))
				{
					flag = false;
				}
				else
				{
					textPointer = TextRangeEdit.InsertParagraphBreak(textPointer, moveIntoSecondParagraph: true);
				}
			}
			else if (command == EditingCommands.EnterLineBreak)
			{
				textPointer = textPointer.InsertLineBreak();
			}
			if (flag)
			{
				This.Selection.Select(textPointer, textPointer);
			}
		}
		return flag;
	}

	private static bool HandleEnterBreakForPlainText(TextEditor This)
	{
		bool result = true;
		if (This._FilterText(Environment.NewLine, This.Selection) != string.Empty)
		{
			This.Selection.Text = Environment.NewLine;
		}
		else
		{
			result = false;
		}
		return result;
	}

	private static bool HandleEnterBreakWhenStructuralBoundaryIsCrossed(TextEditor This, ICommand command)
	{
		Invariant.Assert(This.Selection.Start is TextPointer);
		TextPointer textPointer = (TextPointer)This.Selection.Start;
		bool result = true;
		if (TextPointerBase.IsAtRowEnd(textPointer))
		{
			TextRange textRange = ((TextSelection)This.Selection).InsertRows(1);
			This.Selection.SetCaretToPosition(textRange.Start, LogicalDirection.Forward, allowStopAtLineEnd: false, allowStopNearSpace: false);
		}
		else if (This.Selection.IsEmpty && (TextPointerBase.IsInEmptyListItem(textPointer) || IsAtListItemChildStart(textPointer, emptyChildOnly: true)) && command == EditingCommands.EnterParagraphBreak)
		{
			TextEditorLists.DecreaseIndentation(This);
		}
		else if (TextPointerBase.IsBeforeFirstTable(textPointer) || TextPointerBase.IsAtBlockUIContainerStart(textPointer))
		{
			TextRangeEditTables.EnsureInsertionPosition(textPointer);
		}
		else if (TextPointerBase.IsAtBlockUIContainerEnd(textPointer))
		{
			TextPointer textPointer2 = TextRangeEditTables.EnsureInsertionPosition(textPointer);
			This.Selection.Select(textPointer2, textPointer2);
		}
		else
		{
			result = false;
		}
		return result;
	}

	private static void OnFlowDirectionCommand(TextEditor This, Key key)
	{
		using (This.Selection.DeclareChangeBlock())
		{
			if (key == Key.LeftShift)
			{
				if (This.AcceptsRichContent && This.Selection is TextSelection)
				{
					((TextSelection)This.Selection).ApplyPropertyValue(FlowDocument.FlowDirectionProperty, FlowDirection.LeftToRight, applyToParagraphs: true);
				}
				else
				{
					Invariant.Assert(This.UiScope != null);
					UIElementPropertyUndoUnit.Add(This.TextContainer, This.UiScope, FrameworkElement.FlowDirectionProperty, FlowDirection.LeftToRight);
					This.UiScope.SetValue(FrameworkElement.FlowDirectionProperty, FlowDirection.LeftToRight);
				}
			}
			else
			{
				Invariant.Assert(key == Key.RightShift);
				if (This.AcceptsRichContent && This.Selection is TextSelection)
				{
					((TextSelection)This.Selection).ApplyPropertyValue(FlowDocument.FlowDirectionProperty, FlowDirection.RightToLeft, applyToParagraphs: true);
				}
				else
				{
					Invariant.Assert(This.UiScope != null);
					UIElementPropertyUndoUnit.Add(This.TextContainer, This.UiScope, FrameworkElement.FlowDirectionProperty, FlowDirection.RightToLeft);
					This.UiScope.SetValue(FrameworkElement.FlowDirectionProperty, FlowDirection.RightToLeft);
				}
			}
			((TextSelection)This.Selection).UpdateCaretState(CaretScrollMethod.Simple);
		}
	}

	private static void OnSpace(object sender, ExecutedRoutedEventArgs e)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor != null && textEditor._IsEnabled && !textEditor.IsReadOnly && textEditor._IsSourceInScope(e.OriginalSource) && (textEditor.TextStore == null || !textEditor.TextStore.IsComposing) && (textEditor.ImmComposition == null || !textEditor.ImmComposition.IsComposition))
		{
			e.Handled = true;
			if (textEditor.TextView != null)
			{
				textEditor.TextView.ThrottleBackgroundTasksForUserInput();
			}
			ScheduleInput(textEditor, new TextInputItem(textEditor, " ", !textEditor._OvertypeMode));
		}
	}

	private static void OnQueryStatusTabForward(object sender, CanExecuteRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor != null && textEditor.AcceptsTab)
		{
			args.CanExecute = true;
		}
		else
		{
			args.ContinueRouting = true;
		}
	}

	private static void OnQueryStatusTabBackward(object sender, CanExecuteRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor != null && textEditor.AcceptsTab)
		{
			args.CanExecute = true;
		}
		else
		{
			args.ContinueRouting = true;
		}
	}

	private static void OnTabForward(object sender, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null || !textEditor._IsEnabled || textEditor.IsReadOnly || !textEditor.UiScope.IsKeyboardFocused)
		{
			return;
		}
		_FlushPendingInputItems(textEditor);
		if (!HandleTabInTables(textEditor, LogicalDirection.Forward))
		{
			if (textEditor.AcceptsRichContent && (!textEditor.Selection.IsEmpty || TextPointerBase.IsAtParagraphOrBlockUIContainerStart(textEditor.Selection.Start)) && EditingCommands.IncreaseIndentation.CanExecute(null, (IInputElement)sender))
			{
				EditingCommands.IncreaseIndentation.Execute(null, (IInputElement)sender);
			}
			else
			{
				DoTextInput(textEditor, "\t", !textEditor._OvertypeMode, acceptControlCharacters: true);
			}
		}
	}

	private static void OnTabBackward(object sender, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null || !textEditor._IsEnabled || textEditor.IsReadOnly || !textEditor.UiScope.IsKeyboardFocused)
		{
			return;
		}
		_FlushPendingInputItems(textEditor);
		if (!HandleTabInTables(textEditor, LogicalDirection.Backward))
		{
			if (textEditor.AcceptsRichContent && (!textEditor.Selection.IsEmpty || TextPointerBase.IsAtParagraphOrBlockUIContainerStart(textEditor.Selection.Start)) && EditingCommands.DecreaseIndentation.CanExecute(null, (IInputElement)sender))
			{
				EditingCommands.DecreaseIndentation.Execute(null, (IInputElement)sender);
			}
			else
			{
				DoTextInput(textEditor, "\t", !textEditor._OvertypeMode, acceptControlCharacters: true);
			}
		}
	}

	private static bool HandleTabInTables(TextEditor This, LogicalDirection direction)
	{
		if (!This.AcceptsRichContent)
		{
			return false;
		}
		if (This.Selection.IsTableCellRange)
		{
			This.Selection.SetCaretToPosition(This.Selection.Start, LogicalDirection.Backward, allowStopAtLineEnd: false, allowStopNearSpace: false);
			return true;
		}
		if (This.Selection.IsEmpty && TextPointerBase.IsAtRowEnd(This.Selection.End))
		{
			TableCell tableCell = null;
			TableRow tableRow = ((TextPointer)This.Selection.End).Parent as TableRow;
			Invariant.Assert(tableRow != null);
			TableRowGroup rowGroup = tableRow.RowGroup;
			int num = rowGroup.Rows.IndexOf(tableRow);
			if (direction == LogicalDirection.Forward)
			{
				if (num + 1 < rowGroup.Rows.Count)
				{
					tableCell = rowGroup.Rows[num + 1].Cells[0];
				}
			}
			else if (num > 0)
			{
				tableCell = rowGroup.Rows[num - 1].Cells[rowGroup.Rows[num - 1].Cells.Count - 1];
			}
			if (tableCell != null)
			{
				This.Selection.Select(tableCell.ContentStart, tableCell.ContentEnd);
			}
			return true;
		}
		TextElement textElement = ((TextPointer)This.Selection.Start).Parent as TextElement;
		while (textElement != null && !(textElement is TableCell))
		{
			textElement = textElement.Parent as TextElement;
		}
		if (textElement is TableCell)
		{
			TableCell tableCell2 = (TableCell)textElement;
			TableRow row = tableCell2.Row;
			TableRowGroup rowGroup2 = row.RowGroup;
			int num2 = row.Cells.IndexOf(tableCell2);
			int num3 = rowGroup2.Rows.IndexOf(row);
			if (direction == LogicalDirection.Forward)
			{
				if (num2 + 1 < row.Cells.Count)
				{
					tableCell2 = row.Cells[num2 + 1];
				}
				else if (num3 + 1 < rowGroup2.Rows.Count)
				{
					tableCell2 = rowGroup2.Rows[num3 + 1].Cells[0];
				}
			}
			else if (num2 > 0)
			{
				tableCell2 = row.Cells[num2 - 1];
			}
			else if (num3 > 0)
			{
				tableCell2 = rowGroup2.Rows[num3 - 1].Cells[rowGroup2.Rows[num3 - 1].Cells.Count - 1];
			}
			Invariant.Assert(tableCell2 != null);
			This.Selection.Select(tableCell2.ContentStart, tableCell2.ContentEnd);
			return true;
		}
		return false;
	}

	private static void DoTextInput(TextEditor This, string textData, bool isInsertKeyToggled, bool acceptControlCharacters)
	{
		HideCursor(This);
		if (!acceptControlCharacters)
		{
			for (int i = 0; i < textData.Length; i++)
			{
				if (char.IsControl(textData[i]))
				{
					textData = textData.Remove(i--, 1);
				}
			}
		}
		string text = This._FilterText(textData, This.Selection);
		if (text.Length == 0)
		{
			return;
		}
		OpenTypingUndoUnit(This);
		UndoCloseAction closeAction = UndoCloseAction.Rollback;
		try
		{
			using (This.Selection.DeclareChangeBlock())
			{
				This.Selection.ApplyTypingHeuristics(This.AllowOvertype && This._OvertypeMode && text != "\t");
				This.SetSelectedText(text, InputLanguageManager.Current.CurrentInputLanguage);
				ITextPointer caretPosition = This.Selection.End.CreatePointer(LogicalDirection.Backward);
				This.Selection.SetCaretToPosition(caretPosition, LogicalDirection.Backward, allowStopAtLineEnd: true, allowStopNearSpace: true);
				closeAction = UndoCloseAction.Commit;
			}
		}
		finally
		{
			CloseTypingUndoUnit(This, closeAction);
		}
	}

	private static void ScheduleInput(TextEditor This, InputItem item)
	{
		if (!This.AcceptsRichContent || IsMouseInputPending(This))
		{
			_FlushPendingInputItems(This);
			item.Do();
			return;
		}
		TextEditorThreadLocalStore threadLocalStore = TextEditor._ThreadLocalStore;
		if (threadLocalStore.PendingInputItems == null)
		{
			threadLocalStore.PendingInputItems = new ArrayList(1);
			Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(BackgroundInputCallback), This);
		}
		threadLocalStore.PendingInputItems.Add(item);
	}

	private static bool IsMouseInputPending(TextEditor This)
	{
		bool result = false;
		if (PresentationSource.CriticalFromVisual(This.UiScope) is IWin32Window win32Window)
		{
			nint zero = IntPtr.Zero;
			zero = win32Window.Handle;
			if (zero != 0)
			{
				MSG msg = default(MSG);
				result = MS.Win32.UnsafeNativeMethods.PeekMessage(ref msg, new HandleRef(null, zero), WindowMessage.WM_MOUSEMOVE, WindowMessage.WM_MOUSEHWHEEL, 0);
			}
		}
		return result;
	}

	private static object BackgroundInputCallback(object This)
	{
		TextEditorThreadLocalStore threadLocalStore = TextEditor._ThreadLocalStore;
		Invariant.Assert(This is TextEditor);
		Invariant.Assert(threadLocalStore.PendingInputItems != null);
		try
		{
			_FlushPendingInputItems((TextEditor)This);
		}
		finally
		{
			threadLocalStore.PendingInputItems = null;
		}
		return null;
	}

	private static void OnDispatcherShutdownFinished(object sender, EventArgs args)
	{
		Dispatcher.CurrentDispatcher.ShutdownFinished -= OnDispatcherShutdownFinished;
		InputLanguageManager.Current.InputLanguageChanged -= OnInputLanguageChanged;
		TextEditor._ThreadLocalStore.InputLanguageChangeEventHandlerCount = 0;
	}

	private static void OnInputLanguageChanged(object sender, InputLanguageEventArgs e)
	{
		TextSelection.OnInputLanguageChanged(e.NewLanguage);
	}

	private static void OpenTypingUndoUnit(TextEditor This)
	{
		UndoManager undoManager = This._GetUndoManager();
		if (undoManager != null && undoManager.IsEnabled)
		{
			if (This._typingUndoUnit != null && undoManager.LastUnit == This._typingUndoUnit && !This._typingUndoUnit.Locked)
			{
				undoManager.Reopen(This._typingUndoUnit);
				return;
			}
			This._typingUndoUnit = new TextParentUndoUnit(This.Selection);
			undoManager.Open(This._typingUndoUnit);
		}
	}

	private static void CloseTypingUndoUnit(TextEditor This, UndoCloseAction closeAction)
	{
		UndoManager undoManager = This._GetUndoManager();
		if (undoManager != null && undoManager.IsEnabled)
		{
			if (This._typingUndoUnit != null && undoManager.LastUnit == This._typingUndoUnit && !This._typingUndoUnit.Locked)
			{
				if (This._typingUndoUnit is TextParentUndoUnit)
				{
					((TextParentUndoUnit)This._typingUndoUnit).RecordRedoSelectionState();
				}
				undoManager.Close(This._typingUndoUnit, closeAction);
			}
		}
		else
		{
			This._typingUndoUnit = null;
		}
	}

	private static void OnQueryStatusNYI(object target, CanExecuteRoutedEventArgs args)
	{
		if (TextEditor._GetTextEditor(target) != null)
		{
			args.CanExecute = true;
		}
	}

	private static void OnMouseMove(object sender, MouseEventArgs e)
	{
		_ShowCursor();
	}

	private static void OnMouseLeave(object sender, MouseEventArgs e)
	{
		_ShowCursor();
	}

	private static void HideCursor(TextEditor This)
	{
		if (!TextEditor._ThreadLocalStore.HideCursor && SystemParameters.MouseVanish && This.UiScope.IsMouseOver)
		{
			TextEditor._ThreadLocalStore.HideCursor = true;
			SafeNativeMethods.ShowCursor(show: false);
		}
	}

	private static void UpdateHyperlinkCursor(TextEditor This)
	{
		if (This.UiScope is RichTextBox && This.TextView != null && This.TextView.IsValid)
		{
			TextPointer textPointer = (TextPointer)This.TextView.GetTextPositionFromPoint(Mouse.GetPosition(This.TextView.RenderScope), snapToText: false);
			if (textPointer != null && textPointer.Parent is TextElement && TextSchema.HasHyperlinkAncestor((TextElement)textPointer.Parent))
			{
				Mouse.UpdateCursor();
			}
		}
	}
}
