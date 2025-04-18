using System;

namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Options_DontReadModRM : OpCodeHandlerModRM
{
	private readonly OpCodeHandler defaultHandler;

	private readonly HandlerOptions[] infos;

	public OpCodeHandler_Options_DontReadModRM(OpCodeHandler defaultHandler, OpCodeHandler handler1, DecoderOptions options1)
	{
		this.defaultHandler = defaultHandler ?? throw new ArgumentNullException("defaultHandler");
		infos = new HandlerOptions[1]
		{
			new HandlerOptions(handler1, options1)
		};
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		OpCodeHandler handler = defaultHandler;
		DecoderOptions options = decoder.options;
		HandlerOptions[] array = infos;
		for (int i = 0; i < array.Length; i++)
		{
			HandlerOptions handlerOptions = array[i];
			if ((options & handlerOptions.options) != 0)
			{
				handler = handlerOptions.handler;
				break;
			}
		}
		handler.Decode(decoder, ref instruction);
	}
}
