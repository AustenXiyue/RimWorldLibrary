using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

namespace System.Windows.Input.StylusPlugIns;

internal class DynamicRendererThreadManager : IWeakEventListener, IDisposable
{
	private class DynamicRendererThreadManagerWorker
	{
		private Dispatcher _dispatcher;

		private AutoResetEvent _startupCompleted;

		internal DynamicRendererThreadManagerWorker()
		{
		}

		internal Dispatcher StartUpAndReturnDispatcher()
		{
			_startupCompleted = new AutoResetEvent(initialState: false);
			Thread thread = new Thread(InkingThreadProc);
			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground = true;
			thread.Start();
			_startupCompleted.WaitOne();
			_startupCompleted.Close();
			_startupCompleted = null;
			return _dispatcher;
		}

		public void InkingThreadProc()
		{
			Thread.CurrentThread.Name = "DynamicRenderer";
			_dispatcher = Dispatcher.CurrentDispatcher;
			_startupCompleted.Set();
			Dispatcher.Run();
		}
	}

	[ThreadStatic]
	private static WeakReference _tsDRTMWeakRef;

	private volatile Dispatcher __inkingDispatcher;

	private bool _disposed;

	internal Dispatcher ThreadDispatcher => __inkingDispatcher;

	internal static DynamicRendererThreadManager GetCurrentThreadInstance()
	{
		if (_tsDRTMWeakRef == null || _tsDRTMWeakRef.Target == null)
		{
			_tsDRTMWeakRef = new WeakReference(new DynamicRendererThreadManager());
		}
		return _tsDRTMWeakRef.Target as DynamicRendererThreadManager;
	}

	private DynamicRendererThreadManager()
	{
		DynamicRendererThreadManagerWorker dynamicRendererThreadManagerWorker = new DynamicRendererThreadManagerWorker();
		__inkingDispatcher = dynamicRendererThreadManagerWorker.StartUpAndReturnDispatcher();
		DispatcherShutdownStartedEventManager.AddListener(Dispatcher.CurrentDispatcher, this);
	}

	~DynamicRendererThreadManager()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs args)
	{
		if (managerType == typeof(DispatcherShutdownStartedEventManager))
		{
			OnAppDispatcherShutdown(sender, args);
			return true;
		}
		return false;
	}

	private void OnAppDispatcherShutdown(object sender, EventArgs e)
	{
		__inkingDispatcher?.Invoke(DispatcherPriority.Send, (DispatcherOperationCallback)delegate
		{
			Dispose();
			return (object)null;
		}, null);
	}

	private void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			_disposed = true;
			if (__inkingDispatcher != null && !Environment.HasShutdownStarted)
			{
				try
				{
					__inkingDispatcher.CriticalInvokeShutdown();
				}
				catch (Win32Exception ex)
				{
					_ = ex.NativeErrorCode;
					_ = 1400;
				}
				finally
				{
					__inkingDispatcher = null;
				}
			}
		}
		GC.KeepAlive(this);
	}
}
