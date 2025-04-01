using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace System;

/// <summary>Represents the base class for custom attributes.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ComDefaultInterface(typeof(_Attribute))]
public abstract class Attribute : _Attribute
{
	/// <summary>When implemented in a derived class, gets a unique identifier for this <see cref="T:System.Attribute" />.</summary>
	/// <returns>An <see cref="T:System.Object" /> that is a unique identifier for the attribute.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual object TypeId => GetType();

	private static Attribute[] InternalGetCustomAttributes(PropertyInfo element, Type type, bool inherit)
	{
		return (Attribute[])MonoCustomAttrs.GetCustomAttributes(element, type, inherit);
	}

	private static Attribute[] InternalGetCustomAttributes(EventInfo element, Type type, bool inherit)
	{
		return (Attribute[])MonoCustomAttrs.GetCustomAttributes(element, type, inherit);
	}

	private static Attribute[] InternalParamGetCustomAttributes(ParameterInfo parameter, Type attributeType, bool inherit)
	{
		if (parameter.Member.MemberType != MemberTypes.Method)
		{
			return null;
		}
		MethodInfo methodInfo = (MethodInfo)parameter.Member;
		MethodInfo baseDefinition = methodInfo.GetBaseDefinition();
		if (attributeType == null)
		{
			attributeType = typeof(Attribute);
		}
		if (methodInfo == baseDefinition)
		{
			return (Attribute[])parameter.GetCustomAttributes(attributeType, inherit);
		}
		List<Type> list = new List<Type>();
		List<Attribute> list2 = new List<Attribute>();
		while (true)
		{
			Attribute[] array = (Attribute[])methodInfo.GetParametersInternal()[parameter.Position].GetCustomAttributes(attributeType, inherit: false);
			foreach (Attribute attribute in array)
			{
				Type type = attribute.GetType();
				if (!list.Contains(type))
				{
					list.Add(type);
					list2.Add(attribute);
				}
			}
			MethodInfo baseMethod = methodInfo.GetBaseMethod();
			if (baseMethod == methodInfo)
			{
				break;
			}
			methodInfo = baseMethod;
		}
		Attribute[] array2 = (Attribute[])Array.CreateInstance(attributeType, list2.Count);
		list2.CopyTo(array2, 0);
		return array2;
	}

	private static bool InternalIsDefined(PropertyInfo element, Type attributeType, bool inherit)
	{
		return MonoCustomAttrs.IsDefined(element, attributeType, inherit);
	}

	private static bool InternalIsDefined(EventInfo element, Type attributeType, bool inherit)
	{
		return MonoCustomAttrs.IsDefined(element, attributeType, inherit);
	}

	private static bool InternalParamIsDefined(ParameterInfo parameter, Type attributeType, bool inherit)
	{
		if (parameter.IsDefined(attributeType, inherit))
		{
			return true;
		}
		if (!inherit)
		{
			return false;
		}
		MemberInfo member = parameter.Member;
		if (member.MemberType != MemberTypes.Method)
		{
			return false;
		}
		MethodInfo methodInfo = ((MethodInfo)member).GetBaseMethod();
		while (true)
		{
			if (methodInfo.GetParametersInternal()[parameter.Position].IsDefined(attributeType, inherit: false))
			{
				return true;
			}
			MethodInfo baseMethod = methodInfo.GetBaseMethod();
			if (baseMethod == methodInfo)
			{
				break;
			}
			methodInfo = baseMethod;
		}
		return false;
	}

	/// <summary>Retrieves an array of the custom attributes applied to a member of a type. Parameters specify the member, and the type of the custom attribute to search for.</summary>
	/// <returns>An <see cref="T:System.Attribute" /> array that contains the custom attributes of type <paramref name="type" /> applied to <paramref name="element" />, or an empty array if no such custom attributes exist.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.MemberInfo" /> class that describes a constructor, event, field, method, or property member of a class. </param>
	/// <param name="type">The type, or a base type, of the custom attribute to search for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="type" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="type" /> is not derived from <see cref="T:System.Attribute" />. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="element" /> is not a constructor, method, property, event, type, or field. </exception>
	/// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded. </exception>
	/// <filterpriority>1</filterpriority>
	public static Attribute[] GetCustomAttributes(MemberInfo element, Type type)
	{
		return GetCustomAttributes(element, type, inherit: true);
	}

	/// <summary>Retrieves an array of the custom attributes applied to a member of a type. Parameters specify the member, the type of the custom attribute to search for, and whether to search ancestors of the member.</summary>
	/// <returns>An <see cref="T:System.Attribute" /> array that contains the custom attributes of type <paramref name="type" /> applied to <paramref name="element" />, or an empty array if no such custom attributes exist.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.MemberInfo" /> class that describes a constructor, event, field, method, or property member of a class. </param>
	/// <param name="type">The type, or a base type, of the custom attribute to search for.</param>
	/// <param name="inherit">If true, specifies to also search the ancestors of <paramref name="element" /> for custom attributes. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="type" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="type" /> is not derived from <see cref="T:System.Attribute" />. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="element" /> is not a constructor, method, property, event, type, or field. </exception>
	/// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded. </exception>
	/// <filterpriority>1</filterpriority>
	public static Attribute[] GetCustomAttributes(MemberInfo element, Type type, bool inherit)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (!type.IsSubclassOf(typeof(Attribute)) && type != typeof(Attribute))
		{
			throw new ArgumentException(Environment.GetResourceString("Type passed in must be derived from System.Attribute or System.Attribute itself."));
		}
		return element.MemberType switch
		{
			MemberTypes.Property => InternalGetCustomAttributes((PropertyInfo)element, type, inherit), 
			MemberTypes.Event => InternalGetCustomAttributes((EventInfo)element, type, inherit), 
			_ => element.GetCustomAttributes(type, inherit) as Attribute[], 
		};
	}

	/// <summary>Retrieves an array of the custom attributes applied to a member of a type. A parameter specifies the member.</summary>
	/// <returns>An <see cref="T:System.Attribute" /> array that contains the custom attributes applied to <paramref name="element" />, or an empty array if no such custom attributes exist.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.MemberInfo" /> class that describes a constructor, event, field, method, or property member of a class. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="element" /> is not a constructor, method, property, event, type, or field. </exception>
	/// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded. </exception>
	/// <filterpriority>1</filterpriority>
	public static Attribute[] GetCustomAttributes(MemberInfo element)
	{
		return GetCustomAttributes(element, inherit: true);
	}

	/// <summary>Retrieves an array of the custom attributes applied to a member of a type. Parameters specify the member, the type of the custom attribute to search for, and whether to search ancestors of the member.</summary>
	/// <returns>An <see cref="T:System.Attribute" /> array that contains the custom attributes applied to <paramref name="element" />, or an empty array if no such custom attributes exist.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.MemberInfo" /> class that describes a constructor, event, field, method, or property member of a class. </param>
	/// <param name="inherit">If true, specifies to also search the ancestors of <paramref name="element" /> for custom attributes. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="element" /> is not a constructor, method, property, event, type, or field. </exception>
	/// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded. </exception>
	/// <filterpriority>1</filterpriority>
	public static Attribute[] GetCustomAttributes(MemberInfo element, bool inherit)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return element.MemberType switch
		{
			MemberTypes.Property => InternalGetCustomAttributes((PropertyInfo)element, typeof(Attribute), inherit), 
			MemberTypes.Event => InternalGetCustomAttributes((EventInfo)element, typeof(Attribute), inherit), 
			_ => element.GetCustomAttributes(typeof(Attribute), inherit) as Attribute[], 
		};
	}

	/// <summary>Determines whether any custom attributes are applied to a member of a type. Parameters specify the member, and the type of the custom attribute to search for.</summary>
	/// <returns>true if a custom attribute of type <paramref name="attributeType" /> is applied to <paramref name="element" />; otherwise, false.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.MemberInfo" /> class that describes a constructor, event, field, method, type, or property member of a class. </param>
	/// <param name="attributeType">The type, or a base type, of the custom attribute to search for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="element" /> is not a constructor, method, property, event, type, or field. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsDefined(MemberInfo element, Type attributeType)
	{
		return IsDefined(element, attributeType, inherit: true);
	}

	/// <summary>Determines whether any custom attributes are applied to a member of a type. Parameters specify the member, the type of the custom attribute to search for, and whether to search ancestors of the member.</summary>
	/// <returns>true if a custom attribute of type <paramref name="attributeType" /> is applied to <paramref name="element" />; otherwise, false.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.MemberInfo" /> class that describes a constructor, event, field, method, type, or property member of a class. </param>
	/// <param name="attributeType">The type, or a base type, of the custom attribute to search for.</param>
	/// <param name="inherit">If true, specifies to also search the ancestors of <paramref name="element" /> for custom attributes. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="element" /> is not a constructor, method, property, event, type, or field. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsDefined(MemberInfo element, Type attributeType, bool inherit)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (attributeType == null)
		{
			throw new ArgumentNullException("attributeType");
		}
		if (!attributeType.IsSubclassOf(typeof(Attribute)) && attributeType != typeof(Attribute))
		{
			throw new ArgumentException(Environment.GetResourceString("Type passed in must be derived from System.Attribute or System.Attribute itself."));
		}
		return element.MemberType switch
		{
			MemberTypes.Property => InternalIsDefined((PropertyInfo)element, attributeType, inherit), 
			MemberTypes.Event => InternalIsDefined((EventInfo)element, attributeType, inherit), 
			_ => element.IsDefined(attributeType, inherit), 
		};
	}

	/// <summary>Retrieves a custom attribute applied to a member of a type. Parameters specify the member, and the type of the custom attribute to search for.</summary>
	/// <returns>A reference to the single custom attribute of type <paramref name="attributeType" /> that is applied to <paramref name="element" />, or null if there is no such attribute.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.MemberInfo" /> class that describes a constructor, event, field, method, or property member of a class. </param>
	/// <param name="attributeType">The type, or a base type, of the custom attribute to search for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="element" /> is not a constructor, method, property, event, type, or field. </exception>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found. </exception>
	/// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded. </exception>
	/// <filterpriority>1</filterpriority>
	public static Attribute GetCustomAttribute(MemberInfo element, Type attributeType)
	{
		return GetCustomAttribute(element, attributeType, inherit: true);
	}

	/// <summary>Retrieves a custom attribute applied to a member of a type. Parameters specify the member, the type of the custom attribute to search for, and whether to search ancestors of the member.</summary>
	/// <returns>A reference to the single custom attribute of type <paramref name="attributeType" /> that is applied to <paramref name="element" />, or null if there is no such attribute.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.MemberInfo" /> class that describes a constructor, event, field, method, or property member of a class. </param>
	/// <param name="attributeType">The type, or a base type, of the custom attribute to search for.</param>
	/// <param name="inherit">If true, specifies to also search the ancestors of <paramref name="element" /> for custom attributes. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="element" /> is not a constructor, method, property, event, type, or field. </exception>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found. </exception>
	/// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded. </exception>
	/// <filterpriority>1</filterpriority>
	public static Attribute GetCustomAttribute(MemberInfo element, Type attributeType, bool inherit)
	{
		Attribute[] customAttributes = GetCustomAttributes(element, attributeType, inherit);
		if (customAttributes == null || customAttributes.Length == 0)
		{
			return null;
		}
		if (customAttributes.Length == 1)
		{
			return customAttributes[0];
		}
		throw new AmbiguousMatchException(Environment.GetResourceString("Multiple custom attributes of the same type found."));
	}

	/// <summary>Retrieves an array of the custom attributes applied to a method parameter. A parameter specifies the method parameter.</summary>
	/// <returns>An <see cref="T:System.Attribute" /> array that contains the custom attributes applied to <paramref name="element" />, or an empty array if no such custom attributes exist.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.ParameterInfo" /> class that describes a parameter of a member of a class. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null. </exception>
	/// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded. </exception>
	/// <filterpriority>1</filterpriority>
	public static Attribute[] GetCustomAttributes(ParameterInfo element)
	{
		return GetCustomAttributes(element, inherit: true);
	}

	/// <summary>Retrieves an array of the custom attributes applied to a method parameter. Parameters specify the method parameter, and the type of the custom attribute to search for.</summary>
	/// <returns>An <see cref="T:System.Attribute" /> array that contains the custom attributes of type <paramref name="attributeType" /> applied to <paramref name="element" />, or an empty array if no such custom attributes exist.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.ParameterInfo" /> class that describes a parameter of a member of a class. </param>
	/// <param name="attributeType">The type, or a base type, of the custom attribute to search for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />. </exception>
	/// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded. </exception>
	/// <filterpriority>1</filterpriority>
	public static Attribute[] GetCustomAttributes(ParameterInfo element, Type attributeType)
	{
		return GetCustomAttributes(element, attributeType, inherit: true);
	}

	/// <summary>Retrieves an array of the custom attributes applied to a method parameter. Parameters specify the method parameter, the type of the custom attribute to search for, and whether to search ancestors of the method parameter.</summary>
	/// <returns>An <see cref="T:System.Attribute" /> array that contains the custom attributes of type <paramref name="attributeType" /> applied to <paramref name="element" />, or an empty array if no such custom attributes exist.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.ParameterInfo" /> class that describes a parameter of a member of a class. </param>
	/// <param name="attributeType">The type, or a base type, of the custom attribute to search for.</param>
	/// <param name="inherit">If true, specifies to also search the ancestors of <paramref name="element" /> for custom attributes. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />. </exception>
	/// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded. </exception>
	/// <filterpriority>1</filterpriority>
	public static Attribute[] GetCustomAttributes(ParameterInfo element, Type attributeType, bool inherit)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (attributeType == null)
		{
			throw new ArgumentNullException("attributeType");
		}
		if (!attributeType.IsSubclassOf(typeof(Attribute)) && attributeType != typeof(Attribute))
		{
			throw new ArgumentException(Environment.GetResourceString("Type passed in must be derived from System.Attribute or System.Attribute itself."));
		}
		if (element.Member == null)
		{
			throw new ArgumentException(Environment.GetResourceString("The ParameterInfo object is not valid."), "element");
		}
		if (element.Member.MemberType == MemberTypes.Method && inherit)
		{
			return InternalParamGetCustomAttributes(element, attributeType, inherit);
		}
		return element.GetCustomAttributes(attributeType, inherit) as Attribute[];
	}

	/// <summary>Retrieves an array of the custom attributes applied to a method parameter. Parameters specify the method parameter, and whether to search ancestors of the method parameter.</summary>
	/// <returns>An <see cref="T:System.Attribute" /> array that contains the custom attributes applied to <paramref name="element" />, or an empty array if no such custom attributes exist.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.ParameterInfo" /> class that describes a parameter of a member of a class. </param>
	/// <param name="inherit">If true, specifies to also search the ancestors of <paramref name="element" /> for custom attributes. </param>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Reflection.ParameterInfo.Member" /> property of <paramref name="element" /> is null. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null. </exception>
	/// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded. </exception>
	/// <filterpriority>1</filterpriority>
	public static Attribute[] GetCustomAttributes(ParameterInfo element, bool inherit)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (element.Member == null)
		{
			throw new ArgumentException(Environment.GetResourceString("The ParameterInfo object is not valid."), "element");
		}
		if (element.Member.MemberType == MemberTypes.Method && inherit)
		{
			return InternalParamGetCustomAttributes(element, null, inherit);
		}
		return element.GetCustomAttributes(typeof(Attribute), inherit) as Attribute[];
	}

	/// <summary>Determines whether any custom attributes are applied to a method parameter. Parameters specify the method parameter, and the type of the custom attribute to search for.</summary>
	/// <returns>true if a custom attribute of type <paramref name="attributeType" /> is applied to <paramref name="element" />; otherwise, false.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.ParameterInfo" /> class that describes a parameter of a member of a class. </param>
	/// <param name="attributeType">The type, or a base type, of the custom attribute to search for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsDefined(ParameterInfo element, Type attributeType)
	{
		return IsDefined(element, attributeType, inherit: true);
	}

	/// <summary>Determines whether any custom attributes are applied to a method parameter. Parameters specify the method parameter, the type of the custom attribute to search for, and whether to search ancestors of the method parameter.</summary>
	/// <returns>true if a custom attribute of type <paramref name="attributeType" /> is applied to <paramref name="element" />; otherwise, false.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.ParameterInfo" /> class that describes a parameter of a member of a class. </param>
	/// <param name="attributeType">The type, or a base type, of the custom attribute to search for.</param>
	/// <param name="inherit">If true, specifies to also search the ancestors of <paramref name="element" /> for custom attributes. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />. </exception>
	/// <exception cref="T:System.ExecutionEngineException">
	///   <paramref name="element" /> is not a method, constructor, or type. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsDefined(ParameterInfo element, Type attributeType, bool inherit)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (attributeType == null)
		{
			throw new ArgumentNullException("attributeType");
		}
		if (!attributeType.IsSubclassOf(typeof(Attribute)) && attributeType != typeof(Attribute))
		{
			throw new ArgumentException(Environment.GetResourceString("Type passed in must be derived from System.Attribute or System.Attribute itself."));
		}
		return element.Member.MemberType switch
		{
			MemberTypes.Method => InternalParamIsDefined(element, attributeType, inherit), 
			MemberTypes.Constructor => element.IsDefined(attributeType, inherit: false), 
			MemberTypes.Property => element.IsDefined(attributeType, inherit: false), 
			_ => throw new ArgumentException(Environment.GetResourceString("Invalid type for ParameterInfo member in Attribute class.")), 
		};
	}

	/// <summary>Retrieves a custom attribute applied to a method parameter. Parameters specify the method parameter, and the type of the custom attribute to search for.</summary>
	/// <returns>A reference to the single custom attribute of type <paramref name="attributeType" /> that is applied to <paramref name="element" />, or null if there is no such attribute.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.ParameterInfo" /> class that describes a parameter of a member of a class. </param>
	/// <param name="attributeType">The type, or a base type, of the custom attribute to search for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />. </exception>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found. </exception>
	/// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded. </exception>
	/// <filterpriority>1</filterpriority>
	public static Attribute GetCustomAttribute(ParameterInfo element, Type attributeType)
	{
		return GetCustomAttribute(element, attributeType, inherit: true);
	}

	/// <summary>Retrieves a custom attribute applied to a method parameter. Parameters specify the method parameter, the type of the custom attribute to search for, and whether to search ancestors of the method parameter.</summary>
	/// <returns>A reference to the single custom attribute of type <paramref name="attributeType" /> that is applied to <paramref name="element" />, or null if there is no such attribute.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.ParameterInfo" /> class that describes a parameter of a member of a class. </param>
	/// <param name="attributeType">The type, or a base type, of the custom attribute to search for.</param>
	/// <param name="inherit">If true, specifies to also search the ancestors of <paramref name="element" /> for custom attributes. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />. </exception>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found. </exception>
	/// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded. </exception>
	/// <filterpriority>1</filterpriority>
	public static Attribute GetCustomAttribute(ParameterInfo element, Type attributeType, bool inherit)
	{
		Attribute[] customAttributes = GetCustomAttributes(element, attributeType, inherit);
		if (customAttributes == null || customAttributes.Length == 0)
		{
			return null;
		}
		if (customAttributes.Length == 0)
		{
			return null;
		}
		if (customAttributes.Length == 1)
		{
			return customAttributes[0];
		}
		throw new AmbiguousMatchException(Environment.GetResourceString("Multiple custom attributes of the same type found."));
	}

	/// <summary>Retrieves an array of the custom attributes applied to a module. Parameters specify the module, and the type of the custom attribute to search for.</summary>
	/// <returns>An <see cref="T:System.Attribute" /> array that contains the custom attributes of type <paramref name="attributeType" /> applied to <paramref name="element" />, or an empty array if no such custom attributes exist.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.Module" /> class that describes a portable executable file. </param>
	/// <param name="attributeType">The type, or a base type, of the custom attribute to search for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static Attribute[] GetCustomAttributes(Module element, Type attributeType)
	{
		return GetCustomAttributes(element, attributeType, inherit: true);
	}

	/// <summary>Retrieves an array of the custom attributes applied to a module. A parameter specifies the module.</summary>
	/// <returns>An <see cref="T:System.Attribute" /> array that contains the custom attributes applied to <paramref name="element" />, or an empty array if no such custom attributes exist.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.Module" /> class that describes a portable executable file. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public static Attribute[] GetCustomAttributes(Module element)
	{
		return GetCustomAttributes(element, inherit: true);
	}

	/// <summary>Retrieves an array of the custom attributes applied to a module. Parameters specify the module, and an ignored search option.</summary>
	/// <returns>An <see cref="T:System.Attribute" /> array that contains the custom attributes applied to <paramref name="element" />, or an empty array if no such custom attributes exist.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.Module" /> class that describes a portable executable file. </param>
	/// <param name="inherit">This parameter is ignored, and does not affect the operation of this method. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="attributeType" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public static Attribute[] GetCustomAttributes(Module element, bool inherit)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (Attribute[])element.GetCustomAttributes(typeof(Attribute), inherit);
	}

	/// <summary>Retrieves an array of the custom attributes applied to a module. Parameters specify the module, the type of the custom attribute to search for, and an ignored search option.</summary>
	/// <returns>An <see cref="T:System.Attribute" /> array that contains the custom attributes of type <paramref name="attributeType" /> applied to <paramref name="element" />, or an empty array if no such custom attributes exist.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.Module" /> class that describes a portable executable file. </param>
	/// <param name="attributeType">The type, or a base type, of the custom attribute to search for.</param>
	/// <param name="inherit">This parameter is ignored, and does not affect the operation of this method. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static Attribute[] GetCustomAttributes(Module element, Type attributeType, bool inherit)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (attributeType == null)
		{
			throw new ArgumentNullException("attributeType");
		}
		if (!attributeType.IsSubclassOf(typeof(Attribute)) && attributeType != typeof(Attribute))
		{
			throw new ArgumentException(Environment.GetResourceString("Type passed in must be derived from System.Attribute or System.Attribute itself."));
		}
		return (Attribute[])element.GetCustomAttributes(attributeType, inherit);
	}

	/// <summary>Determines whether any custom attributes of a specified type are applied to a module. Parameters specify the module, and the type of the custom attribute to search for.</summary>
	/// <returns>true if a custom attribute of type <paramref name="attributeType" /> is applied to <paramref name="element" />; otherwise, false.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.Module" /> class that describes a portable executable file. </param>
	/// <param name="attributeType">The type, or a base type, of the custom attribute to search for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsDefined(Module element, Type attributeType)
	{
		return IsDefined(element, attributeType, inherit: false);
	}

	/// <summary>Determines whether any custom attributes are applied to a module. Parameters specify the module, the type of the custom attribute to search for, and an ignored search option. </summary>
	/// <returns>true if a custom attribute of type <paramref name="attributeType" /> is applied to <paramref name="element" />; otherwise, false.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.Module" /> class that describes a portable executable file. </param>
	/// <param name="attributeType">The type, or a base type, of the custom attribute to search for.</param>
	/// <param name="inherit">This parameter is ignored, and does not affect the operation of this method. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsDefined(Module element, Type attributeType, bool inherit)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (attributeType == null)
		{
			throw new ArgumentNullException("attributeType");
		}
		if (!attributeType.IsSubclassOf(typeof(Attribute)) && attributeType != typeof(Attribute))
		{
			throw new ArgumentException(Environment.GetResourceString("Type passed in must be derived from System.Attribute or System.Attribute itself."));
		}
		return element.IsDefined(attributeType, inherit: false);
	}

	/// <summary>Retrieves a custom attribute applied to a module. Parameters specify the module, and the type of the custom attribute to search for.</summary>
	/// <returns>A reference to the single custom attribute of type <paramref name="attributeType" /> that is applied to <paramref name="element" />, or null if there is no such attribute.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.Module" /> class that describes a portable executable file. </param>
	/// <param name="attributeType">The type, or a base type, of the custom attribute to search for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />. </exception>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found. </exception>
	/// <filterpriority>1</filterpriority>
	public static Attribute GetCustomAttribute(Module element, Type attributeType)
	{
		return GetCustomAttribute(element, attributeType, inherit: true);
	}

	/// <summary>Retrieves a custom attribute applied to a module. Parameters specify the module, the type of the custom attribute to search for, and an ignored search option.</summary>
	/// <returns>A reference to the single custom attribute of type <paramref name="attributeType" /> that is applied to <paramref name="element" />, or null if there is no such attribute.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.Module" /> class that describes a portable executable file. </param>
	/// <param name="attributeType">The type, or a base type, of the custom attribute to search for.</param>
	/// <param name="inherit">This parameter is ignored, and does not affect the operation of this method. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />. </exception>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found. </exception>
	/// <filterpriority>1</filterpriority>
	public static Attribute GetCustomAttribute(Module element, Type attributeType, bool inherit)
	{
		Attribute[] customAttributes = GetCustomAttributes(element, attributeType, inherit);
		if (customAttributes == null || customAttributes.Length == 0)
		{
			return null;
		}
		if (customAttributes.Length == 1)
		{
			return customAttributes[0];
		}
		throw new AmbiguousMatchException(Environment.GetResourceString("Multiple custom attributes of the same type found."));
	}

	/// <summary>Retrieves an array of the custom attributes applied to an assembly. Parameters specify the assembly, and the type of the custom attribute to search for.</summary>
	/// <returns>An <see cref="T:System.Attribute" /> array that contains the custom attributes of type <paramref name="attributeType" /> applied to <paramref name="element" />, or an empty array if no such custom attributes exist.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.Assembly" /> class that describes a reusable collection of modules. </param>
	/// <param name="attributeType">The type, or a base type, of the custom attribute to search for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static Attribute[] GetCustomAttributes(Assembly element, Type attributeType)
	{
		return GetCustomAttributes(element, attributeType, inherit: true);
	}

	/// <summary>Retrieves an array of the custom attributes applied to an assembly. Parameters specify the assembly, the type of the custom attribute to search for, and an ignored search option.</summary>
	/// <returns>An <see cref="T:System.Attribute" /> array that contains the custom attributes of type <paramref name="attributeType" /> applied to <paramref name="element" />, or an empty array if no such custom attributes exist.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.Assembly" /> class that describes a reusable collection of modules. </param>
	/// <param name="attributeType">The type, or a base type, of the custom attribute to search for.</param>
	/// <param name="inherit">This parameter is ignored, and does not affect the operation of this method. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static Attribute[] GetCustomAttributes(Assembly element, Type attributeType, bool inherit)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (attributeType == null)
		{
			throw new ArgumentNullException("attributeType");
		}
		if (!attributeType.IsSubclassOf(typeof(Attribute)) && attributeType != typeof(Attribute))
		{
			throw new ArgumentException(Environment.GetResourceString("Type passed in must be derived from System.Attribute or System.Attribute itself."));
		}
		return (Attribute[])element.GetCustomAttributes(attributeType, inherit);
	}

	/// <summary>Retrieves an array of the custom attributes applied to an assembly. A parameter specifies the assembly.</summary>
	/// <returns>An <see cref="T:System.Attribute" /> array that contains the custom attributes applied to <paramref name="element" />, or an empty array if no such custom attributes exist.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.Assembly" /> class that describes a reusable collection of modules. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public static Attribute[] GetCustomAttributes(Assembly element)
	{
		return GetCustomAttributes(element, inherit: true);
	}

	/// <summary>Retrieves an array of the custom attributes applied to an assembly. Parameters specify the assembly, and an ignored search option.</summary>
	/// <returns>An <see cref="T:System.Attribute" /> array that contains the custom attributes applied to <paramref name="element" />, or an empty array if no such custom attributes exist.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.Assembly" /> class that describes a reusable collection of modules. </param>
	/// <param name="inherit">This parameter is ignored, and does not affect the operation of this method. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="attributeType" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public static Attribute[] GetCustomAttributes(Assembly element, bool inherit)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (Attribute[])element.GetCustomAttributes(typeof(Attribute), inherit);
	}

	/// <summary>Determines whether any custom attributes are applied to an assembly. Parameters specify the assembly, and the type of the custom attribute to search for.</summary>
	/// <returns>true if a custom attribute of type <paramref name="attributeType" /> is applied to <paramref name="element" />; otherwise, false.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.Assembly" /> class that describes a reusable collection of modules. </param>
	/// <param name="attributeType">The type, or a base type, of the custom attribute to search for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsDefined(Assembly element, Type attributeType)
	{
		return IsDefined(element, attributeType, inherit: true);
	}

	/// <summary>Determines whether any custom attributes are applied to an assembly. Parameters specify the assembly, the type of the custom attribute to search for, and an ignored search option.</summary>
	/// <returns>true if a custom attribute of type <paramref name="attributeType" /> is applied to <paramref name="element" />; otherwise, false.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.Assembly" /> class that describes a reusable collection of modules. </param>
	/// <param name="attributeType">The type, or a base type, of the custom attribute to search for.</param>
	/// <param name="inherit">This parameter is ignored, and does not affect the operation of this method. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsDefined(Assembly element, Type attributeType, bool inherit)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (attributeType == null)
		{
			throw new ArgumentNullException("attributeType");
		}
		if (!attributeType.IsSubclassOf(typeof(Attribute)) && attributeType != typeof(Attribute))
		{
			throw new ArgumentException(Environment.GetResourceString("Type passed in must be derived from System.Attribute or System.Attribute itself."));
		}
		return element.IsDefined(attributeType, inherit: false);
	}

	/// <summary>Retrieves a custom attribute applied to a specified assembly. Parameters specify the assembly and the type of the custom attribute to search for.</summary>
	/// <returns>A reference to the single custom attribute of type <paramref name="attributeType" /> that is applied to <paramref name="element" />, or null if there is no such attribute.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.Assembly" /> class that describes a reusable collection of modules. </param>
	/// <param name="attributeType">The type, or a base type, of the custom attribute to search for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />. </exception>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found. </exception>
	/// <filterpriority>1</filterpriority>
	public static Attribute GetCustomAttribute(Assembly element, Type attributeType)
	{
		return GetCustomAttribute(element, attributeType, inherit: true);
	}

	/// <summary>Retrieves a custom attribute applied to an assembly. Parameters specify the assembly, the type of the custom attribute to search for, and an ignored search option.</summary>
	/// <returns>A reference to the single custom attribute of type <paramref name="attributeType" /> that is applied to <paramref name="element" />, or null if there is no such attribute.</returns>
	/// <param name="element">An object derived from the <see cref="T:System.Reflection.Assembly" /> class that describes a reusable collection of modules. </param>
	/// <param name="attributeType">The type, or a base type, of the custom attribute to search for.</param>
	/// <param name="inherit">This parameter is ignored, and does not affect the operation of this method. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />. </exception>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found. </exception>
	/// <filterpriority>1</filterpriority>
	public static Attribute GetCustomAttribute(Assembly element, Type attributeType, bool inherit)
	{
		Attribute[] customAttributes = GetCustomAttributes(element, attributeType, inherit);
		if (customAttributes == null || customAttributes.Length == 0)
		{
			return null;
		}
		if (customAttributes.Length == 1)
		{
			return customAttributes[0];
		}
		throw new AmbiguousMatchException(Environment.GetResourceString("Multiple custom attributes of the same type found."));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Attribute" /> class.</summary>
	protected Attribute()
	{
	}

	/// <summary>Returns a value that indicates whether this instance is equal to a specified object.</summary>
	/// <returns>true if <paramref name="obj" /> equals the type and value of this instance; otherwise, false.</returns>
	/// <param name="obj">An <see cref="T:System.Object" /> to compare with this instance or null. </param>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		RuntimeType runtimeType = (RuntimeType)GetType();
		if ((RuntimeType)obj.GetType() != runtimeType)
		{
			return false;
		}
		FieldInfo[] fields = runtimeType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		for (int i = 0; i < fields.Length; i++)
		{
			object thisValue = ((RtFieldInfo)fields[i]).UnsafeGetValue(this);
			object thatValue = ((RtFieldInfo)fields[i]).UnsafeGetValue(obj);
			if (!AreFieldValuesEqual(thisValue, thatValue))
			{
				return false;
			}
		}
		return true;
	}

	private static bool AreFieldValuesEqual(object thisValue, object thatValue)
	{
		if (thisValue == null && thatValue == null)
		{
			return true;
		}
		if (thisValue == null || thatValue == null)
		{
			return false;
		}
		if (thisValue.GetType().IsArray)
		{
			if (!thisValue.GetType().Equals(thatValue.GetType()))
			{
				return false;
			}
			Array array = thisValue as Array;
			Array array2 = thatValue as Array;
			if (array.Length != array2.Length)
			{
				return false;
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (!AreFieldValuesEqual(array.GetValue(i), array2.GetValue(i)))
				{
					return false;
				}
			}
		}
		else if (!thisValue.Equals(thatValue))
		{
			return false;
		}
		return true;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	public override int GetHashCode()
	{
		Type type = GetType();
		FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		object obj = null;
		for (int i = 0; i < fields.Length; i++)
		{
			object obj2 = ((RtFieldInfo)fields[i]).UnsafeGetValue(this);
			if (obj2 != null && !obj2.GetType().IsArray)
			{
				obj = obj2;
			}
			if (obj != null)
			{
				break;
			}
		}
		return obj?.GetHashCode() ?? type.GetHashCode();
	}

	/// <summary>When overridden in a derived class, returns a value that indicates whether this instance equals a specified object.</summary>
	/// <returns>true if this instance equals <paramref name="obj" />; otherwise, false.</returns>
	/// <param name="obj">An <see cref="T:System.Object" /> to compare with this instance of <see cref="T:System.Attribute" />. </param>
	/// <filterpriority>2</filterpriority>
	public virtual bool Match(object obj)
	{
		return Equals(obj);
	}

	/// <summary>When overridden in a derived class, indicates whether the value of this instance is the default value for the derived class.</summary>
	/// <returns>true if this instance is the default attribute for the class; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsDefaultAttribute()
	{
		return false;
	}

	/// <summary>Retrieves the number of type information interfaces that an object provides (either 0 or 1).</summary>
	/// <param name="pcTInfo">Points to a location that receives the number of type information interfaces provided by the object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _Attribute.GetTypeInfoCount(out uint pcTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the type information for an object, which can be used to get the type information for an interface.</summary>
	/// <param name="iTInfo">The type information to return.</param>
	/// <param name="lcid">The locale identifier for the type information.</param>
	/// <param name="ppTInfo">Receives a pointer to the requested type information object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _Attribute.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Maps a set of names to a corresponding set of dispatch identifiers.</summary>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="rgszNames">Passed-in array of names to be mapped.</param>
	/// <param name="cNames">Count of the names to be mapped.</param>
	/// <param name="lcid">The locale context in which to interpret the names.</param>
	/// <param name="rgDispId">Caller-allocated array that receives the IDs corresponding to the names.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _Attribute.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
	{
		throw new NotImplementedException();
	}

	/// <summary>Provides access to properties and methods exposed by an object.</summary>
	/// <param name="dispIdMember">Identifies the member.</param>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="lcid">The locale context in which to interpret arguments.</param>
	/// <param name="wFlags">Flags describing the context of the call.</param>
	/// <param name="pDispParams">Pointer to a structure containing an array of arguments, an array of argument DISPIDs for named arguments, and counts for the number of elements in the arrays.</param>
	/// <param name="pVarResult">Pointer to the location where the result is to be stored.</param>
	/// <param name="pExcepInfo">Pointer to a structure that contains exception information.</param>
	/// <param name="puArgErr">The index of the first argument that has an error.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _Attribute.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
	{
		throw new NotImplementedException();
	}
}
