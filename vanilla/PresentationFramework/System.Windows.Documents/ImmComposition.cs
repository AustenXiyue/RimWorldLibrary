using System.Collections;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.Documents;
using MS.Internal.Interop;
using MS.Win32;

namespace System.Windows.Documents;

internal class ImmComposition
{
	private HwndSource _source;

	private TextEditor _editor;

	private ITextPointer _startComposition;

	private ITextPointer _endComposition;

	private int _caretOffset;

	private CompositionAdorner _compositionAdorner;

	private static Hashtable _list;

	private const double _dashLength = 2.0;

	private const int _maxSrounding = 32;

	private MS.Win32.NativeMethods.RECONVERTSTRING _reconv;

	private bool _isReconvReady;

	private static WindowMessage s_MsImeMouseMessage;

	private TextParentUndoUnit _compositionUndoUnit;

	private bool _handlingImeMessage;

	private bool _updateCompWndPosAtNextLayoutUpdate;

	private bool _compositionModifiedByEventListener;

	private bool _handledByEditorListener;

	private bool _losingFocus;

	internal bool IsComposition => _startComposition != null;

	private UIElement RenderScope
	{
		get
		{
			if (_editor.TextView != null)
			{
				return _editor.TextView.RenderScope;
			}
			return null;
		}
	}

	private FrameworkElement UiScope
	{
		get
		{
			if (_editor != null)
			{
				return _editor.UiScope;
			}
			return null;
		}
	}

	private bool IsReadOnly
	{
		get
		{
			if (!(bool)UiScope.GetValue(TextEditor.IsReadOnlyProperty))
			{
				return _editor.IsReadOnly;
			}
			return true;
		}
	}

	private bool IsInKeyboardFocus
	{
		get
		{
			if (_editor == null)
			{
				return false;
			}
			if (UiScope == null)
			{
				return false;
			}
			if (!UiScope.IsKeyboardFocused)
			{
				return false;
			}
			return true;
		}
	}

	static ImmComposition()
	{
		_list = new Hashtable(1);
		s_MsImeMouseMessage = MS.Win32.UnsafeNativeMethods.RegisterWindowMessage("MSIMEMouseOperation");
	}

	internal ImmComposition(HwndSource source)
	{
		UpdateSource(null, source);
	}

	internal static ImmComposition GetImmComposition(FrameworkElement scope)
	{
		HwndSource hwndSource = PresentationSource.CriticalFromVisual(scope) as HwndSource;
		ImmComposition immComposition = null;
		if (hwndSource != null)
		{
			lock (_list)
			{
				immComposition = (ImmComposition)_list[hwndSource];
				if (immComposition == null)
				{
					immComposition = new ImmComposition(hwndSource);
					_list[hwndSource] = immComposition;
				}
			}
		}
		return immComposition;
	}

	internal void OnDetach(TextEditor editor)
	{
		if (editor == _editor)
		{
			if (_editor != null)
			{
				PresentationSource.RemoveSourceChangedHandler(UiScope, OnSourceChanged);
				_editor.TextContainer.Change -= OnTextContainerChange;
			}
			_editor = null;
		}
	}

	internal void OnGotFocus(TextEditor editor)
	{
		if (editor != _editor)
		{
			if (_editor != null)
			{
				PresentationSource.RemoveSourceChangedHandler(UiScope, OnSourceChanged);
				_editor.TextContainer.Change -= OnTextContainerChange;
			}
			_editor = editor;
			PresentationSource.AddSourceChangedHandler(UiScope, OnSourceChanged);
			_editor.TextContainer.Change += OnTextContainerChange;
			UpdateNearCaretCompositionWindow();
		}
	}

	internal void OnLostFocus()
	{
		if (_editor == null)
		{
			return;
		}
		_losingFocus = true;
		try
		{
			CompleteComposition();
		}
		finally
		{
			_losingFocus = false;
		}
	}

	internal void OnLayoutUpdated()
	{
		if (_updateCompWndPosAtNextLayoutUpdate && IsReadingWindowIme())
		{
			UpdateNearCaretCompositionWindow();
		}
		_updateCompWndPosAtNextLayoutUpdate = false;
	}

	internal void CompleteComposition()
	{
		UnregisterMouseListeners();
		if (_source != null)
		{
			_compositionModifiedByEventListener = true;
			nint zero = IntPtr.Zero;
			zero = ((IWin32Window)_source).Handle;
			nint num = MS.Win32.UnsafeNativeMethods.ImmGetContext(new HandleRef(this, zero));
			if (num != IntPtr.Zero)
			{
				MS.Win32.UnsafeNativeMethods.ImmNotifyIME(new HandleRef(this, num), 21, 1, 0);
				MS.Win32.UnsafeNativeMethods.ImmReleaseContext(new HandleRef(this, zero), new HandleRef(this, num));
			}
			if (_compositionAdorner != null)
			{
				_compositionAdorner.Uninitialize();
				_compositionAdorner = null;
			}
			_startComposition = null;
			_endComposition = null;
		}
	}

	internal void OnSelectionChange()
	{
		_compositionModifiedByEventListener = true;
	}

	internal void OnSelectionChanged()
	{
		if (IsInKeyboardFocus)
		{
			UpdateNearCaretCompositionWindow();
		}
	}

	private void OnSourceChanged(object sender, SourceChangedEventArgs e)
	{
		HwndSource hwndSource = null;
		HwndSource hwndSource2 = null;
		hwndSource = e.NewSource as HwndSource;
		hwndSource2 = e.OldSource as HwndSource;
		UpdateSource(hwndSource2, hwndSource);
		if (hwndSource2 != null && UiScope != null)
		{
			PresentationSource.RemoveSourceChangedHandler(UiScope, OnSourceChanged);
		}
	}

	private void UpdateSource(HwndSource oldSource, HwndSource newSource)
	{
		OnDetach(_editor);
		if (_source != null)
		{
			_source.RemoveHook(ImmCompositionFilterMessage);
			_source.Disposed -= OnHwndDisposed;
			_list.Remove(_source);
			_source = null;
		}
		if (newSource != null)
		{
			_list[newSource] = this;
			_source = newSource;
			_source.AddHook(ImmCompositionFilterMessage);
			_source.Disposed += OnHwndDisposed;
		}
	}

	private nint ImmCompositionFilterMessage(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		nint result = IntPtr.Zero;
		switch ((WindowMessage)msg)
		{
		case WindowMessage.WM_IME_CHAR:
			OnWmImeChar(wParam, ref handled);
			break;
		case WindowMessage.WM_IME_NOTIFY:
			OnWmImeNotify(hwnd, wParam);
			break;
		case WindowMessage.WM_IME_STARTCOMPOSITION:
		case WindowMessage.WM_IME_ENDCOMPOSITION:
			if (IsInKeyboardFocus && !IsReadOnly && !IsReadingWindowIme())
			{
				handled = true;
			}
			break;
		case WindowMessage.WM_IME_COMPOSITION:
			OnWmImeComposition(hwnd, lParam, ref handled);
			break;
		case WindowMessage.WM_IME_REQUEST:
			result = OnWmImeRequest(wParam, lParam, ref handled);
			break;
		case WindowMessage.WM_INPUTLANGCHANGE:
			if (IsReadingWindowIme())
			{
				UpdateNearCaretCompositionWindow();
			}
			break;
		}
		return result;
	}

	private void OnWmImeComposition(nint hwnd, nint lParam, ref bool handled)
	{
		int caretOffset = 0;
		int deltaStart = 0;
		char[] array = null;
		char[] array2 = null;
		byte[] array3 = null;
		if (IsReadingWindowIme() || (!IsInKeyboardFocus && !_losingFocus) || IsReadOnly)
		{
			return;
		}
		nint num = MS.Win32.UnsafeNativeMethods.ImmGetContext(new HandleRef(this, hwnd));
		if (num == IntPtr.Zero)
		{
			return;
		}
		if (((int)lParam & 0x800) != 0)
		{
			int num2 = MS.Win32.UnsafeNativeMethods.ImmGetCompositionString(new HandleRef(this, num), 2048, IntPtr.Zero, 0);
			if (num2 > 0)
			{
				array = new char[num2 / Marshal.SizeOf(typeof(short))];
				MS.Win32.UnsafeNativeMethods.ImmGetCompositionString(new HandleRef(this, num), 2048, array, num2);
			}
		}
		if (((int)lParam & 8) != 0)
		{
			int num2 = MS.Win32.UnsafeNativeMethods.ImmGetCompositionString(new HandleRef(this, num), 8, IntPtr.Zero, 0);
			if (num2 > 0)
			{
				array2 = new char[num2 / Marshal.SizeOf(typeof(short))];
				MS.Win32.UnsafeNativeMethods.ImmGetCompositionString(new HandleRef(this, num), 8, array2, num2);
				if (((int)lParam & 0x80) != 0)
				{
					caretOffset = MS.Win32.UnsafeNativeMethods.ImmGetCompositionString(new HandleRef(this, num), 128, IntPtr.Zero, 0);
				}
				if (((int)lParam & 0x100) != 0)
				{
					deltaStart = MS.Win32.UnsafeNativeMethods.ImmGetCompositionString(new HandleRef(this, num), 256, IntPtr.Zero, 0);
				}
				if (((int)lParam & 0x10) != 0)
				{
					num2 = MS.Win32.UnsafeNativeMethods.ImmGetCompositionString(new HandleRef(this, num), 16, IntPtr.Zero, 0);
					if (num2 > 0)
					{
						array3 = new byte[num2 / Marshal.SizeOf(typeof(byte))];
						MS.Win32.UnsafeNativeMethods.ImmGetCompositionString(new HandleRef(this, num), 16, array3, num2);
					}
				}
			}
		}
		UpdateCompositionString(array, array2, caretOffset, deltaStart, array3);
		MS.Win32.UnsafeNativeMethods.ImmReleaseContext(new HandleRef(this, hwnd), new HandleRef(this, num));
		handled = true;
	}

	private void OnWmImeChar(nint wParam, ref bool handled)
	{
		if ((!IsInKeyboardFocus && !_losingFocus) || IsReadOnly || _handlingImeMessage)
		{
			return;
		}
		_handlingImeMessage = true;
		try
		{
			int resultLength;
			string text = BuildCompositionString(null, new char[1] { (char)wParam }, out resultLength);
			if (text == null)
			{
				CompleteComposition();
			}
			else
			{
				FrameworkTextComposition composition = TextStore.CreateComposition(_editor, this);
				_compositionModifiedByEventListener = false;
				_caretOffset = 1;
				if (RaiseTextInputStartEvent(composition, resultLength, text))
				{
					CompleteComposition();
				}
				else if (RaiseTextInputEvent(composition, text))
				{
					CompleteComposition();
					goto IL_00a2;
				}
			}
		}
		finally
		{
			_handlingImeMessage = false;
		}
		if (IsReadingWindowIme())
		{
			UpdateNearCaretCompositionWindow();
		}
		goto IL_00a2;
		IL_00a2:
		handled = true;
	}

	private void OnWmImeNotify(nint hwnd, nint wParam)
	{
		if (!IsInKeyboardFocus || (int)wParam != 5)
		{
			return;
		}
		nint num = MS.Win32.UnsafeNativeMethods.ImmGetContext(new HandleRef(this, hwnd));
		if (num == IntPtr.Zero)
		{
			return;
		}
		MS.Win32.NativeMethods.CANDIDATEFORM candform = default(MS.Win32.NativeMethods.CANDIDATEFORM);
		if (IsReadingWindowIme())
		{
			candform.dwIndex = 0;
			candform.dwStyle = 0;
			candform.rcArea.left = 0;
			candform.rcArea.right = 0;
			candform.rcArea.top = 0;
			candform.rcArea.bottom = 0;
			candform.ptCurrentPos = default(MS.Win32.NativeMethods.POINT);
		}
		else
		{
			CompositionTarget compositionTarget = _source.CompositionTarget;
			ITextPointer textPointer = ((_startComposition == null) ? _editor.Selection.Start.CreatePointer() : _startComposition.CreatePointer());
			ITextPointer textPointer2 = ((_endComposition == null) ? _editor.Selection.End.CreatePointer() : _endComposition.CreatePointer());
			ITextPointer textPointer3 = ((_startComposition == null) ? _editor.Selection.End.CreatePointer() : ((_caretOffset > 0) ? _startComposition.CreatePointer(_caretOffset, LogicalDirection.Forward) : _endComposition));
			ITextPointer textPointer4 = textPointer.CreatePointer(LogicalDirection.Forward);
			ITextPointer textPointer5 = textPointer2.CreatePointer(LogicalDirection.Backward);
			ITextPointer textPointer6 = textPointer3.CreatePointer(LogicalDirection.Forward);
			if (!textPointer4.ValidateLayout() || !textPointer5.ValidateLayout() || !textPointer6.ValidateLayout())
			{
				return;
			}
			ITextView textView = TextEditor.GetTextView(RenderScope);
			Rect rectangleFromTextPosition = textView.GetRectangleFromTextPosition(textPointer4);
			Rect rectangleFromTextPosition2 = textView.GetRectangleFromTextPosition(textPointer5);
			Rect rectangleFromTextPosition3 = textView.GetRectangleFromTextPosition(textPointer6);
			Point result = new Point(Math.Min(rectangleFromTextPosition.Left, rectangleFromTextPosition2.Left), Math.Min(rectangleFromTextPosition.Top, rectangleFromTextPosition2.Top));
			Point result2 = new Point(Math.Max(rectangleFromTextPosition.Left, rectangleFromTextPosition2.Left), Math.Max(rectangleFromTextPosition.Bottom, rectangleFromTextPosition2.Bottom));
			Point result3 = new Point(rectangleFromTextPosition3.Left, rectangleFromTextPosition3.Bottom);
			GeneralTransform generalTransform = RenderScope.TransformToAncestor(compositionTarget.RootVisual);
			generalTransform.TryTransform(result, out result);
			generalTransform.TryTransform(result2, out result2);
			generalTransform.TryTransform(result3, out result3);
			result = compositionTarget.TransformToDevice.Transform(result);
			result2 = compositionTarget.TransformToDevice.Transform(result2);
			result3 = compositionTarget.TransformToDevice.Transform(result3);
			candform.dwIndex = 0;
			candform.dwStyle = 128;
			candform.rcArea.left = ConvertToInt32(result.X);
			candform.rcArea.right = ConvertToInt32(result2.X);
			candform.rcArea.top = ConvertToInt32(result.Y);
			candform.rcArea.bottom = ConvertToInt32(result2.Y);
			candform.ptCurrentPos = new MS.Win32.NativeMethods.POINT(ConvertToInt32(result3.X), ConvertToInt32(result3.Y));
		}
		MS.Win32.UnsafeNativeMethods.ImmSetCandidateWindow(new HandleRef(this, num), ref candform);
		MS.Win32.UnsafeNativeMethods.ImmReleaseContext(new HandleRef(this, hwnd), new HandleRef(this, num));
	}

	private void UpdateNearCaretCompositionWindow()
	{
		if (!IsInKeyboardFocus || _source == null)
		{
			return;
		}
		nint handle = ((IWin32Window)_source).Handle;
		Rect visualContentBounds = UiScope.VisualContentBounds;
		ITextView textView = _editor.TextView;
		if (!_editor.Selection.End.HasValidLayout)
		{
			_updateCompWndPosAtNextLayoutUpdate = true;
			return;
		}
		CompositionTarget compositionTarget = _source.CompositionTarget;
		if (compositionTarget != null && compositionTarget.RootVisual != null && compositionTarget.RootVisual.IsAncestorOf(RenderScope))
		{
			nint num = MS.Win32.UnsafeNativeMethods.ImmGetContext(new HandleRef(this, handle));
			if (num != IntPtr.Zero)
			{
				Rect rectangleFromTextPosition = textView.GetRectangleFromTextPosition(_editor.Selection.End.CreatePointer(LogicalDirection.Backward));
				Point result = new Point(visualContentBounds.Left, visualContentBounds.Top);
				Point result2 = new Point(visualContentBounds.Right, visualContentBounds.Bottom);
				Point result3 = new Point(rectangleFromTextPosition.Left, rectangleFromTextPosition.Bottom);
				GeneralTransform generalTransform = RenderScope.TransformToAncestor(compositionTarget.RootVisual);
				generalTransform.TryTransform(result, out result);
				generalTransform.TryTransform(result2, out result2);
				generalTransform.TryTransform(result3, out result3);
				result = compositionTarget.TransformToDevice.Transform(result);
				result2 = compositionTarget.TransformToDevice.Transform(result2);
				result3 = compositionTarget.TransformToDevice.Transform(result3);
				MS.Win32.NativeMethods.COMPOSITIONFORM compform = default(MS.Win32.NativeMethods.COMPOSITIONFORM);
				compform.dwStyle = 1;
				compform.rcArea.left = ConvertToInt32(result.X);
				compform.rcArea.right = ConvertToInt32(result2.X);
				compform.rcArea.top = ConvertToInt32(result.Y);
				compform.rcArea.bottom = ConvertToInt32(result2.Y);
				compform.ptCurrentPos = new MS.Win32.NativeMethods.POINT(ConvertToInt32(result3.X), ConvertToInt32(result3.Y));
				MS.Win32.UnsafeNativeMethods.ImmSetCompositionWindow(new HandleRef(this, num), ref compform);
				MS.Win32.UnsafeNativeMethods.ImmReleaseContext(new HandleRef(this, handle), new HandleRef(this, num));
			}
		}
	}

	private void OnHwndDisposed(object sender, EventArgs args)
	{
		UpdateSource(_source, null);
	}

	private void UpdateCompositionString(char[] resultChars, char[] compositionChars, int caretOffset, int deltaStart, byte[] attributes)
	{
		if (_handlingImeMessage)
		{
			return;
		}
		_handlingImeMessage = true;
		try
		{
			if (_compositionAdorner != null)
			{
				_compositionAdorner.Uninitialize();
				_compositionAdorner = null;
			}
			int resultLength;
			string text = BuildCompositionString(resultChars, compositionChars, out resultLength);
			if (text == null)
			{
				CompleteComposition();
				return;
			}
			RecordCaretOffset(caretOffset, attributes, text.Length);
			FrameworkTextComposition composition = TextStore.CreateComposition(_editor, this);
			_compositionModifiedByEventListener = false;
			if (_startComposition == null)
			{
				Invariant.Assert(_endComposition == null);
				if (RaiseTextInputStartEvent(composition, resultLength, text))
				{
					CompleteComposition();
					return;
				}
			}
			else if (compositionChars != null && RaiseTextInputUpdateEvent(composition, resultLength, text))
			{
				CompleteComposition();
				return;
			}
			if (compositionChars == null && RaiseTextInputEvent(composition, text))
			{
				CompleteComposition();
			}
			else if (_startComposition != null)
			{
				SetCompositionAdorner(attributes);
			}
		}
		finally
		{
			_handlingImeMessage = false;
		}
	}

	private string BuildCompositionString(char[] resultChars, char[] compositionChars, out int resultLength)
	{
		int num = ((compositionChars != null) ? compositionChars.Length : 0);
		resultLength = ((resultChars != null) ? resultChars.Length : 0);
		char[] array;
		if (resultChars == null)
		{
			array = compositionChars;
		}
		else if (compositionChars == null)
		{
			array = resultChars;
		}
		else
		{
			array = new char[resultLength + num];
			Array.Copy(resultChars, 0, array, 0, resultLength);
			Array.Copy(compositionChars, 0, array, resultLength, num);
		}
		string text = new string(array);
		int num2 = ((array != null) ? array.Length : 0);
		if (text.Length != num2)
		{
			return null;
		}
		return text;
	}

	private void RecordCaretOffset(int caretOffset, byte[] attributes, int compositionLength)
	{
		if (attributes != null && ((caretOffset >= 0 && caretOffset < attributes.Length && attributes[caretOffset] == 0) || (caretOffset > 0 && caretOffset - 1 < attributes.Length && attributes[caretOffset - 1] == 0)))
		{
			_caretOffset = caretOffset;
		}
		else
		{
			_caretOffset = -1;
		}
	}

	private bool RaiseTextInputStartEvent(FrameworkTextComposition composition, int resultLength, string compositionString)
	{
		composition.Stage = TextCompositionStage.None;
		composition.SetCompositionPositions(_editor.Selection.Start, _editor.Selection.End, compositionString);
		if (TextCompositionManager.StartComposition(composition) || composition.PendingComplete || _compositionModifiedByEventListener)
		{
			return true;
		}
		UpdateCompositionText(composition, resultLength, includeResultText: true, out _startComposition, out _endComposition);
		if (_compositionModifiedByEventListener)
		{
			return true;
		}
		RegisterMouseListeners();
		return false;
	}

	private bool RaiseTextInputUpdateEvent(FrameworkTextComposition composition, int resultLength, string compositionString)
	{
		composition.Stage = TextCompositionStage.Started;
		composition.SetCompositionPositions(_startComposition, _endComposition, compositionString);
		if (TextCompositionManager.UpdateComposition(composition) || composition.PendingComplete || _compositionModifiedByEventListener)
		{
			return true;
		}
		UpdateCompositionText(composition, resultLength, includeResultText: false, out _startComposition, out _endComposition);
		if (_compositionModifiedByEventListener)
		{
			return true;
		}
		return false;
	}

	private bool RaiseTextInputEvent(FrameworkTextComposition composition, string compositionString)
	{
		composition.Stage = TextCompositionStage.Started;
		composition.SetResultPositions(_startComposition, _endComposition, compositionString);
		_startComposition = null;
		_endComposition = null;
		UnregisterMouseListeners();
		_handledByEditorListener = false;
		TextCompositionManager.CompleteComposition(composition);
		_compositionUndoUnit = null;
		if (_handledByEditorListener && !composition.PendingComplete)
		{
			return _compositionModifiedByEventListener;
		}
		return true;
	}

	internal void UpdateCompositionText(FrameworkTextComposition composition)
	{
		UpdateCompositionText(composition, 0, includeResultText: true, out var _, out var _);
	}

	internal void UpdateCompositionText(FrameworkTextComposition composition, int resultLength, bool includeResultText, out ITextPointer start, out ITextPointer end)
	{
		start = null;
		end = null;
		if (_compositionModifiedByEventListener)
		{
			return;
		}
		_handledByEditorListener = true;
		bool flag = false;
		UndoCloseAction undoCloseAction = UndoCloseAction.Rollback;
		OpenCompositionUndoUnit();
		try
		{
			_editor.Selection.BeginChange();
			try
			{
				ITextRange textRange;
				string text;
				if (composition._ResultStart != null)
				{
					textRange = new TextRange(composition._ResultStart, composition._ResultEnd, ignoreTextUnitBoundaries: true);
					text = _editor._FilterText(composition.Text, textRange);
					flag = text != composition.Text;
					if (flag)
					{
						_caretOffset = Math.Min(_caretOffset, text.Length);
					}
				}
				else
				{
					textRange = new TextRange(composition._CompositionStart, composition._CompositionEnd, ignoreTextUnitBoundaries: true);
					text = composition.CompositionText;
				}
				_editor.SetText(textRange, text, InputLanguageManager.Current.CurrentInputLanguage);
				if (includeResultText)
				{
					start = textRange.Start;
				}
				else
				{
					start = textRange.Start.CreatePointer(resultLength, LogicalDirection.Forward);
				}
				end = textRange.End;
				ITextPointer textPointer = ((_caretOffset >= 0) ? start.CreatePointer(_caretOffset, LogicalDirection.Forward) : end);
				_editor.Selection.Select(textPointer, textPointer);
			}
			finally
			{
				_compositionModifiedByEventListener = false;
				_editor.Selection.EndChange();
				if (flag)
				{
					_compositionModifiedByEventListener = true;
				}
			}
			undoCloseAction = UndoCloseAction.Commit;
		}
		finally
		{
			CloseCompositionUndoUnit(undoCloseAction, end);
		}
	}

	private void SetCompositionAdorner(byte[] attributes)
	{
		if (attributes == null)
		{
			return;
		}
		int offset = 0;
		for (int i = 0; i < attributes.Length; i++)
		{
			if (i + 1 >= attributes.Length || attributes[i] != attributes[i + 1])
			{
				ITextPointer start = _startComposition.CreatePointer(offset, LogicalDirection.Backward);
				ITextPointer end = _startComposition.CreatePointer(i + 1, LogicalDirection.Forward);
				if (_compositionAdorner == null)
				{
					_compositionAdorner = new CompositionAdorner(_editor.TextView);
					_compositionAdorner.Initialize(_editor.TextView);
				}
				MS.Win32.UnsafeNativeMethods.TF_DISPLAYATTRIBUTE attr = default(MS.Win32.UnsafeNativeMethods.TF_DISPLAYATTRIBUTE);
				attr.crLine.type = MS.Win32.UnsafeNativeMethods.TF_DA_COLORTYPE.TF_CT_COLORREF;
				attr.crLine.indexOrColorRef = 0;
				attr.lsStyle = MS.Win32.UnsafeNativeMethods.TF_DA_LINESTYLE.TF_LS_NONE;
				attr.fBoldLine = false;
				switch (attributes[i])
				{
				case 0:
					attr.lsStyle = MS.Win32.UnsafeNativeMethods.TF_DA_LINESTYLE.TF_LS_DOT;
					break;
				case 1:
					attr.lsStyle = MS.Win32.UnsafeNativeMethods.TF_DA_LINESTYLE.TF_LS_SOLID;
					attr.fBoldLine = true;
					break;
				case 2:
					attr.lsStyle = MS.Win32.UnsafeNativeMethods.TF_DA_LINESTYLE.TF_LS_DOT;
					break;
				case 3:
					attr.lsStyle = MS.Win32.UnsafeNativeMethods.TF_DA_LINESTYLE.TF_LS_SOLID;
					break;
				}
				TextServicesDisplayAttribute textServiceDisplayAttribute = new TextServicesDisplayAttribute(attr);
				_compositionAdorner.AddAttributeRange(start, end, textServiceDisplayAttribute);
				offset = i + 1;
			}
		}
		if (_compositionAdorner != null)
		{
			_editor.TextView.RenderScope.UpdateLayout();
			_compositionAdorner.InvalidateAdorner();
		}
	}

	private void RegisterMouseListeners()
	{
		UiScope.PreviewMouseLeftButtonDown += OnMouseButtonEvent;
		UiScope.PreviewMouseLeftButtonUp += OnMouseButtonEvent;
		UiScope.PreviewMouseRightButtonDown += OnMouseButtonEvent;
		UiScope.PreviewMouseRightButtonUp += OnMouseButtonEvent;
		UiScope.PreviewMouseMove += OnMouseEvent;
	}

	private void UnregisterMouseListeners()
	{
		if (UiScope != null)
		{
			UiScope.PreviewMouseLeftButtonDown -= OnMouseButtonEvent;
			UiScope.PreviewMouseLeftButtonUp -= OnMouseButtonEvent;
			UiScope.PreviewMouseRightButtonDown -= OnMouseButtonEvent;
			UiScope.PreviewMouseRightButtonUp -= OnMouseButtonEvent;
			UiScope.PreviewMouseMove -= OnMouseEvent;
		}
	}

	private nint OnWmImeRequest(nint wParam, nint lParam, ref bool handled)
	{
		nint result = IntPtr.Zero;
		switch ((int)wParam)
		{
		case 4:
			result = OnWmImeRequest_ReconvertString(lParam, ref handled, fDocFeed: false);
			break;
		case 5:
			result = OnWmImeRequest_ConfirmReconvertString(lParam, ref handled);
			break;
		case 7:
			result = OnWmImeRequest_ReconvertString(lParam, ref handled, fDocFeed: true);
			break;
		}
		return result;
	}

	private nint OnWmImeRequest_ReconvertString(nint lParam, ref bool handled, bool fDocFeed)
	{
		if (!fDocFeed)
		{
			_isReconvReady = false;
		}
		if (!IsInKeyboardFocus)
		{
			return IntPtr.Zero;
		}
		ITextRange textRange = ((!fDocFeed || _startComposition == null || _endComposition == null) ? ((ITextRange)_editor.Selection) : ((ITextRange)new TextRange(_startComposition, _endComposition)));
		string text = textRange.Text;
		int num = Marshal.SizeOf(typeof(MS.Win32.NativeMethods.RECONVERTSTRING)) + text.Length * Marshal.SizeOf(typeof(short)) + 33 * Marshal.SizeOf(typeof(short)) * 2;
		nint result = new IntPtr(num);
		if (lParam != IntPtr.Zero)
		{
			int offsetStart;
			string surroundingText = GetSurroundingText(textRange, out offsetStart);
			MS.Win32.NativeMethods.RECONVERTSTRING rECONVERTSTRING = Marshal.PtrToStructure<MS.Win32.NativeMethods.RECONVERTSTRING>(lParam);
			rECONVERTSTRING.dwSize = num;
			rECONVERTSTRING.dwVersion = 0;
			rECONVERTSTRING.dwStrLen = surroundingText.Length;
			rECONVERTSTRING.dwStrOffset = Marshal.SizeOf(typeof(MS.Win32.NativeMethods.RECONVERTSTRING));
			rECONVERTSTRING.dwCompStrLen = text.Length;
			rECONVERTSTRING.dwCompStrOffset = offsetStart * Marshal.SizeOf(typeof(short));
			rECONVERTSTRING.dwTargetStrLen = text.Length;
			rECONVERTSTRING.dwTargetStrOffset = offsetStart * Marshal.SizeOf(typeof(short));
			if (!fDocFeed)
			{
				_reconv = rECONVERTSTRING;
				_isReconvReady = true;
			}
			Marshal.StructureToPtr(rECONVERTSTRING, lParam, fDeleteOld: true);
			StoreSurroundingText(lParam, surroundingText);
		}
		handled = true;
		return result;
	}

	private unsafe static void StoreSurroundingText(nint reconv, string surrounding)
	{
		byte* ptr = (byte*)((IntPtr)reconv).ToPointer();
		ptr += Marshal.SizeOf(typeof(MS.Win32.NativeMethods.RECONVERTSTRING));
		Marshal.Copy(surrounding.ToCharArray(), 0, new IntPtr(ptr), surrounding.Length);
	}

	private static string GetSurroundingText(ITextRange range, out int offsetStart)
	{
		string text = "";
		ITextPointer textPointer = range.Start.CreatePointer();
		bool flag = false;
		int num = 32;
		while (!flag && num > 0)
		{
			switch (textPointer.GetPointerContext(LogicalDirection.Backward))
			{
			case TextPointerContext.Text:
			{
				char[] array = new char[num];
				int textInRun = textPointer.GetTextInRun(LogicalDirection.Backward, array, 0, array.Length);
				Invariant.Assert(textInRun != 0);
				textPointer.MoveByOffset(-textInRun);
				num -= textInRun;
				text = text.Insert(0, new string(array, 0, textInRun));
				break;
			}
			case TextPointerContext.EmbeddedElement:
				flag = true;
				break;
			case TextPointerContext.ElementStart:
			case TextPointerContext.ElementEnd:
				if (!textPointer.GetElementType(LogicalDirection.Backward).IsSubclassOf(typeof(Inline)))
				{
					flag = true;
				}
				textPointer.MoveToNextContextPosition(LogicalDirection.Backward);
				break;
			case TextPointerContext.None:
				flag = true;
				break;
			default:
				textPointer.MoveToNextContextPosition(LogicalDirection.Backward);
				break;
			}
		}
		offsetStart = text.Length;
		text += range.Text;
		textPointer = range.End.CreatePointer();
		flag = false;
		num = 32;
		while (!flag && num > 0)
		{
			switch (textPointer.GetPointerContext(LogicalDirection.Forward))
			{
			case TextPointerContext.Text:
			{
				char[] array2 = new char[num];
				int textInRun2 = textPointer.GetTextInRun(LogicalDirection.Forward, array2, 0, array2.Length);
				textPointer.MoveByOffset(textInRun2);
				num -= textInRun2;
				text += new string(array2, 0, textInRun2);
				break;
			}
			case TextPointerContext.EmbeddedElement:
				flag = true;
				break;
			case TextPointerContext.ElementStart:
			case TextPointerContext.ElementEnd:
				if (!textPointer.GetElementType(LogicalDirection.Forward).IsSubclassOf(typeof(Inline)))
				{
					flag = true;
				}
				textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
				break;
			case TextPointerContext.None:
				flag = true;
				break;
			default:
				textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
				break;
			}
		}
		return text;
	}

	private nint OnWmImeRequest_ConfirmReconvertString(nint lParam, ref bool handled)
	{
		if (!IsInKeyboardFocus)
		{
			return IntPtr.Zero;
		}
		if (!_isReconvReady)
		{
			return IntPtr.Zero;
		}
		MS.Win32.NativeMethods.RECONVERTSTRING rECONVERTSTRING = Marshal.PtrToStructure<MS.Win32.NativeMethods.RECONVERTSTRING>(lParam);
		if (_reconv.dwStrLen != rECONVERTSTRING.dwStrLen)
		{
			handled = true;
			return IntPtr.Zero;
		}
		if (_reconv.dwCompStrLen != rECONVERTSTRING.dwCompStrLen || _reconv.dwCompStrOffset != rECONVERTSTRING.dwCompStrOffset)
		{
			ITextPointer position = _editor.Selection.Start.CreatePointer(LogicalDirection.Backward);
			position = MoveToNextCharPos(position, (rECONVERTSTRING.dwCompStrOffset - _reconv.dwCompStrOffset) / Marshal.SizeOf(typeof(short)));
			ITextPointer position2 = position.CreatePointer(LogicalDirection.Forward);
			position2 = MoveToNextCharPos(position2, rECONVERTSTRING.dwCompStrLen);
			_editor.Selection.Select(position, position2);
		}
		_isReconvReady = false;
		handled = true;
		return new IntPtr(1);
	}

	private static ITextPointer MoveToNextCharPos(ITextPointer position, int offset)
	{
		bool flag = false;
		if (offset < 0)
		{
			while (offset < 0 && !flag)
			{
				switch (position.GetPointerContext(LogicalDirection.Backward))
				{
				case TextPointerContext.Text:
					offset++;
					break;
				case TextPointerContext.None:
					flag = true;
					break;
				}
				position.MoveByOffset(-1);
			}
		}
		else if (offset > 0)
		{
			while (offset > 0 && !flag)
			{
				switch (position.GetPointerContext(LogicalDirection.Forward))
				{
				case TextPointerContext.Text:
					offset--;
					break;
				case TextPointerContext.None:
					flag = true;
					break;
				}
				position.MoveByOffset(1);
			}
		}
		return position;
	}

	private bool IsReadingWindowIme()
	{
		int num = MS.Win32.UnsafeNativeMethods.ImmGetProperty(new HandleRef(this, SafeNativeMethods.GetKeyboardLayout(0)), 4);
		if ((num & 0x10000) != 0)
		{
			return (num & 0x20000) != 0;
		}
		return true;
	}

	private void OnMouseButtonEvent(object sender, MouseButtonEventArgs e)
	{
		e.Handled = InternalMouseEventHandler();
	}

	private void OnMouseEvent(object sender, MouseEventArgs e)
	{
		e.Handled = InternalMouseEventHandler();
	}

	private bool InternalMouseEventHandler()
	{
		int num = 0;
		if (Mouse.LeftButton == MouseButtonState.Pressed)
		{
			num = 1;
		}
		if (Mouse.RightButton == MouseButtonState.Pressed)
		{
			num = 2;
		}
		Point position = Mouse.GetPosition(RenderScope);
		ITextView textView = TextEditor.GetTextView(RenderScope);
		if (!textView.Validate(position))
		{
			return false;
		}
		ITextPointer textPositionFromPoint = textView.GetTextPositionFromPoint(position, snapToText: false);
		if (textPositionFromPoint == null)
		{
			return false;
		}
		Rect rectangleFromTextPosition = textView.GetRectangleFromTextPosition(textPositionFromPoint);
		ITextPointer textPointer = textPositionFromPoint.CreatePointer();
		if (textPointer == null)
		{
			return false;
		}
		if (position.X - rectangleFromTextPosition.Left >= 0.0)
		{
			textPointer.MoveToNextInsertionPosition(LogicalDirection.Forward);
		}
		else
		{
			textPointer.MoveToNextInsertionPosition(LogicalDirection.Backward);
		}
		Rect rectangleFromTextPosition2 = textView.GetRectangleFromTextPosition(textPointer);
		int offsetToPosition = _editor.TextContainer.Start.GetOffsetToPosition(textPositionFromPoint);
		int offsetToPosition2 = _editor.TextContainer.Start.GetOffsetToPosition(_startComposition);
		int offsetToPosition3 = _editor.TextContainer.Start.GetOffsetToPosition(_endComposition);
		if (offsetToPosition < offsetToPosition2)
		{
			return false;
		}
		if (offsetToPosition > offsetToPosition3)
		{
			return false;
		}
		int num2 = ((rectangleFromTextPosition2.Left != rectangleFromTextPosition.Left) ? ((position.X - rectangleFromTextPosition.Left >= 0.0) ? ((!((position.X - rectangleFromTextPosition.Left) * 4.0 / (rectangleFromTextPosition2.Left - rectangleFromTextPosition.Left) <= 1.0)) ? 3 : 2) : ((!((position.X - rectangleFromTextPosition2.Left) * 4.0 / (rectangleFromTextPosition.Left - rectangleFromTextPosition2.Left) <= 3.0)) ? 1 : 0)) : 0);
		if (offsetToPosition == offsetToPosition2 && num2 <= 1)
		{
			return false;
		}
		if (offsetToPosition == offsetToPosition3 && num2 >= 2)
		{
			return false;
		}
		offsetToPosition -= offsetToPosition2;
		int value = (offsetToPosition << 16) + (num2 << 8) + num;
		nint zero = IntPtr.Zero;
		zero = ((IWin32Window)_source).Handle;
		nint num3 = MS.Win32.UnsafeNativeMethods.ImmGetContext(new HandleRef(this, zero));
		nint num4 = IntPtr.Zero;
		if (num3 != IntPtr.Zero)
		{
			_ = IntPtr.Zero;
			num4 = MS.Win32.UnsafeNativeMethods.SendMessage(MS.Win32.UnsafeNativeMethods.ImmGetDefaultIMEWnd(new HandleRef(this, zero)), s_MsImeMouseMessage, new IntPtr(value), num3);
		}
		return num4 != IntPtr.Zero;
	}

	private void OpenCompositionUndoUnit()
	{
		UndoManager undoManager = UndoManager.GetUndoManager(_editor.TextContainer.Parent);
		if (undoManager != null && undoManager.IsEnabled && undoManager.OpenedUnit == null)
		{
			if (_compositionUndoUnit != null && _compositionUndoUnit == undoManager.LastUnit && !_compositionUndoUnit.Locked)
			{
				undoManager.Reopen(_compositionUndoUnit);
				return;
			}
			_compositionUndoUnit = new TextParentUndoUnit(_editor.Selection);
			undoManager.Open(_compositionUndoUnit);
		}
		else
		{
			_compositionUndoUnit = null;
		}
	}

	private void CloseCompositionUndoUnit(UndoCloseAction undoCloseAction, ITextPointer compositionEnd)
	{
		UndoManager undoManager = UndoManager.GetUndoManager(_editor.TextContainer.Parent);
		if (undoManager != null && undoManager.IsEnabled && undoManager.OpenedUnit != null)
		{
			if (_compositionUndoUnit != null)
			{
				if (undoCloseAction == UndoCloseAction.Commit)
				{
					_compositionUndoUnit.RecordRedoSelectionState(compositionEnd, compositionEnd);
				}
				undoManager.Close(_compositionUndoUnit, undoCloseAction);
			}
		}
		else
		{
			_compositionUndoUnit = null;
		}
	}

	private int ConvertToInt32(double value)
	{
		if (double.IsNaN(value))
		{
			return int.MinValue;
		}
		if (value < -2147483648.0)
		{
			return int.MinValue;
		}
		if (value > 2147483647.0)
		{
			return int.MaxValue;
		}
		return Convert.ToInt32(value);
	}

	private void OnTextContainerChange(object sender, TextContainerChangeEventArgs args)
	{
		if (args.IMECharCount > 0 && (args.TextChange == TextChangeType.ContentAdded || args.TextChange == TextChangeType.ContentRemoved))
		{
			_compositionModifiedByEventListener = true;
		}
	}
}
