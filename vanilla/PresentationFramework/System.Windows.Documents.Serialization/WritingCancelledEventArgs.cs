namespace System.Windows.Documents.Serialization;

/// <summary>Provides data for the <see cref="E:System.Windows.Xps.XpsDocumentWriter.WritingCancelled" /> event.</summary>
public class WritingCancelledEventArgs : EventArgs
{
	private Exception _exception;

	/// <summary>Gets the exception that canceled the write operation.</summary>
	/// <returns>The exception that canceled the write operation.</returns>
	public Exception Error => _exception;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Serialization.WritingCancelledEventArgs" /> class.</summary>
	/// <param name="exception">The exception that canceled the write operation.</param>
	public WritingCancelledEventArgs(Exception exception)
	{
		_exception = exception;
	}
}
