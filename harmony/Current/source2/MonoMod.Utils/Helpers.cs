using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using MonoMod.Logs;

namespace MonoMod.Utils;

internal static class Helpers
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Swap<T>(ref T a, ref T b)
	{
		T val = a;
		a = b;
		b = val;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Has<T>(this T value, T flag) where T : struct, Enum
	{
		if (Unsafe.SizeOf<T>() == 8)
		{
			long num = Unsafe.As<T, long>(ref flag);
			return (Unsafe.As<T, long>(ref value) & num) == num;
		}
		if (Unsafe.SizeOf<T>() == 4)
		{
			int num2 = Unsafe.As<T, int>(ref flag);
			return (Unsafe.As<T, int>(ref value) & num2) == num2;
		}
		if (Unsafe.SizeOf<T>() == 2)
		{
			short num3 = Unsafe.As<T, short>(ref flag);
			return (Unsafe.As<T, short>(ref value) & num3) == num3;
		}
		if (Unsafe.SizeOf<T>() == 1)
		{
			byte b = Unsafe.As<T, byte>(ref flag);
			return (Unsafe.As<T, byte>(ref value) & b) == b;
		}
		throw new InvalidOperationException("unknown enum size?");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ThrowIfArgumentNull<T>([NotNull] T? arg, [CallerArgumentExpression("arg")] string name = "")
	{
		if (arg == null)
		{
			ThrowArgumentNull(name);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T ThrowIfNull<T>([NotNull] T? arg, [CallerArgumentExpression("arg")] string name = "")
	{
		if (arg == null)
		{
			ThrowArgumentNull(name);
		}
		return arg;
	}

	public static T EventAdd<T>(ref T? evt, T del) where T : Delegate
	{
		T val;
		T val2;
		do
		{
			val = evt;
			val2 = (T)Delegate.Combine(val, del);
		}
		while (Interlocked.CompareExchange(ref evt, val2, val) != val);
		return val2;
	}

	public static T? EventRemove<T>(ref T? evt, T del) where T : Delegate
	{
		T val;
		T val2;
		do
		{
			val = evt;
			val2 = (T)Delegate.Remove(val, del);
		}
		while (Interlocked.CompareExchange(ref evt, val2, val) != val);
		return val2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Assert([DoesNotReturnIf(false)] bool value, string? message = null, [CallerArgumentExpression("value")] string expr = "")
	{
		if (!value)
		{
			ThrowAssertionFailed(message, expr);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("DEBUG")]
	public static void DAssert([DoesNotReturnIf(false)] bool value, string? message = null, [CallerArgumentExpression("value")] string expr = "")
	{
		if (!value)
		{
			ThrowAssertionFailed(message, expr);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Assert([DoesNotReturnIf(false)] bool value, [InterpolatedStringHandlerArgument("value")] ref AssertionInterpolatedStringHandler message, [CallerArgumentExpression("value")] string expr = "")
	{
		if (!value)
		{
			ThrowAssertionFailed(ref message, expr);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("DEBUG")]
	public static void DAssert([DoesNotReturnIf(false)] bool value, [InterpolatedStringHandlerArgument("value")] ref AssertionInterpolatedStringHandler message, [CallerArgumentExpression("value")] string expr = "")
	{
		if (!value)
		{
			ThrowAssertionFailed(ref message, expr);
		}
	}

	[DoesNotReturn]
	private static void ThrowArgumentNull(string argName)
	{
		throw new ArgumentNullException(argName);
	}

	[DoesNotReturn]
	private static void ThrowAssertionFailed(string? msg, string expr)
	{
		LogLevel logLevel = LogLevel.Assert;
		LogLevel level = logLevel;
		bool isEnabled;
		DebugLogInterpolatedStringHandler message = new DebugLogInterpolatedStringHandler(19, 2, logLevel, out isEnabled);
		if (isEnabled)
		{
			message.AppendLiteral("Assertion failed! ");
			message.AppendFormatted(expr);
			message.AppendLiteral(" ");
			message.AppendFormatted(msg);
		}
		DebugLog.Log("MonoMod.Utils.Assert", level, ref message);
		throw new AssertionFailedException(msg, expr);
	}

	[DoesNotReturn]
	private static void ThrowAssertionFailed(ref AssertionInterpolatedStringHandler message, string expr)
	{
		string text = message.ToStringAndClear();
		LogLevel logLevel = LogLevel.Assert;
		LogLevel level = logLevel;
		bool isEnabled;
		DebugLogInterpolatedStringHandler message2 = new DebugLogInterpolatedStringHandler(19, 2, logLevel, out isEnabled);
		if (isEnabled)
		{
			message2.AppendLiteral("Assertion failed! ");
			message2.AppendFormatted(expr);
			message2.AppendLiteral(" ");
			message2.AppendFormatted(text);
		}
		DebugLog.Log("MonoMod.Utils.Assert", level, ref message2);
		throw new AssertionFailedException(text, expr);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static T GetOrInit<T>(ref T? location, Func<T> init) where T : class
	{
		if (location != null)
		{
			return location;
		}
		return InitializeValue<T, Func<T>>(ref location, (delegate*<Func<T>, T>)(&ILHelpers.TailCallFunc), init);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static T GetOrInitWithLock<T>(ref T? location, object @lock, Func<T> init) where T : class
	{
		if (location != null)
		{
			return location;
		}
		return InitializeValueWithLock<T, Func<T>>(ref location, @lock, (delegate*<Func<T>, T>)(&ILHelpers.TailCallFunc), init);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static T GetOrInit<T>(ref T? location, delegate*<T> init) where T : class
	{
		if (location != null)
		{
			return location;
		}
		return InitializeValue(ref location, (delegate*<IntPtr, T>)(&ILHelpers.TailCallDelegatePtr<T>), (IntPtr)init);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static T GetOrInitWithLock<T>(ref T? location, object @lock, delegate*<T> init) where T : class
	{
		if (location != null)
		{
			return location;
		}
		return InitializeValueWithLock(ref location, @lock, (delegate*<IntPtr, T>)(&ILHelpers.TailCallDelegatePtr<T>), (IntPtr)init);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static T GetOrInit<T, TParam>(ref T? location, delegate*<TParam, T> init, TParam obj) where T : class
	{
		if (location != null)
		{
			return location;
		}
		return InitializeValue(ref location, init, obj);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static T GetOrInitWithLock<T, TParam>(ref T? location, object @lock, delegate*<TParam, T> init, TParam obj) where T : class
	{
		if (location != null)
		{
			return location;
		}
		return InitializeValueWithLock(ref location, @lock, init, obj);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private unsafe static T InitializeValue<T, TParam>(ref T? location, delegate*<TParam, T> init, TParam obj) where T : class
	{
		Interlocked.CompareExchange(ref location, init(obj), null);
		return location;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private unsafe static T InitializeValueWithLock<T, TParam>(ref T? location, object @lock, delegate*<TParam, T> init, TParam obj) where T : class
	{
		lock (@lock)
		{
			if (location != null)
			{
				return location;
			}
			return location = init(obj);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool MaskedSequenceEqual(ReadOnlySpan<byte> first, ReadOnlySpan<byte> second, ReadOnlySpan<byte> mask)
	{
		if (mask.Length < first.Length || mask.Length < second.Length)
		{
			ThrowMaskTooShort();
		}
		if (first.Length == second.Length)
		{
			return MaskedSequenceEqualCore(ref MemoryMarshal.GetReference(first), ref MemoryMarshal.GetReference(second), ref MemoryMarshal.GetReference(mask), (nuint)first.Length);
		}
		return false;
	}

	[DoesNotReturn]
	private static void ThrowMaskTooShort()
	{
		throw new ArgumentException("Mask too short", "mask");
	}

	private unsafe static bool MaskedSequenceEqualCore(ref byte first, ref byte second, ref byte maskBytes, nuint length)
	{
		if (!Unsafe.AreSame(ref first, ref second))
		{
			nint num = 0;
			nint num2 = (nint)length;
			if ((nuint)num2 >= (nuint)sizeof(UIntPtr))
			{
				num2 -= sizeof(UIntPtr);
				while (true)
				{
					nuint num3;
					if ((nuint)num2 > (nuint)num)
					{
						num3 = Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref maskBytes, num));
						if (((nuint)(nint)(nuint)Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref first, num)) & num3) != ((nuint)(nint)(nuint)Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref second, num)) & num3))
						{
							break;
						}
						num += sizeof(UIntPtr);
						continue;
					}
					num3 = Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref maskBytes, num));
					return ((nuint)(nint)(nuint)Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref first, num2)) & num3) == ((nuint)(nint)(nuint)Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref second, num2)) & num3);
				}
				goto IL_00b9;
			}
			while ((nuint)num2 > (nuint)num)
			{
				byte b = Unsafe.AddByteOffset(ref maskBytes, num);
				if ((Unsafe.AddByteOffset(ref first, num) & b) == (Unsafe.AddByteOffset(ref second, num) & b))
				{
					num++;
					continue;
				}
				goto IL_00b9;
			}
		}
		return true;
		IL_00b9:
		return false;
	}

	public static byte[] ReadAllBytes(string path)
	{
		using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1);
		long length = fileStream.Length;
		if (length > int.MaxValue)
		{
			throw new IOException("File is too long (more than 2GB)");
		}
		if (length == 0L)
		{
			return ReadAllBytesUnknownLength(fileStream);
		}
		int num = 0;
		int num2 = (int)length;
		byte[] array = new byte[num2];
		while (num2 > 0)
		{
			int num3 = fileStream.Read(array, num, num2);
			if (num3 == 0)
			{
				throw new IOException("Unexpected end of stream");
			}
			num += num3;
			num2 -= num3;
		}
		return array;
	}

	private static byte[] ReadAllBytesUnknownLength(FileStream fs)
	{
		byte[] array = ArrayPool<byte>.Shared.Rent(256);
		try
		{
			int num = 0;
			while (true)
			{
				if (num == array.Length)
				{
					uint num2 = (uint)(array.Length * 2);
					if (num2 > ArrayEx.MaxLength)
					{
						num2 = (uint)Math.Max(ArrayEx.MaxLength, array.Length + 1);
					}
					byte[] array2 = ArrayPool<byte>.Shared.Rent((int)num2);
					Array.Copy(array, array2, array.Length);
					if (array != null)
					{
						ArrayPool<byte>.Shared.Return(array);
					}
					array = array2;
				}
				int num3 = fs.Read(array, num, array.Length - num);
				if (num3 == 0)
				{
					break;
				}
				num += num3;
			}
			return array.AsSpan(0, num).ToArray();
		}
		finally
		{
			if (array != null)
			{
				ArrayPool<byte>.Shared.Return(array);
			}
		}
	}
}
