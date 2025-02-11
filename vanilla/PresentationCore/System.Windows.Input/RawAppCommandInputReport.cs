namespace System.Windows.Input;

internal class RawAppCommandInputReport : InputReport
{
	private int _appCommand;

	private InputType _device;

	internal int AppCommand => _appCommand;

	internal InputType Device => _device;

	internal RawAppCommandInputReport(PresentationSource inputSource, InputMode mode, int timestamp, int appCommand, InputType device, InputType inputType)
		: base(inputSource, inputType, mode, timestamp)
	{
		_appCommand = appCommand;
		_device = device;
	}
}
