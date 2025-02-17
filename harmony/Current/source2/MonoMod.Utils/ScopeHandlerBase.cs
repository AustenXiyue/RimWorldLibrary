namespace MonoMod.Utils;

internal abstract class ScopeHandlerBase
{
	public abstract void EndScope(object? data);
}
internal abstract class ScopeHandlerBase<T> : ScopeHandlerBase
{
	public sealed override void EndScope(object? data)
	{
		EndScope((T)data);
	}

	public abstract void EndScope(T data);
}
