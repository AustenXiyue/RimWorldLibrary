using System.Globalization;
using System.IO;

namespace System.Windows.Markup;

internal class XamlFigureLengthSerializer : XamlSerializer
{
	private struct FigureUnitTypeStringConvert
	{
		internal string Name;

		internal FigureUnitType UnitType;

		internal FigureUnitTypeStringConvert(string name, FigureUnitType unitType)
		{
			Name = name;
			UnitType = unitType;
		}
	}

	private static FigureUnitTypeStringConvert[] UnitStrings = new FigureUnitTypeStringConvert[6]
	{
		new FigureUnitTypeStringConvert("auto", FigureUnitType.Auto),
		new FigureUnitTypeStringConvert("px", FigureUnitType.Pixel),
		new FigureUnitTypeStringConvert("column", FigureUnitType.Column),
		new FigureUnitTypeStringConvert("columns", FigureUnitType.Column),
		new FigureUnitTypeStringConvert("content", FigureUnitType.Content),
		new FigureUnitTypeStringConvert("page", FigureUnitType.Page)
	};

	private static string[] PixelUnitStrings = new string[3] { "in", "cm", "pt" };

	private static double[] PixelUnitFactors = new double[3] { 96.0, 37.79527559055118, 1.3333333333333333 };

	private XamlFigureLengthSerializer()
	{
	}

	public override bool ConvertStringToCustomBinary(BinaryWriter writer, string stringValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		FromString(stringValue, TypeConverterHelper.InvariantEnglishUS, out var value, out var unit);
		byte b = (byte)unit;
		int num = (int)value;
		if ((double)num == value)
		{
			if (num <= 127 && num >= 0 && unit == FigureUnitType.Pixel)
			{
				writer.Write((byte)num);
			}
			else if (num <= 255 && num >= 0)
			{
				writer.Write((byte)(0x80 | b));
				writer.Write((byte)num);
			}
			else if (num <= 32767 && num >= -32768)
			{
				writer.Write((byte)(0xC0 | b));
				writer.Write((short)num);
			}
			else
			{
				writer.Write((byte)(0xA0 | b));
				writer.Write(num);
			}
		}
		else
		{
			writer.Write((byte)(0xE0 | b));
			writer.Write(value);
		}
		return true;
	}

	public override object ConvertCustomBinaryToObject(BinaryReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		byte b = reader.ReadByte();
		FigureUnitType type;
		double value;
		if ((b & 0x80) == 0)
		{
			type = FigureUnitType.Pixel;
			value = (int)b;
		}
		else
		{
			type = (FigureUnitType)(b & 0x1F);
			value = (byte)(b & 0xE0) switch
			{
				128 => (int)reader.ReadByte(), 
				192 => reader.ReadInt16(), 
				160 => reader.ReadInt32(), 
				_ => reader.ReadDouble(), 
			};
		}
		return new FigureLength(value, type);
	}

	internal static void FromString(string s, CultureInfo cultureInfo, out double value, out FigureUnitType unit)
	{
		string text = s.Trim().ToLowerInvariant();
		value = 0.0;
		unit = FigureUnitType.Pixel;
		int length = text.Length;
		int num = 0;
		double num2 = 1.0;
		int i = 0;
		if (text == UnitStrings[i].Name)
		{
			num = UnitStrings[i].Name.Length;
			unit = UnitStrings[i].UnitType;
		}
		else
		{
			for (i = 1; i < UnitStrings.Length; i++)
			{
				if (text.EndsWith(UnitStrings[i].Name, StringComparison.Ordinal))
				{
					num = UnitStrings[i].Name.Length;
					unit = UnitStrings[i].UnitType;
					break;
				}
			}
		}
		if (i >= UnitStrings.Length)
		{
			for (i = 0; i < PixelUnitStrings.Length; i++)
			{
				if (text.EndsWith(PixelUnitStrings[i], StringComparison.Ordinal))
				{
					num = PixelUnitStrings[i].Length;
					num2 = PixelUnitFactors[i];
					break;
				}
			}
		}
		if (length == num && unit != FigureUnitType.Pixel)
		{
			value = 1.0;
			return;
		}
		ReadOnlySpan<char> s2 = text.AsSpan(0, length - num);
		value = double.Parse(s2, cultureInfo) * num2;
	}
}
