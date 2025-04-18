using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters;
using System.Security;
using System.Text;

namespace System.Runtime.Serialization;

/// <summary>Provides static methods to aid with the implementation of a <see cref="T:System.Runtime.Serialization.Formatter" /> for serialization. This class cannot be inherited.</summary>
[ComVisible(true)]
public static class FormatterServices
{
	internal static ConcurrentDictionary<MemberHolder, MemberInfo[]> m_MemberInfoTable;

	[SecurityCritical]
	private static bool unsafeTypeForwardersIsEnabled;

	[SecurityCritical]
	private static volatile bool unsafeTypeForwardersIsEnabledInitialized;

	private static readonly Type[] advancedTypes;

	private static Binder s_binder;

	[SecuritySafeCritical]
	static FormatterServices()
	{
		m_MemberInfoTable = new ConcurrentDictionary<MemberHolder, MemberInfo[]>();
		unsafeTypeForwardersIsEnabled = false;
		unsafeTypeForwardersIsEnabledInitialized = false;
		advancedTypes = new Type[4]
		{
			typeof(DelegateSerializationHolder),
			typeof(ObjRef),
			typeof(IEnvoyInfo),
			typeof(ISponsor)
		};
		s_binder = Type.DefaultBinder;
	}

	private static MemberInfo[] GetSerializableMembers(RuntimeType type)
	{
		FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		int num = 0;
		for (int i = 0; i < fields.Length; i++)
		{
			if ((fields[i].Attributes & FieldAttributes.NotSerialized) != FieldAttributes.NotSerialized)
			{
				num++;
			}
		}
		if (num != fields.Length)
		{
			FieldInfo[] array = new FieldInfo[num];
			num = 0;
			for (int j = 0; j < fields.Length; j++)
			{
				if ((fields[j].Attributes & FieldAttributes.NotSerialized) != FieldAttributes.NotSerialized)
				{
					array[num] = fields[j];
					num++;
				}
			}
			return array;
		}
		return fields;
	}

	private static bool CheckSerializable(RuntimeType type)
	{
		if (type.IsSerializable)
		{
			return true;
		}
		return false;
	}

	private static MemberInfo[] InternalGetSerializableMembers(RuntimeType type)
	{
		List<SerializationFieldInfo> list = null;
		if (type.IsInterface)
		{
			return new MemberInfo[0];
		}
		if (!CheckSerializable(type))
		{
			throw new SerializationException(Environment.GetResourceString("Type '{0}' in Assembly '{1}' is not marked as serializable.", type.FullName, type.Module.Assembly.FullName));
		}
		MemberInfo[] array = GetSerializableMembers(type);
		RuntimeType runtimeType = (RuntimeType)type.BaseType;
		if (runtimeType != null && runtimeType != (RuntimeType)typeof(object))
		{
			RuntimeType[] parentTypes = null;
			int parentTypeCount = 0;
			bool parentTypes2 = GetParentTypes(runtimeType, out parentTypes, out parentTypeCount);
			if (parentTypeCount > 0)
			{
				list = new List<SerializationFieldInfo>();
				for (int i = 0; i < parentTypeCount; i++)
				{
					runtimeType = parentTypes[i];
					if (!CheckSerializable(runtimeType))
					{
						throw new SerializationException(Environment.GetResourceString("Type '{0}' in Assembly '{1}' is not marked as serializable.", runtimeType.FullName, runtimeType.Module.Assembly.FullName));
					}
					FieldInfo[] fields = runtimeType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
					string namePrefix = (parentTypes2 ? runtimeType.Name : runtimeType.FullName);
					FieldInfo[] array2 = fields;
					foreach (FieldInfo fieldInfo in array2)
					{
						if (!fieldInfo.IsNotSerialized)
						{
							list.Add(new SerializationFieldInfo((RuntimeFieldInfo)fieldInfo, namePrefix));
						}
					}
				}
				if (list != null && list.Count > 0)
				{
					MemberInfo[] array3 = new MemberInfo[list.Count + array.Length];
					Array.Copy(array, array3, array.Length);
					((ICollection)list).CopyTo((Array)array3, array.Length);
					array = array3;
				}
			}
		}
		return array;
	}

	private static bool GetParentTypes(RuntimeType parentType, out RuntimeType[] parentTypes, out int parentTypeCount)
	{
		parentTypes = null;
		parentTypeCount = 0;
		bool flag = true;
		RuntimeType runtimeType = (RuntimeType)typeof(object);
		RuntimeType runtimeType2 = parentType;
		while (runtimeType2 != runtimeType)
		{
			if (!runtimeType2.IsInterface)
			{
				string name = runtimeType2.Name;
				int num = 0;
				while (flag && num < parentTypeCount)
				{
					string name2 = parentTypes[num].Name;
					if (name2.Length == name.Length && name2[0] == name[0] && name == name2)
					{
						flag = false;
						break;
					}
					num++;
				}
				if (parentTypes == null || parentTypeCount == parentTypes.Length)
				{
					RuntimeType[] array = new RuntimeType[Math.Max(parentTypeCount * 2, 12)];
					if (parentTypes != null)
					{
						Array.Copy(parentTypes, 0, array, 0, parentTypeCount);
					}
					parentTypes = array;
				}
				parentTypes[parentTypeCount++] = runtimeType2;
			}
			runtimeType2 = (RuntimeType)runtimeType2.BaseType;
		}
		return flag;
	}

	/// <summary>Gets all the serializable members for a class of the specified <see cref="T:System.Type" />.</summary>
	/// <returns>An array of type <see cref="T:System.Reflection.MemberInfo" /> of the non-transient, non-static members.</returns>
	/// <param name="type">The type being serialized. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="type" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
	/// </PermissionSet>
	[SecurityCritical]
	public static MemberInfo[] GetSerializableMembers(Type type)
	{
		return GetSerializableMembers(type, new StreamingContext(StreamingContextStates.All));
	}

	/// <summary>Gets all the serializable members for a class of the specified <see cref="T:System.Type" /> and in the provided <see cref="T:System.Runtime.Serialization.StreamingContext" />.</summary>
	/// <returns>An array of type <see cref="T:System.Reflection.MemberInfo" /> of the non-transient, non-static members.</returns>
	/// <param name="type">The type being serialized or cloned. </param>
	/// <param name="context">The context where the serialization occurs. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="type" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
	/// </PermissionSet>
	[SecurityCritical]
	public static MemberInfo[] GetSerializableMembers(Type type, StreamingContext context)
	{
		if ((object)type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (!(type is RuntimeType))
		{
			throw new SerializationException(Environment.GetResourceString("Only system-provided types can be passed to the GetUninitializedObject method. '{0}' is not a valid instance of a type.", type.ToString()));
		}
		MemberHolder key = new MemberHolder(type, context);
		return m_MemberInfoTable.GetOrAdd(key, (MemberHolder _) => InternalGetSerializableMembers((RuntimeType)type));
	}

	/// <summary>Determines whether the specified <see cref="T:System.Type" /> can be deserialized with the <see cref="T:System.Runtime.Serialization.Formatters.TypeFilterLevel" /> property set to Low.</summary>
	/// <param name="t">The <see cref="T:System.Type" /> to check for the ability to deserialize. </param>
	/// <param name="securityLevel">The <see cref="T:System.Runtime.Serialization.Formatters.TypeFilterLevel" /> property value. </param>
	/// <exception cref="T:System.Security.SecurityException">The <paramref name="t" /> parameter is an advanced type and cannot be deserialized when the <see cref="T:System.Runtime.Serialization.Formatters.TypeFilterLevel" /> property is set to Low. </exception>
	public static void CheckTypeSecurity(Type t, TypeFilterLevel securityLevel)
	{
		if (securityLevel != TypeFilterLevel.Low)
		{
			return;
		}
		for (int i = 0; i < advancedTypes.Length; i++)
		{
			if (advancedTypes[i].IsAssignableFrom(t))
			{
				throw new SecurityException(Environment.GetResourceString("Type {0} and the types derived from it (such as {1}) are not permitted to be deserialized at this security level.", advancedTypes[i].FullName, t.FullName));
			}
		}
	}

	/// <summary>Creates a new instance of the specified object type.</summary>
	/// <returns>A zeroed object of the specified type.</returns>
	/// <param name="type">The type of object to create. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="type" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
	/// </PermissionSet>
	[SecurityCritical]
	public static object GetUninitializedObject(Type type)
	{
		if ((object)type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (!(type is RuntimeType))
		{
			throw new SerializationException(Environment.GetResourceString("Only system-provided types can be passed to the GetUninitializedObject method. '{0}' is not a valid instance of a type.", type.ToString()));
		}
		return nativeGetUninitializedObject((RuntimeType)type);
	}

	/// <summary>Creates a new instance of the specified object type.</summary>
	/// <returns>A zeroed object of the specified type.</returns>
	/// <param name="type">The type of object to create. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="type" /> parameter is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">The <paramref name="type" /> parameter is not a valid common language runtime type. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
	/// </PermissionSet>
	[SecurityCritical]
	public static object GetSafeUninitializedObject(Type type)
	{
		if ((object)type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (!(type is RuntimeType))
		{
			throw new SerializationException(Environment.GetResourceString("Only system-provided types can be passed to the GetUninitializedObject method. '{0}' is not a valid instance of a type.", type.ToString()));
		}
		if ((object)type == typeof(ConstructionCall) || (object)type == typeof(LogicalCallContext) || (object)type == typeof(SynchronizationAttribute))
		{
			return nativeGetUninitializedObject((RuntimeType)type);
		}
		try
		{
			return nativeGetSafeUninitializedObject((RuntimeType)type);
		}
		catch (SecurityException innerException)
		{
			throw new SerializationException(Environment.GetResourceString("Because of security restrictions, the type {0} cannot be accessed.", type.FullName), innerException);
		}
	}

	private static object nativeGetUninitializedObject(RuntimeType type)
	{
		return ActivationServices.AllocateUninitializedClassInstance(type);
	}

	private static object nativeGetSafeUninitializedObject(RuntimeType type)
	{
		return ActivationServices.AllocateUninitializedClassInstance(type);
	}

	private static bool GetEnableUnsafeTypeForwarders()
	{
		return false;
	}

	[SecuritySafeCritical]
	internal static bool UnsafeTypeForwardersIsEnabled()
	{
		if (!unsafeTypeForwardersIsEnabledInitialized)
		{
			unsafeTypeForwardersIsEnabled = GetEnableUnsafeTypeForwarders();
			unsafeTypeForwardersIsEnabledInitialized = true;
		}
		return unsafeTypeForwardersIsEnabled;
	}

	[SecurityCritical]
	internal static void SerializationSetValue(MemberInfo fi, object target, object value)
	{
		RtFieldInfo rtFieldInfo = fi as RtFieldInfo;
		if (rtFieldInfo != null)
		{
			rtFieldInfo.CheckConsistency(target);
			rtFieldInfo.UnsafeSetValue(target, value, BindingFlags.Default, s_binder, null);
			return;
		}
		SerializationFieldInfo serializationFieldInfo = fi as SerializationFieldInfo;
		if (serializationFieldInfo != null)
		{
			serializationFieldInfo.InternalSetValue(target, value, BindingFlags.Default, s_binder, null);
			return;
		}
		throw new ArgumentException(Environment.GetResourceString("The FieldInfo object is not valid."));
	}

	/// <summary>Populates the specified object with values for each field drawn from the data array of objects.</summary>
	/// <returns>The newly populated object.</returns>
	/// <param name="obj">The object to populate. </param>
	/// <param name="members">An array of <see cref="T:System.Reflection.MemberInfo" /> that describes which fields and properties to populate. </param>
	/// <param name="data">An array of <see cref="T:System.Object" /> that specifies the values for each field and property to populate. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" />, <paramref name="members" />, or <paramref name="data" /> parameter is null.An element of <paramref name="members" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="members" /> does not match the length of <paramref name="data" />. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">An element of <paramref name="members" /> is not an instance of <see cref="T:System.Reflection.FieldInfo" />. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
	/// </PermissionSet>
	[SecurityCritical]
	public static object PopulateObjectMembers(object obj, MemberInfo[] members, object[] data)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (members == null)
		{
			throw new ArgumentNullException("members");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (members.Length != data.Length)
		{
			throw new ArgumentException(Environment.GetResourceString("Parameters 'members' and 'data' must have the same length."));
		}
		for (int i = 0; i < members.Length; i++)
		{
			MemberInfo memberInfo = members[i];
			if (memberInfo == null)
			{
				throw new ArgumentNullException("members", Environment.GetResourceString("Member at position {0} was null.", i));
			}
			if (data[i] != null)
			{
				if (memberInfo.MemberType != MemberTypes.Field)
				{
					throw new SerializationException(Environment.GetResourceString("Only FieldInfo, PropertyInfo, and SerializationMemberInfo are recognized."));
				}
				SerializationSetValue(memberInfo, obj, data[i]);
			}
		}
		return obj;
	}

	/// <summary>Extracts the data from the specified object and returns it as an array of objects.</summary>
	/// <returns>An array of <see cref="T:System.Object" /> that contains data stored in <paramref name="members" /> and associated with <paramref name="obj" />.</returns>
	/// <param name="obj">The object to write to the formatter. </param>
	/// <param name="members">The members to extract from the object. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" /> or <paramref name="members" /> parameter is null.An element of <paramref name="members" /> is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">An element of <paramref name="members" /> does not represent a field. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
	/// </PermissionSet>
	[SecurityCritical]
	public static object[] GetObjectData(object obj, MemberInfo[] members)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (members == null)
		{
			throw new ArgumentNullException("members");
		}
		int num = members.Length;
		object[] array = new object[num];
		for (int i = 0; i < num; i++)
		{
			MemberInfo memberInfo = members[i];
			if (memberInfo == null)
			{
				throw new ArgumentNullException("members", Environment.GetResourceString("Member at position {0} was null.", i));
			}
			if (memberInfo.MemberType == MemberTypes.Field)
			{
				RtFieldInfo rtFieldInfo = memberInfo as RtFieldInfo;
				if (rtFieldInfo != null)
				{
					rtFieldInfo.CheckConsistency(obj);
					array[i] = rtFieldInfo.UnsafeGetValue(obj);
				}
				else
				{
					array[i] = ((SerializationFieldInfo)memberInfo).InternalGetValue(obj);
				}
				continue;
			}
			throw new SerializationException(Environment.GetResourceString("Only FieldInfo, PropertyInfo, and SerializationMemberInfo are recognized."));
		}
		return array;
	}

	/// <summary>Returns a serialization surrogate for the specified <see cref="T:System.Runtime.Serialization.ISerializationSurrogate" />.</summary>
	/// <returns>An <see cref="T:System.Runtime.Serialization.ISerializationSurrogate" /> for the specified <paramref name="innerSurrogate" />.</returns>
	/// <param name="innerSurrogate">The specified surrogate.</param>
	[ComVisible(false)]
	[SecurityCritical]
	public static ISerializationSurrogate GetSurrogateForCyclicalReference(ISerializationSurrogate innerSurrogate)
	{
		if (innerSurrogate == null)
		{
			throw new ArgumentNullException("innerSurrogate");
		}
		return new SurrogateForCyclicalReference(innerSurrogate);
	}

	/// <summary>Looks up the <see cref="T:System.Type" /> of the specified object in the provided <see cref="T:System.Reflection.Assembly" />.</summary>
	/// <returns>The <see cref="T:System.Type" /> of the named object.</returns>
	/// <param name="assem">The assembly where you want to look up the object. </param>
	/// <param name="name">The name of the object. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="assem" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
	/// </PermissionSet>
	[SecurityCritical]
	public static Type GetTypeFromAssembly(Assembly assem, string name)
	{
		if (assem == null)
		{
			throw new ArgumentNullException("assem");
		}
		return assem.GetType(name, throwOnError: false, ignoreCase: false);
	}

	internal static Assembly LoadAssemblyFromString(string assemblyName)
	{
		return Assembly.Load(assemblyName);
	}

	internal static Assembly LoadAssemblyFromStringNoThrow(string assemblyName)
	{
		try
		{
			return LoadAssemblyFromString(assemblyName);
		}
		catch (Exception)
		{
		}
		return null;
	}

	internal static string GetClrAssemblyName(Type type, out bool hasTypeForwardedFrom)
	{
		if ((object)type == null)
		{
			throw new ArgumentNullException("type");
		}
		object[] customAttributes = type.GetCustomAttributes(typeof(TypeForwardedFromAttribute), inherit: false);
		if (customAttributes != null && customAttributes.Length != 0)
		{
			hasTypeForwardedFrom = true;
			return ((TypeForwardedFromAttribute)customAttributes[0]).AssemblyFullName;
		}
		hasTypeForwardedFrom = false;
		return type.Assembly.FullName;
	}

	internal static string GetClrTypeFullName(Type type)
	{
		if (type.IsArray)
		{
			return GetClrTypeFullNameForArray(type);
		}
		return GetClrTypeFullNameForNonArrayTypes(type);
	}

	private static string GetClrTypeFullNameForArray(Type type)
	{
		int arrayRank = type.GetArrayRank();
		if (arrayRank == 1)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}{1}", GetClrTypeFullName(type.GetElementType()), "[]");
		}
		StringBuilder stringBuilder = new StringBuilder(GetClrTypeFullName(type.GetElementType())).Append("[");
		for (int i = 1; i < arrayRank; i++)
		{
			stringBuilder.Append(",");
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}

	private static string GetClrTypeFullNameForNonArrayTypes(Type type)
	{
		if (!type.IsGenericType)
		{
			return type.FullName;
		}
		Type[] genericArguments = type.GetGenericArguments();
		StringBuilder stringBuilder = new StringBuilder(type.GetGenericTypeDefinition().FullName).Append("[");
		Type[] array = genericArguments;
		foreach (Type type2 in array)
		{
			stringBuilder.Append("[").Append(GetClrTypeFullName(type2)).Append(", ");
			stringBuilder.Append(GetClrAssemblyName(type2, out var _)).Append("],");
		}
		return stringBuilder.Remove(stringBuilder.Length - 1, 1).Append("]").ToString();
	}
}
