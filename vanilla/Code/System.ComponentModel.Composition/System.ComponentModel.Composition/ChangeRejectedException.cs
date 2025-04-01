using System.Collections.Generic;
using System.Globalization;
using Microsoft.Internal;

namespace System.ComponentModel.Composition;

/// <summary>An exception that indicates whether a part has been rejected during composition.</summary>
[Serializable]
public class ChangeRejectedException : CompositionException
{
	/// <summary>Gets or sets the message associated with the component rejection.</summary>
	/// <returns>The message associated with the component rejection.</returns>
	public override string Message => string.Format(CultureInfo.CurrentCulture, Strings.CompositionException_ChangesRejected, base.Message);

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.ChangeRejectedException" /> class with a system-supplied message that describes the error.</summary>
	public ChangeRejectedException()
		: this(null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.ChangeRejectedException" /> class with a specified message that describes the error.</summary>
	/// <param name="message">The message that describes the exception. The caller of this constructor is required to ensure that this string has been localized for the current system culture.</param>
	public ChangeRejectedException(string message)
		: this(message, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.ChangeRejectedException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The message that describes the exception. The caller of this constructor is required to ensure that this string has been localized for the current system culture. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public ChangeRejectedException(string message, Exception innerException)
		: base(message, innerException, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.ChangeRejectedException" /> class with a list of composition errors. </summary>
	/// <param name="errors">A collection of errors that occurred during composition.</param>
	public ChangeRejectedException(IEnumerable<CompositionError> errors)
		: base(null, null, errors)
	{
	}
}
