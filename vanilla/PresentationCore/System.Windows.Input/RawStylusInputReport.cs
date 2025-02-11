using System.ComponentModel;
using System.Windows.Input.StylusPlugIns;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

internal class RawStylusInputReport : InputReport
{
	private RawStylusActions _actions;

	private int _tabletDeviceId;

	private int _stylusDeviceId;

	private bool _isQueued;

	private int[] _data;

	private StylusDevice _stylusDevice;

	private MS.Internal.SecurityCriticalDataForSet<RawStylusInput> _rawStylusInput;

	private bool _isSynchronize;

	private Func<StylusPointDescription> _stylusPointDescGenerator;

	internal RawStylusInput RawStylusInput
	{
		get
		{
			return _rawStylusInput.Value;
		}
		set
		{
			_rawStylusInput.Value = value;
		}
	}

	internal bool Synchronized
	{
		get
		{
			return _isSynchronize;
		}
		set
		{
			_isSynchronize = value;
		}
	}

	internal RawStylusActions Actions => _actions;

	internal int TabletDeviceId => _tabletDeviceId;

	internal PenContext PenContext { get; private set; }

	internal StylusPointDescription StylusPointDescription => _stylusPointDescGenerator();

	internal int StylusDeviceId => _stylusDeviceId;

	internal StylusDevice StylusDevice
	{
		get
		{
			return _stylusDevice;
		}
		set
		{
			_stylusDevice = value;
		}
	}

	internal bool IsQueued
	{
		get
		{
			return _isQueued;
		}
		set
		{
			_isQueued = value;
		}
	}

	internal int[] Data => _data;

	internal RawStylusInputReport(InputMode mode, int timestamp, PresentationSource inputSource, PenContext penContext, RawStylusActions actions, int tabletDeviceId, int stylusDeviceId, int[] data)
		: this(mode, timestamp, inputSource, actions, () => penContext.StylusPointDescription, tabletDeviceId, stylusDeviceId, data)
	{
		if (!RawStylusActionsHelper.IsValid(actions))
		{
			throw new InvalidEnumArgumentException(SR.Format(SR.Enum_Invalid, "actions"));
		}
		if (data == null && actions != RawStylusActions.InRange)
		{
			throw new ArgumentNullException("data");
		}
		_actions = actions;
		_data = data;
		_isSynchronize = false;
		_tabletDeviceId = tabletDeviceId;
		_stylusDeviceId = stylusDeviceId;
		PenContext = penContext;
	}

	internal RawStylusInputReport(InputMode mode, int timestamp, PresentationSource inputSource, RawStylusActions actions, Func<StylusPointDescription> stylusPointDescGenerator, int tabletDeviceId, int stylusDeviceId, int[] data)
		: base(inputSource, InputType.Stylus, mode, timestamp)
	{
		if (!RawStylusActionsHelper.IsValid(actions))
		{
			throw new InvalidEnumArgumentException(SR.Format(SR.Enum_Invalid, "actions"));
		}
		if (data == null && actions != RawStylusActions.InRange)
		{
			throw new ArgumentNullException("data");
		}
		_actions = actions;
		_stylusPointDescGenerator = stylusPointDescGenerator;
		_data = data;
		_isSynchronize = false;
		_tabletDeviceId = tabletDeviceId;
		_stylusDeviceId = stylusDeviceId;
	}

	internal int[] GetRawPacketData()
	{
		if (_data == null)
		{
			return null;
		}
		return (int[])_data.Clone();
	}

	internal Point GetLastTabletPoint()
	{
		int inputArrayLengthPerPoint = StylusPointDescription.GetInputArrayLengthPerPoint();
		int num = _data.Length - inputArrayLengthPerPoint;
		return new Point(_data[num], _data[num + 1]);
	}
}
