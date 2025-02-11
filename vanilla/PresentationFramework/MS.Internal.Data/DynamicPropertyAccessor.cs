using System;

namespace MS.Internal.Data;

internal abstract class DynamicPropertyAccessor : DynamicObjectAccessor
{
	protected DynamicPropertyAccessor(Type ownerType, string propertyName)
		: base(ownerType, propertyName)
	{
	}

	public abstract object GetValue(object component);

	public abstract void SetValue(object component, object value);
}
