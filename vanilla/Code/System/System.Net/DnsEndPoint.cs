using System.Net.Sockets;

namespace System.Net;

/// <summary>Represents a network endpoint as a host name or a string representation of an IP address and a port number.</summary>
public class DnsEndPoint : EndPoint
{
	private string m_Host;

	private int m_Port;

	private AddressFamily m_Family;

	/// <summary>Gets the host name or string representation of the Internet Protocol (IP) address of the host.</summary>
	/// <returns>A host name or string representation of an IP address.</returns>
	public string Host => m_Host;

	/// <summary>Gets the Internet Protocol (IP) address family.</summary>
	/// <returns>One of the <see cref="T:System.Net.Sockets.AddressFamily" /> values.</returns>
	public override AddressFamily AddressFamily => m_Family;

	/// <summary>Gets the port number of the <see cref="T:System.Net.DnsEndPoint" />.</summary>
	/// <returns>An integer value in the range 0 to 0xffff indicating the port number of the <see cref="T:System.Net.DnsEndPoint" />.</returns>
	public int Port => m_Port;

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.DnsEndPoint" /> class with the host name or string representation of an IP address and a port number.</summary>
	/// <param name="host">The host name or a string representation of the IP address.</param>
	/// <param name="port">The port number associated with the address, or 0 to specify any available port. <paramref name="port" /> is in host order.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="host" /> parameter contains an empty string.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="host" /> parameter is a null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="port" /> is less than <see cref="F:System.Net.IPEndPoint.MinPort" />.-or- <paramref name="port" /> is greater than <see cref="F:System.Net.IPEndPoint.MaxPort" />. </exception>
	public DnsEndPoint(string host, int port)
		: this(host, port, AddressFamily.Unspecified)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.DnsEndPoint" /> class with the host name or string representation of an IP address, a port number, and an address family.</summary>
	/// <param name="host">The host name or a string representation of the IP address.</param>
	/// <param name="port">The port number associated with the address, or 0 to specify any available port. <paramref name="port" /> is in host order.</param>
	/// <param name="addressFamily">One of the <see cref="T:System.Net.Sockets.AddressFamily" /> values.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="host" /> parameter contains an empty string.-or- <paramref name="addressFamily" /> is <see cref="F:System.Net.Sockets.AddressFamily.Unknown" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="host" /> parameter is a null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="port" /> is less than <see cref="F:System.Net.IPEndPoint.MinPort" />.-or- <paramref name="port" /> is greater than <see cref="F:System.Net.IPEndPoint.MaxPort" />.</exception>
	public DnsEndPoint(string host, int port, AddressFamily addressFamily)
	{
		if (host == null)
		{
			throw new ArgumentNullException("host");
		}
		if (string.IsNullOrEmpty(host))
		{
			throw new ArgumentException(global::SR.GetString("The parameter '{0}' cannot be an empty string.", "host"));
		}
		if (port < 0 || port > 65535)
		{
			throw new ArgumentOutOfRangeException("port");
		}
		if (addressFamily != AddressFamily.InterNetwork && addressFamily != AddressFamily.InterNetworkV6 && addressFamily != 0)
		{
			throw new ArgumentException(global::SR.GetString("The specified value is not valid."), "addressFamily");
		}
		m_Host = host;
		m_Port = port;
		m_Family = addressFamily;
	}

	/// <summary>Compares two <see cref="T:System.Net.DnsEndPoint" /> objects.</summary>
	/// <returns>true if the two <see cref="T:System.Net.DnsEndPoint" /> instances are equal; otherwise, false.</returns>
	/// <param name="comparand">A <see cref="T:System.Net.DnsEndPoint" /> instance to compare to the current instance.</param>
	public override bool Equals(object comparand)
	{
		if (!(comparand is DnsEndPoint dnsEndPoint))
		{
			return false;
		}
		if (m_Family == dnsEndPoint.m_Family && m_Port == dnsEndPoint.m_Port)
		{
			return m_Host == dnsEndPoint.m_Host;
		}
		return false;
	}

	/// <summary>Returns a hash value for a <see cref="T:System.Net.DnsEndPoint" />.</summary>
	/// <returns>An integer hash value for the <see cref="T:System.Net.DnsEndPoint" />.</returns>
	public override int GetHashCode()
	{
		return StringComparer.InvariantCultureIgnoreCase.GetHashCode(ToString());
	}

	/// <summary>Returns the host name or string representation of the IP address and port number of the <see cref="T:System.Net.DnsEndPoint" />.</summary>
	/// <returns>A string containing the address family, host name or IP address string, and the port number of the specified <see cref="T:System.Net.DnsEndPoint" />.</returns>
	public override string ToString()
	{
		return string.Concat(m_Family, "/", m_Host, ":", m_Port);
	}
}
