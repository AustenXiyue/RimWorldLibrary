using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Specifies an exception that is handled as a warning instead of an error.</summary>
[Serializable]
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class WarningException : SystemException
{
	private readonly string helpUrl;

	private readonly string helpTopic;

	/// <summary>Gets the Help file associated with the warning.</summary>
	/// <returns>The Help file associated with the warning.</returns>
	public string HelpUrl => helpUrl;

	/// <summary>Gets the Help topic associated with the warning.</summary>
	/// <returns>The Help topic associated with the warning.</returns>
	public string HelpTopic => helpTopic;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.WarningException" /> class. </summary>
	public WarningException()
		: this(null, null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.WarningException" /> class with the specified message and no Help file.</summary>
	/// <param name="message">The message to display to the end user. </param>
	public WarningException(string message)
		: this(message, null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.WarningException" /> class with the specified message, and with access to the specified Help file.</summary>
	/// <param name="message">The message to display to the end user. </param>
	/// <param name="helpUrl">The Help file to display if the user requests help. </param>
	public WarningException(string message, string helpUrl)
		: this(message, helpUrl, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.WarningException" /> class with the specified detailed description and the specified exception. </summary>
	/// <param name="message">A detailed description of the error.</param>
	/// <param name="innerException">A reference to the inner exception that is the cause of this exception.</param>
	public WarningException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.WarningException" /> class with the specified message, and with access to the specified Help file and topic.</summary>
	/// <param name="message">The message to display to the end user. </param>
	/// <param name="helpUrl">The Help file to display if the user requests help. </param>
	/// <param name="helpTopic">The Help topic to display if the user requests help. </param>
	public WarningException(string message, string helpUrl, string helpTopic)
		: base(message)
	{
		this.helpUrl = helpUrl;
		this.helpTopic = helpTopic;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.WarningException" /> class using the specified serialization data and context.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to be used for deserialization.</param>
	/// <param name="context">The destination to be used for deserialization.</param>
	protected WarningException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		helpUrl = (string)info.GetValue("helpUrl", typeof(string));
		helpTopic = (string)info.GetValue("helpTopic", typeof(string));
	}

	/// <summary>Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the parameter name and additional exception information.</summary>
	/// <param name="info">Stores the data that was being used to serialize or deserialize the object that the <see cref="T:System.ComponentModel.Design.Serialization.CodeDomSerializer" /> was serializing or deserializing. </param>
	/// <param name="context">Describes the source and destination of the stream that generated the exception, as well as a means for serialization to retain that context and an additional caller-defined context. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null.</exception>
	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("helpUrl", helpUrl);
		info.AddValue("helpTopic", helpTopic);
		base.GetObjectData(info, context);
	}
}
