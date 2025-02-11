namespace MS.Internal.Globalization;

internal class PropertyComment
{
	private string _target;

	private object _value;

	internal string PropertyName
	{
		get
		{
			return _target;
		}
		set
		{
			_target = value;
		}
	}

	internal object Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}

	internal PropertyComment()
	{
	}
}
