using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Security.Cryptography;

/// <summary>Defines a stream that links data streams to cryptographic transformations.</summary>
[ComVisible(true)]
public class CryptoStream : Stream, IDisposable
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct HopToThreadPoolAwaitable : INotifyCompletion
	{
		public bool IsCompleted => false;

		public HopToThreadPoolAwaitable GetAwaiter()
		{
			return this;
		}

		public void OnCompleted(Action continuation)
		{
			Task.Run(continuation);
		}

		public void GetResult()
		{
		}
	}

	private Stream _stream;

	private ICryptoTransform _Transform;

	private byte[] _InputBuffer;

	private int _InputBufferIndex;

	private int _InputBlockSize;

	private byte[] _OutputBuffer;

	private int _OutputBufferIndex;

	private int _OutputBlockSize;

	private CryptoStreamMode _transformMode;

	private bool _canRead;

	private bool _canWrite;

	private bool _finalBlockTransformed;

	/// <summary>Gets a value indicating whether the current <see cref="T:System.Security.Cryptography.CryptoStream" /> is readable.</summary>
	/// <returns>true if the current stream is readable; otherwise, false.</returns>
	public override bool CanRead => _canRead;

	/// <summary>Gets a value indicating whether you can seek within the current <see cref="T:System.Security.Cryptography.CryptoStream" />.</summary>
	/// <returns>Always false.</returns>
	public override bool CanSeek => false;

	/// <summary>Gets a value indicating whether the current <see cref="T:System.Security.Cryptography.CryptoStream" /> is writable.</summary>
	/// <returns>true if the current stream is writable; otherwise, false.</returns>
	public override bool CanWrite => _canWrite;

	/// <summary>Gets the length in bytes of the stream.</summary>
	/// <returns>This property is not supported.</returns>
	/// <exception cref="T:System.NotSupportedException">This property is not supported. </exception>
	public override long Length
	{
		get
		{
			throw new NotSupportedException(Environment.GetResourceString("Stream does not support seeking."));
		}
	}

	/// <summary>Gets or sets the position within the current stream.</summary>
	/// <returns>This property is not supported.</returns>
	/// <exception cref="T:System.NotSupportedException">This property is not supported. </exception>
	public override long Position
	{
		get
		{
			throw new NotSupportedException(Environment.GetResourceString("Stream does not support seeking."));
		}
		set
		{
			throw new NotSupportedException(Environment.GetResourceString("Stream does not support seeking."));
		}
	}

	/// <summary>Gets a value indicating whether the final buffer block has been written to the underlying stream. </summary>
	/// <returns>true if the final block has been flushed; otherwise, false. </returns>
	public bool HasFlushedFinalBlock => _finalBlockTransformed;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.CryptoStream" /> class with a target data stream, the transformation to use, and the mode of the stream.</summary>
	/// <param name="stream">The stream on which to perform the cryptographic transformation. </param>
	/// <param name="transform">The cryptographic transformation that is to be performed on the stream. </param>
	/// <param name="mode">One of the <see cref="T:System.Security.Cryptography.CryptoStreamMode" /> values. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stream" /> is not readable.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stream" /> is not writable.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stream" /> is invalid.</exception>
	public CryptoStream(Stream stream, ICryptoTransform transform, CryptoStreamMode mode)
	{
		_stream = stream;
		_transformMode = mode;
		_Transform = transform;
		switch (_transformMode)
		{
		case CryptoStreamMode.Read:
			if (!_stream.CanRead)
			{
				throw new ArgumentException(Environment.GetResourceString("Stream was not readable."), "stream");
			}
			_canRead = true;
			break;
		case CryptoStreamMode.Write:
			if (!_stream.CanWrite)
			{
				throw new ArgumentException(Environment.GetResourceString("Stream was not writable."), "stream");
			}
			_canWrite = true;
			break;
		default:
			throw new ArgumentException(Environment.GetResourceString("Value was invalid."));
		}
		InitializeBuffer();
	}

	/// <summary>Updates the underlying data source or repository with the current state of the buffer, then clears the buffer.</summary>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The key is corrupt which can cause invalid padding to the stream. </exception>
	/// <exception cref="T:System.NotSupportedException">The current stream is not writable.-or- The final block has already been transformed. </exception>
	public void FlushFinalBlock()
	{
		if (_finalBlockTransformed)
		{
			throw new NotSupportedException(Environment.GetResourceString("FlushFinalBlock() method was called twice on a CryptoStream. It can only be called once."));
		}
		byte[] array = _Transform.TransformFinalBlock(_InputBuffer, 0, _InputBufferIndex);
		_finalBlockTransformed = true;
		if (_canWrite && _OutputBufferIndex > 0)
		{
			_stream.Write(_OutputBuffer, 0, _OutputBufferIndex);
			_OutputBufferIndex = 0;
		}
		if (_canWrite)
		{
			_stream.Write(array, 0, array.Length);
		}
		if (_stream is CryptoStream cryptoStream)
		{
			if (!cryptoStream.HasFlushedFinalBlock)
			{
				cryptoStream.FlushFinalBlock();
			}
		}
		else
		{
			_stream.Flush();
		}
		if (_InputBuffer != null)
		{
			Array.Clear(_InputBuffer, 0, _InputBuffer.Length);
		}
		if (_OutputBuffer != null)
		{
			Array.Clear(_OutputBuffer, 0, _OutputBuffer.Length);
		}
	}

	/// <summary>Clears all buffers for the current stream and causes any buffered data to be written to the underlying device.</summary>
	public override void Flush()
	{
	}

	/// <summary>Clears all buffers for the current stream asynchronously, causes any buffered data to be written to the underlying device, and monitors cancellation requests.</summary>
	/// <returns>A task that represents the asynchronous flush operation.</returns>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	public override Task FlushAsync(CancellationToken cancellationToken)
	{
		if (GetType() != typeof(CryptoStream))
		{
			return base.FlushAsync(cancellationToken);
		}
		if (!cancellationToken.IsCancellationRequested)
		{
			return Task.CompletedTask;
		}
		return Task.FromCancellation(cancellationToken);
	}

	/// <summary>Sets the position within the current stream.</summary>
	/// <returns>This method is not supported.</returns>
	/// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter. </param>
	/// <param name="origin">A <see cref="T:System.IO.SeekOrigin" /> object indicating the reference point used to obtain the new position. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not supported. </exception>
	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException(Environment.GetResourceString("Stream does not support seeking."));
	}

	/// <summary>Sets the length of the current stream.</summary>
	/// <param name="value">The desired length of the current stream in bytes. </param>
	/// <exception cref="T:System.NotSupportedException">This property exists only to support inheritance from <see cref="T:System.IO.Stream" />, and cannot be used.</exception>
	public override void SetLength(long value)
	{
		throw new NotSupportedException(Environment.GetResourceString("Stream does not support seeking."));
	}

	/// <summary>Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.</summary>
	/// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero if the end of the stream has been reached.</returns>
	/// <param name="buffer">An array of bytes. A maximum of <paramref name="count" /> bytes are read from the current stream and stored in <paramref name="buffer" />. </param>
	/// <param name="offset">The byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream. </param>
	/// <param name="count">The maximum number of bytes to be read from the current stream. </param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Security.Cryptography.CryptoStreamMode" /> associated with current <see cref="T:System.Security.Cryptography.CryptoStream" /> object does not match the underlying stream.  For example, this exception is thrown when using <see cref="F:System.Security.Cryptography.CryptoStreamMode.Read" /> with an underlying stream that is write only.  </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="offset" /> parameter is less than zero.-or- The <paramref name="count" /> parameter is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">Thesum of the <paramref name="count" /> and <paramref name="offset" /> parameters is longer than the length of the buffer. </exception>
	public override int Read([In][Out] byte[] buffer, int offset, int count)
	{
		if (!CanRead)
		{
			throw new NotSupportedException(Environment.GetResourceString("Stream does not support reading."));
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
		int num = count;
		int num2 = offset;
		if (_OutputBufferIndex != 0)
		{
			if (_OutputBufferIndex > count)
			{
				Buffer.InternalBlockCopy(_OutputBuffer, 0, buffer, offset, count);
				Buffer.InternalBlockCopy(_OutputBuffer, count, _OutputBuffer, 0, _OutputBufferIndex - count);
				_OutputBufferIndex -= count;
				return count;
			}
			Buffer.InternalBlockCopy(_OutputBuffer, 0, buffer, offset, _OutputBufferIndex);
			num -= _OutputBufferIndex;
			num2 += _OutputBufferIndex;
			_OutputBufferIndex = 0;
		}
		if (_finalBlockTransformed)
		{
			return count - num;
		}
		int num3 = 0;
		if (num > _OutputBlockSize && _Transform.CanTransformMultipleBlocks)
		{
			int num4 = num / _OutputBlockSize * _InputBlockSize;
			byte[] array = new byte[num4];
			Buffer.InternalBlockCopy(_InputBuffer, 0, array, 0, _InputBufferIndex);
			num3 = _InputBufferIndex;
			num3 += _stream.Read(array, _InputBufferIndex, num4 - _InputBufferIndex);
			_InputBufferIndex = 0;
			if (num3 <= _InputBlockSize)
			{
				_InputBuffer = array;
				_InputBufferIndex = num3;
			}
			else
			{
				int num5 = num3 / _InputBlockSize * _InputBlockSize;
				int num6 = num3 - num5;
				if (num6 != 0)
				{
					_InputBufferIndex = num6;
					Buffer.InternalBlockCopy(array, num5, _InputBuffer, 0, num6);
				}
				byte[] array2 = new byte[num5 / _InputBlockSize * _OutputBlockSize];
				int num7 = _Transform.TransformBlock(array, 0, num5, array2, 0);
				Buffer.InternalBlockCopy(array2, 0, buffer, num2, num7);
				Array.Clear(array, 0, array.Length);
				Array.Clear(array2, 0, array2.Length);
				num -= num7;
				num2 += num7;
			}
		}
		while (num > 0)
		{
			while (_InputBufferIndex < _InputBlockSize)
			{
				num3 = _stream.Read(_InputBuffer, _InputBufferIndex, _InputBlockSize - _InputBufferIndex);
				if (num3 != 0)
				{
					_InputBufferIndex += num3;
					continue;
				}
				_OutputBufferIndex = (_OutputBuffer = _Transform.TransformFinalBlock(_InputBuffer, 0, _InputBufferIndex)).Length;
				_finalBlockTransformed = true;
				if (num < _OutputBufferIndex)
				{
					Buffer.InternalBlockCopy(_OutputBuffer, 0, buffer, num2, num);
					_OutputBufferIndex -= num;
					Buffer.InternalBlockCopy(_OutputBuffer, num, _OutputBuffer, 0, _OutputBufferIndex);
					return count;
				}
				Buffer.InternalBlockCopy(_OutputBuffer, 0, buffer, num2, _OutputBufferIndex);
				num -= _OutputBufferIndex;
				_OutputBufferIndex = 0;
				return count - num;
			}
			int num7 = _Transform.TransformBlock(_InputBuffer, 0, _InputBlockSize, _OutputBuffer, 0);
			_InputBufferIndex = 0;
			if (num >= num7)
			{
				Buffer.InternalBlockCopy(_OutputBuffer, 0, buffer, num2, num7);
				num2 += num7;
				num -= num7;
				continue;
			}
			Buffer.InternalBlockCopy(_OutputBuffer, 0, buffer, num2, num);
			_OutputBufferIndex = num7 - num;
			Buffer.InternalBlockCopy(_OutputBuffer, num, _OutputBuffer, 0, _OutputBufferIndex);
			return count;
		}
		return count;
	}

	/// <summary>Reads a sequence of bytes from the current stream asynchronously, advances the position within the stream by the number of bytes read, and monitors cancellation requests.</summary>
	/// <returns>A task that represents the asynchronous read operation. The value of the task object's <paramref name="TResult" /> parameter contains the total number of bytes read into the buffer. The result can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached. </returns>
	/// <param name="buffer">The buffer to write the data into.</param>
	/// <param name="offset">The byte offset in <paramref name="buffer" /> at which to begin writing data from the stream.</param>
	/// <param name="count">The maximum number of bytes to read.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
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
		if (!CanRead)
		{
			throw new NotSupportedException(Environment.GetResourceString("Stream does not support reading."));
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
		if (GetType() != typeof(CryptoStream))
		{
			return base.ReadAsync(buffer, offset, count, cancellationToken);
		}
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCancellation<int>(cancellationToken);
		}
		return ReadAsyncInternal(buffer, offset, count, cancellationToken);
	}

	private async Task<int> ReadAsyncInternal(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		await default(HopToThreadPoolAwaitable);
		SemaphoreSlim sem = EnsureAsyncActiveSemaphoreInitialized();
		await sem.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			int bytesToDeliver = count;
			int currentOutputIndex = offset;
			if (_OutputBufferIndex != 0)
			{
				if (_OutputBufferIndex > count)
				{
					Buffer.InternalBlockCopy(_OutputBuffer, 0, buffer, offset, count);
					Buffer.InternalBlockCopy(_OutputBuffer, count, _OutputBuffer, 0, _OutputBufferIndex - count);
					_OutputBufferIndex -= count;
					return count;
				}
				Buffer.InternalBlockCopy(_OutputBuffer, 0, buffer, offset, _OutputBufferIndex);
				bytesToDeliver -= _OutputBufferIndex;
				currentOutputIndex += _OutputBufferIndex;
				_OutputBufferIndex = 0;
			}
			if (_finalBlockTransformed)
			{
				return count - bytesToDeliver;
			}
			if (bytesToDeliver > _OutputBlockSize && _Transform.CanTransformMultipleBlocks)
			{
				int num = bytesToDeliver / _OutputBlockSize * _InputBlockSize;
				byte[] tempInputBuffer = new byte[num];
				Buffer.InternalBlockCopy(_InputBuffer, 0, tempInputBuffer, 0, _InputBufferIndex);
				int inputBufferIndex = _InputBufferIndex;
				int num2 = inputBufferIndex;
				inputBufferIndex = num2 + await _stream.ReadAsync(tempInputBuffer, _InputBufferIndex, num - _InputBufferIndex, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				_InputBufferIndex = 0;
				if (inputBufferIndex <= _InputBlockSize)
				{
					_InputBuffer = tempInputBuffer;
					_InputBufferIndex = inputBufferIndex;
				}
				else
				{
					int num3 = inputBufferIndex / _InputBlockSize * _InputBlockSize;
					int num4 = inputBufferIndex - num3;
					if (num4 != 0)
					{
						_InputBufferIndex = num4;
						Buffer.InternalBlockCopy(tempInputBuffer, num3, _InputBuffer, 0, num4);
					}
					byte[] array = new byte[num3 / _InputBlockSize * _OutputBlockSize];
					int num5 = _Transform.TransformBlock(tempInputBuffer, 0, num3, array, 0);
					Buffer.InternalBlockCopy(array, 0, buffer, currentOutputIndex, num5);
					Array.Clear(tempInputBuffer, 0, tempInputBuffer.Length);
					Array.Clear(array, 0, array.Length);
					bytesToDeliver -= num5;
					currentOutputIndex += num5;
				}
			}
			while (bytesToDeliver > 0)
			{
				while (_InputBufferIndex < _InputBlockSize)
				{
					int inputBufferIndex = await _stream.ReadAsync(_InputBuffer, _InputBufferIndex, _InputBlockSize - _InputBufferIndex, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					if (inputBufferIndex != 0)
					{
						_InputBufferIndex += inputBufferIndex;
						continue;
					}
					_OutputBufferIndex = (_OutputBuffer = _Transform.TransformFinalBlock(_InputBuffer, 0, _InputBufferIndex)).Length;
					_finalBlockTransformed = true;
					if (bytesToDeliver < _OutputBufferIndex)
					{
						Buffer.InternalBlockCopy(_OutputBuffer, 0, buffer, currentOutputIndex, bytesToDeliver);
						_OutputBufferIndex -= bytesToDeliver;
						Buffer.InternalBlockCopy(_OutputBuffer, bytesToDeliver, _OutputBuffer, 0, _OutputBufferIndex);
						return count;
					}
					Buffer.InternalBlockCopy(_OutputBuffer, 0, buffer, currentOutputIndex, _OutputBufferIndex);
					bytesToDeliver -= _OutputBufferIndex;
					_OutputBufferIndex = 0;
					return count - bytesToDeliver;
				}
				int num5 = _Transform.TransformBlock(_InputBuffer, 0, _InputBlockSize, _OutputBuffer, 0);
				_InputBufferIndex = 0;
				if (bytesToDeliver >= num5)
				{
					Buffer.InternalBlockCopy(_OutputBuffer, 0, buffer, currentOutputIndex, num5);
					currentOutputIndex += num5;
					bytesToDeliver -= num5;
					continue;
				}
				Buffer.InternalBlockCopy(_OutputBuffer, 0, buffer, currentOutputIndex, bytesToDeliver);
				_OutputBufferIndex = num5 - bytesToDeliver;
				Buffer.InternalBlockCopy(_OutputBuffer, bytesToDeliver, _OutputBuffer, 0, _OutputBufferIndex);
				return count;
			}
			return count;
		}
		finally
		{
			sem.Release();
		}
	}

	/// <summary>Writes a sequence of bytes to the current <see cref="T:System.Security.Cryptography.CryptoStream" /> and advances the current position within the stream by the number of bytes written.</summary>
	/// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream. </param>
	/// <param name="offset">The byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream. </param>
	/// <param name="count">The number of bytes to be written to the current stream. </param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Security.Cryptography.CryptoStreamMode" /> associated with current <see cref="T:System.Security.Cryptography.CryptoStream" /> object does not match the underlying stream.  For example, this exception is thrown when using <see cref="F:System.Security.Cryptography.CryptoStreamMode.Write" />  with an underlying stream that is read only.  </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="offset" /> parameter is less than zero.-or- The <paramref name="count" /> parameter is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">The sum of the <paramref name="count" /> and <paramref name="offset" /> parameters is longer than the length of the buffer. </exception>
	public override void Write(byte[] buffer, int offset, int count)
	{
		if (!CanWrite)
		{
			throw new NotSupportedException(Environment.GetResourceString("Stream does not support writing."));
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
		int num = count;
		int num2 = offset;
		if (_InputBufferIndex > 0)
		{
			if (count < _InputBlockSize - _InputBufferIndex)
			{
				Buffer.InternalBlockCopy(buffer, offset, _InputBuffer, _InputBufferIndex, count);
				_InputBufferIndex += count;
				return;
			}
			Buffer.InternalBlockCopy(buffer, offset, _InputBuffer, _InputBufferIndex, _InputBlockSize - _InputBufferIndex);
			num2 += _InputBlockSize - _InputBufferIndex;
			num -= _InputBlockSize - _InputBufferIndex;
			_InputBufferIndex = _InputBlockSize;
		}
		if (_OutputBufferIndex > 0)
		{
			_stream.Write(_OutputBuffer, 0, _OutputBufferIndex);
			_OutputBufferIndex = 0;
		}
		if (_InputBufferIndex == _InputBlockSize)
		{
			int count2 = _Transform.TransformBlock(_InputBuffer, 0, _InputBlockSize, _OutputBuffer, 0);
			_stream.Write(_OutputBuffer, 0, count2);
			_InputBufferIndex = 0;
		}
		while (num > 0)
		{
			if (num >= _InputBlockSize)
			{
				if (_Transform.CanTransformMultipleBlocks)
				{
					int num3 = num / _InputBlockSize;
					int num4 = num3 * _InputBlockSize;
					byte[] array = new byte[num3 * _OutputBlockSize];
					int count2 = _Transform.TransformBlock(buffer, num2, num4, array, 0);
					_stream.Write(array, 0, count2);
					num2 += num4;
					num -= num4;
				}
				else
				{
					int count2 = _Transform.TransformBlock(buffer, num2, _InputBlockSize, _OutputBuffer, 0);
					_stream.Write(_OutputBuffer, 0, count2);
					num2 += _InputBlockSize;
					num -= _InputBlockSize;
				}
				continue;
			}
			Buffer.InternalBlockCopy(buffer, num2, _InputBuffer, 0, num);
			_InputBufferIndex += num;
			break;
		}
	}

	/// <summary>Writes a sequence of bytes to the current stream asynchronously, advances the current position within the stream by the number of bytes written, and monitors cancellation requests.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="buffer">The buffer to write data from.</param>
	/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> from which to begin writing bytes to the stream.</param>
	/// <param name="count">The maximum number of bytes to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
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
		if (!CanWrite)
		{
			throw new NotSupportedException(Environment.GetResourceString("Stream does not support writing."));
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
		if (GetType() != typeof(CryptoStream))
		{
			return base.WriteAsync(buffer, offset, count, cancellationToken);
		}
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCancellation(cancellationToken);
		}
		return WriteAsyncInternal(buffer, offset, count, cancellationToken);
	}

	private async Task WriteAsyncInternal(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		await default(HopToThreadPoolAwaitable);
		SemaphoreSlim sem = EnsureAsyncActiveSemaphoreInitialized();
		await sem.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			int bytesToWrite = count;
			int currentInputIndex = offset;
			if (_InputBufferIndex > 0)
			{
				if (count < _InputBlockSize - _InputBufferIndex)
				{
					Buffer.InternalBlockCopy(buffer, offset, _InputBuffer, _InputBufferIndex, count);
					_InputBufferIndex += count;
					return;
				}
				Buffer.InternalBlockCopy(buffer, offset, _InputBuffer, _InputBufferIndex, _InputBlockSize - _InputBufferIndex);
				currentInputIndex += _InputBlockSize - _InputBufferIndex;
				bytesToWrite -= _InputBlockSize - _InputBufferIndex;
				_InputBufferIndex = _InputBlockSize;
			}
			if (_OutputBufferIndex > 0)
			{
				await _stream.WriteAsync(_OutputBuffer, 0, _OutputBufferIndex, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				_OutputBufferIndex = 0;
			}
			if (_InputBufferIndex == _InputBlockSize)
			{
				int count2 = _Transform.TransformBlock(_InputBuffer, 0, _InputBlockSize, _OutputBuffer, 0);
				await _stream.WriteAsync(_OutputBuffer, 0, count2, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				_InputBufferIndex = 0;
			}
			while (bytesToWrite > 0)
			{
				if (bytesToWrite >= _InputBlockSize)
				{
					if (_Transform.CanTransformMultipleBlocks)
					{
						int num = bytesToWrite / _InputBlockSize;
						int numWholeBlocksInBytes = num * _InputBlockSize;
						byte[] array = new byte[num * _OutputBlockSize];
						int count2 = _Transform.TransformBlock(buffer, currentInputIndex, numWholeBlocksInBytes, array, 0);
						await _stream.WriteAsync(array, 0, count2, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						currentInputIndex += numWholeBlocksInBytes;
						bytesToWrite -= numWholeBlocksInBytes;
					}
					else
					{
						int count2 = _Transform.TransformBlock(buffer, currentInputIndex, _InputBlockSize, _OutputBuffer, 0);
						await _stream.WriteAsync(_OutputBuffer, 0, count2, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						currentInputIndex += _InputBlockSize;
						bytesToWrite -= _InputBlockSize;
					}
					continue;
				}
				Buffer.InternalBlockCopy(buffer, currentInputIndex, _InputBuffer, 0, bytesToWrite);
				_InputBufferIndex += bytesToWrite;
				break;
			}
		}
		finally
		{
			sem.Release();
		}
	}

	/// <summary>Releases all resources used by the <see cref="T:System.Security.Cryptography.CryptoStream" />.</summary>
	public void Clear()
	{
		Close();
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Security.Cryptography.CryptoStream" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing)
			{
				if (!_finalBlockTransformed)
				{
					FlushFinalBlock();
				}
				_stream.Close();
			}
		}
		finally
		{
			try
			{
				_finalBlockTransformed = true;
				if (_InputBuffer != null)
				{
					Array.Clear(_InputBuffer, 0, _InputBuffer.Length);
				}
				if (_OutputBuffer != null)
				{
					Array.Clear(_OutputBuffer, 0, _OutputBuffer.Length);
				}
				_InputBuffer = null;
				_OutputBuffer = null;
				_canRead = false;
				_canWrite = false;
			}
			finally
			{
				base.Dispose(disposing);
			}
		}
	}

	private void InitializeBuffer()
	{
		if (_Transform != null)
		{
			_InputBlockSize = _Transform.InputBlockSize;
			_InputBuffer = new byte[_InputBlockSize];
			_OutputBlockSize = _Transform.OutputBlockSize;
			_OutputBuffer = new byte[_OutputBlockSize];
		}
	}
}
