using System;
using System.Runtime.CompilerServices;
using MonoMod.Logs;

namespace MonoMod.Core.Utils;

internal readonly struct AddressMeaning : IEquatable<AddressMeaning>
{
	public AddressKind Kind { get; }

	public int RelativeToOffset { get; }

	public AddressMeaning(AddressKind kind)
	{
		kind.Validate("kind");
		if (!kind.IsAbsolute())
		{
			throw new ArgumentOutOfRangeException("kind");
		}
		Kind = kind;
		RelativeToOffset = 0;
	}

	public AddressMeaning(AddressKind kind, int relativeOffset)
	{
		kind.Validate("kind");
		if (!kind.IsRelative())
		{
			throw new ArgumentOutOfRangeException("kind");
		}
		if (relativeOffset < 0)
		{
			throw new ArgumentOutOfRangeException("relativeOffset");
		}
		Kind = kind;
		RelativeToOffset = relativeOffset;
	}

	private unsafe static nint DoProcessAddress(AddressKind kind, nint basePtr, int offset, ulong address)
	{
		nint num;
		if (kind.IsAbsolute())
		{
			num = (nint)address;
		}
		else
		{
			long num2 = (kind.Is32Bit() ? Unsafe.As<ulong, int>(ref address) : Unsafe.As<ulong, long>(ref address));
			num = (nint)(basePtr + offset + num2);
		}
		if (kind.IsIndirect())
		{
			num = *(nint*)num;
		}
		return num;
	}

	public nint ProcessAddress(nint basePtr, int offset, ulong address)
	{
		return DoProcessAddress(Kind, basePtr, offset + RelativeToOffset, address);
	}

	public override bool Equals(object? obj)
	{
		if (obj is AddressMeaning other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(AddressMeaning other)
	{
		if (Kind == other.Kind)
		{
			return RelativeToOffset == other.RelativeToOffset;
		}
		return false;
	}

	public override string ToString()
	{
		FormatInterpolatedStringHandler handler = new FormatInterpolatedStringHandler(26, 2);
		handler.AppendLiteral("AddressMeaning(");
		handler.AppendFormatted(Kind.FastToString());
		handler.AppendLiteral(", offset: ");
		handler.AppendFormatted(RelativeToOffset);
		handler.AppendLiteral(")");
		return DebugFormatter.Format(ref handler);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Kind, RelativeToOffset);
	}

	public static bool operator ==(AddressMeaning left, AddressMeaning right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(AddressMeaning left, AddressMeaning right)
	{
		return !(left == right);
	}
}
