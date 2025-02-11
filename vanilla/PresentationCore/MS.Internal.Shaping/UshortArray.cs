using System;

namespace MS.Internal.Shaping;

internal class UshortArray : UshortBuffer
{
	private ushort[] _array;

	public override ushort this[int index]
	{
		get
		{
			return _array[index];
		}
		set
		{
			_array[index] = value;
		}
	}

	public override int Length => _array.Length;

	internal UshortArray(ushort[] array)
	{
		_array = array;
	}

	internal UshortArray(int capacity, int leap)
	{
		_array = new ushort[capacity];
		_leap = leap;
	}

	public override ushort[] ToArray()
	{
		return _array;
	}

	public override ushort[] GetSubsetCopy(int index, int count)
	{
		ushort[] array = new ushort[count];
		Buffer.BlockCopy(_array, index * 2, array, 0, ((index + count <= _array.Length) ? count : _array.Length) * 2);
		return array;
	}

	public override void Insert(int index, int count, int length)
	{
		int num = length + count;
		if (num > _array.Length)
		{
			Invariant.Assert(_leap > 0, "Growing an ungrowable list!");
			int num2 = num - _array.Length;
			ushort[] array = new ushort[_array.Length + ((num2 - 1) / _leap + 1) * _leap];
			Buffer.BlockCopy(_array, 0, array, 0, index * 2);
			if (index < length)
			{
				Buffer.BlockCopy(_array, index * 2, array, (index + count) * 2, (length - index) * 2);
			}
			_array = array;
		}
		else if (index < length)
		{
			Buffer.BlockCopy(_array, index * 2, _array, (index + count) * 2, (length - index) * 2);
		}
	}

	public override void Remove(int index, int count, int length)
	{
		Buffer.BlockCopy(_array, (index + count) * 2, _array, index * 2, (length - index - count) * 2);
	}
}
