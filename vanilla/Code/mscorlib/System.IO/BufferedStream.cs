using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO;

/// <summary>Adds a buffering layer to read and write operations on another stream. This class cannot be inherited.</summary>
/// <filterpriority>2</filterpriority>
[ComVisible(true)]
public sealed class BufferedStream : Stream
{
	private const int _DefaultBufferSize = 4096;

	private Stream _stream;

	private byte[] _buffer;

	private readonly int _bufferSize;

	private int _readPos;

	private int _readLen;

	private int _writePos;

	private BeginEndAwaitableAdapter _beginEndAwaitable;

	private Task<int> _lastSyncCompletedReadTask;

	private const int MaxShadowBufferSize = 81920;

	internal Stream UnderlyingStream
	{
		[FriendAccessAllowed]
		get
		{
			return _stream;
		}
	}

	internal int BufferSize
	{
		[FriendAccessAllowed]
		get
		{
			return _bufferSize;
		}
	}

	/// <summary>Gets a value indicating whether the current stream supports reading.</summary>
	/// <returns>true if the stream supports reading; false if the stream is closed or was opened with write-only access.</returns>
	/// <filterpriority>2</filterpriority>
	public override bool CanRead
	{
		get
		{
			if (_stream != null)
			{
				return _stream.CanRead;
			}
			return false;
		}
	}

	/// <summary>Gets a value indicating whether the current stream supports writing.</summary>
	/// <returns>true if the stream supports writing; false if the stream is closed or was opened with read-only access.</returns>
	/// <filterpriority>2</filterpriority>
	public override bool CanWrite
	{
		get
		{
			if (_stream != null)
			{
				return _stream.CanWrite;
			}
			return false;
		}
	}

	/// <summary>Gets a value indicating whether the current stream supports seeking.</summary>
	/// <returns>true if the stream supports seeking; false if the stream is closed or if the stream was constructed from an operating system handle such as a pipe or output to the console.</returns>
	/// <filterpriority>2</filterpriority>
	public override bool CanSeek
	{
		get
		{
			if (_stream != null)
			{
				return _stream.CanSeek;
			}
			return false;
		}
	}

	/// <summary>Gets the stream length in bytes.</summary>
	/// <returns>The stream length in bytes.</returns>
	/// <exception cref="T:System.IO.IOException">The underlying stream is null or closed. </exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception>
	/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
	/// <filterpriority>2</filterpriority>
	public override long Length
	{
		get
		{
			EnsureNotClosed();
			if (_writePos > 0)
			{
				FlushWrite();
			}
			return _stream.Length;
		}
	}

	/// <summary>Gets the position within the current stream.</summary>
	/// <returns>The position within the current stream.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value passed to <see cref="M:System.IO.BufferedStream.Seek(System.Int64,System.IO.SeekOrigin)" /> is negative. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs, such as the stream being closed. </exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception>
	/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
	/// <filterpriority>2</filterpriority>
	public override long Position
	{
		get
		{
			EnsureNotClosed();
			EnsureCanSeek();
			return _stream.Position + (_readPos - _readLen + _writePos);
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("Non-negative number required."));
			}
			EnsureNotClosed();
			EnsureCanSeek();
			if (_writePos > 0)
			{
				FlushWrite();
			}
			_readPos = 0;
			_readLen = 0;
			_stream.Seek(value, SeekOrigin.Begin);
		}
	}

	private BufferedStream()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.BufferedStream" /> class with a default buffer size of 4096 bytes.</summary>
	/// <param name="stream">The current stream. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null. </exception>
	public BufferedStream(Stream stream)
		: this(stream, 4096)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.BufferedStream" /> class with the specified buffer size.</summary>
	/// <param name="stream">The current stream. </param>
	/// <param name="bufferSize">The buffer size in bytes. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="bufferSize" /> is negative. </exception>
	public BufferedStream(Stream stream, int bufferSize)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (bufferSize <= 0)
		{
			throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("'{0}' must be greater than zero.", "bufferSize"));
		}
		_stream = stream;
		_bufferSize = bufferSize;
		if (!_stream.CanRead && !_stream.CanWrite)
		{
			__Error.StreamIsClosed();
		}
	}

	private void EnsureNotClosed()
	{
		if (_stream == null)
		{
			__Error.StreamIsClosed();
		}
	}

	private void EnsureCanSeek()
	{
		if (!_stream.CanSeek)
		{
			__Error.SeekNotSupported();
		}
	}

	private void EnsureCanRead()
	{
		if (!_stream.CanRead)
		{
			__Error.ReadNotSupported();
		}
	}

	private void EnsureCanWrite()
	{
		if (!_stream.CanWrite)
		{
			__Error.WriteNotSupported();
		}
	}

	private void EnsureBeginEndAwaitableAllocated()
	{
		if (_beginEndAwaitable == null)
		{
			_beginEndAwaitable = new BeginEndAwaitableAdapter();
		}
	}

	private void EnsureShadowBufferAllocated()
	{
		if (_buffer.Length == _bufferSize && _bufferSize < 81920)
		{
			byte[] array = new byte[Math.Min(_bufferSize + _bufferSize, 81920)];
			Buffer.InternalBlockCopy(_buffer, 0, array, 0, _writePos);
			_buffer = array;
		}
	}

	private void EnsureBufferAllocated()
	{
		if (_buffer == null)
		{
			_buffer = new byte[_bufferSize];
		}
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && _stream != null)
			{
				try
				{
					Flush();
					return;
				}
				finally
				{
					_stream.Close();
				}
			}
		}
		finally
		{
			_stream = null;
			_buffer = null;
			_lastSyncCompletedReadTask = null;
			base.Dispose(disposing);
		}
	}

	/// <summary>Clears all buffers for this stream and causes any buffered data to be written to the underlying device.</summary>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	/// <exception cref="T:System.IO.IOException">The data source or repository is not open. </exception>
	/// <filterpriority>2</filterpriority>
	public override void Flush()
	{
		EnsureNotClosed();
		if (_writePos > 0)
		{
			FlushWrite();
		}
		else if (_readPos < _readLen)
		{
			if (_stream.CanSeek)
			{
				FlushRead();
				if (_stream.CanWrite || _stream is BufferedStream)
				{
					_stream.Flush();
				}
			}
		}
		else
		{
			if (_stream.CanWrite || _stream is BufferedStream)
			{
				_stream.Flush();
			}
			_writePos = (_readPos = (_readLen = 0));
		}
	}

	/// <summary>Asynchronously clears all buffers for this stream, causes any buffered data to be written to the underlying device, and monitors cancellation requests.</summary>
	/// <returns>A task that represents the asynchronous flush operation.</returns>
	/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	public override Task FlushAsync(CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCancellation<int>(cancellationToken);
		}
		EnsureNotClosed();
		return FlushAsyncInternal(cancellationToken, this, _stream, _writePos, _readPos, _readLen);
	}

	private static async Task FlushAsyncInternal(CancellationToken cancellationToken, BufferedStream _this, Stream stream, int writePos, int readPos, int readLen)
	{
		SemaphoreSlim sem = _this.EnsureAsyncActiveSemaphoreInitialized();
		await sem.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			if (writePos > 0)
			{
				await _this.FlushWriteAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			else if (readPos < readLen)
			{
				if (stream.CanSeek)
				{
					_this.FlushRead();
					if (stream.CanRead || stream is BufferedStream)
					{
						await stream.FlushAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					}
				}
			}
			else if (stream.CanWrite || stream is BufferedStream)
			{
				await stream.FlushAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		finally
		{
			sem.Release();
		}
	}

	private void FlushRead()
	{
		if (_readPos - _readLen != 0)
		{
			_stream.Seek(_readPos - _readLen, SeekOrigin.Current);
		}
		_readPos = 0;
		_readLen = 0;
	}

	private void ClearReadBufferBeforeWrite()
	{
		if (_readPos == _readLen)
		{
			_readPos = (_readLen = 0);
			return;
		}
		if (!_stream.CanSeek)
		{
			throw new NotSupportedException(Environment.GetResourceString("Cannot write to a BufferedStream while the read buffer is not empty if the underlying stream is not seekable. Ensure that the stream underlying this BufferedStream can seek or avoid interleaving read and write operations on this BufferedStream."));
		}
		FlushRead();
	}

	private void FlushWrite()
	{
		_stream.Write(_buffer, 0, _writePos);
		_writePos = 0;
		_stream.Flush();
	}

	private async Task FlushWriteAsync(CancellationToken cancellationToken)
	{
		await _stream.WriteAsync(_buffer, 0, _writePos, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		_writePos = 0;
		await _stream.FlushAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private int ReadFromBuffer(byte[] array, int offset, int count)
	{
		int num = _readLen - _readPos;
		if (num == 0)
		{
			return 0;
		}
		if (num > count)
		{
			num = count;
		}
		Buffer.InternalBlockCopy(_buffer, _readPos, array, offset, num);
		_readPos += num;
		return num;
	}

	private int ReadFromBuffer(byte[] array, int offset, int count, out Exception error)
	{
		try
		{
			error = null;
			return ReadFromBuffer(array, offset, count);
		}
		catch (Exception ex)
		{
			error = ex;
			return 0;
		}
	}

	/// <summary>Copies bytes from the current buffered stream to an array.</summary>
	/// <returns>The total number of bytes read into <paramref name="array" />. This can be less than the number of bytes requested if that many bytes are not currently available, or 0 if the end of the stream has been reached before any data can be read.</returns>
	/// <param name="array">The buffer to which bytes are to be copied. </param>
	/// <param name="offset">The byte offset in the buffer at which to begin reading bytes. </param>
	/// <param name="count">The number of bytes to be read. </param>
	/// <exception cref="T:System.ArgumentException">Length of <paramref name="array" /> minus <paramref name="offset" /> is less than <paramref name="count" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.IO.IOException">The stream is not open or is null. </exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
	/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
	/// <filterpriority>2</filterpriority>
	public override int Read([In][Out] byte[] array, int offset, int count)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array", Environment.GetResourceString("Buffer cannot be null."));
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("Non-negative number required."));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		if (array.Length - offset < count)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		EnsureNotClosed();
		EnsureCanRead();
		int num = ReadFromBuffer(array, offset, count);
		if (num == count)
		{
			return num;
		}
		int num2 = num;
		if (num > 0)
		{
			count -= num;
			offset += num;
		}
		_readPos = (_readLen = 0);
		if (_writePos > 0)
		{
			FlushWrite();
		}
		if (count >= _bufferSize)
		{
			return _stream.Read(array, offset, count) + num2;
		}
		EnsureBufferAllocated();
		_readLen = _stream.Read(_buffer, 0, _bufferSize);
		num = ReadFromBuffer(array, offset, count);
		return num + num2;
	}

	/// <summary>Begins an asynchronous read operation. (Consider using <see cref="M:System.IO.BufferedStream.ReadAsync(System.Byte[],System.Int32,System.Int32,System.Threading.CancellationToken)" /> instead; see the Remarks section.)</summary>
	/// <returns>An object that represents the asynchronous read, which could still be pending.</returns>
	/// <param name="buffer">The buffer to read the data into.</param>
	/// <param name="offset">The byte offset in <paramref name="buffer" /> at which to begin writing data read from the stream.</param>
	/// <param name="count">The maximum number of bytes to read.</param>
	/// <param name="callback">An optional asynchronous callback, to be called when the read is complete.</param>
	/// <param name="state">A user-provided object that distinguishes this particular asynchronous read request from other requests.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
	/// <exception cref="T:System.IO.IOException">Attempted an asynchronous read past the end of the stream.</exception>
	/// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="offset" /> is less than <paramref name="count" />. </exception>
	/// <exception cref="T:System.NotSupportedException">The current stream does not support the read operation. </exception>
	public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", Environment.GetResourceString("Buffer cannot be null."));
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("Non-negative number required."));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		if (buffer.Length - offset < count)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		if (_stream == null)
		{
			__Error.ReadNotSupported();
		}
		EnsureCanRead();
		int num = 0;
		SemaphoreSlim semaphoreSlim = EnsureAsyncActiveSemaphoreInitialized();
		Task task = semaphoreSlim.WaitAsync();
		if (task.Status == TaskStatus.RanToCompletion)
		{
			bool flag = true;
			try
			{
				num = ReadFromBuffer(buffer, offset, count, out var error);
				flag = num == count || error != null;
				if (flag)
				{
					SynchronousAsyncResult synchronousAsyncResult = ((error == null) ? new SynchronousAsyncResult(num, state) : new SynchronousAsyncResult(error, state, isWrite: false));
					callback?.Invoke(synchronousAsyncResult);
					return synchronousAsyncResult;
				}
			}
			finally
			{
				if (flag)
				{
					semaphoreSlim.Release();
				}
			}
		}
		return BeginReadFromUnderlyingStream(buffer, offset + num, count - num, callback, state, num, task);
	}

	private IAsyncResult BeginReadFromUnderlyingStream(byte[] buffer, int offset, int count, AsyncCallback callback, object state, int bytesAlreadySatisfied, Task semaphoreLockTask)
	{
		return TaskToApm.Begin(ReadFromUnderlyingStreamAsync(buffer, offset, count, CancellationToken.None, bytesAlreadySatisfied, semaphoreLockTask, useApmPattern: true), callback, state);
	}

	/// <summary>Waits for the pending asynchronous read operation to complete. (Consider using <see cref="M:System.IO.BufferedStream.ReadAsync(System.Byte[],System.Int32,System.Int32,System.Threading.CancellationToken)" /> instead; see the Remarks section.)</summary>
	/// <returns>The number of bytes read from the stream, between 0 (zero) and the number of bytes you requested. Streams only return 0 only at the end of the stream, otherwise, they should block until at least 1 byte is available.</returns>
	/// <param name="asyncResult">The reference to the pending asynchronous request to wait for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">This <see cref="T:System.IAsyncResult" /> object was not created by calling <see cref="M:System.IO.BufferedStream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" /> on this class. </exception>
	public override int EndRead(IAsyncResult asyncResult)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (asyncResult is SynchronousAsyncResult)
		{
			return SynchronousAsyncResult.EndRead(asyncResult);
		}
		return TaskToApm.End<int>(asyncResult);
	}

	private Task<int> LastSyncCompletedReadTask(int val)
	{
		Task<int> lastSyncCompletedReadTask = _lastSyncCompletedReadTask;
		if (lastSyncCompletedReadTask != null && lastSyncCompletedReadTask.Result == val)
		{
			return lastSyncCompletedReadTask;
		}
		return _lastSyncCompletedReadTask = Task.FromResult(val);
	}

	/// <summary>Asynchronously reads a sequence of bytes from the current stream, advances the position within the stream by the number of bytes read, and monitors cancellation requests.</summary>
	/// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached. </returns>
	/// <param name="buffer">The buffer to write the data into.</param>
	/// <param name="offset">The byte offset in <paramref name="buffer" /> at which to begin writing data from the stream.</param>
	/// <param name="count">The maximum number of bytes to read.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
	/// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length.</exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The stream is currently in use by a previous read operation. </exception>
	public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", Environment.GetResourceString("Buffer cannot be null."));
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("Non-negative number required."));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		if (buffer.Length - offset < count)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCancellation<int>(cancellationToken);
		}
		EnsureNotClosed();
		EnsureCanRead();
		int num = 0;
		SemaphoreSlim semaphoreSlim = EnsureAsyncActiveSemaphoreInitialized();
		Task task = semaphoreSlim.WaitAsync();
		if (task.Status == TaskStatus.RanToCompletion)
		{
			bool flag = true;
			try
			{
				num = ReadFromBuffer(buffer, offset, count, out var error);
				flag = num == count || error != null;
				if (flag)
				{
					return (error == null) ? LastSyncCompletedReadTask(num) : Task.FromException<int>(error);
				}
			}
			finally
			{
				if (flag)
				{
					semaphoreSlim.Release();
				}
			}
		}
		return ReadFromUnderlyingStreamAsync(buffer, offset + num, count - num, cancellationToken, num, task, useApmPattern: false);
	}

	private async Task<int> ReadFromUnderlyingStreamAsync(byte[] array, int offset, int count, CancellationToken cancellationToken, int bytesAlreadySatisfied, Task semaphoreLockTask, bool useApmPattern)
	{
		await semaphoreLockTask.ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			int num = ReadFromBuffer(array, offset, count);
			if (num == count)
			{
				return bytesAlreadySatisfied + num;
			}
			if (num > 0)
			{
				count -= num;
				offset += num;
				bytesAlreadySatisfied += num;
			}
			_readPos = (_readLen = 0);
			if (_writePos > 0)
			{
				await FlushWriteAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			if (count >= _bufferSize)
			{
				int num2;
				if (useApmPattern)
				{
					EnsureBeginEndAwaitableAllocated();
					_stream.BeginRead(array, offset, count, BeginEndAwaitableAdapter.Callback, _beginEndAwaitable);
					num2 = bytesAlreadySatisfied;
					Stream stream = _stream;
					return num2 + stream.EndRead(await _beginEndAwaitable);
				}
				num2 = bytesAlreadySatisfied;
				return num2 + await _stream.ReadAsync(array, offset, count, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			EnsureBufferAllocated();
			if (useApmPattern)
			{
				EnsureBeginEndAwaitableAllocated();
				_stream.BeginRead(_buffer, 0, _bufferSize, BeginEndAwaitableAdapter.Callback, _beginEndAwaitable);
				Stream stream = _stream;
				_readLen = stream.EndRead(await _beginEndAwaitable);
			}
			else
			{
				_readLen = await _stream.ReadAsync(_buffer, 0, _bufferSize, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			num = ReadFromBuffer(array, offset, count);
			return bytesAlreadySatisfied + num;
		}
		finally
		{
			EnsureAsyncActiveSemaphoreInitialized().Release();
		}
	}

	/// <summary>Reads a byte from the underlying stream and returns the byte cast to an int, or returns -1 if reading from the end of the stream.</summary>
	/// <returns>The byte cast to an int, or -1 if reading from the end of the stream.</returns>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs, such as the stream being closed. </exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
	/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
	/// <filterpriority>2</filterpriority>
	public override int ReadByte()
	{
		EnsureNotClosed();
		EnsureCanRead();
		if (_readPos == _readLen)
		{
			if (_writePos > 0)
			{
				FlushWrite();
			}
			EnsureBufferAllocated();
			_readLen = _stream.Read(_buffer, 0, _bufferSize);
			_readPos = 0;
		}
		if (_readPos == _readLen)
		{
			return -1;
		}
		return _buffer[_readPos++];
	}

	private void WriteToBuffer(byte[] array, ref int offset, ref int count)
	{
		int num = Math.Min(_bufferSize - _writePos, count);
		if (num > 0)
		{
			EnsureBufferAllocated();
			Buffer.InternalBlockCopy(array, offset, _buffer, _writePos, num);
			_writePos += num;
			count -= num;
			offset += num;
		}
	}

	private void WriteToBuffer(byte[] array, ref int offset, ref int count, out Exception error)
	{
		try
		{
			error = null;
			WriteToBuffer(array, ref offset, ref count);
		}
		catch (Exception ex)
		{
			error = ex;
		}
	}

	/// <summary>Copies bytes to the buffered stream and advances the current position within the buffered stream by the number of bytes written.</summary>
	/// <param name="array">The byte array from which to copy <paramref name="count" /> bytes to the current buffered stream. </param>
	/// <param name="offset">The offset in the buffer at which to begin copying bytes to the current buffered stream. </param>
	/// <param name="count">The number of bytes to be written to the current buffered stream. </param>
	/// <exception cref="T:System.ArgumentException">Length of <paramref name="array" /> minus <paramref name="offset" /> is less than <paramref name="count" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.IO.IOException">The stream is closed or null. </exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support writing. </exception>
	/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
	/// <filterpriority>2</filterpriority>
	public override void Write(byte[] array, int offset, int count)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array", Environment.GetResourceString("Buffer cannot be null."));
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("Non-negative number required."));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		if (array.Length - offset < count)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		EnsureNotClosed();
		EnsureCanWrite();
		if (_writePos == 0)
		{
			ClearReadBufferBeforeWrite();
		}
		int num;
		checked
		{
			num = _writePos + count;
			if (num + count < _bufferSize + _bufferSize)
			{
				WriteToBuffer(array, ref offset, ref count);
				if (_writePos >= _bufferSize)
				{
					_stream.Write(_buffer, 0, _writePos);
					_writePos = 0;
					WriteToBuffer(array, ref offset, ref count);
				}
				return;
			}
		}
		if (_writePos > 0)
		{
			if (num <= _bufferSize + _bufferSize && num <= 81920)
			{
				EnsureShadowBufferAllocated();
				Buffer.InternalBlockCopy(array, offset, _buffer, _writePos, count);
				_stream.Write(_buffer, 0, num);
				_writePos = 0;
				return;
			}
			_stream.Write(_buffer, 0, _writePos);
			_writePos = 0;
		}
		_stream.Write(array, offset, count);
	}

	/// <summary>Begins an asynchronous write operation. (Consider using <see cref="M:System.IO.BufferedStream.WriteAsync(System.Byte[],System.Int32,System.Int32,System.Threading.CancellationToken)" /> instead; see the Remarks section.)</summary>
	/// <returns>An object that references the asynchronous write which could still be pending.</returns>
	/// <param name="buffer">The buffer containing data to write to the current stream.</param>
	/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
	/// <param name="count">The maximum number of bytes to write.</param>
	/// <param name="callback">The method to be called when the asynchronous write operation is completed.</param>
	/// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="buffer" /> length minus <paramref name="offset" /> is less than <paramref name="count" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support writing. </exception>
	public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", Environment.GetResourceString("Buffer cannot be null."));
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("Non-negative number required."));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		if (buffer.Length - offset < count)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		if (_stream == null)
		{
			__Error.ReadNotSupported();
		}
		EnsureCanWrite();
		SemaphoreSlim semaphoreSlim = EnsureAsyncActiveSemaphoreInitialized();
		Task task = semaphoreSlim.WaitAsync();
		if (task.Status == TaskStatus.RanToCompletion)
		{
			bool flag = true;
			try
			{
				if (_writePos == 0)
				{
					ClearReadBufferBeforeWrite();
				}
				flag = count < _bufferSize - _writePos;
				if (flag)
				{
					WriteToBuffer(buffer, ref offset, ref count, out var error);
					SynchronousAsyncResult synchronousAsyncResult = ((error == null) ? new SynchronousAsyncResult(state) : new SynchronousAsyncResult(error, state, isWrite: true));
					callback?.Invoke(synchronousAsyncResult);
					return synchronousAsyncResult;
				}
			}
			finally
			{
				if (flag)
				{
					semaphoreSlim.Release();
				}
			}
		}
		return BeginWriteToUnderlyingStream(buffer, offset, count, callback, state, task);
	}

	private IAsyncResult BeginWriteToUnderlyingStream(byte[] buffer, int offset, int count, AsyncCallback callback, object state, Task semaphoreLockTask)
	{
		return TaskToApm.Begin(WriteToUnderlyingStreamAsync(buffer, offset, count, CancellationToken.None, semaphoreLockTask, useApmPattern: true), callback, state);
	}

	/// <summary>Ends an asynchronous write operation and blocks until the I/O operation is complete. (Consider using <see cref="M:System.IO.BufferedStream.WriteAsync(System.Byte[],System.Int32,System.Int32,System.Threading.CancellationToken)" /> instead; see the Remarks section.)</summary>
	/// <param name="asyncResult">The pending asynchronous request.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">This <see cref="T:System.IAsyncResult" /> object was not created by calling <see cref="M:System.IO.BufferedStream.BeginWrite(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" /> on this class. </exception>
	public override void EndWrite(IAsyncResult asyncResult)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (asyncResult is SynchronousAsyncResult)
		{
			SynchronousAsyncResult.EndWrite(asyncResult);
		}
		else
		{
			TaskToApm.End(asyncResult);
		}
	}

	/// <summary>Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream by the number of bytes written, and monitors cancellation requests.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="buffer">The buffer to write data from.</param>
	/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> from which to begin copying bytes to the stream.</param>
	/// <param name="count">The maximum number of bytes to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
	/// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length.</exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The stream is currently in use by a previous write operation. </exception>
	public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", Environment.GetResourceString("Buffer cannot be null."));
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("Non-negative number required."));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		if (buffer.Length - offset < count)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCancellation<int>(cancellationToken);
		}
		EnsureNotClosed();
		EnsureCanWrite();
		SemaphoreSlim semaphoreSlim = EnsureAsyncActiveSemaphoreInitialized();
		Task task = semaphoreSlim.WaitAsync();
		if (task.Status == TaskStatus.RanToCompletion)
		{
			bool flag = true;
			try
			{
				if (_writePos == 0)
				{
					ClearReadBufferBeforeWrite();
				}
				flag = count < _bufferSize - _writePos;
				if (flag)
				{
					WriteToBuffer(buffer, ref offset, ref count, out var error);
					return (error == null) ? Task.CompletedTask : Task.FromException(error);
				}
			}
			finally
			{
				if (flag)
				{
					semaphoreSlim.Release();
				}
			}
		}
		return WriteToUnderlyingStreamAsync(buffer, offset, count, cancellationToken, task, useApmPattern: false);
	}

	private async Task WriteToUnderlyingStreamAsync(byte[] array, int offset, int count, CancellationToken cancellationToken, Task semaphoreLockTask, bool useApmPattern)
	{
		await semaphoreLockTask.ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			if (_writePos == 0)
			{
				ClearReadBufferBeforeWrite();
			}
			int totalUserBytes;
			checked
			{
				totalUserBytes = _writePos + count;
				if (totalUserBytes + count < _bufferSize + _bufferSize)
				{
					WriteToBuffer(array, ref offset, ref count);
					if (_writePos >= _bufferSize)
					{
						if (!useApmPattern)
						{
							await _stream.WriteAsync(_buffer, 0, _writePos, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						}
						else
						{
							EnsureBeginEndAwaitableAllocated();
							_stream.BeginWrite(_buffer, 0, _writePos, BeginEndAwaitableAdapter.Callback, _beginEndAwaitable);
							Stream stream = _stream;
							stream.EndWrite(await _beginEndAwaitable);
						}
						_writePos = 0;
						WriteToBuffer(array, ref offset, ref count);
					}
					return;
				}
			}
			if (_writePos > 0)
			{
				if (totalUserBytes <= _bufferSize + _bufferSize && totalUserBytes <= 81920)
				{
					EnsureShadowBufferAllocated();
					Buffer.InternalBlockCopy(array, offset, _buffer, _writePos, count);
					if (!useApmPattern)
					{
						await _stream.WriteAsync(_buffer, 0, totalUserBytes, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					}
					else
					{
						EnsureBeginEndAwaitableAllocated();
						_stream.BeginWrite(_buffer, 0, totalUserBytes, BeginEndAwaitableAdapter.Callback, _beginEndAwaitable);
						Stream stream = _stream;
						stream.EndWrite(await _beginEndAwaitable);
					}
					_writePos = 0;
					return;
				}
				if (!useApmPattern)
				{
					await _stream.WriteAsync(_buffer, 0, _writePos, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				else
				{
					EnsureBeginEndAwaitableAllocated();
					_stream.BeginWrite(_buffer, 0, _writePos, BeginEndAwaitableAdapter.Callback, _beginEndAwaitable);
					Stream stream = _stream;
					stream.EndWrite(await _beginEndAwaitable);
				}
				_writePos = 0;
			}
			if (useApmPattern)
			{
				EnsureBeginEndAwaitableAllocated();
				_stream.BeginWrite(array, offset, count, BeginEndAwaitableAdapter.Callback, _beginEndAwaitable);
				Stream stream = _stream;
				stream.EndWrite(await _beginEndAwaitable);
			}
			else
			{
				await _stream.WriteAsync(array, offset, count, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		finally
		{
			EnsureAsyncActiveSemaphoreInitialized().Release();
		}
	}

	/// <summary>Writes a byte to the current position in the buffered stream.</summary>
	/// <param name="value">A byte to write to the stream. </param>
	/// <exception cref="T:System.NotSupportedException">The stream does not support writing. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
	/// <filterpriority>2</filterpriority>
	public override void WriteByte(byte value)
	{
		EnsureNotClosed();
		if (_writePos == 0)
		{
			EnsureCanWrite();
			ClearReadBufferBeforeWrite();
			EnsureBufferAllocated();
		}
		if (_writePos >= _bufferSize - 1)
		{
			FlushWrite();
		}
		_buffer[_writePos++] = value;
	}

	/// <summary>Sets the position within the current buffered stream.</summary>
	/// <returns>The new position within the current buffered stream.</returns>
	/// <param name="offset">A byte offset relative to <paramref name="origin" />. </param>
	/// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point from which to obtain the new position. </param>
	/// <exception cref="T:System.IO.IOException">The stream is not open or is null. </exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception>
	/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
	/// <filterpriority>2</filterpriority>
	public override long Seek(long offset, SeekOrigin origin)
	{
		EnsureNotClosed();
		EnsureCanSeek();
		if (_writePos > 0)
		{
			FlushWrite();
			return _stream.Seek(offset, origin);
		}
		if (_readLen - _readPos > 0 && origin == SeekOrigin.Current)
		{
			offset -= _readLen - _readPos;
		}
		long position = Position;
		long num = _stream.Seek(offset, origin);
		_readPos = (int)(num - (position - _readPos));
		if (0 <= _readPos && _readPos < _readLen)
		{
			_stream.Seek(_readLen - _readPos, SeekOrigin.Current);
		}
		else
		{
			_readPos = (_readLen = 0);
		}
		return num;
	}

	/// <summary>Sets the length of the buffered stream.</summary>
	/// <param name="value">An integer indicating the desired length of the current buffered stream in bytes. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="value" /> is negative. </exception>
	/// <exception cref="T:System.IO.IOException">The stream is not open or is null. </exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking. </exception>
	/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
	/// <filterpriority>2</filterpriority>
	public override void SetLength(long value)
	{
		if (value < 0)
		{
			throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("Length must be non-negative."));
		}
		EnsureNotClosed();
		EnsureCanSeek();
		EnsureCanWrite();
		Flush();
		_stream.SetLength(value);
	}
}
