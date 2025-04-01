using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace System.Threading;

/// <summary>Contains methods for performing volatile memory operations.</summary>
public static class Volatile
{
	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method. </summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
	/// <param name="location">The field to read.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern bool Read(ref bool location);

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
	/// <param name="location">The field to read.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern byte Read(ref byte location);

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
	/// <param name="location">The field to read.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[CLSCompliant(false)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern sbyte Read(ref sbyte location);

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
	/// <param name="location">The field to read.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern short Read(ref short location);

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache.</returns>
	/// <param name="location">The field to read.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[CLSCompliant(false)]
	public static extern ushort Read(ref ushort location);

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
	/// <param name="location">The field to read.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern int Read(ref int location);

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache.</returns>
	/// <param name="location">The field to read.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[CLSCompliant(false)]
	public static extern uint Read(ref uint location);

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
	/// <param name="location">The field to read.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern long Read(ref long location);

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
	/// <param name="location">The field to read.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[CLSCompliant(false)]
	public static extern ulong Read(ref ulong location);

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
	/// <param name="location">The field to read.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern IntPtr Read(ref IntPtr location);

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
	/// <param name="location">The field to read.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[CLSCompliant(false)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern UIntPtr Read(ref UIntPtr location);

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
	/// <param name="location">The field to read.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern double Read(ref double location);

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
	/// <param name="location">The field to read.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern float Read(ref float location);

	/// <summary>Reads the object reference from the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The reference to <paramref name="T" /> that was read. This reference is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
	/// <param name="location">The field to read.</param>
	/// <typeparam name="T">The type of field to read. This must be a reference type, not a value type.</typeparam>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern T Read<T>(ref T location) where T : class;

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer. </param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern void Write(ref bool location, bool value);

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern void Write(ref byte location, byte value);

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[CLSCompliant(false)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern void Write(ref sbyte location, sbyte value);

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern void Write(ref short location, short value);

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[CLSCompliant(false)]
	public static extern void Write(ref ushort location, ushort value);

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern void Write(ref int location, int value);

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[CLSCompliant(false)]
	public static extern void Write(ref uint location, uint value);

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a memory operation appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern void Write(ref long location, long value);

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[CLSCompliant(false)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern void Write(ref ulong location, ulong value);

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern void Write(ref IntPtr location, IntPtr value);

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[CLSCompliant(false)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern void Write(ref UIntPtr location, UIntPtr value);

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern void Write(ref double location, double value);

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern void Write(ref float location, float value);

	/// <summary>Writes the specified object reference to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method. </summary>
	/// <param name="location">The field where the object reference is written.</param>
	/// <param name="value">The object reference to write. The reference is written immediately so that it is visible to all processors in the computer.</param>
	/// <typeparam name="T">The type of field to write. This must be a reference type, not a value type. </typeparam>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern void Write<T>(ref T location, T value) where T : class;
}
