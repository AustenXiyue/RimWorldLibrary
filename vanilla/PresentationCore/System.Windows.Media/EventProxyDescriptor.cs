using System.Runtime.InteropServices;

namespace System.Windows.Media;

internal struct EventProxyDescriptor
{
	internal delegate void Dispose(ref EventProxyDescriptor pEPD);

	internal delegate int RaiseEvent(ref EventProxyDescriptor pEPD, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] buffer, uint cb);

	internal Dispose pfnDispose;

	internal RaiseEvent pfnRaiseEvent;

	internal GCHandle m_handle;

	internal static void StaticDispose(ref EventProxyDescriptor pEPD)
	{
		_ = (EventProxyWrapper)pEPD.m_handle.Target;
		GCHandle handle = pEPD.m_handle;
		handle.Free();
	}
}
