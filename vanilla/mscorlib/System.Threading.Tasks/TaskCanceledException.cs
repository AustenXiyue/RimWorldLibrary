using System.Runtime.Serialization;

namespace System.Threading.Tasks;

/// <summary>Represents an exception used to communicate task cancellation.</summary>
[Serializable]
public class TaskCanceledException : OperationCanceledException
{
	[NonSerialized]
	private Task m_canceledTask;

	/// <summary>Gets the task associated with this exception.</summary>
	/// <returns>A reference to the <see cref="T:System.Threading.Tasks.Task" /> that is associated with this exception.</returns>
	public Task Task => m_canceledTask;

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.Tasks.TaskCanceledException" /> class with a system-supplied message that describes the error.</summary>
	public TaskCanceledException()
		: base(Environment.GetResourceString("A task was canceled."))
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.Tasks.TaskCanceledException" /> class with a specified message that describes the error.</summary>
	/// <param name="message">The message that describes the exception. The caller of this constructor is required to ensure that this string has been localized for the current system culture.</param>
	public TaskCanceledException(string message)
		: base(message)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.Tasks.TaskCanceledException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The message that describes the exception. The caller of this constructor is required to ensure that this string has been localized for the current system culture. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public TaskCanceledException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.Tasks.TaskCanceledException" /> class with a reference to the <see cref="T:System.Threading.Tasks.Task" /> that has been canceled.</summary>
	/// <param name="task">A task that has been canceled.</param>
	public TaskCanceledException(Task task)
		: base(Environment.GetResourceString("A task was canceled."), task?.CancellationToken ?? default(CancellationToken))
	{
		m_canceledTask = task;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.Tasks.TaskCanceledException" /> class with serialized data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	protected TaskCanceledException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
