using System;
using System.Collections.Generic;
using Iced.Intel;

namespace MonoMod.Core.Utils;

internal static class IcedExtensions
{
	[Obsolete("This method is not supported.", true)]
	public static string FormatInsns(this IList<Instruction> insns)
	{
		throw new NotSupportedException();
	}

	[Obsolete("This method is not supported.", true)]
	public static string FormatInsns(this InstructionList insns)
	{
		throw new NotSupportedException();
	}
}
