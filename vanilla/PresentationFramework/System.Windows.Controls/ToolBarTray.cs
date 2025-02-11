using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.Controls;
using MS.Internal.KnownBoxes;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Represents the container that handles the layout of a <see cref="T:System.Windows.Controls.ToolBar" />. </summary>
[ContentProperty("ToolBars")]
public class ToolBarTray : FrameworkElement, IAddChild
{
	private class ToolBarCollection : Collection<ToolBar>
	{
		private readonly ToolBarTray _parent;

		public ToolBarCollection(ToolBarTray parent)
		{
			_parent = parent;
		}

		protected override void InsertItem(int index, ToolBar toolBar)
		{
			base.InsertItem(index, toolBar);
			_parent.AddLogicalChild(toolBar);
			_parent.AddVisualChild(toolBar);
			_parent.InvalidateMeasure();
		}

		protected override void SetItem(int index, ToolBar toolBar)
		{
			ToolBar toolBar2 = base.Items[index];
			if (toolBar != toolBar2)
			{
				base.SetItem(index, toolBar);
				_parent.RemoveVisualChild(toolBar2);
				_parent.RemoveLogicalChild(toolBar2);
				_parent.AddLogicalChild(toolBar);
				_parent.AddVisualChild(toolBar);
				_parent.InvalidateMeasure();
			}
		}

		protected override void RemoveItem(int index)
		{
			ToolBar child = base[index];
			base.RemoveItem(index);
			_parent.RemoveVisualChild(child);
			_parent.RemoveLogicalChild(child);
			_parent.InvalidateMeasure();
		}

		protected override void ClearItems()
		{
			int count = base.Count;
			if (count > 0)
			{
				for (int i = 0; i < count; i++)
				{
					ToolBar child = base[i];
					_parent.RemoveVisualChild(child);
					_parent.RemoveLogicalChild(child);
				}
				_parent.InvalidateMeasure();
			}
			base.ClearItems();
		}
	}

	private class BandInfo
	{
		private List<ToolBar> _band = new List<ToolBar>();

		private double _thickness;

		public List<ToolBar> Band => _band;

		public double Thickness
		{
			get
			{
				return _thickness;
			}
			set
			{
				_thickness = value;
			}
		}
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolBarTray.Background" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolBarTray.Background" /> dependency property.</returns>
	public static readonly DependencyProperty BackgroundProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolBarTray.Orientation" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolBarTray.Orientation" /> dependency property.</returns>
	public static readonly DependencyProperty OrientationProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolBarTray.IsLocked" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolBarTray.IsLocked" /> dependency property.</returns>
	public static readonly DependencyProperty IsLockedProperty;

	private List<BandInfo> _bands = new List<BandInfo>(0);

	private bool _bandsDirty = true;

	private ToolBarCollection _toolBarsCollection;

	private static DependencyObjectType _dType;

	/// <summary>Gets or sets a brush to use for the background color of the <see cref="T:System.Windows.Controls.ToolBarTray" />.   </summary>
	/// <returns>A brush to use for the background color of the <see cref="T:System.Windows.Controls.ToolBarTray" />.</returns>
	public Brush Background
	{
		get
		{
			return (Brush)GetValue(BackgroundProperty);
		}
		set
		{
			SetValue(BackgroundProperty, value);
		}
	}

	/// <summary>Specifies the orientation of a <see cref="T:System.Windows.Controls.ToolBarTray" />.   </summary>
	/// <returns>One of the <see cref="T:System.Windows.Controls.Orientation" /> values. The default is <see cref="F:System.Windows.Controls.Orientation.Horizontal" />.</returns>
	public Orientation Orientation
	{
		get
		{
			return (Orientation)GetValue(OrientationProperty);
		}
		set
		{
			SetValue(OrientationProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a <see cref="T:System.Windows.Controls.ToolBar" /> can be moved inside a <see cref="T:System.Windows.Controls.ToolBarTray" />.   </summary>
	/// <returns>true if the toolbar cannot be moved inside the toolbar tray; otherwise, false. The default is false.</returns>
	public bool IsLocked
	{
		get
		{
			return (bool)GetValue(IsLockedProperty);
		}
		set
		{
			SetValue(IsLockedProperty, value);
		}
	}

	/// <summary>Gets the collection of <see cref="T:System.Windows.Controls.ToolBar" /> elements in the <see cref="T:System.Windows.Controls.ToolBarTray" />.</summary>
	/// <returns>A collection of <see cref="T:System.Windows.Controls.ToolBar" /> objects.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public Collection<ToolBar> ToolBars
	{
		get
		{
			if (_toolBarsCollection == null)
			{
				_toolBarsCollection = new ToolBarCollection(this);
			}
			return _toolBarsCollection;
		}
	}

	/// <summary>Gets an enumerator to the logical child elements of a <see cref="T:System.Windows.Controls.ToolBarTray" />. </summary>
	/// <returns>An enumerator to the children of a <see cref="T:System.Windows.Controls.ToolBarTray" /> element.</returns>
	protected internal override IEnumerator LogicalChildren
	{
		get
		{
			if (VisualChildrenCount == 0)
			{
				return EmptyEnumerator.Instance;
			}
			return ToolBars.GetEnumerator();
		}
	}

	/// <summary>Gets the number of children that are currently visible.</summary>
	/// <returns>The number of visible <see cref="T:System.Windows.Controls.ToolBar" /> objects in the <see cref="T:System.Windows.Controls.ToolBarTray" />.</returns>
	protected override int VisualChildrenCount
	{
		get
		{
			if (_toolBarsCollection == null)
			{
				return 0;
			}
			return _toolBarsCollection.Count;
		}
	}

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	static ToolBarTray()
	{
		BackgroundProperty = Panel.BackgroundProperty.AddOwner(typeof(ToolBarTray), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
		OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(ToolBarTray), new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsParentMeasure, OnOrientationPropertyChanged), ScrollBar.IsValidOrientation);
		IsLockedProperty = DependencyProperty.RegisterAttached("IsLocked", typeof(bool), typeof(ToolBarTray), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.Inherits));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolBarTray), new FrameworkPropertyMetadata(typeof(ToolBarTray)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(ToolBarTray));
		EventManager.RegisterClassHandler(typeof(ToolBarTray), Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnThumbDragDelta));
		KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata(typeof(ToolBarTray), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
		ControlsTraceLogger.AddControl(TelemetryControls.ToolBarTray);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ToolBarTray" /> class. </summary>
	public ToolBarTray()
	{
	}

	private static void OnOrientationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Collection<ToolBar> toolBars = ((ToolBarTray)d).ToolBars;
		for (int i = 0; i < toolBars.Count; i++)
		{
			toolBars[i].CoerceValue(ToolBar.OrientationProperty);
		}
	}

	/// <summary>Writes the value of the <see cref="P:System.Windows.Controls.ToolBarTray.IsLocked" /> property to the specified element. </summary>
	/// <param name="element">The element to write the property to.</param>
	/// <param name="value">The property value to set.</param>
	public static void SetIsLocked(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(IsLockedProperty, value);
	}

	/// <summary>Reads the value of the <see cref="P:System.Windows.Controls.ToolBarTray.IsLocked" /> property from the specified element. </summary>
	/// <returns>true if the toolbar cannot be moved inside the toolbar tray; otherwise, false. The default is false.</returns>
	/// <param name="element">The element from which to read the property.</param>
	public static bool GetIsLocked(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(IsLockedProperty);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="value">  An object to add as a child.</param>
	void IAddChild.AddChild(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is ToolBar item))
		{
			throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(ToolBar)), "value");
		}
		ToolBars.Add(item);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="text">  A string to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
	}

	/// <summary>Called when a <see cref="T:System.Windows.Controls.ToolBarTray" /> is displayed to get the Drawing Context (DC) to use to render the <see cref="T:System.Windows.Controls.ToolBarTray" />.</summary>
	/// <param name="dc">Drawing context to use to render the <see cref="T:System.Windows.Controls.ToolBarTray" />.</param>
	protected override void OnRender(DrawingContext dc)
	{
		Brush background = Background;
		if (background != null)
		{
			dc.DrawRectangle(background, null, new Rect(0.0, 0.0, base.RenderSize.Width, base.RenderSize.Height));
		}
	}

	/// <summary>Called to remeasure a <see cref="T:System.Windows.Controls.ToolBarTray" />. </summary>
	/// <returns>The size of the control.</returns>
	/// <param name="constraint">The measurement constraints; a <see cref="T:System.Windows.Controls.ToolBarTray" /> cannot return a size larger than the constraint.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		GenerateBands();
		Size result = default(Size);
		bool flag = Orientation == Orientation.Horizontal;
		Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
		for (int i = 0; i < _bands.Count; i++)
		{
			double num = (flag ? constraint.Width : constraint.Height);
			List<ToolBar> band = _bands[i].Band;
			double num2 = 0.0;
			double num3 = 0.0;
			for (int j = 0; j < band.Count; j++)
			{
				ToolBar toolBar = band[j];
				num -= toolBar.MinLength;
				if (DoubleUtil.LessThan(num, 0.0))
				{
					num = 0.0;
					break;
				}
			}
			for (int j = 0; j < band.Count; j++)
			{
				ToolBar toolBar2 = band[j];
				num += toolBar2.MinLength;
				if (flag)
				{
					availableSize.Width = num;
				}
				else
				{
					availableSize.Height = num;
				}
				toolBar2.Measure(availableSize);
				num2 = Math.Max(num2, flag ? toolBar2.DesiredSize.Height : toolBar2.DesiredSize.Width);
				num3 += (flag ? toolBar2.DesiredSize.Width : toolBar2.DesiredSize.Height);
				num -= (flag ? toolBar2.DesiredSize.Width : toolBar2.DesiredSize.Height);
				if (DoubleUtil.LessThan(num, 0.0))
				{
					num = 0.0;
				}
			}
			_bands[i].Thickness = num2;
			if (flag)
			{
				result.Height += num2;
				result.Width = Math.Max(result.Width, num3);
			}
			else
			{
				result.Width += num2;
				result.Height = Math.Max(result.Height, num3);
			}
		}
		return result;
	}

	/// <summary> Called to arrange and size its <see cref="T:System.Windows.Controls.ToolBar" /> children. </summary>
	/// <returns>The size of the control.</returns>
	/// <param name="arrangeSize">The size that the <see cref="T:System.Windows.Controls.ToolBarTray" /> assumes to position its children.</param>
	protected override Size ArrangeOverride(Size arrangeSize)
	{
		bool flag = Orientation == Orientation.Horizontal;
		Rect finalRect = default(Rect);
		for (int i = 0; i < _bands.Count; i++)
		{
			List<ToolBar> band = _bands[i].Band;
			double thickness = _bands[i].Thickness;
			if (flag)
			{
				finalRect.X = 0.0;
			}
			else
			{
				finalRect.Y = 0.0;
			}
			for (int j = 0; j < band.Count; j++)
			{
				ToolBar toolBar = band[j];
				Size size2 = (finalRect.Size = new Size(flag ? toolBar.DesiredSize.Width : thickness, flag ? thickness : toolBar.DesiredSize.Height));
				toolBar.Arrange(finalRect);
				if (flag)
				{
					finalRect.X += size2.Width;
				}
				else
				{
					finalRect.Y += size2.Height;
				}
			}
			if (flag)
			{
				finalRect.Y += thickness;
			}
			else
			{
				finalRect.X += thickness;
			}
		}
		return arrangeSize;
	}

	/// <summary>Gets the index number of the visible child.</summary>
	/// <returns>The index number of the visible child.</returns>
	/// <param name="index">Index of the visual child.</param>
	protected override Visual GetVisualChild(int index)
	{
		if (_toolBarsCollection == null)
		{
			throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
		}
		return _toolBarsCollection[index];
	}

	private static void OnThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		ToolBarTray toolBarTray = (ToolBarTray)sender;
		if (!toolBarTray.IsLocked)
		{
			toolBarTray.ProcessThumbDragDelta(e);
		}
	}

	private void ProcessThumbDragDelta(DragDeltaEventArgs e)
	{
		if (!(e.OriginalSource is Thumb { TemplatedParent: ToolBar templatedParent }) || templatedParent.Parent != this)
		{
			return;
		}
		if (_bandsDirty)
		{
			GenerateBands();
		}
		bool flag = Orientation == Orientation.Horizontal;
		int band = templatedParent.Band;
		Point position = Mouse.PrimaryDevice.GetPosition(this);
		Point point = TransformPointToToolBar(templatedParent, position);
		int bandFromOffset = GetBandFromOffset(flag ? position.Y : position.X);
		double num = (flag ? e.HorizontalChange : e.VerticalChange);
		double num2 = ((!flag) ? (position.Y - point.Y) : (position.X - point.X));
		double num3 = num2 + num;
		if (bandFromOffset == band)
		{
			List<ToolBar> band2 = _bands[band].Band;
			int bandIndex = templatedParent.BandIndex;
			if (DoubleUtil.LessThan(num, 0.0))
			{
				double num4 = ToolBarsTotalMinimum(band2, 0, bandIndex - 1);
				if (DoubleUtil.LessThanOrClose(num4, num3))
				{
					ShrinkToolBars(band2, 0, bandIndex - 1, 0.0 - num);
				}
				else if (bandIndex > 0)
				{
					ToolBar toolBar = band2[bandIndex - 1];
					Point point2 = TransformPointToToolBar(toolBar, position);
					if (DoubleUtil.LessThan(flag ? point2.X : point2.Y, 0.0))
					{
						toolBar.BandIndex = bandIndex;
						band2[bandIndex] = toolBar;
						templatedParent.BandIndex = bandIndex - 1;
						band2[bandIndex - 1] = templatedParent;
						if (bandIndex + 1 == band2.Count)
						{
							toolBar.ClearValue(flag ? FrameworkElement.WidthProperty : FrameworkElement.HeightProperty);
						}
					}
					else if (flag)
					{
						if (DoubleUtil.LessThan(num4, position.X - point.X))
						{
							ShrinkToolBars(band2, 0, bandIndex - 1, position.X - point.X - num4);
						}
					}
					else if (DoubleUtil.LessThan(num4, position.Y - point.Y))
					{
						ShrinkToolBars(band2, 0, bandIndex - 1, position.Y - point.Y - num4);
					}
				}
			}
			else if (DoubleUtil.GreaterThan(ToolBarsTotalMaximum(band2, 0, bandIndex - 1), num3))
			{
				ExpandToolBars(band2, 0, bandIndex - 1, num);
			}
			else if (bandIndex < band2.Count - 1)
			{
				ToolBar toolBar2 = band2[bandIndex + 1];
				Point point3 = TransformPointToToolBar(toolBar2, position);
				if (DoubleUtil.GreaterThanOrClose(flag ? point3.X : point3.Y, 0.0))
				{
					toolBar2.BandIndex = bandIndex;
					band2[bandIndex] = toolBar2;
					templatedParent.BandIndex = bandIndex + 1;
					band2[bandIndex + 1] = templatedParent;
					if (bandIndex + 2 == band2.Count)
					{
						templatedParent.ClearValue(flag ? FrameworkElement.WidthProperty : FrameworkElement.HeightProperty);
					}
				}
				else
				{
					ExpandToolBars(band2, 0, bandIndex - 1, num);
				}
			}
			else
			{
				ExpandToolBars(band2, 0, bandIndex - 1, num);
			}
		}
		else
		{
			_bandsDirty = true;
			templatedParent.Band = bandFromOffset;
			templatedParent.ClearValue(flag ? FrameworkElement.WidthProperty : FrameworkElement.HeightProperty);
			if (bandFromOffset >= 0 && bandFromOffset < _bands.Count)
			{
				MoveToolBar(templatedParent, bandFromOffset, num3);
			}
			List<ToolBar> band3 = _bands[band].Band;
			for (int i = 0; i < band3.Count; i++)
			{
				band3[i].ClearValue(flag ? FrameworkElement.WidthProperty : FrameworkElement.HeightProperty);
			}
		}
		e.Handled = true;
	}

	private Point TransformPointToToolBar(ToolBar toolBar, Point point)
	{
		Point result = point;
		TransformToDescendant(toolBar)?.TryTransform(point, out result);
		return result;
	}

	private void ShrinkToolBars(List<ToolBar> band, int startIndex, int endIndex, double shrinkAmount)
	{
		if (Orientation == Orientation.Horizontal)
		{
			for (int num = endIndex; num >= startIndex; num--)
			{
				ToolBar toolBar = band[num];
				if (DoubleUtil.GreaterThanOrClose(toolBar.RenderSize.Width - shrinkAmount, toolBar.MinLength))
				{
					toolBar.Width = toolBar.RenderSize.Width - shrinkAmount;
					break;
				}
				toolBar.Width = toolBar.MinLength;
				shrinkAmount -= toolBar.RenderSize.Width - toolBar.MinLength;
			}
			return;
		}
		for (int num2 = endIndex; num2 >= startIndex; num2--)
		{
			ToolBar toolBar2 = band[num2];
			if (DoubleUtil.GreaterThanOrClose(toolBar2.RenderSize.Height - shrinkAmount, toolBar2.MinLength))
			{
				toolBar2.Height = toolBar2.RenderSize.Height - shrinkAmount;
				break;
			}
			toolBar2.Height = toolBar2.MinLength;
			shrinkAmount -= toolBar2.RenderSize.Height - toolBar2.MinLength;
		}
	}

	private double ToolBarsTotalMinimum(List<ToolBar> band, int startIndex, int endIndex)
	{
		double num = 0.0;
		for (int i = startIndex; i <= endIndex; i++)
		{
			num += band[i].MinLength;
		}
		return num;
	}

	private void ExpandToolBars(List<ToolBar> band, int startIndex, int endIndex, double expandAmount)
	{
		if (Orientation == Orientation.Horizontal)
		{
			for (int num = endIndex; num >= startIndex; num--)
			{
				ToolBar toolBar = band[num];
				if (DoubleUtil.LessThanOrClose(toolBar.RenderSize.Width + expandAmount, toolBar.MaxLength))
				{
					toolBar.Width = toolBar.RenderSize.Width + expandAmount;
					break;
				}
				toolBar.Width = toolBar.MaxLength;
				expandAmount -= toolBar.MaxLength - toolBar.RenderSize.Width;
			}
			return;
		}
		for (int num2 = endIndex; num2 >= startIndex; num2--)
		{
			ToolBar toolBar2 = band[num2];
			if (DoubleUtil.LessThanOrClose(toolBar2.RenderSize.Height + expandAmount, toolBar2.MaxLength))
			{
				toolBar2.Height = toolBar2.RenderSize.Height + expandAmount;
				break;
			}
			toolBar2.Height = toolBar2.MaxLength;
			expandAmount -= toolBar2.MaxLength - toolBar2.RenderSize.Height;
		}
	}

	private double ToolBarsTotalMaximum(List<ToolBar> band, int startIndex, int endIndex)
	{
		double num = 0.0;
		for (int i = startIndex; i <= endIndex; i++)
		{
			num += band[i].MaxLength;
		}
		return num;
	}

	private void MoveToolBar(ToolBar toolBar, int newBandNumber, double position)
	{
		bool flag = Orientation == Orientation.Horizontal;
		List<ToolBar> band = _bands[newBandNumber].Band;
		if (DoubleUtil.LessThanOrClose(position, 0.0))
		{
			toolBar.BandIndex = -1;
			return;
		}
		double num = 0.0;
		int num2 = -1;
		int i;
		for (i = 0; i < band.Count; i++)
		{
			ToolBar toolBar2 = band[i];
			if (num2 == -1)
			{
				num += (flag ? toolBar2.RenderSize.Width : toolBar2.RenderSize.Height);
				if (DoubleUtil.GreaterThan(num, position))
				{
					num2 = (toolBar.BandIndex = i + 1);
					if (flag)
					{
						toolBar2.Width = Math.Max(toolBar2.MinLength, toolBar2.RenderSize.Width - num + position);
					}
					else
					{
						toolBar2.Height = Math.Max(toolBar2.MinLength, toolBar2.RenderSize.Height - num + position);
					}
				}
			}
			else
			{
				toolBar2.BandIndex = i + 1;
			}
		}
		if (num2 == -1)
		{
			toolBar.BandIndex = i;
		}
	}

	private int GetBandFromOffset(double toolBarOffset)
	{
		if (DoubleUtil.LessThan(toolBarOffset, 0.0))
		{
			return -1;
		}
		double num = 0.0;
		for (int i = 0; i < _bands.Count; i++)
		{
			num += _bands[i].Thickness;
			if (DoubleUtil.GreaterThan(num, toolBarOffset))
			{
				return i;
			}
		}
		return _bands.Count;
	}

	private void GenerateBands()
	{
		if (!IsBandsDirty())
		{
			return;
		}
		Collection<ToolBar> toolBars = ToolBars;
		_bands.Clear();
		for (int i = 0; i < toolBars.Count; i++)
		{
			InsertBand(toolBars[i], i);
		}
		for (int j = 0; j < _bands.Count; j++)
		{
			List<ToolBar> band = _bands[j].Band;
			for (int k = 0; k < band.Count; k++)
			{
				ToolBar toolBar = band[k];
				toolBar.Band = j;
				toolBar.BandIndex = k;
			}
		}
		_bandsDirty = false;
	}

	private bool IsBandsDirty()
	{
		if (_bandsDirty)
		{
			return true;
		}
		int num = 0;
		Collection<ToolBar> toolBars = ToolBars;
		for (int i = 0; i < _bands.Count; i++)
		{
			List<ToolBar> band = _bands[i].Band;
			for (int j = 0; j < band.Count; j++)
			{
				ToolBar toolBar = band[j];
				if (toolBar.Band != i || toolBar.BandIndex != j || !toolBars.Contains(toolBar))
				{
					return true;
				}
			}
			num += band.Count;
		}
		return num != toolBars.Count;
	}

	private void InsertBand(ToolBar toolBar, int toolBarIndex)
	{
		int band = toolBar.Band;
		for (int i = 0; i < _bands.Count; i++)
		{
			int band2 = _bands[i].Band[0].Band;
			if (band == band2)
			{
				return;
			}
			if (band < band2)
			{
				_bands.Insert(i, CreateBand(toolBarIndex));
				return;
			}
		}
		_bands.Add(CreateBand(toolBarIndex));
	}

	private BandInfo CreateBand(int startIndex)
	{
		Collection<ToolBar> toolBars = ToolBars;
		BandInfo bandInfo = new BandInfo();
		ToolBar toolBar = toolBars[startIndex];
		bandInfo.Band.Add(toolBar);
		int band = toolBar.Band;
		for (int i = startIndex + 1; i < toolBars.Count; i++)
		{
			toolBar = toolBars[i];
			if (band == toolBar.Band)
			{
				InsertToolBar(toolBar, bandInfo.Band);
			}
		}
		return bandInfo;
	}

	private void InsertToolBar(ToolBar toolBar, List<ToolBar> band)
	{
		for (int i = 0; i < band.Count; i++)
		{
			if (toolBar.BandIndex < band[i].BandIndex)
			{
				band.Insert(i, toolBar);
				return;
			}
		}
		band.Add(toolBar);
	}
}
