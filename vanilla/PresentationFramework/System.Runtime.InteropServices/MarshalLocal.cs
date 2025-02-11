namespace System.Runtime.InteropServices;

internal static class MarshalLocal
{
	public static bool IsTypeVisibleFromCom(Type type)
	{
		return Marshal.IsTypeVisibleFromCom(type);
	}
}
