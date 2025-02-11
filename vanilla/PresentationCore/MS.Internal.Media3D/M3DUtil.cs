using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MS.Utility;

namespace MS.Internal.Media3D;

internal static class M3DUtil
{
	internal static Point3D Interpolate(ref Point3D v0, ref Point3D v1, ref Point3D v2, ref Point barycentric)
	{
		double x = barycentric.X;
		double y = barycentric.Y;
		double num = 1.0 - x - y;
		return new Point3D(num * v0.X + x * v1.X + y * v2.X, num * v0.Y + x * v1.Y + y * v2.Y, num * v0.Z + x * v1.Z + y * v2.Z);
	}

	private static void AddPointToBounds(ref Point3D point, ref Rect3D bounds)
	{
		if (point.X < bounds.X)
		{
			bounds.SizeX += bounds.X - point.X;
			bounds.X = point.X;
		}
		else if (point.X > bounds.X + bounds.SizeX)
		{
			bounds.SizeX = point.X - bounds.X;
		}
		if (point.Y < bounds.Y)
		{
			bounds.SizeY += bounds.Y - point.Y;
			bounds.Y = point.Y;
		}
		else if (point.Y > bounds.Y + bounds.SizeY)
		{
			bounds.SizeY = point.Y - bounds.Y;
		}
		if (point.Z < bounds.Z)
		{
			bounds.SizeZ += bounds.Z - point.Z;
			bounds.Z = point.Z;
		}
		else if (point.Z > bounds.Z + bounds.SizeZ)
		{
			bounds.SizeZ = point.Z - bounds.Z;
		}
	}

	internal static Rect3D ComputeAxisAlignedBoundingBox(Point3DCollection positions)
	{
		if (positions != null)
		{
			FrugalStructList<Point3D> collection = positions._collection;
			if (collection.Count != 0)
			{
				Point3D point3D = collection[0];
				Rect3D bounds = new Rect3D(point3D.X, point3D.Y, point3D.Z, 0.0, 0.0, 0.0);
				for (int i = 1; i < collection.Count; i++)
				{
					point3D = collection[i];
					AddPointToBounds(ref point3D, ref bounds);
				}
				return bounds;
			}
		}
		return Rect3D.Empty;
	}

	internal static Rect3D ComputeTransformedAxisAlignedBoundingBox(ref Rect3D originalBox, Transform3D transform)
	{
		if (transform == null || transform == Transform3D.Identity)
		{
			return originalBox;
		}
		Matrix3D matrix = transform.Value;
		return ComputeTransformedAxisAlignedBoundingBox(ref originalBox, ref matrix);
	}

	internal static Rect3D ComputeTransformedAxisAlignedBoundingBox(ref Rect3D originalBox, ref Matrix3D matrix)
	{
		if (originalBox.IsEmpty)
		{
			return originalBox;
		}
		if (matrix.IsAffine)
		{
			return ComputeTransformedAxisAlignedBoundingBoxAffine(ref originalBox, ref matrix);
		}
		return ComputeTransformedAxisAlignedBoundingBoxNonAffine(ref originalBox, ref matrix);
	}

	internal static Rect3D ComputeTransformedAxisAlignedBoundingBoxAffine(ref Rect3D originalBox, ref Matrix3D matrix)
	{
		double num = originalBox.X + originalBox.SizeX;
		double num2 = originalBox.Y + originalBox.SizeY;
		double num3 = originalBox.Z + originalBox.SizeZ;
		double offsetX = matrix.OffsetX;
		double offsetX2 = matrix.OffsetX;
		double num4 = matrix.M11 * originalBox.X;
		double num5 = matrix.M11 * num;
		if (num5 > num4)
		{
			offsetX += num4;
			offsetX2 += num5;
		}
		else
		{
			offsetX += num5;
			offsetX2 += num4;
		}
		num4 = matrix.M21 * originalBox.Y;
		num5 = matrix.M21 * num2;
		if (num5 > num4)
		{
			offsetX += num4;
			offsetX2 += num5;
		}
		else
		{
			offsetX += num5;
			offsetX2 += num4;
		}
		num4 = matrix.M31 * originalBox.Z;
		num5 = matrix.M31 * num3;
		if (num5 > num4)
		{
			offsetX += num4;
			offsetX2 += num5;
		}
		else
		{
			offsetX += num5;
			offsetX2 += num4;
		}
		double offsetY = matrix.OffsetY;
		double offsetY2 = matrix.OffsetY;
		double num6 = matrix.M12 * originalBox.X;
		double num7 = matrix.M12 * num;
		if (num7 > num6)
		{
			offsetY += num6;
			offsetY2 += num7;
		}
		else
		{
			offsetY += num7;
			offsetY2 += num6;
		}
		num6 = matrix.M22 * originalBox.Y;
		num7 = matrix.M22 * num2;
		if (num7 > num6)
		{
			offsetY += num6;
			offsetY2 += num7;
		}
		else
		{
			offsetY += num7;
			offsetY2 += num6;
		}
		num6 = matrix.M32 * originalBox.Z;
		num7 = matrix.M32 * num3;
		if (num7 > num6)
		{
			offsetY += num6;
			offsetY2 += num7;
		}
		else
		{
			offsetY += num7;
			offsetY2 += num6;
		}
		double offsetZ = matrix.OffsetZ;
		double offsetZ2 = matrix.OffsetZ;
		double num8 = matrix.M13 * originalBox.X;
		double num9 = matrix.M13 * num;
		if (num9 > num8)
		{
			offsetZ += num8;
			offsetZ2 += num9;
		}
		else
		{
			offsetZ += num9;
			offsetZ2 += num8;
		}
		num8 = matrix.M23 * originalBox.Y;
		num9 = matrix.M23 * num2;
		if (num9 > num8)
		{
			offsetZ += num8;
			offsetZ2 += num9;
		}
		else
		{
			offsetZ += num9;
			offsetZ2 += num8;
		}
		num8 = matrix.M33 * originalBox.Z;
		num9 = matrix.M33 * num3;
		if (num9 > num8)
		{
			offsetZ += num8;
			offsetZ2 += num9;
		}
		else
		{
			offsetZ += num9;
			offsetZ2 += num8;
		}
		return new Rect3D(offsetX, offsetY, offsetZ, offsetX2 - offsetX, offsetY2 - offsetY, offsetZ2 - offsetZ);
	}

	internal static Rect3D ComputeTransformedAxisAlignedBoundingBoxNonAffine(ref Rect3D originalBox, ref Matrix3D matrix)
	{
		double x = originalBox.X;
		double y = originalBox.Y;
		double z = originalBox.Z;
		double x2 = originalBox.X + originalBox.SizeX;
		double y2 = originalBox.Y + originalBox.SizeY;
		double z2 = originalBox.Z + originalBox.SizeZ;
		Point3D[] array = new Point3D[8]
		{
			new Point3D(x, y, z),
			new Point3D(x, y, z2),
			new Point3D(x, y2, z),
			new Point3D(x, y2, z2),
			new Point3D(x2, y, z),
			new Point3D(x2, y, z2),
			new Point3D(x2, y2, z),
			new Point3D(x2, y2, z2)
		};
		matrix.Transform(array);
		Point3D point3D = array[0];
		Rect3D bounds = new Rect3D(point3D.X, point3D.Y, point3D.Z, 0.0, 0.0, 0.0);
		for (int i = 1; i < array.Length; i++)
		{
			point3D = array[i];
			AddPointToBounds(ref point3D, ref bounds);
		}
		return bounds;
	}

	internal static double GetAspectRatio(Size viewSize)
	{
		return viewSize.Width / viewSize.Height;
	}

	internal static Point GetNormalizedPoint(Point point, Size size)
	{
		return new Point(2.0 * point.X / size.Width - 1.0, 0.0 - (2.0 * point.Y / size.Height - 1.0));
	}

	internal static double RadiansToDegrees(double radians)
	{
		return radians * (180.0 / Math.PI);
	}

	internal static double DegreesToRadians(double degrees)
	{
		return degrees * (Math.PI / 180.0);
	}

	internal static Matrix3D GetWorldToViewportTransform3D(Camera camera, Rect viewport)
	{
		return camera.GetViewMatrix() * camera.GetProjectionMatrix(GetAspectRatio(viewport.Size)) * GetHomogeneousToViewportTransform3D(viewport);
	}

	internal static Matrix3D GetHomogeneousToViewportTransform3D(Rect viewport)
	{
		double num = viewport.Width / 2.0;
		double num2 = viewport.Height / 2.0;
		double offsetX = viewport.X + num;
		double offsetY = viewport.Y + num2;
		return new Matrix3D(num, 0.0, 0.0, 0.0, 0.0, 0.0 - num2, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, offsetX, offsetY, 0.0, 1.0);
	}

	internal static Matrix GetHomogeneousToViewportTransform(Rect viewport)
	{
		double num = viewport.Width / 2.0;
		double num2 = viewport.Height / 2.0;
		double offsetX = viewport.X + num;
		double offsetY = viewport.Y + num2;
		return new Matrix(num, 0.0, 0.0, 0.0 - num2, offsetX, offsetY);
	}

	internal static Matrix3D GetWorldTransformationMatrix(Visual3D visual)
	{
		Viewport3DVisual viewport;
		return GetWorldTransformationMatrix(visual, out viewport);
	}

	internal static Matrix3D GetWorldTransformationMatrix(Visual3D visual3DStart, out Viewport3DVisual viewport)
	{
		DependencyObject dependencyObject = visual3DStart;
		Matrix3D matrix = Matrix3D.Identity;
		while (dependencyObject != null && dependencyObject is Visual3D visual3D)
		{
			((Transform3D)visual3D.GetValue(Visual3D.TransformProperty))?.Append(ref matrix);
			dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
		}
		if (dependencyObject != null)
		{
			viewport = (Viewport3DVisual)dependencyObject;
		}
		else
		{
			viewport = null;
		}
		return matrix;
	}

	internal static bool TryTransformToViewport3DVisual(Visual3D visual3D, out Viewport3DVisual viewport, out Matrix3D matrix)
	{
		matrix = GetWorldTransformationMatrix(visual3D, out viewport);
		if (viewport != null)
		{
			matrix *= GetWorldToViewportTransform3D(viewport.Camera, viewport.Viewport);
			return true;
		}
		return false;
	}

	internal static bool IsPointInTriangle(Point p, Point[] triUVVertices, Point3D[] tri3DVertices, out Point3D inters3DPoint)
	{
		double num = 0.0;
		inters3DPoint = default(Point3D);
		double num2 = triUVVertices[0].X - triUVVertices[2].X;
		double num3 = triUVVertices[1].X - triUVVertices[2].X;
		double num4 = triUVVertices[2].X - p.X;
		double num5 = triUVVertices[0].Y - triUVVertices[2].Y;
		double num6 = triUVVertices[1].Y - triUVVertices[2].Y;
		double num7 = triUVVertices[2].Y - p.Y;
		num = num2 * num6 - num3 * num5;
		if (num == 0.0)
		{
			return false;
		}
		double num8 = (num3 * num7 - num4 * num6) / num;
		num = num3 * num5 - num2 * num6;
		if (num == 0.0)
		{
			return false;
		}
		double num9 = (num2 * num7 - num4 * num5) / num;
		if (num8 < 0.0 || num8 > 1.0 || num9 < 0.0 || num9 > 1.0 || num8 + num9 > 1.0)
		{
			return false;
		}
		inters3DPoint = (Point3D)(num8 * (Vector3D)tri3DVertices[0] + num9 * (Vector3D)tri3DVertices[1] + (1.0 - num8 - num9) * (Vector3D)tri3DVertices[2]);
		return true;
	}
}
