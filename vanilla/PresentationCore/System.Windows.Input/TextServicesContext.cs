using System.Runtime.InteropServices;
using System.Windows.Threading;
using MS.Internal;
using MS.Win32;

namespace System.Windows.Input;

internal class TextServicesContext
{
	internal enum KeyOp
	{
		TestUp,
		TestDown,
		Up,
		Down
	}

	private sealed class TextServicesContextShutDownListener : ShutDownListener
	{
		public TextServicesContextShutDownListener(TextServicesContext target, ShutDownEvents events)
			: base(target, events)
		{
		}

		internal override void OnShutDown(object target, object sender, EventArgs e)
		{
			((TextServicesContext)target).Uninitialize(!(sender is Dispatcher));
		}
	}

	private DefaultTextStore _defaultTextStore;

	private bool _istimactivated;

	private SecurityCriticalDataClass<MS.Win32.UnsafeNativeMethods.ITfThreadMgr> _threadManager;

	private SecurityCriticalData<int> _clientId;

	private SecurityCriticalDataClass<MS.Win32.UnsafeNativeMethods.ITfDocumentMgr> _dimEmpty;

	internal static TextServicesContext DispatcherCurrent
	{
		get
		{
			if (InputMethod.Current.TextServicesContext == null)
			{
				InputMethod.Current.TextServicesContext = new TextServicesContext();
			}
			return InputMethod.Current.TextServicesContext;
		}
	}

	internal MS.Win32.UnsafeNativeMethods.ITfThreadMgr ThreadManager
	{
		get
		{
			if (_threadManager == null)
			{
				_threadManager = new SecurityCriticalDataClass<MS.Win32.UnsafeNativeMethods.ITfThreadMgr>(TextServicesLoader.Load());
			}
			return _threadManager.Value;
		}
	}

	private MS.Win32.UnsafeNativeMethods.ITfDocumentMgr EmptyDocumentManager
	{
		get
		{
			if (_dimEmpty == null)
			{
				MS.Win32.UnsafeNativeMethods.ITfThreadMgr threadManager = ThreadManager;
				if (threadManager == null)
				{
					return null;
				}
				threadManager.CreateDocumentMgr(out var docMgr);
				_dimEmpty = new SecurityCriticalDataClass<MS.Win32.UnsafeNativeMethods.ITfDocumentMgr>(docMgr);
			}
			return _dimEmpty.Value;
		}
	}

	private TextServicesContext()
	{
		new TextServicesContextShutDownListener(this, ShutDownEvents.DomainUnload | ShutDownEvents.DispatcherShutdown);
	}

	internal void Uninitialize(bool appDomainShutdown)
	{
		if (_defaultTextStore != null)
		{
			StopTransitoryExtension();
			if (_defaultTextStore.DocumentManager != null)
			{
				_defaultTextStore.DocumentManager.Pop(MS.Win32.UnsafeNativeMethods.PopFlags.TF_POPF_ALL);
				Marshal.ReleaseComObject(_defaultTextStore.DocumentManager);
				_defaultTextStore.DocumentManager = null;
			}
			if (!appDomainShutdown)
			{
				InputMethod.Current.DefaultTextStore = null;
			}
			_defaultTextStore = null;
		}
		if (_istimactivated)
		{
			if (!appDomainShutdown || Environment.OSVersion.Version.Major >= 6)
			{
				_threadManager.Value.Deactivate();
			}
			_istimactivated = false;
		}
		if (_dimEmpty != null)
		{
			if (_dimEmpty.Value != null)
			{
				Marshal.ReleaseComObject(_dimEmpty.Value);
			}
			_dimEmpty = null;
		}
		if (_threadManager != null)
		{
			if (_threadManager.Value != null)
			{
				Marshal.ReleaseComObject(_threadManager.Value);
			}
			_threadManager = null;
		}
	}

	internal bool Keystroke(int wParam, int lParam, KeyOp op)
	{
		if (_threadManager == null || _threadManager.Value == null)
		{
			return false;
		}
		MS.Win32.UnsafeNativeMethods.ITfKeystrokeMgr tfKeystrokeMgr = _threadManager.Value as MS.Win32.UnsafeNativeMethods.ITfKeystrokeMgr;
		bool eaten;
		switch (op)
		{
		case KeyOp.TestUp:
			tfKeystrokeMgr.TestKeyUp(wParam, lParam, out eaten);
			break;
		case KeyOp.TestDown:
			tfKeystrokeMgr.TestKeyDown(wParam, lParam, out eaten);
			break;
		case KeyOp.Up:
			tfKeystrokeMgr.KeyUp(wParam, lParam, out eaten);
			break;
		case KeyOp.Down:
			tfKeystrokeMgr.KeyDown(wParam, lParam, out eaten);
			break;
		default:
			return false;
		}
		return eaten;
	}

	internal void RegisterTextStore(DefaultTextStore defaultTextStore)
	{
		_defaultTextStore = defaultTextStore;
		MS.Win32.UnsafeNativeMethods.ITfThreadMgr threadManager = ThreadManager;
		if (threadManager != null)
		{
			int editCookie = -1;
			if (!_istimactivated)
			{
				threadManager.Activate(out var clientId);
				_clientId = new SecurityCriticalData<int>(clientId);
				_istimactivated = true;
			}
			threadManager.CreateDocumentMgr(out var docMgr);
			docMgr.CreateContext(_clientId.Value, (MS.Win32.UnsafeNativeMethods.CreateContextFlags)0, _defaultTextStore, out var context, out editCookie);
			docMgr.Push(context);
			Marshal.ReleaseComObject(context);
			_defaultTextStore.DocumentManager = docMgr;
			_defaultTextStore.EditCookie = editCookie;
			StartTransitoryExtension();
		}
	}

	internal void SetFocusOnDefaultTextStore()
	{
		SetFocusOnDim(DefaultTextStore.Current.DocumentManager);
	}

	internal void SetFocusOnEmptyDim()
	{
		SetFocusOnDim(EmptyDocumentManager);
	}

	private void SetFocusOnDim(MS.Win32.UnsafeNativeMethods.ITfDocumentMgr dim)
	{
		ThreadManager?.SetFocus(dim);
	}

	private void StartTransitoryExtension()
	{
		MS.Win32.UnsafeNativeMethods.ITfCompartmentMgr obj = _defaultTextStore.DocumentManager as MS.Win32.UnsafeNativeMethods.ITfCompartmentMgr;
		Guid guid = MS.Win32.UnsafeNativeMethods.GUID_COMPARTMENT_TRANSITORYEXTENSION;
		obj.GetCompartment(ref guid, out var comp);
		object varValue = 1;
		comp.SetValue(0, ref varValue);
		guid = MS.Win32.UnsafeNativeMethods.IID_ITfTransitoryExtensionSink;
		if (_defaultTextStore.DocumentManager is MS.Win32.UnsafeNativeMethods.ITfSource tfSource)
		{
			tfSource.AdviseSink(ref guid, _defaultTextStore, out var cookie);
			_defaultTextStore.TransitoryExtensionSinkCookie = cookie;
		}
		Marshal.ReleaseComObject(comp);
	}

	private void StopTransitoryExtension()
	{
		if (_defaultTextStore.TransitoryExtensionSinkCookie != -1)
		{
			if (_defaultTextStore.DocumentManager is MS.Win32.UnsafeNativeMethods.ITfSource tfSource)
			{
				tfSource.UnadviseSink(_defaultTextStore.TransitoryExtensionSinkCookie);
			}
			_defaultTextStore.TransitoryExtensionSinkCookie = -1;
		}
		if (_defaultTextStore.DocumentManager is MS.Win32.UnsafeNativeMethods.ITfCompartmentMgr tfCompartmentMgr)
		{
			Guid guid = MS.Win32.UnsafeNativeMethods.GUID_COMPARTMENT_TRANSITORYEXTENSION;
			tfCompartmentMgr.GetCompartment(ref guid, out var comp);
			if (comp != null)
			{
				object varValue = 0;
				comp.SetValue(0, ref varValue);
				Marshal.ReleaseComObject(comp);
			}
		}
	}
}
