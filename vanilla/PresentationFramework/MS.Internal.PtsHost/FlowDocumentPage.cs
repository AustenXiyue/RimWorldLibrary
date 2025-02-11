using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.Documents;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal sealed class FlowDocumentPage : DocumentPage, IServiceProvider, IDisposable, IContentHost
{
	private PtsPage _ptsPage;

	private StructuralCache _structuralCache;

	private int _formattedLinesCount;

	private TextDocumentView _textView;

	private Size _partitionSize;

	private Thickness _pageMargin;

	private int _disposed;

	private TextPointer _DependentMax;

	private bool _visualNeedsUpdate;

	private double _lastFormatWidth;

	public override Visual Visual
	{
		get
		{
			if (IsDisposed)
			{
				return null;
			}
			UpdateVisual();
			return base.Visual;
		}
	}

	internal IEnumerator<IInputElement> HostedElementsCore
	{
		get
		{
			if (IsLayoutDataValid)
			{
				_textView = GetTextView();
				Invariant.Assert(_textView != null && ((ITextView)_textView).TextSegments.Count > 0);
				return new HostedElements(((ITextView)_textView).TextSegments);
			}
			return new HostedElements(new ReadOnlyCollection<TextSegment>(new List<TextSegment>(0)));
		}
	}

	internal ReadOnlyCollection<ParagraphResult> FloatingElementResults
	{
		get
		{
			List<ParagraphResult> list = new List<ParagraphResult>(0);
			List<BaseParaClient> floatingElementList = _ptsPage.PageContext.FloatingElementList;
			if (floatingElementList != null)
			{
				for (int i = 0; i < floatingElementList.Count; i++)
				{
					ParagraphResult item = floatingElementList[i].CreateParagraphResult();
					list.Add(item);
				}
			}
			return new ReadOnlyCollection<ParagraphResult>(list);
		}
	}

	internal bool UseSizingWorkaroundForTextBox
	{
		get
		{
			return _ptsPage.UseSizingWorkaroundForTextBox;
		}
		set
		{
			_ptsPage.UseSizingWorkaroundForTextBox = value;
		}
	}

	internal Thickness Margin => _pageMargin;

	internal bool IsDisposed
	{
		get
		{
			if (_disposed == 0)
			{
				return _structuralCache.PtsContext.Disposed;
			}
			return true;
		}
	}

	internal Size ContentSize
	{
		get
		{
			Size contentSize = _ptsPage.ContentSize;
			contentSize.Width += _pageMargin.Left + _pageMargin.Right;
			contentSize.Height += _pageMargin.Top + _pageMargin.Bottom;
			return contentSize;
		}
	}

	internal bool FinitePage => _ptsPage.FinitePage;

	internal PageContext PageContext => _ptsPage.PageContext;

	internal bool IncrementalUpdate => _ptsPage.IncrementalUpdate;

	internal StructuralCache StructuralCache => _structuralCache;

	internal int FormattedLinesCount => _formattedLinesCount;

	internal bool IsLayoutDataValid
	{
		get
		{
			bool result = false;
			if (!IsDisposed)
			{
				result = _structuralCache.FormattingOwner.IsLayoutDataValid;
			}
			return result;
		}
	}

	internal TextPointer DependentMax
	{
		get
		{
			return _DependentMax;
		}
		set
		{
			if (_DependentMax == null || (value != null && value.CompareTo(_DependentMax) > 0))
			{
				_DependentMax = value;
			}
		}
	}

	internal Rect Viewport => new Rect(Size);

	private PageVisual PageVisual => base.Visual as PageVisual;

	IEnumerator<IInputElement> IContentHost.HostedElements => HostedElementsCore;

	internal FlowDocumentPage(StructuralCache structuralCache)
		: base(null)
	{
		_structuralCache = structuralCache;
		_ptsPage = new PtsPage(structuralCache.Section);
	}

	~FlowDocumentPage()
	{
		Dispose(disposing: false);
	}

	public override void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
		base.Dispose();
	}

	internal void FormatBottomless(Size pageSize, Thickness pageMargin)
	{
		Invariant.Assert(!IsDisposed);
		_formattedLinesCount = 0;
		TextDpi.EnsureValidPageSize(ref pageSize);
		_pageMargin = pageMargin;
		SetSize(pageSize);
		if (!DoubleUtil.AreClose(_lastFormatWidth, pageSize.Width) || !DoubleUtil.AreClose(_pageMargin.Left, pageMargin.Left) || !DoubleUtil.AreClose(_pageMargin.Right, pageMargin.Right))
		{
			_structuralCache.InvalidateFormatCache(destroyStructure: false);
		}
		_lastFormatWidth = pageSize.Width;
		using (_structuralCache.SetDocumentFormatContext(this))
		{
			OnBeforeFormatPage();
			if (_ptsPage.PrepareForBottomlessUpdate())
			{
				_structuralCache.CurrentFormatContext.PushNewPageData(pageSize, _pageMargin, incrementalUpdate: true, finitePage: false);
				_ptsPage.UpdateBottomlessPage();
			}
			else
			{
				_structuralCache.CurrentFormatContext.PushNewPageData(pageSize, _pageMargin, incrementalUpdate: false, finitePage: false);
				_ptsPage.CreateBottomlessPage();
			}
			pageSize = _ptsPage.CalculatedSize;
			pageSize.Width += pageMargin.Left + pageMargin.Right;
			pageSize.Height += pageMargin.Top + pageMargin.Bottom;
			SetSize(pageSize);
			SetContentBox(new Rect(pageMargin.Left, pageMargin.Top, _ptsPage.CalculatedSize.Width, _ptsPage.CalculatedSize.Height));
			_structuralCache.CurrentFormatContext.PopPageData();
			OnAfterFormatPage();
			_structuralCache.DetectInvalidOperation();
		}
	}

	internal PageBreakRecord FormatFinite(Size pageSize, Thickness pageMargin, PageBreakRecord breakRecord)
	{
		Invariant.Assert(!IsDisposed);
		_formattedLinesCount = 0;
		TextDpi.EnsureValidPageSize(ref pageSize);
		TextDpi.EnsureValidPageMargin(ref pageMargin, pageSize);
		double num = PtsHelper.CalculatePageMarginAdjustment(_structuralCache, pageSize.Width - (pageMargin.Left + pageMargin.Right));
		if (!DoubleUtil.IsZero(num))
		{
			pageMargin.Right += num - num / 100.0;
		}
		_pageMargin = pageMargin;
		SetSize(pageSize);
		SetContentBox(new Rect(pageMargin.Left, pageMargin.Top, pageSize.Width - (pageMargin.Left + pageMargin.Right), pageSize.Height - (pageMargin.Top + pageMargin.Bottom)));
		using (_structuralCache.SetDocumentFormatContext(this))
		{
			OnBeforeFormatPage();
			if (_ptsPage.PrepareForFiniteUpdate(breakRecord))
			{
				_structuralCache.CurrentFormatContext.PushNewPageData(pageSize, _pageMargin, incrementalUpdate: true, finitePage: true);
				_ptsPage.UpdateFinitePage(breakRecord);
			}
			else
			{
				_structuralCache.CurrentFormatContext.PushNewPageData(pageSize, _pageMargin, incrementalUpdate: false, finitePage: true);
				_ptsPage.CreateFinitePage(breakRecord);
			}
			_structuralCache.CurrentFormatContext.PopPageData();
			OnAfterFormatPage();
			_structuralCache.DetectInvalidOperation();
		}
		return _ptsPage.BreakRecord;
	}

	internal void Arrange(Size partitionSize)
	{
		Invariant.Assert(!IsDisposed);
		_partitionSize = partitionSize;
		using (_structuralCache.SetDocumentArrangeContext(this))
		{
			_ptsPage.ArrangePage();
			_structuralCache.DetectInvalidOperation();
		}
		ValidateTextView();
	}

	internal void ForceReformat()
	{
		Invariant.Assert(!IsDisposed);
		_ptsPage.ClearUpdateInfo();
		_structuralCache.ForceReformat = true;
	}

	internal IInputElement InputHitTestCore(Point point)
	{
		Invariant.Assert(!IsDisposed);
		if (FrameworkElement.GetFrameworkParent(_structuralCache.FormattingOwner) == null)
		{
			return null;
		}
		IInputElement inputElement = null;
		if (IsLayoutDataValid)
		{
			GeneralTransform generalTransform = PageVisual.Child.TransformToAncestor(PageVisual);
			generalTransform = generalTransform.Inverse;
			if (generalTransform != null)
			{
				point = generalTransform.Transform(point);
				inputElement = _ptsPage.InputHitTest(point);
			}
		}
		if (inputElement == null)
		{
			return _structuralCache.FormattingOwner;
		}
		return inputElement;
	}

	internal ReadOnlyCollection<Rect> GetRectanglesCore(ContentElement child, bool isLimitedToTextView)
	{
		Invariant.Assert(!IsDisposed);
		List<Rect> list = new List<Rect>();
		if (IsLayoutDataValid)
		{
			TextPointer textPointer = FindElementPosition(child, isLimitedToTextView);
			if (textPointer != null)
			{
				int offsetToPosition = _structuralCache.TextContainer.Start.GetOffsetToPosition(textPointer);
				int length = 1;
				if (child is TextElement)
				{
					TextPointer position = new TextPointer(((TextElement)child).ElementEnd);
					length = textPointer.GetOffsetToPosition(position);
				}
				list = _ptsPage.GetRectangles(child, offsetToPosition, length);
			}
		}
		if (PageVisual != null && list.Count > 0)
		{
			List<Rect> list2 = new List<Rect>(list.Count);
			GeneralTransform generalTransform = PageVisual.Child.TransformToAncestor(PageVisual);
			for (int i = 0; i < list.Count; i++)
			{
				list2.Add(generalTransform.TransformBounds(list[i]));
			}
			list = list2;
		}
		Invariant.Assert(list != null);
		return new ReadOnlyCollection<Rect>(list);
	}

	internal void OnChildDesiredSizeChangedCore(UIElement child)
	{
		_structuralCache.FormattingOwner.OnChildDesiredSizeChanged(child);
	}

	internal ReadOnlyCollection<ColumnResult> GetColumnResults(out bool hasTextContent)
	{
		Invariant.Assert(!IsDisposed);
		List<ColumnResult> list = new List<ColumnResult>(0);
		hasTextContent = false;
		if (_ptsPage.PageHandle != IntPtr.Zero)
		{
			PTS.Validate(PTS.FsQueryPageDetails(StructuralCache.PtsContext.Context, _ptsPage.PageHandle, out var pPageDetails));
			if (PTS.ToBoolean(pPageDetails.fSimple))
			{
				PTS.Validate(PTS.FsQueryTrackDetails(StructuralCache.PtsContext.Context, pPageDetails.u.simple.trackdescr.pfstrack, out var pTrackDetails));
				if (pTrackDetails.cParas > 0)
				{
					list = new List<ColumnResult>(1);
					ColumnResult columnResult = new ColumnResult(this, ref pPageDetails.u.simple.trackdescr, default(Vector));
					list.Add(columnResult);
					if (columnResult.HasTextContent)
					{
						hasTextContent = true;
					}
				}
			}
			else if (pPageDetails.u.complex.cSections > 0)
			{
				PtsHelper.SectionListFromPage(StructuralCache.PtsContext, _ptsPage.PageHandle, ref pPageDetails, out var arraySectionDesc);
				PTS.Validate(PTS.FsQuerySectionDetails(StructuralCache.PtsContext.Context, arraySectionDesc[0].pfssection, out var pSectionDetails));
				if (PTS.ToBoolean(pSectionDetails.fFootnotesAsPagenotes) && pSectionDetails.u.withpagenotes.cBasicColumns > 0)
				{
					PtsHelper.TrackListFromSection(StructuralCache.PtsContext, arraySectionDesc[0].pfssection, ref pSectionDetails, out var arrayTrackDesc);
					list = new List<ColumnResult>(pSectionDetails.u.withpagenotes.cBasicColumns);
					for (int i = 0; i < arrayTrackDesc.Length; i++)
					{
						PTS.FSTRACKDESCRIPTION trackDesc = arrayTrackDesc[i];
						if (trackDesc.pfstrack == IntPtr.Zero)
						{
							continue;
						}
						PTS.Validate(PTS.FsQueryTrackDetails(StructuralCache.PtsContext.Context, trackDesc.pfstrack, out var pTrackDetails2));
						if (pTrackDetails2.cParas > 0)
						{
							ColumnResult columnResult2 = new ColumnResult(this, ref trackDesc, default(Vector));
							list.Add(columnResult2);
							if (columnResult2.HasTextContent)
							{
								hasTextContent = true;
							}
						}
					}
				}
			}
		}
		Invariant.Assert(list != null);
		return new ReadOnlyCollection<ColumnResult>(list);
	}

	internal TextContentRange GetTextContentRangeFromColumn(nint pfstrack)
	{
		Invariant.Assert(!IsDisposed);
		PTS.Validate(PTS.FsQueryTrackDetails(StructuralCache.PtsContext.Context, pfstrack, out var pTrackDetails));
		TextContentRange textContentRange = new TextContentRange();
		if (pTrackDetails.cParas != 0)
		{
			PtsHelper.ParaListFromTrack(StructuralCache.PtsContext, pfstrack, ref pTrackDetails, out var arrayParaDesc);
			for (int i = 0; i < arrayParaDesc.Length; i++)
			{
				BaseParaClient baseParaClient = StructuralCache.PtsContext.HandleToObject(arrayParaDesc[i].pfsparaclient) as BaseParaClient;
				PTS.ValidateHandle(baseParaClient);
				textContentRange.Merge(baseParaClient.GetTextContentRange());
			}
		}
		return textContentRange;
	}

	internal ReadOnlyCollection<ParagraphResult> GetParagraphResultsFromColumn(nint pfstrack, Vector parentOffset, out bool hasTextContent)
	{
		Invariant.Assert(!IsDisposed);
		PTS.Validate(PTS.FsQueryTrackDetails(StructuralCache.PtsContext.Context, pfstrack, out var pTrackDetails));
		hasTextContent = false;
		if (pTrackDetails.cParas == 0)
		{
			return new ReadOnlyCollection<ParagraphResult>(new List<ParagraphResult>(0));
		}
		PtsHelper.ParaListFromTrack(StructuralCache.PtsContext, pfstrack, ref pTrackDetails, out var arrayParaDesc);
		List<ParagraphResult> list = new List<ParagraphResult>(arrayParaDesc.Length);
		for (int i = 0; i < arrayParaDesc.Length; i++)
		{
			BaseParaClient obj = StructuralCache.PtsContext.HandleToObject(arrayParaDesc[i].pfsparaclient) as BaseParaClient;
			PTS.ValidateHandle(obj);
			ParagraphResult paragraphResult = obj.CreateParagraphResult();
			if (paragraphResult.HasTextContent)
			{
				hasTextContent = true;
			}
			list.Add(paragraphResult);
		}
		return new ReadOnlyCollection<ParagraphResult>(list);
	}

	internal void OnFormatLine()
	{
		Invariant.Assert(!IsDisposed);
		_formattedLinesCount++;
	}

	internal void EnsureValidVisuals()
	{
		Invariant.Assert(!IsDisposed);
		UpdateVisual();
	}

	internal void UpdateViewport(ref PTS.FSRECT viewport, bool drawBackground)
	{
		GeneralTransform generalTransform = PageVisual.Child.TransformToAncestor(PageVisual);
		generalTransform = generalTransform.Inverse;
		Rect rect = viewport.FromTextDpi();
		if (generalTransform != null)
		{
			rect = generalTransform.TransformBounds(rect);
		}
		if (!IsDisposed)
		{
			if (drawBackground)
			{
				PageVisual.DrawBackground((Brush)_structuralCache.PropertyOwner.GetValue(FlowDocument.BackgroundProperty), rect);
			}
			using (_structuralCache.SetDocumentVisualValidationContext(this))
			{
				PTS.FSRECT viewport2 = new PTS.FSRECT(rect);
				_ptsPage.UpdateViewport(ref viewport2);
				_structuralCache.DetectInvalidOperation();
			}
			ValidateTextView();
		}
	}

	private void Dispose(bool disposing)
	{
		if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
		{
			return;
		}
		if (disposing)
		{
			if (PageVisual != null)
			{
				DestroyVisualLinks(PageVisual);
				PageVisual.Children.Clear();
				PageVisual.ClearDrawingContext();
			}
			if (_ptsPage != null)
			{
				_ptsPage.Dispose();
			}
		}
		try
		{
			if (disposing)
			{
				OnPageDestroyed(EventArgs.Empty);
			}
		}
		finally
		{
			_ptsPage = null;
			_structuralCache = null;
			_textView = null;
			_DependentMax = null;
		}
	}

	private void UpdateVisual()
	{
		if (PageVisual == null)
		{
			SetVisual(new PageVisual(this));
		}
		if (_visualNeedsUpdate)
		{
			PageVisual.DrawBackground((Brush)_structuralCache.PropertyOwner.GetValue(FlowDocument.BackgroundProperty), new Rect(_partitionSize));
			ContainerVisual containerVisual = null;
			using (_structuralCache.SetDocumentVisualValidationContext(this))
			{
				containerVisual = _ptsPage.GetPageVisual();
				_structuralCache.DetectInvalidOperation();
			}
			PageVisual.Child = containerVisual;
			FlowDirection childFD = (FlowDirection)_structuralCache.PropertyOwner.GetValue(FlowDocument.FlowDirectionProperty);
			PtsHelper.UpdateMirroringTransform(FlowDirection.LeftToRight, childFD, containerVisual, Size.Width);
			using (_structuralCache.SetDocumentVisualValidationContext(this))
			{
				_ptsPage.ClearUpdateInfo();
				_structuralCache.DetectInvalidOperation();
			}
			_visualNeedsUpdate = false;
		}
	}

	private void OnBeforeFormatPage()
	{
		if (_visualNeedsUpdate)
		{
			_ptsPage.ClearUpdateInfo();
		}
	}

	private void OnAfterFormatPage()
	{
		if (_textView != null)
		{
			_textView.Invalidate();
		}
		_visualNeedsUpdate = true;
	}

	private TextPointer FindElementPosition(IInputElement e, bool isLimitedToTextView)
	{
		TextPointer textPointer = null;
		if (e is TextElement)
		{
			if ((e as TextElement).TextContainer == _structuralCache.TextContainer)
			{
				textPointer = new TextPointer((e as TextElement).ElementStart);
			}
		}
		else
		{
			if (_structuralCache.TextContainer.Start == null || _structuralCache.TextContainer.End == null)
			{
				return null;
			}
			TextPointer textPointer2 = new TextPointer(_structuralCache.TextContainer.Start);
			while (textPointer == null && ((ITextPointer)textPointer2).CompareTo((ITextPointer)_structuralCache.TextContainer.End) < 0)
			{
				if (textPointer2.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.EmbeddedElement)
				{
					DependencyObject adjacentElement = textPointer2.GetAdjacentElement(LogicalDirection.Forward);
					if ((adjacentElement is ContentElement || adjacentElement is UIElement) && (adjacentElement == e as ContentElement || adjacentElement == e as UIElement))
					{
						textPointer = new TextPointer(textPointer2);
					}
				}
				textPointer2.MoveToNextContextPosition(LogicalDirection.Forward);
			}
		}
		if (textPointer != null && isLimitedToTextView)
		{
			_textView = GetTextView();
			Invariant.Assert(_textView != null);
			for (int i = 0; i < ((ITextView)_textView).TextSegments.Count; i++)
			{
				if (((ITextPointer)textPointer).CompareTo(((ITextView)_textView).TextSegments[i].Start) >= 0 && ((ITextPointer)textPointer).CompareTo(((ITextView)_textView).TextSegments[i].End) < 0)
				{
					return textPointer;
				}
			}
			textPointer = null;
		}
		return textPointer;
	}

	private void DestroyVisualLinks(ContainerVisual visual)
	{
		VisualCollection children = visual.Children;
		if (children == null)
		{
			return;
		}
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i] is UIElementIsland)
			{
				children.RemoveAt(i);
				continue;
			}
			Invariant.Assert(children[i] is ContainerVisual, "The children should always derive from ContainerVisual");
			DestroyVisualLinks((ContainerVisual)children[i]);
		}
	}

	private void ValidateTextView()
	{
		if (_textView != null)
		{
			_textView.OnUpdated();
		}
	}

	private TextDocumentView GetTextView()
	{
		TextDocumentView obj = (TextDocumentView)((IServiceProvider)this).GetService(typeof(ITextView));
		Invariant.Assert(obj != null);
		return obj;
	}

	object IServiceProvider.GetService(Type serviceType)
	{
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (serviceType == typeof(ITextView))
		{
			if (_textView == null)
			{
				_textView = new TextDocumentView(this, _structuralCache.TextContainer);
			}
			return _textView;
		}
		return null;
	}

	IInputElement IContentHost.InputHitTest(Point point)
	{
		return InputHitTestCore(point);
	}

	ReadOnlyCollection<Rect> IContentHost.GetRectangles(ContentElement child)
	{
		return GetRectanglesCore(child, isLimitedToTextView: true);
	}

	void IContentHost.OnChildDesiredSizeChanged(UIElement child)
	{
		OnChildDesiredSizeChangedCore(child);
	}
}
