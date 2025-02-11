namespace System.Windows.Controls.Primitives;

/// <summary>Provides a way to arrange content in a grid where all the cells in the grid have the same size.</summary>
public class UniformGrid : Panel
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.UniformGrid.FirstColumn" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.UniformGrid.FirstColumn" /> dependency property.</returns>
	public static readonly DependencyProperty FirstColumnProperty = DependencyProperty.Register("FirstColumn", typeof(int), typeof(UniformGrid), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure), ValidateFirstColumn);

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.UniformGrid.Columns" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.UniformGrid.Columns" /> dependency property.</returns>
	public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(int), typeof(UniformGrid), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure), ValidateColumns);

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.UniformGrid.Rows" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.UniformGrid.Rows" /> dependency property.</returns>
	public static readonly DependencyProperty RowsProperty = DependencyProperty.Register("Rows", typeof(int), typeof(UniformGrid), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure), ValidateRows);

	private int _rows;

	private int _columns;

	/// <summary>Gets or sets the number of leading blank cells in the first row of the grid.  </summary>
	/// <returns>The number of empty cells that are in the first row of the grid. The default is 0.</returns>
	public int FirstColumn
	{
		get
		{
			return (int)GetValue(FirstColumnProperty);
		}
		set
		{
			SetValue(FirstColumnProperty, value);
		}
	}

	/// <summary>Gets or sets the number of columns that are in the grid.  </summary>
	/// <returns>The number of columns that are in the grid. The default is 0. </returns>
	public int Columns
	{
		get
		{
			return (int)GetValue(ColumnsProperty);
		}
		set
		{
			SetValue(ColumnsProperty, value);
		}
	}

	/// <summary>Gets or sets the number of rows that are in the grid.  </summary>
	/// <returns>The number of rows that are in the grid. The default is 0.</returns>
	public int Rows
	{
		get
		{
			return (int)GetValue(RowsProperty);
		}
		set
		{
			SetValue(RowsProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.UniformGrid" /> class.</summary>
	public UniformGrid()
	{
	}

	private static bool ValidateFirstColumn(object o)
	{
		return (int)o >= 0;
	}

	private static bool ValidateColumns(object o)
	{
		return (int)o >= 0;
	}

	private static bool ValidateRows(object o)
	{
		return (int)o >= 0;
	}

	/// <summary>Computes the desired size of the <see cref="T:System.Windows.Controls.Primitives.UniformGrid" /> by measuring all of the child elements.</summary>
	/// <returns>The desired <see cref="T:System.Windows.Size" /> based on the child content of the grid and the <paramref name="constraint" /> parameter.</returns>
	/// <param name="constraint">The <see cref="T:System.Windows.Size" /> of the available area for the grid.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		UpdateComputedValues();
		Size availableSize = new Size(constraint.Width / (double)_columns, constraint.Height / (double)_rows);
		double num = 0.0;
		double num2 = 0.0;
		int i = 0;
		for (int count = base.InternalChildren.Count; i < count; i++)
		{
			UIElement uIElement = base.InternalChildren[i];
			uIElement.Measure(availableSize);
			Size desiredSize = uIElement.DesiredSize;
			if (num < desiredSize.Width)
			{
				num = desiredSize.Width;
			}
			if (num2 < desiredSize.Height)
			{
				num2 = desiredSize.Height;
			}
		}
		return new Size(num * (double)_columns, num2 * (double)_rows);
	}

	/// <summary>Defines the layout of the <see cref="T:System.Windows.Controls.Primitives.UniformGrid" /> by distributing space evenly among all of the child elements.</summary>
	/// <returns>The actual <see cref="T:System.Windows.Size" /> of the grid that is rendered to display the child elements that are visible.</returns>
	/// <param name="arrangeSize">The <see cref="T:System.Windows.Size" /> of the area for the grid to use.</param>
	protected override Size ArrangeOverride(Size arrangeSize)
	{
		Rect finalRect = new Rect(0.0, 0.0, arrangeSize.Width / (double)_columns, arrangeSize.Height / (double)_rows);
		double width = finalRect.Width;
		double num = arrangeSize.Width - 1.0;
		finalRect.X += finalRect.Width * (double)FirstColumn;
		foreach (UIElement internalChild in base.InternalChildren)
		{
			internalChild.Arrange(finalRect);
			if (internalChild.Visibility != Visibility.Collapsed)
			{
				finalRect.X += width;
				if (finalRect.X >= num)
				{
					finalRect.Y += finalRect.Height;
					finalRect.X = 0.0;
				}
			}
		}
		return arrangeSize;
	}

	private void UpdateComputedValues()
	{
		_columns = Columns;
		_rows = Rows;
		if (FirstColumn >= _columns)
		{
			FirstColumn = 0;
		}
		if (_rows != 0 && _columns != 0)
		{
			return;
		}
		int num = 0;
		int i = 0;
		for (int count = base.InternalChildren.Count; i < count; i++)
		{
			if (base.InternalChildren[i].Visibility != Visibility.Collapsed)
			{
				num++;
			}
		}
		if (num == 0)
		{
			num = 1;
		}
		if (_rows == 0)
		{
			if (_columns > 0)
			{
				_rows = (num + FirstColumn + (_columns - 1)) / _columns;
				return;
			}
			_rows = (int)Math.Sqrt(num);
			if (_rows * _rows < num)
			{
				_rows++;
			}
			_columns = _rows;
		}
		else if (_columns == 0)
		{
			_columns = (num + (_rows - 1)) / _rows;
		}
	}
}
