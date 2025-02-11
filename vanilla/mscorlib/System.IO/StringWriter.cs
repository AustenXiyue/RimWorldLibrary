using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace System.IO;

/// <summary>Implements a <see cref="T:System.IO.TextWriter" /> for writing information to a string. The information is stored in an underlying <see cref="T:System.Text.StringBuilder" />.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public class StringWriter : TextWriter
{
	private static volatile UnicodeEncoding m_encoding;

	private StringBuilder _sb;

	private bool _isOpen;

	/// <summary>Gets the <see cref="T:System.Text.Encoding" /> in which the output is written.</summary>
	/// <returns>The Encoding in which the output is written.</returns>
	/// <filterpriority>1</filterpriority>
	public override Encoding Encoding
	{
		get
		{
			if (m_encoding == null)
			{
				m_encoding = new UnicodeEncoding(bigEndian: false, byteOrderMark: false);
			}
			return m_encoding;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StringWriter" /> class.</summary>
	public StringWriter()
		: this(new StringBuilder(), CultureInfo.CurrentCulture)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StringWriter" /> class with the specified format control.</summary>
	/// <param name="formatProvider">An <see cref="T:System.IFormatProvider" /> object that controls formatting. </param>
	public StringWriter(IFormatProvider formatProvider)
		: this(new StringBuilder(), formatProvider)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StringWriter" /> class that writes to the specified <see cref="T:System.Text.StringBuilder" />.</summary>
	/// <param name="sb">The StringBuilder to write to. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sb" /> is null. </exception>
	public StringWriter(StringBuilder sb)
		: this(sb, CultureInfo.CurrentCulture)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StringWriter" /> class that writes to the specified <see cref="T:System.Text.StringBuilder" /> and has the specified format provider.</summary>
	/// <param name="sb">The StringBuilder to write to. </param>
	/// <param name="formatProvider">An <see cref="T:System.IFormatProvider" /> object that controls formatting. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sb" /> is null. </exception>
	public StringWriter(StringBuilder sb, IFormatProvider formatProvider)
		: base(formatProvider)
	{
		if (sb == null)
		{
			throw new ArgumentNullException("sb", Environment.GetResourceString("Buffer cannot be null."));
		}
		_sb = sb;
		_isOpen = true;
	}

	/// <summary>Closes the current <see cref="T:System.IO.StringWriter" /> and the underlying stream.</summary>
	/// <filterpriority>1</filterpriority>
	public override void Close()
	{
		Dispose(disposing: true);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.StringWriter" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected override void Dispose(bool disposing)
	{
		_isOpen = false;
		base.Dispose(disposing);
	}

	/// <summary>Returns the underlying <see cref="T:System.Text.StringBuilder" />.</summary>
	/// <returns>The underlying StringBuilder.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual StringBuilder GetStringBuilder()
	{
		return _sb;
	}

	/// <summary>Writes a character to the string.</summary>
	/// <param name="value">The character to write. </param>
	/// <exception cref="T:System.ObjectDisposedException">The writer is closed. </exception>
	/// <filterpriority>2</filterpriority>
	public override void Write(char value)
	{
		if (!_isOpen)
		{
			__Error.WriterClosed();
		}
		_sb.Append(value);
	}

	/// <summary>Writes a subarray of characters to the string.</summary>
	/// <param name="buffer">The character array to write data from. </param>
	/// <param name="index">The position in the buffer at which to start reading data.</param>
	/// <param name="count">The maximum number of characters to write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.ArgumentException">(<paramref name="index" /> + <paramref name="count" />)&gt; <paramref name="buffer" />. Length. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The writer is closed. </exception>
	/// <filterpriority>2</filterpriority>
	public override void Write(char[] buffer, int index, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", Environment.GetResourceString("Buffer cannot be null."));
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Non-negative number required."));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		if (buffer.Length - index < count)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		if (!_isOpen)
		{
			__Error.WriterClosed();
		}
		_sb.Append(buffer, index, count);
	}

	/// <summary>Writes a string to the current string.</summary>
	/// <param name="value">The string to write. </param>
	/// <exception cref="T:System.ObjectDisposedException">The writer is closed. </exception>
	/// <filterpriority>2</filterpriority>
	public override void Write(string value)
	{
		if (!_isOpen)
		{
			__Error.WriterClosed();
		}
		if (value != null)
		{
			_sb.Append(value);
		}
	}

	/// <summary>Writes a character to the string asynchronously.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="value">The character to write to the string.</param>
	/// <exception cref="T:System.ObjectDisposedException">The string writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The string writer is currently in use by a previous write operation. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public override Task WriteAsync(char value)
	{
		Write(value);
		return Task.CompletedTask;
	}

	/// <summary>Writes a string to the current string asynchronously.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="value">The string to write. If <paramref name="value" /> is null, nothing is written to the text stream.</param>
	/// <exception cref="T:System.ObjectDisposedException">The string writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The string writer is currently in use by a previous write operation. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public override Task WriteAsync(string value)
	{
		Write(value);
		return Task.CompletedTask;
	}

	/// <summary>Writes a subarray of characters to the string asynchronously.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="buffer">The character array to write data from.</param>
	/// <param name="index">The position in the buffer at which to start reading data.</param>
	/// <param name="count">The maximum number of characters to write.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="index" /> plus <paramref name="count" /> is greater than the buffer length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The string writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The string writer is currently in use by a previous write operation. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public override Task WriteAsync(char[] buffer, int index, int count)
	{
		Write(buffer, index, count);
		return Task.CompletedTask;
	}

	/// <summary>Writes a character followed by a line terminator asynchronously to the string.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="value">The character to write to the string.</param>
	/// <exception cref="T:System.ObjectDisposedException">The string writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The string writer is currently in use by a previous write operation. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public override Task WriteLineAsync(char value)
	{
		WriteLine(value);
		return Task.CompletedTask;
	}

	/// <summary>Writes a string followed by a line terminator asynchronously to the current string.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="value">The string to write. If the value is null, only a line terminator is written.</param>
	/// <exception cref="T:System.ObjectDisposedException">The string writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The string writer is currently in use by a previous write operation. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public override Task WriteLineAsync(string value)
	{
		WriteLine(value);
		return Task.CompletedTask;
	}

	/// <summary>Writes a subarray of characters followed by a line terminator asynchronously to the string.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="buffer">The character array to write data from.</param>
	/// <param name="index">The position in the buffer at which to start reading data.</param>
	/// <param name="count">The maximum number of characters to write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="index" /> plus <paramref name="count" /> is greater than the buffer length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The string writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The string writer is currently in use by a previous write operation. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public override Task WriteLineAsync(char[] buffer, int index, int count)
	{
		WriteLine(buffer, index, count);
		return Task.CompletedTask;
	}

	/// <summary>Asynchronously clears all buffers for the current writer and causes any buffered data to be written to the underlying device. </summary>
	/// <returns>A task that represents the asynchronous flush operation.</returns>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public override Task FlushAsync()
	{
		return Task.CompletedTask;
	}

	/// <summary>Returns a string containing the characters written to the current StringWriter so far.</summary>
	/// <returns>The string containing the characters written to the current StringWriter.</returns>
	/// <filterpriority>2</filterpriority>
	public override string ToString()
	{
		return _sb.ToString();
	}
}
