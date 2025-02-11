using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.ComponentModel.Design;

/// <summary>The exception that is thrown when an attempt to check out a file that is checked into a source code management program is canceled or fails.</summary>
[Serializable]
[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
public class CheckoutException : ExternalException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.CheckoutException" /> class that specifies that the check out was canceled. This field is read-only.</summary>
	public static readonly CheckoutException Canceled = new CheckoutException(global::SR.GetString("The checkout was canceled by the user."), -2147467260);

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.CheckoutException" /> class with no associated message or error code.</summary>
	public CheckoutException()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.CheckoutException" /> class with the specified message.</summary>
	/// <param name="message">A message describing the exception. </param>
	public CheckoutException(string message)
		: base(message)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.CheckoutException" /> class with the specified message and error code.</summary>
	/// <param name="message">A message describing the exception. </param>
	/// <param name="errorCode">The error code to pass. </param>
	public CheckoutException(string message, int errorCode)
		: base(message, errorCode)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.CheckoutException" /> class using the specified serialization data and context. </summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to be used for deserialization.</param>
	/// <param name="context">The destination to be used for deserialization.</param>
	protected CheckoutException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.CheckoutException" /> class with the specified detailed description and the specified exception. </summary>
	/// <param name="message">A detailed description of the error.</param>
	/// <param name="innerException">A reference to the inner exception that is the cause of this exception.</param>
	public CheckoutException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
