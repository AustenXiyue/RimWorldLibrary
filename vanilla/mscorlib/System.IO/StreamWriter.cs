using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO;

/// <summary>Implements a <see cref="T:System.IO.TextWriter" /> for writing characters to a stream in a particular encoding.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
public class StreamWriter : TextWriter
{
	internal const int DefaultBufferSize = 1024;

	private const int DefaultFileStreamBufferSize = 4096;

	private const int MinBufferSize = 128;

	private const int DontCopyOnWriteLineThreshold = 512;

	/// <summary>Provides a StreamWriter with no backing store that can be written to, but not read from.</summary>
	/// <filterpriority>1</filterpriority>
	public new static readonly StreamWriter Null = new StreamWriter(Stream.Null, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true), 128, leaveOpen: true);

	private Stream stream;

	private Encoding encoding;

	private Encoder encoder;

	private byte[] byteBuffer;

	private char[] charBuffer;

	private int charPos;

	private int charLen;

	private bool autoFlush;

	private bool haveWrittenPreamble;

	private bool closable;

	[NonSerialized]
	private volatile Task _asyncWriteTask;

	private static volatile Encoding _UTF8NoBOM;

	internal static Encoding UTF8NoBOM
	{
		[FriendAccessAllowed]
		get
		{
			if (_UTF8NoBOM == null)
			{
				UTF8Encoding uTF8NoBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
				Thread.MemoryBarrier();
				_UTF8NoBOM = uTF8NoBOM;
			}
			return _UTF8NoBOM;
		}
	}

	/// <summary>Gets or sets a value indicating whether the <see cref="T:System.IO.StreamWriter" /> will flush its buffer to the underlying stream after every call to <see cref="M:System.IO.StreamWriter.Write(System.Char)" />.</summary>
	/// <returns>true to force <see cref="T:System.IO.StreamWriter" /> to flush its buffer; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual bool AutoFlush
	{
		get
		{
			return autoFlush;
		}
		set
		{
			CheckAsyncTaskInProgress();
			autoFlush = value;
			if (value)
			{
				Flush(flushStream: true, flushEncoder: false);
			}
		}
	}

	/// <summary>Gets the underlying stream that interfaces with a backing store.</summary>
	/// <returns>The stream this StreamWriter is writing to.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual Stream BaseStream => stream;

	internal bool LeaveOpen => !closable;

	internal bool HaveWrittenPreamble
	{
		set
		{
			haveWrittenPreamble = value;
		}
	}

	/// <summary>Gets the <see cref="T:System.Text.Encoding" /> in which the output is written.</summary>
	/// <returns>The <see cref="T:System.Text.Encoding" /> specified in the constructor for the current instance, or <see cref="T:System.Text.UTF8Encoding" /> if an encoding was not specified.</returns>
	/// <filterpriority>2</filterpriority>
	public override Encoding Encoding => encoding;

	private int CharPos_Prop
	{
		set
		{
			charPos = value;
		}
	}

	private bool HaveWrittenPreamble_Prop
	{
		set
		{
			haveWrittenPreamble = value;
		}
	}

	private void CheckAsyncTaskInProgress()
	{
		Task asyncWriteTask = _asyncWriteTask;
		if (asyncWriteTask != null && !asyncWriteTask.IsCompleted)
		{
			throw new InvalidOperationException(Environment.GetResourceString("The stream is currently in use by a previous operation on the stream."));
		}
	}

	internal StreamWriter()
		: base(null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamWriter" /> class for the specified stream by using UTF-8 encoding and the default buffer size.</summary>
	/// <param name="stream">The stream to write to. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stream" /> is not writable. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null. </exception>
	public StreamWriter(Stream stream)
		: this(stream, UTF8NoBOM, 1024, leaveOpen: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamWriter" /> class for the specified stream by using the specified encoding and the default buffer size.</summary>
	/// <param name="stream">The stream to write to. </param>
	/// <param name="encoding">The character encoding to use. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> or <paramref name="encoding" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stream" /> is not writable. </exception>
	public StreamWriter(Stream stream, Encoding encoding)
		: this(stream, encoding, 1024, leaveOpen: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamWriter" /> class for the specified stream by using the specified encoding and buffer size.</summary>
	/// <param name="stream">The stream to write to. </param>
	/// <param name="encoding">The character encoding to use. </param>
	/// <param name="bufferSize">The buffer size, in bytes. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> or <paramref name="encoding" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="bufferSize" /> is negative. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stream" /> is not writable. </exception>
	public StreamWriter(Stream stream, Encoding encoding, int bufferSize)
		: this(stream, encoding, bufferSize, leaveOpen: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamWriter" /> class for the specified stream by using the specified encoding and buffer size, and optionally leaves the stream open.</summary>
	/// <param name="stream">The stream to write to.</param>
	/// <param name="encoding">The character encoding to use.</param>
	/// <param name="bufferSize">The buffer size, in bytes.</param>
	/// <param name="leaveOpen">true to leave the stream open after the <see cref="T:System.IO.StreamWriter" /> object is disposed; otherwise, false.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> or <paramref name="encoding" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="bufferSize" /> is negative. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stream" /> is not writable. </exception>
	public StreamWriter(Stream stream, Encoding encoding, int bufferSize, bool leaveOpen)
		: base(null)
	{
		if (stream == null || encoding == null)
		{
			throw new ArgumentNullException((stream == null) ? "stream" : "encoding");
		}
		if (!stream.CanWrite)
		{
			throw new ArgumentException(Environment.GetResourceString("Stream was not writable."));
		}
		if (bufferSize <= 0)
		{
			throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("Positive number required."));
		}
		Init(stream, encoding, bufferSize, leaveOpen);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamWriter" /> class for the specified file by using the default encoding and buffer size.</summary>
	/// <param name="path">The complete file path to write to. <paramref name="path" /> can be a file name. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">Access is denied. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""). -or-<paramref name="path" /> contains the name of a system device (com1, com2, and so on).</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters. </exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="path" /> includes an incorrect or invalid syntax for file name, directory name, or volume label syntax. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public StreamWriter(string path)
		: this(path, append: false, UTF8NoBOM, 1024)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamWriter" /> class for the specified file by using the default encoding and buffer size. If the file exists, it can be either overwritten or appended to. If the file does not exist, this constructor creates a new file.</summary>
	/// <param name="path">The complete file path to write to. </param>
	/// <param name="append">true to append data to the file; false to overwrite the file. If the specified file does not exist, this parameter has no effect, and the constructor creates a new file. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">Access is denied. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is empty. -or-<paramref name="path" /> contains the name of a system device (com1, com2, and so on).</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="path" /> includes an incorrect or invalid syntax for file name, directory name, or volume label syntax. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public StreamWriter(string path, bool append)
		: this(path, append, UTF8NoBOM, 1024)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamWriter" /> class for the specified file by using the specified encoding and default buffer size. If the file exists, it can be either overwritten or appended to. If the file does not exist, this constructor creates a new file.</summary>
	/// <param name="path">The complete file path to write to. </param>
	/// <param name="append">true to append data to the file; false to overwrite the file. If the specified file does not exist, this parameter has no effect, and the constructor creates a new file.</param>
	/// <param name="encoding">The character encoding to use. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">Access is denied. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is empty. -or-<paramref name="path" /> contains the name of a system device (com1, com2, and so on).</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="path" /> includes an incorrect or invalid syntax for file name, directory name, or volume label syntax. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public StreamWriter(string path, bool append, Encoding encoding)
		: this(path, append, encoding, 1024)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamWriter" /> class for the specified file on the specified path, using the specified encoding and buffer size. If the file exists, it can be either overwritten or appended to. If the file does not exist, this constructor creates a new file.</summary>
	/// <param name="path">The complete file path to write to. </param>
	/// <param name="append">true to append data to the file; false to overwrite the file. If the specified file does not exist, this parameter has no effect, and the constructor creates a new file.</param>
	/// <param name="encoding">The character encoding to use. </param>
	/// <param name="bufferSize">The buffer size, in bytes. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""). -or-<paramref name="path" /> contains the name of a system device (com1, com2, and so on).</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> or <paramref name="encoding" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="bufferSize" /> is negative. </exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="path" /> includes an incorrect or invalid syntax for file name, directory name, or volume label syntax. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">Access is denied. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters. </exception>
	[SecuritySafeCritical]
	public StreamWriter(string path, bool append, Encoding encoding, int bufferSize)
		: this(path, append, encoding, bufferSize, checkHost: true)
	{
	}

	[SecurityCritical]
	internal StreamWriter(string path, bool append, Encoding encoding, int bufferSize, bool checkHost)
		: base(null)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException(Environment.GetResourceString("Empty path name is not legal."));
		}
		if (bufferSize <= 0)
		{
			throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("Positive number required."));
		}
		Stream streamArg = CreateFile(path, append, checkHost);
		Init(streamArg, encoding, bufferSize, shouldLeaveOpen: false);
	}

	[SecuritySafeCritical]
	private void Init(Stream streamArg, Encoding encodingArg, int bufferSize, bool shouldLeaveOpen)
	{
		stream = streamArg;
		encoding = encodingArg;
		encoder = encoding.GetEncoder();
		if (bufferSize < 128)
		{
			bufferSize = 128;
		}
		charBuffer = new char[bufferSize];
		byteBuffer = new byte[encoding.GetMaxByteCount(bufferSize)];
		charLen = bufferSize;
		if (stream.CanSeek && stream.Position > 0)
		{
			haveWrittenPreamble = true;
		}
		closable = !shouldLeaveOpen;
	}

	[SecurityCritical]
	private static Stream CreateFile(string path, bool append, bool checkHost)
	{
		FileMode mode = (append ? FileMode.Append : FileMode.Create);
		return new FileStream(path, mode, FileAccess.Write, FileShare.Read, 4096, FileOptions.SequentialScan, Path.GetFileName(path), bFromProxy: false, useLongPath: false, checkHost);
	}

	/// <summary>Closes the current StreamWriter object and the underlying stream.</summary>
	/// <exception cref="T:System.Text.EncoderFallbackException">The current encoding does not support displaying half of a Unicode surrogate pair.</exception>
	/// <filterpriority>1</filterpriority>
	public override void Close()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.StreamWriter" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	/// <exception cref="T:System.Text.EncoderFallbackException">The current encoding does not support displaying half of a Unicode surrogate pair.</exception>
	protected override void Dispose(bool disposing)
	{
		try
		{
			if (stream == null)
			{
				return;
			}
			if (!disposing)
			{
				if (LeaveOpen)
				{
					_ = stream;
				}
			}
			else
			{
				CheckAsyncTaskInProgress();
				Flush(flushStream: true, flushEncoder: true);
			}
		}
		finally
		{
			if (!LeaveOpen && stream != null)
			{
				try
				{
					if (disposing)
					{
						stream.Close();
					}
				}
				finally
				{
					stream = null;
					byteBuffer = null;
					charBuffer = null;
					encoding = null;
					encoder = null;
					charLen = 0;
					base.Dispose(disposing);
				}
			}
		}
	}

	/// <summary>Clears all buffers for the current writer and causes any buffered data to be written to the underlying stream.</summary>
	/// <exception cref="T:System.ObjectDisposedException">The current writer is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error has occurred. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">The current encoding does not support displaying half of a Unicode surrogate pair. </exception>
	/// <filterpriority>1</filterpriority>
	public override void Flush()
	{
		CheckAsyncTaskInProgress();
		Flush(flushStream: true, flushEncoder: true);
	}

	private void Flush(bool flushStream, bool flushEncoder)
	{
		if (stream == null)
		{
			__Error.WriterClosed();
		}
		if (charPos == 0 && ((!flushStream && !flushEncoder) || CompatibilitySwitches.IsAppEarlierThanWindowsPhone8))
		{
			return;
		}
		if (!haveWrittenPreamble)
		{
			haveWrittenPreamble = true;
			byte[] preamble = encoding.GetPreamble();
			if (preamble.Length != 0)
			{
				stream.Write(preamble, 0, preamble.Length);
			}
		}
		int bytes = encoder.GetBytes(charBuffer, 0, charPos, byteBuffer, 0, flushEncoder);
		charPos = 0;
		if (bytes > 0)
		{
			stream.Write(byteBuffer, 0, bytes);
		}
		if (flushStream)
		{
			stream.Flush();
		}
	}

	/// <summary>Writes a character to the stream.</summary>
	/// <param name="value">The character to write to the stream. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="P:System.IO.StreamWriter.AutoFlush" /> is true or the <see cref="T:System.IO.StreamWriter" /> buffer is full, and current writer is closed. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <see cref="P:System.IO.StreamWriter.AutoFlush" /> is true or the <see cref="T:System.IO.StreamWriter" /> buffer is full, and the contents of the buffer cannot be written to the underlying fixed size stream because the <see cref="T:System.IO.StreamWriter" /> is at the end the stream. </exception>
	/// <filterpriority>1</filterpriority>
	public override void Write(char value)
	{
		CheckAsyncTaskInProgress();
		if (charPos == charLen)
		{
			Flush(flushStream: false, flushEncoder: false);
		}
		charBuffer[charPos] = value;
		charPos++;
		if (autoFlush)
		{
			Flush(flushStream: true, flushEncoder: false);
		}
	}

	/// <summary>Writes a character array to the stream.</summary>
	/// <param name="buffer">A character array containing the data to write. If <paramref name="buffer" /> is null, nothing is written. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="P:System.IO.StreamWriter.AutoFlush" /> is true or the <see cref="T:System.IO.StreamWriter" /> buffer is full, and current writer is closed. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <see cref="P:System.IO.StreamWriter.AutoFlush" /> is true or the <see cref="T:System.IO.StreamWriter" /> buffer is full, and the contents of the buffer cannot be written to the underlying fixed size stream because the <see cref="T:System.IO.StreamWriter" /> is at the end the stream. </exception>
	/// <filterpriority>1</filterpriority>
	public override void Write(char[] buffer)
	{
		if (buffer == null)
		{
			return;
		}
		CheckAsyncTaskInProgress();
		int num = 0;
		int num2 = buffer.Length;
		while (num2 > 0)
		{
			if (charPos == charLen)
			{
				Flush(flushStream: false, flushEncoder: false);
			}
			int num3 = charLen - charPos;
			if (num3 > num2)
			{
				num3 = num2;
			}
			Buffer.InternalBlockCopy(buffer, num * 2, charBuffer, charPos * 2, num3 * 2);
			charPos += num3;
			num += num3;
			num2 -= num3;
		}
		if (autoFlush)
		{
			Flush(flushStream: true, flushEncoder: false);
		}
	}

	/// <summary>Writes a subarray of characters to the stream.</summary>
	/// <param name="buffer">A character array that contains the data to write. </param>
	/// <param name="index">The character position in the buffer at which to start reading data. </param>
	/// <param name="count">The maximum number of characters to write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="index" /> is less than <paramref name="count" />. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="P:System.IO.StreamWriter.AutoFlush" /> is true or the <see cref="T:System.IO.StreamWriter" /> buffer is full, and current writer is closed. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <see cref="P:System.IO.StreamWriter.AutoFlush" /> is true or the <see cref="T:System.IO.StreamWriter" /> buffer is full, and the contents of the buffer cannot be written to the underlying fixed size stream because the <see cref="T:System.IO.StreamWriter" /> is at the end the stream. </exception>
	/// <filterpriority>1</filterpriority>
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
		CheckAsyncTaskInProgress();
		while (count > 0)
		{
			if (charPos == charLen)
			{
				Flush(flushStream: false, flushEncoder: false);
			}
			int num = charLen - charPos;
			if (num > count)
			{
				num = count;
			}
			Buffer.InternalBlockCopy(buffer, index * 2, charBuffer, charPos * 2, num * 2);
			charPos += num;
			index += num;
			count -= num;
		}
		if (autoFlush)
		{
			Flush(flushStream: true, flushEncoder: false);
		}
	}

	/// <summary>Writes a string to the stream.</summary>
	/// <param name="value">The string to write to the stream. If <paramref name="value" /> is null, nothing is written. </param>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="P:System.IO.StreamWriter.AutoFlush" /> is true or the <see cref="T:System.IO.StreamWriter" /> buffer is full, and current writer is closed. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <see cref="P:System.IO.StreamWriter.AutoFlush" /> is true or the <see cref="T:System.IO.StreamWriter" /> buffer is full, and the contents of the buffer cannot be written to the underlying fixed size stream because the <see cref="T:System.IO.StreamWriter" /> is at the end the stream. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public override void Write(string value)
	{
		if (value == null)
		{
			return;
		}
		CheckAsyncTaskInProgress();
		int num = value.Length;
		int num2 = 0;
		while (num > 0)
		{
			if (charPos == charLen)
			{
				Flush(flushStream: false, flushEncoder: false);
			}
			int num3 = charLen - charPos;
			if (num3 > num)
			{
				num3 = num;
			}
			value.CopyTo(num2, charBuffer, charPos, num3);
			charPos += num3;
			num2 += num3;
			num -= num3;
		}
		if (autoFlush)
		{
			Flush(flushStream: true, flushEncoder: false);
		}
	}

	/// <summary>Writes a character to the stream asynchronously.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="value">The character to write to the stream.</param>
	/// <exception cref="T:System.ObjectDisposedException">The stream writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The stream writer is currently in use by a previous write operation.</exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public override Task WriteAsync(char value)
	{
		if (GetType() != typeof(StreamWriter))
		{
			return base.WriteAsync(value);
		}
		if (stream == null)
		{
			__Error.WriterClosed();
		}
		CheckAsyncTaskInProgress();
		return _asyncWriteTask = WriteAsyncInternal(this, value, charBuffer, charPos, charLen, CoreNewLine, autoFlush, appendNewLine: false);
	}

	private static async Task WriteAsyncInternal(StreamWriter _this, char value, char[] charBuffer, int charPos, int charLen, char[] coreNewLine, bool autoFlush, bool appendNewLine)
	{
		if (charPos == charLen)
		{
			await _this.FlushAsyncInternal(flushStream: false, flushEncoder: false, charBuffer, charPos).ConfigureAwait(continueOnCapturedContext: false);
			charPos = 0;
		}
		charBuffer[charPos] = value;
		charPos++;
		if (appendNewLine)
		{
			for (int i = 0; i < coreNewLine.Length; i++)
			{
				if (charPos == charLen)
				{
					await _this.FlushAsyncInternal(flushStream: false, flushEncoder: false, charBuffer, charPos).ConfigureAwait(continueOnCapturedContext: false);
					charPos = 0;
				}
				charBuffer[charPos] = coreNewLine[i];
				charPos++;
			}
		}
		if (autoFlush)
		{
			await _this.FlushAsyncInternal(flushStream: true, flushEncoder: false, charBuffer, charPos).ConfigureAwait(continueOnCapturedContext: false);
			charPos = 0;
		}
		_this.CharPos_Prop = charPos;
	}

	/// <summary>Writes a string to the stream asynchronously.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="value">The string to write to the stream. If <paramref name="value" /> is null, nothing is written.</param>
	/// <exception cref="T:System.ObjectDisposedException">The stream writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The stream writer is currently in use by a previous write operation.</exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public override Task WriteAsync(string value)
	{
		if (GetType() != typeof(StreamWriter))
		{
			return base.WriteAsync(value);
		}
		if (value != null)
		{
			if (stream == null)
			{
				__Error.WriterClosed();
			}
			CheckAsyncTaskInProgress();
			return _asyncWriteTask = WriteAsyncInternal(this, value, charBuffer, charPos, charLen, CoreNewLine, autoFlush, appendNewLine: false);
		}
		return Task.CompletedTask;
	}

	private static async Task WriteAsyncInternal(StreamWriter _this, string value, char[] charBuffer, int charPos, int charLen, char[] coreNewLine, bool autoFlush, bool appendNewLine)
	{
		int count = value.Length;
		int index = 0;
		while (count > 0)
		{
			if (charPos == charLen)
			{
				await _this.FlushAsyncInternal(flushStream: false, flushEncoder: false, charBuffer, charPos).ConfigureAwait(continueOnCapturedContext: false);
				charPos = 0;
			}
			int num = charLen - charPos;
			if (num > count)
			{
				num = count;
			}
			value.CopyTo(index, charBuffer, charPos, num);
			charPos += num;
			index += num;
			count -= num;
		}
		if (appendNewLine)
		{
			for (int i = 0; i < coreNewLine.Length; i++)
			{
				if (charPos == charLen)
				{
					await _this.FlushAsyncInternal(flushStream: false, flushEncoder: false, charBuffer, charPos).ConfigureAwait(continueOnCapturedContext: false);
					charPos = 0;
				}
				charBuffer[charPos] = coreNewLine[i];
				charPos++;
			}
		}
		if (autoFlush)
		{
			await _this.FlushAsyncInternal(flushStream: true, flushEncoder: false, charBuffer, charPos).ConfigureAwait(continueOnCapturedContext: false);
			charPos = 0;
		}
		_this.CharPos_Prop = charPos;
	}

	/// <summary>Writes a subarray of characters to the stream asynchronously.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="buffer">A character array that contains the data to write.</param>
	/// <param name="index">The character position in the buffer at which to begin reading data.</param>
	/// <param name="count">The maximum number of characters to write.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="index" /> plus <paramref name="count" /> is greater than the buffer length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The stream writer is currently in use by a previous write operation. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public override Task WriteAsync(char[] buffer, int index, int count)
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
		if (GetType() != typeof(StreamWriter))
		{
			return base.WriteAsync(buffer, index, count);
		}
		if (stream == null)
		{
			__Error.WriterClosed();
		}
		CheckAsyncTaskInProgress();
		return _asyncWriteTask = WriteAsyncInternal(this, buffer, index, count, charBuffer, charPos, charLen, CoreNewLine, autoFlush, appendNewLine: false);
	}

	private static async Task WriteAsyncInternal(StreamWriter _this, char[] buffer, int index, int count, char[] charBuffer, int charPos, int charLen, char[] coreNewLine, bool autoFlush, bool appendNewLine)
	{
		while (count > 0)
		{
			if (charPos == charLen)
			{
				await _this.FlushAsyncInternal(flushStream: false, flushEncoder: false, charBuffer, charPos).ConfigureAwait(continueOnCapturedContext: false);
				charPos = 0;
			}
			int num = charLen - charPos;
			if (num > count)
			{
				num = count;
			}
			Buffer.InternalBlockCopy(buffer, index * 2, charBuffer, charPos * 2, num * 2);
			charPos += num;
			index += num;
			count -= num;
		}
		if (appendNewLine)
		{
			for (int i = 0; i < coreNewLine.Length; i++)
			{
				if (charPos == charLen)
				{
					await _this.FlushAsyncInternal(flushStream: false, flushEncoder: false, charBuffer, charPos).ConfigureAwait(continueOnCapturedContext: false);
					charPos = 0;
				}
				charBuffer[charPos] = coreNewLine[i];
				charPos++;
			}
		}
		if (autoFlush)
		{
			await _this.FlushAsyncInternal(flushStream: true, flushEncoder: false, charBuffer, charPos).ConfigureAwait(continueOnCapturedContext: false);
			charPos = 0;
		}
		_this.CharPos_Prop = charPos;
	}

	/// <summary>Writes a line terminator asynchronously to the stream.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The stream writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The stream writer is currently in use by a previous write operation.</exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public override Task WriteLineAsync()
	{
		if (GetType() != typeof(StreamWriter))
		{
			return base.WriteLineAsync();
		}
		if (stream == null)
		{
			__Error.WriterClosed();
		}
		CheckAsyncTaskInProgress();
		return _asyncWriteTask = WriteAsyncInternal(this, null, 0, 0, charBuffer, charPos, charLen, CoreNewLine, autoFlush, appendNewLine: true);
	}

	/// <summary>Writes a character followed by a line terminator asynchronously to the stream.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="value">The character to write to the stream.</param>
	/// <exception cref="T:System.ObjectDisposedException">The stream writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The stream writer is currently in use by a previous write operation.</exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public override Task WriteLineAsync(char value)
	{
		if (GetType() != typeof(StreamWriter))
		{
			return base.WriteLineAsync(value);
		}
		if (stream == null)
		{
			__Error.WriterClosed();
		}
		CheckAsyncTaskInProgress();
		return _asyncWriteTask = WriteAsyncInternal(this, value, charBuffer, charPos, charLen, CoreNewLine, autoFlush, appendNewLine: true);
	}

	/// <summary>Writes a string followed by a line terminator asynchronously to the stream.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="value">The string to write. If the value is null, only a line terminator is written. </param>
	/// <exception cref="T:System.ObjectDisposedException">The stream writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The stream writer is currently in use by a previous write operation.</exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public override Task WriteLineAsync(string value)
	{
		if (GetType() != typeof(StreamWriter))
		{
			return base.WriteLineAsync(value);
		}
		if (stream == null)
		{
			__Error.WriterClosed();
		}
		CheckAsyncTaskInProgress();
		return _asyncWriteTask = WriteAsyncInternal(this, value ?? "", charBuffer, charPos, charLen, CoreNewLine, autoFlush, appendNewLine: true);
	}

	/// <summary>Writes a subarray of characters followed by a line terminator asynchronously to the stream.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="buffer">The character array to write data from.</param>
	/// <param name="index">The character position in the buffer at which to start reading data.</param>
	/// <param name="count">The maximum number of characters to write.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="index" /> plus <paramref name="count" /> is greater than the buffer length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The stream writer is currently in use by a previous write operation. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public override Task WriteLineAsync(char[] buffer, int index, int count)
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
		if (GetType() != typeof(StreamWriter))
		{
			return base.WriteLineAsync(buffer, index, count);
		}
		if (stream == null)
		{
			__Error.WriterClosed();
		}
		CheckAsyncTaskInProgress();
		return _asyncWriteTask = WriteAsyncInternal(this, buffer, index, count, charBuffer, charPos, charLen, CoreNewLine, autoFlush, appendNewLine: true);
	}

	/// <summary>Clears all buffers for this stream asynchronously and causes any buffered data to be written to the underlying device.</summary>
	/// <returns>A task that represents the asynchronous flush operation.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public override Task FlushAsync()
	{
		if (GetType() != typeof(StreamWriter))
		{
			return base.FlushAsync();
		}
		if (stream == null)
		{
			__Error.WriterClosed();
		}
		CheckAsyncTaskInProgress();
		return _asyncWriteTask = FlushAsyncInternal(flushStream: true, flushEncoder: true, charBuffer, charPos);
	}

	private Task FlushAsyncInternal(bool flushStream, bool flushEncoder, char[] sCharBuffer, int sCharPos)
	{
		if (sCharPos == 0 && !flushStream && !flushEncoder)
		{
			return Task.CompletedTask;
		}
		Task result = FlushAsyncInternal(this, flushStream, flushEncoder, sCharBuffer, sCharPos, haveWrittenPreamble, encoding, encoder, byteBuffer, stream);
		charPos = 0;
		return result;
	}

	private static async Task FlushAsyncInternal(StreamWriter _this, bool flushStream, bool flushEncoder, char[] charBuffer, int charPos, bool haveWrittenPreamble, Encoding encoding, Encoder encoder, byte[] byteBuffer, Stream stream)
	{
		if (!haveWrittenPreamble)
		{
			_this.HaveWrittenPreamble_Prop = true;
			byte[] preamble = encoding.GetPreamble();
			if (preamble.Length != 0)
			{
				await stream.WriteAsync(preamble, 0, preamble.Length).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		int bytes = encoder.GetBytes(charBuffer, 0, charPos, byteBuffer, 0, flushEncoder);
		if (bytes > 0)
		{
			await stream.WriteAsync(byteBuffer, 0, bytes).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (flushStream)
		{
			await stream.FlushAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
	}
}
