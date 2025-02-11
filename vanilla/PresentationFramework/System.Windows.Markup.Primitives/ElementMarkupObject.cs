using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Reflection;

namespace System.Windows.Markup.Primitives;

internal class ElementMarkupObject : MarkupObject
{
	private sealed class ElementObjectContext : ValueSerializerContextWrapper, IValueSerializerContext, ITypeDescriptorContext, IServiceProvider
	{
		private ElementMarkupObject _object;

		object ITypeDescriptorContext.Instance => _object.Instance;

		public ElementObjectContext(ElementMarkupObject obj, IValueSerializerContext baseContext)
			: base(baseContext)
		{
			_object = obj;
		}
	}

	private struct ShouldSerializeKey
	{
		private Type _type;

		private string _propertyName;

		public ShouldSerializeKey(Type type, string propertyName)
		{
			_type = type;
			_propertyName = propertyName;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ShouldSerializeKey shouldSerializeKey))
			{
				return false;
			}
			if (shouldSerializeKey._type == _type)
			{
				return shouldSerializeKey._propertyName == _propertyName;
			}
			return false;
		}

		public static bool operator ==(ShouldSerializeKey key1, ShouldSerializeKey key2)
		{
			return key1.Equals(key2);
		}

		public static bool operator !=(ShouldSerializeKey key1, ShouldSerializeKey key2)
		{
			return !key1.Equals(key2);
		}

		public override int GetHashCode()
		{
			return _type.GetHashCode() ^ _propertyName.GetHashCode();
		}
	}

	private static readonly object _shouldSerializeCacheLock = new object();

	private static Hashtable _shouldSerializeCache = new Hashtable();

	private static Type[] _shouldSerializeArgsObject = new Type[1] { typeof(DependencyObject) };

	private static Type[] _shouldSerializeArgsManager = new Type[1] { typeof(XamlDesignerSerializationManager) };

	private static Type[] _shouldSerializeArgsMode = new Type[1] { typeof(XamlWriterMode) };

	private static Type[] _shouldSerializeArgsObjectManager = new Type[2]
	{
		typeof(DependencyObject),
		typeof(XamlDesignerSerializationManager)
	};

	private static Attribute[] _propertyAttributes = new Attribute[1]
	{
		new PropertyFilterAttribute(PropertyFilterOptions.SetValues)
	};

	private object _instance;

	private IValueSerializerContext _context;

	private ElementKey _key;

	private XamlDesignerSerializationManager _manager;

	public override Type ObjectType => _instance.GetType();

	public override object Instance => _instance;

	public override AttributeCollection Attributes => TypeDescriptor.GetAttributes(ObjectType);

	internal IValueSerializerContext Context => _context;

	internal XamlDesignerSerializationManager Manager => _manager;

	internal ElementMarkupObject(object instance, XamlDesignerSerializationManager manager)
	{
		_instance = instance;
		_context = new ElementObjectContext(this, null);
		_manager = manager;
	}

	internal override IEnumerable<MarkupProperty> GetProperties(bool mapToConstructorArgs)
	{
		ValueSerializer serializerFor = ValueSerializer.GetSerializerFor(ObjectType, Context);
		if (serializerFor != null && serializerFor.CanConvertToString(_instance, Context))
		{
			yield return new ElementStringValueProperty(this);
			if (_key != null)
			{
				yield return _key;
			}
			yield break;
		}
		Dictionary<string, string> constructorArguments = null;
		if (mapToConstructorArgs && _instance is MarkupExtension && TryGetConstructorInfoArguments(_instance, out var parameters, out var arguments))
		{
			int i = 0;
			foreach (object item in arguments)
			{
				if (constructorArguments == null)
				{
					constructorArguments = new Dictionary<string, string>();
				}
				constructorArguments.Add(parameters[i].Name, parameters[i].Name);
				yield return new ElementConstructorArgument(item, parameters[i++].ParameterType, this);
			}
		}
		foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(_instance))
		{
			DesignerSerializationVisibility serializationVisibility = property.SerializationVisibility;
			if (serializationVisibility == DesignerSerializationVisibility.Hidden || (property.IsReadOnly && serializationVisibility != DesignerSerializationVisibility.Content) || !ShouldSerialize(property, _instance, _manager))
			{
				continue;
			}
			if (serializationVisibility == DesignerSerializationVisibility.Content)
			{
				object value = property.GetValue(_instance);
				if (value == null || value is ICollection { Count: <1 } || (value is IEnumerable enumerable && !enumerable.GetEnumerator().MoveNext()))
				{
					continue;
				}
			}
			if (constructorArguments == null || !(property.Attributes[typeof(ConstructorArgumentAttribute)] is ConstructorArgumentAttribute constructorArgumentAttribute) || !constructorArguments.ContainsKey(constructorArgumentAttribute.ArgumentName))
			{
				yield return new ElementProperty(this, property);
			}
		}
		if (_instance is IDictionary value2)
		{
			yield return new ElementDictionaryItemsPseudoProperty(value2, typeof(IDictionary), this);
		}
		else if (_instance is IEnumerable enumerable2 && enumerable2.GetEnumerator().MoveNext())
		{
			yield return new ElementItemsPseudoProperty(enumerable2, typeof(IEnumerable), this);
		}
		if (_key != null)
		{
			yield return _key;
		}
	}

	public override void AssignRootContext(IValueSerializerContext context)
	{
		_context = new ElementObjectContext(this, context);
	}

	internal void SetKey(ElementKey key)
	{
		_key = key;
	}

	private static bool ShouldSerialize(PropertyDescriptor pd, object instance, XamlDesignerSerializationManager manager)
	{
		object obj = instance;
		DependencyPropertyDescriptor dependencyPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(pd);
		MethodInfo methodInfo;
		if (dependencyPropertyDescriptor != null && dependencyPropertyDescriptor.IsAttached)
		{
			Type ownerType = dependencyPropertyDescriptor.DependencyProperty.OwnerType;
			string name = dependencyPropertyDescriptor.DependencyProperty.Name;
			string propertyName = name + "!";
			if (!TryGetShouldSerializeMethod(new ShouldSerializeKey(ownerType, propertyName), out methodInfo))
			{
				string name2 = "ShouldSerialize" + name;
				methodInfo = ownerType.GetMethod(name2, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, _shouldSerializeArgsObject, null);
				if (methodInfo == null)
				{
					methodInfo = ownerType.GetMethod(name2, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, _shouldSerializeArgsManager, null);
				}
				if (methodInfo == null)
				{
					methodInfo = ownerType.GetMethod(name2, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, _shouldSerializeArgsMode, null);
				}
				if (methodInfo == null)
				{
					methodInfo = ownerType.GetMethod(name2, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, _shouldSerializeArgsObjectManager, null);
				}
				if (methodInfo != null && methodInfo.ReturnType != typeof(bool))
				{
					methodInfo = null;
				}
				CacheShouldSerializeMethod(new ShouldSerializeKey(ownerType, propertyName), methodInfo);
			}
			obj = null;
		}
		else if (!TryGetShouldSerializeMethod(new ShouldSerializeKey(instance.GetType(), pd.Name), out methodInfo))
		{
			Type type = instance.GetType();
			string name3 = "ShouldSerialize" + pd.Name;
			methodInfo = type.GetMethod(name3, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, _shouldSerializeArgsObject, null);
			if (methodInfo == null)
			{
				methodInfo = type.GetMethod(name3, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, _shouldSerializeArgsManager, null);
			}
			if (methodInfo == null)
			{
				methodInfo = type.GetMethod(name3, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, _shouldSerializeArgsMode, null);
			}
			if (methodInfo == null)
			{
				methodInfo = type.GetMethod(name3, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, _shouldSerializeArgsObjectManager, null);
			}
			if (methodInfo != null && methodInfo.ReturnType != typeof(bool))
			{
				methodInfo = null;
			}
			CacheShouldSerializeMethod(new ShouldSerializeKey(type, pd.Name), methodInfo);
		}
		if (methodInfo != null)
		{
			ParameterInfo[] parameters = methodInfo.GetParameters();
			if (parameters != null)
			{
				object[] parameters2 = ((parameters.Length != 1) ? new object[2]
				{
					instance as DependencyObject,
					manager
				} : ((parameters[0].ParameterType == typeof(DependencyObject)) ? new object[1] { instance as DependencyObject } : ((!(parameters[0].ParameterType == typeof(XamlWriterMode))) ? new object[1] { manager } : new object[1] { manager.XamlWriterMode })));
				return (bool)methodInfo.Invoke(obj, parameters2);
			}
		}
		return pd.ShouldSerializeValue(instance);
	}

	private static bool TryGetShouldSerializeMethod(ShouldSerializeKey key, out MethodInfo methodInfo)
	{
		object obj = _shouldSerializeCache[key];
		if (obj == null || obj == _shouldSerializeCacheLock)
		{
			methodInfo = null;
			return obj != null;
		}
		methodInfo = obj as MethodInfo;
		return true;
	}

	private static void CacheShouldSerializeMethod(ShouldSerializeKey key, MethodInfo methodInfo)
	{
		object value = ((methodInfo == null) ? _shouldSerializeCacheLock : methodInfo);
		lock (_shouldSerializeCacheLock)
		{
			_shouldSerializeCache[key] = value;
		}
	}

	private bool TryGetConstructorInfoArguments(object instance, out ParameterInfo[] parameters, out ICollection arguments)
	{
		TypeConverter converter = TypeDescriptor.GetConverter(instance);
		if (converter != null && converter.CanConvertTo(Context, typeof(InstanceDescriptor)))
		{
			InstanceDescriptor instanceDescriptor;
			try
			{
				instanceDescriptor = converter.ConvertTo(_context, TypeConverterHelper.InvariantEnglishUS, instance, typeof(InstanceDescriptor)) as InstanceDescriptor;
			}
			catch (InvalidOperationException)
			{
				instanceDescriptor = null;
			}
			catch (NotSupportedException)
			{
				instanceDescriptor = null;
			}
			if (instanceDescriptor != null)
			{
				ConstructorInfo constructorInfo = instanceDescriptor.MemberInfo as ConstructorInfo;
				if (constructorInfo != null)
				{
					ParameterInfo[] parameters2 = constructorInfo.GetParameters();
					if (parameters2 != null && parameters2.Length == instanceDescriptor.Arguments.Count)
					{
						parameters = parameters2;
						arguments = instanceDescriptor.Arguments;
						return true;
					}
				}
			}
		}
		parameters = null;
		arguments = null;
		return false;
	}
}
