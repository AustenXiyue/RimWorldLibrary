using System.IO;
using System.Security.Permissions;
using System.Text;

namespace System.Diagnostics;

/// <summary>Directs tracing or debugging output to a <see cref="T:System.IO.TextWriter" /> or to a <see cref="T:System.IO.Stream" />, such as <see cref="T:System.IO.FileStream" />.</summary>
/// <filterpriority>1</filterpriority>
[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
public class TextWriterTraceListener : TraceListener
{
	internal TextWriter writer;

	private string fileName;

	/// <summary>Gets or sets the text writer that receives the tracing or debugging output.</summary>
	/// <returns>A <see cref="T:System.IO.TextWriter" /> that represents the writer that receives the tracing or debugging output.</returns>
	/// <filterpriority>2</filterpriority>
	public TextWriter Writer
	{
		get
		{
			EnsureWriter();
			return writer;
		}
		set
		{
			writer = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.TextWriterTraceListener" /> class with <see cref="T:System.IO.TextWriter" /> as the output recipient.</summary>
	public TextWriterTraceListener()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.TextWriterTraceListener" /> class, using the stream as the recipient of the debugging and tracing output.</summary>
	/// <param name="stream">A <see cref="T:System.IO.Stream" /> that represents the stream the <see cref="T:System.Diagnostics.TextWriterTraceListener" /> writes to. </param>
	/// <exception cref="T:System.ArgumentNullException">The stream is null. </exception>
	public TextWriterTraceListener(Stream stream)
		: this(stream, string.Empty)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.TextWriterTraceListener" /> class with the specified name, using the stream as the recipient of the debugging and tracing output.</summary>
	/// <param name="stream">A <see cref="T:System.IO.Stream" /> that represents the stream the <see cref="T:System.Diagnostics.TextWriterTraceListener" /> writes to. </param>
	/// <param name="name">The name of the new instance. </param>
	/// <exception cref="T:System.ArgumentNullException">The stream is null. </exception>
	public TextWriterTraceListener(Stream stream, string name)
		: base(name)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		writer = new StreamWriter(stream);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.TextWriterTraceListener" /> class using the specified writer as recipient of the tracing or debugging output.</summary>
	/// <param name="writer">A <see cref="T:System.IO.TextWriter" /> that receives the output from the <see cref="T:System.Diagnostics.TextWriterTraceListener" />. </param>
	/// <exception cref="T:System.ArgumentNullException">The writer is null. </exception>
	public TextWriterTraceListener(TextWriter writer)
		: this(writer, string.Empty)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.TextWriterTraceListener" /> class with the specified name, using the specified writer as recipient of the tracing or debugging output.</summary>
	/// <param name="writer">A <see cref="T:System.IO.TextWriter" /> that receives the output from the <see cref="T:System.Diagnostics.TextWriterTraceListener" />. </param>
	/// <param name="name">The name of the new instance. </param>
	/// <exception cref="T:System.ArgumentNullException">The writer is null. </exception>
	public TextWriterTraceListener(TextWriter writer, string name)
		: base(name)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		this.writer = writer;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.TextWriterTraceListener" /> class, using the file as the recipient of the debugging and tracing output.</summary>
	/// <param name="fileName">The name of the file the <see cref="T:System.Diagnostics.TextWriterTraceListener" /> writes to. </param>
	/// <exception cref="T:System.ArgumentNullException">The file is null. </exception>
	public TextWriterTraceListener(string fileName)
	{
		this.fileName = fileName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.TextWriterTraceListener" /> class with the specified name, using the file as the recipient of the debugging and tracing output.</summary>
	/// <param name="fileName">The name of the file the <see cref="T:System.Diagnostics.TextWriterTraceListener" /> writes to. </param>
	/// <param name="name">The name of the new instance. </param>
	/// <exception cref="T:System.ArgumentNullException">The stream is null. </exception>
	public TextWriterTraceListener(string fileName, string name)
		: base(name)
	{
		this.fileName = fileName;
	}

	/// <summary>Closes the <see cref="P:System.Diagnostics.TextWriterTraceListener.Writer" /> so that it no longer receives tracing or debugging output.</summary>
	/// <filterpriority>1</filterpriority>
	public override void Close()
	{
		if (writer != null)
		{
			try
			{
				writer.Close();
			}
			catch (ObjectDisposedException)
			{
			}
		}
		writer = null;
	}

	/// <summary>Disposes this <see cref="T:System.Diagnostics.TextWriterTraceListener" /> object.</summary>
	/// <param name="disposing">true to release managed resources; if false, <see cref="M:System.Diagnostics.TextWriterTraceListener.Dispose(System.Boolean)" /> has no effect.</param>
	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing)
			{
				Close();
				return;
			}
			if (writer != null)
			{
				try
				{
					writer.Close();
				}
				catch (ObjectDisposedException)
				{
				}
			}
			writer = null;
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	/// <summary>Flushes the output buffer for the <see cref="P:System.Diagnostics.TextWriterTraceListener.Writer" />.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override void Flush()
	{
		if (!EnsureWriter())
		{
			return;
		}
		try
		{
			writer.Flush();
		}
		catch (ObjectDisposedException)
		{
		}
	}

	/// <summary>Writes a message to this instance's <see cref="P:System.Diagnostics.TextWriterTraceListener.Writer" />.</summary>
	/// <param name="message">A message to write. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override void Write(string message)
	{
		if (!EnsureWriter())
		{
			return;
		}
		if (base.NeedIndent)
		{
			WriteIndent();
		}
		try
		{
			writer.Write(message);
		}
		catch (ObjectDisposedException)
		{
		}
	}

	/// <summary>Writes a message to this instance's <see cref="P:System.Diagnostics.TextWriterTraceListener.Writer" /> followed by a line terminator. The default line terminator is a carriage return followed by a line feed (\r\n).</summary>
	/// <param name="message">A message to write. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override void WriteLine(string message)
	{
		if (!EnsureWriter())
		{
			return;
		}
		if (base.NeedIndent)
		{
			WriteIndent();
		}
		try
		{
			writer.WriteLine(message);
			base.NeedIndent = true;
		}
		catch (ObjectDisposedException)
		{
		}
	}

	private static Encoding GetEncodingWithFallback(Encoding encoding)
	{
		Encoding obj = (Encoding)encoding.Clone();
		obj.EncoderFallback = EncoderFallback.ReplacementFallback;
		obj.DecoderFallback = DecoderFallback.ReplacementFallback;
		return obj;
	}

	internal bool EnsureWriter()
	{
		bool flag = true;
		if (writer == null)
		{
			flag = false;
			if (fileName == null)
			{
				return flag;
			}
			Encoding encodingWithFallback = GetEncodingWithFallback(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			string path = Path.GetFullPath(fileName);
			string directoryName = Path.GetDirectoryName(path);
			string text = Path.GetFileName(path);
			for (int i = 0; i < 2; i++)
			{
				try
				{
					writer = new StreamWriter(path, append: true, encodingWithFallback, 4096);
					flag = true;
				}
				catch (IOException)
				{
					text = Guid.NewGuid().ToString() + text;
					path = Path.Combine(directoryName, text);
					continue;
				}
				catch (UnauthorizedAccessException)
				{
				}
				catch (Exception)
				{
				}
				break;
			}
			if (!flag)
			{
				fileName = null;
			}
		}
		return flag;
	}
}
