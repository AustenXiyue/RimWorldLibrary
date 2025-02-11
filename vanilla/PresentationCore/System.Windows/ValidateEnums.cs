namespace System.Windows;

internal static class ValidateEnums
{
	public static bool IsTextDecorationLocationValid(object valueObject)
	{
		TextDecorationLocation textDecorationLocation = (TextDecorationLocation)valueObject;
		if (textDecorationLocation != 0 && textDecorationLocation != TextDecorationLocation.OverLine && textDecorationLocation != TextDecorationLocation.Strikethrough)
		{
			return textDecorationLocation == TextDecorationLocation.Baseline;
		}
		return true;
	}

	public static bool IsTextDecorationUnitValid(object valueObject)
	{
		TextDecorationUnit textDecorationUnit = (TextDecorationUnit)valueObject;
		if (textDecorationUnit != 0 && textDecorationUnit != TextDecorationUnit.FontRenderingEmSize)
		{
			return textDecorationUnit == TextDecorationUnit.Pixel;
		}
		return true;
	}
}
