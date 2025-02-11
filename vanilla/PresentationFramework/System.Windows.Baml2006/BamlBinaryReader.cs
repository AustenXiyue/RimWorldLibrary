using System.IO;

namespace System.Windows.Baml2006;

internal class BamlBinaryReader : BinaryReader
{
	public BamlBinaryReader(Stream stream)
		: base(stream)
	{
	}

	public new int Read7BitEncodedInt()
	{
		return base.Read7BitEncodedInt();
	}
}
