using System.Collections.Generic;
using MS.Internal;

namespace System.Windows.Documents.MsSpellCheckLib;

internal static class RetryHelper
{
	internal delegate bool RetryPreamble();

	internal delegate bool RetryActionPreamble(out Action action);

	internal delegate bool RetryFunctionPreamble<TResult>(out Func<TResult> func);

	internal static bool TryCallAction(Action action, RetryActionPreamble preamble, List<Type> ignoredExceptions, int retries = 3, bool throwOnFailure = false)
	{
		ValidateExceptionTypeList(ignoredExceptions);
		int num = retries;
		bool flag = false;
		bool flag2 = true;
		do
		{
			try
			{
				action?.Invoke();
				flag = true;
			}
			catch (Exception exception) when (MatchException(exception, ignoredExceptions))
			{
				goto IL_0034;
			}
			break;
			IL_0034:
			num--;
			if (num > 0)
			{
				flag2 = preamble(out action);
			}
		}
		while (num > 0 && flag2);
		if (!flag && throwOnFailure)
		{
			throw new RetriesExhaustedException();
		}
		return flag;
	}

	internal static bool TryCallAction(Action action, RetryPreamble preamble, List<Type> ignoredExceptions, int retries = 3, bool throwOnFailure = false)
	{
		ValidateExceptionTypeList(ignoredExceptions);
		int num = retries;
		bool flag = false;
		bool flag2 = true;
		do
		{
			try
			{
				action?.Invoke();
				flag = true;
			}
			catch (Exception exception) when (MatchException(exception, ignoredExceptions))
			{
				goto IL_0034;
			}
			break;
			IL_0034:
			num--;
			if (num > 0)
			{
				flag2 = preamble();
			}
		}
		while (num > 0 && flag2);
		if (!flag && throwOnFailure)
		{
			throw new RetriesExhaustedException();
		}
		return flag;
	}

	internal static bool TryExecuteFunction<TResult>(Func<TResult> func, out TResult result, RetryFunctionPreamble<TResult> preamble, List<Type> ignoredExceptions, int retries = 3, bool throwOnFailure = false)
	{
		ValidateExceptionTypeList(ignoredExceptions);
		result = default(TResult);
		int num = retries;
		bool flag = false;
		bool flag2 = true;
		do
		{
			try
			{
				if (func != null)
				{
					result = func();
				}
				flag = true;
			}
			catch (Exception exception) when (MatchException(exception, ignoredExceptions))
			{
				goto IL_0042;
			}
			break;
			IL_0042:
			num--;
			if (num > 0)
			{
				flag2 = preamble(out func);
			}
		}
		while (num > 0 && flag2);
		if (!flag && throwOnFailure)
		{
			throw new RetriesExhaustedException();
		}
		return flag;
	}

	internal static bool TryExecuteFunction<TResult>(Func<TResult> func, out TResult result, RetryPreamble preamble, List<Type> ignoredExceptions, int retries = 3, bool throwOnFailure = false)
	{
		ValidateExceptionTypeList(ignoredExceptions);
		result = default(TResult);
		int num = retries;
		bool flag = false;
		bool flag2 = true;
		do
		{
			try
			{
				if (func != null)
				{
					result = func();
				}
				flag = true;
			}
			catch (Exception exception) when (MatchException(exception, ignoredExceptions))
			{
				goto IL_0042;
			}
			break;
			IL_0042:
			num--;
			if (num > 0)
			{
				flag2 = preamble();
			}
		}
		while (num > 0 && flag2);
		if (!flag && throwOnFailure)
		{
			throw new RetriesExhaustedException();
		}
		return flag;
	}

	private static bool MatchException(Exception exception, List<Type> exceptions)
	{
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		if (exceptions == null)
		{
			throw new ArgumentNullException("exceptions");
		}
		Type exceptionType = exception.GetType();
		return exceptions.Find((Type e) => e.IsAssignableFrom(exceptionType)) != null;
	}

	private static void ValidateExceptionTypeList(List<Type> exceptions)
	{
		if (exceptions == null)
		{
			throw new ArgumentNullException("exceptions");
		}
		Invariant.Assert(exceptions.TrueForAll((Type t) => typeof(Exception).IsAssignableFrom(t)));
	}
}
