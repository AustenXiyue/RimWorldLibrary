using MS.Internal.WindowsBase;

namespace System.Windows;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal struct EffectiveValueEntry
{
	private object _value;

	private short _propertyIndex;

	private FullValueSource _source;

	public int PropertyIndex
	{
		get
		{
			return _propertyIndex;
		}
		set
		{
			_propertyIndex = (short)value;
		}
	}

	internal object Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
			IsDeferredReference = value is DeferredReference;
		}
	}

	internal BaseValueSourceInternal BaseValueSourceInternal
	{
		get
		{
			return (BaseValueSourceInternal)(_source & FullValueSource.ValueSourceMask);
		}
		set
		{
			_source = (FullValueSource)((int)(_source & (FullValueSource)(-16)) | (int)value);
		}
	}

	internal bool IsDeferredReference
	{
		get
		{
			bool flag = ReadPrivateFlag(FullValueSource.IsPotentiallyADeferredReference);
			if (flag)
			{
				ComputeIsDeferred();
				flag = ReadPrivateFlag(FullValueSource.IsPotentiallyADeferredReference);
			}
			return flag;
		}
		private set
		{
			WritePrivateFlag(FullValueSource.IsPotentiallyADeferredReference, value);
		}
	}

	internal bool IsExpression
	{
		get
		{
			return ReadPrivateFlag(FullValueSource.IsExpression);
		}
		private set
		{
			WritePrivateFlag(FullValueSource.IsExpression, value);
		}
	}

	internal bool IsAnimated
	{
		get
		{
			return ReadPrivateFlag(FullValueSource.IsAnimated);
		}
		private set
		{
			WritePrivateFlag(FullValueSource.IsAnimated, value);
		}
	}

	internal bool IsCoerced
	{
		get
		{
			return ReadPrivateFlag(FullValueSource.IsCoerced);
		}
		private set
		{
			WritePrivateFlag(FullValueSource.IsCoerced, value);
		}
	}

	internal bool HasModifiers => (_source & FullValueSource.ModifiersMask) != 0;

	internal FullValueSource FullValueSource => _source;

	internal bool HasExpressionMarker
	{
		get
		{
			return ReadPrivateFlag(FullValueSource.HasExpressionMarker);
		}
		set
		{
			WritePrivateFlag(FullValueSource.HasExpressionMarker, value);
		}
	}

	internal bool IsCoercedWithCurrentValue
	{
		get
		{
			return ReadPrivateFlag(FullValueSource.IsCoercedWithCurrentValue);
		}
		set
		{
			WritePrivateFlag(FullValueSource.IsCoercedWithCurrentValue, value);
		}
	}

	internal object LocalValue
	{
		get
		{
			if (BaseValueSourceInternal == BaseValueSourceInternal.Local)
			{
				if (!HasModifiers)
				{
					return Value;
				}
				return ModifiedValue.BaseValue;
			}
			return DependencyProperty.UnsetValue;
		}
		set
		{
			if (!HasModifiers)
			{
				Value = value;
			}
			else
			{
				ModifiedValue.BaseValue = value;
			}
		}
	}

	internal ModifiedValue ModifiedValue
	{
		get
		{
			if (_value != null)
			{
				return _value as ModifiedValue;
			}
			return null;
		}
	}

	internal static EffectiveValueEntry CreateDefaultValueEntry(DependencyProperty dp, object value)
	{
		EffectiveValueEntry result = new EffectiveValueEntry(dp, BaseValueSourceInternal.Default);
		result.Value = value;
		return result;
	}

	internal EffectiveValueEntry(DependencyProperty dp)
	{
		_propertyIndex = (short)dp.GlobalIndex;
		_value = null;
		_source = (FullValueSource)0;
	}

	internal EffectiveValueEntry(DependencyProperty dp, BaseValueSourceInternal valueSource)
	{
		_propertyIndex = (short)dp.GlobalIndex;
		_value = DependencyProperty.UnsetValue;
		_source = (FullValueSource)valueSource;
	}

	internal EffectiveValueEntry(DependencyProperty dp, FullValueSource fullValueSource)
	{
		_propertyIndex = (short)dp.GlobalIndex;
		_value = DependencyProperty.UnsetValue;
		_source = fullValueSource;
	}

	internal void SetExpressionValue(object value, object baseValue)
	{
		EnsureModifiedValue().ExpressionValue = value;
		IsExpression = true;
		IsDeferredReference = value is DeferredReference;
	}

	internal void SetAnimatedValue(object value, object baseValue)
	{
		EnsureModifiedValue().AnimatedValue = value;
		IsAnimated = true;
		IsDeferredReference = false;
	}

	internal void SetCoercedValue(object value, object baseValue, bool skipBaseValueChecks, bool coerceWithCurrentValue)
	{
		if (IsCoercedWithCurrentValue)
		{
			baseValue = ModifiedValue.BaseValue;
		}
		EnsureModifiedValue(coerceWithCurrentValue).CoercedValue = value;
		IsCoerced = true;
		IsCoercedWithCurrentValue = coerceWithCurrentValue;
		if (coerceWithCurrentValue)
		{
			IsDeferredReference = value is DeferredReference;
		}
		else
		{
			IsDeferredReference = false;
		}
	}

	internal void ResetAnimatedValue()
	{
		if (IsAnimated)
		{
			ModifiedValue modifiedValue = ModifiedValue;
			modifiedValue.AnimatedValue = null;
			IsAnimated = false;
			if (!HasModifiers)
			{
				Value = modifiedValue.BaseValue;
			}
			else
			{
				ComputeIsDeferred();
			}
		}
	}

	internal void ResetCoercedValue()
	{
		if (IsCoerced)
		{
			ModifiedValue modifiedValue = ModifiedValue;
			modifiedValue.CoercedValue = null;
			IsCoerced = false;
			if (!HasModifiers)
			{
				Value = modifiedValue.BaseValue;
			}
			else
			{
				ComputeIsDeferred();
			}
		}
	}

	internal void ResetValue(object value, bool hasExpressionMarker)
	{
		_source &= FullValueSource.ValueSourceMask;
		_value = value;
		if (hasExpressionMarker)
		{
			HasExpressionMarker = true;
		}
		else
		{
			ComputeIsDeferred();
		}
	}

	internal void RestoreExpressionMarker()
	{
		if (HasModifiers)
		{
			ModifiedValue modifiedValue = ModifiedValue;
			modifiedValue.ExpressionValue = modifiedValue.BaseValue;
			modifiedValue.BaseValue = DependencyObject.ExpressionInAlternativeStore;
			_source |= (FullValueSource)272;
			ComputeIsDeferred();
		}
		else
		{
			object value = Value;
			Value = DependencyObject.ExpressionInAlternativeStore;
			SetExpressionValue(value, DependencyObject.ExpressionInAlternativeStore);
			_source |= FullValueSource.HasExpressionMarker;
		}
	}

	private void ComputeIsDeferred()
	{
		bool isDeferredReference = false;
		if (!HasModifiers)
		{
			isDeferredReference = Value is DeferredReference;
		}
		else if (ModifiedValue != null)
		{
			if (IsCoercedWithCurrentValue)
			{
				isDeferredReference = ModifiedValue.CoercedValue is DeferredReference;
			}
			else if (IsExpression)
			{
				isDeferredReference = ModifiedValue.ExpressionValue is DeferredReference;
			}
		}
		IsDeferredReference = isDeferredReference;
	}

	internal EffectiveValueEntry GetFlattenedEntry(RequestFlags requests)
	{
		if ((_source & (FullValueSource)368) == 0)
		{
			return this;
		}
		if (!HasModifiers)
		{
			EffectiveValueEntry result = default(EffectiveValueEntry);
			result.BaseValueSourceInternal = BaseValueSourceInternal;
			result.PropertyIndex = PropertyIndex;
			return result;
		}
		EffectiveValueEntry result2 = default(EffectiveValueEntry);
		result2.BaseValueSourceInternal = BaseValueSourceInternal;
		result2.PropertyIndex = PropertyIndex;
		result2.IsDeferredReference = IsDeferredReference;
		ModifiedValue modifiedValue = ModifiedValue;
		if (IsCoerced)
		{
			if ((requests & RequestFlags.CoercionBaseValue) == 0)
			{
				result2.Value = modifiedValue.CoercedValue;
			}
			else if (IsCoercedWithCurrentValue)
			{
				result2.Value = modifiedValue.CoercedValue;
			}
			else if (IsAnimated && (requests & RequestFlags.AnimationBaseValue) == 0)
			{
				result2.Value = modifiedValue.AnimatedValue;
			}
			else if (IsExpression)
			{
				result2.Value = modifiedValue.ExpressionValue;
			}
			else
			{
				result2.Value = modifiedValue.BaseValue;
			}
		}
		else if (IsAnimated)
		{
			if ((requests & RequestFlags.AnimationBaseValue) == 0)
			{
				result2.Value = modifiedValue.AnimatedValue;
			}
			else if (IsExpression)
			{
				result2.Value = modifiedValue.ExpressionValue;
			}
			else
			{
				result2.Value = modifiedValue.BaseValue;
			}
		}
		else
		{
			object expressionValue = modifiedValue.ExpressionValue;
			result2.Value = expressionValue;
		}
		return result2;
	}

	internal void SetAnimationBaseValue(object animationBaseValue)
	{
		if (!HasModifiers)
		{
			Value = animationBaseValue;
			return;
		}
		ModifiedValue modifiedValue = ModifiedValue;
		if (IsExpression)
		{
			modifiedValue.ExpressionValue = animationBaseValue;
		}
		else
		{
			modifiedValue.BaseValue = animationBaseValue;
		}
		ComputeIsDeferred();
	}

	internal void SetCoersionBaseValue(object coersionBaseValue)
	{
		if (!HasModifiers)
		{
			Value = coersionBaseValue;
			return;
		}
		ModifiedValue modifiedValue = ModifiedValue;
		if (IsAnimated)
		{
			modifiedValue.AnimatedValue = coersionBaseValue;
		}
		else if (IsExpression)
		{
			modifiedValue.ExpressionValue = coersionBaseValue;
		}
		else
		{
			modifiedValue.BaseValue = coersionBaseValue;
		}
		ComputeIsDeferred();
	}

	private ModifiedValue EnsureModifiedValue(bool useWeakReferenceForBaseValue = false)
	{
		ModifiedValue modifiedValue = null;
		if (_value == null)
		{
			modifiedValue = (ModifiedValue)(_value = new ModifiedValue());
		}
		else
		{
			modifiedValue = _value as ModifiedValue;
			if (modifiedValue == null)
			{
				modifiedValue = new ModifiedValue();
				modifiedValue.SetBaseValue(_value, useWeakReferenceForBaseValue);
				_value = modifiedValue;
			}
		}
		return modifiedValue;
	}

	internal void Clear()
	{
		_propertyIndex = -1;
		_value = null;
		_source = (FullValueSource)0;
	}

	private void WritePrivateFlag(FullValueSource bit, bool value)
	{
		if (value)
		{
			_source |= bit;
		}
		else
		{
			_source &= (FullValueSource)(short)(~(int)bit);
		}
	}

	private bool ReadPrivateFlag(FullValueSource bit)
	{
		return (_source & bit) != 0;
	}
}
