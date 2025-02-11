using System.Collections;

namespace System.Windows.Documents;

internal class TextEditorThreadLocalStore
{
	private int _inputLanguageChangeEventHandlerCount;

	private ArrayList _pendingInputItems;

	private bool _pureControlShift;

	private bool _bidi;

	private TextSelection _focusedTextSelection;

	private TextServicesHost _textServicesHost;

	private bool _hideCursor;

	internal int InputLanguageChangeEventHandlerCount
	{
		get
		{
			return _inputLanguageChangeEventHandlerCount;
		}
		set
		{
			_inputLanguageChangeEventHandlerCount = value;
		}
	}

	internal ArrayList PendingInputItems
	{
		get
		{
			return _pendingInputItems;
		}
		set
		{
			_pendingInputItems = value;
		}
	}

	internal bool PureControlShift
	{
		get
		{
			return _pureControlShift;
		}
		set
		{
			_pureControlShift = value;
		}
	}

	internal bool Bidi
	{
		get
		{
			return _bidi;
		}
		set
		{
			_bidi = value;
		}
	}

	internal TextSelection FocusedTextSelection
	{
		get
		{
			return _focusedTextSelection;
		}
		set
		{
			_focusedTextSelection = value;
		}
	}

	internal TextServicesHost TextServicesHost
	{
		get
		{
			return _textServicesHost;
		}
		set
		{
			_textServicesHost = value;
		}
	}

	internal bool HideCursor
	{
		get
		{
			return _hideCursor;
		}
		set
		{
			_hideCursor = value;
		}
	}

	internal TextEditorThreadLocalStore()
	{
	}
}
