using System.Runtime.ConstrainedExecution;
using Microsoft.Win32.SafeHandles;

namespace System.Runtime.InteropServices;

/// <summary>Provides a controlled memory buffer that can be used for reading and writing. Attempts to access memory outside the controlled buffer (underruns and overruns) raise exceptions.</summary>
public abstract class SafeBuffer : SafeHandleZeroOrMinusOneIsInvalid, IDisposable
{
	private ulong byte_length;

	private unsafe byte* last_byte;

	private bool inited;

	/// <summary>Gets the size of the buffer, in bytes.</summary>
	/// <returns>The number of bytes in the memory buffer.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="Overload:System.Runtime.InteropServices.SafeBuffer.Initialize" /> method has not been called.</exception>
	[CLSCompliant(false)]
	public ulong ByteLength => byte_length;

	/// <summary>Creates a new instance of the <see cref="T:System.Runtime.InteropServices.SafeBuffer" /> class, and specifies whether the buffer handle is to be reliably released. </summary>
	/// <param name="ownsHandle">true to reliably release the handle during the finalization phase; false to prevent reliable release (not recommended).</param>
	protected SafeBuffer(bool ownsHandle)
		: base(ownsHandle)
	{
	}

	/// <summary>Defines the allocation size of the memory region in bytes. You must call this method before you use the <see cref="T:System.Runtime.InteropServices.SafeBuffer" /> instance.</summary>
	/// <param name="numBytes">The number of bytes in the buffer.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="numBytes" /> is less than zero.-or-<paramref name="numBytes" /> is greater than the available address space.</exception>
	[CLSCompliant(false)]
	public unsafe void Initialize(ulong numBytes)
	{
		if (numBytes == 0L)
		{
			throw new ArgumentOutOfRangeException("numBytes");
		}
		inited = true;
		byte_length = numBytes;
		last_byte = (byte*)(void*)handle + numBytes;
	}

	/// <summary>Specifies the allocation size of the memory buffer by using the specified number of elements and element size. You must call this method before you use the <see cref="T:System.Runtime.InteropServices.SafeBuffer" /> instance.</summary>
	/// <param name="numElements">The number of elements in the buffer.</param>
	/// <param name="sizeOfEachElement">The size of each element in the buffer.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="numElements" /> is less than zero. -or-<paramref name="sizeOfEachElement" /> is less than zero.-or-<paramref name="numElements" /> multiplied by <paramref name="sizeOfEachElement" /> is greater than the available address space.</exception>
	[CLSCompliant(false)]
	public void Initialize(uint numElements, uint sizeOfEachElement)
	{
		Initialize(numElements * sizeOfEachElement);
	}

	/// <summary>Defines the allocation size of the memory region by specifying the number of value types. You must call this method before you use the <see cref="T:System.Runtime.InteropServices.SafeBuffer" /> instance.</summary>
	/// <param name="numElements">The number of elements of the value type to allocate memory for.</param>
	/// <typeparam name="T">The value type to allocate memory for.</typeparam>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="numElements" /> is less than zero.-or-<paramref name="numElements" /> multiplied by the size of each element is greater than the available address space.</exception>
	[CLSCompliant(false)]
	public void Initialize<T>(uint numElements) where T : struct
	{
		Initialize(numElements, (uint)Marshal.SizeOf(typeof(T)));
	}

	/// <summary>Obtains a pointer from a <see cref="T:System.Runtime.InteropServices.SafeBuffer" /> object for a block of memory.</summary>
	/// <param name="pointer">A byte pointer, passed by reference, to receive the pointer from within the <see cref="T:System.Runtime.InteropServices.SafeBuffer" /> object. You must set this pointer to null before you call this method.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="Overload:System.Runtime.InteropServices.SafeBuffer.Initialize" /> method has not been called. </exception>
	[CLSCompliant(false)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public unsafe void AcquirePointer(ref byte* pointer)
	{
		if (!inited)
		{
			throw new InvalidOperationException();
		}
		bool success = false;
		DangerousAddRef(ref success);
		if (success)
		{
			pointer = (byte*)(void*)handle;
		}
	}

	/// <summary>Releases a pointer that was obtained by the <see cref="M:System.Runtime.InteropServices.SafeBuffer.AcquirePointer(System.Byte*@)" /> method.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="Overload:System.Runtime.InteropServices.SafeBuffer.Initialize" /> method has not been called.</exception>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public void ReleasePointer()
	{
		if (!inited)
		{
			throw new InvalidOperationException();
		}
		DangerousRelease();
	}

	/// <summary>Reads a value type from memory at the specified offset.</summary>
	/// <returns>The value type that was read from memory.</returns>
	/// <param name="byteOffset">The location from which to read the value type. You may have to consider alignment issues.</param>
	/// <typeparam name="T">The value type to read.</typeparam>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="Overload:System.Runtime.InteropServices.SafeBuffer.Initialize" /> method has not been called.</exception>
	[CLSCompliant(false)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public unsafe T Read<T>(ulong byteOffset) where T : struct
	{
		if (!inited)
		{
			throw new InvalidOperationException();
		}
		byte* ptr = (byte*)(void*)handle + byteOffset;
		if (ptr >= last_byte || ptr + Marshal.SizeOf(typeof(T)) > last_byte)
		{
			throw new ArgumentException("byteOffset");
		}
		return (T)Marshal.PtrToStructure((IntPtr)ptr, typeof(T));
	}

	/// <summary>Reads the specified number of value types from memory starting at the offset, and writes them into an array starting at the index. </summary>
	/// <param name="byteOffset">The location from which to start reading.</param>
	/// <param name="array">The output array to write to.</param>
	/// <param name="index">The location in the output array to begin writing to.</param>
	/// <param name="count">The number of value types to read from the input array and to write to the output array.</param>
	/// <typeparam name="T">The value type to read.</typeparam>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or-<paramref name="count" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The length of the array minus the index is less than <paramref name="count" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="Overload:System.Runtime.InteropServices.SafeBuffer.Initialize" /> method has not been called.</exception>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	[CLSCompliant(false)]
	public unsafe void ReadArray<T>(ulong byteOffset, T[] array, int index, int count) where T : struct
	{
		if (!inited)
		{
			throw new InvalidOperationException();
		}
		int num = Marshal.SizeOf(typeof(T)) * count;
		byte* ptr = (byte*)(void*)handle + byteOffset;
		if (ptr >= last_byte || ptr + num > last_byte)
		{
			throw new ArgumentException("byteOffset");
		}
		Marshal.copy_from_unmanaged((IntPtr)ptr, index, array, count);
	}

	/// <summary>Writes a value type to memory at the given location.</summary>
	/// <param name="byteOffset">The location at which to start writing. You may have to consider alignment issues.</param>
	/// <param name="value">The value to write.</param>
	/// <typeparam name="T">The value type to write.</typeparam>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="Overload:System.Runtime.InteropServices.SafeBuffer.Initialize" /> method has not been called.</exception>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	[CLSCompliant(false)]
	public unsafe void Write<T>(ulong byteOffset, T value) where T : struct
	{
		if (!inited)
		{
			throw new InvalidOperationException();
		}
		byte* ptr = (byte*)(void*)handle + byteOffset;
		if (ptr >= last_byte || ptr + Marshal.SizeOf(typeof(T)) > last_byte)
		{
			throw new ArgumentException("byteOffset");
		}
		Marshal.StructureToPtr(value, (IntPtr)ptr, fDeleteOld: false);
	}

	/// <summary>Writes the specified number of value types to a memory location by reading bytes starting from the specified location in the input array.</summary>
	/// <param name="byteOffset">The location in memory to write to.</param>
	/// <param name="array">The input array.</param>
	/// <param name="index">The offset in the array to start reading from.</param>
	/// <param name="count">The number of value types to write.</param>
	/// <typeparam name="T">The value type to write.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">The length of the input array minus <paramref name="index" /> is less than <paramref name="count" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="Overload:System.Runtime.InteropServices.SafeBuffer.Initialize" /> method has not been called.</exception>
	[CLSCompliant(false)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public unsafe void WriteArray<T>(ulong byteOffset, T[] array, int index, int count) where T : struct
	{
		if (!inited)
		{
			throw new InvalidOperationException();
		}
		byte* ptr = (byte*)(void*)handle + byteOffset;
		int num = Marshal.SizeOf(typeof(T)) * count;
		if (ptr >= last_byte || ptr + num > last_byte)
		{
			throw new ArgumentException("would overrite");
		}
		Marshal.copy_to_unmanaged(array, index, (IntPtr)ptr, count);
	}
}
