using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;

namespace MS.Internal.Documents;

internal interface IDocumentScrollInfo : IScrollInfo
{
	DynamicDocumentPaginator Content { get; set; }

	int PageCount { get; }

	int FirstVisiblePageNumber { get; }

	double Scale { get; }

	int MaxPagesAcross { get; }

	double VerticalPageSpacing { get; set; }

	double HorizontalPageSpacing { get; set; }

	bool ShowPageBorders { get; set; }

	bool LockViewModes { get; set; }

	ITextView TextView { get; }

	ITextContainer TextContainer { get; }

	ReadOnlyCollection<DocumentPageView> PageViews { get; }

	DocumentViewer DocumentViewerOwner { get; set; }

	void MakePageVisible(int pageNumber);

	void MakeSelectionVisible();

	Rect MakeVisible(object o, Rect r, int pageNumber);

	void ScrollToNextRow();

	void ScrollToPreviousRow();

	void ScrollToHome();

	void ScrollToEnd();

	void SetScale(double scale);

	void SetColumns(int columns);

	void FitColumns(int columns);

	void FitToPageWidth();

	void FitToPageHeight();

	void ViewThumbnails();
}
