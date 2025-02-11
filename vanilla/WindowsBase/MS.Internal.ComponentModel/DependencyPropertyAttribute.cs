using System;
using System.Windows;

namespace MS.Internal.ComponentModel;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class DependencyPropertyAttribute : Attribute
{
	private DependencyProperty _dp;

	private bool _isAttached;

	public override object TypeId => typeof(DependencyPropertyAttribute);

	internal bool IsAttached => _isAttached;

	internal DependencyProperty DependencyProperty => _dp;

	internal DependencyPropertyAttribute(DependencyProperty dependencyProperty, bool isAttached)
	{
		if (dependencyProperty == null)
		{
			throw new ArgumentNullException("dependencyProperty");
		}
		_dp = dependencyProperty;
		_isAttached = isAttached;
	}

	public override bool Equals(object value)
	{
		if (value is DependencyPropertyAttribute dependencyPropertyAttribute && dependencyPropertyAttribute._dp == _dp && dependencyPropertyAttribute._isAttached == _isAttached)
		{
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _dp.GetHashCode();
	}
}
