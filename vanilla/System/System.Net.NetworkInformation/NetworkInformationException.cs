using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Net.NetworkInformation;

/// <summary>The exception that is thrown when an error occurs while retrieving network information.</summary>
[Serializable]
public class NetworkInformationException : Win32Exception
{
	/// <summary>Gets the Win32 error code for this exception.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that contains the Win32 error code.</returns>
	public override int ErrorCode => base.NativeErrorCode;

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.NetworkInformation.NetworkInformationException" /> class.</summary>
	public NetworkInformationException()
		: base(Marshal.GetLastWin32Error())
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.NetworkInformation.NetworkInformationException" /> class with the specified error code.</summary>
	/// <param name="errorCode">A Win32 error code. </param>
	public NetworkInformationException(int errorCode)
		: base(errorCode)
	{
	}

	internal NetworkInformationException(SocketError socketError)
		: base((int)socketError)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.NetworkInformation.NetworkInformationException" /> class with serialized data.</summary>
	/// <param name="serializationInfo">A SerializationInfo object that contains the serialized exception data. </param>
	/// <param name="streamingContext">A StreamingContext that contains contextual information about the serialized exception. </param>
	protected NetworkInformationException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
