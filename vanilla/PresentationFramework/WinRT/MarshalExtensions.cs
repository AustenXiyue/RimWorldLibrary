using System.Runtime.InteropServices;

namespace WinRT;

internal static class MarshalExtensions
{
	public static void Dispose(this GCHandle handle)
	{
		if (handle.IsAllocated)
		{
			handle.Free();
		}
	}
}
