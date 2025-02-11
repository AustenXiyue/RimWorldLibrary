using System;
using System.Windows;

namespace MS.Internal;

internal struct UncommonValueTable
{
	private object[] _table;

	private uint _bitmask;

	public bool HasValue(int id)
	{
		return (_bitmask & (uint)(1 << id)) != 0;
	}

	public object GetValue(int id)
	{
		return GetValue(id, DependencyProperty.UnsetValue);
	}

	public object GetValue(int id, object defaultValue)
	{
		int num = IndexOf(id);
		if (num >= 0)
		{
			return _table[num];
		}
		return defaultValue;
	}

	public void SetValue(int id, object value)
	{
		int num = Find(id);
		if (num < 0)
		{
			if (_table == null)
			{
				_table = new object[1];
				num = 0;
			}
			else
			{
				int num2 = _table.Length;
				object[] array = new object[num2 + 1];
				num = ~num;
				Array.Copy(_table, 0, array, 0, num);
				Array.Copy(_table, num, array, num + 1, num2 - num);
				_table = array;
			}
			_bitmask |= (uint)(1 << id);
		}
		_table[num] = value;
	}

	public void ClearValue(int id)
	{
		int num = Find(id);
		if (num >= 0)
		{
			int num2 = _table.Length - 1;
			if (num2 == 0)
			{
				_table = null;
			}
			else
			{
				object[] array = new object[num2];
				Array.Copy(_table, 0, array, 0, num);
				Array.Copy(_table, num + 1, array, num, num2 - num);
				_table = array;
			}
			_bitmask &= (uint)(~(1 << id));
		}
	}

	private int IndexOf(int id)
	{
		if (!HasValue(id))
		{
			return -1;
		}
		return GetIndex(id);
	}

	private int Find(int id)
	{
		int num = GetIndex(id);
		if (!HasValue(id))
		{
			num = ~num;
		}
		return num;
	}

	private int GetIndex(int id)
	{
		uint num = _bitmask << 31 - id << 1;
		num -= (num >> 1) & 0x55555555;
		num = (num & 0x33333333) + ((num >> 2) & 0x33333333);
		num = (num + (num >> 4)) & 0xF0F0F0F;
		return (int)(num * 16843009 >> 24);
	}
}
