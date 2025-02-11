using System.Runtime.Serialization;

namespace System.Threading.Tasks;

/// <summary>Represents an exception used to communicate an invalid operation by a <see cref="T:System.Threading.Tasks.TaskScheduler" />.</summary>
[Serializable]
public class TaskSchedulerException : Exception
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.Tasks.TaskSchedulerException" /> class with a system-supplied message that describes the error.</summary>
	public TaskSchedulerException()
		: base(Environment.GetResourceString("An exception was thrown by a TaskScheduler."))
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.Tasks.TaskSchedulerException" /> class with a specified message that describes the error.</summary>
	/// <param name="message">The message that describes the exception. The caller of this constructor is required to ensure that this string has been localized for the current system culture.</param>
	public TaskSchedulerException(string message)
		: base(message)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.Tasks.TaskSchedulerException" /> class using the default error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="innerException">The exception that is the cause of the current exception.</param>
	public TaskSchedulerException(Exception innerException)
		: base(Environment.GetResourceString("An exception was thrown by a TaskScheduler."), innerException)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.Tasks.TaskSchedulerException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The message that describes the exception. The caller of this constructor is required to ensure that this string has been localized for the current system culture. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public TaskSchedulerException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.Tasks.TaskSchedulerException" /> class with serialized data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	protected TaskSchedulerException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
