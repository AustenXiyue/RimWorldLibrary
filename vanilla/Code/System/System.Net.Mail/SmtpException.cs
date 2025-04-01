using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Net.Mail;

/// <summary>Represents the exception that is thrown when the <see cref="T:System.Net.Mail.SmtpClient" /> is not able to complete a <see cref="Overload:System.Net.Mail.SmtpClient.Send" /> or <see cref="Overload:System.Net.Mail.SmtpClient.SendAsync" /> operation.</summary>
[Serializable]
public class SmtpException : Exception, ISerializable
{
	private SmtpStatusCode statusCode;

	/// <summary>Gets the status code returned by an SMTP server when an e-mail message is transmitted.</summary>
	/// <returns>An <see cref="T:System.Net.Mail.SmtpStatusCode" /> value that indicates the error that occurred.</returns>
	public SmtpStatusCode StatusCode
	{
		get
		{
			return statusCode;
		}
		set
		{
			statusCode = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Mail.SmtpException" /> class. </summary>
	public SmtpException()
		: this(SmtpStatusCode.GeneralFailure)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Mail.SmtpException" /> class with the specified status code.</summary>
	/// <param name="statusCode">An <see cref="T:System.Net.Mail.SmtpStatusCode" /> value.</param>
	public SmtpException(SmtpStatusCode statusCode)
		: this(statusCode, "Syntax error, command unrecognized.")
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Mail.SmtpException" /> class with the specified error message.</summary>
	/// <param name="message">A <see cref="T:System.String" /> that describes the error that occurred.</param>
	public SmtpException(string message)
		: this(SmtpStatusCode.GeneralFailure, message)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Mail.SmtpException" /> class from the specified instances of the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> and <see cref="T:System.Runtime.Serialization.StreamingContext" /> classes. </summary>
	/// <param name="serializationInfo">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that contains the information required to serialize the new <see cref="T:System.Net.Mail.SmtpException" />. </param>
	/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains the source and destination of the serialized stream associated with the new instance. </param>
	protected SmtpException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
		try
		{
			statusCode = (SmtpStatusCode)serializationInfo.GetValue("Status", typeof(int));
		}
		catch (SerializationException)
		{
			statusCode = (SmtpStatusCode)serializationInfo.GetValue("statusCode", typeof(SmtpStatusCode));
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Mail.SmtpException" /> class with the specified status code and error message.</summary>
	/// <param name="statusCode">An <see cref="T:System.Net.Mail.SmtpStatusCode" /> value.</param>
	/// <param name="message">A <see cref="T:System.String" /> that describes the error that occurred.</param>
	public SmtpException(SmtpStatusCode statusCode, string message)
		: base(message)
	{
		this.statusCode = statusCode;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Mail.SmtpException" /> class with the specified error message and inner exception.</summary>
	/// <param name="message">A <see cref="T:System.String" /> that describes the error that occurred.</param>
	/// <param name="innerException">The exception that is the cause of the current exception. </param>
	public SmtpException(string message, Exception innerException)
		: base(message, innerException)
	{
		statusCode = SmtpStatusCode.GeneralFailure;
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> instance with the data needed to serialize the <see cref="T:System.Net.Mail.SmtpException" />.</summary>
	/// <param name="serializationInfo">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data. </param>
	/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that specifies the destination for this serialization.</param>
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		if (serializationInfo == null)
		{
			throw new ArgumentNullException("serializationInfo");
		}
		base.GetObjectData(serializationInfo, streamingContext);
		serializationInfo.AddValue("Status", statusCode, typeof(int));
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> instance with the data needed to serialize the <see cref="T:System.Net.Mail.SmtpException" />.</summary>
	/// <param name="serializationInfo">A <see cref="T:System.Runtime.Serialization.SerializationInfo" />, which holds the serialized data for the <see cref="T:System.Net.Mail.SmtpException" />. </param>
	/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains the destination of the serialized stream associated with the new <see cref="T:System.Net.Mail.SmtpException" />. </param>
	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		GetObjectData(info, context);
	}
}
