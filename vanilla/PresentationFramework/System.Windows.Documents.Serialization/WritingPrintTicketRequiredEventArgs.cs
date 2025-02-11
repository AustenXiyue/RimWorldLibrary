using System.Printing;
using System.Windows.Xps.Serialization;

namespace System.Windows.Documents.Serialization;

/// <summary>Provides data for the <see cref="E:System.Windows.Documents.Serialization.SerializerWriter.WritingPrintTicketRequired" /> event.</summary>
public class WritingPrintTicketRequiredEventArgs : EventArgs
{
	private PrintTicketLevel _printTicketLevel;

	private int _sequence;

	private PrintTicket _printTicket;

	/// <summary>Gets a value that indicates the scope of the <see cref="E:System.Windows.Documents.Serialization.SerializerWriter.WritingPrintTicketRequired" /> event.</summary>
	/// <returns>An enumeration that indicates the scope of the <see cref="E:System.Windows.Documents.Serialization.SerializerWriter.WritingPrintTicketRequired" /> event as for a sequence of documents, a single document, or a single page.</returns>
	public PrintTicketLevel CurrentPrintTicketLevel => _printTicketLevel;

	/// <summary>Gets the number of documents or pages output with the <see cref="P:System.Windows.Documents.Serialization.WritingPrintTicketRequiredEventArgs.CurrentPrintTicket" />.</summary>
	/// <returns>The number of documents or pages output with the <see cref="P:System.Windows.Documents.Serialization.WritingPrintTicketRequiredEventArgs.CurrentPrintTicket" />.</returns>
	public int Sequence => _sequence;

	/// <summary>Gets or sets the default printer settings to use when the document is printed.</summary>
	/// <returns>The default printer settings to use when the document is printed.</returns>
	public PrintTicket CurrentPrintTicket
	{
		get
		{
			return _printTicket;
		}
		set
		{
			_printTicket = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Serialization.WritingPrintTicketRequiredEventArgs" /> class.</summary>
	/// <param name="printTicketLevel">An enumeration value that specifies scope of the <see cref="P:System.Windows.Documents.Serialization.WritingPrintTicketRequiredEventArgs.CurrentPrintTicket" /> as a page, document, or sequence of documents.</param>
	/// <param name="sequence">Based on the scope of defined by <paramref name="printTicketLevel" />, the number of pages or the number of documents associated with the <see cref="P:System.Windows.Documents.Serialization.WritingPrintTicketRequiredEventArgs.CurrentPrintTicket" />.</param>
	public WritingPrintTicketRequiredEventArgs(PrintTicketLevel printTicketLevel, int sequence)
	{
		_printTicketLevel = printTicketLevel;
		_sequence = sequence;
	}
}
