using System.Runtime.InteropServices;
using System.Runtime.Remoting.Metadata;

namespace System.Runtime.Serialization.Formatters;

/// <summary>Contains information for a server fault. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
[SoapType(Embedded = true)]
public sealed class ServerFault
{
	private string exceptionType;

	private string message;

	private string stackTrace;

	private Exception exception;

	/// <summary>Gets or sets the type of exception that was thrown by the server.</summary>
	/// <returns>The type of exception that was thrown by the server.</returns>
	public string ExceptionType
	{
		get
		{
			return exceptionType;
		}
		set
		{
			exceptionType = value;
		}
	}

	/// <summary>Gets or sets the exception message that accompanied the exception thrown on the server.</summary>
	/// <returns>The exception message that accompanied the exception thrown on the server.</returns>
	public string ExceptionMessage
	{
		get
		{
			return message;
		}
		set
		{
			message = value;
		}
	}

	/// <summary>Gets or sets the stack trace of the thread that threw the exception on the server.</summary>
	/// <returns>The stack trace of the thread that threw the exception on the server.</returns>
	public string StackTrace
	{
		get
		{
			return stackTrace;
		}
		set
		{
			stackTrace = value;
		}
	}

	internal Exception Exception => exception;

	internal ServerFault(Exception exception)
	{
		this.exception = exception;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.Formatters.ServerFault" /> class.</summary>
	/// <param name="exceptionType">The type of the exception that occurred on the server. </param>
	/// <param name="message">The message that accompanied the exception. </param>
	/// <param name="stackTrace">The stack trace of the thread that threw the exception on the server. </param>
	public ServerFault(string exceptionType, string message, string stackTrace)
	{
		this.exceptionType = exceptionType;
		this.message = message;
		this.stackTrace = stackTrace;
	}
}
