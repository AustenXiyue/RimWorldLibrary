using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MS.Internal;

namespace System.Windows.Controls;

internal static class DataGridHelper
{
	private static ConditionalWeakTable<DependencyObject, Dictionary<DependencyProperty, bool>> _propertyTransferEnabledMap = new ConditionalWeakTable<DependencyObject, Dictionary<DependencyProperty, bool>>();

	private static readonly DependencyProperty FlowDirectionCacheProperty = DependencyProperty.Register("FlowDirectionCache", typeof(FlowDirection), typeof(DataGridHelper));

	private const char _escapeChar = '\u001b';

	public static Size SubtractFromSize(Size size, double thickness, bool height)
	{
		if (height)
		{
			return new Size(size.Width, Math.Max(0.0, size.Height - thickness));
		}
		return new Size(Math.Max(0.0, size.Width - thickness), size.Height);
	}

	public static bool IsGridLineVisible(DataGrid dataGrid, bool isHorizontal)
	{
		if (dataGrid != null)
		{
			switch (dataGrid.GridLinesVisibility)
			{
			case DataGridGridLinesVisibility.All:
				return true;
			case DataGridGridLinesVisibility.Horizontal:
				return isHorizontal;
			case DataGridGridLinesVisibility.None:
				return false;
			case DataGridGridLinesVisibility.Vertical:
				return !isHorizontal;
			}
		}
		return false;
	}

	public static bool ShouldNotifyCells(DataGridNotificationTarget target)
	{
		return TestTarget(target, DataGridNotificationTarget.Cells);
	}

	public static bool ShouldNotifyCellsPresenter(DataGridNotificationTarget target)
	{
		return TestTarget(target, DataGridNotificationTarget.CellsPresenter);
	}

	public static bool ShouldNotifyColumns(DataGridNotificationTarget target)
	{
		return TestTarget(target, DataGridNotificationTarget.Columns);
	}

	public static bool ShouldNotifyColumnHeaders(DataGridNotificationTarget target)
	{
		return TestTarget(target, DataGridNotificationTarget.ColumnHeaders);
	}

	public static bool ShouldNotifyColumnHeadersPresenter(DataGridNotificationTarget target)
	{
		return TestTarget(target, DataGridNotificationTarget.ColumnHeadersPresenter);
	}

	public static bool ShouldNotifyColumnCollection(DataGridNotificationTarget target)
	{
		return TestTarget(target, DataGridNotificationTarget.ColumnCollection);
	}

	public static bool ShouldNotifyDataGrid(DataGridNotificationTarget target)
	{
		return TestTarget(target, DataGridNotificationTarget.DataGrid);
	}

	public static bool ShouldNotifyDetailsPresenter(DataGridNotificationTarget target)
	{
		return TestTarget(target, DataGridNotificationTarget.DetailsPresenter);
	}

	public static bool ShouldRefreshCellContent(DataGridNotificationTarget target)
	{
		return TestTarget(target, DataGridNotificationTarget.RefreshCellContent);
	}

	public static bool ShouldNotifyRowHeaders(DataGridNotificationTarget target)
	{
		return TestTarget(target, DataGridNotificationTarget.RowHeaders);
	}

	public static bool ShouldNotifyRows(DataGridNotificationTarget target)
	{
		return TestTarget(target, DataGridNotificationTarget.Rows);
	}

	public static bool ShouldNotifyRowSubtree(DataGridNotificationTarget target)
	{
		DataGridNotificationTarget value = DataGridNotificationTarget.Cells | DataGridNotificationTarget.CellsPresenter | DataGridNotificationTarget.DetailsPresenter | DataGridNotificationTarget.RefreshCellContent | DataGridNotificationTarget.RowHeaders | DataGridNotificationTarget.Rows;
		return TestTarget(target, value);
	}

	private static bool TestTarget(DataGridNotificationTarget target, DataGridNotificationTarget value)
	{
		return (target & value) != 0;
	}

	public static T FindParent<T>(FrameworkElement element) where T : FrameworkElement
	{
		for (FrameworkElement frameworkElement = element.TemplatedParent as FrameworkElement; frameworkElement != null; frameworkElement = frameworkElement.TemplatedParent as FrameworkElement)
		{
			if (frameworkElement is T result)
			{
				return result;
			}
		}
		return null;
	}

	public static T FindVisualParent<T>(UIElement element) where T : UIElement
	{
		for (UIElement uIElement = element; uIElement != null; uIElement = VisualTreeHelper.GetParent(uIElement) as UIElement)
		{
			if (uIElement is T result)
			{
				return result;
			}
		}
		return null;
	}

	public static bool TreeHasFocusAndTabStop(DependencyObject element)
	{
		if (element == null)
		{
			return false;
		}
		if (element is UIElement uIElement)
		{
			if (uIElement.Focusable && KeyboardNavigation.GetIsTabStop(uIElement))
			{
				return true;
			}
		}
		else if (element is ContentElement { Focusable: not false } contentElement && KeyboardNavigation.GetIsTabStop(contentElement))
		{
			return true;
		}
		int childrenCount = VisualTreeHelper.GetChildrenCount(element);
		for (int i = 0; i < childrenCount; i++)
		{
			if (TreeHasFocusAndTabStop(VisualTreeHelper.GetChild(element, i)))
			{
				return true;
			}
		}
		return false;
	}

	public static void OnColumnWidthChanged(IProvideDataGridColumn cell, DependencyPropertyChangedEventArgs e)
	{
		UIElement uIElement = (UIElement)cell;
		DataGridColumn column = cell.Column;
		bool flag = cell is DataGridColumnHeader;
		if (column == null)
		{
			return;
		}
		DataGridLength width = column.Width;
		if (!width.IsAuto && (flag || !width.IsSizeToCells) && (!flag || !width.IsSizeToHeader))
		{
			return;
		}
		DataGridLength dataGridLength = (DataGridLength)e.OldValue;
		double num = 0.0;
		if (dataGridLength.UnitType != width.UnitType)
		{
			double constraintWidth = column.GetConstraintWidth(flag);
			if (!DoubleUtil.AreClose(uIElement.DesiredSize.Width, constraintWidth))
			{
				uIElement.InvalidateMeasure();
				uIElement.Measure(new Size(constraintWidth, double.PositiveInfinity));
			}
			num = uIElement.DesiredSize.Width;
		}
		else
		{
			num = dataGridLength.DesiredValue;
		}
		if (double.IsNaN(width.DesiredValue) || DoubleUtil.LessThan(width.DesiredValue, num))
		{
			column.SetWidthInternal(new DataGridLength(width.Value, width.UnitType, num, width.DisplayValue));
		}
	}

	public static Geometry GetFrozenClipForCell(IProvideDataGridColumn cell)
	{
		return GetParentPanelForCell(cell)?.GetFrozenClipForChild((UIElement)cell);
	}

	public static DataGridCellsPanel GetParentPanelForCell(IProvideDataGridColumn cell)
	{
		return VisualTreeHelper.GetParent((UIElement)cell) as DataGridCellsPanel;
	}

	public static double GetParentCellsPanelHorizontalOffset(IProvideDataGridColumn cell)
	{
		return GetParentPanelForCell(cell)?.ComputeCellsPanelHorizontalOffset() ?? 0.0;
	}

	public static bool IsDefaultValue(DependencyObject d, DependencyProperty dp)
	{
		return DependencyPropertyHelper.GetValueSource(d, dp).BaseValueSource == BaseValueSource.Default;
	}

	public static object GetCoercedTransferPropertyValue(DependencyObject baseObject, object baseValue, DependencyProperty baseProperty, DependencyObject parentObject, DependencyProperty parentProperty)
	{
		return GetCoercedTransferPropertyValue(baseObject, baseValue, baseProperty, parentObject, parentProperty, null, null);
	}

	public static object GetCoercedTransferPropertyValue(DependencyObject baseObject, object baseValue, DependencyProperty baseProperty, DependencyObject parentObject, DependencyProperty parentProperty, DependencyObject grandParentObject, DependencyProperty grandParentProperty)
	{
		object result = baseValue;
		if (IsPropertyTransferEnabled(baseObject, baseProperty))
		{
			BaseValueSource baseValueSource = DependencyPropertyHelper.GetValueSource(baseObject, baseProperty).BaseValueSource;
			if (parentObject != null)
			{
				ValueSource valueSource = DependencyPropertyHelper.GetValueSource(parentObject, parentProperty);
				if (valueSource.BaseValueSource > baseValueSource)
				{
					result = parentObject.GetValue(parentProperty);
					baseValueSource = valueSource.BaseValueSource;
				}
			}
			if (grandParentObject != null)
			{
				ValueSource valueSource2 = DependencyPropertyHelper.GetValueSource(grandParentObject, grandParentProperty);
				if (valueSource2.BaseValueSource > baseValueSource)
				{
					result = grandParentObject.GetValue(grandParentProperty);
					baseValueSource = valueSource2.BaseValueSource;
				}
			}
		}
		return result;
	}

	public static void TransferProperty(DependencyObject d, DependencyProperty p)
	{
		Dictionary<DependencyProperty, bool> propertyTransferEnabledMapForObject = GetPropertyTransferEnabledMapForObject(d);
		propertyTransferEnabledMapForObject[p] = true;
		d.CoerceValue(p);
		propertyTransferEnabledMapForObject[p] = false;
	}

	private static Dictionary<DependencyProperty, bool> GetPropertyTransferEnabledMapForObject(DependencyObject d)
	{
		if (!_propertyTransferEnabledMap.TryGetValue(d, out var value))
		{
			value = new Dictionary<DependencyProperty, bool>();
			_propertyTransferEnabledMap.Add(d, value);
		}
		return value;
	}

	internal static bool IsPropertyTransferEnabled(DependencyObject d, DependencyProperty p)
	{
		if (_propertyTransferEnabledMap.TryGetValue(d, out var value) && value.TryGetValue(p, out var value2))
		{
			return value2;
		}
		return false;
	}

	internal static bool IsOneWay(BindingBase bindingBase)
	{
		if (bindingBase == null)
		{
			return false;
		}
		if (bindingBase is Binding binding)
		{
			return binding.Mode == BindingMode.OneWay;
		}
		if (bindingBase is MultiBinding multiBinding)
		{
			return multiBinding.Mode == BindingMode.OneWay;
		}
		if (bindingBase is PriorityBinding { Bindings: var bindings })
		{
			int count = bindings.Count;
			for (int i = 0; i < count; i++)
			{
				if (IsOneWay(bindings[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	internal static BindingExpression GetBindingExpression(FrameworkElement element, DependencyProperty dp)
	{
		return element?.GetBindingExpression(dp);
	}

	internal static bool ValidateWithoutUpdate(FrameworkElement element)
	{
		bool flag = true;
		BindingGroup bindingGroup = element.BindingGroup;
		DataGridCell dataGridCell = ((element != null) ? (element.Parent as DataGridCell) : null);
		if (bindingGroup != null && dataGridCell != null)
		{
			Collection<BindingExpressionBase> bindingExpressions = bindingGroup.BindingExpressions;
			BindingExpressionBase[] array = new BindingExpressionBase[bindingExpressions.Count];
			bindingExpressions.CopyTo(array, 0);
			foreach (BindingExpressionBase bindingExpressionBase in array)
			{
				if (BindingExpressionBelongsToElement(bindingExpressionBase, dataGridCell))
				{
					flag = bindingExpressionBase.ValidateWithoutUpdate() && flag;
				}
			}
		}
		return flag;
	}

	internal static bool BindingExpressionBelongsToElement<T>(BindingExpressionBase beb, T element) where T : FrameworkElement
	{
		DependencyObject targetElement = beb.TargetElement;
		if (targetElement != null)
		{
			DependencyObject dependencyObject = FindContextElement(beb);
			if (dependencyObject == null)
			{
				dependencyObject = targetElement;
			}
			if (dependencyObject is Visual || dependencyObject is Visual3D)
			{
				return VisualTreeHelper.IsAncestorOf(element, dependencyObject, typeof(T));
			}
		}
		return false;
	}

	private static DependencyObject FindContextElement(BindingExpressionBase beb)
	{
		if (beb is BindingExpression bindingExpression)
		{
			return bindingExpression.ContextElement;
		}
		ReadOnlyCollection<BindingExpressionBase> readOnlyCollection = null;
		if (beb is MultiBindingExpression multiBindingExpression)
		{
			readOnlyCollection = multiBindingExpression.BindingExpressions;
		}
		else if (beb is PriorityBindingExpression priorityBindingExpression)
		{
			readOnlyCollection = priorityBindingExpression.BindingExpressions;
		}
		if (readOnlyCollection != null)
		{
			foreach (BindingExpressionBase item in readOnlyCollection)
			{
				DependencyObject dependencyObject = FindContextElement(item);
				if (dependencyObject != null)
				{
					return dependencyObject;
				}
			}
		}
		return null;
	}

	internal static void CacheFlowDirection(FrameworkElement element, DataGridCell cell)
	{
		if (element != null && cell != null)
		{
			object obj = element.ReadLocalValue(FrameworkElement.FlowDirectionProperty);
			if (obj != DependencyProperty.UnsetValue)
			{
				cell.SetValue(FlowDirectionCacheProperty, obj);
			}
		}
	}

	internal static void RestoreFlowDirection(FrameworkElement element, DataGridCell cell)
	{
		if (element != null && cell != null)
		{
			object obj = cell.ReadLocalValue(FlowDirectionCacheProperty);
			if (obj != DependencyProperty.UnsetValue)
			{
				element.SetValue(FrameworkElement.FlowDirectionProperty, obj);
			}
		}
	}

	internal static void UpdateTarget(FrameworkElement element)
	{
		BindingGroup bindingGroup = element.BindingGroup;
		DataGridCell dataGridCell = ((element != null) ? (element.Parent as DataGridCell) : null);
		if (bindingGroup == null || dataGridCell == null)
		{
			return;
		}
		Collection<BindingExpressionBase> bindingExpressions = bindingGroup.BindingExpressions;
		BindingExpressionBase[] array = new BindingExpressionBase[bindingExpressions.Count];
		bindingExpressions.CopyTo(array, 0);
		foreach (BindingExpressionBase bindingExpressionBase in array)
		{
			DependencyObject targetElement = bindingExpressionBase.TargetElement;
			if (targetElement != null && VisualTreeHelper.IsAncestorOf(dataGridCell, targetElement, typeof(DataGridCell)))
			{
				bindingExpressionBase.UpdateTarget();
			}
		}
	}

	internal static void SyncColumnProperty(DependencyObject column, DependencyObject content, DependencyProperty contentProperty, DependencyProperty columnProperty)
	{
		if (IsDefaultValue(column, columnProperty))
		{
			content.ClearValue(contentProperty);
		}
		else
		{
			content.SetValue(contentProperty, column.GetValue(columnProperty));
		}
	}

	internal static string GetPathFromBinding(Binding binding)
	{
		if (binding != null)
		{
			if (!string.IsNullOrEmpty(binding.XPath))
			{
				return binding.XPath;
			}
			if (binding.Path != null)
			{
				return binding.Path.Path;
			}
		}
		return null;
	}

	public static bool AreRowHeadersVisible(DataGridHeadersVisibility headersVisibility)
	{
		return (headersVisibility & DataGridHeadersVisibility.Row) == DataGridHeadersVisibility.Row;
	}

	public static double CoerceToMinMax(double value, double minValue, double maxValue)
	{
		value = Math.Max(value, minValue);
		value = Math.Min(value, maxValue);
		return value;
	}

	public static bool HasNonEscapeCharacters(TextCompositionEventArgs textArgs)
	{
		if (textArgs != null)
		{
			string text = textArgs.Text;
			int i = 0;
			for (int length = text.Length; i < length; i++)
			{
				if (text[i] != '\u001b')
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsImeProcessed(KeyEventArgs keyArgs)
	{
		if (keyArgs != null)
		{
			return keyArgs.Key == Key.ImeProcessed;
		}
		return false;
	}
}
