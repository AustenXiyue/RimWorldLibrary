namespace System.Net.Sockets;

/// <summary>Presents UDP receive result information from a call to the <see cref="M:System.Net.Sockets.UdpClient.ReceiveAsync" /> method.</summary>
public struct UdpReceiveResult : IEquatable<UdpReceiveResult>
{
	private byte[] m_buffer;

	private IPEndPoint m_remoteEndPoint;

	/// <summary>Gets a buffer with the data received in the UDP packet.</summary>
	/// <returns>Returns <see cref="T:System.Byte" />.A <see cref="T:System.Byte" /> array with the data received in the UDP packet.</returns>
	public byte[] Buffer => m_buffer;

	/// <summary>Gets the remote endpoint from which the UDP packet was received. </summary>
	/// <returns>Returns <see cref="T:System.Net.IPEndPoint" />.The remote endpoint from which the UDP packet was received.</returns>
	public IPEndPoint RemoteEndPoint => m_remoteEndPoint;

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Sockets.UdpReceiveResult" /> class.</summary>
	/// <param name="buffer">A buffer for data to receive in the UDP packet.</param>
	/// <param name="remoteEndPoint">The remote endpoint of the UDP packet.</param>
	public UdpReceiveResult(byte[] buffer, IPEndPoint remoteEndPoint)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (remoteEndPoint == null)
		{
			throw new ArgumentNullException("remoteEndPoint");
		}
		m_buffer = buffer;
		m_remoteEndPoint = remoteEndPoint;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.The hash code.</returns>
	public override int GetHashCode()
	{
		if (m_buffer == null)
		{
			return 0;
		}
		return m_buffer.GetHashCode() ^ m_remoteEndPoint.GetHashCode();
	}

	/// <summary>Returns a value that indicates whether this instance is equal to a specified object.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="obj" /> is an instance of <see cref="T:System.Net.Sockets.UdpReceiveResult" /> and equals the value of the instance; otherwise, false.</returns>
	/// <param name="obj">The object to compare with this instance.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is UdpReceiveResult))
		{
			return false;
		}
		return Equals((UdpReceiveResult)obj);
	}

	/// <summary>Returns a value that indicates whether this instance is equal to a specified object.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="other" /> is an instance of <see cref="T:System.Net.Sockets.UdpReceiveResult" /> and equals the value of the instance; otherwise, false.</returns>
	/// <param name="other">The object to compare with this instance.</param>
	public bool Equals(UdpReceiveResult other)
	{
		if (object.Equals(m_buffer, other.m_buffer))
		{
			return object.Equals(m_remoteEndPoint, other.m_remoteEndPoint);
		}
		return false;
	}

	/// <summary>Tests whether two specified <see cref="T:System.Net.Sockets.UdpReceiveResult" /> instances are equivalent.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise, false.</returns>
	/// <param name="left">The <see cref="T:System.Net.Sockets.UdpReceiveResult" /> instance that is to the left of the equality operator.</param>
	/// <param name="right">The <see cref="T:System.Net.Sockets.UdpReceiveResult" /> instance that is to the right of the equality operator.</param>
	public static bool operator ==(UdpReceiveResult left, UdpReceiveResult right)
	{
		return left.Equals(right);
	}

	/// <summary>Tests whether two specified <see cref="T:System.Net.Sockets.UdpReceiveResult" /> instances are not equal.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="left" /> and <paramref name="right" /> are unequal; otherwise, false.</returns>
	/// <param name="left">The <see cref="T:System.Net.Sockets.UdpReceiveResult" /> instance that is to the left of the not equal operator.</param>
	/// <param name="right">The <see cref="T:System.Net.Sockets.UdpReceiveResult" /> instance that is to the right of the not equal operator.</param>
	public static bool operator !=(UdpReceiveResult left, UdpReceiveResult right)
	{
		return !left.Equals(right);
	}
}
