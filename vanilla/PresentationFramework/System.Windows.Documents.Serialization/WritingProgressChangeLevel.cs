namespace System.Windows.Documents.Serialization;

/// <summary>Specifies the scope of a <see cref="E:System.Windows.Documents.Serialization.SerializerWriter.WritingProgressChanged" /> event.</summary>
public enum WritingProgressChangeLevel
{
	/// <summary>The output progress is unspecified.</summary>
	None,
	/// <summary>The output progress of a multiple document sequence.</summary>
	FixedDocumentSequenceWritingProgress,
	/// <summary>The output progress of a single document.</summary>
	FixedDocumentWritingProgress,
	/// <summary>The output progress of a single page.</summary>
	FixedPageWritingProgress
}
