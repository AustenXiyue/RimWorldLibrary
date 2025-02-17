namespace System.Threading.Tasks;

/// <summary>Provides completion status on the execution of a <see cref="T:System.Threading.Tasks.Parallel" /> loop.</summary>
public struct ParallelLoopResult
{
	internal bool m_completed;

	internal long? m_lowestBreakIteration;

	/// <summary>Gets whether the loop ran to completion, such that all iterations of the loop were executed and the loop didn't receive a request to end prematurely.</summary>
	/// <returns>true if the loop ran to completion; otherwise false;</returns>
	public bool IsCompleted => m_completed;

	/// <summary>Gets the index of the lowest iteration from which <see cref="M:System.Threading.Tasks.ParallelLoopState.Break" /> was called.</summary>
	/// <returns>Returns an integer that represents the lowest iteration from which the Break statement was called.</returns>
	public long? LowestBreakIteration => m_lowestBreakIteration;
}
