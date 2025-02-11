namespace System.Windows.Documents;

internal sealed class FixedSOMTable : FixedSOMPageElement
{
	private const double _minColumnWidth = 5.0;

	private const double _minRowHeight = 10.0;

	private int _RTLCount;

	private int _LTRCount;

	private int _numCols;

	public override bool IsRTL => _RTLCount > _LTRCount;

	internal override FixedElement.ElementType[] ElementTypes => new FixedElement.ElementType[2]
	{
		FixedElement.ElementType.Table,
		FixedElement.ElementType.TableRowGroup
	};

	internal bool IsEmpty
	{
		get
		{
			foreach (FixedSOMTableRow semanticBox in base.SemanticBoxes)
			{
				if (!semanticBox.IsEmpty)
				{
					return false;
				}
			}
			return true;
		}
	}

	internal bool IsSingleCelled
	{
		get
		{
			if (base.SemanticBoxes.Count == 1)
			{
				return (base.SemanticBoxes[0] as FixedSOMTableRow).SemanticBoxes.Count == 1;
			}
			return false;
		}
	}

	public FixedSOMTable(FixedSOMPage page)
		: base(page)
	{
		_numCols = 0;
	}

	public void AddRow(FixedSOMTableRow row)
	{
		Add(row);
		int count = row.SemanticBoxes.Count;
		if (count > _numCols)
		{
			_numCols = count;
		}
	}

	public bool AddContainer(FixedSOMContainer container)
	{
		Rect boundingRect = container.BoundingRect;
		double num = boundingRect.Height * 0.2;
		double num2 = boundingRect.Width * 0.2;
		boundingRect.Inflate(0.0 - num2, 0.0 - num);
		if (base.BoundingRect.Contains(boundingRect))
		{
			foreach (FixedSOMTableRow semanticBox in base.SemanticBoxes)
			{
				if (!semanticBox.BoundingRect.Contains(boundingRect))
				{
					continue;
				}
				foreach (FixedSOMTableCell semanticBox2 in semanticBox.SemanticBoxes)
				{
					if (!semanticBox2.BoundingRect.Contains(boundingRect))
					{
						continue;
					}
					semanticBox2.AddContainer(container);
					if (container is FixedSOMFixedBlock fixedSOMFixedBlock)
					{
						if (fixedSOMFixedBlock.IsRTL)
						{
							_RTLCount++;
						}
						else
						{
							_LTRCount++;
						}
					}
					return true;
				}
			}
		}
		return false;
	}

	public override void SetRTFProperties(FixedElement element)
	{
		if (element.Type == typeof(Table))
		{
			element.SetValue(Table.CellSpacingProperty, 0.0);
		}
	}

	internal void DeleteEmptyRows()
	{
		int num = 0;
		while (num < base.SemanticBoxes.Count)
		{
			if (base.SemanticBoxes[num] is FixedSOMTableRow { IsEmpty: not false, BoundingRect: { Height: <10.0 } })
			{
				base.SemanticBoxes.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	internal void DeleteEmptyColumns()
	{
		int count = base.SemanticBoxes.Count;
		int[] array = new int[count];
		while (true)
		{
			double num = double.MaxValue;
			bool flag = true;
			for (int i = 0; i < count; i++)
			{
				FixedSOMTableRow fixedSOMTableRow = (FixedSOMTableRow)base.SemanticBoxes[i];
				int num2 = array[i];
				flag = flag && num2 < fixedSOMTableRow.SemanticBoxes.Count;
				FixedSOMTableCell fixedSOMTableCell;
				if (flag)
				{
					fixedSOMTableCell = (FixedSOMTableCell)fixedSOMTableRow.SemanticBoxes[num2];
					flag = fixedSOMTableCell.IsEmpty && fixedSOMTableCell.BoundingRect.Width < 5.0;
				}
				if (num2 + 1 >= fixedSOMTableRow.SemanticBoxes.Count)
				{
					continue;
				}
				fixedSOMTableCell = (FixedSOMTableCell)fixedSOMTableRow.SemanticBoxes[num2 + 1];
				double left = fixedSOMTableCell.BoundingRect.Left;
				if (left < num)
				{
					if (num != double.MaxValue)
					{
						flag = false;
					}
					num = left;
				}
				else if (left > num)
				{
					flag = false;
				}
			}
			if (flag)
			{
				for (int i = 0; i < count; i++)
				{
					((FixedSOMTableRow)base.SemanticBoxes[i]).SemanticBoxes.RemoveAt(array[i]);
				}
				if (num == double.MaxValue)
				{
					break;
				}
				continue;
			}
			if (num == double.MaxValue)
			{
				break;
			}
			for (int i = 0; i < count; i++)
			{
				FixedSOMTableRow fixedSOMTableRow2 = (FixedSOMTableRow)base.SemanticBoxes[i];
				int num3 = array[i];
				if (num3 + 1 < fixedSOMTableRow2.SemanticBoxes.Count && fixedSOMTableRow2.SemanticBoxes[num3 + 1].BoundingRect.Left == num)
				{
					array[i] = num3 + 1;
				}
				else
				{
					((FixedSOMTableCell)fixedSOMTableRow2.SemanticBoxes[num3]).ColumnSpan++;
				}
			}
		}
	}
}
