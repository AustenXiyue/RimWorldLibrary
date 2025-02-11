using System.IO;
using System.Windows.Media.Media3D;
using MS.Internal.Media;

namespace System.Windows.Markup;

internal class XamlPoint3DCollectionSerializer : XamlSerializer
{
	public override bool ConvertStringToCustomBinary(BinaryWriter writer, string stringValue)
	{
		return XamlSerializationHelper.SerializePoint3D(writer, stringValue);
	}

	public override object ConvertCustomBinaryToObject(BinaryReader reader)
	{
		return Point3DCollection.DeserializeFrom(reader);
	}

	public static object StaticConvertCustomBinaryToObject(BinaryReader reader)
	{
		return Point3DCollection.DeserializeFrom(reader);
	}
}
