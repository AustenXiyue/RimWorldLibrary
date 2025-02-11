using System.IO;
using MS.Internal;

namespace System.Windows.Markup;

internal class XamlPathDataSerializer : XamlSerializer
{
	public override bool ConvertStringToCustomBinary(BinaryWriter writer, string stringValue)
	{
		Parsers.PathMinilanguageToBinary(writer, stringValue);
		return true;
	}

	public override object ConvertCustomBinaryToObject(BinaryReader reader)
	{
		return Parsers.DeserializeStreamGeometry(reader);
	}

	public static object StaticConvertCustomBinaryToObject(BinaryReader reader)
	{
		return Parsers.DeserializeStreamGeometry(reader);
	}
}
