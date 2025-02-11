namespace System.Windows.Documents;

internal class RtfToken
{
	internal const long INVALID_PARAMETER = 268435456L;

	private RtfTokenType _type;

	private RtfControlWordInfo _rtfControlWordInfo;

	private long _parameter;

	private string _text;

	internal RtfTokenType Type
	{
		get
		{
			return _type;
		}
		set
		{
			_type = value;
		}
	}

	internal RtfControlWordInfo RtfControlWordInfo
	{
		get
		{
			return _rtfControlWordInfo;
		}
		set
		{
			_rtfControlWordInfo = value;
		}
	}

	internal long Parameter
	{
		get
		{
			if (!HasParameter)
			{
				return 0L;
			}
			return _parameter;
		}
		set
		{
			_parameter = value;
		}
	}

	internal string Text
	{
		get
		{
			return _text;
		}
		set
		{
			_text = value;
		}
	}

	internal long ToggleValue
	{
		get
		{
			if (!HasParameter)
			{
				return 1L;
			}
			return Parameter;
		}
	}

	internal bool FlagValue
	{
		get
		{
			if (HasParameter && (!HasParameter || Parameter <= 0))
			{
				return false;
			}
			return true;
		}
	}

	internal bool HasParameter => _parameter != 268435456;

	internal RtfToken()
	{
	}

	internal void Empty()
	{
		_type = RtfTokenType.TokenInvalid;
		_rtfControlWordInfo = null;
		_parameter = 268435456L;
		_text = "";
	}
}
