namespace System.Windows.Documents;

internal class RtfControlWordInfo
{
	private RtfControlWord _controlWord;

	private string _controlName;

	private uint _flags;

	internal RtfControlWord Control => _controlWord;

	internal string ControlName => _controlName;

	internal uint Flags => _flags;

	internal RtfControlWordInfo(RtfControlWord controlWord, string controlName, uint flags)
	{
		_controlWord = controlWord;
		_controlName = controlName;
		_flags = flags;
	}
}
