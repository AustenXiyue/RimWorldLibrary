namespace System.Windows.Data;

/// <summary>Provides a <see cref="T:System.Windows.WeakEventManager" /> implementation so that you can use the "weak event listener" pattern to attach listeners for the <see cref="E:System.Windows.Data.DataSourceProvider.DataChanged" /> event.</summary>
public class DataChangedEventManager : WeakEventManager
{
	private static DataChangedEventManager CurrentManager
	{
		get
		{
			Type typeFromHandle = typeof(DataChangedEventManager);
			DataChangedEventManager dataChangedEventManager = (DataChangedEventManager)WeakEventManager.GetCurrentManager(typeFromHandle);
			if (dataChangedEventManager == null)
			{
				dataChangedEventManager = new DataChangedEventManager();
				WeakEventManager.SetCurrentManager(typeFromHandle, dataChangedEventManager);
			}
			return dataChangedEventManager;
		}
	}

	private DataChangedEventManager()
	{
	}

	/// <summary>Adds the specified listener to the <see cref="E:System.Windows.Data.DataSourceProvider.DataChanged" /> event of the specified source.</summary>
	/// <param name="source">The object with the event.</param>
	/// <param name="listener">The object to add as a listener.</param>
	public static void AddListener(DataSourceProvider source, IWeakEventListener listener)
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

	/// <summary>Removes the specified listener from the <see cref="E:System.Windows.Data.DataSourceProvider.DataChanged" /> event of the specified source.</summary>
	/// <param name="source">The object with the event.</param>
	/// <param name="listener">The listener to remove.</param>
	public static void RemoveListener(DataSourceProvider source, IWeakEventListener listener)
	{
		if (listener == null)
		{
			throw new ArgumentNullException("listener");
		}
		CurrentManager.ProtectedRemoveListener(source, listener);
	}

	/// <summary>Adds the specified event handler, which is called when specified source raises the <see cref="E:System.ComponentModel.ICollectionView.CurrentChanging" /> event.</summary>
	/// <param name="source">The source object that the raises the <see cref="E:System.Windows.Data.DataSourceProvider.DataChanged" /> event.</param>
	/// <param name="handler">The delegate that handles the <see cref="E:System.Windows.Data.DataSourceProvider.DataChanged" /> event.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="handler" /> is null.</exception>
	public static void AddHandler(DataSourceProvider source, EventHandler<EventArgs> handler)
	{
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		CurrentManager.ProtectedAddHandler(source, handler);
	}

	/// <summary>Removes the specified event handler from the specified source.</summary>
	/// <param name="source">The source object that the raises the <see cref="E:System.Windows.Data.DataSourceProvider.DataChanged" /> event.</param>
	/// <param name="handler">The delegate that handles the <see cref="E:System.Windows.Data.DataSourceProvider.DataChanged" /> event.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="handler" /> is null.</exception>
	public static void RemoveHandler(DataSourceProvider source, EventHandler<EventArgs> handler)
	{
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		CurrentManager.ProtectedRemoveHandler(source, handler);
	}

	/// <summary>Returns a new object to contain listeners to the <see cref="E:System.Windows.Data.DataSourceProvider.DataChanged" /> event.</summary>
	/// <returns>A new object to contain listeners to the <see cref="E:System.Windows.Data.DataSourceProvider.DataChanged" /> event.</returns>
	protected override ListenerList NewListenerList()
	{
		return new ListenerList<EventArgs>();
	}

	/// <summary>Begins listening for the <see cref="E:System.Windows.Data.DataSourceProvider.DataChanged" /> event on the specified source.</summary>
	/// <param name="source">The object with the event.</param>
	protected override void StartListening(object source)
	{
		((DataSourceProvider)source).DataChanged += OnDataChanged;
	}

	/// <summary>Stops listening for the <see cref="E:System.Windows.Data.DataSourceProvider.DataChanged" /> event on the specified source.</summary>
	/// <param name="source">The source object to stop listening for.</param>
	protected override void StopListening(object source)
	{
		((DataSourceProvider)source).DataChanged -= OnDataChanged;
	}

	private void OnDataChanged(object sender, EventArgs args)
	{
		DeliverEvent(sender, args);
	}
}
