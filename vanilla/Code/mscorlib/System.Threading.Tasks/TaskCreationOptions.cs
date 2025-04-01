namespace System.Threading.Tasks;

/// <summary>Specifies flags that control optional behavior for the creation and execution of tasks.</summary>
[Serializable]
[Flags]
public enum TaskCreationOptions
{
	/// <summary>Specifies that the default behavior should be used.</summary>
	None = 0,
	/// <summary>A hint to a <see cref="T:System.Threading.Tasks.TaskScheduler" /> to schedule a task in as fair a manner as possible, meaning that tasks scheduled sooner will be more likely to be run sooner, and tasks scheduled later will be more likely to be run later.</summary>
	PreferFairness = 1,
	/// <summary>Specifies that a task will be a long-running, coarse-grained operation involving fewer, larger components than fine-grained systems. It provides a hint to the <see cref="T:System.Threading.Tasks.TaskScheduler" /> that oversubscription may be warranted. Oversubscription lets you create more threads than the available number of hardware threads.</summary>
	LongRunning = 2,
	/// <summary>Specifies that a task is attached to a parent in the task hierarchy. For more information, see Nested Tasks and Child Tasks.</summary>
	AttachedToParent = 4,
	/// <summary>Specifies that an <see cref="T:System.InvalidOperationException" /> will be thrown if an attempt is made to attach a child task to the created task.</summary>
	DenyChildAttach = 8,
	/// <summary>Prevents the ambient scheduler from being seen as the current scheduler in the created task. This means that operations like StartNew or ContinueWith that are performed in the created task will see <see cref="P:System.Threading.Tasks.TaskScheduler.Default" /> as the current scheduler.</summary>
	HideScheduler = 0x10,
	RunContinuationsAsynchronously = 0x40
}
