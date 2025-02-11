using System.Windows.Media;
using MS.Internal;

namespace System.Windows.Ink;

/// <summary>Represents the tip of a stylus.</summary>
public abstract class StylusShape
{
	private double m_width;

	private double m_height;

	private double m_rotation;

	private Point[] m_vertices;

	private StylusTip m_tip;

	private Matrix _transform = Matrix.Identity;

	/// <summary>Gets the width of the stylus. </summary>
	/// <returns>The width of the stylus.</returns>
	public double Width => m_width;

	/// <summary>Gets the height of the stylus.</summary>
	/// <returns>The height of the stylus.</returns>
	public double Height => m_height;

	/// <summary>Gets the angle of the stylus.</summary>
	/// <returns>The angle of the stylus.</returns>
	public double Rotation => m_rotation;

	internal Matrix Transform
	{
		get
		{
			return _transform;
		}
		set
		{
			_transform = value;
		}
	}

	internal bool IsEllipse => m_vertices == null;

	internal bool IsPolygon => m_vertices != null;

	internal Rect BoundingBox
	{
		get
		{
			Rect result;
			if (IsPolygon)
			{
				result = Rect.Empty;
				Point[] vertices = m_vertices;
				foreach (Point point in vertices)
				{
					result.Union(point);
				}
			}
			else
			{
				result = new Rect(0.0 - m_width * 0.5, 0.0 - m_height * 0.5, m_width, m_height);
			}
			return result;
		}
	}

	internal StylusShape()
	{
	}

	internal StylusShape(StylusTip tip, double width, double height, double rotation)
	{
		if (double.IsNaN(width) || double.IsInfinity(width) || width < DrawingAttributes.MinWidth || width > DrawingAttributes.MaxWidth)
		{
			throw new ArgumentOutOfRangeException("width");
		}
		if (double.IsNaN(height) || double.IsInfinity(height) || height < DrawingAttributes.MinHeight || height > DrawingAttributes.MaxHeight)
		{
			throw new ArgumentOutOfRangeException("height");
		}
		if (double.IsNaN(rotation) || double.IsInfinity(rotation))
		{
			throw new ArgumentOutOfRangeException("rotation");
		}
		if (!StylusTipHelper.IsDefined(tip))
		{
			throw new ArgumentOutOfRangeException("tip");
		}
		m_width = width;
		m_height = height;
		m_rotation = ((rotation == 0.0) ? 0.0 : (rotation % 360.0));
		m_tip = tip;
		if (tip == StylusTip.Rectangle)
		{
			ComputeRectangleVertices();
		}
	}

	internal Vector[] GetVerticesAsVectors()
	{
		Vector[] array;
		if (m_vertices != null)
		{
			array = new Vector[m_vertices.Length];
			if (_transform.IsIdentity)
			{
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = (Vector)m_vertices[i];
				}
			}
			else
			{
				for (int j = 0; j < array.Length; j++)
				{
					array[j] = _transform.Transform((Vector)m_vertices[j]);
				}
				FixCounterClockwiseVertices(array);
			}
		}
		else
		{
			Point[] bezierControlPoints = GetBezierControlPoints();
			array = new Vector[bezierControlPoints.Length];
			for (int k = 0; k < array.Length; k++)
			{
				array[k] = (Vector)bezierControlPoints[k];
			}
		}
		return array;
	}

	private void ComputeRectangleVertices()
	{
		Point point = new Point(0.0 - m_width * 0.5, 0.0 - m_height * 0.5);
		m_vertices = new Point[4]
		{
			point,
			point + new Vector(m_width, 0.0),
			point + new Vector(m_width, m_height),
			point + new Vector(0.0, m_height)
		};
		if (!DoubleUtil.IsZero(m_rotation))
		{
			Matrix identity = Matrix.Identity;
			identity.Rotate(m_rotation);
			identity.Transform(m_vertices);
		}
	}

	private void FixCounterClockwiseVertices(Vector[] vertices)
	{
		Point point = (Point)vertices[^1];
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < vertices.Length; i++)
		{
			Point point2 = (Point)vertices[i];
			double num3 = Vector.Determinant(point2 - point, (Point)vertices[(i + 1) % vertices.Length] - point2);
			if (0.0 > num3)
			{
				num++;
			}
			else if (0.0 < num3)
			{
				num2++;
			}
			point = point2;
		}
		if (num == vertices.Length)
		{
			int num4 = vertices.Length - 1;
			for (int j = 0; j < vertices.Length / 2; j++)
			{
				Vector vector = vertices[j];
				vertices[j] = vertices[num4 - j];
				vertices[num4 - j] = vector;
			}
		}
	}

	private Point[] GetBezierControlPoints()
	{
		double num = m_width / 2.0;
		double num2 = m_height / 2.0;
		double num3 = num * 0.5522847498307935;
		double num4 = num2 * 0.5522847498307935;
		Point[] array = new Point[12]
		{
			new Point(0.0 - num, 0.0 - num4),
			new Point(0.0 - num3, 0.0 - num2),
			new Point(0.0, 0.0 - num2),
			new Point(num3, 0.0 - num2),
			new Point(num, 0.0 - num4),
			new Point(num, 0.0),
			new Point(num, num4),
			new Point(num3, num2),
			new Point(0.0, num2),
			new Point(0.0 - num3, num2),
			new Point(0.0 - num, num4),
			new Point(0.0 - num, 0.0)
		};
		Matrix identity = Matrix.Identity;
		if (m_rotation != 0.0)
		{
			identity.Rotate(m_rotation);
		}
		if (!_transform.IsIdentity)
		{
			identity *= _transform;
		}
		if (!identity.IsIdentity)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = identity.Transform(array[i]);
			}
		}
		return array;
	}
}
