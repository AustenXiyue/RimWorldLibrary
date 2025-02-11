using System.IO;
using System.Windows.Media.Media3D;
using MS.Internal.Media;

namespace System.Windows.Markup;

internal class XamlVector3DCollectionSerializer : XamlSerializer
{
	internal XamlVector3DCollectionSerializer()
	{
	}

	public override bool ConvertStringToCustomBinary(BinaryWriter writer, string stringValue)
	{
		return XamlSerializationHelper.SerializeVector3D(writer, stringValue);
	}

	public override object ConvertCustomBinaryToObject(BinaryReader reader)
	{
		return Vector3DCollection.DeserializeFrom(reader);
	}

	public static object StaticConvertCustomBinaryToObject(BinaryReader reader)
	{
		return Vector3DCollection.DeserializeFrom(reader);
	}
}
