using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;
using MS.Internal;
using MS.Win32;

namespace System.Windows.Documents;

internal class TextServicesHost : DispatcherObject
{
	private int _registeredtextstorecount;

	private SecurityCriticalData<int> _clientId;

	private SecurityCriticalDataClass<MS.Win32.UnsafeNativeMethods.ITfThreadMgr> _threadManager;

	private bool _isDispatcherShutdownFinished;

	private MoveSizeWinEventHandler _winEvent;

	private Thread _thread;

	internal static TextServicesHost Current
	{
		get
		{
			TextEditorThreadLocalStore threadLocalStore = TextEditor._ThreadLocalStore;
			if (threadLocalStore.TextServicesHost == null)
			{
				threadLocalStore.TextServicesHost = new TextServicesHost();
			}
			return threadLocalStore.TextServicesHost;
		}
	}

	internal MS.Win32.UnsafeNativeMethods.ITfThreadMgr ThreadManager
	{
		get
		{
			if (_threadManager == null)
			{
				return null;
			}
			return _threadManager.Value;
		}
	}

	internal TextServicesHost()
	{
	}

	internal void RegisterTextStore(TextStore textstore)
	{
		_RegisterTextStore(textstore);
		_thread = Thread.CurrentThread;
	}

	internal void UnregisterTextStore(TextStore textstore, bool finalizer)
	{
		if (!finalizer)
		{
			OnUnregisterTextStore(textstore);
		}
		else if (!_isDispatcherShutdownFinished)
		{
			base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(OnUnregisterTextStore), textstore);
		}
	}

	internal void RegisterWinEventSink(TextStore textstore)
	{
		if (_winEvent == null)
		{
			_winEvent = new MoveSizeWinEventHandler();
			_winEvent.Start();
		}
		_winEvent.RegisterTextStore(textstore);
	}

	internal void UnregisterWinEventSink(TextStore textstore)
	{
		_winEvent.UnregisterTextStore(textstore);
		if (_winEvent.TextStoreCount == 0)
		{
			_winEvent.Stop();
			_winEvent.Clear();
			_winEvent = null;
		}
	}

	internal static void StartTransitoryExtension(TextStore textstore)
	{
		MS.Win32.UnsafeNativeMethods.ITfCompartmentMgr obj = textstore.DocumentManager as MS.Win32.UnsafeNativeMethods.ITfCompartmentMgr;
		Guid guid = MS.Win32.UnsafeNativeMethods.GUID_COMPARTMENT_TRANSITORYEXTENSION;
		obj.GetCompartment(ref guid, out var comp);
		object varValue = 2;
		comp.SetValue(0, ref varValue);
		guid = MS.Win32.UnsafeNativeMethods.IID_ITfTransitoryExtensionSink;
		if (textstore.DocumentManager is MS.Win32.UnsafeNativeMethods.ITfSource tfSource)
		{
			tfSource.AdviseSink(ref guid, textstore, out var cookie);
			textstore.TransitoryExtensionSinkCookie = cookie;
		}
		Marshal.ReleaseComObject(comp);
	}

	internal static void StopTransitoryExtension(TextStore textstore)
	{
		MS.Win32.UnsafeNativeMethods.ITfCompartmentMgr obj = textstore.DocumentManager as MS.Win32.UnsafeNativeMethods.ITfCompartmentMgr;
		if (textstore.TransitoryExtensionSinkCookie != -1)
		{
			if (textstore.DocumentManager is MS.Win32.UnsafeNativeMethods.ITfSource tfSource)
			{
				tfSource.UnadviseSink(textstore.TransitoryExtensionSinkCookie);
			}
			textstore.TransitoryExtensionSinkCookie = -1;
		}
		Guid guid = MS.Win32.UnsafeNativeMethods.GUID_COMPARTMENT_TRANSITORYEXTENSION;
		obj.GetCompartment(ref guid, out var comp);
		object varValue = 0;
		comp.SetValue(0, ref varValue);
		Marshal.ReleaseComObject(comp);
	}

	private object OnUnregisterTextStore(object arg)
	{
		if (_threadManager == null || _threadManager.Value == null)
		{
			return null;
		}
		TextStore textStore = (TextStore)arg;
		if (textStore.ThreadFocusCookie != -1)
		{
			(_threadManager.Value as MS.Win32.UnsafeNativeMethods.ITfSource).UnadviseSink(textStore.ThreadFocusCookie);
			textStore.ThreadFocusCookie = -1;
		}
		textStore.DocumentManager.GetBase(out var context);
		if (context != null)
		{
			if (textStore.EditSinkCookie != -1)
			{
				(context as MS.Win32.UnsafeNativeMethods.ITfSource).UnadviseSink(textStore.EditSinkCookie);
				textStore.EditSinkCookie = -1;
			}
			Marshal.ReleaseComObject(context);
		}
		textStore.DocumentManager.Pop(MS.Win32.UnsafeNativeMethods.PopFlags.TF_POPF_ALL);
		Marshal.ReleaseComObject(textStore.DocumentManager);
		textStore.DocumentManager = null;
		_registeredtextstorecount--;
		if (_isDispatcherShutdownFinished && _registeredtextstorecount == 0)
		{
			DeactivateThreadManager();
		}
		return null;
	}

	private void OnDispatcherShutdownFinished(object sender, EventArgs args)
	{
		base.Dispatcher.ShutdownFinished -= OnDispatcherShutdownFinished;
		if (_registeredtextstorecount == 0)
		{
			DeactivateThreadManager();
		}
		_isDispatcherShutdownFinished = true;
	}

	private void _RegisterTextStore(TextStore textstore)
	{
		int editCookie = -1;
		int cookie = -1;
		int cookie2 = -1;
		if (_threadManager == null)
		{
			_threadManager = new SecurityCriticalDataClass<MS.Win32.UnsafeNativeMethods.ITfThreadMgr>(TextServicesLoader.Load());
			if (_threadManager.Value == null)
			{
				_threadManager = null;
				return;
			}
			_threadManager.Value.Activate(out var clientId);
			_clientId = new SecurityCriticalData<int>(clientId);
			base.Dispatcher.ShutdownFinished += OnDispatcherShutdownFinished;
		}
		_threadManager.Value.CreateDocumentMgr(out var docMgr);
		docMgr.CreateContext(_clientId.Value, (MS.Win32.UnsafeNativeMethods.CreateContextFlags)0, textstore, out var context, out editCookie);
		docMgr.Push(context);
		if (textstore != null)
		{
			Guid riid = MS.Win32.UnsafeNativeMethods.IID_ITfThreadFocusSink;
			(_threadManager.Value as MS.Win32.UnsafeNativeMethods.ITfSource).AdviseSink(ref riid, textstore, out cookie);
		}
		if (textstore != null)
		{
			Guid riid = MS.Win32.UnsafeNativeMethods.IID_ITfTextEditSink;
			(context as MS.Win32.UnsafeNativeMethods.ITfSource).AdviseSink(ref riid, textstore, out cookie2);
		}
		Marshal.ReleaseComObject(context);
		textstore.DocumentManager = docMgr;
		textstore.ThreadFocusCookie = cookie;
		textstore.EditSinkCookie = cookie2;
		textstore.EditCookie = editCookie;
		if (textstore.UiScope.IsKeyboardFocused)
		{
			textstore.OnGotFocus();
		}
		_registeredtextstorecount++;
	}

	private void DeactivateThreadManager()
	{
		if (_threadManager != null)
		{
			if (_threadManager.Value != null)
			{
				if (_thread == Thread.CurrentThread || Environment.OSVersion.Version.Major >= 6)
				{
					_threadManager.Value.Deactivate();
				}
				Marshal.ReleaseComObject(_threadManager.Value);
			}
			_threadManager = null;
		}
		TextEditor._ThreadLocalStore.TextServicesHost = null;
	}
}
