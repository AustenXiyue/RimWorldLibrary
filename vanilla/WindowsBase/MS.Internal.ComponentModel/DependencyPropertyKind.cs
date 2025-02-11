using System;
using System.Windows;

namespace MS.Internal.ComponentModel;

internal class DependencyPropertyKind
{
	private readonly DependencyProperty _dp;

	private readonly Type _targetType;

	private bool _isAttached;

	private bool _isAttachedChecked;

	private bool _isInternal;

	private bool _isInternalChecked;

	private bool _isDirect;

	private bool _isDirectChecked;

	internal bool IsInternal
	{
		get
		{
			if (!_isInternalChecked)
			{
				if (!_isAttached && !_isDirect && DependencyObjectPropertyDescriptor.GetAttachedPropertyMethod(_dp) == null && _dp.OwnerType.GetProperty(_dp.Name, _dp.PropertyType) == null)
				{
					_isInternal = true;
				}
				_isInternalChecked = true;
			}
			return _isInternal;
		}
	}

	internal bool IsAttached
	{
		get
		{
			if (!_isAttachedChecked)
			{
				if (!IsDirect && (_dp.OwnerType == _targetType || _dp.OwnerType.IsAssignableFrom(_targetType) || DependencyProperty.FromName(_dp.Name, _targetType) != _dp) && DependencyObjectPropertyDescriptor.GetAttachedPropertyMethod(_dp) != null)
				{
					_isAttached = true;
				}
				_isAttachedChecked = true;
			}
			return _isAttached;
		}
	}

	internal bool IsDirect
	{
		get
		{
			if (!_isDirectChecked)
			{
				if (!_isAttached && DependencyProperty.FromName(_dp.Name, _targetType) == _dp && _targetType.GetProperty(_dp.Name, _dp.PropertyType) != null)
				{
					_isDirect = true;
					_isAttachedChecked = true;
				}
				_isDirectChecked = true;
			}
			return _isDirect;
		}
	}

	internal DependencyPropertyKind(DependencyProperty dp, Type targetType)
	{
		_dp = dp;
		_targetType = targetType;
	}
}
