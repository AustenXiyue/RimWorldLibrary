namespace System.Windows.Input;

internal class MultiTouchSystemGestureLogic
{
	private enum State
	{
		Idle,
		OneFingerDown,
		TwoFingersDown,
		OneFingerInStaticGesture,
		TwoFingersInWisptisGesture,
		OneFingerInWisptisGesture
	}

	private State _currentState;

	private int? _firstStylusDeviceId;

	private int? _secondStylusDeviceId;

	private int _firstDownTime;

	private int _firstUpTime;

	private const int TwoFingerTapTime = 150;

	private const int RolloverTime = 1158;

	internal MultiTouchSystemGestureLogic()
	{
		_currentState = State.Idle;
		Reset();
	}

	internal SystemGesture? GenerateStaticGesture(RawStylusInputReport stylusInputReport)
	{
		switch (stylusInputReport.Actions)
		{
		case RawStylusActions.Down:
			OnTouchDown(stylusInputReport);
			return null;
		case RawStylusActions.Up:
			return OnTouchUp(stylusInputReport);
		case RawStylusActions.SystemGesture:
			OnSystemGesture((RawStylusSystemGestureInputReport)stylusInputReport);
			return null;
		default:
			return null;
		}
	}

	private void OnTouchDown(RawStylusInputReport stylusInputReport)
	{
		switch (_currentState)
		{
		case State.Idle:
			Reset();
			_firstStylusDeviceId = stylusInputReport.StylusDeviceId;
			_currentState = State.OneFingerDown;
			_firstDownTime = Environment.TickCount;
			break;
		case State.OneFingerDown:
			_secondStylusDeviceId = stylusInputReport.StylusDeviceId;
			_currentState = State.TwoFingersDown;
			break;
		}
	}

	private SystemGesture? OnTouchUp(RawStylusInputReport stylusInputReport)
	{
		switch (_currentState)
		{
		case State.TwoFingersDown:
			if (IsTrackedStylusId(stylusInputReport.StylusDeviceId))
			{
				_firstUpTime = Environment.TickCount;
				_currentState = State.OneFingerInStaticGesture;
			}
			break;
		case State.OneFingerDown:
			if (IsTrackedStylusId(stylusInputReport.StylusDeviceId))
			{
				_currentState = State.Idle;
			}
			break;
		case State.OneFingerInStaticGesture:
			_currentState = State.Idle;
			if (IsTwoFingerTap())
			{
				return SystemGesture.TwoFingerTap;
			}
			break;
		case State.TwoFingersInWisptisGesture:
			if (IsTrackedStylusId(stylusInputReport.StylusDeviceId))
			{
				_currentState = State.OneFingerInWisptisGesture;
			}
			break;
		case State.OneFingerInWisptisGesture:
			if (IsTrackedStylusId(stylusInputReport.StylusDeviceId))
			{
				_currentState = State.Idle;
			}
			break;
		}
		return null;
	}

	private void OnSystemGesture(RawStylusSystemGestureInputReport stylusInputReport)
	{
		switch (_currentState)
		{
		case State.TwoFingersDown:
		{
			SystemGesture systemGesture = stylusInputReport.SystemGesture;
			if ((uint)(systemGesture - 19) <= 1u || systemGesture == SystemGesture.Flick)
			{
				_currentState = State.TwoFingersInWisptisGesture;
			}
			break;
		}
		case State.OneFingerDown:
		case State.OneFingerInStaticGesture:
		{
			SystemGesture systemGesture = stylusInputReport.SystemGesture;
			if ((uint)(systemGesture - 19) <= 1u || systemGesture == SystemGesture.Flick)
			{
				_currentState = State.OneFingerInWisptisGesture;
			}
			break;
		}
		}
	}

	private void Reset()
	{
		_firstStylusDeviceId = null;
		_secondStylusDeviceId = null;
		_firstDownTime = 0;
		_firstUpTime = 0;
	}

	private bool IsTrackedStylusId(int id)
	{
		if (id != _firstStylusDeviceId)
		{
			return id == _secondStylusDeviceId;
		}
		return true;
	}

	private bool IsTwoFingerTap()
	{
		int tickCount = Environment.TickCount;
		int num = tickCount - _firstDownTime;
		if (tickCount - _firstUpTime < 150)
		{
			return num < 1158;
		}
		return false;
	}
}
