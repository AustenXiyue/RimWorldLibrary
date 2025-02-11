using System.Windows;

namespace MS.Internal.FontFace;

internal struct MatchingStyle
{
	private struct Vector
	{
		private double _x;

		private double _y;

		private double _z;

		internal double X => _x;

		internal double Y => _y;

		internal double Z => _z;

		internal double LengthSquared => _x * _x + _y * _y + _z * _z;

		internal Vector(double x, double y, double z)
		{
			_x = x;
			_y = y;
			_z = z;
		}

		internal static double DotProduct(Vector l, Vector r)
		{
			return l._x * r._x + l._y * r._y + l._z * r._z;
		}

		public static Vector operator -(Vector l, Vector r)
		{
			return new Vector(l._x - r._x, l._y - r._y, l._z - r._z);
		}

		public static bool operator ==(Vector l, Vector r)
		{
			if (l._x == r._x && l._y == r._y)
			{
				return l._z == r._z;
			}
			return false;
		}

		public static bool operator !=(Vector l, Vector r)
		{
			return !(l == r);
		}

		public override bool Equals(object o)
		{
			if (o == null)
			{
				return false;
			}
			if (o is Vector vector)
			{
				return this == vector;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return _x.GetHashCode() ^ _y.GetHashCode() ^ _z.GetHashCode();
		}
	}

	private Vector _vector;

	private const double FontWeightScale = 5.0;

	private const double FontStyleScale = 7.0;

	private const double FontStretchScale = 11.0;

	internal MatchingStyle(FontStyle style, FontWeight weight, FontStretch stretch)
	{
		_vector = new Vector((double)(stretch.ToOpenTypeStretch() - FontStretches.Normal.ToOpenTypeStretch()) * 11.0, (double)style.GetStyleForInternalConstruction() * 7.0, (double)(weight.ToOpenTypeWeight() - FontWeights.Normal.ToOpenTypeWeight()) / 100.0 * 5.0);
	}

	public static bool operator ==(MatchingStyle l, MatchingStyle r)
	{
		return l._vector == r._vector;
	}

	public static bool operator !=(MatchingStyle l, MatchingStyle r)
	{
		return l._vector != r._vector;
	}

	public override bool Equals(object o)
	{
		if (o == null)
		{
			return false;
		}
		if (o is MatchingStyle)
		{
			return this == (MatchingStyle)o;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _vector.GetHashCode();
	}

	internal static bool IsBetterMatch(MatchingStyle target, MatchingStyle best, ref MatchingStyle matching)
	{
		return matching.IsBetterMatch(target, best);
	}

	internal bool IsBetterMatch(MatchingStyle target, MatchingStyle best)
	{
		double lengthSquared = (_vector - target._vector).LengthSquared;
		double lengthSquared2 = (best._vector - target._vector).LengthSquared;
		if (lengthSquared < lengthSquared2)
		{
			return true;
		}
		if (lengthSquared == lengthSquared2)
		{
			double num = Vector.DotProduct(_vector, target._vector);
			double num2 = Vector.DotProduct(best._vector, target._vector);
			if (num > num2)
			{
				return true;
			}
			if (num == num2 && (_vector.X > best._vector.X || (_vector.X == best._vector.X && (_vector.Y > best._vector.Y || (_vector.Y == best._vector.Y && _vector.Z > best._vector.Z)))))
			{
				return true;
			}
		}
		return false;
	}
}
