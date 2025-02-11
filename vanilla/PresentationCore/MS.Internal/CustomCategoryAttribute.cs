using System.ComponentModel;
using MS.Internal.PresentationCore;

namespace MS.Internal;

internal sealed class CustomCategoryAttribute : CategoryAttribute
{
	internal CustomCategoryAttribute()
	{
	}

	internal CustomCategoryAttribute(string category)
		: base(category)
	{
	}

	protected override string GetLocalizedString(string value)
	{
		string resourceString = SR.GetResourceString(value);
		if (resourceString != null)
		{
			return resourceString;
		}
		return value;
	}
}
