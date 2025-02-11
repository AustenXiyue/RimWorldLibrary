using System.Collections;
using System.Globalization;
using System.Threading;

namespace System.Diagnostics;

/// <summary>Provides trace event data specific to a thread and a process.</summary>
/// <filterpriority>2</filterpriority>
public class TraceEventCache
{
	private static volatile int processId;

	private static volatile string processName;

	private long timeStamp = -1L;

	private DateTime dateTime = DateTime.MinValue;

	private string stackTrace;

	internal Guid ActivityId => Trace.CorrelationManager.ActivityId;

	/// <summary>Gets the call stack for the current thread.</summary>
	/// <returns>A string containing stack trace information. This value can be an empty string ("").</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" />
	/// </PermissionSet>
	public string Callstack
	{
		get
		{
			if (stackTrace == null)
			{
				stackTrace = Environment.StackTrace;
			}
			return stackTrace;
		}
	}

	/// <summary>Gets the correlation data, contained in a stack. </summary>
	/// <returns>A <see cref="T:System.Collections.Stack" /> containing correlation data.</returns>
	/// <filterpriority>1</filterpriority>
	public Stack LogicalOperationStack => Trace.CorrelationManager.LogicalOperationStack;

	/// <summary>Gets the date and time at which the event trace occurred.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> structure whose value is a date and time expressed in Coordinated Universal Time (UTC).</returns>
	/// <filterpriority>2</filterpriority>
	public DateTime DateTime
	{
		get
		{
			if (dateTime == DateTime.MinValue)
			{
				dateTime = DateTime.UtcNow;
			}
			return dateTime;
		}
	}

	/// <summary>Gets the unique identifier of the current process.</summary>
	/// <returns>The system-generated unique identifier of the current process.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public int ProcessId => GetProcessId();

	/// <summary>Gets a unique identifier for the current managed thread.  </summary>
	/// <returns>A string that represents a unique integer identifier for this managed thread.</returns>
	/// <filterpriority>2</filterpriority>
	public string ThreadId => GetThreadId().ToString(CultureInfo.InvariantCulture);

	/// <summary>Gets the current number of ticks in the timer mechanism.</summary>
	/// <returns>The tick counter value of the underlying timer mechanism.</returns>
	/// <filterpriority>2</filterpriority>
	public long Timestamp
	{
		get
		{
			if (timeStamp == -1)
			{
				timeStamp = Stopwatch.GetTimestamp();
			}
			return timeStamp;
		}
	}

	private static void InitProcessInfo()
	{
		if (processName == null)
		{
			Process currentProcess = Process.GetCurrentProcess();
			try
			{
				processId = currentProcess.Id;
				processName = currentProcess.ProcessName;
			}
			finally
			{
				currentProcess.Dispose();
			}
		}
	}

	internal static int GetProcessId()
	{
		InitProcessInfo();
		return processId;
	}

	internal static string GetProcessName()
	{
		InitProcessInfo();
		return processName;
	}

	internal static int GetThreadId()
	{
		return Thread.CurrentThread.ManagedThreadId;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.TraceEventCache" /> class. </summary>
	public TraceEventCache()
	{
	}
}
