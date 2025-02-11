using System;
using System.Windows;
using System.Windows.Media.Media3D;

namespace MS.Internal.Media3D;

internal static class LineUtil
{
	private struct JacobiRotation
	{
		private int _p;

		private int _q;

		private double _c;

		private double _s;

		public JacobiRotation(int p, int q, double[,] a)
		{
			_p = p;
			_q = q;
			double num = (a[q, q] - a[p, p]) / (2.0 * a[p, q]);
			if (num < double.MaxValue && num > double.MinValue)
			{
				double num2 = Math.Sqrt(1.0 + num * num);
				double num3 = ((0.0 - num < 0.0) ? (0.0 - num + num2) : (0.0 - num - num2));
				_c = 1.0 / Math.Sqrt(1.0 + num3 * num3);
				_s = num3 * _c;
			}
			else
			{
				_c = 1.0;
				_s = 0.0;
			}
		}

		public double[,] LeftRightMultiply(double[,] a)
		{
			return RightMultiply(LeftMultiplyTranspose(a));
		}

		public double[,] RightMultiply(double[,] a)
		{
			for (int i = 0; i < 4; i++)
			{
				double num = a[i, _p];
				double num2 = a[i, _q];
				a[i, _p] = _c * num - _s * num2;
				a[i, _q] = _s * num + _c * num2;
			}
			return a;
		}

		public double[,] LeftMultiplyTranspose(double[,] a)
		{
			for (int i = 0; i < 4; i++)
			{
				double num = a[_p, i];
				double num2 = a[_q, i];
				a[_p, i] = _c * num - _s * num2;
				a[_q, i] = _s * num + _c * num2;
			}
			return a;
		}
	}

	private static readonly int[,] s_pairs = new int[6, 2]
	{
		{ 0, 1 },
		{ 0, 2 },
		{ 0, 3 },
		{ 1, 2 },
		{ 1, 3 },
		{ 2, 3 }
	};

	private const int s_pairsCount = 6;

	public static void Transform(Matrix3D modelMatrix, ref Point3D origin, ref Vector3D direction, out bool isRay)
	{
		if (modelMatrix.InvertCore())
		{
			Point4D point = new Point4D(origin.X, origin.Y, origin.Z, 1.0);
			Point4D point2 = new Point4D(direction.X, direction.Y, direction.Z, 0.0);
			modelMatrix.MultiplyPoint(ref point);
			modelMatrix.MultiplyPoint(ref point2);
			if (point.W == 1.0 && point2.W == 0.0)
			{
				origin = new Point3D(point.X, point.Y, point.Z);
				direction = new Vector3D(point2.X, point2.Y, point2.Z);
				isRay = true;
				return;
			}
			ColumnsToAffinePointVector(new double[4, 2]
			{
				{ point.X, point2.X },
				{ point.Y, point2.Y },
				{ point.Z, point2.Z },
				{ point.W, point2.W }
			}, 0, 1, out origin, out direction);
			isRay = false;
		}
		else
		{
			TransformSingular(ref modelMatrix, ref origin, ref direction);
			isRay = false;
		}
	}

	private static void TransformSingular(ref Matrix3D modelMatrix, ref Point3D origin, ref Vector3D direction)
	{
		double[,] m = TransformedLineMatrix(ref modelMatrix, ref origin, ref direction);
		m = Square(m);
		double[,] array = new double[4, 4]
		{
			{ 1.0, 0.0, 0.0, 0.0 },
			{ 0.0, 1.0, 0.0, 0.0 },
			{ 0.0, 0.0, 1.0, 0.0 },
			{ 0.0, 0.0, 0.0, 1.0 }
		};
		int num = 30;
		for (int i = 0; i < num; i++)
		{
			int num2 = i % 6;
			JacobiRotation jacobiRotation = new JacobiRotation(s_pairs[num2, 0], s_pairs[num2, 1], m);
			m = jacobiRotation.LeftRightMultiply(m);
			array = jacobiRotation.RightMultiply(array);
		}
		FindSmallestTwoDiagonal(m, out var evec, out var evec2);
		ColumnsToAffinePointVector(array, evec, evec2, out origin, out direction);
	}

	private static void ColumnsToAffinePointVector(double[,] matrix, int col1, int col2, out Point3D origin, out Vector3D direction)
	{
		if (matrix[3, col1] * matrix[3, col1] < matrix[3, col2] * matrix[3, col2])
		{
			int num = col1;
			col1 = col2;
			col2 = num;
		}
		double num2 = 1.0 / matrix[3, col1];
		origin = new Point3D(num2 * matrix[0, col1], num2 * matrix[1, col1], num2 * matrix[2, col1]);
		num2 = 0.0 - matrix[3, col2];
		direction = new Vector3D(matrix[0, col2] + num2 * origin.X, matrix[1, col2] + num2 * origin.Y, matrix[2, col2] + num2 * origin.Z);
	}

	private static void FindSmallestTwoDiagonal(double[,] matrix, out int evec1, out int evec2)
	{
		evec1 = 0;
		evec2 = 1;
		double num = matrix[0, 0] * matrix[0, 0];
		double num2 = matrix[1, 1] * matrix[1, 1];
		for (int i = 2; i < 4; i++)
		{
			double num3 = matrix[i, i] * matrix[i, i];
			if (num3 < num)
			{
				if (num < num2)
				{
					num2 = num3;
					evec2 = i;
				}
				else
				{
					num = num3;
					evec1 = i;
				}
			}
			else if (num3 < num2)
			{
				num2 = num3;
				evec2 = i;
			}
		}
	}

	private static double[,] TransformedLineMatrix(ref Matrix3D modelMatrix, ref Point3D origin, ref Vector3D direction)
	{
		double x = origin.X;
		double y = origin.Y;
		double z = origin.Z;
		double x2 = direction.X;
		double y2 = direction.Y;
		double z2 = direction.Z;
		double num = y2 * z - y * z2;
		double num2 = x * z2 - x2 * z;
		double num3 = x2 * y - x * y2;
		Matrix3D matrix3D = modelMatrix * new Matrix3D(num, y2, z2, 0.0, num2, 0.0 - x2, 0.0, z2, num3, 0.0, 0.0 - x2, 0.0 - y2, 0.0, num3, 0.0 - num2, num);
		return new double[4, 4]
		{
			{ matrix3D.M11, matrix3D.M12, matrix3D.M13, matrix3D.M14 },
			{ matrix3D.M21, matrix3D.M22, matrix3D.M23, matrix3D.M24 },
			{ matrix3D.M31, matrix3D.M32, matrix3D.M33, matrix3D.M34 },
			{ matrix3D.OffsetX, matrix3D.OffsetY, matrix3D.OffsetZ, matrix3D.M44 }
		};
	}

	private static double[,] Square(double[,] m)
	{
		double[,] array = new double[4, 4];
		double num = 0.0;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				num = Math.Max(num, m[i, j] * m[i, j]);
			}
		}
		num = Math.Sqrt(num);
		for (int k = 0; k < 4; k++)
		{
			for (int l = 0; l < 4; l++)
			{
				m[k, l] /= num;
			}
		}
		for (int n = 0; n < 4; n++)
		{
			for (int num2 = 0; num2 < 4; num2++)
			{
				double num3 = 0.0;
				for (int num4 = 0; num4 < 4; num4++)
				{
					num3 += m[n, num4] * m[num2, num4];
				}
				array[n, num2] = num3;
			}
		}
		return array;
	}

	internal static bool ComputeLineTriangleIntersection(FaceType type, ref Point3D origin, ref Vector3D direction, ref Point3D v0, ref Point3D v1, ref Point3D v2, out Point hitCoord, out double dist)
	{
		Point3D.Subtract(ref v1, ref v0, out var result);
		Point3D.Subtract(ref v2, ref v0, out var result2);
		Vector3D.CrossProduct(ref direction, ref result2, out var result3);
		double num = Vector3D.DotProduct(ref result, ref result3);
		Vector3D result4;
		if (num > 0.0 && (type & FaceType.Front) != 0)
		{
			Point3D.Subtract(ref origin, ref v0, out result4);
		}
		else
		{
			if (!(num < 0.0) || (type & FaceType.Back) == 0)
			{
				hitCoord = default(Point);
				dist = 0.0;
				return false;
			}
			Point3D.Subtract(ref v0, ref origin, out result4);
			num = 0.0 - num;
		}
		double num2 = Vector3D.DotProduct(ref result4, ref result3);
		if (num2 < 0.0 || num < num2)
		{
			hitCoord = default(Point);
			dist = 0.0;
			return false;
		}
		Vector3D.CrossProduct(ref result4, ref result, out var result5);
		double num3 = Vector3D.DotProduct(ref direction, ref result5);
		if (num3 < 0.0 || num < num2 + num3)
		{
			hitCoord = default(Point);
			dist = 0.0;
			return false;
		}
		double num4 = Vector3D.DotProduct(ref result2, ref result5);
		double num5 = 1.0 / num;
		num4 *= num5;
		num2 *= num5;
		num3 *= num5;
		hitCoord = new Point(num2, num3);
		dist = num4;
		return true;
	}

	internal static bool ComputeLineBoxIntersection(ref Point3D origin, ref Vector3D direction, ref Rect3D box, bool isRay)
	{
		if (box.IsEmpty)
		{
			return false;
		}
		bool flag = true;
		bool[] array = new bool[3];
		double[] array2 = new double[3];
		double[] array3 = new double[3] { box.X, box.Y, box.Z };
		double[] array4 = new double[3]
		{
			box.X + box.SizeX,
			box.Y + box.SizeY,
			box.Z + box.SizeZ
		};
		double[] array5 = new double[3] { origin.X, origin.Y, origin.Z };
		double[] array6 = new double[3] { direction.X, direction.Y, direction.Z };
		for (int i = 0; i < 3; i++)
		{
			if (array5[i] < array3[i])
			{
				array[i] = false;
				array2[i] = array3[i];
				flag = false;
			}
			else if (array5[i] > array4[i])
			{
				array[i] = false;
				array2[i] = array4[i];
				flag = false;
			}
			else
			{
				array[i] = true;
			}
		}
		if (flag)
		{
			return true;
		}
		double num = ((!isRay) ? 0.0 : (-1.0));
		int num2 = 0;
		for (int i = 0; i < 3; i++)
		{
			if (array[i] || array6[i] == 0.0)
			{
				continue;
			}
			double num3 = (array2[i] - array5[i]) / array6[i];
			if (isRay)
			{
				if (num3 > num)
				{
					num = num3;
					num2 = i;
				}
			}
			else if (num3 * num3 > num * num)
			{
				num = num3;
				num2 = i;
			}
		}
		if (isRay && num < 0.0)
		{
			return false;
		}
		for (int i = 0; i < 3; i++)
		{
			if (i != num2)
			{
				double num4 = array5[i] + num * array6[i];
				if (num4 < array3[i] || array4[i] < num4)
				{
					return false;
				}
			}
		}
		return true;
	}
}
