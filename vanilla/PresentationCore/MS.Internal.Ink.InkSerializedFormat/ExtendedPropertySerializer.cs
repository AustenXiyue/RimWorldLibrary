using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Ink;
using System.Windows.Media;
using MS.Internal.PresentationCore;

namespace MS.Internal.Ink.InkSerializedFormat;

internal static class ExtendedPropertySerializer
{
	private static bool UsesEmbeddedTypeInformation(Guid propGuid)
	{
		for (int i = 0; i < KnownIdCache.OriginalISFIdTable.Length; i++)
		{
			if (propGuid.Equals(KnownIdCache.OriginalISFIdTable[i]))
			{
				return false;
			}
		}
		for (int j = 0; j < KnownIdCache.TabletInternalIdTable.Length; j++)
		{
			if (propGuid.Equals(KnownIdCache.TabletInternalIdTable[j]))
			{
				return false;
			}
		}
		return true;
	}

	internal static void EncodeToStream(ExtendedProperty attribute, Stream stream)
	{
		object value = attribute.Value;
		EncodeAttribute(type: (attribute.Id == KnownIds.DrawingFlags) ? VarEnum.VT_I4 : ((attribute.Id == KnownIds.StylusTip) ? VarEnum.VT_I4 : ((!UsesEmbeddedTypeInformation(attribute.Id)) ? ((VarEnum)8209) : SerializationHelper.ConvertToVarEnum(attribute.Value.GetType(), throwOnError: true))), guid: attribute.Id, value: value, stream: stream);
	}

	internal static void EncodeAttribute(Guid guid, object value, VarEnum type, Stream stream)
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);
		if (UsesEmbeddedTypeInformation(guid))
		{
			ushort value2 = (ushort)type;
			binaryWriter.Write(value2);
		}
		switch (type)
		{
		case (VarEnum)8208:
		{
			char[] chars = (char[])value;
			binaryWriter.Write(chars);
			break;
		}
		case (VarEnum)8209:
		{
			byte[] buffer = (byte[])value;
			binaryWriter.Write(buffer);
			break;
		}
		case (VarEnum)8194:
		{
			short[] array7 = (short[])value;
			for (int num = 0; num < array7.Length; num++)
			{
				binaryWriter.Write(array7[num]);
			}
			break;
		}
		case (VarEnum)8210:
		{
			ushort[] array = (ushort[])value;
			for (int i = 0; i < array.Length; i++)
			{
				binaryWriter.Write(array[i]);
			}
			break;
		}
		case (VarEnum)8195:
		{
			int[] array9 = (int[])value;
			for (int num3 = 0; num3 < array9.Length; num3++)
			{
				binaryWriter.Write(array9[num3]);
			}
			break;
		}
		case (VarEnum)8211:
		{
			uint[] array5 = (uint[])value;
			for (int m = 0; m < array5.Length; m++)
			{
				binaryWriter.Write(array5[m]);
			}
			break;
		}
		case (VarEnum)8212:
		{
			long[] array2 = (long[])value;
			for (int j = 0; j < array2.Length; j++)
			{
				binaryWriter.Write(array2[j]);
			}
			break;
		}
		case (VarEnum)8213:
		{
			ulong[] array11 = (ulong[])value;
			for (int num5 = 0; num5 < array11.Length; num5++)
			{
				binaryWriter.Write(array11[num5]);
			}
			break;
		}
		case (VarEnum)8196:
		{
			float[] array10 = (float[])value;
			for (int num4 = 0; num4 < array10.Length; num4++)
			{
				binaryWriter.Write(array10[num4]);
			}
			break;
		}
		case (VarEnum)8197:
		{
			double[] array8 = (double[])value;
			for (int num2 = 0; num2 < array8.Length; num2++)
			{
				binaryWriter.Write(array8[num2]);
			}
			break;
		}
		case (VarEnum)8199:
		{
			DateTime[] array6 = (DateTime[])value;
			for (int n = 0; n < array6.Length; n++)
			{
				binaryWriter.Write(array6[n].ToOADate());
			}
			break;
		}
		case (VarEnum)8203:
		{
			bool[] array4 = (bool[])value;
			for (int l = 0; l < array4.Length; l++)
			{
				if (array4[l])
				{
					binaryWriter.Write(byte.MaxValue);
					binaryWriter.Write(byte.MaxValue);
				}
				else
				{
					binaryWriter.Write((byte)0);
					binaryWriter.Write((byte)0);
				}
			}
			break;
		}
		case (VarEnum)8206:
		{
			decimal[] array3 = (decimal[])value;
			for (int k = 0; k < array3.Length; k++)
			{
				binaryWriter.Write(array3[k]);
			}
			break;
		}
		case VarEnum.VT_I1:
		{
			char ch = (char)value;
			binaryWriter.Write(ch);
			break;
		}
		case VarEnum.VT_UI1:
		{
			byte value12 = (byte)value;
			binaryWriter.Write(value12);
			break;
		}
		case VarEnum.VT_I2:
		{
			short value11 = (short)value;
			binaryWriter.Write(value11);
			break;
		}
		case VarEnum.VT_UI2:
		{
			ushort value10 = (ushort)value;
			binaryWriter.Write(value10);
			break;
		}
		case VarEnum.VT_I4:
		{
			int value9 = (int)value;
			binaryWriter.Write(value9);
			break;
		}
		case VarEnum.VT_UI4:
		{
			uint value8 = (uint)value;
			binaryWriter.Write(value8);
			break;
		}
		case VarEnum.VT_I8:
		{
			long value7 = (long)value;
			binaryWriter.Write(value7);
			break;
		}
		case VarEnum.VT_UI8:
		{
			ulong value6 = (ulong)value;
			binaryWriter.Write(value6);
			break;
		}
		case VarEnum.VT_R4:
		{
			float value5 = (float)value;
			binaryWriter.Write(value5);
			break;
		}
		case VarEnum.VT_R8:
		{
			double value4 = (double)value;
			binaryWriter.Write(value4);
			break;
		}
		case VarEnum.VT_DATE:
			binaryWriter.Write(((DateTime)value).ToOADate());
			break;
		case VarEnum.VT_BOOL:
			if ((bool)value)
			{
				binaryWriter.Write(byte.MaxValue);
				binaryWriter.Write(byte.MaxValue);
			}
			else
			{
				binaryWriter.Write((byte)0);
				binaryWriter.Write((byte)0);
			}
			break;
		case VarEnum.VT_DECIMAL:
		{
			decimal value3 = (decimal)value;
			binaryWriter.Write(value3);
			break;
		}
		case VarEnum.VT_BSTR:
		{
			string s = (string)value;
			binaryWriter.Write(Encoding.Unicode.GetBytes(s));
			break;
		}
		default:
			throw new InvalidOperationException(SR.InvalidEpInIsf);
		}
	}

	internal static uint EncodeAsISF(Guid id, byte[] data, Stream strm, GuidList guidList, byte compressionAlgorithm, bool fTag)
	{
		uint num = 0u;
		uint dataSizeIfKnownGuid = GuidList.GetDataSizeIfKnownGuid(id);
		if (fTag)
		{
			uint value = (uint)guidList.FindTag(id, bFindInKnownListFirst: true);
			num += SerializationHelper.Encode(strm, value);
		}
		if (dataSizeIfKnownGuid == 0)
		{
			dataSizeIfKnownGuid = (uint)data.Length;
			byte[] array = Compressor.CompressPropertyData(data, compressionAlgorithm);
			num += SerializationHelper.Encode(strm, (uint)(array.Length - 1));
			strm.Write(array, 0, array.Length);
			return num + (uint)array.Length;
		}
		strm.Write(data, 0, data.Length);
		return num + (uint)data.Length;
	}

	internal static uint DecodeAsISF(Stream stream, uint cbSize, GuidList guidList, KnownTagCache.KnownTagIndex tag, ref Guid guid, out object data)
	{
		uint num = 0u;
		uint num2 = cbSize;
		if (cbSize == 0)
		{
			throw new InvalidOperationException(SR.EmptyDataToLoad);
		}
		if (tag == KnownTagCache.KnownTagIndex.Unknown)
		{
			uint dw;
			uint num3 = SerializationHelper.Decode(stream, out dw);
			tag = (KnownTagCache.KnownTagIndex)dw;
			if (num3 > num2)
			{
				throw new ArgumentException(SR.InvalidSizeSpecified, "cbSize");
			}
			num2 -= num3;
			num += num3;
			guid = guidList.FindGuid(tag);
		}
		if (guid == Guid.Empty)
		{
			throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Custom Attribute tag embedded in ISF stream does not match guid table"), "tag");
		}
		uint dw2 = GuidList.GetDataSizeIfKnownGuid(guid);
		if (dw2 > num2)
		{
			throw new ArgumentException(SR.InvalidSizeSpecified, "cbSize");
		}
		if (dw2 == 0)
		{
			uint num3 = SerializationHelper.Decode(stream, out dw2);
			uint num4 = dw2 + 1;
			num += num3;
			num2 -= num3;
			if (num4 > num2)
			{
				throw new ArgumentException();
			}
			byte[] array = new byte[num4];
			uint num5 = (uint)stream.Read(array, 0, (int)num4);
			if (num4 != num5)
			{
				throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Read different size from stream then expected"), "cbSize");
			}
			num += num4;
			num2 -= num4;
			using MemoryStream stream2 = new MemoryStream(Compressor.DecompressPropertyData(array));
			data = DecodeAttribute(guid, stream2);
		}
		else
		{
			byte[] buffer = new byte[dw2];
			uint num6 = (uint)stream.Read(buffer, 0, (int)dw2);
			if (dw2 != num6)
			{
				throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Read different size from stream then expected"), "cbSize");
			}
			using (MemoryStream stream3 = new MemoryStream(buffer))
			{
				data = DecodeAttribute(guid, stream3);
			}
			num2 -= dw2;
			num += dw2;
		}
		return num;
	}

	internal static object DecodeAttribute(Guid guid, Stream stream)
	{
		VarEnum type;
		return DecodeAttribute(guid, stream, out type);
	}

	internal static object DecodeAttribute(Guid guid, Stream memStream, out VarEnum type)
	{
		using BinaryReader binaryReader = new BinaryReader(memStream);
		bool flag = UsesEmbeddedTypeInformation(guid);
		if (flag)
		{
			type = (VarEnum)binaryReader.ReadUInt16();
		}
		else
		{
			type = (VarEnum)8209;
		}
		switch (type)
		{
		case (VarEnum)8208:
			return binaryReader.ReadChars((int)(memStream.Length - 2));
		case (VarEnum)8209:
		{
			int num6 = 2;
			if (!flag)
			{
				num6 = 0;
			}
			return binaryReader.ReadBytes((int)(memStream.Length - num6));
		}
		case (VarEnum)8194:
		{
			int num14 = (int)(memStream.Length - 2) / 2;
			short[] array10 = new short[num14];
			for (int num15 = 0; num15 < num14; num15++)
			{
				array10[num15] = binaryReader.ReadInt16();
			}
			return array10;
		}
		case (VarEnum)8210:
		{
			int num3 = (int)(memStream.Length - 2) / 2;
			ushort[] array3 = new ushort[num3];
			for (int k = 0; k < num3; k++)
			{
				array3[k] = binaryReader.ReadUInt16();
			}
			return array3;
		}
		case (VarEnum)8195:
		{
			int num8 = (int)(memStream.Length - 2) / 4;
			int[] array7 = new int[num8];
			for (int num9 = 0; num9 < num8; num9++)
			{
				array7[num9] = binaryReader.ReadInt32();
			}
			return array7;
		}
		case (VarEnum)8211:
		{
			int num2 = (int)(memStream.Length - 2) / 4;
			uint[] array2 = new uint[num2];
			for (int j = 0; j < num2; j++)
			{
				array2[j] = binaryReader.ReadUInt32();
			}
			return array2;
		}
		case (VarEnum)8212:
		{
			int num10 = (int)(memStream.Length - 2) / 8;
			long[] array8 = new long[num10];
			for (int num11 = 0; num11 < num10; num11++)
			{
				array8[num11] = binaryReader.ReadInt64();
			}
			return array8;
		}
		case (VarEnum)8213:
		{
			int num5 = (int)(memStream.Length - 2) / 8;
			ulong[] array5 = new ulong[num5];
			for (int m = 0; m < num5; m++)
			{
				array5[m] = binaryReader.ReadUInt64();
			}
			return array5;
		}
		case (VarEnum)8196:
		{
			int num16 = (int)(memStream.Length - 2) / 4;
			float[] array11 = new float[num16];
			for (int num17 = 0; num17 < num16; num17++)
			{
				array11[num17] = binaryReader.ReadSingle();
			}
			return array11;
		}
		case (VarEnum)8197:
		{
			int num12 = (int)(memStream.Length - 2) / 8;
			double[] array9 = new double[num12];
			for (int num13 = 0; num13 < num12; num13++)
			{
				array9[num13] = binaryReader.ReadDouble();
			}
			return array9;
		}
		case (VarEnum)8199:
		{
			int num7 = (int)(memStream.Length - 2) / 8;
			DateTime[] array6 = new DateTime[num7];
			for (int n = 0; n < num7; n++)
			{
				array6[n] = DateTime.FromOADate(binaryReader.ReadDouble());
			}
			return array6;
		}
		case (VarEnum)8203:
		{
			int num4 = (int)(memStream.Length - 2);
			bool[] array4 = new bool[num4];
			for (int l = 0; l < num4; l++)
			{
				array4[l] = binaryReader.ReadBoolean();
			}
			return array4;
		}
		case (VarEnum)8206:
		{
			int num = (int)((memStream.Length - 2) / Native.SizeOfDecimal);
			decimal[] array = new decimal[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = binaryReader.ReadDecimal();
			}
			return array;
		}
		case VarEnum.VT_I1:
			return binaryReader.ReadChar();
		case VarEnum.VT_UI1:
			return binaryReader.ReadByte();
		case VarEnum.VT_I2:
			return binaryReader.ReadInt16();
		case VarEnum.VT_UI2:
			return binaryReader.ReadUInt16();
		case VarEnum.VT_I4:
			return binaryReader.ReadInt32();
		case VarEnum.VT_UI4:
			return binaryReader.ReadUInt32();
		case VarEnum.VT_I8:
			return binaryReader.ReadInt64();
		case VarEnum.VT_UI8:
			return binaryReader.ReadUInt64();
		case VarEnum.VT_R4:
			return binaryReader.ReadSingle();
		case VarEnum.VT_R8:
			return binaryReader.ReadDouble();
		case VarEnum.VT_DATE:
			return DateTime.FromOADate(binaryReader.ReadDouble());
		case VarEnum.VT_BOOL:
			return binaryReader.ReadBoolean();
		case VarEnum.VT_DECIMAL:
			return binaryReader.ReadDecimal();
		case VarEnum.VT_BSTR:
		{
			byte[] bytes = binaryReader.ReadBytes((int)memStream.Length);
			return Encoding.Unicode.GetString(bytes);
		}
		default:
			throw new InvalidOperationException(SR.InvalidEpInIsf);
		}
	}

	internal static uint EncodeAsISF(ExtendedPropertyCollection attributes, Stream stream, GuidList guidList, byte compressionAlgorithm, bool fTag)
	{
		uint num = 0u;
		for (int i = 0; i < attributes.Count; i++)
		{
			ExtendedProperty extendedProperty = attributes[i];
			using MemoryStream memoryStream = new MemoryStream(10);
			EncodeToStream(extendedProperty, memoryStream);
			byte[] data = memoryStream.ToArray();
			num += EncodeAsISF(extendedProperty.Id, data, stream, guidList, compressionAlgorithm, fTag);
		}
		return num;
	}

	internal static Guid[] GetUnknownGuids(ExtendedPropertyCollection attributes, out int count)
	{
		Guid[] array = new Guid[attributes.Count];
		count = 0;
		for (int i = 0; i < attributes.Count; i++)
		{
			ExtendedProperty extendedProperty = attributes[i];
			if (GuidList.FindKnownTag(extendedProperty.Id) == KnownTagCache.KnownTagIndex.Unknown)
			{
				array[count++] = extendedProperty.Id;
			}
		}
		return array;
	}

	internal static void Validate(Guid id, object value)
	{
		if (id == Guid.Empty)
		{
			throw new ArgumentException(SR.InvalidGuid);
		}
		if (id == KnownIds.Color)
		{
			if (!(value is Color))
			{
				throw new ArgumentException(SR.Format(SR.InvalidValueType, typeof(Color)), "value");
			}
			return;
		}
		if (id == KnownIds.CurveFittingError)
		{
			if (!(value.GetType() == typeof(int)))
			{
				throw new ArgumentException(SR.Format(SR.InvalidValueType, typeof(int)), "value");
			}
			return;
		}
		if (id == KnownIds.DrawingFlags)
		{
			if (value.GetType() != typeof(DrawingFlags))
			{
				throw new ArgumentException(SR.Format(SR.InvalidValueType, typeof(DrawingFlags)), "value");
			}
			return;
		}
		if (id == KnownIds.StylusTip)
		{
			Type type = value.GetType();
			bool flag = type == typeof(StylusTip);
			bool flag2 = type == typeof(int);
			if (!flag && !flag2)
			{
				throw new ArgumentException(SR.Format(SR.InvalidValueType1, typeof(StylusTip), typeof(int)), "value");
			}
			if (!StylusTipHelper.IsDefined((StylusTip)value))
			{
				throw new ArgumentException(SR.Format(SR.InvalidValueOfType, value, typeof(StylusTip)), "value");
			}
			return;
		}
		if (id == KnownIds.StylusTipTransform)
		{
			Type type2 = value.GetType();
			if (type2 != typeof(string) && type2 != typeof(Matrix))
			{
				throw new ArgumentException(SR.Format(SR.InvalidValueType1, typeof(string), typeof(Matrix)), "value");
			}
			if (type2 == typeof(Matrix))
			{
				Matrix matrix = (Matrix)value;
				if (!matrix.HasInverse)
				{
					throw new ArgumentException(SR.MatrixNotInvertible, "value");
				}
				if (MatrixHelper.ContainsNaN(matrix))
				{
					throw new ArgumentException(SR.InvalidMatrixContainsNaN, "value");
				}
				if (MatrixHelper.ContainsInfinity(matrix))
				{
					throw new ArgumentException(SR.InvalidMatrixContainsInfinity, "value");
				}
			}
			return;
		}
		if (id == KnownIds.IsHighlighter)
		{
			if (value.GetType() != typeof(bool))
			{
				throw new ArgumentException(SR.Format(SR.InvalidValueType, typeof(bool)), "value");
			}
			return;
		}
		if (id == KnownIds.StylusHeight || id == KnownIds.StylusWidth)
		{
			if (value.GetType() != typeof(double))
			{
				throw new ArgumentException(SR.Format(SR.InvalidValueType, typeof(double)), "value");
			}
			double num = (double)value;
			if (id == KnownIds.StylusHeight)
			{
				if (double.IsNaN(num) || num < DrawingAttributes.MinHeight || num > DrawingAttributes.MaxHeight)
				{
					throw new ArgumentOutOfRangeException("value", SR.InvalidDrawingAttributesHeight);
				}
			}
			else if (double.IsNaN(num) || num < DrawingAttributes.MinWidth || num > DrawingAttributes.MaxWidth)
			{
				throw new ArgumentOutOfRangeException("value", SR.InvalidDrawingAttributesWidth);
			}
			return;
		}
		if (id == KnownIds.Transparency)
		{
			if (value.GetType() != typeof(byte))
			{
				throw new ArgumentException(SR.Format(SR.InvalidValueType, typeof(byte)), "value");
			}
			_ = (double)value;
			return;
		}
		if (!UsesEmbeddedTypeInformation(id))
		{
			if (value.GetType() != typeof(byte[]))
			{
				throw new ArgumentException(SR.Format(SR.InvalidValueType, typeof(byte[])), "value");
			}
			return;
		}
		VarEnum varEnum = SerializationHelper.ConvertToVarEnum(value.GetType(), throwOnError: true);
		switch (varEnum)
		{
		case VarEnum.VT_DATE:
		case VarEnum.VT_I1:
		case (VarEnum)8199:
		case (VarEnum)8208:
		{
			using MemoryStream output = new MemoryStream(32);
			using BinaryWriter binaryWriter = new BinaryWriter(output);
			try
			{
				switch (varEnum)
				{
				case (VarEnum)8208:
					binaryWriter.Write((char[])value);
					break;
				case VarEnum.VT_I1:
					binaryWriter.Write((char)value);
					break;
				case (VarEnum)8199:
				{
					DateTime[] array = (DateTime[])value;
					for (int i = 0; i < array.Length; i++)
					{
						binaryWriter.Write(array[i].ToOADate());
					}
					break;
				}
				case VarEnum.VT_DATE:
					binaryWriter.Write(((DateTime)value).ToOADate());
					break;
				}
				break;
			}
			catch (ArgumentException innerException)
			{
				throw new ArgumentException(SR.InvalidDataInISF, innerException);
			}
			catch (OverflowException innerException2)
			{
				throw new ArgumentException(SR.InvalidDataInISF, innerException2);
			}
		}
		}
	}
}
