using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Markup;

namespace System.Xaml.Schema;

/// <summary>Provides an extension point that can construct instances of a XAML type through techniques other than reflection and constructors.</summary>
public class XamlTypeInvoker
{
	private static class DefaultCtorXamlActivator
	{
		private static ThreeValuedBool s_securityFailureWithCtorDelegate;

		private static ConstructorInfo s_actionCtor = typeof(Action<object>).GetConstructor(new Type[2]
		{
			typeof(object),
			typeof(nint)
		});

		public static object CreateInstance(XamlTypeInvoker type)
		{
			if (!EnsureConstructorDelegate(type))
			{
				return null;
			}
			return CallCtorDelegate(type);
		}

		private static object CallCtorDelegate(XamlTypeInvoker type)
		{
			object uninitializedObject = FormatterServices.GetUninitializedObject(type._xamlType.UnderlyingType);
			InvokeDelegate(type._constructorDelegate, uninitializedObject);
			return uninitializedObject;
		}

		private static void InvokeDelegate(Action<object> action, object argument)
		{
			action(argument);
		}

		private static bool EnsureConstructorDelegate(XamlTypeInvoker type)
		{
			if (type._constructorDelegate != null)
			{
				return true;
			}
			if (!type.IsPublic)
			{
				return false;
			}
			if (s_securityFailureWithCtorDelegate == ThreeValuedBool.NotSet)
			{
				s_securityFailureWithCtorDelegate = ThreeValuedBool.False;
			}
			if (s_securityFailureWithCtorDelegate == ThreeValuedBool.True)
			{
				return false;
			}
			Type underlyingSystemType = type._xamlType.UnderlyingType.UnderlyingSystemType;
			ConstructorInfo constructor = underlyingSystemType.GetConstructor(Type.EmptyTypes);
			if (constructor == null)
			{
				throw new MissingMethodException(System.SR.Format(System.SR.NoDefaultConstructor, underlyingSystemType.FullName));
			}
			if ((constructor.IsSecurityCritical && !constructor.IsSecuritySafeCritical) || (constructor.Attributes & MethodAttributes.HasSecurity) == MethodAttributes.HasSecurity || (underlyingSystemType.Attributes & TypeAttributes.HasSecurity) == TypeAttributes.HasSecurity)
			{
				type._isPublic = ThreeValuedBool.False;
				return false;
			}
			nint functionPointer = constructor.MethodHandle.GetFunctionPointer();
			Action<object> action = (action = (Action<object>)s_actionCtor.Invoke(new object[2] { null, functionPointer }));
			type._constructorDelegate = action;
			return true;
		}
	}

	private static XamlTypeInvoker s_Unknown;

	private static object[] s_emptyObjectArray = Array.Empty<object>();

	private Dictionary<XamlType, MethodInfo> _addMethods;

	private XamlType _xamlType;

	private Action<object> _constructorDelegate;

	private ThreeValuedBool _isPublic;

	private ThreeValuedBool _isInSystemXaml;

	internal MethodInfo EnumeratorMethod { get; set; }

	/// <summary>Provides a static value that represents an unknown, not fully implemented <see cref="T:System.Xaml.Schema.XamlTypeInvoker" />.</summary>
	/// <returns>A static value that represents an unknown, not fully implemented <see cref="T:System.Xaml.Schema.XamlTypeInvoker" />.</returns>
	public static XamlTypeInvoker UnknownInvoker
	{
		get
		{
			if (s_Unknown == null)
			{
				s_Unknown = new XamlTypeInvoker();
			}
			return s_Unknown;
		}
	}

	/// <summary>Gets the handler to use when a <see cref="T:System.Xaml.XamlObjectWriter" /> calls into an implemented <see cref="T:System.Windows.Markup.MarkupExtension" />.</summary>
	/// <returns>A handler implementation that handles this case.</returns>
	public EventHandler<XamlSetMarkupExtensionEventArgs> SetMarkupExtensionHandler
	{
		get
		{
			if (!(_xamlType != null))
			{
				return null;
			}
			return _xamlType.SetMarkupExtensionHandler;
		}
	}

	/// <summary>Gets the handler to use when a <see cref="T:System.Xaml.XamlObjectWriter" /> calls into a CLR-implemented <see cref="T:System.ComponentModel.TypeConverter" />.</summary>
	/// <returns>A handler implementation that handles this case.</returns>
	public EventHandler<XamlSetTypeConverterEventArgs> SetTypeConverterHandler
	{
		get
		{
			if (!(_xamlType != null))
			{
				return null;
			}
			return _xamlType.SetTypeConverterHandler;
		}
	}

	private bool IsInSystemXaml
	{
		get
		{
			if (_isInSystemXaml == ThreeValuedBool.NotSet)
			{
				bool flag = SafeReflectionInvoker.IsInSystemXaml(_xamlType.UnderlyingType.UnderlyingSystemType);
				_isInSystemXaml = ((!flag) ? ThreeValuedBool.False : ThreeValuedBool.True);
			}
			return _isInSystemXaml == ThreeValuedBool.True;
		}
	}

	private bool IsPublic
	{
		get
		{
			if (_isPublic == ThreeValuedBool.NotSet)
			{
				Type underlyingSystemType = _xamlType.UnderlyingType.UnderlyingSystemType;
				_isPublic = ((!underlyingSystemType.IsVisible) ? ThreeValuedBool.False : ThreeValuedBool.True);
			}
			return _isPublic == ThreeValuedBool.True;
		}
	}

	private bool IsUnknown
	{
		get
		{
			if (!(_xamlType == null))
			{
				return _xamlType.UnderlyingType == null;
			}
			return true;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.Schema.XamlTypeInvoker" /> class.</summary>
	protected XamlTypeInvoker()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.Schema.XamlTypeInvoker" /> class, based on a provided <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <param name="type">The <see cref="T:System.Xaml.XamlType" /> value for the specific XAML type relevant to this <see cref="T:System.Xaml.Schema.XamlTypeInvoker" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null.</exception>
	public XamlTypeInvoker(XamlType type)
	{
		_xamlType = type ?? throw new ArgumentNullException("type");
	}

	/// <summary>Adds the provided item to an instance of the type that is relevant to this <see cref="T:System.Xaml.Schema.XamlTypeInvoker" />. </summary>
	/// <param name="instance">An instance of the type specified by the <see cref="T:System.Xaml.XamlType" /> used for constructing this <see cref="T:System.Xaml.Schema.XamlTypeInvoker" />.</param>
	/// <param name="item">The item to add.</param>
	/// <exception cref="T:System.NotSupportedException">Invoked this method on a <see cref="T:System.Xaml.Schema.XamlTypeInvoker" /> that is based on an unknown <see cref="T:System.Xaml.XamlType" />.-or-Invoked this method on a <see cref="T:System.Xaml.Schema.XamlTypeInvoker" /> where the relevant type is not a collection.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="instance" /> is null.</exception>
	/// <exception cref="T:System.Xaml.XamlSchemaException">
	///   <see cref="M:System.Xaml.Schema.XamlTypeInvoker.GetAddMethod(System.Xaml.XamlType)" /> for this<see cref="T:System.Xaml.Schema.XamlTypeInvoker" /> returns null.</exception>
	public virtual void AddToCollection(object instance, object item)
	{
		ArgumentNullException.ThrowIfNull(instance, "instance");
		if (instance is IList list)
		{
			list.Add(item);
			return;
		}
		ThrowIfUnknown();
		if (!_xamlType.IsCollection)
		{
			throw new NotSupportedException(System.SR.OnlySupportedOnCollections);
		}
		XamlType xamlType = ((item == null) ? _xamlType.ItemType : _xamlType.SchemaContext.GetXamlType(item.GetType()));
		MethodInfo addMethod = GetAddMethod(xamlType);
		if (addMethod == null)
		{
			throw new XamlSchemaException(System.SR.Format(System.SR.NoAddMethodFound, _xamlType, xamlType));
		}
		addMethod.Invoke(instance, new object[1] { item });
	}

	/// <summary>Adds the provided key and item value to an instance of the type that is relevant to this <see cref="T:System.Xaml.Schema.XamlTypeInvoker" />. </summary>
	/// <param name="instance">An instance of the type specified by the <see cref="T:System.Xaml.XamlType" /> used for constructing this <see cref="T:System.Xaml.Schema.XamlTypeInvoker" />.</param>
	/// <param name="key">Dictionary key for the item to add.</param>
	/// <param name="item">The item value to add.</param>
	/// <exception cref="T:System.NotSupportedException">Invoked this method on a <see cref="T:System.Xaml.Schema.XamlTypeInvoker" /> that is based on an unknown <see cref="T:System.Xaml.XamlType" />.-or-Invoked this method on a <see cref="T:System.Xaml.Schema.XamlTypeInvoker" /> where the relevant type is not a dictionary.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="instance" /> is null.</exception>
	/// <exception cref="T:System.Xaml.XamlSchemaException">
	///   <see cref="M:System.Xaml.Schema.XamlTypeInvoker.GetAddMethod(System.Xaml.XamlType)" /> for this<see cref="T:System.Xaml.Schema.XamlTypeInvoker" /> returns null.</exception>
	public virtual void AddToDictionary(object instance, object key, object item)
	{
		ArgumentNullException.ThrowIfNull(instance, "instance");
		if (instance is IDictionary dictionary)
		{
			dictionary.Add(key, item);
			return;
		}
		ThrowIfUnknown();
		if (!_xamlType.IsDictionary)
		{
			throw new NotSupportedException(System.SR.OnlySupportedOnDictionaries);
		}
		XamlType xamlType = ((item == null) ? _xamlType.ItemType : _xamlType.SchemaContext.GetXamlType(item.GetType()));
		MethodInfo addMethod = GetAddMethod(xamlType);
		if (addMethod == null)
		{
			throw new XamlSchemaException(System.SR.Format(System.SR.NoAddMethodFound, _xamlType, xamlType));
		}
		addMethod.Invoke(instance, new object[2] { key, item });
	}

	/// <summary>Creates an object instance based on the construction-initiated <see cref="T:System.Xaml.XamlType" /> for this <see cref="T:System.Xaml.Schema.XamlTypeInvoker" />.</summary>
	/// <returns>The created instance based on the construction-initiated <see cref="T:System.Xaml.XamlType" /> for this <see cref="T:System.Xaml.Schema.XamlTypeInvoker" />.</returns>
	/// <param name="arguments">An array of objects that supply the x:ConstructorArgs for the instance. May be null for types that do not require or use x:ConstructorArgs.</param>
	/// <exception cref="T:System.MissingMethodException">Could not resolve a constructor.</exception>
	public virtual object CreateInstance(object[] arguments)
	{
		ThrowIfUnknown();
		if (!_xamlType.UnderlyingType.IsValueType && (arguments == null || arguments.Length == 0))
		{
			object obj = DefaultCtorXamlActivator.CreateInstance(this);
			if (obj != null)
			{
				return obj;
			}
		}
		return Activator.CreateInstance(_xamlType.UnderlyingType, arguments);
	}

	/// <summary>Returns the relevant Add method for a <see cref="T:System.Xaml.Schema.XamlTypeInvoker" /> that is relevant to a collection or dictionary. </summary>
	/// <returns>CLR <see cref="T:System.Reflection.MethodInfo" /> information for the Add method, or null.</returns>
	/// <param name="contentType">
	///   <see cref="T:System.Xaml.XamlType" /> for the item type that is used by the Add method.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="contentType" /> is null.</exception>
	public virtual MethodInfo GetAddMethod(XamlType contentType)
	{
		ArgumentNullException.ThrowIfNull(contentType, "contentType");
		if (IsUnknown || _xamlType.ItemType == null)
		{
			return null;
		}
		if (contentType == _xamlType.ItemType || (_xamlType.AllowedContentTypes.Count == 1 && contentType.CanAssignTo(_xamlType.ItemType)))
		{
			return _xamlType.AddMethod;
		}
		if (!_xamlType.IsCollection)
		{
			return null;
		}
		MethodInfo value;
		if (_addMethods == null)
		{
			Dictionary<XamlType, MethodInfo> dictionary = new Dictionary<XamlType, MethodInfo>();
			dictionary.Add(_xamlType.ItemType, _xamlType.AddMethod);
			foreach (XamlType allowedContentType in _xamlType.AllowedContentTypes)
			{
				value = CollectionReflector.GetAddMethod(_xamlType.UnderlyingType, allowedContentType.UnderlyingType);
				if (value != null)
				{
					dictionary.TryAdd(allowedContentType, value);
				}
			}
			_addMethods = dictionary;
		}
		if (_addMethods.TryGetValue(contentType, out value))
		{
			return value;
		}
		foreach (KeyValuePair<XamlType, MethodInfo> addMethod in _addMethods)
		{
			if (contentType.CanAssignTo(addMethod.Key))
			{
				return addMethod.Value;
			}
		}
		return null;
	}

	/// <summary>Returns an object representing a method that can enumerate over items.</summary>
	/// <returns>
	///   <see cref="T:System.Reflection.MethodInfo" /> for an enumerator method, or null.</returns>
	public virtual MethodInfo GetEnumeratorMethod()
	{
		if (IsUnknown)
		{
			return null;
		}
		return _xamlType.GetEnumeratorMethod;
	}

	/// <summary>Returns an <see cref="T:System.Collections.IEnumerator" /> object representing the set of items.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object, or null.</returns>
	/// <param name="instance">An instance of the type specified by the <see cref="T:System.Xaml.XamlType" /> used for constructing this <see cref="T:System.Xaml.Schema.XamlTypeInvoker" />.</param>
	/// <exception cref="T:System.NotSupportedException">Invoked this method on a <see cref="T:System.Xaml.Schema.XamlTypeInvoker" /> that is based on an unknown <see cref="T:System.Xaml.XamlType" />.-or-Invoked this method on a <see cref="T:System.Xaml.Schema.XamlTypeInvoker" /> where the relevant type is not a collection or dictionary.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="instance" /> is null.</exception>
	public virtual IEnumerator GetItems(object instance)
	{
		ArgumentNullException.ThrowIfNull(instance, "instance");
		if (instance is IEnumerable enumerable)
		{
			return enumerable.GetEnumerator();
		}
		ThrowIfUnknown();
		if (!_xamlType.IsCollection && !_xamlType.IsDictionary)
		{
			throw new NotSupportedException(System.SR.OnlySupportedOnCollectionsAndDictionaries);
		}
		return (IEnumerator)GetEnumeratorMethod().Invoke(instance, s_emptyObjectArray);
	}

	private void ThrowIfUnknown()
	{
		if (IsUnknown)
		{
			throw new NotSupportedException(System.SR.NotSupportedOnUnknownType);
		}
	}
}
