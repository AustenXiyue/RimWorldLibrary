using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Markup;
using MS.Internal;

namespace System.Windows.Controls;

/// <summary>Represents a column that displays data.</summary>
[ContentProperty("Header")]
[StyleTypedProperty(Property = "HeaderContainerStyle", StyleTargetType = typeof(GridViewColumnHeader))]
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public class GridViewColumn : DependencyObject, INotifyPropertyChanged
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridViewColumn.Header" /> dependency property. </summary>
	public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(object), typeof(GridViewColumn), new FrameworkPropertyMetadata(OnHeaderChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridViewColumn.HeaderContainerStyle" /> dependency property. </summary>
	public static readonly DependencyProperty HeaderContainerStyleProperty = DependencyProperty.Register("HeaderContainerStyle", typeof(Style), typeof(GridViewColumn), new FrameworkPropertyMetadata(OnHeaderContainerStyleChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridViewColumn.HeaderTemplate" /> dependency property. </summary>
	public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(GridViewColumn), new FrameworkPropertyMetadata(OnHeaderTemplateChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridViewColumn.HeaderTemplateSelector" /> dependency property. </summary>
	public static readonly DependencyProperty HeaderTemplateSelectorProperty = DependencyProperty.Register("HeaderTemplateSelector", typeof(DataTemplateSelector), typeof(GridViewColumn), new FrameworkPropertyMetadata(OnHeaderTemplateSelectorChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridViewColumn.HeaderStringFormat" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.GridViewColumn.HeaderStringFormat" /> dependency property.</returns>
	public static readonly DependencyProperty HeaderStringFormatProperty = DependencyProperty.Register("HeaderStringFormat", typeof(string), typeof(GridViewColumn), new FrameworkPropertyMetadata(null, OnHeaderStringFormatChanged));

	private BindingBase _displayMemberBinding;

	internal const string c_DisplayMemberBindingName = "DisplayMemberBinding";

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridViewColumn.CellTemplate" /> dependency property. </summary>
	public static readonly DependencyProperty CellTemplateProperty = DependencyProperty.Register("CellTemplate", typeof(DataTemplate), typeof(GridViewColumn), new PropertyMetadata(OnCellTemplateChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridViewColumn.CellTemplateSelector" /> dependency property. </summary>
	public static readonly DependencyProperty CellTemplateSelectorProperty = DependencyProperty.Register("CellTemplateSelector", typeof(DataTemplateSelector), typeof(GridViewColumn), new PropertyMetadata(OnCellTemplateSelectorChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridViewColumn.Width" /> dependency property. </summary>
	public static readonly DependencyProperty WidthProperty = FrameworkElement.WidthProperty.AddOwner(typeof(GridViewColumn), new PropertyMetadata(double.NaN, OnWidthChanged));

	internal const string c_ActualWidthName = "ActualWidth";

	private DependencyObject _inheritanceContext;

	private double _desiredWidth;

	private int _actualIndex;

	private double _actualWidth;

	private ColumnMeasureState _state;

	/// <summary>Gets or sets the content of the header of a <see cref="T:System.Windows.Controls.GridViewColumn" />. </summary>
	/// <returns>The object to use for the column header. The default is null.</returns>
	public object Header
	{
		get
		{
			return GetValue(HeaderProperty);
		}
		set
		{
			SetValue(HeaderProperty, value);
		}
	}

	/// <summary>Gets or sets the style to use for the header of the <see cref="T:System.Windows.Controls.GridViewColumn" />. </summary>
	/// <returns>The <see cref="T:System.Windows.Style" /> that defines the display properties for the column header. The default is null.</returns>
	public Style HeaderContainerStyle
	{
		get
		{
			return (Style)GetValue(HeaderContainerStyleProperty);
		}
		set
		{
			SetValue(HeaderContainerStyleProperty, value);
		}
	}

	/// <summary>Gets or sets the template to use to display the content of the column header. </summary>
	/// <returns>A <see cref="T:System.Windows.DataTemplate" /> to use to display the column header. The default is null.</returns>
	public DataTemplate HeaderTemplate
	{
		get
		{
			return (DataTemplate)GetValue(HeaderTemplateProperty);
		}
		set
		{
			SetValue(HeaderTemplateProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Controls.DataTemplateSelector" /> that provides logic to select the template to use to display the column header. </summary>
	/// <returns>The <see cref="T:System.Windows.Controls.DataTemplateSelector" /> object that provides data template selection for each <see cref="T:System.Windows.Controls.GridViewColumn" />. The default is null.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public DataTemplateSelector HeaderTemplateSelector
	{
		get
		{
			return (DataTemplateSelector)GetValue(HeaderTemplateSelectorProperty);
		}
		set
		{
			SetValue(HeaderTemplateSelectorProperty, value);
		}
	}

	/// <summary>Gets or sets a composite string that specifies how to format the <see cref="P:System.Windows.Controls.GridViewColumn.Header" /> property if it is displayed as a string.</summary>
	/// <returns>A composite string that specifies how to format the <see cref="P:System.Windows.Controls.GridViewColumn.Header" /> property if it is displayed as a string. The default is null.</returns>
	public string HeaderStringFormat
	{
		get
		{
			return (string)GetValue(HeaderStringFormatProperty);
		}
		set
		{
			SetValue(HeaderStringFormatProperty, value);
		}
	}

	/// <summary>Gets or sets the data item to bind to for this column.</summary>
	/// <returns>The specified data item type that displays in the column. The default is null.</returns>
	public BindingBase DisplayMemberBinding
	{
		get
		{
			return _displayMemberBinding;
		}
		set
		{
			if (_displayMemberBinding != value)
			{
				_displayMemberBinding = value;
				OnDisplayMemberBindingChanged();
			}
		}
	}

	/// <summary>Gets or sets the template to use to display the contents of a column cell. </summary>
	/// <returns>A <see cref="T:System.Windows.DataTemplate" /> that is used to format a column cell. The default is null.</returns>
	public DataTemplate CellTemplate
	{
		get
		{
			return (DataTemplate)GetValue(CellTemplateProperty);
		}
		set
		{
			SetValue(CellTemplateProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Controls.DataTemplateSelector" /> that determines the template to use to display cells in a column. </summary>
	/// <returns>A <see cref="T:System.Windows.Controls.DataTemplateSelector" /> that provides <see cref="T:System.Windows.DataTemplate" /> selection for column cells. The default is null.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public DataTemplateSelector CellTemplateSelector
	{
		get
		{
			return (DataTemplateSelector)GetValue(CellTemplateSelectorProperty);
		}
		set
		{
			SetValue(CellTemplateSelectorProperty, value);
		}
	}

	/// <summary>Gets or sets the width of the column. </summary>
	/// <returns>The width of the column. The default is <see cref="F:System.Double.NaN" />, which automatically sizes to the largest column item that is not the column header.</returns>
	[TypeConverter(typeof(LengthConverter))]
	public double Width
	{
		get
		{
			return (double)GetValue(WidthProperty);
		}
		set
		{
			SetValue(WidthProperty, value);
		}
	}

	/// <summary>Gets the actual width of a <see cref="T:System.Windows.Controls.GridViewColumn" />.</summary>
	/// <returns>The current width of the column. The default is zero (0.0).</returns>
	public double ActualWidth
	{
		get
		{
			return _actualWidth;
		}
		private set
		{
			if (!double.IsNaN(value) && !double.IsInfinity(value) && !(value < 0.0) && _actualWidth != value)
			{
				_actualWidth = value;
				OnPropertyChanged("ActualWidth");
			}
		}
	}

	internal ColumnMeasureState State
	{
		get
		{
			return _state;
		}
		set
		{
			if (_state != value)
			{
				_state = value;
				if (value != 0)
				{
					UpdateActualWidth();
				}
				else
				{
					DesiredWidth = 0.0;
				}
			}
			else if (value == ColumnMeasureState.SpecificWidth)
			{
				UpdateActualWidth();
			}
		}
	}

	internal int ActualIndex
	{
		get
		{
			return _actualIndex;
		}
		set
		{
			_actualIndex = value;
		}
	}

	internal double DesiredWidth
	{
		get
		{
			return _desiredWidth;
		}
		private set
		{
			_desiredWidth = value;
		}
	}

	internal override DependencyObject InheritanceContext => _inheritanceContext;

	/// <summary>Occurs when the value of any <see cref="T:System.Windows.Controls.GridViewColumn" /> property changes.</summary>
	event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
	{
		add
		{
			_propertyChanged += value;
		}
		remove
		{
			_propertyChanged -= value;
		}
	}

	private event PropertyChangedEventHandler _propertyChanged;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.GridViewColumn" /> class.</summary>
	public GridViewColumn()
	{
		ResetPrivateData();
		_state = ((!double.IsNaN(Width)) ? ColumnMeasureState.SpecificWidth : ColumnMeasureState.Init);
	}

	/// <summary>Creates a string representation of the <see cref="T:System.Windows.Controls.GridViewColumn" />.</summary>
	/// <returns>A string that identifies the object as a <see cref="T:System.Windows.Controls.GridViewColumn" /> object and displays the value of the <see cref="P:System.Windows.Controls.GridViewColumn.Header" /> property.</returns>
	public override string ToString()
	{
		return SR.Format(SR.ToStringFormatString_GridViewColumn, GetType(), Header);
	}

	private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((GridViewColumn)d).OnPropertyChanged(HeaderProperty.Name);
	}

	private static void OnHeaderContainerStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((GridViewColumn)d).OnPropertyChanged(HeaderContainerStyleProperty.Name);
	}

	private static void OnHeaderTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		GridViewColumn gridViewColumn = (GridViewColumn)d;
		Helper.CheckTemplateAndTemplateSelector("Header", HeaderTemplateProperty, HeaderTemplateSelectorProperty, gridViewColumn);
		gridViewColumn.OnPropertyChanged(HeaderTemplateProperty.Name);
	}

	private static void OnHeaderTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		GridViewColumn gridViewColumn = (GridViewColumn)d;
		Helper.CheckTemplateAndTemplateSelector("Header", HeaderTemplateProperty, HeaderTemplateSelectorProperty, gridViewColumn);
		gridViewColumn.OnPropertyChanged(HeaderTemplateSelectorProperty.Name);
	}

	private static void OnHeaderStringFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((GridViewColumn)d).OnHeaderStringFormatChanged((string)e.OldValue, (string)e.NewValue);
	}

	/// <summary>Occurs when the <see cref="P:System.Windows.Controls.GridViewColumn.HeaderStringFormat" /> property changes.</summary>
	/// <param name="oldHeaderStringFormat">The old value of the <see cref="P:System.Windows.Controls.GridViewColumn.HeaderStringFormat" /> property.</param>
	/// <param name="newHeaderStringFormat">The new value of the <see cref="P:System.Windows.Controls.GridViewColumn.HeaderStringFormat" /> property.</param>
	protected virtual void OnHeaderStringFormatChanged(string oldHeaderStringFormat, string newHeaderStringFormat)
	{
	}

	private void OnDisplayMemberBindingChanged()
	{
		OnPropertyChanged("DisplayMemberBinding");
	}

	private static void OnCellTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((GridViewColumn)d).OnPropertyChanged(CellTemplateProperty.Name);
	}

	private static void OnCellTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((GridViewColumn)d).OnPropertyChanged(CellTemplateSelectorProperty.Name);
	}

	private static void OnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		GridViewColumn obj = (GridViewColumn)d;
		double d2 = (double)e.NewValue;
		obj.State = ((!double.IsNaN(d2)) ? ColumnMeasureState.SpecificWidth : ColumnMeasureState.Init);
		obj.OnPropertyChanged(WidthProperty.Name);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.GridViewColumn.System#ComponentModel#INotifyPropertyChanged#PropertyChanged" /> event.</summary>
	/// <param name="e">The event data.</param>
	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		if (this._propertyChanged != null)
		{
			this._propertyChanged(this, e);
		}
	}

	internal void OnThemeChanged()
	{
		if (Header != null && Header is DependencyObject d)
		{
			Helper.DowncastToFEorFCE(d, out var fe, out var fce, throwIfNeither: false);
			if (fe != null || fce != null)
			{
				TreeWalkHelper.InvalidateOnResourcesChange(fe, fce, ResourcesChangeInfo.ThemeChangeInfo);
			}
		}
	}

	internal double EnsureWidth(double width)
	{
		if (width > DesiredWidth)
		{
			DesiredWidth = width;
		}
		return DesiredWidth;
	}

	internal void ResetPrivateData()
	{
		_actualIndex = -1;
		_desiredWidth = 0.0;
		_state = ((!double.IsNaN(Width)) ? ColumnMeasureState.SpecificWidth : ColumnMeasureState.Init);
	}

	internal override void AddInheritanceContext(DependencyObject context, DependencyProperty property)
	{
		if (_inheritanceContext == null && context != null)
		{
			_inheritanceContext = context;
			OnInheritanceContextChanged(EventArgs.Empty);
		}
	}

	internal override void RemoveInheritanceContext(DependencyObject context, DependencyProperty property)
	{
		if (_inheritanceContext == context)
		{
			_inheritanceContext = null;
			OnInheritanceContextChanged(EventArgs.Empty);
		}
	}

	private void OnPropertyChanged(string propertyName)
	{
		OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
	}

	private void UpdateActualWidth()
	{
		ActualWidth = ((State == ColumnMeasureState.SpecificWidth) ? Width : DesiredWidth);
	}
}
