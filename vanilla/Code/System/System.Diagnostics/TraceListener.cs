using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;

namespace System.Diagnostics;

/// <summary>Provides the abstract base class for the listeners who monitor trace and debug output.</summary>
/// <filterpriority>2</filterpriority>
[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
public abstract class TraceListener : MarshalByRefObject, IDisposable
{
	private int indentLevel;

	private int indentSize = 4;

	private TraceOptions traceOptions;

	private bool needIndent = true;

	private string listenerName;

	private TraceFilter filter;

	private StringDictionary attributes;

	internal string initializeData;

	/// <summary>Gets the custom trace listener attributes defined in the application configuration file.</summary>
	/// <returns>A <see cref="T:System.Collections.Specialized.StringDictionary" /> containing the custom attributes for the trace listener.</returns>
	/// <filterpriority>1</filterpriority>
	public StringDictionary Attributes
	{
		get
		{
			if (attributes == null)
			{
				attributes = new StringDictionary();
			}
			return attributes;
		}
	}

	/// <summary>Gets or sets a name for this <see cref="T:System.Diagnostics.TraceListener" />.</summary>
	/// <returns>A name for this <see cref="T:System.Diagnostics.TraceListener" />. The default is an empty string ("").</returns>
	/// <filterpriority>2</filterpriority>
	public virtual string Name
	{
		get
		{
			if (listenerName != null)
			{
				return listenerName;
			}
			return "";
		}
		set
		{
			listenerName = value;
		}
	}

	/// <summary>Gets a value indicating whether the trace listener is thread safe. </summary>
	/// <returns>true if the trace listener is thread safe; otherwise, false. The default is false.</returns>
	public virtual bool IsThreadSafe => false;

	/// <summary>Gets or sets the indent level.</summary>
	/// <returns>The indent level. The default is zero.</returns>
	/// <filterpriority>2</filterpriority>
	public int IndentLevel
	{
		get
		{
			return indentLevel;
		}
		set
		{
			indentLevel = ((value >= 0) ? value : 0);
		}
	}

	/// <summary>Gets or sets the number of spaces in an indent.</summary>
	/// <returns>The number of spaces in an indent. The default is four spaces.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Set operation failed because the value is less than zero.</exception>
	/// <filterpriority>2</filterpriority>
	public int IndentSize
	{
		get
		{
			return indentSize;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("IndentSize", value, global::SR.GetString("The IndentSize property must be non-negative."));
			}
			indentSize = value;
		}
	}

	/// <summary>Gets and sets the trace filter for the trace listener.</summary>
	/// <returns>An object derived from the <see cref="T:System.Diagnostics.TraceFilter" /> base class.</returns>
	/// <filterpriority>2</filterpriority>
	[ComVisible(false)]
	public TraceFilter Filter
	{
		get
		{
			return filter;
		}
		set
		{
			filter = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether to indent the output.</summary>
	/// <returns>true if the output should be indented; otherwise, false.</returns>
	protected bool NeedIndent
	{
		get
		{
			return needIndent;
		}
		set
		{
			needIndent = value;
		}
	}

	/// <summary>Gets or sets the trace output options.</summary>
	/// <returns>A bitwise combination of the enumeration values. The default is <see cref="F:System.Diagnostics.TraceOptions.None" />. </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Set operation failed because the value is invalid.</exception>
	/// <filterpriority>2</filterpriority>
	[ComVisible(false)]
	public TraceOptions TraceOutputOptions
	{
		get
		{
			return traceOptions;
		}
		set
		{
			if ((int)value >> 6 != 0)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			traceOptions = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.TraceListener" /> class.</summary>
	protected TraceListener()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.TraceListener" /> class using the specified name as the listener.</summary>
	/// <param name="name">The name of the <see cref="T:System.Diagnostics.TraceListener" />. </param>
	protected TraceListener(string name)
	{
		listenerName = name;
	}

	/// <summary>Releases all resources used by the <see cref="T:System.Diagnostics.TraceListener" />.</summary>
	/// <filterpriority>2</filterpriority>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Diagnostics.TraceListener" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
	}

	/// <summary>When overridden in a derived class, closes the output stream so it no longer receives tracing or debugging output.</summary>
	/// <filterpriority>2</filterpriority>
	public virtual void Close()
	{
	}

	/// <summary>When overridden in a derived class, flushes the output buffer.</summary>
	/// <filterpriority>2</filterpriority>
	public virtual void Flush()
	{
	}

	internal void SetAttributes(Hashtable attribs)
	{
		TraceUtils.VerifyAttributes(attribs, GetSupportedAttributes(), this);
		attributes = new StringDictionary();
		attributes.ReplaceHashtable(attribs);
	}

	/// <summary>Emits an error message to the listener you create when you implement the <see cref="T:System.Diagnostics.TraceListener" /> class.</summary>
	/// <param name="message">A message to emit. </param>
	/// <filterpriority>2</filterpriority>
	public virtual void Fail(string message)
	{
		Fail(message, null);
	}

	/// <summary>Emits an error message and a detailed error message to the listener you create when you implement the <see cref="T:System.Diagnostics.TraceListener" /> class.</summary>
	/// <param name="message">A message to emit. </param>
	/// <param name="detailMessage">A detailed message to emit. </param>
	/// <filterpriority>2</filterpriority>
	public virtual void Fail(string message, string detailMessage)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(global::SR.GetString("Fail:"));
		stringBuilder.Append(" ");
		stringBuilder.Append(message);
		if (detailMessage != null)
		{
			stringBuilder.Append(" ");
			stringBuilder.Append(detailMessage);
		}
		WriteLine(stringBuilder.ToString());
	}

	/// <summary>Gets the custom attributes supported by the trace listener.</summary>
	/// <returns>A string array naming the custom attributes supported by the trace listener, or null if there are no custom attributes.</returns>
	protected internal virtual string[] GetSupportedAttributes()
	{
		return null;
	}

	/// <summary>When overridden in a derived class, writes the specified message to the listener you create in the derived class.</summary>
	/// <param name="message">A message to write. </param>
	/// <filterpriority>2</filterpriority>
	public abstract void Write(string message);

	/// <summary>Writes the value of the object's <see cref="M:System.Object.ToString" /> method to the listener you create when you implement the <see cref="T:System.Diagnostics.TraceListener" /> class.</summary>
	/// <param name="o">An <see cref="T:System.Object" /> whose fully qualified class name you want to write. </param>
	/// <filterpriority>2</filterpriority>
	public virtual void Write(object o)
	{
		if ((Filter == null || Filter.ShouldTrace(null, "", TraceEventType.Verbose, 0, null, null, o)) && o != null)
		{
			Write(o.ToString());
		}
	}

	/// <summary>Writes a category name and a message to the listener you create when you implement the <see cref="T:System.Diagnostics.TraceListener" /> class.</summary>
	/// <param name="message">A message to write. </param>
	/// <param name="category">A category name used to organize the output. </param>
	/// <filterpriority>2</filterpriority>
	public virtual void Write(string message, string category)
	{
		if (Filter == null || Filter.ShouldTrace(null, "", TraceEventType.Verbose, 0, message))
		{
			if (category == null)
			{
				Write(message);
			}
			else
			{
				Write(category + ": " + ((message == null) ? string.Empty : message));
			}
		}
	}

	/// <summary>Writes a category name and the value of the object's <see cref="M:System.Object.ToString" /> method to the listener you create when you implement the <see cref="T:System.Diagnostics.TraceListener" /> class.</summary>
	/// <param name="o">An <see cref="T:System.Object" /> whose fully qualified class name you want to write. </param>
	/// <param name="category">A category name used to organize the output. </param>
	/// <filterpriority>2</filterpriority>
	public virtual void Write(object o, string category)
	{
		if (Filter == null || Filter.ShouldTrace(null, "", TraceEventType.Verbose, 0, category, null, o))
		{
			if (category == null)
			{
				Write(o);
			}
			else
			{
				Write((o == null) ? "" : o.ToString(), category);
			}
		}
	}

	/// <summary>Writes the indent to the listener you create when you implement this class, and resets the <see cref="P:System.Diagnostics.TraceListener.NeedIndent" /> property to false.</summary>
	protected virtual void WriteIndent()
	{
		NeedIndent = false;
		for (int i = 0; i < indentLevel; i++)
		{
			if (indentSize == 4)
			{
				Write("    ");
				continue;
			}
			for (int j = 0; j < indentSize; j++)
			{
				Write(" ");
			}
		}
	}

	/// <summary>When overridden in a derived class, writes a message to the listener you create in the derived class, followed by a line terminator.</summary>
	/// <param name="message">A message to write. </param>
	/// <filterpriority>2</filterpriority>
	public abstract void WriteLine(string message);

	/// <summary>Writes the value of the object's <see cref="M:System.Object.ToString" /> method to the listener you create when you implement the <see cref="T:System.Diagnostics.TraceListener" /> class, followed by a line terminator.</summary>
	/// <param name="o">An <see cref="T:System.Object" /> whose fully qualified class name you want to write. </param>
	/// <filterpriority>2</filterpriority>
	public virtual void WriteLine(object o)
	{
		if (Filter == null || Filter.ShouldTrace(null, "", TraceEventType.Verbose, 0, null, null, o))
		{
			WriteLine((o == null) ? "" : o.ToString());
		}
	}

	/// <summary>Writes a category name and a message to the listener you create when you implement the <see cref="T:System.Diagnostics.TraceListener" /> class, followed by a line terminator.</summary>
	/// <param name="message">A message to write. </param>
	/// <param name="category">A category name used to organize the output. </param>
	/// <filterpriority>2</filterpriority>
	public virtual void WriteLine(string message, string category)
	{
		if (Filter == null || Filter.ShouldTrace(null, "", TraceEventType.Verbose, 0, message))
		{
			if (category == null)
			{
				WriteLine(message);
			}
			else
			{
				WriteLine(category + ": " + ((message == null) ? string.Empty : message));
			}
		}
	}

	/// <summary>Writes a category name and the value of the object's <see cref="M:System.Object.ToString" /> method to the listener you create when you implement the <see cref="T:System.Diagnostics.TraceListener" /> class, followed by a line terminator.</summary>
	/// <param name="o">An <see cref="T:System.Object" /> whose fully qualified class name you want to write. </param>
	/// <param name="category">A category name used to organize the output. </param>
	/// <filterpriority>2</filterpriority>
	public virtual void WriteLine(object o, string category)
	{
		if (Filter == null || Filter.ShouldTrace(null, "", TraceEventType.Verbose, 0, category, null, o))
		{
			WriteLine((o == null) ? "" : o.ToString(), category);
		}
	}

	/// <summary>Writes trace information, a data object and event information to the listener specific output.</summary>
	/// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
	/// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
	/// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType" /> values specifying the type of event that has caused the trace.</param>
	/// <param name="id">A numeric identifier for the event.</param>
	/// <param name="data">The trace data to emit.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[ComVisible(false)]
	public virtual void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
	{
		if (Filter == null || Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data))
		{
			WriteHeader(source, eventType, id);
			string message = string.Empty;
			if (data != null)
			{
				message = data.ToString();
			}
			WriteLine(message);
			WriteFooter(eventCache);
		}
	}

	/// <summary>Writes trace information, an array of data objects and event information to the listener specific output.</summary>
	/// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
	/// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
	/// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType" /> values specifying the type of event that has caused the trace.</param>
	/// <param name="id">A numeric identifier for the event.</param>
	/// <param name="data">An array of objects to emit as data.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[ComVisible(false)]
	public virtual void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
	{
		if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data))
		{
			return;
		}
		WriteHeader(source, eventType, id);
		StringBuilder stringBuilder = new StringBuilder();
		if (data != null)
		{
			for (int i = 0; i < data.Length; i++)
			{
				if (i != 0)
				{
					stringBuilder.Append(", ");
				}
				if (data[i] != null)
				{
					stringBuilder.Append(data[i].ToString());
				}
			}
		}
		WriteLine(stringBuilder.ToString());
		WriteFooter(eventCache);
	}

	/// <summary>Writes trace and event information to the listener specific output.</summary>
	/// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
	/// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
	/// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType" /> values specifying the type of event that has caused the trace.</param>
	/// <param name="id">A numeric identifier for the event.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[ComVisible(false)]
	public virtual void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
	{
		TraceEvent(eventCache, source, eventType, id, string.Empty);
	}

	/// <summary>Writes trace information, a message, and event information to the listener specific output.</summary>
	/// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
	/// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
	/// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType" /> values specifying the type of event that has caused the trace.</param>
	/// <param name="id">A numeric identifier for the event.</param>
	/// <param name="message">A message to write.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[ComVisible(false)]
	public virtual void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
	{
		if (Filter == null || Filter.ShouldTrace(eventCache, source, eventType, id, message))
		{
			WriteHeader(source, eventType, id);
			WriteLine(message);
			WriteFooter(eventCache);
		}
	}

	/// <summary>Writes trace information, a formatted array of objects and event information to the listener specific output.</summary>
	/// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
	/// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
	/// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType" /> values specifying the type of event that has caused the trace.</param>
	/// <param name="id">A numeric identifier for the event.</param>
	/// <param name="format">A format string that contains zero or more format items, which correspond to objects in the <paramref name="args" /> array.</param>
	/// <param name="args">An object array containing zero or more objects to format.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[ComVisible(false)]
	public virtual void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
	{
		if (Filter == null || Filter.ShouldTrace(eventCache, source, eventType, id, format, args))
		{
			WriteHeader(source, eventType, id);
			if (args != null)
			{
				WriteLine(string.Format(CultureInfo.InvariantCulture, format, args));
			}
			else
			{
				WriteLine(format);
			}
			WriteFooter(eventCache);
		}
	}

	/// <summary>Writes trace information, a message, a related activity identity and event information to the listener specific output.</summary>
	/// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
	/// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
	/// <param name="id">A numeric identifier for the event.</param>
	/// <param name="message">A message to write.</param>
	/// <param name="relatedActivityId"> A <see cref="T:System.Guid" />  object identifying a related activity.</param>
	/// <filterpriority>1</filterpriority>
	[ComVisible(false)]
	public virtual void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
	{
		TraceEvent(eventCache, source, TraceEventType.Transfer, id, message + ", relatedActivityId=" + relatedActivityId.ToString());
	}

	private void WriteHeader(string source, TraceEventType eventType, int id)
	{
		Write(string.Format(CultureInfo.InvariantCulture, "{0} {1}: {2} : ", source, eventType.ToString(), id.ToString(CultureInfo.InvariantCulture)));
	}

	private void WriteFooter(TraceEventCache eventCache)
	{
		if (eventCache == null)
		{
			return;
		}
		indentLevel++;
		if (IsEnabled(TraceOptions.ProcessId))
		{
			WriteLine("ProcessId=" + eventCache.ProcessId);
		}
		if (IsEnabled(TraceOptions.LogicalOperationStack))
		{
			Write("LogicalOperationStack=");
			Stack logicalOperationStack = eventCache.LogicalOperationStack;
			bool flag = true;
			foreach (object item in logicalOperationStack)
			{
				if (!flag)
				{
					Write(", ");
				}
				else
				{
					flag = false;
				}
				Write(item.ToString());
			}
			WriteLine(string.Empty);
		}
		if (IsEnabled(TraceOptions.ThreadId))
		{
			WriteLine("ThreadId=" + eventCache.ThreadId);
		}
		if (IsEnabled(TraceOptions.DateTime))
		{
			WriteLine("DateTime=" + eventCache.DateTime.ToString("o", CultureInfo.InvariantCulture));
		}
		if (IsEnabled(TraceOptions.Timestamp))
		{
			WriteLine("Timestamp=" + eventCache.Timestamp);
		}
		if (IsEnabled(TraceOptions.Callstack))
		{
			WriteLine("Callstack=" + eventCache.Callstack);
		}
		indentLevel--;
	}

	internal bool IsEnabled(TraceOptions opts)
	{
		return (opts & TraceOutputOptions) != 0;
	}
}
