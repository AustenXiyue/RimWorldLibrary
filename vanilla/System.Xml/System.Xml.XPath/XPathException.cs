using System.Resources;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Xml.XPath;

/// <summary>Provides the exception thrown when an error occurs while processing an XPath expression. </summary>
[Serializable]
public class XPathException : SystemException
{
	private string res;

	private string[] args;

	private string message;

	/// <summary>Gets the description of the error condition for this exception.</summary>
	/// <returns>The string description of the error condition for this exception.</returns>
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

	/// <summary>Uses the information in the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> and <see cref="T:System.Runtime.Serialization.StreamingContext" /> objects to initialize a new instance of the <see cref="T:System.Xml.XPath.XPathException" /> class.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that contains all the properties of an <see cref="T:System.Xml.XPath.XPathException" />. </param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> object. </param>
	protected XPathException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		res = (string)info.GetValue("res", typeof(string));
		args = (string[])info.GetValue("args", typeof(string[]));
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

	/// <summary>Streams all the <see cref="T:System.Xml.XPath.XPathException" /> properties into the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> class for the specified <see cref="T:System.Runtime.Serialization.StreamingContext" />.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object.</param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> object.</param>
	[SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("res", res);
		info.AddValue("args", args);
		info.AddValue("version", "2.0");
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XPath.XPathException" /> class.</summary>
	public XPathException()
		: this(string.Empty, (Exception)null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XPath.XPathException" /> class with the specified exception message.</summary>
	/// <param name="message">The description of the error condition.</param>
	public XPathException(string message)
		: this(message, (Exception)null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XPath.XPathException" /> class using the specified exception message and <see cref="T:System.Exception" /> object.</summary>
	/// <param name="message">The description of the error condition. </param>
	/// <param name="innerException">The <see cref="T:System.Exception" /> that threw the <see cref="T:System.Xml.XPath.XPathException" />, if any. This value can be null. </param>
	public XPathException(string message, Exception innerException)
		: this("{0}", new string[1] { message }, innerException)
	{
	}

	internal static XPathException Create(string res)
	{
		return new XPathException(res, (string[])null);
	}

	internal static XPathException Create(string res, string arg)
	{
		return new XPathException(res, new string[1] { arg });
	}

	internal static XPathException Create(string res, string arg, string arg2)
	{
		return new XPathException(res, new string[2] { arg, arg2 });
	}

	internal static XPathException Create(string res, string arg, Exception innerException)
	{
		return new XPathException(res, new string[1] { arg }, innerException);
	}

	private XPathException(string res, string[] args)
		: this(res, args, null)
	{
	}

	private XPathException(string res, string[] args, Exception inner)
		: base(CreateMessage(res, args), inner)
	{
		base.HResult = -2146231997;
		this.res = res;
		this.args = args;
	}

	private static string CreateMessage(string res, string[] args)
	{
		try
		{
			string text = Res.GetString(res, args);
			if (text == null)
			{
				text = "UNKNOWN(" + res + ")";
			}
			return text;
		}
		catch (MissingManifestResourceException)
		{
			return "UNKNOWN(" + res + ")";
		}
	}
}
