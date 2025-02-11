using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Permissions;

namespace System.Diagnostics;

/// <summary>Provides a set of methods and properties that enable applications to trace the execution of code and associate trace messages with their source. </summary>
/// <filterpriority>1</filterpriority>
public class TraceSource
{
	private static List<WeakReference> tracesources = new List<WeakReference>();

	private static int s_LastCollectionCount;

	private volatile SourceSwitch internalSwitch;

	private volatile TraceListenerCollection listeners;

	private StringDictionary attributes;

	private SourceLevels switchLevel;

	private volatile string sourceName;

	internal volatile bool _initCalled;

	/// <summary>Gets the custom switch attributes defined in the application configuration file.</summary>
	/// <returns>A <see cref="T:System.Collections.Specialized.StringDictionary" /> containing the custom attributes for the trace switch.</returns>
	/// <filterpriority>1</filterpriority>
	public StringDictionary Attributes
	{
		get
		{
			Initialize();
			if (attributes == null)
			{
				attributes = new StringDictionary();
			}
			return attributes;
		}
	}

	/// <summary>Gets the name of the trace source.</summary>
	/// <returns>The name of the trace source.</returns>
	/// <filterpriority>1</filterpriority>
	public string Name => sourceName;

	/// <summary>Gets the collection of trace listeners for the trace source.</summary>
	/// <returns>A <see cref="T:System.Diagnostics.TraceListenerCollection" /> that contains the active trace listeners associated with the source. </returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public TraceListenerCollection Listeners
	{
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		get
		{
			Initialize();
			return listeners;
		}
	}

	/// <summary>Gets or sets the source switch value.</summary>
	/// <returns>A <see cref="T:System.Diagnostics.SourceSwitch" /> object representing the source switch value.</returns>
	/// <exception cref="T:System.ArgumentNullException">
	///   <see cref="P:System.Diagnostics.TraceSource.Switch" /> is set to null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public SourceSwitch Switch
	{
		get
		{
			Initialize();
			return internalSwitch;
		}
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Switch");
			}
			Initialize();
			internalSwitch = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.TraceSource" /> class, using the specified name for the source. </summary>
	/// <param name="name">The name of the source (typically, the name of the application).</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is an empty string ("").</exception>
	public TraceSource(string name)
		: this(name, SourceLevels.Off)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.TraceSource" /> class, using the specified name for the source and the default source level at which tracing is to occur.  </summary>
	/// <param name="name">The name of the source, typically the name of the application.</param>
	/// <param name="defaultLevel">A bitwise combination of the enumeration values that specifies the default source level at which to trace.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is an empty string ("").</exception>
	public TraceSource(string name, SourceLevels defaultLevel)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException("name");
		}
		sourceName = name;
		switchLevel = defaultLevel;
		lock (tracesources)
		{
			_pruneCachedTraceSources();
			tracesources.Add(new WeakReference(this));
		}
	}

	private static void _pruneCachedTraceSources()
	{
		lock (tracesources)
		{
			if (s_LastCollectionCount == GC.CollectionCount(2))
			{
				return;
			}
			List<WeakReference> list = new List<WeakReference>(tracesources.Count);
			for (int i = 0; i < tracesources.Count; i++)
			{
				if ((TraceSource)tracesources[i].Target != null)
				{
					list.Add(tracesources[i]);
				}
			}
			if (list.Count < tracesources.Count)
			{
				tracesources.Clear();
				tracesources.AddRange(list);
				tracesources.TrimExcess();
			}
			s_LastCollectionCount = GC.CollectionCount(2);
		}
	}

	private void Initialize()
	{
		if (_initCalled)
		{
			return;
		}
		lock (this)
		{
			if (_initCalled)
			{
				return;
			}
			SourceElementsCollection sources = DiagnosticsConfiguration.Sources;
			if (sources != null)
			{
				SourceElement sourceElement = sources[sourceName];
				if (sourceElement != null)
				{
					if (!string.IsNullOrEmpty(sourceElement.SwitchName))
					{
						CreateSwitch(sourceElement.SwitchType, sourceElement.SwitchName);
					}
					else
					{
						CreateSwitch(sourceElement.SwitchType, sourceName);
						if (!string.IsNullOrEmpty(sourceElement.SwitchValue))
						{
							internalSwitch.Level = (SourceLevels)Enum.Parse(typeof(SourceLevels), sourceElement.SwitchValue);
						}
					}
					listeners = sourceElement.Listeners.GetRuntimeObject();
					attributes = new StringDictionary();
					TraceUtils.VerifyAttributes(sourceElement.Attributes, GetSupportedAttributes(), this);
					attributes.ReplaceHashtable(sourceElement.Attributes);
				}
				else
				{
					NoConfigInit();
				}
			}
			else
			{
				NoConfigInit();
			}
			_initCalled = true;
		}
	}

	private void NoConfigInit()
	{
		internalSwitch = new SourceSwitch(sourceName, switchLevel.ToString());
		listeners = new TraceListenerCollection();
		listeners.Add(new DefaultTraceListener());
		attributes = null;
	}

	/// <summary>Closes all the trace listeners in the trace listener collection.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	public void Close()
	{
		if (listeners == null)
		{
			return;
		}
		lock (TraceInternal.critSec)
		{
			foreach (TraceListener listener in listeners)
			{
				listener.Close();
			}
		}
	}

	/// <summary>Flushes all the trace listeners in the trace listener collection.</summary>
	/// <exception cref="T:System.ObjectDisposedException">An attempt was made to trace an event during finalization.</exception>
	/// <filterpriority>1</filterpriority>
	public void Flush()
	{
		if (listeners == null)
		{
			return;
		}
		if (TraceInternal.UseGlobalLock)
		{
			lock (TraceInternal.critSec)
			{
				foreach (TraceListener listener in listeners)
				{
					listener.Flush();
				}
				return;
			}
		}
		foreach (TraceListener listener2 in listeners)
		{
			if (!listener2.IsThreadSafe)
			{
				lock (listener2)
				{
					listener2.Flush();
				}
			}
			else
			{
				listener2.Flush();
			}
		}
	}

	/// <summary>Gets the custom attributes supported by the trace source.</summary>
	/// <returns>A string array naming the custom attributes supported by the trace source, or null if there are no custom attributes.</returns>
	protected internal virtual string[] GetSupportedAttributes()
	{
		return null;
	}

	internal static void RefreshAll()
	{
		lock (tracesources)
		{
			_pruneCachedTraceSources();
			for (int i = 0; i < tracesources.Count; i++)
			{
				((TraceSource)tracesources[i].Target)?.Refresh();
			}
		}
	}

	internal void Refresh()
	{
		if (!_initCalled)
		{
			Initialize();
			return;
		}
		SourceElementsCollection sources = DiagnosticsConfiguration.Sources;
		if (sources == null)
		{
			return;
		}
		SourceElement sourceElement = sources[Name];
		if (sourceElement != null)
		{
			if ((string.IsNullOrEmpty(sourceElement.SwitchType) && internalSwitch.GetType() != typeof(SourceSwitch)) || sourceElement.SwitchType != internalSwitch.GetType().AssemblyQualifiedName)
			{
				if (!string.IsNullOrEmpty(sourceElement.SwitchName))
				{
					CreateSwitch(sourceElement.SwitchType, sourceElement.SwitchName);
				}
				else
				{
					CreateSwitch(sourceElement.SwitchType, Name);
					if (!string.IsNullOrEmpty(sourceElement.SwitchValue))
					{
						internalSwitch.Level = (SourceLevels)Enum.Parse(typeof(SourceLevels), sourceElement.SwitchValue);
					}
				}
			}
			else if (!string.IsNullOrEmpty(sourceElement.SwitchName))
			{
				if (sourceElement.SwitchName != internalSwitch.DisplayName)
				{
					CreateSwitch(sourceElement.SwitchType, sourceElement.SwitchName);
				}
				else
				{
					internalSwitch.Refresh();
				}
			}
			else if (!string.IsNullOrEmpty(sourceElement.SwitchValue))
			{
				internalSwitch.Level = (SourceLevels)Enum.Parse(typeof(SourceLevels), sourceElement.SwitchValue);
			}
			else
			{
				internalSwitch.Level = SourceLevels.Off;
			}
			TraceListenerCollection traceListenerCollection = new TraceListenerCollection();
			foreach (ListenerElement listener in sourceElement.Listeners)
			{
				TraceListener traceListener = listeners[listener.Name];
				if (traceListener != null)
				{
					traceListenerCollection.Add(listener.RefreshRuntimeObject(traceListener));
				}
				else
				{
					traceListenerCollection.Add(listener.GetRuntimeObject());
				}
			}
			TraceUtils.VerifyAttributes(sourceElement.Attributes, GetSupportedAttributes(), this);
			attributes = new StringDictionary();
			attributes.ReplaceHashtable(sourceElement.Attributes);
			listeners = traceListenerCollection;
		}
		else
		{
			internalSwitch.Level = switchLevel;
			listeners.Clear();
			attributes = null;
		}
	}

	/// <summary>Writes a trace event message to the trace listeners in the <see cref="P:System.Diagnostics.TraceSource.Listeners" /> collection using the specified event type and event identifier.</summary>
	/// <param name="eventType">One of the enumeration values that specifies the event type of the trace data.</param>
	/// <param name="id">A numeric identifier for the event.</param>
	/// <exception cref="T:System.ObjectDisposedException">An attempt was made to trace an event during finalization.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public void TraceEvent(TraceEventType eventType, int id)
	{
		Initialize();
		TraceEventCache eventCache = new TraceEventCache();
		if (!internalSwitch.ShouldTrace(eventType) || listeners == null)
		{
			return;
		}
		if (TraceInternal.UseGlobalLock)
		{
			lock (TraceInternal.critSec)
			{
				for (int i = 0; i < listeners.Count; i++)
				{
					TraceListener traceListener = listeners[i];
					traceListener.TraceEvent(eventCache, Name, eventType, id);
					if (Trace.AutoFlush)
					{
						traceListener.Flush();
					}
				}
				return;
			}
		}
		for (int j = 0; j < listeners.Count; j++)
		{
			TraceListener traceListener2 = listeners[j];
			if (!traceListener2.IsThreadSafe)
			{
				lock (traceListener2)
				{
					traceListener2.TraceEvent(eventCache, Name, eventType, id);
					if (Trace.AutoFlush)
					{
						traceListener2.Flush();
					}
				}
			}
			else
			{
				traceListener2.TraceEvent(eventCache, Name, eventType, id);
				if (Trace.AutoFlush)
				{
					traceListener2.Flush();
				}
			}
		}
	}

	/// <summary>Writes a trace event message to the trace listeners in the <see cref="P:System.Diagnostics.TraceSource.Listeners" /> collection using the specified event type, event identifier, and message.</summary>
	/// <param name="eventType">One of the enumeration values that specifies the event type of the trace data.</param>
	/// <param name="id">A numeric identifier for the event.</param>
	/// <param name="message">The trace message to write.</param>
	/// <exception cref="T:System.ObjectDisposedException">An attempt was made to trace an event during finalization.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public void TraceEvent(TraceEventType eventType, int id, string message)
	{
		Initialize();
		TraceEventCache eventCache = new TraceEventCache();
		if (!internalSwitch.ShouldTrace(eventType) || listeners == null)
		{
			return;
		}
		if (TraceInternal.UseGlobalLock)
		{
			lock (TraceInternal.critSec)
			{
				for (int i = 0; i < listeners.Count; i++)
				{
					TraceListener traceListener = listeners[i];
					traceListener.TraceEvent(eventCache, Name, eventType, id, message);
					if (Trace.AutoFlush)
					{
						traceListener.Flush();
					}
				}
				return;
			}
		}
		for (int j = 0; j < listeners.Count; j++)
		{
			TraceListener traceListener2 = listeners[j];
			if (!traceListener2.IsThreadSafe)
			{
				lock (traceListener2)
				{
					traceListener2.TraceEvent(eventCache, Name, eventType, id, message);
					if (Trace.AutoFlush)
					{
						traceListener2.Flush();
					}
				}
			}
			else
			{
				traceListener2.TraceEvent(eventCache, Name, eventType, id, message);
				if (Trace.AutoFlush)
				{
					traceListener2.Flush();
				}
			}
		}
	}

	/// <summary>Writes a trace event to the trace listeners in the <see cref="P:System.Diagnostics.TraceSource.Listeners" /> collection using the specified event type, event identifier, and argument array and format.</summary>
	/// <param name="eventType">One of the enumeration values that specifies the event type of the trace data.</param>
	/// <param name="id">A numeric identifier for the event.</param>
	/// <param name="format">A composite format string (see Remarks) that contains text intermixed with zero or more format items, which correspond to objects in the <paramref name="args" /> array.</param>
	/// <param name="args">An object array containing zero or more objects to format.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is invalid.-or- The number that indicates an argument to format is less than zero, or greater than or equal to the number of specified objects to format. </exception>
	/// <exception cref="T:System.ObjectDisposedException">An attempt was made to trace an event during finalization.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public void TraceEvent(TraceEventType eventType, int id, string format, params object[] args)
	{
		Initialize();
		TraceEventCache eventCache = new TraceEventCache();
		if (!internalSwitch.ShouldTrace(eventType) || listeners == null)
		{
			return;
		}
		if (TraceInternal.UseGlobalLock)
		{
			lock (TraceInternal.critSec)
			{
				for (int i = 0; i < listeners.Count; i++)
				{
					TraceListener traceListener = listeners[i];
					traceListener.TraceEvent(eventCache, Name, eventType, id, format, args);
					if (Trace.AutoFlush)
					{
						traceListener.Flush();
					}
				}
				return;
			}
		}
		for (int j = 0; j < listeners.Count; j++)
		{
			TraceListener traceListener2 = listeners[j];
			if (!traceListener2.IsThreadSafe)
			{
				lock (traceListener2)
				{
					traceListener2.TraceEvent(eventCache, Name, eventType, id, format, args);
					if (Trace.AutoFlush)
					{
						traceListener2.Flush();
					}
				}
			}
			else
			{
				traceListener2.TraceEvent(eventCache, Name, eventType, id, format, args);
				if (Trace.AutoFlush)
				{
					traceListener2.Flush();
				}
			}
		}
	}

	/// <summary>Writes trace data to the trace listeners in the <see cref="P:System.Diagnostics.TraceSource.Listeners" /> collection using the specified event type, event identifier, and trace data.</summary>
	/// <param name="eventType">One of the enumeration values that specifies the event type of the trace data.</param>
	/// <param name="id">A numeric identifier for the event.</param>
	/// <param name="data">The trace data.</param>
	/// <exception cref="T:System.ObjectDisposedException">An attempt was made to trace an event during finalization.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public void TraceData(TraceEventType eventType, int id, object data)
	{
		Initialize();
		TraceEventCache eventCache = new TraceEventCache();
		if (!internalSwitch.ShouldTrace(eventType) || listeners == null)
		{
			return;
		}
		if (TraceInternal.UseGlobalLock)
		{
			lock (TraceInternal.critSec)
			{
				for (int i = 0; i < listeners.Count; i++)
				{
					TraceListener traceListener = listeners[i];
					traceListener.TraceData(eventCache, Name, eventType, id, data);
					if (Trace.AutoFlush)
					{
						traceListener.Flush();
					}
				}
				return;
			}
		}
		for (int j = 0; j < listeners.Count; j++)
		{
			TraceListener traceListener2 = listeners[j];
			if (!traceListener2.IsThreadSafe)
			{
				lock (traceListener2)
				{
					traceListener2.TraceData(eventCache, Name, eventType, id, data);
					if (Trace.AutoFlush)
					{
						traceListener2.Flush();
					}
				}
			}
			else
			{
				traceListener2.TraceData(eventCache, Name, eventType, id, data);
				if (Trace.AutoFlush)
				{
					traceListener2.Flush();
				}
			}
		}
	}

	/// <summary>Writes trace data to the trace listeners in the <see cref="P:System.Diagnostics.TraceSource.Listeners" /> collection using the specified event type, event identifier, and trace data array.</summary>
	/// <param name="eventType">One of the enumeration values that specifies the event type of the trace data.</param>
	/// <param name="id">A numeric identifier for the event.</param>
	/// <param name="data">An object array containing the trace data.</param>
	/// <exception cref="T:System.ObjectDisposedException">An attempt was made to trace an event during finalization.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public void TraceData(TraceEventType eventType, int id, params object[] data)
	{
		Initialize();
		TraceEventCache eventCache = new TraceEventCache();
		if (!internalSwitch.ShouldTrace(eventType) || listeners == null)
		{
			return;
		}
		if (TraceInternal.UseGlobalLock)
		{
			lock (TraceInternal.critSec)
			{
				for (int i = 0; i < listeners.Count; i++)
				{
					TraceListener traceListener = listeners[i];
					traceListener.TraceData(eventCache, Name, eventType, id, data);
					if (Trace.AutoFlush)
					{
						traceListener.Flush();
					}
				}
				return;
			}
		}
		for (int j = 0; j < listeners.Count; j++)
		{
			TraceListener traceListener2 = listeners[j];
			if (!traceListener2.IsThreadSafe)
			{
				lock (traceListener2)
				{
					traceListener2.TraceData(eventCache, Name, eventType, id, data);
					if (Trace.AutoFlush)
					{
						traceListener2.Flush();
					}
				}
			}
			else
			{
				traceListener2.TraceData(eventCache, Name, eventType, id, data);
				if (Trace.AutoFlush)
				{
					traceListener2.Flush();
				}
			}
		}
	}

	/// <summary>Writes an informational message to the trace listeners in the <see cref="P:System.Diagnostics.TraceSource.Listeners" /> collection using the specified message.</summary>
	/// <param name="message">The informative message to write.</param>
	/// <exception cref="T:System.ObjectDisposedException">An attempt was made to trace an event during finalization.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public void TraceInformation(string message)
	{
	}

	/// <summary>Writes an informational message to the trace listeners in the <see cref="P:System.Diagnostics.TraceSource.Listeners" /> collection using the specified object array and formatting information.</summary>
	/// <param name="format">A composite format string (see Remarks) that contains text intermixed with zero or more format items, which correspond to objects in the <paramref name="args" /> array.</param>
	/// <param name="args">An array containing zero or more objects to format.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is invalid.-or- The number that indicates an argument to format is less than zero, or greater than or equal to the number of specified objects to format. </exception>
	/// <exception cref="T:System.ObjectDisposedException">An attempt was made to trace an event during finalization.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Conditional("TRACE")]
	public void TraceInformation(string format, params object[] args)
	{
	}

	/// <summary>Writes a trace transfer message to the trace listeners in the <see cref="P:System.Diagnostics.TraceSource.Listeners" /> collection using the specified numeric identifier, message, and related activity identifier.</summary>
	/// <param name="id">A numeric identifier for the event.</param>
	/// <param name="message">The trace message to write.</param>
	/// <param name="relatedActivityId">A structure that identifies the related activity.</param>
	/// <filterpriority>1</filterpriority>
	[Conditional("TRACE")]
	public void TraceTransfer(int id, string message, Guid relatedActivityId)
	{
		Initialize();
		TraceEventCache eventCache = new TraceEventCache();
		if (!internalSwitch.ShouldTrace(TraceEventType.Transfer) || listeners == null)
		{
			return;
		}
		if (TraceInternal.UseGlobalLock)
		{
			lock (TraceInternal.critSec)
			{
				for (int i = 0; i < listeners.Count; i++)
				{
					TraceListener traceListener = listeners[i];
					traceListener.TraceTransfer(eventCache, Name, id, message, relatedActivityId);
					if (Trace.AutoFlush)
					{
						traceListener.Flush();
					}
				}
				return;
			}
		}
		for (int j = 0; j < listeners.Count; j++)
		{
			TraceListener traceListener2 = listeners[j];
			if (!traceListener2.IsThreadSafe)
			{
				lock (traceListener2)
				{
					traceListener2.TraceTransfer(eventCache, Name, id, message, relatedActivityId);
					if (Trace.AutoFlush)
					{
						traceListener2.Flush();
					}
				}
			}
			else
			{
				traceListener2.TraceTransfer(eventCache, Name, id, message, relatedActivityId);
				if (Trace.AutoFlush)
				{
					traceListener2.Flush();
				}
			}
		}
	}

	private void CreateSwitch(string typename, string name)
	{
		if (!string.IsNullOrEmpty(typename))
		{
			internalSwitch = (SourceSwitch)TraceUtils.GetRuntimeObject(typename, typeof(SourceSwitch), name);
		}
		else
		{
			internalSwitch = new SourceSwitch(name, switchLevel.ToString());
		}
	}
}
