using System;
using System.Windows;

namespace MS.Internal.Data;

internal class DynamicObjectAccessor
{
	private Type _ownerType;

	private string _propertyName;

	public Type OwnerType => _ownerType;

	public string PropertyName => _propertyName;

	public bool IsReadOnly => false;

	public Type PropertyType => typeof(object);

	protected DynamicObjectAccessor(Type ownerType, string propertyName)
	{
		_ownerType = ownerType;
		_propertyName = propertyName;
	}

	public static string MissingMemberErrorString(object target, string name)
	{
		return SR.Format(SR.PropertyPathNoProperty, target, "Items");
	}
}
