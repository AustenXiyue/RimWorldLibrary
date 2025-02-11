using System.IO;

namespace NAudio.Wave;

public class RawSourceWaveStream : WaveStream
{
	private Stream sourceStream;

	private WaveFormat waveFormat;

	public override WaveFormat WaveFormat => waveFormat;

	public override long Length => sourceStream.Length;

	public override long Position
	{
		get
		{
			return sourceStream.Position;
		}
		set
		{
			sourceStream.Position = value;
		}
	}

	public RawSourceWaveStream(Stream sourceStream, WaveFormat waveFormat)
	{
		this.sourceStream = sourceStream;
		this.waveFormat = waveFormat;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		try
		{
			return sourceStream.Read(buffer, offset, count);
		}
		catch (EndOfStreamException)
		{
			return 0;
		}
	}
}
