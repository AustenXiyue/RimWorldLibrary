using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using MS.Internal;

namespace System.Windows.Controls;

/// <summary>Represents an object that specifies the layout of a row of data.</summary>
public class GridViewRowPresenter : GridViewRowPresenterBase
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridViewRowPresenter.Content" /> dependency property. </summary>
	public static readonly DependencyProperty ContentProperty = ContentControl.ContentProperty.AddOwner(typeof(GridViewRowPresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnContentChanged));

	private FrameworkElement _viewPort;

	private FrameworkElement _viewItem;

	private Type _oldContentType;

	private bool _viewPortValid;

	private bool _isOnCurrentPage;

	private bool _isOnCurrentPageValid;

	private static readonly Thickness _defalutCellMargin = new Thickness(6.0, 0.0, 6.0, 0.0);

	/// <summary>Gets or sets the data content to display in a row. </summary>
	/// <returns>The object that represents the content of a row.</returns>
	public object Content
	{
		get
		{
			return GetValue(ContentProperty);
		}
		set
		{
			SetValue(ContentProperty, value);
		}
	}

	internal List<UIElement> ActualCells
	{
		get
		{
			List<UIElement> list = new List<UIElement>();
			GridViewColumnCollection columns = base.Columns;
			if (columns != null)
			{
				UIElementCollection internalChildren = base.InternalChildren;
				List<int> indexList = columns.IndexList;
				if (internalChildren.Count == columns.Count)
				{
					int i = 0;
					for (int count = columns.Count; i < count; i++)
					{
						UIElement uIElement = internalChildren[indexList[i]];
						if (uIElement != null)
						{
							list.Add(uIElement);
						}
					}
				}
			}
			return list;
		}
	}

	private bool IsOnCurrentPage
	{
		get
		{
			if (!_isOnCurrentPageValid)
			{
				_isOnCurrentPage = base.IsVisible && CheckVisibleOnCurrentPage();
				_isOnCurrentPageValid = true;
			}
			return _isOnCurrentPage;
		}
	}

	/// <summary>Returns a string representation of the <see cref="P:System.Windows.Controls.GridViewRowPresenter.Content" />.</summary>
	/// <returns>A string that shows the <see cref="P:System.Windows.Controls.GridViewRowPresenter.Content" />.</returns>
	public override string ToString()
	{
		return SR.Format(SR.ToStringFormatString_GridViewRowPresenter, GetType(), (Content != null) ? Content.ToString() : string.Empty, (base.Columns != null) ? base.Columns.Count : 0);
	}

	private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		GridViewRowPresenter gridViewRowPresenter = (GridViewRowPresenter)d;
		Type type = ((e.OldValue != null) ? e.OldValue.GetType() : null);
		Type type2 = ((e.NewValue != null) ? e.NewValue.GetType() : null);
		if (e.NewValue == BindingExpressionBase.DisconnectedItem)
		{
			gridViewRowPresenter._oldContentType = type;
			type2 = type;
		}
		else if (e.OldValue == BindingExpressionBase.DisconnectedItem)
		{
			type = gridViewRowPresenter._oldContentType;
		}
		if (type != type2)
		{
			gridViewRowPresenter.NeedUpdateVisualTree = true;
		}
		else
		{
			gridViewRowPresenter.UpdateCells();
		}
	}

	/// <summary>Determines the area that is required to display the row. </summary>
	/// <returns>The actual <see cref="T:System.Windows.Size" /> of the area that displays the <see cref="P:System.Windows.Controls.GridViewRowPresenter.Content" />.</returns>
	/// <param name="constraint">The maximum area to use to display the <see cref="P:System.Windows.Controls.GridViewRowPresenter.Content" />. </param>
	protected override Size MeasureOverride(Size constraint)
	{
		GridViewColumnCollection columns = base.Columns;
		if (columns == null)
		{
			return default(Size);
		}
		UIElementCollection internalChildren = base.InternalChildren;
		double num = 0.0;
		double num2 = 0.0;
		double height = constraint.Height;
		bool flag = false;
		foreach (GridViewColumn item in columns)
		{
			UIElement uIElement = internalChildren[item.ActualIndex];
			if (uIElement == null)
			{
				continue;
			}
			double num3 = Math.Max(0.0, constraint.Width - num2);
			if (item.State == ColumnMeasureState.Init || item.State == ColumnMeasureState.Headered)
			{
				if (!flag)
				{
					EnsureDesiredWidthList();
					base.LayoutUpdated += OnLayoutUpdated;
					flag = true;
				}
				uIElement.Measure(new Size(num3, height));
				if (IsOnCurrentPage)
				{
					item.EnsureWidth(uIElement.DesiredSize.Width);
				}
				base.DesiredWidthList[item.ActualIndex] = item.DesiredWidth;
				num2 += item.DesiredWidth;
			}
			else if (item.State == ColumnMeasureState.Data)
			{
				num3 = Math.Min(num3, item.DesiredWidth);
				uIElement.Measure(new Size(num3, height));
				num2 += item.DesiredWidth;
			}
			else
			{
				num3 = Math.Min(num3, item.Width);
				uIElement.Measure(new Size(num3, height));
				num2 += item.Width;
			}
			num = Math.Max(num, uIElement.DesiredSize.Height);
		}
		_isOnCurrentPageValid = false;
		num2 += 2.0;
		return new Size(num2, num);
	}

	/// <summary>Positions the content of a row according to the size of the corresponding <see cref="T:System.Windows.Controls.GridViewColumn" /> objects.</summary>
	/// <returns>The actual <see cref="T:System.Windows.Size" /> that is used to display the <see cref="P:System.Windows.Controls.GridViewRowPresenter.Content" />.</returns>
	/// <param name="arrangeSize">The area to use to display the <see cref="P:System.Windows.Controls.GridViewRowPresenter.Content" />.</param>
	protected override Size ArrangeOverride(Size arrangeSize)
	{
		GridViewColumnCollection columns = base.Columns;
		if (columns == null)
		{
			return arrangeSize;
		}
		UIElementCollection internalChildren = base.InternalChildren;
		double num = 0.0;
		double num2 = arrangeSize.Width;
		foreach (GridViewColumn item in columns)
		{
			UIElement uIElement = internalChildren[item.ActualIndex];
			if (uIElement != null)
			{
				double num3 = Math.Min(num2, (item.State == ColumnMeasureState.SpecificWidth) ? item.Width : item.DesiredWidth);
				uIElement.Arrange(new Rect(num, 0.0, num3, arrangeSize.Height));
				num2 -= num3;
				num += num3;
			}
		}
		return arrangeSize;
	}

	internal override void OnPreApplyTemplate()
	{
		base.OnPreApplyTemplate();
		if (base.NeedUpdateVisualTree)
		{
			base.InternalChildren.Clear();
			GridViewColumnCollection columns = base.Columns;
			if (columns != null)
			{
				foreach (GridViewColumn item in columns.ColumnCollection)
				{
					base.InternalChildren.AddInternal(CreateCell(item));
				}
			}
			base.NeedUpdateVisualTree = false;
		}
		_viewPortValid = false;
	}

	internal override void OnColumnPropertyChanged(GridViewColumn column, string propertyName)
	{
		int actualIndex;
		if ("ActualWidth".Equals(propertyName) || (actualIndex = column.ActualIndex) < 0 || actualIndex >= base.InternalChildren.Count)
		{
			return;
		}
		if (GridViewColumn.WidthProperty.Name.Equals(propertyName))
		{
			InvalidateMeasure();
		}
		else if ("DisplayMemberBinding".Equals(propertyName))
		{
			if (base.InternalChildren[actualIndex] is FrameworkElement frameworkElement)
			{
				BindingBase displayMemberBinding = column.DisplayMemberBinding;
				if (displayMemberBinding != null && frameworkElement is TextBlock)
				{
					frameworkElement.SetBinding(TextBlock.TextProperty, displayMemberBinding);
				}
				else
				{
					RenewCell(actualIndex, column);
				}
			}
		}
		else
		{
			if (!(base.InternalChildren[actualIndex] is ContentPresenter contentPresenter))
			{
				return;
			}
			if (GridViewColumn.CellTemplateProperty.Name.Equals(propertyName))
			{
				DataTemplate cellTemplate;
				if ((cellTemplate = column.CellTemplate) == null)
				{
					contentPresenter.ClearValue(ContentControl.ContentTemplateProperty);
				}
				else
				{
					contentPresenter.ContentTemplate = cellTemplate;
				}
			}
			else if (GridViewColumn.CellTemplateSelectorProperty.Name.Equals(propertyName))
			{
				DataTemplateSelector cellTemplateSelector;
				if ((cellTemplateSelector = column.CellTemplateSelector) == null)
				{
					contentPresenter.ClearValue(ContentControl.ContentTemplateSelectorProperty);
				}
				else
				{
					contentPresenter.ContentTemplateSelector = cellTemplateSelector;
				}
			}
		}
	}

	internal override void OnColumnCollectionChanged(GridViewColumnCollectionChangedEventArgs e)
	{
		base.OnColumnCollectionChanged(e);
		if (e.Action == NotifyCollectionChangedAction.Move)
		{
			InvalidateArrange();
			return;
		}
		switch (e.Action)
		{
		case NotifyCollectionChangedAction.Add:
			base.InternalChildren.AddInternal(CreateCell((GridViewColumn)e.NewItems[0]));
			break;
		case NotifyCollectionChangedAction.Remove:
			base.InternalChildren.RemoveAt(e.ActualIndex);
			break;
		case NotifyCollectionChangedAction.Replace:
			base.InternalChildren.RemoveAt(e.ActualIndex);
			base.InternalChildren.AddInternal(CreateCell((GridViewColumn)e.NewItems[0]));
			break;
		case NotifyCollectionChangedAction.Reset:
			base.InternalChildren.Clear();
			break;
		}
		InvalidateMeasure();
	}

	private void FindViewPort()
	{
		_viewItem = base.TemplatedParent as FrameworkElement;
		if (_viewItem == null)
		{
			return;
		}
		ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(_viewItem);
		if (itemsControl == null)
		{
			return;
		}
		ScrollViewer scrollHost = itemsControl.ScrollHost;
		if (scrollHost != null && itemsControl.ItemsHost is VirtualizingPanel && scrollHost.CanContentScroll)
		{
			_viewPort = scrollHost.GetTemplateChild("PART_ScrollContentPresenter") as FrameworkElement;
			if (_viewPort == null)
			{
				_viewPort = scrollHost;
			}
		}
	}

	private bool CheckVisibleOnCurrentPage()
	{
		if (!_viewPortValid)
		{
			FindViewPort();
		}
		bool result = true;
		if (_viewItem != null && _viewPort != null)
		{
			Rect container = new Rect(default(Point), _viewPort.RenderSize);
			Rect rect = new Rect(default(Point), _viewItem.RenderSize);
			rect = _viewItem.TransformToAncestor(_viewPort).TransformBounds(rect);
			result = CheckContains(container, rect);
		}
		return result;
	}

	private bool CheckContains(Rect container, Rect element)
	{
		if ((!CheckIsPointBetween(container, element.Top) || !CheckIsPointBetween(container, element.Bottom)) && !CheckIsPointBetween(element, container.Top + 2.0))
		{
			return CheckIsPointBetween(element, container.Bottom - 2.0);
		}
		return true;
	}

	private bool CheckIsPointBetween(Rect rect, double pointY)
	{
		if (DoubleUtil.LessThanOrClose(rect.Top, pointY))
		{
			return DoubleUtil.LessThanOrClose(pointY, rect.Bottom);
		}
		return false;
	}

	private void OnLayoutUpdated(object sender, EventArgs e)
	{
		bool flag = false;
		GridViewColumnCollection columns = base.Columns;
		if (columns != null)
		{
			foreach (GridViewColumn item in columns)
			{
				if (item.State != ColumnMeasureState.SpecificWidth)
				{
					item.State = ColumnMeasureState.Data;
					if (base.DesiredWidthList == null || item.ActualIndex >= base.DesiredWidthList.Count)
					{
						flag = true;
						break;
					}
					if (!DoubleUtil.AreClose(item.DesiredWidth, base.DesiredWidthList[item.ActualIndex]))
					{
						base.DesiredWidthList[item.ActualIndex] = item.DesiredWidth;
						flag = true;
					}
				}
			}
		}
		if (flag)
		{
			InvalidateMeasure();
		}
		base.LayoutUpdated -= OnLayoutUpdated;
	}

	private FrameworkElement CreateCell(GridViewColumn column)
	{
		BindingBase displayMemberBinding;
		FrameworkElement frameworkElement;
		if ((displayMemberBinding = column.DisplayMemberBinding) != null)
		{
			frameworkElement = new TextBlock();
			frameworkElement.DataContext = Content;
			frameworkElement.SetBinding(TextBlock.TextProperty, displayMemberBinding);
		}
		else
		{
			ContentPresenter contentPresenter = new ContentPresenter();
			contentPresenter.Content = Content;
			DataTemplate cellTemplate;
			if ((cellTemplate = column.CellTemplate) != null)
			{
				contentPresenter.ContentTemplate = cellTemplate;
			}
			DataTemplateSelector cellTemplateSelector;
			if ((cellTemplateSelector = column.CellTemplateSelector) != null)
			{
				contentPresenter.ContentTemplateSelector = cellTemplateSelector;
			}
			frameworkElement = contentPresenter;
		}
		if (base.TemplatedParent is ContentControl contentControl)
		{
			frameworkElement.VerticalAlignment = contentControl.VerticalContentAlignment;
			frameworkElement.HorizontalAlignment = contentControl.HorizontalContentAlignment;
		}
		frameworkElement.Margin = _defalutCellMargin;
		return frameworkElement;
	}

	private void RenewCell(int index, GridViewColumn column)
	{
		base.InternalChildren.RemoveAt(index);
		base.InternalChildren.Insert(index, CreateCell(column));
	}

	private void UpdateCells()
	{
		UIElementCollection internalChildren = base.InternalChildren;
		ContentControl contentControl = base.TemplatedParent as ContentControl;
		for (int i = 0; i < internalChildren.Count; i++)
		{
			FrameworkElement frameworkElement = (FrameworkElement)internalChildren[i];
			if (frameworkElement is ContentPresenter contentPresenter)
			{
				contentPresenter.Content = Content;
			}
			else
			{
				frameworkElement.DataContext = Content;
			}
			if (contentControl != null)
			{
				frameworkElement.VerticalAlignment = contentControl.VerticalContentAlignment;
				frameworkElement.HorizontalAlignment = contentControl.HorizontalContentAlignment;
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.GridViewRowPresenter" /> class.</summary>
	public GridViewRowPresenter()
	{
	}
}
