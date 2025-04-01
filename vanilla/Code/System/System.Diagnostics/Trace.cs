using System.Security.Permissions;

namespace System.Diagnostics;

/// <summary>Provides a set of methods and properties that help you trace the execution of your code. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
public sealed class Trace
{
	private static volatile CorrelationManager correlationManager;

	/// <summary>Gets the collection of listeners that is monitoring the trace output.</summary>
	/// <returns>A <see cref="T:System.Diagnostics.TraceListenerCollection" /> that represents a collection of type <see cref="T:System.Diagnostics.TraceListener" /> monitoring the trace output.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static TraceListenerCollection Listeners
	{
		[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
		get
		{
			return TraceInternal.Listeners;
		}
	}

	/// <summary>Gets or sets whether <see cref="M:System.Diagnostics.Trace.Flush" /> should be called on the <see cref="P:System.Diagnostics.Trace.Listeners" /> after every write.</summary>
	/// <returns>true if <see cref="M:System.Diagnostics.Trace.Flush" /> is called on the <see cref="P:System.Diagnostics.Trace.Listeners" /> after every write; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static bool AutoFlush
	{
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		get
		{
			return TraceInternal.AutoFlush;
		}
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		set
		{
			TraceInternal.AutoFlush = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether the global lock should be used.  </summary>
	/// <returns>true if the global lock is to be used; otherwise, false. The default is true.</returns>
	public static bool UseGlobalLock
	{
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		get
		{
			return TraceInternal.UseGlobalLock;
		}
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		set
		{
			TraceInternal.UseGlobalLock = value;
		}
	}

	/// <summary>Gets the correlation manager for the thread for this trace.</summary>
	/// <returns>The <see cref="T:System.Diagnostics.CorrelationManager" /> object associated with the thread for this trace.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static CorrelationManager CorrelationManager
	{
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		get
		{
			if (correlationManager == null)
			{
				correlationManager = new CorrelationManager();
			}
			return correlationManager;
		}
	}

	/// <summary>Gets or sets the indent level.</summary>
	/// <returns>The indent level. The default is zero.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public static int IndentLevel
	{
		get
		{
			return TraceInternal.IndentLevel;
		}
		set
		{
			TraceInternal.IndentLevel = value;
		}
	}

	/// <summary>Gets or sets the number of spaces in an indent.</summary>
	/// <returns>The number of spaces in an indent. The default is four.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public static int IndentSize
	{
		get
		{
			return TraceInternal.IndentSize;
		}
		set
		{
			TraceInternal.IndentSize = value;
		}
	}

	private Trace()
	{
	}

	/// <summary>Flushes the output buffer, and causes buffered data to be written to the <see cref="P:System.Diagnostics.Trace.Listeners" />.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void Flush()
	{
		TraceInternal.Flush();
	}

	/// <summary>Flushes the output buffer, and then closes the <see cref="P:System.Diagnostics.Trace.Listeners" />.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void Close()
	{
		TraceInternal.Close();
	}

	/// <summary>Checks for a condition; if the condition is false, displays a message box that shows the call stack.</summary>
	/// <param name="condition">The conditional expression to evaluate. If the condition is true, a failure message is not sent and the message box is not displayed.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void Assert(bool condition)
	{
		TraceInternal.Assert(condition);
	}

	/// <summary>Checks for a condition; if the condition is false, outputs a specified message and displays a message box that shows the call stack.</summary>
	/// <param name="condition">The conditional expression to evaluate. If the condition is true, the specified message is not sent and the message box is not displayed.  </param>
	/// <param name="message">The message to send to the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void Assert(bool condition, string message)
	{
		TraceInternal.Assert(condition, message);
	}

	/// <summary>Checks for a condition; if the condition is false, outputs two specified messages and displays a message box that shows the call stack.</summary>
	/// <param name="condition">The conditional expression to evaluate. If the condition is true, the specified messages are not sent and the message box is not displayed.  </param>
	/// <param name="message">The message to send to the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection. </param>
	/// <param name="detailMessage">The detailed message to send to the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void Assert(bool condition, string message, string detailMessage)
	{
		TraceInternal.Assert(condition, message, detailMessage);
	}

	/// <summary>Emits the specified error message.</summary>
	/// <param name="message">A message to emit. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void Fail(string message)
	{
		TraceInternal.Fail(message);
	}

	/// <summary>Emits an error message, and a detailed error message.</summary>
	/// <param name="message">A message to emit. </param>
	/// <param name="detailMessage">A detailed message to emit. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void Fail(string message, string detailMessage)
	{
		TraceInternal.Fail(message, detailMessage);
	}

	/// <summary>Refreshes the trace configuration data.</summary>
	/// <filterpriority>1</filterpriority>
	public static void Refresh()
	{
		DiagnosticsConfiguration.Refresh();
		Switch.RefreshAll();
		TraceSource.RefreshAll();
		TraceInternal.Refresh();
	}

	/// <summary>Writes an informational message to the trace listeners in the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection using the specified message.</summary>
	/// <param name="message">The informative message to write.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void TraceInformation(string message)
	{
		TraceInternal.TraceEvent(TraceEventType.Information, 0, message, null);
	}

	/// <summary>Writes an informational message to the trace listeners in the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection using the specified array of objects and formatting information.</summary>
	/// <param name="format">A format string that contains zero or more format items, which correspond to objects in the <paramref name="args" /> array.</param>
	/// <param name="args">An object array containing zero or more objects to format.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void TraceInformation(string format, params object[] args)
	{
		TraceInternal.TraceEvent(TraceEventType.Information, 0, format, args);
	}

	/// <summary>Writes a warning message to the trace listeners in the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection using the specified message.</summary>
	/// <param name="message">The informative message to write.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void TraceWarning(string message)
	{
		TraceInternal.TraceEvent(TraceEventType.Warning, 0, message, null);
	}

	/// <summary>Writes a warning message to the trace listeners in the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection using the specified array of objects and formatting information.</summary>
	/// <param name="format">A format string that contains zero or more format items, which correspond to objects in the <paramref name="args" /> array.</param>
	/// <param name="args">An object array containing zero or more objects to format.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void TraceWarning(string format, params object[] args)
	{
		TraceInternal.TraceEvent(TraceEventType.Warning, 0, format, args);
	}

	/// <summary>Writes an error message to the trace listeners in the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection using the specified message.</summary>
	/// <param name="message">The informative message to write.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void TraceError(string message)
	{
		TraceInternal.TraceEvent(TraceEventType.Error, 0, message, null);
	}

	/// <summary>Writes an error message to the trace listeners in the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection using the specified array of objects and formatting information.</summary>
	/// <param name="format">A format string that contains zero or more format items, which correspond to objects in the <paramref name="args" /> array.</param>
	/// <param name="args">An object array containing zero or more objects to format.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void TraceError(string format, params object[] args)
	{
		TraceInternal.TraceEvent(TraceEventType.Error, 0, format, args);
	}

	/// <summary>Writes a message to the trace listeners in the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection.</summary>
	/// <param name="message">A message to write. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void Write(string message)
	{
		TraceInternal.Write(message);
	}

	/// <summary>Writes the value of the object's <see cref="M:System.Object.ToString" /> method to the trace listeners in the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection.</summary>
	/// <param name="value">An <see cref="T:System.Object" /> whose name is sent to the <see cref="P:System.Diagnostics.Trace.Listeners" />. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void Write(object value)
	{
		TraceInternal.Write(value);
	}

	/// <summary>Writes a category name and a message to the trace listeners in the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection.</summary>
	/// <param name="message">A message to write. </param>
	/// <param name="category">A category name used to organize the output. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void Write(string message, string category)
	{
		TraceInternal.Write(message, category);
	}

	/// <summary>Writes a category name and the value of the object's <see cref="M:System.Object.ToString" /> method to the trace listeners in the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection.</summary>
	/// <param name="value">An <see cref="T:System.Object" /> name is sent to the <see cref="P:System.Diagnostics.Trace.Listeners" />. </param>
	/// <param name="category">A category name used to organize the output. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void Write(object value, string category)
	{
		TraceInternal.Write(value, category);
	}

	/// <summary>Writes a message to the trace listeners in the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection.</summary>
	/// <param name="message">A message to write. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void WriteLine(string message)
	{
		TraceInternal.WriteLine(message);
	}

	/// <summary>Writes the value of the object's <see cref="M:System.Object.ToString" /> method to the trace listeners in the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection.</summary>
	/// <param name="value">An <see cref="T:System.Object" /> whose name is sent to the <see cref="P:System.Diagnostics.Trace.Listeners" />. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void WriteLine(object value)
	{
		TraceInternal.WriteLine(value);
	}

	/// <summary>Writes a category name and message to the trace listeners in the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection.</summary>
	/// <param name="message">A message to write. </param>
	/// <param name="category">A category name used to organize the output. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void WriteLine(string message, string category)
	{
		TraceInternal.WriteLine(message, category);
	}

	/// <summary>Writes a category name and the value of the object's <see cref="M:System.Object.ToString" /> method to the trace listeners in the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection.</summary>
	/// <param name="value">An <see cref="T:System.Object" /> whose name is sent to the <see cref="P:System.Diagnostics.Trace.Listeners" />. </param>
	/// <param name="category">A category name used to organize the output. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void WriteLine(object value, string category)
	{
		TraceInternal.WriteLine(value, category);
	}

	/// <summary>Writes a message to the trace listeners in the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection if a condition is true.</summary>
	/// <param name="condition">true to cause a message to be written; otherwise, false. </param>
	/// <param name="message">A message to write. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void WriteIf(bool condition, string message)
	{
		TraceInternal.WriteIf(condition, message);
	}

	/// <summary>Writes the value of the object's <see cref="M:System.Object.ToString" /> method to the trace listeners in the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection if a condition is true.</summary>
	/// <param name="condition">true to cause a message to be written; otherwise, false. </param>
	/// <param name="value">An <see cref="T:System.Object" /> whose name is sent to the <see cref="P:System.Diagnostics.Trace.Listeners" />. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void WriteIf(bool condition, object value)
	{
		TraceInternal.WriteIf(condition, value);
	}

	/// <summary>Writes a category name and message to the trace listeners in the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection if a condition is true.</summary>
	/// <param name="condition">true to cause a message to be written; otherwise, false. </param>
	/// <param name="message">A message to write. </param>
	/// <param name="category">A category name used to organize the output. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void WriteIf(bool condition, string message, string category)
	{
		TraceInternal.WriteIf(condition, message, category);
	}

	/// <summary>Writes a category name and the value of the object's <see cref="M:System.Object.ToString" /> method to the trace listeners in the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection if a condition is true.</summary>
	/// <param name="condition">true to cause a message to be written; otherwise, false. </param>
	/// <param name="value">An <see cref="T:System.Object" /> whose name is sent to the <see cref="P:System.Diagnostics.Trace.Listeners" />. </param>
	/// <param name="category">A category name used to organize the output. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void WriteIf(bool condition, object value, string category)
	{
		TraceInternal.WriteIf(condition, value, category);
	}

	/// <summary>Writes a message to the trace listeners in the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection if a condition is true.</summary>
	/// <param name="condition">true to cause a message to be written; otherwise, false. </param>
	/// <param name="message">A message to write. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void WriteLineIf(bool condition, string message)
	{
		TraceInternal.WriteLineIf(condition, message);
	}

	/// <summary>Writes the value of the object's <see cref="M:System.Object.ToString" /> method to the trace listeners in the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection if a condition is true.</summary>
	/// <param name="condition">true to cause a message to be written; otherwise, false. </param>
	/// <param name="value">An <see cref="T:System.Object" /> whose name is sent to the <see cref="P:System.Diagnostics.Trace.Listeners" />. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void WriteLineIf(bool condition, object value)
	{
		TraceInternal.WriteLineIf(condition, value);
	}

	/// <summary>Writes a category name and message to the trace listeners in the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection if a condition is true.</summary>
	/// <param name="condition">true to cause a message to be written; otherwise, false. </param>
	/// <param name="message">A message to write. </param>
	/// <param name="category">A category name used to organize the output. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void WriteLineIf(bool condition, string message, string category)
	{
		TraceInternal.WriteLineIf(condition, message, category);
	}

	/// <summary>Writes a category name and the value of the object's <see cref="M:System.Object.ToString" /> method to the trace listeners in the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection if a condition is true.</summary>
	/// <param name="condition">true to cause a message to be written; otherwise, false. </param>
	/// <param name="value">An <see cref="T:System.Object" /> whose name is sent to the <see cref="P:System.Diagnostics.Trace.Listeners" />. </param>
	/// <param name="category">A category name used to organize the output. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void WriteLineIf(bool condition, object value, string category)
	{
		TraceInternal.WriteLineIf(condition, value, category);
	}

	/// <summary>Increases the current <see cref="P:System.Diagnostics.Trace.IndentLevel" /> by one.</summary>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void Indent()
	{
		TraceInternal.Indent();
	}

	/// <summary>Decreases the current <see cref="P:System.Diagnostics.Trace.IndentLevel" /> by one.</summary>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public static void Unindent()
	{
		TraceInternal.Unindent();
	}
}
