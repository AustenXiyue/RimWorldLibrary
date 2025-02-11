namespace System.Windows;

/// <summary>Specifies that an attached property has a browsable scope that extends to child elements in the logical tree.</summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class AttachedPropertyBrowsableForChildrenAttribute : AttachedPropertyBrowsableAttribute
{
	private bool _includeDescendants;

	/// <summary>Gets or sets a value that declares whether to use the deep mode for detection of parent elements on the attached property where this  .NET Framework attribute is applied.</summary>
	/// <returns>true if the attached property is browsable for all child elements in the logical tree of the parent element that owns the attached property. false if the attached property is only browsable for immediate child elements of a parent element that owns the attached property. The default is false.</returns>
	public bool IncludeDescendants
	{
		get
		{
			return _includeDescendants;
		}
		set
		{
			_includeDescendants = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.AttachedPropertyBrowsableForChildrenAttribute" /> class.</summary>
	public AttachedPropertyBrowsableForChildrenAttribute()
	{
	}

	/// <summary>Determines whether the current <see cref="T:System.Windows.AttachedPropertyBrowsableForChildrenAttribute" /> .NET Framework attribute is equal to a specified object.</summary>
	/// <returns>true if the specified <see cref="T:System.Windows.AttachedPropertyBrowsableForChildrenAttribute" /> is equal to the current <see cref="T:System.Windows.AttachedPropertyBrowsableForChildrenAttribute" />; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.Windows.AttachedPropertyBrowsableForChildrenAttribute" /> to compare to the current <see cref="T:System.Windows.AttachedPropertyBrowsableForChildrenAttribute" />.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is AttachedPropertyBrowsableForChildrenAttribute attachedPropertyBrowsableForChildrenAttribute))
		{
			return false;
		}
		return _includeDescendants == attachedPropertyBrowsableForChildrenAttribute._includeDescendants;
	}

	/// <summary>Returns the hash code for this <see cref="T:System.Windows.AttachedPropertyBrowsableForChildrenAttribute" /> .NET Framework attribute.</summary>
	/// <returns>An unsigned 32-bit integer value.</returns>
	public override int GetHashCode()
	{
		return _includeDescendants.GetHashCode();
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
		DependencyObject dependencyObject = d;
		Type ownerType = dp.OwnerType;
		do
		{
			dependencyObject = FrameworkElement.GetFrameworkParent(dependencyObject);
			if (dependencyObject != null && ownerType.IsInstanceOfType(dependencyObject))
			{
				return true;
			}
		}
		while (_includeDescendants && dependencyObject != null);
		return false;
	}
}
