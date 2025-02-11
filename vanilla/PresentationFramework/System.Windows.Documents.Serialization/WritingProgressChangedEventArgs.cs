using System.ComponentModel;

namespace System.Windows.Documents.Serialization;

/// <summary>Provides data for the <see cref="E:System.Windows.Xps.XpsDocumentWriter.WritingProgressChanged" /> event.</summary>
public class WritingProgressChangedEventArgs : ProgressChangedEventArgs
{
	private int _number;

	private WritingProgressChangeLevel _writingLevel;

	/// <summary>Gets the number of documents or pages that have been written.</summary>
	/// <returns>The number of documents or pages that have been written at the time of the event.</returns>
	public int Number => _number;

	/// <summary>Gets a value that indicates the scope of the writing progress.</summary>
	/// <returns>An enumeration that indicates the scope of writing a multiple document sequence, a single document, or single page.</returns>
	public WritingProgressChangeLevel WritingLevel => _writingLevel;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Serialization.WritingProgressChangedEventArgs" /> class.</summary>
	/// <param name="writingLevel">An enumeration value that specifies the scope of the progress changed event such as for an entire multiple document sequence, a single document, or a single page.</param>
	/// <param name="number">Based on the scope defined by <paramref name="writingLevel" />, the number of documents or the number of pages that have been written.</param>
	/// <param name="progressPercentage">The percentage of data that has been written.</param>
	/// <param name="state">The user-supplied object that identifies the write operation.</param>
	public WritingProgressChangedEventArgs(WritingProgressChangeLevel writingLevel, int number, int progressPercentage, object state)
		: base(progressPercentage, state)
	{
		_number = number;
		_writingLevel = writingLevel;
	}
}
