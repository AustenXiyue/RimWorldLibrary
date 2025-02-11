using System.IO;
using System.Windows.Media;

namespace System.Windows.Markup;

internal class XamlInt32CollectionSerializer : XamlSerializer
{
	internal enum IntegerCollectionType : byte
	{
		Unknown,
		Consecutive,
		Byte,
		UShort,
		Integer
	}

	public override bool ConvertStringToCustomBinary(BinaryWriter writer, string stringValue)
	{
		Int32Collection int32Collection = Int32Collection.Parse(stringValue);
		int num = 0;
		int count = int32Collection.Count;
		bool flag = true;
		bool flag2 = true;
		for (int i = 1; i < count; i++)
		{
			int num2 = int32Collection.Internal_GetItem(i - 1);
			int num3 = int32Collection.Internal_GetItem(i);
			if (flag && num2 + 1 != num3)
			{
				flag = false;
			}
			if (num3 < 0)
			{
				flag2 = false;
			}
			if (num3 > num)
			{
				num = num3;
			}
		}
		if (flag)
		{
			writer.Write((byte)1);
			writer.Write(count);
			writer.Write(int32Collection.Internal_GetItem(0));
		}
		else
		{
			IntegerCollectionType integerCollectionType = ((flag2 && num <= 255) ? IntegerCollectionType.Byte : ((!flag2 || num > 65535) ? IntegerCollectionType.Integer : IntegerCollectionType.UShort));
			writer.Write((byte)integerCollectionType);
			writer.Write(count);
			switch (integerCollectionType)
			{
			case IntegerCollectionType.Byte:
			{
				for (int k = 0; k < count; k++)
				{
					writer.Write((byte)int32Collection.Internal_GetItem(k));
				}
				break;
			}
			case IntegerCollectionType.UShort:
			{
				for (int l = 0; l < count; l++)
				{
					writer.Write((ushort)int32Collection.Internal_GetItem(l));
				}
				break;
			}
			case IntegerCollectionType.Integer:
			{
				for (int j = 0; j < count; j++)
				{
					writer.Write(int32Collection.Internal_GetItem(j));
				}
				break;
			}
			}
		}
		return true;
	}

	public override object ConvertCustomBinaryToObject(BinaryReader reader)
	{
		return DeserializeFrom(reader);
	}

	public static object StaticConvertCustomBinaryToObject(BinaryReader reader)
	{
		return DeserializeFrom(reader);
	}

	private static Int32Collection DeserializeFrom(BinaryReader reader)
	{
		IntegerCollectionType integerCollectionType = (IntegerCollectionType)reader.ReadByte();
		int num = reader.ReadInt32();
		if (num < 0)
		{
			throw new ArgumentException(SR.IntegerCollectionLengthLessThanZero);
		}
		Int32Collection int32Collection = new Int32Collection(num);
		switch (integerCollectionType)
		{
		case IntegerCollectionType.Consecutive:
		{
			int num2 = reader.ReadInt32();
			for (int l = 0; l < num; l++)
			{
				int32Collection.Add(num2 + l);
			}
			break;
		}
		case IntegerCollectionType.Byte:
		{
			for (int j = 0; j < num; j++)
			{
				int32Collection.Add(reader.ReadByte());
			}
			break;
		}
		case IntegerCollectionType.UShort:
		{
			for (int k = 0; k < num; k++)
			{
				int32Collection.Add(reader.ReadUInt16());
			}
			break;
		}
		case IntegerCollectionType.Integer:
		{
			for (int i = 0; i < num; i++)
			{
				int value = reader.ReadInt32();
				int32Collection.Add(value);
			}
			break;
		}
		default:
			throw new ArgumentException(SR.UnknownIndexType);
		}
		return int32Collection;
	}
}
