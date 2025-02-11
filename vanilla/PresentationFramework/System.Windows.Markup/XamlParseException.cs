using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using MS.Internal;

namespace System.Windows.Markup;

/// <summary>Represents the exception class for parser-specific exceptions from a WPF XAML parser. This exception is used in XAML API or WPF XAML parser operations from .NET Framework 3.0 and .NET Framework 3.5, or for specific use of the WPF XAML parser by calling <see cref="T:System.Windows.Markup.XamlReader" /> API. </summary>
[Serializable]
public class XamlParseException : SystemException
{
	[Flags]
	private enum ContextBits
	{
		Type = 1,
		File = 2,
		Line = 4
	}

	internal const string BamlExt = ".baml";

	internal const string XamlExt = ".xaml";

	private int _lineNumber;

	private int _linePosition;

	private object _keyContext;

	private string _uidContext;

	private string _nameContext;

	private Uri _baseUri;

	/// <summary>Gets the line number where the exception occurred. </summary>
	/// <returns>The line number.</returns>
	public int LineNumber
	{
		get
		{
			return _lineNumber;
		}
		internal set
		{
			_lineNumber = value;
		}
	}

	/// <summary>Gets the position in the line where the exception occurred. </summary>
	/// <returns>The line position.</returns>
	public int LinePosition
	{
		get
		{
			return _linePosition;
		}
		internal set
		{
			_linePosition = value;
		}
	}

	/// <summary>Gets or sets the key value of the item in a dictionary where the exception occurred. </summary>
	/// <returns>The relevant XAML x:Key value.</returns>
	public object KeyContext
	{
		get
		{
			return _keyContext;
		}
		internal set
		{
			_keyContext = value;
		}
	}

	/// <summary>Gets or sets the x:Uid Directive of the object where the exception occurred. </summary>
	/// <returns>The value of the Uid string.</returns>
	public string UidContext
	{
		get
		{
			return _uidContext;
		}
		internal set
		{
			_uidContext = value;
		}
	}

	/// <summary>Gets or sets the XAML name of the object where the exception occurred.</summary>
	/// <returns>The XAML name of the object.</returns>
	public string NameContext
	{
		get
		{
			return _nameContext;
		}
		internal set
		{
			_nameContext = value;
		}
	}

	/// <summary>Gets base URI information when the exception is thrown.</summary>
	/// <returns>The parser context base URI. </returns>
	public Uri BaseUri
	{
		get
		{
			return _baseUri;
		}
		internal set
		{
			_baseUri = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XamlParseException" /> class.</summary>
	public XamlParseException()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XamlParseException" /> class, using the specified exception message string.</summary>
	/// <param name="message">The exception message.</param>
	public XamlParseException(string message)
		: base(message)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XamlParseException" /> class, using the specified exception message string and inner exception. </summary>
	/// <param name="message">The exception message.</param>
	/// <param name="innerException">The initial exception that occurred.</param>
	public XamlParseException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XamlParseException" /> class, using the specified exception message string, and the specified line number and position in the line.</summary>
	/// <param name="message">The exception message.</param>
	/// <param name="lineNumber">The line number where the exception occurred.</param>
	/// <param name="linePosition">The position in the line at which the exception occurred.</param>
	public XamlParseException(string message, int lineNumber, int linePosition)
		: this(message)
	{
		_lineNumber = lineNumber;
		_linePosition = linePosition;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XamlParseException" /> class, using the specified exception message, inner exception, line number, and position in the line.</summary>
	/// <param name="message">The exception message.</param>
	/// <param name="lineNumber">The line number where the exception occurred.</param>
	/// <param name="linePosition">The position in the line at which the exception occurred.</param>
	/// <param name="innerException">The initial exception that occurred.</param>
	public XamlParseException(string message, int lineNumber, int linePosition, Exception innerException)
		: this(message, innerException)
	{
		_lineNumber = lineNumber;
		_linePosition = linePosition;
	}

	internal XamlParseException(string message, int lineNumber, int linePosition, Uri baseUri, Exception innerException)
		: this(message, innerException)
	{
		_lineNumber = lineNumber;
		_linePosition = linePosition;
		_baseUri = baseUri;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XamlParseException" /> class. </summary>
	/// <param name="info">Contains all the information that is required to serialize or deserialize the object.</param>
	/// <param name="context">The source and destination of a serialized stream.</param>
	protected XamlParseException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		_lineNumber = info.GetInt32("Line");
		_linePosition = info.GetInt32("Position");
	}

	/// <summary>Gets the data that is required to serialize the specified object by populating the specified <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object.</summary>
	/// <param name="info">The serialization information object to add the serialization data to.</param>
	/// <param name="context">The destination for this serialization.</param>
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Line", _lineNumber);
		info.AddValue("Position", _linePosition);
	}

	internal static string GetMarkupFilePath(Uri resourceUri)
	{
		string empty = string.Empty;
		string text = string.Empty;
		if (resourceUri != null)
		{
			empty = ((!resourceUri.IsAbsoluteUri) ? resourceUri.OriginalString : resourceUri.GetComponents(UriComponents.Path, UriFormat.Unescaped));
			text = empty.Replace(".baml", ".xaml");
			if (-1 == text.LastIndexOf(".xaml", StringComparison.Ordinal))
			{
				text = string.Empty;
			}
		}
		return text;
	}

	internal static string GenerateErrorMessageContext(int lineNumber, int linePosition, Uri baseUri, XamlObjectIds xamlObjectIds, Type objectType)
	{
		string result = " ";
		string markupFilePath = GetMarkupFilePath(baseUri);
		string text = null;
		if (xamlObjectIds != null)
		{
			if (xamlObjectIds.Name != null)
			{
				text = xamlObjectIds.Name;
			}
			else if (xamlObjectIds.Key != null)
			{
				text = xamlObjectIds.Key.ToString();
			}
			else if (xamlObjectIds.Uid != null)
			{
				text = xamlObjectIds.Uid;
			}
		}
		if (text == null && objectType != null)
		{
			text = objectType.ToString();
		}
		ContextBits contextBits = (ContextBits)0;
		if (text != null)
		{
			contextBits |= ContextBits.Type;
		}
		if (!string.IsNullOrEmpty(markupFilePath))
		{
			contextBits |= ContextBits.File;
		}
		if (lineNumber > 0)
		{
			contextBits |= ContextBits.Line;
		}
		switch (contextBits)
		{
		case (ContextBits)0:
			result = string.Empty;
			break;
		case ContextBits.Type:
			result = SR.Format(SR.ParserErrorContext_Type, text);
			break;
		case ContextBits.File:
			result = SR.Format(SR.ParserErrorContext_File, markupFilePath);
			break;
		case ContextBits.Type | ContextBits.File:
			result = SR.Format(SR.ParserErrorContext_Type_File, text, markupFilePath);
			break;
		case ContextBits.Line:
			result = SR.Format(SR.ParserErrorContext_Line, lineNumber, linePosition);
			break;
		case ContextBits.Type | ContextBits.Line:
			result = SR.Format(SR.ParserErrorContext_Type_Line, text, lineNumber, linePosition);
			break;
		case ContextBits.File | ContextBits.Line:
			result = SR.Format(SR.ParserErrorContext_File_Line, markupFilePath, lineNumber, linePosition);
			break;
		case ContextBits.Type | ContextBits.File | ContextBits.Line:
			result = SR.Format(SR.ParserErrorContext_Type_File_Line, text, markupFilePath, lineNumber, linePosition);
			break;
		}
		return result;
	}

	internal static void ThrowException(string message, Exception innerException, int lineNumber, int linePosition, Uri baseUri, XamlObjectIds currentXamlObjectIds, XamlObjectIds contextXamlObjectIds, Type objectType)
	{
		if (innerException != null && innerException.Message != null)
		{
			StringBuilder stringBuilder = new StringBuilder(message);
			if (innerException.Message != string.Empty)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append(innerException.Message);
			message = stringBuilder.ToString();
		}
		string text = GenerateErrorMessageContext(lineNumber, linePosition, baseUri, currentXamlObjectIds, objectType);
		message = message + "  " + text;
		XamlParseException ex = ((innerException is TargetInvocationException && innerException.InnerException is XamlParseException) ? ((XamlParseException)innerException.InnerException) : ((lineNumber <= 0) ? new XamlParseException(message, innerException) : new XamlParseException(message, lineNumber, linePosition, innerException)));
		if (contextXamlObjectIds != null)
		{
			ex.NameContext = contextXamlObjectIds.Name;
			ex.UidContext = contextXamlObjectIds.Uid;
			ex.KeyContext = contextXamlObjectIds.Key;
		}
		ex.BaseUri = baseUri;
		if (TraceMarkup.IsEnabled)
		{
			TraceMarkup.TraceActivityItem(TraceMarkup.ThrowException, ex);
		}
		throw ex;
	}

	internal static void ThrowException(ParserContext parserContext, int lineNumber, int linePosition, string message, Exception innerException)
	{
		ThrowException(message, innerException, lineNumber, linePosition);
	}

	internal static void ThrowException(string message, Exception innerException, int lineNumber, int linePosition)
	{
		ThrowException(message, innerException, lineNumber, linePosition, null, null, null, null);
	}
}
