namespace System.Windows;

/// <summary>Specifies that an attached property is browsable only for elements that derive from a specified type.</summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class AttachedPropertyBrowsableForTypeAttribute : AttachedPropertyBrowsableAttribute
{
	private Type _targetType;

	private DependencyObjectType _dTargetType;

	private bool _dTargetTypeChecked;

	/// <summary>Gets the base type that scopes the use of the attached property where this .NET Framework attribute applies.</summary>
	/// <returns>The requested <see cref="T:System.Type" />.</returns>
	public Type TargetType => _targetType;

	/// <summary>Gets a unique type identifier for this <see cref="T:System.Windows.AttachedPropertyBrowsableForTypeAttribute" /> .NET Framework attribute.</summary>
	/// <returns>An object that is a unique identifier for the <see cref="T:System.Windows.AttachedPropertyBrowsableForTypeAttribute" />.</returns>
	public override object TypeId => this;

	internal override bool UnionResults => true;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.AttachedPropertyBrowsableForTypeAttribute" /> class, using the provided <paramref name="targetType" />.</summary>
	/// <param name="targetType">The intended type that scopes the use of the attached property where this .NET Framework attribute applies.</param>
	public AttachedPropertyBrowsableForTypeAttribute(Type targetType)
	{
		if (targetType == null)
		{
			throw new ArgumentNullException("targetType");
		}
		_targetType = targetType;
	}

	/// <summary>Determines whether the current <see cref="T:System.Windows.AttachedPropertyBrowsableForTypeAttribute" /> .NET Framework attribute is equal to a specified object.</summary>
	/// <returns>true if the specified <see cref="T:System.Windows.AttachedPropertyBrowsableForTypeAttribute" /> is equal to the current <see cref="T:System.Windows.AttachedPropertyBrowsableForTypeAttribute" />; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.Windows.AttachedPropertyBrowsableForTypeAttribute" /> to compare to the current <see cref="T:System.Windows.AttachedPropertyBrowsableForTypeAttribute" />.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is AttachedPropertyBrowsableForTypeAttribute attachedPropertyBrowsableForTypeAttribute))
		{
			return false;
		}
		return _targetType == attachedPropertyBrowsableForTypeAttribute._targetType;
	}

	/// <summary>Returns the hash code for this <see cref="T:System.Windows.AttachedPropertyBrowsableForTypeAttribute" /> .NET Framework attribute.</summary>
	/// <returns>An unsigned 32-bit integer value.</returns>
	public override int GetHashCode()
	{
		return _targetType.GetHashCode();
	}

	internal override bool IsBrowsable(DependencyObject d, DependencyProperty dp)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		if (dp == null)
		{
			throw new ArgumentNullException("dp");
		}
		if (!_dTargetTypeChecked)
		{
			try
			{
				_dTargetType = DependencyObjectType.FromSystemType(_targetType);
			}
			catch (ArgumentException)
			{
			}
			_dTargetTypeChecked = true;
		}
		if (_dTargetType != null && _dTargetType.IsInstanceOfType(d))
		{
			return true;
		}
		return false;
	}
}
