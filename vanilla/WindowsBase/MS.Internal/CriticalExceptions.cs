using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using MS.Internal.WindowsBase;

namespace MS.Internal;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal static class CriticalExceptions
{
	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal static bool IsCriticalException(Exception ex)
	{
		ex = Unwrap(ex);
		if (!(ex is NullReferenceException) && !(ex is StackOverflowException) && !(ex is OutOfMemoryException) && !(ex is ThreadAbortException) && !(ex is SEHException))
		{
			return ex is SecurityException;
		}
		return true;
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal static bool IsCriticalApplicationException(Exception ex)
	{
		ex = Unwrap(ex);
		if (!(ex is StackOverflowException) && !(ex is OutOfMemoryException) && !(ex is ThreadAbortException))
		{
			return ex is SecurityException;
		}
		return true;
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal static Exception Unwrap(Exception ex)
	{
		while (ex.InnerException != null && ex is TargetInvocationException)
		{
			ex = ex.InnerException;
		}
		return ex;
	}
}
