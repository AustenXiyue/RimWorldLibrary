using System.Windows.Media.Composition;

namespace System.Windows.Media;

internal interface IDrawingContent : DUCE.IResource
{
	Rect GetContentBounds(BoundsDrawingContextWalker ctx);

	void WalkContent(DrawingContextWalker walker);

	bool HitTestPoint(Point point);

	IntersectionDetail HitTestGeometry(PathGeometry geometry);

	void PropagateChangedHandler(EventHandler handler, bool adding);
}
