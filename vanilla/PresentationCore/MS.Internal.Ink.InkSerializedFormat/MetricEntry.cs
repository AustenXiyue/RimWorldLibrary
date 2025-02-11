using System;
using System.IO;
using System.Windows.Input;

namespace MS.Internal.Ink.InkSerializedFormat;

internal class MetricEntry
{
	private static int MAX_METRIC_DATA_BUFF = 24;

	private KnownTagCache.KnownTagIndex _tag;

	private uint _size;

	private MetricEntry _next;

	private byte[] _data = new byte[MAX_METRIC_DATA_BUFF];

	private static MetricEntryList[] _metricEntryOptional;

	public static StylusPointPropertyInfo DefaultXMetric = MetricEntry_Optional[0].PropertyMetrics;

	public static StylusPointPropertyInfo DefaultYMetric = MetricEntry_Optional[1].PropertyMetrics;

	private static KnownTagCache.KnownTagIndex[] MetricEntry_Must = new KnownTagCache.KnownTagIndex[3]
	{
		KnownIdCache.KnownGuidBaseIndex + 14,
		KnownIdCache.KnownGuidBaseIndex + 15,
		KnownIdCache.KnownGuidBaseIndex + 16
	};

	private static KnownTagCache.KnownTagIndex[] MetricEntry_Never = new KnownTagCache.KnownTagIndex[3]
	{
		KnownIdCache.KnownGuidBaseIndex + 3,
		KnownIdCache.KnownGuidBaseIndex + 4,
		KnownIdCache.KnownGuidBaseIndex + 5
	};

	private static StylusPointPropertyInfo DefaultPropertyMetrics = StylusPointPropertyInfoDefaults.DefaultValue;

	public static MetricEntryList[] MetricEntry_Optional
	{
		get
		{
			if (_metricEntryOptional == null)
			{
				_metricEntryOptional = new MetricEntryList[11]
				{
					new MetricEntryList(KnownIdCache.KnownGuidBaseIndex + 0, StylusPointPropertyInfoDefaults.X),
					new MetricEntryList(KnownIdCache.KnownGuidBaseIndex + 1, StylusPointPropertyInfoDefaults.Y),
					new MetricEntryList(KnownIdCache.KnownGuidBaseIndex + 2, StylusPointPropertyInfoDefaults.Z),
					new MetricEntryList(KnownIdCache.KnownGuidBaseIndex + 6, StylusPointPropertyInfoDefaults.NormalPressure),
					new MetricEntryList(KnownIdCache.KnownGuidBaseIndex + 7, StylusPointPropertyInfoDefaults.TangentPressure),
					new MetricEntryList(KnownIdCache.KnownGuidBaseIndex + 8, StylusPointPropertyInfoDefaults.ButtonPressure),
					new MetricEntryList(KnownIdCache.KnownGuidBaseIndex + 9, StylusPointPropertyInfoDefaults.XTiltOrientation),
					new MetricEntryList(KnownIdCache.KnownGuidBaseIndex + 10, StylusPointPropertyInfoDefaults.YTiltOrientation),
					new MetricEntryList(KnownIdCache.KnownGuidBaseIndex + 11, StylusPointPropertyInfoDefaults.AzimuthOrientation),
					new MetricEntryList(KnownIdCache.KnownGuidBaseIndex + 12, StylusPointPropertyInfoDefaults.AltitudeOrientation),
					new MetricEntryList(KnownIdCache.KnownGuidBaseIndex + 13, StylusPointPropertyInfoDefaults.TwistOrientation)
				};
			}
			return _metricEntryOptional;
		}
	}

	public KnownTagCache.KnownTagIndex Tag
	{
		get
		{
			return _tag;
		}
		set
		{
			_tag = value;
		}
	}

	public uint Size => _size;

	public byte[] Data
	{
		get
		{
			return _data;
		}
		set
		{
			if (value.Length > MAX_METRIC_DATA_BUFF)
			{
				_size = (uint)MAX_METRIC_DATA_BUFF;
			}
			else
			{
				_size = (uint)value.Length;
			}
			for (int i = 0; i < (int)_size; i++)
			{
				_data[i] = value[i];
			}
		}
	}

	public MetricEntry Next
	{
		get
		{
			return _next;
		}
		set
		{
			_next = value;
		}
	}

	public bool Compare(MetricEntry metricEntry)
	{
		if (Tag != metricEntry.Tag)
		{
			return false;
		}
		if (Size != metricEntry.Size)
		{
			return false;
		}
		for (int i = 0; i < Size; i++)
		{
			if (Data[i] != metricEntry.Data[i])
			{
				return false;
			}
		}
		return true;
	}

	public void Add(MetricEntry next)
	{
		if (_next == null)
		{
			_next = next;
			return;
		}
		MetricEntry next2 = _next;
		while (next2.Next != null)
		{
			next2 = next2.Next;
		}
		next2.Next = next;
	}

	public void Initialize(StylusPointPropertyInfo originalInfo, StylusPointPropertyInfo defaultInfo)
	{
		_size = 0u;
		using MemoryStream memoryStream = new MemoryStream(_data);
		if (!DoubleUtil.AreClose(originalInfo.Resolution, defaultInfo.Resolution))
		{
			_size += SerializationHelper.SignEncode(memoryStream, originalInfo.Minimum);
			_size += SerializationHelper.SignEncode(memoryStream, originalInfo.Maximum);
			_size += SerializationHelper.Encode(memoryStream, (uint)originalInfo.Unit);
			using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(originalInfo.Resolution);
			_size += 4u;
			return;
		}
		if (originalInfo.Unit != defaultInfo.Unit)
		{
			_size += SerializationHelper.SignEncode(memoryStream, originalInfo.Minimum);
			_size += SerializationHelper.SignEncode(memoryStream, originalInfo.Maximum);
			_size += SerializationHelper.Encode(memoryStream, (uint)originalInfo.Unit);
		}
		else if (originalInfo.Maximum != defaultInfo.Maximum)
		{
			_size += SerializationHelper.SignEncode(memoryStream, originalInfo.Minimum);
			_size += SerializationHelper.SignEncode(memoryStream, originalInfo.Maximum);
		}
		else if (originalInfo.Minimum != defaultInfo.Minimum)
		{
			_size += SerializationHelper.SignEncode(memoryStream, originalInfo.Minimum);
		}
	}

	public MetricEntryType CreateMetricEntry(StylusPointPropertyInfo propertyInfo, KnownTagCache.KnownTagIndex tag)
	{
		uint index = 0u;
		Tag = tag;
		if (IsValidMetricEntry(propertyInfo, Tag, out var metricEntryType, out index))
		{
			switch (metricEntryType)
			{
			case MetricEntryType.Optional:
				Initialize(propertyInfo, MetricEntry_Optional[index].PropertyMetrics);
				break;
			case MetricEntryType.Must:
			case MetricEntryType.Custom:
				Initialize(propertyInfo, DefaultPropertyMetrics);
				break;
			default:
				throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("MetricEntryType was persisted with Never flag which should never happen"));
			}
		}
		return metricEntryType;
	}

	private static bool IsValidMetricEntry(StylusPointPropertyInfo propertyInfo, KnownTagCache.KnownTagIndex tag, out MetricEntryType metricEntryType, out uint index)
	{
		index = 0u;
		if ((uint)tag >= KnownIdCache.CustomGuidBaseIndex)
		{
			metricEntryType = MetricEntryType.Custom;
			if (int.MinValue == propertyInfo.Minimum && int.MaxValue == propertyInfo.Maximum && propertyInfo.Unit == StylusPointPropertyUnit.None && DoubleUtil.AreClose(1.0, propertyInfo.Resolution))
			{
				return false;
			}
			return true;
		}
		for (int i = 0; i < MetricEntry_Never.Length; i++)
		{
			if (MetricEntry_Never[i] == tag)
			{
				metricEntryType = MetricEntryType.Never;
				return false;
			}
		}
		for (int i = 0; i < MetricEntry_Must.Length; i++)
		{
			if (MetricEntry_Must[i] == tag)
			{
				metricEntryType = MetricEntryType.Must;
				if (propertyInfo.Minimum == DefaultPropertyMetrics.Minimum && propertyInfo.Maximum == DefaultPropertyMetrics.Maximum && propertyInfo.Unit == DefaultPropertyMetrics.Unit && DoubleUtil.AreClose(propertyInfo.Resolution, DefaultPropertyMetrics.Resolution))
				{
					return false;
				}
				return true;
			}
		}
		for (int i = 0; i < MetricEntry_Optional.Length; i++)
		{
			if (MetricEntry_Optional[i].Tag == tag)
			{
				metricEntryType = MetricEntryType.Optional;
				if (propertyInfo.Minimum == MetricEntry_Optional[i].PropertyMetrics.Minimum && propertyInfo.Maximum == MetricEntry_Optional[i].PropertyMetrics.Maximum && propertyInfo.Unit == MetricEntry_Optional[i].PropertyMetrics.Unit && DoubleUtil.AreClose(propertyInfo.Resolution, MetricEntry_Optional[i].PropertyMetrics.Resolution))
				{
					return false;
				}
				index = (uint)i;
				return true;
			}
		}
		metricEntryType = MetricEntryType.Must;
		return true;
	}
}
