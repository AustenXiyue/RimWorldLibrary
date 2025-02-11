using System.Collections;
using System.Globalization;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace System.Diagnostics;

/// <summary>Directs tracing or debugging output as XML-encoded data to a <see cref="T:System.IO.TextWriter" /> or to a <see cref="T:System.IO.Stream" />, such as a <see cref="T:System.IO.FileStream" />.</summary>
/// <filterpriority>1</filterpriority>
[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
public class XmlWriterTraceListener : TextWriterTraceListener
{
	private const string fixedHeader = "<E2ETraceEvent xmlns=\"http://schemas.microsoft.com/2004/06/E2ETraceEvent\"><System xmlns=\"http://schemas.microsoft.com/2004/06/windows/eventlog/system\">";

	private readonly string machineName = Environment.MachineName;

	private StringBuilder strBldr;

	private XmlTextWriter xmlBlobWriter;

	internal bool shouldRespectFilterOnTraceTransfer;

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.XmlWriterTraceListener" /> class, using the specified stream as the recipient of the debugging and tracing output. </summary>
	/// <param name="stream">A <see cref="T:System.IO.Stream" /> that represents the stream the trace listener writes to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null. </exception>
	public XmlWriterTraceListener(Stream stream)
		: base(stream)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.XmlWriterTraceListener" /> class with the specified name, using the specified stream as the recipient of the debugging and tracing output. </summary>
	/// <param name="stream">A <see cref="T:System.IO.Stream" /> that represents the stream the trace listener writes to. </param>
	/// <param name="name">The name of the new instance. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null. </exception>
	public XmlWriterTraceListener(Stream stream, string name)
		: base(stream, name)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.XmlWriterTraceListener" /> class using the specified writer as the recipient of the debugging and tracing output. </summary>
	/// <param name="writer">A <see cref="T:System.IO.TextWriter" /> that receives the output from the trace listener.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="writer" /> is null. </exception>
	public XmlWriterTraceListener(TextWriter writer)
		: base(writer)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.XmlWriterTraceListener" /> class with the specified name, using the specified writer as the recipient of the debugging and tracing output. </summary>
	/// <param name="writer">A <see cref="T:System.IO.TextWriter" /> that receives the output from the trace listener. </param>
	/// <param name="name">The name of the new instance. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="writer" /> is null. </exception>
	public XmlWriterTraceListener(TextWriter writer, string name)
		: base(writer, name)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.XmlWriterTraceListener" /> class, using the specified file as the recipient of the debugging and tracing output. </summary>
	/// <param name="filename">The name of the file to write to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="filename" /> is null. </exception>
	public XmlWriterTraceListener(string filename)
		: base(filename)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.XmlWriterTraceListener" /> class with the specified name, using the specified file as the recipient of the debugging and tracing output.  </summary>
	/// <param name="filename">The name of the file to write to. </param>
	/// <param name="name">The name of the new instance. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null. </exception>
	public XmlWriterTraceListener(string filename, string name)
		: base(filename, name)
	{
	}

	/// <summary>Writes a verbatim message without any additional context information to the file or stream.</summary>
	/// <param name="message">The message to write.</param>
	public override void Write(string message)
	{
		WriteLine(message);
	}

	/// <summary>Writes a verbatim message without any additional context information followed by the current line terminator to the file or stream.</summary>
	/// <param name="message">The message to write.</param>
	public override void WriteLine(string message)
	{
		TraceEvent(null, global::SR.GetString("Trace"), TraceEventType.Information, 0, message);
	}

	/// <summary>Writes trace information including an error message and a detailed error message to the file or stream.</summary>
	/// <param name="message">The error message to write.</param>
	/// <param name="detailMessage">The detailed error message to append to the error message.</param>
	public override void Fail(string message, string detailMessage)
	{
		StringBuilder stringBuilder = new StringBuilder(message);
		if (detailMessage != null)
		{
			stringBuilder.Append(" ");
			stringBuilder.Append(detailMessage);
		}
		TraceEvent(null, global::SR.GetString("Trace"), TraceEventType.Error, 0, stringBuilder.ToString());
	}

	/// <summary>Writes trace information, a formatted message, and event information to the file or stream.</summary>
	/// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache" /> that contains the current process ID, thread ID, and stack trace information.</param>
	/// <param name="source">The source name. </param>
	/// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType" /> values.</param>
	/// <param name="id">A numeric identifier for the event.</param>
	/// <param name="format">A format string that contains zero or more format items that correspond to objects in the <paramref name="args" /> array.</param>
	/// <param name="args">An object array containing zero or more objects to format.</param>
	/// <filterpriority>1</filterpriority>
	public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
	{
		if (base.Filter == null || base.Filter.ShouldTrace(eventCache, source, eventType, id, format, args))
		{
			WriteHeader(source, eventType, id, eventCache);
			string str = ((args == null) ? format : string.Format(CultureInfo.InvariantCulture, format, args));
			WriteEscaped(str);
			WriteFooter(eventCache);
		}
	}

	/// <summary>Writes trace information, a message, and event information to the file or stream.</summary>
	/// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache" /> that contains the current process ID, thread ID, and stack trace information.</param>
	/// <param name="source">The source name. </param>
	/// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType" /> values.</param>
	/// <param name="id">A numeric identifier for the event.</param>
	/// <param name="message">The message to write.</param>
	/// <filterpriority>1</filterpriority>
	public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
	{
		if (base.Filter == null || base.Filter.ShouldTrace(eventCache, source, eventType, id, message))
		{
			WriteHeader(source, eventType, id, eventCache);
			WriteEscaped(message);
			WriteFooter(eventCache);
		}
	}

	/// <summary>Writes trace information, a data object, and event information to the file or stream.</summary>
	/// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache" /> that contains the current process ID, thread ID, and stack trace information.</param>
	/// <param name="source">The source name. </param>
	/// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType" /> values.</param>
	/// <param name="id">A numeric identifier for the event.</param>
	/// <param name="data">A data object to emit.</param>
	/// <filterpriority>1</filterpriority>
	public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
	{
		if (base.Filter == null || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data))
		{
			WriteHeader(source, eventType, id, eventCache);
			InternalWrite("<TraceData>");
			if (data != null)
			{
				InternalWrite("<DataItem>");
				WriteData(data);
				InternalWrite("</DataItem>");
			}
			InternalWrite("</TraceData>");
			WriteFooter(eventCache);
		}
	}

	/// <summary>Writes trace information, data objects, and event information to the file or stream.</summary>
	/// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache" /> that contains the current process ID, thread ID, and stack trace information.</param>
	/// <param name="source">The source name. </param>
	/// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType" /> values.</param>
	/// <param name="id">A numeric identifier for the event.</param>
	/// <param name="data">An array of data objects to emit.</param>
	/// <filterpriority>1</filterpriority>
	public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
	{
		if (base.Filter != null && !base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data))
		{
			return;
		}
		WriteHeader(source, eventType, id, eventCache);
		InternalWrite("<TraceData>");
		if (data != null)
		{
			for (int i = 0; i < data.Length; i++)
			{
				InternalWrite("<DataItem>");
				if (data[i] != null)
				{
					WriteData(data[i]);
				}
				InternalWrite("</DataItem>");
			}
		}
		InternalWrite("</TraceData>");
		WriteFooter(eventCache);
	}

	private void WriteData(object data)
	{
		if (!(data is XPathNavigator xPathNavigator))
		{
			WriteEscaped(data.ToString());
			return;
		}
		if (strBldr == null)
		{
			strBldr = new StringBuilder();
			xmlBlobWriter = new XmlTextWriter(new StringWriter(strBldr, CultureInfo.CurrentCulture));
		}
		else
		{
			strBldr.Length = 0;
		}
		try
		{
			xPathNavigator.MoveToRoot();
			xmlBlobWriter.WriteNode(xPathNavigator, defattr: false);
			InternalWrite(strBldr.ToString());
		}
		catch (Exception)
		{
			InternalWrite(data.ToString());
		}
	}

	/// <summary>Closes the <see cref="P:System.Diagnostics.TextWriterTraceListener.Writer" /> for this listener so that it no longer receives tracing or debugging output.</summary>
	public override void Close()
	{
		base.Close();
		if (xmlBlobWriter != null)
		{
			xmlBlobWriter.Close();
		}
		xmlBlobWriter = null;
		strBldr = null;
	}

	/// <summary>Writes trace information including the identity of a related activity, a message, and event information to the file or stream.</summary>
	/// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache" /> that contains the current process ID, thread ID, and stack trace information.</param>
	/// <param name="source">The source name. </param>
	/// <param name="id">A numeric identifier for the event.</param>
	/// <param name="message">A trace message to write.</param>
	/// <param name="relatedActivityId">A <see cref="T:System.Guid" /> structure that identifies a related activity.</param>
	/// <filterpriority>1</filterpriority>
	public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
	{
		if (!shouldRespectFilterOnTraceTransfer || base.Filter == null || base.Filter.ShouldTrace(eventCache, source, TraceEventType.Transfer, id, message))
		{
			WriteHeader(source, TraceEventType.Transfer, id, eventCache, relatedActivityId);
			WriteEscaped(message);
			WriteFooter(eventCache);
		}
	}

	private void WriteHeader(string source, TraceEventType eventType, int id, TraceEventCache eventCache, Guid relatedActivityId)
	{
		WriteStartHeader(source, eventType, id, eventCache);
		InternalWrite("\" RelatedActivityID=\"");
		InternalWrite(relatedActivityId.ToString("B"));
		WriteEndHeader(eventCache);
	}

	private void WriteHeader(string source, TraceEventType eventType, int id, TraceEventCache eventCache)
	{
		WriteStartHeader(source, eventType, id, eventCache);
		WriteEndHeader(eventCache);
	}

	private void WriteStartHeader(string source, TraceEventType eventType, int id, TraceEventCache eventCache)
	{
		InternalWrite("<E2ETraceEvent xmlns=\"http://schemas.microsoft.com/2004/06/E2ETraceEvent\"><System xmlns=\"http://schemas.microsoft.com/2004/06/windows/eventlog/system\">");
		InternalWrite("<EventID>");
		uint num = (uint)id;
		InternalWrite(num.ToString(CultureInfo.InvariantCulture));
		InternalWrite("</EventID>");
		InternalWrite("<Type>3</Type>");
		InternalWrite("<SubType Name=\"");
		InternalWrite(eventType.ToString());
		InternalWrite("\">0</SubType>");
		InternalWrite("<Level>");
		int num2 = (int)eventType;
		if (num2 > 255)
		{
			num2 = 255;
		}
		if (num2 < 0)
		{
			num2 = 0;
		}
		InternalWrite(num2.ToString(CultureInfo.InvariantCulture));
		InternalWrite("</Level>");
		InternalWrite("<TimeCreated SystemTime=\"");
		if (eventCache != null)
		{
			InternalWrite(eventCache.DateTime.ToString("o", CultureInfo.InvariantCulture));
		}
		else
		{
			InternalWrite(DateTime.Now.ToString("o", CultureInfo.InvariantCulture));
		}
		InternalWrite("\" />");
		InternalWrite("<Source Name=\"");
		WriteEscaped(source);
		InternalWrite("\" />");
		InternalWrite("<Correlation ActivityID=\"");
		if (eventCache != null)
		{
			InternalWrite(eventCache.ActivityId.ToString("B"));
		}
		else
		{
			InternalWrite(Guid.Empty.ToString("B"));
		}
	}

	private void WriteEndHeader(TraceEventCache eventCache)
	{
		InternalWrite("\" />");
		InternalWrite("<Execution ProcessName=\"");
		InternalWrite(TraceEventCache.GetProcessName());
		InternalWrite("\" ProcessID=\"");
		InternalWrite(((uint)TraceEventCache.GetProcessId()).ToString(CultureInfo.InvariantCulture));
		InternalWrite("\" ThreadID=\"");
		if (eventCache != null)
		{
			WriteEscaped(eventCache.ThreadId.ToString(CultureInfo.InvariantCulture));
		}
		else
		{
			WriteEscaped(TraceEventCache.GetThreadId().ToString(CultureInfo.InvariantCulture));
		}
		InternalWrite("\" />");
		InternalWrite("<Channel/>");
		InternalWrite("<Computer>");
		InternalWrite(machineName);
		InternalWrite("</Computer>");
		InternalWrite("</System>");
		InternalWrite("<ApplicationData>");
	}

	private void WriteFooter(TraceEventCache eventCache)
	{
		bool flag = IsEnabled(TraceOptions.LogicalOperationStack);
		bool flag2 = IsEnabled(TraceOptions.Callstack);
		if (eventCache != null && (flag || flag2))
		{
			InternalWrite("<System.Diagnostics xmlns=\"http://schemas.microsoft.com/2004/08/System.Diagnostics\">");
			if (flag)
			{
				InternalWrite("<LogicalOperationStack>");
				Stack logicalOperationStack = eventCache.LogicalOperationStack;
				if (logicalOperationStack != null)
				{
					foreach (object item in logicalOperationStack)
					{
						InternalWrite("<LogicalOperation>");
						WriteEscaped(item.ToString());
						InternalWrite("</LogicalOperation>");
					}
				}
				InternalWrite("</LogicalOperationStack>");
			}
			InternalWrite("<Timestamp>");
			InternalWrite(eventCache.Timestamp.ToString(CultureInfo.InvariantCulture));
			InternalWrite("</Timestamp>");
			if (flag2)
			{
				InternalWrite("<Callstack>");
				WriteEscaped(eventCache.Callstack);
				InternalWrite("</Callstack>");
			}
			InternalWrite("</System.Diagnostics>");
		}
		InternalWrite("</ApplicationData></E2ETraceEvent>");
	}

	private void WriteEscaped(string str)
	{
		if (str == null)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < str.Length; i++)
		{
			switch (str[i])
			{
			case '&':
				InternalWrite(str.Substring(num, i - num));
				InternalWrite("&amp;");
				num = i + 1;
				break;
			case '<':
				InternalWrite(str.Substring(num, i - num));
				InternalWrite("&lt;");
				num = i + 1;
				break;
			case '>':
				InternalWrite(str.Substring(num, i - num));
				InternalWrite("&gt;");
				num = i + 1;
				break;
			case '"':
				InternalWrite(str.Substring(num, i - num));
				InternalWrite("&quot;");
				num = i + 1;
				break;
			case '\'':
				InternalWrite(str.Substring(num, i - num));
				InternalWrite("&apos;");
				num = i + 1;
				break;
			case '\r':
				InternalWrite(str.Substring(num, i - num));
				InternalWrite("&#xD;");
				num = i + 1;
				break;
			case '\n':
				InternalWrite(str.Substring(num, i - num));
				InternalWrite("&#xA;");
				num = i + 1;
				break;
			}
		}
		InternalWrite(str.Substring(num, str.Length - num));
	}

	private void InternalWrite(string message)
	{
		if (EnsureWriter())
		{
			writer.Write(message);
		}
	}
}
