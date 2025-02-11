using System.Windows.Documents;
using System.Windows.Media;

namespace MS.Internal.PtsHost;

internal sealed class RowVisual : ContainerVisual
{
	private readonly TableRow _row;

	internal TableRow Row => _row;

	internal RowVisual(TableRow row)
	{
		_row = row;
	}
}
