using System;

namespace MS.Internal.Threading;

internal sealed class ExceptionFilterHelper
{
	private InternalRealCallDelegate _internalRealCall;

	private FilterExceptionDelegate _filterException;

	private CatchExceptionDelegate _catchException;

	internal ExceptionFilterHelper(InternalRealCallDelegate internalRealCall, FilterExceptionDelegate filterException, CatchExceptionDelegate catchException)
	{
		_internalRealCall = internalRealCall;
		_filterException = filterException;
		_catchException = catchException;
	}

	internal object TryCatchWhen(object source, Delegate method, object args, int numArgs, Delegate catchHandler)
	{
		object result = null;
		try
		{
			result = _internalRealCall(method, args, numArgs);
		}
		catch (Exception e) when (_filterException(source, e))
		{
			if (!_catchException(source, e, catchHandler))
			{
				throw;
			}
		}
		return result;
	}
}
