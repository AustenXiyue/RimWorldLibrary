using System;

namespace WinRT;

internal static class ExceptionExtensions
{
	public static void SetHResult(this Exception ex, int value)
	{
		ex.GetType().GetProperty("HResult").SetValue(ex, value);
	}

	internal static Exception GetExceptionForHR(this Exception innerException, int hresult, string messageResource)
	{
		Exception ex = ((innerException == null) ? new Exception(messageResource) : new Exception(innerException.Message ?? messageResource, innerException));
		ex.SetHResult(hresult);
		return ex;
	}
}
