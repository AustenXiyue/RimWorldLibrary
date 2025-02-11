namespace MS.Internal.WindowsRuntime.ABI.System.Collections.Generic;

internal static class IEnumerator_Delegates
{
	public delegate int MoveNext_2(nint thisPtr, out byte __return_value__);

	public delegate int GetMany_3(nint thisPtr, int __itemsSize, nint items, out uint __return_value__);
}
