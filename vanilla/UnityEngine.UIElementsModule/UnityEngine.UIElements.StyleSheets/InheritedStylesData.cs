using System;

namespace UnityEngine.UIElements.StyleSheets;

internal class InheritedStylesData : IEquatable<InheritedStylesData>
{
	public static readonly InheritedStylesData none = new InheritedStylesData();

	public StyleColor color;

	public StyleFont font;

	public StyleLength fontSize;

	public StyleInt unityFontStyle;

	public StyleInt unityTextAlign;

	public StyleInt visibility;

	public StyleInt whiteSpace;

	public InheritedStylesData()
	{
		color = StyleSheetCache.GetInitialValue(StylePropertyID.Color).color;
	}

	public InheritedStylesData(InheritedStylesData other)
	{
		CopyFrom(other);
	}

	public void CopyFrom(InheritedStylesData other)
	{
		if (other != null)
		{
			color = other.color;
			font = other.font;
			fontSize = other.fontSize;
			visibility = other.visibility;
			whiteSpace = other.whiteSpace;
			unityFontStyle = other.unityFontStyle;
			unityTextAlign = other.unityTextAlign;
		}
	}

	public bool Equals(InheritedStylesData other)
	{
		if (other == null)
		{
			return false;
		}
		return color == other.color && font == other.font && fontSize == other.fontSize && unityFontStyle == other.unityFontStyle && unityTextAlign == other.unityTextAlign && visibility == other.visibility && whiteSpace == other.whiteSpace;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is InheritedStylesData))
		{
			return false;
		}
		InheritedStylesData other = (InheritedStylesData)obj;
		return Equals(other);
	}

	public override int GetHashCode()
	{
		int num = -2037960190;
		num = num * -1521134295 + color.GetHashCode();
		num = num * -1521134295 + font.GetHashCode();
		num = num * -1521134295 + fontSize.GetHashCode();
		num = num * -1521134295 + unityFontStyle.GetHashCode();
		num = num * -1521134295 + unityTextAlign.GetHashCode();
		num = num * -1521134295 + visibility.GetHashCode();
		return num * -1521134295 + whiteSpace.GetHashCode();
	}
}
