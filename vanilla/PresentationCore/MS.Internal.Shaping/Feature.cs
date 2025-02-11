namespace MS.Internal.Shaping;

internal class Feature
{
	private ushort _startIndex;

	private ushort _length;

	private uint _tag;

	private uint _parameter;

	public uint Tag
	{
		get
		{
			return _tag;
		}
		set
		{
			_tag = value;
		}
	}

	public uint Parameter
	{
		get
		{
			return _parameter;
		}
		set
		{
			_parameter = value;
		}
	}

	public ushort StartIndex
	{
		get
		{
			return _startIndex;
		}
		set
		{
			_startIndex = value;
		}
	}

	public ushort Length
	{
		get
		{
			return _length;
		}
		set
		{
			_length = value;
		}
	}

	public Feature(ushort startIndex, ushort length, uint tag, uint parameter)
	{
		_startIndex = startIndex;
		_length = length;
		_tag = tag;
		_parameter = parameter;
	}
}
