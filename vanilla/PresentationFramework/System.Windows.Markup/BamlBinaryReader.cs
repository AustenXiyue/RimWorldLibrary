using System.IO;
using System.Text;

namespace System.Windows.Markup;

internal class BamlBinaryReader : BinaryReader
{
	public BamlBinaryReader(Stream stream, Encoding code)
		: base(stream, code)
	{
	}

	public new int Read7BitEncodedInt()
	{
		return base.Read7BitEncodedInt();
	}
}
