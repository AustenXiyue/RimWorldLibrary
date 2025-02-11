using System;
using System.IO;
using System.Runtime.InteropServices;
using MS.Internal.PresentationCore;

namespace MS.Internal.FontCache;

[FriendAccessAllowed]
internal struct CheckedPointer
{
	private unsafe void* _pointer;

	private int _size;

	internal unsafe bool IsNull => _pointer == null;

	internal int Size => _size;

	internal unsafe CheckedPointer(void* pointer, int size)
	{
		_pointer = pointer;
		_size = size;
	}

	internal unsafe CheckedPointer(UnmanagedMemoryStream stream)
	{
		_pointer = stream.PositionPointer;
		long length = stream.Length;
		if (length < 0 || length > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException();
		}
		_size = (int)length;
	}

	internal unsafe byte[] ToArray()
	{
		byte[] array = new byte[_size];
		if (_pointer == null)
		{
			throw new ArgumentOutOfRangeException();
		}
		Marshal.Copy((nint)_pointer, array, 0, Size);
		return array;
	}

	internal unsafe void CopyTo(CheckedPointer dest)
	{
		if (_pointer == null)
		{
			throw new ArgumentOutOfRangeException();
		}
		byte* pointer = (byte*)_pointer;
		byte* ptr = (byte*)dest.Probe(0, _size);
		for (int i = 0; i < _size; i++)
		{
			ptr[i] = pointer[i];
		}
	}

	internal unsafe int OffsetOf(void* pointer)
	{
		long num = (byte*)pointer - (byte*)_pointer;
		if (num < 0 || num > _size || _pointer == null || pointer == null)
		{
			throw new ArgumentOutOfRangeException();
		}
		return (int)num;
	}

	internal unsafe int OffsetOf(CheckedPointer pointer)
	{
		return OffsetOf(pointer._pointer);
	}

	public unsafe static CheckedPointer operator +(CheckedPointer rhs, int offset)
	{
		if (offset < 0 || offset > rhs._size || rhs._pointer == null)
		{
			throw new ArgumentOutOfRangeException();
		}
		rhs._pointer = (byte*)rhs._pointer + offset;
		rhs._size -= offset;
		return rhs;
	}

	internal unsafe void* Probe(int offset, int length)
	{
		if (_pointer == null || offset < 0 || offset > _size || offset + length > _size || offset + length < 0)
		{
			throw new ArgumentOutOfRangeException();
		}
		return (byte*)_pointer + offset;
	}

	internal unsafe CheckedPointer CheckedProbe(int offset, int length)
	{
		if (_pointer == null || offset < 0 || offset > _size || offset + length > _size || offset + length < 0)
		{
			throw new ArgumentOutOfRangeException();
		}
		return new CheckedPointer((byte*)_pointer + offset, length);
	}

	internal void SetSize(int newSize)
	{
		_size = newSize;
	}

	internal unsafe bool PointerEquals(CheckedPointer pointer)
	{
		return _pointer == pointer._pointer;
	}

	internal unsafe void WriteBool(bool value)
	{
		*(bool*)Probe(0, 1) = value;
	}

	internal unsafe bool ReadBool()
	{
		return *(bool*)Probe(0, 1);
	}
}
