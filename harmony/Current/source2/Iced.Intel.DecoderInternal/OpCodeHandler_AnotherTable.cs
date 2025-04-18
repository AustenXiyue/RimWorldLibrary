using System;

namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_AnotherTable : OpCodeHandler
{
	private readonly OpCodeHandler[] otherTable;

	public OpCodeHandler_AnotherTable(OpCodeHandler[] otherTable)
	{
		this.otherTable = otherTable ?? throw new ArgumentNullException("otherTable");
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		decoder.DecodeTable(otherTable, ref instruction);
	}
}
