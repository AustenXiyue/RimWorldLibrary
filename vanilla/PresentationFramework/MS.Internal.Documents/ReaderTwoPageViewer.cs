using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using MS.Internal.KnownBoxes;

namespace MS.Internal.Documents;

internal class ReaderTwoPageViewer : ReaderPageViewer
{
	protected override void OnPreviousPageCommand()
	{
		GoToPage(Math.Max(1, MasterPageNumber - 2));
	}

	protected override void OnNextPageCommand()
	{
		GoToPage(Math.Min(base.PageCount, MasterPageNumber + 2));
	}

	protected override void OnLastPageCommand()
	{
		GoToPage(base.PageCount);
	}

	protected override void OnGoToPageCommand(int pageNumber)
	{
		base.OnGoToPageCommand((pageNumber - 1) / 2 * 2 + 1);
	}

	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.Property == DocumentViewerBase.MasterPageNumberProperty)
		{
			int num = (int)e.NewValue;
			num = (num - 1) / 2 * 2 + 1;
			if (num != (int)e.NewValue)
			{
				GoToPage(num);
			}
		}
	}

	static ReaderTwoPageViewer()
	{
		DocumentViewerBase.CanGoToNextPagePropertyKey.OverrideMetadata(typeof(ReaderTwoPageViewer), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, null, CoerceCanGoToNextPage));
	}

	private static object CoerceCanGoToNextPage(DependencyObject d, object value)
	{
		Invariant.Assert(d != null && d is ReaderTwoPageViewer);
		ReaderTwoPageViewer readerTwoPageViewer = (ReaderTwoPageViewer)d;
		return readerTwoPageViewer.MasterPageNumber < readerTwoPageViewer.PageCount - 1;
	}
}
