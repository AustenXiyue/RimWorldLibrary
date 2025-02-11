using System.Runtime.InteropServices;

namespace Standard;

[StructLayout(LayoutKind.Sequential)]
internal class StartupInput
{
	public int GdiplusVersion = 1;

	public nint DebugEventCallback;

	public bool SuppressBackgroundThread;

	public bool SuppressExternalCodecs;
}
