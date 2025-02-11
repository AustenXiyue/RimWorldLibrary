using System.IO;
using System.Windows.Media;
using MS.Internal.Media;

namespace System.Windows.Markup;

internal class XamlPointCollectionSerializer : XamlSerializer
{
	public override bool ConvertStringToCustomBinary(BinaryWriter writer, string stringValue)
	{
		return XamlSerializationHelper.SerializePoint(writer, stringValue);
	}

	public override object ConvertCustomBinaryToObject(BinaryReader reader)
	{
		return PointCollection.DeserializeFrom(reader);
	}

	public static object StaticConvertCustomBinaryToObject(BinaryReader reader)
	{
		return PointCollection.DeserializeFrom(reader);
	}
}
