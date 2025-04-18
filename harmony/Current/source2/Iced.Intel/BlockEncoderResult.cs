using System;
using System.Collections.Generic;

namespace Iced.Intel;

internal readonly struct BlockEncoderResult
{
	public readonly ulong RIP;

	public readonly List<RelocInfo>? RelocInfos;

	public readonly uint[] NewInstructionOffsets;

	public readonly ConstantOffsets[] ConstantOffsets;

	internal BlockEncoderResult(ulong rip, List<RelocInfo>? relocInfos, uint[]? newInstructionOffsets, ConstantOffsets[]? constantOffsets)
	{
		RIP = rip;
		RelocInfos = relocInfos;
		NewInstructionOffsets = newInstructionOffsets ?? Array2.Empty<uint>();
		ConstantOffsets = constantOffsets ?? Array2.Empty<ConstantOffsets>();
	}
}
