using System.Windows;
using System.Windows.Xps;

namespace MS.Internal.Documents;

internal class FlowDocumentPrintingState
{
	internal XpsDocumentWriter XpsDocumentWriter;

	internal Size PageSize;

	internal Thickness PagePadding;

	internal double ColumnWidth;

	internal bool IsSelectionEnabled;
}
