using System;

namespace NAudio;

public class InvalidDataException : Exception
{
	public InvalidDataException()
	{
	}

	public InvalidDataException(string message)
		: base(message)
	{
	}
}
