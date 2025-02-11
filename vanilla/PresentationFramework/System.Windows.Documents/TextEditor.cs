using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Commands;
using MS.Internal.Documents;
using MS.Win32;

namespace System.Windows.Documents;

internal class TextEditor
{
	private sealed class TextEditorShutDownListener : ShutDownListener
	{
		public TextEditorShutDownListener(TextEditor target)
			: base(target, ShutDownEvents.DomainUnload | ShutDownEvents.DispatcherShutdown)
		{
		}

		internal override void OnShutDown(object target, object sender, EventArgs e)
		{
			((TextEditor)target).DetachTextStore(finalizer: false);
		}
	}

	private class MouseSelectionState
	{
		internal DispatcherTimer Timer;

		internal Point Point;

		internal bool BringIntoViewInProgress;
	}

	internal static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.RegisterAttached("IsReadOnly", typeof(bool), typeof(TextEditor), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits, OnIsReadOnlyChanged));

	internal static readonly DependencyProperty AllowOvertypeProperty = DependencyProperty.RegisterAttached("AllowOvertype", typeof(bool), typeof(TextEditor), new FrameworkPropertyMetadata(true));

	internal static readonly DependencyProperty PageHeightProperty = DependencyProperty.RegisterAttached("PageHeight", typeof(double), typeof(TextEditor), new FrameworkPropertyMetadata(0.0));

	private static readonly DependencyProperty InstanceProperty = DependencyProperty.RegisterAttached("Instance", typeof(TextEditor), typeof(TextEditor), new FrameworkPropertyMetadata((object)null));

	internal Dispatcher _dispatcher;

	private bool _isReadOnly;

	private static ArrayList _registeredEditingTypes = new ArrayList(4);

	private ITextContainer _textContainer;

	private long _contentChangeCounter;

	private FrameworkElement _uiScope;

	private ITextView _textView;

	private ITextSelection _selection;

	private bool _overtypeMode;

	internal double _suggestedX;

	private TextStore _textstore;

	private TextEditorShutDownListener _weakThis;

	private Speller _speller;

	private bool _textStoreInitStarted;

	private bool _pendingTextStoreInit;

	internal Cursor _cursor;

	internal IParentUndoUnit _typingUndoUnit;

	internal TextEditorDragDrop._DragDropProcess _dragDropProcess;

	internal bool _forceWordSelection;

	internal bool _forceParagraphSelection;

	internal TextRangeEditTables.TableColumnResizeInfo _tableColResizeInfo;

	private UndoState _undoState;

	private bool _acceptsRichContent;

	private static bool _immEnabled = SafeSystemMetrics.IsImmEnabled;

	private ImmComposition _immComposition;

	private WeakReference<ImmComposition> _immCompositionForDetach;

	private static LocalDataStoreSlot _threadLocalStoreSlot = Thread.AllocateDataSlot();

	internal bool _mouseCapturingInProgress;

	private MouseSelectionState _mouseSelectionState;

	private bool _isContextMenuOpen;

	private static bool _isTableEditingEnabled;

	private ITextPointer _nextLineAdvanceMovingPosition;

	internal bool _isNextLineAdvanceMovingPositionAtDocumentHead;

	private const string KeyAltUndo = "Alt+Backspace";

	private const string KeyRedo = "Ctrl+Y";

	private const string KeyUndo = "Ctrl+Z";

	internal ITextContainer TextContainer => _textContainer;

	internal FrameworkElement UiScope => _uiScope;

	internal ITextView TextView
	{
		get
		{
			return _textView;
		}
		set
		{
			if (value != _textView)
			{
				if (_textView != null)
				{
					_textView.Updated -= OnTextViewUpdated;
					_textView = null;
					_selection.UpdateCaretAndHighlight();
				}
				if (value != null)
				{
					_textView = value;
					_textView.Updated += OnTextViewUpdated;
					_selection.UpdateCaretAndHighlight();
				}
			}
		}
	}

	internal ITextSelection Selection => _selection;

	internal TextStore TextStore => _textstore;

	internal ImmComposition ImmComposition
	{
		get
		{
			if (!_immEnabled)
			{
				return null;
			}
			return _immComposition;
		}
	}

	internal bool AcceptsReturn
	{
		get
		{
			if (_uiScope != null)
			{
				return (bool)_uiScope.GetValue(KeyboardNavigation.AcceptsReturnProperty);
			}
			return true;
		}
	}

	internal bool AcceptsTab
	{
		get
		{
			if (_uiScope != null)
			{
				return (bool)_uiScope.GetValue(TextBoxBase.AcceptsTabProperty);
			}
			return true;
		}
		set
		{
			Invariant.Assert(_uiScope != null);
			if (AcceptsTab != value)
			{
				_uiScope.SetValue(TextBoxBase.AcceptsTabProperty, value);
			}
		}
	}

	internal bool IsReadOnly
	{
		get
		{
			if (_isReadOnly)
			{
				return true;
			}
			if (_uiScope != null)
			{
				return (bool)_uiScope.GetValue(IsReadOnlyProperty);
			}
			return false;
		}
		set
		{
			_isReadOnly = value;
		}
	}

	internal bool IsSpellCheckEnabled
	{
		get
		{
			if (_uiScope != null)
			{
				return (bool)_uiScope.GetValue(SpellCheck.IsEnabledProperty);
			}
			return false;
		}
		set
		{
			Invariant.Assert(_uiScope != null);
			_uiScope.SetValue(SpellCheck.IsEnabledProperty, value);
		}
	}

	internal bool AcceptsRichContent
	{
		get
		{
			return _acceptsRichContent;
		}
		set
		{
			_acceptsRichContent = value;
		}
	}

	internal bool AllowOvertype
	{
		get
		{
			if (_uiScope != null)
			{
				return (bool)_uiScope.GetValue(AllowOvertypeProperty);
			}
			return true;
		}
	}

	internal int MaxLength
	{
		get
		{
			if (_uiScope != null)
			{
				return (int)_uiScope.GetValue(TextBox.MaxLengthProperty);
			}
			return 0;
		}
	}

	internal CharacterCasing CharacterCasing
	{
		get
		{
			if (_uiScope != null)
			{
				return (CharacterCasing)_uiScope.GetValue(TextBox.CharacterCasingProperty);
			}
			return CharacterCasing.Normal;
		}
	}

	internal bool AutoWordSelection
	{
		get
		{
			if (_uiScope != null)
			{
				return (bool)_uiScope.GetValue(TextBoxBase.AutoWordSelectionProperty);
			}
			return false;
		}
	}

	internal bool IsReadOnlyCaretVisible
	{
		get
		{
			if (_uiScope != null)
			{
				return (bool)_uiScope.GetValue(TextBoxBase.IsReadOnlyCaretVisibleProperty);
			}
			return false;
		}
	}

	internal UndoState UndoState => _undoState;

	internal bool IsContextMenuOpen
	{
		get
		{
			return _isContextMenuOpen;
		}
		set
		{
			_isContextMenuOpen = value;
		}
	}

	internal Speller Speller => _speller;

	internal bool _IsEnabled
	{
		get
		{
			if (_uiScope != null)
			{
				return _uiScope.IsEnabled;
			}
			return false;
		}
	}

	internal bool _OvertypeMode
	{
		get
		{
			return _overtypeMode;
		}
		set
		{
			_overtypeMode = value;
		}
	}

	internal FrameworkElement _Scroller
	{
		get
		{
			FrameworkElement frameworkElement = ((TextView == null) ? null : (TextView.RenderScope as FrameworkElement));
			while (frameworkElement != null && frameworkElement != UiScope)
			{
				frameworkElement = FrameworkElement.GetFrameworkParent(frameworkElement) as FrameworkElement;
				if (frameworkElement is ScrollViewer || frameworkElement is ScrollContentPresenter)
				{
					return frameworkElement;
				}
			}
			return null;
		}
	}

	internal static TextEditorThreadLocalStore _ThreadLocalStore
	{
		get
		{
			TextEditorThreadLocalStore textEditorThreadLocalStore = (TextEditorThreadLocalStore)Thread.GetData(_threadLocalStoreSlot);
			if (textEditorThreadLocalStore == null)
			{
				textEditorThreadLocalStore = new TextEditorThreadLocalStore();
				Thread.SetData(_threadLocalStoreSlot, textEditorThreadLocalStore);
			}
			return textEditorThreadLocalStore;
		}
	}

	internal long _ContentChangeCounter => _contentChangeCounter;

	internal static bool IsTableEditingEnabled
	{
		get
		{
			return _isTableEditingEnabled;
		}
		set
		{
			_isTableEditingEnabled = value;
		}
	}

	internal ITextPointer _NextLineAdvanceMovingPosition
	{
		get
		{
			return _nextLineAdvanceMovingPosition;
		}
		set
		{
			_nextLineAdvanceMovingPosition = value;
		}
	}

	internal bool _IsNextLineAdvanceMovingPositionAtDocumentHead
	{
		get
		{
			return _isNextLineAdvanceMovingPositionAtDocumentHead;
		}
		set
		{
			_isNextLineAdvanceMovingPositionAtDocumentHead = value;
		}
	}

	internal TextEditor(ITextContainer textContainer, FrameworkElement uiScope, bool isUndoEnabled)
	{
		Invariant.Assert(uiScope != null);
		_acceptsRichContent = true;
		_textContainer = textContainer;
		_uiScope = uiScope;
		if (isUndoEnabled && _textContainer is TextContainer)
		{
			((TextContainer)_textContainer).EnableUndo(_uiScope);
		}
		_selection = new TextSelection(this);
		textContainer.TextSelection = _selection;
		_dragDropProcess = new TextEditorDragDrop._DragDropProcess(this);
		_cursor = Cursors.IBeam;
		TextEditorTyping._AddInputLanguageChangedEventHandler(this);
		TextContainer.Changed += OnTextContainerChanged;
		_uiScope.IsEnabledChanged += OnIsEnabledChanged;
		_uiScope.SetValue(InstanceProperty, this);
		if ((bool)_uiScope.GetValue(SpellCheck.IsEnabledProperty))
		{
			SetSpellCheckEnabled(value: true);
			SetCustomDictionaries(add: true);
		}
		if (!TextServicesLoader.ServicesInstalled)
		{
			GC.SuppressFinalize(this);
		}
	}

	~TextEditor()
	{
		DetachTextStore(finalizer: true);
	}

	internal void OnDetach()
	{
		Invariant.Assert(_textContainer != null);
		SetSpellCheckEnabled(value: false);
		if (UndoManager.GetUndoManager(_uiScope) != null)
		{
			if (_textContainer is TextContainer)
			{
				((TextContainer)_textContainer).DisableUndo(_uiScope);
			}
			else
			{
				UndoManager.DetachUndoManager(_uiScope);
			}
		}
		_textContainer.TextSelection = null;
		TextEditorTyping._RemoveInputLanguageChangedEventHandler(this);
		_textContainer.Changed -= OnTextContainerChanged;
		_uiScope.IsEnabledChanged -= OnIsEnabledChanged;
		_pendingTextStoreInit = false;
		DetachTextStore(finalizer: false);
		if (_immCompositionForDetach != null)
		{
			if (_immCompositionForDetach.TryGetTarget(out var target))
			{
				target.OnDetach(this);
			}
			_immComposition = null;
			_immCompositionForDetach = null;
		}
		TextView = null;
		_selection.OnDetach();
		_selection = null;
		_uiScope.ClearValue(InstanceProperty);
		_uiScope = null;
		_textContainer = null;
	}

	private void DetachTextStore(bool finalizer)
	{
		if (_textstore != null)
		{
			_textstore.OnDetach(finalizer);
			_textstore = null;
		}
		if (_weakThis != null)
		{
			_weakThis.StopListening();
			_weakThis = null;
		}
		if (!finalizer)
		{
			GC.SuppressFinalize(this);
		}
	}

	internal void SetSpellCheckEnabled(bool value)
	{
		value = value && !IsReadOnly && _IsEnabled;
		if (value && _speller == null)
		{
			_speller = new Speller(this);
		}
		else if (!value && _speller != null)
		{
			_speller.Detach();
			_speller = null;
		}
	}

	internal void SetCustomDictionaries(bool add)
	{
		if (_uiScope is TextBoxBase textBoxBase && _speller != null)
		{
			CustomDictionarySources dictionaryLocations = (CustomDictionarySources)SpellCheck.GetCustomDictionaries(textBoxBase);
			_speller.SetCustomDictionaries(dictionaryLocations, add);
		}
	}

	internal void SetSpellingReform(SpellingReform spellingReform)
	{
		if (_speller != null)
		{
			_speller.SetSpellingReform(spellingReform);
		}
	}

	internal static ITextView GetTextView(UIElement scope)
	{
		if (!(scope is IServiceProvider serviceProvider))
		{
			return null;
		}
		return serviceProvider.GetService(typeof(ITextView)) as ITextView;
	}

	internal static ITextSelection GetTextSelection(FrameworkElement frameworkElement)
	{
		return _GetTextEditor(frameworkElement)?.Selection;
	}

	internal static void RegisterCommandHandlers(Type controlType, bool acceptsRichContent, bool readOnly, bool registerEventListeners)
	{
		Invariant.Assert(_registeredEditingTypes != null);
		lock (_registeredEditingTypes)
		{
			for (int i = 0; i < _registeredEditingTypes.Count; i++)
			{
				if (((Type)_registeredEditingTypes[i]).IsAssignableFrom(controlType))
				{
					return;
				}
				if (controlType.IsAssignableFrom((Type)_registeredEditingTypes[i]))
				{
					throw new InvalidOperationException(SR.Format(SR.TextEditorCanNotRegisterCommandHandler, ((Type)_registeredEditingTypes[i]).Name, controlType.Name));
				}
			}
			_registeredEditingTypes.Add(controlType);
		}
		TextEditorMouse._RegisterClassHandlers(controlType, registerEventListeners);
		if (!readOnly)
		{
			TextEditorTyping._RegisterClassHandlers(controlType, registerEventListeners);
		}
		TextEditorDragDrop._RegisterClassHandlers(controlType, readOnly, registerEventListeners);
		TextEditorCopyPaste._RegisterClassHandlers(controlType, acceptsRichContent, readOnly, registerEventListeners);
		TextEditorSelection._RegisterClassHandlers(controlType, registerEventListeners);
		if (!readOnly)
		{
			TextEditorParagraphs._RegisterClassHandlers(controlType, acceptsRichContent, registerEventListeners);
		}
		TextEditorContextMenu._RegisterClassHandlers(controlType, registerEventListeners);
		if (!readOnly)
		{
			TextEditorSpelling._RegisterClassHandlers(controlType, registerEventListeners);
		}
		if (acceptsRichContent && !readOnly)
		{
			TextEditorCharacters._RegisterClassHandlers(controlType, registerEventListeners);
			TextEditorLists._RegisterClassHandlers(controlType, registerEventListeners);
			if (_isTableEditingEnabled)
			{
				TextEditorTables._RegisterClassHandlers(controlType, registerEventListeners);
			}
		}
		if (registerEventListeners)
		{
			EventManager.RegisterClassHandler(controlType, Keyboard.GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnGotKeyboardFocus));
			EventManager.RegisterClassHandler(controlType, Keyboard.LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnLostKeyboardFocus));
			EventManager.RegisterClassHandler(controlType, UIElement.LostFocusEvent, new RoutedEventHandler(OnLostFocus));
		}
		if (!readOnly)
		{
			CommandHelpers.RegisterCommandHandler(controlType, ApplicationCommands.Undo, OnUndo, OnQueryStatusUndo, KeyGesture.CreateFromResourceStrings("Ctrl+Z", SR.KeyUndoDisplayString), KeyGesture.CreateFromResourceStrings("Alt+Backspace", SR.KeyAltUndoDisplayString));
			CommandHelpers.RegisterCommandHandler(controlType, ApplicationCommands.Redo, OnRedo, OnQueryStatusRedo, KeyGesture.CreateFromResourceStrings("Ctrl+Y", "KeyRedoDisplayString"));
		}
	}

	internal SpellingError GetSpellingErrorAtPosition(ITextPointer position, LogicalDirection direction)
	{
		return TextEditorSpelling.GetSpellingErrorAtPosition(this, position, direction);
	}

	internal SpellingError GetSpellingErrorAtSelection()
	{
		return TextEditorSpelling.GetSpellingErrorAtSelection(this);
	}

	internal ITextPointer GetNextSpellingErrorPosition(ITextPointer position, LogicalDirection direction)
	{
		return TextEditorSpelling.GetNextSpellingErrorPosition(this, position, direction);
	}

	internal void SetText(ITextRange range, string text, CultureInfo cultureInfo)
	{
		range.Text = text;
		if (range is TextRange)
		{
			MarkCultureProperty((TextRange)range, cultureInfo);
		}
	}

	internal void SetSelectedText(string text, CultureInfo cultureInfo)
	{
		SetText(Selection, text, cultureInfo);
		((TextSelection)Selection).ApplySpringloadFormatting();
		TextEditorSelection._ClearSuggestedX(this);
	}

	internal void MarkCultureProperty(TextRange range, CultureInfo inputCultureInfo)
	{
		Invariant.Assert(UiScope != null);
		if (AcceptsRichContent)
		{
			XmlLanguage xmlLanguage = (XmlLanguage)((ITextPointer)range.Start).GetValue(FrameworkElement.LanguageProperty);
			Invariant.Assert(xmlLanguage != null);
			if (!string.Equals(inputCultureInfo.IetfLanguageTag, xmlLanguage.IetfLanguageTag, StringComparison.OrdinalIgnoreCase))
			{
				range.ApplyPropertyValue(FrameworkElement.LanguageProperty, XmlLanguage.GetLanguage(inputCultureInfo.IetfLanguageTag));
			}
			FlowDirection flowDirection = (inputCultureInfo.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight);
			if ((FlowDirection)((ITextPointer)range.Start).GetValue(FrameworkElement.FlowDirectionProperty) != flowDirection)
			{
				range.ApplyPropertyValue(FrameworkElement.FlowDirectionProperty, flowDirection);
			}
		}
	}

	internal void RequestExtendSelection(Point point)
	{
		if (_mouseSelectionState == null)
		{
			_mouseSelectionState = new MouseSelectionState();
			_mouseSelectionState.Timer = new DispatcherTimer(DispatcherPriority.Normal);
			_mouseSelectionState.Timer.Tick += HandleMouseSelectionTick;
			_mouseSelectionState.Timer.Interval = TimeSpan.FromMilliseconds(Math.Max(SystemParameters.MenuShowDelay, 200));
			_mouseSelectionState.Timer.Start();
			_mouseSelectionState.Point = point;
			HandleMouseSelectionTick(_mouseSelectionState.Timer, EventArgs.Empty);
		}
		else
		{
			_mouseSelectionState.Point = point;
		}
	}

	internal void CancelExtendSelection()
	{
		if (_mouseSelectionState != null)
		{
			_mouseSelectionState.Timer.Stop();
			_mouseSelectionState.Timer.Tick -= HandleMouseSelectionTick;
			_mouseSelectionState = null;
		}
	}

	internal void CloseToolTip()
	{
		PopupControlService.Current.DismissToolTipsForOwner(_uiScope);
	}

	internal void Undo()
	{
		TextEditorTyping._FlushPendingInputItems(this);
		CompleteComposition();
		_undoState = UndoState.Undo;
		bool coversEntireContent = Selection.CoversEntireContent;
		try
		{
			_selection.BeginChangeNoUndo();
			try
			{
				UndoManager undoManager = _GetUndoManager();
				if (undoManager != null && undoManager.UndoCount > undoManager.MinUndoStackCount)
				{
					undoManager.Undo(1);
				}
				TextEditorSelection._ClearSuggestedX(this);
				TextEditorTyping._BreakTypingSequence(this);
				if (_selection is TextSelection)
				{
					((TextSelection)_selection).ClearSpringloadFormatting();
				}
			}
			finally
			{
				_selection.EndChange();
			}
		}
		finally
		{
			_undoState = UndoState.Normal;
		}
		if (coversEntireContent)
		{
			Selection.ValidateLayout();
		}
	}

	internal void Redo()
	{
		TextEditorTyping._FlushPendingInputItems(this);
		_undoState = UndoState.Redo;
		bool coversEntireContent = Selection.CoversEntireContent;
		try
		{
			_selection.BeginChangeNoUndo();
			try
			{
				UndoManager undoManager = _GetUndoManager();
				if (undoManager != null && undoManager.RedoCount > 0)
				{
					undoManager.Redo(1);
				}
				TextEditorSelection._ClearSuggestedX(this);
				TextEditorTyping._BreakTypingSequence(this);
				if (_selection is TextSelection)
				{
					((TextSelection)_selection).ClearSpringloadFormatting();
				}
			}
			finally
			{
				_selection.EndChange();
			}
		}
		finally
		{
			_undoState = UndoState.Normal;
		}
		if (coversEntireContent)
		{
			Selection.ValidateLayout();
		}
	}

	internal void OnPreviewKeyDown(KeyEventArgs e)
	{
		TextEditorTyping.OnPreviewKeyDown(_uiScope, e);
	}

	internal void OnKeyDown(KeyEventArgs e)
	{
		TextEditorTyping.OnKeyDown(_uiScope, e);
	}

	internal void OnKeyUp(KeyEventArgs e)
	{
		TextEditorTyping.OnKeyUp(_uiScope, e);
	}

	internal void OnTextInput(TextCompositionEventArgs e)
	{
		TextEditorTyping.OnTextInput(_uiScope, e);
	}

	internal void OnMouseDown(MouseButtonEventArgs e)
	{
		TextEditorMouse.OnMouseDown(_uiScope, e);
	}

	internal void OnMouseMove(MouseEventArgs e)
	{
		TextEditorMouse.OnMouseMove(_uiScope, e);
	}

	internal void OnMouseUp(MouseButtonEventArgs e)
	{
		TextEditorMouse.OnMouseUp(_uiScope, e);
	}

	internal void OnQueryCursor(QueryCursorEventArgs e)
	{
		TextEditorMouse.OnQueryCursor(_uiScope, e);
	}

	internal void OnQueryContinueDrag(QueryContinueDragEventArgs e)
	{
		TextEditorDragDrop.OnQueryContinueDrag(_uiScope, e);
	}

	internal void OnGiveFeedback(GiveFeedbackEventArgs e)
	{
		TextEditorDragDrop.OnGiveFeedback(_uiScope, e);
	}

	internal void OnDragEnter(DragEventArgs e)
	{
		TextEditorDragDrop.OnDragEnter(_uiScope, e);
	}

	internal void OnDragOver(DragEventArgs e)
	{
		TextEditorDragDrop.OnDragOver(_uiScope, e);
	}

	internal void OnDragLeave(DragEventArgs e)
	{
		TextEditorDragDrop.OnDragLeave(_uiScope, e);
	}

	internal void OnDrop(DragEventArgs e)
	{
		TextEditorDragDrop.OnDrop(_uiScope, e);
	}

	internal void OnContextMenuOpening(ContextMenuEventArgs e)
	{
		TextEditorContextMenu.OnContextMenuOpening(_uiScope, e);
	}

	internal void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
		OnGotKeyboardFocus(_uiScope, e);
	}

	internal void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
		OnLostKeyboardFocus(_uiScope, e);
	}

	internal void OnLostFocus(RoutedEventArgs e)
	{
		OnLostFocus(_uiScope, e);
	}

	internal static TextEditor _GetTextEditor(object element)
	{
		if (!(element is DependencyObject))
		{
			return null;
		}
		return ((DependencyObject)element).ReadLocalValue(InstanceProperty) as TextEditor;
	}

	internal UndoManager _GetUndoManager()
	{
		UndoManager result = null;
		if (TextContainer is TextContainer)
		{
			result = ((TextContainer)TextContainer).UndoManager;
		}
		return result;
	}

	internal string _FilterText(string textData, ITextRange range)
	{
		return _FilterText(textData, range.Start.GetOffsetToPosition(range.End));
	}

	internal string _FilterText(string textData, int charsToReplaceCount)
	{
		return _FilterText(textData, charsToReplaceCount, filterMaxLength: true);
	}

	internal string _FilterText(string textData, ITextRange range, bool filterMaxLength)
	{
		return _FilterText(textData, range.Start.GetOffsetToPosition(range.End), filterMaxLength);
	}

	internal string _FilterText(string textData, int charsToReplaceCount, bool filterMaxLength)
	{
		if (!AcceptsRichContent)
		{
			if (filterMaxLength && MaxLength > 0)
			{
				ITextContainer textContainer = TextContainer;
				int num = textContainer.SymbolCount - charsToReplaceCount;
				int num2 = Math.Max(0, MaxLength - num);
				if (num2 == 0)
				{
					return string.Empty;
				}
				if (textData.Length > num2)
				{
					int num3 = num2;
					if (IsBadSplitPosition(textData, num3))
					{
						num3--;
					}
					textData = textData.Substring(0, num3);
				}
				if (textData.Length == num2 && char.IsHighSurrogate(textData, num2 - 1))
				{
					textData = textData.Substring(0, num2 - 1);
				}
				if (!string.IsNullOrEmpty(textData) && char.IsLowSurrogate(textData, 0))
				{
					string textInRun = textContainer.TextSelection.AnchorPosition.GetTextInRun(LogicalDirection.Backward);
					if (string.IsNullOrEmpty(textInRun) || !char.IsHighSurrogate(textInRun, textInRun.Length - 1))
					{
						return string.Empty;
					}
				}
			}
			if (string.IsNullOrEmpty(textData))
			{
				return textData;
			}
			if (CharacterCasing == CharacterCasing.Upper)
			{
				textData = textData.ToUpper(InputLanguageManager.Current.CurrentInputLanguage);
			}
			else if (CharacterCasing == CharacterCasing.Lower)
			{
				textData = textData.ToLower(InputLanguageManager.Current.CurrentInputLanguage);
			}
			if (!AcceptsReturn)
			{
				int num4 = textData.IndexOf(Environment.NewLine, StringComparison.Ordinal);
				if (num4 >= 0)
				{
					textData = textData.Substring(0, num4);
				}
				num4 = textData.IndexOfAny(TextPointerBase.NextLineCharacters);
				if (num4 >= 0)
				{
					textData = textData.Substring(0, num4);
				}
			}
			if (!AcceptsTab)
			{
				textData = textData.Replace('\t', ' ');
			}
		}
		return textData;
	}

	internal bool _IsSourceInScope(object source)
	{
		if (source == UiScope)
		{
			return true;
		}
		if (source is FrameworkElement && ((FrameworkElement)source).TemplatedParent == UiScope)
		{
			return true;
		}
		return false;
	}

	internal void CompleteComposition()
	{
		if (TextStore != null)
		{
			TextStore.CompleteComposition();
		}
		if (ImmComposition != null)
		{
			ImmComposition.CompleteComposition();
		}
	}

	private bool IsBadSplitPosition(string text, int position)
	{
		if ((text[position - 1] == '\r' && text[position] == '\n') || (char.IsHighSurrogate(text, position - 1) && char.IsLowSurrogate(text, position)))
		{
			return true;
		}
		return false;
	}

	private void HandleMouseSelectionTick(object sender, EventArgs e)
	{
		if (_mouseSelectionState != null && !_mouseSelectionState.BringIntoViewInProgress && TextView != null && TextView.IsValid && TextEditorSelection.IsPaginated(TextView))
		{
			_mouseSelectionState.BringIntoViewInProgress = true;
			TextView.BringPointIntoViewCompleted += HandleBringPointIntoViewCompleted;
			TextView.BringPointIntoViewAsync(_mouseSelectionState.Point, this);
		}
	}

	private void HandleBringPointIntoViewCompleted(object sender, BringPointIntoViewCompletedEventArgs e)
	{
		Invariant.Assert(sender is ITextView);
		((ITextView)sender).BringPointIntoViewCompleted -= HandleBringPointIntoViewCompleted;
		if (_mouseSelectionState == null)
		{
			return;
		}
		_mouseSelectionState.BringIntoViewInProgress = false;
		if (e != null && !e.Cancelled && e.Error == null)
		{
			Invariant.Assert(e.UserState == this && TextView == sender);
			ITextPointer textPointer = e.Position;
			if (textPointer != null)
			{
				if (textPointer.GetNextInsertionPosition(LogicalDirection.Forward) == null && textPointer.ParentType != null)
				{
					Rect characterRect = textPointer.GetCharacterRect(LogicalDirection.Backward);
					if (e.Point.X > characterRect.X + characterRect.Width)
					{
						textPointer = TextContainer.End;
					}
				}
				Selection.ExtendSelectionByMouse(textPointer, _forceWordSelection, _forceParagraphSelection);
			}
			else
			{
				CancelExtendSelection();
			}
		}
		else
		{
			CancelExtendSelection();
		}
	}

	private object InitTextStore(object o)
	{
		if (!_pendingTextStoreInit)
		{
			return null;
		}
		if (_textContainer is TextContainer && TextServicesHost.Current != null)
		{
			MS.Win32.UnsafeNativeMethods.ITfThreadMgr tfThreadMgr = TextServicesLoader.Load();
			if (tfThreadMgr != null)
			{
				if (_textstore == null)
				{
					_textstore = new TextStore(this);
					_weakThis = new TextEditorShutDownListener(this);
				}
				_textstore.OnAttach();
				Marshal.ReleaseComObject(tfThreadMgr);
			}
		}
		_pendingTextStoreInit = false;
		return null;
	}

	private void OnTextContainerChanged(object sender, TextContainerChangedEventArgs e)
	{
		_contentChangeCounter++;
	}

	private void OnTextViewUpdated(object sender, EventArgs e)
	{
		_selection.OnTextViewUpdated();
		UiScope.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(OnTextViewUpdatedWorker), EventArgs.Empty);
		if (!_textStoreInitStarted)
		{
			Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(InitTextStore), null);
			_pendingTextStoreInit = true;
			_textStoreInitStarted = true;
		}
	}

	private object OnTextViewUpdatedWorker(object o)
	{
		if (TextView == null)
		{
			return null;
		}
		if (_textstore != null)
		{
			_textstore.OnLayoutUpdated();
		}
		if (_immEnabled && _immComposition != null)
		{
			_immComposition.OnLayoutUpdated();
		}
		return null;
	}

	private static void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		TextEditor textEditor = _GetTextEditor((FrameworkElement)sender);
		if (textEditor != null)
		{
			textEditor._selection.UpdateCaretAndHighlight();
			textEditor.SetSpellCheckEnabled(textEditor.IsSpellCheckEnabled);
			textEditor.SetCustomDictionaries(textEditor.IsSpellCheckEnabled);
		}
	}

	private static void OnIsReadOnlyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (!(sender is FrameworkElement element))
		{
			return;
		}
		TextEditor textEditor = _GetTextEditor(element);
		if (textEditor != null)
		{
			textEditor.SetSpellCheckEnabled(textEditor.IsSpellCheckEnabled);
			if ((bool)e.NewValue && textEditor._textstore != null)
			{
				textEditor._textstore.CompleteCompositionAsync();
			}
		}
	}

	private static void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
	{
		if (sender != e.NewFocus)
		{
			return;
		}
		TextEditor textEditor = _GetTextEditor((FrameworkElement)sender);
		if (textEditor == null || !textEditor._IsEnabled)
		{
			return;
		}
		if (textEditor._textstore != null)
		{
			textEditor._textstore.OnGotFocus();
		}
		if (_immEnabled)
		{
			textEditor._immComposition = ImmComposition.GetImmComposition(textEditor._uiScope);
			if (textEditor._immComposition != null)
			{
				textEditor._immCompositionForDetach = new WeakReference<ImmComposition>(textEditor._immComposition);
				textEditor._immComposition.OnGotFocus(textEditor);
			}
			else
			{
				textEditor._immCompositionForDetach = null;
			}
		}
		textEditor._selection.RefreshCaret();
		textEditor._selection.UpdateCaretAndHighlight();
	}

	private static void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
	{
		if (sender != e.OldFocus)
		{
			return;
		}
		TextEditor textEditor = _GetTextEditor((FrameworkElement)sender);
		if (textEditor != null && textEditor._IsEnabled)
		{
			textEditor._selection.UpdateCaretAndHighlight();
			if (textEditor._textstore != null)
			{
				textEditor._textstore.OnLostFocus();
			}
			if (_immEnabled && textEditor._immComposition != null)
			{
				textEditor._immComposition.OnLostFocus();
				textEditor._immComposition = null;
			}
		}
	}

	private static void OnLostFocus(object sender, RoutedEventArgs e)
	{
		TextEditor textEditor = _GetTextEditor((FrameworkElement)sender);
		if (textEditor == null)
		{
			return;
		}
		TextEditorTyping._ShowCursor();
		if (textEditor._IsEnabled)
		{
			TextEditorTyping._FlushPendingInputItems(textEditor);
			TextEditorTyping._BreakTypingSequence(textEditor);
			if (textEditor._tableColResizeInfo != null)
			{
				textEditor._tableColResizeInfo.DisposeAdorner();
				textEditor._tableColResizeInfo = null;
			}
			textEditor._selection.UpdateCaretAndHighlight();
		}
	}

	private static void OnUndo(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = _GetTextEditor((FrameworkElement)target);
		if (textEditor != null && textEditor._IsEnabled && !textEditor.IsReadOnly)
		{
			textEditor.Undo();
		}
	}

	private static void OnRedo(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = _GetTextEditor((FrameworkElement)target);
		if (textEditor != null && textEditor._IsEnabled && !textEditor.IsReadOnly)
		{
			textEditor.Redo();
		}
	}

	private static void OnQueryStatusUndo(object sender, CanExecuteRoutedEventArgs args)
	{
		TextEditor textEditor = _GetTextEditor((FrameworkElement)sender);
		if (textEditor != null)
		{
			UndoManager undoManager = textEditor._GetUndoManager();
			if (undoManager != null && undoManager.UndoCount > undoManager.MinUndoStackCount)
			{
				args.CanExecute = true;
			}
		}
	}

	private static void OnQueryStatusRedo(object sender, CanExecuteRoutedEventArgs args)
	{
		TextEditor textEditor = _GetTextEditor((FrameworkElement)sender);
		if (textEditor != null)
		{
			UndoManager undoManager = textEditor._GetUndoManager();
			if (undoManager != null && undoManager.RedoCount > 0)
			{
				args.CanExecute = true;
			}
		}
	}
}
