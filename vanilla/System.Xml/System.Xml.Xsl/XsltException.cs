using System.Globalization;
using System.Resources;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml.Utils;

namespace System.Xml.Xsl;

/// <summary>The exception that is thrown when an error occurs while processing an XSLT transformation.</summary>
[Serializable]
public class XsltException : SystemException
{
	private string res;

	private string[] args;

	private string sourceUri;

	private int lineNumber;

	private int linePosition;

	private string message;

	/// <summary>Gets the location path of the style sheet.</summary>
	/// <returns>The location path of the style sheet.</returns>
	public virtual string SourceUri => sourceUri;

	/// <summary>Gets the line number indicating where the error occurred in the style sheet.</summary>
	/// <returns>The line number indicating where the error occurred in the style sheet.</returns>
	public virtual int LineNumber => lineNumber;

	/// <summary>Gets the line position indicating where the error occurred in the style sheet.</summary>
	/// <returns>The line position indicating where the error occurred in the style sheet.</returns>
	public virtual int LinePosition => linePosition;

	/// <summary>Gets the formatted error message describing the current exception.</summary>
	/// <returns>The formatted error message describing the current exception.</returns>
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

	/// <summary>Initializes a new instance of the XsltException class using the information in the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> and <see cref="T:System.Runtime.Serialization.StreamingContext" /> objects.</summary>
	/// <param name="info">The SerializationInfo object containing all the properties of an XsltException. </param>
	/// <param name="context">The StreamingContext object. </param>
	protected XsltException(SerializationInfo info, StreamingContext context)
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
			message = CreateMessage(res, args, sourceUri, lineNumber, linePosition);
		}
		else
		{
			message = null;
		}
	}

	/// <summary>Streams all the XsltException properties into the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> class for the given <see cref="T:System.Runtime.Serialization.StreamingContext" />.</summary>
	/// <param name="info">The SerializationInfo object. </param>
	/// <param name="context">The StreamingContext object. </param>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Xsl.XsltException" /> class.</summary>
	public XsltException()
		: this(string.Empty, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Xsl.XsltException" /> class with a specified error message. </summary>
	/// <param name="message">The message that describes the error.</param>
	public XsltException(string message)
		: this(message, null)
	{
	}

	/// <summary>Initializes a new instance of the XsltException class.</summary>
	/// <param name="message">The description of the error condition. </param>
	/// <param name="innerException">The <see cref="T:System.Exception" /> which threw the XsltException, if any. This value can be null. </param>
	public XsltException(string message, Exception innerException)
		: this("{0}", new string[1] { message }, null, 0, 0, innerException)
	{
	}

	internal static XsltException Create(string res, params string[] args)
	{
		return new XsltException(res, args, null, 0, 0, null);
	}

	internal static XsltException Create(string res, string[] args, Exception inner)
	{
		return new XsltException(res, args, null, 0, 0, inner);
	}

	internal XsltException(string res, string[] args, string sourceUri, int lineNumber, int linePosition, Exception inner)
		: base(CreateMessage(res, args, sourceUri, lineNumber, linePosition), inner)
	{
		base.HResult = -2146231998;
		this.res = res;
		this.sourceUri = sourceUri;
		this.lineNumber = lineNumber;
		this.linePosition = linePosition;
	}

	private static string CreateMessage(string res, string[] args, string sourceUri, int lineNumber, int linePosition)
	{
		try
		{
			string text = FormatMessage(res, args);
			if (res != "XSLT compile error at {0}({1},{2}). See InnerException for details." && lineNumber != 0)
			{
				text = text + " " + FormatMessage("An error occurred at {0}({1},{2}).", sourceUri, lineNumber.ToString(CultureInfo.InvariantCulture), linePosition.ToString(CultureInfo.InvariantCulture));
			}
			return text;
		}
		catch (MissingManifestResourceException)
		{
			return "UNKNOWN(" + res + ")";
		}
	}

	private static string FormatMessage(string key, params string[] args)
	{
		string text = System.Xml.Utils.Res.GetString(key);
		if (text != null && args != null)
		{
			text = string.Format(CultureInfo.InvariantCulture, text, args);
		}
		return text;
	}
}
