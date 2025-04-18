namespace Iced.Intel.DecoderInternal;

internal readonly struct HandlerInfo
{
	public readonly OpCodeHandler? handler;

	public readonly OpCodeHandler?[]? handlers;

	public HandlerInfo(OpCodeHandler handler)
	{
		this.handler = handler;
		handlers = null;
	}

	public HandlerInfo(OpCodeHandler?[] handlers)
	{
		handler = null;
		this.handlers = handlers;
	}
}
