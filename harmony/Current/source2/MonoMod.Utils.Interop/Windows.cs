using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MonoMod.Utils.Interop;

internal static class Windows
{
	[Conditional("NEVER")]
	[AttributeUsage(AttributeTargets.All)]
	private sealed class SetsLastSystemErrorAttribute : Attribute
	{
	}

	[Conditional("NEVER")]
	[AttributeUsage(AttributeTargets.All)]
	private sealed class NativeTypeNameAttribute : Attribute
	{
		public NativeTypeNameAttribute(string x)
		{
		}
	}

	public struct SYSTEM_INFO
	{
		[StructLayout(LayoutKind.Explicit)]
		public struct _Anonymous_e__Union
		{
			public struct _Anonymous_e__Struct
			{
				public ushort wProcessorArchitecture;

				public ushort wReserved;
			}

			[FieldOffset(0)]
			public uint dwOemId;

			[FieldOffset(0)]
			public _Anonymous_e__Struct Anonymous;
		}

		public _Anonymous_e__Union Anonymous;

		public uint dwPageSize;

		public unsafe void* lpMinimumApplicationAddress;

		public unsafe void* lpMaximumApplicationAddress;

		public nuint dwActiveProcessorMask;

		public uint dwNumberOfProcessors;

		public uint dwProcessorType;

		public uint dwAllocationGranularity;

		public ushort wProcessorLevel;

		public ushort wProcessorRevision;

		[UnscopedRef]
		public ref uint dwOemId
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return ref Anonymous.dwOemId;
			}
		}

		[UnscopedRef]
		public ref ushort wProcessorArchitecture
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return ref Anonymous.Anonymous.wProcessorArchitecture;
			}
		}

		[UnscopedRef]
		public ref ushort wReserved
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return ref Anonymous.Anonymous.wReserved;
			}
		}
	}

	public readonly struct BOOL : IComparable, IComparable<BOOL>, IEquatable<BOOL>, IFormattable
	{
		public readonly int Value;

		public static BOOL FALSE => new BOOL(0);

		public static BOOL TRUE => new BOOL(1);

		public BOOL(int value)
		{
			Value = value;
		}

		public static bool operator ==(BOOL left, BOOL right)
		{
			return left.Value == right.Value;
		}

		public static bool operator !=(BOOL left, BOOL right)
		{
			return left.Value != right.Value;
		}

		public static bool operator <(BOOL left, BOOL right)
		{
			return left.Value < right.Value;
		}

		public static bool operator <=(BOOL left, BOOL right)
		{
			return left.Value <= right.Value;
		}

		public static bool operator >(BOOL left, BOOL right)
		{
			return left.Value > right.Value;
		}

		public static bool operator >=(BOOL left, BOOL right)
		{
			return left.Value >= right.Value;
		}

		public static implicit operator bool(BOOL value)
		{
			return value.Value != 0;
		}

		public static implicit operator BOOL(bool value)
		{
			return new BOOL(value ? 1 : 0);
		}

		public static bool operator false(BOOL value)
		{
			return value.Value == 0;
		}

		public static bool operator true(BOOL value)
		{
			return value.Value != 0;
		}

		public static implicit operator BOOL(byte value)
		{
			return new BOOL(value);
		}

		public static explicit operator byte(BOOL value)
		{
			return (byte)value.Value;
		}

		public static implicit operator BOOL(short value)
		{
			return new BOOL(value);
		}

		public static explicit operator short(BOOL value)
		{
			return (short)value.Value;
		}

		public static implicit operator BOOL(int value)
		{
			return new BOOL(value);
		}

		public static implicit operator int(BOOL value)
		{
			return value.Value;
		}

		public static explicit operator BOOL(long value)
		{
			return new BOOL((int)value);
		}

		public static implicit operator long(BOOL value)
		{
			return value.Value;
		}

		public static explicit operator BOOL(nint value)
		{
			return new BOOL((int)value);
		}

		public static implicit operator nint(BOOL value)
		{
			return value.Value;
		}

		public static implicit operator BOOL(sbyte value)
		{
			return new BOOL(value);
		}

		public static explicit operator sbyte(BOOL value)
		{
			return (sbyte)value.Value;
		}

		public static implicit operator BOOL(ushort value)
		{
			return new BOOL(value);
		}

		public static explicit operator ushort(BOOL value)
		{
			return (ushort)value.Value;
		}

		public static explicit operator BOOL(uint value)
		{
			return new BOOL((int)value);
		}

		public static explicit operator uint(BOOL value)
		{
			return (uint)value.Value;
		}

		public static explicit operator BOOL(ulong value)
		{
			return new BOOL((int)value);
		}

		public static explicit operator ulong(BOOL value)
		{
			return (ulong)value.Value;
		}

		public static explicit operator BOOL(nuint value)
		{
			return new BOOL((int)value);
		}

		public static explicit operator nuint(BOOL value)
		{
			return (nuint)value.Value;
		}

		public int CompareTo(object? obj)
		{
			if (obj is BOOL other)
			{
				return CompareTo(other);
			}
			if (obj != null)
			{
				throw new ArgumentException("obj is not an instance of BOOL.");
			}
			return 1;
		}

		public int CompareTo(BOOL other)
		{
			return Value.CompareTo(other.Value);
		}

		public override bool Equals(object? obj)
		{
			if (obj is BOOL other)
			{
				return Equals(other);
			}
			return false;
		}

		public bool Equals(BOOL other)
		{
			return Value.Equals(other.Value);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public override string ToString()
		{
			return Value.ToString((IFormatProvider)null);
		}

		public string ToString(string? format, IFormatProvider? formatProvider)
		{
			return Value.ToString(format, formatProvider);
		}
	}

	public readonly struct HANDLE : IComparable, IComparable<HANDLE>, IEquatable<HANDLE>, IFormattable
	{
		public unsafe readonly void* Value;

		public unsafe static HANDLE INVALID_VALUE => new HANDLE((void*)(-1));

		public static HANDLE NULL => new HANDLE(null);

		public unsafe HANDLE(void* value)
		{
			Value = value;
		}

		public unsafe static bool operator ==(HANDLE left, HANDLE right)
		{
			return left.Value == right.Value;
		}

		public unsafe static bool operator !=(HANDLE left, HANDLE right)
		{
			return left.Value != right.Value;
		}

		public unsafe static bool operator <(HANDLE left, HANDLE right)
		{
			return left.Value < right.Value;
		}

		public unsafe static bool operator <=(HANDLE left, HANDLE right)
		{
			return left.Value <= right.Value;
		}

		public unsafe static bool operator >(HANDLE left, HANDLE right)
		{
			return left.Value > right.Value;
		}

		public unsafe static bool operator >=(HANDLE left, HANDLE right)
		{
			return left.Value >= right.Value;
		}

		public unsafe static explicit operator HANDLE(void* value)
		{
			return new HANDLE(value);
		}

		public unsafe static implicit operator void*(HANDLE value)
		{
			return value.Value;
		}

		public unsafe static explicit operator HANDLE(byte value)
		{
			return new HANDLE((void*)value);
		}

		public unsafe static explicit operator byte(HANDLE value)
		{
			return (byte)value.Value;
		}

		public unsafe static explicit operator HANDLE(short value)
		{
			return new HANDLE((void*)value);
		}

		public unsafe static explicit operator short(HANDLE value)
		{
			return (short)value.Value;
		}

		public unsafe static explicit operator HANDLE(int value)
		{
			return new HANDLE((void*)value);
		}

		public unsafe static explicit operator int(HANDLE value)
		{
			return (int)value.Value;
		}

		public unsafe static explicit operator HANDLE(long value)
		{
			return new HANDLE((void*)value);
		}

		public unsafe static explicit operator long(HANDLE value)
		{
			return (long)value.Value;
		}

		public unsafe static explicit operator HANDLE(nint value)
		{
			return new HANDLE((void*)value);
		}

		public unsafe static implicit operator nint(HANDLE value)
		{
			return (nint)value.Value;
		}

		public unsafe static explicit operator HANDLE(sbyte value)
		{
			return new HANDLE((void*)value);
		}

		public unsafe static explicit operator sbyte(HANDLE value)
		{
			return (sbyte)value.Value;
		}

		public unsafe static explicit operator HANDLE(ushort value)
		{
			return new HANDLE((void*)value);
		}

		public unsafe static explicit operator ushort(HANDLE value)
		{
			return (ushort)value.Value;
		}

		public unsafe static explicit operator HANDLE(uint value)
		{
			return new HANDLE((void*)value);
		}

		public unsafe static explicit operator uint(HANDLE value)
		{
			return (uint)value.Value;
		}

		public unsafe static explicit operator HANDLE(ulong value)
		{
			return new HANDLE((void*)value);
		}

		public unsafe static explicit operator ulong(HANDLE value)
		{
			return (ulong)value.Value;
		}

		public unsafe static explicit operator HANDLE(nuint value)
		{
			return new HANDLE((void*)value);
		}

		public unsafe static implicit operator nuint(HANDLE value)
		{
			return (nuint)value.Value;
		}

		public int CompareTo(object? obj)
		{
			if (obj is HANDLE other)
			{
				return CompareTo(other);
			}
			if (obj != null)
			{
				throw new ArgumentException("obj is not an instance of HANDLE.");
			}
			return 1;
		}

		public unsafe int CompareTo(HANDLE other)
		{
			if (sizeof(IntPtr) != 4)
			{
				return ((ulong)Value).CompareTo((ulong)other.Value);
			}
			return ((uint)Value).CompareTo((uint)other.Value);
		}

		public override bool Equals(object? obj)
		{
			if (obj is HANDLE other)
			{
				return Equals(other);
			}
			return false;
		}

		public unsafe bool Equals(HANDLE other)
		{
			return ((UIntPtr)Value).Equals((nuint)other.Value);
		}

		public unsafe override int GetHashCode()
		{
			return ((UIntPtr)Value).GetHashCode();
		}

		public unsafe override string ToString()
		{
			if (sizeof(UIntPtr) != 4)
			{
				return ((ulong)Value).ToString("X16", null);
			}
			return ((uint)Value).ToString("X8", null);
		}

		public unsafe string ToString(string? format, IFormatProvider? formatProvider)
		{
			if (sizeof(IntPtr) != 4)
			{
				return ((ulong)Value).ToString(format, formatProvider);
			}
			return ((uint)Value).ToString(format, formatProvider);
		}
	}

	public readonly struct HMODULE : IComparable, IComparable<HMODULE>, IEquatable<HMODULE>, IFormattable
	{
		public unsafe readonly void* Value;

		public unsafe static HMODULE INVALID_VALUE => new HMODULE((void*)(-1));

		public static HMODULE NULL => new HMODULE(null);

		public unsafe HMODULE(void* value)
		{
			Value = value;
		}

		public unsafe static bool operator ==(HMODULE left, HMODULE right)
		{
			return left.Value == right.Value;
		}

		public unsafe static bool operator !=(HMODULE left, HMODULE right)
		{
			return left.Value != right.Value;
		}

		public unsafe static bool operator <(HMODULE left, HMODULE right)
		{
			return left.Value < right.Value;
		}

		public unsafe static bool operator <=(HMODULE left, HMODULE right)
		{
			return left.Value <= right.Value;
		}

		public unsafe static bool operator >(HMODULE left, HMODULE right)
		{
			return left.Value > right.Value;
		}

		public unsafe static bool operator >=(HMODULE left, HMODULE right)
		{
			return left.Value >= right.Value;
		}

		public unsafe static explicit operator HMODULE(void* value)
		{
			return new HMODULE(value);
		}

		public unsafe static implicit operator void*(HMODULE value)
		{
			return value.Value;
		}

		public static explicit operator HMODULE(HANDLE value)
		{
			return new HMODULE(value);
		}

		public unsafe static implicit operator HANDLE(HMODULE value)
		{
			return new HANDLE(value.Value);
		}

		public unsafe static explicit operator HMODULE(byte value)
		{
			return new HMODULE((void*)value);
		}

		public unsafe static explicit operator byte(HMODULE value)
		{
			return (byte)value.Value;
		}

		public unsafe static explicit operator HMODULE(short value)
		{
			return new HMODULE((void*)value);
		}

		public unsafe static explicit operator short(HMODULE value)
		{
			return (short)value.Value;
		}

		public unsafe static explicit operator HMODULE(int value)
		{
			return new HMODULE((void*)value);
		}

		public unsafe static explicit operator int(HMODULE value)
		{
			return (int)value.Value;
		}

		public unsafe static explicit operator HMODULE(long value)
		{
			return new HMODULE((void*)value);
		}

		public unsafe static explicit operator long(HMODULE value)
		{
			return (long)value.Value;
		}

		public unsafe static explicit operator HMODULE(nint value)
		{
			return new HMODULE((void*)value);
		}

		public unsafe static implicit operator nint(HMODULE value)
		{
			return (nint)value.Value;
		}

		public unsafe static explicit operator HMODULE(sbyte value)
		{
			return new HMODULE((void*)value);
		}

		public unsafe static explicit operator sbyte(HMODULE value)
		{
			return (sbyte)value.Value;
		}

		public unsafe static explicit operator HMODULE(ushort value)
		{
			return new HMODULE((void*)value);
		}

		public unsafe static explicit operator ushort(HMODULE value)
		{
			return (ushort)value.Value;
		}

		public unsafe static explicit operator HMODULE(uint value)
		{
			return new HMODULE((void*)value);
		}

		public unsafe static explicit operator uint(HMODULE value)
		{
			return (uint)value.Value;
		}

		public unsafe static explicit operator HMODULE(ulong value)
		{
			return new HMODULE((void*)value);
		}

		public unsafe static explicit operator ulong(HMODULE value)
		{
			return (ulong)value.Value;
		}

		public unsafe static explicit operator HMODULE(nuint value)
		{
			return new HMODULE((void*)value);
		}

		public unsafe static implicit operator nuint(HMODULE value)
		{
			return (nuint)value.Value;
		}

		public int CompareTo(object? obj)
		{
			if (obj is HMODULE other)
			{
				return CompareTo(other);
			}
			if (obj != null)
			{
				throw new ArgumentException("obj is not an instance of HMODULE.");
			}
			return 1;
		}

		public unsafe int CompareTo(HMODULE other)
		{
			if (sizeof(IntPtr) != 4)
			{
				return ((ulong)Value).CompareTo((ulong)other.Value);
			}
			return ((uint)Value).CompareTo((uint)other.Value);
		}

		public override bool Equals(object? obj)
		{
			if (obj is HMODULE other)
			{
				return Equals(other);
			}
			return false;
		}

		public unsafe bool Equals(HMODULE other)
		{
			return ((UIntPtr)Value).Equals((nuint)other.Value);
		}

		public unsafe override int GetHashCode()
		{
			return ((UIntPtr)Value).GetHashCode();
		}

		public unsafe override string ToString()
		{
			if (sizeof(UIntPtr) != 4)
			{
				return ((ulong)Value).ToString("X16", null);
			}
			return ((uint)Value).ToString("X8", null);
		}

		public unsafe string ToString(string? format, IFormatProvider? formatProvider)
		{
			if (sizeof(IntPtr) != 4)
			{
				return ((ulong)Value).ToString(format, formatProvider);
			}
			return ((uint)Value).ToString(format, formatProvider);
		}
	}

	public const int PROCESSOR_ARCHITECTURE_INTEL = 0;

	public const int PROCESSOR_ARCHITECTURE_MIPS = 1;

	public const int PROCESSOR_ARCHITECTURE_ALPHA = 2;

	public const int PROCESSOR_ARCHITECTURE_PPC = 3;

	public const int PROCESSOR_ARCHITECTURE_SHX = 4;

	public const int PROCESSOR_ARCHITECTURE_ARM = 5;

	public const int PROCESSOR_ARCHITECTURE_IA64 = 6;

	public const int PROCESSOR_ARCHITECTURE_ALPHA64 = 7;

	public const int PROCESSOR_ARCHITECTURE_MSIL = 8;

	public const int PROCESSOR_ARCHITECTURE_AMD64 = 9;

	public const int PROCESSOR_ARCHITECTURE_IA32_ON_WIN64 = 10;

	public const int PROCESSOR_ARCHITECTURE_NEUTRAL = 11;

	public const int PROCESSOR_ARCHITECTURE_ARM64 = 12;

	public const int PROCESSOR_ARCHITECTURE_ARM32_ON_WIN64 = 13;

	public const int PROCESSOR_ARCHITECTURE_IA32_ON_ARM64 = 14;

	public const int PROCESSOR_ARCHITECTURE_UNKNOWN = 65535;

	[DllImport("kernel32", ExactSpelling = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	public unsafe static extern void GetSystemInfo(SYSTEM_INFO* lpSystemInfo);

	[DllImport("kernel32", ExactSpelling = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	public unsafe static extern HMODULE GetModuleHandleW(ushort* lpModuleName);

	[DllImport("kernel32", ExactSpelling = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	public unsafe static extern IntPtr GetProcAddress(HMODULE hModule, sbyte* lpProcName);

	[DllImport("kernel32", ExactSpelling = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	public unsafe static extern HMODULE LoadLibraryW(ushort* lpLibFileName);

	[DllImport("kernel32", ExactSpelling = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	public static extern BOOL FreeLibrary(HMODULE hLibModule);

	[DllImport("kernel32", ExactSpelling = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	public static extern uint GetLastError();
}
