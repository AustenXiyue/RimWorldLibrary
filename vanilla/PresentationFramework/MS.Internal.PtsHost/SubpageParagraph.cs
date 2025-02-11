using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal class SubpageParagraph : BaseParagraph
{
	private BaseParagraph _mainTextSegment;

	protected bool _isInterruptible = true;

	internal SubpageParagraph(DependencyObject element, StructuralCache structuralCache)
		: base(element, structuralCache)
	{
	}

	public override void Dispose()
	{
		base.Dispose();
		if (_mainTextSegment != null)
		{
			_mainTextSegment.Dispose();
			_mainTextSegment = null;
		}
		GC.SuppressFinalize(this);
	}

	internal override void GetParaProperties(ref PTS.FSPAP fspap)
	{
		GetParaProperties(ref fspap, ignoreElementProps: false);
		fspap.idobj = PtsHost.SubpageParagraphId;
		if (_mainTextSegment == null)
		{
			_mainTextSegment = new ContainerParagraph(_element, _structuralCache);
		}
	}

	internal override void CreateParaclient(out nint paraClientHandle)
	{
		SubpageParaClient subpageParaClient = new SubpageParaClient(this);
		paraClientHandle = subpageParaClient.Handle;
	}

	internal unsafe void FormatParaFinite(SubpageParaClient paraClient, nint pbrkrecIn, int fBRFromPreviousPage, nint footnoteRejector, int fEmptyOk, int fSuppressTopSpace, uint fswdir, ref PTS.FSRECT fsrcToFill, MarginCollapsingState mcs, PTS.FSKCLEAR fskclearIn, PTS.FSKSUPPRESSHARDBREAKBEFOREFIRSTPARA fsksuppresshardbreakbeforefirstparaIn, out PTS.FSFMTR fsfmtr, out nint pfspara, out nint pbrkrecOut, out int dvrUsed, out PTS.FSBBOX fsbbox, out nint pmcsclientOut, out PTS.FSKCLEAR fskclearOut, out int dvrTopSpace)
	{
		uint num = PTS.FlowDirectionToFswdir((FlowDirection)base.Element.GetValue(FrameworkElement.FlowDirectionProperty));
		if (mcs != null && pbrkrecIn != IntPtr.Zero)
		{
			mcs = null;
		}
		PTS.FSRECT rcMargin = default(PTS.FSRECT);
		int du = fsrcToFill.du;
		int num2 = fsrcToFill.dv;
		Invariant.Assert(base.Element is TableCell || base.Element is AnchoredBlock);
		fskclearIn = PTS.WrapDirectionToFskclear((WrapDirection)base.Element.GetValue(Block.ClearFloatersProperty));
		int margin = 0;
		MarginCollapsingState mcsNew = null;
		MbpInfo mbpInfo = MbpInfo.FromElement(base.Element, base.StructuralCache.TextFormatterHost.PixelsPerDip);
		if (num != fswdir)
		{
			PTS.FSRECT rectPage = base.StructuralCache.CurrentFormatContext.PageRect;
			PTS.Validate(PTS.FsTransformRectangle(fswdir, ref rectPage, ref fsrcToFill, num, out fsrcToFill));
			mbpInfo.MirrorMargin();
		}
		du = Math.Max(1, du - (mbpInfo.MBPLeft + mbpInfo.MBPRight));
		if (pbrkrecIn == IntPtr.Zero)
		{
			MarginCollapsingState.CollapseTopMargin(base.PtsContext, mbpInfo, mcs, out mcsNew, out margin);
			if (PTS.ToBoolean(fSuppressTopSpace))
			{
				margin = 0;
			}
			num2 = Math.Max(1, num2 - (margin + mbpInfo.BPTop));
			if (mcsNew != null)
			{
				mcsNew.Dispose();
				mcsNew = null;
			}
		}
		rcMargin.du = du;
		rcMargin.dv = num2;
		ColumnPropertiesGroup columnProperties = new ColumnPropertiesGroup(_element);
		double lineHeightValue = DynamicPropertyReader.GetLineHeightValue(_element);
		double pageFontSize = (double)_structuralCache.PropertyOwner.GetValue(TextElement.FontSizeProperty);
		FontFamily pageFontFamily = (FontFamily)_structuralCache.PropertyOwner.GetValue(TextElement.FontFamilyProperty);
		int num3 = PtsHelper.CalculateColumnCount(columnProperties, lineHeightValue, TextDpi.FromTextDpi(du), pageFontSize, pageFontFamily, enableColumns: false);
		PTS.FSCOLUMNINFO[] array = new PTS.FSCOLUMNINFO[num3];
		fixed (PTS.FSCOLUMNINFO* pfscolinfo = array)
		{
			PtsHelper.GetColumnsInfo(columnProperties, lineHeightValue, TextDpi.FromTextDpi(du), pageFontSize, pageFontFamily, num3, pfscolinfo, enableColumns: false);
		}
		base.StructuralCache.CurrentFormatContext.PushNewPageData(new Size(TextDpi.FromTextDpi(du), TextDpi.FromTextDpi(num2)), default(Thickness), incrementalUpdate: false, finitePage: true);
		fixed (PTS.FSCOLUMNINFO* rgColumnInfo = array)
		{
			PTS.Validate(PTS.FsCreateSubpageFinite(base.PtsContext.Context, pbrkrecIn, fBRFromPreviousPage, _mainTextSegment.Handle, footnoteRejector, fEmptyOk, fSuppressTopSpace, fswdir, du, num2, ref rcMargin, num3, rgColumnInfo, 0, 0, null, null, 0, null, null, PTS.FromBoolean(condition: false), fsksuppresshardbreakbeforefirstparaIn, out fsfmtr, out pfspara, out pbrkrecOut, out dvrUsed, out fsbbox, out pmcsclientOut, out dvrTopSpace), base.PtsContext);
		}
		base.StructuralCache.CurrentFormatContext.PopPageData();
		fskclearOut = PTS.FSKCLEAR.fskclearNone;
		if (PTS.ToBoolean(fsbbox.fDefined))
		{
			dvrUsed = Math.Max(dvrUsed, fsbbox.fsrc.dv + fsbbox.fsrc.v);
			fsrcToFill.du = Math.Max(fsrcToFill.du, fsbbox.fsrc.du + fsbbox.fsrc.u);
		}
		if (pbrkrecIn == IntPtr.Zero)
		{
			dvrTopSpace = ((mbpInfo.BPTop != 0) ? margin : dvrTopSpace);
			dvrUsed += margin + mbpInfo.BPTop;
		}
		if (pmcsclientOut != IntPtr.Zero)
		{
			mcsNew = base.PtsContext.HandleToObject(pmcsclientOut) as MarginCollapsingState;
			PTS.ValidateHandle(mcsNew);
			pmcsclientOut = IntPtr.Zero;
		}
		if (fsfmtr.kstop >= PTS.FSFMTRKSTOP.fmtrNoProgressOutOfSpace)
		{
			dvrUsed = (dvrTopSpace = 0);
		}
		else
		{
			if (fsfmtr.kstop == PTS.FSFMTRKSTOP.fmtrGoalReached)
			{
				MarginCollapsingState.CollapseBottomMargin(base.PtsContext, mbpInfo, mcsNew, out var mcsNew2, out var margin2);
				pmcsclientOut = mcsNew2?.Handle ?? IntPtr.Zero;
				if (pmcsclientOut == IntPtr.Zero)
				{
					dvrUsed += margin2 + mbpInfo.BPBottom;
				}
			}
			fsbbox.fsrc.u = fsrcToFill.u + mbpInfo.MarginLeft;
			fsbbox.fsrc.v = fsrcToFill.v + dvrTopSpace;
			fsbbox.fsrc.du = Math.Max(fsrcToFill.du - (mbpInfo.MarginLeft + mbpInfo.MarginRight), 0);
			fsbbox.fsrc.dv = Math.Max(dvrUsed - dvrTopSpace, 0);
		}
		if (num != fswdir)
		{
			PTS.FSRECT rectPage2 = base.StructuralCache.CurrentFormatContext.PageRect;
			PTS.Validate(PTS.FsTransformBbox(num, ref rectPage2, ref fsbbox, fswdir, out fsbbox));
		}
		if (mcsNew != null)
		{
			mcsNew.Dispose();
			mcsNew = null;
		}
		paraClient.SetChunkInfo(pbrkrecIn == IntPtr.Zero, pbrkrecOut == IntPtr.Zero);
	}

	internal unsafe void FormatParaBottomless(SubpageParaClient paraClient, int fSuppressTopSpace, uint fswdir, int urTrack, int durTrack, int vrTrack, MarginCollapsingState mcs, PTS.FSKCLEAR fskclearIn, int fInterruptable, out PTS.FSFMTRBL fsfmtrbl, out nint pfspara, out int dvrUsed, out PTS.FSBBOX fsbbox, out nint pmcsclientOut, out PTS.FSKCLEAR fskclearOut, out int dvrTopSpace, out int fPageBecomesUninterruptable)
	{
		uint num = PTS.FlowDirectionToFswdir((FlowDirection)base.Element.GetValue(FrameworkElement.FlowDirectionProperty));
		int num2 = durTrack;
		int vrMargin;
		int urMargin = (vrMargin = 0);
		Invariant.Assert(base.Element is TableCell || base.Element is AnchoredBlock);
		fskclearIn = PTS.WrapDirectionToFskclear((WrapDirection)base.Element.GetValue(Block.ClearFloatersProperty));
		MbpInfo mbpInfo = MbpInfo.FromElement(base.Element, base.StructuralCache.TextFormatterHost.PixelsPerDip);
		if (num != fswdir)
		{
			PTS.FSRECT rectTransform = new PTS.FSRECT(urTrack, 0, durTrack, 0);
			PTS.FSRECT rectPage = base.StructuralCache.CurrentFormatContext.PageRect;
			PTS.Validate(PTS.FsTransformRectangle(fswdir, ref rectPage, ref rectTransform, num, out rectTransform));
			urTrack = rectTransform.u;
			durTrack = rectTransform.du;
			mbpInfo.MirrorMargin();
		}
		num2 = Math.Max(1, num2 - (mbpInfo.MBPLeft + mbpInfo.MBPRight));
		MarginCollapsingState.CollapseTopMargin(base.PtsContext, mbpInfo, mcs, out var mcsNew, out var margin);
		if (mcsNew != null)
		{
			mcsNew.Dispose();
			mcsNew = null;
		}
		int durMargin = num2;
		ColumnPropertiesGroup columnProperties = new ColumnPropertiesGroup(_element);
		double lineHeightValue = DynamicPropertyReader.GetLineHeightValue(_element);
		double pageFontSize = (double)_structuralCache.PropertyOwner.GetValue(TextElement.FontSizeProperty);
		FontFamily pageFontFamily = (FontFamily)_structuralCache.PropertyOwner.GetValue(TextElement.FontFamilyProperty);
		int num3 = PtsHelper.CalculateColumnCount(columnProperties, lineHeightValue, TextDpi.FromTextDpi(num2), pageFontSize, pageFontFamily, enableColumns: false);
		PTS.FSCOLUMNINFO[] array = new PTS.FSCOLUMNINFO[num3];
		fixed (PTS.FSCOLUMNINFO* pfscolinfo = array)
		{
			PtsHelper.GetColumnsInfo(columnProperties, lineHeightValue, TextDpi.FromTextDpi(num2), pageFontSize, pageFontFamily, num3, pfscolinfo, enableColumns: false);
		}
		base.StructuralCache.CurrentFormatContext.PushNewPageData(new Size(TextDpi.FromTextDpi(num2), TextDpi.MaxWidth), default(Thickness), incrementalUpdate: false, finitePage: false);
		fixed (PTS.FSCOLUMNINFO* rgColumnInfo = array)
		{
			PTS.Validate(PTS.FsCreateSubpageBottomless(base.PtsContext.Context, _mainTextSegment.Handle, fSuppressTopSpace, fswdir, num2, urMargin, durMargin, vrMargin, num3, rgColumnInfo, 0, null, null, 0, null, null, PTS.FromBoolean(_isInterruptible), out fsfmtrbl, out pfspara, out dvrUsed, out fsbbox, out pmcsclientOut, out dvrTopSpace, out fPageBecomesUninterruptable), base.PtsContext);
		}
		base.StructuralCache.CurrentFormatContext.PopPageData();
		fskclearOut = PTS.FSKCLEAR.fskclearNone;
		if (fsfmtrbl != PTS.FSFMTRBL.fmtrblCollision)
		{
			if (pmcsclientOut != IntPtr.Zero)
			{
				mcsNew = base.PtsContext.HandleToObject(pmcsclientOut) as MarginCollapsingState;
				PTS.ValidateHandle(mcsNew);
				pmcsclientOut = IntPtr.Zero;
			}
			MarginCollapsingState.CollapseBottomMargin(base.PtsContext, mbpInfo, mcsNew, out var mcsNew2, out var margin2);
			pmcsclientOut = mcsNew2?.Handle ?? IntPtr.Zero;
			if (mcsNew != null)
			{
				mcsNew.Dispose();
				mcsNew = null;
			}
			if (PTS.ToBoolean(fsbbox.fDefined))
			{
				dvrUsed = Math.Max(dvrUsed, fsbbox.fsrc.dv + fsbbox.fsrc.v);
				durTrack = Math.Max(durTrack, fsbbox.fsrc.du + fsbbox.fsrc.u);
			}
			dvrTopSpace = ((mbpInfo.BPTop != 0) ? margin : dvrTopSpace);
			dvrUsed += margin + mbpInfo.BPTop + (margin2 + mbpInfo.BPBottom);
			fsbbox.fsrc.u = urTrack + mbpInfo.MarginLeft;
			fsbbox.fsrc.v = vrTrack + dvrTopSpace;
			fsbbox.fsrc.du = Math.Max(durTrack - (mbpInfo.MarginLeft + mbpInfo.MarginRight), 0);
			fsbbox.fsrc.dv = Math.Max(dvrUsed - dvrTopSpace, 0);
		}
		else
		{
			pfspara = IntPtr.Zero;
			dvrUsed = (dvrTopSpace = 0);
		}
		if (num != fswdir)
		{
			PTS.FSRECT rectPage2 = base.StructuralCache.CurrentFormatContext.PageRect;
			PTS.Validate(PTS.FsTransformBbox(num, ref rectPage2, ref fsbbox, fswdir, out fsbbox));
		}
		paraClient.SetChunkInfo(isFirstChunk: true, isLastChunk: true);
	}

	internal unsafe void UpdateBottomlessPara(nint pfspara, SubpageParaClient paraClient, int fSuppressTopSpace, uint fswdir, int urTrack, int durTrack, int vrTrack, MarginCollapsingState mcs, PTS.FSKCLEAR fskclearIn, int fInterruptable, out PTS.FSFMTRBL fsfmtrbl, out int dvrUsed, out PTS.FSBBOX fsbbox, out nint pmcsclientOut, out PTS.FSKCLEAR fskclearOut, out int dvrTopSpace, out int fPageBecomesUninterruptable)
	{
		uint num = PTS.FlowDirectionToFswdir((FlowDirection)base.Element.GetValue(FrameworkElement.FlowDirectionProperty));
		int num2 = durTrack;
		int vrMargin;
		int urMargin = (vrMargin = 0);
		Invariant.Assert(base.Element is TableCell || base.Element is AnchoredBlock);
		fskclearIn = PTS.WrapDirectionToFskclear((WrapDirection)base.Element.GetValue(Block.ClearFloatersProperty));
		MbpInfo mbpInfo = MbpInfo.FromElement(base.Element, base.StructuralCache.TextFormatterHost.PixelsPerDip);
		if (num != fswdir)
		{
			PTS.FSRECT rectTransform = new PTS.FSRECT(urTrack, 0, durTrack, 0);
			PTS.FSRECT rectPage = base.StructuralCache.CurrentFormatContext.PageRect;
			PTS.Validate(PTS.FsTransformRectangle(fswdir, ref rectPage, ref rectTransform, num, out rectTransform));
			urTrack = rectTransform.u;
			durTrack = rectTransform.du;
			mbpInfo.MirrorMargin();
		}
		num2 = Math.Max(1, num2 - (mbpInfo.MBPLeft + mbpInfo.MBPRight));
		MarginCollapsingState.CollapseTopMargin(base.PtsContext, mbpInfo, mcs, out var mcsNew, out var margin);
		if (mcsNew != null)
		{
			mcsNew.Dispose();
			mcsNew = null;
		}
		int durMargin = num2;
		ColumnPropertiesGroup columnProperties = new ColumnPropertiesGroup(_element);
		double lineHeightValue = DynamicPropertyReader.GetLineHeightValue(_element);
		double pageFontSize = (double)_structuralCache.PropertyOwner.GetValue(TextElement.FontSizeProperty);
		FontFamily pageFontFamily = (FontFamily)_structuralCache.PropertyOwner.GetValue(TextElement.FontFamilyProperty);
		int num3 = PtsHelper.CalculateColumnCount(columnProperties, lineHeightValue, TextDpi.FromTextDpi(num2), pageFontSize, pageFontFamily, enableColumns: false);
		PTS.FSCOLUMNINFO[] array = new PTS.FSCOLUMNINFO[num3];
		fixed (PTS.FSCOLUMNINFO* pfscolinfo = array)
		{
			PtsHelper.GetColumnsInfo(columnProperties, lineHeightValue, TextDpi.FromTextDpi(num2), pageFontSize, pageFontFamily, num3, pfscolinfo, enableColumns: false);
		}
		base.StructuralCache.CurrentFormatContext.PushNewPageData(new Size(TextDpi.FromTextDpi(num2), TextDpi.MaxWidth), default(Thickness), incrementalUpdate: true, finitePage: false);
		fixed (PTS.FSCOLUMNINFO* rgColumnInfo = array)
		{
			PTS.Validate(PTS.FsUpdateBottomlessSubpage(base.PtsContext.Context, pfspara, _mainTextSegment.Handle, fSuppressTopSpace, fswdir, num2, urMargin, durMargin, vrMargin, num3, rgColumnInfo, 0, null, null, 0, null, null, PTS.FromBoolean(condition: true), out fsfmtrbl, out dvrUsed, out fsbbox, out pmcsclientOut, out dvrTopSpace, out fPageBecomesUninterruptable), base.PtsContext);
		}
		base.StructuralCache.CurrentFormatContext.PopPageData();
		fskclearOut = PTS.FSKCLEAR.fskclearNone;
		if (fsfmtrbl != PTS.FSFMTRBL.fmtrblCollision)
		{
			if (pmcsclientOut != IntPtr.Zero)
			{
				mcsNew = base.PtsContext.HandleToObject(pmcsclientOut) as MarginCollapsingState;
				PTS.ValidateHandle(mcsNew);
				pmcsclientOut = IntPtr.Zero;
			}
			MarginCollapsingState.CollapseBottomMargin(base.PtsContext, mbpInfo, mcsNew, out var mcsNew2, out var margin2);
			pmcsclientOut = mcsNew2?.Handle ?? IntPtr.Zero;
			if (mcsNew != null)
			{
				mcsNew.Dispose();
				mcsNew = null;
			}
			if (PTS.ToBoolean(fsbbox.fDefined))
			{
				dvrUsed = Math.Max(dvrUsed, fsbbox.fsrc.dv + fsbbox.fsrc.v);
				durTrack = Math.Max(durTrack, fsbbox.fsrc.du + fsbbox.fsrc.u);
			}
			dvrTopSpace = ((mbpInfo.BPTop != 0) ? margin : dvrTopSpace);
			dvrUsed += margin + mbpInfo.BPTop + (margin2 + mbpInfo.BPBottom);
			fsbbox.fsrc.u = urTrack + mbpInfo.MarginLeft;
			fsbbox.fsrc.v = vrTrack + dvrTopSpace;
			fsbbox.fsrc.du = Math.Max(durTrack - (mbpInfo.MarginLeft + mbpInfo.MarginRight), 0);
			fsbbox.fsrc.dv = Math.Max(dvrUsed - dvrTopSpace, 0);
		}
		else
		{
			pfspara = IntPtr.Zero;
			dvrUsed = (dvrTopSpace = 0);
		}
		if (num != fswdir)
		{
			PTS.FSRECT rectPage2 = base.StructuralCache.CurrentFormatContext.PageRect;
			PTS.Validate(PTS.FsTransformBbox(num, ref rectPage2, ref fsbbox, fswdir, out fsbbox));
		}
		paraClient.SetChunkInfo(isFirstChunk: true, isLastChunk: true);
	}

	internal override void ClearUpdateInfo()
	{
		if (_mainTextSegment != null)
		{
			_mainTextSegment.ClearUpdateInfo();
		}
		base.ClearUpdateInfo();
	}

	internal override bool InvalidateStructure(int startPosition)
	{
		if (_mainTextSegment != null && _mainTextSegment.InvalidateStructure(startPosition))
		{
			_mainTextSegment.Dispose();
			_mainTextSegment = null;
		}
		return _mainTextSegment == null;
	}

	internal override void InvalidateFormatCache()
	{
		if (_mainTextSegment != null)
		{
			_mainTextSegment.InvalidateFormatCache();
		}
	}

	internal void UpdateSegmentLastFormatPositions()
	{
		_mainTextSegment.UpdateLastFormatPositions();
	}
}
