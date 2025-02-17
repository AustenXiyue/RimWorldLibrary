namespace System.Xml;

/// <summary>Specifies the state of the reader.</summary>
public enum ReadState
{
	/// <summary>The Read method has not been called.</summary>
	Initial,
	/// <summary>The Read method has been called. Additional methods may be called on the reader.</summary>
	Interactive,
	/// <summary>An error occurred that prevents the read operation from continuing.</summary>
	Error,
	/// <summary>The end of the file has been reached successfully.</summary>
	EndOfFile,
	/// <summary>The <see cref="M:System.Xml.XmlReader.Close" /> method has been called.</summary>
	Closed
}
