using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MonoMod.Utils;

internal sealed class DynData<TTarget> : IDisposable where TTarget : class
{
	private class _Data_ : IDisposable
	{
		public readonly Dictionary<string, Func<TTarget, object?>> Getters = new Dictionary<string, Func<TTarget, object>>();

		public readonly Dictionary<string, Action<TTarget, object?>> Setters = new Dictionary<string, Action<TTarget, object>>();

		public readonly Dictionary<string, object?> Data = new Dictionary<string, object>();

		public readonly HashSet<string> Disposable = new HashSet<string>();

		~_Data_()
		{
			Dispose();
		}

		public void Dispose()
		{
			lock (Data)
			{
				if (Data.Count == 0)
				{
					return;
				}
				foreach (string item in Disposable)
				{
					if (Data.TryGetValue(item, out object value) && value is IDisposable disposable)
					{
						disposable.Dispose();
					}
				}
				Disposable.Clear();
				Data.Clear();
			}
			GC.SuppressFinalize(this);
		}
	}

	private static readonly _Data_ _DataStatic;

	private static readonly ConditionalWeakTable<object, _Data_> _DataMap;

	private static readonly Dictionary<string, Func<TTarget, object?>> _SpecialGetters;

	private static readonly Dictionary<string, Action<TTarget, object?>> _SpecialSetters;

	private readonly WeakReference? Weak;

	private TTarget? KeepAlive;

	private readonly _Data_ _Data;

	public Dictionary<string, Func<TTarget, object?>> Getters => _Data.Getters;

	public Dictionary<string, Action<TTarget, object?>> Setters => _Data.Setters;

	public Dictionary<string, object?> Data => _Data.Data;

	public bool IsAlive
	{
		get
		{
			if (Weak != null)
			{
				return Weak.SafeGetIsAlive();
			}
			return true;
		}
	}

	public TTarget Target => (TTarget)(Weak?.SafeGetTarget());

	public object? this[string name]
	{
		get
		{
			if (_SpecialGetters.TryGetValue(name, out Func<TTarget, object> value) || Getters.TryGetValue(name, out value))
			{
				return value(Target);
			}
			if (Data.TryGetValue(name, out object value2))
			{
				return value2;
			}
			return null;
		}
		set
		{
			if (_SpecialSetters.TryGetValue(name, out Action<TTarget, object> value2) || Setters.TryGetValue(name, out value2))
			{
				value2(Target, value);
				return;
			}
			object obj;
			if (_Data.Disposable.Contains(name) && (obj = this[name]) != null && obj is IDisposable disposable)
			{
				disposable.Dispose();
			}
			Data[name] = value;
		}
	}

	public static event Action<DynData<TTarget>, TTarget?>? OnInitialize;

	static DynData()
	{
		_DataStatic = new _Data_();
		_DataMap = new ConditionalWeakTable<object, _Data_>();
		_SpecialGetters = new Dictionary<string, Func<TTarget, object>>();
		_SpecialSetters = new Dictionary<string, Action<TTarget, object>>();
		FieldInfo[] fields = typeof(TTarget).GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo field in fields)
		{
			string name = field.Name;
			_SpecialGetters[name] = (TTarget obj) => field.GetValue(obj);
			_SpecialSetters[name] = delegate(TTarget obj, object? value)
			{
				field.SetValue(obj, value);
			};
		}
		PropertyInfo[] properties = typeof(TTarget).GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (PropertyInfo propertyInfo in properties)
		{
			string name2 = propertyInfo.Name;
			MethodInfo get = propertyInfo.GetGetMethod(nonPublic: true);
			if (get != null)
			{
				_SpecialGetters[name2] = (TTarget obj) => get.Invoke(obj, ArrayEx.Empty<object>());
			}
			MethodInfo set = propertyInfo.GetSetMethod(nonPublic: true);
			if (set != null)
			{
				_SpecialSetters[name2] = delegate(TTarget obj, object? value)
				{
					set.Invoke(obj, new object[1] { value });
				};
			}
		}
	}

	public DynData()
		: this((TTarget?)null, keepAlive: false)
	{
	}

	public DynData(TTarget? obj)
		: this(obj, keepAlive: true)
	{
	}

	public DynData(TTarget? obj, bool keepAlive)
	{
		if (obj != null)
		{
			WeakReference weak = new WeakReference(obj);
			if (!_DataMap.TryGetValue(obj, out _Data_ value))
			{
				value = new _Data_();
				_DataMap.Add(obj, value);
			}
			_Data = value;
			Weak = weak;
			if (keepAlive)
			{
				KeepAlive = obj;
			}
		}
		else
		{
			_Data = _DataStatic;
		}
		DynData<TTarget>.OnInitialize?.Invoke(this, obj);
	}

	public T? Get<T>(string name)
	{
		return (T)this[name];
	}

	public void Set<T>(string name, T value)
	{
		this[name] = value;
	}

	public void RegisterProperty(string name, Func<TTarget, object?> getter, Action<TTarget, object?> setter)
	{
		Getters[name] = getter;
		Setters[name] = setter;
	}

	public void UnregisterProperty(string name)
	{
		Getters.Remove(name);
		Setters.Remove(name);
	}

	private void Dispose(bool disposing)
	{
		KeepAlive = null;
		if (disposing)
		{
			_Data.Dispose();
		}
	}

	~DynData()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
