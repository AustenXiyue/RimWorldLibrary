using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Net.Mail;
using System.Text;

namespace System.Net.Mime;

/// <summary>Represents a MIME protocol Content-Disposition header.</summary>
public class ContentDisposition
{
	private string dispositionType;

	private TrackingValidationObjectDictionary parameters;

	private bool isChanged;

	private bool isPersisted;

	private string disposition;

	private const string creationDate = "creation-date";

	private const string readDate = "read-date";

	private const string modificationDate = "modification-date";

	private const string size = "size";

	private const string fileName = "filename";

	private static readonly TrackingValidationObjectDictionary.ValidateAndParseValue dateParser;

	private static readonly TrackingValidationObjectDictionary.ValidateAndParseValue longParser;

	private static readonly IDictionary<string, TrackingValidationObjectDictionary.ValidateAndParseValue> validators;

	/// <summary>Gets or sets the disposition type for an e-mail attachment.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the disposition type. The value is not restricted but is typically one of the <see cref="P:System.Net.Mime.ContentDisposition.DispositionType" /> values.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value specified for a set operation is null.</exception>
	/// <exception cref="T:System.ArgumentException">The value specified for a set operation is equal to <see cref="F:System.String.Empty" /> ("").</exception>
	public string DispositionType
	{
		get
		{
			return dispositionType;
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
			isChanged = true;
			dispositionType = value;
		}
	}

	/// <summary>Gets the parameters included in the Content-Disposition header represented by this instance.</summary>
	/// <returns>A writable <see cref="T:System.Collections.Specialized.StringDictionary" /> that contains parameter name/value pairs.</returns>
	public StringDictionary Parameters
	{
		get
		{
			if (parameters == null)
			{
				parameters = new TrackingValidationObjectDictionary(validators);
			}
			return parameters;
		}
	}

	/// <summary>Gets or sets the suggested file name for an e-mail attachment.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the file name. </returns>
	public string FileName
	{
		get
		{
			return Parameters["filename"];
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				Parameters.Remove("filename");
			}
			else
			{
				Parameters["filename"] = value;
			}
		}
	}

	/// <summary>Gets or sets the creation date for a file attachment.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> value that indicates the file creation date; otherwise, <see cref="F:System.DateTime.MinValue" /> if no date was specified.</returns>
	public DateTime CreationDate
	{
		get
		{
			return GetDateParameter("creation-date");
		}
		set
		{
			SmtpDateTime value2 = new SmtpDateTime(value);
			((TrackingValidationObjectDictionary)Parameters).InternalSet("creation-date", value2);
		}
	}

	/// <summary>Gets or sets the modification date for a file attachment.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> value that indicates the file modification date; otherwise, <see cref="F:System.DateTime.MinValue" /> if no date was specified.</returns>
	public DateTime ModificationDate
	{
		get
		{
			return GetDateParameter("modification-date");
		}
		set
		{
			SmtpDateTime value2 = new SmtpDateTime(value);
			((TrackingValidationObjectDictionary)Parameters).InternalSet("modification-date", value2);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that determines the disposition type (Inline or Attachment) for an e-mail attachment.</summary>
	/// <returns>true if content in the attachment is presented inline as part of the e-mail body; otherwise, false. </returns>
	public bool Inline
	{
		get
		{
			return dispositionType == "inline";
		}
		set
		{
			isChanged = true;
			if (value)
			{
				dispositionType = "inline";
			}
			else
			{
				dispositionType = "attachment";
			}
		}
	}

	/// <summary>Gets or sets the read date for a file attachment.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> value that indicates the file read date; otherwise, <see cref="F:System.DateTime.MinValue" /> if no date was specified.</returns>
	public DateTime ReadDate
	{
		get
		{
			return GetDateParameter("read-date");
		}
		set
		{
			SmtpDateTime value2 = new SmtpDateTime(value);
			((TrackingValidationObjectDictionary)Parameters).InternalSet("read-date", value2);
		}
	}

	/// <summary>Gets or sets the size of a file attachment.</summary>
	/// <returns>A <see cref="T:System.Int32" /> that specifies the number of bytes in the file attachment. The default value is -1, which indicates that the file size is unknown.</returns>
	public long Size
	{
		get
		{
			object obj = ((TrackingValidationObjectDictionary)Parameters).InternalGet("size");
			if (obj == null)
			{
				return -1L;
			}
			return (long)obj;
		}
		set
		{
			((TrackingValidationObjectDictionary)Parameters).InternalSet("size", value);
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

	static ContentDisposition()
	{
		dateParser = (object value) => new SmtpDateTime(value.ToString());
		longParser = delegate(object value)
		{
			if (!long.TryParse(value.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out var result))
			{
				throw new FormatException(global::SR.GetString("The specified content disposition is invalid."));
			}
			return result;
		};
		validators = new Dictionary<string, TrackingValidationObjectDictionary.ValidateAndParseValue>();
		validators.Add("creation-date", dateParser);
		validators.Add("modification-date", dateParser);
		validators.Add("read-date", dateParser);
		validators.Add("size", longParser);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Mime.ContentDisposition" /> class with a <see cref="P:System.Net.Mime.ContentDisposition.DispositionType" /> of <see cref="F:System.Net.Mime.DispositionTypeNames.Attachment" />. </summary>
	public ContentDisposition()
	{
		isChanged = true;
		dispositionType = "attachment";
		disposition = dispositionType;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Mime.ContentDisposition" /> class with the specified disposition information.</summary>
	/// <param name="disposition">A <see cref="T:System.Net.Mime.DispositionTypeNames" /> value that contains the disposition.</param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="disposition" /> is null or equal to <see cref="F:System.String.Empty" /> ("").</exception>
	public ContentDisposition(string disposition)
	{
		if (disposition == null)
		{
			throw new ArgumentNullException("disposition");
		}
		isChanged = true;
		this.disposition = disposition;
		ParseValue();
	}

	internal DateTime GetDateParameter(string parameterName)
	{
		if (!(((TrackingValidationObjectDictionary)Parameters).InternalGet(parameterName) is SmtpDateTime smtpDateTime))
		{
			return DateTime.MinValue;
		}
		return smtpDateTime.Date;
	}

	internal void Set(string contentDisposition, HeaderCollection headers)
	{
		disposition = contentDisposition;
		ParseValue();
		headers.InternalSet(MailHeaderInfo.GetString(MailHeaderID.ContentDisposition), ToString());
		isPersisted = true;
	}

	internal void PersistIfNeeded(HeaderCollection headers, bool forcePersist)
	{
		if (IsChanged || !isPersisted || forcePersist)
		{
			headers.InternalSet(MailHeaderInfo.GetString(MailHeaderID.ContentDisposition), ToString());
			isPersisted = true;
		}
	}

	/// <summary>Returns a <see cref="T:System.String" /> representation of this instance.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the property values for this instance.</returns>
	public override string ToString()
	{
		if (disposition == null || isChanged || (parameters != null && parameters.IsChanged))
		{
			disposition = Encode(allowUnicode: false);
			isChanged = false;
			parameters.IsChanged = false;
			isPersisted = false;
		}
		return disposition;
	}

	internal string Encode(bool allowUnicode)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(dispositionType);
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

	/// <summary>Determines whether the content-disposition header of the specified <see cref="T:System.Net.Mime.ContentDisposition" /> object is equal to the content-disposition header of this object.</summary>
	/// <returns>true if the content-disposition headers are the same; otherwise false.</returns>
	/// <param name="rparam">The <see cref="T:System.Net.Mime.ContentDisposition" /> object to compare with this object.</param>
	public override bool Equals(object rparam)
	{
		if (rparam == null)
		{
			return false;
		}
		return string.Compare(ToString(), rparam.ToString(), StringComparison.OrdinalIgnoreCase) == 0;
	}

	/// <summary>Determines the hash code of the specified <see cref="T:System.Net.Mime.ContentDisposition" /> object</summary>
	/// <returns>An integer hash value.</returns>
	public override int GetHashCode()
	{
		return ToString().ToLowerInvariant().GetHashCode();
	}

	private void ParseValue()
	{
		int offset = 0;
		try
		{
			dispositionType = MailBnfHelper.ReadToken(disposition, ref offset, null);
			if (string.IsNullOrEmpty(dispositionType))
			{
				throw new FormatException(global::SR.GetString("The mail header is malformed."));
			}
			if (parameters == null)
			{
				parameters = new TrackingValidationObjectDictionary(validators);
			}
			else
			{
				parameters.Clear();
			}
			while (MailBnfHelper.SkipCFWS(disposition, ref offset))
			{
				if (disposition[offset++] != ';')
				{
					throw new FormatException(global::SR.GetString("An invalid character was found in the mail header: '{0}'.", disposition[offset - 1]));
				}
				if (MailBnfHelper.SkipCFWS(disposition, ref offset))
				{
					string text = MailBnfHelper.ReadParameterAttribute(disposition, ref offset, null);
					if (disposition[offset++] != '=')
					{
						throw new FormatException(global::SR.GetString("The mail header is malformed."));
					}
					if (!MailBnfHelper.SkipCFWS(disposition, ref offset))
					{
						throw new FormatException(global::SR.GetString("The specified content disposition is invalid."));
					}
					string value = ((disposition[offset] != '"') ? MailBnfHelper.ReadToken(disposition, ref offset, null) : MailBnfHelper.ReadQuotedString(disposition, ref offset, null));
					if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(value))
					{
						throw new FormatException(global::SR.GetString("The specified content disposition is invalid."));
					}
					Parameters.Add(text, value);
					continue;
				}
				break;
			}
		}
		catch (FormatException innerException)
		{
			throw new FormatException(global::SR.GetString("The specified content disposition is invalid."), innerException);
		}
		parameters.IsChanged = false;
	}
}
