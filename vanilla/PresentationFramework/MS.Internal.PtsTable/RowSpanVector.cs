using System;
using System.Windows.Documents;

namespace MS.Internal.PtsTable;

internal sealed class RowSpanVector
{
	private struct Entry
	{
		internal TableCell Cell;

		internal int Start;

		internal int Range;

		internal int Ttl;
	}

	private Entry[] _entries;

	private int _size;

	private int _index;

	private const int c_defaultCapacity = 8;

	private static TableCell[] s_noCells = Array.Empty<TableCell>();

	internal RowSpanVector()
	{
		_entries = new Entry[8];
		_entries[0].Cell = null;
		_entries[0].Start = 1073741823;
		_entries[0].Range = 1073741823;
		_entries[0].Ttl = int.MaxValue;
		_size = 1;
	}

	internal void Register(TableCell cell)
	{
		int columnIndex = cell.ColumnIndex;
		if (_size == _entries.Length)
		{
			InflateCapacity();
		}
		for (int num = _size - 1; num >= _index; num--)
		{
			_entries[num + 1] = _entries[num];
		}
		_entries[_index].Cell = cell;
		_entries[_index].Start = columnIndex;
		_entries[_index].Range = cell.ColumnSpan;
		_entries[_index].Ttl = cell.RowSpan - 1;
		_size++;
		_index++;
	}

	internal void GetFirstAvailableRange(out int firstAvailableIndex, out int firstOccupiedIndex)
	{
		_index = 0;
		firstAvailableIndex = 0;
		firstOccupiedIndex = _entries[_index].Start;
	}

	internal void GetNextAvailableRange(out int firstAvailableIndex, out int firstOccupiedIndex)
	{
		firstAvailableIndex = _entries[_index].Start + _entries[_index].Range;
		_entries[_index].Ttl--;
		_index++;
		firstOccupiedIndex = _entries[_index].Start;
	}

	internal void GetSpanCells(out TableCell[] cells, out bool isLastRowOfAnySpan)
	{
		cells = s_noCells;
		isLastRowOfAnySpan = false;
		while (_index < _size)
		{
			_entries[_index].Ttl--;
			_index++;
		}
		if (_size <= 1)
		{
			return;
		}
		cells = new TableCell[_size - 1];
		int num = 0;
		int num2 = 0;
		do
		{
			cells[num] = _entries[num].Cell;
			if (_entries[num].Ttl > 0)
			{
				if (num != num2)
				{
					_entries[num2] = _entries[num];
				}
				num2++;
			}
			num++;
		}
		while (num < _size - 1);
		if (num != num2)
		{
			_entries[num2] = _entries[num];
			isLastRowOfAnySpan = true;
		}
		_size = num2 + 1;
	}

	internal bool Empty()
	{
		return _size == 1;
	}

	private void InflateCapacity()
	{
		Entry[] array = new Entry[_entries.Length * 2];
		Array.Copy(_entries, array, _entries.Length);
		_entries = array;
	}
}
