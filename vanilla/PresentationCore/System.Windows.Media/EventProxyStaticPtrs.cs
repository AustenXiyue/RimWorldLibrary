namespace System.Windows.Media;

internal static class EventProxyStaticPtrs
{
	internal static EventProxyDescriptor.Dispose pfnDispose;

	internal static EventProxyDescriptor.RaiseEvent pfnRaiseEvent;

	static EventProxyStaticPtrs()
	{
		pfnDispose = EventProxyDescriptor.StaticDispose;
		pfnRaiseEvent = EventProxyWrapper.RaiseEvent;
	}
}
