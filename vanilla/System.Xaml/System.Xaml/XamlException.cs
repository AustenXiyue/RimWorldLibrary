using System.Runtime.Serialization;

namespace System.Xaml;

/// <summary>The exception that is thrown for a general XAML reader or XAML writer exception. See Remarks.</summary>
[Serializable]
public class XamlException : Exception
{
	/// <summary>Gets or sets the exception message, and if line information is available, appends the line information to the message.</summary>
	/// <returns>The exception message that includes the appended line information.</returns>
	public override string Message
	{
		get
		{
			if (LineNumber != 0)
			{
				if (LinePosition != 0)
				{
					return System.SR.Format(System.SR.LineNumberAndPosition, base.Message, LineNumber, LinePosition);
				}
				return System.SR.Format(System.SR.LineNumberOnly, base.Message, LineNumber);
			}
			return base.Message;
		}
	}

	/// <summary>Gets or sets the line number component of XAML text line information that the exception reports.</summary>
	/// <returns>The line number component of the XAML text line information.</returns>
	public int LineNumber { get; protected set; }

	/// <summary>Gets or sets the line position component of XAML text line information that the exception reports.</summary>
	/// <returns>The line position component of XAML text line information.</returns>
	public int LinePosition { get; protected set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlException" /> class. The instance contains a specified error message, inner exception, and line information.</summary>
	/// <param name="message">The message that describes the exception. The caller of this constructor must make sure that this string has been localized for the current system culture. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. </param>
	/// <param name="lineNumber">The line number to report to debugging or to line information consumers.</param>
	/// <param name="linePosition">The line position to report to debugging or line information consumers.</param>
	public XamlException(string message, Exception innerException, int lineNumber, int linePosition)
		: base(message, innerException)
	{
		LineNumber = lineNumber;
		LinePosition = linePosition;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlException" /> class. The instance contains a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The message that describes the exception. The caller of this constructor must make sure that this string has been localized for the current system culture. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public XamlException(string message, Exception innerException)
		: base(message, innerException)
	{
		if (innerException is XamlException ex)
		{
			LineNumber = ex.LineNumber;
			LinePosition = ex.LinePosition;
		}
	}

	internal void SetLineInfo(int lineNumber, int linePosition)
	{
		LineNumber = lineNumber;
		LinePosition = linePosition;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlException" /> class. The instance contains a system-supplied message that describes the error.</summary>
	public XamlException()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlException" /> class. The instance contains a specified message that describes the error.</summary>
	/// <param name="message">The message that describes the exception. The caller of this constructor must make sure that this string has been localized for the current system culture.</param>
	public XamlException(string message)
		: base(message)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlException" /> class with serialized data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null.</exception>
	protected XamlException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		ArgumentNullException.ThrowIfNull(info, "info");
		LineNumber = info.GetInt32("Line");
		LinePosition = info.GetInt32("Offset");
	}

	/// <summary>Implements <see cref="M:System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo,System.Runtime.Serialization.StreamingContext)" /> and provides serialization support for the line information data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null.</exception>
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		ArgumentNullException.ThrowIfNull(info, "info");
		info.AddValue("Line", LineNumber);
		info.AddValue("Offset", LinePosition);
		base.GetObjectData(info, context);
	}
}
