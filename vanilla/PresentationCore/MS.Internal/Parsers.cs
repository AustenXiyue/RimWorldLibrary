using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Markup;
using System.Windows.Media;
using MS.Internal.Media;
using MS.Internal.PresentationCore;

namespace MS.Internal;

internal static class Parsers
{
	private const int s_zeroChar = 48;

	private const int s_aLower = 97;

	private const int s_aUpper = 65;

	internal const string s_ContextColor = "ContextColor ";

	internal const string s_ContextColorNoSpace = "ContextColor";

	private static int ParseHexChar(char c)
	{
		if (c >= '0' && c <= '9')
		{
			return c - 48;
		}
		if (c >= 'a' && c <= 'f')
		{
			return c - 97 + 10;
		}
		if (c >= 'A' && c <= 'F')
		{
			return c - 65 + 10;
		}
		throw new FormatException(SR.Parsers_IllegalToken);
	}

	private static Color ParseHexColor(string trimmedColor)
	{
		int num = 255;
		int num2;
		int num3;
		int num4;
		if (trimmedColor.Length > 7)
		{
			num = ParseHexChar(trimmedColor[1]) * 16 + ParseHexChar(trimmedColor[2]);
			num2 = ParseHexChar(trimmedColor[3]) * 16 + ParseHexChar(trimmedColor[4]);
			num3 = ParseHexChar(trimmedColor[5]) * 16 + ParseHexChar(trimmedColor[6]);
			num4 = ParseHexChar(trimmedColor[7]) * 16 + ParseHexChar(trimmedColor[8]);
		}
		else if (trimmedColor.Length > 5)
		{
			num2 = ParseHexChar(trimmedColor[1]) * 16 + ParseHexChar(trimmedColor[2]);
			num3 = ParseHexChar(trimmedColor[3]) * 16 + ParseHexChar(trimmedColor[4]);
			num4 = ParseHexChar(trimmedColor[5]) * 16 + ParseHexChar(trimmedColor[6]);
		}
		else if (trimmedColor.Length > 4)
		{
			num = ParseHexChar(trimmedColor[1]);
			num += num * 16;
			num2 = ParseHexChar(trimmedColor[2]);
			num2 += num2 * 16;
			num3 = ParseHexChar(trimmedColor[3]);
			num3 += num3 * 16;
			num4 = ParseHexChar(trimmedColor[4]);
			num4 += num4 * 16;
		}
		else
		{
			num2 = ParseHexChar(trimmedColor[1]);
			num2 += num2 * 16;
			num3 = ParseHexChar(trimmedColor[2]);
			num3 += num3 * 16;
			num4 = ParseHexChar(trimmedColor[3]);
			num4 += num4 * 16;
		}
		return Color.FromArgb((byte)num, (byte)num2, (byte)num3, (byte)num4);
	}

	private static Color ParseContextColor(string trimmedColor, IFormatProvider formatProvider, ITypeDescriptorContext context)
	{
		if (!trimmedColor.StartsWith("ContextColor ", StringComparison.OrdinalIgnoreCase))
		{
			throw new FormatException(SR.Parsers_IllegalToken);
		}
		string text = trimmedColor.Substring("ContextColor ".Length).Trim();
		string[] array = text.Split(' ');
		if (array.GetLength(0) < 2)
		{
			throw new FormatException(SR.Parsers_IllegalToken);
		}
		string text2 = text.Substring(array[0].Length);
		TokenizerHelper tokenizerHelper = new TokenizerHelper(text2, formatProvider);
		int length = text2.Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).GetLength(0);
		float a = Convert.ToSingle(tokenizerHelper.NextTokenRequired(), formatProvider);
		float[] array2 = new float[length - 1];
		for (int i = 0; i < length - 1; i++)
		{
			array2[i] = Convert.ToSingle(tokenizerHelper.NextTokenRequired(), formatProvider);
		}
		string inputString = array[0];
		UriHolder uriFromUriContext = System.Windows.Media.TypeConverterHelper.GetUriFromUriContext(context, inputString);
		Uri profileUri = ((!(uriFromUriContext.BaseUri != null)) ? uriFromUriContext.OriginalUri : new Uri(uriFromUriContext.BaseUri, uriFromUriContext.OriginalUri));
		Color result = Color.FromAValues(a, array2, profileUri);
		if (result.ColorContext.NumChannels != array2.Length)
		{
			throw new FormatException(SR.Parsers_IllegalToken);
		}
		return result;
	}

	private static Color ParseScRgbColor(string trimmedColor, IFormatProvider formatProvider)
	{
		if (!trimmedColor.StartsWith("sc#", StringComparison.Ordinal))
		{
			throw new FormatException(SR.Parsers_IllegalToken);
		}
		TokenizerHelper tokenizerHelper = new TokenizerHelper(trimmedColor.Substring(3, trimmedColor.Length - 3), formatProvider);
		float[] array = new float[4];
		for (int i = 0; i < 3; i++)
		{
			array[i] = Convert.ToSingle(tokenizerHelper.NextTokenRequired(), formatProvider);
		}
		if (tokenizerHelper.NextToken())
		{
			array[3] = Convert.ToSingle(tokenizerHelper.GetCurrentToken(), formatProvider);
			if (tokenizerHelper.NextToken())
			{
				throw new FormatException(SR.Parsers_IllegalToken);
			}
			return Color.FromScRgb(array[0], array[1], array[2], array[3]);
		}
		return Color.FromScRgb(1f, array[0], array[1], array[2]);
	}

	internal static Color ParseColor(string color, IFormatProvider formatProvider)
	{
		return ParseColor(color, formatProvider, null);
	}

	internal static Color ParseColor(string color, IFormatProvider formatProvider, ITypeDescriptorContext context)
	{
		bool isKnownColor;
		bool isNumericColor;
		bool isContextColor;
		bool isScRgbColor;
		string text = KnownColors.MatchColor(color, out isKnownColor, out isNumericColor, out isContextColor, out isScRgbColor);
		if (!isKnownColor && !isNumericColor && !isScRgbColor && !isContextColor)
		{
			throw new FormatException(SR.Parsers_IllegalToken);
		}
		if (isNumericColor)
		{
			return ParseHexColor(text);
		}
		if (isContextColor)
		{
			return ParseContextColor(text, formatProvider, context);
		}
		if (isScRgbColor)
		{
			return ParseScRgbColor(text, formatProvider);
		}
		KnownColor num = KnownColors.ColorStringToKnownColor(text);
		if (num == KnownColor.UnknownColor)
		{
			throw new FormatException(SR.Parsers_IllegalToken);
		}
		return Color.FromUInt32((uint)num);
	}

	internal static Brush ParseBrush(string brush, IFormatProvider formatProvider, ITypeDescriptorContext context)
	{
		bool isKnownColor;
		bool isNumericColor;
		bool isContextColor;
		bool isScRgbColor;
		string text = KnownColors.MatchColor(brush, out isKnownColor, out isNumericColor, out isContextColor, out isScRgbColor);
		if (text.Length == 0)
		{
			throw new FormatException(SR.Parser_Empty);
		}
		if (isNumericColor)
		{
			return new SolidColorBrush(ParseHexColor(text));
		}
		if (isContextColor)
		{
			return new SolidColorBrush(ParseContextColor(text, formatProvider, context));
		}
		if (isScRgbColor)
		{
			return new SolidColorBrush(ParseScRgbColor(text, formatProvider));
		}
		if (isKnownColor)
		{
			SolidColorBrush solidColorBrush = KnownColors.ColorStringToKnownBrush(text);
			if (solidColorBrush != null)
			{
				return solidColorBrush;
			}
		}
		throw new FormatException(SR.Parsers_IllegalToken);
	}

	internal static Transform ParseTransform(string transformString, IFormatProvider formatProvider)
	{
		return new MatrixTransform(Matrix.Parse(transformString));
	}

	internal static PathFigureCollection ParsePathFigureCollection(string pathString, IFormatProvider formatProvider)
	{
		PathStreamGeometryContext pathStreamGeometryContext = new PathStreamGeometryContext();
		new AbbreviatedGeometryParser().ParseToGeometryContext(pathStreamGeometryContext, pathString, 0);
		return pathStreamGeometryContext.GetPathGeometry().Figures;
	}

	internal static object DeserializeStreamGeometry(BinaryReader reader)
	{
		StreamGeometry streamGeometry = new StreamGeometry();
		using (StreamGeometryContext sc = streamGeometry.Open())
		{
			ParserStreamGeometryContext.Deserialize(reader, sc, streamGeometry);
		}
		streamGeometry.Freeze();
		return streamGeometry;
	}

	internal static void PathMinilanguageToBinary(BinaryWriter bw, string stringValue)
	{
		ParserStreamGeometryContext parserStreamGeometryContext = new ParserStreamGeometryContext(bw);
		FillRule fillRule = FillRule.EvenOdd;
		ParseStringToStreamGeometryContext(parserStreamGeometryContext, stringValue, System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS, ref fillRule);
		parserStreamGeometryContext.SetFillRule(fillRule);
		parserStreamGeometryContext.MarkEOF();
	}

	internal static Geometry ParseGeometry(string pathString, IFormatProvider formatProvider)
	{
		FillRule fillRule = FillRule.EvenOdd;
		StreamGeometry streamGeometry = new StreamGeometry();
		ParseStringToStreamGeometryContext(streamGeometry.Open(), pathString, formatProvider, ref fillRule);
		streamGeometry.FillRule = fillRule;
		streamGeometry.Freeze();
		return streamGeometry;
	}

	private static void ParseStringToStreamGeometryContext(StreamGeometryContext context, string pathString, IFormatProvider formatProvider, ref FillRule fillRule)
	{
		using (context)
		{
			if (pathString == null)
			{
				return;
			}
			int i;
			for (i = 0; i < pathString.Length && char.IsWhiteSpace(pathString, i); i++)
			{
			}
			if (i < pathString.Length && pathString[i] == 'F')
			{
				for (i++; i < pathString.Length && char.IsWhiteSpace(pathString, i); i++)
				{
				}
				if (i == pathString.Length || (pathString[i] != '0' && pathString[i] != '1'))
				{
					throw new FormatException(SR.Parsers_IllegalToken);
				}
				fillRule = ((pathString[i] != '0') ? FillRule.Nonzero : FillRule.EvenOdd);
				i++;
			}
			new AbbreviatedGeometryParser().ParseToGeometryContext(context, pathString, i);
		}
	}
}
