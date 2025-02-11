using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace System.IO;

/// <summary>Provides random access to unmanaged blocks of memory from managed code.</summary>
public class UnmanagedMemoryAccessor : IDisposable
{
	[SecurityCritical]
	private SafeBuffer _buffer;

	private long _offset;

	private long _capacity;

	private FileAccess _access;

	private bool _isOpen;

	private bool _canRead;

	private bool _canWrite;

	/// <summary>Gets the capacity of the accessor.</summary>
	/// <returns>The capacity of the accessor.</returns>
	public long Capacity => _capacity;

	/// <summary>Determines whether the accessor is readable.</summary>
	/// <returns>true if the accessor is readable; otherwise, false. </returns>
	public bool CanRead
	{
		get
		{
			if (_isOpen)
			{
				return _canRead;
			}
			return false;
		}
	}

	/// <summary>Determines whether the accessory is writable.</summary>
	/// <returns>true if the accessor is writable; otherwise, false. </returns>
	public bool CanWrite
	{
		get
		{
			if (_isOpen)
			{
				return _canWrite;
			}
			return false;
		}
	}

	/// <summary>Determines whether the accessor is currently open by a process.</summary>
	/// <returns>true if the accessor is open; otherwise, false. </returns>
	protected bool IsOpen => _isOpen;

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.UnmanagedMemoryAccessor" /> class. </summary>
	protected UnmanagedMemoryAccessor()
	{
		_isOpen = false;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.UnmanagedMemoryAccessor" /> class with a specified buffer, offset, and capacity.</summary>
	/// <param name="buffer">The buffer to contain the accessor.</param>
	/// <param name="offset">The byte at which to start the accessor.</param>
	/// <param name="capacity">The size, in bytes, of memory to allocate.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="offset" /> plus <paramref name="capacity" /> is greater than <paramref name="buffer" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="capacity" /> is less than zero.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="offset" /> plus <paramref name="capacity" /> would wrap around the high end of the address space.</exception>
	[SecuritySafeCritical]
	public UnmanagedMemoryAccessor(SafeBuffer buffer, long offset, long capacity)
	{
		Initialize(buffer, offset, capacity, FileAccess.Read);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.UnmanagedMemoryAccessor" /> class with a specified buffer, offset, capacity, and access right.</summary>
	/// <param name="buffer">The buffer to contain the accessor.</param>
	/// <param name="offset">The byte at which to start the accessor.</param>
	/// <param name="capacity">The size, in bytes, of memory to allocate.</param>
	/// <param name="access">The type of access allowed to the memory. The default is <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.ReadWrite" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="offset" /> plus <paramref name="capacity" /> is greater than <paramref name="buffer" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="capacity" /> is less than zero.-or-<paramref name="access" /> is not a valid <see cref="T:System.IO.MemoryMappedFiles.MemoryMappedFileAccess" /> enumeration value.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="offset" /> plus <paramref name="capacity" /> would wrap around the high end of the address space.</exception>
	[SecuritySafeCritical]
	public UnmanagedMemoryAccessor(SafeBuffer buffer, long offset, long capacity, FileAccess access)
	{
		Initialize(buffer, offset, capacity, access);
	}

	/// <summary>Sets the initial values for the accessor.</summary>
	/// <param name="buffer">The buffer to contain the accessor.</param>
	/// <param name="offset">The byte at which to start the accessor.</param>
	/// <param name="capacity">The size, in bytes, of memory to allocate.</param>
	/// <param name="access">The type of access allowed to the memory. The default is <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.ReadWrite" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="offset" /> plus <paramref name="capacity" /> is greater than <paramref name="buffer" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="capacity" /> is less than zero.-or-<paramref name="access" /> is not a valid <see cref="T:System.IO.MemoryMappedFiles.MemoryMappedFileAccess" /> enumeration value.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="offset" /> plus <paramref name="capacity" /> would wrap around the high end of the address space.</exception>
	[SecuritySafeCritical]
	[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	protected unsafe void Initialize(SafeBuffer buffer, long offset, long capacity, FileAccess access)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("Non-negative number required."));
		}
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("Non-negative number required."));
		}
		if (buffer.ByteLength < (ulong)(offset + capacity))
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and capacity were greater than the size of the view."));
		}
		if (access < FileAccess.Read || access > FileAccess.ReadWrite)
		{
			throw new ArgumentOutOfRangeException("access");
		}
		if (_isOpen)
		{
			throw new InvalidOperationException(Environment.GetResourceString("The method cannot be called twice on the same instance."));
		}
		byte* pointer = null;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			buffer.AcquirePointer(ref pointer);
			if ((nuint)((long)pointer + offset + capacity) < (nuint)pointer)
			{
				throw new ArgumentException(Environment.GetResourceString("The UnmanagedMemoryAccessor capacity and offset would wrap around the high end of the address space."));
			}
		}
		finally
		{
			if (pointer != null)
			{
				buffer.ReleasePointer();
			}
		}
		_offset = offset;
		_buffer = buffer;
		_capacity = capacity;
		_access = access;
		_isOpen = true;
		_canRead = (_access & FileAccess.Read) != 0;
		_canWrite = (_access & FileAccess.Write) != 0;
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.UnmanagedMemoryAccessor" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		_isOpen = false;
	}

	/// <summary>Releases all resources used by the <see cref="T:System.IO.UnmanagedMemoryAccessor" />. </summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Reads a Boolean value from the accessor.</summary>
	/// <returns>true or false.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading. </param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public bool ReadBoolean(long position)
	{
		int sizeOfType = 1;
		EnsureSafeToRead(position, sizeOfType);
		return InternalReadByte(position) != 0;
	}

	/// <summary>Reads a byte value from the accessor.</summary>
	/// <returns>The value that was read.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public byte ReadByte(long position)
	{
		int sizeOfType = 1;
		EnsureSafeToRead(position, sizeOfType);
		return InternalReadByte(position);
	}

	/// <summary>Reads a character from the accessor.</summary>
	/// <returns>The value that was read.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[SecuritySafeCritical]
	public unsafe char ReadChar(long position)
	{
		int sizeOfType = 2;
		EnsureSafeToRead(position, sizeOfType);
		byte* pointer = null;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			_buffer.AcquirePointer(ref pointer);
			pointer += _offset + position;
			return *(char*)pointer;
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Reads a 16-bit integer from the accessor.</summary>
	/// <returns>The value that was read.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[SecuritySafeCritical]
	public unsafe short ReadInt16(long position)
	{
		int sizeOfType = 2;
		EnsureSafeToRead(position, sizeOfType);
		byte* pointer = null;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			_buffer.AcquirePointer(ref pointer);
			pointer += _offset + position;
			return *(short*)pointer;
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Reads a 32-bit integer from the accessor.</summary>
	/// <returns>The value that was read.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[SecuritySafeCritical]
	public unsafe int ReadInt32(long position)
	{
		int sizeOfType = 4;
		EnsureSafeToRead(position, sizeOfType);
		byte* pointer = null;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			_buffer.AcquirePointer(ref pointer);
			pointer += _offset + position;
			return *(int*)pointer;
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Reads a 64-bit integer from the accessor.</summary>
	/// <returns>The value that was read.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[SecuritySafeCritical]
	public unsafe long ReadInt64(long position)
	{
		int sizeOfType = 8;
		EnsureSafeToRead(position, sizeOfType);
		byte* pointer = null;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			_buffer.AcquirePointer(ref pointer);
			pointer += _offset + position;
			return *(long*)pointer;
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Reads a decimal value from the accessor.</summary>
	/// <returns>The value that was read.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.-or-The decimal to read is invalid.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[SecuritySafeCritical]
	public decimal ReadDecimal(long position)
	{
		int sizeOfType = 16;
		EnsureSafeToRead(position, sizeOfType);
		int[] array = new int[4];
		ReadArray(position, array, 0, array.Length);
		return new decimal(array);
	}

	/// <summary>Reads a single-precision floating-point value from the accessor.</summary>
	/// <returns>The value that was read.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[SecuritySafeCritical]
	public unsafe float ReadSingle(long position)
	{
		int sizeOfType = 4;
		EnsureSafeToRead(position, sizeOfType);
		byte* pointer = null;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			_buffer.AcquirePointer(ref pointer);
			pointer += _offset + position;
			return *(float*)pointer;
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Reads a double-precision floating-point value from the accessor.</summary>
	/// <returns>The value that was read.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[SecuritySafeCritical]
	public unsafe double ReadDouble(long position)
	{
		int sizeOfType = 8;
		EnsureSafeToRead(position, sizeOfType);
		byte* pointer = null;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			_buffer.AcquirePointer(ref pointer);
			pointer += _offset + position;
			return *(double*)pointer;
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Reads an 8-bit signed integer from the accessor.</summary>
	/// <returns>The value that was read.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[SecuritySafeCritical]
	[CLSCompliant(false)]
	public unsafe sbyte ReadSByte(long position)
	{
		int sizeOfType = 1;
		EnsureSafeToRead(position, sizeOfType);
		byte* pointer = null;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			_buffer.AcquirePointer(ref pointer);
			pointer += _offset + position;
			return (sbyte)(*pointer);
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Reads an unsigned 16-bit integer from the accessor.</summary>
	/// <returns>The value that was read.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[CLSCompliant(false)]
	[SecuritySafeCritical]
	public unsafe ushort ReadUInt16(long position)
	{
		int sizeOfType = 2;
		EnsureSafeToRead(position, sizeOfType);
		byte* pointer = null;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			_buffer.AcquirePointer(ref pointer);
			pointer += _offset + position;
			return *(ushort*)pointer;
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Reads an unsigned 32-bit integer from the accessor.</summary>
	/// <returns>The value that was read.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[CLSCompliant(false)]
	[SecuritySafeCritical]
	public unsafe uint ReadUInt32(long position)
	{
		int sizeOfType = 4;
		EnsureSafeToRead(position, sizeOfType);
		byte* pointer = null;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			_buffer.AcquirePointer(ref pointer);
			pointer += _offset + position;
			return *(uint*)pointer;
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Reads an unsigned 64-bit integer from the accessor.</summary>
	/// <returns>The value that was read.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[CLSCompliant(false)]
	[SecuritySafeCritical]
	public unsafe ulong ReadUInt64(long position)
	{
		int sizeOfType = 8;
		EnsureSafeToRead(position, sizeOfType);
		byte* pointer = null;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			_buffer.AcquirePointer(ref pointer);
			pointer += _offset + position;
			return *(ulong*)pointer;
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Reads a structure of type <paramref name="T" /> from the accessor into a provided reference.</summary>
	/// <param name="position">The position in the accessor at which to begin reading.</param>
	/// <param name="structure">The structure to contain the read data.</param>
	/// <typeparam name="T">The type of structure.</typeparam>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read in a structure of type <paramref name="T" />.-or-T is a value type that contains one or more reference types.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[SecurityCritical]
	public void Read<T>(long position, out T structure) where T : struct
	{
		if (position < 0)
		{
			throw new ArgumentOutOfRangeException("position", Environment.GetResourceString("Non-negative number required."));
		}
		if (!_isOpen)
		{
			throw new ObjectDisposedException("UnmanagedMemoryAccessor", Environment.GetResourceString("Cannot access a closed accessor."));
		}
		if (!CanRead)
		{
			throw new NotSupportedException(Environment.GetResourceString("Accessor does not support reading."));
		}
		uint num = Marshal.SizeOfType(typeof(T));
		if (position > _capacity - num)
		{
			if (position >= _capacity)
			{
				throw new ArgumentOutOfRangeException("position", Environment.GetResourceString("The position may not be greater or equal to the capacity of the accessor."));
			}
			throw new ArgumentException(Environment.GetResourceString("There are not enough bytes remaining in the accessor to read at this position.", typeof(T).FullName), "position");
		}
		structure = _buffer.Read<T>((ulong)(_offset + position));
	}

	/// <summary>Reads structures of type <paramref name="T" /> from the accessor into an array of type <paramref name="T" />.</summary>
	/// <returns>The number of structures read into <paramref name="array" />. This value can be less than <paramref name="count" /> if there are fewer structures available, or zero if the end of the accessor is reached.</returns>
	/// <param name="position">The number of bytes in the accessor at which to begin reading.</param>
	/// <param name="array">The array to contain the structures read from the accessor.</param>
	/// <param name="offset">The index in <paramref name="array" /> in which to place the first copied structure. </param>
	/// <param name="count">The number of structures of type <paramref name="T" /> to read from the accessor.</param>
	/// <typeparam name="T">The type of structure.</typeparam>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is not large enough to contain <paramref name="count" /> of structures (starting from <paramref name="position" />). </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[SecurityCritical]
	public int ReadArray<T>(long position, T[] array, int offset, int count) where T : struct
	{
		if (array == null)
		{
			throw new ArgumentNullException("array", "Buffer cannot be null.");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("Non-negative number required."));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		if (array.Length - offset < count)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		if (!CanRead)
		{
			if (!_isOpen)
			{
				throw new ObjectDisposedException("UnmanagedMemoryAccessor", Environment.GetResourceString("Cannot access a closed accessor."));
			}
			throw new NotSupportedException(Environment.GetResourceString("Accessor does not support reading."));
		}
		if (position < 0)
		{
			throw new ArgumentOutOfRangeException("position", Environment.GetResourceString("Non-negative number required."));
		}
		uint num = Marshal.AlignedSizeOf<T>();
		if (position >= _capacity)
		{
			throw new ArgumentOutOfRangeException("position", Environment.GetResourceString("The position may not be greater or equal to the capacity of the accessor."));
		}
		int num2 = count;
		long num3 = _capacity - position;
		if (num3 < 0)
		{
			num2 = 0;
		}
		else
		{
			ulong num4 = (ulong)(num * count);
			if ((ulong)num3 < num4)
			{
				num2 = (int)(num3 / num);
			}
		}
		_buffer.ReadArray((ulong)(_offset + position), array, offset, num2);
		return num2;
	}

	/// <summary>Writes a Boolean value into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to write a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public void Write(long position, bool value)
	{
		int sizeOfType = 1;
		EnsureSafeToWrite(position, sizeOfType);
		byte value2 = (byte)(value ? 1u : 0u);
		InternalWrite(position, value2);
	}

	/// <summary>Writes a byte value into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to write a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public void Write(long position, byte value)
	{
		int sizeOfType = 1;
		EnsureSafeToWrite(position, sizeOfType);
		InternalWrite(position, value);
	}

	/// <summary>Writes a character into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to write a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[SecuritySafeCritical]
	public unsafe void Write(long position, char value)
	{
		int sizeOfType = 2;
		EnsureSafeToWrite(position, sizeOfType);
		byte* pointer = null;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			_buffer.AcquirePointer(ref pointer);
			pointer += _offset + position;
			*(char*)pointer = value;
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Writes a 16-bit integer into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to write a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[SecuritySafeCritical]
	public unsafe void Write(long position, short value)
	{
		int sizeOfType = 2;
		EnsureSafeToWrite(position, sizeOfType);
		byte* pointer = null;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			_buffer.AcquirePointer(ref pointer);
			pointer += _offset + position;
			*(short*)pointer = value;
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Writes a 32-bit integer into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to write a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[SecuritySafeCritical]
	public unsafe void Write(long position, int value)
	{
		int sizeOfType = 4;
		EnsureSafeToWrite(position, sizeOfType);
		byte* pointer = null;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			_buffer.AcquirePointer(ref pointer);
			pointer += _offset + position;
			*(int*)pointer = value;
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Writes a 64-bit integer into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after position to write a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[SecuritySafeCritical]
	public unsafe void Write(long position, long value)
	{
		int sizeOfType = 8;
		EnsureSafeToWrite(position, sizeOfType);
		byte* pointer = null;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			_buffer.AcquirePointer(ref pointer);
			pointer += _offset + position;
			*(long*)pointer = value;
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Writes a decimal value into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to write a value.-or-The decimal is invalid.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[SecuritySafeCritical]
	public void Write(long position, decimal value)
	{
		int sizeOfType = 16;
		EnsureSafeToWrite(position, sizeOfType);
		byte[] array = new byte[16];
		decimal.GetBytes(value, array);
		int[] array2 = new int[4];
		int num = array[12] | (array[13] << 8) | (array[14] << 16) | (array[15] << 24);
		int num2 = array[0] | (array[1] << 8) | (array[2] << 16) | (array[3] << 24);
		int num3 = array[4] | (array[5] << 8) | (array[6] << 16) | (array[7] << 24);
		int num4 = array[8] | (array[9] << 8) | (array[10] << 16) | (array[11] << 24);
		array2[0] = num2;
		array2[1] = num3;
		array2[2] = num4;
		array2[3] = num;
		WriteArray(position, array2, 0, array2.Length);
	}

	/// <summary>Writes a Single into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to write a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[SecuritySafeCritical]
	public unsafe void Write(long position, float value)
	{
		int sizeOfType = 4;
		EnsureSafeToWrite(position, sizeOfType);
		byte* pointer = null;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			_buffer.AcquirePointer(ref pointer);
			pointer += _offset + position;
			*(float*)pointer = value;
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Writes a Double value into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to write a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[SecuritySafeCritical]
	public unsafe void Write(long position, double value)
	{
		int sizeOfType = 8;
		EnsureSafeToWrite(position, sizeOfType);
		byte* pointer = null;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			_buffer.AcquirePointer(ref pointer);
			pointer += _offset + position;
			*(double*)pointer = value;
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Writes an 8-bit integer into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to write a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[SecuritySafeCritical]
	[CLSCompliant(false)]
	public unsafe void Write(long position, sbyte value)
	{
		int sizeOfType = 1;
		EnsureSafeToWrite(position, sizeOfType);
		byte* pointer = null;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			_buffer.AcquirePointer(ref pointer);
			pointer += _offset + position;
			*pointer = (byte)value;
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Writes an unsigned 16-bit integer into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to write a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[SecuritySafeCritical]
	[CLSCompliant(false)]
	public unsafe void Write(long position, ushort value)
	{
		int sizeOfType = 2;
		EnsureSafeToWrite(position, sizeOfType);
		byte* pointer = null;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			_buffer.AcquirePointer(ref pointer);
			pointer += _offset + position;
			*(ushort*)pointer = value;
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Writes an unsigned 32-bit integer into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to write a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[SecuritySafeCritical]
	[CLSCompliant(false)]
	public unsafe void Write(long position, uint value)
	{
		int sizeOfType = 4;
		EnsureSafeToWrite(position, sizeOfType);
		byte* pointer = null;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			_buffer.AcquirePointer(ref pointer);
			pointer += _offset + position;
			*(uint*)pointer = value;
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Writes an unsigned 64-bit integer into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to write a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[SecuritySafeCritical]
	[CLSCompliant(false)]
	public unsafe void Write(long position, ulong value)
	{
		int sizeOfType = 8;
		EnsureSafeToWrite(position, sizeOfType);
		byte* pointer = null;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			_buffer.AcquirePointer(ref pointer);
			pointer += _offset + position;
			*(ulong*)pointer = value;
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Writes a structure into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="structure">The structure to write.</param>
	/// <typeparam name="T">The type of structure.</typeparam>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes in the accessor after <paramref name="position" /> to write a structure of type <paramref name="T" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[SecurityCritical]
	public void Write<T>(long position, ref T structure) where T : struct
	{
		if (position < 0)
		{
			throw new ArgumentOutOfRangeException("position", Environment.GetResourceString("Non-negative number required."));
		}
		if (!_isOpen)
		{
			throw new ObjectDisposedException("UnmanagedMemoryAccessor", Environment.GetResourceString("Cannot access a closed accessor."));
		}
		if (!CanWrite)
		{
			throw new NotSupportedException(Environment.GetResourceString("Accessor does not support writing."));
		}
		uint num = Marshal.SizeOfType(typeof(T));
		if (position > _capacity - num)
		{
			if (position >= _capacity)
			{
				throw new ArgumentOutOfRangeException("position", Environment.GetResourceString("The position may not be greater or equal to the capacity of the accessor."));
			}
			throw new ArgumentException(Environment.GetResourceString("There are not enough bytes remaining in the accessor to write at this position.", typeof(T).FullName), "position");
		}
		_buffer.Write((ulong)(_offset + position), structure);
	}

	/// <summary>Writes structures from an array of type <paramref name="T" /> into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="array">The array to write into the accessor.</param>
	/// <param name="offset">The index in <paramref name="array" /> to start writing from.</param>
	/// <param name="count">The number of structures in <paramref name="array" /> to write.</param>
	/// <typeparam name="T">The type of structure.</typeparam>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes in the accessor after <paramref name="position" /> to write the number of structures specified by <paramref name="count" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.-or-<paramref name="offset" /> or <paramref name="count" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[SecurityCritical]
	public void WriteArray<T>(long position, T[] array, int offset, int count) where T : struct
	{
		if (array == null)
		{
			throw new ArgumentNullException("array", "Buffer cannot be null.");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("Non-negative number required."));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		if (array.Length - offset < count)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		if (position < 0)
		{
			throw new ArgumentOutOfRangeException("position", Environment.GetResourceString("Non-negative number required."));
		}
		if (position >= Capacity)
		{
			throw new ArgumentOutOfRangeException("position", Environment.GetResourceString("The position may not be greater or equal to the capacity of the accessor."));
		}
		if (!_isOpen)
		{
			throw new ObjectDisposedException("UnmanagedMemoryAccessor", Environment.GetResourceString("Cannot access a closed accessor."));
		}
		if (!CanWrite)
		{
			throw new NotSupportedException(Environment.GetResourceString("Accessor does not support writing."));
		}
		_buffer.WriteArray((ulong)(_offset + position), array, offset, count);
	}

	[SecuritySafeCritical]
	private unsafe byte InternalReadByte(long position)
	{
		byte* pointer = null;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			_buffer.AcquirePointer(ref pointer);
			return (pointer + _offset)[position];
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	[SecuritySafeCritical]
	private unsafe void InternalWrite(long position, byte value)
	{
		byte* pointer = null;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			_buffer.AcquirePointer(ref pointer);
			(pointer + _offset)[position] = value;
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	private void EnsureSafeToRead(long position, int sizeOfType)
	{
		if (!_isOpen)
		{
			throw new ObjectDisposedException("UnmanagedMemoryAccessor", Environment.GetResourceString("Cannot access a closed accessor."));
		}
		if (!CanRead)
		{
			throw new NotSupportedException(Environment.GetResourceString("Accessor does not support reading."));
		}
		if (position < 0)
		{
			throw new ArgumentOutOfRangeException("position", Environment.GetResourceString("Non-negative number required."));
		}
		if (position > _capacity - sizeOfType)
		{
			if (position >= _capacity)
			{
				throw new ArgumentOutOfRangeException("position", Environment.GetResourceString("The position may not be greater or equal to the capacity of the accessor."));
			}
			throw new ArgumentException(Environment.GetResourceString("There are not enough bytes remaining in the accessor to read at this position."), "position");
		}
	}

	private void EnsureSafeToWrite(long position, int sizeOfType)
	{
		if (!_isOpen)
		{
			throw new ObjectDisposedException("UnmanagedMemoryAccessor", Environment.GetResourceString("Cannot access a closed accessor."));
		}
		if (!CanWrite)
		{
			throw new NotSupportedException(Environment.GetResourceString("Accessor does not support writing."));
		}
		if (position < 0)
		{
			throw new ArgumentOutOfRangeException("position", Environment.GetResourceString("Non-negative number required."));
		}
		if (position > _capacity - sizeOfType)
		{
			if (position >= _capacity)
			{
				throw new ArgumentOutOfRangeException("position", Environment.GetResourceString("The position may not be greater or equal to the capacity of the accessor."));
			}
			throw new ArgumentException(Environment.GetResourceString("There are not enough bytes remaining in the accessor to write at this position.", "Byte"), "position");
		}
	}
}
