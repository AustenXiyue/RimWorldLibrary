namespace System.Windows.Media.Imaging;

internal static class ValidateEnums
{
	public static bool IsRotationValid(object valueObject)
	{
		Rotation rotation = (Rotation)valueObject;
		if (rotation != 0 && rotation != Rotation.Rotate90 && rotation != Rotation.Rotate180)
		{
			return rotation == Rotation.Rotate270;
		}
		return true;
	}
}
