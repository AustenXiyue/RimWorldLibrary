namespace System.Windows.Documents;

internal struct PropertyRecord
{
	private DependencyProperty _property;

	private object _value;

	internal DependencyProperty Property
	{
		get
		{
			return _property;
		}
		set
		{
			_property = value;
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
}
