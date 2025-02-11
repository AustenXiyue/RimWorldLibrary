using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace MS.Internal.WindowsRuntime.Windows.UI.ViewManagement;

internal class InputPane : IDisposable
{
	private static readonly bool _isSupported;

	private static object _winRtActivationFactory;

	private InputPaneRcw.IInputPane2 _inputPane;

	private bool _disposed;

	static InputPane()
	{
		try
		{
			if (GetWinRtActivationFactory(forceInitialization: true) == null)
			{
				_isSupported = false;
			}
			_isSupported = true;
		}
		catch
		{
			_isSupported = false;
		}
	}

	private InputPane(nint? hwnd)
	{
		if (!_isSupported)
		{
			throw new PlatformNotSupportedException();
		}
		try
		{
			if (hwnd.HasValue)
			{
				InputPaneRcw.IInputPaneInterop inputPaneInterop;
				try
				{
					inputPaneInterop = GetWinRtActivationFactory() as InputPaneRcw.IInputPaneInterop;
				}
				catch (COMException)
				{
					inputPaneInterop = GetWinRtActivationFactory(forceInitialization: true) as InputPaneRcw.IInputPaneInterop;
				}
				object inputPane;
				if (inputPaneInterop == null)
				{
					inputPane = null;
				}
				else
				{
					InputPaneRcw.IInputPaneInterop inputPaneInterop2 = inputPaneInterop;
					nint value = hwnd.Value;
					Guid riid = typeof(InputPaneRcw.IInputPane2).GUID;
					inputPane = inputPaneInterop2.GetForWindow(value, ref riid);
				}
				_inputPane = (InputPaneRcw.IInputPane2)inputPane;
			}
		}
		catch (COMException)
		{
		}
		if (_inputPane == null)
		{
			throw new PlatformNotSupportedException();
		}
	}

	internal static InputPane GetForWindow(HwndSource source)
	{
		return new InputPane(source?.CriticalHandle);
	}

	internal bool TryShow()
	{
		bool result = false;
		try
		{
			result = _inputPane?.TryShow() ?? false;
		}
		catch (COMException)
		{
		}
		return result;
	}

	internal bool TryHide()
	{
		bool result = false;
		try
		{
			result = _inputPane?.TryHide() ?? false;
		}
		catch (COMException)
		{
		}
		return result;
	}

	private static object GetWinRtActivationFactory(bool forceInitialization = false)
	{
		if (forceInitialization || _winRtActivationFactory == null)
		{
			try
			{
				_winRtActivationFactory = InputPaneRcw.GetInputPaneActivationFactory();
			}
			catch (Exception ex) when (ex is TypeLoadException || ex is FileNotFoundException || ex is EntryPointNotFoundException || ex is DllNotFoundException || ex.HResult == -2147467262 || ex.HResult == -2147221164)
			{
				_winRtActivationFactory = null;
			}
		}
		return _winRtActivationFactory;
	}

	~InputPane()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (_disposed)
		{
			return;
		}
		if (_inputPane != null)
		{
			try
			{
				Marshal.ReleaseComObject(_inputPane);
			}
			catch
			{
			}
			_inputPane = null;
		}
		_disposed = true;
	}
}
