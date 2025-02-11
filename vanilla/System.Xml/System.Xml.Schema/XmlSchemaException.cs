using System.Resources;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Xml.Schema;

/// <summary>Returns detailed information about the schema exception.</summary>
[Serializable]
public class XmlSchemaException : SystemException
{
	private string res;

	private string[] args;

	private string sourceUri;

	private int lineNumber;

	private int linePosition;

	[NonSerialized]
	private XmlSchemaObject sourceSchemaObject;

	private string message;

	internal string GetRes => res;

	internal string[] Args => args;

	/// <summary>Gets the Uniform Resource Identifier (URI) location of the schema that caused the exception.</summary>
	/// <returns>The URI location of the schema that caused the exception.</returns>
	public string SourceUri => sourceUri;

	/// <summary>Gets the line number indicating where the error occurred.</summary>
	/// <returns>The line number indicating where the error occurred.</returns>
	public int LineNumber => lineNumber;

	/// <summary>Gets the line position indicating where the error occurred.</summary>
	/// <returns>The line position indicating where the error occurred.</returns>
	public int LinePosition => linePosition;

	/// <summary>The XmlSchemaObject that produced the XmlSchemaException.</summary>
	/// <returns>A valid object instance represents a structural validation error in the XML Schema Object Model (SOM).</returns>
	public XmlSchemaObject SourceSchemaObject => sourceSchemaObject;

	/// <summary>Gets the description of the error condition of this exception.</summary>
	/// <returns>The description of the error condition of this exception.</returns>
	public override string Message
	{
		get
		{
			if (message != null)
			{
				return message;
			}
			return base.Message;
		}
	}

	/// <summary>Constructs a new XmlSchemaException object with the given SerializationInfo and StreamingContext information that contains all the properties of the XmlSchemaException.</summary>
	/// <param name="info">SerializationInfo.</param>
	/// <param name="context">StreamingContext.</param>
	protected XmlSchemaException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		res = (string)info.GetValue("res", typeof(string));
		args = (string[])info.GetValue("args", typeof(string[]));
		sourceUri = (string)info.GetValue("sourceUri", typeof(string));
		lineNumber = (int)info.GetValue("lineNumber", typeof(int));
		linePosition = (int)info.GetValue("linePosition", typeof(int));
		string text = null;
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			if (current.Name == "version")
			{
				text = (string)current.Value;
			}
		}
		if (text == null)
		{
			message = CreateMessage(res, args);
		}
		else
		{
			message = null;
		}
	}

	/// <summary>Streams all the XmlSchemaException properties into the SerializationInfo class for the given StreamingContext.</summary>
	/// <param name="info">The SerializationInfo. </param>
	/// <param name="context">The StreamingContext information. </param>
	[SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("res", res);
		info.AddValue("args", args);
		info.AddValue("sourceUri", sourceUri);
		info.AddValue("lineNumber", lineNumber);
		info.AddValue("linePosition", linePosition);
		info.AddValue("version", "2.0");
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaException" /> class.</summary>
	public XmlSchemaException()
		: this(null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaException" /> class with the exception message specified.</summary>
	/// <param name="message">A string description of the error condition.</param>
	public XmlSchemaException(string message)
		: this(message, (Exception)null, 0, 0)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaException" /> class with the exception message and original <see cref="T:System.Exception" /> object that caused this exception specified.</summary>
	/// <param name="message">A string description of the error condition.</param>
	/// <param name="innerException">The original T:System.Exception object that caused this exception.</param>
	public XmlSchemaException(string message, Exception innerException)
		: this(message, innerException, 0, 0)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaException" /> class with the exception message specified, and the original <see cref="T:System.Exception" /> object, line number, and line position of the XML that cause this exception specified.</summary>
	/// <param name="message">A string description of the error condition.</param>
	/// <param name="innerException">The original T:System.Exception object that caused this exception.</param>
	/// <param name="lineNumber">The line number of the XML that caused this exception.</param>
	/// <param name="linePosition">The line position of the XML that caused this exception.</param>
	public XmlSchemaException(string message, Exception innerException, int lineNumber, int linePosition)
		: this((message == null) ? "A schema error occurred." : "{0}", new string[1] { message }, innerException, null, lineNumber, linePosition, null)
	{
	}

	internal XmlSchemaException(string res, string[] args)
		: this(res, args, null, null, 0, 0, null)
	{
	}

	internal XmlSchemaException(string res, string arg)
		: this(res, new string[1] { arg }, null, null, 0, 0, null)
	{
	}

	internal XmlSchemaException(string res, string arg, string sourceUri, int lineNumber, int linePosition)
		: this(res, new string[1] { arg }, null, sourceUri, lineNumber, linePosition, null)
	{
	}

	internal XmlSchemaException(string res, string sourceUri, int lineNumber, int linePosition)
		: this(res, null, null, sourceUri, lineNumber, linePosition, null)
	{
	}

	internal XmlSchemaException(string res, string[] args, string sourceUri, int lineNumber, int linePosition)
		: this(res, args, null, sourceUri, lineNumber, linePosition, null)
	{
	}

	internal XmlSchemaException(string res, XmlSchemaObject source)
		: this(res, (string[])null, source)
	{
	}

	internal XmlSchemaException(string res, string arg, XmlSchemaObject source)
		: this(res, new string[1] { arg }, source)
	{
	}

	internal XmlSchemaException(string res, string[] args, XmlSchemaObject source)
		: this(res, args, null, source.SourceUri, source.LineNumber, source.LinePosition, source)
	{
	}

	internal XmlSchemaException(string res, string[] args, Exception innerException, string sourceUri, int lineNumber, int linePosition, XmlSchemaObject source)
		: base(CreateMessage(res, args), innerException)
	{
		base.HResult = -2146231999;
		this.res = res;
		this.args = args;
		this.sourceUri = sourceUri;
		this.lineNumber = lineNumber;
		this.linePosition = linePosition;
		sourceSchemaObject = source;
	}

	internal static string CreateMessage(string res, string[] args)
	{
		try
		{
			return Res.GetString(res, args);
		}
		catch (MissingManifestResourceException)
		{
			return "UNKNOWN(" + res + ")";
		}
	}

	internal void SetSource(string sourceUri, int lineNumber, int linePosition)
	{
		this.sourceUri = sourceUri;
		this.lineNumber = lineNumber;
		this.linePosition = linePosition;
	}

	internal void SetSchemaObject(XmlSchemaObject source)
	{
		sourceSchemaObject = source;
	}

	internal void SetSource(XmlSchemaObject source)
	{
		sourceSchemaObject = source;
		sourceUri = source.SourceUri;
		lineNumber = source.LineNumber;
		linePosition = source.LinePosition;
	}

	internal void SetResourceId(string resourceId)
	{
		res = resourceId;
	}
}
