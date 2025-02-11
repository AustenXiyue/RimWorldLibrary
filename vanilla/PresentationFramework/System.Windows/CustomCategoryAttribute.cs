using System.ComponentModel;

namespace System.Windows;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
internal sealed class CustomCategoryAttribute : CategoryAttribute
{
	internal CustomCategoryAttribute(string name)
		: base(name)
	{
	}

	protected override string GetLocalizedString(string value)
	{
		if (string.Compare(value, "Content", StringComparison.Ordinal) == 0)
		{
			return SR.DesignerMetadata_CustomCategory_Content;
		}
		if (string.Compare(value, "Accessibility", StringComparison.Ordinal) == 0)
		{
			return SR.DesignerMetadata_CustomCategory_Accessibility;
		}
		return SR.DesignerMetadata_CustomCategory_Navigation;
	}
}
