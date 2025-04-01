using System.Collections;
using System.Runtime.Remoting.Messaging;

namespace System.Diagnostics;

/// <summary>Correlates traces that are part of a logical transaction.</summary>
/// <filterpriority>2</filterpriority>
public class CorrelationManager
{
	private const string transactionSlotName = "System.Diagnostics.Trace.CorrelationManagerSlot";

	private const string activityIdSlotName = "E2ETrace.ActivityID";

	/// <summary>Gets or sets the identity for a global activity.</summary>
	/// <returns>A <see cref="T:System.Guid" /> structure that identifies the global activity.</returns>
	/// <filterpriority>1</filterpriority>
	public Guid ActivityId
	{
		get
		{
			object obj = CallContext.LogicalGetData("E2ETrace.ActivityID");
			if (obj != null)
			{
				return (Guid)obj;
			}
			return Guid.Empty;
		}
		set
		{
			CallContext.LogicalSetData("E2ETrace.ActivityID", value);
		}
	}

	/// <summary>Gets the logical operation stack from the call context.</summary>
	/// <returns>A <see cref="T:System.Collections.Stack" /> object that represents the logical operation stack for the call context.</returns>
	/// <filterpriority>1</filterpriority>
	public Stack LogicalOperationStack => GetLogicalOperationStack();

	internal CorrelationManager()
	{
	}

	/// <summary>Starts a logical operation with the specified identity on a thread.</summary>
	/// <param name="operationId">An object identifying the operation.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="operationId" /> parameter is null. </exception>
	/// <filterpriority>1</filterpriority>
	public void StartLogicalOperation(object operationId)
	{
		if (operationId == null)
		{
			throw new ArgumentNullException("operationId");
		}
		GetLogicalOperationStack().Push(operationId);
	}

	/// <summary>Starts a logical operation on a thread.</summary>
	/// <filterpriority>1</filterpriority>
	public void StartLogicalOperation()
	{
		StartLogicalOperation(Guid.NewGuid());
	}

	/// <summary>Stops the current logical operation.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Diagnostics.CorrelationManager.LogicalOperationStack" /> property is an empty stack.</exception>
	/// <filterpriority>1</filterpriority>
	public void StopLogicalOperation()
	{
		GetLogicalOperationStack().Pop();
	}

	private Stack GetLogicalOperationStack()
	{
		Stack stack = CallContext.LogicalGetData("System.Diagnostics.Trace.CorrelationManagerSlot") as Stack;
		if (stack == null)
		{
			stack = new Stack();
			CallContext.LogicalSetData("System.Diagnostics.Trace.CorrelationManagerSlot", stack);
		}
		return stack;
	}
}
