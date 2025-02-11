using System.Windows.Input;
using System.Windows.Media;

namespace System.Windows.Controls.Primitives;

/// <summary>Handles the layout of the <see cref="T:System.Windows.Controls.TabItem" /> objects on a <see cref="T:System.Windows.Controls.TabControl" />. </summary>
public class TabPanel : Panel
{
	private int _numRows = 1;

	private int _numHeaders;

	private double _rowHeight;

	private Dock TabStripPlacement
	{
		get
		{
			Dock result = Dock.Top;
			if (base.TemplatedParent is TabControl tabControl)
			{
				result = tabControl.TabStripPlacement;
			}
			return result;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.TabPanel" /> class. </summary>
	public TabPanel()
	{
	}

	static TabPanel()
	{
		KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(TabPanel), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
		KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(TabPanel), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
	}

	/// <summary>Called when remeasuring the control is required. </summary>
	/// <returns>The desired size.</returns>
	/// <param name="constraint">Constraint size is an upper limit. The return value should not exceed this size.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		Size result = default(Size);
		Dock tabStripPlacement = TabStripPlacement;
		_numRows = 1;
		_numHeaders = 0;
		_rowHeight = 0.0;
		switch (tabStripPlacement)
		{
		case Dock.Top:
		case Dock.Bottom:
		{
			int num = 0;
			double num2 = 0.0;
			double num3 = 0.0;
			foreach (UIElement internalChild in base.InternalChildren)
			{
				if (internalChild.Visibility == Visibility.Collapsed)
				{
					continue;
				}
				_numHeaders++;
				internalChild.Measure(constraint);
				Size desiredSizeWithoutMargin2 = GetDesiredSizeWithoutMargin(internalChild);
				if (_rowHeight < desiredSizeWithoutMargin2.Height)
				{
					_rowHeight = desiredSizeWithoutMargin2.Height;
				}
				if (num2 + desiredSizeWithoutMargin2.Width > constraint.Width && num > 0)
				{
					if (num3 < num2)
					{
						num3 = num2;
					}
					num2 = desiredSizeWithoutMargin2.Width;
					num = 1;
					_numRows++;
				}
				else
				{
					num2 += desiredSizeWithoutMargin2.Width;
					num++;
				}
			}
			if (num3 < num2)
			{
				num3 = num2;
			}
			result.Height = _rowHeight * (double)_numRows;
			if (double.IsInfinity(result.Width) || double.IsNaN(result.Width) || num3 < constraint.Width)
			{
				result.Width = num3;
			}
			else
			{
				result.Width = constraint.Width;
			}
			break;
		}
		case Dock.Left:
		case Dock.Right:
			foreach (UIElement internalChild2 in base.InternalChildren)
			{
				if (internalChild2.Visibility != Visibility.Collapsed)
				{
					_numHeaders++;
					internalChild2.Measure(constraint);
					Size desiredSizeWithoutMargin = GetDesiredSizeWithoutMargin(internalChild2);
					if (result.Width < desiredSizeWithoutMargin.Width)
					{
						result.Width = desiredSizeWithoutMargin.Width;
					}
					result.Height += desiredSizeWithoutMargin.Height;
				}
			}
			break;
		}
		return result;
	}

	/// <summary>Arranges and sizes the content of a <see cref="T:System.Windows.Controls.Primitives.TabPanel" /> object. </summary>
	/// <returns>The size of the tab panel.</returns>
	/// <param name="arrangeSize">The size that a tab panel assumes to position child elements.</param>
	protected override Size ArrangeOverride(Size arrangeSize)
	{
		switch (TabStripPlacement)
		{
		case Dock.Top:
		case Dock.Bottom:
			ArrangeHorizontal(arrangeSize);
			break;
		case Dock.Left:
		case Dock.Right:
			ArrangeVertical(arrangeSize);
			break;
		}
		return arrangeSize;
	}

	/// <summary>Used to override default clipping.</summary>
	/// <returns>A size that is the layout size of the <see cref="T:System.Windows.Controls.Primitives.TabPanel" />.</returns>
	/// <param name="layoutSlotSize">The size of the panel.</param>
	protected override Geometry GetLayoutClip(Size layoutSlotSize)
	{
		return null;
	}

	private Size GetDesiredSizeWithoutMargin(UIElement element)
	{
		Thickness thickness = (Thickness)element.GetValue(FrameworkElement.MarginProperty);
		Size result = default(Size);
		result.Height = Math.Max(0.0, element.DesiredSize.Height - thickness.Top - thickness.Bottom);
		result.Width = Math.Max(0.0, element.DesiredSize.Width - thickness.Left - thickness.Right);
		return result;
	}

	private double[] GetHeadersSize()
	{
		double[] array = new double[_numHeaders];
		int num = 0;
		foreach (UIElement internalChild in base.InternalChildren)
		{
			if (internalChild.Visibility != Visibility.Collapsed)
			{
				array[num] = GetDesiredSizeWithoutMargin(internalChild).Width;
				num++;
			}
		}
		return array;
	}

	private void ArrangeHorizontal(Size arrangeSize)
	{
		Dock tabStripPlacement = TabStripPlacement;
		bool flag = _numRows > 1;
		int num = 0;
		int[] array = Array.Empty<int>();
		Vector vector = default(Vector);
		double[] headersSize = GetHeadersSize();
		if (flag)
		{
			array = CalculateHeaderDistribution(arrangeSize.Width, headersSize);
			num = GetActiveRow(array);
			if (tabStripPlacement == Dock.Top)
			{
				vector.Y = (double)(_numRows - 1 - num) * _rowHeight;
			}
			if (tabStripPlacement == Dock.Bottom && num != 0)
			{
				vector.Y = (double)(_numRows - num) * _rowHeight;
			}
		}
		int num2 = 0;
		int num3 = 0;
		foreach (UIElement internalChild in base.InternalChildren)
		{
			if (internalChild.Visibility == Visibility.Collapsed)
			{
				continue;
			}
			Thickness thickness = (Thickness)internalChild.GetValue(FrameworkElement.MarginProperty);
			double left = thickness.Left;
			double right = thickness.Right;
			double top = thickness.Top;
			double bottom = thickness.Bottom;
			bool num4 = flag && ((num3 < array.Length && array[num3] == num2) || num2 == _numHeaders - 1);
			Size size = new Size(headersSize[num2], _rowHeight);
			if (num4)
			{
				size.Width = arrangeSize.Width - vector.X;
			}
			internalChild.Arrange(new Rect(vector.X, vector.Y, size.Width, size.Height));
			Size size2 = size;
			size2.Height = Math.Max(0.0, size2.Height - top - bottom);
			size2.Width = Math.Max(0.0, size2.Width - left - right);
			vector.X += size.Width;
			if (num4)
			{
				if ((num3 == num && tabStripPlacement == Dock.Top) || (num3 == num - 1 && tabStripPlacement == Dock.Bottom))
				{
					vector.Y = 0.0;
				}
				else
				{
					vector.Y += _rowHeight;
				}
				vector.X = 0.0;
				num3++;
			}
			num2++;
		}
	}

	private void ArrangeVertical(Size arrangeSize)
	{
		double num = 0.0;
		foreach (UIElement internalChild in base.InternalChildren)
		{
			if (internalChild.Visibility != Visibility.Collapsed)
			{
				Size desiredSizeWithoutMargin = GetDesiredSizeWithoutMargin(internalChild);
				internalChild.Arrange(new Rect(0.0, num, arrangeSize.Width, desiredSizeWithoutMargin.Height));
				num += desiredSizeWithoutMargin.Height;
			}
		}
	}

	private int GetActiveRow(int[] solution)
	{
		int num = 0;
		int num2 = 0;
		if (solution.Length != 0)
		{
			foreach (UIElement internalChild in base.InternalChildren)
			{
				if (internalChild.Visibility != Visibility.Collapsed)
				{
					if ((bool)internalChild.GetValue(Selector.IsSelectedProperty))
					{
						return num;
					}
					if (num < solution.Length && solution[num] == num2)
					{
						num++;
					}
					num2++;
				}
			}
		}
		if (TabStripPlacement == Dock.Top)
		{
			num = _numRows - 1;
		}
		return num;
	}

	private int[] CalculateHeaderDistribution(double rowWidthLimit, double[] headerWidth)
	{
		double num = 0.0;
		int num2 = headerWidth.Length;
		int num3 = _numRows - 1;
		double num4 = 0.0;
		int num5 = 0;
		double num6 = 0.0;
		int[] array = new int[num3];
		int[] array2 = new int[num3];
		int[] array3 = new int[_numRows];
		double[] array4 = new double[_numRows];
		double[] array5 = new double[_numRows];
		double[] array6 = new double[_numRows];
		int num7 = 0;
		for (int i = 0; i < num2; i++)
		{
			if (num4 + headerWidth[i] > rowWidthLimit && num5 > 0)
			{
				array4[num7] = num4;
				array3[num7] = num5;
				num6 = (array5[num7] = Math.Max(0.0, (rowWidthLimit - num4) / (double)num5));
				array[num7] = i - 1;
				if (num < num6)
				{
					num = num6;
				}
				num7++;
				num4 = headerWidth[i];
				num5 = 1;
			}
			else
			{
				num4 += headerWidth[i];
				if (headerWidth[i] != 0.0)
				{
					num5++;
				}
			}
		}
		if (num7 == 0)
		{
			return Array.Empty<int>();
		}
		array4[num7] = num4;
		array3[num7] = num5;
		num6 = (array5[num7] = (rowWidthLimit - num4) / (double)num5);
		if (num < num6)
		{
			num = num6;
		}
		array.CopyTo(array2, 0);
		array5.CopyTo(array6, 0);
		while (true)
		{
			int num8 = 0;
			double num9 = 0.0;
			for (int j = 0; j < _numRows; j++)
			{
				if (num9 < array5[j])
				{
					num9 = array5[j];
					num8 = j;
				}
			}
			if (num8 == 0)
			{
				break;
			}
			int num10 = num8;
			int num11 = num10 - 1;
			int num12 = array[num11];
			double num13 = headerWidth[num12];
			array4[num10] += num13;
			if (array4[num10] > rowWidthLimit)
			{
				break;
			}
			array[num11]--;
			array3[num10]++;
			array4[num11] -= num13;
			array3[num11]--;
			array5[num11] = (rowWidthLimit - array4[num11]) / (double)array3[num11];
			array5[num10] = (rowWidthLimit - array4[num10]) / (double)array3[num10];
			num9 = 0.0;
			for (int k = 0; k < _numRows; k++)
			{
				if (num9 < array5[k])
				{
					num9 = array5[k];
				}
			}
			if (num9 < num)
			{
				num = num9;
				array.CopyTo(array2, 0);
				array5.CopyTo(array6, 0);
			}
		}
		num7 = 0;
		for (int l = 0; l < num2; l++)
		{
			headerWidth[l] += array6[num7];
			if (num7 < num3 && array2[num7] == l)
			{
				num7++;
			}
		}
		return array2;
	}
}
