using System.IO;
using System.Text;

namespace System.Windows.Markup;

internal class BamlBinaryWriter : BinaryWriter
{
	public BamlBinaryWriter(Stream stream, Encoding code)
		: base(stream, code)
	{
	}

	public new void Write7BitEncodedInt(int value)
	{
		base.Write7BitEncodedInt(value);
	}

	public static int SizeOf7bitEncodedSize(int size)
	{
		if ((size & -128) == 0)
		{
			return 1;
		}
		if ((size & -16384) == 0)
		{
			return 2;
		}
		if ((size & -2097152) == 0)
		{
			return 3;
		}
		if ((size & -268435456) == 0)
		{
			return 4;
		}
		return 5;
	}
}
