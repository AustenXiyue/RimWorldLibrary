namespace System.Net.NetworkInformation;

/// <summary>Provides Internet Protocol (IP) statistical data for an network interface on the local computer.</summary>
public abstract class IPInterfaceStatistics
{
	/// <summary>Gets the number of bytes that were received on the interface.</summary>
	/// <returns>Returns <see cref="T:System.Int64" />.The total number of bytes that were received on the interface.</returns>
	public abstract long BytesReceived { get; }

	/// <summary>Gets the number of bytes that were sent on the interface.</summary>
	/// <returns>Returns <see cref="T:System.Int64" />.The total number of bytes that were sent on the interface.</returns>
	public abstract long BytesSent { get; }

	/// <summary>Gets the number of incoming packets that were discarded.</summary>
	/// <returns>Returns <see cref="T:System.Int64" />.The total number of incoming packets that were discarded.</returns>
	public abstract long IncomingPacketsDiscarded { get; }

	/// <summary>Gets the number of incoming packets with errors.</summary>
	/// <returns>Returns <see cref="T:System.Int64" />.The total number of incoming packets with errors.</returns>
	public abstract long IncomingPacketsWithErrors { get; }

	/// <summary>Gets the number of incoming packets with an unknown protocol that were received on the interface.</summary>
	/// <returns>Returns <see cref="T:System.Int64" />.The total number of incoming packets with an unknown protocol that were received on the interface.</returns>
	public abstract long IncomingUnknownProtocolPackets { get; }

	/// <summary>Gets the number of non-unicast packets that were received on the interface.</summary>
	/// <returns>Returns <see cref="T:System.Int64" />.The total number of incoming non-unicast packets received on the interface.</returns>
	public abstract long NonUnicastPacketsReceived { get; }

	/// <summary>Gets the number of non-unicast packets that were sent on the interface.</summary>
	/// <returns>Returns <see cref="T:System.Int64" />.The total number of non-unicast packets that were sent on the interface.</returns>
	public abstract long NonUnicastPacketsSent { get; }

	/// <summary>Gets the number of outgoing packets that were discarded.</summary>
	/// <returns>Returns <see cref="T:System.Int64" />.The total number of outgoing packets that were discarded.</returns>
	public abstract long OutgoingPacketsDiscarded { get; }

	/// <summary>Gets the number of outgoing packets with errors.</summary>
	/// <returns>Returns <see cref="T:System.Int64" />.The total number of outgoing packets with errors.</returns>
	public abstract long OutgoingPacketsWithErrors { get; }

	/// <summary>Gets the length of the output queue.</summary>
	/// <returns>Returns <see cref="T:System.Int64" />.The total number of packets in the output queue.</returns>
	public abstract long OutputQueueLength { get; }

	/// <summary>Gets the number of unicast packets that were received on the interface.</summary>
	/// <returns>Returns <see cref="T:System.Int64" />.The total number of unicast packets that were received on the interface.</returns>
	public abstract long UnicastPacketsReceived { get; }

	/// <summary>Gets the number of unicast packets that were sent on the interface.</summary>
	/// <returns>Returns <see cref="T:System.Int64" />.The total number of unicast packets that were sent on the interface.</returns>
	public abstract long UnicastPacketsSent { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.NetworkInformation.IPInterfaceStatistics" /> class.</summary>
	protected IPInterfaceStatistics()
	{
	}
}
