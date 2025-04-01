using System.Globalization;
using System.Runtime.Serialization;

namespace System.Runtime.InteropServices;

/// <summary>The base exception type for all COM interop exceptions and structured exception handling (SEH) exceptions.</summary>
[Serializable]
[ComVisible(true)]
public class ExternalException : SystemException
{
	/// <summary>Gets the HRESULT of the error.</summary>
	/// <returns>The HRESULT of the error.</returns>
	public virtual int ErrorCode => base.HResult;

	/// <summary>Initializes a new instance of the ExternalException class with default properties.</summary>
	public ExternalException()
		: base(Environment.GetResourceString("External component has thrown an exception."))
	{
		SetErrorCode(-2147467259);
	}

	/// <summary>Initializes a new instance of the ExternalException class with a specified error message.</summary>
	/// <param name="message">The error message that specifies the reason for the exception. </param>
	public ExternalException(string message)
		: base(message)
	{
		SetErrorCode(-2147467259);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.ExternalException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public ExternalException(string message, Exception inner)
		: base(message, inner)
	{
		SetErrorCode(-2147467259);
	}

	/// <summary>Initializes a new instance of the ExternalException class with a specified error message and the HRESULT of the error.</summary>
	/// <param name="message">The error message that specifies the reason for the exception. </param>
	/// <param name="errorCode">The HRESULT of the error. </param>
	public ExternalException(string message, int errorCode)
		: base(message)
	{
		SetErrorCode(errorCode);
	}

	/// <summary>Initializes a new instance of the ExternalException class from serialization data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null. </exception>
	protected ExternalException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	/// <summary>Returns a string that contains the HRESULT of the error.</summary>
	/// <returns>A string that represents the HRESULT. </returns>
	public override string ToString()
	{
		string message = Message;
		string text = GetType().ToString() + " (0x" + base.HResult.ToString("X8", CultureInfo.InvariantCulture) + ")";
		if (!string.IsNullOrEmpty(message))
		{
			text = text + ": " + message;
		}
		Exception innerException = base.InnerException;
		if (innerException != null)
		{
			text = text + " ---> " + innerException.ToString();
		}
		if (StackTrace != null)
		{
			text = text + Environment.NewLine + StackTrace;
		}
		return text;
	}
}
