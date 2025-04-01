using System.Security.Permissions;
using Unity;

namespace System.Diagnostics;

/// <summary>Directs tracing or debugging output of end-to-end events to an XML-encoded, schema-compliant log file.</summary>
/// <filterpriority>2</filterpriority>
[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public class EventSchemaTraceListener : TextWriterTraceListener
{
	/// <summary>Gets the size of the output buffer.</summary>
	/// <returns>The size of the output buffer, in bytes. </returns>
	/// <filterpriority>2</filterpriority>
	public int BufferSize
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(int);
		}
	}

	/// <summary>Gets the maximum size of the log file.</summary>
	/// <returns>The maximum file size, in bytes.</returns>
	/// <filterpriority>2</filterpriority>
	public long MaximumFileSize
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(long);
		}
	}

	/// <summary>Gets the maximum number of log files.</summary>
	/// <returns>The maximum number of log files, determined by the value of the <see cref="P:System.Diagnostics.EventSchemaTraceListener.TraceLogRetentionOption" /> property for the file.</returns>
	/// <filterpriority>2</filterpriority>
	public int MaximumNumberOfFiles
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(int);
		}
	}

	/// <summary>Gets the trace log retention option for the file.</summary>
	/// <returns>One of the <see cref="T:System.Diagnostics.TraceLogRetentionOption" /> values. The default is <see cref="F:System.Diagnostics.TraceLogRetentionOption.SingleFileUnboundedSize" />. </returns>
	/// <filterpriority>2</filterpriority>
	public TraceLogRetentionOption TraceLogRetentionOption
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(TraceLogRetentionOption);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.EventSchemaTraceListener" /> class, using the specified file as the recipient of debugging and tracing output.</summary>
	/// <param name="fileName">The path for the log file.</param>
	/// <filterpriority>2</filterpriority>
	public EventSchemaTraceListener(string fileName)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.EventSchemaTraceListener" /> class with the specified name, using the specified file as the recipient of debugging and tracing output.</summary>
	/// <param name="fileName">The path for the log file.</param>
	/// <param name="name">The name of the listener.</param>
	/// <filterpriority>2</filterpriority>
	public EventSchemaTraceListener(string fileName, string name)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.EventSchemaTraceListener" /> class with the specified name and specified buffer size, using the specified file as the recipient of debugging and tracing output.</summary>
	/// <param name="fileName">The path for the log file.</param>
	/// <param name="name">The name of the listener.</param>
	/// <param name="bufferSize">The size of the output buffer, in bytes.</param>
	/// <filterpriority>2</filterpriority>
	public EventSchemaTraceListener(string fileName, string name, int bufferSize)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.EventSchemaTraceListener" /> class with the specified name and specified buffer size, using the specified file with the specified log retention policy as the recipient of the debugging and tracing output.</summary>
	/// <param name="fileName">The path for the log file.</param>
	/// <param name="name">The name of the listener.</param>
	/// <param name="bufferSize">The size of the output buffer, in bytes.</param>
	/// <param name="logRetentionOption">One of the <see cref="T:System.Diagnostics.TraceLogRetentionOption" /> values. </param>
	/// <filterpriority>2</filterpriority>
	public EventSchemaTraceListener(string fileName, string name, int bufferSize, TraceLogRetentionOption logRetentionOption)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.EventSchemaTraceListener" /> class with the specified name and specified buffer size, using the specified file with the specified log retention policy and maximum size as the recipient of the debugging and tracing output.</summary>
	/// <param name="fileName">The path for the log file.</param>
	/// <param name="name">The name of the listener.</param>
	/// <param name="bufferSize">The size of the output buffer, in bytes.</param>
	/// <param name="logRetentionOption">One of the <see cref="T:System.Diagnostics.TraceLogRetentionOption" /> values.</param>
	/// <param name="maximumFileSize">The maximum file size, in bytes.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="maximumFileSize" /> is less than <paramref name="bufferSize" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="maximumFileSize" /> is a negative number.</exception>
	/// <filterpriority>2</filterpriority>
	public EventSchemaTraceListener(string fileName, string name, int bufferSize, TraceLogRetentionOption logRetentionOption, long maximumFileSize)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.EventSchemaTraceListener" /> class with the specified name and specified buffer size, using the specified file with the specified log retention policy, maximum size, and file count as the recipient of the debugging and tracing output.</summary>
	/// <param name="fileName">The path for the log file.</param>
	/// <param name="name">The name of the listener.</param>
	/// <param name="bufferSize">The size of the output buffer, in bytes.</param>
	/// <param name="logRetentionOption">One of the <see cref="T:System.Diagnostics.TraceLogRetentionOption" /> values.</param>
	/// <param name="maximumFileSize">The maximum file size, in bytes.</param>
	/// <param name="maximumNumberOfFiles">The maximum number of output log files.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="maximumFileSize" /> is less than <paramref name="bufferSize" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="maximumFileSize" /> is a negative number.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="maximumNumberOfFiles" /> is less than 1, and <paramref name="logRetentionOption" /> is <see cref="F:System.Diagnostics.TraceLogRetentionOption.LimitedSequentialFiles" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="maximumNumberOfFiles" /> is less than 2, and <paramref name="logRetentionOption" /> is <see cref="F:System.Diagnostics.TraceLogRetentionOption.LimitedCircularFiles" />.</exception>
	/// <filterpriority>2</filterpriority>
	public EventSchemaTraceListener(string fileName, string name, int bufferSize, TraceLogRetentionOption logRetentionOption, long maximumFileSize, int maximumNumberOfFiles)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
