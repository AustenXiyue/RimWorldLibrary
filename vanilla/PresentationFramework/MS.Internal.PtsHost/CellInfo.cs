using System.Windows;
using System.Windows.Documents;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal class CellInfo
{
	private Rect _rectCell;

	private Rect _rectTable;

	private TableCell _cell;

	private double[] _columnWidths;

	private double _autofitWidth;

	internal TableCell Cell => _cell;

	internal double[] TableColumnWidths => _columnWidths;

	internal double TableAutofitWidth => _autofitWidth;

	internal Rect TableArea => _rectTable;

	internal Rect CellArea => _rectCell;

	internal CellInfo(TableParaClient tpc, CellParaClient cpc)
	{
		_rectTable = new Rect(TextDpi.FromTextDpi(tpc.Rect.u), TextDpi.FromTextDpi(tpc.Rect.v), TextDpi.FromTextDpi(tpc.Rect.du), TextDpi.FromTextDpi(tpc.Rect.dv));
		_rectCell = new Rect(TextDpi.FromTextDpi(cpc.Rect.u), TextDpi.FromTextDpi(cpc.Rect.v), TextDpi.FromTextDpi(cpc.Rect.du), TextDpi.FromTextDpi(cpc.Rect.dv));
		_autofitWidth = tpc.AutofitWidth;
		_columnWidths = new double[tpc.CalculatedColumns.Length];
		for (int i = 0; i < tpc.CalculatedColumns.Length; i++)
		{
			_columnWidths[i] = tpc.CalculatedColumns[i].DurWidth;
		}
		_cell = cpc.Cell;
	}

	internal void Adjust(Point ptAdjust)
	{
		_rectTable.X += ptAdjust.X;
		_rectTable.Y += ptAdjust.Y;
		_rectCell.X += ptAdjust.X;
		_rectCell.Y += ptAdjust.Y;
	}
}
