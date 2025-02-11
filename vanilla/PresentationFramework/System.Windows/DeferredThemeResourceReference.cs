namespace System.Windows;

internal class DeferredThemeResourceReference : DeferredResourceReference
{
	private bool _canCacheAsThemeResource;

	internal DeferredThemeResourceReference(ResourceDictionary dictionary, object resourceKey, bool canCacheAsThemeResource)
		: base(dictionary, resourceKey)
	{
		_canCacheAsThemeResource = canCacheAsThemeResource;
	}

	internal override object GetValue(BaseValueSourceInternal valueSource)
	{
		lock (SystemResources.ThemeDictionaryLock)
		{
			if (base.Dictionary != null)
			{
				object key = Key;
				SystemResources.IsSystemResourcesParsing = true;
				object value;
				bool canCache;
				try
				{
					value = base.Dictionary.GetValue(key, out canCache);
					if (canCache)
					{
						Value = value;
						base.Dictionary = null;
					}
				}
				finally
				{
					SystemResources.IsSystemResourcesParsing = false;
				}
				if ((key is Type || key is ResourceKey) && _canCacheAsThemeResource && canCache)
				{
					SystemResources.CacheResource(key, value, isTraceEnabled: false);
				}
				return value;
			}
			return Value;
		}
	}

	internal override Type GetValueType()
	{
		lock (SystemResources.ThemeDictionaryLock)
		{
			return base.GetValueType();
		}
	}

	internal override void RemoveFromDictionary()
	{
	}
}
