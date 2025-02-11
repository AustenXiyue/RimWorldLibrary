using System.Collections;
using System.Windows.Media;

namespace System.Windows.Documents;

internal sealed class HighlightVisual : Adorner
{
	private FixedDocument _panel;

	private RubberbandSelector _rubberbandSelector;

	private FixedPage _page;

	internal HighlightVisual(FixedDocument panel, FixedPage page)
		: base(page)
	{
		_panel = panel;
		_page = page;
	}

	protected override GeometryHitTestResult HitTestCore(GeometryHitTestParameters hitTestParameters)
	{
		return null;
	}

	protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
	{
		return null;
	}

	protected override void OnRender(DrawingContext dc)
	{
		if (_panel.Highlights.ContainsKey(_page))
		{
			ArrayList arrayList = _panel.Highlights[_page];
			Size size = _panel.ComputePageSize(_page);
			Rect rect = new Rect(new Point(0.0, 0.0), size);
			dc.PushClip(new RectangleGeometry(rect));
			if (arrayList != null)
			{
				_UpdateHighlightBackground(dc, arrayList);
				_UpdateHighlightForeground(dc, arrayList);
			}
			dc.Pop();
		}
		if (_rubberbandSelector != null && _rubberbandSelector.Page == _page)
		{
			Rect selectionRect = _rubberbandSelector.SelectionRect;
			if (!selectionRect.IsEmpty)
			{
				dc.DrawRectangle(SelectionHighlightInfo.ObjectMaskBrush, null, selectionRect);
			}
		}
	}

	internal void InvalidateHighlights()
	{
		AdornerLayer.GetAdornerLayer(_page)?.Update(_page);
	}

	internal void UpdateRubberbandSelection(RubberbandSelector selector)
	{
		_rubberbandSelector = selector;
		InvalidateHighlights();
	}

	internal static HighlightVisual GetHighlightVisual(FixedPage page)
	{
		AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(page);
		if (adornerLayer == null)
		{
			return null;
		}
		Adorner[] adorners = adornerLayer.GetAdorners(page);
		if (adorners != null)
		{
			Adorner[] array = adorners;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] is HighlightVisual result)
				{
					return result;
				}
			}
		}
		return null;
	}

	private void _UpdateHighlightBackground(DrawingContext dc, ArrayList highlights)
	{
		PathGeometry pathGeometry = null;
		Brush brush = null;
		Rect rect = Rect.Empty;
		foreach (FixedHighlight highlight in highlights)
		{
			Brush brush2 = null;
			if (highlight.HighlightType == FixedHighlightType.None)
			{
				continue;
			}
			Rect rect2 = highlight.ComputeDesignRect();
			if (rect2 == Rect.Empty)
			{
				continue;
			}
			GeneralTransform generalTransform = highlight.Element.TransformToAncestor(_page);
			Transform transform = generalTransform.AffineTransform;
			if (transform == null)
			{
				transform = Transform.Identity;
			}
			Glyphs glyphs = highlight.Glyphs;
			if (highlight.HighlightType == FixedHighlightType.TextSelection)
			{
				brush2 = ((glyphs == null) ? SelectionHighlightInfo.ObjectMaskBrush : SelectionHighlightInfo.BackgroundBrush);
			}
			else if (highlight.HighlightType == FixedHighlightType.AnnotationHighlight)
			{
				brush2 = highlight.BackgroundBrush;
			}
			if (highlight.Element.Clip != null)
			{
				Rect bounds = highlight.Element.Clip.Bounds;
				rect2.Intersect(bounds);
			}
			Geometry geometry = new RectangleGeometry(rect2)
			{
				Transform = transform
			};
			rect2 = generalTransform.TransformBounds(rect2);
			if (brush2 != brush || rect2.Top > rect.Bottom + 0.1 || rect2.Bottom + 0.1 < rect.Top || rect2.Left > rect.Right + 0.1 || rect2.Right + 0.1 < rect.Left)
			{
				if (brush != null)
				{
					pathGeometry.FillRule = FillRule.Nonzero;
					dc.DrawGeometry(brush, null, pathGeometry);
				}
				brush = brush2;
				pathGeometry = new PathGeometry();
				pathGeometry.AddGeometry(geometry);
				rect = rect2;
			}
			else
			{
				pathGeometry.AddGeometry(geometry);
				rect.Union(rect2);
			}
		}
		if (brush != null)
		{
			pathGeometry.FillRule = FillRule.Nonzero;
			dc.DrawGeometry(brush, null, pathGeometry);
		}
	}

	private void _UpdateHighlightForeground(DrawingContext dc, ArrayList highlights)
	{
		foreach (FixedHighlight highlight in highlights)
		{
			Brush brush = null;
			if (highlight.HighlightType == FixedHighlightType.None)
			{
				continue;
			}
			Glyphs glyphs = highlight.Glyphs;
			if (glyphs == null)
			{
				continue;
			}
			Rect rect = highlight.ComputeDesignRect();
			if (!(rect == Rect.Empty))
			{
				Transform affineTransform = highlight.Element.TransformToAncestor(_page).AffineTransform;
				if (affineTransform != null)
				{
					dc.PushTransform(affineTransform);
				}
				else
				{
					dc.PushTransform(Transform.Identity);
				}
				dc.PushClip(new RectangleGeometry(rect));
				if (highlight.HighlightType == FixedHighlightType.TextSelection)
				{
					brush = SelectionHighlightInfo.ForegroundBrush;
				}
				else if (highlight.HighlightType == FixedHighlightType.AnnotationHighlight)
				{
					brush = highlight.ForegroundBrush;
				}
				GlyphRun glyphRun = glyphs.ToGlyphRun();
				if (brush == null)
				{
					brush = glyphs.Fill;
				}
				dc.PushGuidelineY1(glyphRun.BaselineOrigin.Y);
				dc.PushClip(glyphs.Clip);
				dc.DrawGlyphRun(brush, glyphRun);
				dc.Pop();
				dc.Pop();
				dc.Pop();
				dc.Pop();
			}
		}
	}
}
