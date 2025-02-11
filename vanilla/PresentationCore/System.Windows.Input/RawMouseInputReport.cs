using System.ComponentModel;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

[FriendAccessAllowed]
internal class RawMouseInputReport : InputReport
{
	private RawMouseActions _actions;

	private int _x;

	private int _y;

	private int _wheel;

	internal bool _isSynchronize;

	private SecurityCriticalData<nint> _extraInformation;

	public RawMouseActions Actions => _actions;

	public int X => _x;

	public int Y => _y;

	public int Wheel => _wheel;

	public nint ExtraInformation => _extraInformation.Value;

	public RawMouseInputReport(InputMode mode, int timestamp, PresentationSource inputSource, RawMouseActions actions, int x, int y, int wheel, nint extraInformation)
		: base(inputSource, InputType.Mouse, mode, timestamp)
	{
		if (!IsValidRawMouseActions(actions))
		{
			throw new InvalidEnumArgumentException("actions", (int)actions, typeof(RawMouseActions));
		}
		_actions = actions;
		_x = x;
		_y = y;
		_wheel = wheel;
		_extraInformation = new SecurityCriticalData<nint>(extraInformation);
	}

	internal static bool IsValidRawMouseActions(RawMouseActions actions)
	{
		if (actions == RawMouseActions.None)
		{
			return true;
		}
		if (((RawMouseActions.AttributesChanged | RawMouseActions.Activate | RawMouseActions.Deactivate | RawMouseActions.RelativeMove | RawMouseActions.AbsoluteMove | RawMouseActions.VirtualDesktopMove | RawMouseActions.Button1Press | RawMouseActions.Button1Release | RawMouseActions.Button2Press | RawMouseActions.Button2Release | RawMouseActions.Button3Press | RawMouseActions.Button3Release | RawMouseActions.Button4Press | RawMouseActions.Button4Release | RawMouseActions.Button5Press | RawMouseActions.Button5Release | RawMouseActions.VerticalWheelRotate | RawMouseActions.HorizontalWheelRotate | RawMouseActions.QueryCursor | RawMouseActions.CancelCapture) & actions) == actions && ((RawMouseActions.Deactivate & actions) != actions || RawMouseActions.Deactivate == actions) && ((RawMouseActions.Button1Press | RawMouseActions.Button1Release) & actions) != (RawMouseActions.Button1Press | RawMouseActions.Button1Release) && ((RawMouseActions.Button2Press | RawMouseActions.Button2Release) & actions) != (RawMouseActions.Button2Press | RawMouseActions.Button2Release) && ((RawMouseActions.Button3Press | RawMouseActions.Button3Release) & actions) != (RawMouseActions.Button3Press | RawMouseActions.Button3Release) && ((RawMouseActions.Button4Press | RawMouseActions.Button4Release) & actions) != (RawMouseActions.Button4Press | RawMouseActions.Button4Release) && ((RawMouseActions.Button5Press | RawMouseActions.Button5Release) & actions) != (RawMouseActions.Button5Press | RawMouseActions.Button5Release))
		{
			return true;
		}
		return false;
	}
}
