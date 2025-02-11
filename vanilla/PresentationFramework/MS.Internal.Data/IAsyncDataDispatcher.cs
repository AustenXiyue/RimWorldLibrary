namespace MS.Internal.Data;

internal interface IAsyncDataDispatcher
{
	void AddRequest(AsyncDataRequest request);

	void CancelAllRequests();
}
