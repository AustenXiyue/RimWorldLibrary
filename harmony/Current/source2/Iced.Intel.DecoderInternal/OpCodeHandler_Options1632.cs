using System;

namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Options1632 : OpCodeHandler
{
	private readonly OpCodeHandler defaultHandler;

	private readonly HandlerOptions[] infos;

	private readonly DecoderOptions infoOptions;

	public OpCodeHandler_Options1632(OpCodeHandler defaultHandler, OpCodeHandler handler1, DecoderOptions options1)
	{
		this.defaultHandler = defaultHandler ?? throw new ArgumentNullException("defaultHandler");
		infos = new HandlerOptions[1]
		{
			new HandlerOptions(handler1, options1)
		};
		infoOptions = options1;
	}

	public OpCodeHandler_Options1632(OpCodeHandler defaultHandler, OpCodeHandler handler1, DecoderOptions options1, OpCodeHandler handler2, DecoderOptions options2)
	{
		this.defaultHandler = defaultHandler ?? throw new ArgumentNullException("defaultHandler");
		infos = new HandlerOptions[2]
		{
			new HandlerOptions(handler1 ?? throw new ArgumentNullException("handler1"), options1),
			new HandlerOptions(handler2 ?? throw new ArgumentNullException("handler2"), options2)
		};
		infoOptions = options1 | options2;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		OpCodeHandler handler = defaultHandler;
		DecoderOptions options = decoder.options;
		if (!decoder.is64bMode && (decoder.options & infoOptions) != 0)
		{
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
		}
		if (handler.HasModRM)
		{
			decoder.ReadModRM();
		}
		handler.Decode(decoder, ref instruction);
	}
}
