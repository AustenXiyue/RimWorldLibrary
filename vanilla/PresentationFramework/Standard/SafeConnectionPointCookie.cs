using System;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Win32.SafeHandles;

namespace Standard;

internal sealed class SafeConnectionPointCookie : SafeHandleZeroOrMinusOneIsInvalid
{
	private IConnectionPoint _cp;

	public SafeConnectionPointCookie(IConnectionPointContainer target, object sink, Guid eventId)
		: base(ownsHandle: true)
	{
		Verify.IsNotNull(target, "target");
		Verify.IsNotNull(sink, "sink");
		Verify.IsNotDefault(eventId, "eventId");
		handle = IntPtr.Zero;
		IConnectionPoint ppCP = null;
		try
		{
			target.FindConnectionPoint(ref eventId, out ppCP);
			ppCP.Advise(sink, out var pdwCookie);
			if (pdwCookie == 0)
			{
				throw new InvalidOperationException("IConnectionPoint::Advise returned an invalid cookie.");
			}
			handle = new IntPtr(pdwCookie);
			_cp = ppCP;
			ppCP = null;
		}
		finally
		{
			Utility.SafeRelease(ref ppCP);
		}
	}

	public void Disconnect()
	{
		ReleaseHandle();
	}

	protected override bool ReleaseHandle()
	{
		try
		{
			if (!IsInvalid)
			{
				int dwCookie = ((IntPtr)handle).ToInt32();
				handle = IntPtr.Zero;
				try
				{
					_cp.Unadvise(dwCookie);
				}
				finally
				{
					Utility.SafeRelease(ref _cp);
				}
			}
			return true;
		}
		catch
		{
			return false;
		}
	}
}
