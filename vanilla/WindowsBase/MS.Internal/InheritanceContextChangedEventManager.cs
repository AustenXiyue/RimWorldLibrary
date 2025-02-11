using System;
using System.Windows;
using MS.Internal.WindowsBase;

namespace MS.Internal;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal class InheritanceContextChangedEventManager : WeakEventManager
{
	private static InheritanceContextChangedEventManager CurrentManager
	{
		get
		{
			Type typeFromHandle = typeof(InheritanceContextChangedEventManager);
			InheritanceContextChangedEventManager inheritanceContextChangedEventManager = (InheritanceContextChangedEventManager)WeakEventManager.GetCurrentManager(typeFromHandle);
			if (inheritanceContextChangedEventManager == null)
			{
				inheritanceContextChangedEventManager = new InheritanceContextChangedEventManager();
				WeakEventManager.SetCurrentManager(typeFromHandle, inheritanceContextChangedEventManager);
			}
			return inheritanceContextChangedEventManager;
		}
	}

	private InheritanceContextChangedEventManager()
	{
	}

	public static void AddListener(DependencyObject source, IWeakEventListener listener)
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

	public static void RemoveListener(DependencyObject source, IWeakEventListener listener)
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

	public static void AddHandler(DependencyObject source, EventHandler<EventArgs> handler)
	{
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		CurrentManager.ProtectedAddHandler(source, handler);
	}

	public static void RemoveHandler(DependencyObject source, EventHandler<EventArgs> handler)
	{
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		CurrentManager.ProtectedRemoveHandler(source, handler);
	}

	protected override ListenerList NewListenerList()
	{
		return new ListenerList<EventArgs>();
	}

	protected override void StartListening(object source)
	{
		((DependencyObject)source).InheritanceContextChanged += OnInheritanceContextChanged;
	}

	protected override void StopListening(object source)
	{
		((DependencyObject)source).InheritanceContextChanged -= OnInheritanceContextChanged;
	}

	private void OnInheritanceContextChanged(object sender, EventArgs args)
	{
		DeliverEvent(sender, args);
	}
}
