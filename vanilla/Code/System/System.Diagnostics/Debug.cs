using System.Globalization;
using System.Security.Permissions;

namespace System.Diagnostics;

/// <summary>Provides a set of methods and properties that help debug your code. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
public static class Debug
{
	/// <summary>Gets the collection of listeners that is monitoring the debug output.</summary>
	/// <returns>A <see cref="T:System.Diagnostics.TraceListenerCollection" /> representing a collection of type <see cref="T:System.Diagnostics.TraceListener" /> that monitors the debug output.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static TraceListenerCollection Listeners
	{
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
		get
		{
			return TraceInternal.Listeners;
		}
	}

	/// <summary>Gets or sets a value indicating whether <see cref="M:System.Diagnostics.Debug.Flush" /> should be called on the <see cref="P:System.Diagnostics.Debug.Listeners" /> after every write.</summary>
	/// <returns>true if <see cref="M:System.Diagnostics.Debug.Flush" /> is called on the <see cref="P:System.Diagnostics.Debug.Listeners" /> after every write; otherwise, false.</returns>
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

	/// <summary>Gets or sets the indent level.</summary>
	/// <returns>The indent level. The default is 0.</returns>
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

	/// <summary>Flushes the output buffer and causes buffered data to write to the <see cref="P:System.Diagnostics.Debug.Listeners" /> collection.</summary>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void Flush()
	{
		TraceInternal.Flush();
	}

	/// <summary>Flushes the output buffer and then calls the Close method on each of the <see cref="P:System.Diagnostics.Debug.Listeners" />.</summary>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	public static void Close()
	{
		TraceInternal.Close();
	}

	/// <summary>Checks for a condition; if the condition is false, displays a message box that shows the call stack.</summary>
	/// <param name="condition">The conditional expression to evaluate. If the condition is true, a failure message is not sent and the message box is not displayed.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void Assert(bool condition)
	{
		TraceInternal.Assert(condition);
	}

	/// <summary>Checks for a condition; if the condition is false, outputs a specified message and displays a message box that shows the call stack.</summary>
	/// <param name="condition">The conditional expression to evaluate. If the condition is true, the specified message is not sent and the message box is not displayed.  </param>
	/// <param name="message">The message to send to the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void Assert(bool condition, string message)
	{
		TraceInternal.Assert(condition, message);
	}

	/// <summary>Checks for a condition; if the condition is false, outputs two specified messages and displays a message box that shows the call stack.</summary>
	/// <param name="condition">The conditional expression to evaluate. If the condition is true, the specified messages are not sent and the message box is not displayed.  </param>
	/// <param name="message">The message to send to the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection. </param>
	/// <param name="detailMessage">The detailed message to send to the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void Assert(bool condition, string message, string detailMessage)
	{
		TraceInternal.Assert(condition, message, detailMessage);
	}

	/// <summary>Checks for a condition; if the condition is false, outputs two messages (simple and formatted) and displays a message box that shows the call stack.</summary>
	/// <param name="condition">The conditional expression to evaluate. If the condition is true, the specified messages are not sent and the message box is not displayed.  </param>
	/// <param name="message">The message to send to the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection. </param>
	/// <param name="detailMessageFormat">The composite format string (see Remarks) to send to the <see cref="P:System.Diagnostics.Trace.Listeners" /> collection. This message contains text intermixed with zero or more format items, which correspond to objects in the <paramref name="args" /> array.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	[Conditional("DEBUG")]
	public static void Assert(bool condition, string message, string detailMessageFormat, params object[] args)
	{
		TraceInternal.Assert(condition, message, string.Format(CultureInfo.InvariantCulture, detailMessageFormat, args));
	}

	/// <summary>Emits the specified error message.</summary>
	/// <param name="message">A message to emit. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void Fail(string message)
	{
		TraceInternal.Fail(message);
	}

	/// <summary>Emits an error message and a detailed error message.</summary>
	/// <param name="message">A message to emit. </param>
	/// <param name="detailMessage">A detailed message to emit. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void Fail(string message, string detailMessage)
	{
		TraceInternal.Fail(message, detailMessage);
	}

	/// <summary>Writes a message followed by a line terminator to the trace listeners in the <see cref="P:System.Diagnostics.Debug.Listeners" /> collection.</summary>
	/// <param name="message">The message to write.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void Print(string message)
	{
		TraceInternal.WriteLine(message);
	}

	/// <summary>Writes a formatted string followed by a line terminator to the trace listeners in the <see cref="P:System.Diagnostics.Debug.Listeners" /> collection.</summary>
	/// <param name="format">A composite format string (see Remarks) that contains text intermixed with zero or more format items, which correspond to objects in the <paramref name="args" /> array.</param>
	/// <param name="args">An object array containing zero or more objects to format. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is invalid.-or- The number that indicates an argument to format is less than zero, or greater than or equal to the number of specified objects to format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void Print(string format, params object[] args)
	{
		TraceInternal.WriteLine(string.Format(CultureInfo.InvariantCulture, format, args));
	}

	/// <summary>Writes a message to the trace listeners in the <see cref="P:System.Diagnostics.Debug.Listeners" /> collection.</summary>
	/// <param name="message">A message to write. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void Write(string message)
	{
		TraceInternal.Write(message);
	}

	/// <summary>Writes the value of the object's <see cref="M:System.Object.ToString" /> method to the trace listeners in the <see cref="P:System.Diagnostics.Debug.Listeners" /> collection.</summary>
	/// <param name="value">An object whose name is sent to the <see cref="P:System.Diagnostics.Debug.Listeners" />. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void Write(object value)
	{
		TraceInternal.Write(value);
	}

	/// <summary>Writes a category name and message to the trace listeners in the <see cref="P:System.Diagnostics.Debug.Listeners" /> collection.</summary>
	/// <param name="message">A message to write. </param>
	/// <param name="category">A category name used to organize the output. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void Write(string message, string category)
	{
		TraceInternal.Write(message, category);
	}

	/// <summary>Writes a category name and the value of the object's <see cref="M:System.Object.ToString" /> method to the trace listeners in the <see cref="P:System.Diagnostics.Debug.Listeners" /> collection.</summary>
	/// <param name="value">An object whose name is sent to the <see cref="P:System.Diagnostics.Debug.Listeners" />. </param>
	/// <param name="category">A category name used to organize the output. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void Write(object value, string category)
	{
		TraceInternal.Write(value, category);
	}

	/// <summary>Writes a message followed by a line terminator to the trace listeners in the <see cref="P:System.Diagnostics.Debug.Listeners" /> collection.</summary>
	/// <param name="message">A message to write. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void WriteLine(string message)
	{
		TraceInternal.WriteLine(message);
	}

	/// <summary>Writes the value of the object's <see cref="M:System.Object.ToString" /> method to the trace listeners in the <see cref="P:System.Diagnostics.Debug.Listeners" /> collection.</summary>
	/// <param name="value">An object whose name is sent to the <see cref="P:System.Diagnostics.Debug.Listeners" />. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void WriteLine(object value)
	{
		TraceInternal.WriteLine(value);
	}

	/// <summary>Writes a category name and message to the trace listeners in the <see cref="P:System.Diagnostics.Debug.Listeners" /> collection.</summary>
	/// <param name="message">A message to write. </param>
	/// <param name="category">A category name used to organize the output. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void WriteLine(string message, string category)
	{
		TraceInternal.WriteLine(message, category);
	}

	/// <summary>Writes a category name and the value of the object's <see cref="M:System.Object.ToString" /> method to the trace listeners in the <see cref="P:System.Diagnostics.Debug.Listeners" /> collection.</summary>
	/// <param name="value">An object whose name is sent to the <see cref="P:System.Diagnostics.Debug.Listeners" />. </param>
	/// <param name="category">A category name used to organize the output. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void WriteLine(object value, string category)
	{
		TraceInternal.WriteLine(value, category);
	}

	/// <summary>Writes a formatted message followed by a line terminator to the trace listeners in the <see cref="P:System.Diagnostics.Debug.Listeners" /> collection.</summary>
	/// <param name="format">A composite format string (see Remarks) that contains text intermixed with zero or more format items, which correspond to objects in the <paramref name="args" /> array.</param>
	/// <param name="args">An object array that contains zero or more objects to format. </param>
	[Conditional("DEBUG")]
	public static void WriteLine(string format, params object[] args)
	{
		TraceInternal.WriteLine(string.Format(CultureInfo.InvariantCulture, format, args));
	}

	/// <summary>Writes a message to the trace listeners in the <see cref="P:System.Diagnostics.Debug.Listeners" /> collection if a condition is true.</summary>
	/// <param name="condition">The conditional expression to evaluate. If the condition is true, the message is written to the trace listeners in the collection.</param>
	/// <param name="message">A message to write. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void WriteIf(bool condition, string message)
	{
		TraceInternal.WriteIf(condition, message);
	}

	/// <summary>Writes the value of the object's <see cref="M:System.Object.ToString" /> method to the trace listeners in the <see cref="P:System.Diagnostics.Debug.Listeners" /> collection if a condition is true.</summary>
	/// <param name="condition">The conditional expression to evaluate. If the condition is true, the value is written to the trace listeners in the collection.</param>
	/// <param name="value">An object whose name is sent to the <see cref="P:System.Diagnostics.Debug.Listeners" />. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void WriteIf(bool condition, object value)
	{
		TraceInternal.WriteIf(condition, value);
	}

	/// <summary>Writes a category name and message to the trace listeners in the <see cref="P:System.Diagnostics.Debug.Listeners" /> collection if a condition is true.</summary>
	/// <param name="condition">The conditional expression to evaluate. If the condition is true, the category name and message are written to the trace listeners in the collection.</param>
	/// <param name="message">A message to write. </param>
	/// <param name="category">A category name used to organize the output. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void WriteIf(bool condition, string message, string category)
	{
		TraceInternal.WriteIf(condition, message, category);
	}

	/// <summary>Writes a category name and the value of the object's <see cref="M:System.Object.ToString" /> method to the trace listeners in the <see cref="P:System.Diagnostics.Debug.Listeners" /> collection if a condition is true.</summary>
	/// <param name="condition">The conditional expression to evaluate. If the condition is true, the category name and value are written to the trace listeners in the collection.</param>
	/// <param name="value">An object whose name is sent to the <see cref="P:System.Diagnostics.Debug.Listeners" />. </param>
	/// <param name="category">A category name used to organize the output. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void WriteIf(bool condition, object value, string category)
	{
		TraceInternal.WriteIf(condition, value, category);
	}

	/// <summary>Writes a message to the trace listeners in the <see cref="P:System.Diagnostics.Debug.Listeners" /> collection if a condition is true.</summary>
	/// <param name="condition">The conditional expression to evaluate. If the condition is true, the message is written to the trace listeners in the collection.</param>
	/// <param name="message">A message to write. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void WriteLineIf(bool condition, string message)
	{
		TraceInternal.WriteLineIf(condition, message);
	}

	/// <summary>Writes the value of the object's <see cref="M:System.Object.ToString" /> method to the trace listeners in the <see cref="P:System.Diagnostics.Debug.Listeners" /> collection if a condition is true.</summary>
	/// <param name="condition">The conditional expression to evaluate. If the condition is true, the value is written to the trace listeners in the collection.</param>
	/// <param name="value">An object whose name is sent to the <see cref="P:System.Diagnostics.Debug.Listeners" />. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void WriteLineIf(bool condition, object value)
	{
		TraceInternal.WriteLineIf(condition, value);
	}

	/// <summary>Writes a category name and message to the trace listeners in the <see cref="P:System.Diagnostics.Debug.Listeners" /> collection if a condition is true.</summary>
	/// <param name="condition">true to cause a message to be written; otherwise, false. </param>
	/// <param name="message">A message to write. </param>
	/// <param name="category">A category name used to organize the output. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void WriteLineIf(bool condition, string message, string category)
	{
		TraceInternal.WriteLineIf(condition, message, category);
	}

	/// <summary>Writes a category name and the value of the object's <see cref="M:System.Object.ToString" /> method to the trace listeners in the <see cref="P:System.Diagnostics.Debug.Listeners" /> collection if a condition is true.</summary>
	/// <param name="condition">The conditional expression to evaluate. If the condition is true, the category name and value are written to the trace listeners in the collection.</param>
	/// <param name="value">An object whose name is sent to the <see cref="P:System.Diagnostics.Debug.Listeners" />. </param>
	/// <param name="category">A category name used to organize the output. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void WriteLineIf(bool condition, object value, string category)
	{
		TraceInternal.WriteLineIf(condition, value, category);
	}

	/// <summary>Increases the current <see cref="P:System.Diagnostics.Debug.IndentLevel" /> by one.</summary>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void Indent()
	{
		TraceInternal.Indent();
	}

	/// <summary>Decreases the current <see cref="P:System.Diagnostics.Debug.IndentLevel" /> by one.</summary>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Conditional("DEBUG")]
	public static void Unindent()
	{
		TraceInternal.Unindent();
	}
}
