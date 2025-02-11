using System.ComponentModel;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Ink;

/// <summary>Specifies the appearance of a <see cref="T:System.Windows.Ink.Stroke" /></summary>
public class DrawingAttributes : INotifyPropertyChanged
{
	private PropertyChangedEventHandler _propertyChanged;

	private ExtendedPropertyCollection _extendedProperties;

	private uint _v1RasterOperation = 13u;

	private bool _heightChangedForCompatabity;

	internal const float StylusPrecision = 1000f;

	internal const double DefaultWidth = 2.0031496062992127;

	internal const double DefaultHeight = 2.0031496062992127;

	/// <summary>Specifies the smallest value allowed for the <see cref="P:System.Windows.Ink.DrawingAttributes.Height" /> property.</summary>
	public static readonly double MinHeight = 3.77952755905512E-05;

	/// <summary>Specifies the smallest value allowed for the <see cref="P:System.Windows.Ink.DrawingAttributes.Width" /> property.</summary>
	public static readonly double MinWidth = 3.77952755905512E-05;

	/// <summary>Specifies the largest value allowed for the <see cref="P:System.Windows.Ink.DrawingAttributes.Height" /> property.</summary>
	public static readonly double MaxHeight = 162329.461417323;

	/// <summary>Specifies the largest value allowed for the <see cref="P:System.Windows.Ink.DrawingAttributes.Width" /> property.</summary>
	public static readonly double MaxWidth = 162329.461417323;

	/// <summary>Gets or sets the color of a <see cref="T:System.Windows.Ink.Stroke" />.</summary>
	/// <returns>The color of a <see cref="T:System.Windows.Ink.Stroke" />.</returns>
	public Color Color
	{
		get
		{
			if (!_extendedProperties.Contains(KnownIds.Color))
			{
				return Colors.Black;
			}
			return (Color)GetExtendedPropertyBackedProperty(KnownIds.Color);
		}
		set
		{
			SetExtendedPropertyBackedProperty(KnownIds.Color, value);
		}
	}

	/// <summary>Gets or sets the shape of the stylus used to draw the <see cref="T:System.Windows.Ink.Stroke" />.</summary>
	/// <returns>One of the <see cref="T:System.Windows.Ink.StylusShape" /> values.</returns>
	public StylusTip StylusTip
	{
		get
		{
			if (!_extendedProperties.Contains(KnownIds.StylusTip))
			{
				return StylusTip.Ellipse;
			}
			return StylusTip.Rectangle;
		}
		set
		{
			SetExtendedPropertyBackedProperty(KnownIds.StylusTip, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Matrix" /> that specifies the transformation to perform on the stylus' tip.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Matrix" /> that specifies the transformation to perform on the stylus' tip.</returns>
	/// <exception cref="T:System.ArgumentException">The matrix set to <see cref="P:System.Windows.Ink.DrawingAttributes.StylusTipTransform" /> is not an invertible matrix.-or-The <see cref="P:System.Windows.Media.Matrix.OffsetX" /> or <see cref="P:System.Windows.Media.Matrix.OffsetY" /> property of the matrix is not zero.</exception>
	public Matrix StylusTipTransform
	{
		get
		{
			if (!_extendedProperties.Contains(KnownIds.StylusTipTransform))
			{
				return Matrix.Identity;
			}
			return (Matrix)GetExtendedPropertyBackedProperty(KnownIds.StylusTipTransform);
		}
		set
		{
			Matrix matrix = value;
			if (matrix.OffsetX != 0.0 || matrix.OffsetY != 0.0)
			{
				throw new ArgumentException(SR.InvalidSttValue, "value");
			}
			SetExtendedPropertyBackedProperty(KnownIds.StylusTipTransform, value);
		}
	}

	/// <summary>Gets or sets the height of the stylus used to draw the <see cref="T:System.Windows.Ink.Stroke" />.</summary>
	/// <returns>The value that indicates the height of the stylus used to draw the <see cref="T:System.Windows.Ink.Stroke" />.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <see cref="P:System.Windows.Ink.DrawingAttributes.Height" /> property is less than <see cref="F:System.Double.Epsilon" /> or <see cref="F:System.Double.NaN" />.</exception>
	public double Height
	{
		get
		{
			if (!_extendedProperties.Contains(KnownIds.StylusHeight))
			{
				return 2.0031496062992127;
			}
			return (double)GetExtendedPropertyBackedProperty(KnownIds.StylusHeight);
		}
		set
		{
			if (double.IsNaN(value) || value < MinHeight || value > MaxHeight)
			{
				throw new ArgumentOutOfRangeException("Height", SR.InvalidDrawingAttributesHeight);
			}
			SetExtendedPropertyBackedProperty(KnownIds.StylusHeight, value);
		}
	}

	/// <summary>Gets or sets the width of the stylus used to draw the <see cref="T:System.Windows.Ink.Stroke" />.</summary>
	/// <returns>The width of the stylus used to draw the <see cref="T:System.Windows.Ink.Stroke" />.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <see cref="P:System.Windows.Ink.DrawingAttributes.Width" /> property is less than <see cref="F:System.Double.Epsilon" /> or <see cref="F:System.Double.NaN" />.</exception>
	public double Width
	{
		get
		{
			if (!_extendedProperties.Contains(KnownIds.StylusWidth))
			{
				return 2.0031496062992127;
			}
			return (double)GetExtendedPropertyBackedProperty(KnownIds.StylusWidth);
		}
		set
		{
			if (double.IsNaN(value) || value < MinWidth || value > MaxWidth)
			{
				throw new ArgumentOutOfRangeException("Width", SR.InvalidDrawingAttributesWidth);
			}
			SetExtendedPropertyBackedProperty(KnownIds.StylusWidth, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether Bezier smoothing is used to render the <see cref="T:System.Windows.Ink.Stroke" />.</summary>
	/// <returns>true to use Bezier smoothing to render the <see cref="T:System.Windows.Ink.Stroke" />; otherwise false. The default is false.</returns>
	public bool FitToCurve
	{
		get
		{
			DrawingFlags drawingFlags = (DrawingFlags)GetExtendedPropertyBackedProperty(KnownIds.DrawingFlags);
			return (drawingFlags & DrawingFlags.FitToCurve) != 0;
		}
		set
		{
			DrawingFlags drawingFlags = (DrawingFlags)GetExtendedPropertyBackedProperty(KnownIds.DrawingFlags);
			drawingFlags = ((!value) ? (drawingFlags & ~DrawingFlags.FitToCurve) : (drawingFlags | DrawingFlags.FitToCurve));
			SetExtendedPropertyBackedProperty(KnownIds.DrawingFlags, drawingFlags);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the thickness of a rendered <see cref="T:System.Windows.Ink.Stroke" /> changes according the amount of pressure applied.</summary>
	/// <returns>true to indicate that the thickness of the stroke is uniform; false to indicate that the thickness of a rendered <see cref="T:System.Windows.Ink.Stroke" /> increases when pressure is increased. The default is false.</returns>
	public bool IgnorePressure
	{
		get
		{
			DrawingFlags drawingFlags = (DrawingFlags)GetExtendedPropertyBackedProperty(KnownIds.DrawingFlags);
			return (drawingFlags & DrawingFlags.IgnorePressure) != 0;
		}
		set
		{
			DrawingFlags drawingFlags = (DrawingFlags)GetExtendedPropertyBackedProperty(KnownIds.DrawingFlags);
			drawingFlags = ((!value) ? (drawingFlags & ~DrawingFlags.IgnorePressure) : (drawingFlags | DrawingFlags.IgnorePressure));
			SetExtendedPropertyBackedProperty(KnownIds.DrawingFlags, drawingFlags);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Ink.Stroke" /> looks like a highlighter.</summary>
	/// <returns>true to render the <see cref="T:System.Windows.Ink.Stroke" /> as a highlighter; otherwise, false. The default is false.</returns>
	public bool IsHighlighter
	{
		get
		{
			if (!_extendedProperties.Contains(KnownIds.IsHighlighter))
			{
				return false;
			}
			return true;
		}
		set
		{
			SetExtendedPropertyBackedProperty(KnownIds.IsHighlighter, value);
			if (value)
			{
				_v1RasterOperation = 9u;
			}
			else
			{
				_v1RasterOperation = 13u;
			}
		}
	}

	internal ExtendedPropertyCollection ExtendedProperties => _extendedProperties;

	internal StylusShape StylusShape
	{
		get
		{
			StylusShape stylusShape = ((StylusTip != 0) ? ((StylusShape)new EllipseStylusShape(Width, Height)) : ((StylusShape)new RectangleStylusShape(Width, Height)));
			stylusShape.Transform = StylusTipTransform;
			return stylusShape;
		}
	}

	internal int FittingError
	{
		get
		{
			if (!_extendedProperties.Contains(KnownIds.CurveFittingError))
			{
				return 0;
			}
			return (int)_extendedProperties[KnownIds.CurveFittingError];
		}
		set
		{
			_extendedProperties[KnownIds.CurveFittingError] = value;
		}
	}

	internal DrawingFlags DrawingFlags
	{
		get
		{
			return (DrawingFlags)GetExtendedPropertyBackedProperty(KnownIds.DrawingFlags);
		}
		set
		{
			SetExtendedPropertyBackedProperty(KnownIds.DrawingFlags, value);
		}
	}

	internal uint RasterOperation
	{
		get
		{
			return _v1RasterOperation;
		}
		set
		{
			_v1RasterOperation = value;
		}
	}

	internal bool HeightChangedForCompatabity
	{
		get
		{
			return _heightChangedForCompatabity;
		}
		set
		{
			_heightChangedForCompatabity = value;
		}
	}

	/// <summary>Occurs when the value of any <see cref="T:System.Windows.Ink.DrawingAttributes" /> property has changed.</summary>
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

	/// <summary>Occurs when a property in the <see cref="T:System.Windows.Ink.DrawingAttributes" /> object changes.</summary>
	public event PropertyDataChangedEventHandler AttributeChanged;

	/// <summary>Occurs when property data is added or removed from the <see cref="T:System.Windows.Ink.StrokeCollection" />.</summary>
	public event PropertyDataChangedEventHandler PropertyDataChanged;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Ink.DrawingAttributes" /> class. </summary>
	public DrawingAttributes()
	{
		_extendedProperties = new ExtendedPropertyCollection();
		Initialize();
	}

	internal DrawingAttributes(ExtendedPropertyCollection extendedProperties)
	{
		_extendedProperties = extendedProperties;
		Initialize();
	}

	private void Initialize()
	{
		_extendedProperties.Changed += ExtendedPropertiesChanged_EventForwarder;
	}

	/// <summary>Adds a custom property to the <see cref="T:System.Windows.Ink.DrawingAttributes" /> object.</summary>
	/// <param name="propertyDataId">The <see cref="T:System.Guid" /> to associate with the custom property.</param>
	/// <param name="propertyData">The value of the custom property. <paramref name="propertyData" /> must be of type <see cref="T:System.Char" />, <see cref="T:System.Byte" />, <see cref="T:System.Int16" />, <see cref="T:System.UInt16" />, <see cref="T:System.Int32" />, <see cref="T:System.UInt32" />, <see cref="T:System.Int64" />, <see cref="T:System.UInt64" />, <see cref="T:System.Single" />, <see cref="T:System.Double" />, <see cref="T:System.DateTime" />, <see cref="T:System.Boolean" />, <see cref="T:System.String" />, <see cref="T:System.Decimal" /> or an array of these data types; however it cannot be an array of type <see cref="T:System.String" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="propertyData" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="propertyDataId" /> is an empty <see cref="T:System.Guid" />.-or-<paramref name="propertyData" /> is not one of the allowed data types listed in the Parameters section.</exception>
	public void AddPropertyData(Guid propertyDataId, object propertyData)
	{
		ValidateStylusTipTransform(propertyDataId, propertyData);
		SetExtendedPropertyBackedProperty(propertyDataId, propertyData);
	}

	/// <summary>Removes the custom property associated with the specified <see cref="T:System.Guid" />.</summary>
	/// <param name="propertyDataId">The <see cref="T:System.Guid" /> associated with the custom property to remove.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="propertyDataId" /> is not associated with a custom property of the <see cref="T:System.Windows.Ink.DrawingAttributes" /> object.</exception>
	public void RemovePropertyData(Guid propertyDataId)
	{
		ExtendedProperties.Remove(propertyDataId);
	}

	/// <summary>Gets the value of the custom property associated with the specified <see cref="T:System.Guid" />.</summary>
	/// <returns>The value of the custom property associated with the specified <see cref="T:System.Guid" />.</returns>
	/// <param name="propertyDataId">The <see cref="T:System.Guid" /> associated with the custom property to get.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="propertyDataId" /> is not associated with a custom property of the <see cref="T:System.Windows.Ink.DrawingAttributes" /> object.</exception>
	public object GetPropertyData(Guid propertyDataId)
	{
		return GetExtendedPropertyBackedProperty(propertyDataId);
	}

	/// <summary>Returns the GUIDs of any custom properties associated with the <see cref="T:System.Windows.Ink.StrokeCollection" />.</summary>
	/// <returns>An array of type <see cref="T:System.Guid" /> that represents the property data identifiers.</returns>
	public Guid[] GetPropertyDataIds()
	{
		return ExtendedProperties.GetGuidArray();
	}

	/// <summary>Returns a value that indicates whether the specified property data identifier is in the <see cref="T:System.Windows.Ink.DrawingAttributes" /> object.</summary>
	/// <returns>true if the specified property data identifier is in the <see cref="T:System.Windows.Ink.DrawingAttributes" /> object; otherwise, false.</returns>
	/// <param name="propertyDataId">The <see cref="T:System.Guid" /> to locate in the <see cref="T:System.Windows.Ink.DrawingAttributes" /> object .</param>
	public bool ContainsPropertyData(Guid propertyDataId)
	{
		return ExtendedProperties.Contains(propertyDataId);
	}

	internal ExtendedPropertyCollection CopyPropertyData()
	{
		return ExtendedProperties.Clone();
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Determines whether the specified <see cref="T:System.Windows.Ink.DrawingAttributes" /> object is equal to the current <see cref="T:System.Windows.Ink.DrawingAttributes" /> object. </summary>
	/// <returns>true if the objects are equal; otherwise, false.</returns>
	/// <param name="o">The <see cref="T:System.Windows.Ink.DrawingAttributes" /> object to compare to the current <see cref="T:System.Windows.Ink.DrawingAttributes" /> object.</param>
	public override bool Equals(object o)
	{
		if (o == null || o.GetType() != GetType())
		{
			return false;
		}
		DrawingAttributes drawingAttributes = o as DrawingAttributes;
		if (drawingAttributes == null)
		{
			return false;
		}
		return _extendedProperties == drawingAttributes._extendedProperties;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Windows.Ink.DrawingAttributes" /> objects are equal.</summary>
	/// <returns>true if the objects are equal; otherwise, false.</returns>
	/// <param name="first">The first <see cref="T:System.Windows.Ink.DrawingAttributes" /> object to compare.</param>
	/// <param name="second">The second <see cref="T:System.Windows.Ink.DrawingAttributes" /> object to compare.</param>
	public static bool operator ==(DrawingAttributes first, DrawingAttributes second)
	{
		if (((object)first == null && (object)second == null) || (object)first == second)
		{
			return true;
		}
		if ((object)first == null || (object)second == null)
		{
			return false;
		}
		return first.Equals(second);
	}

	/// <summary>Determines whether the specified <see cref="T:System.Windows.Ink.DrawingAttributes" /> objects are not equal.</summary>
	/// <returns>true if the objects are not equal; otherwise, false.</returns>
	/// <param name="first">The first <see cref="T:System.Windows.Ink.DrawingAttributes" /> object to compare.</param>
	/// <param name="second">The second <see cref="T:System.Windows.Ink.DrawingAttributes" /> object to compare.</param>
	public static bool operator !=(DrawingAttributes first, DrawingAttributes second)
	{
		return !(first == second);
	}

	/// <summary>Copies the <see cref="T:System.Windows.Ink.DrawingAttributes" /> object.</summary>
	/// <returns>A copy of the <see cref="T:System.Windows.Ink.DrawingAttributes" /> object.</returns>
	public virtual DrawingAttributes Clone()
	{
		DrawingAttributes obj = (DrawingAttributes)MemberwiseClone();
		obj.AttributeChanged = null;
		obj.PropertyDataChanged = null;
		obj._extendedProperties = _extendedProperties.Clone();
		obj.Initialize();
		return obj;
	}

	/// <summary>Raises the <see cref="E:System.Windows.Ink.DrawingAttributes.AttributeChanged" /> event. </summary>
	/// <param name="e">A <see cref="T:System.Windows.Ink.PropertyDataChangedEventArgs" /> that contains the event data. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="e" /> is null.</exception>
	protected virtual void OnAttributeChanged(PropertyDataChangedEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e", SR.EventArgIsNull);
		}
		try
		{
			PrivateNotifyPropertyChanged(e);
		}
		finally
		{
			if (this.AttributeChanged != null)
			{
				this.AttributeChanged(this, e);
			}
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Ink.DrawingAttributes.PropertyDataChanged" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Ink.PropertyDataChangedEventArgs" /> that contains the event data.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="e" /> is null.</exception>
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

	/// <summary>Occurs when any <see cref="T:System.Windows.Ink.DrawingAttributes" /> property changes.</summary>
	/// <param name="e">EventArgs</param>
	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		if (_propertyChanged != null)
		{
			_propertyChanged(this, e);
		}
	}

	internal static object GetDefaultDrawingAttributeValue(Guid id)
	{
		if (KnownIds.Color == id)
		{
			return Colors.Black;
		}
		if (KnownIds.StylusWidth == id)
		{
			return 2.0031496062992127;
		}
		if (KnownIds.StylusTip == id)
		{
			return StylusTip.Ellipse;
		}
		if (KnownIds.DrawingFlags == id)
		{
			return DrawingFlags.AntiAliased;
		}
		if (KnownIds.StylusHeight == id)
		{
			return 2.0031496062992127;
		}
		if (KnownIds.StylusTipTransform == id)
		{
			return Matrix.Identity;
		}
		if (KnownIds.IsHighlighter == id)
		{
			return false;
		}
		return null;
	}

	internal static void ValidateStylusTipTransform(Guid propertyDataId, object propertyData)
	{
		if (propertyData == null)
		{
			throw new ArgumentNullException("propertyData");
		}
		if (propertyDataId == KnownIds.StylusTipTransform && propertyData.GetType() == typeof(string))
		{
			throw new ArgumentException(SR.Format(SR.InvalidValueType, typeof(Matrix)), "propertyData");
		}
	}

	internal static bool RemoveIdFromExtendedProperties(Guid id)
	{
		if (KnownIds.Color == id || KnownIds.Transparency == id || KnownIds.StylusWidth == id || KnownIds.DrawingFlags == id || KnownIds.StylusHeight == id || KnownIds.CurveFittingError == id)
		{
			return true;
		}
		return false;
	}

	internal static bool GeometricallyEqual(DrawingAttributes left, DrawingAttributes right)
	{
		if ((object)left == right)
		{
			return true;
		}
		if (left.StylusTip == right.StylusTip && left.StylusTipTransform == right.StylusTipTransform && DoubleUtil.AreClose(left.Width, right.Width) && DoubleUtil.AreClose(left.Height, right.Height) && left.DrawingFlags == right.DrawingFlags)
		{
			return true;
		}
		return false;
	}

	internal static bool IsGeometricalDaGuid(Guid guid)
	{
		if (guid == KnownIds.StylusHeight || guid == KnownIds.StylusWidth || guid == KnownIds.StylusTipTransform || guid == KnownIds.StylusTip || guid == KnownIds.DrawingFlags)
		{
			return true;
		}
		return false;
	}

	private void ExtendedPropertiesChanged_EventForwarder(object sender, ExtendedPropertiesChangedEventArgs args)
	{
		if (args.NewProperty == null)
		{
			object defaultDrawingAttributeValue = GetDefaultDrawingAttributeValue(args.OldProperty.Id);
			if (defaultDrawingAttributeValue != null)
			{
				ExtendedProperty extendedProperty = new ExtendedProperty(args.OldProperty.Id, defaultDrawingAttributeValue);
				PropertyDataChangedEventArgs e = new PropertyDataChangedEventArgs(args.OldProperty.Id, extendedProperty.Value, args.OldProperty.Value);
				OnAttributeChanged(e);
			}
			else
			{
				PropertyDataChangedEventArgs e2 = new PropertyDataChangedEventArgs(args.OldProperty.Id, null, args.OldProperty.Value);
				OnPropertyDataChanged(e2);
			}
		}
		else if (args.OldProperty == null)
		{
			object defaultDrawingAttributeValue2 = GetDefaultDrawingAttributeValue(args.NewProperty.Id);
			if (defaultDrawingAttributeValue2 != null)
			{
				if (!defaultDrawingAttributeValue2.Equals(args.NewProperty.Value))
				{
					PropertyDataChangedEventArgs e3 = new PropertyDataChangedEventArgs(args.NewProperty.Id, args.NewProperty.Value, defaultDrawingAttributeValue2);
					OnAttributeChanged(e3);
				}
			}
			else
			{
				PropertyDataChangedEventArgs e4 = new PropertyDataChangedEventArgs(args.NewProperty.Id, args.NewProperty.Value, null);
				OnPropertyDataChanged(e4);
			}
		}
		else if (GetDefaultDrawingAttributeValue(args.NewProperty.Id) != null)
		{
			if (!args.NewProperty.Value.Equals(args.OldProperty.Value))
			{
				PropertyDataChangedEventArgs e5 = new PropertyDataChangedEventArgs(args.NewProperty.Id, args.NewProperty.Value, args.OldProperty.Value);
				OnAttributeChanged(e5);
			}
		}
		else if (!args.NewProperty.Value.Equals(args.OldProperty.Value))
		{
			PropertyDataChangedEventArgs e6 = new PropertyDataChangedEventArgs(args.NewProperty.Id, args.NewProperty.Value, args.OldProperty.Value);
			OnPropertyDataChanged(e6);
		}
	}

	private void SetExtendedPropertyBackedProperty(Guid id, object value)
	{
		if (_extendedProperties.Contains(id))
		{
			object defaultDrawingAttributeValue = GetDefaultDrawingAttributeValue(id);
			if (defaultDrawingAttributeValue != null && defaultDrawingAttributeValue.Equals(value))
			{
				_extendedProperties.Remove(id);
			}
			else if (!GetExtendedPropertyBackedProperty(id).Equals(value))
			{
				_extendedProperties[id] = value;
			}
		}
		else
		{
			object defaultDrawingAttributeValue2 = GetDefaultDrawingAttributeValue(id);
			if (defaultDrawingAttributeValue2 == null || !defaultDrawingAttributeValue2.Equals(value))
			{
				_extendedProperties[id] = value;
			}
		}
	}

	private object GetExtendedPropertyBackedProperty(Guid id)
	{
		if (!_extendedProperties.Contains(id))
		{
			if (GetDefaultDrawingAttributeValue(id) != null)
			{
				return GetDefaultDrawingAttributeValue(id);
			}
			throw new ArgumentException(SR.EPGuidNotFound, "id");
		}
		return _extendedProperties[id];
	}

	private void PrivateNotifyPropertyChanged(PropertyDataChangedEventArgs e)
	{
		if (e.PropertyGuid == KnownIds.Color)
		{
			OnPropertyChanged("Color");
		}
		else if (e.PropertyGuid == KnownIds.StylusTip)
		{
			OnPropertyChanged("StylusTip");
		}
		else if (e.PropertyGuid == KnownIds.StylusTipTransform)
		{
			OnPropertyChanged("StylusTipTransform");
		}
		else if (e.PropertyGuid == KnownIds.StylusHeight)
		{
			OnPropertyChanged("Height");
		}
		else if (e.PropertyGuid == KnownIds.StylusWidth)
		{
			OnPropertyChanged("Width");
		}
		else if (e.PropertyGuid == KnownIds.IsHighlighter)
		{
			OnPropertyChanged("IsHighlighter");
		}
		else if (e.PropertyGuid == KnownIds.DrawingFlags)
		{
			DrawingFlags num = (DrawingFlags)e.PreviousValue ^ (DrawingFlags)e.NewValue;
			if ((num & DrawingFlags.FitToCurve) != 0)
			{
				OnPropertyChanged("FitToCurve");
			}
			if ((num & DrawingFlags.IgnorePressure) != 0)
			{
				OnPropertyChanged("IgnorePressure");
			}
		}
	}

	private void OnPropertyChanged(string propertyName)
	{
		OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
	}
}
