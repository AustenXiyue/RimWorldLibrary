using System;

namespace MS.Internal.Data;

internal abstract class DynamicIndexerAccessor : DynamicObjectAccessor
{
	protected DynamicIndexerAccessor(Type ownerType, string propertyName)
		: base(ownerType, propertyName)
	{
	}

	public abstract object GetValue(object component, object[] args);

	public abstract void SetValue(object component, object[] args, object value);
}
