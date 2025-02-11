using MS.Internal.WindowsBase;

namespace System.ComponentModel;

/// <summary>Defines the direction and the property name to be used as the criteria for sorting a collection.</summary>
public struct SortDescription
{
	private string _propertyName;

	private ListSortDirection _direction;

	private bool _sealed;

	/// <summary>Gets or sets the property name being used as the sorting criteria.</summary>
	/// <returns>The default value is null.</returns>
	public string PropertyName
	{
		get
		{
			return _propertyName;
		}
		set
		{
			if (_sealed)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "SortDescription"));
			}
			_propertyName = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether to sort in ascending or descending order.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.ListSortDirection" /> value to indicate whether to sort in ascending or descending order.</returns>
	public ListSortDirection Direction
	{
		get
		{
			return _direction;
		}
		set
		{
			if (_sealed)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "SortDescription"));
			}
			if (value < ListSortDirection.Ascending || value > ListSortDirection.Descending)
			{
				throw new InvalidEnumArgumentException("value", (int)value, typeof(ListSortDirection));
			}
			_direction = value;
		}
	}

	/// <summary>Gets a value that indicates whether this object is in an immutable state.</summary>
	/// <returns>true if this object is in use; otherwise, false.</returns>
	public bool IsSealed => _sealed;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.SortDescription" /> structure.</summary>
	/// <param name="propertyName">The name of the property to sort the list by.</param>
	/// <param name="direction">The sort order.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="propertyName" /> parameter cannot be null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="propertyName" /> parameter cannot be empty</exception>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">The <paramref name="direction" /> parameter does not specify a valid value.</exception>
	public SortDescription(string propertyName, ListSortDirection direction)
	{
		if (direction != 0 && direction != ListSortDirection.Descending)
		{
			throw new InvalidEnumArgumentException("direction", (int)direction, typeof(ListSortDirection));
		}
		_propertyName = propertyName;
		_direction = direction;
		_sealed = false;
	}

	/// <summary>Compares the specified instance and the current instance of <see cref="T:System.ComponentModel.SortDescription" /> for value equality.</summary>
	/// <returns>true if <paramref name="obj" /> and this instance of <see cref="T:System.ComponentModel.SortDescription" /> have the same values.</returns>
	/// <param name="obj">The <see cref="T:System.ComponentModel.SortDescription" /> instance to compare.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is SortDescription))
		{
			return false;
		}
		return this == (SortDescription)obj;
	}

	/// <summary>Compares two <see cref="T:System.ComponentModel.SortDescription" /> objects for value equality.</summary>
	/// <returns>true if the two objects are equal; otherwise, false.</returns>
	/// <param name="sd1">The first instance to compare.</param>
	/// <param name="sd2">The second instance to compare.</param>
	public static bool operator ==(SortDescription sd1, SortDescription sd2)
	{
		if (sd1.PropertyName == sd2.PropertyName)
		{
			return sd1.Direction == sd2.Direction;
		}
		return false;
	}

	/// <summary>Compares two <see cref="T:System.ComponentModel.SortDescription" /> objects for value inequality.</summary>
	/// <returns>true if the values are not equal; otherwise, false.</returns>
	/// <param name="sd1">The first instance to compare.</param>
	/// <param name="sd2">The second instance to compare.</param>
	public static bool operator !=(SortDescription sd1, SortDescription sd2)
	{
		return !(sd1 == sd2);
	}

	/// <summary>Returns the hash code for this instance of <see cref="T:System.ComponentModel.SortDescription" />.</summary>
	/// <returns>The hash code for this instance of <see cref="T:System.ComponentModel.SortDescription" />.</returns>
	public override int GetHashCode()
	{
		int num = Direction.GetHashCode();
		if (PropertyName != null)
		{
			num = PropertyName.GetHashCode() + num;
		}
		return num;
	}

	internal void Seal()
	{
		_sealed = true;
	}
}
