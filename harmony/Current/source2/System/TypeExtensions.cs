namespace System;

internal static class TypeExtensions
{
	public static bool IsByRefLike(this Type type)
	{
		System.ThrowHelper.ThrowIfArgumentNull(type, System.ExceptionArgument.type);
		if ((object)type == null)
		{
			System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.type);
		}
		object[] customAttributes = type.GetCustomAttributes(inherit: false);
		for (int i = 0; i < customAttributes.Length; i++)
		{
			if (customAttributes[i].GetType().FullName == "System.Runtime.CompilerServices.IsByRefLikeAttribute")
			{
				return true;
			}
		}
		return false;
	}
}
