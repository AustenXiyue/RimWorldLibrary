using System.Runtime.InteropServices;
using System.Windows.Threading;
using MS.Internal;
using MS.Win32;

namespace System.Windows.Input;

internal class DefaultTextStore : MS.Win32.UnsafeNativeMethods.ITfContextOwner, MS.Win32.UnsafeNativeMethods.ITfContextOwnerCompositionSink, MS.Win32.UnsafeNativeMethods.ITfTransitoryExtensionSink
{
	private readonly Dispatcher _dispatcher;

	private TextComposition _composition;

	private SecurityCriticalData<MS.Win32.UnsafeNativeMethods.ITfDocumentMgr> _doc;

	private int _editCookie;

	private int _transitoryExtensionSinkCookie;

	internal static DefaultTextStore Current
	{
		get
		{
			DefaultTextStore defaultTextStore = InputMethod.Current.DefaultTextStore;
			if (defaultTextStore == null)
			{
				defaultTextStore = new DefaultTextStore(Dispatcher.CurrentDispatcher);
				InputMethod.Current.DefaultTextStore = defaultTextStore;
				defaultTextStore.Register();
			}
			return defaultTextStore;
		}
	}

	internal MS.Win32.UnsafeNativeMethods.ITfDocumentMgr DocumentManager
	{
		get
		{
			return _doc.Value;
		}
		set
		{
			_doc = new SecurityCriticalData<MS.Win32.UnsafeNativeMethods.ITfDocumentMgr>(value);
		}
	}

	internal int EditCookie
	{
		set
		{
			_editCookie = value;
		}
	}

	internal int TransitoryExtensionSinkCookie
	{
		get
		{
			return _transitoryExtensionSinkCookie;
		}
		set
		{
			_transitoryExtensionSinkCookie = value;
		}
	}

	internal MS.Win32.UnsafeNativeMethods.ITfDocumentMgr TransitoryDocumentManager
	{
		get
		{
			MS.Win32.UnsafeNativeMethods.ITfCompartmentMgr obj = (MS.Win32.UnsafeNativeMethods.ITfCompartmentMgr)DocumentManager;
			Guid guid = MS.Win32.UnsafeNativeMethods.GUID_COMPARTMENT_TRANSITORYEXTENSION_DOCUMENTMANAGER;
			obj.GetCompartment(ref guid, out var comp);
			comp.GetValue(out var varValue);
			MS.Win32.UnsafeNativeMethods.ITfDocumentMgr result = varValue as MS.Win32.UnsafeNativeMethods.ITfDocumentMgr;
			Marshal.ReleaseComObject(comp);
			return result;
		}
	}

	internal DefaultTextStore(Dispatcher dispatcher)
	{
		_dispatcher = dispatcher;
		_editCookie = -1;
		_transitoryExtensionSinkCookie = -1;
	}

	public void GetACPFromPoint(ref MS.Win32.UnsafeNativeMethods.POINT point, MS.Win32.UnsafeNativeMethods.GetPositionFromPointFlags flags, out int position)
	{
		position = 0;
	}

	public void GetTextExt(int start, int end, out MS.Win32.UnsafeNativeMethods.RECT rect, out bool clipped)
	{
		rect = default(MS.Win32.UnsafeNativeMethods.RECT);
		clipped = false;
	}

	public void GetScreenExt(out MS.Win32.UnsafeNativeMethods.RECT rect)
	{
		rect = default(MS.Win32.UnsafeNativeMethods.RECT);
	}

	public void GetStatus(out MS.Win32.UnsafeNativeMethods.TS_STATUS status)
	{
		status = default(MS.Win32.UnsafeNativeMethods.TS_STATUS);
	}

	public void GetWnd(out nint hwnd)
	{
		hwnd = IntPtr.Zero;
	}

	public void GetValue(ref Guid guidAttribute, out object varValue)
	{
		varValue = null;
	}

	public void OnStartComposition(MS.Win32.UnsafeNativeMethods.ITfCompositionView view, out bool ok)
	{
		ok = true;
	}

	public void OnUpdateComposition(MS.Win32.UnsafeNativeMethods.ITfCompositionView view, MS.Win32.UnsafeNativeMethods.ITfRange rangeNew)
	{
	}

	public void OnEndComposition(MS.Win32.UnsafeNativeMethods.ITfCompositionView view)
	{
	}

	public void OnTransitoryExtensionUpdated(MS.Win32.UnsafeNativeMethods.ITfContext context, int ecReadOnly, MS.Win32.UnsafeNativeMethods.ITfRange rangeResult, MS.Win32.UnsafeNativeMethods.ITfRange rangeComposition, out bool fDeleteResultRange)
	{
		fDeleteResultRange = true;
		_ = InputManager.Current.PrimaryKeyboardDevice.TextCompositionManager;
		if (rangeResult != null)
		{
			string text = StringFromITfRange(rangeResult, ecReadOnly);
			if (text.Length > 0)
			{
				if (_composition == null)
				{
					_composition = new DefaultTextStoreTextComposition(InputManager.Current, Keyboard.FocusedElement, text, TextCompositionAutoComplete.On);
					TextCompositionManager.StartComposition(_composition);
					_composition = null;
				}
				else
				{
					_composition.SetCompositionText("");
					_composition.SetText(text);
					TextCompositionManager.CompleteComposition(_composition);
					_composition = null;
				}
			}
		}
		if (rangeComposition == null)
		{
			return;
		}
		string text2 = StringFromITfRange(rangeComposition, ecReadOnly);
		if (text2.Length > 0)
		{
			if (_composition == null)
			{
				_composition = new DefaultTextStoreTextComposition(InputManager.Current, Keyboard.FocusedElement, "", TextCompositionAutoComplete.Off);
				_composition.SetCompositionText(text2);
				TextCompositionManager.StartComposition(_composition);
			}
			else
			{
				_composition.SetCompositionText(text2);
				_composition.SetText("");
				TextCompositionManager.UpdateComposition(_composition);
			}
		}
	}

	private string StringFromITfRange(MS.Win32.UnsafeNativeMethods.ITfRange range, int ecReadOnly)
	{
		MS.Win32.UnsafeNativeMethods.ITfRangeACP obj = (MS.Win32.UnsafeNativeMethods.ITfRangeACP)range;
		obj.GetExtent(out var _, out var count);
		char[] array = new char[count];
		obj.GetText(ecReadOnly, 0, array, count, out var _);
		return new string(array);
	}

	private void Register()
	{
		TextServicesContext.DispatcherCurrent.RegisterTextStore(this);
	}
}
