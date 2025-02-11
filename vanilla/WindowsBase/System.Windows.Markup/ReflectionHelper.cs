using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using MS.Internal;
using MS.Internal.WindowsBase;

namespace System.Windows.Markup;

internal static class ReflectionHelper
{
	private const string SystemReflectionAssemblyName = "System";

	internal const string MscorlibReflectionAssemblyName = "mscorlib";

	private static Hashtable _loadedAssembliesHash = new Hashtable(8);

	internal static Type GetQualifiedType(string typeName)
	{
		string[] array = typeName.Split(new char[1] { ',' }, 2);
		if (array.Length == 1)
		{
			return Type.GetType(array[0]);
		}
		Assembly assembly = null;
		try
		{
			assembly = LoadAssembly(array[1].TrimStart(), null);
		}
		catch (Exception ex) when (!CriticalExceptions.IsCriticalException(ex))
		{
		}
		if (assembly == null)
		{
			return null;
		}
		try
		{
			return assembly.GetType(array[0]);
		}
		catch (ArgumentException)
		{
		}
		return null;
	}

	internal static bool IsNullableType(Type type)
	{
		if (type.IsGenericType)
		{
			return type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}
		return false;
	}

	internal static bool IsInternalType(Type type)
	{
		Type type2 = type;
		while (type.IsNestedAssembly || type.IsNestedFamORAssem || (type2 != type && type.IsNestedPublic))
		{
			type = type.DeclaringType;
		}
		if (!type.IsNotPublic)
		{
			if (type2 != type)
			{
				return type.IsPublic;
			}
			return false;
		}
		return true;
	}

	internal static bool IsPublicType(Type type)
	{
		while (type.IsNestedPublic)
		{
			type = type.DeclaringType;
		}
		return type.IsPublic;
	}

	internal static Type GetFrameworkType(string assemblyName, Type type)
	{
		return type;
	}

	internal static Type GetSystemType(Type type)
	{
		return GetFrameworkType("System", type);
	}

	internal static Type GetReflectionType(object item)
	{
		if (item == null)
		{
			return null;
		}
		if (!(item is ICustomTypeProvider customTypeProvider))
		{
			return item.GetType();
		}
		return customTypeProvider.GetCustomType();
	}

	internal static string GetTypeConverterAttributeData(Type type, out Type converterType)
	{
		bool attributeDataFound = false;
		return GetCustomAttributeData(type, GetSystemType(typeof(TypeConverterAttribute)), allowTypeAlso: true, ref attributeDataFound, out converterType);
	}

	internal static string GetTypeConverterAttributeData(MemberInfo mi, out Type converterType)
	{
		return GetCustomAttributeData(mi, GetSystemType(typeof(TypeConverterAttribute)), out converterType);
	}

	private static string GetCustomAttributeData(MemberInfo mi, Type attrType, out Type typeValue)
	{
		string customAttributeData = GetCustomAttributeData(CustomAttributeData.GetCustomAttributes(mi), attrType, out typeValue, allowTypeAlso: true, allowZeroArgs: false);
		if (customAttributeData != null)
		{
			return customAttributeData;
		}
		return string.Empty;
	}

	private static string GetCustomAttributeData(IList<CustomAttributeData> list, Type attrType, out Type typeValue, bool allowTypeAlso, bool allowZeroArgs)
	{
		typeValue = null;
		string text = null;
		for (int i = 0; i < list.Count; i++)
		{
			text = GetCustomAttributeData(list[i], attrType, out typeValue, allowTypeAlso, noArgs: false, allowZeroArgs);
			if (text != null)
			{
				break;
			}
		}
		return text;
	}

	internal static string GetCustomAttributeData(Type t, Type attrType, bool allowTypeAlso, ref bool attributeDataFound, out Type typeValue)
	{
		typeValue = null;
		attributeDataFound = false;
		Type type = t;
		string result = null;
		while (type != null && !attributeDataFound)
		{
			IList<CustomAttributeData> customAttributes = CustomAttributeData.GetCustomAttributes(type);
			for (int i = 0; i < customAttributes.Count; i++)
			{
				if (attributeDataFound)
				{
					break;
				}
				CustomAttributeData customAttributeData = customAttributes[i];
				if (customAttributeData.Constructor.ReflectedType == attrType)
				{
					attributeDataFound = true;
					result = GetCustomAttributeData(customAttributeData, attrType, out typeValue, allowTypeAlso, noArgs: false, zeroArgsAllowed: false);
				}
			}
			if (!attributeDataFound)
			{
				type = type.BaseType;
			}
		}
		return result;
	}

	private static string GetCustomAttributeData(CustomAttributeData cad, Type attrType, out Type typeValue, bool allowTypeAlso, bool noArgs, bool zeroArgsAllowed)
	{
		string text = null;
		typeValue = null;
		if (cad.Constructor.ReflectedType == attrType)
		{
			IList<CustomAttributeTypedArgument> constructorArguments = cad.ConstructorArguments;
			if (constructorArguments.Count == 1 && !noArgs)
			{
				CustomAttributeTypedArgument customAttributeTypedArgument = constructorArguments[0];
				text = customAttributeTypedArgument.Value as string;
				if (text == null && allowTypeAlso && customAttributeTypedArgument.ArgumentType == typeof(Type))
				{
					typeValue = customAttributeTypedArgument.Value as Type;
					text = typeValue.AssemblyQualifiedName;
				}
				if (text == null)
				{
					throw new ArgumentException(SR.Format(SR.ParserAttributeArgsLow, attrType.Name));
				}
			}
			else
			{
				if (constructorArguments.Count != 0)
				{
					throw new ArgumentException(SR.Format(SR.ParserAttributeArgsHigh, attrType.Name));
				}
				if (!(noArgs || zeroArgsAllowed))
				{
					throw new ArgumentException(SR.Format(SR.ParserAttributeArgsLow, attrType.Name));
				}
				text = string.Empty;
			}
		}
		return text;
	}

	internal static void ResetCacheForAssembly(string assemblyName)
	{
		string key = assemblyName.ToUpper(CultureInfo.InvariantCulture);
		_loadedAssembliesHash[key] = null;
	}

	internal static Assembly LoadAssembly(string assemblyName, string assemblyPath)
	{
		return LoadAssemblyHelper(assemblyName, assemblyPath);
	}

	internal static Assembly GetAlreadyLoadedAssembly(string assemblyNameLookup)
	{
		return (Assembly)_loadedAssembliesHash[assemblyNameLookup];
	}

	private static Assembly LoadAssemblyHelper(string assemblyGivenName, string assemblyPath)
	{
		AssemblyName assemblyName = new AssemblyName(assemblyGivenName);
		string name = assemblyName.Name;
		name = name.ToUpper(CultureInfo.InvariantCulture);
		Assembly assembly = (Assembly)_loadedAssembliesHash[name];
		if (assembly != null)
		{
			if (assemblyName.Version != null)
			{
				AssemblyName assemblyName2 = new AssemblyName(assembly.FullName);
				if (!AssemblyName.ReferenceMatchesDefinition(assemblyName, assemblyName2))
				{
					string p = assemblyName.ToString();
					string p2 = assemblyName2.ToString();
					throw new InvalidOperationException(SR.Format(SR.ParserAssemblyLoadVersionMismatch, p, p2));
				}
			}
		}
		else
		{
			if (string.IsNullOrEmpty(assemblyPath))
			{
				assembly = SafeSecurityHelper.GetLoadedAssembly(assemblyName);
			}
			if (assembly == null)
			{
				if (!string.IsNullOrEmpty(assemblyPath))
				{
					assembly = Assembly.LoadFile(assemblyPath);
				}
				else
				{
					try
					{
						assembly = Assembly.Load(assemblyGivenName);
					}
					catch (FileNotFoundException)
					{
						assembly = null;
					}
				}
			}
			if (assembly != null)
			{
				_loadedAssembliesHash[name] = assembly;
			}
		}
		return assembly;
	}
}
