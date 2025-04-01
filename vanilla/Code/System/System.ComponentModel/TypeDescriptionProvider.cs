using System.Collections;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides supplemental metadata to the <see cref="T:System.ComponentModel.TypeDescriptor" />.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public abstract class TypeDescriptionProvider
{
	private sealed class EmptyCustomTypeDescriptor : CustomTypeDescriptor
	{
	}

	private TypeDescriptionProvider _parent;

	private EmptyCustomTypeDescriptor _emptyDescriptor;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.TypeDescriptionProvider" /> class.</summary>
	protected TypeDescriptionProvider()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.TypeDescriptionProvider" /> class using a parent type description provider.</summary>
	/// <param name="parent">The parent type description provider.</param>
	protected TypeDescriptionProvider(TypeDescriptionProvider parent)
	{
		_parent = parent;
	}

	/// <summary>Creates an object that can substitute for another data type.</summary>
	/// <returns>The substitute <see cref="T:System.Object" />.</returns>
	/// <param name="provider">An optional service provider.</param>
	/// <param name="objectType">The type of object to create. This parameter is never null.</param>
	/// <param name="argTypes">An optional array of types that represent the parameter types to be passed to the object's constructor. This array can be null or of zero length.</param>
	/// <param name="args">An optional array of parameter values to pass to the object's constructor.</param>
	public virtual object CreateInstance(IServiceProvider provider, Type objectType, Type[] argTypes, object[] args)
	{
		if (_parent != null)
		{
			return _parent.CreateInstance(provider, objectType, argTypes, args);
		}
		if (objectType == null)
		{
			throw new ArgumentNullException("objectType");
		}
		return SecurityUtils.SecureCreateInstance(objectType, args);
	}

	/// <summary>Gets a per-object cache, accessed as an <see cref="T:System.Collections.IDictionary" /> of key/value pairs.</summary>
	/// <returns>An <see cref="T:System.Collections.IDictionary" /> if the provided object supports caching; otherwise, null.</returns>
	/// <param name="instance">The object for which to get the cache.</param>
	public virtual IDictionary GetCache(object instance)
	{
		if (_parent != null)
		{
			return _parent.GetCache(instance);
		}
		return null;
	}

	/// <summary>Gets an extended custom type descriptor for the given object.</summary>
	/// <returns>An <see cref="T:System.ComponentModel.ICustomTypeDescriptor" /> that can provide extended metadata for the object.</returns>
	/// <param name="instance">The object for which to get the extended type descriptor.</param>
	public virtual ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
	{
		if (_parent != null)
		{
			return _parent.GetExtendedTypeDescriptor(instance);
		}
		if (_emptyDescriptor == null)
		{
			_emptyDescriptor = new EmptyCustomTypeDescriptor();
		}
		return _emptyDescriptor;
	}

	/// <summary>Gets the extender providers for the specified object.</summary>
	/// <returns>An array of extender providers for <paramref name="instance" />.</returns>
	/// <param name="instance">The object to get extender providers for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="instance" /> is null.</exception>
	protected internal virtual IExtenderProvider[] GetExtenderProviders(object instance)
	{
		if (_parent != null)
		{
			return _parent.GetExtenderProviders(instance);
		}
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		return new IExtenderProvider[0];
	}

	/// <summary>Gets the name of the specified component, or null if the component has no name.</summary>
	/// <returns>The name of the specified component.</returns>
	/// <param name="component">The specified component.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="component" /> is null.</exception>
	public virtual string GetFullComponentName(object component)
	{
		if (_parent != null)
		{
			return _parent.GetFullComponentName(component);
		}
		return GetTypeDescriptor(component).GetComponentName();
	}

	/// <summary>Performs normal reflection against a type.</summary>
	/// <returns>The type of reflection for this <paramref name="objectType" />.</returns>
	/// <param name="objectType">The type of object for which to retrieve the <see cref="T:System.Reflection.IReflect" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="objectType" /> is null.</exception>
	public Type GetReflectionType(Type objectType)
	{
		return GetReflectionType(objectType, null);
	}

	/// <summary>Performs normal reflection against the given object.</summary>
	/// <returns>The type of reflection for this <paramref name="instance" />.</returns>
	/// <param name="instance">An instance of the type (should not be null).</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="instance" /> is null.</exception>
	public Type GetReflectionType(object instance)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		return GetReflectionType(instance.GetType(), instance);
	}

	/// <summary>Performs normal reflection against the given object with the given type.</summary>
	/// <returns>The type of reflection for this <paramref name="objectType" />.</returns>
	/// <param name="objectType">The type of object for which to retrieve the <see cref="T:System.Reflection.IReflect" />.</param>
	/// <param name="instance">An instance of the type. Can be null.</param>
	public virtual Type GetReflectionType(Type objectType, object instance)
	{
		if (_parent != null)
		{
			return _parent.GetReflectionType(objectType, instance);
		}
		return objectType;
	}

	/// <summary>Converts a reflection type into a runtime type.</summary>
	/// <returns>A <see cref="T:System.Type" /> that represents the runtime equivalent of <paramref name="reflectionType" />.</returns>
	/// <param name="reflectionType">The type to convert to its runtime equivalent.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="reflectionType" /> is null.</exception>
	public virtual Type GetRuntimeType(Type reflectionType)
	{
		if (_parent != null)
		{
			return _parent.GetRuntimeType(reflectionType);
		}
		if (reflectionType == null)
		{
			throw new ArgumentNullException("reflectionType");
		}
		if (reflectionType.GetType().Assembly == typeof(object).Assembly)
		{
			return reflectionType;
		}
		return reflectionType.UnderlyingSystemType;
	}

	/// <summary>Gets a custom type descriptor for the given type.</summary>
	/// <returns>An <see cref="T:System.ComponentModel.ICustomTypeDescriptor" /> that can provide metadata for the type.</returns>
	/// <param name="objectType">The type of object for which to retrieve the type descriptor.</param>
	public ICustomTypeDescriptor GetTypeDescriptor(Type objectType)
	{
		return GetTypeDescriptor(objectType, null);
	}

	/// <summary>Gets a custom type descriptor for the given object.</summary>
	/// <returns>An <see cref="T:System.ComponentModel.ICustomTypeDescriptor" /> that can provide metadata for the type.</returns>
	/// <param name="instance">An instance of the type. Can be null if no instance was passed to the <see cref="T:System.ComponentModel.TypeDescriptor" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="instance" /> is null.</exception>
	public ICustomTypeDescriptor GetTypeDescriptor(object instance)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		return GetTypeDescriptor(instance.GetType(), instance);
	}

	/// <summary>Gets a custom type descriptor for the given type and object.</summary>
	/// <returns>An <see cref="T:System.ComponentModel.ICustomTypeDescriptor" /> that can provide metadata for the type.</returns>
	/// <param name="objectType">The type of object for which to retrieve the type descriptor.</param>
	/// <param name="instance">An instance of the type. Can be null if no instance was passed to the <see cref="T:System.ComponentModel.TypeDescriptor" />.</param>
	public virtual ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
	{
		if (_parent != null)
		{
			return _parent.GetTypeDescriptor(objectType, instance);
		}
		if (_emptyDescriptor == null)
		{
			_emptyDescriptor = new EmptyCustomTypeDescriptor();
		}
		return _emptyDescriptor;
	}

	/// <summary>Gets a value that indicates whether the specified type is compatible with the type description and its chain of type description providers. </summary>
	/// <returns>true if <paramref name="type" /> is compatible with the type description and its chain of type description providers; otherwise, false. </returns>
	/// <param name="type">The type to test for compatibility.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null.</exception>
	public virtual bool IsSupportedType(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (_parent != null)
		{
			return _parent.IsSupportedType(type);
		}
		return true;
	}
}
