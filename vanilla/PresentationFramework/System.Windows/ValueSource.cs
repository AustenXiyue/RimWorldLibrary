namespace System.Windows;

/// <summary>Reports the information returned from <see cref="M:System.Windows.DependencyPropertyHelper.GetValueSource(System.Windows.DependencyObject,System.Windows.DependencyProperty)" />.</summary>
public struct ValueSource
{
	private BaseValueSource _baseValueSource;

	private bool _isExpression;

	private bool _isAnimated;

	private bool _isCoerced;

	private bool _isCurrent;

	/// <summary>Gets a value of the <see cref="T:System.Windows.BaseValueSource" /> enumeration, which reports the source that provided the dependency property system with a value.</summary>
	/// <returns>A value of the enumeration.</returns>
	public BaseValueSource BaseValueSource => _baseValueSource;

	/// <summary>Gets a value that declares whether this value resulted from an evaluated expression. This might be a <see cref="T:System.Windows.Data.BindingExpression" /> supporting a binding, or an internal expression such as those that support the DynamicResource Markup Extension.</summary>
	/// <returns>true if the value came from an evaluated expression; otherwise, false.</returns>
	public bool IsExpression => _isExpression;

	/// <summary>Gets a value that declares whether the property is being animated.</summary>
	/// <returns>true if the property is animated; otherwise, false.</returns>
	public bool IsAnimated => _isAnimated;

	/// <summary>Gets a value that declares whether this value resulted from a <see cref="T:System.Windows.CoerceValueCallback" /> implementation applied to a dependency property.</summary>
	/// <returns>true if the value resulted from a <see cref="T:System.Windows.CoerceValueCallback" /> implementation applied to a dependency property; otherwise, false.</returns>
	public bool IsCoerced => _isCoerced;

	/// <summary>Gets whether the value was set by the <see cref="M:System.Windows.DependencyObject.SetCurrentValue(System.Windows.DependencyProperty,System.Object)" /> method.</summary>
	/// <returns>true if the value was set by the <see cref="M:System.Windows.DependencyObject.SetCurrentValue(System.Windows.DependencyProperty,System.Object)" /> method; otherwise, false.</returns>
	public bool IsCurrent => _isCurrent;

	internal ValueSource(BaseValueSourceInternal source, bool isExpression, bool isAnimated, bool isCoerced, bool isCurrent)
	{
		_baseValueSource = (BaseValueSource)source;
		_isExpression = isExpression;
		_isAnimated = isAnimated;
		_isCoerced = isCoerced;
		_isCurrent = isCurrent;
	}

	/// <summary>Returns the hash code for this <see cref="T:System.Windows.ValueSource" />.</summary>
	/// <returns>A 32-bit unsigned integer hash code.</returns>
	public override int GetHashCode()
	{
		return _baseValueSource.GetHashCode();
	}

	/// <summary>Returns a value indicating whether this <see cref="T:System.Windows.ValueSource" /> is equal to a specified object.</summary>
	/// <returns>true if the provided object is equivalent to the current <see cref="T:System.Windows.ValueSource" />; otherwise, false.</returns>
	/// <param name="o">The object to compare with this <see cref="T:System.Windows.ValueSource" />.</param>
	public override bool Equals(object o)
	{
		if (o is ValueSource valueSource)
		{
			if (_baseValueSource == valueSource._baseValueSource && _isExpression == valueSource._isExpression && _isAnimated == valueSource._isAnimated)
			{
				return _isCoerced == valueSource._isCoerced;
			}
			return false;
		}
		return false;
	}

	/// <summary>Determines whether two <see cref="T:System.Windows.ValueSource" /> instances have the same value.</summary>
	/// <returns>true if the two <see cref="T:System.Windows.ValueSource" /> instances are equivalent; otherwise, false.</returns>
	/// <param name="vs1">The first <see cref="T:System.Windows.ValueSource" /> to compare.</param>
	/// <param name="vs2">The second <see cref="T:System.Windows.ValueSource" /> to compare.</param>
	public static bool operator ==(ValueSource vs1, ValueSource vs2)
	{
		return vs1.Equals(vs2);
	}

	/// <summary>Determines whether two <see cref="T:System.Windows.ValueSource" /> instances do not have the same value.</summary>
	/// <returns>true if the two <see cref="T:System.Windows.ValueSource" /> instances are not equivalent; otherwise, false.</returns>
	/// <param name="vs1">The first <see cref="T:System.Windows.ValueSource" /> to compare.</param>
	/// <param name="vs2">The second <see cref="T:System.Windows.ValueSource" /> to compare.</param>
	public static bool operator !=(ValueSource vs1, ValueSource vs2)
	{
		return !vs1.Equals(vs2);
	}
}
