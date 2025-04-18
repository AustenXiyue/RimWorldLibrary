namespace System.Net.Mime;

/// <summary>Specifies the Content-Transfer-Encoding header information for an e-mail message attachment.</summary>
public enum TransferEncoding
{
	/// <summary>Encodes data that consists of printable characters in the US-ASCII character set. See RFC 2406 Section 6.7.</summary>
	QuotedPrintable = 0,
	/// <summary>Encodes stream-based data. See RFC 2406 Section 6.8.</summary>
	Base64 = 1,
	/// <summary>Used for data that is not encoded. The data is in 7-bit US-ASCII characters with a total line length of no longer than 1000 characters. See RFC2406 Section 2.7.</summary>
	SevenBit = 2,
	/// <summary>The data is in 8-bit characters that may represent international characters with a total line length of no longer than 1000 8-bit characters. For more information about this 8-bit MIME transport extension, see IETF RFC 6152.</summary>
	EightBit = 3,
	/// <summary>Indicates that the transfer encoding is unknown.</summary>
	Unknown = -1
}
