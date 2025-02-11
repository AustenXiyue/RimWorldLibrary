using System.Runtime.Serialization;

namespace System.Windows.Controls;

/// <summary>The exception that is thrown when an error condition occurs during the opening, accessing, or using of a PrintDialog.</summary>
[Serializable]
public class PrintDialogException : Exception
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.PrintDialogException" /> class.</summary>
	public PrintDialogException()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.PrintDialogException" /> class that provides a specific error condition in a <see cref="T:System.String" /> .</summary>
	/// <param name="message">A <see cref="T:System.String" /> that describes the error condition.</param>
	public PrintDialogException(string message)
		: base(message)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.PrintDialogException" /> class that provides a specific error condition, including its underlying cause.</summary>
	/// <param name="message">The <see cref="T:System.String" /> that describes the error condition.</param>
	/// <param name="innerException">The underlying error condition that caused the <see cref="T:System.Windows.Controls.PrintDialogException" />.</param>
	public PrintDialogException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.PrintDialogException" /> class that provides specific <see cref="T:System.Runtime.Serialization.SerializationInfo" /> and <see cref="T:System.Runtime.Serialization.StreamingContext" />. This constructor is protected.</summary>
	/// <param name="info">The data that is required to serialize or deserialize an object.</param>
	/// <param name="context">The context, including source and destination, of the serialized stream.</param>
	protected PrintDialogException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
