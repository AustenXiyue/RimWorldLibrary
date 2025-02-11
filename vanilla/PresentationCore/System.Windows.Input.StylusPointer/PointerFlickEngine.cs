namespace System.Windows.Input.StylusPointer;

internal class PointerFlickEngine
{
	internal class FlickResult
	{
		internal Point PhysicalStart { get; set; }

		internal Point TabletStart { get; set; }

		internal int PhysicalLength { get; set; }

		internal int TabletLength { get; set; }

		internal int DirectionDeg { get; set; }

		internal bool CanBeFlick { get; set; }

		internal bool IsLengthOk { get; set; }

		internal bool IsSpeedOk { get; set; }

		internal bool IsCurvatureOk { get; set; }

		internal bool IsLiftOk { get; set; }
	}

	private class FlickRecognitionData
	{
		internal Point PhysicalPoint { get; set; }

		internal double Time { get; set; }

		internal double Displacement { get; set; }

		internal double Velocity { get; set; }

		internal Point TabletPoint { get; set; }
	}

	[Flags]
	private enum FlickState
	{

	}

	private const double ThresholdTime = 150.0;

	private const double ThresholdLength = 100.0;

	private const double RelaxedFlickMinimumLength = 400.0;

	private const double RelaxedFlickMaximumLengthRatio = 1.1;

	private const double RelaxedFlickMinimumVelocity = 8.0;

	private const double RelaxedFlickMaximumTime = 300.0;

	private const double RelaxedFlickMaximumStationaryTime = 45.0;

	private const double RelaxedFlickMaxStationaryDispX = 150.0;

	private const double RelaxedFlickMaxStationaryDispY = 150.0;

	private const double PreciseFlickMinimumLength = 800.0;

	private const double PreciseFlickMaximumLengthRatio = 1.01;

	private const double PreciseFlickMinimumVelocity = 19.0;

	private const double PreciseFlickMaximumTime = 200.0;

	private const double PreciseFlickMaximumStationaryTime = 45.0;

	private const double PreciseFlickMaxStationaryDispX = 0.0;

	private const double PreciseFlickMaxStationaryDispY = 0.0;

	private bool _collectingData;

	private bool _analyzingData;

	private bool _lastPhysicalPointValid;

	private bool _movedEnoughFromPenDown;

	private bool _canDetectFlick;

	private bool _allowPressFlicks;

	private bool _previousFlickDataValid;

	private Point _flickStartPhysical;

	private Point _flickStartTablet;

	private Point _lastPhysicalPoint;

	private PointerStylusDevice _stylusDevice;

	private double _distance;

	private double _flickDirectionRadians;

	private double _flickPathDistance;

	private double _flickLength;

	private double _flickTimeLowVelocity;

	private double _flickMaximumStationaryTime;

	private double _flickMaximumLengthRatio;

	private double _flickMinimumLength;

	private double _flickMinimumVelocity;

	private double _flickMaximumStationaryDisplacementX;

	private double _flickMaximumStationaryDisplacementY;

	private double _tolerance;

	private FlickRecognitionData _previousFlickData;

	private Rect _drag;

	private double _timePeriod;

	private double _timePeriodAlpha;

	private int _previousTickCount;

	private double _elapsedTime;

	private double _flickTime;

	private double _flickMaximumTime;

	internal FlickResult Result { get; private set; } = new FlickResult();

	internal PointerFlickEngine(PointerStylusDevice stylusDevice)
	{
		_stylusDevice = stylusDevice;
		_timePeriod = 8.0;
		_timePeriodAlpha = 0.001;
		_collectingData = false;
		_analyzingData = false;
		_previousFlickDataValid = false;
		_allowPressFlicks = true;
		Reset();
		SetTolerance(0.5);
	}

	internal void Reset()
	{
		ResetResult();
		_collectingData = false;
		_analyzingData = false;
		_movedEnoughFromPenDown = !_allowPressFlicks;
		_canDetectFlick = true;
		_lastPhysicalPointValid = false;
		_distance = 0.0;
		_drag = default(Rect);
		_flickStartPhysical = default(Point);
		_flickStartTablet = default(Point);
		_elapsedTime = 0.0;
		_flickLength = 0.0;
		_flickDirectionRadians = 0.0;
		_flickPathDistance = 0.0;
		_flickTime = 0.0;
		_flickTimeLowVelocity = 0.0;
		_previousFlickDataValid = false;
	}

	internal void ResetResult()
	{
		Result.CanBeFlick = true;
		Result.IsLengthOk = false;
		Result.IsSpeedOk = false;
		Result.IsCurvatureOk = false;
		Result.IsLiftOk = false;
		Result.DirectionDeg = 0;
		Result.PhysicalLength = 0;
		Result.TabletLength = 0;
		Result.PhysicalStart = default(Point);
		Result.TabletStart = default(Point);
	}

	internal void Update(RawStylusInputReport rsir, bool initial = false)
	{
		if (_stylusDevice.TabletDevice.Type != 0)
		{
			return;
		}
		switch (rsir.Actions)
		{
		case RawStylusActions.Down:
			Reset();
			_collectingData = true;
			ProcessPacket(rsir, initial: true);
			if (_analyzingData)
			{
				Analyze(decide: false);
			}
			break;
		case RawStylusActions.Up:
			if (_canDetectFlick)
			{
				ProcessPacket(rsir, initial: false);
				if (_analyzingData)
				{
					Analyze(decide: true);
				}
			}
			_collectingData = false;
			_analyzingData = false;
			break;
		case RawStylusActions.Move:
			if (_canDetectFlick)
			{
				ProcessPacket(rsir, initial);
				if (_analyzingData)
				{
					Analyze(decide: false);
				}
			}
			break;
		}
	}

	private void UpdateTimePeriod(int tickCount, bool initial)
	{
		if (!_collectingData)
		{
			return;
		}
		if (!initial)
		{
			double num = tickCount - _previousTickCount;
			if (num >= 0.0 && num <= 1000.0)
			{
				_timePeriod = (1.0 - _timePeriodAlpha) * _timePeriod + _timePeriodAlpha * num;
			}
		}
		_previousTickCount = tickCount;
	}

	private void ProcessPacket(RawStylusInputReport rsir, bool initial)
	{
		UpdateTimePeriod(rsir.Timestamp, initial);
		if (!_collectingData)
		{
			return;
		}
		Point lastTabletPoint = rsir.GetLastTabletPoint();
		Point physicalCoordinates = GetPhysicalCoordinates(lastTabletPoint);
		if (initial)
		{
			_flickStartPhysical = physicalCoordinates;
			_flickStartTablet = lastTabletPoint;
			_elapsedTime = 0.0;
			SetStableRect();
		}
		else
		{
			_elapsedTime += _timePeriod;
		}
		if (!_movedEnoughFromPenDown)
		{
			if (_lastPhysicalPointValid)
			{
				double num = Distance(_lastPhysicalPoint, physicalCoordinates);
				_distance += num;
				if (num > 19.0 || num >= _flickMaximumStationaryDisplacementX)
				{
					_movedEnoughFromPenDown = true;
				}
			}
			if (!_movedEnoughFromPenDown && (!_drag.Contains(physicalCoordinates) || _elapsedTime > 3000.0))
			{
				_movedEnoughFromPenDown = true;
			}
			_lastPhysicalPoint = physicalCoordinates;
			_lastPhysicalPointValid = true;
		}
		if (_movedEnoughFromPenDown && !_analyzingData)
		{
			CheckWithThreshold(physicalCoordinates);
		}
		if (_analyzingData)
		{
			AddPoint(physicalCoordinates, lastTabletPoint);
		}
	}

	private void Analyze(bool decide)
	{
		Result.CanBeFlick = true;
		Result.IsLengthOk = true;
		Result.IsSpeedOk = true;
		Result.IsCurvatureOk = true;
		Result.IsLiftOk = true;
		Result.DirectionDeg = Convert.ToInt32(RadiansToDegrees(_flickDirectionRadians));
		Result.PhysicalStart = _flickStartPhysical;
		Result.TabletStart = _flickStartTablet;
		Result.PhysicalLength = Convert.ToInt32(0.5 + Distance(Result.PhysicalStart, _previousFlickData.PhysicalPoint));
		Result.TabletLength = Convert.ToInt32(0.5 + Distance(Result.TabletStart, _previousFlickData.TabletPoint));
		double num = _flickPathDistance - _flickLength;
		double num2 = 1.0;
		if (_flickLength > 0.0)
		{
			num2 = _flickPathDistance / _flickLength;
		}
		if (_flickTimeLowVelocity > _flickMaximumStationaryTime)
		{
			Result.CanBeFlick = false;
			Result.IsLiftOk = false;
		}
		if (_flickTime > _flickMaximumTime)
		{
			Result.CanBeFlick = false;
			Result.IsSpeedOk = false;
		}
		if ((num2 > _flickMaximumLengthRatio && _flickLength > 500.0 && num > 200.0) || num > 300.0)
		{
			Result.CanBeFlick = false;
			Result.IsCurvatureOk = false;
		}
		if (_flickLength < _flickMinimumLength && decide)
		{
			Result.CanBeFlick = false;
			Result.IsLengthOk = false;
		}
		if (!Result.CanBeFlick || decide)
		{
			_collectingData = false;
			_analyzingData = false;
		}
	}

	private void AddPoint(Point physicalPoint, Point tabletPoint)
	{
		FlickRecognitionData flickRecognitionData = new FlickRecognitionData
		{
			PhysicalPoint = physicalPoint,
			TabletPoint = tabletPoint,
			Time = 0.0,
			Displacement = 0.0,
			Velocity = 0.0
		};
		if (_previousFlickDataValid)
		{
			flickRecognitionData.Time = _previousFlickData.Time + _timePeriod;
			flickRecognitionData.Displacement = Distance(physicalPoint, _previousFlickData.PhysicalPoint);
			flickRecognitionData.Velocity = flickRecognitionData.Displacement / _timePeriod;
		}
		else
		{
			_flickPathDistance = Distance(physicalPoint, _flickStartPhysical);
		}
		_flickLength = Distance(physicalPoint, _flickStartPhysical);
		_flickDirectionRadians = Math.Atan2(flickRecognitionData.PhysicalPoint.Y - _flickStartPhysical.Y, flickRecognitionData.PhysicalPoint.X - _flickStartPhysical.X);
		_flickPathDistance += flickRecognitionData.Displacement;
		_flickTime += _timePeriod;
		_flickTimeLowVelocity += ((flickRecognitionData.Velocity < _flickMinimumVelocity) ? _timePeriod : 0.0);
		_previousFlickDataValid = true;
		_previousFlickData = flickRecognitionData;
	}

	private void CheckWithThreshold(Point physicalPoint)
	{
		_analyzingData = Distance(physicalPoint, _flickStartPhysical) > 100.0 || _elapsedTime > 150.0;
	}

	private void SetStableRect()
	{
		if (_collectingData)
		{
			_drag = new Rect(_flickStartPhysical, new Size(_flickMaximumStationaryDisplacementX, _flickMaximumStationaryDisplacementY));
		}
	}

	private double RadiansToDegrees(double radians)
	{
		return (180.0 * radians / Math.PI + 360.0) % 360.0;
	}

	private double Distance(Point p1, Point p2)
	{
		return Math.Sqrt(Math.Pow(p1.X - p2.X, 2.0) + Math.Pow(p1.Y - p2.Y, 2.0));
	}

	private Point GetPhysicalCoordinates(Point tabletPoint)
	{
		double num = _stylusDevice.PointerTabletDevice.DeviceInfo.DeviceRect.right - _stylusDevice.PointerTabletDevice.DeviceInfo.DeviceRect.left;
		double num2 = _stylusDevice.PointerTabletDevice.DeviceInfo.DeviceRect.top - _stylusDevice.PointerTabletDevice.DeviceInfo.DeviceRect.bottom;
		double width = _stylusDevice.PointerTabletDevice.DeviceInfo.SizeInfo.TabletSize.Width;
		double height = _stylusDevice.PointerTabletDevice.DeviceInfo.SizeInfo.TabletSize.Height;
		return new Point(tabletPoint.X * num / width, tabletPoint.Y * num2 / height);
	}

	private bool SetTolerance(double tolerance)
	{
		int num;
		if (tolerance > 0.0)
		{
			num = ((tolerance < 1.0) ? 1 : 0);
			if (num != 0)
			{
				_flickMinimumLength = tolerance * 800.0 + (1.0 - tolerance) * 400.0;
				_flickMaximumLengthRatio = tolerance * 1.01 + (1.0 - tolerance) * 1.1;
				_flickMinimumVelocity = tolerance * 19.0 + (1.0 - tolerance) * 8.0;
				_flickMaximumTime = tolerance * 200.0 + (1.0 - tolerance) * 300.0;
				_flickMaximumStationaryTime = tolerance * 45.0 + (1.0 - tolerance) * 45.0;
				_flickMaximumStationaryDisplacementX = tolerance * 0.0 + (1.0 - tolerance) * 150.0;
				_flickMaximumStationaryDisplacementY = tolerance * 0.0 + (1.0 - tolerance) * 150.0;
				_tolerance = tolerance;
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}
}
