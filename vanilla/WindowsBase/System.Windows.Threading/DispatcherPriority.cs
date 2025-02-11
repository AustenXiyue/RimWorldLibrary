namespace System.Windows.Threading;

/// <summary>Describes the priorities at which operations can be invoked by way of the <see cref="T:System.Windows.Threading.Dispatcher" />. </summary>
public enum DispatcherPriority
{
	/// <summary>The enumeration value is -1.  This is an invalid priority.</summary>
	Invalid = -1,
	/// <summary>The enumeration value is 0.  Operations are not processed.</summary>
	Inactive,
	/// <summary>The enumeration value is 1.  Operations are processed when the system is idle.</summary>
	SystemIdle,
	/// <summary>The enumeration value is 2.  Operations are processed when the application is idle. </summary>
	ApplicationIdle,
	/// <summary>The enumeration value is 3.  Operations are processed after background operations have completed.</summary>
	ContextIdle,
	/// <summary>The enumeration value is 4.  Operations are processed after all other non-idle operations are completed.</summary>
	Background,
	/// <summary>The enumeration value is 5.  Operations are processed at the same priority as input.</summary>
	Input,
	/// <summary>The enumeration value is 6.  Operations are processed when layout and render has finished but just before items at input priority are serviced. Specifically this is used when raising the Loaded event.</summary>
	Loaded,
	/// <summary>The enumeration value is 7.  Operations processed at the same priority as rendering.</summary>
	Render,
	/// <summary>The enumeration value is 8.  Operations are processed at the same priority as data binding.</summary>
	DataBind,
	/// <summary>The enumeration value is 9.  Operations are processed at normal priority.  This is the typical application priority.</summary>
	Normal,
	/// <summary>The enumeration value is 10.  Operations are processed before other asynchronous operations.  This is the highest priority. </summary>
	Send
}
