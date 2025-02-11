using MS.Internal;

namespace System.Windows;

/// <summary>Provides a <see cref="T:System.Windows.WeakEventManager" /> implementation so that you can use the "weak event listener" pattern to attach listeners for the <see cref="E:System.Windows.UIElement.LostFocus" /> or <see cref="E:System.Windows.ContentElement.LostFocus" /> events.</summary>
public class LostFocusEventManager : WeakEventManager
{
	private static LostFocusEventManager CurrentManager
	{
		get
		{
			Type typeFromHandle = typeof(LostFocusEventManager);
			LostFocusEventManager lostFocusEventManager = (LostFocusEventManager)WeakEventManager.GetCurrentManager(typeFromHandle);
			if (lostFocusEventManager == null)
			{
				lostFocusEventManager = new LostFocusEventManager();
				WeakEventManager.SetCurrentManager(typeFromHandle, lostFocusEventManager);
			}
			return lostFocusEventManager;
		}
	}

	private LostFocusEventManager()
	{
	}

	/// <summary>Adds the provided listener to the list of listeners on the provided source.</summary>
	/// <param name="source">The object with the event.</param>
	/// <param name="listener">The object to add as a listener.</param>
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

	/// <summary>Removes the specified listener from the list of listeners on the provided source.</summary>
	/// <param name="source">The object to remove the listener from.</param>
	/// <param name="listener">The listener to remove.</param>
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

	/// <summary>Adds the specified event handler, which is called when specified source raises the <see cref="E:System.Windows.UIElement.LostFocus" /> or <see cref="E:System.Windows.ContentElement.LostFocus" /> event.</summary>
	/// <param name="source">The source object that the raises the <see cref="E:System.Windows.UIElement.LostFocus" /> or <see cref="E:System.Windows.ContentElement.LostFocus" /> event. </param>
	/// <param name="handler">The delegate that handles the <see cref="E:System.Windows.UIElement.LostFocus" /> or <see cref="E:System.Windows.ContentElement.LostFocus" /> event.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="handler" /> is null.</exception>
	public static void AddHandler(DependencyObject source, EventHandler<RoutedEventArgs> handler)
	{
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		CurrentManager.ProtectedAddHandler(source, handler);
	}

	/// <summary>Removes the specified event handler from the specified source.</summary>
	/// <param name="source">The source object that the raises the <see cref="E:System.Windows.UIElement.LostFocus" /> or <see cref="E:System.Windows.ContentElement.LostFocus" /> event.</param>
	/// <param name="handler">The delegate that handles the <see cref="E:System.Windows.UIElement.LostFocus" /> or <see cref="E:System.Windows.ContentElement.LostFocus" /> event.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="handler" /> is null.</exception>
	public static void RemoveHandler(DependencyObject source, EventHandler<RoutedEventArgs> handler)
	{
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		CurrentManager.ProtectedRemoveHandler(source, handler);
	}

	/// <summary>Returns a new object to contain listeners to the <see cref="E:System.Windows.UIElement.LostFocus" /> or <see cref="E:System.Windows.ContentElement.LostFocus" /> event.</summary>
	/// <returns>A new object to contain listeners to the <see cref="E:System.Windows.UIElement.LostFocus" /> or <see cref="E:System.Windows.ContentElement.LostFocus" /> event.</returns>
	protected override ListenerList NewListenerList()
	{
		return new ListenerList<RoutedEventArgs>();
	}

	/// <summary>Begins listening for the <see cref="E:System.Windows.UIElement.LostFocus" /> event on the given source, attaching an internal class handler to that source.</summary>
	/// <param name="source">The object on which to start listening for the pertinent <see cref="E:System.Windows.UIElement.LostFocus" /> event.</param>
	protected override void StartListening(object source)
	{
		Helper.DowncastToFEorFCE((DependencyObject)source, out var fe, out var fce, throwIfNeither: true);
		if (fe != null)
		{
			fe.LostFocus += OnLostFocus;
		}
		else if (fce != null)
		{
			fce.LostFocus += OnLostFocus;
		}
	}

	/// <summary>Stops listening for the <see cref="E:System.Windows.UIElement.LostFocus" /> event on the given source.</summary>
	/// <param name="source">The source object on which to stop listening for <see cref="E:System.Windows.UIElement.LostFocus" />.</param>
	protected override void StopListening(object source)
	{
		Helper.DowncastToFEorFCE((DependencyObject)source, out var fe, out var fce, throwIfNeither: true);
		if (fe != null)
		{
			fe.LostFocus -= OnLostFocus;
		}
		else if (fce != null)
		{
			fce.LostFocus -= OnLostFocus;
		}
	}

	private void OnLostFocus(object sender, RoutedEventArgs args)
	{
		DeliverEvent(sender, args);
	}
}
