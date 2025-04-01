using System;

namespace ICSharpCode.SharpZipLib;

public class StreamDecodingException : SharpZipBaseException
{
	private const string GenericMessage = "Input stream could not be decoded";

	public StreamDecodingException()
		: base("Input stream could not be decoded")
	{
	}

	public StreamDecodingException(string message)
		: base(message)
	{
	}

	public StreamDecodingException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
