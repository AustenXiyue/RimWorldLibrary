namespace MonoMod.Utils;

internal static class OSKindExtensions
{
	public static bool Is(this OSKind operatingSystem, OSKind test)
	{
		return operatingSystem.Has(test);
	}

	public static OSKind GetKernel(this OSKind operatingSystem)
	{
		return operatingSystem & (OSKind)31;
	}

	public static int GetSubtypeId(this OSKind operatingSystem)
	{
		return (int)operatingSystem >> 5;
	}
}
