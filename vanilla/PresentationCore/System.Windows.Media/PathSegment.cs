using System.Windows.Media.Animation;
using MS.Internal.KnownBoxes;

namespace System.Windows.Media;

/// <summary>Represents a segment of a <see cref="T:System.Windows.Media.PathFigure" /> object.  </summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public abstract class PathSegment : Animatable
{
	internal const bool c_isStrokedDefault = true;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.PathSegment.IsStroked" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.PathSegment.IsStroked" /> dependency property.</returns>
	public static readonly DependencyProperty IsStrokedProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.PathSegment.IsSmoothJoin" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.PathSegment.IsSmoothJoin" /> dependency property.</returns>
	public static readonly DependencyProperty IsSmoothJoinProperty;

	internal const bool c_IsStroked = true;

	internal const bool c_IsSmoothJoin = false;

	/// <summary>Gets or sets a value that indicates whether the segment is stroked. </summary>
	/// <returns>true if the segment is stroked when a <see cref="T:System.Windows.Media.Pen" /> is used to render the segment; otherwise, the segment is not stroked. The default is true.</returns>
	public bool IsStroked
	{
		get
		{
			return (bool)GetValue(IsStrokedProperty);
		}
		set
		{
			SetValueInternal(IsStrokedProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets a value that indicates whether the join between this <see cref="T:System.Windows.Media.PathSegment" /> and the previous <see cref="T:System.Windows.Media.PathSegment" /> is treated as a corner when it is stroked with a <see cref="T:System.Windows.Media.Pen" />.  </summary>
	/// <returns>true if the join between this <see cref="T:System.Windows.Media.PathSegment" /> and the previous <see cref="T:System.Windows.Media.PathSegment" /> is not to be treated as a corner; otherwise, false. The default is false. </returns>
	public bool IsSmoothJoin
	{
		get
		{
			return (bool)GetValue(IsSmoothJoinProperty);
		}
		set
		{
			SetValueInternal(IsSmoothJoinProperty, BooleanBoxes.Box(value));
		}
	}

	internal PathSegment()
	{
	}

	internal abstract void AddToFigure(Matrix matrix, PathFigure figure, ref Point current);

	internal virtual bool IsEmpty()
	{
		return false;
	}

	internal abstract bool IsCurved();

	internal abstract string ConvertToString(string format, IFormatProvider provider);

	internal abstract void SerializeData(StreamGeometryContext ctx);

	/// <summary>Creates a modifiable copy of this <see cref="T:System.Windows.Media.PathSegment" /> by making deep copies of its values. </summary>
	/// <returns>A modifiable deep copy of the current object. The <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the cloned object returns false even if the <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the source is true.</returns>
	public new PathSegment Clone()
	{
		return (PathSegment)base.Clone();
	}

	/// <summary>Creates a modifiable copy of this <see cref="T:System.Windows.Media.PathSegment" /> object by making deep copies of its values. This method does not copy resource references, data bindings, and animations, although it does copy their current values. </summary>
	/// <returns>A modifiable deep copy of the current object. The <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the source is true.</returns>
	public new PathSegment CloneCurrentValue()
	{
		return (PathSegment)base.CloneCurrentValue();
	}

	static PathSegment()
	{
		Type typeFromHandle = typeof(PathSegment);
		IsStrokedProperty = Animatable.RegisterProperty("IsStroked", typeof(bool), typeFromHandle, true, null, null, isIndependentlyAnimated: false, null);
		IsSmoothJoinProperty = Animatable.RegisterProperty("IsSmoothJoin", typeof(bool), typeFromHandle, false, null, null, isIndependentlyAnimated: false, null);
	}
}
