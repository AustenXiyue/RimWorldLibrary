using System.Collections;
using System.Globalization;
using System.IO;
using System.Security.Permissions;
using System.Text;

namespace System.Diagnostics;

/// <summary>Directs tracing or debugging output to a text writer, such as a stream writer, or to a stream, such as a file stream.</summary>
/// <filterpriority>1</filterpriority>
[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
public class DelimitedListTraceListener : TextWriterTraceListener
{
	private string delimiter = ";";

	private string secondaryDelim = ",";

	private bool initializedDelim;

	/// <summary>Gets or sets the delimiter for the delimited list.</summary>
	/// <returns>The delimiter for the delimited list.</returns>
	/// <exception cref="T:System.ArgumentNullException">
	///   <see cref="P:System.Diagnostics.DelimitedListTraceListener.Delimiter" /> is set to null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Diagnostics.DelimitedListTraceListener.Delimiter" /> is set to an empty string ("").</exception>
	/// <filterpriority>1</filterpriority>
	public string Delimiter
	{
		get
		{
			lock (this)
			{
				if (!initializedDelim)
				{
					if (base.Attributes.ContainsKey("delimiter"))
					{
						delimiter = base.Attributes["delimiter"];
					}
					initializedDelim = true;
				}
			}
			return delimiter;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Delimiter");
			}
			if (value.Length == 0)
			{
				throw new ArgumentException(global::SR.GetString("Generic_ArgCantBeEmptyString", "Delimiter"));
			}
			lock (this)
			{
				delimiter = value;
				initializedDelim = true;
			}
			if (delimiter == ",")
			{
				secondaryDelim = ";";
			}
			else
			{
				secondaryDelim = ",";
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.DelimitedListTraceListener" /> class that writes to the specified output stream. </summary>
	/// <param name="stream">The <see cref="T:System.IO.Stream" /> to receive the output.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null. </exception>
	public DelimitedListTraceListener(Stream stream)
		: base(stream)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.DelimitedListTraceListener" /> class that writes to the specified output stream and has the specified name. </summary>
	/// <param name="stream">The <see cref="T:System.IO.Stream" /> to receive the output.</param>
	/// <param name="name">The name of the new instance of the trace listener. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null. </exception>
	public DelimitedListTraceListener(Stream stream, string name)
		: base(stream, name)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.DelimitedListTraceListener" /> class that writes to the specified text writer. </summary>
	/// <param name="writer">The <see cref="T:System.IO.TextWriter" /> to receive the output.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="writer" /> is null. </exception>
	public DelimitedListTraceListener(TextWriter writer)
		: base(writer)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.DelimitedListTraceListener" /> class that writes to the specified text writer and has the specified name. </summary>
	/// <param name="writer">The <see cref="T:System.IO.TextWriter" /> to receive the output.</param>
	/// <param name="name">The name of the new instance of the trace listener. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="writer" /> is null. </exception>
	public DelimitedListTraceListener(TextWriter writer, string name)
		: base(writer, name)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.DelimitedListTraceListener" /> class that writes to the specified file.  </summary>
	/// <param name="fileName">The name of the file to receive the output.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="fileName" /> is null. </exception>
	public DelimitedListTraceListener(string fileName)
		: base(fileName)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.DelimitedListTraceListener" /> class that writes to the specified file and has the specified name. </summary>
	/// <param name="fileName">The name of the file to receive the output. </param>
	/// <param name="name">The name of the new instance of the trace listener. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="fileName" /> is null. </exception>
	public DelimitedListTraceListener(string fileName, string name)
		: base(fileName, name)
	{
	}

	/// <summary>Returns the custom configuration file attribute supported by the delimited trace listener.</summary>
	/// <returns>A string array that contains the single value "delimiter".</returns>
	protected internal override string[] GetSupportedAttributes()
	{
		return new string[1] { "delimiter" };
	}

	/// <summary>Writes trace information, a formatted array of objects, and event information to the output file or stream.</summary>
	/// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
	/// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
	/// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType" /> values specifying the type of event that has caused the trace.</param>
	/// <param name="id">A numeric identifier for the event.</param>
	/// <param name="format">A format string that contains zero or more format items that correspond to objects in the <paramref name="args" /> array.</param>
	/// <param name="args">An array containing zero or more objects to format.</param>
	/// <filterpriority>1</filterpriority>
	public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
	{
		if (base.Filter == null || base.Filter.ShouldTrace(eventCache, source, eventType, id, format, args))
		{
			WriteHeader(source, eventType, id);
			if (args != null)
			{
				WriteEscaped(string.Format(CultureInfo.InvariantCulture, format, args));
			}
			else
			{
				WriteEscaped(format);
			}
			Write(Delimiter);
			Write(Delimiter);
			WriteFooter(eventCache);
		}
	}

	/// <summary>Writes trace information, a message, and event information to the output file or stream.</summary>
	/// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
	/// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
	/// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType" /> values specifying the type of event that has caused the trace.</param>
	/// <param name="id">A numeric identifier for the event.</param>
	/// <param name="message">The trace message to write to the output file or stream.</param>
	/// <filterpriority>1</filterpriority>
	public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
	{
		if (base.Filter == null || base.Filter.ShouldTrace(eventCache, source, eventType, id, message))
		{
			WriteHeader(source, eventType, id);
			WriteEscaped(message);
			Write(Delimiter);
			Write(Delimiter);
			WriteFooter(eventCache);
		}
	}

	/// <summary>Writes trace information, a data object, and event information to the output file or stream.</summary>
	/// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
	/// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
	/// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType" /> values specifying the type of event that has caused the trace.</param>
	/// <param name="id">A numeric identifier for the event.</param>
	/// <param name="data">A data object to write to the output file or stream.</param>
	/// <filterpriority>1</filterpriority>
	public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
	{
		if (base.Filter == null || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data))
		{
			WriteHeader(source, eventType, id);
			Write(Delimiter);
			WriteEscaped(data.ToString());
			Write(Delimiter);
			WriteFooter(eventCache);
		}
	}

	/// <summary>Writes trace information, an array of data objects, and event information to the output file or stream.</summary>
	/// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
	/// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
	/// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType" /> values specifying the type of event that has caused the trace.</param>
	/// <param name="id">A numeric identifier for the event.</param>
	/// <param name="data">An array of data objects to write to the output file or stream.</param>
	/// <filterpriority>1</filterpriority>
	public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
	{
		if (base.Filter != null && !base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data))
		{
			return;
		}
		WriteHeader(source, eventType, id);
		Write(Delimiter);
		if (data != null)
		{
			for (int i = 0; i < data.Length; i++)
			{
				if (i != 0)
				{
					Write(secondaryDelim);
				}
				WriteEscaped(data[i].ToString());
			}
		}
		Write(Delimiter);
		WriteFooter(eventCache);
	}

	private void WriteHeader(string source, TraceEventType eventType, int id)
	{
		WriteEscaped(source);
		Write(Delimiter);
		Write(eventType.ToString());
		Write(Delimiter);
		Write(id.ToString(CultureInfo.InvariantCulture));
		Write(Delimiter);
	}

	private void WriteFooter(TraceEventCache eventCache)
	{
		if (eventCache != null)
		{
			if (IsEnabled(TraceOptions.ProcessId))
			{
				Write(eventCache.ProcessId.ToString(CultureInfo.InvariantCulture));
			}
			Write(Delimiter);
			if (IsEnabled(TraceOptions.LogicalOperationStack))
			{
				WriteStackEscaped(eventCache.LogicalOperationStack);
			}
			Write(Delimiter);
			if (IsEnabled(TraceOptions.ThreadId))
			{
				WriteEscaped(eventCache.ThreadId.ToString(CultureInfo.InvariantCulture));
			}
			Write(Delimiter);
			if (IsEnabled(TraceOptions.DateTime))
			{
				WriteEscaped(eventCache.DateTime.ToString("o", CultureInfo.InvariantCulture));
			}
			Write(Delimiter);
			if (IsEnabled(TraceOptions.Timestamp))
			{
				Write(eventCache.Timestamp.ToString(CultureInfo.InvariantCulture));
			}
			Write(Delimiter);
			if (IsEnabled(TraceOptions.Callstack))
			{
				WriteEscaped(eventCache.Callstack);
			}
		}
		else
		{
			for (int i = 0; i < 5; i++)
			{
				Write(Delimiter);
			}
		}
		WriteLine("");
	}

	private void WriteEscaped(string message)
	{
		if (!string.IsNullOrEmpty(message))
		{
			StringBuilder stringBuilder = new StringBuilder("\"");
			int num = 0;
			int num2;
			while ((num2 = message.IndexOf('"', num)) != -1)
			{
				stringBuilder.Append(message, num, num2 - num);
				stringBuilder.Append("\"\"");
				num = num2 + 1;
			}
			stringBuilder.Append(message, num, message.Length - num);
			stringBuilder.Append("\"");
			Write(stringBuilder.ToString());
		}
	}

	private void WriteStackEscaped(Stack stack)
	{
		StringBuilder stringBuilder = new StringBuilder("\"");
		bool flag = true;
		foreach (object item in stack)
		{
			if (!flag)
			{
				stringBuilder.Append(", ");
			}
			else
			{
				flag = false;
			}
			string text = item.ToString();
			int num = 0;
			int num2;
			while ((num2 = text.IndexOf('"', num)) != -1)
			{
				stringBuilder.Append(text, num, num2 - num);
				stringBuilder.Append("\"\"");
				num = num2 + 1;
			}
			stringBuilder.Append(text, num, text.Length - num);
		}
		stringBuilder.Append("\"");
		Write(stringBuilder.ToString());
	}
}
