using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Ink;
using System.Windows.Media;

namespace MS.Internal.Ink.InkSerializedFormat;

internal static class DrawingAttributeSerializer
{
	private enum PenTip
	{
		Circle = 0,
		Rectangle = 1,
		Default = 0
	}

	private static class PenTipHelper
	{
		internal static bool IsDefined(PenTip penTip)
		{
			if (penTip < PenTip.Circle || penTip > PenTip.Rectangle)
			{
				return false;
			}
			return true;
		}
	}

	private enum PenStyle
	{
		Cosmetic = 0,
		Geometric = 65536,
		Default = 65536
	}

	internal static class PersistenceTypes
	{
		public static readonly Type StylusTip = typeof(int);

		public static readonly Type IsHollow = typeof(bool);

		public static readonly Type StylusTipTransform = typeof(string);
	}

	private const double V1PenWidthWhenWidthIsMissing = 25.0;

	private const double V1PenHeightWhenHeightIsMissing = 25.0;

	private const int TransparencyDefaultV1 = 0;

	internal const uint RasterOperationMaskPen = 9u;

	internal const uint RasterOperationDefaultV1 = 13u;

	internal static uint DecodeAsISF(Stream stream, GuidList guidList, uint maximumStreamSize, DrawingAttributes da)
	{
		PenTip penTip = PenTip.Circle;
		double num = 25.0;
		double num2 = 25.0;
		uint num3 = 13u;
		int num4 = 0;
		bool flag = false;
		bool flag2 = false;
		uint result = maximumStreamSize;
		while (maximumStreamSize != 0)
		{
			uint num5 = SerializationHelper.Decode(stream, out var dw);
			KnownTagCache.KnownTagIndex tag = (KnownTagCache.KnownTagIndex)dw;
			if (maximumStreamSize < num5)
			{
				throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("ISF size is larger than maximum stream size"));
			}
			maximumStreamSize -= num5;
			Guid guid = guidList.FindGuid(tag);
			if (guid == Guid.Empty)
			{
				throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Drawing Attribute tag embedded in ISF stream does not match guid table"));
			}
			uint dw2 = 0u;
			if (KnownIds.PenTip == guid)
			{
				num5 = SerializationHelper.Decode(stream, out dw2);
				penTip = (PenTip)dw2;
				if (!PenTipHelper.IsDefined(penTip))
				{
					throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Invalid PenTip value found in ISF stream"));
				}
				maximumStreamSize -= num5;
			}
			else if (KnownIds.PenStyle == guid)
			{
				num5 = SerializationHelper.Decode(stream, out dw2);
				maximumStreamSize -= num5;
			}
			else if (KnownIds.DrawingFlags == guid)
			{
				num5 = SerializationHelper.Decode(stream, out dw2);
				DrawingFlags drawingFlags = (DrawingFlags)dw2;
				da.DrawingFlags = drawingFlags;
				maximumStreamSize -= num5;
			}
			else if (KnownIds.RasterOperation == guid)
			{
				uint dataSizeIfKnownGuid = GuidList.GetDataSizeIfKnownGuid(KnownIds.RasterOperation);
				if (dataSizeIfKnownGuid == 0)
				{
					throw new InvalidOperationException(StrokeCollectionSerializer.ISFDebugMessage("ROP data size was not found"));
				}
				byte[] array = new byte[dataSizeIfKnownGuid];
				stream.Read(array, 0, (int)dataSizeIfKnownGuid);
				if (array != null && array.Length != 0)
				{
					num3 = Convert.ToUInt32(array[0]);
				}
				maximumStreamSize -= dataSizeIfKnownGuid;
			}
			else if (KnownIds.CurveFittingError == guid)
			{
				num5 = SerializationHelper.Decode(stream, out dw2);
				da.FittingError = (int)dw2;
				maximumStreamSize -= num5;
			}
			else if (KnownIds.StylusHeight == guid || KnownIds.StylusWidth == guid)
			{
				num5 = SerializationHelper.Decode(stream, out dw2);
				double num6 = dw2;
				maximumStreamSize -= num5;
				if (maximumStreamSize != 0)
				{
					num5 = SerializationHelper.Decode(stream, out dw2);
					maximumStreamSize -= num5;
					if (27 == dw2)
					{
						num5 = SerializationHelper.Decode(stream, out var dw3);
						maximumStreamSize -= num5;
						dw3++;
						if (dw3 > maximumStreamSize)
						{
							throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("ISF size if greater then maximum stream size"));
						}
						byte[] array2 = new byte[dw3];
						uint num7 = (uint)stream.Read(array2, 0, (int)dw3);
						if (dw3 != num7)
						{
							throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Read different size from stream then expected"));
						}
						using MemoryStream input = new MemoryStream(Compressor.DecompressPropertyData(array2));
						using BinaryReader binaryReader = new BinaryReader(input);
						short num8 = binaryReader.ReadInt16();
						num6 += (double)((float)num8 / 1000f);
						maximumStreamSize -= dw3;
					}
					else
					{
						stream.Seek(0L - (long)num5, SeekOrigin.Current);
						maximumStreamSize += num5;
					}
				}
				if (KnownIds.StylusWidth == guid)
				{
					flag = true;
					num = num6;
				}
				else
				{
					flag2 = true;
					num2 = num6;
				}
			}
			else if (KnownIds.Transparency == guid)
			{
				num5 = SerializationHelper.Decode(stream, out dw2);
				num4 = (int)dw2;
				maximumStreamSize -= num5;
			}
			else if (KnownIds.Color == guid)
			{
				num5 = SerializationHelper.Decode(stream, out dw2);
				Color color = Color.FromRgb((byte)(dw2 & 0xFF), (byte)((dw2 & 0xFF00) >> 8), (byte)((dw2 & 0xFF0000) >> 16));
				da.Color = color;
				maximumStreamSize -= num5;
			}
			else if (KnownIds.StylusTipTransform == guid)
			{
				try
				{
					num5 = ExtendedPropertySerializer.DecodeAsISF(stream, maximumStreamSize, guidList, tag, ref guid, out var data);
					Matrix stylusTipTransform = Matrix.Parse((string)data);
					da.StylusTipTransform = stylusTipTransform;
				}
				catch (InvalidOperationException)
				{
				}
				finally
				{
					maximumStreamSize -= num5;
				}
			}
			else
			{
				num5 = ExtendedPropertySerializer.DecodeAsISF(stream, maximumStreamSize, guidList, tag, ref guid, out var data2);
				maximumStreamSize -= num5;
				da.AddPropertyData(guid, data2);
			}
		}
		if (maximumStreamSize != 0)
		{
			throw new ArgumentException();
		}
		if (penTip == PenTip.Circle)
		{
			if (da.StylusTip != StylusTip.Ellipse)
			{
				da.StylusTip = StylusTip.Ellipse;
			}
		}
		else if (da.StylusTip == StylusTip.Ellipse)
		{
			da.StylusTip = StylusTip.Rectangle;
		}
		if (da.StylusTip == StylusTip.Ellipse && flag && !flag2)
		{
			num2 = num;
			da.HeightChangedForCompatabity = true;
		}
		num2 *= 24.0 / 635.0;
		num *= 24.0 / 635.0;
		double heightOrWidth = (DoubleUtil.IsZero(num2) ? ((double)DrawingAttributes.GetDefaultDrawingAttributeValue(KnownIds.StylusHeight)) : num2);
		double heightOrWidth2 = (DoubleUtil.IsZero(num) ? ((double)DrawingAttributes.GetDefaultDrawingAttributeValue(KnownIds.StylusWidth)) : num);
		da.Height = GetCappedHeightOrWidth(heightOrWidth);
		da.Width = GetCappedHeightOrWidth(heightOrWidth2);
		da.RasterOperation = num3;
		switch (num3)
		{
		case 13u:
			if (da.ContainsPropertyData(KnownIds.IsHighlighter))
			{
				da.RemovePropertyData(KnownIds.IsHighlighter);
			}
			break;
		case 9u:
			da.IsHighlighter = true;
			break;
		}
		if (num4 > 0)
		{
			int value = MathHelper.AbsNoThrow(num4 - 255);
			Color color2 = da.Color;
			color2.A = Convert.ToByte(value);
			da.Color = color2;
		}
		return result;
	}

	internal static double GetCappedHeightOrWidth(double heightOrWidth)
	{
		if (heightOrWidth > DrawingAttributes.MaxHeight)
		{
			return DrawingAttributes.MaxHeight;
		}
		if (heightOrWidth < DrawingAttributes.MinHeight)
		{
			return DrawingAttributes.MinHeight;
		}
		return heightOrWidth;
	}

	internal static uint EncodeAsISF(DrawingAttributes da, Stream stream, GuidList guidList, byte compressionAlgorithm, bool fTag)
	{
		uint cbData = 0u;
		BinaryWriter bw = new BinaryWriter(stream);
		PersistDrawingFlags(da, stream, guidList, ref cbData, ref bw);
		PersistColorAndTransparency(da, stream, guidList, ref cbData, ref bw);
		PersistRasterOperation(da, stream, guidList, ref cbData, ref bw);
		PersistWidthHeight(da, stream, guidList, ref cbData, ref bw);
		PersistStylusTip(da, stream, guidList, ref cbData, ref bw);
		PersistExtendedProperties(da, stream, guidList, ref cbData, ref bw, compressionAlgorithm, fTag);
		return cbData;
	}

	private static void PersistDrawingFlags(DrawingAttributes da, Stream stream, GuidList guidList, ref uint cbData, ref BinaryWriter bw)
	{
		cbData += SerializationHelper.Encode(stream, (uint)guidList.FindTag(KnownIds.DrawingFlags, bFindInKnownListFirst: true));
		cbData += SerializationHelper.Encode(stream, (uint)da.DrawingFlags);
		if (da.ContainsPropertyData(KnownIds.CurveFittingError))
		{
			cbData += SerializationHelper.Encode(stream, (uint)guidList.FindTag(KnownIds.CurveFittingError, bFindInKnownListFirst: true));
			cbData += SerializationHelper.Encode(stream, (uint)(int)da.GetPropertyData(KnownIds.CurveFittingError));
		}
	}

	private static void PersistColorAndTransparency(DrawingAttributes da, Stream stream, GuidList guidList, ref uint cbData, ref BinaryWriter bw)
	{
		if (da.ContainsPropertyData(KnownIds.Color))
		{
			Color color = da.Color;
			byte r = color.R;
			uint g = color.G;
			uint b = color.B;
			uint value = r + (g << 8) + (b << 16);
			cbData += SerializationHelper.Encode(stream, (uint)guidList.FindTag(KnownIds.Color, bFindInKnownListFirst: true));
			cbData += SerializationHelper.Encode(stream, value);
		}
		byte a = da.Color.A;
		if (a != byte.MaxValue)
		{
			int value2 = MathHelper.AbsNoThrow(a - 255);
			cbData += SerializationHelper.Encode(stream, (uint)guidList.FindTag(KnownIds.Transparency, bFindInKnownListFirst: true));
			cbData += SerializationHelper.Encode(stream, Convert.ToUInt32(value2));
		}
	}

	private static void PersistRasterOperation(DrawingAttributes da, Stream stream, GuidList guidList, ref uint cbData, ref BinaryWriter bw)
	{
		if (da.RasterOperation != 13)
		{
			uint dataSizeIfKnownGuid = GuidList.GetDataSizeIfKnownGuid(KnownIds.RasterOperation);
			if (dataSizeIfKnownGuid == 0)
			{
				throw new InvalidOperationException(StrokeCollectionSerializer.ISFDebugMessage("ROP data size was not found"));
			}
			cbData += SerializationHelper.Encode(stream, (uint)guidList.FindTag(KnownIds.RasterOperation, bFindInKnownListFirst: true));
			long position = stream.Position;
			bw.Write(da.RasterOperation);
			if ((uint)(stream.Position - position) != dataSizeIfKnownGuid)
			{
				throw new InvalidOperationException(StrokeCollectionSerializer.ISFDebugMessage("ROP data was incorrectly serialized"));
			}
			cbData += dataSizeIfKnownGuid;
		}
	}

	private static void PersistExtendedProperties(DrawingAttributes da, Stream stream, GuidList guidList, ref uint cbData, ref BinaryWriter bw, byte compressionAlgorithm, bool fTag)
	{
		ExtendedPropertyCollection extendedPropertyCollection = da.CopyPropertyData();
		for (int num = extendedPropertyCollection.Count - 1; num >= 0; num--)
		{
			if (extendedPropertyCollection[num].Id == KnownIds.StylusTipTransform)
			{
				string value = ((Matrix)extendedPropertyCollection[num].Value).ToString(CultureInfo.InvariantCulture);
				extendedPropertyCollection[num].Value = value;
			}
			else if (DrawingAttributes.RemoveIdFromExtendedProperties(extendedPropertyCollection[num].Id))
			{
				extendedPropertyCollection.Remove(extendedPropertyCollection[num].Id);
			}
		}
		cbData += ExtendedPropertySerializer.EncodeAsISF(extendedPropertyCollection, stream, guidList, compressionAlgorithm, fTag);
	}

	private static void PersistStylusTip(DrawingAttributes da, Stream stream, GuidList guidList, ref uint cbData, ref BinaryWriter bw)
	{
		if (da.ContainsPropertyData(KnownIds.StylusTip))
		{
			cbData += SerializationHelper.Encode(stream, (uint)guidList.FindTag(KnownIds.PenTip, bFindInKnownListFirst: true));
			cbData += SerializationHelper.Encode(stream, 1u);
			using MemoryStream memoryStream = new MemoryStream(6);
			int num = Convert.ToInt32(da.StylusTip, CultureInfo.InvariantCulture);
			VarEnum type = SerializationHelper.ConvertToVarEnum(PersistenceTypes.StylusTip, throwOnError: true);
			ExtendedPropertySerializer.EncodeAttribute(KnownIds.StylusTip, num, type, memoryStream);
			cbData += ExtendedPropertySerializer.EncodeAsISF(KnownIds.StylusTip, memoryStream.ToArray(), stream, guidList, 0, fTag: true);
		}
	}

	private static void PersistWidthHeight(DrawingAttributes da, Stream stream, GuidList guidList, ref uint cbData, ref BinaryWriter bw)
	{
		double width = da.Width;
		double height = da.Height;
		for (int i = 0; i < 2; i++)
		{
			Guid guid = ((i == 0) ? KnownIds.StylusWidth : KnownIds.StylusHeight);
			double num = ((i == 0) ? width : height);
			num *= 26.458333333333332;
			double value = ((i == 0) ? 25.0 : 25.0);
			bool flag = DoubleUtil.AreClose(num, value);
			if (width == height && da.StylusTip == StylusTip.Ellipse && guid == KnownIds.StylusHeight && da.HeightChangedForCompatabity)
			{
				flag = true;
			}
			if (!flag)
			{
				uint num2 = (uint)(num + 0.5);
				cbData += SerializationHelper.Encode(stream, (uint)guidList.FindTag(guid, bFindInKnownListFirst: true));
				cbData += SerializationHelper.Encode(stream, num2);
				short num3 = ((num > (double)num2) ? ((short)(1000.0 * (num - (double)num2) + 0.5)) : ((short)(1000.0 * (num - (double)num2) - 0.5)));
				if (num3 != 0)
				{
					uint sizeOfUShort = Native.SizeOfUShort;
					cbData += SerializationHelper.Encode(stream, 27u);
					cbData += SerializationHelper.Encode(stream, sizeOfUShort);
					bw.Write((byte)0);
					bw.Write(num3);
					cbData += sizeOfUShort + 1;
				}
			}
		}
	}
}
