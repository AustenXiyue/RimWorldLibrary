using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using MS.Internal.PresentationCore;

namespace MS.Internal.Ink.InkSerializedFormat;

internal class StrokeCollectionSerializer
{
	internal class StrokeLookupEntry
	{
		internal uint MetricDescriptorTableIndex;

		internal uint StrokeDescriptorTableIndex;

		internal uint TransformTableIndex;

		internal uint DrawingAttributesTableIndex;

		internal byte CompressionData;

		internal int[][] ISFReadyStrokeData;

		internal bool StorePressure;
	}

	internal const double AvalonToHimetricMultiplier = 26.458333333333332;

	internal const double HimetricToAvalonMultiplier = 24.0 / 635.0;

	internal static readonly TransformDescriptor IdentityTransformDescriptor;

	internal PersistenceFormat CurrentPersistenceFormat;

	internal CompressionMode CurrentCompressionMode;

	internal List<int> StrokeIds;

	private static readonly byte[] Base64HeaderBytes;

	private StrokeCollection _coreStrokes;

	private List<StrokeDescriptor> _strokeDescriptorTable;

	private List<TransformDescriptor> _transformTable;

	private List<DrawingAttributes> _drawingAttributesTable;

	private List<MetricBlock> _metricTable;

	private Vector _himetricSize = new Vector(0.0, 0.0);

	private Rect _inkSpaceRectangle;

	private Dictionary<Stroke, StrokeLookupEntry> _strokeLookupTable;

	static StrokeCollectionSerializer()
	{
		Base64HeaderBytes = new byte[7] { 98, 97, 115, 101, 54, 52, 58 };
		TransformDescriptor transformDescriptor = new TransformDescriptor();
		transformDescriptor.Transform[0] = 1.0;
		transformDescriptor.Tag = KnownTagCache.KnownTagIndex.TransformIsotropicScale;
		transformDescriptor.Size = 1u;
		IdentityTransformDescriptor = transformDescriptor;
	}

	private StrokeCollectionSerializer()
	{
	}

	internal StrokeCollectionSerializer(StrokeCollection coreStrokes)
	{
		_coreStrokes = coreStrokes;
	}

	internal void DecodeISF(Stream inkData)
	{
		try
		{
			ExamineStreamHeader(inkData, out var fBase, out var fGif, out var _);
			if (fBase)
			{
				int num = Base64HeaderBytes.Length;
				inkData.Position = num;
				List<char> list = new List<char>((int)inkData.Length);
				for (int num2 = inkData.ReadByte(); num2 != -1; num2 = inkData.ReadByte())
				{
					byte item = (byte)num2;
					list.Add((char)item);
				}
				if ((byte)list[list.Count - 1] == 0)
				{
					list.RemoveAt(list.Count - 1);
				}
				char[] array = list.ToArray();
				MemoryStream memoryStream = new MemoryStream(Convert.FromBase64CharArray(array, 0, array.Length));
				if (IsGIFData(memoryStream))
				{
					DecodeRawISF(SystemDrawingHelper.GetCommentFromGifStream(memoryStream));
				}
				else
				{
					DecodeRawISF(memoryStream);
				}
			}
			else if (fGif)
			{
				DecodeRawISF(SystemDrawingHelper.GetCommentFromGifStream(inkData));
			}
			else
			{
				DecodeRawISF(inkData);
			}
		}
		catch (ArgumentException)
		{
			throw new ArgumentException(SR.IsfOperationFailed, "stream");
		}
		catch (InvalidOperationException)
		{
			throw new ArgumentException(SR.IsfOperationFailed, "stream");
		}
		catch (IndexOutOfRangeException)
		{
			throw new ArgumentException(SR.IsfOperationFailed, "stream");
		}
		catch (NullReferenceException)
		{
			throw new ArgumentException(SR.IsfOperationFailed, "stream");
		}
		catch (EndOfStreamException)
		{
			throw new ArgumentException(SR.IsfOperationFailed, "stream");
		}
		catch (OverflowException)
		{
			throw new ArgumentException(SR.IsfOperationFailed, "stream");
		}
	}

	internal uint LoadStrokeIds(Stream isfStream, uint cbSize)
	{
		if (cbSize == 0)
		{
			return 0u;
		}
		uint num = cbSize;
		uint num2 = SerializationHelper.Decode(isfStream, out var dw);
		if (num2 > num)
		{
			throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "isfStream");
		}
		num -= num2;
		if (dw == 0)
		{
			return cbSize - num;
		}
		num2 = num;
		byte[] buffer = new byte[num2];
		uint num3 = ReliableRead(isfStream, buffer, num2);
		if (num2 != num3)
		{
			throw new ArgumentException(ISFDebugMessage("Read different size from stream then expected"), "isfStream");
		}
		if (num - num2 != 0)
		{
			throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "isfStream");
		}
		return cbSize;
	}

	private bool IsGIFData(Stream inkdata)
	{
		long position = inkdata.Position;
		try
		{
			return (byte)inkdata.ReadByte() == 71 && (byte)inkdata.ReadByte() == 73 && (byte)inkdata.ReadByte() == 70;
		}
		finally
		{
			inkdata.Position = position;
		}
	}

	private void ExamineStreamHeader(Stream inkdata, out bool fBase64, out bool fGif, out uint cbData)
	{
		fGif = false;
		cbData = 0u;
		fBase64 = false;
		if (inkdata.Length >= 7)
		{
			fBase64 = IsBase64Data(inkdata);
		}
		if (!fBase64 && inkdata.Length >= 3)
		{
			fGif = IsGIFData(inkdata);
		}
	}

	private void DecodeRawISF(Stream inputStream)
	{
		uint num = 0u;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		uint dw = 0u;
		uint num2 = uint.MaxValue;
		uint dw2 = 0u;
		uint num3 = uint.MaxValue;
		uint dw3 = 0u;
		uint num4 = uint.MaxValue;
		uint dw4 = 0u;
		uint num5 = uint.MaxValue;
		GuidList guidList = new GuidList();
		int num6 = 0;
		StylusPointDescription stylusPointDescription = null;
		Matrix transform = Matrix.Identity;
		_strokeDescriptorTable = new List<StrokeDescriptor>();
		_drawingAttributesTable = new List<DrawingAttributes>();
		_transformTable = new List<TransformDescriptor>();
		_metricTable = new List<MetricBlock>();
		if (_coreStrokes.Count != 0 || _coreStrokes.ExtendedProperties.Count != 0)
		{
			throw new InvalidOperationException(ISFDebugMessage("ISF decoder cannot operate on non-empty ink container"));
		}
		uint num7 = SerializationHelper.Decode(inputStream, out var dw5);
		if (dw5 != 0)
		{
			throw new ArgumentException(SR.InvalidStream);
		}
		num7 = SerializationHelper.Decode(inputStream, out var dw6);
		if (dw6 == 0)
		{
			return;
		}
		while (0 < dw6)
		{
			num = 0u;
			num7 = SerializationHelper.Decode(inputStream, out dw5);
			KnownTagCache.KnownTagIndex knownTagIndex = (KnownTagCache.KnownTagIndex)dw5;
			if (dw6 >= num7)
			{
				dw6 -= num7;
				switch (knownTagIndex)
				{
				case KnownTagCache.KnownTagIndex.GuidTable:
				case KnownTagCache.KnownTagIndex.DrawingAttributesTable:
				case KnownTagCache.KnownTagIndex.DrawingAttributesBlock:
				case KnownTagCache.KnownTagIndex.StrokeDescriptorTable:
				case KnownTagCache.KnownTagIndex.StrokeDescriptorBlock:
				case KnownTagCache.KnownTagIndex.Stroke:
				case KnownTagCache.KnownTagIndex.CompressionHeader:
				case KnownTagCache.KnownTagIndex.TransformTable:
				case KnownTagCache.KnownTagIndex.MetricTable:
				case KnownTagCache.KnownTagIndex.MetricBlock:
				case KnownTagCache.KnownTagIndex.PersistenceFormat:
				case KnownTagCache.KnownTagIndex.HimetricSize:
				case KnownTagCache.KnownTagIndex.StrokeIds:
				case KnownTagCache.KnownTagIndex.ExtendedTransformTable:
					num7 = SerializationHelper.Decode(inputStream, out num);
					if (dw6 < num7 + num)
					{
						throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "inputStream");
					}
					dw6 -= num7;
					switch (knownTagIndex)
					{
					case KnownTagCache.KnownTagIndex.GuidTable:
						num7 = guidList.Load(inputStream, num);
						break;
					case KnownTagCache.KnownTagIndex.DrawingAttributesTable:
						num7 = LoadDrawAttrsTable(inputStream, guidList, num);
						flag2 = true;
						break;
					case KnownTagCache.KnownTagIndex.DrawingAttributesBlock:
					{
						ExtendedPropertyCollection extendedPropertyCollection = new ExtendedPropertyCollection();
						extendedPropertyCollection.Add(KnownIds.DrawingFlags, DrawingFlags.Polyline);
						DrawingAttributes drawingAttributes2 = new DrawingAttributes(extendedPropertyCollection);
						num7 = DrawingAttributeSerializer.DecodeAsISF(inputStream, guidList, num, drawingAttributes2);
						_drawingAttributesTable.Add(drawingAttributes2);
						flag2 = true;
						break;
					}
					case KnownTagCache.KnownTagIndex.StrokeDescriptorTable:
						num7 = DecodeStrokeDescriptorTable(inputStream, num);
						flag = true;
						break;
					case KnownTagCache.KnownTagIndex.StrokeDescriptorBlock:
						num7 = DecodeStrokeDescriptorBlock(inputStream, num);
						flag = true;
						break;
					case KnownTagCache.KnownTagIndex.MetricTable:
						num7 = DecodeMetricTable(inputStream, num);
						flag3 = true;
						break;
					case KnownTagCache.KnownTagIndex.MetricBlock:
					{
						num7 = DecodeMetricBlock(inputStream, num, out var block2);
						_metricTable.Clear();
						_metricTable.Add(block2);
						flag3 = true;
						break;
					}
					case KnownTagCache.KnownTagIndex.TransformTable:
						num7 = DecodeTransformTable(inputStream, num, useDoubles: false);
						flag4 = true;
						break;
					case KnownTagCache.KnownTagIndex.ExtendedTransformTable:
						if (!flag4)
						{
							throw new ArgumentException(ISFDebugMessage("Invalid ISF data"));
						}
						num7 = DecodeTransformTable(inputStream, num, useDoubles: true);
						break;
					case KnownTagCache.KnownTagIndex.PersistenceFormat:
					{
						num7 = SerializationHelper.Decode(inputStream, out var dw7);
						if (dw7 == 0)
						{
							CurrentPersistenceFormat = PersistenceFormat.InkSerializedFormat;
						}
						else if (1 == dw7)
						{
							CurrentPersistenceFormat = PersistenceFormat.Gif;
						}
						break;
					}
					case KnownTagCache.KnownTagIndex.HimetricSize:
					{
						num7 = SerializationHelper.SignDecode(inputStream, out var i);
						if (num7 > dw6)
						{
							throw new ArgumentException(ISFDebugMessage("Invalid ISF data"));
						}
						_himetricSize.X = i;
						num7 += SerializationHelper.SignDecode(inputStream, out i);
						_himetricSize.Y = i;
						break;
					}
					case KnownTagCache.KnownTagIndex.CompressionHeader:
						inputStream.Seek(num, SeekOrigin.Current);
						num7 = num;
						break;
					case KnownTagCache.KnownTagIndex.StrokeIds:
						num7 = LoadStrokeIds(inputStream, num);
						break;
					case KnownTagCache.KnownTagIndex.Stroke:
					{
						StrokeDescriptor strokeDescriptor = null;
						if (flag)
						{
							if (num2 != dw && _strokeDescriptorTable.Count <= dw)
							{
								throw new ArgumentException(ISFDebugMessage("Invalid ISF data"));
							}
							strokeDescriptor = _strokeDescriptorTable[(int)dw];
						}
						if (num5 != dw4)
						{
							if (flag4)
							{
								if (_transformTable.Count <= dw4)
								{
									throw new ArgumentException(ISFDebugMessage("Invalid ISF data"));
								}
								transform = LoadTransform(_transformTable[(int)dw4]);
							}
							num5 = dw4;
							transform.Scale(24.0 / 635.0, 24.0 / 635.0);
						}
						MetricBlock block = null;
						if (flag3)
						{
							if (num4 != dw3 && _metricTable.Count <= dw3)
							{
								throw new ArgumentException(ISFDebugMessage("Invalid ISF data"));
							}
							block = _metricTable[(int)dw3];
						}
						DrawingAttributes drawingAttributes = null;
						if (flag2)
						{
							if (num3 != dw2)
							{
								if (_drawingAttributesTable.Count <= dw2)
								{
									throw new ArgumentException(ISFDebugMessage("Invalid ISF data"));
								}
								num3 = dw2;
							}
							drawingAttributes = _drawingAttributesTable[(int)dw2].Clone();
						}
						if (drawingAttributes == null)
						{
							drawingAttributes = new DrawingAttributes();
						}
						if (num4 != dw3 || num2 != dw)
						{
							stylusPointDescription = BuildStylusPointDescription(strokeDescriptor, block, guidList);
							num2 = dw;
							num4 = dw3;
						}
						num7 = StrokeSerializer.DecodeStroke(inputStream, num, guidList, strokeDescriptor, stylusPointDescription, drawingAttributes, transform, out var stroke);
						if (stroke != null)
						{
							_coreStrokes.AddWithoutEvent(stroke);
							num6++;
						}
						break;
					}
					default:
						throw new InvalidOperationException(ISFDebugMessage("Invalid ISF tag logic"));
					}
					if (num7 != num)
					{
						throw new ArgumentException(ISFDebugMessage("Invalid ISF data"));
					}
					break;
				case KnownTagCache.KnownTagIndex.Transform:
				case KnownTagCache.KnownTagIndex.TransformIsotropicScale:
				case KnownTagCache.KnownTagIndex.TransformAnisotropicScale:
				case KnownTagCache.KnownTagIndex.TransformRotate:
				case KnownTagCache.KnownTagIndex.TransformTranslate:
				case KnownTagCache.KnownTagIndex.TransformScaleAndTranslate:
				{
					num = DecodeTransformBlock(inputStream, knownTagIndex, dw6, useDoubles: false, out var xform);
					flag4 = true;
					_transformTable.Clear();
					_transformTable.Add(xform);
					break;
				}
				case KnownTagCache.KnownTagIndex.TransformTableIndex:
					num = SerializationHelper.Decode(inputStream, out dw4);
					break;
				case KnownTagCache.KnownTagIndex.MetricTableIndex:
					num = SerializationHelper.Decode(inputStream, out dw3);
					break;
				case KnownTagCache.KnownTagIndex.DrawingAttributesTableIndex:
					num = SerializationHelper.Decode(inputStream, out dw2);
					break;
				case KnownTagCache.KnownTagIndex.Unknown:
					num = DecodeInkSpaceRectangle(inputStream, dw6);
					break;
				case KnownTagCache.KnownTagIndex.StrokeDescriptorTableIndex:
					num = SerializationHelper.Decode(inputStream, out dw);
					break;
				default:
					if ((uint)knownTagIndex >= KnownIdCache.CustomGuidBaseIndex || ((uint)knownTagIndex >= KnownTagCache.KnownTagCount && (long)knownTagIndex < KnownTagCache.KnownTagCount + KnownIdCache.OriginalISFIdTable.Length))
					{
						num = dw6;
						Guid guid = guidList.FindGuid(knownTagIndex);
						if (guid == Guid.Empty)
						{
							throw new ArgumentException(ISFDebugMessage("Global Custom Attribute tag embedded in ISF stream does not match guid table"), "inkdata");
						}
						num7 = ExtendedPropertySerializer.DecodeAsISF(inputStream, num, guidList, knownTagIndex, ref guid, out var data);
						if (num7 > num)
						{
							throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "inkdata");
						}
						_coreStrokes.ExtendedProperties[guid] = data;
					}
					else
					{
						num7 = SerializationHelper.Decode(inputStream, out num);
						if (dw6 < num7 + num)
						{
							throw new ArgumentException(ISFDebugMessage("Invalid ISF data"));
						}
						inputStream.Seek(num + num7, SeekOrigin.Current);
					}
					num = num7;
					break;
				}
				if (num > dw6)
				{
					throw new ArgumentException(ISFDebugMessage("Invalid ISF data"));
				}
				dw6 -= num;
				continue;
			}
			throw new ArgumentException(ISFDebugMessage("Invalid ISF data"));
		}
		if (dw6 == 0)
		{
			return;
		}
		throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "inkdata");
	}

	private uint LoadDrawAttrsTable(Stream strm, GuidList guidList, uint cbSize)
	{
		_drawingAttributesTable.Clear();
		uint num = cbSize;
		uint dw = 0u;
		while (num != 0)
		{
			uint num2 = SerializationHelper.Decode(strm, out dw);
			if (cbSize < num2)
			{
				throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
			}
			num -= num2;
			if (num < dw)
			{
				throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
			}
			DrawingAttributes drawingAttributes = new DrawingAttributes();
			drawingAttributes.DrawingFlags = DrawingFlags.Polyline;
			num2 = DrawingAttributeSerializer.DecodeAsISF(strm, guidList, dw, drawingAttributes);
			if (cbSize < dw)
			{
				throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
			}
			num -= dw;
			_drawingAttributesTable.Add(drawingAttributes);
		}
		if (num != 0)
		{
			throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
		}
		return cbSize;
	}

	private uint DecodeStrokeDescriptor(Stream strm, uint cbSize, out StrokeDescriptor descr)
	{
		descr = new StrokeDescriptor();
		if (cbSize == 0)
		{
			return 0u;
		}
		uint num = cbSize;
		while (num != 0)
		{
			uint num2 = SerializationHelper.Decode(strm, out var dw);
			KnownTagCache.KnownTagIndex knownTagIndex = (KnownTagCache.KnownTagIndex)dw;
			if (num2 > num)
			{
				throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
			}
			num -= num2;
			descr.Template.Add(knownTagIndex);
			if (KnownTagCache.KnownTagIndex.Buttons == knownTagIndex && num != 0)
			{
				num2 = SerializationHelper.Decode(strm, out var dw2);
				if (num2 > num)
				{
					throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
				}
				num -= num2;
				descr.Template.Add((KnownTagCache.KnownTagIndex)dw2);
				while (num != 0 && dw2 != 0)
				{
					num2 = SerializationHelper.Decode(strm, out var dw3);
					if (num2 > num)
					{
						throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
					}
					num -= num2;
					dw2--;
					descr.Template.Add((KnownTagCache.KnownTagIndex)dw3);
				}
			}
			else
			{
				if (KnownTagCache.KnownTagIndex.StrokePropertyList != knownTagIndex || num == 0)
				{
					continue;
				}
				while (num != 0)
				{
					num2 = SerializationHelper.Decode(strm, out var dw4);
					if (num2 > num)
					{
						throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
					}
					num -= num2;
					descr.Template.Add((KnownTagCache.KnownTagIndex)dw4);
				}
			}
		}
		if (num != 0)
		{
			throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
		}
		return cbSize;
	}

	private uint DecodeStrokeDescriptorBlock(Stream strm, uint cbSize)
	{
		_strokeDescriptorTable.Clear();
		if (cbSize == 0)
		{
			return 0u;
		}
		StrokeDescriptor descr;
		uint num = DecodeStrokeDescriptor(strm, cbSize, out descr);
		if (num != cbSize)
		{
			throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
		}
		_strokeDescriptorTable.Add(descr);
		return num;
	}

	private uint DecodeStrokeDescriptorTable(Stream strm, uint cbSize)
	{
		_strokeDescriptorTable.Clear();
		if (cbSize == 0)
		{
			return 0u;
		}
		uint num = cbSize;
		while (num != 0)
		{
			uint num2 = SerializationHelper.Decode(strm, out var dw);
			if (num2 > num)
			{
				throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
			}
			num -= num2;
			if (dw > num)
			{
				throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
			}
			num2 = DecodeStrokeDescriptor(strm, dw, out var descr);
			if (num2 != dw)
			{
				throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
			}
			num -= num2;
			_strokeDescriptorTable.Add(descr);
		}
		if (num != 0)
		{
			throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
		}
		return cbSize;
	}

	private uint DecodeMetricTable(Stream strm, uint cbSize)
	{
		_metricTable.Clear();
		if (cbSize == 0)
		{
			return 0u;
		}
		uint num = cbSize;
		while (num != 0)
		{
			uint num2 = SerializationHelper.Decode(strm, out var dw);
			if (num2 + dw > num)
			{
				throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
			}
			num -= num2;
			num2 = DecodeMetricBlock(strm, dw, out var block);
			if (num2 != dw)
			{
				throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
			}
			num -= num2;
			_metricTable.Add(block);
		}
		if (num != 0)
		{
			throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
		}
		return cbSize;
	}

	private uint DecodeMetricBlock(Stream strm, uint cbSize, out MetricBlock block)
	{
		block = new MetricBlock();
		if (cbSize == 0)
		{
			return 0u;
		}
		uint num = cbSize;
		while (num != 0)
		{
			uint num2 = SerializationHelper.Decode(strm, out var dw);
			if (num2 > num)
			{
				throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
			}
			num -= num2;
			num2 = SerializationHelper.Decode(strm, out var dw2);
			if (num2 + dw2 > num)
			{
				throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
			}
			num -= num2;
			MetricEntry metricEntry = new MetricEntry();
			metricEntry.Tag = (KnownTagCache.KnownTagIndex)dw;
			byte[] array = new byte[dw2];
			uint num3 = ReliableRead(strm, array, dw2);
			num -= num3;
			if (num3 != dw2)
			{
				break;
			}
			metricEntry.Data = array;
			block.AddMetricEntry(metricEntry);
		}
		if (num != 0)
		{
			throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
		}
		return cbSize;
	}

	private uint DecodeTransformTable(Stream strm, uint cbSize, bool useDoubles)
	{
		if (!useDoubles)
		{
			_transformTable.Clear();
		}
		if (cbSize == 0)
		{
			return 0u;
		}
		uint num = cbSize;
		int num2 = 0;
		while (num != 0)
		{
			uint num3 = SerializationHelper.Decode(strm, out var dw);
			KnownTagCache.KnownTagIndex tag = (KnownTagCache.KnownTagIndex)dw;
			if (num3 > num)
			{
				throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
			}
			num -= num3;
			num3 = DecodeTransformBlock(strm, tag, num, useDoubles, out var xform);
			num -= num3;
			if (useDoubles)
			{
				_transformTable[num2] = xform;
			}
			else
			{
				_transformTable.Add(xform);
			}
			num2++;
		}
		if (num != 0)
		{
			throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
		}
		return cbSize;
	}

	internal static uint ReliableRead(Stream stream, byte[] buffer, uint requestedCount)
	{
		if (stream == null || buffer == null || requestedCount > buffer.Length)
		{
			throw new ArgumentException(ISFDebugMessage("Invalid argument passed to ReliableRead"));
		}
		uint num;
		int num2;
		for (num = 0u; num < requestedCount; num += (uint)num2)
		{
			num2 = stream.Read(buffer, (int)num, (int)(requestedCount - num));
			if (num2 == 0)
			{
				break;
			}
		}
		return num;
	}

	private uint DecodeTransformBlock(Stream strm, KnownTagCache.KnownTagIndex tag, uint cbSize, bool useDoubles, out TransformDescriptor xform)
	{
		xform = new TransformDescriptor();
		xform.Tag = tag;
		uint num = 0u;
		if (cbSize == 0)
		{
			return 0u;
		}
		BinaryReader binaryReader = new BinaryReader(strm);
		if (KnownTagCache.KnownTagIndex.TransformRotate == tag)
		{
			num = SerializationHelper.Decode(strm, out var dw);
			if (num > cbSize)
			{
				throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
			}
			xform.Transform[0] = dw;
			xform.Size = 1u;
		}
		else
		{
			switch (tag)
			{
			case KnownTagCache.KnownTagIndex.TransformIsotropicScale:
				xform.Size = 1u;
				break;
			case KnownTagCache.KnownTagIndex.TransformAnisotropicScale:
			case KnownTagCache.KnownTagIndex.TransformTranslate:
				xform.Size = 2u;
				break;
			case KnownTagCache.KnownTagIndex.TransformScaleAndTranslate:
				xform.Size = 4u;
				break;
			default:
				xform.Size = 6u;
				break;
			}
			num = ((!useDoubles) ? (xform.Size * Native.SizeOfFloat) : (xform.Size * Native.SizeOfDouble));
			if (num > cbSize)
			{
				throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
			}
			for (int i = 0; i < xform.Size; i++)
			{
				if (useDoubles)
				{
					xform.Transform[i] = binaryReader.ReadDouble();
				}
				else
				{
					xform.Transform[i] = binaryReader.ReadSingle();
				}
			}
		}
		return num;
	}

	private uint DecodeInkSpaceRectangle(Stream strm, uint cbSize)
	{
		uint num = cbSize;
		uint num2 = SerializationHelper.SignDecode(strm, out var i);
		if (num2 > num)
		{
			throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
		}
		num -= num2;
		uint num3 = 0 + num2;
		_inkSpaceRectangle.X = i;
		if (num3 > cbSize)
		{
			throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
		}
		num2 = SerializationHelper.SignDecode(strm, out i);
		if (num2 > num)
		{
			throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
		}
		num -= num2;
		uint num4 = num3 + num2;
		_inkSpaceRectangle.Y = i;
		if (num4 > cbSize)
		{
			throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
		}
		num2 = SerializationHelper.SignDecode(strm, out i);
		if (num2 > num)
		{
			throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
		}
		num -= num2;
		uint num5 = num4 + num2;
		_inkSpaceRectangle.Width = (double)i - _inkSpaceRectangle.Left;
		if (num5 > cbSize)
		{
			throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
		}
		num2 = SerializationHelper.SignDecode(strm, out i);
		if (num2 > num)
		{
			throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
		}
		num -= num2;
		uint num6 = num5 + num2;
		_inkSpaceRectangle.Height = (double)i - _inkSpaceRectangle.Top;
		if (num6 > cbSize)
		{
			throw new ArgumentException(ISFDebugMessage("Invalid ISF data"), "strm");
		}
		return num6;
	}

	private Matrix LoadTransform(TransformDescriptor tdrd)
	{
		double m = 0.0;
		double num = 0.0;
		double m2 = 0.0;
		double num2 = 0.0;
		double offsetX = 0.0;
		double offsetY = 0.0;
		if (KnownTagCache.KnownTagIndex.TransformIsotropicScale == tdrd.Tag)
		{
			m = (num2 = tdrd.Transform[0]);
		}
		else if (KnownTagCache.KnownTagIndex.TransformRotate == tdrd.Tag)
		{
			double num3 = tdrd.Transform[0] / 100.0 * (Math.PI / 180.0);
			m = (num2 = Math.Cos(num3));
			num = Math.Sin(num3);
			m2 = ((num != 0.0 || num2 != 1.0) ? (0.0 - num2) : 0.0);
		}
		else if (KnownTagCache.KnownTagIndex.TransformAnisotropicScale == tdrd.Tag)
		{
			m = tdrd.Transform[0];
			num2 = tdrd.Transform[1];
		}
		else if (KnownTagCache.KnownTagIndex.TransformTranslate == tdrd.Tag)
		{
			offsetX = tdrd.Transform[0];
			offsetY = tdrd.Transform[1];
		}
		else if (KnownTagCache.KnownTagIndex.TransformScaleAndTranslate == tdrd.Tag)
		{
			m = tdrd.Transform[0];
			num2 = tdrd.Transform[1];
			offsetX = tdrd.Transform[2];
			offsetY = tdrd.Transform[3];
		}
		else
		{
			m = tdrd.Transform[0];
			num = tdrd.Transform[1];
			m2 = tdrd.Transform[2];
			num2 = tdrd.Transform[3];
			offsetX = tdrd.Transform[4];
			offsetY = tdrd.Transform[5];
		}
		return new Matrix(m, num, m2, num2, offsetX, offsetY);
	}

	private StylusPointPropertyInfo GetStylusPointPropertyInfo(Guid guid, KnownTagCache.KnownTagIndex tag, MetricBlock block)
	{
		int num = 0;
		bool flag = false;
		int minimum = 0;
		int maximum = 0;
		StylusPointPropertyUnit unit = StylusPointPropertyUnit.None;
		float resolution = 1f;
		for (num = 0; num < 11; num++)
		{
			if (MetricEntry.MetricEntry_Optional[num].Tag == tag)
			{
				minimum = MetricEntry.MetricEntry_Optional[num].PropertyMetrics.Minimum;
				maximum = MetricEntry.MetricEntry_Optional[num].PropertyMetrics.Maximum;
				resolution = MetricEntry.MetricEntry_Optional[num].PropertyMetrics.Resolution;
				unit = MetricEntry.MetricEntry_Optional[num].PropertyMetrics.Unit;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			minimum = int.MinValue;
			maximum = int.MaxValue;
			unit = StylusPointPropertyUnit.None;
			resolution = 1f;
			flag = true;
		}
		if (block != null)
		{
			for (MetricEntry metricEntry = block.GetMetricEntryList(); metricEntry != null; metricEntry = metricEntry.Next)
			{
				if (metricEntry.Tag == tag)
				{
					uint num2 = 0u;
					using (MemoryStream memoryStream = new MemoryStream(metricEntry.Data))
					{
						num2 += SerializationHelper.SignDecode(memoryStream, out var i);
						if (num2 >= metricEntry.Size)
						{
							break;
						}
						minimum = i;
						num2 += SerializationHelper.SignDecode(memoryStream, out i);
						if (num2 >= metricEntry.Size)
						{
							break;
						}
						maximum = i;
						num2 += SerializationHelper.Decode(memoryStream, out var dw);
						unit = (StylusPointPropertyUnit)dw;
						if (num2 < metricEntry.Size)
						{
							using BinaryReader binaryReader = new BinaryReader(memoryStream);
							resolution = binaryReader.ReadSingle();
							num2 += Native.SizeOfFloat;
						}
					}
					break;
				}
			}
		}
		return new StylusPointPropertyInfo(new StylusPointProperty(guid, StylusPointPropertyIds.IsKnownButton(guid)), minimum, maximum, unit, resolution);
	}

	private StylusPointDescription BuildStylusPointDescription(StrokeDescriptor strd, MetricBlock block, GuidList guidList)
	{
		int num = 0;
		int num2 = 0;
		uint num3 = 0u;
		Guid[] array = null;
		List<KnownTagCache.KnownTagIndex> list = null;
		if (strd != null)
		{
			list = new List<KnownTagCache.KnownTagIndex>();
			while (num < strd.Template.Count)
			{
				KnownTagCache.KnownTagIndex knownTagIndex = strd.Template[num];
				if (KnownTagCache.KnownTagIndex.Buttons == knownTagIndex)
				{
					num++;
					num3 = (uint)strd.Template[num];
					num++;
					array = new Guid[num3];
					for (uint num4 = 0u; num4 < num3; num4++)
					{
						Guid guid = guidList.FindGuid(strd.Template[num]);
						if (guid == Guid.Empty)
						{
							throw new ArgumentException(ISFDebugMessage("Button guid tag embedded in ISF stream does not match guid table"), "strd");
						}
						array[num4] = guid;
						num++;
					}
				}
				else
				{
					if (KnownTagCache.KnownTagIndex.StrokePropertyList == knownTagIndex)
					{
						break;
					}
					if (KnownTagCache.KnownTagIndex.NoX == knownTagIndex || KnownTagCache.KnownTagIndex.NoY == knownTagIndex)
					{
						throw new ArgumentException(ISFDebugMessage("Invalid ISF with NoX or NoY specified"), "strd");
					}
					list.Add(strd.Template[num]);
					num2++;
					num++;
				}
			}
		}
		List<StylusPointPropertyInfo> list2 = new List<StylusPointPropertyInfo>();
		list2.Add(GetStylusPointPropertyInfo(KnownIds.X, KnownIdCache.KnownGuidBaseIndex, block));
		list2.Add(GetStylusPointPropertyInfo(KnownIds.Y, KnownIdCache.KnownGuidBaseIndex + 1, block));
		list2.Add(GetStylusPointPropertyInfo(KnownIds.NormalPressure, KnownIdCache.KnownGuidBaseIndex + 6, block));
		int num5 = -1;
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				Guid guid2 = guidList.FindGuid(list[i]);
				if (guid2 == Guid.Empty)
				{
					throw new ArgumentException(ISFDebugMessage("Packet Description Property tag embedded in ISF stream does not match guid table"), "strd");
				}
				if (num5 == -1 && guid2 == StylusPointPropertyIds.NormalPressure)
				{
					num5 = i + 2;
				}
				else
				{
					list2.Add(GetStylusPointPropertyInfo(guid2, list[i], block));
				}
			}
			if (array != null)
			{
				for (int j = 0; j < array.Length; j++)
				{
					StylusPointPropertyInfo item = new StylusPointPropertyInfo(new StylusPointProperty(array[j], isButton: true));
					list2.Add(item);
				}
			}
		}
		return new StylusPointDescription(list2, num5);
	}

	internal void EncodeISF(Stream outputStream)
	{
		_strokeLookupTable = new Dictionary<Stroke, StrokeLookupEntry>(_coreStrokes.Count);
		for (int i = 0; i < _coreStrokes.Count; i++)
		{
			_strokeLookupTable.Add(_coreStrokes[i], new StrokeLookupEntry());
		}
		_strokeDescriptorTable = new List<StrokeDescriptor>(_coreStrokes.Count);
		_drawingAttributesTable = new List<DrawingAttributes>();
		_metricTable = new List<MetricBlock>();
		_transformTable = new List<TransformDescriptor>();
		using MemoryStream memoryStream = new MemoryStream(_coreStrokes.Count * 125);
		GuidList guidList = BuildGuidList();
		uint num = 0u;
		uint num2 = 0u;
		byte compressionData = (byte)((CurrentCompressionMode != CompressionMode.NoCompression) ? 192 : 0);
		foreach (Stroke coreStroke in _coreStrokes)
		{
			_strokeLookupTable[coreStroke].CompressionData = compressionData;
			coreStroke.StylusPoints.ToISFReadyArrays(out var output, out var shouldPersistPressure);
			_strokeLookupTable[coreStroke].ISFReadyStrokeData = output;
			_strokeLookupTable[coreStroke].StorePressure = shouldPersistPressure;
		}
		if (_inkSpaceRectangle != default(Rect))
		{
			num2 = num;
			Rect inkSpaceRectangle = _inkSpaceRectangle;
			num += SerializationHelper.Encode(memoryStream, 0u);
			int value = (int)inkSpaceRectangle.Left;
			num += SerializationHelper.SignEncode(memoryStream, value);
			value = (int)inkSpaceRectangle.Top;
			num += SerializationHelper.SignEncode(memoryStream, value);
			value = (int)inkSpaceRectangle.Right;
			num += SerializationHelper.SignEncode(memoryStream, value);
			value = (int)inkSpaceRectangle.Bottom;
			num += SerializationHelper.SignEncode(memoryStream, value);
			num2 = num - num2;
			if (num != memoryStream.Length)
			{
				throw new InvalidOperationException(ISFDebugMessage("Calculated ISF stream size != actual stream size"));
			}
		}
		if (CurrentPersistenceFormat != 0)
		{
			num2 = num;
			num += SerializationHelper.Encode(memoryStream, 28u);
			num += SerializationHelper.Encode(memoryStream, SerializationHelper.VarSize((uint)CurrentPersistenceFormat));
			num += SerializationHelper.Encode(memoryStream, (uint)CurrentPersistenceFormat);
			num2 = num - num2;
			if (num != memoryStream.Length)
			{
				throw new InvalidOperationException(ISFDebugMessage("Calculated ISF stream size != actual stream size"));
			}
		}
		num2 = num;
		num += guidList.Save(memoryStream);
		num2 = num - num2;
		if (num != memoryStream.Length)
		{
			throw new InvalidOperationException(ISFDebugMessage("Calculated ISF stream size != actual stream size"));
		}
		BuildTables(guidList);
		num2 = num;
		num += SerializeDrawingAttrsTable(memoryStream, guidList);
		num2 = num - num2;
		if (num != memoryStream.Length)
		{
			throw new InvalidOperationException(ISFDebugMessage("Calculated ISF stream size != actual stream size"));
		}
		num2 = num;
		num += SerializePacketDescrTable(memoryStream);
		num2 = num - num2;
		if (num != memoryStream.Length)
		{
			throw new InvalidOperationException(ISFDebugMessage("Calculated ISF stream size != actual stream size"));
		}
		num2 = num;
		num += SerializeMetricTable(memoryStream);
		num2 = num - num2;
		if (num != memoryStream.Length)
		{
			throw new InvalidOperationException(ISFDebugMessage("Calculated ISF stream size != actual stream size"));
		}
		num2 = num;
		num += SerializeTransformTable(memoryStream);
		num2 = num - num2;
		if (num != memoryStream.Length)
		{
			throw new InvalidOperationException(ISFDebugMessage("Calculated ISF stream size != actual stream size"));
		}
		if (_coreStrokes.ExtendedProperties.Count > 0)
		{
			num2 = num;
			num += ExtendedPropertySerializer.EncodeAsISF(_coreStrokes.ExtendedProperties, memoryStream, guidList, GetCompressionAlgorithm(), fTag: true);
			num2 = num - num2;
			if (num != memoryStream.Length)
			{
				throw new InvalidOperationException(ISFDebugMessage("Calculated ISF stream size != actual stream size"));
			}
		}
		num2 = num;
		num += SaveStrokeIds(_coreStrokes, memoryStream, forceSave: false);
		num2 = num - num2;
		if (num != memoryStream.Length)
		{
			throw new InvalidOperationException(ISFDebugMessage("Calculated ISF stream size != actual stream size"));
		}
		StoreStrokeData(memoryStream, guidList, ref num, ref num2);
		long position = outputStream.Position;
		uint num3 = SerializationHelper.Encode(outputStream, 0u) + SerializationHelper.Encode(outputStream, num);
		outputStream.Write(memoryStream.GetBuffer(), 0, (int)num);
		if (num3 + num != outputStream.Position - position)
		{
			throw new InvalidOperationException(ISFDebugMessage("Calculated ISF stream size != actual stream size"));
		}
	}

	private void StoreStrokeData(Stream localStream, GuidList guidList, ref uint cumulativeEncodedSize, ref uint localEncodedSize)
	{
		uint num = 0u;
		uint num2 = 0u;
		uint num3 = 0u;
		uint num4 = 0u;
		StrokeIdGenerator.GetStrokeIds(_coreStrokes);
		for (int i = 0; i < _coreStrokes.Count; i++)
		{
			Stroke stroke = _coreStrokes[i];
			uint num5 = 0u;
			if (num != _strokeLookupTable[stroke].DrawingAttributesTableIndex)
			{
				localEncodedSize = cumulativeEncodedSize;
				cumulativeEncodedSize += SerializationHelper.Encode(localStream, 9u);
				cumulativeEncodedSize += SerializationHelper.Encode(localStream, _strokeLookupTable[stroke].DrawingAttributesTableIndex);
				num = _strokeLookupTable[stroke].DrawingAttributesTableIndex;
				localEncodedSize = cumulativeEncodedSize - localEncodedSize;
				_ = localEncodedSize;
				if (cumulativeEncodedSize != localStream.Length)
				{
					throw new InvalidOperationException(ISFDebugMessage("Calculated ISF stream size != actual stream size"));
				}
			}
			if (num2 != _strokeLookupTable[stroke].StrokeDescriptorTableIndex)
			{
				localEncodedSize = cumulativeEncodedSize;
				cumulativeEncodedSize += SerializationHelper.Encode(localStream, 13u);
				cumulativeEncodedSize += SerializationHelper.Encode(localStream, _strokeLookupTable[stroke].StrokeDescriptorTableIndex);
				num2 = _strokeLookupTable[stroke].StrokeDescriptorTableIndex;
				localEncodedSize = cumulativeEncodedSize - localEncodedSize;
				_ = localEncodedSize;
				if (cumulativeEncodedSize != localStream.Length)
				{
					throw new InvalidOperationException(ISFDebugMessage("Calculated ISF stream size != actual stream size"));
				}
			}
			if (num3 != _strokeLookupTable[stroke].MetricDescriptorTableIndex)
			{
				localEncodedSize = cumulativeEncodedSize;
				cumulativeEncodedSize += SerializationHelper.Encode(localStream, 26u);
				cumulativeEncodedSize += SerializationHelper.Encode(localStream, _strokeLookupTable[stroke].MetricDescriptorTableIndex);
				num3 = _strokeLookupTable[stroke].MetricDescriptorTableIndex;
				localEncodedSize = cumulativeEncodedSize - localEncodedSize;
				_ = localEncodedSize;
				if (cumulativeEncodedSize != localStream.Length)
				{
					throw new InvalidOperationException(ISFDebugMessage("Calculated ISF stream size != actual stream size"));
				}
			}
			if (num4 != _strokeLookupTable[stroke].TransformTableIndex)
			{
				localEncodedSize = cumulativeEncodedSize;
				cumulativeEncodedSize += SerializationHelper.Encode(localStream, 23u);
				cumulativeEncodedSize += SerializationHelper.Encode(localStream, _strokeLookupTable[stroke].TransformTableIndex);
				num4 = _strokeLookupTable[stroke].TransformTableIndex;
				localEncodedSize = cumulativeEncodedSize - localEncodedSize;
				_ = localEncodedSize;
				if (cumulativeEncodedSize != localStream.Length)
				{
					throw new InvalidOperationException(ISFDebugMessage("Calculated ISF stream size != actual stream size"));
				}
			}
			using (MemoryStream memoryStream = new MemoryStream(stroke.StylusPoints.Count * 5))
			{
				localEncodedSize = cumulativeEncodedSize;
				num5 = StrokeSerializer.EncodeStroke(stroke, memoryStream, GetCompressionAlgorithm(), guidList, _strokeLookupTable[stroke]);
				if (num5 != memoryStream.Length)
				{
					throw new InvalidOperationException(ISFDebugMessage("Encoded stroke size != reported size"));
				}
				cumulativeEncodedSize += SerializationHelper.Encode(localStream, 10u);
				cumulativeEncodedSize += SerializationHelper.Encode(localStream, num5);
				localStream.Write(memoryStream.GetBuffer(), 0, (int)num5);
				cumulativeEncodedSize += num5;
				localEncodedSize = cumulativeEncodedSize - localEncodedSize;
				_ = localEncodedSize;
				if (cumulativeEncodedSize != localStream.Length)
				{
					throw new InvalidOperationException(ISFDebugMessage("Calculated ISF stream size != actual stream size"));
				}
			}
			if (cumulativeEncodedSize != localStream.Length)
			{
				throw new InvalidOperationException(ISFDebugMessage("Calculated ISF stream size != actual stream size"));
			}
		}
	}

	internal static uint SaveStrokeIds(StrokeCollection strokes, Stream strm, bool forceSave)
	{
		if (strokes.Count == 0)
		{
			return 0u;
		}
		int[] strokeIds = StrokeIdGenerator.GetStrokeIds(strokes);
		bool flag = true;
		if (!forceSave)
		{
			for (int i = 0; i < strokeIds.Length; i++)
			{
				if (strokeIds[i] != i + 1)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return 0u;
			}
		}
		uint num = SerializationHelper.Encode(strm, 30u);
		byte algorithm = 192;
		byte[] array = Compressor.CompressPacketData(strokeIds, ref algorithm);
		if (array != null)
		{
			num += SerializationHelper.Encode(strm, (uint)(array.Length + SerializationHelper.VarSize((uint)strokes.Count)));
			num += SerializationHelper.Encode(strm, (uint)strokes.Count);
			strm.Write(array, 0, array.Length);
			num += (uint)array.Length;
		}
		else
		{
			byte value = 0;
			uint value2 = (uint)(strokes.Count * Native.SizeOfInt + 1 + SerializationHelper.VarSize((uint)strokes.Count));
			num += SerializationHelper.Encode(strm, value2);
			num += SerializationHelper.Encode(strm, (uint)strokes.Count);
			strm.WriteByte(value);
			num++;
			BinaryWriter binaryWriter = new BinaryWriter(strm);
			for (int j = 0; j < strokeIds.Length; j++)
			{
				binaryWriter.Write(strokeIds[j]);
				num += Native.SizeOfInt;
			}
		}
		return num;
	}

	private bool IsBase64Data(Stream data)
	{
		long position = data.Position;
		try
		{
			byte[] base64HeaderBytes = Base64HeaderBytes;
			if (data.Length < base64HeaderBytes.Length)
			{
				return false;
			}
			for (int i = 0; i < base64HeaderBytes.Length; i++)
			{
				if ((byte)data.ReadByte() != base64HeaderBytes[i])
				{
					return false;
				}
			}
			return true;
		}
		finally
		{
			data.Position = position;
		}
	}

	private GuidList BuildGuidList()
	{
		GuidList guidList = new GuidList();
		int num = 0;
		ExtendedPropertyCollection extendedProperties = _coreStrokes.ExtendedProperties;
		for (num = 0; num < extendedProperties.Count; num++)
		{
			guidList.Add(extendedProperties[num].Id);
		}
		for (int i = 0; i < _coreStrokes.Count; i++)
		{
			BuildStrokeGuidList(_coreStrokes[i], guidList);
		}
		return guidList;
	}

	private void BuildStrokeGuidList(Stroke stroke, GuidList guidList)
	{
		int num = 0;
		int count;
		Guid[] unknownGuids = ExtendedPropertySerializer.GetUnknownGuids(stroke.DrawingAttributes.ExtendedProperties, out count);
		for (num = 0; num < count; num++)
		{
			guidList.Add(unknownGuids[num]);
		}
		Guid[] stylusPointPropertyIds = stroke.StylusPoints.Description.GetStylusPointPropertyIds();
		for (num = 0; num < stylusPointPropertyIds.Length; num++)
		{
			guidList.Add(stylusPointPropertyIds[num]);
		}
		if (stroke.ExtendedProperties.Count > 0)
		{
			for (num = 0; num < stroke.ExtendedProperties.Count; num++)
			{
				guidList.Add(stroke.ExtendedProperties[num].Id);
			}
		}
	}

	private byte GetCompressionAlgorithm()
	{
		if (CurrentCompressionMode == CompressionMode.Compressed)
		{
			return 192;
		}
		return 0;
	}

	private uint SerializePacketDescrTable(Stream strm)
	{
		if (_strokeDescriptorTable.Count == 0)
		{
			return 0u;
		}
		int num = 0;
		uint num2 = 0u;
		if (_strokeDescriptorTable.Count == 1)
		{
			StrokeDescriptor strokeDescriptor = _strokeDescriptorTable[0];
			if (strokeDescriptor.Template.Count == 0)
			{
				return 0u;
			}
			num2 += SerializationHelper.Encode(strm, 5u);
			num2 += EncodeStrokeDescriptor(strm, strokeDescriptor);
		}
		else
		{
			uint num3 = 0u;
			for (num = 0; num < _strokeDescriptorTable.Count; num++)
			{
				num3 += SerializationHelper.VarSize(_strokeDescriptorTable[num].Size) + _strokeDescriptorTable[num].Size;
			}
			num2 += SerializationHelper.Encode(strm, 4u);
			num2 += SerializationHelper.Encode(strm, num3);
			for (num = 0; num < _strokeDescriptorTable.Count; num++)
			{
				num2 += EncodeStrokeDescriptor(strm, _strokeDescriptorTable[num]);
			}
		}
		return num2;
	}

	private uint SerializeMetricTable(Stream strm)
	{
		uint num = 0u;
		if (_metricTable.Count == 0)
		{
			return 0u;
		}
		for (int i = 0; i < _metricTable.Count; i++)
		{
			num += _metricTable[i].Size;
		}
		uint num2 = 0u;
		if (1 == num)
		{
			return 0u;
		}
		if (1 == _metricTable.Count)
		{
			num2 += SerializationHelper.Encode(strm, 25u);
		}
		else
		{
			num2 += SerializationHelper.Encode(strm, 24u);
			num2 += SerializationHelper.Encode(strm, num);
		}
		for (int j = 0; j < _metricTable.Count; j++)
		{
			MetricBlock metricBlock = _metricTable[j];
			num2 += metricBlock.Pack(strm);
		}
		return num2;
	}

	private uint EncodeStrokeDescriptor(Stream strm, StrokeDescriptor strd)
	{
		uint num = 0u;
		num += SerializationHelper.Encode(strm, strd.Size);
		for (int i = 0; i < strd.Template.Count; i++)
		{
			num += SerializationHelper.Encode(strm, (uint)strd.Template[i]);
		}
		return num;
	}

	private uint SerializeTransformTable(Stream strm)
	{
		if (_transformTable.Count == 1 && _transformTable[0].Size == 0)
		{
			return 0u;
		}
		uint num = 0u;
		uint num2 = 0u;
		for (int i = 0; i < _transformTable.Count; i++)
		{
			TransformDescriptor transformDescriptor = _transformTable[i];
			uint num3 = SerializationHelper.VarSize((uint)transformDescriptor.Tag);
			num += num3;
			num2 += num3;
			if (KnownTagCache.KnownTagIndex.TransformRotate == transformDescriptor.Tag)
			{
				num3 = SerializationHelper.VarSize((uint)(transformDescriptor.Transform[0] + 0.5));
				num += num3;
				num2 += num3;
			}
			else
			{
				num3 = transformDescriptor.Size * Native.SizeOfFloat;
				num += num3;
				num2 += num3 * 2;
			}
		}
		uint num4 = 0u;
		if (_transformTable.Count == 1)
		{
			TransformDescriptor xform = _transformTable[0];
			num4 = EncodeTransformDescriptor(strm, xform, useDoubles: false);
		}
		else
		{
			num4 += SerializationHelper.Encode(strm, 15u);
			num4 += SerializationHelper.Encode(strm, num);
			for (int j = 0; j < _transformTable.Count; j++)
			{
				num4 += EncodeTransformDescriptor(strm, _transformTable[j], useDoubles: false);
			}
		}
		num4 += SerializationHelper.Encode(strm, 31u);
		num4 += SerializationHelper.Encode(strm, num2);
		for (int k = 0; k < _transformTable.Count; k++)
		{
			num4 += EncodeTransformDescriptor(strm, _transformTable[k], useDoubles: true);
		}
		return num4;
	}

	private uint EncodeTransformDescriptor(Stream strm, TransformDescriptor xform, bool useDoubles)
	{
		uint num = 0u;
		num = SerializationHelper.Encode(strm, (uint)xform.Tag);
		if (KnownTagCache.KnownTagIndex.TransformRotate == xform.Tag)
		{
			uint value = (uint)(xform.Transform[0] + 0.5);
			num += SerializationHelper.Encode(strm, value);
		}
		else
		{
			BinaryWriter binaryWriter = new BinaryWriter(strm);
			for (int i = 0; i < xform.Size; i++)
			{
				if (useDoubles)
				{
					binaryWriter.Write(xform.Transform[i]);
					num += Native.SizeOfDouble;
				}
				else
				{
					binaryWriter.Write((float)xform.Transform[i]);
					num += Native.SizeOfFloat;
				}
			}
		}
		return num;
	}

	private uint SerializeDrawingAttrsTable(Stream stream, GuidList guidList)
	{
		uint num = 0u;
		uint num2 = 0u;
		if (1 == _drawingAttributesTable.Count)
		{
			DrawingAttributes da = _drawingAttributesTable[0];
			num += SerializationHelper.Encode(stream, 3u);
			using MemoryStream memoryStream = new MemoryStream(16);
			num2 = DrawingAttributeSerializer.EncodeAsISF(da, memoryStream, guidList, 0, fTag: true);
			num += SerializationHelper.Encode(stream, num2);
			uint num3 = Convert.ToUInt32(memoryStream.Position);
			num += num3;
			stream.Write(memoryStream.GetBuffer(), 0, Convert.ToInt32(num3));
			memoryStream.Dispose();
		}
		else
		{
			uint[] array = new uint[_drawingAttributesTable.Count];
			MemoryStream[] array2 = new MemoryStream[_drawingAttributesTable.Count];
			for (int i = 0; i < _drawingAttributesTable.Count; i++)
			{
				DrawingAttributes da2 = _drawingAttributesTable[i];
				array2[i] = new MemoryStream(16);
				array[i] = DrawingAttributeSerializer.EncodeAsISF(da2, array2[i], guidList, 0, fTag: true);
				num2 += SerializationHelper.VarSize(array[i]) + array[i];
			}
			num = SerializationHelper.Encode(stream, 2u);
			num += SerializationHelper.Encode(stream, num2);
			for (int j = 0; j < _drawingAttributesTable.Count; j++)
			{
				_ = _drawingAttributesTable[j];
				num += SerializationHelper.Encode(stream, array[j]);
				uint num4 = Convert.ToUInt32(array2[j].Position);
				num += num4;
				stream.Write(array2[j].GetBuffer(), 0, Convert.ToInt32(num4));
				array2[j].Dispose();
			}
		}
		return num;
	}

	private void BuildTables(GuidList guidList)
	{
		_transformTable.Clear();
		_strokeDescriptorTable.Clear();
		_metricTable.Clear();
		_drawingAttributesTable.Clear();
		int num = 0;
		for (num = 0; num < _coreStrokes.Count; num++)
		{
			Stroke stroke = _coreStrokes[num];
			StrokeSerializer.BuildStrokeDescriptor(stroke, guidList, _strokeLookupTable[stroke], out var strokeDescriptor, out var metricBlock);
			bool flag = false;
			for (int i = 0; i < _strokeDescriptorTable.Count; i++)
			{
				if (strokeDescriptor.IsEqual(_strokeDescriptorTable[i]))
				{
					flag = true;
					_strokeLookupTable[stroke].StrokeDescriptorTableIndex = (uint)i;
					break;
				}
			}
			if (!flag)
			{
				_strokeDescriptorTable.Add(strokeDescriptor);
				_strokeLookupTable[stroke].StrokeDescriptorTableIndex = (uint)(_strokeDescriptorTable.Count - 1);
			}
			flag = false;
			for (int j = 0; j < _metricTable.Count; j++)
			{
				MetricBlock metricBlock2 = _metricTable[j];
				SetType setType = SetType.SubSet;
				if (metricBlock2.CompareMetricBlock(metricBlock, ref setType))
				{
					if (setType == SetType.SuperSet)
					{
						_metricTable[j] = metricBlock;
					}
					flag = true;
					_strokeLookupTable[stroke].MetricDescriptorTableIndex = (uint)j;
					break;
				}
			}
			if (!flag)
			{
				_metricTable.Add(metricBlock);
				_strokeLookupTable[stroke].MetricDescriptorTableIndex = (uint)(_metricTable.Count - 1);
			}
			flag = false;
			TransformDescriptor identityTransformDescriptor = IdentityTransformDescriptor;
			for (int k = 0; k < _transformTable.Count; k++)
			{
				if (identityTransformDescriptor.Compare(_transformTable[k]))
				{
					flag = true;
					_strokeLookupTable[stroke].TransformTableIndex = (uint)k;
					break;
				}
			}
			if (!flag)
			{
				_transformTable.Add(identityTransformDescriptor);
				_strokeLookupTable[stroke].TransformTableIndex = (uint)(_transformTable.Count - 1);
			}
			flag = false;
			DrawingAttributes drawingAttributes = _coreStrokes[num].DrawingAttributes;
			for (int l = 0; l < _drawingAttributesTable.Count; l++)
			{
				if (drawingAttributes.Equals(_drawingAttributesTable[l]))
				{
					flag = true;
					_strokeLookupTable[stroke].DrawingAttributesTableIndex = (uint)l;
					break;
				}
			}
			if (!flag)
			{
				_drawingAttributesTable.Add(drawingAttributes);
				_strokeLookupTable[stroke].DrawingAttributesTableIndex = (uint)(_drawingAttributesTable.Count - 1);
			}
		}
	}

	[Conditional("DEBUG_ISF")]
	private static void ISFDebugTrace(string message)
	{
	}

	internal static string ISFDebugMessage(string debugMessage)
	{
		return SR.IsfOperationFailed;
	}
}
