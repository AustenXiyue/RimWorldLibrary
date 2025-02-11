namespace MS.Internal.Data;

internal class AsyncSetValueRequest : AsyncDataRequest
{
	private object _item;

	private string _propertyName;

	private object _value;

	public object TargetItem => _item;

	public object Value => _value;

	internal AsyncSetValueRequest(object item, string propertyName, object value, object bindingState, AsyncRequestCallback workCallback, AsyncRequestCallback completedCallback, params object[] args)
		: base(bindingState, workCallback, completedCallback, args)
	{
		_item = item;
		_propertyName = propertyName;
		_value = value;
	}
}
