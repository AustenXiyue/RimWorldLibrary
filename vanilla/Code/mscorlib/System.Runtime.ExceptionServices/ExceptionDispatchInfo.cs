using System.Diagnostics;
using Unity;

namespace System.Runtime.ExceptionServices;

/// <summary>Represents an exception whose state is captured at a certain point in code. </summary>
public sealed class ExceptionDispatchInfo
{
	private Exception m_Exception;

	private object m_stackTrace;

	internal object BinaryStackTraceArray => m_stackTrace;

	/// <summary>Gets the exception that is represented by the current instance. </summary>
	/// <returns>The exception that is represented by the current instance. </returns>
	public Exception SourceException => m_Exception;

	private ExceptionDispatchInfo(Exception exception)
	{
		m_Exception = exception;
		StackTrace[] captured_traces = exception.captured_traces;
		int num = ((captured_traces != null) ? captured_traces.Length : 0);
		StackTrace[] array = new StackTrace[num + 1];
		if (num != 0)
		{
			Array.Copy(captured_traces, 0, array, 0, num);
		}
		array[num] = new StackTrace(exception, 0, fNeedFileInfo: true);
		m_stackTrace = array;
	}

	/// <summary>Creates an <see cref="T:System.Runtime.ExceptionServices.ExceptionDispatchInfo" /> object that represents the specified exception at the current point in code. </summary>
	/// <returns>An object that represents the specified exception at the current point in code. </returns>
	/// <param name="source">The exception whose state is captured, and which is represented by the returned object. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="source" /> is null. </exception>
	public static ExceptionDispatchInfo Capture(Exception source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source", Environment.GetResourceString("Object cannot be null."));
		}
		return new ExceptionDispatchInfo(source);
	}

	/// <summary>Throws the exception that is represented by the current <see cref="T:System.Runtime.ExceptionServices.ExceptionDispatchInfo" /> object, after restoring the state that was saved when the exception was captured. </summary>
	public void Throw()
	{
		m_Exception.RestoreExceptionDispatchInfo(this);
		throw m_Exception;
	}

	public static void Throw(Exception source)
	{
		Capture(source).Throw();
	}

	internal ExceptionDispatchInfo()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
