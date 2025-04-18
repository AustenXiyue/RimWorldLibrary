using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Reflection;

/// <summary>The exception that is thrown when binding to a member results in more than one member matching the binding criteria. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class AmbiguousMatchException : SystemException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.AmbiguousMatchException" /> class with an empty message string and the root cause exception set to null.</summary>
	public AmbiguousMatchException()
		: base(Environment.GetResourceString("Ambiguous match found."))
	{
		SetErrorCode(-2147475171);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.AmbiguousMatchException" /> class with its message string set to the given message and the root cause exception set to null.</summary>
	/// <param name="message">A string indicating the reason this exception was thrown. </param>
	public AmbiguousMatchException(string message)
		: base(message)
	{
		SetErrorCode(-2147475171);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.AmbiguousMatchException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public AmbiguousMatchException(string message, Exception inner)
		: base(message, inner)
	{
		SetErrorCode(-2147475171);
	}

	internal AmbiguousMatchException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
