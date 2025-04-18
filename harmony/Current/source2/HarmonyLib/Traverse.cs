using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HarmonyLib;

public class Traverse<T>
{
	private readonly Traverse traverse;

	public T Value
	{
		get
		{
			return traverse.GetValue<T>();
		}
		set
		{
			traverse.SetValue(value);
		}
	}

	private Traverse()
	{
	}

	public Traverse(Traverse traverse)
	{
		this.traverse = traverse;
	}
}
public class Traverse
{
	private static readonly AccessCache Cache;

	private readonly Type _type;

	private readonly object _root;

	private readonly MemberInfo _info;

	private readonly MethodBase _method;

	private readonly object[] _params;

	public static Action<Traverse, Traverse> CopyFields;

	[MethodImpl(MethodImplOptions.Synchronized)]
	static Traverse()
	{
		CopyFields = delegate(Traverse from, Traverse to)
		{
			to.SetValue(from.GetValue());
		};
		if (Cache == null)
		{
			Cache = new AccessCache();
		}
	}

	public static Traverse Create(Type type)
	{
		return new Traverse(type);
	}

	public static Traverse Create<T>()
	{
		return Create(typeof(T));
	}

	public static Traverse Create(object root)
	{
		return new Traverse(root);
	}

	public static Traverse CreateWithType(string name)
	{
		return new Traverse(AccessTools.TypeByName(name));
	}

	private Traverse()
	{
	}

	public Traverse(Type type)
	{
		_type = type;
	}

	public Traverse(object root)
	{
		_root = root;
		_type = root?.GetType();
	}

	private Traverse(object root, MemberInfo info, object[] index)
	{
		_root = root;
		_type = root?.GetType() ?? info.GetUnderlyingType();
		_info = info;
		_params = index;
	}

	private Traverse(object root, MethodInfo method, object[] parameter)
	{
		_root = root;
		_type = method.ReturnType;
		_method = method;
		_params = parameter;
	}

	public object GetValue()
	{
		if (_info is FieldInfo)
		{
			return ((FieldInfo)_info).GetValue(_root);
		}
		if (_info is PropertyInfo)
		{
			return ((PropertyInfo)_info).GetValue(_root, AccessTools.all, null, _params, CultureInfo.CurrentCulture);
		}
		if ((object)_method != null)
		{
			return _method.Invoke(_root, _params);
		}
		if (_root == null && (object)_type != null)
		{
			return _type;
		}
		return _root;
	}

	public T GetValue<T>()
	{
		object value = GetValue();
		if (value == null)
		{
			return default(T);
		}
		return (T)value;
	}

	public object GetValue(params object[] arguments)
	{
		if ((object)_method == null)
		{
			throw new Exception("cannot get method value without method");
		}
		return _method.Invoke(_root, arguments);
	}

	public T GetValue<T>(params object[] arguments)
	{
		if ((object)_method == null)
		{
			throw new Exception("cannot get method value without method");
		}
		return (T)_method.Invoke(_root, arguments);
	}

	public Traverse SetValue(object value)
	{
		if (_info is FieldInfo)
		{
			((FieldInfo)_info).SetValue(_root, value, AccessTools.all, null, CultureInfo.CurrentCulture);
		}
		if (_info is PropertyInfo)
		{
			((PropertyInfo)_info).SetValue(_root, value, AccessTools.all, null, _params, CultureInfo.CurrentCulture);
		}
		if ((object)_method != null)
		{
			throw new Exception("cannot set value of method " + _method.FullDescription());
		}
		return this;
	}

	public Type GetValueType()
	{
		if (_info is FieldInfo)
		{
			return ((FieldInfo)_info).FieldType;
		}
		if (_info is PropertyInfo)
		{
			return ((PropertyInfo)_info).PropertyType;
		}
		return null;
	}

	private Traverse Resolve()
	{
		if (_root == null)
		{
			if (_info is FieldInfo { IsStatic: not false })
			{
				return new Traverse(GetValue());
			}
			if (_info is PropertyInfo propertyInfo && propertyInfo.GetGetMethod().IsStatic)
			{
				return new Traverse(GetValue());
			}
			if ((object)_method != null && _method.IsStatic)
			{
				return new Traverse(GetValue());
			}
			if ((object)_type != null)
			{
				return this;
			}
		}
		return new Traverse(GetValue());
	}

	public Traverse Type(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if ((object)_type == null)
		{
			return new Traverse();
		}
		Type type = AccessTools.Inner(_type, name);
		if ((object)type == null)
		{
			return new Traverse();
		}
		return new Traverse(type);
	}

	public Traverse Field(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		Traverse traverse = Resolve();
		if ((object)traverse._type == null)
		{
			return new Traverse();
		}
		FieldInfo fieldInfo = Cache.GetFieldInfo(traverse._type, name);
		if ((object)fieldInfo == null)
		{
			return new Traverse();
		}
		if (!fieldInfo.IsStatic && traverse._root == null)
		{
			return new Traverse();
		}
		return new Traverse(traverse._root, fieldInfo, null);
	}

	public Traverse<T> Field<T>(string name)
	{
		return new Traverse<T>(Field(name));
	}

	public List<string> Fields()
	{
		Traverse traverse = Resolve();
		return AccessTools.GetFieldNames(traverse._type);
	}

	public Traverse Property(string name, object[] index = null)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		Traverse traverse = Resolve();
		if ((object)traverse._type == null)
		{
			return new Traverse();
		}
		PropertyInfo propertyInfo = Cache.GetPropertyInfo(traverse._type, name);
		if ((object)propertyInfo == null)
		{
			return new Traverse();
		}
		return new Traverse(traverse._root, propertyInfo, index);
	}

	public Traverse<T> Property<T>(string name, object[] index = null)
	{
		return new Traverse<T>(Property(name, index));
	}

	public List<string> Properties()
	{
		Traverse traverse = Resolve();
		return AccessTools.GetPropertyNames(traverse._type);
	}

	public Traverse Method(string name, params object[] arguments)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		Traverse traverse = Resolve();
		if ((object)traverse._type == null)
		{
			return new Traverse();
		}
		Type[] types = AccessTools.GetTypes(arguments);
		MethodBase methodInfo = Cache.GetMethodInfo(traverse._type, name, types);
		if ((object)methodInfo == null)
		{
			return new Traverse();
		}
		return new Traverse(traverse._root, (MethodInfo)methodInfo, arguments);
	}

	public Traverse Method(string name, Type[] paramTypes, object[] arguments = null)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		Traverse traverse = Resolve();
		if ((object)traverse._type == null)
		{
			return new Traverse();
		}
		MethodBase methodInfo = Cache.GetMethodInfo(traverse._type, name, paramTypes);
		if ((object)methodInfo == null)
		{
			return new Traverse();
		}
		return new Traverse(traverse._root, (MethodInfo)methodInfo, arguments);
	}

	public List<string> Methods()
	{
		Traverse traverse = Resolve();
		return AccessTools.GetMethodNames(traverse._type);
	}

	public bool FieldExists()
	{
		if ((object)_info != null)
		{
			return _info is FieldInfo;
		}
		return false;
	}

	public bool PropertyExists()
	{
		if ((object)_info != null)
		{
			return _info is PropertyInfo;
		}
		return false;
	}

	public bool MethodExists()
	{
		return (object)_method != null;
	}

	public bool TypeExists()
	{
		return (object)_type != null;
	}

	public static void IterateFields(object source, Action<Traverse> action)
	{
		Traverse sourceTrv = Create(source);
		AccessTools.GetFieldNames(source).ForEach(delegate(string f)
		{
			action(sourceTrv.Field(f));
		});
	}

	public static void IterateFields(object source, object target, Action<Traverse, Traverse> action)
	{
		Traverse sourceTrv = Create(source);
		Traverse targetTrv = Create(target);
		AccessTools.GetFieldNames(source).ForEach(delegate(string f)
		{
			action(sourceTrv.Field(f), targetTrv.Field(f));
		});
	}

	public static void IterateFields(object source, object target, Action<string, Traverse, Traverse> action)
	{
		Traverse sourceTrv = Create(source);
		Traverse targetTrv = Create(target);
		AccessTools.GetFieldNames(source).ForEach(delegate(string f)
		{
			action(f, sourceTrv.Field(f), targetTrv.Field(f));
		});
	}

	public static void IterateProperties(object source, Action<Traverse> action)
	{
		Traverse sourceTrv = Create(source);
		AccessTools.GetPropertyNames(source).ForEach(delegate(string f)
		{
			action(sourceTrv.Property(f));
		});
	}

	public static void IterateProperties(object source, object target, Action<Traverse, Traverse> action)
	{
		Traverse sourceTrv = Create(source);
		Traverse targetTrv = Create(target);
		AccessTools.GetPropertyNames(source).ForEach(delegate(string f)
		{
			action(sourceTrv.Property(f), targetTrv.Property(f));
		});
	}

	public static void IterateProperties(object source, object target, Action<string, Traverse, Traverse> action)
	{
		Traverse sourceTrv = Create(source);
		Traverse targetTrv = Create(target);
		AccessTools.GetPropertyNames(source).ForEach(delegate(string f)
		{
			action(f, sourceTrv.Property(f), targetTrv.Property(f));
		});
	}

	public override string ToString()
	{
		return (_method ?? GetValue())?.ToString();
	}
}
