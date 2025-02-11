using System.Windows;

namespace MS.Internal.Globalization;

internal class LocalizabilityGroup
{
	private const int InvalidValue = -1;

	internal Modifiability Modifiability;

	internal Readability Readability;

	internal LocalizationCategory Category;

	internal LocalizabilityGroup()
	{
		Modifiability = (Modifiability)(-1);
		Readability = (Readability)(-1);
		Category = (LocalizationCategory)(-1);
	}

	internal LocalizabilityAttribute Override(LocalizabilityAttribute attribute)
	{
		Modifiability modifiability = attribute.Modifiability;
		Readability readability = attribute.Readability;
		LocalizationCategory category = attribute.Category;
		bool flag = false;
		if (Modifiability != (Modifiability)(-1))
		{
			modifiability = Modifiability;
			flag = true;
		}
		if (Readability != (Readability)(-1))
		{
			readability = Readability;
			flag = true;
		}
		if (Category != (LocalizationCategory)(-1))
		{
			category = Category;
			flag = true;
		}
		if (flag)
		{
			attribute = new LocalizabilityAttribute(category);
			attribute.Modifiability = modifiability;
			attribute.Readability = readability;
		}
		return attribute;
	}
}
