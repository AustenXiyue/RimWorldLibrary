namespace Iced.Intel.DecoderInternal;

internal readonly struct HandlerOptions
{
	public readonly OpCodeHandler handler;

	public readonly DecoderOptions options;

	public HandlerOptions(OpCodeHandler handler, DecoderOptions options)
	{
		this.handler = handler;
		this.options = options;
	}
}
