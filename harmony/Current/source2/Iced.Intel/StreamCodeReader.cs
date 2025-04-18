using System.IO;

namespace Iced.Intel;

internal sealed class StreamCodeReader : CodeReader
{
	public readonly Stream Stream;

	public StreamCodeReader(Stream stream)
	{
		Stream = stream;
	}

	public override int ReadByte()
	{
		return Stream.ReadByte();
	}
}
