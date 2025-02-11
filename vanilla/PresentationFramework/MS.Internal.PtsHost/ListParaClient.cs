using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal sealed class ListParaClient : ContainerParaClient
{
	internal ListParaClient(ListParagraph paragraph)
		: base(paragraph)
	{
	}

	internal override void ValidateVisual(PTS.FSKUPDATE fskupdInherited)
	{
		PTS.Validate(PTS.FsQuerySubtrackDetails(base.PtsContext.Context, _paraHandle.Value, out var pSubTrackDetails));
		MbpInfo mbpInfo = MbpInfo.FromElement(base.Paragraph.Element, base.Paragraph.StructuralCache.TextFormatterHost.PixelsPerDip);
		if (base.ThisFlowDirection != base.PageFlowDirection)
		{
			mbpInfo.MirrorBP();
		}
		PTS.FlowDirectionToFswdir((FlowDirection)base.Paragraph.Element.GetValue(FrameworkElement.FlowDirectionProperty));
		Brush backgroundBrush = (Brush)base.Paragraph.Element.GetValue(TextElement.BackgroundProperty);
		TextProperties defaultTextProperties = new TextProperties(base.Paragraph.Element, StaticTextPointer.Null, inlineObjects: false, getBackground: false, base.Paragraph.StructuralCache.TextFormatterHost.PixelsPerDip);
		if (pSubTrackDetails.cParas != 0)
		{
			PtsHelper.ParaListFromSubtrack(base.PtsContext, _paraHandle.Value, ref pSubTrackDetails, out var arrayParaDesc);
			using (DrawingContext drawingContext = _visual.RenderOpen())
			{
				_visual.DrawBackgroundAndBorderIntoContext(drawingContext, backgroundBrush, mbpInfo.BorderBrush, mbpInfo.Border, _rect.FromTextDpi(), IsFirstChunk, IsLastChunk);
				ListMarkerLine listMarkerLine = new ListMarkerLine(base.Paragraph.StructuralCache.TextFormatterHost, this);
				int num = 0;
				for (int i = 0; i < pSubTrackDetails.cParas; i++)
				{
					List list = base.Paragraph.Element as List;
					BaseParaClient baseParaClient = base.PtsContext.HandleToObject(arrayParaDesc[i].pfsparaclient) as BaseParaClient;
					PTS.ValidateHandle(baseParaClient);
					if (i == 0)
					{
						num = list.GetListItemIndex(baseParaClient.Paragraph.Element as ListItem);
					}
					if (baseParaClient.IsFirstChunk)
					{
						int firstTextLineBaseline = baseParaClient.GetFirstTextLineBaseline();
						if (base.PageFlowDirection != base.ThisFlowDirection)
						{
							drawingContext.PushTransform(new MatrixTransform(-1.0, 0.0, 0.0, 1.0, TextDpi.FromTextDpi(2 * baseParaClient.Rect.u + baseParaClient.Rect.du), 0.0));
						}
						int index = ((int.MaxValue - i >= num) ? (num + i) : int.MaxValue);
						LineProperties lineProps = new LineProperties(base.Paragraph.Element, base.Paragraph.StructuralCache.FormattingOwner, defaultTextProperties, new MarkerProperties(list, index));
						listMarkerLine.FormatAndDrawVisual(drawingContext, lineProps, baseParaClient.Rect.u, firstTextLineBaseline);
						if (base.PageFlowDirection != base.ThisFlowDirection)
						{
							drawingContext.Pop();
						}
					}
				}
				listMarkerLine.Dispose();
			}
			PtsHelper.UpdateParaListVisuals(base.PtsContext, _visual.Children, fskupdInherited, arrayParaDesc);
		}
		else
		{
			_visual.Children.Clear();
		}
	}
}
