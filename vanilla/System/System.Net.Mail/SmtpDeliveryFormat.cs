namespace System.Net.Mail;

/// <summary>The delivery format to use for sending outgoing e-mail using the Simple Mail Transport Protocol (SMTP).</summary>
public enum SmtpDeliveryFormat
{
	/// <summary>A delivery format using 7-bit ASCII. The traditional delivery format used in the Simple Mail Transport Protocol (SMTP) for mail messages.</summary>
	SevenBit,
	/// <summary>A delivery format where non-ASCII characters in the envelope and header fields used in the Simple Mail Transport Protocol (SMTP) for mail messages are encoded with UTF-8 characters. The extensions to support international e-mail are defined in IETF RFC 6530, 6531, and 6532.</summary>
	International
}
