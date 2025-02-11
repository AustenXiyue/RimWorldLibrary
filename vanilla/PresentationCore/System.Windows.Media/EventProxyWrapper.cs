using System.Runtime.InteropServices;
using MS.Internal;

namespace System.Windows.Media;

internal class EventProxyWrapper
{
	private WeakReference target;

	private EventProxyWrapper(IInvokable invokable)
	{
		target = new WeakReference(invokable);
	}

	private void Verify()
	{
		if (target == null)
		{
			throw new ObjectDisposedException("EventProxyWrapper");
		}
	}

	public int RaiseEvent(byte[] buffer, uint cb)
	{
		try
		{
			Verify();
			IInvokable invokable = (IInvokable)target.Target;
			if (invokable == null)
			{
				return -2147024890;
			}
			invokable.RaiseEvent(buffer, (int)cb);
		}
		catch (Exception e)
		{
			return Marshal.GetHRForException(e);
		}
		return 0;
	}

	internal static EventProxyWrapper FromEPD(ref EventProxyDescriptor epd)
	{
		GCHandle handle = epd.m_handle;
		return (EventProxyWrapper)handle.Target;
	}

	internal static int RaiseEvent(ref EventProxyDescriptor pEPD, byte[] buffer, uint cb)
	{
		return FromEPD(ref pEPD)?.RaiseEvent(buffer, cb) ?? (-2147024890);
	}

	internal static SafeMILHandle CreateEventProxyWrapper(IInvokable invokable)
	{
		if (invokable == null)
		{
			throw new ArgumentNullException("invokable");
		}
		SafeMILHandle ppEventProxy = null;
		EventProxyWrapper value = new EventProxyWrapper(invokable);
		EventProxyDescriptor pEPD = default(EventProxyDescriptor);
		pEPD.pfnDispose = EventProxyStaticPtrs.pfnDispose;
		pEPD.pfnRaiseEvent = EventProxyStaticPtrs.pfnRaiseEvent;
		pEPD.m_handle = GCHandle.Alloc(value, GCHandleType.Normal);
		HRESULT.Check(MILCreateEventProxy(ref pEPD, out ppEventProxy));
		return ppEventProxy;
	}

	[DllImport("wpfgfx_cor3.dll")]
	private static extern int MILCreateEventProxy(ref EventProxyDescriptor pEPD, out SafeMILHandle ppEventProxy);
}
