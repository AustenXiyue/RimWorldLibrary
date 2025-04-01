using System.ComponentModel;
using Unity;

namespace System.Net;

/// <summary>Provides data for the <see cref="E:System.Net.WebClient.DownloadProgressChanged" /> event of a <see cref="T:System.Net.WebClient" />.</summary>
public class DownloadProgressChangedEventArgs : ProgressChangedEventArgs
{
	private long m_BytesReceived;

	private long m_TotalBytesToReceive;

	/// <summary>Gets the number of bytes received.</summary>
	/// <returns>An <see cref="T:System.Int64" /> value that indicates the number of bytes received.</returns>
	public long BytesReceived => m_BytesReceived;

	/// <summary>Gets the total number of bytes in a <see cref="T:System.Net.WebClient" /> data download operation.</summary>
	/// <returns>An <see cref="T:System.Int64" /> value that indicates the number of bytes that will be received.</returns>
	public long TotalBytesToReceive => m_TotalBytesToReceive;

	internal DownloadProgressChangedEventArgs(int progressPercentage, object userToken, long bytesReceived, long totalBytesToReceive)
		: base(progressPercentage, userToken)
	{
		m_BytesReceived = bytesReceived;
		m_TotalBytesToReceive = totalBytesToReceive;
	}

	internal DownloadProgressChangedEventArgs()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
