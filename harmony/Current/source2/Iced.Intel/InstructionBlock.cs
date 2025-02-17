using System;
using System.Collections.Generic;

namespace Iced.Intel;

internal readonly struct InstructionBlock
{
	public readonly CodeWriter CodeWriter;

	public readonly IList<Instruction> Instructions;

	public readonly ulong RIP;

	public InstructionBlock(CodeWriter codeWriter, IList<Instruction> instructions, ulong rip)
	{
		CodeWriter = codeWriter ?? throw new ArgumentNullException("codeWriter");
		Instructions = instructions ?? throw new ArgumentNullException("instructions");
		RIP = rip;
	}
}
