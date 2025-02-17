using System;

namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_PrefixREX : OpCodeHandler
{
	private readonly OpCodeHandler handler;

	private readonly uint rex;

	public OpCodeHandler_PrefixREX(OpCodeHandler handler, uint rex)
	{
		this.handler = handler ?? throw new InvalidOperationException();
		this.rex = rex;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (decoder.is64bMode)
		{
			if ((rex & 8) != 0)
			{
				decoder.state.operandSize = OpSize.Size64;
				decoder.state.zs.flags |= StateFlags.HasRex | StateFlags.W;
			}
			else
			{
				decoder.state.zs.flags |= StateFlags.HasRex;
				decoder.state.zs.flags &= ~StateFlags.W;
				if ((decoder.state.zs.flags & StateFlags.Has66) == 0)
				{
					decoder.state.operandSize = OpSize.Size32;
				}
				else
				{
					decoder.state.operandSize = OpSize.Size16;
				}
			}
			decoder.state.zs.extraRegisterBase = (rex << 1) & 8;
			decoder.state.zs.extraIndexRegisterBase = (rex << 2) & 8;
			decoder.state.zs.extraBaseRegisterBase = (rex << 3) & 8;
			decoder.CallOpCodeHandlerXXTable(ref instruction);
		}
		else
		{
			handler.Decode(decoder, ref instruction);
		}
	}
}
