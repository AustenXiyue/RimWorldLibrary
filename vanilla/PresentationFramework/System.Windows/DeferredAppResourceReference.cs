using System.Collections;

namespace System.Windows;

internal class DeferredAppResourceReference : DeferredResourceReference
{
	internal DeferredAppResourceReference(ResourceDictionary dictionary, object resourceKey)
		: base(dictionary, resourceKey)
	{
	}

	internal override object GetValue(BaseValueSourceInternal valueSource)
	{
		lock (((ICollection)Application.Current.Resources).SyncRoot)
		{
			return base.GetValue(valueSource);
		}
	}

	internal override Type GetValueType()
	{
		lock (((ICollection)Application.Current.Resources).SyncRoot)
		{
			return base.GetValueType();
		}
	}
}
