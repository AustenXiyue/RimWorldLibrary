using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal sealed class Section : UnmanagedHandle
{
	private BaseParagraph _mainTextSegment;

	private readonly StructuralCache _structuralCache;

	internal bool CanUpdate => _mainTextSegment != null;

	internal StructuralCache StructuralCache => _structuralCache;

	internal DependencyObject Element => _structuralCache.PropertyOwner;

	internal Section(StructuralCache structuralCache)
		: base(structuralCache.PtsContext)
	{
		_structuralCache = structuralCache;
	}

	public override void Dispose()
	{
		DestroyStructure();
		base.Dispose();
	}

	internal void FSkipPage(out int fSkip)
	{
		fSkip = 0;
	}

	internal void GetPageDimensions(out uint fswdir, out int fHeaderFooterAtTopBottom, out int durPage, out int dvrPage, ref PTS.FSRECT fsrcMargin)
	{
		Size pageSize = _structuralCache.CurrentFormatContext.PageSize;
		durPage = TextDpi.ToTextDpi(pageSize.Width);
		dvrPage = TextDpi.ToTextDpi(pageSize.Height);
		Thickness pageMargin = _structuralCache.CurrentFormatContext.PageMargin;
		TextDpi.EnsureValidPageMargin(ref pageMargin, pageSize);
		fsrcMargin.u = TextDpi.ToTextDpi(pageMargin.Left);
		fsrcMargin.v = TextDpi.ToTextDpi(pageMargin.Top);
		fsrcMargin.du = durPage - TextDpi.ToTextDpi(pageMargin.Left + pageMargin.Right);
		fsrcMargin.dv = dvrPage - TextDpi.ToTextDpi(pageMargin.Top + pageMargin.Bottom);
		StructuralCache.PageFlowDirection = (FlowDirection)_structuralCache.PropertyOwner.GetValue(FrameworkElement.FlowDirectionProperty);
		fswdir = PTS.FlowDirectionToFswdir(StructuralCache.PageFlowDirection);
		fHeaderFooterAtTopBottom = 0;
	}

	internal unsafe void GetJustificationProperties(nint* rgnms, int cnms, int fLastSectionNotBroken, out int fJustify, out PTS.FSKALIGNPAGE fskal, out int fCancelAtLastColumn)
	{
		fJustify = 0;
		fCancelAtLastColumn = 0;
		fskal = PTS.FSKALIGNPAGE.fskalpgTop;
	}

	internal void GetNextSection(out int fSuccess, out nint nmsNext)
	{
		fSuccess = 0;
		nmsNext = IntPtr.Zero;
	}

	internal void GetSectionProperties(out int fNewPage, out uint fswdir, out int fApplyColumnBalancing, out int ccol, out int cSegmentDefinedColumnSpanAreas, out int cHeightDefinedColumnSpanAreas)
	{
		ColumnPropertiesGroup columnProperties = new ColumnPropertiesGroup(Element);
		Size pageSize = _structuralCache.CurrentFormatContext.PageSize;
		double lineHeightValue = DynamicPropertyReader.GetLineHeightValue(Element);
		Thickness pageMargin = _structuralCache.CurrentFormatContext.PageMargin;
		double pageFontSize = (double)_structuralCache.PropertyOwner.GetValue(TextElement.FontSizeProperty);
		FontFamily pageFontFamily = (FontFamily)_structuralCache.PropertyOwner.GetValue(TextElement.FontFamilyProperty);
		bool finitePage = _structuralCache.CurrentFormatContext.FinitePage;
		fNewPage = 0;
		fswdir = PTS.FlowDirectionToFswdir((FlowDirection)_structuralCache.PropertyOwner.GetValue(FrameworkElement.FlowDirectionProperty));
		fApplyColumnBalancing = 0;
		ccol = PtsHelper.CalculateColumnCount(columnProperties, lineHeightValue, pageSize.Width - (pageMargin.Left + pageMargin.Right), pageFontSize, pageFontFamily, finitePage);
		cSegmentDefinedColumnSpanAreas = 0;
		cHeightDefinedColumnSpanAreas = 0;
	}

	internal void GetMainTextSegment(out nint nmSegment)
	{
		if (_mainTextSegment == null)
		{
			_mainTextSegment = new ContainerParagraph(Element, _structuralCache);
		}
		nmSegment = _mainTextSegment.Handle;
	}

	internal void GetHeaderSegment(nint pfsbrpagePrelim, uint fswdir, out int fHeaderPresent, out int fHardMargin, out int dvrMaxHeight, out int dvrFromEdge, out uint fswdirHeader, out nint nmsHeader)
	{
		fHeaderPresent = 0;
		fHardMargin = 0;
		dvrMaxHeight = (dvrFromEdge = 0);
		fswdirHeader = fswdir;
		nmsHeader = IntPtr.Zero;
	}

	internal void GetFooterSegment(nint pfsbrpagePrelim, uint fswdir, out int fFooterPresent, out int fHardMargin, out int dvrMaxHeight, out int dvrFromEdge, out uint fswdirFooter, out nint nmsFooter)
	{
		fFooterPresent = 0;
		fHardMargin = 0;
		dvrMaxHeight = (dvrFromEdge = 0);
		fswdirFooter = fswdir;
		nmsFooter = IntPtr.Zero;
	}

	internal unsafe void GetSectionColumnInfo(uint fswdir, int ncol, PTS.FSCOLUMNINFO* pfscolinfo, out int ccol)
	{
		ColumnPropertiesGroup columnProperties = new ColumnPropertiesGroup(Element);
		Size pageSize = _structuralCache.CurrentFormatContext.PageSize;
		double lineHeightValue = DynamicPropertyReader.GetLineHeightValue(Element);
		Thickness pageMargin = _structuralCache.CurrentFormatContext.PageMargin;
		double pageFontSize = (double)_structuralCache.PropertyOwner.GetValue(TextElement.FontSizeProperty);
		FontFamily pageFontFamily = (FontFamily)_structuralCache.PropertyOwner.GetValue(TextElement.FontFamilyProperty);
		bool finitePage = _structuralCache.CurrentFormatContext.FinitePage;
		ccol = ncol;
		PtsHelper.GetColumnsInfo(columnProperties, lineHeightValue, pageSize.Width - (pageMargin.Left + pageMargin.Right), pageFontSize, pageFontFamily, ncol, pfscolinfo, finitePage);
	}

	internal void GetEndnoteSegment(out int fEndnotesPresent, out nint nmsEndnotes)
	{
		fEndnotesPresent = 0;
		nmsEndnotes = IntPtr.Zero;
	}

	internal void GetEndnoteSeparators(out nint nmsEndnoteSeparator, out nint nmsEndnoteContSeparator, out nint nmsEndnoteContNotice)
	{
		nmsEndnoteSeparator = IntPtr.Zero;
		nmsEndnoteContSeparator = IntPtr.Zero;
		nmsEndnoteContNotice = IntPtr.Zero;
	}

	internal void InvalidateFormatCache()
	{
		if (_mainTextSegment != null)
		{
			_mainTextSegment.InvalidateFormatCache();
		}
	}

	internal void ClearUpdateInfo()
	{
		if (_mainTextSegment != null)
		{
			_mainTextSegment.ClearUpdateInfo();
		}
	}

	internal void InvalidateStructure()
	{
		if (_mainTextSegment != null)
		{
			DtrList dtrList = _structuralCache.DtrList;
			if (dtrList != null)
			{
				_mainTextSegment.InvalidateStructure(dtrList[0].StartIndex);
			}
		}
	}

	internal void DestroyStructure()
	{
		if (_mainTextSegment != null)
		{
			_mainTextSegment.Dispose();
			_mainTextSegment = null;
		}
	}

	internal void UpdateSegmentLastFormatPositions()
	{
		if (_mainTextSegment != null)
		{
			_mainTextSegment.UpdateLastFormatPositions();
		}
	}
}
