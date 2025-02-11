using System;
using System.Windows;

namespace MS.Internal.ComponentModel;

internal struct PropertyKey : IEquatable<PropertyKey>
{
	internal DependencyProperty DependencyProperty;

	internal Type AttachedType;

	private int _hashCode;

	internal PropertyKey(Type attachedType, DependencyProperty prop)
	{
		DependencyProperty = prop;
		AttachedType = attachedType;
		_hashCode = AttachedType.GetHashCode() ^ DependencyProperty.GetHashCode();
	}

	public override int GetHashCode()
	{
		return _hashCode;
	}

	public override bool Equals(object obj)
	{
		return Equals((PropertyKey)obj);
	}

	public bool Equals(PropertyKey key)
	{
		if (key.AttachedType == AttachedType)
		{
			return key.DependencyProperty == DependencyProperty;
		}
		return false;
	}

	public static bool operator ==(PropertyKey key1, PropertyKey key2)
	{
		if (key1.AttachedType == key2.AttachedType)
		{
			return key1.DependencyProperty == key2.DependencyProperty;
		}
		return false;
	}

	public static bool operator !=(PropertyKey key1, PropertyKey key2)
	{
		if (!(key1.AttachedType != key2.AttachedType))
		{
			return key1.DependencyProperty != key2.DependencyProperty;
		}
		return true;
	}
}
