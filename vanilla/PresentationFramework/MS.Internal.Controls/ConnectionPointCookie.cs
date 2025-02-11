using System;
using System.Runtime.InteropServices;
using System.Windows;
using MS.Win32;

namespace MS.Internal.Controls;

internal class ConnectionPointCookie
{
	private MS.Win32.UnsafeNativeMethods.IConnectionPoint connectionPoint;

	private int cookie;

	internal ConnectionPointCookie(object source, object sink, Type eventInterface)
	{
		Exception ex = null;
		if (source is MS.Win32.UnsafeNativeMethods.IConnectionPointContainer)
		{
			MS.Win32.UnsafeNativeMethods.IConnectionPointContainer connectionPointContainer = (MS.Win32.UnsafeNativeMethods.IConnectionPointContainer)source;
			try
			{
				Guid guid = eventInterface.GUID;
				if (connectionPointContainer.FindConnectionPoint(ref guid, out connectionPoint) != 0)
				{
					connectionPoint = null;
				}
			}
			catch (Exception ex2)
			{
				if (CriticalExceptions.IsCriticalException(ex2))
				{
					throw;
				}
				connectionPoint = null;
			}
			if (connectionPoint == null)
			{
				ex = new ArgumentException(SR.Format(SR.AxNoEventInterface, eventInterface.Name));
			}
			else if (sink == null || (!eventInterface.IsInstanceOfType(sink) && !Marshal.IsComObject(sink)))
			{
				ex = new InvalidCastException(SR.Format(SR.AxNoSinkImplementation, eventInterface.Name));
			}
			else
			{
				int num = connectionPoint.Advise(sink, ref cookie);
				if (num != 0)
				{
					cookie = 0;
					Marshal.FinalReleaseComObject(connectionPoint);
					connectionPoint = null;
					ex = new InvalidOperationException(SR.Format(SR.AxNoSinkAdvise, eventInterface.Name, num));
				}
			}
		}
		else
		{
			ex = new InvalidCastException(SR.AxNoConnectionPointContainer);
		}
		if (connectionPoint == null || cookie == 0)
		{
			if (connectionPoint != null)
			{
				Marshal.FinalReleaseComObject(connectionPoint);
			}
			if (ex == null)
			{
				throw new ArgumentException(SR.Format(SR.AxNoConnectionPoint, eventInterface.Name));
			}
			throw ex;
		}
	}

	internal void Disconnect()
	{
		if (connectionPoint == null || cookie == 0)
		{
			return;
		}
		try
		{
			connectionPoint.Unadvise(cookie);
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
		}
		finally
		{
			cookie = 0;
		}
		try
		{
			Marshal.FinalReleaseComObject(connectionPoint);
		}
		catch (Exception ex2)
		{
			if (CriticalExceptions.IsCriticalException(ex2))
			{
				throw;
			}
		}
		finally
		{
			connectionPoint = null;
		}
	}

	~ConnectionPointCookie()
	{
		Disconnect();
	}
}
