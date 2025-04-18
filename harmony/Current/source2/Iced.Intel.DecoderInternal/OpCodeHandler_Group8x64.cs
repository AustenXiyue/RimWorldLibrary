using System;

namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Group8x64 : OpCodeHandlerModRM
{
	private readonly OpCodeHandler[] tableLow;

	private readonly OpCodeHandler[] tableHigh;

	public OpCodeHandler_Group8x64(OpCodeHandler[] tableLow, OpCodeHandler?[] tableHigh)
	{
		if (tableLow.Length != 8)
		{
			throw new ArgumentOutOfRangeException("tableLow");
		}
		if (tableHigh.Length != 64)
		{
			throw new ArgumentOutOfRangeException("tableHigh");
		}
		this.tableLow = tableLow;
		this.tableHigh = tableHigh;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		OpCodeHandler opCodeHandler = ((decoder.state.mod != 3) ? tableLow[decoder.state.reg] : (tableHigh[decoder.state.modrm & 0x3F] ?? tableLow[decoder.state.reg]));
		opCodeHandler.Decode(decoder, ref instruction);
	}
}
