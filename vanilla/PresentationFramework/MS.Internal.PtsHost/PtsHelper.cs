using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.Documents;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal static class PtsHelper
{
	internal static void UpdateMirroringTransform(FlowDirection parentFD, FlowDirection childFD, ContainerVisual visualChild, double width)
	{
		if (parentFD != childFD)
		{
			MatrixTransform transform = new MatrixTransform(-1.0, 0.0, 0.0, 1.0, width, 0.0);
			visualChild.Transform = transform;
			visualChild.SetValue(FrameworkElement.FlowDirectionProperty, childFD);
		}
		else
		{
			visualChild.Transform = null;
			visualChild.ClearValue(FrameworkElement.FlowDirectionProperty);
		}
	}

	internal static void ClipChildrenToRect(ContainerVisual visual, Rect rect)
	{
		VisualCollection children = visual.Children;
		for (int i = 0; i < children.Count; i++)
		{
			((ContainerVisual)children[i]).Clip = new RectangleGeometry(rect);
		}
	}

	internal static void UpdateFloatingElementVisuals(ContainerVisual visual, List<BaseParaClient> floatingElementList)
	{
		VisualCollection children = visual.Children;
		int num = 0;
		if (floatingElementList == null || floatingElementList.Count == 0)
		{
			children.Clear();
			return;
		}
		for (int i = 0; i < floatingElementList.Count; i++)
		{
			Visual visual2 = floatingElementList[i].Visual;
			while (num < children.Count && children[num] != visual2)
			{
				children.RemoveAt(num);
			}
			if (num == children.Count)
			{
				children.Add(visual2);
			}
			num++;
		}
		if (children.Count > floatingElementList.Count)
		{
			children.RemoveRange(floatingElementList.Count, children.Count - floatingElementList.Count);
		}
	}

	internal static void ArrangeTrack(PtsContext ptsContext, ref PTS.FSTRACKDESCRIPTION trackDesc, uint fswdirTrack)
	{
		if (trackDesc.pfstrack != IntPtr.Zero)
		{
			PTS.Validate(PTS.FsQueryTrackDetails(ptsContext.Context, trackDesc.pfstrack, out var pTrackDetails));
			if (pTrackDetails.cParas != 0)
			{
				ParaListFromTrack(ptsContext, trackDesc.pfstrack, ref pTrackDetails, out var arrayParaDesc);
				ArrangeParaList(ptsContext, trackDesc.fsrc, arrayParaDesc, fswdirTrack);
			}
		}
	}

	internal static void ArrangeParaList(PtsContext ptsContext, PTS.FSRECT rcTrackContent, PTS.FSPARADESCRIPTION[] arrayParaDesc, uint fswdirTrack)
	{
		int num = 0;
		for (int i = 0; i < arrayParaDesc.Length; i++)
		{
			BaseParaClient baseParaClient = ptsContext.HandleToObject(arrayParaDesc[i].pfsparaclient) as BaseParaClient;
			PTS.ValidateHandle(baseParaClient);
			if (i == 0)
			{
				uint num2 = PTS.FlowDirectionToFswdir(baseParaClient.PageFlowDirection);
				if (fswdirTrack != num2)
				{
					PTS.FSRECT rectPage = baseParaClient.Paragraph.StructuralCache.CurrentArrangeContext.PageContext.PageRect;
					PTS.Validate(PTS.FsTransformRectangle(fswdirTrack, ref rectPage, ref rcTrackContent, num2, out rcTrackContent));
				}
			}
			int dvrTopSpace = arrayParaDesc[i].dvrTopSpace;
			PTS.FSRECT rcPara = rcTrackContent;
			rcPara.v += num + dvrTopSpace;
			rcPara.dv = arrayParaDesc[i].dvrUsed - dvrTopSpace;
			baseParaClient.Arrange(arrayParaDesc[i].pfspara, rcPara, dvrTopSpace, fswdirTrack);
			num += arrayParaDesc[i].dvrUsed;
		}
	}

	internal static void UpdateTrackVisuals(PtsContext ptsContext, VisualCollection visualCollection, PTS.FSKUPDATE fskupdInherited, ref PTS.FSTRACKDESCRIPTION trackDesc)
	{
		PTS.FSKUPDATE fSKUPDATE = trackDesc.fsupdinf.fskupd;
		if (trackDesc.fsupdinf.fskupd == PTS.FSKUPDATE.fskupdInherited)
		{
			fSKUPDATE = fskupdInherited;
		}
		if (fSKUPDATE == PTS.FSKUPDATE.fskupdNoChange)
		{
			return;
		}
		ErrorHandler.Assert(fSKUPDATE != PTS.FSKUPDATE.fskupdShifted, ErrorHandler.UpdateShiftedNotValid);
		bool flag = trackDesc.pfstrack == IntPtr.Zero;
		if (!flag)
		{
			PTS.Validate(PTS.FsQueryTrackDetails(ptsContext.Context, trackDesc.pfstrack, out var pTrackDetails));
			flag = pTrackDetails.cParas == 0;
			if (!flag)
			{
				ParaListFromTrack(ptsContext, trackDesc.pfstrack, ref pTrackDetails, out var arrayParaDesc);
				UpdateParaListVisuals(ptsContext, visualCollection, fSKUPDATE, arrayParaDesc);
			}
		}
		if (flag)
		{
			visualCollection.Clear();
		}
	}

	internal static void UpdateParaListVisuals(PtsContext ptsContext, VisualCollection visualCollection, PTS.FSKUPDATE fskupdInherited, PTS.FSPARADESCRIPTION[] arrayParaDesc)
	{
		for (int i = 0; i < arrayParaDesc.Length; i++)
		{
			BaseParaClient baseParaClient = ptsContext.HandleToObject(arrayParaDesc[i].pfsparaclient) as BaseParaClient;
			PTS.ValidateHandle(baseParaClient);
			PTS.FSKUPDATE fSKUPDATE = arrayParaDesc[i].fsupdinf.fskupd;
			if (fSKUPDATE == PTS.FSKUPDATE.fskupdInherited)
			{
				fSKUPDATE = fskupdInherited;
			}
			if (fSKUPDATE == PTS.FSKUPDATE.fskupdNew)
			{
				if (VisualTreeHelper.GetParent(baseParaClient.Visual) is Visual visual)
				{
					ContainerVisual obj = visual as ContainerVisual;
					Invariant.Assert(obj != null, "parent should always derives from ContainerVisual");
					obj.Children.Remove(baseParaClient.Visual);
				}
				visualCollection.Insert(i, baseParaClient.Visual);
				baseParaClient.ValidateVisual(fSKUPDATE);
			}
			else
			{
				while (visualCollection[i] != baseParaClient.Visual)
				{
					visualCollection.RemoveAt(i);
					Invariant.Assert(i < visualCollection.Count);
				}
				if (fSKUPDATE == PTS.FSKUPDATE.fskupdChangeInside || fSKUPDATE == PTS.FSKUPDATE.fskupdShifted)
				{
					baseParaClient.ValidateVisual(fSKUPDATE);
				}
			}
		}
		if (arrayParaDesc.Length < visualCollection.Count)
		{
			visualCollection.RemoveRange(arrayParaDesc.Length, visualCollection.Count - arrayParaDesc.Length);
		}
	}

	internal static void UpdateViewportTrack(PtsContext ptsContext, ref PTS.FSTRACKDESCRIPTION trackDesc, ref PTS.FSRECT viewport)
	{
		if (trackDesc.pfstrack != IntPtr.Zero)
		{
			PTS.Validate(PTS.FsQueryTrackDetails(ptsContext.Context, trackDesc.pfstrack, out var pTrackDetails));
			if (pTrackDetails.cParas != 0)
			{
				ParaListFromTrack(ptsContext, trackDesc.pfstrack, ref pTrackDetails, out var arrayParaDesc);
				UpdateViewportParaList(ptsContext, arrayParaDesc, ref viewport);
			}
		}
	}

	internal static void UpdateViewportParaList(PtsContext ptsContext, PTS.FSPARADESCRIPTION[] arrayParaDesc, ref PTS.FSRECT viewport)
	{
		for (int i = 0; i < arrayParaDesc.Length; i++)
		{
			BaseParaClient obj = ptsContext.HandleToObject(arrayParaDesc[i].pfsparaclient) as BaseParaClient;
			PTS.ValidateHandle(obj);
			obj.UpdateViewport(ref viewport);
		}
	}

	internal static IInputElement InputHitTestTrack(PtsContext ptsContext, PTS.FSPOINT pt, ref PTS.FSTRACKDESCRIPTION trackDesc)
	{
		if (trackDesc.pfstrack == IntPtr.Zero)
		{
			return null;
		}
		IInputElement result = null;
		PTS.Validate(PTS.FsQueryTrackDetails(ptsContext.Context, trackDesc.pfstrack, out var pTrackDetails));
		if (pTrackDetails.cParas != 0)
		{
			ParaListFromTrack(ptsContext, trackDesc.pfstrack, ref pTrackDetails, out var arrayParaDesc);
			result = InputHitTestParaList(ptsContext, pt, ref trackDesc.fsrc, arrayParaDesc);
		}
		return result;
	}

	internal static IInputElement InputHitTestParaList(PtsContext ptsContext, PTS.FSPOINT pt, ref PTS.FSRECT rcTrack, PTS.FSPARADESCRIPTION[] arrayParaDesc)
	{
		IInputElement inputElement = null;
		for (int i = 0; i < arrayParaDesc.Length; i++)
		{
			if (inputElement != null)
			{
				break;
			}
			BaseParaClient baseParaClient = ptsContext.HandleToObject(arrayParaDesc[i].pfsparaclient) as BaseParaClient;
			PTS.ValidateHandle(baseParaClient);
			if (baseParaClient.Rect.Contains(pt))
			{
				inputElement = baseParaClient.InputHitTest(pt);
			}
		}
		return inputElement;
	}

	internal static List<Rect> GetRectanglesInTrack(PtsContext ptsContext, ContentElement e, int start, int length, ref PTS.FSTRACKDESCRIPTION trackDesc)
	{
		List<Rect> result = new List<Rect>();
		if (trackDesc.pfstrack == IntPtr.Zero)
		{
			return result;
		}
		PTS.Validate(PTS.FsQueryTrackDetails(ptsContext.Context, trackDesc.pfstrack, out var pTrackDetails));
		if (pTrackDetails.cParas != 0)
		{
			ParaListFromTrack(ptsContext, trackDesc.pfstrack, ref pTrackDetails, out var arrayParaDesc);
			result = GetRectanglesInParaList(ptsContext, e, start, length, arrayParaDesc);
		}
		return result;
	}

	internal static List<Rect> GetRectanglesInParaList(PtsContext ptsContext, ContentElement e, int start, int length, PTS.FSPARADESCRIPTION[] arrayParaDesc)
	{
		List<Rect> list = new List<Rect>();
		for (int i = 0; i < arrayParaDesc.Length; i++)
		{
			BaseParaClient baseParaClient = ptsContext.HandleToObject(arrayParaDesc[i].pfsparaclient) as BaseParaClient;
			PTS.ValidateHandle(baseParaClient);
			if (start < baseParaClient.Paragraph.ParagraphEndCharacterPosition)
			{
				list = baseParaClient.GetRectangles(e, start, length);
				Invariant.Assert(list != null);
				if (list.Count != 0)
				{
					break;
				}
			}
		}
		return list;
	}

	internal static List<Rect> OffsetRectangleList(List<Rect> rectangleList, double xOffset, double yOffset)
	{
		List<Rect> list = new List<Rect>(rectangleList.Count);
		for (int i = 0; i < rectangleList.Count; i++)
		{
			Rect item = rectangleList[i];
			item.X += xOffset;
			item.Y += yOffset;
			list.Add(item);
		}
		return list;
	}

	internal unsafe static void SectionListFromPage(PtsContext ptsContext, nint page, ref PTS.FSPAGEDETAILS pageDetails, out PTS.FSSECTIONDESCRIPTION[] arraySectionDesc)
	{
		arraySectionDesc = new PTS.FSSECTIONDESCRIPTION[pageDetails.u.complex.cSections];
		int cActualSize;
		fixed (PTS.FSSECTIONDESCRIPTION* rgSectionDescription = arraySectionDesc)
		{
			PTS.Validate(PTS.FsQueryPageSectionList(ptsContext.Context, page, pageDetails.u.complex.cSections, rgSectionDescription, out cActualSize));
		}
		ErrorHandler.Assert(pageDetails.u.complex.cSections == cActualSize, ErrorHandler.PTSObjectsCountMismatch);
	}

	internal unsafe static void TrackListFromSubpage(PtsContext ptsContext, nint subpage, ref PTS.FSSUBPAGEDETAILS subpageDetails, out PTS.FSTRACKDESCRIPTION[] arrayTrackDesc)
	{
		arrayTrackDesc = new PTS.FSTRACKDESCRIPTION[subpageDetails.u.complex.cBasicColumns];
		int cActualSize;
		fixed (PTS.FSTRACKDESCRIPTION* rgColumnDescription = arrayTrackDesc)
		{
			PTS.Validate(PTS.FsQuerySubpageBasicColumnList(ptsContext.Context, subpage, subpageDetails.u.complex.cBasicColumns, rgColumnDescription, out cActualSize));
		}
		ErrorHandler.Assert(subpageDetails.u.complex.cBasicColumns == cActualSize, ErrorHandler.PTSObjectsCountMismatch);
	}

	internal unsafe static void TrackListFromSection(PtsContext ptsContext, nint section, ref PTS.FSSECTIONDETAILS sectionDetails, out PTS.FSTRACKDESCRIPTION[] arrayTrackDesc)
	{
		arrayTrackDesc = new PTS.FSTRACKDESCRIPTION[sectionDetails.u.withpagenotes.cBasicColumns];
		int cActualSize;
		fixed (PTS.FSTRACKDESCRIPTION* rgColumnDescription = arrayTrackDesc)
		{
			PTS.Validate(PTS.FsQuerySectionBasicColumnList(ptsContext.Context, section, sectionDetails.u.withpagenotes.cBasicColumns, rgColumnDescription, out cActualSize));
		}
		ErrorHandler.Assert(sectionDetails.u.withpagenotes.cBasicColumns == cActualSize, ErrorHandler.PTSObjectsCountMismatch);
	}

	internal unsafe static void ParaListFromTrack(PtsContext ptsContext, nint track, ref PTS.FSTRACKDETAILS trackDetails, out PTS.FSPARADESCRIPTION[] arrayParaDesc)
	{
		arrayParaDesc = new PTS.FSPARADESCRIPTION[trackDetails.cParas];
		int cParaDesc;
		fixed (PTS.FSPARADESCRIPTION* rgParaDesc = arrayParaDesc)
		{
			PTS.Validate(PTS.FsQueryTrackParaList(ptsContext.Context, track, trackDetails.cParas, rgParaDesc, out cParaDesc));
		}
		ErrorHandler.Assert(trackDetails.cParas == cParaDesc, ErrorHandler.PTSObjectsCountMismatch);
	}

	internal unsafe static void ParaListFromSubtrack(PtsContext ptsContext, nint subtrack, ref PTS.FSSUBTRACKDETAILS subtrackDetails, out PTS.FSPARADESCRIPTION[] arrayParaDesc)
	{
		arrayParaDesc = new PTS.FSPARADESCRIPTION[subtrackDetails.cParas];
		int cParaDesc;
		fixed (PTS.FSPARADESCRIPTION* rgParaDesc = arrayParaDesc)
		{
			PTS.Validate(PTS.FsQuerySubtrackParaList(ptsContext.Context, subtrack, subtrackDetails.cParas, rgParaDesc, out cParaDesc));
		}
		ErrorHandler.Assert(subtrackDetails.cParas == cParaDesc, ErrorHandler.PTSObjectsCountMismatch);
	}

	internal unsafe static void LineListSimpleFromTextPara(PtsContext ptsContext, nint para, ref PTS.FSTEXTDETAILSFULL textDetails, out PTS.FSLINEDESCRIPTIONSINGLE[] arrayLineDesc)
	{
		arrayLineDesc = new PTS.FSLINEDESCRIPTIONSINGLE[textDetails.cLines];
		int cLineDesc;
		fixed (PTS.FSLINEDESCRIPTIONSINGLE* rgLineDesc = arrayLineDesc)
		{
			PTS.Validate(PTS.FsQueryLineListSingle(ptsContext.Context, para, textDetails.cLines, rgLineDesc, out cLineDesc));
		}
		ErrorHandler.Assert(textDetails.cLines == cLineDesc, ErrorHandler.PTSObjectsCountMismatch);
	}

	internal unsafe static void LineListCompositeFromTextPara(PtsContext ptsContext, nint para, ref PTS.FSTEXTDETAILSFULL textDetails, out PTS.FSLINEDESCRIPTIONCOMPOSITE[] arrayLineDesc)
	{
		arrayLineDesc = new PTS.FSLINEDESCRIPTIONCOMPOSITE[textDetails.cLines];
		int cLineElements;
		fixed (PTS.FSLINEDESCRIPTIONCOMPOSITE* rgLineDescription = arrayLineDesc)
		{
			PTS.Validate(PTS.FsQueryLineListComposite(ptsContext.Context, para, textDetails.cLines, rgLineDescription, out cLineElements));
		}
		ErrorHandler.Assert(textDetails.cLines == cLineElements, ErrorHandler.PTSObjectsCountMismatch);
	}

	internal unsafe static void LineElementListFromCompositeLine(PtsContext ptsContext, ref PTS.FSLINEDESCRIPTIONCOMPOSITE lineDesc, out PTS.FSLINEELEMENT[] arrayLineElement)
	{
		arrayLineElement = new PTS.FSLINEELEMENT[lineDesc.cElements];
		int cLineElements;
		fixed (PTS.FSLINEELEMENT* rgLineElement = arrayLineElement)
		{
			PTS.Validate(PTS.FsQueryLineCompositeElementList(ptsContext.Context, lineDesc.pline, lineDesc.cElements, rgLineElement, out cLineElements));
		}
		ErrorHandler.Assert(lineDesc.cElements == cLineElements, ErrorHandler.PTSObjectsCountMismatch);
	}

	internal unsafe static void AttachedObjectListFromParagraph(PtsContext ptsContext, nint para, int cAttachedObject, out PTS.FSATTACHEDOBJECTDESCRIPTION[] arrayAttachedObjectDesc)
	{
		arrayAttachedObjectDesc = new PTS.FSATTACHEDOBJECTDESCRIPTION[cAttachedObject];
		int cAttachedObjectDesc;
		fixed (PTS.FSATTACHEDOBJECTDESCRIPTION* rgAttachedObjects = arrayAttachedObjectDesc)
		{
			PTS.Validate(PTS.FsQueryAttachedObjectList(ptsContext.Context, para, cAttachedObject, rgAttachedObjects, out cAttachedObjectDesc));
		}
		ErrorHandler.Assert(cAttachedObject == cAttachedObjectDesc, ErrorHandler.PTSObjectsCountMismatch);
	}

	internal static TextContentRange TextContentRangeFromTrack(PtsContext ptsContext, nint pfstrack)
	{
		PTS.Validate(PTS.FsQueryTrackDetails(ptsContext.Context, pfstrack, out var pTrackDetails));
		TextContentRange textContentRange = new TextContentRange();
		if (pTrackDetails.cParas != 0)
		{
			ParaListFromTrack(ptsContext, pfstrack, ref pTrackDetails, out var arrayParaDesc);
			for (int i = 0; i < arrayParaDesc.Length; i++)
			{
				BaseParaClient baseParaClient = ptsContext.HandleToObject(arrayParaDesc[i].pfsparaclient) as BaseParaClient;
				PTS.ValidateHandle(baseParaClient);
				textContentRange.Merge(baseParaClient.GetTextContentRange());
			}
		}
		return textContentRange;
	}

	internal static double CalculatePageMarginAdjustment(StructuralCache structuralCache, double pageMarginWidth)
	{
		double result = 0.0;
		DependencyObject element = structuralCache.Section.Element;
		if (element is FlowDocument)
		{
			ColumnPropertiesGroup columnPropertiesGroup = new ColumnPropertiesGroup(element);
			if (!columnPropertiesGroup.IsColumnWidthFlexible)
			{
				double lineHeight = DynamicPropertyReader.GetLineHeightValue(element);
				double pageFontSize = (double)structuralCache.PropertyOwner.GetValue(TextElement.FontSizeProperty);
				FontFamily pageFontFamily = (FontFamily)structuralCache.PropertyOwner.GetValue(TextElement.FontFamilyProperty);
				int cColumns = CalculateColumnCount(columnPropertiesGroup, lineHeight, pageMarginWidth, pageFontSize, pageFontFamily, enableColumns: true);
				GetColumnMetrics(columnPropertiesGroup, pageMarginWidth, pageFontSize, pageFontFamily, enableColumns: true, cColumns, ref lineHeight, out var _, out var freeSpace, out var _);
				result = freeSpace;
			}
		}
		return result;
	}

	internal static int CalculateColumnCount(ColumnPropertiesGroup columnProperties, double lineHeight, double pageWidth, double pageFontSize, FontFamily pageFontFamily, bool enableColumns)
	{
		int val = 1;
		_ = columnProperties.ColumnRuleWidth;
		if (enableColumns)
		{
			double num = ((!columnProperties.ColumnGapAuto) ? columnProperties.ColumnGap : (1.0 * lineHeight));
			if (!columnProperties.ColumnWidthAuto)
			{
				double columnWidth = columnProperties.ColumnWidth;
				val = (int)((pageWidth + num) / (columnWidth + num));
			}
			else
			{
				double num2 = 20.0 * pageFontSize;
				val = (int)((pageWidth + num) / (num2 + num));
			}
		}
		return Math.Max(1, Math.Min(999, val));
	}

	internal static void GetColumnMetrics(ColumnPropertiesGroup columnProperties, double pageWidth, double pageFontSize, FontFamily pageFontFamily, bool enableColumns, int cColumns, ref double lineHeight, out double columnWidth, out double freeSpace, out double gapSpace)
	{
		_ = columnProperties.ColumnRuleWidth;
		if (!enableColumns)
		{
			Invariant.Assert(cColumns == 1);
			columnWidth = pageWidth;
			gapSpace = 0.0;
			lineHeight = 0.0;
			freeSpace = 0.0;
		}
		else
		{
			if (columnProperties.ColumnWidthAuto)
			{
				columnWidth = 20.0 * pageFontSize;
			}
			else
			{
				columnWidth = columnProperties.ColumnWidth;
			}
			if (columnProperties.ColumnGapAuto)
			{
				gapSpace = 1.0 * lineHeight;
			}
			else
			{
				gapSpace = columnProperties.ColumnGap;
			}
		}
		columnWidth = Math.Max(1.0, Math.Min(columnWidth, pageWidth));
		freeSpace = pageWidth - (double)cColumns * columnWidth - (double)(cColumns - 1) * gapSpace;
		freeSpace = Math.Max(0.0, freeSpace);
	}

	internal unsafe static void GetColumnsInfo(ColumnPropertiesGroup columnProperties, double lineHeight, double pageWidth, double pageFontSize, FontFamily pageFontFamily, int cColumns, PTS.FSCOLUMNINFO* pfscolinfo, bool enableColumns)
	{
		_ = columnProperties.ColumnRuleWidth;
		GetColumnMetrics(columnProperties, pageWidth, pageFontSize, pageFontFamily, enableColumns, cColumns, ref lineHeight, out var columnWidth, out var freeSpace, out var gapSpace);
		if (!columnProperties.IsColumnWidthFlexible)
		{
			for (int i = 0; i < cColumns; i++)
			{
				pfscolinfo[i].durBefore = TextDpi.ToTextDpi((i == 0) ? 0.0 : gapSpace);
				pfscolinfo[i].durWidth = TextDpi.ToTextDpi(columnWidth);
				pfscolinfo[i].durBefore = Math.Max(0, pfscolinfo[i].durBefore);
				pfscolinfo[i].durWidth = Math.Max(1, pfscolinfo[i].durWidth);
			}
			return;
		}
		for (int j = 0; j < cColumns; j++)
		{
			if (columnProperties.ColumnSpaceDistribution == ColumnSpaceDistribution.Right)
			{
				pfscolinfo[j].durWidth = TextDpi.ToTextDpi((j == cColumns - 1) ? (columnWidth + freeSpace) : columnWidth);
			}
			else if (columnProperties.ColumnSpaceDistribution == ColumnSpaceDistribution.Left)
			{
				pfscolinfo[j].durWidth = TextDpi.ToTextDpi((j == 0) ? (columnWidth + freeSpace) : columnWidth);
			}
			else
			{
				pfscolinfo[j].durWidth = TextDpi.ToTextDpi(columnWidth + freeSpace / (double)cColumns);
			}
			if (pfscolinfo[j].durWidth > TextDpi.ToTextDpi(pageWidth))
			{
				pfscolinfo[j].durWidth = TextDpi.ToTextDpi(pageWidth);
			}
			pfscolinfo[j].durBefore = TextDpi.ToTextDpi((j == 0) ? 0.0 : gapSpace);
			pfscolinfo[j].durBefore = Math.Max(0, pfscolinfo[j].durBefore);
			pfscolinfo[j].durWidth = Math.Max(1, pfscolinfo[j].durWidth);
		}
	}
}
