using System.Reflection;

namespace System.Xaml.Schema;

internal static class SafeReflectionInvoker
{
	private static readonly Assembly SystemXaml = typeof(SafeReflectionInvoker).Assembly;

	public static bool IsInSystemXaml(Type type)
	{
		if (type.Assembly == SystemXaml)
		{
			return true;
		}
		if (type.IsGenericType)
		{
			Type[] genericArguments = type.GetGenericArguments();
			for (int i = 0; i < genericArguments.Length; i++)
			{
				if (IsInSystemXaml(genericArguments[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	internal static bool IsSystemXamlNonPublic(MethodInfo method)
	{
		Type declaringType = method.DeclaringType;
		if (IsInSystemXaml(declaringType) && (!method.IsPublic || !declaringType.IsVisible))
		{
			return true;
		}
		if (method.IsGenericMethod)
		{
			Type[] genericArguments = method.GetGenericArguments();
			foreach (Type type in genericArguments)
			{
				if (IsInSystemXaml(type) && !type.IsVisible)
				{
					return true;
				}
			}
		}
		return false;
	}
}
