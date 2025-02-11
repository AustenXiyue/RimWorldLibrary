using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Reflection;

/// <summary>Provides access to custom attribute data for assemblies, modules, types, members and parameters that are loaded into the reflection-only context.</summary>
[Serializable]
[ComVisible(true)]
public class CustomAttributeData
{
	private class LazyCAttrData
	{
		internal Assembly assembly;

		internal IntPtr data;

		internal uint data_length;
	}

	private ConstructorInfo ctorInfo;

	private IList<CustomAttributeTypedArgument> ctorArgs;

	private IList<CustomAttributeNamedArgument> namedArgs;

	private LazyCAttrData lazyData;

	/// <summary>Gets a <see cref="T:System.Reflection.ConstructorInfo" /> object that represents the constructor that would have initialized the custom attribute.</summary>
	/// <returns>An object that represents the constructor that would have initialized the custom attribute represented by the current instance of the <see cref="T:System.Reflection.CustomAttributeData" /> class.</returns>
	[ComVisible(true)]
	public virtual ConstructorInfo Constructor => ctorInfo;

	/// <summary>Gets the list of positional arguments specified for the attribute instance represented by the <see cref="T:System.Reflection.CustomAttributeData" /> object.</summary>
	/// <returns>A collection of structures that represent the positional arguments specified for the custom attribute instance.</returns>
	[ComVisible(true)]
	public virtual IList<CustomAttributeTypedArgument> ConstructorArguments
	{
		get
		{
			ResolveArguments();
			return ctorArgs;
		}
	}

	/// <summary>Gets the list of named arguments specified for the attribute instance represented by the <see cref="T:System.Reflection.CustomAttributeData" /> object.</summary>
	/// <returns>A collection of structures that represent the named arguments specified for the custom attribute instance.</returns>
	public virtual IList<CustomAttributeNamedArgument> NamedArguments
	{
		get
		{
			ResolveArguments();
			return namedArgs;
		}
	}

	/// <summary>Gets the type of the attribute.</summary>
	/// <returns>The type of the attribute.</returns>
	public Type AttributeType => ctorInfo.DeclaringType;

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.CustomAttributeData" /> class.</summary>
	protected CustomAttributeData()
	{
	}

	internal CustomAttributeData(ConstructorInfo ctorInfo, Assembly assembly, IntPtr data, uint data_length)
	{
		this.ctorInfo = ctorInfo;
		lazyData = new LazyCAttrData();
		lazyData.assembly = assembly;
		lazyData.data = data;
		lazyData.data_length = data_length;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void ResolveArgumentsInternal(ConstructorInfo ctor, Assembly assembly, IntPtr data, uint data_length, out object[] ctorArgs, out object[] namedArgs);

	private void ResolveArguments()
	{
		if (lazyData != null)
		{
			ResolveArgumentsInternal(ctorInfo, lazyData.assembly, lazyData.data, lazyData.data_length, out var array, out var array2);
			ctorArgs = Array.AsReadOnly((array != null) ? UnboxValues<CustomAttributeTypedArgument>(array) : EmptyArray<CustomAttributeTypedArgument>.Value);
			namedArgs = Array.AsReadOnly((array2 != null) ? UnboxValues<CustomAttributeNamedArgument>(array2) : EmptyArray<CustomAttributeNamedArgument>.Value);
			lazyData = null;
		}
	}

	/// <summary>Returns a list of <see cref="T:System.Reflection.CustomAttributeData" /> objects representing data about the attributes that have been applied to the target assembly.</summary>
	/// <returns>A list of objects that represent data about the attributes that have been applied to the target assembly.</returns>
	/// <param name="target">The assembly whose custom attribute data is to be retrieved.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="target" /> is null.</exception>
	public static IList<CustomAttributeData> GetCustomAttributes(Assembly target)
	{
		return MonoCustomAttrs.GetCustomAttributesData(target);
	}

	/// <summary>Returns a list of <see cref="T:System.Reflection.CustomAttributeData" /> objects representing data about the attributes that have been applied to the target member.</summary>
	/// <returns>A list of objects that represent data about the attributes that have been applied to the target member.</returns>
	/// <param name="target">The member whose attribute data is to be retrieved.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="target" /> is null.</exception>
	public static IList<CustomAttributeData> GetCustomAttributes(MemberInfo target)
	{
		return MonoCustomAttrs.GetCustomAttributesData(target);
	}

	internal static IList<CustomAttributeData> GetCustomAttributesInternal(RuntimeType target)
	{
		return MonoCustomAttrs.GetCustomAttributesData(target);
	}

	/// <summary>Returns a list of <see cref="T:System.Reflection.CustomAttributeData" /> objects representing data about the attributes that have been applied to the target module.</summary>
	/// <returns>A list of objects that represent data about the attributes that have been applied to the target module.</returns>
	/// <param name="target">The module whose custom attribute data is to be retrieved.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="target" /> is null.</exception>
	public static IList<CustomAttributeData> GetCustomAttributes(Module target)
	{
		return MonoCustomAttrs.GetCustomAttributesData(target);
	}

	/// <summary>Returns a list of <see cref="T:System.Reflection.CustomAttributeData" /> objects representing data about the attributes that have been applied to the target parameter.</summary>
	/// <returns>A list of objects that represent data about the attributes that have been applied to the target parameter.</returns>
	/// <param name="target">The parameter whose attribute data is to be retrieved.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="target" /> is null.</exception>
	public static IList<CustomAttributeData> GetCustomAttributes(ParameterInfo target)
	{
		return MonoCustomAttrs.GetCustomAttributesData(target);
	}

	/// <summary>Returns a string representation of the custom attribute.</summary>
	/// <returns>A string value that represents the custom attribute.</returns>
	public override string ToString()
	{
		ResolveArguments();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[" + ctorInfo.DeclaringType.FullName + "(");
		for (int i = 0; i < ctorArgs.Count; i++)
		{
			stringBuilder.Append(ctorArgs[i].ToString());
			if (i + 1 < ctorArgs.Count)
			{
				stringBuilder.Append(", ");
			}
		}
		if (namedArgs.Count > 0)
		{
			stringBuilder.Append(", ");
		}
		for (int j = 0; j < namedArgs.Count; j++)
		{
			stringBuilder.Append(namedArgs[j].ToString());
			if (j + 1 < namedArgs.Count)
			{
				stringBuilder.Append(", ");
			}
		}
		stringBuilder.AppendFormat(")]");
		return stringBuilder.ToString();
	}

	private static T[] UnboxValues<T>(object[] values)
	{
		T[] array = new T[values.Length];
		for (int i = 0; i < values.Length; i++)
		{
			array[i] = (T)values[i];
		}
		return array;
	}

	/// <summary>Returns a value that indicates whether this instance is equal to a specified object.</summary>
	/// <returns>true if <paramref name="obj" /> is equal to the current instance; otherwise, false.</returns>
	/// <param name="obj">An object to compare with this instance, or null.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is CustomAttributeData customAttributeData) || customAttributeData.ctorInfo != ctorInfo || customAttributeData.ctorArgs.Count != ctorArgs.Count || customAttributeData.namedArgs.Count != namedArgs.Count)
		{
			return false;
		}
		for (int i = 0; i < ctorArgs.Count; i++)
		{
			if (ctorArgs[i].Equals(customAttributeData.ctorArgs[i]))
			{
				return false;
			}
		}
		for (int j = 0; j < namedArgs.Count; j++)
		{
			bool flag = false;
			for (int k = 0; k < customAttributeData.namedArgs.Count; k++)
			{
				if (namedArgs[j].Equals(customAttributeData.namedArgs[k]))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		int num = ((ctorInfo == null) ? 13 : (ctorInfo.GetHashCode() << 16));
		if (ctorArgs != null)
		{
			for (int i = 0; i < ctorArgs.Count; i++)
			{
				num += num ^ (7 + ctorArgs[i].GetHashCode() << i * 4);
			}
		}
		if (namedArgs != null)
		{
			for (int j = 0; j < namedArgs.Count; j++)
			{
				num += namedArgs[j].GetHashCode() << 5;
			}
		}
		return num;
	}
}
