using System.Collections.Generic;
using System.Globalization;

namespace System.Windows.Media;

internal static class KnownColors
{
	private static Dictionary<uint, SolidColorBrush> s_solidColorBrushCache;

	private static Dictionary<string, KnownColor> s_knownArgbColors;

	static KnownColors()
	{
		s_solidColorBrushCache = new Dictionary<uint, SolidColorBrush>();
		s_knownArgbColors = new Dictionary<string, KnownColor>();
		foreach (KnownColor value in Enum.GetValues(typeof(KnownColor)))
		{
			string key = $"#{(uint)value,8:X8}";
			s_knownArgbColors[key] = value;
		}
	}

	public static SolidColorBrush ColorStringToKnownBrush(string s)
	{
		if (s != null)
		{
			KnownColor knownColor = ColorStringToKnownColor(s);
			if (knownColor != KnownColor.UnknownColor)
			{
				return SolidColorBrushFromUint((uint)knownColor);
			}
		}
		return null;
	}

	public static bool IsKnownSolidColorBrush(SolidColorBrush scp)
	{
		lock (s_solidColorBrushCache)
		{
			return s_solidColorBrushCache.ContainsValue(scp);
		}
	}

	public static SolidColorBrush SolidColorBrushFromUint(uint argb)
	{
		SolidColorBrush value = null;
		lock (s_solidColorBrushCache)
		{
			if (!s_solidColorBrushCache.TryGetValue(argb, out value))
			{
				value = new SolidColorBrush(Color.FromUInt32(argb));
				value.Freeze();
				s_solidColorBrushCache[argb] = value;
			}
		}
		return value;
	}

	internal static string MatchColor(string colorString, out bool isKnownColor, out bool isNumericColor, out bool isContextColor, out bool isScRgbColor)
	{
		string text = colorString.Trim();
		if ((text.Length == 4 || text.Length == 5 || text.Length == 7 || text.Length == 9) && text[0] == '#')
		{
			isNumericColor = true;
			isScRgbColor = false;
			isKnownColor = false;
			isContextColor = false;
			return text;
		}
		isNumericColor = false;
		if (text.StartsWith("sc#", StringComparison.Ordinal))
		{
			isNumericColor = false;
			isScRgbColor = true;
			isKnownColor = false;
			isContextColor = false;
		}
		else
		{
			isScRgbColor = false;
		}
		if (text.StartsWith("ContextColor ", StringComparison.OrdinalIgnoreCase))
		{
			isContextColor = true;
			isScRgbColor = false;
			isKnownColor = false;
			return text;
		}
		isContextColor = false;
		isKnownColor = true;
		return text;
	}

	internal static KnownColor ColorStringToKnownColor(string colorString)
	{
		if (colorString != null)
		{
			string text = colorString.ToUpper(CultureInfo.InvariantCulture);
			switch (text.Length)
			{
			case 3:
				if (text.Equals("RED"))
				{
					return KnownColor.Red;
				}
				if (text.Equals("TAN"))
				{
					return KnownColor.Tan;
				}
				break;
			case 4:
				switch (text[0])
				{
				case 'A':
					if (text.Equals("AQUA"))
					{
						return KnownColor.Aqua;
					}
					break;
				case 'B':
					if (text.Equals("BLUE"))
					{
						return KnownColor.Blue;
					}
					break;
				case 'C':
					if (text.Equals("CYAN"))
					{
						return KnownColor.Aqua;
					}
					break;
				case 'G':
					if (text.Equals("GOLD"))
					{
						return KnownColor.Gold;
					}
					if (text.Equals("GRAY"))
					{
						return KnownColor.Gray;
					}
					break;
				case 'L':
					if (text.Equals("LIME"))
					{
						return KnownColor.Lime;
					}
					break;
				case 'N':
					if (text.Equals("NAVY"))
					{
						return KnownColor.Navy;
					}
					break;
				case 'P':
					if (text.Equals("PERU"))
					{
						return KnownColor.Peru;
					}
					if (text.Equals("PINK"))
					{
						return KnownColor.Pink;
					}
					if (text.Equals("PLUM"))
					{
						return KnownColor.Plum;
					}
					break;
				case 'S':
					if (text.Equals("SNOW"))
					{
						return KnownColor.Snow;
					}
					break;
				case 'T':
					if (text.Equals("TEAL"))
					{
						return KnownColor.Teal;
					}
					break;
				}
				break;
			case 5:
				switch (text[0])
				{
				case 'A':
					if (text.Equals("AZURE"))
					{
						return KnownColor.Azure;
					}
					break;
				case 'B':
					if (text.Equals("BEIGE"))
					{
						return KnownColor.Beige;
					}
					if (text.Equals("BLACK"))
					{
						return KnownColor.Black;
					}
					if (text.Equals("BROWN"))
					{
						return KnownColor.Brown;
					}
					break;
				case 'C':
					if (text.Equals("CORAL"))
					{
						return KnownColor.Coral;
					}
					break;
				case 'G':
					if (text.Equals("GREEN"))
					{
						return KnownColor.Green;
					}
					break;
				case 'I':
					if (text.Equals("IVORY"))
					{
						return KnownColor.Ivory;
					}
					break;
				case 'K':
					if (text.Equals("KHAKI"))
					{
						return KnownColor.Khaki;
					}
					break;
				case 'L':
					if (text.Equals("LINEN"))
					{
						return KnownColor.Linen;
					}
					break;
				case 'O':
					if (text.Equals("OLIVE"))
					{
						return KnownColor.Olive;
					}
					break;
				case 'W':
					if (text.Equals("WHEAT"))
					{
						return KnownColor.Wheat;
					}
					if (text.Equals("WHITE"))
					{
						return KnownColor.White;
					}
					break;
				}
				break;
			case 6:
				switch (text[0])
				{
				case 'B':
					if (text.Equals("BISQUE"))
					{
						return KnownColor.Bisque;
					}
					break;
				case 'I':
					if (text.Equals("INDIGO"))
					{
						return KnownColor.Indigo;
					}
					break;
				case 'M':
					if (text.Equals("MAROON"))
					{
						return KnownColor.Maroon;
					}
					break;
				case 'O':
					if (text.Equals("ORANGE"))
					{
						return KnownColor.Orange;
					}
					if (text.Equals("ORCHID"))
					{
						return KnownColor.Orchid;
					}
					break;
				case 'P':
					if (text.Equals("PURPLE"))
					{
						return KnownColor.Purple;
					}
					break;
				case 'S':
					if (text.Equals("SALMON"))
					{
						return KnownColor.Salmon;
					}
					if (text.Equals("SIENNA"))
					{
						return KnownColor.Sienna;
					}
					if (text.Equals("SILVER"))
					{
						return KnownColor.Silver;
					}
					break;
				case 'T':
					if (text.Equals("TOMATO"))
					{
						return KnownColor.Tomato;
					}
					break;
				case 'V':
					if (text.Equals("VIOLET"))
					{
						return KnownColor.Violet;
					}
					break;
				case 'Y':
					if (text.Equals("YELLOW"))
					{
						return KnownColor.Yellow;
					}
					break;
				}
				break;
			case 7:
				switch (text[0])
				{
				case 'C':
					if (text.Equals("CRIMSON"))
					{
						return KnownColor.Crimson;
					}
					break;
				case 'D':
					if (text.Equals("DARKRED"))
					{
						return KnownColor.DarkRed;
					}
					if (text.Equals("DIMGRAY"))
					{
						return KnownColor.DimGray;
					}
					break;
				case 'F':
					if (text.Equals("FUCHSIA"))
					{
						return KnownColor.Fuchsia;
					}
					break;
				case 'H':
					if (text.Equals("HOTPINK"))
					{
						return KnownColor.HotPink;
					}
					break;
				case 'M':
					if (text.Equals("MAGENTA"))
					{
						return KnownColor.Fuchsia;
					}
					break;
				case 'O':
					if (text.Equals("OLDLACE"))
					{
						return KnownColor.OldLace;
					}
					break;
				case 'S':
					if (text.Equals("SKYBLUE"))
					{
						return KnownColor.SkyBlue;
					}
					break;
				case 'T':
					if (text.Equals("THISTLE"))
					{
						return KnownColor.Thistle;
					}
					break;
				}
				break;
			case 8:
				switch (text[0])
				{
				case 'C':
					if (text.Equals("CORNSILK"))
					{
						return KnownColor.Cornsilk;
					}
					break;
				case 'D':
					if (text.Equals("DARKBLUE"))
					{
						return KnownColor.DarkBlue;
					}
					if (text.Equals("DARKCYAN"))
					{
						return KnownColor.DarkCyan;
					}
					if (text.Equals("DARKGRAY"))
					{
						return KnownColor.DarkGray;
					}
					if (text.Equals("DEEPPINK"))
					{
						return KnownColor.DeepPink;
					}
					break;
				case 'H':
					if (text.Equals("HONEYDEW"))
					{
						return KnownColor.Honeydew;
					}
					break;
				case 'L':
					if (text.Equals("LAVENDER"))
					{
						return KnownColor.Lavender;
					}
					break;
				case 'M':
					if (text.Equals("MOCCASIN"))
					{
						return KnownColor.Moccasin;
					}
					break;
				case 'S':
					if (text.Equals("SEAGREEN"))
					{
						return KnownColor.SeaGreen;
					}
					if (text.Equals("SEASHELL"))
					{
						return KnownColor.SeaShell;
					}
					break;
				}
				break;
			case 9:
				switch (text[0])
				{
				case 'A':
					if (text.Equals("ALICEBLUE"))
					{
						return KnownColor.AliceBlue;
					}
					break;
				case 'B':
					if (text.Equals("BURLYWOOD"))
					{
						return KnownColor.BurlyWood;
					}
					break;
				case 'C':
					if (text.Equals("CADETBLUE"))
					{
						return KnownColor.CadetBlue;
					}
					if (text.Equals("CHOCOLATE"))
					{
						return KnownColor.Chocolate;
					}
					break;
				case 'D':
					if (text.Equals("DARKGREEN"))
					{
						return KnownColor.DarkGreen;
					}
					if (text.Equals("DARKKHAKI"))
					{
						return KnownColor.DarkKhaki;
					}
					break;
				case 'F':
					if (text.Equals("FIREBRICK"))
					{
						return KnownColor.Firebrick;
					}
					break;
				case 'G':
					if (text.Equals("GAINSBORO"))
					{
						return KnownColor.Gainsboro;
					}
					if (text.Equals("GOLDENROD"))
					{
						return KnownColor.Goldenrod;
					}
					break;
				case 'I':
					if (text.Equals("INDIANRED"))
					{
						return KnownColor.IndianRed;
					}
					break;
				case 'L':
					if (text.Equals("LAWNGREEN"))
					{
						return KnownColor.LawnGreen;
					}
					if (text.Equals("LIGHTBLUE"))
					{
						return KnownColor.LightBlue;
					}
					if (text.Equals("LIGHTCYAN"))
					{
						return KnownColor.LightCyan;
					}
					if (text.Equals("LIGHTGRAY"))
					{
						return KnownColor.LightGray;
					}
					if (text.Equals("LIGHTPINK"))
					{
						return KnownColor.LightPink;
					}
					if (text.Equals("LIMEGREEN"))
					{
						return KnownColor.LimeGreen;
					}
					break;
				case 'M':
					if (text.Equals("MINTCREAM"))
					{
						return KnownColor.MintCream;
					}
					if (text.Equals("MISTYROSE"))
					{
						return KnownColor.MistyRose;
					}
					break;
				case 'O':
					if (text.Equals("OLIVEDRAB"))
					{
						return KnownColor.OliveDrab;
					}
					if (text.Equals("ORANGERED"))
					{
						return KnownColor.OrangeRed;
					}
					break;
				case 'P':
					if (text.Equals("PALEGREEN"))
					{
						return KnownColor.PaleGreen;
					}
					if (text.Equals("PEACHPUFF"))
					{
						return KnownColor.PeachPuff;
					}
					break;
				case 'R':
					if (text.Equals("ROSYBROWN"))
					{
						return KnownColor.RosyBrown;
					}
					if (text.Equals("ROYALBLUE"))
					{
						return KnownColor.RoyalBlue;
					}
					break;
				case 'S':
					if (text.Equals("SLATEBLUE"))
					{
						return KnownColor.SlateBlue;
					}
					if (text.Equals("SLATEGRAY"))
					{
						return KnownColor.SlateGray;
					}
					if (text.Equals("STEELBLUE"))
					{
						return KnownColor.SteelBlue;
					}
					break;
				case 'T':
					if (text.Equals("TURQUOISE"))
					{
						return KnownColor.Turquoise;
					}
					break;
				}
				break;
			case 10:
				switch (text[0])
				{
				case 'A':
					if (text.Equals("AQUAMARINE"))
					{
						return KnownColor.Aquamarine;
					}
					break;
				case 'B':
					if (text.Equals("BLUEVIOLET"))
					{
						return KnownColor.BlueViolet;
					}
					break;
				case 'C':
					if (text.Equals("CHARTREUSE"))
					{
						return KnownColor.Chartreuse;
					}
					break;
				case 'D':
					if (text.Equals("DARKORANGE"))
					{
						return KnownColor.DarkOrange;
					}
					if (text.Equals("DARKORCHID"))
					{
						return KnownColor.DarkOrchid;
					}
					if (text.Equals("DARKSALMON"))
					{
						return KnownColor.DarkSalmon;
					}
					if (text.Equals("DARKVIOLET"))
					{
						return KnownColor.DarkViolet;
					}
					if (text.Equals("DODGERBLUE"))
					{
						return KnownColor.DodgerBlue;
					}
					break;
				case 'G':
					if (text.Equals("GHOSTWHITE"))
					{
						return KnownColor.GhostWhite;
					}
					break;
				case 'L':
					if (text.Equals("LIGHTCORAL"))
					{
						return KnownColor.LightCoral;
					}
					if (text.Equals("LIGHTGREEN"))
					{
						return KnownColor.LightGreen;
					}
					break;
				case 'M':
					if (text.Equals("MEDIUMBLUE"))
					{
						return KnownColor.MediumBlue;
					}
					break;
				case 'P':
					if (text.Equals("PAPAYAWHIP"))
					{
						return KnownColor.PapayaWhip;
					}
					if (text.Equals("POWDERBLUE"))
					{
						return KnownColor.PowderBlue;
					}
					break;
				case 'S':
					if (text.Equals("SANDYBROWN"))
					{
						return KnownColor.SandyBrown;
					}
					break;
				case 'W':
					if (text.Equals("WHITESMOKE"))
					{
						return KnownColor.WhiteSmoke;
					}
					break;
				}
				break;
			case 11:
				switch (text[0])
				{
				case 'D':
					if (text.Equals("DARKMAGENTA"))
					{
						return KnownColor.DarkMagenta;
					}
					if (text.Equals("DEEPSKYBLUE"))
					{
						return KnownColor.DeepSkyBlue;
					}
					break;
				case 'F':
					if (text.Equals("FLORALWHITE"))
					{
						return KnownColor.FloralWhite;
					}
					if (text.Equals("FORESTGREEN"))
					{
						return KnownColor.ForestGreen;
					}
					break;
				case 'G':
					if (text.Equals("GREENYELLOW"))
					{
						return KnownColor.GreenYellow;
					}
					break;
				case 'L':
					if (text.Equals("LIGHTSALMON"))
					{
						return KnownColor.LightSalmon;
					}
					if (text.Equals("LIGHTYELLOW"))
					{
						return KnownColor.LightYellow;
					}
					break;
				case 'N':
					if (text.Equals("NAVAJOWHITE"))
					{
						return KnownColor.NavajoWhite;
					}
					break;
				case 'S':
					if (text.Equals("SADDLEBROWN"))
					{
						return KnownColor.SaddleBrown;
					}
					if (text.Equals("SPRINGGREEN"))
					{
						return KnownColor.SpringGreen;
					}
					break;
				case 'T':
					if (text.Equals("TRANSPARENT"))
					{
						return KnownColor.Transparent;
					}
					break;
				case 'Y':
					if (text.Equals("YELLOWGREEN"))
					{
						return KnownColor.YellowGreen;
					}
					break;
				}
				break;
			case 12:
				switch (text[0])
				{
				case 'A':
					if (text.Equals("ANTIQUEWHITE"))
					{
						return KnownColor.AntiqueWhite;
					}
					break;
				case 'D':
					if (text.Equals("DARKSEAGREEN"))
					{
						return KnownColor.DarkSeaGreen;
					}
					break;
				case 'L':
					if (text.Equals("LIGHTSKYBLUE"))
					{
						return KnownColor.LightSkyBlue;
					}
					if (text.Equals("LEMONCHIFFON"))
					{
						return KnownColor.LemonChiffon;
					}
					break;
				case 'M':
					if (text.Equals("MEDIUMORCHID"))
					{
						return KnownColor.MediumOrchid;
					}
					if (text.Equals("MEDIUMPURPLE"))
					{
						return KnownColor.MediumPurple;
					}
					if (text.Equals("MIDNIGHTBLUE"))
					{
						return KnownColor.MidnightBlue;
					}
					break;
				}
				break;
			case 13:
				switch (text[0])
				{
				case 'D':
					if (text.Equals("DARKSLATEBLUE"))
					{
						return KnownColor.DarkSlateBlue;
					}
					if (text.Equals("DARKSLATEGRAY"))
					{
						return KnownColor.DarkSlateGray;
					}
					if (text.Equals("DARKGOLDENROD"))
					{
						return KnownColor.DarkGoldenrod;
					}
					if (text.Equals("DARKTURQUOISE"))
					{
						return KnownColor.DarkTurquoise;
					}
					break;
				case 'L':
					if (text.Equals("LIGHTSEAGREEN"))
					{
						return KnownColor.LightSeaGreen;
					}
					if (text.Equals("LAVENDERBLUSH"))
					{
						return KnownColor.LavenderBlush;
					}
					break;
				case 'P':
					if (text.Equals("PALEGOLDENROD"))
					{
						return KnownColor.PaleGoldenrod;
					}
					if (text.Equals("PALETURQUOISE"))
					{
						return KnownColor.PaleTurquoise;
					}
					if (text.Equals("PALEVIOLETRED"))
					{
						return KnownColor.PaleVioletRed;
					}
					break;
				}
				break;
			case 14:
				switch (text[0])
				{
				case 'B':
					if (text.Equals("BLANCHEDALMOND"))
					{
						return KnownColor.BlanchedAlmond;
					}
					break;
				case 'C':
					if (text.Equals("CORNFLOWERBLUE"))
					{
						return KnownColor.CornflowerBlue;
					}
					break;
				case 'D':
					if (text.Equals("DARKOLIVEGREEN"))
					{
						return KnownColor.DarkOliveGreen;
					}
					break;
				case 'L':
					if (text.Equals("LIGHTSLATEGRAY"))
					{
						return KnownColor.LightSlateGray;
					}
					if (text.Equals("LIGHTSTEELBLUE"))
					{
						return KnownColor.LightSteelBlue;
					}
					break;
				case 'M':
					if (text.Equals("MEDIUMSEAGREEN"))
					{
						return KnownColor.MediumSeaGreen;
					}
					break;
				}
				break;
			case 15:
				if (text.Equals("MEDIUMSLATEBLUE"))
				{
					return KnownColor.MediumSlateBlue;
				}
				if (text.Equals("MEDIUMTURQUOISE"))
				{
					return KnownColor.MediumTurquoise;
				}
				if (text.Equals("MEDIUMVIOLETRED"))
				{
					return KnownColor.MediumVioletRed;
				}
				break;
			case 16:
				if (text.Equals("MEDIUMAQUAMARINE"))
				{
					return KnownColor.MediumAquamarine;
				}
				break;
			case 17:
				if (text.Equals("MEDIUMSPRINGGREEN"))
				{
					return KnownColor.MediumSpringGreen;
				}
				break;
			case 20:
				if (text.Equals("LIGHTGOLDENRODYELLOW"))
				{
					return KnownColor.LightGoldenrodYellow;
				}
				break;
			}
		}
		return KnownColor.UnknownColor;
	}

	internal static KnownColor ArgbStringToKnownColor(string argbString)
	{
		string key = argbString.Trim().ToUpper(CultureInfo.InvariantCulture);
		if (s_knownArgbColors.TryGetValue(key, out var value))
		{
			return value;
		}
		return KnownColor.UnknownColor;
	}
}
