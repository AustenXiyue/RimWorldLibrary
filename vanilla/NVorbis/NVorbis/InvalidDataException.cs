using System;

namespace NVorbis;

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
