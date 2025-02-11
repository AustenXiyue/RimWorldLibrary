using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System;

/// <summary>The exception that is thrown when time zone information is invalid.</summary>
[Serializable]
[TypeForwardedFrom("System.Core, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=b77a5c561934e089")]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public class InvalidTimeZoneException : Exception
{
	/// <summary>Initializes a new instance of the <see cref="T:System.InvalidTimeZoneException" /> class with the specified message string.</summary>
	/// <param name="message">A string that describes the exception.</param>
	public InvalidTimeZoneException(string message)
		: base(message)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.InvalidTimeZoneException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">A string that describes the exception.    </param>
	/// <param name="innerException">The exception that is the cause of the current exception.  </param>
	public InvalidTimeZoneException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.InvalidTimeZoneException" /> class from serialized data.</summary>
	/// <param name="info">The object that contains the serialized data. </param>
	/// <param name="context">The stream that contains the serialized data.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="info" /> parameter is null.-or-The <paramref name="context" /> parameter is null.</exception>
	protected InvalidTimeZoneException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.InvalidTimeZoneException" /> class with a system-supplied message.</summary>
	public InvalidTimeZoneException()
	{
	}
}
