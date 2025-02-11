using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using MS.Internal.PresentationCore;

namespace MS.Internal.Media;

internal class ParserStreamGeometryContext : StreamGeometryContext
{
	private enum ParserGeometryContextOpCodes : byte
	{
		BeginFigure,
		LineTo,
		QuadraticBezierTo,
		BezierTo,
		PolyLineTo,
		PolyQuadraticBezierTo,
		PolyBezierTo,
		ArcTo,
		Closed,
		FillRule
	}

	private const byte HighNibble = 240;

	private const byte LowNibble = 15;

	private const byte SetBool1 = 16;

	private const byte SetBool2 = 32;

	private const byte SetBool3 = 64;

	private const byte SetBool4 = 128;

	private BinaryWriter _bw;

	private Point _startPoint;

	private bool _isClosed;

	private bool _isFilled;

	private int _figureStreamPosition = -1;

	internal bool FigurePending => _figureStreamPosition > -1;

	internal int CurrentStreamPosition => checked((int)_bw.Seek(0, SeekOrigin.Current));

	internal ParserStreamGeometryContext(BinaryWriter bw)
	{
		_bw = bw;
	}

	internal void SetFillRule(FillRule fillRule)
	{
		bool @bool = FillRuleToBool(fillRule);
		byte value = PackByte(ParserGeometryContextOpCodes.FillRule, @bool, bool2: false);
		_bw.Write(value);
	}

	public override void BeginFigure(Point startPoint, bool isFilled, bool isClosed)
	{
		FinishFigure();
		_startPoint = startPoint;
		_isFilled = isFilled;
		_isClosed = isClosed;
		_figureStreamPosition = CurrentStreamPosition;
		SerializePointAndTwoBools(ParserGeometryContextOpCodes.BeginFigure, startPoint, isFilled, isClosed);
	}

	public override void LineTo(Point point, bool isStroked, bool isSmoothJoin)
	{
		SerializePointAndTwoBools(ParserGeometryContextOpCodes.LineTo, point, isStroked, isSmoothJoin);
	}

	public override void QuadraticBezierTo(Point point1, Point point2, bool isStroked, bool isSmoothJoin)
	{
		SerializePointAndTwoBools(ParserGeometryContextOpCodes.QuadraticBezierTo, point1, isStroked, isSmoothJoin);
		XamlSerializationHelper.WriteDouble(_bw, point2.X);
		XamlSerializationHelper.WriteDouble(_bw, point2.Y);
	}

	public override void BezierTo(Point point1, Point point2, Point point3, bool isStroked, bool isSmoothJoin)
	{
		SerializePointAndTwoBools(ParserGeometryContextOpCodes.BezierTo, point1, isStroked, isSmoothJoin);
		XamlSerializationHelper.WriteDouble(_bw, point2.X);
		XamlSerializationHelper.WriteDouble(_bw, point2.Y);
		XamlSerializationHelper.WriteDouble(_bw, point3.X);
		XamlSerializationHelper.WriteDouble(_bw, point3.Y);
	}

	public override void PolyLineTo(IList<Point> points, bool isStroked, bool isSmoothJoin)
	{
		SerializeListOfPointsAndTwoBools(ParserGeometryContextOpCodes.PolyLineTo, points, isStroked, isSmoothJoin);
	}

	public override void PolyQuadraticBezierTo(IList<Point> points, bool isStroked, bool isSmoothJoin)
	{
		SerializeListOfPointsAndTwoBools(ParserGeometryContextOpCodes.PolyQuadraticBezierTo, points, isStroked, isSmoothJoin);
	}

	public override void PolyBezierTo(IList<Point> points, bool isStroked, bool isSmoothJoin)
	{
		SerializeListOfPointsAndTwoBools(ParserGeometryContextOpCodes.PolyBezierTo, points, isStroked, isSmoothJoin);
	}

	public override void ArcTo(Point point, Size size, double rotationAngle, bool isLargeArc, SweepDirection sweepDirection, bool isStroked, bool isSmoothJoin)
	{
		SerializePointAndTwoBools(ParserGeometryContextOpCodes.ArcTo, point, isStroked, isSmoothJoin);
		byte b = 0;
		if (isLargeArc)
		{
			b = 15;
		}
		if (SweepToBool(sweepDirection))
		{
			b |= 0xF0;
		}
		_bw.Write(b);
		XamlSerializationHelper.WriteDouble(_bw, size.Width);
		XamlSerializationHelper.WriteDouble(_bw, size.Height);
		XamlSerializationHelper.WriteDouble(_bw, rotationAngle);
	}

	internal void FinishFigure()
	{
		if (FigurePending)
		{
			int currentStreamPosition = CurrentStreamPosition;
			_bw.Seek(_figureStreamPosition, SeekOrigin.Begin);
			SerializePointAndTwoBools(ParserGeometryContextOpCodes.BeginFigure, _startPoint, _isFilled, _isClosed);
			_bw.Seek(currentStreamPosition, SeekOrigin.Begin);
		}
	}

	internal override void DisposeCore()
	{
	}

	internal override void SetClosedState(bool closed)
	{
		_isClosed = closed;
	}

	internal void MarkEOF()
	{
		FinishFigure();
		_bw.Write((byte)8);
	}

	internal static void Deserialize(BinaryReader br, StreamGeometryContext sc, StreamGeometry geometry)
	{
		bool flag = false;
		while (!flag)
		{
			byte b = br.ReadByte();
			switch (UnPackOpCode(b))
			{
			case ParserGeometryContextOpCodes.FillRule:
				DeserializeFillRule(br, b, geometry);
				break;
			case ParserGeometryContextOpCodes.BeginFigure:
				DeserializeBeginFigure(br, b, sc);
				break;
			case ParserGeometryContextOpCodes.LineTo:
				DeserializeLineTo(br, b, sc);
				break;
			case ParserGeometryContextOpCodes.QuadraticBezierTo:
				DeserializeQuadraticBezierTo(br, b, sc);
				break;
			case ParserGeometryContextOpCodes.BezierTo:
				DeserializeBezierTo(br, b, sc);
				break;
			case ParserGeometryContextOpCodes.PolyLineTo:
				DeserializePolyLineTo(br, b, sc);
				break;
			case ParserGeometryContextOpCodes.PolyQuadraticBezierTo:
				DeserializePolyQuadraticBezierTo(br, b, sc);
				break;
			case ParserGeometryContextOpCodes.PolyBezierTo:
				DeserializePolyBezierTo(br, b, sc);
				break;
			case ParserGeometryContextOpCodes.ArcTo:
				DeserializeArcTo(br, b, sc);
				break;
			case ParserGeometryContextOpCodes.Closed:
				flag = true;
				break;
			}
		}
	}

	private static void DeserializeFillRule(BinaryReader br, byte firstByte, StreamGeometry geometry)
	{
		UnPackBools(firstByte, out var @bool, out var _);
		FillRule fillRule = BoolToFillRule(@bool);
		geometry.FillRule = fillRule;
	}

	private static void DeserializeBeginFigure(BinaryReader br, byte firstByte, StreamGeometryContext sc)
	{
		DeserializePointAndTwoBools(br, firstByte, out var point, out var @bool, out var bool2);
		sc.BeginFigure(point, @bool, bool2);
	}

	private static void DeserializeLineTo(BinaryReader br, byte firstByte, StreamGeometryContext sc)
	{
		DeserializePointAndTwoBools(br, firstByte, out var point, out var @bool, out var bool2);
		sc.LineTo(point, @bool, bool2);
	}

	private static void DeserializeQuadraticBezierTo(BinaryReader br, byte firstByte, StreamGeometryContext sc)
	{
		Point point = default(Point);
		DeserializePointAndTwoBools(br, firstByte, out var point2, out var @bool, out var bool2);
		point.X = XamlSerializationHelper.ReadDouble(br);
		point.Y = XamlSerializationHelper.ReadDouble(br);
		sc.QuadraticBezierTo(point2, point, @bool, bool2);
	}

	private static void DeserializeBezierTo(BinaryReader br, byte firstByte, StreamGeometryContext sc)
	{
		Point point = default(Point);
		Point point2 = default(Point);
		DeserializePointAndTwoBools(br, firstByte, out var point3, out var @bool, out var bool2);
		point.X = XamlSerializationHelper.ReadDouble(br);
		point.Y = XamlSerializationHelper.ReadDouble(br);
		point2.X = XamlSerializationHelper.ReadDouble(br);
		point2.Y = XamlSerializationHelper.ReadDouble(br);
		sc.BezierTo(point3, point, point2, @bool, bool2);
	}

	private static void DeserializePolyLineTo(BinaryReader br, byte firstByte, StreamGeometryContext sc)
	{
		bool @bool;
		bool bool2;
		IList<Point> points = DeserializeListOfPointsAndTwoBools(br, firstByte, out @bool, out bool2);
		sc.PolyLineTo(points, @bool, bool2);
	}

	private static void DeserializePolyQuadraticBezierTo(BinaryReader br, byte firstByte, StreamGeometryContext sc)
	{
		bool @bool;
		bool bool2;
		IList<Point> points = DeserializeListOfPointsAndTwoBools(br, firstByte, out @bool, out bool2);
		sc.PolyQuadraticBezierTo(points, @bool, bool2);
	}

	private static void DeserializePolyBezierTo(BinaryReader br, byte firstByte, StreamGeometryContext sc)
	{
		bool @bool;
		bool bool2;
		IList<Point> points = DeserializeListOfPointsAndTwoBools(br, firstByte, out @bool, out bool2);
		sc.PolyBezierTo(points, @bool, bool2);
	}

	private static void DeserializeArcTo(BinaryReader br, byte firstByte, StreamGeometryContext sc)
	{
		Size size = default(Size);
		DeserializePointAndTwoBools(br, firstByte, out var point, out var @bool, out var bool2);
		byte num = br.ReadByte();
		bool isLargeArc = (num & 0xF) != 0;
		SweepDirection sweepDirection = BoolToSweep((num & 0xF0) != 0);
		size.Width = XamlSerializationHelper.ReadDouble(br);
		size.Height = XamlSerializationHelper.ReadDouble(br);
		double rotationAngle = XamlSerializationHelper.ReadDouble(br);
		sc.ArcTo(point, size, rotationAngle, isLargeArc, sweepDirection, @bool, bool2);
	}

	private static void UnPackBools(byte packedByte, out bool bool1, out bool bool2)
	{
		bool1 = (packedByte & 0x10) != 0;
		bool2 = (packedByte & 0x20) != 0;
	}

	private static void UnPackBools(byte packedByte, out bool bool1, out bool bool2, out bool bool3, out bool bool4)
	{
		bool1 = (packedByte & 0x10) != 0;
		bool2 = (packedByte & 0x20) != 0;
		bool3 = (packedByte & 0x40) != 0;
		bool4 = (packedByte & 0x80) != 0;
	}

	private static ParserGeometryContextOpCodes UnPackOpCode(byte packedByte)
	{
		return (ParserGeometryContextOpCodes)(packedByte & 0xF);
	}

	private static IList<Point> DeserializeListOfPointsAndTwoBools(BinaryReader br, byte firstByte, out bool bool1, out bool bool2)
	{
		UnPackBools(firstByte, out bool1, out bool2);
		int num = br.ReadInt32();
		IList<Point> list = new List<Point>(num);
		for (int i = 0; i < num; i++)
		{
			Point item = new Point(XamlSerializationHelper.ReadDouble(br), XamlSerializationHelper.ReadDouble(br));
			list.Add(item);
		}
		return list;
	}

	private static void DeserializePointAndTwoBools(BinaryReader br, byte firstByte, out Point point, out bool bool1, out bool bool2)
	{
		bool bool3 = false;
		bool bool4 = false;
		UnPackBools(firstByte, out bool1, out bool2, out bool3, out bool4);
		point = new Point(DeserializeDouble(br, bool3), DeserializeDouble(br, bool4));
	}

	private static double DeserializeDouble(BinaryReader br, bool isScaledInt)
	{
		if (isScaledInt)
		{
			return XamlSerializationHelper.ReadScaledInteger(br);
		}
		return XamlSerializationHelper.ReadDouble(br);
	}

	private static SweepDirection BoolToSweep(bool value)
	{
		if (!value)
		{
			return SweepDirection.Counterclockwise;
		}
		return SweepDirection.Clockwise;
	}

	private static bool SweepToBool(SweepDirection sweep)
	{
		if (sweep == SweepDirection.Counterclockwise)
		{
			return false;
		}
		return true;
	}

	private static FillRule BoolToFillRule(bool value)
	{
		if (!value)
		{
			return FillRule.EvenOdd;
		}
		return FillRule.Nonzero;
	}

	private static bool FillRuleToBool(FillRule fill)
	{
		if (fill == FillRule.EvenOdd)
		{
			return false;
		}
		return true;
	}

	private void SerializePointAndTwoBools(ParserGeometryContextOpCodes opCode, Point point, bool bool1, bool bool2)
	{
		int intValue = 0;
		int intValue2 = 0;
		bool flag = XamlSerializationHelper.CanConvertToInteger(point.X, ref intValue);
		bool flag2 = XamlSerializationHelper.CanConvertToInteger(point.Y, ref intValue2);
		_bw.Write(PackByte(opCode, bool1, bool2, flag, flag2));
		SerializeDouble(point.X, flag, intValue);
		SerializeDouble(point.Y, flag2, intValue2);
	}

	private void SerializeListOfPointsAndTwoBools(ParserGeometryContextOpCodes opCode, IList<Point> points, bool bool1, bool bool2)
	{
		byte value = PackByte(opCode, bool1, bool2);
		_bw.Write(value);
		_bw.Write(points.Count);
		for (int i = 0; i < points.Count; i++)
		{
			XamlSerializationHelper.WriteDouble(_bw, points[i].X);
			XamlSerializationHelper.WriteDouble(_bw, points[i].Y);
		}
	}

	private void SerializeDouble(double value, bool isScaledInt, int scaledIntValue)
	{
		if (isScaledInt)
		{
			_bw.Write(scaledIntValue);
		}
		else
		{
			XamlSerializationHelper.WriteDouble(_bw, value);
		}
	}

	private static byte PackByte(ParserGeometryContextOpCodes opCode, bool bool1, bool bool2)
	{
		return PackByte(opCode, bool1, bool2, bool3: false, bool4: false);
	}

	private static byte PackByte(ParserGeometryContextOpCodes opCode, bool bool1, bool bool2, bool bool3, bool bool4)
	{
		byte b = (byte)opCode;
		if (b >= 16)
		{
			throw new ArgumentException(SR.UnknownPathOperationType);
		}
		if (bool1)
		{
			b |= 0x10;
		}
		if (bool2)
		{
			b |= 0x20;
		}
		if (bool3)
		{
			b |= 0x40;
		}
		if (bool4)
		{
			b |= 0x80;
		}
		return b;
	}
}
