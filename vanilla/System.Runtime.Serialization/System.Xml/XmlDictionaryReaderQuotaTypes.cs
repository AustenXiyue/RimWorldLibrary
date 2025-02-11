namespace System.Xml;

/// <summary>Enumerates the configurable quota values for XmlDictionaryReaders.</summary>
[Flags]
public enum XmlDictionaryReaderQuotaTypes
{
	/// <summary>Specifies the maximum nested node depth.</summary>
	MaxDepth = 1,
	/// <summary>Specifies the maximum string length returned by the reader.</summary>
	MaxStringContentLength = 2,
	/// <summary>Specifies the maximum allowed array length.</summary>
	MaxArrayLength = 4,
	/// <summary>Specifies the maximum allowed bytes returned for each read.</summary>
	MaxBytesPerRead = 8,
	/// <summary>Specifies the maximum characters allowed in a table name.</summary>
	MaxNameTableCharCount = 0x10
}
