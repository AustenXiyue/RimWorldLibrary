using System;
using System.Windows.Documents;

namespace MS.Internal.Documents;

internal interface IFlowDocumentViewer
{
	ContentPosition ContentPosition { get; set; }

	ITextSelection TextSelection { get; set; }

	bool CanGoToPreviousPage { get; }

	bool CanGoToNextPage { get; }

	int PageNumber { get; }

	int PageCount { get; }

	event EventHandler PageNumberChanged;

	event EventHandler PageCountChanged;

	event EventHandler PrintStarted;

	event EventHandler PrintCompleted;

	void PreviousPage();

	void NextPage();

	void FirstPage();

	void LastPage();

	void Print();

	void CancelPrint();

	void ShowFindResult(ITextRange findResult);

	bool CanGoToPage(int pageNumber);

	void GoToPage(int pageNumber);

	void SetDocument(FlowDocument document);
}
