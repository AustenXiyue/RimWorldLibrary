namespace MS.Internal.Data;

internal class AsyncGetValueRequest : AsyncDataRequest
{
	private object _item;

	private string _propertyName;

	public object SourceItem => _item;

	internal AsyncGetValueRequest(object item, string propertyName, object bindingState, AsyncRequestCallback workCallback, AsyncRequestCallback completedCallback, params object[] args)
		: base(bindingState, workCallback, completedCallback, args)
	{
		_item = item;
		_propertyName = propertyName;
	}
}
