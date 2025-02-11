using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Input;
using System.Windows.Media;

namespace System.Windows.Controls.Primitives;

/// <summary>Used within the template of a <see cref="T:System.Windows.Controls.DataGrid" /> to specify the location in the control's visual tree where the row details are to be added.</summary>
public class DataGridDetailsPresenter : ContentPresenter
{
	internal FrameworkElement DetailsElement
	{
		get
		{
			if (VisualTreeHelper.GetChildrenCount(this) > 0)
			{
				return VisualTreeHelper.GetChild(this, 0) as FrameworkElement;
			}
			return null;
		}
	}

	private DataGrid DataGridOwner => DataGridRowOwner?.DataGridOwner;

	internal DataGridRow DataGridRowOwner => DataGridHelper.FindParent<DataGridRow>(this);

	static DataGridDetailsPresenter()
	{
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridDetailsPresenter), new FrameworkPropertyMetadata(typeof(DataGridDetailsPresenter)));
		ContentPresenter.ContentTemplateProperty.OverrideMetadata(typeof(DataGridDetailsPresenter), new FrameworkPropertyMetadata(OnNotifyPropertyChanged, OnCoerceContentTemplate));
		ContentPresenter.ContentTemplateSelectorProperty.OverrideMetadata(typeof(DataGridDetailsPresenter), new FrameworkPropertyMetadata(OnNotifyPropertyChanged, OnCoerceContentTemplateSelector));
		AutomationProperties.IsOffscreenBehaviorProperty.OverrideMetadata(typeof(DataGridDetailsPresenter), new FrameworkPropertyMetadata(IsOffscreenBehavior.FromClip));
		EventManager.RegisterClassHandler(typeof(DataGridDetailsPresenter), UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnAnyMouseLeftButtonDownThunk), handledEventsToo: true);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.DataGridDetailsPresenter" /> class. </summary>
	public DataGridDetailsPresenter()
	{
	}

	/// <summary>Returns a new <see cref="T:System.Windows.Automation.Peers.DataGridDetailsPresenterAutomationPeer" /> for this presenter.</summary>
	/// <returns>A new automation peer for this presenter.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new DataGridDetailsPresenterAutomationPeer(this);
	}

	private static object OnCoerceContentTemplate(DependencyObject d, object baseValue)
	{
		DataGridDetailsPresenter obj = d as DataGridDetailsPresenter;
		DataGridRow dataGridRowOwner = obj.DataGridRowOwner;
		return DataGridHelper.GetCoercedTransferPropertyValue(grandParentObject: dataGridRowOwner?.DataGridOwner, baseObject: obj, baseValue: baseValue, baseProperty: ContentPresenter.ContentTemplateProperty, parentObject: dataGridRowOwner, parentProperty: DataGridRow.DetailsTemplateProperty, grandParentProperty: DataGrid.RowDetailsTemplateProperty);
	}

	private static object OnCoerceContentTemplateSelector(DependencyObject d, object baseValue)
	{
		DataGridDetailsPresenter obj = d as DataGridDetailsPresenter;
		DataGridRow dataGridRowOwner = obj.DataGridRowOwner;
		return DataGridHelper.GetCoercedTransferPropertyValue(grandParentObject: dataGridRowOwner?.DataGridOwner, baseObject: obj, baseValue: baseValue, baseProperty: ContentPresenter.ContentTemplateSelectorProperty, parentObject: dataGridRowOwner, parentProperty: DataGridRow.DetailsTemplateSelectorProperty, grandParentProperty: DataGrid.RowDetailsTemplateSelectorProperty);
	}

	/// <param name="oldParent">The old parent element. May be null to indicate that the element did not have a visual parent previously.</param>
	protected internal override void OnVisualParentChanged(DependencyObject oldParent)
	{
		base.OnVisualParentChanged(oldParent);
		DataGridRow dataGridRowOwner = DataGridRowOwner;
		if (dataGridRowOwner != null)
		{
			dataGridRowOwner.DetailsPresenter = this;
			SyncProperties();
		}
	}

	private static void OnAnyMouseLeftButtonDownThunk(object sender, MouseButtonEventArgs e)
	{
		((DataGridDetailsPresenter)sender).OnAnyMouseLeftButtonDown(e);
	}

	private void OnAnyMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		if (!PresentationSource.UnderSamePresentationSource(e.OriginalSource as DependencyObject, this))
		{
			return;
		}
		DataGridRow dataGridRowOwner = DataGridRowOwner;
		DataGrid dataGrid = dataGridRowOwner?.DataGridOwner;
		if (dataGrid != null && dataGridRowOwner != null)
		{
			if (dataGrid.CurrentCell.Item != dataGridRowOwner.Item)
			{
				dataGrid.ScrollIntoView(dataGridRowOwner.Item, dataGrid.ColumnFromDisplayIndex(0));
			}
			dataGrid.HandleSelectionForRowHeaderAndDetailsInput(dataGridRowOwner, Mouse.Captured == null);
		}
	}

	internal void SyncProperties()
	{
		base.Content = DataGridRowOwner?.Item;
		DataGridHelper.TransferProperty(this, ContentPresenter.ContentTemplateProperty);
		DataGridHelper.TransferProperty(this, ContentPresenter.ContentTemplateSelectorProperty);
	}

	private static void OnNotifyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGridDetailsPresenter)d).NotifyPropertyChanged(d, e);
	}

	internal void NotifyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.Property == DataGrid.RowDetailsTemplateProperty || e.Property == DataGridRow.DetailsTemplateProperty || e.Property == ContentPresenter.ContentTemplateProperty)
		{
			DataGridHelper.TransferProperty(this, ContentPresenter.ContentTemplateProperty);
		}
		else if (e.Property == DataGrid.RowDetailsTemplateSelectorProperty || e.Property == DataGridRow.DetailsTemplateSelectorProperty || e.Property == ContentPresenter.ContentTemplateSelectorProperty)
		{
			DataGridHelper.TransferProperty(this, ContentPresenter.ContentTemplateSelectorProperty);
		}
	}

	/// <returns>The size that is required to arrange child content.</returns>
	/// <param name="availableSize">The available size that this element can give to child elements.</param>
	protected override Size MeasureOverride(Size availableSize)
	{
		DataGridRow dataGridRowOwner = DataGridRowOwner;
		if (dataGridRowOwner == null)
		{
			return base.MeasureOverride(availableSize);
		}
		DataGrid dataGridOwner = dataGridRowOwner.DataGridOwner;
		if (dataGridOwner == null)
		{
			return base.MeasureOverride(availableSize);
		}
		if (dataGridRowOwner.DetailsPresenterDrawsGridLines && DataGridHelper.IsGridLineVisible(dataGridOwner, isHorizontal: true))
		{
			double horizontalGridLineThickness = dataGridOwner.HorizontalGridLineThickness;
			Size result = base.MeasureOverride(DataGridHelper.SubtractFromSize(availableSize, horizontalGridLineThickness, height: true));
			result.Height += horizontalGridLineThickness;
			return result;
		}
		return base.MeasureOverride(availableSize);
	}

	/// <returns>The actual size needed by the element.</returns>
	/// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
	protected override Size ArrangeOverride(Size finalSize)
	{
		DataGridRow dataGridRowOwner = DataGridRowOwner;
		if (dataGridRowOwner == null)
		{
			return base.ArrangeOverride(finalSize);
		}
		DataGrid dataGridOwner = dataGridRowOwner.DataGridOwner;
		if (dataGridOwner == null)
		{
			return base.ArrangeOverride(finalSize);
		}
		if (dataGridRowOwner.DetailsPresenterDrawsGridLines && DataGridHelper.IsGridLineVisible(dataGridOwner, isHorizontal: true))
		{
			double horizontalGridLineThickness = dataGridOwner.HorizontalGridLineThickness;
			Size result = base.ArrangeOverride(DataGridHelper.SubtractFromSize(finalSize, horizontalGridLineThickness, height: true));
			result.Height += horizontalGridLineThickness;
			return result;
		}
		return base.ArrangeOverride(finalSize);
	}

	/// <summary>Called by the layout system to draw a horizontal line below the row details section if horizontal grid lines are visible.</summary>
	/// <param name="drawingContext">The drawing instructions for the row details. This context is provided to the layout system.</param>
	protected override void OnRender(DrawingContext drawingContext)
	{
		base.OnRender(drawingContext);
		DataGridRow dataGridRowOwner = DataGridRowOwner;
		if (dataGridRowOwner != null)
		{
			DataGrid dataGridOwner = dataGridRowOwner.DataGridOwner;
			if (dataGridOwner != null && dataGridRowOwner.DetailsPresenterDrawsGridLines && DataGridHelper.IsGridLineVisible(dataGridOwner, isHorizontal: true))
			{
				double horizontalGridLineThickness = dataGridOwner.HorizontalGridLineThickness;
				Rect rectangle = new Rect(new Size(base.RenderSize.Width, horizontalGridLineThickness));
				rectangle.Y = base.RenderSize.Height - horizontalGridLineThickness;
				drawingContext.DrawRectangle(dataGridOwner.HorizontalGridLinesBrush, null, rectangle);
			}
		}
	}
}
