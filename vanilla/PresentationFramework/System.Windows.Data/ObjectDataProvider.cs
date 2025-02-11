using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using MS.Internal;
using MS.Internal.Data;

namespace System.Windows.Data;

/// <summary>Wraps and creates an object that you can use as a binding source.</summary>
[Localizability(LocalizationCategory.NeverLocalize)]
public class ObjectDataProvider : DataSourceProvider
{
	private enum SourceMode
	{
		NoSource,
		FromType,
		FromInstance
	}

	private Type _objectType;

	private object _objectInstance;

	private string _methodName;

	private DataSourceProvider _instanceProvider;

	private ParameterCollection _constructorParameters;

	private ParameterCollection _methodParameters;

	private bool _isAsynchronous;

	private SourceMode _mode;

	private bool _needNewInstance = true;

	private EventHandler _sourceDataChangedHandler;

	private const string s_instance = "ObjectInstance";

	private const string s_type = "ObjectType";

	private const string s_method = "MethodName";

	private const string s_async = "IsAsynchronous";

	private const BindingFlags s_invokeMethodFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.InvokeMethod | BindingFlags.OptionalParamBinding;

	/// <summary>Gets or sets the type of object to create an instance of.</summary>
	/// <returns>This property is null when the <see cref="T:System.Windows.Data.ObjectDataProvider" /> is uninitialized or explicitly set to null. If <see cref="P:System.Windows.Data.ObjectDataProvider.ObjectInstance" /> is assigned, <see cref="P:System.Windows.Data.ObjectDataProvider.ObjectType" /> returns the type of the object or null if the object is null. The default value is null.</returns>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="T:System.Windows.Data.ObjectDataProvider" /> is assigned both an <see cref="P:System.Windows.Data.ObjectDataProvider.ObjectType" /> and an <see cref="P:System.Windows.Data.ObjectDataProvider.ObjectInstance" />; only one is allowed.</exception>
	public Type ObjectType
	{
		get
		{
			return _objectType;
		}
		set
		{
			if (_mode == SourceMode.FromInstance)
			{
				throw new InvalidOperationException(SR.ObjectDataProviderCanHaveOnlyOneSource);
			}
			_mode = ((!(value == null)) ? SourceMode.FromType : SourceMode.NoSource);
			_constructorParameters.SetReadOnly(isReadOnly: false);
			if ((_needNewInstance = SetObjectType(value)) && !base.IsRefreshDeferred)
			{
				Refresh();
			}
		}
	}

	/// <summary>Gets or sets the object used as the binding source.</summary>
	/// <returns>The instance of the object constructed from <see cref="P:System.Windows.Data.ObjectDataProvider.ObjectType" /> and <see cref="P:System.Windows.Data.ObjectDataProvider.ConstructorParameters" />, or the <see cref="T:System.Windows.Data.DataSourceProvider" /> of which the <see cref="P:System.Windows.Data.DataSourceProvider.Data" /> is used as the <see cref="P:System.Windows.Data.ObjectDataProvider.ObjectInstance" />.</returns>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="T:System.Windows.Data.ObjectDataProvider" /> is assigned both an <see cref="P:System.Windows.Data.ObjectDataProvider.ObjectType" /> and an <see cref="P:System.Windows.Data.ObjectDataProvider.ObjectInstance" />; only one is allowed.</exception>
	public object ObjectInstance
	{
		get
		{
			if (_instanceProvider == null)
			{
				return _objectInstance;
			}
			return _instanceProvider;
		}
		set
		{
			if (_mode == SourceMode.FromType)
			{
				throw new InvalidOperationException(SR.ObjectDataProviderCanHaveOnlyOneSource);
			}
			_mode = ((value != null) ? SourceMode.FromInstance : SourceMode.NoSource);
			if (ObjectInstance != value)
			{
				if (value != null)
				{
					_constructorParameters.SetReadOnly(isReadOnly: true);
					_constructorParameters.ClearInternal();
				}
				else
				{
					_constructorParameters.SetReadOnly(isReadOnly: false);
				}
				value = TryInstanceProvider(value);
				if (SetObjectInstance(value) && !base.IsRefreshDeferred)
				{
					Refresh();
				}
			}
		}
	}

	/// <summary>Gets or sets the name of the method to call.</summary>
	/// <returns>The name of the method to call. The default value is null.</returns>
	[DefaultValue(null)]
	public string MethodName
	{
		get
		{
			return _methodName;
		}
		set
		{
			_methodName = value;
			OnPropertyChanged("MethodName");
			if (!base.IsRefreshDeferred)
			{
				Refresh();
			}
		}
	}

	/// <summary>Gets the list of parameters to pass to the constructor.</summary>
	/// <returns>The list of parameters to pass to the constructor. The default value is null.</returns>
	public IList ConstructorParameters => _constructorParameters;

	/// <summary>Gets the list of parameters to pass to the method.</summary>
	/// <returns>The list of parameters to pass to the method. The default is an empty list.</returns>
	public IList MethodParameters => _methodParameters;

	/// <summary>Gets or sets a value that indicates whether to perform object creation in a worker thread or in the active context.</summary>
	/// <returns>true to perform object creation in a worker thread; otherwise, false. The default is false.</returns>
	[DefaultValue(false)]
	public bool IsAsynchronous
	{
		get
		{
			return _isAsynchronous;
		}
		set
		{
			_isAsynchronous = value;
			OnPropertyChanged("IsAsynchronous");
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.ObjectDataProvider" /> class.</summary>
	public ObjectDataProvider()
	{
		_constructorParameters = new ParameterCollection(OnParametersChanged);
		_methodParameters = new ParameterCollection(OnParametersChanged);
		_sourceDataChangedHandler = OnSourceDataChanged;
	}

	/// <summary>Indicates whether the <see cref="P:System.Windows.Data.ObjectDataProvider.ObjectType" /> property should be persisted.</summary>
	/// <returns>true if the property value has changed from its default; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeObjectType()
	{
		if (_mode == SourceMode.FromType)
		{
			return ObjectType != null;
		}
		return false;
	}

	/// <summary>Indicates whether the <see cref="P:System.Windows.Data.ObjectDataProvider.ObjectInstance" /> property should be persisted.</summary>
	/// <returns>true if the property value has changed from its default; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeObjectInstance()
	{
		if (_mode == SourceMode.FromInstance)
		{
			return ObjectInstance != null;
		}
		return false;
	}

	/// <summary>Indicates whether the <see cref="P:System.Windows.Data.ObjectDataProvider.ConstructorParameters" /> property should be persisted.</summary>
	/// <returns>true if the property value has changed from its default; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeConstructorParameters()
	{
		if (_mode == SourceMode.FromType)
		{
			return _constructorParameters.Count > 0;
		}
		return false;
	}

	/// <summary>Indicates whether the <see cref="P:System.Windows.Data.ObjectDataProvider.MethodParameters" /> property should be persisted.</summary>
	/// <returns>true if the property value has changed from its default; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeMethodParameters()
	{
		return _methodParameters.Count > 0;
	}

	/// <summary>Starts to create the requested object, either immediately or on a background thread, based on the value of the <see cref="P:System.Windows.Data.ObjectDataProvider.IsAsynchronous" /> property.</summary>
	protected override void BeginQuery()
	{
		if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Attach))
		{
			TraceData.TraceAndNotify(TraceEventType.Warning, TraceData.BeginQuery(TraceData.Identify(this), IsAsynchronous ? "asynchronous" : "synchronous"));
		}
		if (IsAsynchronous)
		{
			ThreadPool.QueueUserWorkItem(QueryWorker, null);
		}
		else
		{
			QueryWorker(null);
		}
	}

	private object TryInstanceProvider(object value)
	{
		if (_instanceProvider != null)
		{
			_instanceProvider.DataChanged -= _sourceDataChangedHandler;
		}
		_instanceProvider = value as DataSourceProvider;
		if (_instanceProvider != null)
		{
			_instanceProvider.DataChanged += _sourceDataChangedHandler;
			value = _instanceProvider.Data;
		}
		return value;
	}

	private bool SetObjectInstance(object value)
	{
		if (_objectInstance == value)
		{
			return false;
		}
		_objectInstance = value;
		SetObjectType(value?.GetType());
		OnPropertyChanged("ObjectInstance");
		return true;
	}

	private bool SetObjectType(Type newType)
	{
		if (_objectType != newType)
		{
			_objectType = newType;
			OnPropertyChanged("ObjectType");
			return true;
		}
		return false;
	}

	private void QueryWorker(object obj)
	{
		object obj2 = null;
		Exception e = null;
		if (_mode == SourceMode.NoSource || _objectType == null)
		{
			if (TraceData.IsEnabled)
			{
				TraceData.TraceAndNotify(TraceEventType.Error, TraceData.ObjectDataProviderHasNoSource);
			}
			e = new InvalidOperationException(SR.ObjectDataProviderHasNoSource);
		}
		else
		{
			Exception e2 = null;
			if (_needNewInstance && _mode == SourceMode.FromType)
			{
				if (_objectType.GetConstructors().Length != 0)
				{
					_objectInstance = CreateObjectInstance(out e2);
				}
				_needNewInstance = false;
			}
			if (string.IsNullOrEmpty(MethodName))
			{
				obj2 = _objectInstance;
			}
			else
			{
				obj2 = InvokeMethodOnInstance(out e);
				if (e != null && e2 != null)
				{
					e = e2;
				}
			}
		}
		if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Attach))
		{
			TraceData.TraceAndNotify(TraceEventType.Warning, TraceData.QueryFinished(TraceData.Identify(this), base.Dispatcher.CheckAccess() ? "synchronous" : "asynchronous", TraceData.Identify(obj2), TraceData.IdentifyException(e)));
		}
		OnQueryFinished(obj2, e, null, null);
	}

	private object CreateObjectInstance(out Exception e)
	{
		object result = null;
		string text = null;
		e = null;
		try
		{
			object[] array = new object[_constructorParameters.Count];
			_constructorParameters.CopyTo(array, 0);
			result = Activator.CreateInstance(_objectType, BindingFlags.Default, null, array, CultureInfo.InvariantCulture);
			OnPropertyChanged("ObjectInstance");
		}
		catch (ArgumentException ex)
		{
			text = "Cannot create Context Affinity object.";
			e = ex;
		}
		catch (COMException ex2)
		{
			text = "Marshaling issue detected.";
			e = ex2;
		}
		catch (MissingMethodException ex3)
		{
			text = "Wrong parameters for constructor.";
			e = ex3;
		}
		catch (Exception ex4)
		{
			if (CriticalExceptions.IsCriticalApplicationException(ex4))
			{
				throw;
			}
			text = null;
			e = ex4;
		}
		catch
		{
			text = null;
			e = new InvalidOperationException(SR.Format(SR.ObjectDataProviderNonCLSException, _objectType.Name));
		}
		if (e != null || text != null)
		{
			if (TraceData.IsEnabled)
			{
				TraceData.TraceAndNotify(TraceEventType.Error, TraceData.ObjDPCreateFailed, null, new object[3] { _objectType.Name, text, e }, new object[1] { e });
			}
			if (!IsAsynchronous && text == null)
			{
				throw e;
			}
		}
		return result;
	}

	private object InvokeMethodOnInstance(out Exception e)
	{
		object result = null;
		string text = null;
		e = null;
		object[] array = new object[_methodParameters.Count];
		_methodParameters.CopyTo(array, 0);
		try
		{
			result = _objectType.InvokeMember(MethodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.InvokeMethod | BindingFlags.OptionalParamBinding, null, _objectInstance, array, CultureInfo.InvariantCulture);
		}
		catch (ArgumentException ex)
		{
			text = "Parameter array contains a string that is a null reference.";
			e = ex;
		}
		catch (MethodAccessException ex2)
		{
			text = "The specified member is a class initializer.";
			e = ex2;
		}
		catch (MissingMethodException ex3)
		{
			text = "No method was found with matching parameter signature.";
			e = ex3;
		}
		catch (TargetException ex4)
		{
			text = "The specified member cannot be invoked on target.";
			e = ex4;
		}
		catch (AmbiguousMatchException ex5)
		{
			text = "More than one method matches the binding criteria.";
			e = ex5;
		}
		catch (Exception ex6)
		{
			if (CriticalExceptions.IsCriticalApplicationException(ex6))
			{
				throw;
			}
			text = null;
			e = ex6;
		}
		catch
		{
			text = null;
			e = new InvalidOperationException(SR.Format(SR.ObjectDataProviderNonCLSExceptionInvoke, MethodName, _objectType.Name));
		}
		if (e != null || text != null)
		{
			if (TraceData.IsEnabled)
			{
				TraceData.TraceAndNotify(TraceEventType.Error, TraceData.ObjDPInvokeFailed, null, new object[4] { MethodName, _objectType.Name, text, e }, new object[1] { e });
			}
			if (!IsAsynchronous && text == null)
			{
				throw e;
			}
		}
		return result;
	}

	private void OnParametersChanged(ParameterCollection sender)
	{
		if (sender == _constructorParameters)
		{
			Invariant.Assert(_mode != SourceMode.FromInstance);
			_needNewInstance = true;
		}
		if (!base.IsRefreshDeferred)
		{
			Refresh();
		}
	}

	private void OnSourceDataChanged(object sender, EventArgs args)
	{
		Invariant.Assert(sender == _instanceProvider);
		if (SetObjectInstance(_instanceProvider.Data) && !base.IsRefreshDeferred)
		{
			Refresh();
		}
	}

	private void OnPropertyChanged(string propertyName)
	{
		OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
	}
}
