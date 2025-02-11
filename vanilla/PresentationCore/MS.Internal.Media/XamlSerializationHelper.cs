using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MS.Internal.PresentationCore;

namespace MS.Internal.Media;

internal static class XamlSerializationHelper
{
	internal enum SerializationFloatType : byte
	{
		Unknown,
		Zero,
		One,
		MinusOne,
		ScaledInteger,
		Double,
		Other
	}

	private const double scaleFactor = 1000000.0;

	private const double inverseScaleFactor = 1E-06;

	[FriendAccessAllowed]
	internal static bool SerializePoint3D(BinaryWriter writer, string stringValues)
	{
		Point3DCollection point3DCollection = Point3DCollection.Parse(stringValues);
		writer.Write((uint)point3DCollection.Count);
		for (int i = 0; i < point3DCollection.Count; i++)
		{
			Point3D point3D = point3DCollection[i];
			WriteDouble(writer, point3D.X);
			WriteDouble(writer, point3D.Y);
			WriteDouble(writer, point3D.Z);
		}
		return true;
	}

	[FriendAccessAllowed]
	internal static bool SerializeVector3D(BinaryWriter writer, string stringValues)
	{
		Vector3DCollection vector3DCollection = Vector3DCollection.Parse(stringValues);
		writer.Write((uint)vector3DCollection.Count);
		for (int i = 0; i < vector3DCollection.Count; i++)
		{
			Vector3D vector3D = vector3DCollection[i];
			WriteDouble(writer, vector3D.X);
			WriteDouble(writer, vector3D.Y);
			WriteDouble(writer, vector3D.Z);
		}
		return true;
	}

	[FriendAccessAllowed]
	internal static bool SerializePoint(BinaryWriter writer, string stringValue)
	{
		PointCollection pointCollection = PointCollection.Parse(stringValue);
		writer.Write((uint)pointCollection.Count);
		for (int i = 0; i < pointCollection.Count; i++)
		{
			Point point = pointCollection[i];
			WriteDouble(writer, point.X);
			WriteDouble(writer, point.Y);
		}
		return true;
	}

	internal static void WriteDouble(BinaryWriter writer, double value)
	{
		if (value == 0.0)
		{
			writer.Write((byte)1);
			return;
		}
		if (value == 1.0)
		{
			writer.Write((byte)2);
			return;
		}
		if (value == -1.0)
		{
			writer.Write((byte)3);
			return;
		}
		int intValue = 0;
		if (CanConvertToInteger(value, ref intValue))
		{
			writer.Write((byte)4);
			writer.Write(intValue);
		}
		else
		{
			writer.Write((byte)5);
			writer.Write(value);
		}
	}

	internal static double ReadDouble(BinaryReader reader)
	{
		return (SerializationFloatType)reader.ReadByte() switch
		{
			SerializationFloatType.Zero => 0.0, 
			SerializationFloatType.One => 1.0, 
			SerializationFloatType.MinusOne => -1.0, 
			SerializationFloatType.ScaledInteger => ReadScaledInteger(reader), 
			SerializationFloatType.Double => reader.ReadDouble(), 
			_ => throw new ArgumentException(SR.FloatUnknownBamlType), 
		};
	}

	internal static double ReadScaledInteger(BinaryReader reader)
	{
		return (double)reader.ReadInt32() * 1E-06;
	}

	internal static bool CanConvertToInteger(double doubleValue, ref int intValue)
	{
		double num = doubleValue * 1000000.0;
		double num2 = Math.Floor(num);
		if (!(num2 <= 2147483647.0) || !(num2 >= -2147483648.0))
		{
			return false;
		}
		if (num - num2 > double.Epsilon)
		{
			return false;
		}
		intValue = (int)num;
		return true;
	}
}
