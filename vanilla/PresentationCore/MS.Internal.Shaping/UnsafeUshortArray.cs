namespace MS.Internal.Shaping;

internal class UnsafeUshortArray : UshortBuffer
{
	private unsafe ushort* _array;

	private MS.Internal.SecurityCriticalDataForSet<int> _arrayLength;

	public unsafe override ushort this[int index]
	{
		get
		{
			Invariant.Assert(index >= 0 && index < _arrayLength.Value);
			return _array[index];
		}
		set
		{
			Invariant.Assert(index >= 0 && index < _arrayLength.Value);
			_array[index] = value;
		}
	}

	public override int Length => _arrayLength.Value;

	internal unsafe UnsafeUshortArray(CheckedUShortPointer array, int arrayLength)
	{
		_array = array.Probe(0, arrayLength);
		_arrayLength.Value = arrayLength;
	}
}
