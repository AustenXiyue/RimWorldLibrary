using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.Diagnostics;

/// <summary>Provides access to local and remote processes and enables you to start and stop local system processes.</summary>
/// <filterpriority>1</filterpriority>
[MonitoringDescription("Provides access to local and remote processes, enabling starting and stopping of local processes.")]
[DefaultEvent("Exited")]
[Designer("System.Diagnostics.Design.ProcessDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
[DefaultProperty("StartInfo")]
[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
[HostProtection(SecurityAction.LinkDemand, SharedState = true, Synchronization = true, ExternalProcessMgmt = true, SelfAffectingProcessMgmt = true)]
[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
public class Process : Component
{
	private enum StreamReadMode
	{
		undefined,
		syncMode,
		asyncMode
	}

	private enum State
	{
		HaveId = 1,
		IsLocal = 2,
		IsNt = 4,
		HaveProcessInfo = 8,
		Exited = 16,
		Associated = 32,
		IsWin2k = 64,
		HaveNtProcessInfo = 12
	}

	private struct ProcInfo
	{
		public IntPtr process_handle;

		public int pid;

		public string[] envVariables;

		public string UserName;

		public string Domain;

		public IntPtr Password;

		public bool LoadUserProfile;
	}

	private bool haveProcessId;

	private int processId;

	private bool haveProcessHandle;

	private SafeProcessHandle m_processHandle;

	private bool isRemoteMachine;

	private string machineName;

	private int m_processAccess;

	private ProcessThreadCollection threads;

	private ProcessModuleCollection modules;

	private bool haveWorkingSetLimits;

	private IntPtr minWorkingSet;

	private IntPtr maxWorkingSet;

	private bool havePriorityClass;

	private ProcessPriorityClass priorityClass;

	private ProcessStartInfo startInfo;

	private bool watchForExit;

	private bool watchingForExit;

	private EventHandler onExited;

	private bool exited;

	private int exitCode;

	private bool signaled;

	private DateTime exitTime;

	private bool haveExitTime;

	private bool raisedOnExited;

	private RegisteredWaitHandle registeredWaitHandle;

	private WaitHandle waitHandle;

	private ISynchronizeInvoke synchronizingObject;

	private StreamReader standardOutput;

	private StreamWriter standardInput;

	private StreamReader standardError;

	private OperatingSystem operatingSystem;

	private bool disposed;

	private StreamReadMode outputStreamReadMode;

	private StreamReadMode errorStreamReadMode;

	private StreamReadMode inputStreamReadMode;

	internal AsyncStreamReader output;

	internal AsyncStreamReader error;

	internal bool pendingOutputRead;

	internal bool pendingErrorRead;

	internal static TraceSwitch processTracing;

	private string process_name;

	private static ProcessModule current_main_module;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("Indicates if the process component is associated with a real process.")]
	[Browsable(false)]
	private bool Associated
	{
		get
		{
			if (!haveProcessId)
			{
				return haveProcessHandle;
			}
			return true;
		}
	}

	/// <summary>Gets the value that the associated process specified when it terminated.</summary>
	/// <returns>The code that the associated process specified when it terminated.</returns>
	/// <exception cref="T:System.InvalidOperationException">The process has not exited.-or- The process <see cref="P:System.Diagnostics.Process.Handle" /> is not valid. </exception>
	/// <exception cref="T:System.NotSupportedException">You are trying to access the <see cref="P:System.Diagnostics.Process.ExitCode" /> property for a process that is running on a remote computer. This property is available only for processes that are running on the local computer.</exception>
	/// <filterpriority>1</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	[MonitoringDescription("The value returned from the associated process when it terminated.")]
	public int ExitCode
	{
		get
		{
			EnsureState(State.Exited);
			if (exitCode == -1 && !Environment.IsRunningOnWindows)
			{
				throw new InvalidOperationException("Cannot get the exit code from a non-child process on Unix");
			}
			return exitCode;
		}
	}

	/// <summary>Gets a value indicating whether the associated process has been terminated.</summary>
	/// <returns>true if the operating system process referenced by the <see cref="T:System.Diagnostics.Process" /> component has terminated; otherwise, false.</returns>
	/// <exception cref="T:System.InvalidOperationException">There is no process associated with the object. </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The exit code for the process could not be retrieved. </exception>
	/// <exception cref="T:System.NotSupportedException">You are trying to access the <see cref="P:System.Diagnostics.Process.HasExited" /> property for a process that is running on a remote computer. This property is available only for processes that are running on the local computer.</exception>
	/// <filterpriority>1</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("Indicates if the associated process has been terminated.")]
	[Browsable(false)]
	public bool HasExited
	{
		get
		{
			if (!exited)
			{
				EnsureState(State.Associated);
				SafeProcessHandle safeProcessHandle = null;
				try
				{
					safeProcessHandle = GetProcessHandle(1049600, throwIfExited: false);
					int num;
					if (safeProcessHandle.IsInvalid)
					{
						exited = true;
					}
					else if (NativeMethods.GetExitCodeProcess(safeProcessHandle, out num) && num != 259)
					{
						exited = true;
						exitCode = num;
					}
					else
					{
						if (!signaled)
						{
							ProcessWaitHandle processWaitHandle = null;
							try
							{
								processWaitHandle = new ProcessWaitHandle(safeProcessHandle);
								signaled = processWaitHandle.WaitOne(0, exitContext: false);
							}
							finally
							{
								processWaitHandle?.Close();
							}
						}
						if (signaled)
						{
							if (!NativeMethods.GetExitCodeProcess(safeProcessHandle, out num))
							{
								throw new Win32Exception();
							}
							exited = true;
							exitCode = num;
						}
					}
				}
				finally
				{
					ReleaseProcessHandle(safeProcessHandle);
				}
				if (exited)
				{
					RaiseOnExited();
				}
			}
			return exited;
		}
	}

	/// <summary>Gets the time that the associated process exited.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> that indicates when the associated process was terminated.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property. </exception>
	/// <exception cref="T:System.NotSupportedException">You are trying to access the <see cref="P:System.Diagnostics.Process.ExitTime" /> property for a process that is running on a remote computer. This property is available only for processes that are running on the local computer.</exception>
	/// <filterpriority>1</filterpriority>
	[MonitoringDescription("The time that the associated process exited.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public DateTime ExitTime
	{
		get
		{
			if (!haveExitTime)
			{
				EnsureState((State)20);
				exitTime = GetProcessTimes().ExitTime;
				haveExitTime = true;
			}
			return exitTime;
		}
	}

	/// <summary>Gets the native handle of the associated process.</summary>
	/// <returns>The handle that the operating system assigned to the associated process when the process was started. The system uses this handle to keep track of process attributes.</returns>
	/// <exception cref="T:System.InvalidOperationException">The process has not been started or has exited. The <see cref="P:System.Diagnostics.Process.Handle" /> property cannot be read because there is no process associated with this <see cref="T:System.Diagnostics.Process" /> instance.-or- The <see cref="T:System.Diagnostics.Process" /> instance has been attached to a running process but you do not have the necessary permissions to get a handle with full access rights. </exception>
	/// <exception cref="T:System.NotSupportedException">You are trying to access the <see cref="P:System.Diagnostics.Process.Handle" /> property for a process that is running on a remote computer. This property is available only for processes that are running on the local computer.</exception>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("Returns the native handle for this process.   The handle is only available if the process was started using this component.")]
	public IntPtr Handle
	{
		get
		{
			EnsureState(State.Associated);
			return OpenProcessHandle(m_processAccess).DangerousGetHandle();
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public SafeProcessHandle SafeHandle
	{
		get
		{
			EnsureState(State.Associated);
			return OpenProcessHandle(m_processAccess);
		}
	}

	/// <summary>Gets the unique identifier for the associated process.</summary>
	/// <returns>The system-generated unique identifier of the process that is referenced by this <see cref="T:System.Diagnostics.Process" /> instance.</returns>
	/// <exception cref="T:System.InvalidOperationException">The process's <see cref="P:System.Diagnostics.Process.Id" /> property has not been set.-or- There is no process associated with this <see cref="T:System.Diagnostics.Process" /> object. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me); set the <see cref="P:System.Diagnostics.ProcessStartInfo.UseShellExecute" /> property to false to access this property on Windows 98 and Windows Me.</exception>
	/// <filterpriority>1</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("The unique identifier for the process.")]
	public int Id
	{
		get
		{
			EnsureState(State.HaveId);
			return processId;
		}
	}

	/// <summary>Gets the name of the computer the associated process is running on.</summary>
	/// <returns>The name of the computer that the associated process is running on.</returns>
	/// <exception cref="T:System.InvalidOperationException">There is no process associated with this <see cref="T:System.Diagnostics.Process" /> object. </exception>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("The name of the machine the running the process.")]
	public string MachineName
	{
		get
		{
			EnsureState(State.Associated);
			return machineName;
		}
	}

	/// <summary>Gets or sets the maximum allowable working set size for the associated process.</summary>
	/// <returns>The maximum working set size that is allowed in memory for the process, in bytes.</returns>
	/// <exception cref="T:System.ArgumentException">The maximum working set size is invalid. It must be greater than or equal to the minimum working set size.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">Working set information cannot be retrieved from the associated process resource.-or- The process identifier or process handle is zero because the process has not been started. </exception>
	/// <exception cref="T:System.NotSupportedException">You are trying to access the <see cref="P:System.Diagnostics.Process.MaxWorkingSet" /> property for a process that is running on a remote computer. This property is available only for processes that are running on the local computer.</exception>
	/// <exception cref="T:System.InvalidOperationException">The process <see cref="P:System.Diagnostics.Process.Id" /> is not available.-or- The process has exited. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property. </exception>
	/// <filterpriority>2</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("The maximum amount of physical memory the process has required since it was started.")]
	public IntPtr MaxWorkingSet
	{
		get
		{
			EnsureWorkingSetLimits();
			return maxWorkingSet;
		}
		set
		{
			SetWorkingSetLimits(null, value);
		}
	}

	/// <summary>Gets or sets the minimum allowable working set size for the associated process.</summary>
	/// <returns>The minimum working set size that is required in memory for the process, in bytes.</returns>
	/// <exception cref="T:System.ArgumentException">The minimum working set size is invalid. It must be less than or equal to the maximum working set size.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">Working set information cannot be retrieved from the associated process resource.-or- The process identifier or process handle is zero because the process has not been started. </exception>
	/// <exception cref="T:System.NotSupportedException">You are trying to access the <see cref="P:System.Diagnostics.Process.MinWorkingSet" /> property for a process that is running on a remote computer. This property is available only for processes that are running on the local computer. </exception>
	/// <exception cref="T:System.InvalidOperationException">The process <see cref="P:System.Diagnostics.Process.Id" /> is not available.-or- The process has exited.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property. </exception>
	/// <filterpriority>2</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("The minimum amount of physical memory the process has required since it was started.")]
	public IntPtr MinWorkingSet
	{
		get
		{
			EnsureWorkingSetLimits();
			return minWorkingSet;
		}
		set
		{
			SetWorkingSetLimits(value, null);
		}
	}

	private OperatingSystem OperatingSystem
	{
		get
		{
			if (operatingSystem == null)
			{
				operatingSystem = Environment.OSVersion;
			}
			return operatingSystem;
		}
	}

	/// <summary>Gets or sets the overall priority category for the associated process.</summary>
	/// <returns>The priority category for the associated process, from which the <see cref="P:System.Diagnostics.Process.BasePriority" /> of the process is calculated.</returns>
	/// <exception cref="T:System.ComponentModel.Win32Exception">Process priority information could not be set or retrieved from the associated process resource.-or- The process identifier or process handle is zero. (The process has not been started.) </exception>
	/// <exception cref="T:System.NotSupportedException">You are attempting to access the <see cref="P:System.Diagnostics.Process.PriorityClass" /> property for a process that is running on a remote computer. This property is available only for processes that are running on the local computer. </exception>
	/// <exception cref="T:System.InvalidOperationException">The process <see cref="P:System.Diagnostics.Process.Id" /> is not available.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">You have set the <see cref="P:System.Diagnostics.Process.PriorityClass" /> to AboveNormal or BelowNormal when using Windows 98 or Windows Millennium Edition (Windows Me). These platforms do not support those values for the priority class. </exception>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">Priority class cannot be set because it does not use a valid value, as defined in the <see cref="T:System.Diagnostics.ProcessPriorityClass" /> enumeration.</exception>
	/// <filterpriority>1</filterpriority>
	[MonitoringDescription("The priority that the threads in the process run relative to.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public ProcessPriorityClass PriorityClass
	{
		get
		{
			if (!havePriorityClass)
			{
				SafeProcessHandle handle = null;
				try
				{
					handle = GetProcessHandle(1024);
					int num = NativeMethods.GetPriorityClass(handle);
					if (num == 0)
					{
						throw new Win32Exception();
					}
					priorityClass = (ProcessPriorityClass)num;
					havePriorityClass = true;
				}
				finally
				{
					ReleaseProcessHandle(handle);
				}
			}
			return priorityClass;
		}
		set
		{
			if (!Enum.IsDefined(typeof(ProcessPriorityClass), value))
			{
				throw new InvalidEnumArgumentException("value", (int)value, typeof(ProcessPriorityClass));
			}
			SafeProcessHandle handle = null;
			try
			{
				handle = GetProcessHandle(512);
				if (!NativeMethods.SetPriorityClass(handle, (int)value))
				{
					throw new Win32Exception();
				}
				priorityClass = value;
				havePriorityClass = true;
			}
			finally
			{
				ReleaseProcessHandle(handle);
			}
		}
	}

	/// <summary>Gets the privileged processor time for this process.</summary>
	/// <returns>A <see cref="T:System.TimeSpan" /> that indicates the amount of time that the process has spent running code inside the operating system core.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property. </exception>
	/// <exception cref="T:System.NotSupportedException">You are attempting to access the <see cref="P:System.Diagnostics.Process.PrivilegedProcessorTime" /> property for a process that is running on a remote computer. This property is available only for processes that are running on the local computer. </exception>
	/// <filterpriority>2</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("The amount of CPU time the process spent inside the operating system core.")]
	public TimeSpan PrivilegedProcessorTime
	{
		get
		{
			EnsureState(State.IsNt);
			return GetProcessTimes().PrivilegedProcessorTime;
		}
	}

	/// <summary>Gets or sets the properties to pass to the <see cref="M:System.Diagnostics.Process.Start" /> method of the <see cref="T:System.Diagnostics.Process" />.</summary>
	/// <returns>The <see cref="T:System.Diagnostics.ProcessStartInfo" /> that represents the data with which to start the process. These arguments include the name of the executable file or document used to start the process.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value that specifies the <see cref="P:System.Diagnostics.Process.StartInfo" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	[MonitoringDescription("Specifies information used to start a process.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Browsable(false)]
	public ProcessStartInfo StartInfo
	{
		get
		{
			if (startInfo == null)
			{
				startInfo = new ProcessStartInfo(this);
			}
			return startInfo;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			startInfo = value;
		}
	}

	/// <summary>Gets the time that the associated process was started.</summary>
	/// <returns>An object  that indicates when the process started. An exception is thrown if the process is not running.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property. </exception>
	/// <exception cref="T:System.NotSupportedException">You are attempting to access the <see cref="P:System.Diagnostics.Process.StartTime" /> property for a process that is running on a remote computer. This property is available only for processes that are running on the local computer. </exception>
	/// <exception cref="T:System.InvalidOperationException">The process has exited.-or-The process has not been started.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">An error occurred in the call to the Windows function.</exception>
	/// <filterpriority>1</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("The time at which the process was started.")]
	public DateTime StartTime
	{
		get
		{
			EnsureState(State.IsNt);
			return GetProcessTimes().StartTime;
		}
	}

	/// <summary>Gets or sets the object used to marshal the event handler calls that are issued as a result of a process exit event.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.ISynchronizeInvoke" /> used to marshal event handler calls that are issued as a result of an <see cref="E:System.Diagnostics.Process.Exited" /> event on the process.</returns>
	/// <filterpriority>2</filterpriority>
	[MonitoringDescription("The object used to marshal the event handler calls issued as a result of a Process exit.")]
	[DefaultValue(null)]
	[Browsable(false)]
	public ISynchronizeInvoke SynchronizingObject
	{
		get
		{
			if (synchronizingObject == null && base.DesignMode)
			{
				IDesignerHost designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
				if (designerHost != null)
				{
					object rootComponent = designerHost.RootComponent;
					if (rootComponent != null && rootComponent is ISynchronizeInvoke)
					{
						synchronizingObject = (ISynchronizeInvoke)rootComponent;
					}
				}
			}
			return synchronizingObject;
		}
		set
		{
			synchronizingObject = value;
		}
	}

	/// <summary>Gets the total processor time for this process.</summary>
	/// <returns>A <see cref="T:System.TimeSpan" /> that indicates the amount of time that the associated process has spent utilizing the CPU. This value is the sum of the <see cref="P:System.Diagnostics.Process.UserProcessorTime" /> and the <see cref="P:System.Diagnostics.Process.PrivilegedProcessorTime" />.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property. </exception>
	/// <exception cref="T:System.NotSupportedException">You are attempting to access the <see cref="P:System.Diagnostics.Process.TotalProcessorTime" /> property for a process that is running on a remote computer. This property is available only for processes that are running on the local computer. </exception>
	/// <filterpriority>2</filterpriority>
	[MonitoringDescription("The amount of CPU time the process has used.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public TimeSpan TotalProcessorTime
	{
		get
		{
			EnsureState(State.IsNt);
			return GetProcessTimes().TotalProcessorTime;
		}
	}

	/// <summary>Gets the user processor time for this process.</summary>
	/// <returns>A <see cref="T:System.TimeSpan" /> that indicates the amount of time that the associated process has spent running code inside the application portion of the process (not inside the operating system core).</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property. </exception>
	/// <exception cref="T:System.NotSupportedException">You are attempting to access the <see cref="P:System.Diagnostics.Process.UserProcessorTime" /> property for a process that is running on a remote computer. This property is available only for processes that are running on the local computer. </exception>
	/// <filterpriority>2</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("The amount of CPU time the process spent outside the operating system core.")]
	public TimeSpan UserProcessorTime
	{
		get
		{
			EnsureState(State.IsNt);
			return GetProcessTimes().UserProcessorTime;
		}
	}

	/// <summary>Gets or sets whether the <see cref="E:System.Diagnostics.Process.Exited" /> event should be raised when the process terminates.</summary>
	/// <returns>true if the <see cref="E:System.Diagnostics.Process.Exited" /> event should be raised when the associated process is terminated (through either an exit or a call to <see cref="M:System.Diagnostics.Process.Kill" />); otherwise, false. The default is false.</returns>
	/// <filterpriority>2</filterpriority>
	[Browsable(false)]
	[DefaultValue(false)]
	[MonitoringDescription("Whether the process component should watch for the associated process to exit, and raise the Exited event.")]
	public bool EnableRaisingEvents
	{
		get
		{
			return watchForExit;
		}
		set
		{
			if (value == watchForExit)
			{
				return;
			}
			if (Associated)
			{
				if (value)
				{
					OpenProcessHandle();
					EnsureWatchingForExit();
				}
				else
				{
					StopWatchingForExit();
				}
			}
			watchForExit = value;
		}
	}

	/// <summary>Gets a stream used to write the input of the application.</summary>
	/// <returns>A <see cref="T:System.IO.StreamWriter" /> that can be used to write the standard input stream of the application.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Diagnostics.Process.StandardInput" /> stream has not been defined because <see cref="P:System.Diagnostics.ProcessStartInfo.RedirectStandardInput" /> is set to false. </exception>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("Standard input stream of the process.")]
	public StreamWriter StandardInput
	{
		get
		{
			if (standardInput == null)
			{
				throw new InvalidOperationException(global::SR.GetString("StandardIn has not been redirected."));
			}
			inputStreamReadMode = StreamReadMode.syncMode;
			return standardInput;
		}
	}

	/// <summary>Gets a stream used to read the output of the application.</summary>
	/// <returns>A <see cref="T:System.IO.StreamReader" /> that can be used to read the standard output stream of the application.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Diagnostics.Process.StandardOutput" /> stream has not been defined for redirection; ensure <see cref="P:System.Diagnostics.ProcessStartInfo.RedirectStandardOutput" /> is set to true and <see cref="P:System.Diagnostics.ProcessStartInfo.UseShellExecute" /> is set to false.- or - The <see cref="P:System.Diagnostics.Process.StandardOutput" /> stream has been opened for asynchronous read operations with <see cref="M:System.Diagnostics.Process.BeginOutputReadLine" />. </exception>
	/// <filterpriority>1</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("Standard output stream of the process.")]
	[Browsable(false)]
	public StreamReader StandardOutput
	{
		get
		{
			if (standardOutput == null)
			{
				throw new InvalidOperationException(global::SR.GetString("StandardOut has not been redirected or the process hasn't started yet."));
			}
			if (outputStreamReadMode == StreamReadMode.undefined)
			{
				outputStreamReadMode = StreamReadMode.syncMode;
			}
			else if (outputStreamReadMode != StreamReadMode.syncMode)
			{
				throw new InvalidOperationException(global::SR.GetString("Cannot mix synchronous and asynchronous operation on process stream."));
			}
			return standardOutput;
		}
	}

	/// <summary>Gets a stream used to read the error output of the application.</summary>
	/// <returns>A <see cref="T:System.IO.StreamReader" /> that can be used to read the standard error stream of the application.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Diagnostics.Process.StandardError" /> stream has not been defined for redirection; ensure <see cref="P:System.Diagnostics.ProcessStartInfo.RedirectStandardError" /> is set to true and <see cref="P:System.Diagnostics.ProcessStartInfo.UseShellExecute" /> is set to false.- or - The <see cref="P:System.Diagnostics.Process.StandardError" /> stream has been opened for asynchronous read operations with <see cref="M:System.Diagnostics.Process.BeginErrorReadLine" />. </exception>
	/// <filterpriority>1</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	[MonitoringDescription("Standard error stream of the process.")]
	public StreamReader StandardError
	{
		get
		{
			if (standardError == null)
			{
				throw new InvalidOperationException(global::SR.GetString("StandardError has not been redirected."));
			}
			if (errorStreamReadMode == StreamReadMode.undefined)
			{
				errorStreamReadMode = StreamReadMode.syncMode;
			}
			else if (errorStreamReadMode != StreamReadMode.syncMode)
			{
				throw new InvalidOperationException(global::SR.GetString("Cannot mix synchronous and asynchronous operation on process stream."));
			}
			return standardError;
		}
	}

	/// <summary>Gets the base priority of the associated process.</summary>
	/// <returns>The base priority, which is computed from the <see cref="P:System.Diagnostics.Process.PriorityClass" /> of the associated process.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me); set the <see cref="P:System.Diagnostics.ProcessStartInfo.UseShellExecute" /> property to false to access this property on Windows 98 and Windows Me.</exception>
	/// <exception cref="T:System.InvalidOperationException">The process has exited.-or- The process has not started, so there is no process ID. </exception>
	/// <filterpriority>2</filterpriority>
	[System.MonoTODO]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("Base process priority.")]
	public int BasePriority => 0;

	/// <summary>Gets the number of handles opened by the process.</summary>
	/// <returns>The number of operating system handles the process has opened.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me); set the <see cref="P:System.Diagnostics.ProcessStartInfo.UseShellExecute" /> property to false to access this property on Windows 98 and Windows Me.</exception>
	/// <filterpriority>2</filterpriority>
	[System.MonoTODO]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("Handles for this process.")]
	public int HandleCount => 0;

	/// <summary>Gets the main module for the associated process.</summary>
	/// <returns>The <see cref="T:System.Diagnostics.ProcessModule" /> that was used to start the process.</returns>
	/// <exception cref="T:System.NotSupportedException">You are trying to access the <see cref="P:System.Diagnostics.Process.MainModule" /> property for a process that is running on a remote computer. This property is available only for processes that are running on the local computer.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A 32-bit process is trying to access the modules of a 64-bit process.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me); set <see cref="P:System.Diagnostics.ProcessStartInfo.UseShellExecute" /> to false to access this property on Windows 98 and Windows Me.</exception>
	/// <exception cref="T:System.InvalidOperationException">The process <see cref="P:System.Diagnostics.Process.Id" /> is not available.-or- The process has exited. </exception>
	/// <filterpriority>1</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	[MonitoringDescription("The main module of the process.")]
	public ProcessModule MainModule
	{
		get
		{
			if (processId == NativeMethods.GetCurrentProcessId())
			{
				if (current_main_module == null)
				{
					current_main_module = Modules[0];
				}
				return current_main_module;
			}
			return Modules[0];
		}
	}

	/// <summary>Gets the window handle of the main window of the associated process.</summary>
	/// <returns>The system-generated window handle of the main window of the associated process.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Diagnostics.Process.MainWindowHandle" /> is not defined because the process has exited. </exception>
	/// <exception cref="T:System.NotSupportedException">You are trying to access the <see cref="P:System.Diagnostics.Process.MainWindowHandle" /> property for a process that is running on a remote computer. This property is available only for processes that are running on the local computer.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me); set <see cref="P:System.Diagnostics.ProcessStartInfo.UseShellExecute" /> to false to access this property on Windows 98 and Windows Me. </exception>
	/// <filterpriority>2</filterpriority>
	[MonitoringDescription("The handle of the main window of the process.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[System.MonoTODO]
	public IntPtr MainWindowHandle => (IntPtr)0;

	/// <summary>Gets the caption of the main window of the process.</summary>
	/// <returns>The main window title of the process.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Diagnostics.Process.MainWindowTitle" /> property is not defined because the process has exited. </exception>
	/// <exception cref="T:System.NotSupportedException">You are trying to access the <see cref="P:System.Diagnostics.Process.MainWindowTitle" /> property for a process that is running on a remote computer. This property is available only for processes that are running on the local computer.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me); set <see cref="P:System.Diagnostics.ProcessStartInfo.UseShellExecute" /> to false to access this property on Windows 98 and Windows Me.</exception>
	/// <filterpriority>1</filterpriority>
	[MonitoringDescription("The title of the main window of the process.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[System.MonoTODO]
	public string MainWindowTitle => "null";

	/// <summary>Gets the modules that have been loaded by the associated process.</summary>
	/// <returns>An array of type <see cref="T:System.Diagnostics.ProcessModule" /> that represents the modules that have been loaded by the associated process.</returns>
	/// <exception cref="T:System.NotSupportedException">You are attempting to access the <see cref="P:System.Diagnostics.Process.Modules" /> property for a process that is running on a remote computer. This property is available only for processes that are running on the local computer. </exception>
	/// <exception cref="T:System.InvalidOperationException">The process <see cref="P:System.Diagnostics.Process.Id" /> is not available.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me); set <see cref="P:System.Diagnostics.ProcessStartInfo.UseShellExecute" /> to false to access this property on Windows 98 and Windows Me.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">You are attempting to access the <see cref="P:System.Diagnostics.Process.Modules" /> property for either the system process or the idle process. These processes do not have modules.</exception>
	/// <filterpriority>2</filterpriority>
	[MonitoringDescription("The modules that are loaded as part of this process.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public ProcessModuleCollection Modules
	{
		get
		{
			if (modules == null)
			{
				SafeProcessHandle handle = null;
				try
				{
					handle = GetProcessHandle(1024);
					modules = new ProcessModuleCollection(GetModules_internal(handle));
				}
				finally
				{
					ReleaseProcessHandle(handle);
				}
			}
			return modules;
		}
	}

	/// <summary>Gets the nonpaged system memory size allocated to this process.</summary>
	/// <returns>The amount of memory, in bytes, the system has allocated for the associated process that cannot be written to the virtual memory paging file.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property. </exception>
	/// <filterpriority>2</filterpriority>
	[MonitoringDescription("The number of bytes that are not pageable.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Obsolete("Use NonpagedSystemMemorySize64")]
	[System.MonoTODO]
	public int NonpagedSystemMemorySize => 0;

	/// <summary>Gets the paged memory size.</summary>
	/// <returns>The amount of memory, in bytes, allocated by the associated process that can be written to the virtual memory paging file.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property. </exception>
	/// <filterpriority>2</filterpriority>
	[MonitoringDescription("The number of bytes that are paged.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Obsolete("Use PagedMemorySize64")]
	public int PagedMemorySize => (int)PagedMemorySize64;

	/// <summary>Gets the paged system memory size.</summary>
	/// <returns>The amount of memory, in bytes, the system has allocated for the associated process that can be written to the virtual memory paging file.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property. </exception>
	/// <filterpriority>2</filterpriority>
	[MonitoringDescription("The amount of paged system memory in bytes.")]
	[Obsolete("Use PagedSystemMemorySize64")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int PagedSystemMemorySize => (int)PagedMemorySize64;

	/// <summary>Gets the peak paged memory size.</summary>
	/// <returns>The maximum amount of memory, in bytes, allocated by the associated process that could be written to the virtual memory paging file.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property. </exception>
	/// <filterpriority>2</filterpriority>
	[MonitoringDescription("The maximum amount of paged memory used by this process.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Obsolete("Use PeakPagedMemorySize64")]
	[System.MonoTODO]
	public int PeakPagedMemorySize => 0;

	/// <summary>Gets the peak virtual memory size.</summary>
	/// <returns>The maximum amount of virtual memory, in bytes, that the associated process has requested.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property. </exception>
	/// <filterpriority>2</filterpriority>
	[MonitoringDescription("The maximum amount of virtual memory used by this process.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Obsolete("Use PeakVirtualMemorySize64")]
	public int PeakVirtualMemorySize
	{
		get
		{
			int num;
			return (int)GetProcessData(processId, 8, out num);
		}
	}

	/// <summary>Gets the peak working set size for the associated process.</summary>
	/// <returns>The maximum amount of physical memory that the associated process has required all at once, in bytes.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property. </exception>
	/// <filterpriority>2</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Obsolete("Use PeakWorkingSet64")]
	[MonitoringDescription("The maximum amount of system memory used by this process.")]
	public int PeakWorkingSet
	{
		get
		{
			int num;
			return (int)GetProcessData(processId, 5, out num);
		}
	}

	/// <summary>Gets the amount of nonpaged system memory allocated for the associated process.</summary>
	/// <returns>The amount of system memory, in bytes, allocated for the associated process that cannot be written to the virtual memory paging file.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property.</exception>
	/// <filterpriority>2</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[System.MonoTODO]
	[MonitoringDescription("The number of bytes that are not pageable.")]
	[ComVisible(false)]
	public long NonpagedSystemMemorySize64 => 0L;

	/// <summary>Gets the amount of paged memory allocated for the associated process.</summary>
	/// <returns>The amount of memory, in bytes, allocated in the virtual memory paging file for the associated process.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property.</exception>
	/// <filterpriority>2</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("The number of bytes that are paged.")]
	[ComVisible(false)]
	public long PagedMemorySize64
	{
		get
		{
			int num;
			return GetProcessData(processId, 12, out num);
		}
	}

	/// <summary>Gets the amount of pageable system memory allocated for the associated process.</summary>
	/// <returns>The amount of system memory, in bytes, allocated for the associated process that can be written to the virtual memory paging file.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property.</exception>
	/// <filterpriority>2</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("The amount of paged system memory in bytes.")]
	[ComVisible(false)]
	public long PagedSystemMemorySize64 => PagedMemorySize64;

	/// <summary>Gets the maximum amount of memory in the virtual memory paging file used by the associated process.</summary>
	/// <returns>The maximum amount of memory, in bytes, allocated in the virtual memory paging file for the associated process since it was started.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property.</exception>
	/// <filterpriority>2</filterpriority>
	[MonitoringDescription("The maximum amount of paged memory used by this process.")]
	[System.MonoTODO]
	[ComVisible(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public long PeakPagedMemorySize64 => 0L;

	/// <summary>Gets the maximum amount of virtual memory used by the associated process.</summary>
	/// <returns>The maximum amount of virtual memory, in bytes, allocated for the associated process since it was started.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property.</exception>
	/// <filterpriority>2</filterpriority>
	[ComVisible(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("The maximum amount of virtual memory used by this process.")]
	public long PeakVirtualMemorySize64
	{
		get
		{
			int num;
			return GetProcessData(processId, 8, out num);
		}
	}

	/// <summary>Gets the maximum amount of physical memory used by the associated process.</summary>
	/// <returns>The maximum amount of physical memory, in bytes, allocated for the associated process since it was started.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property.</exception>
	/// <filterpriority>2</filterpriority>
	[ComVisible(false)]
	[MonitoringDescription("The maximum amount of system memory used by this process.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public long PeakWorkingSet64
	{
		get
		{
			int num;
			return GetProcessData(processId, 5, out num);
		}
	}

	/// <summary>Gets or sets a value indicating whether the associated process priority should temporarily be boosted by the operating system when the main window has the focus.</summary>
	/// <returns>true if dynamic boosting of the process priority should take place for a process when it is taken out of the wait state; otherwise, false. The default is false.</returns>
	/// <exception cref="T:System.ComponentModel.Win32Exception">Priority boost information could not be retrieved from the associated process resource. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property.-or- The process identifier or process handle is zero. (The process has not been started.) </exception>
	/// <exception cref="T:System.NotSupportedException">You are attempting to access the <see cref="P:System.Diagnostics.Process.PriorityBoostEnabled" /> property for a process that is running on a remote computer. This property is available only for processes that are running on the local computer. </exception>
	/// <exception cref="T:System.InvalidOperationException">The process <see cref="P:System.Diagnostics.Process.Id" /> is not available.</exception>
	/// <filterpriority>1</filterpriority>
	[MonitoringDescription("Process will be of higher priority while it is actively used.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[System.MonoTODO]
	public bool PriorityBoostEnabled
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	/// <summary>Gets the private memory size.</summary>
	/// <returns>The number of bytes allocated by the associated process that cannot be shared with other processes.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property. </exception>
	/// <filterpriority>2</filterpriority>
	[Obsolete("Use PrivateMemorySize64")]
	[MonitoringDescription("The amount of memory exclusively used by this process.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int PrivateMemorySize
	{
		get
		{
			int num;
			return (int)GetProcessData(processId, 6, out num);
		}
	}

	/// <summary>Gets the Terminal Services session identifier for the associated process.</summary>
	/// <returns>The Terminal Services session identifier for the associated process.</returns>
	/// <exception cref="T:System.NullReferenceException">There is no session associated with this process.</exception>
	/// <exception cref="T:System.InvalidOperationException">There is no process associated with this session identifier.-or-The associated process is not on this machine. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The <see cref="P:System.Diagnostics.Process.SessionId" /> property is not supported on Windows 98.</exception>
	/// <filterpriority>1</filterpriority>
	[System.MonoNotSupported("")]
	[MonitoringDescription("The session ID for this process.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int SessionId => 0;

	/// <summary>Gets the name of the process.</summary>
	/// <returns>The name that the system uses to identify the process to the user.</returns>
	/// <exception cref="T:System.InvalidOperationException">The process does not have an identifier, or no process is associated with the <see cref="T:System.Diagnostics.Process" />.-or- The associated process has exited. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me); set <see cref="P:System.Diagnostics.ProcessStartInfo.UseShellExecute" /> to false to access this property on Windows 98 and Windows Me.</exception>
	/// <exception cref="T:System.NotSupportedException">The process is not on this computer.</exception>
	/// <filterpriority>1</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("The name of this process.")]
	public string ProcessName
	{
		get
		{
			if (process_name == null)
			{
				SafeProcessHandle handle = null;
				try
				{
					handle = GetProcessHandle(1024);
					process_name = ProcessName_internal(handle);
					if (process_name == null)
					{
						throw new InvalidOperationException("Process has exited or is inaccessible, so the requested information is not available.");
					}
					if (process_name.EndsWith(".exe") || process_name.EndsWith(".bat") || process_name.EndsWith(".com"))
					{
						process_name = process_name.Substring(0, process_name.Length - 4);
					}
				}
				finally
				{
					ReleaseProcessHandle(handle);
				}
			}
			return process_name;
		}
	}

	/// <summary>Gets or sets the processors on which the threads in this process can be scheduled to run.</summary>
	/// <returns>A bitmask representing the processors that the threads in the associated process can run on. The default depends on the number of processors on the computer. The default value is 2 n -1, where n is the number of processors.</returns>
	/// <exception cref="T:System.ComponentModel.Win32Exception">
	///   <see cref="P:System.Diagnostics.Process.ProcessorAffinity" /> information could not be set or retrieved from the associated process resource.-or- The process identifier or process handle is zero. (The process has not been started.) </exception>
	/// <exception cref="T:System.NotSupportedException">You are attempting to access the <see cref="P:System.Diagnostics.Process.ProcessorAffinity" /> property for a process that is running on a remote computer. This property is available only for processes that are running on the local computer. </exception>
	/// <exception cref="T:System.InvalidOperationException">The process <see cref="P:System.Diagnostics.Process.Id" /> was not available.-or- The process has exited. </exception>
	/// <filterpriority>2</filterpriority>
	[System.MonoTODO]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("Allowed processor that can be used by this process.")]
	public IntPtr ProcessorAffinity
	{
		get
		{
			return (IntPtr)0;
		}
		set
		{
		}
	}

	/// <summary>Gets a value indicating whether the user interface of the process is responding.</summary>
	/// <returns>true if the user interface of the associated process is responding to the system; otherwise, false.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me); set <see cref="P:System.Diagnostics.ProcessStartInfo.UseShellExecute" /> to false to access this property on Windows 98 and Windows Me.</exception>
	/// <exception cref="T:System.InvalidOperationException">There is no process associated with this <see cref="T:System.Diagnostics.Process" /> object. </exception>
	/// <exception cref="T:System.NotSupportedException">You are attempting to access the <see cref="P:System.Diagnostics.Process.Responding" /> property for a process that is running on a remote computer. This property is available only for processes that are running on the local computer. </exception>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("Is this process responsive.")]
	public bool Responding => false;

	/// <summary>Gets the set of threads that are running in the associated process.</summary>
	/// <returns>An array of type <see cref="T:System.Diagnostics.ProcessThread" /> representing the operating system threads currently running in the associated process.</returns>
	/// <exception cref="T:System.SystemException">The process does not have an <see cref="P:System.Diagnostics.Process.Id" />, or no process is associated with the <see cref="T:System.Diagnostics.Process" /> instance.-or- The associated process has exited. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me); set <see cref="P:System.Diagnostics.ProcessStartInfo.UseShellExecute" /> to false to access this property on Windows 98 and Windows Me.</exception>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("The number of threads of this process.")]
	public ProcessThreadCollection Threads
	{
		get
		{
			if (threads == null)
			{
				threads = new ProcessThreadCollection(new ProcessThread[GetProcessData(processId, 0, out var _)]);
			}
			return threads;
		}
	}

	/// <summary>Gets the size of the process's virtual memory.</summary>
	/// <returns>The amount of virtual memory, in bytes, that the associated process has requested.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property. </exception>
	/// <filterpriority>2</filterpriority>
	[Obsolete("Use VirtualMemorySize64")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("The amount of virtual memory currently used for this process.")]
	public int VirtualMemorySize
	{
		get
		{
			int num;
			return (int)GetProcessData(processId, 7, out num);
		}
	}

	/// <summary>Gets the associated process's physical memory usage.</summary>
	/// <returns>The total amount of physical memory the associated process is using, in bytes.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property. </exception>
	/// <filterpriority>2</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("The amount of physical memory currently used for this process.")]
	[Obsolete("Use WorkingSet64")]
	public int WorkingSet
	{
		get
		{
			int num;
			return (int)GetProcessData(processId, 4, out num);
		}
	}

	/// <summary>Gets the amount of private memory allocated for the associated process.</summary>
	/// <returns>The amount of memory, in bytes, allocated for the associated process that cannot be shared with other processes.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property.</exception>
	/// <filterpriority>2</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("The amount of memory exclusively used by this process.")]
	[ComVisible(false)]
	public long PrivateMemorySize64
	{
		get
		{
			int num;
			return GetProcessData(processId, 6, out num);
		}
	}

	/// <summary>Gets the amount of the virtual memory allocated for the associated process.</summary>
	/// <returns>The amount of virtual memory, in bytes, allocated for the associated process.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property.</exception>
	/// <filterpriority>2</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("The amount of virtual memory currently used for this process.")]
	[ComVisible(false)]
	public long VirtualMemorySize64
	{
		get
		{
			int num;
			return GetProcessData(processId, 7, out num);
		}
	}

	/// <summary>Gets the amount of physical memory allocated for the associated process.</summary>
	/// <returns>The amount of physical memory, in bytes, allocated for the associated process.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me), which does not support this property.</exception>
	/// <filterpriority>2</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("The amount of physical memory currently used for this process.")]
	[ComVisible(false)]
	public long WorkingSet64
	{
		get
		{
			int num;
			return GetProcessData(processId, 4, out num);
		}
	}

	private static bool IsWindows
	{
		get
		{
			PlatformID platform = Environment.OSVersion.Platform;
			if (platform == PlatformID.Win32S || platform == PlatformID.Win32Windows || platform == PlatformID.Win32NT || platform == PlatformID.WinCE)
			{
				return true;
			}
			return false;
		}
	}

	/// <summary>Occurs when an application writes to its redirected <see cref="P:System.Diagnostics.Process.StandardOutput" /> stream.</summary>
	/// <filterpriority>2</filterpriority>
	[Browsable(true)]
	[MonitoringDescription("Indicates if the process component is associated with a real process.")]
	public event DataReceivedEventHandler OutputDataReceived;

	/// <summary>Occurs when an application writes to its redirected <see cref="P:System.Diagnostics.Process.StandardError" /> stream.</summary>
	/// <filterpriority>2</filterpriority>
	[Browsable(true)]
	[MonitoringDescription("Indicates if the process component is associated with a real process.")]
	public event DataReceivedEventHandler ErrorDataReceived;

	/// <summary>Occurs when a process exits.</summary>
	/// <filterpriority>2</filterpriority>
	[Category("Behavior")]
	[MonitoringDescription("If the WatchForExit property is set to true, then this event is raised when the associated process exits.")]
	public event EventHandler Exited
	{
		add
		{
			onExited = (EventHandler)Delegate.Combine(onExited, value);
		}
		remove
		{
			onExited = (EventHandler)Delegate.Remove(onExited, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Process" /> class.</summary>
	public Process()
	{
		machineName = ".";
		outputStreamReadMode = StreamReadMode.undefined;
		errorStreamReadMode = StreamReadMode.undefined;
		m_processAccess = 2035711;
	}

	private Process(string machineName, bool isRemoteMachine, int processId, ProcessInfo processInfo)
	{
		this.machineName = machineName;
		this.isRemoteMachine = isRemoteMachine;
		this.processId = processId;
		haveProcessId = true;
		outputStreamReadMode = StreamReadMode.undefined;
		errorStreamReadMode = StreamReadMode.undefined;
		m_processAccess = 2035711;
	}

	private ProcessThreadTimes GetProcessTimes()
	{
		ProcessThreadTimes processThreadTimes = new ProcessThreadTimes();
		SafeProcessHandle safeProcessHandle = null;
		try
		{
			int access = 1024;
			if (EnvironmentHelpers.IsWindowsVistaOrAbove())
			{
				access = 4096;
			}
			safeProcessHandle = GetProcessHandle(access, throwIfExited: false);
			if (safeProcessHandle.IsInvalid)
			{
				throw new InvalidOperationException(global::SR.GetString("Cannot process request because the process ({0}) has exited.", processId.ToString(CultureInfo.CurrentCulture)));
			}
			if (!NativeMethods.GetProcessTimes(safeProcessHandle, out processThreadTimes.create, out processThreadTimes.exit, out processThreadTimes.kernel, out processThreadTimes.user))
			{
				throw new Win32Exception();
			}
			return processThreadTimes;
		}
		finally
		{
			ReleaseProcessHandle(safeProcessHandle);
		}
	}

	private void ReleaseProcessHandle(SafeProcessHandle handle)
	{
		if (handle != null && (!haveProcessHandle || handle != m_processHandle))
		{
			handle.Close();
		}
	}

	private void CompletionCallback(object context, bool wasSignaled)
	{
		StopWatchingForExit();
		RaiseOnExited();
	}

	/// <summary>Release all resources used by this process.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected override void Dispose(bool disposing)
	{
		if (!disposed)
		{
			if (disposing)
			{
				Close();
			}
			disposed = true;
			base.Dispose(disposing);
		}
	}

	/// <summary>Frees all the resources that are associated with this component.</summary>
	/// <filterpriority>2</filterpriority>
	public void Close()
	{
		if (Associated)
		{
			if (haveProcessHandle)
			{
				StopWatchingForExit();
				m_processHandle.Close();
				m_processHandle = null;
				haveProcessHandle = false;
			}
			haveProcessId = false;
			isRemoteMachine = false;
			machineName = ".";
			raisedOnExited = false;
			StreamWriter streamWriter = standardInput;
			standardInput = null;
			if (inputStreamReadMode == StreamReadMode.undefined)
			{
				streamWriter?.Close();
			}
			StreamReader streamReader = standardOutput;
			standardOutput = null;
			if (outputStreamReadMode == StreamReadMode.undefined)
			{
				streamReader?.Close();
			}
			streamReader = standardError;
			standardError = null;
			if (errorStreamReadMode == StreamReadMode.undefined)
			{
				streamReader?.Close();
			}
			AsyncStreamReader asyncStreamReader = output;
			output = null;
			if (outputStreamReadMode == StreamReadMode.asyncMode && asyncStreamReader != null)
			{
				asyncStreamReader.CancelOperation();
				asyncStreamReader.Close();
			}
			asyncStreamReader = error;
			error = null;
			if (errorStreamReadMode == StreamReadMode.asyncMode && asyncStreamReader != null)
			{
				asyncStreamReader.CancelOperation();
				asyncStreamReader.Close();
			}
			Refresh();
		}
	}

	private void EnsureState(State state)
	{
		if ((state & State.Associated) != 0 && !Associated)
		{
			throw new InvalidOperationException(global::SR.GetString("No process is associated with this object."));
		}
		if ((state & State.HaveId) != 0 && !haveProcessId)
		{
			EnsureState(State.Associated);
			throw new InvalidOperationException(global::SR.GetString("Feature requires a process identifier."));
		}
		if ((state & State.IsLocal) != 0 && isRemoteMachine)
		{
			throw new NotSupportedException(global::SR.GetString("Feature is not supported for remote machines."));
		}
		if ((state & State.HaveProcessInfo) != 0)
		{
			throw new InvalidOperationException(global::SR.GetString("Process has exited, so the requested information is not available."));
		}
		if ((state & State.Exited) != 0)
		{
			if (!HasExited)
			{
				throw new InvalidOperationException(global::SR.GetString("Process must exit before requested information can be determined."));
			}
			if (!haveProcessHandle)
			{
				throw new InvalidOperationException(global::SR.GetString("Process was not started by this object, so requested information cannot be determined."));
			}
		}
	}

	private void EnsureWatchingForExit()
	{
		if (watchingForExit)
		{
			return;
		}
		lock (this)
		{
			if (!watchingForExit)
			{
				watchingForExit = true;
				try
				{
					waitHandle = new ProcessWaitHandle(m_processHandle);
					registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(waitHandle, CompletionCallback, null, -1, executeOnlyOnce: true);
					return;
				}
				catch
				{
					watchingForExit = false;
					throw;
				}
			}
		}
	}

	private void EnsureWorkingSetLimits()
	{
		EnsureState(State.IsNt);
		if (haveWorkingSetLimits)
		{
			return;
		}
		SafeProcessHandle handle = null;
		try
		{
			handle = GetProcessHandle(1024);
			if (!NativeMethods.GetProcessWorkingSetSize(handle, out var min, out var max))
			{
				throw new Win32Exception();
			}
			minWorkingSet = min;
			maxWorkingSet = max;
			haveWorkingSetLimits = true;
		}
		finally
		{
			ReleaseProcessHandle(handle);
		}
	}

	/// <summary>Puts a <see cref="T:System.Diagnostics.Process" /> component in state to interact with operating system processes that run in a special mode by enabling the native property SeDebugPrivilege on the current thread.</summary>
	/// <filterpriority>2</filterpriority>
	public static void EnterDebugMode()
	{
	}

	/// <summary>Takes a <see cref="T:System.Diagnostics.Process" /> component out of the state that lets it interact with operating system processes that run in a special mode.</summary>
	/// <filterpriority>2</filterpriority>
	public static void LeaveDebugMode()
	{
	}

	/// <summary>Returns a new <see cref="T:System.Diagnostics.Process" /> component, given the identifier of a process on the local computer.</summary>
	/// <returns>A <see cref="T:System.Diagnostics.Process" /> component that is associated with the local process resource identified by the <paramref name="processId" /> parameter.</returns>
	/// <param name="processId">The system-unique identifier of a process resource. </param>
	/// <exception cref="T:System.ArgumentException">The process specified by the <paramref name="processId" /> parameter is not running. The identifier might be expired. </exception>
	/// <exception cref="T:System.InvalidOperationException">The process was not started by this object.</exception>
	/// <filterpriority>1</filterpriority>
	public static Process GetProcessById(int processId)
	{
		return GetProcessById(processId, ".");
	}

	/// <summary>Creates an array of new <see cref="T:System.Diagnostics.Process" /> components and associates them with all the process resources on the local computer that share the specified process name.</summary>
	/// <returns>An array of type <see cref="T:System.Diagnostics.Process" /> that represents the process resources running the specified application or file.</returns>
	/// <param name="processName">The friendly name of the process. </param>
	/// <exception cref="T:System.InvalidOperationException">There are problems accessing the performance counter API's used to get process information. This exception is specific to Windows NT, Windows 2000, and Windows XP. </exception>
	/// <filterpriority>1</filterpriority>
	public static Process[] GetProcessesByName(string processName)
	{
		return GetProcessesByName(processName, ".");
	}

	/// <summary>Creates a new <see cref="T:System.Diagnostics.Process" /> component for each process resource on the local computer.</summary>
	/// <returns>An array of type <see cref="T:System.Diagnostics.Process" /> that represents all the process resources running on the local computer.</returns>
	/// <filterpriority>1</filterpriority>
	public static Process[] GetProcesses()
	{
		return GetProcesses(".");
	}

	/// <summary>Gets a new <see cref="T:System.Diagnostics.Process" /> component and associates it with the currently active process.</summary>
	/// <returns>A new <see cref="T:System.Diagnostics.Process" /> component associated with the process resource that is running the calling application.</returns>
	/// <filterpriority>1</filterpriority>
	public static Process GetCurrentProcess()
	{
		return new Process(".", isRemoteMachine: false, NativeMethods.GetCurrentProcessId(), null);
	}

	/// <summary>Raises the <see cref="E:System.Diagnostics.Process.Exited" /> event.</summary>
	protected void OnExited()
	{
		EventHandler eventHandler = onExited;
		if (eventHandler != null)
		{
			if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
			{
				SynchronizingObject.BeginInvoke(eventHandler, new object[2]
				{
					this,
					EventArgs.Empty
				});
			}
			else
			{
				eventHandler(this, EventArgs.Empty);
			}
		}
	}

	private SafeProcessHandle GetProcessHandle(int access, bool throwIfExited)
	{
		if (haveProcessHandle)
		{
			if (throwIfExited)
			{
				ProcessWaitHandle processWaitHandle = null;
				try
				{
					processWaitHandle = new ProcessWaitHandle(m_processHandle);
					if (processWaitHandle.WaitOne(0, exitContext: false))
					{
						if (haveProcessId)
						{
							throw new InvalidOperationException(global::SR.GetString("Cannot process request because the process ({0}) has exited.", processId.ToString(CultureInfo.CurrentCulture)));
						}
						throw new InvalidOperationException(global::SR.GetString("Cannot process request because the process has exited."));
					}
				}
				finally
				{
					processWaitHandle?.Close();
				}
			}
			return m_processHandle;
		}
		EnsureState((State)3);
		SafeProcessHandle targetHandle = SafeProcessHandle.InvalidHandle;
		IntPtr currentProcess = NativeMethods.GetCurrentProcess();
		if (!NativeMethods.DuplicateHandle(new HandleRef(this, currentProcess), new HandleRef(this, currentProcess), new HandleRef(this, currentProcess), out targetHandle, 0, bInheritHandle: false, 3))
		{
			throw new Win32Exception();
		}
		if (throwIfExited && (access & 0x400) != 0 && NativeMethods.GetExitCodeProcess(targetHandle, out exitCode) && exitCode != 259)
		{
			throw new InvalidOperationException(global::SR.GetString("Cannot process request because the process ({0}) has exited.", processId.ToString(CultureInfo.CurrentCulture)));
		}
		return targetHandle;
	}

	private SafeProcessHandle GetProcessHandle(int access)
	{
		return GetProcessHandle(access, throwIfExited: true);
	}

	private SafeProcessHandle OpenProcessHandle()
	{
		return OpenProcessHandle(2035711);
	}

	private SafeProcessHandle OpenProcessHandle(int access)
	{
		if (!haveProcessHandle)
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			SetProcessHandle(GetProcessHandle(access));
		}
		return m_processHandle;
	}

	/// <summary>Discards any information about the associated process that has been cached inside the process component.</summary>
	/// <filterpriority>1</filterpriority>
	public void Refresh()
	{
		threads = null;
		modules = null;
		exited = false;
		signaled = false;
		haveWorkingSetLimits = false;
		havePriorityClass = false;
		haveExitTime = false;
	}

	private void SetProcessHandle(SafeProcessHandle processHandle)
	{
		m_processHandle = processHandle;
		haveProcessHandle = true;
		if (watchForExit)
		{
			EnsureWatchingForExit();
		}
	}

	private void SetProcessId(int processId)
	{
		this.processId = processId;
		haveProcessId = true;
	}

	private void SetWorkingSetLimits(object newMin, object newMax)
	{
		EnsureState(State.IsNt);
		SafeProcessHandle handle = null;
		try
		{
			handle = GetProcessHandle(1280);
			if (!NativeMethods.GetProcessWorkingSetSize(handle, out var min, out var max))
			{
				throw new Win32Exception();
			}
			if (newMin != null)
			{
				min = (IntPtr)newMin;
			}
			if (newMax != null)
			{
				max = (IntPtr)newMax;
			}
			if ((long)min > (long)max)
			{
				if (newMin != null)
				{
					throw new ArgumentException(global::SR.GetString("Minimum working set size is invalid. It must be less than or equal to the maximum working set size."));
				}
				throw new ArgumentException(global::SR.GetString("Maximum working set size is invalid. It must be greater than or equal to the minimum working set size."));
			}
			if (!NativeMethods.SetProcessWorkingSetSize(handle, min, max))
			{
				throw new Win32Exception();
			}
			if (!NativeMethods.GetProcessWorkingSetSize(handle, out min, out max))
			{
				throw new Win32Exception();
			}
			minWorkingSet = min;
			maxWorkingSet = max;
			haveWorkingSetLimits = true;
		}
		finally
		{
			ReleaseProcessHandle(handle);
		}
	}

	/// <summary>Starts (or reuses) the process resource that is specified by the <see cref="P:System.Diagnostics.Process.StartInfo" /> property of this <see cref="T:System.Diagnostics.Process" /> component and associates it with the component.</summary>
	/// <returns>true if a process resource is started; false if no new process resource is started (for example, if an existing process is reused).</returns>
	/// <exception cref="T:System.InvalidOperationException">No file name was specified in the <see cref="T:System.Diagnostics.Process" /> component's <see cref="P:System.Diagnostics.Process.StartInfo" />.-or- The <see cref="P:System.Diagnostics.ProcessStartInfo.UseShellExecute" /> member of the <see cref="P:System.Diagnostics.Process.StartInfo" /> property is true while <see cref="P:System.Diagnostics.ProcessStartInfo.RedirectStandardInput" />, <see cref="P:System.Diagnostics.ProcessStartInfo.RedirectStandardOutput" />, or <see cref="P:System.Diagnostics.ProcessStartInfo.RedirectStandardError" /> is true. </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">There was an error in opening the associated file. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The process object has already been disposed. </exception>
	/// <filterpriority>1</filterpriority>
	public bool Start()
	{
		Close();
		ProcessStartInfo processStartInfo = StartInfo;
		if (processStartInfo.FileName.Length == 0)
		{
			throw new InvalidOperationException(global::SR.GetString("Cannot start process because a file name has not been provided."));
		}
		if (processStartInfo.UseShellExecute)
		{
			return StartWithShellExecuteEx(processStartInfo);
		}
		return StartWithCreateProcess(processStartInfo);
	}

	/// <summary>Starts a process resource by specifying the name of an application, a user name, a password, and a domain and associates the resource with a new <see cref="T:System.Diagnostics.Process" /> component.</summary>
	/// <returns>A new <see cref="T:System.Diagnostics.Process" /> component that is associated with the process resource, or null if no process resource is started (for example, if an existing process is reused).</returns>
	/// <param name="fileName">The name of an application file to run in the process.</param>
	/// <param name="userName">The user name to use when starting the process.</param>
	/// <param name="password">A <see cref="T:System.Security.SecureString" /> that contains the password to use when starting the process.</param>
	/// <param name="domain">The domain to use when starting the process.</param>
	/// <exception cref="T:System.InvalidOperationException">No file name was specified. </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">
	///   <paramref name="fileName" /> is not an executable (.exe) file.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">There was an error in opening the associated file. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The process object has already been disposed. </exception>
	/// <filterpriority>1</filterpriority>
	public static Process Start(string fileName, string userName, SecureString password, string domain)
	{
		return Start(new ProcessStartInfo(fileName)
		{
			UserName = userName,
			Password = password,
			Domain = domain,
			UseShellExecute = false
		});
	}

	/// <summary>Starts a process resource by specifying the name of an application, a set of command-line arguments, a user name, a password, and a domain and associates the resource with a new <see cref="T:System.Diagnostics.Process" /> component.</summary>
	/// <returns>A new <see cref="T:System.Diagnostics.Process" /> component that is associated with the process resource, or null if no process resource is started (for example, if an existing process is reused).</returns>
	/// <param name="fileName">The name of an application file to run in the process. </param>
	/// <param name="arguments">Command-line arguments to pass when starting the process. </param>
	/// <param name="userName">The user name to use when starting the process.</param>
	/// <param name="password">A <see cref="T:System.Security.SecureString" /> that contains the password to use when starting the process.</param>
	/// <param name="domain">The domain to use when starting the process.</param>
	/// <exception cref="T:System.InvalidOperationException">No file name was specified.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">
	///   <paramref name="fileName" /> is not an executable (.exe) file.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">An error occurred when opening the associated file. -or-The sum of the length of the arguments and the length of the full path to the associated file exceeds 2080. The error message associated with this exception can be one of the following: "The data area passed to a system call is too small." or "Access is denied."</exception>
	/// <exception cref="T:System.ObjectDisposedException">The process object has already been disposed. </exception>
	/// <filterpriority>1</filterpriority>
	public static Process Start(string fileName, string arguments, string userName, SecureString password, string domain)
	{
		return Start(new ProcessStartInfo(fileName, arguments)
		{
			UserName = userName,
			Password = password,
			Domain = domain,
			UseShellExecute = false
		});
	}

	/// <summary>Starts a process resource by specifying the name of a document or application file and associates the resource with a new <see cref="T:System.Diagnostics.Process" /> component.</summary>
	/// <returns>A new <see cref="T:System.Diagnostics.Process" /> component that is associated with the process resource, or null, if no process resource is started (for example, if an existing process is reused).</returns>
	/// <param name="fileName">The name of a document or application file to run in the process. </param>
	/// <exception cref="T:System.ComponentModel.Win32Exception">An error occurred when opening the associated file. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The process object has already been disposed. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The PATH environment variable has a string containing quotes.</exception>
	/// <filterpriority>1</filterpriority>
	public static Process Start(string fileName)
	{
		return Start(new ProcessStartInfo(fileName));
	}

	/// <summary>Starts a process resource by specifying the name of an application and a set of command-line arguments, and associates the resource with a new <see cref="T:System.Diagnostics.Process" /> component.</summary>
	/// <returns>A new <see cref="T:System.Diagnostics.Process" /> component that is associated with the process, or null, if no process resource is started (for example, if an existing process is reused).</returns>
	/// <param name="fileName">The name of an application file to run in the process. </param>
	/// <param name="arguments">Command-line arguments to pass when starting the process. </param>
	/// <exception cref="T:System.InvalidOperationException">The <paramref name="fileName" /> or <paramref name="arguments" /> parameter is null. </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">An error occurred when opening the associated file. -or-The sum of the length of the arguments and the length of the full path to the process exceeds 2080. The error message associated with this exception can be one of the following: "The data area passed to a system call is too small." or "Access is denied."</exception>
	/// <exception cref="T:System.ObjectDisposedException">The process object has already been disposed. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The PATH environment variable has a string containing quotes.</exception>
	/// <filterpriority>1</filterpriority>
	public static Process Start(string fileName, string arguments)
	{
		return Start(new ProcessStartInfo(fileName, arguments));
	}

	/// <summary>Starts the process resource that is specified by the parameter containing process start information (for example, the file name of the process to start) and associates the resource with a new <see cref="T:System.Diagnostics.Process" /> component.</summary>
	/// <returns>A new <see cref="T:System.Diagnostics.Process" /> component that is associated with the process resource, or null if no process resource is started (for example, if an existing process is reused).</returns>
	/// <param name="startInfo">The <see cref="T:System.Diagnostics.ProcessStartInfo" /> that contains the information that is used to start the process, including the file name and any command-line arguments. </param>
	/// <exception cref="T:System.InvalidOperationException">No file name was specified in the <paramref name="startInfo" /> parameter's <see cref="P:System.Diagnostics.ProcessStartInfo.FileName" /> property.-or- The <see cref="P:System.Diagnostics.ProcessStartInfo.UseShellExecute" /> property of the <paramref name="startInfo" /> parameter is true and the <see cref="P:System.Diagnostics.ProcessStartInfo.RedirectStandardInput" />, <see cref="P:System.Diagnostics.ProcessStartInfo.RedirectStandardOutput" />, or <see cref="P:System.Diagnostics.ProcessStartInfo.RedirectStandardError" /> property is also true.-or-The <see cref="P:System.Diagnostics.ProcessStartInfo.UseShellExecute" /> property of the <paramref name="startInfo" /> parameter is true and the <see cref="P:System.Diagnostics.ProcessStartInfo.UserName" /> property is not null or empty or the <see cref="P:System.Diagnostics.ProcessStartInfo.Password" /> property is not null.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="startInfo" /> parameter is null. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The process object has already been disposed. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in the <paramref name="startInfo" /> parameter's <see cref="P:System.Diagnostics.ProcessStartInfo.FileName" /> property could not be found.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">An error occurred when opening the associated file. -or-The sum of the length of the arguments and the length of the full path to the process exceeds 2080. The error message associated with this exception can be one of the following: "The data area passed to a system call is too small." or "Access is denied."</exception>
	/// <filterpriority>1</filterpriority>
	public static Process Start(ProcessStartInfo startInfo)
	{
		Process process = new Process();
		if (startInfo == null)
		{
			throw new ArgumentNullException("startInfo");
		}
		process.StartInfo = startInfo;
		if (process.Start())
		{
			return process;
		}
		return null;
	}

	/// <summary>Immediately stops the associated process.</summary>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The associated process could not be terminated. -or-The process is terminating.-or- The associated process is a Win16 executable.</exception>
	/// <exception cref="T:System.NotSupportedException">You are attempting to call <see cref="M:System.Diagnostics.Process.Kill" /> for a process that is running on a remote computer. The method is available only for processes running on the local computer.</exception>
	/// <exception cref="T:System.InvalidOperationException">The process has already exited. -or-There is no process associated with this <see cref="T:System.Diagnostics.Process" /> object.</exception>
	/// <filterpriority>1</filterpriority>
	public void Kill()
	{
		SafeProcessHandle safeProcessHandle = null;
		try
		{
			safeProcessHandle = GetProcessHandle(1);
			if (!NativeMethods.TerminateProcess(safeProcessHandle, -1))
			{
				throw new Win32Exception();
			}
		}
		finally
		{
			ReleaseProcessHandle(safeProcessHandle);
		}
	}

	private void StopWatchingForExit()
	{
		if (!watchingForExit)
		{
			return;
		}
		lock (this)
		{
			if (watchingForExit)
			{
				watchingForExit = false;
				registeredWaitHandle.Unregister(null);
				waitHandle.Close();
				waitHandle = null;
				registeredWaitHandle = null;
			}
		}
	}

	/// <summary>Formats the process's name as a string, combined with the parent component type, if applicable.</summary>
	/// <returns>The <see cref="P:System.Diagnostics.Process.ProcessName" />, combined with the base component's <see cref="M:System.Object.ToString" /> return value.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">
	///   <see cref="M:System.Diagnostics.Process.ToString" /> is not supported on Windows 98.</exception>
	/// <filterpriority>2</filterpriority>
	public override string ToString()
	{
		if (Associated)
		{
			string text = string.Empty;
			try
			{
				text = ProcessName;
			}
			catch (PlatformNotSupportedException)
			{
			}
			if (text.Length != 0)
			{
				return string.Format(CultureInfo.CurrentCulture, "{0} ({1})", base.ToString(), text);
			}
			return base.ToString();
		}
		return base.ToString();
	}

	/// <summary>Instructs the <see cref="T:System.Diagnostics.Process" /> component to wait the specified number of milliseconds for the associated process to exit.</summary>
	/// <returns>true if the associated process has exited; otherwise, false.</returns>
	/// <param name="milliseconds">The amount of time, in milliseconds, to wait for the associated process to exit. The maximum is the largest possible value of a 32-bit integer, which represents infinity to the operating system. </param>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The wait setting could not be accessed. </exception>
	/// <exception cref="T:System.SystemException">No process <see cref="P:System.Diagnostics.Process.Id" /> has been set, and a <see cref="P:System.Diagnostics.Process.Handle" /> from which the <see cref="P:System.Diagnostics.Process.Id" /> property can be determined does not exist.-or- There is no process associated with this <see cref="T:System.Diagnostics.Process" /> object.-or- You are attempting to call <see cref="M:System.Diagnostics.Process.WaitForExit(System.Int32)" /> for a process that is running on a remote computer. This method is available only for processes that are running on the local computer. </exception>
	/// <filterpriority>1</filterpriority>
	public bool WaitForExit(int milliseconds)
	{
		SafeProcessHandle safeProcessHandle = null;
		ProcessWaitHandle processWaitHandle = null;
		bool flag;
		try
		{
			safeProcessHandle = GetProcessHandle(1048576, throwIfExited: false);
			if (safeProcessHandle.IsInvalid)
			{
				flag = true;
			}
			else
			{
				processWaitHandle = new ProcessWaitHandle(safeProcessHandle);
				if (processWaitHandle.WaitOne(milliseconds, exitContext: false))
				{
					flag = true;
					signaled = true;
				}
				else
				{
					flag = false;
					signaled = false;
				}
			}
			if (output != null && milliseconds == -1)
			{
				output.WaitUtilEOF();
			}
			if (error != null && milliseconds == -1)
			{
				error.WaitUtilEOF();
			}
		}
		finally
		{
			processWaitHandle?.Close();
			ReleaseProcessHandle(safeProcessHandle);
		}
		if (flag && watchForExit)
		{
			RaiseOnExited();
		}
		return flag;
	}

	/// <summary>Instructs the <see cref="T:System.Diagnostics.Process" /> component to wait indefinitely for the associated process to exit.</summary>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The wait setting could not be accessed. </exception>
	/// <exception cref="T:System.SystemException">No process <see cref="P:System.Diagnostics.Process.Id" /> has been set, and a <see cref="P:System.Diagnostics.Process.Handle" /> from which the <see cref="P:System.Diagnostics.Process.Id" /> property can be determined does not exist.-or- There is no process associated with this <see cref="T:System.Diagnostics.Process" /> object.-or- You are attempting to call <see cref="M:System.Diagnostics.Process.WaitForExit" /> for a process that is running on a remote computer. This method is available only for processes that are running on the local computer. </exception>
	/// <filterpriority>1</filterpriority>
	public void WaitForExit()
	{
		WaitForExit(-1);
	}

	/// <summary>Causes the <see cref="T:System.Diagnostics.Process" /> component to wait the specified number of milliseconds for the associated process to enter an idle state. This overload applies only to processes with a user interface and, therefore, a message loop.</summary>
	/// <returns>true if the associated process has reached an idle state; otherwise, false.</returns>
	/// <param name="milliseconds">A value of 1 to <see cref="F:System.Int32.MaxValue" /> that specifies the amount of time, in milliseconds, to wait for the associated process to become idle. A value of 0 specifies an immediate return, and a value of -1 specifies an infinite wait. </param>
	/// <exception cref="T:System.InvalidOperationException">The process does not have a graphical interface.-or-An unknown error occurred. The process failed to enter an idle state.-or-The process has already exited. -or-No process is associated with this <see cref="T:System.Diagnostics.Process" /> object.</exception>
	/// <filterpriority>1</filterpriority>
	public bool WaitForInputIdle(int milliseconds)
	{
		SafeProcessHandle handle = null;
		try
		{
			handle = GetProcessHandle(1049600);
			return NativeMethods.WaitForInputIdle(handle, milliseconds) switch
			{
				0 => true, 
				258 => false, 
				_ => throw new InvalidOperationException(global::SR.GetString("WaitForInputIdle failed.  This could be because the process does not have a graphical interface.")), 
			};
		}
		finally
		{
			ReleaseProcessHandle(handle);
		}
	}

	/// <summary>Causes the <see cref="T:System.Diagnostics.Process" /> component to wait indefinitely for the associated process to enter an idle state. This overload applies only to processes with a user interface and, therefore, a message loop.</summary>
	/// <returns>true if the associated process has reached an idle state.</returns>
	/// <exception cref="T:System.InvalidOperationException">The process does not have a graphical interface.-or-An unknown error occurred. The process failed to enter an idle state.-or-The process has already exited. -or-No process is associated with this <see cref="T:System.Diagnostics.Process" /> object.</exception>
	/// <filterpriority>1</filterpriority>
	public bool WaitForInputIdle()
	{
		return WaitForInputIdle(int.MaxValue);
	}

	/// <summary>Begins asynchronous read operations on the redirected <see cref="P:System.Diagnostics.Process.StandardOutput" /> stream of the application.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Diagnostics.ProcessStartInfo.RedirectStandardOutput" /> property is false.- or - An asynchronous read operation is already in progress on the <see cref="P:System.Diagnostics.Process.StandardOutput" /> stream.- or - The <see cref="P:System.Diagnostics.Process.StandardOutput" /> stream has been used by a synchronous read operation. </exception>
	/// <filterpriority>2</filterpriority>
	[ComVisible(false)]
	public void BeginOutputReadLine()
	{
		if (outputStreamReadMode == StreamReadMode.undefined)
		{
			outputStreamReadMode = StreamReadMode.asyncMode;
		}
		else if (outputStreamReadMode != StreamReadMode.asyncMode)
		{
			throw new InvalidOperationException(global::SR.GetString("Cannot mix synchronous and asynchronous operation on process stream."));
		}
		if (pendingOutputRead)
		{
			throw new InvalidOperationException(global::SR.GetString("An async read operation has already been started on the stream."));
		}
		pendingOutputRead = true;
		if (output == null)
		{
			if (standardOutput == null)
			{
				throw new InvalidOperationException(global::SR.GetString("StandardOut has not been redirected or the process hasn't started yet."));
			}
			Stream baseStream = standardOutput.BaseStream;
			output = new AsyncStreamReader(this, baseStream, OutputReadNotifyUser, standardOutput.CurrentEncoding);
		}
		output.BeginReadLine();
	}

	/// <summary>Begins asynchronous read operations on the redirected <see cref="P:System.Diagnostics.Process.StandardError" /> stream of the application.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Diagnostics.ProcessStartInfo.RedirectStandardError" /> property is false.- or - An asynchronous read operation is already in progress on the <see cref="P:System.Diagnostics.Process.StandardError" /> stream.- or - The <see cref="P:System.Diagnostics.Process.StandardError" /> stream has been used by a synchronous read operation. </exception>
	/// <filterpriority>2</filterpriority>
	[ComVisible(false)]
	public void BeginErrorReadLine()
	{
		if (errorStreamReadMode == StreamReadMode.undefined)
		{
			errorStreamReadMode = StreamReadMode.asyncMode;
		}
		else if (errorStreamReadMode != StreamReadMode.asyncMode)
		{
			throw new InvalidOperationException(global::SR.GetString("Cannot mix synchronous and asynchronous operation on process stream."));
		}
		if (pendingErrorRead)
		{
			throw new InvalidOperationException(global::SR.GetString("An async read operation has already been started on the stream."));
		}
		pendingErrorRead = true;
		if (error == null)
		{
			if (standardError == null)
			{
				throw new InvalidOperationException(global::SR.GetString("StandardError has not been redirected."));
			}
			Stream baseStream = standardError.BaseStream;
			error = new AsyncStreamReader(this, baseStream, ErrorReadNotifyUser, standardError.CurrentEncoding);
		}
		error.BeginReadLine();
	}

	/// <summary>Cancels the asynchronous read operation on the redirected <see cref="P:System.Diagnostics.Process.StandardOutput" /> stream of an application.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Diagnostics.Process.StandardOutput" /> stream is not enabled for asynchronous read operations. </exception>
	/// <filterpriority>2</filterpriority>
	[ComVisible(false)]
	public void CancelOutputRead()
	{
		if (output != null)
		{
			output.CancelOperation();
			pendingOutputRead = false;
			return;
		}
		throw new InvalidOperationException(global::SR.GetString("No async read operation is in progress on the stream."));
	}

	/// <summary>Cancels the asynchronous read operation on the redirected <see cref="P:System.Diagnostics.Process.StandardError" /> stream of an application.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Diagnostics.Process.StandardError" /> stream is not enabled for asynchronous read operations. </exception>
	/// <filterpriority>2</filterpriority>
	[ComVisible(false)]
	public void CancelErrorRead()
	{
		if (error != null)
		{
			error.CancelOperation();
			pendingErrorRead = false;
			return;
		}
		throw new InvalidOperationException(global::SR.GetString("No async read operation is in progress on the stream."));
	}

	internal void OutputReadNotifyUser(string data)
	{
		DataReceivedEventHandler dataReceivedEventHandler = this.OutputDataReceived;
		if (dataReceivedEventHandler != null)
		{
			DataReceivedEventArgs dataReceivedEventArgs = new DataReceivedEventArgs(data);
			if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
			{
				SynchronizingObject.Invoke(dataReceivedEventHandler, new object[2] { this, dataReceivedEventArgs });
			}
			else
			{
				dataReceivedEventHandler(this, dataReceivedEventArgs);
			}
		}
	}

	internal void ErrorReadNotifyUser(string data)
	{
		DataReceivedEventHandler dataReceivedEventHandler = this.ErrorDataReceived;
		if (dataReceivedEventHandler != null)
		{
			DataReceivedEventArgs dataReceivedEventArgs = new DataReceivedEventArgs(data);
			if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
			{
				SynchronizingObject.Invoke(dataReceivedEventHandler, new object[2] { this, dataReceivedEventArgs });
			}
			else
			{
				dataReceivedEventHandler(this, dataReceivedEventArgs);
			}
		}
	}

	private Process(SafeProcessHandle handle, int id)
	{
		SetProcessHandle(handle);
		SetProcessId(id);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern ProcessModule[] GetModules_internal(IntPtr handle);

	private ProcessModule[] GetModules_internal(SafeProcessHandle handle)
	{
		bool success = false;
		try
		{
			handle.DangerousAddRef(ref success);
			return GetModules_internal(handle.DangerousGetHandle());
		}
		finally
		{
			if (success)
			{
				handle.DangerousRelease();
			}
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern long GetProcessData(int pid, int data_type, out int error);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string ProcessName_internal(IntPtr handle);

	private static string ProcessName_internal(SafeProcessHandle handle)
	{
		bool success = false;
		try
		{
			handle.DangerousAddRef(ref success);
			return ProcessName_internal(handle.DangerousGetHandle());
		}
		finally
		{
			if (success)
			{
				handle.DangerousRelease();
			}
		}
	}

	/// <summary>Closes a process that has a user interface by sending a close message to its main window.</summary>
	/// <returns>true if the close message was successfully sent; false if the associated process does not have a main window or if the main window is disabled (for example if a modal dialog is being shown).</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is Windows 98 or Windows Millennium Edition (Windows Me); set the <see cref="P:System.Diagnostics.ProcessStartInfo.UseShellExecute" /> property to false to access this property on Windows 98 and Windows Me.</exception>
	/// <exception cref="T:System.InvalidOperationException">The process has already exited. -or-No process is associated with this <see cref="T:System.Diagnostics.Process" /> object.</exception>
	/// <filterpriority>1</filterpriority>
	public bool CloseMainWindow()
	{
		SafeProcessHandle safeProcessHandle = null;
		try
		{
			safeProcessHandle = GetProcessHandle(1);
			return NativeMethods.TerminateProcess(safeProcessHandle, -2);
		}
		finally
		{
			ReleaseProcessHandle(safeProcessHandle);
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern IntPtr GetProcess_internal(int pid);

	/// <summary>Returns a new <see cref="T:System.Diagnostics.Process" /> component, given a process identifier and the name of a computer on the network.</summary>
	/// <returns>A <see cref="T:System.Diagnostics.Process" /> component that is associated with a remote process resource identified by the <paramref name="processId" /> parameter.</returns>
	/// <param name="processId">The system-unique identifier of a process resource. </param>
	/// <param name="machineName">The name of a computer on the network. </param>
	/// <exception cref="T:System.ArgumentException">The process specified by the <paramref name="processId" /> parameter is not running. The identifier might be expired.-or- The <paramref name="machineName" /> parameter syntax is invalid. The name might have length zero (0). </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="machineName" /> parameter is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The process was not started by this object.</exception>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("There is no support for retrieving process information from a remote machine")]
	public static Process GetProcessById(int processId, string machineName)
	{
		if (machineName == null)
		{
			throw new ArgumentNullException("machineName");
		}
		if (!IsLocalMachine(machineName))
		{
			throw new NotImplementedException();
		}
		IntPtr process_internal = GetProcess_internal(processId);
		if (process_internal == IntPtr.Zero)
		{
			throw new ArgumentException("Can't find process with ID " + processId);
		}
		return new Process(new SafeProcessHandle(process_internal, ownsHandle: true), processId);
	}

	/// <summary>Creates an array of new <see cref="T:System.Diagnostics.Process" /> components and associates them with all the process resources on a remote computer that share the specified process name.</summary>
	/// <returns>An array of type <see cref="T:System.Diagnostics.Process" /> that represents the process resources running the specified application or file.</returns>
	/// <param name="processName">The friendly name of the process. </param>
	/// <param name="machineName">The name of a computer on the network. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="machineName" /> parameter syntax is invalid. It might have length zero (0). </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="machineName" /> parameter is null. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system platform does not support this operation on remote computers. </exception>
	/// <exception cref="T:System.InvalidOperationException">There are problems accessing the performance counter API's used to get process information. This exception is specific to Windows NT, Windows 2000, and Windows XP. </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A problem occurred accessing an underlying system API. </exception>
	/// <filterpriority>1</filterpriority>
	public static Process[] GetProcessesByName(string processName, string machineName)
	{
		if (machineName == null)
		{
			throw new ArgumentNullException("machineName");
		}
		if (!IsLocalMachine(machineName))
		{
			throw new NotImplementedException();
		}
		Process[] array = GetProcesses();
		if (array.Length == 0)
		{
			return array;
		}
		int newSize = 0;
		for (int i = 0; i < array.Length; i++)
		{
			try
			{
				if (string.Compare(processName, array[i].ProcessName, ignoreCase: true) == 0)
				{
					array[newSize++] = array[i];
				}
			}
			catch (SystemException)
			{
			}
		}
		Array.Resize(ref array, newSize);
		return array;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern int[] GetProcesses_internal();

	/// <summary>Creates a new <see cref="T:System.Diagnostics.Process" /> component for each process resource on the specified computer.</summary>
	/// <returns>An array of type <see cref="T:System.Diagnostics.Process" /> that represents all the process resources running on the specified computer.</returns>
	/// <param name="machineName">The computer from which to read the list of processes. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="machineName" /> parameter syntax is invalid. It might have length zero (0). </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="machineName" /> parameter is null. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system platform does not support this operation on remote computers. </exception>
	/// <exception cref="T:System.InvalidOperationException">There are problems accessing the performance counter API's used to get process information. This exception is specific to Windows NT, Windows 2000, and Windows XP. </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A problem occurred accessing an underlying system API. </exception>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("There is no support for retrieving process information from a remote machine")]
	public static Process[] GetProcesses(string machineName)
	{
		if (machineName == null)
		{
			throw new ArgumentNullException("machineName");
		}
		if (!IsLocalMachine(machineName))
		{
			throw new NotImplementedException();
		}
		int[] processes_internal = GetProcesses_internal();
		if (processes_internal == null)
		{
			return new Process[0];
		}
		List<Process> list = new List<Process>(processes_internal.Length);
		for (int i = 0; i < processes_internal.Length; i++)
		{
			try
			{
				list.Add(GetProcessById(processes_internal[i]));
			}
			catch (SystemException)
			{
			}
		}
		return list.ToArray();
	}

	private static bool IsLocalMachine(string machineName)
	{
		if (machineName == "." || machineName.Length == 0)
		{
			return true;
		}
		return string.Compare(machineName, Environment.MachineName, ignoreCase: true) == 0;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool ShellExecuteEx_internal(ProcessStartInfo startInfo, ref ProcInfo procInfo);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool CreateProcess_internal(ProcessStartInfo startInfo, IntPtr stdin, IntPtr stdout, IntPtr stderr, ref ProcInfo procInfo);

	private bool StartWithShellExecuteEx(ProcessStartInfo startInfo)
	{
		if (disposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!string.IsNullOrEmpty(startInfo.UserName) || startInfo.Password != null)
		{
			throw new InvalidOperationException(global::SR.GetString("The Process object must have the UseShellExecute property set to false in order to start a process as a user."));
		}
		if (startInfo.RedirectStandardInput || startInfo.RedirectStandardOutput || startInfo.RedirectStandardError)
		{
			throw new InvalidOperationException(global::SR.GetString("The Process object must have the UseShellExecute property set to false in order to redirect IO streams."));
		}
		if (startInfo.StandardErrorEncoding != null)
		{
			throw new InvalidOperationException(global::SR.GetString("StandardErrorEncoding is only supported when standard error is redirected."));
		}
		if (startInfo.StandardOutputEncoding != null)
		{
			throw new InvalidOperationException(global::SR.GetString("StandardOutputEncoding is only supported when standard output is redirected."));
		}
		if (startInfo.environmentVariables != null)
		{
			throw new InvalidOperationException(global::SR.GetString("The Process object must have the UseShellExecute property set to false in order to use environment variables."));
		}
		ProcInfo procInfo = default(ProcInfo);
		FillUserInfo(startInfo, ref procInfo);
		bool flag;
		try
		{
			flag = ShellExecuteEx_internal(startInfo, ref procInfo);
		}
		finally
		{
			if (procInfo.Password != IntPtr.Zero)
			{
				Marshal.ZeroFreeBSTR(procInfo.Password);
			}
			procInfo.Password = IntPtr.Zero;
		}
		if (!flag)
		{
			throw new Win32Exception(-procInfo.pid);
		}
		SetProcessHandle(new SafeProcessHandle(procInfo.process_handle, ownsHandle: true));
		SetProcessId(procInfo.pid);
		return flag;
	}

	private static void CreatePipe(out IntPtr read, out IntPtr write, bool writeDirection)
	{
		if (!MonoIO.CreatePipe(out read, out write, out var monoIOError))
		{
			throw MonoIO.GetException(monoIOError);
		}
		if (!IsWindows)
		{
			return;
		}
		IntPtr target_handle = (writeDirection ? write : read);
		if (!MonoIO.DuplicateHandle(GetCurrentProcess().Handle, target_handle, GetCurrentProcess().Handle, out target_handle, 0, 0, 2, out monoIOError))
		{
			throw MonoIO.GetException(monoIOError);
		}
		if (writeDirection)
		{
			if (!MonoIO.Close(write, out monoIOError))
			{
				throw MonoIO.GetException(monoIOError);
			}
			write = target_handle;
		}
		else
		{
			if (!MonoIO.Close(read, out monoIOError))
			{
				throw MonoIO.GetException(monoIOError);
			}
			read = target_handle;
		}
	}

	private bool StartWithCreateProcess(ProcessStartInfo startInfo)
	{
		if (startInfo.StandardOutputEncoding != null && !startInfo.RedirectStandardOutput)
		{
			throw new InvalidOperationException(global::SR.GetString("StandardOutputEncoding is only supported when standard output is redirected."));
		}
		if (startInfo.StandardErrorEncoding != null && !startInfo.RedirectStandardError)
		{
			throw new InvalidOperationException(global::SR.GetString("StandardErrorEncoding is only supported when standard error is redirected."));
		}
		if (disposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		ProcInfo procInfo = default(ProcInfo);
		if (startInfo.HaveEnvVars)
		{
			List<string> list = null;
			StringBuilder stringBuilder = null;
			foreach (DictionaryEntry environmentVariable in startInfo.EnvironmentVariables)
			{
				if (environmentVariable.Value != null)
				{
					if (list == null)
					{
						list = new List<string>();
					}
					if (stringBuilder == null)
					{
						stringBuilder = new StringBuilder();
					}
					else
					{
						stringBuilder.Clear();
					}
					stringBuilder.Append((string)environmentVariable.Key);
					stringBuilder.Append('=');
					stringBuilder.Append((string)environmentVariable.Value);
					list.Add(stringBuilder.ToString());
				}
			}
			procInfo.envVariables = list?.ToArray();
		}
		IntPtr read = IntPtr.Zero;
		IntPtr write = IntPtr.Zero;
		IntPtr read2 = IntPtr.Zero;
		IntPtr write2 = IntPtr.Zero;
		IntPtr read3 = IntPtr.Zero;
		IntPtr write3 = IntPtr.Zero;
		MonoIOError monoIOError;
		try
		{
			if (startInfo.RedirectStandardInput)
			{
				CreatePipe(out read, out write, writeDirection: true);
			}
			else
			{
				read = MonoIO.ConsoleInput;
				write = IntPtr.Zero;
			}
			if (startInfo.RedirectStandardOutput)
			{
				CreatePipe(out read2, out write2, writeDirection: false);
			}
			else
			{
				read2 = IntPtr.Zero;
				write2 = MonoIO.ConsoleOutput;
			}
			if (startInfo.RedirectStandardError)
			{
				CreatePipe(out read3, out write3, writeDirection: false);
			}
			else
			{
				read3 = IntPtr.Zero;
				write3 = MonoIO.ConsoleError;
			}
			FillUserInfo(startInfo, ref procInfo);
			if (!CreateProcess_internal(startInfo, read, write2, write3, ref procInfo))
			{
				throw new Win32Exception(-procInfo.pid, "ApplicationName='" + startInfo.FileName + "', CommandLine='" + startInfo.Arguments + "', CurrentDirectory='" + startInfo.WorkingDirectory + "', Native error= " + Win32Exception.GetErrorMessage(-procInfo.pid));
			}
		}
		catch
		{
			if (startInfo.RedirectStandardInput)
			{
				if (read != IntPtr.Zero)
				{
					MonoIO.Close(read, out monoIOError);
				}
				if (write != IntPtr.Zero)
				{
					MonoIO.Close(write, out monoIOError);
				}
			}
			if (startInfo.RedirectStandardOutput)
			{
				if (read2 != IntPtr.Zero)
				{
					MonoIO.Close(read2, out monoIOError);
				}
				if (write2 != IntPtr.Zero)
				{
					MonoIO.Close(write2, out monoIOError);
				}
			}
			if (startInfo.RedirectStandardError)
			{
				if (read3 != IntPtr.Zero)
				{
					MonoIO.Close(read3, out monoIOError);
				}
				if (write3 != IntPtr.Zero)
				{
					MonoIO.Close(write3, out monoIOError);
				}
			}
			throw;
		}
		finally
		{
			if (procInfo.Password != IntPtr.Zero)
			{
				Marshal.ZeroFreeBSTR(procInfo.Password);
				procInfo.Password = IntPtr.Zero;
			}
		}
		SetProcessHandle(new SafeProcessHandle(procInfo.process_handle, ownsHandle: true));
		SetProcessId(procInfo.pid);
		if (startInfo.RedirectStandardInput)
		{
			MonoIO.Close(read, out monoIOError);
			Encoding inputEncoding = Console.InputEncoding;
			standardInput = new StreamWriter(new FileStream(write, FileAccess.Write, ownsHandle: true, 8192), inputEncoding)
			{
				AutoFlush = true
			};
		}
		if (startInfo.RedirectStandardOutput)
		{
			MonoIO.Close(write2, out monoIOError);
			Encoding encoding = startInfo.StandardOutputEncoding ?? Console.Out.Encoding;
			standardOutput = new StreamReader(new FileStream(read2, FileAccess.Read, ownsHandle: true, 8192), encoding, detectEncodingFromByteOrderMarks: true);
		}
		if (startInfo.RedirectStandardError)
		{
			MonoIO.Close(write3, out monoIOError);
			Encoding encoding2 = startInfo.StandardErrorEncoding ?? Console.Out.Encoding;
			standardError = new StreamReader(new FileStream(read3, FileAccess.Read, ownsHandle: true, 8192), encoding2, detectEncodingFromByteOrderMarks: true);
		}
		return true;
	}

	private static void FillUserInfo(ProcessStartInfo startInfo, ref ProcInfo procInfo)
	{
		if (startInfo.UserName.Length != 0)
		{
			procInfo.UserName = startInfo.UserName;
			procInfo.Domain = startInfo.Domain;
			if (startInfo.Password != null)
			{
				procInfo.Password = Marshal.SecureStringToBSTR(startInfo.Password);
			}
			else
			{
				procInfo.Password = IntPtr.Zero;
			}
			procInfo.LoadUserProfile = startInfo.LoadUserProfile;
		}
	}

	private void RaiseOnExited()
	{
		if (!watchForExit || raisedOnExited)
		{
			return;
		}
		lock (this)
		{
			if (!raisedOnExited)
			{
				raisedOnExited = true;
				OnExited();
			}
		}
	}
}
