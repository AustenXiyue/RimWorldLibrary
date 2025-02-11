using System.Printing;
using System.Windows.Media;

namespace System.Windows.Documents.Serialization;

/// <summary>Defines the abstract methods and events that are required to implement a plug-in document output serializer. </summary>
public abstract class SerializerWriter
{
	/// <summary>When overridden in a derived class, occurs just before a <see cref="T:System.Printing.PrintTicket" /> is added to a stream by a <see cref="Overload:System.Windows.Documents.Serialization.SerializerWriter.Write" /> or <see cref="Overload:System.Windows.Documents.Serialization.SerializerWriter.WriteAsync" /> method.</summary>
	public abstract event WritingPrintTicketRequiredEventHandler WritingPrintTicketRequired;

	/// <summary>When overridden in a derived class, occurs when the <see cref="T:System.Windows.Documents.Serialization.SerializerWriter" /> updates its progress. </summary>
	public abstract event WritingProgressChangedEventHandler WritingProgressChanged;

	/// <summary>When overridden in a derived class, occurs when a write operation finishes.</summary>
	public abstract event WritingCompletedEventHandler WritingCompleted;

	/// <summary>When overridden in a derived class, occurs when a <see cref="M:System.Windows.Documents.Serialization.SerializerWriter.CancelAsync" /> operation is performed.</summary>
	public abstract event WritingCancelledEventHandler WritingCancelled;

	/// <summary>When overridden in a derived class, synchronously writes a given <see cref="T:System.Windows.Media.Visual" /> element to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="visual">The <see cref="T:System.Windows.Media.Visual" /> element to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	public abstract void Write(Visual visual);

	/// <summary>When overridden in a derived class, synchronously writes a given <see cref="T:System.Windows.Media.Visual" /> element together with an associated <see cref="T:System.Printing.PrintTicket" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="visual">The <see cref="T:System.Windows.Media.Visual" /> element to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	/// <param name="printTicket">The default print preferences for the <paramref name="visual" /> element.</param>
	public abstract void Write(Visual visual, PrintTicket printTicket);

	/// <summary>When overridden in a derived class, asynchronously writes a given <see cref="T:System.Windows.Media.Visual" /> element to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="visual">The <see cref="T:System.Windows.Media.Visual" /> element to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	public abstract void WriteAsync(Visual visual);

	/// <summary>When overridden in a derived class, asynchronously writes a given <see cref="T:System.Windows.Media.Visual" /> element to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="visual">The <see cref="T:System.Windows.Media.Visual" /> element to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	/// <param name="userState">A caller-specified object to identify the asynchronous write operation.</param>
	public abstract void WriteAsync(Visual visual, object userState);

	/// <summary>When overridden in a derived class, asynchronously writes a given <see cref="T:System.Windows.Media.Visual" /> element together with an associated <see cref="T:System.Printing.PrintTicket" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="visual">The <see cref="T:System.Windows.Media.Visual" /> element to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	/// <param name="printTicket">The default print preferences for the <paramref name="visual" /> element.</param>
	public abstract void WriteAsync(Visual visual, PrintTicket printTicket);

	/// <summary>When overridden in a derived class, asynchronously writes a given <see cref="T:System.Windows.Media.Visual" /> element together with an associated <see cref="T:System.Printing.PrintTicket" /> and identifier to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="visual">The <see cref="T:System.Windows.Media.Visual" /> element to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	/// <param name="printTicket">The default print preferences for the <paramref name="visual" /> element.</param>
	/// <param name="userState">A caller-specified object to identify the asynchronous write operation.</param>
	public abstract void WriteAsync(Visual visual, PrintTicket printTicket, object userState);

	/// <summary>When overridden in a derived class, synchronously writes the content of a given <see cref="T:System.Windows.Documents.DocumentPaginator" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="documentPaginator">The document paginator that defines the content to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	public abstract void Write(DocumentPaginator documentPaginator);

	/// <summary>When overridden in a derived class, synchronously writes paginated content together with an associated <see cref="T:System.Printing.PrintTicket" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="documentPaginator">The document paginator that defines the content to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	/// <param name="printTicket">The default print preferences for the <paramref name="documentPaginator" /> content.</param>
	public abstract void Write(DocumentPaginator documentPaginator, PrintTicket printTicket);

	/// <summary>When overridden in a derived class, asynchronously writes the content of a given <see cref="T:System.Windows.Documents.DocumentPaginator" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="documentPaginator">The document paginator that defines the content to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	public abstract void WriteAsync(DocumentPaginator documentPaginator);

	/// <summary>When overridden in a derived class, asynchronously writes the content of a given <see cref="T:System.Windows.Documents.DocumentPaginator" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="documentPaginator">The document paginator that defines the content to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	/// <param name="printTicket">The default print preferences for the <paramref name="documentPaginator" /> content.</param>
	public abstract void WriteAsync(DocumentPaginator documentPaginator, PrintTicket printTicket);

	/// <summary>When overridden in a derived class, asynchronously writes the content of a given <see cref="T:System.Windows.Documents.DocumentPaginator" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="documentPaginator">The document paginator that defines the content to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	/// <param name="userState">A caller-specified object to identify the asynchronous write operation.</param>
	public abstract void WriteAsync(DocumentPaginator documentPaginator, object userState);

	/// <summary>When overridden in a derived class, asynchronously writes paginated content together with an associated <see cref="T:System.Printing.PrintTicket" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="documentPaginator">The document paginator that defines the content to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	/// <param name="printTicket">The default print preferences for the <paramref name="documentPaginator" /> content.</param>
	/// <param name="userState">A caller-specified object to identify the asynchronous write operation.</param>
	public abstract void WriteAsync(DocumentPaginator documentPaginator, PrintTicket printTicket, object userState);

	/// <summary>When overridden in a derived class, synchronously writes a given <see cref="T:System.Windows.Documents.FixedPage" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="fixedPage">The page to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	public abstract void Write(FixedPage fixedPage);

	/// <summary>When overridden in a derived class, synchronously writes a given <see cref="T:System.Windows.Documents.FixedPage" /> together with an associated <see cref="T:System.Printing.PrintTicket" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="fixedPage">The page to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	/// <param name="printTicket">The default print preferences for the <paramref name="fixedPage" /> content.</param>
	public abstract void Write(FixedPage fixedPage, PrintTicket printTicket);

	/// <summary>When overridden in a derived class, asynchronously writes a given <see cref="T:System.Windows.Documents.FixedPage" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="fixedPage">The page to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	public abstract void WriteAsync(FixedPage fixedPage);

	/// <summary>When overridden in a derived class, asynchronously writes a given <see cref="T:System.Windows.Documents.FixedPage" /> together with an associated <see cref="T:System.Printing.PrintTicket" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="fixedPage">The page to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	/// <param name="printTicket">The default print preferences for the <paramref name="fixedPage" /> content.</param>
	public abstract void WriteAsync(FixedPage fixedPage, PrintTicket printTicket);

	/// <summary>When overridden in a derived class, asynchronously writes a given <see cref="T:System.Windows.Documents.FixedPage" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="fixedPage">The page to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	/// <param name="userState">A caller-specified object to identify the asynchronous write operation.</param>
	public abstract void WriteAsync(FixedPage fixedPage, object userState);

	/// <summary>When overridden in a derived class, asynchronously writes a given <see cref="T:System.Windows.Documents.FixedPage" /> together with an associated <see cref="T:System.Printing.PrintTicket" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="fixedPage">The page to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	/// <param name="printTicket">The default print preferences for the <paramref name="fixedPage" /> content.</param>
	/// <param name="userState">A caller-specified object to identify the asynchronous write operation.</param>
	public abstract void WriteAsync(FixedPage fixedPage, PrintTicket printTicket, object userState);

	/// <summary>When overridden in a derived class, synchronously writes a given <see cref="T:System.Windows.Documents.FixedDocument" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="fixedDocument">The document to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	public abstract void Write(FixedDocument fixedDocument);

	/// <summary>When overridden in a derived class, synchronously writes a given <see cref="T:System.Windows.Documents.FixedDocument" /> together with an associated <see cref="T:System.Printing.PrintTicket" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="fixedDocument">The document to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	/// <param name="printTicket">The default print preferences for the <paramref name="fixedDocument" /> content.</param>
	public abstract void Write(FixedDocument fixedDocument, PrintTicket printTicket);

	/// <summary>When overridden in a derived class, asynchronously writes a given <see cref="T:System.Windows.Documents.FixedDocument" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="fixedDocument">The document to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	public abstract void WriteAsync(FixedDocument fixedDocument);

	/// <summary>When overridden in a derived class, asynchronously writes a given <see cref="T:System.Windows.Documents.FixedDocument" /> together with an associated <see cref="T:System.Printing.PrintTicket" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="fixedDocument">The document to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	/// <param name="printTicket">The default print preferences for the <paramref name="fixedDocument" /> content.</param>
	public abstract void WriteAsync(FixedDocument fixedDocument, PrintTicket printTicket);

	/// <summary>When overridden in a derived class, asynchronously writes a given <see cref="T:System.Windows.Documents.FixedDocument" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="fixedDocument">The document to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	/// <param name="userState">A caller-specified object to identify the asynchronous write operation.</param>
	public abstract void WriteAsync(FixedDocument fixedDocument, object userState);

	/// <summary>When overridden in a derived class, asynchronously writes a given <see cref="T:System.Windows.Documents.FixedDocument" /> together with an associated <see cref="T:System.Printing.PrintTicket" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="fixedDocument">The document to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	/// <param name="printTicket">The default print preferences for the <paramref name="fixedDocument" /> content.</param>
	/// <param name="userState">A caller-specified object to identify the asynchronous write operation.</param>
	public abstract void WriteAsync(FixedDocument fixedDocument, PrintTicket printTicket, object userState);

	/// <summary>When overridden in a derived class, synchronously writes a given <see cref="T:System.Windows.Documents.FixedDocumentSequence" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="fixedDocumentSequence">The document sequence that defines the content to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	public abstract void Write(FixedDocumentSequence fixedDocumentSequence);

	/// <summary>When overridden in a derived class, synchronously writes a given <see cref="T:System.Windows.Documents.FixedDocumentSequence" /> together with an associated <see cref="T:System.Printing.PrintTicket" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="fixedDocumentSequence">The document sequence that defines the content to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	/// <param name="printTicket">The default print preferences for the <paramref name="fixedDocumentSequence" /> content.</param>
	public abstract void Write(FixedDocumentSequence fixedDocumentSequence, PrintTicket printTicket);

	/// <summary>When overridden in a derived class, asynchronously writes a given <see cref="T:System.Windows.Documents.FixedDocumentSequence" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="fixedDocumentSequence">The document sequence that defines the content to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	public abstract void WriteAsync(FixedDocumentSequence fixedDocumentSequence);

	/// <summary>When overridden in a derived class, asynchronously writes a given <see cref="T:System.Windows.Documents.FixedDocumentSequence" /> together with an associated <see cref="T:System.Printing.PrintTicket" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="fixedDocumentSequence">The document sequence that defines the content to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	/// <param name="printTicket">The default print preferences for the <paramref name="fixedDocumentSequence" /> content.</param>
	public abstract void WriteAsync(FixedDocumentSequence fixedDocumentSequence, PrintTicket printTicket);

	/// <summary>When overridden in a derived class, asynchronously writes a given <see cref="T:System.Windows.Documents.FixedDocumentSequence" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="fixedDocumentSequence">The document sequence that defines the content to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	/// <param name="userState">A caller-specified object to identify the asynchronous write operation.</param>
	public abstract void WriteAsync(FixedDocumentSequence fixedDocumentSequence, object userState);

	/// <summary>When overridden in a derived class, asynchronously writes a given <see cref="T:System.Windows.Documents.FixedDocumentSequence" /> together with an associated <see cref="T:System.Printing.PrintTicket" /> to the serialization <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="fixedDocumentSequence">The document sequence that defines the content to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	/// <param name="printTicket">The default print preferences for the <paramref name="fixedDocumentSequence" /> content.</param>
	/// <param name="userState">A caller-specified object to identify the asynchronous write operation.</param>
	public abstract void WriteAsync(FixedDocumentSequence fixedDocumentSequence, PrintTicket printTicket, object userState);

	/// <summary>When overridden in a derived class, cancels an asynchronous write operation.</summary>
	public abstract void CancelAsync();

	/// <summary>When overridden in a derived class, returns a <see cref="T:System.Windows.Documents.Serialization.SerializerWriterCollator" /> that writes collated <see cref="T:System.Windows.Media.Visual" /> elements.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.Serialization.SerializerWriterCollator" /> that writes collated <see cref="T:System.Windows.Media.Visual" /> elements to the document output serialization <see cref="T:System.IO.Stream" />. </returns>
	public abstract SerializerWriterCollator CreateVisualsCollator();

	/// <summary>When overridden in a derived class, returns a <see cref="T:System.Windows.Documents.Serialization.SerializerWriterCollator" /> that writes collated <see cref="T:System.Windows.Media.Visual" /> elements together with the given print tickets.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.Serialization.SerializerWriterCollator" /> that writes collated <see cref="T:System.Windows.Media.Visual" /> elements to the document output serialization <see cref="T:System.IO.Stream" />.</returns>
	/// <param name="documentSequencePT">The default print preferences for <see cref="T:System.Windows.Documents.FixedDocumentSequence" /> content.</param>
	/// <param name="documentPT">The default print preferences for <see cref="T:System.Windows.Documents.FixedDocument" /> content.</param>
	public abstract SerializerWriterCollator CreateVisualsCollator(PrintTicket documentSequencePT, PrintTicket documentPT);

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Serialization.SerializerWriter" /> class.</summary>
	protected SerializerWriter()
	{
	}
}
