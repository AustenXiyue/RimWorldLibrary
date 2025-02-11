using System.Windows;

namespace MS.Internal.Ink;

internal readonly struct StrokeNodeData
{
	private static readonly StrokeNodeData s_empty;

	private readonly Point _position;

	private readonly float _pressure;

	internal static ref readonly StrokeNodeData Empty => ref s_empty;

	internal bool IsEmpty => DoubleUtil.AreClose(_pressure, s_empty._pressure);

	internal Point Position => _position;

	internal float PressureFactor => _pressure;

	internal StrokeNodeData(Point position)
	{
		_position = position;
		_pressure = 1f;
	}

	internal StrokeNodeData(Point position, float pressure)
	{
		_position = position;
		_pressure = pressure;
	}
}
