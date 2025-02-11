using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.Ink;
using MS.Internal.PresentationCore;

namespace System.Windows.Ink;

/// <summary>Represents a single ink stroke.</summary>
public class Stroke : INotifyPropertyChanged
{
	private ExtendedPropertyCollection _extendedProperties;

	private DrawingAttributes _drawingAttributes;

	private StylusPointCollection _stylusPoints;

	internal double TapHitPointSize = 1.0;

	internal double TapHitRotation;

	private Geometry _cachedGeometry;

	private bool _isSelected;

	private bool _drawAsHollow;

	private bool _cloneStylusPoints = true;

	private bool _delayRaiseInvalidated;

	private const double HollowLineSize = 1.0;

	private Rect _cachedBounds = Rect.Empty;

	private PropertyChangedEventHandler _propertyChanged;

	private const string DrawingAttributesName = "DrawingAttributes";

	private const string StylusPointsName = "StylusPoints";

	internal const double PercentageTolerance = 0.0001;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Ink.DrawingAttributes" /> for the <see cref="T:System.Windows.Ink.Stroke" /> object. </summary>
	/// <exception cref="T:System.ArgumentNullException">The set value is null.</exception>
	public DrawingAttributes DrawingAttributes
	{
		get
		{
			return _drawingAttributes;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_drawingAttributes.AttributeChanged -= DrawingAttributes_Changed;
			DrawingAttributesReplacedEventArgs e = new DrawingAttributesReplacedEventArgs(value, _drawingAttributes);
			DrawingAttributes drawingAttributes = _drawingAttributes;
			_drawingAttributes = value;
			if (!DrawingAttributes.GeometricallyEqual(drawingAttributes, _drawingAttributes))
			{
				_cachedGeometry = null;
				_cachedBounds = Rect.Empty;
			}
			_drawingAttributes.AttributeChanged += DrawingAttributes_Changed;
			OnDrawingAttributesReplaced(e);
			OnInvalidated(EventArgs.Empty);
			OnPropertyChanged("DrawingAttributes");
		}
	}

	/// <summary>Returns the stylus points of the <see cref="T:System.Windows.Ink.Stroke" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Input.StylusPointCollection" /> that contains the stylus points that represent the current <see cref="T:System.Windows.Ink.Stroke" />.</returns>
	public StylusPointCollection StylusPoints
	{
		get
		{
			return _stylusPoints;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value.Count == 0)
			{
				throw new ArgumentException(SR.InvalidStylusPointCollectionZeroCount);
			}
			_cachedGeometry = null;
			_cachedBounds = Rect.Empty;
			StylusPointsReplacedEventArgs e = new StylusPointsReplacedEventArgs(value, _stylusPoints);
			_stylusPoints.Changed -= StylusPoints_Changed;
			_stylusPoints.CountGoingToZero -= StylusPoints_CountGoingToZero;
			_stylusPoints = value;
			_stylusPoints.Changed += StylusPoints_Changed;
			_stylusPoints.CountGoingToZero += StylusPoints_CountGoingToZero;
			OnStylusPointsReplaced(e);
			OnInvalidated(EventArgs.Empty);
			OnPropertyChanged("StylusPoints");
		}
	}

	internal ExtendedPropertyCollection ExtendedProperties
	{
		get
		{
			if (_extendedProperties == null)
			{
				_extendedProperties = new ExtendedPropertyCollection();
			}
			return _extendedProperties;
		}
	}

	[FriendAccessAllowed]
	internal bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			if (value != _isSelected)
			{
				_isSelected = value;
				OnInvalidated(EventArgs.Empty);
			}
		}
	}

	/// <summary>Occurs when the <see cref="P:System.Windows.Ink.Stroke.DrawingAttributes" /> associated with the <see cref="T:System.Windows.Ink.Stroke" /> object change. </summary>
	public event PropertyDataChangedEventHandler DrawingAttributesChanged;

	/// <summary>Occurs when the drawing attributes of a <see cref="T:System.Windows.Ink.Stroke" /> object are replaced.</summary>
	public event DrawingAttributesReplacedEventHandler DrawingAttributesReplaced;

	/// <summary>Occurs when the <see cref="P:System.Windows.Ink.Stroke.StylusPoints" /> property is assigned a new <see cref="T:System.Windows.Input.StylusPointCollection" />.</summary>
	public event StylusPointsReplacedEventHandler StylusPointsReplaced;

	/// <summary>Occurs when the <see cref="P:System.Windows.Ink.Stroke.StylusPoints" /> property changes.</summary>
	public event EventHandler StylusPointsChanged;

	/// <summary>Occurs when the custom properties on a <see cref="T:System.Windows.Ink.Stroke" /> object changes.</summary>
	public event PropertyDataChangedEventHandler PropertyDataChanged;

	/// <summary>Occurs when the appearance of the <see cref="T:System.Windows.Ink.Stroke" /> changes.</summary>
	public event EventHandler Invalidated;

	/// <summary>Occurs when the value of any <see cref="T:System.Windows.Ink.Stroke" /> property has changed.</summary>
	event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
	{
		add
		{
			_propertyChanged = (PropertyChangedEventHandler)Delegate.Combine(_propertyChanged, value);
		}
		remove
		{
			_propertyChanged = (PropertyChangedEventHandler)Delegate.Remove(_propertyChanged, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Ink.Stroke" /> class. </summary>
	/// <param name="stylusPoints">A <see cref="T:System.Windows.Input.StylusPointCollection" /> that represents the <see cref="T:System.Windows.Ink.Stroke" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stylusPoints" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stylusPoints" /> is empty..</exception>
	public Stroke(StylusPointCollection stylusPoints)
		: this(stylusPoints, new DrawingAttributes(), null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Ink.Stroke" /> class. </summary>
	/// <param name="stylusPoints">A <see cref="T:System.Windows.Input.StylusPointCollection" /> that represents the <see cref="T:System.Windows.Ink.Stroke" />.</param>
	/// <param name="drawingAttributes">A <see cref="T:System.Windows.Ink.DrawingAttributes" /> object that specifies the appearance of the <see cref="T:System.Windows.Ink.Stroke" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stylusPoints" /> is null.-or-<paramref name="drawingAtrributes" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stylusPoints" /> is empty.</exception>
	public Stroke(StylusPointCollection stylusPoints, DrawingAttributes drawingAttributes)
		: this(stylusPoints, drawingAttributes, null)
	{
	}

	internal Stroke(StylusPointCollection stylusPoints, DrawingAttributes drawingAttributes, ExtendedPropertyCollection extendedProperties)
	{
		if (stylusPoints == null)
		{
			throw new ArgumentNullException("stylusPoints");
		}
		if (stylusPoints.Count == 0)
		{
			throw new ArgumentException(SR.InvalidStylusPointCollectionZeroCount, "stylusPoints");
		}
		if (drawingAttributes == null)
		{
			throw new ArgumentNullException("drawingAttributes");
		}
		_drawingAttributes = drawingAttributes;
		_stylusPoints = stylusPoints;
		_extendedProperties = extendedProperties;
		Initialize();
	}

	private void Initialize()
	{
		_drawingAttributes.AttributeChanged += DrawingAttributes_Changed;
		_stylusPoints.Changed += StylusPoints_Changed;
		_stylusPoints.CountGoingToZero += StylusPoints_CountGoingToZero;
	}

	/// <summary>Returns a deep copy of the existing <see cref="T:System.Windows.Ink.Stroke" /> object.</summary>
	/// <returns>The new <see cref="T:System.Windows.Ink.Stroke" /> object.</returns>
	public virtual Stroke Clone()
	{
		Stroke stroke = (Stroke)MemberwiseClone();
		stroke.DrawingAttributesChanged = null;
		stroke.DrawingAttributesReplaced = null;
		stroke.StylusPointsReplaced = null;
		stroke.StylusPointsChanged = null;
		stroke.PropertyDataChanged = null;
		stroke.Invalidated = null;
		stroke._propertyChanged = null;
		if (_cloneStylusPoints)
		{
			stroke._stylusPoints = _stylusPoints.Clone();
		}
		stroke._drawingAttributes = _drawingAttributes.Clone();
		if (_extendedProperties != null)
		{
			stroke._extendedProperties = _extendedProperties.Clone();
		}
		stroke.Initialize();
		stroke._cloneStylusPoints = true;
		return stroke;
	}

	/// <summary>Performs a transformation based upon the specified <see cref="T:System.Windows.Media.Matrix" /> object.</summary>
	/// <param name="transformMatrix">The <see cref="T:System.Windows.Media.Matrix" /> object defining the transformation.</param>
	/// <param name="applyToStylusTip">true to apply the transformation to the tip of the stylus; otherwise, false.</param>
	public virtual void Transform(Matrix transformMatrix, bool applyToStylusTip)
	{
		if (transformMatrix.IsIdentity)
		{
			return;
		}
		if (!transformMatrix.HasInverse)
		{
			throw new ArgumentException(SR.MatrixNotInvertible, "transformMatrix");
		}
		if (MatrixHelper.ContainsNaN(transformMatrix))
		{
			throw new ArgumentException(SR.InvalidMatrixContainsNaN, "transformMatrix");
		}
		if (MatrixHelper.ContainsInfinity(transformMatrix))
		{
			throw new ArgumentException(SR.InvalidMatrixContainsInfinity, "transformMatrix");
		}
		_cachedGeometry = null;
		_cachedBounds = Rect.Empty;
		if (applyToStylusTip)
		{
			_delayRaiseInvalidated = true;
		}
		try
		{
			_stylusPoints.Transform(new MatrixTransform(transformMatrix));
			if (applyToStylusTip)
			{
				Matrix stylusTipTransform = _drawingAttributes.StylusTipTransform;
				transformMatrix.OffsetX = 0.0;
				transformMatrix.OffsetY = 0.0;
				stylusTipTransform *= transformMatrix;
				if (stylusTipTransform.HasInverse)
				{
					_drawingAttributes.StylusTipTransform = stylusTipTransform;
				}
			}
			if (_delayRaiseInvalidated)
			{
				OnInvalidated(EventArgs.Empty);
			}
		}
		finally
		{
			_delayRaiseInvalidated = false;
		}
	}

	/// <summary>Returns the stylus points the <see cref="T:System.Windows.Ink.Stroke" /> uses when <see cref="P:System.Windows.Ink.DrawingAttributes.FitToCurve" /> is true.</summary>
	/// <returns>A <see cref="T:System.Windows.Input.StylusPointCollection" /> that contains the stylus points along the spine of a <see cref="T:System.Windows.Ink.Stroke" /> when <see cref="P:System.Windows.Ink.DrawingAttributes.FitToCurve" /> is true</returns>
	public StylusPointCollection GetBezierStylusPoints()
	{
		if (_stylusPoints.Count < 2)
		{
			return _stylusPoints;
		}
		Bezier bezier = new Bezier();
		if (!bezier.ConstructBezierState(_stylusPoints, DrawingAttributes.FittingError))
		{
			return _stylusPoints.Clone();
		}
		double num = 0.5;
		StylusShape stylusShape = DrawingAttributes.StylusShape;
		if (stylusShape != null)
		{
			Rect boundingBox = stylusShape.BoundingBox;
			double num2 = Math.Min(boundingBox.Width, boundingBox.Height);
			num = Math.Log10(num2 + num2);
			num *= 13.229166666666666;
			if (num < 0.5)
			{
				num = 0.5;
			}
		}
		List<Point> bezierPoints = bezier.Flatten(num);
		return GetInterpolatedStylusPoints(bezierPoints);
	}

	private StylusPointCollection GetInterpolatedStylusPoints(List<Point> bezierPoints)
	{
		StylusPointCollection stylusPointCollection = new StylusPointCollection(_stylusPoints.Description, bezierPoints.Count);
		AddInterpolatedBezierPoint(stylusPointCollection, bezierPoints[0], _stylusPoints[0].GetAdditionalData(), _stylusPoints[0].PressureFactor);
		if (bezierPoints.Count == 1)
		{
			return stylusPointCollection;
		}
		double num = 0.0;
		double num2 = 0.0;
		double num3 = GetDistanceBetweenPoints((Point)_stylusPoints[0], (Point)_stylusPoints[1]);
		int num4 = 1;
		int count = _stylusPoints.Count;
		for (int i = 1; i < bezierPoints.Count - 1; i++)
		{
			num += GetDistanceBetweenPoints(bezierPoints[i - 1], bezierPoints[i]);
			while (count > num4)
			{
				if (num >= num2 && num < num3)
				{
					StylusPoint stylusPoint = _stylusPoints[num4 - 1];
					float num5 = ((float)num - (float)num2) / ((float)num3 - (float)num2);
					float pressureFactor = stylusPoint.PressureFactor;
					float num6 = _stylusPoints[num4].PressureFactor - pressureFactor;
					float pressure = num5 * num6 + pressureFactor;
					AddInterpolatedBezierPoint(stylusPointCollection, bezierPoints[i], stylusPoint.GetAdditionalData(), pressure);
					break;
				}
				num4++;
				if (count > num4)
				{
					num2 = num3;
					num3 += GetDistanceBetweenPoints((Point)_stylusPoints[num4 - 1], (Point)_stylusPoints[num4]);
				}
			}
		}
		AddInterpolatedBezierPoint(stylusPointCollection, bezierPoints[bezierPoints.Count - 1], _stylusPoints[count - 1].GetAdditionalData(), _stylusPoints[count - 1].PressureFactor);
		return stylusPointCollection;
	}

	private double GetDistanceBetweenPoints(Point p1, Point p2)
	{
		return Math.Sqrt((p2 - p1).LengthSquared);
	}

	private void AddInterpolatedBezierPoint(StylusPointCollection bezierStylusPoints, Point bezierPoint, int[] additionalData, float pressure)
	{
		double x = ((bezierPoint.X > StylusPoint.MaxXY) ? StylusPoint.MaxXY : ((bezierPoint.X < StylusPoint.MinXY) ? StylusPoint.MinXY : bezierPoint.X));
		double y = ((bezierPoint.Y > StylusPoint.MaxXY) ? StylusPoint.MaxXY : ((bezierPoint.Y < StylusPoint.MinXY) ? StylusPoint.MinXY : bezierPoint.Y));
		StylusPoint item = new StylusPoint(x, y, pressure, bezierStylusPoints.Description, additionalData, validateAdditionalData: false, validatePressureFactor: false);
		bezierStylusPoints.Add(item);
	}

	/// <summary>Adds a custom property to the <see cref="T:System.Windows.Ink.Stroke" /> object.</summary>
	/// <param name="propertyDataId">The unique identifier for the property.</param>
	/// <param name="propertyData">The value of the custom property. <paramref name="propertyData" /> must be of type <see cref="T:System.Char" />, <see cref="T:System.Byte" />,<see cref="T:System.Int16" />,,<see cref="T:System.UInt16" />, <see cref="T:System.Int32" />, <see cref="T:System.UInt32" />, <see cref="T:System.Int64" />, <see cref="T:System.UInt64" />, <see cref="T:System.Single" />, <see cref="T:System.Double" />, <see cref="T:System.DateTime" />, <see cref="T:System.Boolean" />, <see cref="T:System.String" />, <see cref="T:System.Decimal" />  or an array of these data types, except <see cref="T:System.String" />, which is not allowed.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="propertyData" /> argument is not one of the allowed data types listed in the Parameters section.</exception>
	public void AddPropertyData(Guid propertyDataId, object propertyData)
	{
		DrawingAttributes.ValidateStylusTipTransform(propertyDataId, propertyData);
		object previousValue = null;
		if (ContainsPropertyData(propertyDataId))
		{
			previousValue = GetPropertyData(propertyDataId);
			ExtendedProperties[propertyDataId] = propertyData;
		}
		else
		{
			ExtendedProperties.Add(propertyDataId, propertyData);
		}
		OnPropertyDataChanged(new PropertyDataChangedEventArgs(propertyDataId, propertyData, previousValue));
	}

	/// <summary>Deletes a custom property from the <see cref="T:System.Windows.Ink.Stroke" /> object.</summary>
	/// <param name="propertyDataId">The unique identifier for the property.</param>
	public void RemovePropertyData(Guid propertyDataId)
	{
		object propertyData = GetPropertyData(propertyDataId);
		ExtendedProperties.Remove(propertyDataId);
		OnPropertyDataChanged(new PropertyDataChangedEventArgs(propertyDataId, null, propertyData));
	}

	/// <summary>Retrieves the property data for the specified GUID.</summary>
	/// <returns>An object containing the property data.</returns>
	/// <param name="propertyDataId">The unique identifier for the property.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="propertyDataId" /> is not associated with a custom property of the <see cref="T:System.Windows.Ink.Stroke" />.</exception>
	public object GetPropertyData(Guid propertyDataId)
	{
		return ExtendedProperties[propertyDataId];
	}

	/// <summary>Retrieves the GUIDs of any custom properties associated with the <see cref="T:System.Windows.Ink.Stroke" /> object.</summary>
	/// <returns>An array of <see cref="T:System.Guid" /> objects.</returns>
	public Guid[] GetPropertyDataIds()
	{
		return ExtendedProperties.GetGuidArray();
	}

	/// <summary>Returns a value that indicates whether the <see cref="T:System.Windows.Ink.Stroke" /> object contains the specified custom property.</summary>
	/// <returns>Returns true if the custom property exists; otherwise, returns false.</returns>
	/// <param name="propertyDataId">The unique identifier for the property.</param>
	public bool ContainsPropertyData(Guid propertyDataId)
	{
		return ExtendedProperties.Contains(propertyDataId);
	}

	/// <summary>Allows derived classes to modify the default behavior of the <see cref="E:System.Windows.Ink.Stroke.DrawingAttributesChanged" /> event.</summary>
	/// <param name="e">The <see cref="T:System.Windows.Ink.PropertyDataChangedEventArgs" /> object that contains the event data.</param>
	protected virtual void OnDrawingAttributesChanged(PropertyDataChangedEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e", SR.EventArgIsNull);
		}
		if (this.DrawingAttributesChanged != null)
		{
			this.DrawingAttributesChanged(this, e);
		}
	}

	/// <summary>Allows derived classes to modify the default behavior of the <see cref="E:System.Windows.Ink.Stroke.DrawingAttributesReplaced" /> event.</summary>
	/// <param name="e">The <see cref="T:System.Windows.Ink.DrawingAttributesReplacedEventArgs" /> object that contains the event data.</param>
	protected virtual void OnDrawingAttributesReplaced(DrawingAttributesReplacedEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		if (this.DrawingAttributesReplaced != null)
		{
			this.DrawingAttributesReplaced(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Ink.Stroke.StylusPointsReplaced" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Ink.StylusPointsReplacedEventArgs" /> that contains the event data. </param>
	protected virtual void OnStylusPointsReplaced(StylusPointsReplacedEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e", SR.EventArgIsNull);
		}
		if (this.StylusPointsReplaced != null)
		{
			this.StylusPointsReplaced(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Ink.Stroke.StylusPointsChanged" /> event.</summary>
	/// <param name="e">A <see cref="T:System.EventArgs" /> that contains the event data. </param>
	protected virtual void OnStylusPointsChanged(EventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e", SR.EventArgIsNull);
		}
		if (this.StylusPointsChanged != null)
		{
			this.StylusPointsChanged(this, e);
		}
	}

	/// <summary>Allows derived classes to modify the default behavior of the <see cref="E:System.Windows.Ink.Stroke.PropertyDataChanged" /> event.</summary>
	/// <param name="e">The <see cref="T:System.Windows.Ink.PropertyDataChangedEventArgs" /> object that contains the event data.</param>
	protected virtual void OnPropertyDataChanged(PropertyDataChangedEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e", SR.EventArgIsNull);
		}
		if (this.PropertyDataChanged != null)
		{
			this.PropertyDataChanged(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Ink.Stroke.Invalidated" /> event.</summary>
	/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
	protected virtual void OnInvalidated(EventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e", SR.EventArgIsNull);
		}
		if (this.Invalidated != null)
		{
			this.Invalidated(this, e);
		}
	}

	/// <summary>Occurs when any <see cref="T:System.Windows.Ink.Stroke" /> property changes.</summary>
	/// <param name="e">The event data that describes the property that changed, as well as old and new values.</param>
	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		if (_propertyChanged != null)
		{
			_propertyChanged(this, e);
		}
	}

	private StrokeCollection Clip(StrokeFIndices[] cutAt)
	{
		StrokeCollection strokeCollection = new StrokeCollection();
		if (cutAt.Length == 0)
		{
			return strokeCollection;
		}
		if (cutAt.Length == 1 && cutAt[0].IsFull)
		{
			strokeCollection.Add(Clone());
			return strokeCollection;
		}
		StylusPointCollection sourceStylusPoints = StylusPoints;
		if (DrawingAttributes.FitToCurve)
		{
			sourceStylusPoints = GetBezierStylusPoints();
		}
		for (int i = 0; i < cutAt.Length; i++)
		{
			StrokeFIndices strokeFIndices = cutAt[i];
			if (!DoubleUtil.GreaterThanOrClose(strokeFIndices.BeginFIndex, strokeFIndices.EndFIndex))
			{
				Stroke item = Copy(sourceStylusPoints, strokeFIndices.BeginFIndex, strokeFIndices.EndFIndex);
				strokeCollection.Add(item);
			}
		}
		return strokeCollection;
	}

	private StrokeCollection Erase(StrokeFIndices[] cutAt)
	{
		StrokeCollection strokeCollection = new StrokeCollection();
		if (cutAt.Length == 0 || (cutAt.Length == 1 && cutAt[0].IsFull))
		{
			return strokeCollection;
		}
		StylusPointCollection sourceStylusPoints = StylusPoints;
		if (DrawingAttributes.FitToCurve)
		{
			sourceStylusPoints = GetBezierStylusPoints();
		}
		int i = 0;
		double num = StrokeFIndices.BeforeFirst;
		if (cutAt[0].BeginFIndex == StrokeFIndices.BeforeFirst)
		{
			num = cutAt[0].EndFIndex;
			i++;
		}
		for (; i < cutAt.Length; i++)
		{
			StrokeFIndices strokeFIndices = cutAt[i];
			if (!DoubleUtil.GreaterThanOrClose(num, strokeFIndices.BeginFIndex))
			{
				Stroke item = Copy(sourceStylusPoints, num, strokeFIndices.BeginFIndex);
				strokeCollection.Add(item);
				num = strokeFIndices.EndFIndex;
			}
		}
		if (num != StrokeFIndices.AfterLast)
		{
			Stroke item2 = Copy(sourceStylusPoints, num, StrokeFIndices.AfterLast);
			strokeCollection.Add(item2);
		}
		return strokeCollection;
	}

	private Stroke Copy(StylusPointCollection sourceStylusPoints, double beginFIndex, double endFIndex)
	{
		int num = ((!DoubleUtil.AreClose(StrokeFIndices.BeforeFirst, beginFIndex)) ? ((int)Math.Floor(beginFIndex)) : 0);
		int num2 = (DoubleUtil.AreClose(StrokeFIndices.AfterLast, endFIndex) ? (sourceStylusPoints.Count - 1) : ((int)Math.Ceiling(endFIndex)));
		int num3 = num2 - num + 1;
		StylusPointCollection stylusPointCollection = new StylusPointCollection(StylusPoints.Description, num3);
		for (int i = 0; i < num3; i++)
		{
			StylusPoint item = sourceStylusPoints[i + num];
			stylusPointCollection.Add(item);
		}
		if (!DoubleUtil.AreClose(beginFIndex, StrokeFIndices.BeforeFirst))
		{
			beginFIndex -= (double)num;
		}
		if (!DoubleUtil.AreClose(endFIndex, StrokeFIndices.AfterLast))
		{
			endFIndex -= (double)num;
		}
		if (stylusPointCollection.Count > 1)
		{
			Point point = (Point)stylusPointCollection[0];
			Point point2 = (Point)stylusPointCollection[stylusPointCollection.Count - 1];
			if (!DoubleUtil.AreClose(endFIndex, StrokeFIndices.AfterLast) && !DoubleUtil.AreClose(num2, endFIndex))
			{
				double findex = Math.Ceiling(endFIndex) - endFIndex;
				point2 = GetIntermediatePoint(stylusPointCollection[stylusPointCollection.Count - 1], stylusPointCollection[stylusPointCollection.Count - 2], findex);
			}
			if (!DoubleUtil.AreClose(beginFIndex, StrokeFIndices.BeforeFirst) && !DoubleUtil.AreClose(num, beginFIndex))
			{
				point = GetIntermediatePoint(stylusPointCollection[0], stylusPointCollection[1], beginFIndex);
			}
			StylusPoint value = stylusPointCollection[stylusPointCollection.Count - 1];
			value.X = point2.X;
			value.Y = point2.Y;
			stylusPointCollection[stylusPointCollection.Count - 1] = value;
			StylusPoint value2 = stylusPointCollection[0];
			value2.X = point.X;
			value2.Y = point.Y;
			stylusPointCollection[0] = value2;
		}
		Stroke stroke = null;
		try
		{
			_cloneStylusPoints = false;
			stroke = Clone();
			if (stroke.DrawingAttributes.FitToCurve)
			{
				stroke.DrawingAttributes.FitToCurve = false;
			}
			stroke.StylusPoints = stylusPointCollection;
			return stroke;
		}
		finally
		{
			_cloneStylusPoints = true;
		}
	}

	private Point GetIntermediatePoint(StylusPoint p1, StylusPoint p2, double findex)
	{
		double num = p2.X - p1.X;
		double num2 = p2.Y - p1.Y;
		double num3 = num * findex;
		double num4 = num2 * findex;
		return new Point(p1.X + num3, p1.Y + num4);
	}

	private void DrawingAttributes_Changed(object sender, PropertyDataChangedEventArgs e)
	{
		if (DrawingAttributes.IsGeometricalDaGuid(e.PropertyGuid))
		{
			_cachedGeometry = null;
			_cachedBounds = Rect.Empty;
		}
		OnDrawingAttributesChanged(e);
		if (!_delayRaiseInvalidated)
		{
			OnInvalidated(EventArgs.Empty);
		}
	}

	private void StylusPoints_Changed(object sender, EventArgs e)
	{
		_cachedGeometry = null;
		_cachedBounds = Rect.Empty;
		OnStylusPointsChanged(EventArgs.Empty);
		if (!_delayRaiseInvalidated)
		{
			OnInvalidated(EventArgs.Empty);
		}
	}

	private void StylusPoints_CountGoingToZero(object sender, CancelEventArgs e)
	{
		e.Cancel = true;
	}

	private void OnPropertyChanged(string propertyName)
	{
		OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
	}

	/// <summary>Retrieves the bounding box for the <see cref="T:System.Windows.Ink.Stroke" /> object.</summary>
	/// <returns>A <see cref="T:System.Windows.Rect" /> structure defining the bounding box for the <see cref="T:System.Windows.Ink.Stroke" /> object.</returns>
	public virtual Rect GetBounds()
	{
		if (_cachedBounds.IsEmpty)
		{
			StrokeNodeIterator iterator = StrokeNodeIterator.GetIterator(this, DrawingAttributes);
			for (int i = 0; i < iterator.Count; i++)
			{
				StrokeNode strokeNode = iterator[i];
				_cachedBounds.Union(strokeNode.GetBounds());
			}
		}
		return _cachedBounds;
	}

	/// <summary>Renders the <see cref="T:System.Windows.Ink.Stroke" /> object based upon the specified <see cref="T:System.Windows.Media.DrawingContext" />.</summary>
	public void Draw(DrawingContext context)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		Draw(context, DrawingAttributes);
	}

	/// <summary>Renders the <see cref="T:System.Windows.Ink.Stroke" /> object based upon the specified <see cref="T:System.Windows.Media.DrawingContext" /> and <see cref="T:Microsoft.Ink.DrawingAttributes" />.</summary>
	/// <param name="drawingContext">The <see cref="T:System.Windows.Media.DrawingContext" /> object onto which the stroke will be rendered.</param>
	/// <param name="drawingAttributes">The <see cref="T:Microsoft.Ink.DrawingAttributes" /> object defining the attributes of the stroke that is drawn.</param>
	public void Draw(DrawingContext drawingContext, DrawingAttributes drawingAttributes)
	{
		if (drawingContext == null)
		{
			throw new ArgumentNullException("context");
		}
		if (null == drawingAttributes)
		{
			throw new ArgumentNullException("drawingAttributes");
		}
		if (drawingAttributes.IsHighlighter)
		{
			drawingContext.PushOpacity(0.5);
			try
			{
				DrawInternal(drawingContext, StrokeRenderer.GetHighlighterAttributes(this, DrawingAttributes), drawAsHollow: false);
				return;
			}
			finally
			{
				drawingContext.Pop();
			}
		}
		DrawInternal(drawingContext, drawingAttributes, drawAsHollow: false);
	}

	/// <summary>Returns segments of the current <see cref="T:System.Windows.Ink.Stroke" /> that are within the specified rectangle.</summary>
	/// <returns>A <see cref="T:System.Windows.Ink.StrokeCollection" /> that contains copies of the segments of the current <see cref="T:System.Windows.Ink.Stroke" /> that are within the bounds of <paramref name="bounds" />.</returns>
	/// <param name="bounds">A <see cref="T:System.Windows.Rect" /> that specifies the area to clip.</param>
	public StrokeCollection GetClipResult(Rect bounds)
	{
		return GetClipResult(new Point[4] { bounds.TopLeft, bounds.TopRight, bounds.BottomRight, bounds.BottomLeft });
	}

	/// <summary>Returns segments of the current <see cref="T:System.Windows.Ink.Stroke" /> that are within the specified bounds.</summary>
	/// <returns>A <see cref="T:System.Windows.Ink.StrokeCollection" /> that contains copies of the segments of the current <see cref="T:System.Windows.Ink.Stroke" /> that are within the specified bounds.</returns>
	/// <param name="lassoPoints">The points that specify the line which defines where to clip.</param>
	public StrokeCollection GetClipResult(IEnumerable<Point> lassoPoints)
	{
		if (lassoPoints == null)
		{
			throw new ArgumentNullException("lassoPoints");
		}
		if (IEnumerablePointHelper.GetCount(lassoPoints) == 0)
		{
			throw new ArgumentException(SR.EmptyArray);
		}
		Lasso lasso = new SingleLoopLasso();
		lasso.AddPoints(lassoPoints);
		return Clip(HitTest(lasso));
	}

	/// <summary>Returns segments of the current <see cref="T:System.Windows.Ink.Stroke" /> that are outside the specified rectangle.</summary>
	/// <returns>A <see cref="T:System.Windows.Ink.StrokeCollection" /> that contains the segments of the current <see cref="T:System.Windows.Ink.Stroke" /> that are outside the bounds of the specified <see cref="T:System.Windows.Rect" />.</returns>
	/// <param name="bounds">A <see cref="T:System.Windows.Rect" /> that specifies the area to erase.</param>
	public StrokeCollection GetEraseResult(Rect bounds)
	{
		return GetEraseResult(new Point[4] { bounds.TopLeft, bounds.TopRight, bounds.BottomRight, bounds.BottomLeft });
	}

	/// <summary>Returns segments of the current <see cref="T:System.Windows.Ink.Stroke" /> that are outside the specified bounds.</summary>
	/// <returns>A <see cref="T:System.Windows.Ink.StrokeCollection" /> that contains the segments of the current <see cref="T:System.Windows.Ink.Stroke" /> that are outside the specified bounds.</returns>
	/// <param name="lassoPoints">An array of type <see cref="T:System.Windows.Point" /> that specifies the area to erase.</param>
	public StrokeCollection GetEraseResult(IEnumerable<Point> lassoPoints)
	{
		if (lassoPoints == null)
		{
			throw new ArgumentNullException("lassoPoints");
		}
		if (IEnumerablePointHelper.GetCount(lassoPoints) == 0)
		{
			throw new ArgumentException(SR.EmptyArray);
		}
		Lasso lasso = new SingleLoopLasso();
		lasso.AddPoints(lassoPoints);
		return Erase(HitTest(lasso));
	}

	/// <summary>Returns the segments of the current <see cref="T:System.Windows.Ink.Stroke" /> after it is dissected by the designated path using the specified <see cref="T:System.Windows.Ink.StylusShape" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Ink.StrokeCollection" /> that contains copies of the segments of the current <see cref="T:System.Windows.Ink.Stroke" /> after it is dissected by the specified path.</returns>
	/// <param name="eraserPath">An array of type <see cref="T:System.Windows.Point" /> that specifies the path that dissects the <see cref="T:System.Windows.Ink.Stroke" />.</param>
	/// <param name="eraserShape">A <see cref="T:System.Windows.Ink.StylusShape" /> that specifies the shape of the eraser.</param>
	public StrokeCollection GetEraseResult(IEnumerable<Point> eraserPath, StylusShape eraserShape)
	{
		if (eraserShape == null)
		{
			throw new ArgumentNullException("eraserShape");
		}
		if (eraserPath == null)
		{
			throw new ArgumentNullException("eraserPath");
		}
		return Erase(EraseTest(eraserPath, eraserShape));
	}

	/// <summary>Returns a value that indicates whether current <see cref="T:System.Windows.Ink.Stroke" /> intersects the specified point.</summary>
	/// <returns>true if <paramref name="point" /> intersects the current stroke; otherwise, false.</returns>
	/// <param name="point">The <see cref="T:System.Windows.Point" /> to hit test.</param>
	public bool HitTest(Point point)
	{
		return HitTest(new Point[1] { point }, new EllipseStylusShape(TapHitPointSize, TapHitPointSize, TapHitRotation));
	}

	/// <summary>Returns a value that indicates whether current <see cref="T:System.Windows.Ink.Stroke" /> intersects the specified area.</summary>
	/// <returns>true if the specified area intersects the current stroke; otherwise, false.</returns>
	/// <param name="point">The <see cref="T:System.Windows.Point" /> that defines the center of the area to hit test.</param>
	/// <param name="diameter">The diameter of the area to hit test.</param>
	public bool HitTest(Point point, double diameter)
	{
		if (double.IsNaN(diameter) || diameter < DrawingAttributes.MinWidth || diameter > DrawingAttributes.MaxWidth)
		{
			throw new ArgumentOutOfRangeException("diameter", SR.InvalidDiameter);
		}
		return HitTest(new Point[1] { point }, new EllipseStylusShape(diameter, diameter, TapHitRotation));
	}

	/// <summary>Returns a value that indicates whether the <see cref="T:System.Windows.Ink.Stroke" /> is within the bounds of the specified rectangle.</summary>
	/// <returns>true if the current stroke is within the bounds of <paramref name="bounds" />; otherwise false.</returns>
	/// <param name="percentageWithinBounds">The percentage of the length of the <see cref="T:System.Windows.Ink.Stroke" />, that must be in <paramref name="percentageWithinBounds" /> for the <see cref="T:System.Windows.Ink.Stroke" /> to be considered hit..</param>
	public bool HitTest(Rect bounds, int percentageWithinBounds)
	{
		switch (percentageWithinBounds)
		{
		default:
			throw new ArgumentOutOfRangeException("percentageWithinBounds");
		case 0:
			return true;
		case 1:
		case 2:
		case 3:
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
		case 9:
		case 10:
		case 11:
		case 12:
		case 13:
		case 14:
		case 15:
		case 16:
		case 17:
		case 18:
		case 19:
		case 20:
		case 21:
		case 22:
		case 23:
		case 24:
		case 25:
		case 26:
		case 27:
		case 28:
		case 29:
		case 30:
		case 31:
		case 32:
		case 33:
		case 34:
		case 35:
		case 36:
		case 37:
		case 38:
		case 39:
		case 40:
		case 41:
		case 42:
		case 43:
		case 44:
		case 45:
		case 46:
		case 47:
		case 48:
		case 49:
		case 50:
		case 51:
		case 52:
		case 53:
		case 54:
		case 55:
		case 56:
		case 57:
		case 58:
		case 59:
		case 60:
		case 61:
		case 62:
		case 63:
		case 64:
		case 65:
		case 66:
		case 67:
		case 68:
		case 69:
		case 70:
		case 71:
		case 72:
		case 73:
		case 74:
		case 75:
		case 76:
		case 77:
		case 78:
		case 79:
		case 80:
		case 81:
		case 82:
		case 83:
		case 84:
		case 85:
		case 86:
		case 87:
		case 88:
		case 89:
		case 90:
		case 91:
		case 92:
		case 93:
		case 94:
		case 95:
		case 96:
		case 97:
		case 98:
		case 99:
		case 100:
		{
			StrokeInfo strokeInfo = null;
			try
			{
				strokeInfo = new StrokeInfo(this);
				StylusPointCollection stylusPoints = strokeInfo.StylusPoints;
				double num = strokeInfo.TotalWeight * (double)percentageWithinBounds / 100.0 - 0.0001;
				for (int i = 0; i < stylusPoints.Count; i++)
				{
					if (bounds.Contains((Point)stylusPoints[i]))
					{
						num -= strokeInfo.GetPointWeight(i);
						if (DoubleUtil.LessThanOrClose(num, 0.0))
						{
							return true;
						}
					}
				}
				return false;
			}
			finally
			{
				strokeInfo?.Detach();
			}
		}
		}
	}

	/// <summary>Returns a value that indicates whether the current <see cref="T:System.Windows.Ink.Stroke" /> is within the specified bounds.</summary>
	/// <returns>true if the current stroke is within the specified bounds; otherwise false.</returns>
	/// <param name="lassoPoints">An array of type <see cref="T:System.Windows.Point" /> that represents the bounds of the area to hit test.</param>
	/// <param name="percentageWithinLasso">The percentage of the length of the <see cref="T:System.Windows.Ink.Stroke" />, that must be in <paramref name="lassoPoints" /> for the <see cref="T:System.Windows.Ink.Stroke" /> to be considered hit.</param>
	public bool HitTest(IEnumerable<Point> lassoPoints, int percentageWithinLasso)
	{
		if (lassoPoints == null)
		{
			throw new ArgumentNullException("lassoPoints");
		}
		switch (percentageWithinLasso)
		{
		default:
			throw new ArgumentOutOfRangeException("percentageWithinLasso");
		case 0:
			return true;
		case 1:
		case 2:
		case 3:
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
		case 9:
		case 10:
		case 11:
		case 12:
		case 13:
		case 14:
		case 15:
		case 16:
		case 17:
		case 18:
		case 19:
		case 20:
		case 21:
		case 22:
		case 23:
		case 24:
		case 25:
		case 26:
		case 27:
		case 28:
		case 29:
		case 30:
		case 31:
		case 32:
		case 33:
		case 34:
		case 35:
		case 36:
		case 37:
		case 38:
		case 39:
		case 40:
		case 41:
		case 42:
		case 43:
		case 44:
		case 45:
		case 46:
		case 47:
		case 48:
		case 49:
		case 50:
		case 51:
		case 52:
		case 53:
		case 54:
		case 55:
		case 56:
		case 57:
		case 58:
		case 59:
		case 60:
		case 61:
		case 62:
		case 63:
		case 64:
		case 65:
		case 66:
		case 67:
		case 68:
		case 69:
		case 70:
		case 71:
		case 72:
		case 73:
		case 74:
		case 75:
		case 76:
		case 77:
		case 78:
		case 79:
		case 80:
		case 81:
		case 82:
		case 83:
		case 84:
		case 85:
		case 86:
		case 87:
		case 88:
		case 89:
		case 90:
		case 91:
		case 92:
		case 93:
		case 94:
		case 95:
		case 96:
		case 97:
		case 98:
		case 99:
		case 100:
		{
			StrokeInfo strokeInfo = null;
			try
			{
				strokeInfo = new StrokeInfo(this);
				StylusPointCollection stylusPoints = strokeInfo.StylusPoints;
				double num = strokeInfo.TotalWeight * (double)percentageWithinLasso / 100.0 - 0.0001;
				Lasso lasso = new SingleLoopLasso();
				lasso.AddPoints(lassoPoints);
				for (int i = 0; i < stylusPoints.Count; i++)
				{
					if (lasso.Contains((Point)stylusPoints[i]))
					{
						num -= strokeInfo.GetPointWeight(i);
						if (DoubleUtil.LessThan(num, 0.0))
						{
							return true;
						}
					}
				}
				return false;
			}
			finally
			{
				strokeInfo?.Detach();
			}
		}
		}
	}

	/// <summary>Returns whether the specified path intersects the <see cref="T:System.Windows.Ink.Stroke" /> using the specified <see cref="T:System.Windows.Ink.StylusShape" />.</summary>
	/// <returns>true if <paramref name="stylusShape" /> intersects the current stroke; otherwise, false.</returns>
	/// <param name="path">The path that <paramref name="stylusShape" /> follows for hit testing</param>
	/// <param name="stylusShape">The shape of <paramref name="path" /> with which to hit test.</param>
	public bool HitTest(IEnumerable<Point> path, StylusShape stylusShape)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (stylusShape == null)
		{
			throw new ArgumentNullException("stylusShape");
		}
		if (IEnumerablePointHelper.GetCount(path) == 0)
		{
			return false;
		}
		ErasingStroke erasingStroke = new ErasingStroke(stylusShape);
		erasingStroke.MoveTo(path);
		Rect bounds = erasingStroke.Bounds;
		if (bounds.IsEmpty)
		{
			return false;
		}
		if (bounds.IntersectsWith(GetBounds()))
		{
			return erasingStroke.HitTest(StrokeNodeIterator.GetIterator(this, DrawingAttributes));
		}
		return false;
	}

	/// <summary>Renders the <see cref="T:System.Windows.Ink.Stroke" /> on the specified <see cref="T:System.Windows.Media.DrawingContext" /> using the specified <see cref="T:Microsoft.Ink.DrawingAttributes" />.</summary>
	/// <param name="drawingContext">The <see cref="T:System.Windows.Media.DrawingContext" /> object onto which the stroke will be rendered.</param>
	/// <param name="drawingAttributes">The <see cref="T:Microsoft.Ink.DrawingAttributes" /> object defining the attributes of the stroke that is drawn.</param>
	protected virtual void DrawCore(DrawingContext drawingContext, DrawingAttributes drawingAttributes)
	{
		if (drawingContext == null)
		{
			throw new ArgumentNullException("drawingContext");
		}
		if (null == drawingAttributes)
		{
			throw new ArgumentNullException("drawingAttributes");
		}
		if (_drawAsHollow)
		{
			DrawingAttributes drawingAttributes2 = drawingAttributes.Clone();
			drawingAttributes2.Height = Math.Max(drawingAttributes2.Height, 2.0031496062992127);
			drawingAttributes2.Width = Math.Max(drawingAttributes2.Width, 2.0031496062992127);
			CalcHollowTransforms(drawingAttributes2, out var innerTransform, out var outerTransform);
			drawingAttributes2.StylusTipTransform = outerTransform;
			SolidColorBrush solidColorBrush = new SolidColorBrush(drawingAttributes.Color);
			solidColorBrush.Freeze();
			drawingContext.DrawGeometry(solidColorBrush, null, GetGeometry(drawingAttributes2));
			drawingAttributes2.StylusTipTransform = innerTransform;
			drawingContext.DrawGeometry(Brushes.White, null, GetGeometry(drawingAttributes2));
		}
		else
		{
			SolidColorBrush solidColorBrush2 = new SolidColorBrush(drawingAttributes.Color);
			solidColorBrush2.Freeze();
			drawingContext.DrawGeometry(solidColorBrush2, null, GetGeometry(drawingAttributes));
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.Media.Geometry" /> of the current <see cref="T:System.Windows.Ink.Stroke" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Geometry" /> that represents the <see cref="T:System.Windows.Ink.Stroke" />.</returns>
	public Geometry GetGeometry()
	{
		return GetGeometry(DrawingAttributes);
	}

	/// <summary>Gets the <see cref="T:System.Windows.Media.Geometry" /> of the current <see cref="T:System.Windows.Ink.Stroke" /> using the specified <see cref="T:System.Windows.Ink.DrawingAttributes" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Geometry" /> that represents the <see cref="T:System.Windows.Ink.Stroke" />.</returns>
	/// <param name="drawingAttributes">The <see cref="T:System.Windows.Ink.DrawingAttributes" /> that determines the <see cref="T:System.Windows.Media.Geometry" /> of the <see cref="T:System.Windows.Ink.Stroke" />.</param>
	public Geometry GetGeometry(DrawingAttributes drawingAttributes)
	{
		if (drawingAttributes == null)
		{
			throw new ArgumentNullException("drawingAttributes");
		}
		bool flag = DrawingAttributes.GeometricallyEqual(drawingAttributes, DrawingAttributes);
		if (!flag || (flag && _cachedGeometry == null))
		{
			StrokeRenderer.CalcGeometryAndBounds(StrokeNodeIterator.GetIterator(this, drawingAttributes), drawingAttributes, calculateBounds: true, out var geometry, out var bounds);
			if (!flag)
			{
				return geometry;
			}
			SetGeometry(geometry);
			SetBounds(bounds);
			return geometry;
		}
		return _cachedGeometry;
	}

	[FriendAccessAllowed]
	internal void DrawInternal(DrawingContext dc, DrawingAttributes DrawingAttributes, bool drawAsHollow)
	{
		if (drawAsHollow)
		{
			try
			{
				_drawAsHollow = true;
				DrawCore(dc, DrawingAttributes);
				return;
			}
			finally
			{
				_drawAsHollow = false;
			}
		}
		DrawCore(dc, DrawingAttributes);
	}

	internal void SetGeometry(Geometry geometry)
	{
		_cachedGeometry = geometry;
	}

	internal void SetBounds(Rect newBounds)
	{
		_cachedBounds = newBounds;
	}

	internal StrokeIntersection[] EraseTest(IEnumerable<Point> path, StylusShape shape)
	{
		if (IEnumerablePointHelper.GetCount(path) == 0)
		{
			return Array.Empty<StrokeIntersection>();
		}
		ErasingStroke erasingStroke = new ErasingStroke(shape, path);
		List<StrokeIntersection> list = new List<StrokeIntersection>();
		erasingStroke.EraseTest(StrokeNodeIterator.GetIterator(this, DrawingAttributes), list);
		return list.ToArray();
	}

	internal StrokeIntersection[] HitTest(Lasso lasso)
	{
		if (lasso.IsEmpty)
		{
			return Array.Empty<StrokeIntersection>();
		}
		if (!lasso.Bounds.IntersectsWith(GetBounds()))
		{
			return Array.Empty<StrokeIntersection>();
		}
		return lasso.HitTest(StrokeNodeIterator.GetIterator(this, DrawingAttributes));
	}

	internal StrokeCollection Erase(StrokeIntersection[] cutAt)
	{
		if (cutAt.Length == 0)
		{
			return new StrokeCollection { Clone() };
		}
		StrokeFIndices[] hitSegments = StrokeIntersection.GetHitSegments(cutAt);
		return Erase(hitSegments);
	}

	internal StrokeCollection Clip(StrokeIntersection[] cutAt)
	{
		if (cutAt.Length == 0)
		{
			return new StrokeCollection();
		}
		StrokeFIndices[] inSegments = StrokeIntersection.GetInSegments(cutAt);
		if (inSegments.Length == 0)
		{
			return new StrokeCollection();
		}
		return Clip(inSegments);
	}

	private static void CalcHollowTransforms(DrawingAttributes originalDa, out Matrix innerTransform, out Matrix outerTransform)
	{
		innerTransform = (outerTransform = Matrix.Identity);
		Point point = originalDa.StylusTipTransform.Transform(new Point(originalDa.Width, 0.0));
		Point point2 = originalDa.StylusTipTransform.Transform(new Point(0.0, originalDa.Height));
		double num = Math.Sqrt(point.X * point.X + point.Y * point.Y);
		double num2 = Math.Sqrt(point2.X * point2.X + point2.Y * point2.Y);
		double scaleX = (DoubleUtil.GreaterThan(num, 1.0) ? ((num - 1.0) / num) : 1.0);
		double scaleY = (DoubleUtil.GreaterThan(num2, 1.0) ? ((num2 - 1.0) / num2) : 1.0);
		innerTransform.Scale(scaleX, scaleY);
		innerTransform *= originalDa.StylusTipTransform;
		outerTransform.Scale((num + 1.0) / num, (num2 + 1.0) / num2);
		outerTransform *= originalDa.StylusTipTransform;
	}
}
