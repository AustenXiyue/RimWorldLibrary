using System;

namespace ICSharpCode.SharpZipLib.Core;

public class InvalidNameException : SharpZipBaseException
{
	public InvalidNameException()
		: base("An invalid name was specified")
	{
	}

	public InvalidNameException(string message)
		: base(message)
	{
	}

	public InvalidNameException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
