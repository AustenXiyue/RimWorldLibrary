using System;
using System.IO;
using System.Windows.Input;

namespace MS.Internal.Ink.InkSerializedFormat;

internal class MetricBlock
{
	private MetricEntry _Entry;

	private uint _Count;

	private uint _size;

	public uint MetricEntryCount => _Count;

	public uint Size => _size + SerializationHelper.VarSize(_size);

	public MetricEntry GetMetricEntryList()
	{
		return _Entry;
	}

	public void AddMetricEntry(MetricEntry newEntry)
	{
		if (newEntry == null)
		{
			throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("MetricEntry cannot be null"));
		}
		if (_Entry == null)
		{
			_Entry = newEntry;
		}
		else
		{
			_Entry.Add(newEntry);
		}
		_Count++;
		_size += newEntry.Size + SerializationHelper.VarSize(newEntry.Size) + SerializationHelper.VarSize((uint)newEntry.Tag);
	}

	public MetricEntryType AddMetricEntry(StylusPointPropertyInfo property, KnownTagCache.KnownTagIndex tag)
	{
		MetricEntry metricEntry = new MetricEntry();
		MetricEntryType result = metricEntry.CreateMetricEntry(property, tag);
		if (metricEntry.Size == 0)
		{
			return result;
		}
		MetricEntry metricEntry2 = _Entry;
		if (metricEntry2 == null)
		{
			_Entry = metricEntry;
		}
		else
		{
			while (metricEntry2.Next != null)
			{
				metricEntry2 = metricEntry2.Next;
			}
			metricEntry2.Next = metricEntry;
		}
		_Count++;
		_size += metricEntry.Size + SerializationHelper.VarSize(metricEntry.Size) + SerializationHelper.VarSize((uint)_Entry.Tag);
		return result;
	}

	public uint Pack(Stream strm)
	{
		uint num = 0u;
		num = SerializationHelper.Encode(strm, _size);
		for (MetricEntry metricEntry = _Entry; metricEntry != null; metricEntry = metricEntry.Next)
		{
			num += SerializationHelper.Encode(strm, (uint)metricEntry.Tag);
			num += SerializationHelper.Encode(strm, metricEntry.Size);
			strm.Write(metricEntry.Data, 0, (int)metricEntry.Size);
			num += metricEntry.Size;
		}
		return num;
	}

	public bool CompareMetricBlock(MetricBlock metricColl, ref SetType setType)
	{
		if (metricColl == null)
		{
			return false;
		}
		if (GetMetricEntryList() == null)
		{
			return metricColl.GetMetricEntryList() == null;
		}
		if (metricColl.GetMetricEntryList() == null)
		{
			return false;
		}
		bool flag = false;
		_ = MetricEntryCount;
		_ = metricColl.MetricEntryCount;
		MetricEntry metricEntry;
		MetricEntry metricEntryList;
		if (metricColl.MetricEntryCount <= MetricEntryCount)
		{
			metricEntry = metricColl.GetMetricEntryList();
			metricEntryList = GetMetricEntryList();
		}
		else
		{
			metricEntryList = metricColl.GetMetricEntryList();
			metricEntry = GetMetricEntryList();
			setType = SetType.SuperSet;
		}
		while (metricEntry != null)
		{
			flag = false;
			for (MetricEntry metricEntry2 = metricEntryList; metricEntry2 != null; metricEntry2 = metricEntry2.Next)
			{
				if (metricEntry.Compare(metricEntry2))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
			metricEntry = metricEntry.Next;
		}
		return true;
	}
}
