using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace System.Windows.Markup.Primitives;

internal abstract class ElementPropertyBase : MarkupProperty
{
	private sealed class ElementPropertyContext : ValueSerializerContextWrapper, IValueSerializerContext, ITypeDescriptorContext, IServiceProvider
	{
		private ElementPropertyBase _property;

		PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor => _property.PropertyDescriptor;

		public ElementPropertyContext(ElementPropertyBase property, IValueSerializerContext baseContext)
			: base(baseContext)
		{
			_property = property;
		}
	}

	private static readonly List<Type> EmptyTypes = new List<Type>();

	private static Dictionary<Type, Type> _keyTypeMap;

	private bool _isComposite;

	private bool _isCompositeCalculated;

	private IValueSerializerContext _context;

	private XamlDesignerSerializationManager _manager;

	public override bool IsComposite
	{
		get
		{
			if (!_isCompositeCalculated)
			{
				_isCompositeCalculated = true;
				object value = Value;
				if (value == null)
				{
					_isComposite = true;
				}
				else if (value is string && PropertyType.IsAssignableFrom(typeof(object)))
				{
					_isComposite = false;
				}
				else if (value is MarkupExtension)
				{
					_isComposite = true;
				}
				else
				{
					_isComposite = !CanConvertToString(value);
				}
			}
			return _isComposite;
		}
	}

	public override string StringValue
	{
		get
		{
			if (IsComposite)
			{
				return string.Empty;
			}
			object value = Value;
			if (value is string result)
			{
				return result;
			}
			ValueSerializer valueSerializer = GetValueSerializer();
			if (valueSerializer == null)
			{
				return string.Empty;
			}
			return valueSerializer.ConvertToString(value, Context);
		}
	}

	public override IEnumerable<MarkupObject> Items
	{
		get
		{
			object value = Value;
			if (value != null)
			{
				if (PropertyDescriptor != null && (PropertyDescriptor.IsReadOnly || (!PropertyIsAttached(PropertyDescriptor) && PropertyType == value.GetType() && (typeof(IList).IsAssignableFrom(PropertyType) || typeof(IDictionary).IsAssignableFrom(PropertyType) || typeof(Freezable).IsAssignableFrom(PropertyType) || typeof(FrameworkElementFactory).IsAssignableFrom(PropertyType)) && HasNoSerializableProperties(value) && !IsEmpty(value))))
				{
					if (value is IDictionary dictionary)
					{
						Type keyType = GetDictionaryKeyType(dictionary);
						DictionaryEntry[] array = new DictionaryEntry[dictionary.Count];
						dictionary.CopyTo(array, 0);
						Array.Sort(array, (DictionaryEntry one, DictionaryEntry two) => string.Compare(one.Key.ToString(), two.Key.ToString()));
						DictionaryEntry[] array2 = array;
						for (int i = 0; i < array2.Length; i++)
						{
							DictionaryEntry dictionaryEntry = array2[i];
							ElementMarkupObject elementMarkupObject = new ElementMarkupObject(ElementProperty.CheckForMarkupExtension(typeof(object), dictionaryEntry.Value, Context, convertEnums: false), Manager);
							elementMarkupObject.SetKey(new ElementKey(dictionaryEntry.Key, keyType, elementMarkupObject));
							yield return elementMarkupObject;
						}
					}
					else if (value is IEnumerable enumerable)
					{
						foreach (object item in enumerable)
						{
							yield return new ElementMarkupObject(ElementProperty.CheckForMarkupExtension(typeof(object), item, Context, convertEnums: false), Manager);
						}
					}
					else if (PropertyType == typeof(FrameworkElementFactory) && value is FrameworkElementFactory)
					{
						yield return new FrameworkElementFactoryMarkupObject(value as FrameworkElementFactory, Manager);
					}
					else
					{
						yield return new ElementMarkupObject(ElementProperty.CheckForMarkupExtension(typeof(object), value, Context, convertEnums: true), Manager);
					}
				}
				else
				{
					yield return new ElementMarkupObject(ElementProperty.CheckForMarkupExtension(typeof(object), value, Context, convertEnums: true), Manager);
				}
			}
			else
			{
				yield return new ElementMarkupObject(new NullExtension(), Manager);
			}
		}
	}

	protected IValueSerializerContext Context
	{
		get
		{
			if (_context == null)
			{
				_context = new ElementPropertyContext(this, GetItemContext());
			}
			return _context;
		}
	}

	internal XamlDesignerSerializationManager Manager => _manager;

	public override IEnumerable<Type> TypeReferences
	{
		get
		{
			ValueSerializer valueSerializer = GetValueSerializer();
			if (valueSerializer == null)
			{
				return EmptyTypes;
			}
			return valueSerializer.TypeReferences(Value, Context);
		}
	}

	public ElementPropertyBase(XamlDesignerSerializationManager manager)
	{
		_manager = manager;
	}

	private bool PropertyIsAttached(PropertyDescriptor descriptor)
	{
		return DependencyPropertyDescriptor.FromProperty(PropertyDescriptor)?.IsAttached ?? false;
	}

	private bool HasNoSerializableProperties(object value)
	{
		if (value is FrameworkElementFactory)
		{
			return true;
		}
		foreach (MarkupProperty property in new ElementMarkupObject(value, Manager).Properties)
		{
			if (!property.IsContent)
			{
				return false;
			}
		}
		return true;
	}

	private bool IsEmpty(object value)
	{
		if (value is IEnumerable enumerable)
		{
			{
				IEnumerator enumerator = enumerable.GetEnumerator();
				try
				{
					if (enumerator.MoveNext())
					{
						_ = enumerator.Current;
						return false;
					}
				}
				finally
				{
					IDisposable disposable = enumerator as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
			}
			return true;
		}
		return false;
	}

	protected bool CanConvertToString(object value)
	{
		if (value == null)
		{
			return false;
		}
		return GetValueSerializer()?.CanConvertToString(value, Context) ?? false;
	}

	protected abstract IValueSerializerContext GetItemContext();

	protected abstract Type GetObjectType();

	private ValueSerializer GetValueSerializer()
	{
		PropertyDescriptor propertyDescriptor = PropertyDescriptor;
		if (propertyDescriptor == null)
		{
			DependencyProperty dependencyProperty = DependencyProperty;
			if (dependencyProperty != null)
			{
				propertyDescriptor = DependencyPropertyDescriptor.FromProperty(dependencyProperty, GetObjectType());
			}
		}
		if (propertyDescriptor != null)
		{
			return ValueSerializer.GetSerializerFor(propertyDescriptor, GetItemContext());
		}
		return ValueSerializer.GetSerializerFor(PropertyType, GetItemContext());
	}

	private static Type GetDictionaryKeyType(IDictionary value)
	{
		Type type = value.GetType();
		if (_keyTypeMap == null)
		{
			_keyTypeMap = new Dictionary<Type, Type>();
		}
		if (!_keyTypeMap.TryGetValue(type, out var value2))
		{
			Type[] interfaces = type.GetInterfaces();
			foreach (Type type2 in interfaces)
			{
				if (type2.IsGenericType && type2.GetGenericTypeDefinition() == typeof(IDictionary<, >))
				{
					return type2.GetGenericArguments()[0];
				}
			}
			value2 = typeof(object);
			_keyTypeMap[type] = value2;
		}
		return value2;
	}
}
