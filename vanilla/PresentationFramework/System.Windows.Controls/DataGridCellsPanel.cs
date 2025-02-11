using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal;

namespace System.Windows.Controls;

/// <summary>Represents a panel that lays out cells and column headers in a data grid. </summary>
public class DataGridCellsPanel : VirtualizingPanel
{
	private class ArrangeState
	{
		public int FrozenColumnCount { get; set; }

		public double ChildHeight { get; set; }

		public double NextFrozenCellStart { get; set; }

		public double NextNonFrozenCellStart { get; set; }

		public double ViewportStartX { get; set; }

		public double DataGridHorizontalScrollStartX { get; set; }

		public UIElement OldClippedChild { get; set; }

		public UIElement NewClippedChild { get; set; }

		public ArrangeState()
		{
			FrozenColumnCount = 0;
			ChildHeight = 0.0;
			NextFrozenCellStart = 0.0;
			NextNonFrozenCellStart = 0.0;
			ViewportStartX = 0.0;
			DataGridHorizontalScrollStartX = 0.0;
			OldClippedChild = null;
			NewClippedChild = null;
		}
	}

	private DataGrid _parentDataGrid;

	private UIElement _clippedChildForFrozenBehaviour;

	private RectangleGeometry _childClipForFrozenBehavior = new RectangleGeometry();

	private List<UIElement> _realizedChildren;

	internal bool HasCorrectRealizedColumns
	{
		get
		{
			DataGridColumnCollection dataGridColumnCollection = (DataGridColumnCollection)ParentDataGrid.Columns;
			EnsureRealizedChildren();
			IList realizedChildren = RealizedChildren;
			if (realizedChildren.Count == dataGridColumnCollection.Count)
			{
				return true;
			}
			List<int> displayIndexMap = dataGridColumnCollection.DisplayIndexMap;
			List<RealizedColumnsBlock> realizedColumnsBlockList = RealizedColumnsBlockList;
			int i = 0;
			int count = realizedChildren.Count;
			for (int j = 0; j < realizedColumnsBlockList.Count; j++)
			{
				RealizedColumnsBlock realizedColumnsBlock = realizedColumnsBlockList[j];
				for (int k = realizedColumnsBlock.StartIndex; k <= realizedColumnsBlock.EndIndex; k++)
				{
					for (; i < count; i++)
					{
						if (realizedChildren[i] is IProvideDataGridColumn provideDataGridColumn)
						{
							int displayIndex = provideDataGridColumn.Column.DisplayIndex;
							int num = ((displayIndex < 0) ? (-1) : displayIndexMap[displayIndex]);
							if (k < num)
							{
								return false;
							}
							if (k == num)
							{
								break;
							}
						}
					}
					if (i == count)
					{
						return false;
					}
					i++;
				}
			}
			return true;
		}
	}

	private bool RebuildRealizedColumnsBlockList
	{
		get
		{
			DataGrid parentDataGrid = ParentDataGrid;
			if (parentDataGrid != null)
			{
				DataGridColumnCollection internalColumns = parentDataGrid.InternalColumns;
				if (!IsVirtualizing)
				{
					return internalColumns.RebuildRealizedColumnsBlockListForNonVirtualizedRows;
				}
				return internalColumns.RebuildRealizedColumnsBlockListForVirtualizedRows;
			}
			return true;
		}
		set
		{
			DataGrid parentDataGrid = ParentDataGrid;
			if (parentDataGrid != null)
			{
				if (IsVirtualizing)
				{
					parentDataGrid.InternalColumns.RebuildRealizedColumnsBlockListForVirtualizedRows = value;
				}
				else
				{
					parentDataGrid.InternalColumns.RebuildRealizedColumnsBlockListForNonVirtualizedRows = value;
				}
			}
		}
	}

	private List<RealizedColumnsBlock> RealizedColumnsBlockList
	{
		get
		{
			DataGrid parentDataGrid = ParentDataGrid;
			if (parentDataGrid != null)
			{
				DataGridColumnCollection internalColumns = parentDataGrid.InternalColumns;
				if (!IsVirtualizing)
				{
					return internalColumns.RealizedColumnsBlockListForNonVirtualizedRows;
				}
				return internalColumns.RealizedColumnsBlockListForVirtualizedRows;
			}
			return null;
		}
		set
		{
			DataGrid parentDataGrid = ParentDataGrid;
			if (parentDataGrid != null)
			{
				if (IsVirtualizing)
				{
					parentDataGrid.InternalColumns.RealizedColumnsBlockListForVirtualizedRows = value;
				}
				else
				{
					parentDataGrid.InternalColumns.RealizedColumnsBlockListForNonVirtualizedRows = value;
				}
			}
		}
	}

	private List<RealizedColumnsBlock> RealizedColumnsDisplayIndexBlockList
	{
		get
		{
			DataGrid parentDataGrid = ParentDataGrid;
			if (parentDataGrid != null)
			{
				DataGridColumnCollection internalColumns = parentDataGrid.InternalColumns;
				if (!IsVirtualizing)
				{
					return internalColumns.RealizedColumnsDisplayIndexBlockListForNonVirtualizedRows;
				}
				return internalColumns.RealizedColumnsDisplayIndexBlockListForVirtualizedRows;
			}
			return null;
		}
		set
		{
			DataGrid parentDataGrid = ParentDataGrid;
			if (parentDataGrid != null)
			{
				if (IsVirtualizing)
				{
					parentDataGrid.InternalColumns.RealizedColumnsDisplayIndexBlockListForVirtualizedRows = value;
				}
				else
				{
					parentDataGrid.InternalColumns.RealizedColumnsDisplayIndexBlockListForNonVirtualizedRows = value;
				}
			}
		}
	}

	private DataGridRowsPresenter ParentRowsPresenter
	{
		get
		{
			DataGrid parentDataGrid = ParentDataGrid;
			if (parentDataGrid == null)
			{
				return null;
			}
			if (!parentDataGrid.IsGrouping)
			{
				return parentDataGrid.InternalItemsHost as DataGridRowsPresenter;
			}
			if (ParentPresenter is DataGridCellsPresenter { DataGridRowOwner: { } dataGridRowOwner })
			{
				return VisualTreeHelper.GetParent(dataGridRowOwner) as DataGridRowsPresenter;
			}
			return null;
		}
	}

	private bool IsVirtualizing { get; set; }

	private bool InRecyclingMode { get; set; }

	private IList RealizedChildren
	{
		get
		{
			if (IsVirtualizing && InRecyclingMode)
			{
				return _realizedChildren;
			}
			return base.InternalChildren;
		}
	}

	private ObservableCollection<DataGridColumn> Columns => ParentDataGrid?.Columns;

	private DataGrid ParentDataGrid
	{
		get
		{
			if (_parentDataGrid == null)
			{
				if (ParentPresenter is DataGridCellsPresenter { DataGridRowOwner: var dataGridRowOwner })
				{
					if (dataGridRowOwner != null)
					{
						_parentDataGrid = dataGridRowOwner.DataGridOwner;
					}
				}
				else if (ParentPresenter is DataGridColumnHeadersPresenter dataGridColumnHeadersPresenter)
				{
					_parentDataGrid = dataGridColumnHeadersPresenter.ParentDataGrid;
				}
			}
			return _parentDataGrid;
		}
	}

	private ItemsControl ParentPresenter
	{
		get
		{
			if (base.TemplatedParent is FrameworkElement frameworkElement)
			{
				return frameworkElement.TemplatedParent as ItemsControl;
			}
			return null;
		}
	}

	static DataGridCellsPanel()
	{
		KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(DataGridCellsPanel), new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridCellsPanel" /> class.</summary>
	public DataGridCellsPanel()
	{
		IsVirtualizing = false;
		InRecyclingMode = false;
	}

	/// <summary>Determines the desired size of the panel.</summary>
	/// <returns>The desired size of the panel.</returns>
	/// <param name="constraint">The maximum size that the panel can occupy.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		Size size = default(Size);
		DetermineVirtualizationState();
		EnsureRealizedChildren();
		_ = RealizedChildren;
		size = ((!RebuildRealizedColumnsBlockList) ? GenerateAndMeasureChildrenForRealizedColumns(constraint) : DetermineRealizedColumnsBlockList(constraint));
		if (IsVirtualizing && InRecyclingMode)
		{
			DisconnectRecycledContainers();
		}
		if (!DoubleUtil.AreClose(base.DesiredSize, size) && base.MeasureDuringArrange)
		{
			ParentPresenter.InvalidateMeasure();
			if (VisualTreeHelper.GetParent(this) is UIElement uIElement)
			{
				uIElement.InvalidateMeasure();
			}
		}
		return size;
	}

	private static void MeasureChild(UIElement child, Size constraint)
	{
		IProvideDataGridColumn provideDataGridColumn = child as IProvideDataGridColumn;
		bool flag = child is DataGridColumnHeader;
		Size availableSize = new Size(double.PositiveInfinity, constraint.Height);
		double num = 0.0;
		bool flag2 = false;
		if (provideDataGridColumn != null)
		{
			DataGridColumn column = provideDataGridColumn.Column;
			DataGridLength width = column.Width;
			if (width.IsAuto || (width.IsSizeToHeader && flag) || (width.IsSizeToCells && !flag))
			{
				child.Measure(availableSize);
				num = child.DesiredSize.Width;
				flag2 = true;
			}
			availableSize.Width = column.GetConstraintWidth(flag);
		}
		if (DoubleUtil.AreClose(num, 0.0))
		{
			child.Measure(availableSize);
		}
		Size desiredSize = child.DesiredSize;
		if (provideDataGridColumn != null)
		{
			DataGridColumn column2 = provideDataGridColumn.Column;
			column2.UpdateDesiredWidthForAutoColumn(flag, DoubleUtil.AreClose(num, 0.0) ? desiredSize.Width : num);
			DataGridLength width2 = column2.Width;
			if (flag2 && !double.IsNaN(width2.DisplayValue) && DoubleUtil.GreaterThan(num, width2.DisplayValue))
			{
				availableSize.Width = width2.DisplayValue;
				child.Measure(availableSize);
			}
		}
	}

	private Size GenerateAndMeasureChildrenForRealizedColumns(Size constraint)
	{
		double num = 0.0;
		double num2 = 0.0;
		DataGrid parentDataGrid = ParentDataGrid;
		double averageColumnWidth = parentDataGrid.InternalColumns.AverageColumnWidth;
		IItemContainerGenerator itemContainerGenerator = base.ItemContainerGenerator;
		List<RealizedColumnsBlock> realizedColumnsBlockList = RealizedColumnsBlockList;
		VirtualizeChildren(realizedColumnsBlockList, itemContainerGenerator);
		if (realizedColumnsBlockList.Count > 0)
		{
			int i = 0;
			for (int count = realizedColumnsBlockList.Count; i < count; i++)
			{
				RealizedColumnsBlock realizedColumnsBlock = realizedColumnsBlockList[i];
				Size size = GenerateChildren(itemContainerGenerator, realizedColumnsBlock.StartIndex, realizedColumnsBlock.EndIndex, constraint);
				num += size.Width;
				num2 = Math.Max(num2, size.Height);
				if (i != count - 1)
				{
					RealizedColumnsBlock realizedColumnsBlock2 = realizedColumnsBlockList[i + 1];
					num += GetColumnEstimatedMeasureWidthSum(realizedColumnsBlock.EndIndex + 1, realizedColumnsBlock2.StartIndex - 1, averageColumnWidth);
				}
			}
			num += GetColumnEstimatedMeasureWidthSum(0, realizedColumnsBlockList[0].StartIndex - 1, averageColumnWidth);
			num += GetColumnEstimatedMeasureWidthSum(realizedColumnsBlockList[realizedColumnsBlockList.Count - 1].EndIndex + 1, parentDataGrid.Columns.Count - 1, averageColumnWidth);
		}
		else
		{
			num = 0.0;
		}
		return new Size(num, num2);
	}

	private Size DetermineRealizedColumnsBlockList(Size constraint)
	{
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		Size result = default(Size);
		DataGrid parentDataGrid = ParentDataGrid;
		if (parentDataGrid == null)
		{
			return result;
		}
		double horizontalScrollOffset = parentDataGrid.HorizontalScrollOffset;
		double cellsPanelHorizontalOffset = parentDataGrid.CellsPanelHorizontalOffset;
		double num = horizontalScrollOffset;
		double num2 = 0.0 - cellsPanelHorizontalOffset;
		double num3 = horizontalScrollOffset - cellsPanelHorizontalOffset;
		int num4 = -1;
		int lastVisibleNonFrozenDisplayIndex = -1;
		double num5 = GetViewportWidth() - cellsPanelHorizontalOffset;
		double num6 = 0.0;
		if (IsVirtualizing && DoubleUtil.LessThan(num5, 0.0))
		{
			return result;
		}
		bool hasVisibleStarColumns = parentDataGrid.InternalColumns.HasVisibleStarColumns;
		double averageColumnWidth = parentDataGrid.InternalColumns.AverageColumnWidth;
		bool flag = DoubleUtil.AreClose(averageColumnWidth, 0.0);
		bool flag2 = !IsVirtualizing;
		bool flag3 = flag || hasVisibleStarColumns || flag2;
		int frozenColumnCount = parentDataGrid.FrozenColumnCount;
		int num7 = -1;
		bool redeterminationNeeded = false;
		IItemContainerGenerator itemContainerGenerator = base.ItemContainerGenerator;
		IDisposable generatorState = null;
		int childIndex = 0;
		try
		{
			int i = 0;
			for (int count = parentDataGrid.Columns.Count; i < count; i++)
			{
				DataGridColumn dataGridColumn = parentDataGrid.ColumnFromDisplayIndex(i);
				if (!dataGridColumn.IsVisible)
				{
					continue;
				}
				int num8 = parentDataGrid.ColumnIndexFromDisplayIndex(i);
				if (num8 != childIndex || num7 != num8 - 1)
				{
					childIndex = num8;
					if (generatorState != null)
					{
						generatorState.Dispose();
						generatorState = null;
					}
				}
				num7 = num8;
				Size childSize;
				if (flag3)
				{
					if (GenerateChild(itemContainerGenerator, constraint, dataGridColumn, ref generatorState, ref childIndex, out childSize) == null)
					{
						break;
					}
				}
				else
				{
					childSize = new Size(GetColumnEstimatedMeasureWidth(dataGridColumn, averageColumnWidth), 0.0);
				}
				if (flag2 || hasVisibleStarColumns || DoubleUtil.LessThan(num6, num5))
				{
					if (i < frozenColumnCount)
					{
						if (!flag3 && GenerateChild(itemContainerGenerator, constraint, dataGridColumn, ref generatorState, ref childIndex, out childSize) == null)
						{
							break;
						}
						list.Add(num8);
						list2.Add(i);
						num6 += childSize.Width;
						num += childSize.Width;
					}
					else if (DoubleUtil.LessThanOrClose(num2, num3))
					{
						if (DoubleUtil.LessThanOrClose(num2 + childSize.Width, num3))
						{
							if (flag3)
							{
								if (flag2 || hasVisibleStarColumns)
								{
									list.Add(num8);
									list2.Add(i);
								}
								else if (flag)
								{
									redeterminationNeeded = true;
								}
							}
							else if (generatorState != null)
							{
								generatorState.Dispose();
								generatorState = null;
							}
							num2 += childSize.Width;
						}
						else
						{
							if (!flag3 && GenerateChild(itemContainerGenerator, constraint, dataGridColumn, ref generatorState, ref childIndex, out childSize) == null)
							{
								break;
							}
							double num9 = num3 - num2;
							if (DoubleUtil.AreClose(num9, 0.0))
							{
								num2 = num + childSize.Width;
								num6 += childSize.Width;
							}
							else
							{
								double num10 = childSize.Width - num9;
								num2 = num + num10;
								num6 += num10;
							}
							list.Add(num8);
							list2.Add(i);
							num4 = i;
							lastVisibleNonFrozenDisplayIndex = i;
						}
					}
					else
					{
						if (!flag3 && GenerateChild(itemContainerGenerator, constraint, dataGridColumn, ref generatorState, ref childIndex, out childSize) == null)
						{
							break;
						}
						if (num4 < 0)
						{
							num4 = i;
						}
						lastVisibleNonFrozenDisplayIndex = i;
						num2 += childSize.Width;
						num6 += childSize.Width;
						list.Add(num8);
						list2.Add(i);
					}
				}
				result.Width += childSize.Width;
				result.Height = Math.Max(result.Height, childSize.Height);
			}
		}
		finally
		{
			if (generatorState != null)
			{
				generatorState.Dispose();
				generatorState = null;
			}
		}
		if (!hasVisibleStarColumns && !flag2)
		{
			if (ParentPresenter is DataGridColumnHeadersPresenter)
			{
				Size size = EnsureAtleastOneHeader(itemContainerGenerator, constraint, list, list2);
				result.Height = Math.Max(result.Height, size.Height);
				redeterminationNeeded = true;
			}
			else
			{
				EnsureFocusTrail(list, list2, num4, lastVisibleNonFrozenDisplayIndex, constraint);
			}
		}
		UpdateRealizedBlockLists(list, list2, redeterminationNeeded);
		VirtualizeChildren(RealizedColumnsBlockList, itemContainerGenerator);
		return result;
	}

	private void UpdateRealizedBlockLists(List<int> realizedColumnIndices, List<int> realizedColumnDisplayIndices, bool redeterminationNeeded)
	{
		realizedColumnIndices.Sort();
		RealizedColumnsBlockList = BuildRealizedColumnsBlockList(realizedColumnIndices);
		RealizedColumnsDisplayIndexBlockList = BuildRealizedColumnsBlockList(realizedColumnDisplayIndices);
		if (!redeterminationNeeded)
		{
			RebuildRealizedColumnsBlockList = false;
		}
	}

	private static List<RealizedColumnsBlock> BuildRealizedColumnsBlockList(List<int> indexList)
	{
		List<RealizedColumnsBlock> list = new List<RealizedColumnsBlock>();
		if (indexList.Count == 1)
		{
			list.Add(new RealizedColumnsBlock(indexList[0], indexList[0], 0));
		}
		else if (indexList.Count > 0)
		{
			int startIndex = indexList[0];
			int i = 1;
			for (int count = indexList.Count; i < count; i++)
			{
				if (indexList[i] != indexList[i - 1] + 1)
				{
					if (list.Count == 0)
					{
						list.Add(new RealizedColumnsBlock(startIndex, indexList[i - 1], 0));
					}
					else
					{
						RealizedColumnsBlock realizedColumnsBlock = list[list.Count - 1];
						int startIndexOffset = realizedColumnsBlock.StartIndexOffset + realizedColumnsBlock.EndIndex - realizedColumnsBlock.StartIndex + 1;
						list.Add(new RealizedColumnsBlock(startIndex, indexList[i - 1], startIndexOffset));
					}
					startIndex = indexList[i];
				}
				if (i == count - 1)
				{
					if (list.Count == 0)
					{
						list.Add(new RealizedColumnsBlock(startIndex, indexList[i], 0));
						continue;
					}
					RealizedColumnsBlock realizedColumnsBlock2 = list[list.Count - 1];
					int startIndexOffset2 = realizedColumnsBlock2.StartIndexOffset + realizedColumnsBlock2.EndIndex - realizedColumnsBlock2.StartIndex + 1;
					list.Add(new RealizedColumnsBlock(startIndex, indexList[i], startIndexOffset2));
				}
			}
		}
		return list;
	}

	private static GeneratorPosition IndexToGeneratorPositionForStart(IItemContainerGenerator generator, int index, out int childIndex)
	{
		GeneratorPosition result = generator?.GeneratorPositionFromIndex(index) ?? new GeneratorPosition(-1, index + 1);
		childIndex = ((result.Offset == 0) ? result.Index : (result.Index + 1));
		return result;
	}

	private UIElement GenerateChild(IItemContainerGenerator generator, Size constraint, DataGridColumn column, ref IDisposable generatorState, ref int childIndex, out Size childSize)
	{
		if (generatorState == null)
		{
			generatorState = generator.StartAt(IndexToGeneratorPositionForStart(generator, childIndex, out childIndex), GeneratorDirection.Forward, allowStartAtRealizedItem: true);
		}
		return GenerateChild(generator, constraint, column, ref childIndex, out childSize);
	}

	private UIElement GenerateChild(IItemContainerGenerator generator, Size constraint, DataGridColumn column, ref int childIndex, out Size childSize)
	{
		if (!(generator.GenerateNext(out var isNewlyRealized) is UIElement uIElement))
		{
			childSize = default(Size);
			return null;
		}
		AddContainerFromGenerator(childIndex, uIElement, isNewlyRealized);
		childIndex++;
		MeasureChild(uIElement, constraint);
		DataGridLength width = column.Width;
		childSize = uIElement.DesiredSize;
		if (!double.IsNaN(width.DisplayValue))
		{
			childSize = new Size(width.DisplayValue, childSize.Height);
		}
		return uIElement;
	}

	private Size GenerateChildren(IItemContainerGenerator generator, int startIndex, int endIndex, Size constraint)
	{
		double num = 0.0;
		double num2 = 0.0;
		int childIndex;
		GeneratorPosition position = IndexToGeneratorPositionForStart(generator, startIndex, out childIndex);
		DataGrid parentDataGrid = ParentDataGrid;
		using (generator.StartAt(position, GeneratorDirection.Forward, allowStartAtRealizedItem: true))
		{
			for (int i = startIndex; i <= endIndex; i++)
			{
				if (parentDataGrid.Columns[i].IsVisible)
				{
					if (GenerateChild(generator, constraint, parentDataGrid.Columns[i], ref childIndex, out var childSize) == null)
					{
						return new Size(num, num2);
					}
					num += childSize.Width;
					num2 = Math.Max(num2, childSize.Height);
				}
			}
		}
		return new Size(num, num2);
	}

	private void AddContainerFromGenerator(int childIndex, UIElement child, bool newlyRealized)
	{
		if (!newlyRealized)
		{
			if (InRecyclingMode)
			{
				IList realizedChildren = RealizedChildren;
				if (childIndex >= realizedChildren.Count || realizedChildren[childIndex] != child)
				{
					InsertRecycledContainer(childIndex, child);
					child.Measure(default(Size));
				}
			}
		}
		else
		{
			InsertNewContainer(childIndex, child);
		}
	}

	private void InsertRecycledContainer(int childIndex, UIElement container)
	{
		InsertContainer(childIndex, container, isRecycled: true);
	}

	private void InsertNewContainer(int childIndex, UIElement container)
	{
		InsertContainer(childIndex, container, isRecycled: false);
	}

	private void InsertContainer(int childIndex, UIElement container, bool isRecycled)
	{
		UIElementCollection internalChildren = base.InternalChildren;
		int num = 0;
		if (childIndex > 0)
		{
			num = ChildIndexFromRealizedIndex(childIndex - 1);
			num++;
		}
		if (!isRecycled || num >= internalChildren.Count || internalChildren[num] != container)
		{
			if (num < internalChildren.Count)
			{
				int num2 = num;
				if (isRecycled && VisualTreeHelper.GetParent(container) != null)
				{
					int num3 = internalChildren.IndexOf(container);
					RemoveInternalChildRange(num3, 1);
					if (num3 < num2)
					{
						num2--;
					}
					InsertInternalChild(num2, container);
				}
				else
				{
					InsertInternalChild(num2, container);
				}
			}
			else if (isRecycled && VisualTreeHelper.GetParent(container) != null)
			{
				int index = internalChildren.IndexOf(container);
				RemoveInternalChildRange(index, 1);
				AddInternalChild(container);
			}
			else
			{
				AddInternalChild(container);
			}
		}
		if (IsVirtualizing && InRecyclingMode)
		{
			_realizedChildren.Insert(childIndex, container);
		}
		base.ItemContainerGenerator.PrepareItemContainer(container);
	}

	private int ChildIndexFromRealizedIndex(int realizedChildIndex)
	{
		if (IsVirtualizing && InRecyclingMode && realizedChildIndex < _realizedChildren.Count)
		{
			UIElement uIElement = _realizedChildren[realizedChildIndex];
			UIElementCollection internalChildren = base.InternalChildren;
			for (int i = realizedChildIndex; i < internalChildren.Count; i++)
			{
				if (internalChildren[i] == uIElement)
				{
					return i;
				}
			}
		}
		return realizedChildIndex;
	}

	private static bool InBlockOrNextBlock(List<RealizedColumnsBlock> blockList, int index, ref int blockIndex, ref RealizedColumnsBlock block, out bool pastLastBlock)
	{
		pastLastBlock = false;
		bool result = true;
		if (index < block.StartIndex)
		{
			result = false;
		}
		else if (index > block.EndIndex)
		{
			if (blockIndex == blockList.Count - 1)
			{
				blockIndex++;
				pastLastBlock = true;
				result = false;
			}
			else
			{
				block = blockList[++blockIndex];
				if (index < block.StartIndex || index > block.EndIndex)
				{
					result = false;
				}
			}
		}
		return result;
	}

	private Size EnsureAtleastOneHeader(IItemContainerGenerator generator, Size constraint, List<int> realizedColumnIndices, List<int> realizedColumnDisplayIndices)
	{
		DataGrid parentDataGrid = ParentDataGrid;
		int count = parentDataGrid.Columns.Count;
		Size childSize = default(Size);
		if (RealizedChildren.Count == 0 && count > 0)
		{
			for (int i = 0; i < count; i++)
			{
				DataGridColumn dataGridColumn = parentDataGrid.Columns[i];
				if (!dataGridColumn.IsVisible)
				{
					continue;
				}
				int childIndex = i;
				using (generator.StartAt(IndexToGeneratorPositionForStart(generator, childIndex, out childIndex), GeneratorDirection.Forward, allowStartAtRealizedItem: true))
				{
					if (GenerateChild(generator, constraint, dataGridColumn, ref childIndex, out childSize) != null)
					{
						int displayIndexListIterator = 0;
						AddToIndicesListIfNeeded(realizedColumnIndices, realizedColumnDisplayIndices, i, dataGridColumn.DisplayIndex, ref displayIndexListIterator);
						return childSize;
					}
				}
			}
		}
		return childSize;
	}

	private void EnsureFocusTrail(List<int> realizedColumnIndices, List<int> realizedColumnDisplayIndices, int firstVisibleNonFrozenDisplayIndex, int lastVisibleNonFrozenDisplayIndex, Size constraint)
	{
		if (firstVisibleNonFrozenDisplayIndex < 0)
		{
			return;
		}
		int frozenColumnCount = ParentDataGrid.FrozenColumnCount;
		int count = Columns.Count;
		ItemsControl parentPresenter = ParentPresenter;
		if (parentPresenter == null)
		{
			return;
		}
		ItemContainerGenerator itemContainerGenerator = parentPresenter.ItemContainerGenerator;
		int displayIndexListIterator = 0;
		int num = -1;
		for (int i = 0; i < firstVisibleNonFrozenDisplayIndex; i++)
		{
			if (GenerateChildForFocusTrail(itemContainerGenerator, realizedColumnIndices, realizedColumnDisplayIndices, constraint, i, ref displayIndexListIterator))
			{
				num = i;
				break;
			}
		}
		if (num < frozenColumnCount)
		{
			for (int j = frozenColumnCount; j < count; j++)
			{
				if (GenerateChildForFocusTrail(itemContainerGenerator, realizedColumnIndices, realizedColumnDisplayIndices, constraint, j, ref displayIndexListIterator))
				{
					num = j;
					break;
				}
			}
		}
		for (int num2 = firstVisibleNonFrozenDisplayIndex - 1; num2 > num; num2--)
		{
			if (GenerateChildForFocusTrail(itemContainerGenerator, realizedColumnIndices, realizedColumnDisplayIndices, constraint, num2, ref displayIndexListIterator))
			{
				num = num2;
				break;
			}
		}
		for (int k = lastVisibleNonFrozenDisplayIndex + 1; k < count; k++)
		{
			if (GenerateChildForFocusTrail(itemContainerGenerator, realizedColumnIndices, realizedColumnDisplayIndices, constraint, k, ref displayIndexListIterator))
			{
				num = k;
				break;
			}
		}
		int num3 = count - 1;
		while (num3 > num && !GenerateChildForFocusTrail(itemContainerGenerator, realizedColumnIndices, realizedColumnDisplayIndices, constraint, num3, ref displayIndexListIterator))
		{
			num3--;
		}
	}

	private bool GenerateChildForFocusTrail(ItemContainerGenerator generator, List<int> realizedColumnIndices, List<int> realizedColumnDisplayIndices, Size constraint, int displayIndex, ref int displayIndexListIterator)
	{
		DataGrid parentDataGrid = ParentDataGrid;
		DataGridColumn dataGridColumn = parentDataGrid.ColumnFromDisplayIndex(displayIndex);
		if (dataGridColumn.IsVisible)
		{
			int num = parentDataGrid.ColumnIndexFromDisplayIndex(displayIndex);
			UIElement uIElement = generator.ContainerFromIndex(num) as UIElement;
			if (uIElement == null)
			{
				int childIndex = num;
				using (((IItemContainerGenerator)generator).StartAt(IndexToGeneratorPositionForStart(generator, childIndex, out childIndex), GeneratorDirection.Forward, allowStartAtRealizedItem: true))
				{
					uIElement = GenerateChild(generator, constraint, dataGridColumn, ref childIndex, out var _);
				}
			}
			if (uIElement != null && DataGridHelper.TreeHasFocusAndTabStop(uIElement))
			{
				AddToIndicesListIfNeeded(realizedColumnIndices, realizedColumnDisplayIndices, num, displayIndex, ref displayIndexListIterator);
				return true;
			}
		}
		return false;
	}

	private static void AddToIndicesListIfNeeded(List<int> realizedColumnIndices, List<int> realizedColumnDisplayIndices, int columnIndex, int displayIndex, ref int displayIndexListIterator)
	{
		int count = realizedColumnDisplayIndices.Count;
		while (displayIndexListIterator < count)
		{
			if (realizedColumnDisplayIndices[displayIndexListIterator] == displayIndex)
			{
				return;
			}
			if (realizedColumnDisplayIndices[displayIndexListIterator] > displayIndex)
			{
				realizedColumnDisplayIndices.Insert(displayIndexListIterator, displayIndex);
				realizedColumnIndices.Add(columnIndex);
				return;
			}
			displayIndexListIterator++;
		}
		realizedColumnIndices.Add(columnIndex);
		realizedColumnDisplayIndices.Add(displayIndex);
	}

	private void VirtualizeChildren(List<RealizedColumnsBlock> blockList, IItemContainerGenerator generator)
	{
		DataGrid parentDataGrid = ParentDataGrid;
		ObservableCollection<DataGridColumn> columns = parentDataGrid.Columns;
		int count = columns.Count;
		int i = 0;
		IList realizedChildren = RealizedChildren;
		int num = realizedChildren.Count;
		if (num == 0)
		{
			return;
		}
		int blockIndex = 0;
		int count2 = blockList.Count;
		RealizedColumnsBlock block = ((count2 > 0) ? blockList[blockIndex] : new RealizedColumnsBlock(-1, -1, -1));
		bool pastLastBlock = count2 <= 0;
		int num2 = -1;
		int num3 = 0;
		int num4 = -1;
		ItemsControl parentPresenter = ParentPresenter;
		DataGridCellsPresenter dataGridCellsPresenter = parentPresenter as DataGridCellsPresenter;
		DataGridColumnHeadersPresenter dataGridColumnHeadersPresenter = parentPresenter as DataGridColumnHeadersPresenter;
		for (int j = 0; j < num; j++)
		{
			int num5 = j;
			UIElement uIElement = realizedChildren[j] as UIElement;
			if (uIElement is IProvideDataGridColumn { Column: var column })
			{
				for (; i < count && column != columns[i]; i++)
				{
				}
				num5 = i++;
			}
			bool flag = pastLastBlock || !InBlockOrNextBlock(blockList, num5, ref blockIndex, ref block, out pastLastBlock);
			if ((uIElement is DataGridCell dataGridCell && (dataGridCell.IsEditing || dataGridCell.IsKeyboardFocusWithin || dataGridCell == parentDataGrid.FocusedCell)) || (dataGridCellsPresenter != null && dataGridCellsPresenter.IsItemItsOwnContainerInternal(dataGridCellsPresenter.Items[num5])) || (dataGridColumnHeadersPresenter != null && dataGridColumnHeadersPresenter.IsItemItsOwnContainerInternal(dataGridColumnHeadersPresenter.Items[num5])))
			{
				flag = false;
			}
			if (!columns[num5].IsVisible)
			{
				flag = true;
			}
			if (flag)
			{
				if (num2 == -1)
				{
					num2 = j;
					num3 = 1;
				}
				else if (num4 == num5 - 1)
				{
					num3++;
				}
				else
				{
					CleanupRange(realizedChildren, generator, num2, num3);
					num -= num3;
					j -= num3;
					num3 = 1;
					num2 = j;
				}
				num4 = num5;
			}
			else if (num3 > 0)
			{
				CleanupRange(realizedChildren, generator, num2, num3);
				num -= num3;
				j -= num3;
				num3 = 0;
				num2 = -1;
			}
		}
		if (num3 > 0)
		{
			CleanupRange(realizedChildren, generator, num2, num3);
		}
	}

	private void CleanupRange(IList children, IItemContainerGenerator generator, int startIndex, int count)
	{
		if (count > 0)
		{
			if (IsVirtualizing && InRecyclingMode)
			{
				GeneratorPosition position = new GeneratorPosition(startIndex, 0);
				((IRecyclingItemContainerGenerator)generator).Recycle(position, count);
				_realizedChildren.RemoveRange(startIndex, count);
			}
			else
			{
				RemoveInternalChildRange(startIndex, count);
				generator.Remove(new GeneratorPosition(startIndex, 0), count);
			}
		}
	}

	private void DisconnectRecycledContainers()
	{
		int num = 0;
		UIElement uIElement = ((_realizedChildren.Count > 0) ? _realizedChildren[0] : null);
		UIElementCollection internalChildren = base.InternalChildren;
		int num2 = -1;
		int num3 = 0;
		for (int i = 0; i < internalChildren.Count; i++)
		{
			if (internalChildren[i] == uIElement)
			{
				if (num3 > 0)
				{
					RemoveInternalChildRange(num2, num3);
					i -= num3;
					num3 = 0;
					num2 = -1;
				}
				num++;
				uIElement = ((num >= _realizedChildren.Count) ? null : _realizedChildren[num]);
			}
			else
			{
				if (num2 == -1)
				{
					num2 = i;
				}
				num3++;
			}
		}
		if (num3 > 0)
		{
			RemoveInternalChildRange(num2, num3);
		}
	}

	private void InitializeArrangeState(ArrangeState arrangeState)
	{
		DataGrid parentDataGrid = ParentDataGrid;
		double horizontalScrollOffset = parentDataGrid.HorizontalScrollOffset;
		double cellsPanelHorizontalOffset = parentDataGrid.CellsPanelHorizontalOffset;
		arrangeState.NextFrozenCellStart = horizontalScrollOffset;
		arrangeState.NextNonFrozenCellStart -= cellsPanelHorizontalOffset;
		arrangeState.ViewportStartX = horizontalScrollOffset - cellsPanelHorizontalOffset;
		arrangeState.FrozenColumnCount = parentDataGrid.FrozenColumnCount;
	}

	private void FinishArrange(ArrangeState arrangeState)
	{
		DataGrid parentDataGrid = ParentDataGrid;
		if (parentDataGrid != null)
		{
			parentDataGrid.NonFrozenColumnsViewportHorizontalOffset = arrangeState.DataGridHorizontalScrollStartX;
		}
		if (arrangeState.OldClippedChild != null)
		{
			arrangeState.OldClippedChild.CoerceValue(UIElement.ClipProperty);
		}
		_clippedChildForFrozenBehaviour = arrangeState.NewClippedChild;
		if (_clippedChildForFrozenBehaviour != null)
		{
			_clippedChildForFrozenBehaviour.CoerceValue(UIElement.ClipProperty);
		}
	}

	private void SetDataGridCellPanelWidth(IList children, double newWidth)
	{
		if (children.Count != 0 && children[0] is DataGridColumnHeader && !DoubleUtil.AreClose(ParentDataGrid.CellsPanelActualWidth, newWidth))
		{
			ParentDataGrid.CellsPanelActualWidth = newWidth;
		}
	}

	[Conditional("DEBUG")]
	private static void Debug_VerifyRealizedIndexCountVsDisplayIndexCount(List<RealizedColumnsBlock> blockList, List<RealizedColumnsBlock> displayIndexBlockList)
	{
		_ = blockList[blockList.Count - 1];
		_ = displayIndexBlockList[displayIndexBlockList.Count - 1];
	}

	/// <summary>Determines the final size and placement of the panel.</summary>
	/// <returns>The final size and placement of the panel.</returns>
	/// <param name="arrangeSize">The maximum size that the panel can occupy.</param>
	protected override Size ArrangeOverride(Size arrangeSize)
	{
		IList realizedChildren = RealizedChildren;
		ArrangeState arrangeState = new ArrangeState();
		arrangeState.ChildHeight = arrangeSize.Height;
		DataGrid parentDataGrid = ParentDataGrid;
		if (parentDataGrid != null)
		{
			parentDataGrid.QueueInvalidateCellsPanelHorizontalOffset();
			SetDataGridCellPanelWidth(realizedChildren, arrangeSize.Width);
			InitializeArrangeState(arrangeState);
		}
		List<RealizedColumnsBlock> realizedColumnsDisplayIndexBlockList = RealizedColumnsDisplayIndexBlockList;
		if (realizedColumnsDisplayIndexBlockList != null && realizedColumnsDisplayIndexBlockList.Count > 0)
		{
			double averageColumnWidth = parentDataGrid.InternalColumns.AverageColumnWidth;
			List<RealizedColumnsBlock> realizedColumnsBlockList = RealizedColumnsBlockList;
			List<int> realizedChildrenNotInBlockList = GetRealizedChildrenNotInBlockList(realizedColumnsBlockList, realizedChildren);
			int num = -1;
			RealizedColumnsBlock block = realizedColumnsDisplayIndexBlockList[++num];
			bool pastLastBlock = false;
			int i = 0;
			for (int count = parentDataGrid.Columns.Count; i < count; i++)
			{
				bool flag = InBlockOrNextBlock(realizedColumnsDisplayIndexBlockList, i, ref num, ref block, out pastLastBlock);
				if (pastLastBlock)
				{
					break;
				}
				if (flag)
				{
					int num2 = parentDataGrid.ColumnIndexFromDisplayIndex(i);
					RealizedColumnsBlock realizedBlockForColumn = GetRealizedBlockForColumn(realizedColumnsBlockList, num2);
					int num3 = realizedBlockForColumn.StartIndexOffset + num2 - realizedBlockForColumn.StartIndex;
					if (realizedChildrenNotInBlockList != null)
					{
						int j = 0;
						for (int count2 = realizedChildrenNotInBlockList.Count; j < count2 && realizedChildrenNotInBlockList[j] <= num3; j++)
						{
							num3++;
						}
					}
					ArrangeChild(realizedChildren[num3] as UIElement, i, arrangeState);
				}
				else
				{
					DataGridColumn dataGridColumn = parentDataGrid.ColumnFromDisplayIndex(i);
					if (dataGridColumn.IsVisible)
					{
						double columnEstimatedMeasureWidth = GetColumnEstimatedMeasureWidth(dataGridColumn, averageColumnWidth);
						arrangeState.NextNonFrozenCellStart += columnEstimatedMeasureWidth;
					}
				}
			}
			if (realizedChildrenNotInBlockList != null)
			{
				int k = 0;
				for (int count3 = realizedChildrenNotInBlockList.Count; k < count3; k++)
				{
					(realizedChildren[realizedChildrenNotInBlockList[k]] as UIElement).Arrange(default(Rect));
				}
			}
		}
		FinishArrange(arrangeState);
		return arrangeSize;
	}

	private void ArrangeChild(UIElement child, int displayIndex, ArrangeState arrangeState)
	{
		double num = 0.0;
		IProvideDataGridColumn provideDataGridColumn = child as IProvideDataGridColumn;
		if (child == _clippedChildForFrozenBehaviour)
		{
			arrangeState.OldClippedChild = child;
			_clippedChildForFrozenBehaviour = null;
		}
		if (provideDataGridColumn != null)
		{
			num = provideDataGridColumn.Column.Width.DisplayValue;
			if (double.IsNaN(num))
			{
				num = provideDataGridColumn.Column.ActualWidth;
			}
		}
		else
		{
			num = child.DesiredSize.Width;
		}
		Rect finalRect = new Rect(new Size(num, arrangeState.ChildHeight));
		if (displayIndex < arrangeState.FrozenColumnCount)
		{
			finalRect.X = arrangeState.NextFrozenCellStart;
			arrangeState.NextFrozenCellStart += num;
			arrangeState.DataGridHorizontalScrollStartX += num;
		}
		else if (DoubleUtil.LessThanOrClose(arrangeState.NextNonFrozenCellStart, arrangeState.ViewportStartX))
		{
			if (DoubleUtil.LessThanOrClose(arrangeState.NextNonFrozenCellStart + num, arrangeState.ViewportStartX))
			{
				finalRect.X = arrangeState.NextNonFrozenCellStart;
				arrangeState.NextNonFrozenCellStart += num;
			}
			else
			{
				double num2 = arrangeState.ViewportStartX - arrangeState.NextNonFrozenCellStart;
				if (DoubleUtil.AreClose(num2, 0.0))
				{
					finalRect.X = arrangeState.NextFrozenCellStart;
					arrangeState.NextNonFrozenCellStart = arrangeState.NextFrozenCellStart + num;
				}
				else
				{
					finalRect.X = arrangeState.NextFrozenCellStart - num2;
					double num3 = num - num2;
					arrangeState.NewClippedChild = child;
					_childClipForFrozenBehavior.Rect = new Rect(num2, 0.0, num3, finalRect.Height);
					arrangeState.NextNonFrozenCellStart = arrangeState.NextFrozenCellStart + num3;
				}
			}
		}
		else
		{
			finalRect.X = arrangeState.NextNonFrozenCellStart;
			arrangeState.NextNonFrozenCellStart += num;
		}
		child.Arrange(finalRect);
	}

	private static RealizedColumnsBlock GetRealizedBlockForColumn(List<RealizedColumnsBlock> blockList, int columnIndex)
	{
		int i = 0;
		for (int count = blockList.Count; i < count; i++)
		{
			RealizedColumnsBlock result = blockList[i];
			if (columnIndex >= result.StartIndex && columnIndex <= result.EndIndex)
			{
				return result;
			}
		}
		return new RealizedColumnsBlock(-1, -1, -1);
	}

	private List<int> GetRealizedChildrenNotInBlockList(List<RealizedColumnsBlock> blockList, IList children)
	{
		DataGrid parentDataGrid = ParentDataGrid;
		RealizedColumnsBlock realizedColumnsBlock = blockList[blockList.Count - 1];
		int num = realizedColumnsBlock.StartIndexOffset + realizedColumnsBlock.EndIndex - realizedColumnsBlock.StartIndex + 1;
		if (children.Count == num)
		{
			return null;
		}
		List<int> list = new List<int>();
		if (blockList.Count == 0)
		{
			int i = 0;
			for (int count = children.Count; i < count; i++)
			{
				list.Add(i);
			}
		}
		else
		{
			int num2 = 0;
			RealizedColumnsBlock realizedColumnsBlock2 = blockList[num2++];
			int j = 0;
			for (int count2 = children.Count; j < count2; j++)
			{
				IProvideDataGridColumn provideDataGridColumn = children[j] as IProvideDataGridColumn;
				int num3 = j;
				if (provideDataGridColumn != null)
				{
					num3 = parentDataGrid.Columns.IndexOf(provideDataGridColumn.Column);
				}
				if (num3 < realizedColumnsBlock2.StartIndex)
				{
					list.Add(j);
				}
				else
				{
					if (num3 <= realizedColumnsBlock2.EndIndex)
					{
						continue;
					}
					if (num2 >= blockList.Count)
					{
						for (int k = j; k < count2; k++)
						{
							list.Add(k);
						}
						break;
					}
					realizedColumnsBlock2 = blockList[num2++];
					if (num3 < realizedColumnsBlock2.StartIndex)
					{
						list.Add(j);
					}
				}
			}
		}
		return list;
	}

	/// <summary>Indicates that the <see cref="P:System.Windows.Controls.Panel.IsItemsHost" /> property value has changed.</summary>
	/// <param name="oldIsItemsHost">The old value of the <see cref="P:System.Windows.Controls.Panel.IsItemsHost" /> property.</param>
	/// <param name="newIsItemsHost">The new value of the <see cref="P:System.Windows.Controls.Panel.IsItemsHost" /> property.</param>
	protected override void OnIsItemsHostChanged(bool oldIsItemsHost, bool newIsItemsHost)
	{
		base.OnIsItemsHostChanged(oldIsItemsHost, newIsItemsHost);
		if (newIsItemsHost)
		{
			ItemsControl parentPresenter = ParentPresenter;
			if (parentPresenter == null)
			{
				return;
			}
			IItemContainerGenerator itemContainerGenerator = parentPresenter.ItemContainerGenerator;
			if (itemContainerGenerator != null && itemContainerGenerator == itemContainerGenerator.GetItemContainerGeneratorForPanel(this))
			{
				if (parentPresenter is DataGridCellsPresenter dataGridCellsPresenter)
				{
					dataGridCellsPresenter.InternalItemsHost = this;
				}
				else if (parentPresenter is DataGridColumnHeadersPresenter dataGridColumnHeadersPresenter)
				{
					dataGridColumnHeadersPresenter.InternalItemsHost = this;
				}
			}
			return;
		}
		ItemsControl parentPresenter2 = ParentPresenter;
		if (parentPresenter2 == null)
		{
			return;
		}
		if (parentPresenter2 is DataGridCellsPresenter dataGridCellsPresenter2)
		{
			if (dataGridCellsPresenter2.InternalItemsHost == this)
			{
				dataGridCellsPresenter2.InternalItemsHost = null;
			}
		}
		else if (parentPresenter2 is DataGridColumnHeadersPresenter dataGridColumnHeadersPresenter2 && dataGridColumnHeadersPresenter2.InternalItemsHost == this)
		{
			dataGridColumnHeadersPresenter2.InternalItemsHost = null;
		}
	}

	private void DetermineVirtualizationState()
	{
		ItemsControl parentPresenter = ParentPresenter;
		if (parentPresenter != null)
		{
			IsVirtualizing = VirtualizingPanel.GetIsVirtualizing(parentPresenter);
			InRecyclingMode = VirtualizingPanel.GetVirtualizationMode(parentPresenter) == VirtualizationMode.Recycling;
		}
	}

	private static double GetColumnEstimatedMeasureWidth(DataGridColumn column, double averageColumnWidth)
	{
		if (!column.IsVisible)
		{
			return 0.0;
		}
		double num = column.Width.DisplayValue;
		if (double.IsNaN(num))
		{
			num = Math.Max(averageColumnWidth, column.MinWidth);
			num = Math.Min(num, column.MaxWidth);
		}
		return num;
	}

	private double GetColumnEstimatedMeasureWidthSum(int startIndex, int endIndex, double averageColumnWidth)
	{
		double num = 0.0;
		DataGrid parentDataGrid = ParentDataGrid;
		for (int i = startIndex; i <= endIndex; i++)
		{
			num += GetColumnEstimatedMeasureWidth(parentDataGrid.Columns[i], averageColumnWidth);
		}
		return num;
	}

	private void EnsureRealizedChildren()
	{
		if (IsVirtualizing && InRecyclingMode)
		{
			if (_realizedChildren == null)
			{
				UIElementCollection internalChildren = base.InternalChildren;
				_realizedChildren = new List<UIElement>(internalChildren.Count);
				for (int i = 0; i < internalChildren.Count; i++)
				{
					_realizedChildren.Add(internalChildren[i]);
				}
			}
		}
		else
		{
			_realizedChildren = null;
		}
	}

	internal double ComputeCellsPanelHorizontalOffset()
	{
		double result = 0.0;
		DataGrid parentDataGrid = ParentDataGrid;
		double horizontalScrollOffset = parentDataGrid.HorizontalScrollOffset;
		ScrollViewer internalScrollHost = parentDataGrid.InternalScrollHost;
		if (internalScrollHost != null)
		{
			result = horizontalScrollOffset + TransformToAncestor(internalScrollHost).Transform(default(Point)).X;
		}
		return result;
	}

	private double GetViewportWidth()
	{
		double num = 0.0;
		DataGrid parentDataGrid = ParentDataGrid;
		if (parentDataGrid != null)
		{
			ScrollContentPresenter internalScrollContentPresenter = parentDataGrid.InternalScrollContentPresenter;
			if (internalScrollContentPresenter != null && !internalScrollContentPresenter.CanContentScroll)
			{
				num = internalScrollContentPresenter.ViewportWidth;
			}
			else if (parentDataGrid.InternalItemsHost is IScrollInfo scrollInfo)
			{
				num = scrollInfo.ViewportWidth;
			}
		}
		DataGridRowsPresenter parentRowsPresenter = ParentRowsPresenter;
		if (DoubleUtil.AreClose(num, 0.0) && parentRowsPresenter != null)
		{
			Size availableSize = parentRowsPresenter.AvailableSize;
			if (!double.IsNaN(availableSize.Width) && !double.IsInfinity(availableSize.Width))
			{
				num = availableSize.Width;
			}
			else if (parentDataGrid.IsGrouping)
			{
				IHierarchicalVirtualizationAndScrollInfo hierarchicalVirtualizationAndScrollInfo = DataGridHelper.FindParent<GroupItem>(parentRowsPresenter);
				if (hierarchicalVirtualizationAndScrollInfo != null)
				{
					num = hierarchicalVirtualizationAndScrollInfo.Constraints.Viewport.Width;
				}
			}
		}
		return num;
	}

	/// <summary>Updates the visible cells when an item in the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection has changed.</summary>
	/// <param name="sender">The object that raised the <see cref="E:System.Windows.Controls.ItemContainerGenerator.ItemsChanged" /> event.</param>
	/// <param name="args">The data for the event.</param>
	protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
	{
		base.OnItemsChanged(sender, args);
		switch (args.Action)
		{
		case NotifyCollectionChangedAction.Remove:
			OnItemsRemove(args);
			break;
		case NotifyCollectionChangedAction.Replace:
			OnItemsReplace(args);
			break;
		case NotifyCollectionChangedAction.Move:
			OnItemsMove(args);
			break;
		case NotifyCollectionChangedAction.Reset:
			break;
		}
	}

	private void OnItemsRemove(ItemsChangedEventArgs args)
	{
		RemoveChildRange(args.Position, args.ItemCount, args.ItemUICount);
	}

	private void OnItemsReplace(ItemsChangedEventArgs args)
	{
		RemoveChildRange(args.Position, args.ItemCount, args.ItemUICount);
	}

	private void OnItemsMove(ItemsChangedEventArgs args)
	{
		RemoveChildRange(args.OldPosition, args.ItemCount, args.ItemUICount);
	}

	private void RemoveChildRange(GeneratorPosition position, int itemCount, int itemUICount)
	{
		if (!base.IsItemsHost)
		{
			return;
		}
		UIElementCollection internalChildren = base.InternalChildren;
		int num = position.Index;
		if (position.Offset > 0)
		{
			num++;
		}
		if (num < internalChildren.Count && itemUICount > 0)
		{
			RemoveInternalChildRange(num, itemUICount);
			if (IsVirtualizing && InRecyclingMode)
			{
				_realizedChildren.RemoveRange(num, itemUICount);
			}
		}
	}

	/// <summary>Called when the collection of children in the <see cref="T:System.Windows.Controls.DataGrid" /> is cleared.</summary>
	protected override void OnClearChildren()
	{
		base.OnClearChildren();
		_realizedChildren = null;
	}

	internal void InternalBringIndexIntoView(int index)
	{
		BringIndexIntoView(index);
	}

	/// <summary>Scrolls the viewport to the item at the specified index.</summary>
	/// <param name="index">The index of the item that should become visible.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index is out of range.</exception>
	protected internal override void BringIndexIntoView(int index)
	{
		DataGrid parentDataGrid = ParentDataGrid;
		if (parentDataGrid == null)
		{
			base.BringIndexIntoView(index);
			return;
		}
		if (index < 0 || index >= parentDataGrid.Columns.Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (parentDataGrid.InternalColumns.ColumnWidthsComputationPending)
		{
			base.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action<int>(RetryBringIndexIntoView), index);
			return;
		}
		ScrollContentPresenter internalScrollContentPresenter = parentDataGrid.InternalScrollContentPresenter;
		IScrollInfo scrollInfo = null;
		if (internalScrollContentPresenter != null && !internalScrollContentPresenter.CanContentScroll)
		{
			scrollInfo = internalScrollContentPresenter;
		}
		else
		{
			ScrollViewer internalScrollHost = parentDataGrid.InternalScrollHost;
			if (internalScrollHost != null)
			{
				scrollInfo = internalScrollHost.ScrollInfo;
			}
		}
		if (scrollInfo == null)
		{
			base.BringIndexIntoView(index);
			return;
		}
		bool measureDirty = base.MeasureDirty;
		bool retryRequested = measureDirty;
		double newHorizontalOffset = 0.0;
		double value = parentDataGrid.HorizontalScrollOffset;
		while (!IsChildInView(index, out newHorizontalOffset) && !DoubleUtil.AreClose(value, newHorizontalOffset))
		{
			retryRequested = true;
			scrollInfo.SetHorizontalOffset(newHorizontalOffset);
			UpdateLayout();
			value = newHorizontalOffset;
		}
		if (parentDataGrid.RetryBringColumnIntoView(retryRequested))
		{
			DispatcherPriority priority = (measureDirty ? DispatcherPriority.Background : DispatcherPriority.Loaded);
			base.Dispatcher.BeginInvoke(priority, new Action<int>(RetryBringIndexIntoView), index);
			InvalidateMeasure();
		}
	}

	private void RetryBringIndexIntoView(int index)
	{
		DataGrid parentDataGrid = ParentDataGrid;
		if (parentDataGrid != null && 0 <= index && index < parentDataGrid.Columns.Count)
		{
			BringIndexIntoView(index);
		}
	}

	private bool IsChildInView(int index, out double newHorizontalOffset)
	{
		DataGrid parentDataGrid = ParentDataGrid;
		double num = (newHorizontalOffset = parentDataGrid.HorizontalScrollOffset);
		double averageColumnWidth = parentDataGrid.InternalColumns.AverageColumnWidth;
		int frozenColumnCount = parentDataGrid.FrozenColumnCount;
		double cellsPanelHorizontalOffset = parentDataGrid.CellsPanelHorizontalOffset;
		double viewportWidth = GetViewportWidth();
		double num2 = num;
		double num3 = 0.0 - cellsPanelHorizontalOffset;
		double num4 = num - cellsPanelHorizontalOffset;
		int displayIndex = Columns[index].DisplayIndex;
		double num5 = 0.0;
		double num6 = 0.0;
		for (int i = 0; i <= displayIndex; i++)
		{
			DataGridColumn dataGridColumn = parentDataGrid.ColumnFromDisplayIndex(i);
			if (!dataGridColumn.IsVisible)
			{
				continue;
			}
			double columnEstimatedMeasureWidth = GetColumnEstimatedMeasureWidth(dataGridColumn, averageColumnWidth);
			if (i < frozenColumnCount)
			{
				num5 = num2;
				num6 = num5 + columnEstimatedMeasureWidth;
				num2 += columnEstimatedMeasureWidth;
			}
			else if (DoubleUtil.LessThanOrClose(num3, num4))
			{
				if (DoubleUtil.LessThanOrClose(num3 + columnEstimatedMeasureWidth, num4))
				{
					num5 = num3;
					num6 = num5 + columnEstimatedMeasureWidth;
					num3 += columnEstimatedMeasureWidth;
					continue;
				}
				num5 = num2;
				double num7 = num4 - num3;
				if (DoubleUtil.AreClose(num7, 0.0))
				{
					num6 = num5 + columnEstimatedMeasureWidth;
					num3 = num2 + columnEstimatedMeasureWidth;
					continue;
				}
				double num8 = columnEstimatedMeasureWidth - num7;
				num6 = num5 + num8;
				num3 = num2 + num8;
				if (i == displayIndex)
				{
					newHorizontalOffset = num - num7;
					return false;
				}
			}
			else
			{
				num5 = num3;
				num6 = num5 + columnEstimatedMeasureWidth;
				num3 += columnEstimatedMeasureWidth;
			}
		}
		double num9 = num4 + viewportWidth;
		if (DoubleUtil.LessThan(num5, num4))
		{
			newHorizontalOffset = num5 + cellsPanelHorizontalOffset;
		}
		else
		{
			if (!DoubleUtil.GreaterThan(num6, num9))
			{
				return true;
			}
			double num10 = num6 - num9;
			if (displayIndex < frozenColumnCount)
			{
				num2 -= num6 - num5;
			}
			if (DoubleUtil.LessThan(num5 - num10, num2))
			{
				num10 = num5 - num2;
			}
			if (DoubleUtil.AreClose(num10, 0.0))
			{
				return true;
			}
			newHorizontalOffset = num + num10;
		}
		return false;
	}

	internal Geometry GetFrozenClipForChild(UIElement child)
	{
		if (child == _clippedChildForFrozenBehaviour)
		{
			return _childClipForFrozenBehavior;
		}
		return null;
	}
}
