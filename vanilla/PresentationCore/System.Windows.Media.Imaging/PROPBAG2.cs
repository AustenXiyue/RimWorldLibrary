using System.Runtime.InteropServices;

namespace System.Windows.Media.Imaging;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct PROPBAG2
{
	internal uint dwType;

	internal ushort vt;

	internal ushort cfType;

	internal nint dwHint;

	internal nint pstrName;

	internal Guid clsid;

	internal void Init(string name)
	{
		pstrName = Marshal.StringToCoTaskMemUni(name);
	}

	internal void Clear()
	{
		Marshal.FreeCoTaskMem(pstrName);
		pstrName = IntPtr.Zero;
	}
}
