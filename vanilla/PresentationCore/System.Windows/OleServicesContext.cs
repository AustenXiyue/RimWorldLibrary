using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;
using MS.Internal.PresentationCore;
using MS.Win32;

namespace System.Windows;

internal class OleServicesContext
{
	private static readonly LocalDataStoreSlot _threadDataSlot = Thread.AllocateDataSlot();

	internal static OleServicesContext CurrentOleServicesContext
	{
		get
		{
			OleServicesContext oleServicesContext = (OleServicesContext)Thread.GetData(_threadDataSlot);
			if (oleServicesContext == null)
			{
				oleServicesContext = new OleServicesContext();
				Thread.SetData(_threadDataSlot, oleServicesContext);
			}
			return oleServicesContext;
		}
	}

	private OleServicesContext()
	{
		SetDispatcherThread();
	}

	internal int OleSetClipboard(System.Runtime.InteropServices.ComTypes.IDataObject dataObject)
	{
		if (Thread.CurrentThread.GetApartmentState() != 0)
		{
			throw new ThreadStateException(SR.OleServicesContext_ThreadMustBeSTA);
		}
		return MS.Win32.UnsafeNativeMethods.OleSetClipboard(dataObject);
	}

	internal int OleGetClipboard(ref System.Runtime.InteropServices.ComTypes.IDataObject dataObject)
	{
		if (Thread.CurrentThread.GetApartmentState() != 0)
		{
			throw new ThreadStateException(SR.OleServicesContext_ThreadMustBeSTA);
		}
		return MS.Win32.UnsafeNativeMethods.OleGetClipboard(ref dataObject);
	}

	internal int OleFlushClipboard()
	{
		if (Thread.CurrentThread.GetApartmentState() != 0)
		{
			throw new ThreadStateException(SR.OleServicesContext_ThreadMustBeSTA);
		}
		return MS.Win32.UnsafeNativeMethods.OleFlushClipboard();
	}

	internal int OleIsCurrentClipboard(System.Runtime.InteropServices.ComTypes.IDataObject dataObject)
	{
		if (Thread.CurrentThread.GetApartmentState() != 0)
		{
			throw new ThreadStateException(SR.OleServicesContext_ThreadMustBeSTA);
		}
		return MS.Win32.UnsafeNativeMethods.OleIsCurrentClipboard(dataObject);
	}

	internal void OleDoDragDrop(System.Runtime.InteropServices.ComTypes.IDataObject dataObject, MS.Win32.UnsafeNativeMethods.IOleDropSource dropSource, int allowedEffects, int[] finalEffect)
	{
		if (Thread.CurrentThread.GetApartmentState() != 0)
		{
			throw new ThreadStateException(SR.OleServicesContext_ThreadMustBeSTA);
		}
		InputManager inputManager = (InputManager)Dispatcher.CurrentDispatcher.InputManager;
		if (inputManager != null)
		{
			inputManager.InDragDrop = true;
		}
		try
		{
			MS.Win32.UnsafeNativeMethods.DoDragDrop(dataObject, dropSource, allowedEffects, finalEffect);
		}
		finally
		{
			if (inputManager != null)
			{
				inputManager.InDragDrop = false;
			}
		}
	}

	internal int OleRegisterDragDrop(HandleRef windowHandle, MS.Win32.UnsafeNativeMethods.IOleDropTarget dropTarget)
	{
		if (Thread.CurrentThread.GetApartmentState() != 0)
		{
			throw new ThreadStateException(SR.OleServicesContext_ThreadMustBeSTA);
		}
		return MS.Win32.UnsafeNativeMethods.RegisterDragDrop(windowHandle, dropTarget);
	}

	internal int OleRevokeDragDrop(HandleRef windowHandle)
	{
		if (Thread.CurrentThread.GetApartmentState() != 0)
		{
			throw new ThreadStateException(SR.OleServicesContext_ThreadMustBeSTA);
		}
		return MS.Win32.UnsafeNativeMethods.RevokeDragDrop(windowHandle);
	}

	private void SetDispatcherThread()
	{
		if (Thread.CurrentThread.GetApartmentState() != 0)
		{
			throw new ThreadStateException(SR.OleServicesContext_ThreadMustBeSTA);
		}
		int num = OleInitialize();
		if (!MS.Win32.NativeMethods.Succeeded(num))
		{
			throw new SystemException(SR.Format(SR.OleServicesContext_oleInitializeFailure, num));
		}
		Dispatcher.CurrentDispatcher.ShutdownFinished += OnDispatcherShutdown;
	}

	private void OnDispatcherShutdown(object sender, EventArgs args)
	{
		if (Thread.CurrentThread.GetApartmentState() != 0)
		{
			throw new ThreadStateException(SR.OleServicesContext_ThreadMustBeSTA);
		}
		OleUninitialize();
	}

	private int OleInitialize()
	{
		return MS.Win32.UnsafeNativeMethods.OleInitialize();
	}

	private int OleUninitialize()
	{
		return MS.Win32.UnsafeNativeMethods.OleUninitialize();
	}
}
