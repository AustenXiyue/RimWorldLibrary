using System.Collections.Generic;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using MS.Internal;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Represents a subsection of a geometry, a single connected series of two-dimensional geometric segments. </summary>
[ContentProperty("Segments")]
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public sealed class PathFigure : Animatable, IFormattable
{
	/// <summary>The identifier for the <see cref="P:System.Windows.Media.PathFigure.StartPoint" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.PathFigure.StartPoint" /> dependency property.</returns>
	public static readonly DependencyProperty StartPointProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.PathFigure.IsFilled" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.PathFigure.IsFilled" /> dependency property identifier.</returns>
	public static readonly DependencyProperty IsFilledProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.PathFigure.Segments" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.PathFigure.Segments" /> dependency property identifier.</returns>
	public static readonly DependencyProperty SegmentsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.PathFigure.IsClosed" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.PathFigure.IsClosed" /> dependency property.</returns>
	public static readonly DependencyProperty IsClosedProperty;

	internal static Point s_StartPoint;

	internal const bool c_IsFilled = true;

	internal static PathSegmentCollection s_Segments;

	internal const bool c_IsClosed = false;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Point" /> where the <see cref="T:System.Windows.Media.PathFigure" /> begins.  </summary>
	/// <returns>The <see cref="T:System.Windows.Point" /> where the <see cref="T:System.Windows.Media.PathFigure" /> begins. The default value is 0,0.</returns>
	public Point StartPoint
	{
		get
		{
			return (Point)GetValue(StartPointProperty);
		}
		set
		{
			SetValueInternal(StartPointProperty, value);
		}
	}

	/// <summary>Gets or sets whether the contained area of this <see cref="T:System.Windows.Media.PathFigure" /> is to be used for hit-testing, rendering, and clipping.   </summary>
	/// <returns>Determines whether the contained area of this <see cref="T:System.Windows.Media.PathFigure" /> is to be used for hit-testing, rendering, and clipping.  The default value is true.</returns>
	public bool IsFilled
	{
		get
		{
			return (bool)GetValue(IsFilledProperty);
		}
		set
		{
			SetValueInternal(IsFilledProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets the collection of segments that define the shape of this <see cref="T:System.Windows.Media.PathFigure" /> object.   </summary>
	/// <returns>The collection of segments that define the shape of this <see cref="T:System.Windows.Media.PathFigure" /> object. The default value is an empty collection.</returns>
	public PathSegmentCollection Segments
	{
		get
		{
			return (PathSegmentCollection)GetValue(SegmentsProperty);
		}
		set
		{
			SetValueInternal(SegmentsProperty, value);
		}
	}

	/// <summary>Gets or sets a value that specifies whether this figures first and last segments are connected.</summary>
	/// <returns>true if this figure's first and last segments are connected; otherwise, false. The default value is false.</returns>
	public bool IsClosed
	{
		get
		{
			return (bool)GetValue(IsClosedProperty);
		}
		set
		{
			SetValueInternal(IsClosedProperty, BooleanBoxes.Box(value));
		}
	}

	internal override int EffectiveValuesInitialSize => 3;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.PathFigure" /> class. </summary>
	public PathFigure()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.PathFigure" /> class with the specified <see cref="P:System.Windows.Media.PathFigure.StartPoint" />, <see cref="P:System.Windows.Media.PathFigure.Segments" />, and <see cref="P:System.Windows.Media.PathFigure.IsClosed" /> values.</summary>
	/// <param name="start">The <see cref="P:System.Windows.Media.PathFigure.StartPoint" /> for the <see cref="T:System.Windows.Media.PathFigure" />.</param>
	/// <param name="segments">The <see cref="P:System.Windows.Media.PathFigure.Segments" /> for the <see cref="T:System.Windows.Media.PathFigure" />.</param>
	/// <param name="closed">The <see cref="P:System.Windows.Media.PathFigure.IsClosed" /> for the <see cref="T:System.Windows.Media.PathFigure" />.</param>
	public PathFigure(Point start, IEnumerable<PathSegment> segments, bool closed)
	{
		StartPoint = start;
		PathSegmentCollection segments2 = Segments;
		if (segments != null)
		{
			foreach (PathSegment segment in segments)
			{
				segments2.Add(segment);
			}
			IsClosed = closed;
			return;
		}
		throw new ArgumentNullException("segments");
	}

	/// <summary> Gets a <see cref="T:System.Windows.Media.PathFigure" /> object, within the specified error of tolerance, that is an polygonal approximation of this <see cref="T:System.Windows.Media.PathFigure" /> object. </summary>
	/// <returns>The polygonal approximation of this <see cref="T:System.Windows.Media.PathFigure" /> object.</returns>
	/// <param name="tolerance">The computational tolerance of error.</param>
	/// <param name="type">Specifies how the error of tolerance is interpreted.</param>
	public PathFigure GetFlattenedPathFigure(double tolerance, ToleranceType type)
	{
		PathGeometry flattenedPathGeometry = new PathGeometry
		{
			Figures = { this }
		}.GetFlattenedPathGeometry(tolerance, type);
		return flattenedPathGeometry.Figures.Count switch
		{
			0 => new PathFigure(), 
			1 => flattenedPathGeometry.Figures[0], 
			_ => throw new InvalidOperationException(SR.PathGeometry_InternalReadBackError), 
		};
	}

	/// <summary>Gets a <see cref="T:System.Windows.Media.PathFigure" /> object that is an polygonal approximation of this <see cref="T:System.Windows.Media.PathFigure" /> object. </summary>
	/// <returns>The polygonal approximation of this <see cref="T:System.Windows.Media.PathFigure" /> object.</returns>
	public PathFigure GetFlattenedPathFigure()
	{
		return GetFlattenedPathFigure(Geometry.StandardFlatteningTolerance, ToleranceType.Absolute);
	}

	/// <summary>Determines whether this <see cref="T:System.Windows.Media.PathFigure" /> object may have curved segments. </summary>
	/// <returns>true if this <see cref="T:System.Windows.Media.PathFigure" /> object may have curved segments; otherwise, false.</returns>
	public bool MayHaveCurves()
	{
		PathSegmentCollection segments = Segments;
		if (segments == null)
		{
			return false;
		}
		int count = segments.Count;
		for (int i = 0; i < count; i++)
		{
			if (segments.Internal_GetItem(i).IsCurved())
			{
				return true;
			}
		}
		return false;
	}

	internal PathFigure GetTransformedCopy(Matrix matrix)
	{
		PathSegmentCollection segments = Segments;
		PathFigure pathFigure = new PathFigure();
		Point current = StartPoint;
		pathFigure.StartPoint = current * matrix;
		if (segments != null)
		{
			int count = segments.Count;
			for (int i = 0; i < count; i++)
			{
				segments.Internal_GetItem(i).AddToFigure(matrix, pathFigure, ref current);
			}
		}
		pathFigure.IsClosed = IsClosed;
		pathFigure.IsFilled = IsFilled;
		return pathFigure;
	}

	/// <summary>Creates a string representation of this object.</summary>
	/// <returns>A string representation of this <see cref="T:System.Windows.Media.PathFigure" />.</returns>
	public override string ToString()
	{
		ReadPreamble();
		return ConvertToString(null, null);
	}

	/// <summary>Creates a string representation of this object using the specified culture-specific formatting. </summary>
	/// <returns>A formatted string representation of this <see cref="T:System.Windows.Media.PathFigure" />.</returns>
	/// <param name="provider">Culture-specific formatting information; otherwise, null to use the current culture and default formatting settings.</param>
	public string ToString(IFormatProvider provider)
	{
		ReadPreamble();
		return ConvertToString(null, provider);
	}

	/// <summary> Formats the value of the current instance using the specified format.</summary>
	/// <returns>The value of the current instance in the specified format.</returns>
	/// <param name="format">The format to use.-or- A null reference (Nothing in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation. </param>
	/// <param name="provider">The provider to use to format the value.-or- A null reference (Nothing in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system. </param>
	string IFormattable.ToString(string format, IFormatProvider provider)
	{
		ReadPreamble();
		return ConvertToString(format, provider);
	}

	internal bool CanSerializeToString()
	{
		PathSegmentCollection segments = Segments;
		if (IsFilled)
		{
			return segments?.CanSerializeToString() ?? true;
		}
		return false;
	}

	internal string ConvertToString(string format, IFormatProvider provider)
	{
		PathSegmentCollection segments = Segments;
		return "M" + ((IFormattable)StartPoint).ToString(format, provider) + ((segments != null) ? segments.ConvertToString(format, provider) : "") + (IsClosed ? "z" : "");
	}

	internal void SerializeData(StreamGeometryContext ctx)
	{
		ctx.BeginFigure(StartPoint, IsFilled, IsClosed);
		PathSegmentCollection segments = Segments;
		int num = segments?.Count ?? 0;
		for (int i = 0; i < num; i++)
		{
			segments.Internal_GetItem(i).SerializeData(ctx);
		}
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.PathFigure" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new PathFigure Clone()
	{
		return (PathFigure)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.PathFigure" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new PathFigure CloneCurrentValue()
	{
		return (PathFigure)base.CloneCurrentValue();
	}

	protected override Freezable CreateInstanceCore()
	{
		return new PathFigure();
	}

	static PathFigure()
	{
		s_StartPoint = default(Point);
		s_Segments = PathSegmentCollection.Empty;
		Type typeFromHandle = typeof(PathFigure);
		StartPointProperty = Animatable.RegisterProperty("StartPoint", typeof(Point), typeFromHandle, default(Point), null, null, isIndependentlyAnimated: false, null);
		IsFilledProperty = Animatable.RegisterProperty("IsFilled", typeof(bool), typeFromHandle, true, null, null, isIndependentlyAnimated: false, null);
		SegmentsProperty = Animatable.RegisterProperty("Segments", typeof(PathSegmentCollection), typeFromHandle, new FreezableDefaultValueFactory(PathSegmentCollection.Empty), null, null, isIndependentlyAnimated: false, null);
		IsClosedProperty = Animatable.RegisterProperty("IsClosed", typeof(bool), typeFromHandle, false, null, null, isIndependentlyAnimated: false, null);
	}
}
