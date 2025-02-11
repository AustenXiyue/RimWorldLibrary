using System.Collections;

namespace System.Windows.Documents;

internal class RowFormat
{
	private CellFormat _rowCellFormat;

	private CellWidth _widthA;

	private CellWidth _widthB;

	private CellWidth _widthRow;

	private ArrayList _cellFormats;

	private long _nTrgaph;

	private long _nTrleft;

	private DirState _dir;

	internal CellFormat RowCellFormat => _rowCellFormat;

	internal int CellCount => _cellFormats.Count;

	internal CellFormat TopCellFormat
	{
		get
		{
			if (CellCount <= 0)
			{
				return null;
			}
			return NthCellFormat(CellCount - 1);
		}
	}

	internal CellWidth WidthA => _widthA;

	internal CellWidth WidthB => _widthB;

	internal CellWidth WidthRow => _widthRow;

	internal long Trgaph
	{
		get
		{
			return _nTrgaph;
		}
		set
		{
			_nTrgaph = value;
		}
	}

	internal long Trleft
	{
		get
		{
			return _nTrleft;
		}
		set
		{
			_nTrleft = value;
		}
	}

	internal DirState Dir
	{
		get
		{
			return _dir;
		}
		set
		{
			_dir = value;
		}
	}

	internal bool IsVMerge
	{
		get
		{
			for (int i = 0; i < CellCount; i++)
			{
				if (NthCellFormat(i).IsVMerge)
				{
					return true;
				}
			}
			return false;
		}
	}

	internal RowFormat()
	{
		_rowCellFormat = new CellFormat();
		_widthA = new CellWidth();
		_widthB = new CellWidth();
		_widthRow = new CellWidth();
		_cellFormats = new ArrayList();
		_dir = DirState.DirLTR;
		_nTrgaph = -1L;
		_nTrleft = 0L;
	}

	internal RowFormat(RowFormat ri)
	{
		_rowCellFormat = new CellFormat(ri.RowCellFormat);
		_cellFormats = new ArrayList();
		_widthA = new CellWidth(ri.WidthA);
		_widthB = new CellWidth(ri.WidthB);
		_widthRow = new CellWidth(ri.WidthRow);
		_nTrgaph = ri.Trgaph;
		_dir = ri.Dir;
		_nTrleft = ri._nTrleft;
		for (int i = 0; i < ri.CellCount; i++)
		{
			_cellFormats.Add(new CellFormat(ri.NthCellFormat(i)));
		}
	}

	internal CellFormat NthCellFormat(int n)
	{
		if (n < 0 || n >= CellCount)
		{
			return RowCellFormat;
		}
		return (CellFormat)_cellFormats[n];
	}

	internal CellFormat NextCellFormat()
	{
		_cellFormats.Add(new CellFormat(RowCellFormat));
		return TopCellFormat;
	}

	internal CellFormat CurrentCellFormat()
	{
		if (CellCount == 0 || !TopCellFormat.IsPending)
		{
			return NextCellFormat();
		}
		return TopCellFormat;
	}

	internal void CanonicalizeWidthsFromRTF()
	{
		if (CellCount == 0)
		{
			return;
		}
		CellFormat cellFormat = null;
		long num = Trleft;
		for (int i = 0; i < CellCount; i++)
		{
			CellFormat cellFormat2 = NthCellFormat(i);
			if (cellFormat2.IsHMerge)
			{
				continue;
			}
			if (cellFormat2.IsHMergeFirst)
			{
				for (int j = i + 1; j < CellCount; j++)
				{
					CellFormat cellFormat3 = NthCellFormat(j);
					if (!cellFormat3.IsHMerge)
					{
						break;
					}
					cellFormat2.CellX = cellFormat3.CellX;
				}
			}
			if (cellFormat2.Width.Value == 0L && cellFormat2.IsCellXSet)
			{
				cellFormat2.Width.Type = WidthType.WidthTwips;
				cellFormat2.Width.Value = ((cellFormat == null) ? (cellFormat2.CellX - Trleft) : (cellFormat2.CellX - cellFormat.CellX));
			}
			else if (cellFormat2.Width.Value > 0 && !cellFormat2.IsCellXSet)
			{
				num = (cellFormat2.CellX = num + cellFormat2.Width.Value);
			}
			cellFormat = cellFormat2;
		}
		num = NthCellFormat(0).CellX;
		for (int k = 1; k < CellCount; k++)
		{
			CellFormat cellFormat4 = NthCellFormat(k);
			if (cellFormat4.CellX < num)
			{
				cellFormat4.CellX = num + 1;
			}
			num = cellFormat4.CellX;
		}
	}

	internal void CanonicalizeWidthsFromXaml()
	{
		long num = Trleft;
		for (int i = 0; i < CellCount; i++)
		{
			CellFormat cellFormat = NthCellFormat(i);
			num = (cellFormat.CellX = ((cellFormat.Width.Type != WidthType.WidthTwips) ? (num + 1440) : (num + cellFormat.Width.Value)));
		}
	}
}
