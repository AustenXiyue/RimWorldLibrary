namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VEX_VHIs5W : OpCodeHandlerModRM
{
	private readonly Register baseReg;

	private readonly Code code;

	public OpCodeHandler_VEX_VHIs5W(Register baseReg, Code code)
	{
		this.baseReg = baseReg;
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = (Register)((int)(decoder.state.reg + decoder.state.zs.extraRegisterBase) + (int)baseReg);
		instruction.Op1Register = (Register)((int)decoder.state.vvvv + (int)baseReg);
		if (decoder.state.mod == 3)
		{
			instruction.Op3Register = (Register)((int)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase) + (int)baseReg);
		}
		else
		{
			instruction.Op3Kind = OpKind.Memory;
			decoder.ReadOpMem(ref instruction);
		}
		uint num = decoder.ReadByte();
		instruction.Op2Register = (Register)((int)((num >> 4) & decoder.reg15Mask) + (int)baseReg);
		instruction.InternalImmediate8 = num & 0xF;
	}
}
