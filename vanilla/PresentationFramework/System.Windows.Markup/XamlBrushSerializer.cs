using System.IO;
using System.Windows.Media;

namespace System.Windows.Markup;

internal class XamlBrushSerializer : XamlSerializer
{
	public override bool ConvertStringToCustomBinary(BinaryWriter writer, string stringValue)
	{
		return SolidColorBrush.SerializeOn(writer, stringValue.Trim());
	}

	public override object ConvertCustomBinaryToObject(BinaryReader reader)
	{
		return SolidColorBrush.DeserializeFrom(reader);
	}
}
