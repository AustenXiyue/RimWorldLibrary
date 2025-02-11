using System.ComponentModel;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

[FriendAccessAllowed]
internal abstract class InputReport
{
	private SecurityCriticalData<PresentationSource> _inputSource;

	private InputType _type;

	private InputMode _mode;

	private int _timestamp;

	public PresentationSource InputSource => _inputSource.Value;

	public InputType Type => _type;

	public InputMode Mode => _mode;

	public int Timestamp => _timestamp;

	protected InputReport(PresentationSource inputSource, InputType type, InputMode mode, int timestamp)
	{
		if (inputSource == null)
		{
			throw new ArgumentNullException("inputSource");
		}
		Validate_InputType(type);
		Validate_InputMode(mode);
		_inputSource = new SecurityCriticalData<PresentationSource>(inputSource);
		_type = type;
		_mode = mode;
		_timestamp = timestamp;
	}

	private void Validate_InputMode(InputMode mode)
	{
		if ((uint)mode > 1u)
		{
			throw new InvalidEnumArgumentException("mode", (int)mode, typeof(InputMode));
		}
	}

	private void Validate_InputType(InputType type)
	{
		if ((uint)type > 5u)
		{
			throw new InvalidEnumArgumentException("type", (int)type, typeof(InputType));
		}
	}
}
