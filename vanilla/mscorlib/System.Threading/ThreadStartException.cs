using System.Runtime.Serialization;

namespace System.Threading;

/// <summary>The exception that is thrown when a failure occurs in a managed thread after the underlying operating system thread has been started, but before the thread is ready to execute user code.</summary>
[Serializable]
public sealed class ThreadStartException : SystemException
{
	private ThreadStartException()
		: base(Environment.GetResourceString("Thread failed to start."))
	{
		SetErrorCode(-2146233051);
	}

	private ThreadStartException(Exception reason)
		: base(Environment.GetResourceString("Thread failed to start."), reason)
	{
		SetErrorCode(-2146233051);
	}

	internal ThreadStartException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
