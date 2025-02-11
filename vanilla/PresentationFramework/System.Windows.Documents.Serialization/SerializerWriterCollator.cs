using System.Printing;
using System.Windows.Media;

namespace System.Windows.Documents.Serialization;

/// <summary>Defines the abstract methods required to implement a plug-in document serialization <see cref="T:System.Windows.Media.Visual" /> collator.</summary>
public abstract class SerializerWriterCollator
{
	/// <summary>When overridden in a derived class, initiates the start of a batch write operation.</summary>
	public abstract void BeginBatchWrite();

	/// <summary>When overridden in a derived class, completes a batch write operation.</summary>
	public abstract void EndBatchWrite();

	/// <summary>When overridden in a derived class, synchronously writes a given <see cref="T:System.Windows.Media.Visual" /> element to the serialization stream.</summary>
	/// <param name="visual">The visual element to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	public abstract void Write(Visual visual);

	/// <summary>When overridden in a derived class, synchronously writes a given <see cref="T:System.Windows.Media.Visual" /> element together with an associated print ticket to the serialization stream.</summary>
	/// <param name="visual">A <see cref="T:System.Windows.Media.Visual" /> that is written to the stream.</param>
	/// <param name="printTicket">An object specifying preferences for how the material should be printed.</param>
	public abstract void Write(Visual visual, PrintTicket printTicket);

	/// <summary>When overridden in a derived class, asynchronously writes a given <see cref="T:System.Windows.Media.Visual" /> element to the serialization stream.</summary>
	/// <param name="visual">The visual element to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	public abstract void WriteAsync(Visual visual);

	/// <summary>When overridden in a derived class, asynchronously writes a given <see cref="T:System.Windows.Media.Visual" /> element with a specified event identifier to the serialization stream.</summary>
	/// <param name="visual">The visual element to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	/// <param name="userState">A caller-specified object to identify the asynchronous write operation.</param>
	public abstract void WriteAsync(Visual visual, object userState);

	/// <summary>When overridden in a derived class, asynchronously writes a given <see cref="T:System.Windows.Media.Visual" /> element together with an associated print ticket to the serialization stream.</summary>
	/// <param name="visual">The visual element to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	/// <param name="printTicket">The default print preferences for the <paramref name="visual" /> element.</param>
	public abstract void WriteAsync(Visual visual, PrintTicket printTicket);

	/// <summary>When overridden in a derived class, asynchronously writes a given <see cref="T:System.Windows.Media.Visual" /> element together with an associated print ticket and identifier to the serialization stream.</summary>
	/// <param name="visual">The visual element to write to the serialization <see cref="T:System.IO.Stream" />.</param>
	/// <param name="printTicket">The default print preferences for the <paramref name="visual" /> element.</param>
	/// <param name="userState">A caller-specified object to identify the asynchronous write operation.</param>
	public abstract void WriteAsync(Visual visual, PrintTicket printTicket, object userState);

	/// <summary>When overridden in a derived class, cancels an asynchronous <see cref="Overload:System.Windows.Documents.Serialization.SerializerWriterCollator.WriteAsync" /> operation. </summary>
	public abstract void CancelAsync();

	/// <summary>When overridden in a derived class, cancels a synchronous <see cref="Overload:System.Windows.Documents.Serialization.SerializerWriterCollator.Write" /> operation. </summary>
	public abstract void Cancel();

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Serialization.SerializerWriterCollator" /> class.</summary>
	protected SerializerWriterCollator()
	{
	}
}
