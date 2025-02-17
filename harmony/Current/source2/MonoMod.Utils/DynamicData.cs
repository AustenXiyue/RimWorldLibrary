using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MonoMod.Utils;

internal sealed class DynamicData : DynamicObject, IDisposable, IEnumerable<KeyValuePair<string, object?>>, IEnumerable
{
	private class _Cache_
	{
		public readonly Dictionary<string, Func<object?, object?>> Getters = new Dictionary<string, Func<object, object>>();

		public readonly Dictionary<string, Action<object?, object?>> Setters = new Dictionary<string, Action<object, object>>();

		public readonly Dictionary<string, Func<object?, object?[]?, object?>> Methods = new Dictionary<string, Func<object, object[], object>>();

		public _Cache_(Type? targetType)
		{
			bool flag = true;
			while (targetType != null && targetType != targetType.BaseType)
			{
				FieldInfo[] fields = targetType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (FieldInfo fieldInfo in fields)
				{
					string name = fieldInfo.Name;
					if (Getters.ContainsKey(name) || Setters.ContainsKey(name))
					{
						continue;
					}
					try
					{
						FastReflectionHelper.FastInvoker fastInvoker = fieldInfo.GetFastInvoker();
						Getters[name] = (object? obj) => fastInvoker(obj);
						Setters[name] = delegate(object? obj, object? value)
						{
							fastInvoker(obj, value);
						};
					}
					catch
					{
						Getters[name] = fieldInfo.GetValue;
						Setters[name] = fieldInfo.SetValue;
					}
				}
				PropertyInfo[] properties = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (PropertyInfo propertyInfo in properties)
				{
					string name2 = propertyInfo.Name;
					MethodInfo get = propertyInfo.GetGetMethod(nonPublic: true);
					if (get != null && !Getters.ContainsKey(name2))
					{
						try
						{
							FastReflectionHelper.FastInvoker fastInvoker2 = get.GetFastInvoker();
							Getters[name2] = (object? obj) => fastInvoker2(obj);
						}
						catch
						{
							Getters[name2] = (object? obj) => get.Invoke(obj, _NoArgs);
						}
					}
					MethodInfo set = propertyInfo.GetSetMethod(nonPublic: true);
					if (!(set != null) || Setters.ContainsKey(name2))
					{
						continue;
					}
					try
					{
						FastReflectionHelper.FastInvoker fastInvoker3 = set.GetFastInvoker();
						Setters[name2] = delegate(object? obj, object? value)
						{
							fastInvoker3(obj, value);
						};
					}
					catch
					{
						Setters[name2] = delegate(object? obj, object? value)
						{
							set.Invoke(obj, new object[1] { value });
						};
					}
				}
				Dictionary<string, MethodInfo> dictionary = new Dictionary<string, MethodInfo>();
				MethodInfo[] methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (MethodInfo methodInfo in methods)
				{
					string name3 = methodInfo.Name;
					if (flag || !Methods.ContainsKey(name3))
					{
						if (dictionary.ContainsKey(name3))
						{
							dictionary[name3] = null;
						}
						else
						{
							dictionary[name3] = methodInfo;
						}
					}
				}
				foreach (KeyValuePair<string, MethodInfo> item in dictionary)
				{
					if (item.Value == null || item.Value.IsGenericMethod)
					{
						continue;
					}
					try
					{
						FastReflectionHelper.FastInvoker cb = item.Value.GetFastInvoker();
						Methods[item.Key] = (object? target, object?[]? args) => cb(target, args);
					}
					catch
					{
						Methods[item.Key] = item.Value.Invoke;
					}
				}
				flag = false;
				targetType = targetType.BaseType;
			}
		}
	}

	private class _Data_
	{
		public readonly Dictionary<string, Func<object?, object?>> Getters = new Dictionary<string, Func<object, object>>();

		public readonly Dictionary<string, Action<object?, object?>> Setters = new Dictionary<string, Action<object, object>>();

		public readonly Dictionary<string, Func<object?, object?[]?, object?>> Methods = new Dictionary<string, Func<object, object[], object>>();

		public readonly Dictionary<string, object?> Data = new Dictionary<string, object>();

		public _Data_(Type type)
		{
			_ = type == null;
		}
	}

	private static readonly object?[] _NoArgs = ArrayEx.Empty<object>();

	private static readonly Dictionary<Type, _Cache_> _CacheMap = new Dictionary<Type, _Cache_>();

	private static readonly Dictionary<Type, _Data_> _DataStaticMap = new Dictionary<Type, _Data_>();

	private static readonly ConditionalWeakTable<object, _Data_> _DataMap = new ConditionalWeakTable<object, _Data_>();

	private static readonly ConditionalWeakTable<object, DynamicData> _DynamicDataMap = new ConditionalWeakTable<object, DynamicData>();

	private readonly WeakReference? Weak;

	private object? KeepAlive;

	private readonly _Cache_ _Cache;

	private readonly _Data_ _Data;

	public Dictionary<string, Func<object?, object?>> Getters => _Data.Getters;

	public Dictionary<string, Action<object?, object?>> Setters => _Data.Setters;

	public Dictionary<string, Func<object?, object?[]?, object?>> Methods => _Data.Methods;

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

	public object? Target => Weak?.SafeGetTarget();

	public Type TargetType { get; private set; }

	public static event Action<DynamicData, Type, object?>? OnInitialize;

	public DynamicData(Type type)
		: this(type, null, keepAlive: false)
	{
	}

	public DynamicData(object obj)
		: this(Helpers.ThrowIfNull(obj, "obj").GetType(), obj, keepAlive: true)
	{
	}

	public DynamicData(Type type, object? obj)
		: this(type, obj, keepAlive: true)
	{
	}

	public DynamicData(Type type, object? obj, bool keepAlive)
	{
		TargetType = type;
		lock (_CacheMap)
		{
			if (!_CacheMap.TryGetValue(type, out _Cache_ value))
			{
				value = new _Cache_(type);
				_CacheMap.Add(type, value);
			}
			_Cache = value;
		}
		if (obj != null)
		{
			lock (_DataMap)
			{
				if (!_DataMap.TryGetValue(obj, out _Data_ value2))
				{
					value2 = new _Data_(type);
					_DataMap.Add(obj, value2);
				}
				_Data = value2;
			}
			Weak = new WeakReference(obj);
			if (keepAlive)
			{
				KeepAlive = obj;
			}
		}
		else
		{
			lock (_DataStaticMap)
			{
				if (!_DataStaticMap.TryGetValue(type, out _Data_ value3))
				{
					value3 = new _Data_(type);
					_DataStaticMap.Add(type, value3);
				}
				_Data = value3;
			}
		}
		DynamicData.OnInitialize?.Invoke(this, type, obj);
	}

	public static DynamicData For(object obj)
	{
		lock (_DynamicDataMap)
		{
			if (!_DynamicDataMap.TryGetValue(obj, out DynamicData value))
			{
				value = new DynamicData(obj);
				_DynamicDataMap.Add(obj, value);
			}
			return value;
		}
	}

	public static Func<object, T?> New<T>(params object[] args) where T : notnull
	{
		T target = (T)Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, args, null);
		return (object other) => Set(target, other);
	}

	public static Func<object, object?> New(Type type, params object[] args)
	{
		object target = Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, args, null);
		return (object other) => Set(target, other);
	}

	public static Func<object, dynamic> NewWrap<T>(params object[] args)
	{
		T target = (T)Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, args, null);
		return (object other) => Wrap(target, other);
	}

	public static Func<object, dynamic> NewWrap(Type type, params object[] args)
	{
		object target = Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, args, null);
		return (object other) => Wrap(target, other);
	}

	public static dynamic Wrap(object target, object? other = null)
	{
		DynamicData dynamicData = new DynamicData(target);
		dynamicData.CopyFrom(other);
		return dynamicData;
	}

	public static T? Set<T>(T target, object? other = null) where T : notnull
	{
		return (T)Set((object)target, other);
	}

	public static object? Set(object target, object? other = null)
	{
		using DynamicData dynamicData = new DynamicData(target);
		dynamicData.CopyFrom(other);
		return dynamicData.Target;
	}

	public void RegisterProperty(string name, Func<object?, object?> getter, Action<object?, object?> setter)
	{
		Getters[name] = getter;
		Setters[name] = setter;
	}

	public void UnregisterProperty(string name)
	{
		Getters.Remove(name);
		Setters.Remove(name);
	}

	public void RegisterMethod(string name, Func<object?, object?[]?, object?> cb)
	{
		Methods[name] = cb;
	}

	public void UnregisterMethod(string name)
	{
		Methods.Remove(name);
	}

	public void CopyFrom(object? other)
	{
		if (other != null)
		{
			PropertyInfo[] properties = other.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (PropertyInfo propertyInfo in properties)
			{
				Set(propertyInfo.Name, propertyInfo.GetValue(other, null));
			}
		}
	}

	public object? Get(string name)
	{
		TryGet(name, out object value);
		return value;
	}

	public bool TryGet(string name, out object? value)
	{
		object target = Target;
		if (_Data.Getters.TryGetValue(name, out Func<object, object> value2))
		{
			value = value2(target);
			return true;
		}
		if (_Cache.Getters.TryGetValue(name, out value2))
		{
			value = value2(target);
			return true;
		}
		if (_Data.Data.TryGetValue(name, out value))
		{
			return true;
		}
		return false;
	}

	public T? Get<T>(string name)
	{
		return (T)Get(name);
	}

	public bool TryGet<T>(string name, [MaybeNullWhen(false)] out T value)
	{
		object value2;
		bool result = TryGet(name, out value2);
		value = (T)value2;
		return result;
	}

	public void Set(string name, object? value)
	{
		object target = Target;
		if (_Data.Setters.TryGetValue(name, out Action<object, object> value2))
		{
			value2(target, value);
		}
		else if (_Cache.Setters.TryGetValue(name, out value2))
		{
			value2(target, value);
		}
		else
		{
			Data[name] = value;
		}
	}

	public void Add(KeyValuePair<string, object> kvp)
	{
		Set(kvp.Key, kvp.Value);
	}

	public void Add(string key, object value)
	{
		Set(key, value);
	}

	public object? Invoke(string name, params object[] args)
	{
		TryInvoke(name, args, out object result);
		return result;
	}

	public bool TryInvoke(string name, object?[]? args, out object? result)
	{
		if (_Data.Methods.TryGetValue(name, out Func<object, object[], object> value))
		{
			result = value(Target, args);
			return true;
		}
		if (_Cache.Methods.TryGetValue(name, out value))
		{
			result = value(Target, args);
			return true;
		}
		result = null;
		return false;
	}

	public T? Invoke<T>(string name, params object[] args)
	{
		return (T)Invoke(name, args);
	}

	public bool TryInvoke<T>(string name, object[] args, [MaybeNullWhen(false)] out T result)
	{
		object result2;
		bool result3 = TryInvoke(name, args, out result2);
		result = (T)result2;
		return result3;
	}

	private void Dispose(bool disposing)
	{
		KeepAlive = null;
	}

	~DynamicData()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public override IEnumerable<string> GetDynamicMemberNames()
	{
		return _Data.Data.Keys.Union<string>(_Data.Getters.Keys).Union<string>(_Data.Setters.Keys).Union<string>(_Data.Methods.Keys)
			.Union<string>(_Cache.Getters.Keys)
			.Union<string>(_Cache.Setters.Keys)
			.Union<string>(_Cache.Methods.Keys);
	}

	public override bool TryConvert(ConvertBinder binder, out object? result)
	{
		Helpers.ThrowIfArgumentNull(binder, "binder");
		if (TargetType.IsCompatible(binder.Type) || TargetType.IsCompatible(binder.ReturnType) || binder.Type == typeof(object) || binder.ReturnType == typeof(object))
		{
			result = Target;
			return true;
		}
		if (typeof(DynamicData).IsCompatible(binder.Type) || typeof(DynamicData).IsCompatible(binder.ReturnType))
		{
			result = this;
			return true;
		}
		result = null;
		return false;
	}

	public override bool TryGetMember(GetMemberBinder binder, out object? result)
	{
		Helpers.ThrowIfArgumentNull(binder, "binder");
		if (Methods.ContainsKey(binder.Name))
		{
			result = null;
			return false;
		}
		result = Get(binder.Name);
		return true;
	}

	public override bool TrySetMember(SetMemberBinder binder, object? value)
	{
		Helpers.ThrowIfArgumentNull(binder, "binder");
		Set(binder.Name, value);
		return true;
	}

	public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
	{
		Helpers.ThrowIfArgumentNull(binder, "binder");
		return TryInvoke(binder.Name, args, out result);
	}

	public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
	{
		foreach (string item in _Data.Data.Keys.Union<string>(_Data.Getters.Keys).Union<string>(_Data.Setters.Keys).Union<string>(_Cache.Getters.Keys)
			.Union<string>(_Cache.Setters.Keys))
		{
			yield return new KeyValuePair<string, object>(item, Get(item));
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
