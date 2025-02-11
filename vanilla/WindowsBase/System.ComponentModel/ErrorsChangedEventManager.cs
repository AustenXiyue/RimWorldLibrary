using System.Windows;

namespace System.ComponentModel;

/// <summary>Provides a <see cref="T:System.Windows.WeakEventManager" /> implementation so that you can use the weak event listener pattern to attach listeners for the <see cref="E:System.ComponentModel.INotifyDataErrorInfo.ErrorsChanged" /> event.</summary>
public class ErrorsChangedEventManager : WeakEventManager
{
	private static ErrorsChangedEventManager CurrentManager
	{
		get
		{
			Type typeFromHandle = typeof(ErrorsChangedEventManager);
			ErrorsChangedEventManager errorsChangedEventManager = (ErrorsChangedEventManager)WeakEventManager.GetCurrentManager(typeFromHandle);
			if (errorsChangedEventManager == null)
			{
				errorsChangedEventManager = new ErrorsChangedEventManager();
				WeakEventManager.SetCurrentManager(typeFromHandle, errorsChangedEventManager);
			}
			return errorsChangedEventManager;
		}
	}

	private ErrorsChangedEventManager()
	{
	}

	/// <summary>Adds the specified event handler, which is called when specified source raises the <see cref="E:System.ComponentModel.INotifyDataErrorInfo.ErrorsChanged" /> event.</summary>
	/// <param name="source">The source object that raises the <see cref="E:System.ComponentModel.INotifyDataErrorInfo.ErrorsChanged" /> event.</param>
	/// <param name="handler">The delegate that handles the <see cref="E:System.ComponentModel.INotifyDataErrorInfo.ErrorsChanged" /> event.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="handler" /> is null.</exception>
	public static void AddHandler(INotifyDataErrorInfo source, EventHandler<DataErrorsChangedEventArgs> handler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		CurrentManager.ProtectedAddHandler(source, handler);
	}

	/// <summary>Removes the specified event handler from the specified source. </summary>
	/// <param name="source">The source object that raises the <see cref="E:System.ComponentModel.INotifyDataErrorInfo.ErrorsChanged" /> event.</param>
	/// <param name="handler">The delegate that handles the <see cref="E:System.ComponentModel.INotifyDataErrorInfo.ErrorsChanged" /> event.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="handler" /> is null.</exception>
	public static void RemoveHandler(INotifyDataErrorInfo source, EventHandler<DataErrorsChangedEventArgs> handler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		CurrentManager.ProtectedRemoveHandler(source, handler);
	}

	/// <summary>Returns a new object to contain listeners to the <see cref="E:System.ComponentModel.INotifyDataErrorInfo.ErrorsChanged" /> event.</summary>
	/// <returns>A new object to contain listeners to the <see cref="E:System.ComponentModel.INotifyDataErrorInfo.ErrorsChanged" /> event.</returns>
	protected override ListenerList NewListenerList()
	{
		return new ListenerList<DataErrorsChangedEventArgs>();
	}

	/// <summary>Begins listening for the <see cref="E:System.ComponentModel.INotifyDataErrorInfo.ErrorsChanged" /> event on the specified source.</summary>
	/// <param name="source">The source object that raises the <see cref="E:System.ComponentModel.INotifyDataErrorInfo.ErrorsChanged" /> event.</param>
	protected override void StartListening(object source)
	{
		((INotifyDataErrorInfo)source).ErrorsChanged += OnErrorsChanged;
	}

	/// <summary>Stops listening for the <see cref="E:System.ComponentModel.INotifyDataErrorInfo.ErrorsChanged" /> event on the specified source.</summary>
	/// <param name="source">The source object that raises the <see cref="E:System.ComponentModel.INotifyDataErrorInfo.ErrorsChanged" /> event.</param>
	protected override void StopListening(object source)
	{
		((INotifyDataErrorInfo)source).ErrorsChanged -= OnErrorsChanged;
	}

	private void OnErrorsChanged(object sender, DataErrorsChangedEventArgs args)
	{
		DeliverEvent(sender, args);
	}
}
