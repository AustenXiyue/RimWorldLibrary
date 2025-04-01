using System.Globalization;
using System.Runtime.Serialization;
using System.Security;
using Microsoft.Win32;

namespace System.Runtime.InteropServices;

/// <summary>The exception that is thrown when an unrecognized HRESULT is returned from a COM method call.</summary>
[Serializable]
[ComVisible(true)]
public class COMException : ExternalException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.COMException" /> class with default values.</summary>
	public COMException()
		: base(Environment.GetResourceString("Error HRESULT E_FAIL has been returned from a call to a COM component."))
	{
		SetErrorCode(-2147467259);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.COMException" /> class with a specified message.</summary>
	/// <param name="message">The message that indicates the reason for the exception. </param>
	public COMException(string message)
		: base(message)
	{
		SetErrorCode(-2147467259);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.COMException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public COMException(string message, Exception inner)
		: base(message, inner)
	{
		SetErrorCode(-2147467259);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.COMException" /> class with a specified message and error code.</summary>
	/// <param name="message">The message that indicates the reason the exception occurred. </param>
	/// <param name="errorCode">The error code (HRESULT) value associated with this exception. </param>
	public COMException(string message, int errorCode)
		: base(message)
	{
		SetErrorCode(errorCode);
	}

	[SecuritySafeCritical]
	internal COMException(int hresult)
		: base(Win32Native.GetMessage(hresult))
	{
		SetErrorCode(hresult);
	}

	internal COMException(string message, int hresult, Exception inner)
		: base(message, inner)
	{
		SetErrorCode(hresult);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.COMException" /> class from serialization data.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that holds the serialized object data. </param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> object that supplies the contextual information about the source or destination. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null. </exception>
	protected COMException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	/// <summary>Converts the contents of the exception to a string.</summary>
	/// <returns>A string containing the <see cref="P:System.Exception.HResult" />, <see cref="P:System.Exception.Message" />, <see cref="P:System.Exception.InnerException" />, and <see cref="P:System.Exception.StackTrace" /> properties of the exception.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override string ToString()
	{
		string message = Message;
		string text = GetType().ToString() + " (0x" + base.HResult.ToString("X8", CultureInfo.InvariantCulture) + ")";
		if (message != null && message.Length > 0)
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
