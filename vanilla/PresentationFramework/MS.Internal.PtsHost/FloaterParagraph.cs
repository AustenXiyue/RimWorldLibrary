using System;
using System.Windows;
using System.Windows.Documents;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal sealed class FloaterParagraph : FloaterBaseParagraph
{
	private BaseParagraph _mainTextSegment;

	private HorizontalAlignment HorizontalAlignment
	{
		get
		{
			if (base.Element is Floater)
			{
				return ((Floater)base.Element).HorizontalAlignment;
			}
			switch (((Figure)base.Element).HorizontalAnchor)
			{
			case FigureHorizontalAnchor.PageLeft:
			case FigureHorizontalAnchor.ContentLeft:
			case FigureHorizontalAnchor.ColumnLeft:
				return HorizontalAlignment.Left;
			case FigureHorizontalAnchor.PageRight:
			case FigureHorizontalAnchor.ContentRight:
			case FigureHorizontalAnchor.ColumnRight:
				return HorizontalAlignment.Right;
			default:
				_ = 7;
				break;
			case FigureHorizontalAnchor.PageCenter:
			case FigureHorizontalAnchor.ContentCenter:
				break;
			}
			return HorizontalAlignment.Center;
		}
	}

	private WrapDirection WrapDirection
	{
		get
		{
			if (base.Element is Floater)
			{
				if (HorizontalAlignment == HorizontalAlignment.Stretch)
				{
					return WrapDirection.None;
				}
				return WrapDirection.Both;
			}
			return ((Figure)base.Element).WrapDirection;
		}
	}

	internal FloaterParagraph(TextElement element, StructuralCache structuralCache)
		: base(element, structuralCache)
	{
	}

	internal override void UpdGetParaChange(out PTS.FSKCHANGE fskch, out int fNoFurtherChanges)
	{
		base.UpdGetParaChange(out fskch, out fNoFurtherChanges);
		fskch = PTS.FSKCHANGE.fskchNew;
	}

	public override void Dispose()
	{
		base.Dispose();
		if (_mainTextSegment != null)
		{
			_mainTextSegment.Dispose();
			_mainTextSegment = null;
		}
	}

	internal override void CreateParaclient(out nint paraClientHandle)
	{
		FloaterParaClient floaterParaClient = new FloaterParaClient(this);
		paraClientHandle = floaterParaClient.Handle;
		if (_mainTextSegment == null)
		{
			_mainTextSegment = new ContainerParagraph(base.Element, base.StructuralCache);
		}
	}

	internal override void CollapseMargin(BaseParaClient paraClient, MarginCollapsingState mcs, uint fswdir, bool suppressTopSpace, out int dvr)
	{
		dvr = 0;
	}

	internal override void GetFloaterProperties(uint fswdirTrack, out PTS.FSFLOATERPROPS fsfloaterprops)
	{
		fsfloaterprops = default(PTS.FSFLOATERPROPS);
		fsfloaterprops.fFloat = 1;
		fsfloaterprops.fskclear = PTS.WrapDirectionToFskclear((WrapDirection)base.Element.GetValue(Block.ClearFloatersProperty));
		switch (HorizontalAlignment)
		{
		case HorizontalAlignment.Right:
			fsfloaterprops.fskfloatalignment = PTS.FSKFLOATALIGNMENT.fskfloatalignMax;
			break;
		case HorizontalAlignment.Center:
			fsfloaterprops.fskfloatalignment = PTS.FSKFLOATALIGNMENT.fskfloatalignCenter;
			break;
		default:
			fsfloaterprops.fskfloatalignment = PTS.FSKFLOATALIGNMENT.fskfloatalignMin;
			break;
		}
		fsfloaterprops.fskwr = PTS.WrapDirectionToFskwrap(WrapDirection);
		fsfloaterprops.fDelayNoProgress = 1;
	}

	internal override void FormatFloaterContentFinite(FloaterBaseParaClient paraClient, nint pbrkrecIn, int fBRFromPreviousPage, nint footnoteRejector, int fEmptyOk, int fSuppressTopSpace, uint fswdir, int fAtMaxWidth, int durAvailable, int dvrAvailable, PTS.FSKSUPPRESSHARDBREAKBEFOREFIRSTPARA fsksuppresshardbreakbeforefirstparaIn, out PTS.FSFMTR fsfmtr, out nint pfsFloatContent, out nint pbrkrecOut, out int durFloaterWidth, out int dvrFloaterHeight, out PTS.FSBBOX fsbbox, out int cPolygons, out int cVertices)
	{
		PTS.FlowDirectionToFswdir((FlowDirection)base.Element.GetValue(FrameworkElement.FlowDirectionProperty));
		Invariant.Assert(paraClient is FloaterParaClient);
		if (IsFloaterRejected(PTS.ToBoolean(fAtMaxWidth), TextDpi.FromTextDpi(durAvailable)))
		{
			durFloaterWidth = (dvrFloaterHeight = 0);
			cPolygons = (cVertices = 0);
			fsfmtr = default(PTS.FSFMTR);
			fsfmtr.kstop = PTS.FSFMTRKSTOP.fmtrNoProgressOutOfSpace;
			fsfmtr.fContainsItemThatStoppedBeforeFootnote = 0;
			fsfmtr.fForcedProgress = 0;
			fsbbox = default(PTS.FSBBOX);
			fsbbox.fDefined = 0;
			pbrkrecOut = IntPtr.Zero;
			pfsFloatContent = IntPtr.Zero;
		}
		else
		{
			if (!base.StructuralCache.CurrentFormatContext.FinitePage)
			{
				if (double.IsInfinity(base.StructuralCache.CurrentFormatContext.PageHeight))
				{
					if (dvrAvailable > 1073741823)
					{
						dvrAvailable = Math.Min(dvrAvailable, 1073741823);
						fEmptyOk = 0;
					}
				}
				else
				{
					dvrAvailable = Math.Min(dvrAvailable, TextDpi.ToTextDpi(base.StructuralCache.CurrentFormatContext.PageHeight));
				}
			}
			MbpInfo mbpInfo = MbpInfo.FromElement(base.Element, base.StructuralCache.TextFormatterHost.PixelsPerDip);
			double num = CalculateWidth(TextDpi.FromTextDpi(durAvailable));
			AdjustDurAvailable(num, ref durAvailable, out var subpageWidth);
			int num2 = Math.Max(1, dvrAvailable - (mbpInfo.MBPTop + mbpInfo.MBPBottom));
			PTS.FSRECT rcMargin = default(PTS.FSRECT);
			rcMargin.du = subpageWidth;
			rcMargin.dv = num2;
			int num3 = 1;
			PTS.FSCOLUMNINFO[] array = new PTS.FSCOLUMNINFO[num3];
			array[0].durBefore = 0;
			array[0].durWidth = subpageWidth;
			CreateSubpageFiniteHelper(base.PtsContext, pbrkrecIn, fBRFromPreviousPage, _mainTextSegment.Handle, footnoteRejector, fEmptyOk, 1, fswdir, subpageWidth, num2, ref rcMargin, num3, array, 0, fsksuppresshardbreakbeforefirstparaIn, out fsfmtr, out pfsFloatContent, out pbrkrecOut, out dvrFloaterHeight, out fsbbox, out var pfsMcsClient, out var topSpace);
			if (fsfmtr.kstop >= PTS.FSFMTRKSTOP.fmtrNoProgressOutOfSpace)
			{
				durFloaterWidth = (dvrFloaterHeight = 0);
				cPolygons = (cVertices = 0);
			}
			else
			{
				if (PTS.ToBoolean(fsbbox.fDefined))
				{
					if (fsbbox.fsrc.du < subpageWidth && double.IsNaN(num) && HorizontalAlignment != HorizontalAlignment.Stretch)
					{
						if (pfsFloatContent != IntPtr.Zero)
						{
							PTS.Validate(PTS.FsDestroySubpage(base.PtsContext.Context, pfsFloatContent), base.PtsContext);
							pfsFloatContent = IntPtr.Zero;
						}
						if (pbrkrecOut != IntPtr.Zero)
						{
							PTS.Validate(PTS.FsDestroySubpageBreakRecord(base.PtsContext.Context, pbrkrecOut), base.PtsContext);
							pbrkrecOut = IntPtr.Zero;
						}
						if (pfsMcsClient != IntPtr.Zero)
						{
							MarginCollapsingState obj = base.PtsContext.HandleToObject(pfsMcsClient) as MarginCollapsingState;
							PTS.ValidateHandle(obj);
							obj.Dispose();
							pfsMcsClient = IntPtr.Zero;
						}
						subpageWidth = (rcMargin.du = fsbbox.fsrc.du + 1);
						rcMargin.dv = num2;
						array[0].durWidth = subpageWidth;
						CreateSubpageFiniteHelper(base.PtsContext, pbrkrecIn, fBRFromPreviousPage, _mainTextSegment.Handle, footnoteRejector, fEmptyOk, 1, fswdir, subpageWidth, num2, ref rcMargin, num3, array, 0, fsksuppresshardbreakbeforefirstparaIn, out fsfmtr, out pfsFloatContent, out pbrkrecOut, out dvrFloaterHeight, out fsbbox, out pfsMcsClient, out topSpace);
					}
				}
				else
				{
					subpageWidth = TextDpi.ToTextDpi(TextDpi.MinWidth);
				}
				if (pfsMcsClient != IntPtr.Zero)
				{
					MarginCollapsingState obj2 = base.PtsContext.HandleToObject(pfsMcsClient) as MarginCollapsingState;
					PTS.ValidateHandle(obj2);
					obj2.Dispose();
					pfsMcsClient = IntPtr.Zero;
				}
				durFloaterWidth = subpageWidth + mbpInfo.MBPLeft + mbpInfo.MBPRight;
				dvrFloaterHeight += mbpInfo.MBPTop + mbpInfo.MBPBottom;
				fsbbox.fsrc.u = 0;
				fsbbox.fsrc.v = 0;
				fsbbox.fsrc.du = durFloaterWidth;
				fsbbox.fsrc.dv = dvrFloaterHeight;
				fsbbox.fDefined = 1;
				cPolygons = (cVertices = 0);
				if (durFloaterWidth > durAvailable || dvrFloaterHeight > dvrAvailable)
				{
					if (PTS.ToBoolean(fEmptyOk))
					{
						if (pfsFloatContent != IntPtr.Zero)
						{
							PTS.Validate(PTS.FsDestroySubpage(base.PtsContext.Context, pfsFloatContent), base.PtsContext);
							pfsFloatContent = IntPtr.Zero;
						}
						if (pbrkrecOut != IntPtr.Zero)
						{
							PTS.Validate(PTS.FsDestroySubpageBreakRecord(base.PtsContext.Context, pbrkrecOut), base.PtsContext);
							pbrkrecOut = IntPtr.Zero;
						}
						cPolygons = (cVertices = 0);
						fsfmtr.kstop = PTS.FSFMTRKSTOP.fmtrNoProgressOutOfSpace;
					}
					else
					{
						fsfmtr.fForcedProgress = 1;
					}
				}
			}
		}
		((FloaterParaClient)paraClient).SubpageHandle = pfsFloatContent;
	}

	internal override void FormatFloaterContentBottomless(FloaterBaseParaClient paraClient, int fSuppressTopSpace, uint fswdir, int fAtMaxWidth, int durAvailable, int dvrAvailable, out PTS.FSFMTRBL fsfmtrbl, out nint pfsFloatContent, out int durFloaterWidth, out int dvrFloaterHeight, out PTS.FSBBOX fsbbox, out int cPolygons, out int cVertices)
	{
		PTS.FlowDirectionToFswdir((FlowDirection)base.Element.GetValue(FrameworkElement.FlowDirectionProperty));
		Invariant.Assert(paraClient is FloaterParaClient);
		if (IsFloaterRejected(PTS.ToBoolean(fAtMaxWidth), TextDpi.FromTextDpi(durAvailable)))
		{
			durFloaterWidth = durAvailable + 1;
			dvrFloaterHeight = dvrAvailable + 1;
			cPolygons = (cVertices = 0);
			fsfmtrbl = PTS.FSFMTRBL.fmtrblInterrupted;
			fsbbox = default(PTS.FSBBOX);
			fsbbox.fDefined = 0;
			pfsFloatContent = IntPtr.Zero;
		}
		else
		{
			MbpInfo mbpInfo = MbpInfo.FromElement(base.Element, base.StructuralCache.TextFormatterHost.PixelsPerDip);
			double num = CalculateWidth(TextDpi.FromTextDpi(durAvailable));
			AdjustDurAvailable(num, ref durAvailable, out var subpageWidth);
			int durMargin = subpageWidth;
			int vrMargin;
			int urMargin = (vrMargin = 0);
			int num2 = 1;
			PTS.FSCOLUMNINFO[] array = new PTS.FSCOLUMNINFO[num2];
			array[0].durBefore = 0;
			array[0].durWidth = subpageWidth;
			InvalidateMainTextSegment();
			CreateSubpageBottomlessHelper(base.PtsContext, _mainTextSegment.Handle, 1, fswdir, subpageWidth, urMargin, durMargin, vrMargin, num2, array, out fsfmtrbl, out pfsFloatContent, out dvrFloaterHeight, out fsbbox, out var pfsMcsClient, out var pTopSpace, out var fPageBecomesUninterruptible);
			if (fsfmtrbl != PTS.FSFMTRBL.fmtrblCollision)
			{
				if (PTS.ToBoolean(fsbbox.fDefined))
				{
					if (fsbbox.fsrc.du < subpageWidth && double.IsNaN(num) && HorizontalAlignment != HorizontalAlignment.Stretch)
					{
						if (pfsFloatContent != IntPtr.Zero)
						{
							PTS.Validate(PTS.FsDestroySubpage(base.PtsContext.Context, pfsFloatContent), base.PtsContext);
						}
						if (pfsMcsClient != IntPtr.Zero)
						{
							MarginCollapsingState obj = base.PtsContext.HandleToObject(pfsMcsClient) as MarginCollapsingState;
							PTS.ValidateHandle(obj);
							obj.Dispose();
							pfsMcsClient = IntPtr.Zero;
						}
						subpageWidth = (durMargin = fsbbox.fsrc.du + 1);
						array[0].durWidth = subpageWidth;
						CreateSubpageBottomlessHelper(base.PtsContext, _mainTextSegment.Handle, 1, fswdir, subpageWidth, urMargin, durMargin, vrMargin, num2, array, out fsfmtrbl, out pfsFloatContent, out dvrFloaterHeight, out fsbbox, out pfsMcsClient, out pTopSpace, out fPageBecomesUninterruptible);
					}
				}
				else
				{
					subpageWidth = TextDpi.ToTextDpi(TextDpi.MinWidth);
				}
				if (pfsMcsClient != IntPtr.Zero)
				{
					MarginCollapsingState obj2 = base.PtsContext.HandleToObject(pfsMcsClient) as MarginCollapsingState;
					PTS.ValidateHandle(obj2);
					obj2.Dispose();
					pfsMcsClient = IntPtr.Zero;
				}
				durFloaterWidth = subpageWidth + mbpInfo.MBPLeft + mbpInfo.MBPRight;
				dvrFloaterHeight += mbpInfo.MBPTop + mbpInfo.MBPBottom;
				if (dvrFloaterHeight > dvrAvailable || (durFloaterWidth > durAvailable && !PTS.ToBoolean(fAtMaxWidth)))
				{
					if (pfsFloatContent != IntPtr.Zero)
					{
						PTS.Validate(PTS.FsDestroySubpage(base.PtsContext.Context, pfsFloatContent), base.PtsContext);
					}
					cPolygons = (cVertices = 0);
					pfsFloatContent = IntPtr.Zero;
				}
				else
				{
					fsbbox.fsrc.u = 0;
					fsbbox.fsrc.v = 0;
					fsbbox.fsrc.du = durFloaterWidth;
					fsbbox.fsrc.dv = dvrFloaterHeight;
					cPolygons = (cVertices = 0);
				}
			}
			else
			{
				durFloaterWidth = (dvrFloaterHeight = 0);
				cPolygons = (cVertices = 0);
				pfsFloatContent = IntPtr.Zero;
			}
		}
		((FloaterParaClient)paraClient).SubpageHandle = pfsFloatContent;
	}

	internal override void UpdateBottomlessFloaterContent(FloaterBaseParaClient paraClient, int fSuppressTopSpace, uint fswdir, int fAtMaxWidth, int durAvailable, int dvrAvailable, nint pfsFloatContent, out PTS.FSFMTRBL fsfmtrbl, out int durFloaterWidth, out int dvrFloaterHeight, out PTS.FSBBOX fsbbox, out int cPolygons, out int cVertices)
	{
		fsfmtrbl = PTS.FSFMTRBL.fmtrblGoalReached;
		durFloaterWidth = (dvrFloaterHeight = (cPolygons = (cVertices = 0)));
		fsbbox = default(PTS.FSBBOX);
		Invariant.Assert(condition: false, "No appropriate handling for update in attached object floater.");
	}

	internal override void GetMCSClientAfterFloater(uint fswdirTrack, MarginCollapsingState mcs, out nint pmcsclientOut)
	{
		if (mcs != null)
		{
			pmcsclientOut = mcs.Handle;
		}
		else
		{
			pmcsclientOut = IntPtr.Zero;
		}
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

	private void AdjustDurAvailable(double specifiedWidth, ref int durAvailable, out int subpageWidth)
	{
		MbpInfo mbpInfo = MbpInfo.FromElement(base.Element, base.StructuralCache.TextFormatterHost.PixelsPerDip);
		if (!double.IsNaN(specifiedWidth))
		{
			TextDpi.EnsureValidPageWidth(ref specifiedWidth);
			int num = TextDpi.ToTextDpi(specifiedWidth);
			if (num + mbpInfo.MarginRight + mbpInfo.MarginLeft <= durAvailable)
			{
				durAvailable = num + mbpInfo.MarginLeft + mbpInfo.MarginRight;
				subpageWidth = Math.Max(1, num - (mbpInfo.BPLeft + mbpInfo.BPRight));
			}
			else
			{
				subpageWidth = Math.Max(1, durAvailable - (mbpInfo.MBPLeft + mbpInfo.MBPRight));
			}
		}
		else
		{
			subpageWidth = Math.Max(1, durAvailable - (mbpInfo.MBPLeft + mbpInfo.MBPRight));
		}
	}

	private unsafe void CreateSubpageFiniteHelper(PtsContext ptsContext, nint brParaIn, int fFromPreviousPage, nint nSeg, nint pFtnRej, int fEmptyOk, int fSuppressTopSpace, uint fswdir, int lWidth, int lHeight, ref PTS.FSRECT rcMargin, int cColumns, PTS.FSCOLUMNINFO[] columnInfoCollection, int fApplyColumnBalancing, PTS.FSKSUPPRESSHARDBREAKBEFOREFIRSTPARA fsksuppresshardbreakbeforefirstparaIn, out PTS.FSFMTR fsfmtr, out nint pSubPage, out nint brParaOut, out int dvrUsed, out PTS.FSBBOX fsBBox, out nint pfsMcsClient, out int topSpace)
	{
		base.StructuralCache.CurrentFormatContext.PushNewPageData(new Size(TextDpi.FromTextDpi(lWidth), TextDpi.FromTextDpi(lHeight)), default(Thickness), incrementalUpdate: false, finitePage: true);
		fixed (PTS.FSCOLUMNINFO* rgColumnInfo = columnInfoCollection)
		{
			PTS.Validate(PTS.FsCreateSubpageFinite(ptsContext.Context, brParaIn, fFromPreviousPage, nSeg, pFtnRej, fEmptyOk, fSuppressTopSpace, fswdir, lWidth, lHeight, ref rcMargin, cColumns, rgColumnInfo, 0, 0, null, null, 0, null, null, 0, fsksuppresshardbreakbeforefirstparaIn, out fsfmtr, out pSubPage, out brParaOut, out dvrUsed, out fsBBox, out pfsMcsClient, out topSpace), ptsContext);
		}
		base.StructuralCache.CurrentFormatContext.PopPageData();
	}

	private unsafe void CreateSubpageBottomlessHelper(PtsContext ptsContext, nint nSeg, int fSuppressTopSpace, uint fswdir, int lWidth, int urMargin, int durMargin, int vrMargin, int cColumns, PTS.FSCOLUMNINFO[] columnInfoCollection, out PTS.FSFMTRBL pfsfmtr, out nint ppSubPage, out int pdvrUsed, out PTS.FSBBOX pfsBBox, out nint pfsMcsClient, out int pTopSpace, out int fPageBecomesUninterruptible)
	{
		base.StructuralCache.CurrentFormatContext.PushNewPageData(new Size(TextDpi.FromTextDpi(lWidth), TextDpi.MaxWidth), default(Thickness), incrementalUpdate: false, finitePage: false);
		fixed (PTS.FSCOLUMNINFO* rgColumnInfo = columnInfoCollection)
		{
			PTS.Validate(PTS.FsCreateSubpageBottomless(ptsContext.Context, nSeg, fSuppressTopSpace, fswdir, lWidth, urMargin, durMargin, vrMargin, cColumns, rgColumnInfo, 0, null, null, 0, null, null, 0, out pfsfmtr, out ppSubPage, out pdvrUsed, out pfsBBox, out pfsMcsClient, out pTopSpace, out fPageBecomesUninterruptible), ptsContext);
		}
		base.StructuralCache.CurrentFormatContext.PopPageData();
	}

	private void InvalidateMainTextSegment()
	{
		DtrList dtrList = base.StructuralCache.DtrsFromRange(base.ParagraphStartCharacterPosition, base.LastFormatCch);
		if (dtrList != null && dtrList.Length > 0)
		{
			_mainTextSegment.InvalidateStructure(dtrList[0].StartIndex);
		}
	}

	private double CalculateWidth(double spaceAvailable)
	{
		if (base.Element is Floater)
		{
			return ((Floater)base.Element).Width;
		}
		bool isWidthAuto;
		double val = FigureHelper.CalculateFigureWidth(base.StructuralCache, (Figure)base.Element, ((Figure)base.Element).Width, out isWidthAuto);
		if (isWidthAuto)
		{
			return double.NaN;
		}
		return Math.Min(val, spaceAvailable);
	}

	private bool IsFloaterRejected(bool fAtMaxWidth, double availableSpace)
	{
		if (fAtMaxWidth)
		{
			return false;
		}
		if (base.Element is Floater && HorizontalAlignment != HorizontalAlignment.Stretch)
		{
			return false;
		}
		if (base.Element is Figure)
		{
			FigureLength width = ((Figure)base.Element).Width;
			if (width.IsAuto)
			{
				return false;
			}
			if (width.IsAbsolute && width.Value < availableSpace)
			{
				return false;
			}
		}
		return true;
	}
}
