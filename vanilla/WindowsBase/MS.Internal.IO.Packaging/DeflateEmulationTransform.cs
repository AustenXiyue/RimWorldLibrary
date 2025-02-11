using System.IO;
using System.IO.Compression;

namespace MS.Internal.IO.Packaging;

internal class DeflateEmulationTransform : IDeflateTransform
{
	private byte[] _buffer;

	private byte[] Buffer
	{
		get
		{
			if (_buffer == null)
			{
				_buffer = new byte[4096];
			}
			return _buffer;
		}
	}

	public void Decompress(Stream source, Stream sink)
	{
		using DeflateStream deflateStream = new DeflateStream(source, CompressionMode.Decompress, leaveOpen: true);
		int num = 0;
		do
		{
			num = deflateStream.Read(Buffer, 0, Buffer.Length);
			if (num > 0)
			{
				sink.Write(Buffer, 0, num);
			}
		}
		while (num > 0);
	}

	public void Compress(Stream source, Stream sink)
	{
		using (DeflateStream deflateStream = new DeflateStream(sink, CompressionMode.Compress, leaveOpen: true))
		{
			int num = 0;
			do
			{
				num = source.Read(Buffer, 0, Buffer.Length);
				if (num > 0)
				{
					deflateStream.Write(Buffer, 0, num);
				}
			}
			while (num > 0);
		}
		if (sink.CanSeek)
		{
			sink.SetLength(sink.Position);
		}
	}
}
