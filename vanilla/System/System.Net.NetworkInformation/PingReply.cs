namespace System.Net.NetworkInformation;

/// <summary>Provides information about the status and data resulting from a <see cref="Overload:System.Net.NetworkInformation.Ping.Send" /> or <see cref="Overload:System.Net.NetworkInformation.Ping.SendAsync" /> operation.</summary>
public class PingReply
{
	private IPAddress address;

	private PingOptions options;

	private IPStatus ipStatus;

	private long rtt;

	private byte[] buffer;

	/// <summary>Gets the status of an attempt to send an Internet Control Message Protocol (ICMP) echo request and receive the corresponding ICMP echo reply message.</summary>
	/// <returns>An <see cref="T:System.Net.NetworkInformation.IPStatus" /> value indicating the result of the request.</returns>
	public IPStatus Status => ipStatus;

	/// <summary>Gets the address of the host that sends the Internet Control Message Protocol (ICMP) echo reply.</summary>
	/// <returns>An <see cref="T:System.Net.IPAddress" /> containing the destination for the ICMP echo message.</returns>
	public IPAddress Address => address;

	/// <summary>Gets the number of milliseconds taken to send an Internet Control Message Protocol (ICMP) echo request and receive the corresponding ICMP echo reply message.</summary>
	/// <returns>An <see cref="T:System.Int64" /> that specifies the round trip time, in milliseconds. </returns>
	public long RoundtripTime => rtt;

	/// <summary>Gets the options used to transmit the reply to an Internet Control Message Protocol (ICMP) echo request.</summary>
	/// <returns>A <see cref="T:System.Net.NetworkInformation.PingOptions" /> object that contains the Time to Live (TTL) and the fragmentation directive used for transmitting the reply if <see cref="P:System.Net.NetworkInformation.PingReply.Status" /> is <see cref="F:System.Net.NetworkInformation.IPStatus.Success" />; otherwise, null.</returns>
	public PingOptions Options => options;

	/// <summary>Gets the buffer of data received in an Internet Control Message Protocol (ICMP) echo reply message.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the data received in an ICMP echo reply message, or an empty array, if no reply was received.</returns>
	public byte[] Buffer => buffer;

	internal PingReply()
	{
	}

	internal PingReply(IPStatus ipStatus)
	{
		this.ipStatus = ipStatus;
		buffer = new byte[0];
	}

	internal PingReply(byte[] data, int dataLength, IPAddress address, int time)
	{
		this.address = address;
		rtt = time;
		ipStatus = GetIPStatus((IcmpV4Type)data[20], (IcmpV4Code)data[21]);
		if (ipStatus == IPStatus.Success)
		{
			buffer = new byte[dataLength - 28];
			Array.Copy(data, 28, buffer, 0, dataLength - 28);
		}
		else
		{
			buffer = new byte[0];
		}
	}

	internal PingReply(IPAddress address, byte[] buffer, PingOptions options, long roundtripTime, IPStatus status)
	{
		this.address = address;
		this.buffer = buffer;
		this.options = options;
		rtt = roundtripTime;
		ipStatus = status;
	}

	private IPStatus GetIPStatus(IcmpV4Type type, IcmpV4Code code)
	{
		return type switch
		{
			IcmpV4Type.ICMP4_ECHO_REPLY => IPStatus.Success, 
			IcmpV4Type.ICMP4_SOURCE_QUENCH => IPStatus.SourceQuench, 
			IcmpV4Type.ICMP4_PARAM_PROB => IPStatus.ParameterProblem, 
			IcmpV4Type.ICMP4_TIME_EXCEEDED => IPStatus.TtlExpired, 
			IcmpV4Type.ICMP4_DST_UNREACH => code switch
			{
				IcmpV4Code.ICMP4_UNREACH_NET => IPStatus.DestinationNetworkUnreachable, 
				IcmpV4Code.ICMP4_UNREACH_HOST => IPStatus.DestinationHostUnreachable, 
				IcmpV4Code.ICMP4_UNREACH_PROTOCOL => IPStatus.DestinationProtocolUnreachable, 
				IcmpV4Code.ICMP4_UNREACH_PORT => IPStatus.DestinationPortUnreachable, 
				IcmpV4Code.ICMP4_UNREACH_FRAG_NEEDED => IPStatus.PacketTooBig, 
				_ => IPStatus.DestinationUnreachable, 
			}, 
			_ => IPStatus.Unknown, 
		};
	}
}
