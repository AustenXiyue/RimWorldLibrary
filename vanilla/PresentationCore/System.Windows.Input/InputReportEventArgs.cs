using MS.Internal.PresentationCore;

namespace System.Windows.Input;

[FriendAccessAllowed]
internal class InputReportEventArgs : InputEventArgs
{
	private InputReport _report;

	public InputReport Report => _report;

	public InputReportEventArgs(InputDevice inputDevice, InputReport report)
		: base(inputDevice, report?.Timestamp ?? (-1))
	{
		if (report == null)
		{
			throw new ArgumentNullException("report");
		}
		_report = report;
	}

	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((InputReportEventHandler)genericHandler)(genericTarget, this);
	}
}
