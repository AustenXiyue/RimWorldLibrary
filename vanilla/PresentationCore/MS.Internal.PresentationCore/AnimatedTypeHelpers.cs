using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MS.Internal.PresentationCore;

internal static class AnimatedTypeHelpers
{
	internal static byte InterpolateByte(byte from, byte to, double progress)
	{
		return (byte)(from + (int)(((double)(to - from) + 0.5) * progress));
	}

	internal static Color InterpolateColor(Color from, Color to, double progress)
	{
		return from + (to - from) * (float)progress;
	}

	internal static decimal InterpolateDecimal(decimal from, decimal to, double progress)
	{
		return from + (to - from) * (decimal)progress;
	}

	internal static double InterpolateDouble(double from, double to, double progress)
	{
		return from + (to - from) * progress;
	}

	internal static short InterpolateInt16(short from, short to, double progress)
	{
		if (progress == 0.0)
		{
			return from;
		}
		if (progress == 1.0)
		{
			return to;
		}
		double num = to - from;
		num *= progress;
		num += ((num > 0.0) ? 0.5 : (-0.5));
		return (short)(from + (short)num);
	}

	internal static int InterpolateInt32(int from, int to, double progress)
	{
		if (progress == 0.0)
		{
			return from;
		}
		if (progress == 1.0)
		{
			return to;
		}
		double num = to - from;
		num *= progress;
		num += ((num > 0.0) ? 0.5 : (-0.5));
		return from + (int)num;
	}

	internal static long InterpolateInt64(long from, long to, double progress)
	{
		if (progress == 0.0)
		{
			return from;
		}
		if (progress == 1.0)
		{
			return to;
		}
		double num = to - from;
		num *= progress;
		num += ((num > 0.0) ? 0.5 : (-0.5));
		return from + (long)num;
	}

	internal static Point InterpolatePoint(Point from, Point to, double progress)
	{
		return from + (to - from) * progress;
	}

	internal static Point3D InterpolatePoint3D(Point3D from, Point3D to, double progress)
	{
		return from + (to - from) * progress;
	}

	internal static Quaternion InterpolateQuaternion(Quaternion from, Quaternion to, double progress, bool useShortestPath)
	{
		return Quaternion.Slerp(from, to, progress, useShortestPath);
	}

	internal static Rect InterpolateRect(Rect from, Rect to, double progress)
	{
		Rect result = default(Rect);
		result.Location = new Point(from.Location.X + (to.Location.X - from.Location.X) * progress, from.Location.Y + (to.Location.Y - from.Location.Y) * progress);
		result.Size = new Size(from.Size.Width + (to.Size.Width - from.Size.Width) * progress, from.Size.Height + (to.Size.Height - from.Size.Height) * progress);
		return result;
	}

	internal static Rotation3D InterpolateRotation3D(Rotation3D from, Rotation3D to, double progress)
	{
		return new QuaternionRotation3D(InterpolateQuaternion(from.InternalQuaternion, to.InternalQuaternion, progress, useShortestPath: true));
	}

	internal static float InterpolateSingle(float from, float to, double progress)
	{
		return from + (float)((double)(to - from) * progress);
	}

	internal static Size InterpolateSize(Size from, Size to, double progress)
	{
		return (Size)InterpolateVector((Vector)from, (Vector)to, progress);
	}

	internal static Vector InterpolateVector(Vector from, Vector to, double progress)
	{
		return from + (to - from) * progress;
	}

	internal static Vector3D InterpolateVector3D(Vector3D from, Vector3D to, double progress)
	{
		return from + (to - from) * progress;
	}

	internal static byte AddByte(byte value1, byte value2)
	{
		return (byte)(value1 + value2);
	}

	internal static Color AddColor(Color value1, Color value2)
	{
		return value1 + value2;
	}

	internal static decimal AddDecimal(decimal value1, decimal value2)
	{
		return value1 + value2;
	}

	internal static double AddDouble(double value1, double value2)
	{
		return value1 + value2;
	}

	internal static short AddInt16(short value1, short value2)
	{
		return (short)(value1 + value2);
	}

	internal static int AddInt32(int value1, int value2)
	{
		return value1 + value2;
	}

	internal static long AddInt64(long value1, long value2)
	{
		return value1 + value2;
	}

	internal static Point AddPoint(Point value1, Point value2)
	{
		return new Point(value1.X + value2.X, value1.Y + value2.Y);
	}

	internal static Point3D AddPoint3D(Point3D value1, Point3D value2)
	{
		return new Point3D(value1.X + value2.X, value1.Y + value2.Y, value1.Z + value2.Z);
	}

	internal static Quaternion AddQuaternion(Quaternion value1, Quaternion value2)
	{
		return value1 * value2;
	}

	internal static float AddSingle(float value1, float value2)
	{
		return value1 + value2;
	}

	internal static Size AddSize(Size value1, Size value2)
	{
		return new Size(value1.Width + value2.Width, value1.Height + value2.Height);
	}

	internal static Vector AddVector(Vector value1, Vector value2)
	{
		return value1 + value2;
	}

	internal static Vector3D AddVector3D(Vector3D value1, Vector3D value2)
	{
		return value1 + value2;
	}

	internal static Rect AddRect(Rect value1, Rect value2)
	{
		return new Rect(AddPoint(value1.Location, value2.Location), AddSize(value1.Size, value2.Size));
	}

	internal static Rotation3D AddRotation3D(Rotation3D value1, Rotation3D value2)
	{
		if (value1 == null)
		{
			value1 = Rotation3D.Identity;
		}
		if (value2 == null)
		{
			value2 = Rotation3D.Identity;
		}
		return new QuaternionRotation3D(AddQuaternion(value1.InternalQuaternion, value2.InternalQuaternion));
	}

	internal static byte SubtractByte(byte value1, byte value2)
	{
		return (byte)(value1 - value2);
	}

	internal static Color SubtractColor(Color value1, Color value2)
	{
		return value1 - value2;
	}

	internal static decimal SubtractDecimal(decimal value1, decimal value2)
	{
		return value1 - value2;
	}

	internal static double SubtractDouble(double value1, double value2)
	{
		return value1 - value2;
	}

	internal static short SubtractInt16(short value1, short value2)
	{
		return (short)(value1 - value2);
	}

	internal static int SubtractInt32(int value1, int value2)
	{
		return value1 - value2;
	}

	internal static long SubtractInt64(long value1, long value2)
	{
		return value1 - value2;
	}

	internal static Point SubtractPoint(Point value1, Point value2)
	{
		return new Point(value1.X - value2.X, value1.Y - value2.Y);
	}

	internal static Point3D SubtractPoint3D(Point3D value1, Point3D value2)
	{
		return new Point3D(value1.X - value2.X, value1.Y - value2.Y, value1.Z - value2.Z);
	}

	internal static Quaternion SubtractQuaternion(Quaternion value1, Quaternion value2)
	{
		value2.Invert();
		return value1 * value2;
	}

	internal static float SubtractSingle(float value1, float value2)
	{
		return value1 - value2;
	}

	internal static Size SubtractSize(Size value1, Size value2)
	{
		return new Size(value1.Width - value2.Width, value1.Height - value2.Height);
	}

	internal static Vector SubtractVector(Vector value1, Vector value2)
	{
		return value1 - value2;
	}

	internal static Vector3D SubtractVector3D(Vector3D value1, Vector3D value2)
	{
		return value1 - value2;
	}

	internal static Rect SubtractRect(Rect value1, Rect value2)
	{
		return new Rect(SubtractPoint(value1.Location, value2.Location), SubtractSize(value1.Size, value2.Size));
	}

	internal static Rotation3D SubtractRotation3D(Rotation3D value1, Rotation3D value2)
	{
		return new QuaternionRotation3D(SubtractQuaternion(value1.InternalQuaternion, value2.InternalQuaternion));
	}

	internal static double GetSegmentLengthBoolean(bool from, bool to)
	{
		if (from != to)
		{
			return 1.0;
		}
		return 0.0;
	}

	internal static double GetSegmentLengthByte(byte from, byte to)
	{
		return Math.Abs(to - from);
	}

	internal static double GetSegmentLengthChar(char from, char to)
	{
		if (from != to)
		{
			return 1.0;
		}
		return 0.0;
	}

	internal static double GetSegmentLengthColor(Color from, Color to)
	{
		return Math.Abs(to.ScA - from.ScA) + Math.Abs(to.ScR - from.ScR) + Math.Abs(to.ScG - from.ScG) + Math.Abs(to.ScB - from.ScB);
	}

	internal static double GetSegmentLengthDecimal(decimal from, decimal to)
	{
		return (double)Math.Abs(to - from);
	}

	internal static double GetSegmentLengthDouble(double from, double to)
	{
		return Math.Abs(to - from);
	}

	internal static double GetSegmentLengthInt16(short from, short to)
	{
		return Math.Abs(to - from);
	}

	internal static double GetSegmentLengthInt32(int from, int to)
	{
		return Math.Abs(to - from);
	}

	internal static double GetSegmentLengthInt64(long from, long to)
	{
		return Math.Abs(to - from);
	}

	internal static double GetSegmentLengthMatrix(Matrix from, Matrix to)
	{
		if (from != to)
		{
			return 1.0;
		}
		return 0.0;
	}

	internal static double GetSegmentLengthObject(object from, object to)
	{
		return 1.0;
	}

	internal static double GetSegmentLengthPoint(Point from, Point to)
	{
		return Math.Abs((to - from).Length);
	}

	internal static double GetSegmentLengthPoint3D(Point3D from, Point3D to)
	{
		return Math.Abs((to - from).Length);
	}

	internal static double GetSegmentLengthQuaternion(Quaternion from, Quaternion to)
	{
		from.Invert();
		return (to * from).Angle;
	}

	internal static double GetSegmentLengthRect(Rect from, Rect to)
	{
		double segmentLengthPoint = GetSegmentLengthPoint(from.Location, to.Location);
		double segmentLengthSize = GetSegmentLengthSize(from.Size, to.Size);
		return Math.Sqrt(segmentLengthPoint * segmentLengthPoint + segmentLengthSize * segmentLengthSize);
	}

	internal static double GetSegmentLengthRotation3D(Rotation3D from, Rotation3D to)
	{
		return GetSegmentLengthQuaternion(from.InternalQuaternion, to.InternalQuaternion);
	}

	internal static double GetSegmentLengthSingle(float from, float to)
	{
		return Math.Abs(to - from);
	}

	internal static double GetSegmentLengthSize(Size from, Size to)
	{
		return Math.Abs(((Vector)to - (Vector)from).Length);
	}

	internal static double GetSegmentLengthString(string from, string to)
	{
		if (from != to)
		{
			return 1.0;
		}
		return 0.0;
	}

	internal static double GetSegmentLengthVector(Vector from, Vector to)
	{
		return Math.Abs((to - from).Length);
	}

	internal static double GetSegmentLengthVector3D(Vector3D from, Vector3D to)
	{
		return Math.Abs((to - from).Length);
	}

	internal static byte ScaleByte(byte value, double factor)
	{
		return (byte)((double)(int)value * factor);
	}

	internal static Color ScaleColor(Color value, double factor)
	{
		return value * (float)factor;
	}

	internal static decimal ScaleDecimal(decimal value, double factor)
	{
		return value * (decimal)factor;
	}

	internal static double ScaleDouble(double value, double factor)
	{
		return value * factor;
	}

	internal static short ScaleInt16(short value, double factor)
	{
		return (short)((double)value * factor);
	}

	internal static int ScaleInt32(int value, double factor)
	{
		return (int)((double)value * factor);
	}

	internal static long ScaleInt64(long value, double factor)
	{
		return (long)((double)value * factor);
	}

	internal static Point ScalePoint(Point value, double factor)
	{
		return new Point(value.X * factor, value.Y * factor);
	}

	internal static Point3D ScalePoint3D(Point3D value, double factor)
	{
		return new Point3D(value.X * factor, value.Y * factor, value.Z * factor);
	}

	internal static Quaternion ScaleQuaternion(Quaternion value, double factor)
	{
		return new Quaternion(value.Axis, value.Angle * factor);
	}

	internal static Rect ScaleRect(Rect value, double factor)
	{
		Rect result = default(Rect);
		result.Location = new Point(value.Location.X * factor, value.Location.Y * factor);
		result.Size = new Size(value.Size.Width * factor, value.Size.Height * factor);
		return result;
	}

	internal static Rotation3D ScaleRotation3D(Rotation3D value, double factor)
	{
		return new QuaternionRotation3D(ScaleQuaternion(value.InternalQuaternion, factor));
	}

	internal static float ScaleSingle(float value, double factor)
	{
		return (float)((double)value * factor);
	}

	internal static Size ScaleSize(Size value, double factor)
	{
		return (Size)((Vector)value * factor);
	}

	internal static Vector ScaleVector(Vector value, double factor)
	{
		return value * factor;
	}

	internal static Vector3D ScaleVector3D(Vector3D value, double factor)
	{
		return value * factor;
	}

	internal static bool IsValidAnimationValueBoolean(bool value)
	{
		return true;
	}

	internal static bool IsValidAnimationValueByte(byte value)
	{
		return true;
	}

	internal static bool IsValidAnimationValueChar(char value)
	{
		return true;
	}

	internal static bool IsValidAnimationValueColor(Color value)
	{
		return true;
	}

	internal static bool IsValidAnimationValueDecimal(decimal value)
	{
		return true;
	}

	internal static bool IsValidAnimationValueDouble(double value)
	{
		if (IsInvalidDouble(value))
		{
			return false;
		}
		return true;
	}

	internal static bool IsValidAnimationValueInt16(short value)
	{
		return true;
	}

	internal static bool IsValidAnimationValueInt32(int value)
	{
		return true;
	}

	internal static bool IsValidAnimationValueInt64(long value)
	{
		return true;
	}

	internal static bool IsValidAnimationValueMatrix(Matrix value)
	{
		return true;
	}

	internal static bool IsValidAnimationValuePoint(Point value)
	{
		if (IsInvalidDouble(value.X) || IsInvalidDouble(value.Y))
		{
			return false;
		}
		return true;
	}

	internal static bool IsValidAnimationValuePoint3D(Point3D value)
	{
		if (IsInvalidDouble(value.X) || IsInvalidDouble(value.Y) || IsInvalidDouble(value.Z))
		{
			return false;
		}
		return true;
	}

	internal static bool IsValidAnimationValueQuaternion(Quaternion value)
	{
		if (IsInvalidDouble(value.X) || IsInvalidDouble(value.Y) || IsInvalidDouble(value.Z) || IsInvalidDouble(value.W))
		{
			return false;
		}
		return true;
	}

	internal static bool IsValidAnimationValueRect(Rect value)
	{
		if (IsInvalidDouble(value.Location.X) || IsInvalidDouble(value.Location.Y) || IsInvalidDouble(value.Size.Width) || IsInvalidDouble(value.Size.Height) || value.IsEmpty)
		{
			return false;
		}
		return true;
	}

	internal static bool IsValidAnimationValueRotation3D(Rotation3D value)
	{
		return IsValidAnimationValueQuaternion(value.InternalQuaternion);
	}

	internal static bool IsValidAnimationValueSingle(float value)
	{
		if (IsInvalidDouble(value))
		{
			return false;
		}
		return true;
	}

	internal static bool IsValidAnimationValueSize(Size value)
	{
		if (IsInvalidDouble(value.Width) || IsInvalidDouble(value.Height))
		{
			return false;
		}
		return true;
	}

	internal static bool IsValidAnimationValueString(string value)
	{
		return true;
	}

	internal static bool IsValidAnimationValueVector(Vector value)
	{
		if (IsInvalidDouble(value.X) || IsInvalidDouble(value.Y))
		{
			return false;
		}
		return true;
	}

	internal static bool IsValidAnimationValueVector3D(Vector3D value)
	{
		if (IsInvalidDouble(value.X) || IsInvalidDouble(value.Y) || IsInvalidDouble(value.Z))
		{
			return false;
		}
		return true;
	}

	internal static byte GetZeroValueByte(byte baseValue)
	{
		return 0;
	}

	internal static Color GetZeroValueColor(Color baseValue)
	{
		return Color.FromScRgb(0f, 0f, 0f, 0f);
	}

	internal static decimal GetZeroValueDecimal(decimal baseValue)
	{
		return 0m;
	}

	internal static double GetZeroValueDouble(double baseValue)
	{
		return 0.0;
	}

	internal static short GetZeroValueInt16(short baseValue)
	{
		return 0;
	}

	internal static int GetZeroValueInt32(int baseValue)
	{
		return 0;
	}

	internal static long GetZeroValueInt64(long baseValue)
	{
		return 0L;
	}

	internal static Point GetZeroValuePoint(Point baseValue)
	{
		return default(Point);
	}

	internal static Point3D GetZeroValuePoint3D(Point3D baseValue)
	{
		return default(Point3D);
	}

	internal static Quaternion GetZeroValueQuaternion(Quaternion baseValue)
	{
		return Quaternion.Identity;
	}

	internal static float GetZeroValueSingle(float baseValue)
	{
		return 0f;
	}

	internal static Size GetZeroValueSize(Size baseValue)
	{
		return default(Size);
	}

	internal static Vector GetZeroValueVector(Vector baseValue)
	{
		return default(Vector);
	}

	internal static Vector3D GetZeroValueVector3D(Vector3D baseValue)
	{
		return default(Vector3D);
	}

	internal static Rect GetZeroValueRect(Rect baseValue)
	{
		return new Rect(default(Point), default(Vector));
	}

	internal static Rotation3D GetZeroValueRotation3D(Rotation3D baseValue)
	{
		return Rotation3D.Identity;
	}

	private static bool IsInvalidDouble(double value)
	{
		if (!double.IsInfinity(value))
		{
			return double.IsNaN(value);
		}
		return true;
	}
}
