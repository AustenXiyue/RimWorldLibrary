using System.Collections.Specialized;

namespace System.Windows.Controls.Primitives;

internal class InternalCollectionChangedEventManager : WeakEventManager
{
	private static InternalCollectionChangedEventManager CurrentManager
	{
		get
		{
			Type typeFromHandle = typeof(InternalCollectionChangedEventManager);
			InternalCollectionChangedEventManager internalCollectionChangedEventManager = (InternalCollectionChangedEventManager)WeakEventManager.GetCurrentManager(typeFromHandle);
			if (internalCollectionChangedEventManager == null)
			{
				internalCollectionChangedEventManager = new InternalCollectionChangedEventManager();
				WeakEventManager.SetCurrentManager(typeFromHandle, internalCollectionChangedEventManager);
			}
			return internalCollectionChangedEventManager;
		}
	}

	private InternalCollectionChangedEventManager()
	{
	}

	public static void AddListener(GridViewColumnCollection source, IWeakEventListener listener)
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

	public static void RemoveListener(GridViewColumnCollection source, IWeakEventListener listener)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (listener == null)
		{
			throw new ArgumentNullException("listener");
		}
		CurrentManager.ProtectedRemoveListener(source, listener);
	}

	public static void AddHandler(GridViewColumnCollection source, EventHandler<NotifyCollectionChangedEventArgs> handler)
	{
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		CurrentManager.ProtectedAddHandler(source, handler);
	}

	public static void RemoveHandler(GridViewColumnCollection source, EventHandler<NotifyCollectionChangedEventArgs> handler)
	{
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		CurrentManager.ProtectedRemoveHandler(source, handler);
	}

	protected override ListenerList NewListenerList()
	{
		return new ListenerList<NotifyCollectionChangedEventArgs>();
	}

	protected override void StartListening(object source)
	{
		((GridViewColumnCollection)source).InternalCollectionChanged += OnCollectionChanged;
	}

	protected override void StopListening(object source)
	{
		((GridViewColumnCollection)source).InternalCollectionChanged -= OnCollectionChanged;
	}

	private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
	{
		DeliverEvent(sender, args);
	}
}
