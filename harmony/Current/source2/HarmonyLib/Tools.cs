using System;
using System.Reflection;
using System.Reflection.Emit;
using MonoMod.Utils;

namespace HarmonyLib;

internal class Tools
{
	internal struct TypeAndName
	{
		internal Type type;

		internal string name;
	}

	internal static readonly bool isWindows = Environment.OSVersion.Platform.Equals(PlatformID.Win32NT);

	internal static TypeAndName TypColonName(string typeColonName)
	{
		if (typeColonName == null)
		{
			throw new ArgumentNullException("typeColonName");
		}
		string[] array = typeColonName.Split(':');
		if (array.Length != 2)
		{
			throw new ArgumentException(" must be specified as 'Namespace.Type1.Type2:MemberName", "typeColonName");
		}
		TypeAndName result = default(TypeAndName);
		result.type = AccessTools.TypeByName(array[0]);
		result.name = array[1];
		return result;
	}

	internal static void ValidateFieldType<F>(FieldInfo fieldInfo)
	{
		Type typeFromHandle = typeof(F);
		Type fieldType = fieldInfo.FieldType;
		if (typeFromHandle == fieldType)
		{
			return;
		}
		if (fieldType.IsEnum)
		{
			Type underlyingType = Enum.GetUnderlyingType(fieldType);
			if (typeFromHandle != underlyingType)
			{
				throw new ArgumentException("FieldRefAccess return type must be the same as FieldType or " + $"FieldType's underlying integral type ({underlyingType}) for enum types");
			}
		}
		else
		{
			if (fieldType.IsValueType)
			{
				throw new ArgumentException("FieldRefAccess return type must be the same as FieldType for value types");
			}
			if (!typeFromHandle.IsAssignableFrom(fieldType))
			{
				throw new ArgumentException("FieldRefAccess return type must be assignable from FieldType for reference types");
			}
		}
	}

	internal static AccessTools.FieldRef<T, F> FieldRefAccess<T, F>(FieldInfo fieldInfo, bool needCastclass)
	{
		ValidateFieldType<F>(fieldInfo);
		Type typeFromHandle = typeof(T);
		Type declaringType = fieldInfo.DeclaringType;
		DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition("__refget_" + typeFromHandle.Name + "_fi_" + fieldInfo.Name, typeof(F).MakeByRefType(), new Type[1] { typeFromHandle });
		ILGenerator iLGenerator = dynamicMethodDefinition.GetILGenerator();
		if (fieldInfo.IsStatic)
		{
			iLGenerator.Emit(OpCodes.Ldsflda, fieldInfo);
		}
		else
		{
			iLGenerator.Emit(OpCodes.Ldarg_0);
			if (needCastclass)
			{
				iLGenerator.Emit(OpCodes.Castclass, declaringType);
			}
			iLGenerator.Emit(OpCodes.Ldflda, fieldInfo);
		}
		iLGenerator.Emit(OpCodes.Ret);
		return (AccessTools.FieldRef<T, F>)dynamicMethodDefinition.Generate().CreateDelegate(typeof(AccessTools.FieldRef<T, F>));
	}

	internal static AccessTools.StructFieldRef<T, F> StructFieldRefAccess<T, F>(FieldInfo fieldInfo) where T : struct
	{
		ValidateFieldType<F>(fieldInfo);
		DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition("__refget_" + typeof(T).Name + "_struct_fi_" + fieldInfo.Name, typeof(F).MakeByRefType(), new Type[1] { typeof(T).MakeByRefType() });
		ILGenerator iLGenerator = dynamicMethodDefinition.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldflda, fieldInfo);
		iLGenerator.Emit(OpCodes.Ret);
		return (AccessTools.StructFieldRef<T, F>)dynamicMethodDefinition.Generate().CreateDelegate(typeof(AccessTools.StructFieldRef<T, F>));
	}

	internal static AccessTools.FieldRef<F> StaticFieldRefAccess<F>(FieldInfo fieldInfo)
	{
		if (!fieldInfo.IsStatic)
		{
			throw new ArgumentException("Field must be static");
		}
		ValidateFieldType<F>(fieldInfo);
		DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition("__refget_" + (fieldInfo.DeclaringType?.Name ?? "null") + "_static_fi_" + fieldInfo.Name, typeof(F).MakeByRefType(), Array.Empty<Type>());
		ILGenerator iLGenerator = dynamicMethodDefinition.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldsflda, fieldInfo);
		iLGenerator.Emit(OpCodes.Ret);
		return (AccessTools.FieldRef<F>)dynamicMethodDefinition.Generate().CreateDelegate(typeof(AccessTools.FieldRef<F>));
	}

	internal static FieldInfo GetInstanceField(Type type, string fieldName)
	{
		FieldInfo fieldInfo = AccessTools.Field(type, fieldName);
		if ((object)fieldInfo == null)
		{
			throw new MissingFieldException(type.Name, fieldName);
		}
		if (fieldInfo.IsStatic)
		{
			throw new ArgumentException("Field must not be static");
		}
		return fieldInfo;
	}

	internal static bool FieldRefNeedsClasscast(Type delegateInstanceType, Type declaringType)
	{
		bool flag = false;
		if (delegateInstanceType != declaringType)
		{
			flag = delegateInstanceType.IsAssignableFrom(declaringType);
			if (!flag && !declaringType.IsAssignableFrom(delegateInstanceType))
			{
				throw new ArgumentException("FieldDeclaringType must be assignable from or to T (FieldRefAccess instance type) - \"instanceOfT is FieldDeclaringType\" must be possible");
			}
		}
		return flag;
	}

	internal static void ValidateStructField<T, F>(FieldInfo fieldInfo) where T : struct
	{
		if (fieldInfo.IsStatic)
		{
			throw new ArgumentException("Field must not be static");
		}
		if (fieldInfo.DeclaringType != typeof(T))
		{
			throw new ArgumentException("FieldDeclaringType must be T (StructFieldRefAccess instance type)");
		}
	}
}
