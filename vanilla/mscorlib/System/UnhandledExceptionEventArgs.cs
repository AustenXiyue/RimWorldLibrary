using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace System;

/// <summary>Provides data for the event that is raised when there is an exception that is not handled in any application domain.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public class UnhandledExceptionEventArgs : EventArgs
{
	private object _Exception;

	private bool _IsTerminating;

	/// <summary>Gets the unhandled exception object.</summary>
	/// <returns>The unhandled exception object.</returns>
	/// <filterpriority>2</filterpriority>
	public object ExceptionObject
	{
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		get
		{
			return _Exception;
		}
	}

	/// <summary>Indicates whether the common language runtime is terminating.</summary>
	/// <returns>true if the runtime is terminating; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsTerminating
	{
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		get
		{
			return _IsTerminating;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.UnhandledExceptionEventArgs" /> class with the exception object and a common language runtime termination flag.</summary>
	/// <param name="exception">The exception that is not handled. </param>
	/// <param name="isTerminating">true if the runtime is terminating; otherwise, false. </param>
	public UnhandledExceptionEventArgs(object exception, bool isTerminating)
	{
		_Exception = exception;
		_IsTerminating = isTerminating;
	}
}
