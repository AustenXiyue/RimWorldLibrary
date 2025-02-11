using MS.Internal.WindowsBase;

namespace System.Windows;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal class ModifiedValue
{
	private class BaseValueWeakReference : WeakReference
	{
		public BaseValueWeakReference(object target)
			: base(target)
		{
		}
	}

	private object _baseValue;

	private object _expressionValue;

	private object _animatedValue;

	private object _coercedValue;

	internal object BaseValue
	{
		get
		{
			if (!(_baseValue is BaseValueWeakReference baseValueWeakReference))
			{
				return _baseValue;
			}
			return baseValueWeakReference.Target;
		}
		set
		{
			_baseValue = value;
		}
	}

	internal object ExpressionValue
	{
		get
		{
			return _expressionValue;
		}
		set
		{
			_expressionValue = value;
		}
	}

	internal object AnimatedValue
	{
		get
		{
			return _animatedValue;
		}
		set
		{
			_animatedValue = value;
		}
	}

	internal object CoercedValue
	{
		get
		{
			return _coercedValue;
		}
		set
		{
			_coercedValue = value;
		}
	}

	internal void SetBaseValue(object value, bool useWeakReference)
	{
		_baseValue = ((useWeakReference && !value.GetType().IsValueType) ? new BaseValueWeakReference(value) : value);
	}
}
