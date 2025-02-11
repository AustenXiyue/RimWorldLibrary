using System;
using System.Globalization;
using System.Threading;

namespace MS.Internal;

internal class CulturePreservingExecutionContext : IDisposable
{
	private class CultureAndContextManager
	{
		private CultureInfo _culture;

		private CultureInfo _uICulture;

		public ContextCallback Callback { get; private set; }

		public object State { get; private set; }

		private CultureAndContextManager(ContextCallback callback, object state)
		{
			Callback = callback;
			State = state;
			ReadCultureInfosFromCurrentThread();
		}

		public static CultureAndContextManager Initialize(ContextCallback callback, object state)
		{
			return new CultureAndContextManager(callback, state);
		}

		public void ReadCultureInfosFromCurrentThread()
		{
			_culture = Thread.CurrentThread.CurrentCulture;
			_uICulture = Thread.CurrentThread.CurrentUICulture;
		}

		public void WriteCultureInfosToCurrentThread()
		{
			Thread.CurrentThread.CurrentCulture = _culture;
			Thread.CurrentThread.CurrentUICulture = _uICulture;
		}
	}

	private bool _disposed;

	private ExecutionContext _context;

	private CultureAndContextManager _cultureAndContext;

	private static ContextCallback CallbackWrapperDelegate;

	public static CulturePreservingExecutionContext Capture()
	{
		if (ExecutionContext.IsFlowSuppressed())
		{
			return null;
		}
		CulturePreservingExecutionContext culturePreservingExecutionContext = new CulturePreservingExecutionContext();
		if (culturePreservingExecutionContext._context != null)
		{
			return culturePreservingExecutionContext;
		}
		culturePreservingExecutionContext.Dispose();
		return null;
	}

	public static void Run(CulturePreservingExecutionContext executionContext, ContextCallback callback, object state)
	{
		if (executionContext == null)
		{
			throw new ArgumentNullException("executionContext");
		}
		if (callback == null)
		{
			return;
		}
		if (BaseAppContextSwitches.DoNotUseCulturePreservingDispatcherOperations)
		{
			ExecutionContext.Run(executionContext._context, callback, state);
			return;
		}
		executionContext._cultureAndContext = CultureAndContextManager.Initialize(callback, state);
		try
		{
			ExecutionContext.Run(executionContext._context, CallbackWrapperDelegate, executionContext._cultureAndContext);
		}
		finally
		{
			executionContext._cultureAndContext.WriteCultureInfosToCurrentThread();
		}
	}

	private static void CallbackWrapper(object obj)
	{
		CultureAndContextManager obj2 = obj as CultureAndContextManager;
		ContextCallback callback = obj2.Callback;
		object state = obj2.State;
		obj2.WriteCultureInfosToCurrentThread();
		callback(state);
		obj2.ReadCultureInfosFromCurrentThread();
	}

	static CulturePreservingExecutionContext()
	{
		CallbackWrapperDelegate = CallbackWrapper;
	}

	private CulturePreservingExecutionContext()
	{
		_context = ExecutionContext.Capture();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed && disposing)
		{
			_context?.Dispose();
			_disposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
