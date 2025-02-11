namespace System.Windows.Threading;

internal class DispatcherOperationTaskMapping
{
	public DispatcherOperation Operation { get; private set; }

	public DispatcherOperationTaskMapping(DispatcherOperation operation)
	{
		Operation = operation;
	}
}
