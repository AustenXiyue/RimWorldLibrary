using System.Runtime.Serialization;

namespace System.Net.Sockets;

/// <summary>Encapsulates the information that is necessary to duplicate a <see cref="T:System.Net.Sockets.Socket" />.</summary>
[Serializable]
public struct SocketInformation
{
	private byte[] protocolInformation;

	private SocketInformationOptions options;

	[OptionalField]
	private EndPoint remoteEndPoint;

	/// <summary>Gets or sets the protocol information for a <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <returns>An array of type <see cref="T:System.Byte" />.</returns>
	public byte[] ProtocolInformation
	{
		get
		{
			return protocolInformation;
		}
		set
		{
			protocolInformation = value;
		}
	}

	/// <summary>Gets or sets the options for a <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <returns>A <see cref="T:System.Net.Sockets.SocketInformationOptions" /> instance.</returns>
	public SocketInformationOptions Options
	{
		get
		{
			return options;
		}
		set
		{
			options = value;
		}
	}

	internal bool IsNonBlocking
	{
		get
		{
			return (options & SocketInformationOptions.NonBlocking) != 0;
		}
		set
		{
			if (value)
			{
				options |= SocketInformationOptions.NonBlocking;
			}
			else
			{
				options &= ~SocketInformationOptions.NonBlocking;
			}
		}
	}

	internal bool IsConnected
	{
		get
		{
			return (options & SocketInformationOptions.Connected) != 0;
		}
		set
		{
			if (value)
			{
				options |= SocketInformationOptions.Connected;
			}
			else
			{
				options &= ~SocketInformationOptions.Connected;
			}
		}
	}

	internal bool IsListening
	{
		get
		{
			return (options & SocketInformationOptions.Listening) != 0;
		}
		set
		{
			if (value)
			{
				options |= SocketInformationOptions.Listening;
			}
			else
			{
				options &= ~SocketInformationOptions.Listening;
			}
		}
	}

	internal bool UseOnlyOverlappedIO
	{
		get
		{
			return (options & SocketInformationOptions.UseOnlyOverlappedIO) != 0;
		}
		set
		{
			if (value)
			{
				options |= SocketInformationOptions.UseOnlyOverlappedIO;
			}
			else
			{
				options &= ~SocketInformationOptions.UseOnlyOverlappedIO;
			}
		}
	}

	internal EndPoint RemoteEndPoint
	{
		get
		{
			return remoteEndPoint;
		}
		set
		{
			remoteEndPoint = value;
		}
	}
}
