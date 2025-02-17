using System;
using System.Runtime.CompilerServices;
using MonoMod.Logs;

namespace MonoMod.Core.Utils;

internal static class AddressKindExtensions
{
	public const AddressKind IsAbsoluteField = AddressKind.Abs32;

	public const AddressKind Is64BitField = AddressKind.Rel64;

	public const AddressKind IsPrecodeFixupField = AddressKind.PrecodeFixupThunkRel32;

	public const AddressKind IsIndirectField = AddressKind.Indirect;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsRelative(this AddressKind value)
	{
		return (value & AddressKind.Abs32) == 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsAbsolute(this AddressKind value)
	{
		return !value.IsRelative();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Is32Bit(this AddressKind value)
	{
		return (value & AddressKind.Rel64) == 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Is64Bit(this AddressKind value)
	{
		return !value.Is32Bit();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsPrecodeFixup(this AddressKind value)
	{
		return (value & AddressKind.PrecodeFixupThunkRel32) != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsIndirect(this AddressKind value)
	{
		return (value & AddressKind.Indirect) != 0;
	}

	public static void Validate(this AddressKind value, [CallerArgumentExpression("value")] string argName = "")
	{
		if ((value & ~(AddressKind.PrecodeFixupThunkAbs64 | AddressKind.Indirect)) != 0)
		{
			throw new ArgumentOutOfRangeException(argName);
		}
	}

	public static string FastToString(this AddressKind value)
	{
		FormatInterpolatedStringHandler handler = new FormatInterpolatedStringHandler(0, 4);
		handler.AppendFormatted(value.IsPrecodeFixup() ? "PrecodeFixupThunk" : "");
		handler.AppendFormatted(value.IsRelative() ? "Rel" : "Abs");
		handler.AppendFormatted(value.Is32Bit() ? "32" : "64");
		handler.AppendFormatted(value.IsIndirect() ? "Indirect" : "");
		return DebugFormatter.Format(ref handler);
	}
}
