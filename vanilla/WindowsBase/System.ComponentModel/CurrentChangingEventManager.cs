using System.Windows;

namespace System.ComponentModel;

/// <summary>Provides a <see cref="T:System.Windows.WeakEventManager" /> implementation so that you can use the "weak event listener" pattern to attach listeners for the <see cref="E:System.ComponentModel.ICollectionView.CurrentChanging" /> event.</summary>
public class CurrentChangingEventManager : WeakEventManager
{
	private static CurrentChangingEventManager CurrentManager
	{
		get
		{
			Type typeFromHandle = typeof(CurrentChangingEventManager);
			CurrentChangingEventManager currentChangingEventManager = (CurrentChangingEventManager)WeakEventManager.GetCurrentManager(typeFromHandle);
			if (currentChangingEventManager == null)
			{
				currentChangingEventManager = new CurrentChangingEventManager();
				WeakEventManager.SetCurrentManager(typeFromHandle, currentChangingEventManager);
			}
			return currentChangingEventManager;
		}
	}

	private CurrentChangingEventManager()
	{
	}

	/// <summary>Adds the specified listener to the <see cref="E:System.ComponentModel.ICollectionView.CurrentChanging" /> event of the specified source.</summary>
	/// <param name="source">The object with the event.</param>
	/// <param name="listener">The object to add as a listener.</param>
	public static void AddListener(ICollectionView source, IWeakEventListener listener)
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

	/// <summary>Removes the specified listener from the <see cref="E:System.ComponentModel.ICollectionView.CurrentChanging" /> event of the specified source.</summary>
	/// <param name="source">The object with the event.</param>
	/// <param name="listener">The listener to remove.</param>
	public static void RemoveListener(ICollectionView source, IWeakEventListener listener)
	{
		if (listener == null)
		{
			throw new ArgumentNullException("listener");
		}
		CurrentManager.ProtectedRemoveListener(source, listener);
	}

	/// <summary>Adds the specified event handler, which is called when specified source raises the <see cref="E:System.ComponentModel.ICollectionView.CurrentChanging" /> event.</summary>
	/// <param name="source">The source object that the raises the <see cref="E:System.ComponentModel.ICollectionView.CurrentChanging" /> event.</param>
	/// <param name="handler">The delegate that handles the <see cref="E:System.ComponentModel.ICollectionView.CurrentChanging" /> event.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="handler" /> is null.</exception>
	public static void AddHandler(ICollectionView source, EventHandler<CurrentChangingEventArgs> handler)
	{
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		CurrentManager.ProtectedAddHandler(source, handler);
	}

	/// <summary>Removes the specified event handler from the specified source.</summary>
	/// <param name="source">The source object that the raises the <see cref="E:System.ComponentModel.ICollectionView.CurrentChanging" /> event.</param>
	/// <param name="handler">The delegate that handles the <see cref="E:System.ComponentModel.ICollectionView.CurrentChanging" /> event.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="handler" /> is null.</exception>
	public static void RemoveHandler(ICollectionView source, EventHandler<CurrentChangingEventArgs> handler)
	{
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		CurrentManager.ProtectedRemoveHandler(source, handler);
	}

	/// <summary>Returns a new object to contain listeners to the <see cref="E:System.ComponentModel.ICollectionView.CurrentChanging" /> event.</summary>
	/// <returns>A new object to contain listeners to the <see cref="E:System.ComponentModel.ICollectionView.CurrentChanging" /> event.</returns>
	protected override ListenerList NewListenerList()
	{
		return new ListenerList<CurrentChangingEventArgs>();
	}

	/// <summary>Begins listening for the <see cref="E:System.ComponentModel.ICollectionView.CurrentChanging" /> event on the specified source.</summary>
	/// <param name="source">The object with the event.</param>
	protected override void StartListening(object source)
	{
		((ICollectionView)source).CurrentChanging += OnCurrentChanging;
	}

	/// <summary>Stops listening for the <see cref="E:System.ComponentModel.ICollectionView.CurrentChanging" /> event on the specified source.</summary>
	/// <param name="source">The object with the event.</param>
	protected override void StopListening(object source)
	{
		((ICollectionView)source).CurrentChanging -= OnCurrentChanging;
	}

	private void OnCurrentChanging(object sender, CurrentChangingEventArgs args)
	{
		DeliverEvent(sender, args);
	}
}
