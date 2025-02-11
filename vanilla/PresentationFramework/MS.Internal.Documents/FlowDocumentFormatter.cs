using System;
using System.Windows;
using System.Windows.Documents;
using MS.Internal.PtsHost;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.Documents;

internal class FlowDocumentFormatter : IFlowDocumentFormatter
{
	private readonly FlowDocument _document;

	private FlowDocumentPage _documentPage;

	private bool _arrangedAfterFormat;

	private bool _lastFormatSuccessful;

	private const double _defaultWidth = 500.0;

	private bool _isContentFormatValid;

	internal FlowDocumentPage DocumentPage => _documentPage;

	bool IFlowDocumentFormatter.IsLayoutDataValid
	{
		get
		{
			if (_documentPage != null && _document.StructuralCache.IsFormattedOnce && !_document.StructuralCache.ForceReformat && _isContentFormatValid && !_document.StructuralCache.IsContentChangeInProgress)
			{
				return !_document.StructuralCache.IsFormattingInProgress;
			}
			return false;
		}
	}

	internal event EventHandler ContentInvalidated;

	internal event EventHandler Suspended;

	internal FlowDocumentFormatter(FlowDocument document)
	{
		_document = document;
		_documentPage = new FlowDocumentPage(_document.StructuralCache);
	}

	internal void Format(Size constraint)
	{
		if (_document.StructuralCache.IsFormattingInProgress)
		{
			throw new InvalidOperationException(SR.FlowDocumentFormattingReentrancy);
		}
		if (_document.StructuralCache.IsContentChangeInProgress)
		{
			throw new InvalidOperationException(SR.TextContainerChangingReentrancyInvalid);
		}
		if (_document.StructuralCache.IsFormattedOnce)
		{
			if (!_lastFormatSuccessful)
			{
				_document.StructuralCache.InvalidateFormatCache(destroyStructure: true);
			}
			if (!_arrangedAfterFormat && (!_document.StructuralCache.ForceReformat || !_document.StructuralCache.DestroyStructure))
			{
				_documentPage.Arrange(_documentPage.ContentSize);
				_documentPage.EnsureValidVisuals();
			}
		}
		_arrangedAfterFormat = false;
		_lastFormatSuccessful = false;
		_isContentFormatValid = false;
		Size pageSize = ComputePageSize(constraint);
		Thickness pageMargin = ComputePageMargin();
		using (_document.Dispatcher.DisableProcessing())
		{
			_document.StructuralCache.IsFormattingInProgress = true;
			try
			{
				_document.StructuralCache.BackgroundFormatInfo.ViewportHeight = constraint.Height;
				_documentPage.FormatBottomless(pageSize, pageMargin);
			}
			finally
			{
				_document.StructuralCache.IsFormattingInProgress = false;
			}
		}
		_lastFormatSuccessful = true;
	}

	internal void Arrange(Size arrangeSize, Rect viewport)
	{
		Invariant.Assert(_document.StructuralCache.DtrList == null || _document.StructuralCache.DtrList.Length == 0 || (_document.StructuralCache.DtrList.Length == 1 && _document.StructuralCache.BackgroundFormatInfo.DoesFinalDTRCoverRestOfText));
		_documentPage.Arrange(arrangeSize);
		_documentPage.EnsureValidVisuals();
		_arrangedAfterFormat = true;
		if (viewport.IsEmpty)
		{
			viewport = new Rect(0.0, 0.0, arrangeSize.Width, _document.StructuralCache.BackgroundFormatInfo.ViewportHeight);
		}
		PTS.FSRECT viewport2 = new PTS.FSRECT(viewport);
		_documentPage.UpdateViewport(ref viewport2, drawBackground: true);
		_isContentFormatValid = true;
	}

	private Size ComputePageSize(Size constraint)
	{
		Size result = new Size(_document.PageWidth, double.PositiveInfinity);
		if (double.IsNaN(result.Width))
		{
			result.Width = constraint.Width;
			double maxPageWidth = _document.MaxPageWidth;
			if (result.Width > maxPageWidth)
			{
				result.Width = maxPageWidth;
			}
			double minPageWidth = _document.MinPageWidth;
			if (result.Width < minPageWidth)
			{
				result.Width = minPageWidth;
			}
		}
		if (double.IsPositiveInfinity(result.Width))
		{
			result.Width = 500.0;
		}
		return result;
	}

	private Thickness ComputePageMargin()
	{
		double lineHeightValue = DynamicPropertyReader.GetLineHeightValue(_document);
		Thickness pagePadding = _document.PagePadding;
		if (double.IsNaN(pagePadding.Left))
		{
			pagePadding.Left = lineHeightValue;
		}
		if (double.IsNaN(pagePadding.Top))
		{
			pagePadding.Top = lineHeightValue;
		}
		if (double.IsNaN(pagePadding.Right))
		{
			pagePadding.Right = lineHeightValue;
		}
		if (double.IsNaN(pagePadding.Bottom))
		{
			pagePadding.Bottom = lineHeightValue;
		}
		return pagePadding;
	}

	void IFlowDocumentFormatter.OnContentInvalidated(bool affectsLayout)
	{
		if (affectsLayout)
		{
			if (!_arrangedAfterFormat)
			{
				_document.StructuralCache.InvalidateFormatCache(destroyStructure: true);
			}
			_isContentFormatValid = false;
		}
		if (this.ContentInvalidated != null)
		{
			this.ContentInvalidated(this, EventArgs.Empty);
		}
	}

	void IFlowDocumentFormatter.OnContentInvalidated(bool affectsLayout, ITextPointer start, ITextPointer end)
	{
		((IFlowDocumentFormatter)this).OnContentInvalidated(affectsLayout);
	}

	void IFlowDocumentFormatter.Suspend()
	{
		if (this.Suspended != null)
		{
			this.Suspended(this, EventArgs.Empty);
		}
	}
}
