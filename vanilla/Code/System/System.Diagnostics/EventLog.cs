using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Diagnostics;

/// <summary>Provides interaction with Windows event logs. </summary>
/// <filterpriority>1</filterpriority>
[DefaultEvent("EntryWritten")]
[InstallerType(typeof(EventLogInstaller))]
[MonitoringDescription("Represents an event log")]
public class EventLog : Component, ISupportInitialize
{
	private string source;

	private string logName;

	private string machineName;

	private bool doRaiseEvents;

	private ISynchronizeInvoke synchronizingObject;

	internal const string LOCAL_FILE_IMPL = "local";

	private const string WIN32_IMPL = "win32";

	private const string NULL_IMPL = "null";

	internal const string EVENTLOG_TYPE_VAR = "MONO_EVENTLOG_TYPE";

	private EventLogImpl Impl;

	/// <summary>Gets or sets a value indicating whether the <see cref="T:System.Diagnostics.EventLog" /> receives <see cref="E:System.Diagnostics.EventLog.EntryWritten" /> event notifications.</summary>
	/// <returns>true if the <see cref="T:System.Diagnostics.EventLog" /> receives notification when an entry is written to the log; otherwise, false.</returns>
	/// <exception cref="T:System.InvalidOperationException">The event log is on a remote computer.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[MonitoringDescription("If enabled raises event when a log is written.")]
	[Browsable(false)]
	[DefaultValue(false)]
	public bool EnableRaisingEvents
	{
		get
		{
			return doRaiseEvents;
		}
		set
		{
			if (value != doRaiseEvents)
			{
				if (value)
				{
					Impl.EnableNotification();
				}
				else
				{
					Impl.DisableNotification();
				}
				doRaiseEvents = value;
			}
		}
	}

	/// <summary>Gets the contents of the event log.</summary>
	/// <returns>An <see cref="T:System.Diagnostics.EventLogEntryCollection" /> holding the entries in the event log. Each entry is associated with an instance of the <see cref="T:System.Diagnostics.EventLogEntry" /> class.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MonitoringDescription("The entries in the log.")]
	[Browsable(false)]
	public EventLogEntryCollection Entries => new EventLogEntryCollection(Impl);

	/// <summary>Gets or sets the name of the log to read from or write to.</summary>
	/// <returns>The name of the log. This can be Application, System, Security, or a custom log name. The default is an empty string ("").</returns>
	/// <filterpriority>1</filterpriority>
	[DefaultValue("")]
	[RecommendedAsConfigurable(true)]
	[TypeConverter("System.Diagnostics.Design.LogConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[MonitoringDescription("Name of the log that is read and written.")]
	[ReadOnly(true)]
	public string Log
	{
		get
		{
			if (source != null && source.Length > 0)
			{
				return GetLogName();
			}
			return logName;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (string.Compare(logName, value, ignoreCase: true) != 0)
			{
				logName = value;
				Reset();
			}
		}
	}

	/// <summary>Gets the event log's friendly name.</summary>
	/// <returns>A name that represents the event log in the system's event viewer.</returns>
	/// <exception cref="T:System.InvalidOperationException">The specified <see cref="P:System.Diagnostics.EventLog.Log" /> does not exist in the registry for this computer.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[Browsable(false)]
	public string LogDisplayName => Impl.LogDisplayName;

	/// <summary>Gets or sets the name of the computer on which to read or write events.</summary>
	/// <returns>The name of the server on which the event log resides. The default is the local computer (".").</returns>
	/// <exception cref="T:System.ArgumentException">The computer name is invalid. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[ReadOnly(true)]
	[DefaultValue(".")]
	[RecommendedAsConfigurable(true)]
	[MonitoringDescription("Name of the machine that this log get written to.")]
	public string MachineName
	{
		get
		{
			return machineName;
		}
		set
		{
			if (value == null || value.Trim().Length == 0)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Invalid value {0} for property MachineName.", value));
			}
			if (string.Compare(machineName, value, ignoreCase: true) != 0)
			{
				Close();
				machineName = value;
			}
		}
	}

	/// <summary>Gets or sets the source name to register and use when writing to the event log.</summary>
	/// <returns>The name registered with the event log as a source of entries. The default is an empty string ("").</returns>
	/// <exception cref="T:System.ArgumentException">The source name results in a registry key path longer than 254 characters.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[MonitoringDescription("The application name that writes the log.")]
	[TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[RecommendedAsConfigurable(true)]
	[DefaultValue("")]
	[ReadOnly(true)]
	public string Source
	{
		get
		{
			return source;
		}
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}
			if (source == null || (source.Length == 0 && (logName == null || logName.Length == 0)))
			{
				source = value;
			}
			else if (string.Compare(source, value, ignoreCase: true) != 0)
			{
				source = value;
				Reset();
			}
		}
	}

	/// <summary>Gets or sets the object used to marshal the event handler calls issued as a result of an <see cref="T:System.Diagnostics.EventLog" /> entry written event.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.ISynchronizeInvoke" /> used to marshal event-handler calls issued as a result of an <see cref="E:System.Diagnostics.EventLog.EntryWritten" /> event on the event log.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[Browsable(false)]
	[DefaultValue(null)]
	[MonitoringDescription("An object that synchronizes event handler calls.")]
	public ISynchronizeInvoke SynchronizingObject
	{
		get
		{
			return synchronizingObject;
		}
		set
		{
			synchronizingObject = value;
		}
	}

	/// <summary>Gets the configured behavior for storing new entries when the event log reaches its maximum log file size.</summary>
	/// <returns>The <see cref="T:System.Diagnostics.OverflowAction" /> value that specifies the configured behavior for storing new entries when the event log reaches its maximum log size. The default is <see cref="F:System.Diagnostics.OverflowAction.OverwriteOlder" />.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[ComVisible(false)]
	[Browsable(false)]
	[System.MonoTODO]
	public OverflowAction OverflowAction => Impl.OverflowAction;

	/// <summary>Gets the number of days to retain entries in the event log.</summary>
	/// <returns>The number of days that entries in the event log are retained. The default value is 7.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoTODO]
	[ComVisible(false)]
	[Browsable(false)]
	public int MinimumRetentionDays => Impl.MinimumRetentionDays;

	/// <summary>Gets or sets the maximum event log size in kilobytes.</summary>
	/// <returns>The maximum event log size in kilobytes. The default is 512, indicating a maximum file size of 512 kilobytes.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The specified value is less than 64, or greater than 4194240, or not an even multiple of 64. </exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Diagnostics.EventLog.Log" /> value is not a valid log name.- or -The registry key for the event log could not be opened on the target computer.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	[ComVisible(false)]
	[System.MonoTODO]
	public long MaximumKilobytes
	{
		get
		{
			return Impl.MaximumKilobytes;
		}
		set
		{
			Impl.MaximumKilobytes = value;
		}
	}

	private static bool Win32EventLogEnabled => Environment.OSVersion.Platform == PlatformID.Win32NT;

	private static string EventLogImplType
	{
		get
		{
			string environmentVariable = Environment.GetEnvironmentVariable("MONO_EVENTLOG_TYPE");
			if (environmentVariable == null)
			{
				if (Win32EventLogEnabled)
				{
					return "win32";
				}
				return "null";
			}
			if (Win32EventLogEnabled && string.Compare(environmentVariable, "win32", ignoreCase: true) == 0)
			{
				return "win32";
			}
			if (string.Compare(environmentVariable, "null", ignoreCase: true) == 0)
			{
				return "null";
			}
			if (string.Compare(environmentVariable, 0, "local", 0, "local".Length, ignoreCase: true) == 0)
			{
				return "local";
			}
			throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Eventlog implementation '{0}' is not supported.", environmentVariable));
		}
	}

	/// <summary>Occurs when an entry is written to an event log on the local computer.</summary>
	/// <filterpriority>2</filterpriority>
	[MonitoringDescription("Raised for each EventLog entry written.")]
	public event EntryWrittenEventHandler EntryWritten;

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.EventLog" /> class. Does not associate the instance with any log.</summary>
	public EventLog()
		: this(string.Empty)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.EventLog" /> class. Associates the instance with a log on the local computer.</summary>
	/// <param name="logName">The name of the log on the local computer. </param>
	/// <exception cref="T:System.ArgumentNullException">The log name is null. </exception>
	/// <exception cref="T:System.ArgumentException">The log name is invalid. </exception>
	public EventLog(string logName)
		: this(logName, ".")
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.EventLog" /> class. Associates the instance with a log on the specified computer.</summary>
	/// <param name="logName">The name of the log on the specified computer. </param>
	/// <param name="machineName">The computer on which the log exists. </param>
	/// <exception cref="T:System.ArgumentNullException">The log name is null. </exception>
	/// <exception cref="T:System.ArgumentException">The log name is invalid.-or- The computer name is invalid. </exception>
	public EventLog(string logName, string machineName)
		: this(logName, machineName, string.Empty)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.EventLog" /> class. Associates the instance with a log on the specified computer and creates or assigns the specified source to the <see cref="T:System.Diagnostics.EventLog" />.</summary>
	/// <param name="logName">The name of the log on the specified computer </param>
	/// <param name="machineName">The computer on which the log exists. </param>
	/// <param name="source">The source of event log entries. </param>
	/// <exception cref="T:System.ArgumentNullException">The log name is null. </exception>
	/// <exception cref="T:System.ArgumentException">The log name is invalid.-or- The computer name is invalid. </exception>
	public EventLog(string logName, string machineName, string source)
	{
		if (logName == null)
		{
			throw new ArgumentNullException("logName");
		}
		if (machineName == null || machineName.Trim().Length == 0)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Invalid value '{0}' for parameter 'machineName'.", machineName));
		}
		this.source = source;
		this.machineName = machineName;
		this.logName = logName;
		Impl = CreateEventLogImpl(this);
	}

	/// <summary>Changes the configured behavior for writing new entries when the event log reaches its maximum file size.</summary>
	/// <param name="action">The overflow behavior for writing new entries to the event log. </param>
	/// <param name="retentionDays">The minimum number of days each event log entry is retained. This parameter is used only if <paramref name="action" /> is set to <see cref="F:System.Diagnostics.OverflowAction.OverwriteOlder" />. </param>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="action" /> is not a valid <see cref="P:System.Diagnostics.EventLog.OverflowAction" /> value. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="retentionDays" /> is less than one, or larger than 365. </exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Diagnostics.EventLog.Log" /> value is not a valid log name.- or -The registry key for the event log could not be opened on the target computer.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoTODO]
	[ComVisible(false)]
	public void ModifyOverflowPolicy(OverflowAction action, int retentionDays)
	{
		Impl.ModifyOverflowPolicy(action, retentionDays);
	}

	/// <summary>Specifies the localized name of the event log, which is displayed in the server Event Viewer.</summary>
	/// <param name="resourceFile">The fully specified path to a localized resource file. </param>
	/// <param name="resourceId">The resource identifier that indexes a localized string within the resource file. </param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Diagnostics.EventLog.Log" /> value is not a valid log name.- or -The registry key for the event log could not be opened on the target computer.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="resourceFile " />is null.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[ComVisible(false)]
	[System.MonoTODO]
	public void RegisterDisplayName(string resourceFile, long resourceId)
	{
		Impl.RegisterDisplayName(resourceFile, resourceId);
	}

	/// <summary>Begins the initialization of an <see cref="T:System.Diagnostics.EventLog" /> used on a form or used by another component. The initialization occurs at runtime.</summary>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="T:System.Diagnostics.EventLog" /> is already initialized.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void BeginInit()
	{
		Impl.BeginInit();
	}

	/// <summary>Removes all entries from the event log.</summary>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The event log was not cleared successfully.-or- The log cannot be opened. A Windows error code is not available. </exception>
	/// <exception cref="T:System.ArgumentException">A value is not specified for the <see cref="P:System.Diagnostics.EventLog.Log" /> property. Make sure the log name is not an empty string. </exception>
	/// <exception cref="T:System.InvalidOperationException">The log does not exist. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void Clear()
	{
		string log = Log;
		if (log == null || log.Length == 0)
		{
			throw new ArgumentException("Log property value has not been specified.");
		}
		if (!Exists(log, MachineName))
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Event Log '{0}' does not exist on computer '{1}'.", log, machineName));
		}
		Impl.Clear();
		Reset();
	}

	/// <summary>Closes the event log and releases read and write handles.</summary>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The event log's read handle or write handle was not released successfully. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void Close()
	{
		Impl.Close();
		EnableRaisingEvents = false;
	}

	internal void Reset()
	{
		bool enableRaisingEvents = EnableRaisingEvents;
		Close();
		EnableRaisingEvents = enableRaisingEvents;
	}

	/// <summary>Establishes the specified source name as a valid event source for writing entries to a log on the local computer. This method can also create a new custom log on the local computer.</summary>
	/// <param name="source">The source name by which the application is registered on the local computer. </param>
	/// <param name="logName">The name of the log the source's entries are written to. Possible values include Application, System, or a custom event log. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="source" /> is an empty string ("") or null.- or - <paramref name="logName" /> is not a valid event log name. Event log names must consist of printable characters, and cannot include the characters '*', '?', or '\'.- or - <paramref name="logName" /> is not valid for user log creation. The event log names AppEvent, SysEvent, and SecEvent are reserved for system use.- or - The log name matches an existing event source name.- or - The source name results in a registry key path longer than 254 characters.- or - The first 8 characters of <paramref name="logName" /> match the first 8 characters of an existing event log name.- or - The source cannot be registered because it already exists on the local computer.- or - The source name matches an existing event log name. </exception>
	/// <exception cref="T:System.InvalidOperationException">The registry key for the event log could not be opened on the local computer. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void CreateEventSource(string source, string logName)
	{
		CreateEventSource(source, logName, ".");
	}

	/// <summary>Establishes the specified source name as a valid event source for writing entries to a log on the specified computer. This method can also be used to create a new custom log on the specified computer.</summary>
	/// <param name="source">The source by which the application is registered on the specified computer. </param>
	/// <param name="logName">The name of the log the source's entries are written to. Possible values include Application, System, or a custom event log. If you do not specify a value, <paramref name="logName" /> defaults to Application. </param>
	/// <param name="machineName">The name of the computer to register this event source with, or "." for the local computer. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="machineName" /> is not a valid computer name.- or - <paramref name="source" /> is an empty string ("") or null.- or - <paramref name="logName" /> is not a valid event log name. Event log names must consist of printable characters, and cannot include the characters '*', '?', or '\'.- or - <paramref name="logName" /> is not valid for user log creation. The event log names AppEvent, SysEvent, and SecEvent are reserved for system use.- or - The log name matches an existing event source name.- or - The source name results in a registry key path longer than 254 characters.- or - The first 8 characters of <paramref name="logName" /> match the first 8 characters of an existing event log name on the specified computer.- or - The source cannot be registered because it already exists on the specified computer.- or - The source name matches an existing event source name. </exception>
	/// <exception cref="T:System.InvalidOperationException">The registry key for the event log could not be opened on the specified computer. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[Obsolete("use CreateEventSource(EventSourceCreationData) instead")]
	public static void CreateEventSource(string source, string logName, string machineName)
	{
		CreateEventSource(new EventSourceCreationData(source, logName, machineName));
	}

	/// <summary>Establishes a valid event source for writing localized event messages, using the specified configuration properties for the event source and the corresponding event log.</summary>
	/// <param name="sourceData">The configuration properties for the event source and its target event log. </param>
	/// <exception cref="T:System.ArgumentException">The computer name specified in <paramref name="sourceData" /> is not valid.- or - The source name specified in <paramref name="sourceData" /> is null.- or - The log name specified in <paramref name="sourceData" /> is not valid. Event log names must consist of printable characters and cannot include the characters '*', '?', or '\'.- or - The log name specified in <paramref name="sourceData" /> is not valid for user log creation. The Event log names AppEvent, SysEvent, and SecEvent are reserved for system use.- or - The log name matches an existing event source name.- or - The source name specified in <paramref name="sourceData" /> results in a registry key path longer than 254 characters.- or - The first 8 characters of the log name specified in <paramref name="sourceData" /> are not unique.- or - The source name specified in <paramref name="sourceData" /> is already registered.- or - The source name specified in <paramref name="sourceData" /> matches an existing event log name.</exception>
	/// <exception cref="T:System.InvalidOperationException">The registry key for the event log could not be opened. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sourceData" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoNotSupported("remote machine is not supported")]
	public static void CreateEventSource(EventSourceCreationData sourceData)
	{
		if (sourceData.Source == null || sourceData.Source.Length == 0)
		{
			throw new ArgumentException("Source property value has not been specified.");
		}
		if (sourceData.LogName == null || sourceData.LogName.Length == 0)
		{
			throw new ArgumentException("Log property value has not been specified.");
		}
		if (SourceExists(sourceData.Source, sourceData.MachineName))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Source '{0}' already exists on '{1}'.", sourceData.Source, sourceData.MachineName));
		}
		CreateEventLogImpl(sourceData.LogName, sourceData.MachineName, sourceData.Source).CreateEventSource(sourceData);
	}

	/// <summary>Removes an event log from the local computer.</summary>
	/// <param name="logName">The name of the log to delete. Possible values include: Application, Security, System, and any custom event logs on the computer. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="logName" /> is an empty string ("") or null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The registry key for the event log could not be opened on the local computer.- or - The log does not exist on the local computer. </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The event log was not cleared successfully.-or- The log cannot be opened. A Windows error code is not available. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void Delete(string logName)
	{
		Delete(logName, ".");
	}

	/// <summary>Removes an event log from the specified computer.</summary>
	/// <param name="logName">The name of the log to delete. Possible values include: Application, Security, System, and any custom event logs on the specified computer. </param>
	/// <param name="machineName">The name of the computer to delete the log from, or "." for the local computer. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="logName" /> is an empty string ("") or null. - or - <paramref name="machineName" /> is not a valid computer name. </exception>
	/// <exception cref="T:System.InvalidOperationException">The registry key for the event log could not be opened on the specified computer.- or - The log does not exist on the specified computer. </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The event log was not cleared successfully.-or- The log cannot be opened. A Windows error code is not available. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoNotSupported("remote machine is not supported")]
	public static void Delete(string logName, string machineName)
	{
		if (machineName == null || machineName.Trim().Length == 0)
		{
			throw new ArgumentException("Invalid format for argument machineName.");
		}
		if (logName == null || logName.Length == 0)
		{
			throw new ArgumentException("Log to delete was not specified.");
		}
		CreateEventLogImpl(logName, machineName, string.Empty).Delete(logName, machineName);
	}

	/// <summary>Removes the event source registration from the event log of the local computer.</summary>
	/// <param name="source">The name by which the application is registered in the event log system. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="source" /> parameter does not exist in the registry of the local computer.- or - You do not have write access on the registry key for the event log.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void DeleteEventSource(string source)
	{
		DeleteEventSource(source, ".");
	}

	/// <summary>Removes the application's event source registration from the specified computer.</summary>
	/// <param name="source">The name by which the application is registered in the event log system. </param>
	/// <param name="machineName">The name of the computer to remove the registration from, or "." for the local computer. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="machineName" /> parameter is invalid. - or - The <paramref name="source" /> parameter does not exist in the registry of the specified computer.- or - You do not have write access on the registry key for the event log.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> cannot be deleted because in the registry, the parent registry key for <paramref name="source" /> does not contain a subkey with the same name.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoNotSupported("remote machine is not supported")]
	public static void DeleteEventSource(string source, string machineName)
	{
		if (machineName == null || machineName.Trim().Length == 0)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Invalid value '{0}' for parameter 'machineName'.", machineName));
		}
		CreateEventLogImpl(string.Empty, machineName, source).DeleteEventSource(source, machineName);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Diagnostics.EventLog" />, and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected override void Dispose(bool disposing)
	{
		if (Impl != null)
		{
			Impl.Dispose(disposing);
		}
	}

	/// <summary>Ends the initialization of an <see cref="T:System.Diagnostics.EventLog" /> used on a form or by another component. The initialization occurs at runtime.</summary>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void EndInit()
	{
		Impl.EndInit();
	}

	/// <summary>Determines whether the log exists on the local computer.</summary>
	/// <returns>true if the log exists on the local computer; otherwise, false.</returns>
	/// <param name="logName">The name of the log to search for. Possible values include: Application, Security, System, other application-specific logs (such as those associated with Active Directory), or any custom log on the computer. </param>
	/// <exception cref="T:System.ArgumentException">The logName is null or the value is empty. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static bool Exists(string logName)
	{
		return Exists(logName, ".");
	}

	/// <summary>Determines whether the log exists on the specified computer.</summary>
	/// <returns>true if the log exists on the specified computer; otherwise, false.</returns>
	/// <param name="logName">The log for which to search. Possible values include: Application, Security, System, other application-specific logs (such as those associated with Active Directory), or any custom log on the computer. </param>
	/// <param name="machineName">The name of the computer on which to search for the log, or "." for the local computer. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="machineName" /> parameter is an invalid format. Make sure you have used proper syntax for the computer on which you are searching.-or- The <paramref name="logName" /> is null or the value is empty. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoNotSupported("remote machine is not supported")]
	public static bool Exists(string logName, string machineName)
	{
		if (machineName == null || machineName.Trim().Length == 0)
		{
			throw new ArgumentException("Invalid format for argument machineName.");
		}
		if (logName == null || logName.Length == 0)
		{
			return false;
		}
		return CreateEventLogImpl(logName, machineName, string.Empty).Exists(logName, machineName);
	}

	/// <summary>Searches for all event logs on the local computer and creates an array of <see cref="T:System.Diagnostics.EventLog" /> objects that contain the list.</summary>
	/// <returns>An array of type <see cref="T:System.Diagnostics.EventLog" /> that represents the logs on the local computer.</returns>
	/// <exception cref="T:System.SystemException">You do not have read access to the registry.-or- There is no event log service on the computer. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static EventLog[] GetEventLogs()
	{
		return GetEventLogs(".");
	}

	/// <summary>Searches for all event logs on the given computer and creates an array of <see cref="T:System.Diagnostics.EventLog" /> objects that contain the list.</summary>
	/// <returns>An array of type <see cref="T:System.Diagnostics.EventLog" /> that represents the logs on the given computer.</returns>
	/// <param name="machineName">The computer on which to search for event logs. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="machineName" /> parameter is an invalid computer name. </exception>
	/// <exception cref="T:System.InvalidOperationException">You do not have read access to the registry.-or- There is no event log service on the computer. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoNotSupported("remote machine is not supported")]
	public static EventLog[] GetEventLogs(string machineName)
	{
		return CreateEventLogImpl(new EventLog()).GetEventLogs(machineName);
	}

	/// <summary>Gets the name of the log to which the specified source is registered.</summary>
	/// <returns>The name of the log associated with the specified source in the registry.</returns>
	/// <param name="source">The name of the event source. </param>
	/// <param name="machineName">The name of the computer on which to look, or "." for the local computer. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoNotSupported("remote machine is not supported")]
	public static string LogNameFromSourceName(string source, string machineName)
	{
		if (machineName == null || machineName.Trim().Length == 0)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Invalid value '{0}' for parameter 'MachineName'.", machineName));
		}
		return CreateEventLogImpl(string.Empty, machineName, source).LogNameFromSourceName(source, machineName);
	}

	/// <summary>Determines whether an event source is registered on the local computer.</summary>
	/// <returns>true if the event source is registered on the local computer; otherwise, false.</returns>
	/// <param name="source">The name of the event source. </param>
	/// <exception cref="T:System.Security.SecurityException">
	///   <paramref name="source" /> was not found, but some or all of the event logs could not be searched.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static bool SourceExists(string source)
	{
		return SourceExists(source, ".");
	}

	/// <summary>Determines whether an event source is registered on a specified computer.</summary>
	/// <returns>true if the event source is registered on the given computer; otherwise, false.</returns>
	/// <param name="source">The name of the event source. </param>
	/// <param name="machineName">The name the computer on which to look, or "." for the local computer. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="machineName" /> is an invalid computer name. </exception>
	/// <exception cref="T:System.Security.SecurityException">
	///   <paramref name="source" /> was not found, but some or all of the event logs could not be searched.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoNotSupported("remote machine is not supported")]
	public static bool SourceExists(string source, string machineName)
	{
		if (machineName == null || machineName.Trim().Length == 0)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Invalid value '{0}' for parameter 'machineName'.", machineName));
		}
		return CreateEventLogImpl(string.Empty, machineName, source).SourceExists(source, machineName);
	}

	/// <summary>Writes an information type entry, with the given message text, to the event log.</summary>
	/// <param name="message">The string to write to the event log. </param>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Diagnostics.EventLog.Source" /> property of the <see cref="T:System.Diagnostics.EventLog" /> has not been set.-or- The method attempted to register a new event source, but the computer name in <see cref="P:System.Diagnostics.EventLog.MachineName" />  is not valid.- or -The source is already registered for a different event log.- or -The message string is longer than 31,839 bytes (32,766 bytes on Windows operating systems before Windows Vista).- or -The source name results in a registry key path longer than 254 characters.</exception>
	/// <exception cref="T:System.InvalidOperationException">The registry key for the event log could not be opened.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The operating system reported an error when writing the event entry to the event log. A Windows error code is not available.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void WriteEntry(string message)
	{
		WriteEntry(message, EventLogEntryType.Information);
	}

	/// <summary>Writes an error, warning, information, success audit, or failure audit entry with the given message text to the event log.</summary>
	/// <param name="message">The string to write to the event log. </param>
	/// <param name="type">One of the <see cref="T:System.Diagnostics.EventLogEntryType" /> values. </param>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Diagnostics.EventLog.Source" /> property of the <see cref="T:System.Diagnostics.EventLog" /> has not been set.-or- The method attempted to register a new event source, but the computer name in <see cref="P:System.Diagnostics.EventLog.MachineName" />  is not valid.- or -The source is already registered for a different event log.- or -The message string is longer than 31,839 bytes (32,766 bytes on Windows operating systems before Windows Vista).- or -The source name results in a registry key path longer than 254 characters.</exception>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="type" /> is not a valid <see cref="T:System.Diagnostics.EventLogEntryType" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The registry key for the event log could not be opened.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The operating system reported an error when writing the event entry to the event log. A Windows error code is not available.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void WriteEntry(string message, EventLogEntryType type)
	{
		WriteEntry(message, type, 0);
	}

	/// <summary>Writes an entry with the given message text and application-defined event identifier to the event log.</summary>
	/// <param name="message">The string to write to the event log. </param>
	/// <param name="type">One of the <see cref="T:System.Diagnostics.EventLogEntryType" /> values. </param>
	/// <param name="eventID">The application-specific identifier for the event. </param>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Diagnostics.EventLog.Source" /> property of the <see cref="T:System.Diagnostics.EventLog" /> has not been set.-or- The method attempted to register a new event source, but the computer name in <see cref="P:System.Diagnostics.EventLog.MachineName" /> is not valid.- or -The source is already registered for a different event log.- or -<paramref name="eventID" /> is less than zero or greater than <see cref="F:System.UInt16.MaxValue" />.- or -The message string is longer than 31,839 bytes (32,766 bytes on Windows operating systems before Windows Vista).- or -The source name results in a registry key path longer than 254 characters.</exception>
	/// <exception cref="T:System.InvalidOperationException">The registry key for the event log could not be opened.</exception>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="type" /> is not a valid <see cref="T:System.Diagnostics.EventLogEntryType" />.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The operating system reported an error when writing the event entry to the event log. A Windows error code is not available.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void WriteEntry(string message, EventLogEntryType type, int eventID)
	{
		WriteEntry(message, type, eventID, 0);
	}

	/// <summary>Writes an entry with the given message text, application-defined event identifier, and application-defined category to the event log.</summary>
	/// <param name="message">The string to write to the event log. </param>
	/// <param name="type">One of the <see cref="T:System.Diagnostics.EventLogEntryType" /> values. </param>
	/// <param name="eventID">The application-specific identifier for the event. </param>
	/// <param name="category">The application-specific subcategory associated with the message. </param>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Diagnostics.EventLog.Source" /> property of the <see cref="T:System.Diagnostics.EventLog" /> has not been set.-or- The method attempted to register a new event source, but the computer name in <see cref="P:System.Diagnostics.EventLog.MachineName" /> is not valid.- or -The source is already registered for a different event log.- or -<paramref name="eventID" /> is less than zero or greater than <see cref="F:System.UInt16.MaxValue" />.- or -The message string is longer than 31,839 bytes (32,766 bytes on Windows operating systems before Windows Vista).- or -The source name results in a registry key path longer than 254 characters.</exception>
	/// <exception cref="T:System.InvalidOperationException">The registry key for the event log could not be opened.</exception>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="type" /> is not a valid <see cref="T:System.Diagnostics.EventLogEntryType" />.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The operating system reported an error when writing the event entry to the event log. A Windows error code is not available.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void WriteEntry(string message, EventLogEntryType type, int eventID, short category)
	{
		WriteEntry(message, type, eventID, category, null);
	}

	/// <summary>Writes an entry with the given message text, application-defined event identifier, and application-defined category to the event log, and appends binary data to the message.</summary>
	/// <param name="message">The string to write to the event log. </param>
	/// <param name="type">One of the <see cref="T:System.Diagnostics.EventLogEntryType" /> values. </param>
	/// <param name="eventID">The application-specific identifier for the event. </param>
	/// <param name="category">The application-specific subcategory associated with the message. </param>
	/// <param name="rawData">An array of bytes that holds the binary data associated with the entry. </param>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Diagnostics.EventLog.Source" /> property of the <see cref="T:System.Diagnostics.EventLog" /> has not been set.-or- The method attempted to register a new event source, but the computer name in <see cref="P:System.Diagnostics.EventLog.MachineName" /> is not valid.- or -The source is already registered for a different event log.- or -<paramref name="eventID" /> is less than zero or greater than <see cref="F:System.UInt16.MaxValue" />.- or -The message string is longer than 31,839 bytes (32,766 bytes on Windows operating systems before Windows Vista).- or -The source name results in a registry key path longer than 254 characters.</exception>
	/// <exception cref="T:System.InvalidOperationException">The registry key for the event log could not be opened.</exception>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="type" /> is not a valid <see cref="T:System.Diagnostics.EventLogEntryType" />.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The operating system reported an error when writing the event entry to the event log. A Windows error code is not available.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void WriteEntry(string message, EventLogEntryType type, int eventID, short category, byte[] rawData)
	{
		WriteEntry(new string[1] { message }, type, eventID, category, rawData);
	}

	/// <summary>Writes an information type entry with the given message text to the event log, using the specified registered event source.</summary>
	/// <param name="source">The source by which the application is registered on the specified computer. </param>
	/// <param name="message">The string to write to the event log. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="source" /> value is an empty string ("").- or -The <paramref name="source" /> value is null.- or -The message string is longer than 31,839 bytes (32,766 bytes on Windows operating systems before Windows Vista).- or -The source name results in a registry key path longer than 254 characters.</exception>
	/// <exception cref="T:System.InvalidOperationException">The registry key for the event log could not be opened.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The operating system reported an error when writing the event entry to the event log. A Windows error code is not available.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void WriteEntry(string source, string message)
	{
		WriteEntry(source, message, EventLogEntryType.Information);
	}

	/// <summary>Writes an error, warning, information, success audit, or failure audit entry with the given message text to the event log, using the specified registered event source.</summary>
	/// <param name="source">The source by which the application is registered on the specified computer. </param>
	/// <param name="message">The string to write to the event log. </param>
	/// <param name="type">One of the <see cref="T:System.Diagnostics.EventLogEntryType" /> values. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="source" /> value is an empty string ("").- or -The <paramref name="source" /> value is null.- or -The message string is longer than 31,839 bytes (32,766 bytes on Windows operating systems before Windows Vista).- or -The source name results in a registry key path longer than 254 characters.</exception>
	/// <exception cref="T:System.InvalidOperationException">The registry key for the event log could not be opened.</exception>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="type" /> is not a valid <see cref="T:System.Diagnostics.EventLogEntryType" />.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The operating system reported an error when writing the event entry to the event log. A Windows error code is not available.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void WriteEntry(string source, string message, EventLogEntryType type)
	{
		WriteEntry(source, message, type, 0);
	}

	/// <summary>Writes an entry with the given message text and application-defined event identifier to the event log, using the specified registered event source.</summary>
	/// <param name="source">The source by which the application is registered on the specified computer. </param>
	/// <param name="message">The string to write to the event log. </param>
	/// <param name="type">One of the <see cref="T:System.Diagnostics.EventLogEntryType" /> values. </param>
	/// <param name="eventID">The application-specific identifier for the event. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="source" /> value is an empty string ("").- or -The <paramref name="source" /> value is null.- or -<paramref name="eventID" /> is less than zero or greater than <see cref="F:System.UInt16.MaxValue" />.- or -The message string is longer than 31,839 bytes (32,766 bytes on Windows operating systems before Windows Vista).- or -The source name results in a registry key path longer than 254 characters.</exception>
	/// <exception cref="T:System.InvalidOperationException">The registry key for the event log could not be opened.</exception>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="type" /> is not a valid <see cref="T:System.Diagnostics.EventLogEntryType" />.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The operating system reported an error when writing the event entry to the event log. A Windows error code is not available.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void WriteEntry(string source, string message, EventLogEntryType type, int eventID)
	{
		WriteEntry(source, message, type, eventID, 0);
	}

	/// <summary>Writes an entry with the given message text, application-defined event identifier, and application-defined category to the event log, using the specified registered event source. The <paramref name="category" /> can be used by the Event Viewer to filter events in the log.</summary>
	/// <param name="source">The source by which the application is registered on the specified computer. </param>
	/// <param name="message">The string to write to the event log. </param>
	/// <param name="type">One of the <see cref="T:System.Diagnostics.EventLogEntryType" /> values. </param>
	/// <param name="eventID">The application-specific identifier for the event. </param>
	/// <param name="category">The application-specific subcategory associated with the message. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="source" /> value is an empty string ("").- or -The <paramref name="source" /> value is null.- or -<paramref name="eventID" /> is less than zero or greater than <see cref="F:System.UInt16.MaxValue" />.- or -The message string is longer than 31,839 bytes (32,766 bytes on Windows operating systems before Windows Vista).- or -The source name results in a registry key path longer than 254 characters.</exception>
	/// <exception cref="T:System.InvalidOperationException">The registry key for the event log could not be opened.</exception>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="type" /> is not a valid <see cref="T:System.Diagnostics.EventLogEntryType" />.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The operating system reported an error when writing the event entry to the event log. A Windows error code is not available.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void WriteEntry(string source, string message, EventLogEntryType type, int eventID, short category)
	{
		WriteEntry(source, message, type, eventID, category, null);
	}

	/// <summary>Writes an entry with the given message text, application-defined event identifier, and application-defined category to the event log (using the specified registered event source) and appends binary data to the message.</summary>
	/// <param name="source">The source by which the application is registered on the specified computer. </param>
	/// <param name="message">The string to write to the event log. </param>
	/// <param name="type">One of the <see cref="T:System.Diagnostics.EventLogEntryType" /> values. </param>
	/// <param name="eventID">The application-specific identifier for the event. </param>
	/// <param name="category">The application-specific subcategory associated with the message. </param>
	/// <param name="rawData">An array of bytes that holds the binary data associated with the entry. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="source" /> value is an empty string ("").- or -The <paramref name="source" /> value is null.- or -<paramref name="eventID" /> is less than zero or greater than <see cref="F:System.UInt16.MaxValue" />.- or -The message string is longer than 31,839 bytes (32,766 bytes on Windows operating systems before Windows Vista).- or -The source name results in a registry key path longer than 254 characters. </exception>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="type" /> is not a valid <see cref="T:System.Diagnostics.EventLogEntryType" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The registry key for the event log could not be opened.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The operating system reported an error when writing the event entry to the event log. A Windows error code is not available.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void WriteEntry(string source, string message, EventLogEntryType type, int eventID, short category, byte[] rawData)
	{
		using EventLog eventLog = new EventLog();
		eventLog.Source = source;
		eventLog.WriteEntry(message, type, eventID, category, rawData);
	}

	/// <summary>Writes a localized entry to the event log.</summary>
	/// <param name="instance">An <see cref="T:System.Diagnostics.EventInstance" /> instance that represents a localized event log entry. </param>
	/// <param name="values">An array of strings to merge into the message text of the event log entry. </param>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Diagnostics.EventLog.Source" /> property of the <see cref="T:System.Diagnostics.EventLog" /> has not been set.-or- The method attempted to register a new event source, but the computer name in <see cref="P:System.Diagnostics.EventLog.MachineName" /> is not valid.- or -The source is already registered for a different event log.- or -<paramref name="instance.InstanceId" /> is less than zero or greater than <see cref="F:System.UInt16.MaxValue" />.- or -<paramref name="values" /> has more than 256 elements.- or -One of the <paramref name="values" /> elements is longer than 32766 bytes.- or -The source name results in a registry key path longer than 254 characters.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="instance" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The registry key for the event log could not be opened.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The operating system reported an error when writing the event entry to the event log. A Windows error code is not available.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[ComVisible(false)]
	public void WriteEvent(EventInstance instance, params object[] values)
	{
		WriteEvent(instance, null, values);
	}

	/// <summary>Writes an event log entry with the given event data, message replacement strings, and associated binary data.</summary>
	/// <param name="instance">An <see cref="T:System.Diagnostics.EventInstance" /> instance that represents a localized event log entry. </param>
	/// <param name="data">An array of bytes that holds the binary data associated with the entry. </param>
	/// <param name="values">An array of strings to merge into the message text of the event log entry. </param>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Diagnostics.EventLog.Source" /> property of the <see cref="T:System.Diagnostics.EventLog" /> has not been set.-or- The method attempted to register a new event source, but the computer name in <see cref="P:System.Diagnostics.EventLog.MachineName" /> is not valid.- or -The source is already registered for a different event log.- or -<paramref name="instance.InstanceId" /> is less than zero or greater than <see cref="F:System.UInt16.MaxValue" />.- or -<paramref name="values" /> has more than 256 elements.- or -One of the <paramref name="values" /> elements is longer than 32766 bytes.- or -The source name results in a registry key path longer than 254 characters.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="instance" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The registry key for the event log could not be opened.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The operating system reported an error when writing the event entry to the event log. A Windows error code is not available.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[ComVisible(false)]
	public void WriteEvent(EventInstance instance, byte[] data, params object[] values)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		string[] array = null;
		if (values != null)
		{
			array = new string[values.Length];
			for (int i = 0; i < values.Length; i++)
			{
				if (values[i] == null)
				{
					array[i] = string.Empty;
				}
				else
				{
					array[i] = values[i].ToString();
				}
			}
		}
		else
		{
			array = new string[0];
		}
		WriteEntry(array, instance.EntryType, instance.InstanceId, (short)instance.CategoryId, data);
	}

	/// <summary>Writes an event log entry with the given event data and message replacement strings, using the specified registered event source.</summary>
	/// <param name="source">The name of the event source registered for the application on the specified computer. </param>
	/// <param name="instance">An <see cref="T:System.Diagnostics.EventInstance" /> instance that represents a localized event log entry. </param>
	/// <param name="values">An array of strings to merge into the message text of the event log entry. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="source" /> value is an empty string ("").- or -The <paramref name="source" /> value is null.- or -<paramref name="instance.InstanceId" /> is less than zero or greater than <see cref="F:System.UInt16.MaxValue" />.- or -<paramref name="values" /> has more than 256 elements.- or -One of the <paramref name="values" /> elements is longer than 32766 bytes.- or -The source name results in a registry key path longer than 254 characters.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="instance" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The registry key for the event log could not be opened.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The operating system reported an error when writing the event entry to the event log. A Windows error code is not available.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void WriteEvent(string source, EventInstance instance, params object[] values)
	{
		WriteEvent(source, instance, null, values);
	}

	/// <summary>Writes an event log entry with the given event data, message replacement strings, and associated binary data, and using the specified registered event source.</summary>
	/// <param name="source">The name of the event source registered for the application on the specified computer. </param>
	/// <param name="instance">An <see cref="T:System.Diagnostics.EventInstance" /> instance that represents a localized event log entry. </param>
	/// <param name="data">An array of bytes that holds the binary data associated with the entry. </param>
	/// <param name="values">An array of strings to merge into the message text of the event log entry. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="source" /> value is an empty string ("").- or -The <paramref name="source" /> value is null.- or -<paramref name="instance.InstanceId" /> is less than zero or greater than <see cref="F:System.UInt16.MaxValue" />.- or -<paramref name="values" /> has more than 256 elements.- or -One of the <paramref name="values" /> elements is longer than 32766 bytes.- or -The source name results in a registry key path longer than 254 characters.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="instance" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The registry key for the event log could not be opened.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The operating system reported an error when writing the event entry to the event log. A Windows error code is not available.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void WriteEvent(string source, EventInstance instance, byte[] data, params object[] values)
	{
		using EventLog eventLog = new EventLog();
		eventLog.Source = source;
		eventLog.WriteEvent(instance, data, values);
	}

	internal void OnEntryWritten(EventLogEntry newEntry)
	{
		if (doRaiseEvents && this.EntryWritten != null)
		{
			this.EntryWritten(this, new EntryWrittenEventArgs(newEntry));
		}
	}

	internal string GetLogName()
	{
		if (logName != null && logName.Length > 0)
		{
			return logName;
		}
		logName = LogNameFromSourceName(source, machineName);
		return logName;
	}

	private static EventLogImpl CreateEventLogImpl(string logName, string machineName, string source)
	{
		return CreateEventLogImpl(new EventLog(logName, machineName, source));
	}

	private static EventLogImpl CreateEventLogImpl(EventLog eventLog)
	{
		return EventLogImplType switch
		{
			"local" => new LocalFileEventLog(eventLog), 
			"win32" => new Win32EventLog(eventLog), 
			"null" => new NullEventLog(eventLog), 
			_ => throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Eventlog implementation '{0}' is not supported.", EventLogImplType)), 
		};
	}

	private void WriteEntry(string[] replacementStrings, EventLogEntryType type, long instanceID, short category, byte[] rawData)
	{
		if (Source.Length == 0)
		{
			throw new ArgumentException("Source property was not setbefore writing to the event log.");
		}
		if (!Enum.IsDefined(typeof(EventLogEntryType), type))
		{
			throw new InvalidEnumArgumentException("type", (int)type, typeof(EventLogEntryType));
		}
		ValidateEventID(instanceID);
		if (!SourceExists(Source, MachineName))
		{
			if (Log == null || Log.Length == 0)
			{
				Log = "Application";
			}
			CreateEventSource(Source, Log, MachineName);
		}
		else if (logName != null && logName.Length != 0)
		{
			string text = LogNameFromSourceName(Source, MachineName);
			if (string.Compare(logName, text, ignoreCase: true, CultureInfo.InvariantCulture) != 0)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The source '{0}' is not registered in log '{1}' (it is registered in log '{2}'). The Source and Log properties must be matched, or you may set Log to the empty string, and it will automatically be matched to the Source property.", Source, logName, text));
			}
		}
		if (rawData == null)
		{
			rawData = new byte[0];
		}
		Impl.WriteEntry(replacementStrings, type, (uint)instanceID, category, rawData);
	}

	private void ValidateEventID(long instanceID)
	{
		int eventID = GetEventID(instanceID);
		if (eventID < 0 || eventID > 65535)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Invalid eventID value '{0}'. It must be in the range between '{1}' and '{2}'.", instanceID, (ushort)0, ushort.MaxValue));
		}
	}

	internal static int GetEventID(long instanceID)
	{
		int num = (int)(((instanceID < 0) ? (-instanceID) : instanceID) & 0x3FFFFFFF);
		if (instanceID >= 0)
		{
			return num;
		}
		return -num;
	}
}
