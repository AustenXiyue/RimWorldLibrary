namespace System.Windows.Threading;

/// <summary>Describes the possible values for the status of a <see cref="T:System.Windows.Threading.DispatcherOperation" />. </summary>
public enum DispatcherOperationStatus
{
	/// <summary>The operation is pending and is still in the <see cref="T:System.Windows.Threading.Dispatcher" /> queue.</summary>
	Pending,
	/// <summary>The operation has aborted. </summary>
	Aborted,
	/// <summary>The operation is completed. </summary>
	Completed,
	/// <summary>The operation started executing, but has not completed. </summary>
	Executing
}
