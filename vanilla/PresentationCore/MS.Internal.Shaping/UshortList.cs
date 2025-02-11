namespace MS.Internal.Shaping;

internal class UshortList
{
	private UshortBuffer _storage;

	private int _index;

	private int _length;

	public ushort this[int index]
	{
		get
		{
			Invariant.Assert(index >= 0 && index < _length, "Index out of range");
			return _storage[_index + index];
		}
		set
		{
			Invariant.Assert(index >= 0 && index < _length, "Index out of range");
			_storage[_index + index] = value;
		}
	}

	public int Length
	{
		get
		{
			return _length;
		}
		set
		{
			_length = value;
		}
	}

	public int Offset => _index;

	internal UshortList(int capacity, int leap)
	{
		Invariant.Assert(capacity >= 0 && leap >= 0, "Invalid parameter");
		_storage = new UshortArray(capacity, leap);
	}

	internal UshortList(ushort[] array)
	{
		Invariant.Assert(array != null, "Invalid parameter");
		_storage = new UshortArray(array);
	}

	internal UshortList(CheckedUShortPointer unsafeArray, int arrayLength)
	{
		_storage = new UnsafeUshortArray(unsafeArray, arrayLength);
		_length = arrayLength;
	}

	public void SetRange(int index, int length)
	{
		Invariant.Assert(length >= 0 && index + length <= _storage.Length, "List out of storage");
		_index = index;
		_length = length;
	}

	public void Insert(int index, int count)
	{
		Invariant.Assert(index <= _length && index >= 0, "Index out of range");
		Invariant.Assert(count > 0, "Invalid argument");
		_storage.Insert(_index + index, count, _index + _length);
		_length += count;
	}

	public void Remove(int index, int count)
	{
		Invariant.Assert(index < _length && index >= 0, "Index out of range");
		Invariant.Assert(count > 0 && index + count <= _length, "Invalid argument");
		_storage.Remove(_index + index, count, _index + _length);
		_length -= count;
	}

	public ushort[] ToArray()
	{
		return _storage.ToArray();
	}

	public ushort[] GetCopy()
	{
		return _storage.GetSubsetCopy(_index, _length);
	}
}
