using System;
using System.Collections.Generic;
using System.Reflection;

namespace MonoMod.Utils;

internal class GenericMethodInstantiationComparer : IEqualityComparer<MethodBase>
{
	internal static Type? CannonicalFillType = typeof(object).Assembly.GetType("System.__Canon");

	private readonly IEqualityComparer<Type> genericTypeComparer;

	public GenericMethodInstantiationComparer()
		: this(new GenericTypeInstantiationComparer())
	{
	}

	public GenericMethodInstantiationComparer(IEqualityComparer<Type> typeComparer)
	{
		genericTypeComparer = typeComparer;
	}

	public bool Equals(MethodBase? x, MethodBase? y)
	{
		if ((object)x == null && (object)y == null)
		{
			return true;
		}
		if ((object)x == null || (object)y == null)
		{
			return false;
		}
		bool flag = (x.IsGenericMethod && !x.ContainsGenericParameters) || (x.DeclaringType?.IsGenericType ?? false);
		bool flag2 = (y.IsGenericMethod && !y.ContainsGenericParameters) || (y.DeclaringType?.IsGenericType ?? false);
		if (flag != flag2)
		{
			return false;
		}
		if (!flag)
		{
			return x.Equals(y);
		}
		if (!genericTypeComparer.Equals(x.DeclaringType, y.DeclaringType))
		{
			return false;
		}
		MethodBase methodBase = ((!(x is MethodInfo method)) ? x.GetUnfilledMethodOnGenericType() : method.GetActualGenericMethodDefinition());
		MethodBase methodBase2 = ((!(y is MethodInfo method2)) ? y.GetUnfilledMethodOnGenericType() : method2.GetActualGenericMethodDefinition());
		if (!methodBase.Equals(methodBase2))
		{
			return false;
		}
		if (methodBase.Name != methodBase2.Name)
		{
			return false;
		}
		ParameterInfo[] parameters = x.GetParameters();
		ParameterInfo[] parameters2 = y.GetParameters();
		if (parameters.Length != parameters2.Length)
		{
			return false;
		}
		ParameterInfo[] parameters3 = methodBase.GetParameters();
		for (int i = 0; i < parameters.Length; i++)
		{
			Type type = parameters[i].ParameterType;
			Type type2 = parameters2[i].ParameterType;
			if (parameters3[i].ParameterType.IsGenericParameter)
			{
				if (!type.IsValueType)
				{
					type = CannonicalFillType ?? typeof(object);
				}
				if (!type2.IsValueType)
				{
					type2 = CannonicalFillType ?? typeof(object);
				}
			}
			if (!genericTypeComparer.Equals(type, type2))
			{
				return false;
			}
		}
		return true;
	}

	public int GetHashCode(MethodBase obj)
	{
		Helpers.ThrowIfArgumentNull(obj, "obj");
		if (!obj.IsGenericMethod || obj.ContainsGenericParameters)
		{
			Type declaringType = obj.DeclaringType;
			if ((object)declaringType == null || !declaringType.IsGenericType)
			{
				return obj.GetHashCode();
			}
		}
		int num = -559038737;
		if (obj.DeclaringType != null)
		{
			num ^= obj.DeclaringType.Assembly.GetHashCode();
			num ^= genericTypeComparer.GetHashCode(obj.DeclaringType);
		}
		num ^= obj.Name.GetHashCode(StringComparison.Ordinal);
		ParameterInfo[] parameters = obj.GetParameters();
		int num2 = parameters.Length;
		num2 ^= num2 << 4;
		num2 ^= num2 << 8;
		num2 ^= num2 << 16;
		num ^= num2;
		if (obj.IsGenericMethod)
		{
			Type[] genericArguments = obj.GetGenericArguments();
			for (int i = 0; i < genericArguments.Length; i++)
			{
				int num3 = i % 32;
				Type type = genericArguments[i];
				int num4 = (type.IsValueType ? genericTypeComparer.GetHashCode(type) : (CannonicalFillType?.GetHashCode() ?? 1431655765));
				num4 = (num4 << num3) | (num4 >> 32 - num3);
				num ^= num4;
			}
		}
		MethodBase methodBase = ((!(obj is MethodInfo method)) ? obj.GetUnfilledMethodOnGenericType() : method.GetActualGenericMethodDefinition());
		ParameterInfo[] parameters2 = methodBase.GetParameters();
		for (int j = 0; j < parameters.Length; j++)
		{
			int num5 = j % 32;
			Type parameterType = parameters[j].ParameterType;
			int num6 = genericTypeComparer.GetHashCode(parameterType);
			if (parameters2[j].ParameterType.IsGenericParameter && !parameterType.IsValueType)
			{
				num6 = CannonicalFillType?.GetHashCode() ?? 1431655765;
			}
			num6 = (num6 >> num5) | (num6 << 32 - num5);
			num ^= num6;
		}
		return num;
	}
}
