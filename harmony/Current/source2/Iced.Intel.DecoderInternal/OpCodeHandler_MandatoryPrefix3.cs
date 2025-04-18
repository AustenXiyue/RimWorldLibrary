using System;

namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_MandatoryPrefix3 : OpCodeHandlerModRM
{
	private readonly struct Info
	{
		public readonly OpCodeHandler handler;

		public readonly bool mandatoryPrefix;

		public Info(OpCodeHandler handler, bool mandatoryPrefix)
		{
			this.handler = handler;
			this.mandatoryPrefix = mandatoryPrefix;
		}
	}

	private readonly Info[] handlers_reg;

	private readonly Info[] handlers_mem;

	public OpCodeHandler_MandatoryPrefix3(OpCodeHandler handler_reg, OpCodeHandler handler_mem, OpCodeHandler handler66_reg, OpCodeHandler handler66_mem, OpCodeHandler handlerF3_reg, OpCodeHandler handlerF3_mem, OpCodeHandler handlerF2_reg, OpCodeHandler handlerF2_mem, LegacyHandlerFlags flags)
	{
		handlers_reg = new Info[4]
		{
			new Info(handler_reg ?? throw new ArgumentNullException("handler_reg"), (flags & LegacyHandlerFlags.HandlerReg) == 0),
			new Info(handler66_reg ?? throw new ArgumentNullException("handler66_reg"), (flags & LegacyHandlerFlags.Handler66Reg) == 0),
			new Info(handlerF3_reg ?? throw new ArgumentNullException("handlerF3_reg"), (flags & LegacyHandlerFlags.HandlerF3Reg) == 0),
			new Info(handlerF2_reg ?? throw new ArgumentNullException("handlerF2_reg"), (flags & LegacyHandlerFlags.HandlerF2Reg) == 0)
		};
		handlers_mem = new Info[4]
		{
			new Info(handler_mem ?? throw new ArgumentNullException("handler_mem"), (flags & LegacyHandlerFlags.HandlerMem) == 0),
			new Info(handler66_mem ?? throw new ArgumentNullException("handler66_mem"), (flags & LegacyHandlerFlags.Handler66Mem) == 0),
			new Info(handlerF3_mem ?? throw new ArgumentNullException("handlerF3_mem"), (flags & LegacyHandlerFlags.HandlerF3Mem) == 0),
			new Info(handlerF2_mem ?? throw new ArgumentNullException("handlerF2_mem"), (flags & LegacyHandlerFlags.HandlerF2Mem) == 0)
		};
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		Info info = ((decoder.state.mod == 3) ? handlers_reg : handlers_mem)[(uint)decoder.state.zs.mandatoryPrefix];
		if (info.mandatoryPrefix)
		{
			decoder.ClearMandatoryPrefix(ref instruction);
		}
		info.handler.Decode(decoder, ref instruction);
	}
}
