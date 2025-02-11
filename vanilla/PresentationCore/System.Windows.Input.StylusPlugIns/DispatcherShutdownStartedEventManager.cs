using System.Windows.Threading;

namespace System.Windows.Input.StylusPlugIns;

internal sealed class DispatcherShutdownStartedEventManager : WeakEventManager
{
	private static DispatcherShutdownStartedEventManager CurrentManager
	{
		get
		{
			Type typeFromHandle = typeof(DispatcherShutdownStartedEventManager);
			DispatcherShutdownStartedEventManager dispatcherShutdownStartedEventManager = (DispatcherShutdownStartedEventManager)WeakEventManager.GetCurrentManager(typeFromHandle);
			if (dispatcherShutdownStartedEventManager == null)
			{
				dispatcherShutdownStartedEventManager = new DispatcherShutdownStartedEventManager();
				WeakEventManager.SetCurrentManager(typeFromHandle, dispatcherShutdownStartedEventManager);
			}
			return dispatcherShutdownStartedEventManager;
		}
	}

	private DispatcherShutdownStartedEventManager()
	{
	}

	public static void AddListener(Dispatcher source, IWeakEventListener listener)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (listener == null)
		{
			throw new ArgumentNullException("listener");
		}
		CurrentManager.ProtectedAddListener(source, listener);
	}

	protected override void StartListening(object source)
	{
		((Dispatcher)source).ShutdownStarted += OnShutdownStarted;
	}

	protected override void StopListening(object source)
	{
		((Dispatcher)source).ShutdownStarted -= OnShutdownStarted;
	}

	private void OnShutdownStarted(object sender, EventArgs args)
	{
		DeliverEvent(sender, args);
	}
}
