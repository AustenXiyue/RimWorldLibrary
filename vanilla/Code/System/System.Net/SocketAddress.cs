using System.Globalization;
using System.Net.Sockets;
using System.Text;

namespace System.Net;

/// <summary>Stores serialized information from <see cref="T:System.Net.EndPoint" /> derived classes.</summary>
public class SocketAddress
{
	internal const int IPv6AddressSize = 28;

	internal const int IPv4AddressSize = 16;

	internal int m_Size;

	internal byte[] m_Buffer;

	private const int WriteableOffset = 2;

	private const int MaxSize = 32;

	private bool m_changed = true;

	private int m_hash;

	/// <summary>Gets the <see cref="T:System.Net.Sockets.AddressFamily" /> enumerated value of the current <see cref="T:System.Net.SocketAddress" />.</summary>
	/// <returns>One of the <see cref="T:System.Net.Sockets.AddressFamily" /> enumerated values.</returns>
	public AddressFamily Family => (AddressFamily)(m_Buffer[0] | (m_Buffer[1] << 8));

	/// <summary>Gets the underlying buffer size of the <see cref="T:System.Net.SocketAddress" />.</summary>
	/// <returns>The underlying buffer size of the <see cref="T:System.Net.SocketAddress" />.</returns>
	public int Size => m_Size;

	/// <summary>Gets or sets the specified index element in the underlying buffer.</summary>
	/// <returns>The value of the specified index element in the underlying buffer.</returns>
	/// <param name="offset">The array index element of the desired information. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">The specified index does not exist in the buffer. </exception>
	public byte this[int offset]
	{
		get
		{
			if (offset < 0 || offset >= Size)
			{
				throw new IndexOutOfRangeException();
			}
			return m_Buffer[offset];
		}
		set
		{
			if (offset < 0 || offset >= Size)
			{
				throw new IndexOutOfRangeException();
			}
			if (m_Buffer[offset] != value)
			{
				m_changed = true;
			}
			m_Buffer[offset] = value;
		}
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Net.SocketAddress" /> class for the given address family.</summary>
	/// <param name="family">An <see cref="T:System.Net.Sockets.AddressFamily" /> enumerated value. </param>
	public SocketAddress(AddressFamily family)
		: this(family, 32)
	{
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Net.SocketAddress" /> class using the specified address family and buffer size.</summary>
	/// <param name="family">An <see cref="T:System.Net.Sockets.AddressFamily" /> enumerated value. </param>
	/// <param name="size">The number of bytes to allocate for the underlying buffer. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="size" /> is less than 2. These 2 bytes are needed to store <paramref name="family" />. </exception>
	public SocketAddress(AddressFamily family, int size)
	{
		if (size < 2)
		{
			throw new ArgumentOutOfRangeException("size");
		}
		m_Size = size;
		m_Buffer = new byte[(size / IntPtr.Size + 2) * IntPtr.Size];
		m_Buffer[0] = (byte)family;
		m_Buffer[1] = (byte)((int)family >> 8);
	}

	internal SocketAddress(IPAddress ipAddress)
		: this(ipAddress.AddressFamily, (ipAddress.AddressFamily == AddressFamily.InterNetwork) ? 16 : 28)
	{
		m_Buffer[2] = 0;
		m_Buffer[3] = 0;
		if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
		{
			m_Buffer[4] = 0;
			m_Buffer[5] = 0;
			m_Buffer[6] = 0;
			m_Buffer[7] = 0;
			long scopeId = ipAddress.ScopeId;
			m_Buffer[24] = (byte)scopeId;
			m_Buffer[25] = (byte)(scopeId >> 8);
			m_Buffer[26] = (byte)(scopeId >> 16);
			m_Buffer[27] = (byte)(scopeId >> 24);
			byte[] addressBytes = ipAddress.GetAddressBytes();
			for (int i = 0; i < addressBytes.Length; i++)
			{
				m_Buffer[8 + i] = addressBytes[i];
			}
		}
		else
		{
			m_Buffer[4] = (byte)ipAddress.m_Address;
			m_Buffer[5] = (byte)(ipAddress.m_Address >> 8);
			m_Buffer[6] = (byte)(ipAddress.m_Address >> 16);
			m_Buffer[7] = (byte)(ipAddress.m_Address >> 24);
		}
	}

	internal SocketAddress(IPAddress ipaddress, int port)
		: this(ipaddress)
	{
		m_Buffer[2] = (byte)(port >> 8);
		m_Buffer[3] = (byte)port;
	}

	internal IPAddress GetIPAddress()
	{
		if (Family == AddressFamily.InterNetworkV6)
		{
			byte[] array = new byte[16];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = m_Buffer[i + 8];
			}
			long scopeid = (m_Buffer[27] << 24) + (m_Buffer[26] << 16) + (m_Buffer[25] << 8) + m_Buffer[24];
			return new IPAddress(array, scopeid);
		}
		if (Family == AddressFamily.InterNetwork)
		{
			return new IPAddress(((m_Buffer[4] & 0xFF) | ((m_Buffer[5] << 8) & 0xFF00) | ((m_Buffer[6] << 16) & 0xFF0000) | (m_Buffer[7] << 24)) & 0xFFFFFFFFu);
		}
		throw new SocketException(SocketError.AddressFamilyNotSupported);
	}

	internal IPEndPoint GetIPEndPoint()
	{
		IPAddress iPAddress = GetIPAddress();
		int port = ((m_Buffer[2] << 8) & 0xFF00) | m_Buffer[3];
		return new IPEndPoint(iPAddress, port);
	}

	internal void CopyAddressSizeIntoBuffer()
	{
		m_Buffer[m_Buffer.Length - IntPtr.Size] = (byte)m_Size;
		m_Buffer[m_Buffer.Length - IntPtr.Size + 1] = (byte)(m_Size >> 8);
		m_Buffer[m_Buffer.Length - IntPtr.Size + 2] = (byte)(m_Size >> 16);
		m_Buffer[m_Buffer.Length - IntPtr.Size + 3] = (byte)(m_Size >> 24);
	}

	internal int GetAddressSizeOffset()
	{
		return m_Buffer.Length - IntPtr.Size;
	}

	internal unsafe void SetSize(IntPtr ptr)
	{
		m_Size = *(int*)(void*)ptr;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Net.SocketAddress" /> instance.</summary>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	/// <param name="comparand">The specified <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Net.SocketAddress" /> instance.</param>
	public override bool Equals(object comparand)
	{
		if (!(comparand is SocketAddress socketAddress) || Size != socketAddress.Size)
		{
			return false;
		}
		for (int i = 0; i < Size; i++)
		{
			if (this[i] != socketAddress[i])
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Serves as a hash function for a particular type, suitable for use in hashing algorithms and data structures like a hash table.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		if (m_changed)
		{
			m_changed = false;
			m_hash = 0;
			int num = Size & -4;
			int i;
			for (i = 0; i < num; i += 4)
			{
				m_hash ^= m_Buffer[i] | (m_Buffer[i + 1] << 8) | (m_Buffer[i + 2] << 16) | (m_Buffer[i + 3] << 24);
			}
			if ((Size & 3) != 0)
			{
				int num2 = 0;
				int num3 = 0;
				for (; i < Size; i++)
				{
					num2 |= m_Buffer[i] << num3;
					num3 += 8;
				}
				m_hash ^= num2;
			}
		}
		return m_hash;
	}

	/// <summary>Returns information about the socket address.</summary>
	/// <returns>A string that contains information about the <see cref="T:System.Net.SocketAddress" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 2; i < Size; i++)
		{
			if (i > 2)
			{
				stringBuilder.Append(",");
			}
			stringBuilder.Append(this[i].ToString(NumberFormatInfo.InvariantInfo));
		}
		return Family.ToString() + ":" + Size.ToString(NumberFormatInfo.InvariantInfo) + ":{" + stringBuilder.ToString() + "}";
	}
}
