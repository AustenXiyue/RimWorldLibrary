using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;

namespace System;

/// <summary>The exception that is thrown in a thread upon cancellation of an operation that the thread was executing.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public class OperationCanceledException : SystemException
{
	[NonSerialized]
	private CancellationToken _cancellationToken;

	/// <summary>Gets a token associated with the operation that was canceled.</summary>
	/// <returns>A token associated with the operation that was canceled, or a default token.</returns>
	public CancellationToken CancellationToken
	{
		get
		{
			return _cancellationToken;
		}
		private set
		{
			_cancellationToken = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.OperationCanceledException" /> class with a system-supplied error message.</summary>
	public OperationCanceledException()
		: base(Environment.GetResourceString("The operation was canceled."))
	{
		SetErrorCode(-2146233029);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.OperationCanceledException" /> class with a specified error message.</summary>
	/// <param name="message">A <see cref="T:System.String" /> that describes the error.</param>
	public OperationCanceledException(string message)
		: base(message)
	{
		SetErrorCode(-2146233029);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.OperationCanceledException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public OperationCanceledException(string message, Exception innerException)
		: base(message, innerException)
	{
		SetErrorCode(-2146233029);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.OperationCanceledException" /> class with a cancellation token.</summary>
	/// <param name="token">A cancellation token associated with the operation that was canceled.</param>
	public OperationCanceledException(CancellationToken token)
		: this()
	{
		CancellationToken = token;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.OperationCanceledException" /> class with a specified error message and a cancellation token.</summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="token">A cancellation token associated with the operation that was canceled.</param>
	public OperationCanceledException(string message, CancellationToken token)
		: this(message)
	{
		CancellationToken = token;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.OperationCanceledException" /> class with a specified error message, a reference to the inner exception that is the cause of this exception, and a cancellation token.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	/// <param name="token">A cancellation token associated with the operation that was canceled.</param>
	public OperationCanceledException(string message, Exception innerException, CancellationToken token)
		: this(message, innerException)
	{
		CancellationToken = token;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.OperationCanceledException" /> class with serialized data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	protected OperationCanceledException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
