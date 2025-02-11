using System.Globalization;
using System.IO;

namespace System.Windows.Markup;

internal class XamlGridLengthSerializer : XamlSerializer
{
	private static string[] UnitStrings = new string[3] { "auto", "px", "*" };

	private static string[] PixelUnitStrings = new string[3] { "in", "cm", "pt" };

	private static double[] PixelUnitFactors = new double[3] { 96.0, 37.79527559055118, 1.3333333333333333 };

	private XamlGridLengthSerializer()
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
			if (num <= 127 && num >= 0 && unit == GridUnitType.Pixel)
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
		GridUnitType type;
		double value;
		if ((b & 0x80) == 0)
		{
			type = GridUnitType.Pixel;
			value = (int)b;
		}
		else
		{
			type = (GridUnitType)(b & 0x1F);
			value = (byte)(b & 0xE0) switch
			{
				128 => (int)reader.ReadByte(), 
				192 => reader.ReadInt16(), 
				160 => reader.ReadInt32(), 
				_ => reader.ReadDouble(), 
			};
		}
		return new GridLength(value, type);
	}

	internal static void FromString(string s, CultureInfo cultureInfo, out double value, out GridUnitType unit)
	{
		string text = s.Trim().ToLowerInvariant();
		value = 0.0;
		unit = GridUnitType.Pixel;
		int length = text.Length;
		int num = 0;
		double num2 = 1.0;
		int i = 0;
		if (text == UnitStrings[i])
		{
			num = UnitStrings[i].Length;
			unit = (GridUnitType)i;
		}
		else
		{
			for (i = 1; i < UnitStrings.Length; i++)
			{
				if (text.EndsWith(UnitStrings[i], StringComparison.Ordinal))
				{
					num = UnitStrings[i].Length;
					unit = (GridUnitType)i;
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
		if (length == num && (unit == GridUnitType.Auto || unit == GridUnitType.Star))
		{
			value = 1.0;
			return;
		}
		ReadOnlySpan<char> s2 = text.AsSpan(0, length - num);
		value = double.Parse(s2, cultureInfo) * num2;
	}
}
