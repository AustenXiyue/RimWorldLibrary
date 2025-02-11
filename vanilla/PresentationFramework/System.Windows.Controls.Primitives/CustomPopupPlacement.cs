namespace System.Windows.Controls.Primitives;

/// <summary>Defines custom placement parameters for a <see cref="T:System.Windows.Controls.Primitives.Popup" /> control.</summary>
public struct CustomPopupPlacement
{
	private Point _point;

	private PopupPrimaryAxis _primaryAxis;

	/// <summary>Gets or sets the point that is relative to the target object where the upper-left corner of the <see cref="T:System.Windows.Controls.Primitives.Popup" /> control is placedl. </summary>
	/// <returns>A <see cref="T:System.Windows.Point" /> that is used to position a <see cref="T:System.Windows.Controls.Primitives.Popup" /> control. The default value is (0,0).</returns>
	public Point Point
	{
		get
		{
			return _point;
		}
		set
		{
			_point = value;
		}
	}

	/// <summary>Gets or sets the direction in which to move a <see cref="T:System.Windows.Controls.Primitives.Popup" /> control when the <see cref="T:System.Windows.Controls.Primitives.Popup" /> is obscured by screen boundaries.</summary>
	/// <returns>The direction in which to move a <see cref="T:System.Windows.Controls.Primitives.Popup" /> control when the <see cref="T:System.Windows.Controls.Primitives.Popup" /> is obscured by screen boundaries.</returns>
	public PopupPrimaryAxis PrimaryAxis
	{
		get
		{
			return _primaryAxis;
		}
		set
		{
			_primaryAxis = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.CustomPopupPlacement" /> structure.</summary>
	/// <param name="point">The <see cref="T:System.Windows.Point" /> that is relative to the <see cref="P:System.Windows.Controls.Primitives.Popup.PlacementTarget" /> where the upper-left corner of the <see cref="T:System.Windows.Controls.Primitives.Popup" /> is placed.</param>
	/// <param name="primaryAxis">The <see cref="T:System.Windows.Controls.Primitives.PopupPrimaryAxis" /> along which a <see cref="T:System.Windows.Controls.Primitives.Popup" /> control moves when it is obstructed by a screen edge.</param>
	public CustomPopupPlacement(Point point, PopupPrimaryAxis primaryAxis)
	{
		_point = point;
		_primaryAxis = primaryAxis;
	}

	/// <summary>Compares two <see cref="T:System.Windows.Controls.Primitives.CustomPopupPlacement" /> structures to determine whether they are equal.</summary>
	/// <returns>true if the structures have the same values; otherwise, false.</returns>
	/// <param name="placement1">The first <see cref="T:System.Windows.Controls.Primitives.CustomPopupPlacement" /> structure to compare.</param>
	/// <param name="placement2">The second <see cref="T:System.Windows.Controls.Primitives.CustomPopupPlacement" /> structure to compare.</param>
	public static bool operator ==(CustomPopupPlacement placement1, CustomPopupPlacement placement2)
	{
		return placement1.Equals(placement2);
	}

	/// <summary>Compares two <see cref="T:System.Windows.Controls.Primitives.CustomPopupPlacement" /> structures to determine whether they are not equal. </summary>
	/// <returns>true if the structures do not have the same values; otherwise, false.</returns>
	/// <param name="placement1">The first <see cref="T:System.Windows.Controls.Primitives.CustomPopupPlacement" /> structure to compare.</param>
	/// <param name="placement2">The second <see cref="T:System.Windows.Controls.Primitives.CustomPopupPlacement" /> structure to compare.</param>
	public static bool operator !=(CustomPopupPlacement placement1, CustomPopupPlacement placement2)
	{
		return !placement1.Equals(placement2);
	}

	/// <summary>Compares this structure with another <see cref="T:System.Windows.Controls.Primitives.CustomPopupPlacement" /> structure to determine whether they are equal.</summary>
	/// <returns>true if the structures have the same values; otherwise, false.</returns>
	/// <param name="o">The <see cref="T:System.Windows.Controls.Primitives.CustomPopupPlacement" /> structure to compare.</param>
	public override bool Equals(object o)
	{
		if (o is CustomPopupPlacement customPopupPlacement)
		{
			if (customPopupPlacement._primaryAxis == _primaryAxis)
			{
				return customPopupPlacement._point == _point;
			}
			return false;
		}
		return false;
	}

	/// <summary>Gets the hash code for this structure. </summary>
	/// <returns>The hash code for this structure.</returns>
	public override int GetHashCode()
	{
		return _primaryAxis.GetHashCode() ^ _point.GetHashCode();
	}
}
