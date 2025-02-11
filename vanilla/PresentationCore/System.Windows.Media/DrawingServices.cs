namespace System.Windows.Media;

internal static class DrawingServices
{
	internal static bool HitTestPoint(Drawing drawing, Point point)
	{
		if (drawing != null)
		{
			HitTestDrawingContextWalker hitTestDrawingContextWalker = new HitTestWithPointDrawingContextWalker(point);
			drawing.WalkCurrentValue(hitTestDrawingContextWalker);
			return hitTestDrawingContextWalker.IsHit;
		}
		return false;
	}

	internal static IntersectionDetail HitTestGeometry(Drawing drawing, PathGeometry geometry)
	{
		if (drawing != null)
		{
			HitTestDrawingContextWalker hitTestDrawingContextWalker = new HitTestWithGeometryDrawingContextWalker(geometry);
			drawing.WalkCurrentValue(hitTestDrawingContextWalker);
			return hitTestDrawingContextWalker.IntersectionDetail;
		}
		return IntersectionDetail.Empty;
	}

	internal static DrawingGroup DrawingGroupFromRenderData(RenderData renderData)
	{
		DrawingGroup drawingGroup = new DrawingGroup();
		DrawingContext drawingContext = drawingGroup.Open();
		if (drawingContext is DrawingDrawingContext drawingDrawingContext)
		{
			drawingDrawingContext.CanBeInheritanceContext = false;
		}
		DrawingContextDrawingContextWalker ctx = new DrawingContextDrawingContextWalker(drawingContext);
		renderData.BaseValueDrawingContextWalk(ctx);
		drawingContext.Close();
		return drawingGroup;
	}
}
