namespace System.Windows.Documents;

internal enum DocumentNodeType
{
	dnUnknown,
	dnText,
	dnInline,
	dnLineBreak,
	dnHyperlink,
	dnParagraph,
	dnInlineUIContainer,
	dnBlockUIContainer,
	dnImage,
	dnList,
	dnListItem,
	dnTable,
	dnTableBody,
	dnRow,
	dnCell,
	dnSection,
	dnFigure,
	dnFloater,
	dnFieldBegin,
	dnFieldEnd,
	dnShape,
	dnListText
}
