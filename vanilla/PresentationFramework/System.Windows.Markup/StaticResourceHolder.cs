namespace System.Windows.Markup;

internal class StaticResourceHolder : StaticResourceExtension
{
	private DeferredResourceReference _prefetchedValue;

	internal override DeferredResourceReference PrefetchedValue => _prefetchedValue;

	internal StaticResourceHolder(object resourceKey, DeferredResourceReference prefetchedValue)
		: base(resourceKey)
	{
		_prefetchedValue = prefetchedValue;
	}
}
