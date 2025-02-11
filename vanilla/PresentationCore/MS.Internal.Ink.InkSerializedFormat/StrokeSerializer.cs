#define TRACE
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace MS.Internal.Ink.InkSerializedFormat;

internal static class StrokeSerializer
{
	internal static uint DecodeStroke(Stream stream, uint size, GuidList guidList, StrokeDescriptor strokeDescriptor, StylusPointDescription stylusPointDescription, DrawingAttributes drawingAttributes, Matrix transform, out Stroke stroke)
	{
		StylusPointCollection stylusPoints;
		ExtendedPropertyCollection extendedProperties;
		uint num = DecodeISFIntoStroke(stream, size, guidList, strokeDescriptor, stylusPointDescription, transform, out stylusPoints, out extendedProperties);
		if (num != size)
		{
			throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Stroke size (" + num.ToString(CultureInfo.InvariantCulture) + ") != expected (" + size.ToString(CultureInfo.InvariantCulture) + ")"));
		}
		stroke = new Stroke(stylusPoints, drawingAttributes, extendedProperties);
		return num;
	}

	private static uint DecodeISFIntoStroke(Stream stream, uint totalBytesInStrokeBlockOfIsfStream, GuidList guidList, StrokeDescriptor strokeDescriptor, StylusPointDescription stylusPointDescription, Matrix transform, out StylusPointCollection stylusPoints, out ExtendedPropertyCollection extendedProperties)
	{
		stylusPoints = null;
		extendedProperties = null;
		if (totalBytesInStrokeBlockOfIsfStream == 0)
		{
			return 0u;
		}
		uint num = totalBytesInStrokeBlockOfIsfStream;
		uint num2 = LoadPackets(stream, num, stylusPointDescription, transform, out stylusPoints);
		if (num2 > num)
		{
			throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Packet buffer overflowed the ISF stream"));
		}
		num -= num2;
		if (num == 0)
		{
			return num2;
		}
		for (int i = 1; i < strokeDescriptor.Template.Count; i++)
		{
			if (num == 0)
			{
				break;
			}
			switch (strokeDescriptor.Template[i - 1])
			{
			case KnownTagCache.KnownTagIndex.StrokePropertyList:
				for (; i < strokeDescriptor.Template.Count; i++)
				{
					if (num == 0)
					{
						break;
					}
					KnownTagCache.KnownTagIndex tag = strokeDescriptor.Template[i];
					Guid guid = guidList.FindGuid(tag);
					if (guid == Guid.Empty)
					{
						throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Stroke Custom Attribute tag embedded in ISF stream does not match guid table"));
					}
					num2 = ExtendedPropertySerializer.DecodeAsISF(stream, num, guidList, tag, ref guid, out var data);
					if (extendedProperties == null)
					{
						extendedProperties = new ExtendedPropertyCollection();
					}
					extendedProperties[guid] = data;
					if (num2 > num)
					{
						throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Invalid ISF data"));
					}
					num -= num2;
				}
				break;
			case KnownTagCache.KnownTagIndex.Buttons:
				i += (int)(strokeDescriptor.Template[i] + 1);
				break;
			default:
				Trace.WriteLine("Ignoring unhandled stroke tag in ISF stroke descriptor");
				break;
			}
		}
		while (num != 0)
		{
			num2 = SerializationHelper.Decode(stream, out var dw);
			KnownTagCache.KnownTagIndex knownTagIndex = (KnownTagCache.KnownTagIndex)dw;
			if (num2 > num)
			{
				throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Invalid ISF data"));
			}
			num -= num2;
			if (knownTagIndex == KnownTagCache.KnownTagIndex.PointProperty)
			{
				num2 = SerializationHelper.Decode(stream, out var _);
				if (num2 > num)
				{
					throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Invalid ISF data"));
				}
				num -= num2;
				while (num != 0)
				{
					num2 = SerializationHelper.Decode(stream, out dw);
					knownTagIndex = (KnownTagCache.KnownTagIndex)dw;
					if (num2 > num)
					{
						throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Invalid ISF data"));
					}
					num -= num2;
					num2 = SerializationHelper.Decode(stream, out var _);
					if (num2 > num)
					{
						throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Invalid ISF data"));
					}
					num -= num2;
					num2 = SerializationHelper.Decode(stream, out var dw4);
					if (num2 > num)
					{
						throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Invalid ISF data"));
					}
					num -= num2;
					dw4++;
					if (dw4 > num)
					{
						throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Invalid ISF data"));
					}
					byte[] array = new byte[dw4];
					uint num3 = StrokeCollectionSerializer.ReliableRead(stream, array, dw4);
					if (dw4 != num3)
					{
						throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Read different size from stream then expected"));
					}
					Compressor.DecompressPropertyData(array);
					num -= dw4;
				}
			}
			else
			{
				Guid guid2 = guidList.FindGuid(knownTagIndex);
				if (guid2 == Guid.Empty)
				{
					throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Stroke Custom Attribute tag embedded in ISF stream does not match guid table"));
				}
				num2 = ExtendedPropertySerializer.DecodeAsISF(stream, num, guidList, knownTagIndex, ref guid2, out var data2);
				if (extendedProperties == null)
				{
					extendedProperties = new ExtendedPropertyCollection();
				}
				extendedProperties[guid2] = data2;
				if (num2 > num)
				{
					throw new InvalidOperationException(StrokeCollectionSerializer.ISFDebugMessage("ExtendedProperty decoded totalBytesInStrokeBlockOfIsfStream exceeded ISF stream totalBytesInStrokeBlockOfIsfStream"));
				}
				num -= num2;
			}
		}
		if (num != 0)
		{
			throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Invalid ISF data"));
		}
		return totalBytesInStrokeBlockOfIsfStream;
	}

	private static uint LoadPackets(Stream inputStream, uint totalBytesInStrokeBlockOfIsfStream, StylusPointDescription stylusPointDescription, Matrix transform, out StylusPointCollection stylusPoints)
	{
		stylusPoints = null;
		if (totalBytesInStrokeBlockOfIsfStream == 0)
		{
			return 0u;
		}
		uint num = totalBytesInStrokeBlockOfIsfStream;
		uint num2 = SerializationHelper.Decode(inputStream, out var dw);
		if (num < num2)
		{
			throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Invalid ISF data"));
		}
		num -= num2;
		if (num == 0)
		{
			return num2;
		}
		int inputArrayLengthPerPoint = stylusPointDescription.GetInputArrayLengthPerPoint();
		int buttonCount = stylusPointDescription.ButtonCount;
		int num3 = ((buttonCount > 0) ? 1 : 0);
		int num4 = inputArrayLengthPerPoint - num3;
		int[] array = new int[dw * inputArrayLengthPerPoint];
		int[] array2 = new int[dw];
		byte[] array3 = new byte[num];
		if (StrokeCollectionSerializer.ReliableRead(inputStream, array3, num) != num)
		{
			throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Invalid ISF data"));
		}
		int originalPressureIndex = stylusPointDescription.OriginalPressureIndex;
		for (int i = 0; i < num4; i++)
		{
			if (num == 0)
			{
				break;
			}
			num2 = num;
			Compressor.DecompressPacketData(array3, ref num2, array2);
			if (num2 > num)
			{
				throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Invalid ISF data"));
			}
			int num5 = i;
			if (num5 > 1 && originalPressureIndex != -1 && originalPressureIndex != 2)
			{
				if (num5 == originalPressureIndex)
				{
					num5 = 2;
				}
				else if (num5 < originalPressureIndex)
				{
					num5++;
				}
			}
			num -= num2;
			int num6 = 0;
			int num7 = 0;
			while (num6 < dw)
			{
				array[num7 + num5] = array2[num6];
				num6++;
				num7 += inputArrayLengthPerPoint;
			}
			for (uint num8 = 0u; num8 < num; num8++)
			{
				array3[num8] = array3[num8 + (int)num2];
			}
		}
		byte[] array4 = null;
		if (num != 0 && buttonCount > 0)
		{
			int num9 = buttonCount / 8;
			int num10 = buttonCount % 8;
			num2 = (uint)((buttonCount * dw + 7) / 8);
			if (num2 > num)
			{
				throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Buffer range is smaller than expected expected size"));
			}
			num -= num2;
			int num11 = (buttonCount + 7) / 8;
			array4 = new byte[dw * num11];
			BitStreamReader bitStreamReader = new BitStreamReader(array3, (uint)buttonCount * dw);
			int num12 = 0;
			while (!bitStreamReader.EndOfStream)
			{
				for (int j = 0; j < num9; j++)
				{
					array4[num12++] = bitStreamReader.ReadByte(8);
				}
				if (num10 > 0)
				{
					array4[num12++] = bitStreamReader.ReadByte(num10);
				}
			}
			if (num12 != array4.Length)
			{
				throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Button data length not equal to expected length"));
			}
			FillButtonData((int)dw, buttonCount, num4, array, array4);
		}
		stylusPoints = new StylusPointCollection(stylusPointDescription, array, null, transform);
		if (num != 0)
		{
			inputStream.Seek(0L - (long)num, SeekOrigin.Current);
		}
		return totalBytesInStrokeBlockOfIsfStream - num;
	}

	private static void FillButtonData(int pointCount, int buttonCount, int buttonIndex, int[] packets, byte[] buttonData)
	{
		int num = buttonIndex + 1;
		int num2 = buttonIndex;
		if (buttonData == null)
		{
			return;
		}
		int num3 = (buttonCount + 7) / 8;
		int num4 = (int)(num3 / Native.SizeOfInt);
		int num5 = (int)(num3 % Native.SizeOfInt);
		int num6 = 0;
		int num7 = 0;
		int num8;
		while (num7 < num4)
		{
			num6 = (int)(num7 * Native.SizeOfInt);
			num8 = num2;
			int num9 = 0;
			while (num9 < pointCount)
			{
				packets[num8] = (buttonData[num6] << 24) | (buttonData[num6 + 1] << 16) | (buttonData[num6 + 2] << 8) | buttonData[num6 + 3];
				num9++;
				num8 += num;
				num6 += num3;
			}
			num7++;
			num2++;
		}
		if (0 >= num5)
		{
			return;
		}
		num6 = (int)(num4 * Native.SizeOfInt);
		num8 = num2;
		int num10 = 0;
		while (num10 < pointCount)
		{
			uint num11 = buttonData[num6];
			for (int i = 1; i < num5; i++)
			{
				num11 = (num11 << 8) | buttonData[num6 + i];
			}
			packets[num8] = (int)num11;
			num10++;
			num8 += num;
			num6 += num3;
		}
	}

	internal static uint EncodeStroke(Stroke stroke, Stream stream, byte compressionAlgorithm, GuidList guidList, StrokeCollectionSerializer.StrokeLookupEntry strokeLookupEntry)
	{
		uint num = SavePackets(stroke, stream, strokeLookupEntry);
		if (stroke.ExtendedProperties.Count > 0)
		{
			num += ExtendedPropertySerializer.EncodeAsISF(stroke.ExtendedProperties, stream, guidList, compressionAlgorithm, fTag: false);
		}
		return num;
	}

	internal static void BuildStrokeDescriptor(Stroke stroke, GuidList guidList, StrokeCollectionSerializer.StrokeLookupEntry strokeLookupEntry, out StrokeDescriptor strokeDescriptor, out MetricBlock metricBlock)
	{
		metricBlock = new MetricBlock();
		strokeDescriptor = new StrokeDescriptor();
		StylusPointDescription description = stroke.StylusPoints.Description;
		KnownTagCache.KnownTagIndex tag = guidList.FindTag(KnownIds.X, bFindInKnownListFirst: true);
		metricBlock.AddMetricEntry(description.GetPropertyInfo(StylusPointProperties.X), tag);
		tag = guidList.FindTag(KnownIds.Y, bFindInKnownListFirst: true);
		metricBlock.AddMetricEntry(description.GetPropertyInfo(StylusPointProperties.Y), tag);
		ReadOnlyCollection<StylusPointPropertyInfo> stylusPointProperties = description.GetStylusPointProperties();
		int num = 0;
		for (num = 2; num < stylusPointProperties.Count; num++)
		{
			if (num != 2 || strokeLookupEntry.StorePressure)
			{
				StylusPointPropertyInfo stylusPointPropertyInfo = stylusPointProperties[num];
				if (stylusPointPropertyInfo.IsButton)
				{
					break;
				}
				tag = guidList.FindTag(stylusPointPropertyInfo.Id, bFindInKnownListFirst: true);
				strokeDescriptor.Template.Add(tag);
				strokeDescriptor.Size += SerializationHelper.VarSize((uint)tag);
				metricBlock.AddMetricEntry(stylusPointPropertyInfo, tag);
			}
		}
		if (stroke.ExtendedProperties.Count > 0)
		{
			strokeDescriptor.Template.Add(KnownTagCache.KnownTagIndex.StrokePropertyList);
			strokeDescriptor.Size += SerializationHelper.VarSize(11u);
			for (int i = 0; i < stroke.ExtendedProperties.Count; i++)
			{
				tag = guidList.FindTag(stroke.ExtendedProperties[i].Id, bFindInKnownListFirst: false);
				strokeDescriptor.Template.Add(tag);
				strokeDescriptor.Size += SerializationHelper.VarSize((uint)tag);
			}
		}
	}

	private static uint SavePackets(Stroke stroke, Stream stream, StrokeCollectionSerializer.StrokeLookupEntry strokeLookupEntry)
	{
		uint count = (uint)stroke.StylusPoints.Count;
		uint num = ((stream != null) ? SerializationHelper.Encode(stream, count) : SerializationHelper.VarSize(count));
		int[][] iSFReadyStrokeData = strokeLookupEntry.ISFReadyStrokeData;
		ReadOnlyCollection<StylusPointPropertyInfo> stylusPointProperties = stroke.StylusPoints.Description.GetStylusPointProperties();
		for (int i = 0; i < stylusPointProperties.Count; i++)
		{
			StylusPointPropertyInfo stylusPointPropertyInfo = stylusPointProperties[i];
			if (i != 2 || strokeLookupEntry.StorePressure)
			{
				if (stylusPointPropertyInfo.IsButton)
				{
					break;
				}
				byte algo = strokeLookupEntry.CompressionData;
				num += SavePacketPropertyData(iSFReadyStrokeData[i], stream, stylusPointPropertyInfo.Id, ref algo);
			}
		}
		return num;
	}

	private static uint SavePacketPropertyData(int[] packetdata, Stream stream, Guid guid, ref byte algo)
	{
		if (packetdata.Length == 0)
		{
			return 0u;
		}
		byte[] array = Compressor.CompressPacketData(packetdata, ref algo);
		stream.Write(array, 0, array.Length);
		return (uint)array.Length;
	}
}
