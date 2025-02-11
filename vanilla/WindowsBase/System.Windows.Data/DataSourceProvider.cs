using System.ComponentModel;
using System.Windows.Threading;
using MS.Internal;

namespace System.Windows.Data;

/// <summary>Common base class and contract for <see cref="T:System.Windows.Data.DataSourceProvider" /> objects, which are factories that execute some queries to produce a single object or a list of objects that you can use as binding source objects.</summary>
public abstract class DataSourceProvider : INotifyPropertyChanged, ISupportInitialize
{
	private class DeferHelper : IDisposable
	{
		private DataSourceProvider _provider;

		public DeferHelper(DataSourceProvider provider)
		{
			_provider = provider;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			if (_provider != null)
			{
				_provider.EndDefer();
				_provider = null;
			}
		}
	}

	private bool _isInitialLoadEnabled = true;

	private bool _initialLoadCalled;

	private int _deferLevel;

	private object _data;

	private Exception _error;

	private Dispatcher _dispatcher;

	private static readonly DispatcherOperationCallback UpdateWithNewResultCallback = UpdateWithNewResult;

	/// <summary>Gets or sets a value that indicates whether to prevent or delay the automatic loading of data.</summary>
	/// <returns>false to prevent or delay the automatic loading of data; otherwise, true. The default value is true.</returns>
	[DefaultValue(true)]
	public bool IsInitialLoadEnabled
	{
		get
		{
			return _isInitialLoadEnabled;
		}
		set
		{
			_isInitialLoadEnabled = value;
			OnPropertyChanged(new PropertyChangedEventArgs("IsInitialLoadEnabled"));
		}
	}

	/// <summary>Gets the underlying data object.</summary>
	/// <returns>A value of type <see cref="T:System.Object" /> that is the underlying data object.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public object Data => _data;

	/// <summary>Gets the error of the last query operation.</summary>
	/// <returns>A value of type <see cref="T:System.Exception" /> that is the error of the last query operation, or null if there was no error.</returns>
	public Exception Error => _error;

	/// <summary>Gets a value that indicates whether there is an outstanding <see cref="M:System.Windows.Data.DataSourceProvider.DeferRefresh" /> in use.</summary>
	/// <returns>true if there is an outstanding <see cref="M:System.Windows.Data.DataSourceProvider.DeferRefresh" /> in use; otherwise, false.</returns>
	protected bool IsRefreshDeferred
	{
		get
		{
			if (_deferLevel <= 0)
			{
				if (!IsInitialLoadEnabled)
				{
					return !_initialLoadCalled;
				}
				return false;
			}
			return true;
		}
	}

	/// <summary>Gets or sets the current <see cref="T:System.Windows.Threading.Dispatcher" /> object to the UI thread to use.</summary>
	/// <returns>The current <see cref="T:System.Windows.Threading.Dispatcher" /> object to the UI thread to use. By default, this is the <see cref="T:System.Windows.Threading.Dispatcher" /> object that is associated with the thread on which this instance was created.</returns>
	protected Dispatcher Dispatcher
	{
		get
		{
			return _dispatcher;
		}
		set
		{
			if (_dispatcher != value)
			{
				_dispatcher = value;
			}
		}
	}

	/// <summary>Occurs when the <see cref="P:System.Windows.Data.DataSourceProvider.Data" /> property has a new value.</summary>
	public event EventHandler DataChanged;

	/// <summary>Occurs when a property value changes.</summary>
	event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
	{
		add
		{
			PropertyChanged += value;
		}
		remove
		{
			PropertyChanged -= value;
		}
	}

	/// <summary>Occurs when a property value changes.</summary>
	protected virtual event PropertyChangedEventHandler PropertyChanged;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.DataSourceProvider" /> class. This is a protected constructor.</summary>
	protected DataSourceProvider()
	{
		_dispatcher = Dispatcher.CurrentDispatcher;
	}

	/// <summary>Starts the initial query to the underlying data model. The result is returned on the <see cref="P:System.Windows.Data.DataSourceProvider.Data" /> property.</summary>
	public void InitialLoad()
	{
		if (IsInitialLoadEnabled && !_initialLoadCalled)
		{
			_initialLoadCalled = true;
			BeginQuery();
		}
	}

	/// <summary>Initiates a refresh operation to the underlying data model. The result is returned on the <see cref="P:System.Windows.Data.DataSourceProvider.Data" /> property.</summary>
	public void Refresh()
	{
		_initialLoadCalled = true;
		BeginQuery();
	}

	/// <summary>Enters a defer cycle that you can use to change properties of the provider and delay automatic refresh.</summary>
	/// <returns>An <see cref="T:System.IDisposable" /> object that you can use to dispose of the calling object.</returns>
	public virtual IDisposable DeferRefresh()
	{
		_deferLevel++;
		return new DeferHelper(this);
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void ISupportInitialize.BeginInit()
	{
		BeginInit();
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void ISupportInitialize.EndInit()
	{
		EndInit();
	}

	/// <summary>When overridden in a derived class, this base class calls this method when <see cref="M:System.Windows.Data.DataSourceProvider.InitialLoad" /> or <see cref="M:System.Windows.Data.DataSourceProvider.Refresh" /> has been called. The base class delays the call if refresh is deferred or initial load is disabled.</summary>
	protected virtual void BeginQuery()
	{
	}

	/// <summary>Derived classes call this method to indicate that a query has finished.</summary>
	/// <param name="newData">The data that is the result of the query.</param>
	protected void OnQueryFinished(object newData)
	{
		OnQueryFinished(newData, null, null, null);
	}

	/// <summary>Derived classes call this method to indicate that a query has finished.</summary>
	/// <param name="newData">The data that is the result of the query.</param>
	/// <param name="error">The error that occurred while running the query. This value is null if there is no error.</param>
	/// <param name="completionWork">Optional delegate that is used to execute completion work on the UI thread, for example, to set additional properties.</param>
	/// <param name="callbackArguments">Optional arguments to send as a parameter with the <paramref name="completionWork" /> delegate.</param>
	protected virtual void OnQueryFinished(object newData, Exception error, DispatcherOperationCallback completionWork, object callbackArguments)
	{
		Invariant.Assert(Dispatcher != null);
		if (Dispatcher.CheckAccess())
		{
			UpdateWithNewResult(error, newData, completionWork, callbackArguments);
			return;
		}
		Dispatcher.BeginInvoke(DispatcherPriority.Normal, UpdateWithNewResultCallback, new object[5] { this, error, newData, completionWork, callbackArguments });
	}

	/// <summary>Raises the <see cref="E:System.Windows.Data.DataSourceProvider.PropertyChanged" /> event with the provided arguments.</summary>
	/// <param name="e">Arguments of the event being raised.</param>
	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, e);
		}
	}

	/// <summary>Indicates that initialization of this object is about to begin; no implicit <see cref="M:System.Windows.Data.DataSourceProvider.Refresh" /> occurs until the matched <see cref="M:System.Windows.Data.DataSourceProvider.EndInit" /> method is called.</summary>
	protected virtual void BeginInit()
	{
		_deferLevel++;
	}

	/// <summary>Indicates that the initialization of this object has completed; this causes a <see cref="M:System.Windows.Data.DataSourceProvider.Refresh" /> if no other <see cref="M:System.Windows.Data.DataSourceProvider.DeferRefresh" /> is outstanding.</summary>
	protected virtual void EndInit()
	{
		EndDefer();
	}

	private void EndDefer()
	{
		_deferLevel--;
		if (_deferLevel == 0)
		{
			Refresh();
		}
	}

	private static object UpdateWithNewResult(object arg)
	{
		object[] obj = (object[])arg;
		Invariant.Assert(obj.Length == 5);
		DataSourceProvider dataSourceProvider = (DataSourceProvider)obj[0];
		Exception error = (Exception)obj[1];
		object newData = obj[2];
		DispatcherOperationCallback completionWork = (DispatcherOperationCallback)obj[3];
		object callbackArgs = obj[4];
		dataSourceProvider.UpdateWithNewResult(error, newData, completionWork, callbackArgs);
		return null;
	}

	private void UpdateWithNewResult(Exception error, object newData, DispatcherOperationCallback completionWork, object callbackArgs)
	{
		bool num = _error != error;
		_error = error;
		if (error != null)
		{
			newData = null;
			_initialLoadCalled = false;
		}
		_data = newData;
		completionWork?.Invoke(callbackArgs);
		OnPropertyChanged(new PropertyChangedEventArgs("Data"));
		if (this.DataChanged != null)
		{
			this.DataChanged(this, EventArgs.Empty);
		}
		if (num)
		{
			OnPropertyChanged(new PropertyChangedEventArgs("Error"));
		}
	}
}
