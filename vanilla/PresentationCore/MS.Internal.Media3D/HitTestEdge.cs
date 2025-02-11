using System.Windows;
using System.Windows.Media.Media3D;

namespace MS.Internal.Media3D;

internal class HitTestEdge
{
	internal Point3D _p1;

	internal Point3D _p2;

	internal Point _uv1;

	internal Point _uv2;

	internal Point _p1Transformed;

	internal Point _p2Transformed;

	public HitTestEdge(Point3D p1, Point3D p2, Point uv1, Point uv2)
	{
		_p1 = p1;
		_p2 = p2;
		_uv1 = uv1;
		_uv2 = uv2;
	}

	public void Project(GeneralTransform3DTo2D objectToViewportTransform)
	{
		Point point = objectToViewportTransform.Transform(_p1);
		Point point2 = objectToViewportTransform.Transform(_p2);
		_p1Transformed = new Point(point.X, point.Y);
		_p2Transformed = new Point(point2.X, point2.Y);
	}
}
