using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Threading;

/// <summary>The exception that is thrown when one thread acquires a <see cref="T:System.Threading.Mutex" /> object that another thread has abandoned by exiting without releasing it.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(false)]
public class AbandonedMutexException : SystemException
{
	private int m_MutexIndex = -1;

	private Mutex m_Mutex;

	/// <summary>Gets the abandoned mutex that caused the exception, if known.</summary>
	/// <returns>A <see cref="T:System.Threading.Mutex" /> object that represents the abandoned mutex, or null if the abandoned mutex could not be identified.</returns>
	/// <filterpriority>1</filterpriority>
	public Mutex Mutex => m_Mutex;

	/// <summary>Gets the index of the abandoned mutex that caused the exception, if known.</summary>
	/// <returns>The index, in the array of wait handles passed to the <see cref="Overload:System.Threading.WaitHandle.WaitAny" /> method, of the <see cref="T:System.Threading.Mutex" /> object that represents the abandoned mutex, or –1 if the index of the abandoned mutex could not be determined.</returns>
	/// <filterpriority>1</filterpriority>
	public int MutexIndex => m_MutexIndex;

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.AbandonedMutexException" /> class with default values.</summary>
	public AbandonedMutexException()
		: base(Environment.GetResourceString("The wait completed due to an abandoned mutex."))
	{
		SetErrorCode(-2146233043);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.AbandonedMutexException" /> class with a specified error message.</summary>
	/// <param name="message">An error message that explains the reason for the exception.</param>
	public AbandonedMutexException(string message)
		: base(message)
	{
		SetErrorCode(-2146233043);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.AbandonedMutexException" /> class with a specified error message and inner exception. </summary>
	/// <param name="message">An error message that explains the reason for the exception.</param>
	/// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception.</param>
	public AbandonedMutexException(string message, Exception inner)
		: base(message, inner)
	{
		SetErrorCode(-2146233043);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.AbandonedMutexException" /> class with a specified index for the abandoned mutex, if applicable, and a <see cref="T:System.Threading.Mutex" /> object that represents the mutex.</summary>
	/// <param name="location">The index of the abandoned mutex in the array of wait handles if the exception is thrown for the <see cref="Overload:System.Threading.WaitHandle.WaitAny" /> method, or –1 if the exception is thrown for the <see cref="Overload:System.Threading.WaitHandle.WaitOne" /> or <see cref="Overload:System.Threading.WaitHandle.WaitAll" /> methods.</param>
	/// <param name="handle">A <see cref="T:System.Threading.Mutex" /> object that represents the abandoned mutex.</param>
	public AbandonedMutexException(int location, WaitHandle handle)
		: base(Environment.GetResourceString("The wait completed due to an abandoned mutex."))
	{
		SetErrorCode(-2146233043);
		SetupException(location, handle);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.AbandonedMutexException" /> class with a specified error message, the index of the abandoned mutex, if applicable, and the abandoned mutex. </summary>
	/// <param name="message">An error message that explains the reason for the exception.</param>
	/// <param name="location">The index of the abandoned mutex in the array of wait handles if the exception is thrown for the <see cref="Overload:System.Threading.WaitHandle.WaitAny" /> method, or –1 if the exception is thrown for the <see cref="Overload:System.Threading.WaitHandle.WaitOne" /> or <see cref="Overload:System.Threading.WaitHandle.WaitAll" /> methods.</param>
	/// <param name="handle">A <see cref="T:System.Threading.Mutex" /> object that represents the abandoned mutex.</param>
	public AbandonedMutexException(string message, int location, WaitHandle handle)
		: base(message)
	{
		SetErrorCode(-2146233043);
		SetupException(location, handle);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.AbandonedMutexException" /> class with a specified error message, the inner exception, the index for the abandoned mutex, if applicable, and a <see cref="T:System.Threading.Mutex" /> object that represents the mutex.</summary>
	/// <param name="message">An error message that explains the reason for the exception.</param>
	/// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception.</param>
	/// <param name="location">The index of the abandoned mutex in the array of wait handles if the exception is thrown for the <see cref="Overload:System.Threading.WaitHandle.WaitAny" /> method, or –1 if the exception is thrown for the <see cref="Overload:System.Threading.WaitHandle.WaitOne" /> or <see cref="Overload:System.Threading.WaitHandle.WaitAll" /> methods.</param>
	/// <param name="handle">A <see cref="T:System.Threading.Mutex" /> object that represents the abandoned mutex.</param>
	public AbandonedMutexException(string message, Exception inner, int location, WaitHandle handle)
		: base(message, inner)
	{
		SetErrorCode(-2146233043);
		SetupException(location, handle);
	}

	private void SetupException(int location, WaitHandle handle)
	{
		m_MutexIndex = location;
		if (handle != null)
		{
			m_Mutex = handle as Mutex;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.AbandonedMutexException" /> class with serialized data.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that holds the serialized object data about the exception being thrown. </param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> object that contains contextual information about the source or destination.</param>
	protected AbandonedMutexException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
