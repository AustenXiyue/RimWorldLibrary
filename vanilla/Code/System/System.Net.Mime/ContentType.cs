using System.Collections.Specialized;
using System.Net.Mail;
using System.Text;

namespace System.Net.Mime;

/// <summary>Represents a MIME protocol Content-Type header.</summary>
public class ContentType
{
	private string mediaType;

	private string subType;

	private bool isChanged;

	private string type;

	private bool isPersisted;

	private TrackingStringDictionary parameters;

	internal static readonly string Default = "application/octet-stream";

	/// <summary>Gets or sets the value of the boundary parameter included in the Content-Type header represented by this instance.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the value associated with the boundary parameter.</returns>
	public string Boundary
	{
		get
		{
			return Parameters["boundary"];
		}
		set
		{
			if (value == null || value == string.Empty)
			{
				Parameters.Remove("boundary");
			}
			else
			{
				Parameters["boundary"] = value;
			}
		}
	}

	/// <summary>Gets or sets the value of the charset parameter included in the Content-Type header represented by this instance.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the value associated with the charset parameter.</returns>
	public string CharSet
	{
		get
		{
			return Parameters["charset"];
		}
		set
		{
			if (value == null || value == string.Empty)
			{
				Parameters.Remove("charset");
			}
			else
			{
				Parameters["charset"] = value;
			}
		}
	}

	/// <summary>Gets or sets the media type value included in the Content-Type header represented by this instance.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the media type and subtype value. This value does not include the semicolon (;) separator that follows the subtype.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value specified for a set operation is null.</exception>
	/// <exception cref="T:System.ArgumentException">The value specified for a set operation is <see cref="F:System.String.Empty" /> ("").</exception>
	/// <exception cref="T:System.FormatException">The value specified for a set operation is in a form that cannot be parsed.</exception>
	public string MediaType
	{
		get
		{
			return mediaType + "/" + subType;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value == string.Empty)
			{
				throw new ArgumentException(global::SR.GetString("This property cannot be set to an empty string."), "value");
			}
			int offset = 0;
			mediaType = MailBnfHelper.ReadToken(value, ref offset, null);
			if (mediaType.Length == 0 || offset >= value.Length || value[offset++] != '/')
			{
				throw new FormatException(global::SR.GetString("The specified media type is invalid."));
			}
			subType = MailBnfHelper.ReadToken(value, ref offset, null);
			if (subType.Length == 0 || offset < value.Length)
			{
				throw new FormatException(global::SR.GetString("The specified media type is invalid."));
			}
			isChanged = true;
			isPersisted = false;
		}
	}

	/// <summary>Gets or sets the value of the name parameter included in the Content-Type header represented by this instance.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the value associated with the name parameter. </returns>
	public string Name
	{
		get
		{
			string text = Parameters["name"];
			if (MimeBasePart.DecodeEncoding(text) != null)
			{
				text = MimeBasePart.DecodeHeaderValue(text);
			}
			return text;
		}
		set
		{
			if (value == null || value == string.Empty)
			{
				Parameters.Remove("name");
			}
			else
			{
				Parameters["name"] = value;
			}
		}
	}

	/// <summary>Gets the dictionary that contains the parameters included in the Content-Type header represented by this instance.</summary>
	/// <returns>A writable <see cref="T:System.Collections.Specialized.StringDictionary" /> that contains name and value pairs.</returns>
	public StringDictionary Parameters
	{
		get
		{
			if (parameters == null && type == null)
			{
				parameters = new TrackingStringDictionary();
			}
			return parameters;
		}
	}

	internal bool IsChanged
	{
		get
		{
			if (!isChanged)
			{
				if (parameters != null)
				{
					return parameters.IsChanged;
				}
				return false;
			}
			return true;
		}
	}

	/// <summary>Initializes a new default instance of the <see cref="T:System.Net.Mime.ContentType" /> class. </summary>
	public ContentType()
		: this(Default)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Mime.ContentType" /> class using the specified string. </summary>
	/// <param name="contentType">A <see cref="T:System.String" />, for example, "text/plain; charset=us-ascii", that contains the MIME media type, subtype, and optional parameters.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="contentType" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="contentType" /> is <see cref="F:System.String.Empty" /> ("").</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="contentType" /> is in a form that cannot be parsed.</exception>
	public ContentType(string contentType)
	{
		if (contentType == null)
		{
			throw new ArgumentNullException("contentType");
		}
		if (contentType == string.Empty)
		{
			throw new ArgumentException(global::SR.GetString("The parameter '{0}' cannot be an empty string.", "contentType"), "contentType");
		}
		isChanged = true;
		type = contentType;
		ParseValue();
	}

	internal void Set(string contentType, HeaderCollection headers)
	{
		type = contentType;
		ParseValue();
		headers.InternalSet(MailHeaderInfo.GetString(MailHeaderID.ContentType), ToString());
		isPersisted = true;
	}

	internal void PersistIfNeeded(HeaderCollection headers, bool forcePersist)
	{
		if (IsChanged || !isPersisted || forcePersist)
		{
			headers.InternalSet(MailHeaderInfo.GetString(MailHeaderID.ContentType), ToString());
			isPersisted = true;
		}
	}

	/// <summary>Returns a string representation of this <see cref="T:System.Net.Mime.ContentType" /> object.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the current settings for this <see cref="T:System.Net.Mime.ContentType" />.</returns>
	public override string ToString()
	{
		if (type == null || IsChanged)
		{
			type = Encode(allowUnicode: false);
			isChanged = false;
			parameters.IsChanged = false;
			isPersisted = false;
		}
		return type;
	}

	internal string Encode(bool allowUnicode)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(mediaType);
		stringBuilder.Append('/');
		stringBuilder.Append(subType);
		foreach (string key in Parameters.Keys)
		{
			stringBuilder.Append("; ");
			EncodeToBuffer(key, stringBuilder, allowUnicode);
			stringBuilder.Append('=');
			EncodeToBuffer(parameters[key], stringBuilder, allowUnicode);
		}
		return stringBuilder.ToString();
	}

	private static void EncodeToBuffer(string value, StringBuilder builder, bool allowUnicode)
	{
		Encoding encoding = MimeBasePart.DecodeEncoding(value);
		if (encoding != null)
		{
			builder.Append("\"" + value + "\"");
			return;
		}
		if ((allowUnicode && !MailBnfHelper.HasCROrLF(value)) || MimeBasePart.IsAscii(value, permitCROrLF: false))
		{
			MailBnfHelper.GetTokenOrQuotedString(value, builder, allowUnicode);
			return;
		}
		encoding = Encoding.GetEncoding("utf-8");
		builder.Append("\"" + MimeBasePart.EncodeHeaderValue(value, encoding, MimeBasePart.ShouldUseBase64Encoding(encoding)) + "\"");
	}

	/// <summary>Determines whether the content-type header of the specified <see cref="T:System.Net.Mime.ContentType" /> object is equal to the content-type header of this object.</summary>
	/// <returns>true if the content-type headers are the same; otherwise false.</returns>
	/// <param name="rparam">The <see cref="T:System.Net.Mime.ContentType" /> object to compare with this object.</param>
	public override bool Equals(object rparam)
	{
		if (rparam == null)
		{
			return false;
		}
		return string.Compare(ToString(), rparam.ToString(), StringComparison.OrdinalIgnoreCase) == 0;
	}

	/// <summary>Determines the hash code of the specified <see cref="T:System.Net.Mime.ContentType" /> object</summary>
	/// <returns>An integer hash value.</returns>
	public override int GetHashCode()
	{
		return ToString().ToLowerInvariant().GetHashCode();
	}

	private void ParseValue()
	{
		int offset = 0;
		Exception ex = null;
		parameters = new TrackingStringDictionary();
		try
		{
			mediaType = MailBnfHelper.ReadToken(type, ref offset, null);
			if (mediaType == null || mediaType.Length == 0 || offset >= type.Length || type[offset++] != '/')
			{
				ex = new FormatException(global::SR.GetString("The specified content type is invalid."));
			}
			if (ex == null)
			{
				subType = MailBnfHelper.ReadToken(type, ref offset, null);
				if (subType == null || subType.Length == 0)
				{
					ex = new FormatException(global::SR.GetString("The specified content type is invalid."));
				}
			}
			if (ex == null)
			{
				while (MailBnfHelper.SkipCFWS(type, ref offset))
				{
					if (type[offset++] != ';')
					{
						ex = new FormatException(global::SR.GetString("The specified content type is invalid."));
						break;
					}
					if (!MailBnfHelper.SkipCFWS(type, ref offset))
					{
						break;
					}
					string text = MailBnfHelper.ReadParameterAttribute(type, ref offset, null);
					if (text == null || text.Length == 0)
					{
						ex = new FormatException(global::SR.GetString("The specified content type is invalid."));
						break;
					}
					if (offset >= type.Length || type[offset++] != '=')
					{
						ex = new FormatException(global::SR.GetString("The specified content type is invalid."));
						break;
					}
					if (!MailBnfHelper.SkipCFWS(type, ref offset))
					{
						ex = new FormatException(global::SR.GetString("The specified content type is invalid."));
						break;
					}
					string text2 = ((type[offset] != '"') ? MailBnfHelper.ReadToken(type, ref offset, null) : MailBnfHelper.ReadQuotedString(type, ref offset, null));
					if (text2 == null)
					{
						ex = new FormatException(global::SR.GetString("The specified content type is invalid."));
						break;
					}
					parameters.Add(text, text2);
				}
			}
			parameters.IsChanged = false;
		}
		catch (FormatException)
		{
			throw new FormatException(global::SR.GetString("The specified content type is invalid."));
		}
		if (ex != null)
		{
			throw new FormatException(global::SR.GetString("The specified content type is invalid."));
		}
	}
}
