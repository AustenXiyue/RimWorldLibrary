using System.ComponentModel;

namespace System.Windows.Input;

internal class RawUIStateInputReport : InputReport
{
	private RawUIStateActions _action;

	private RawUIStateTargets _targets;

	public RawUIStateActions Action => _action;

	public RawUIStateTargets Targets => _targets;

	public RawUIStateInputReport(PresentationSource inputSource, InputMode mode, int timestamp, RawUIStateActions action, RawUIStateTargets targets)
		: base(inputSource, InputType.Keyboard, mode, timestamp)
	{
		if (!IsValidRawUIStateAction(action))
		{
			throw new InvalidEnumArgumentException("action", (int)action, typeof(RawUIStateActions));
		}
		if (!IsValidRawUIStateTargets(targets))
		{
			throw new InvalidEnumArgumentException("targets", (int)targets, typeof(RawUIStateTargets));
		}
		_action = action;
		_targets = targets;
	}

	internal static bool IsValidRawUIStateAction(RawUIStateActions action)
	{
		if (action != RawUIStateActions.Set && action != RawUIStateActions.Clear)
		{
			return action == RawUIStateActions.Initialize;
		}
		return true;
	}

	internal static bool IsValidRawUIStateTargets(RawUIStateTargets targets)
	{
		return (targets & (RawUIStateTargets.HideFocus | RawUIStateTargets.HideAccelerators | RawUIStateTargets.Active)) == targets;
	}
}
