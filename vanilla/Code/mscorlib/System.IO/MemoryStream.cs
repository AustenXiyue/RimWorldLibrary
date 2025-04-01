using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO;

/// <summary>Creates a stream whose backing store is memory.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public class MemoryStream : Stream
{
	private byte[] _buffer;

	private int _origin;

	private int _position;

	private int _length;

	private int _capacity;

	private bool _expandable;

	private bool _writable;

	private bool _exposable;

	private bool _isOpen;

	[NonSerialized]
	private Task<int> _lastReadTask;

	private const int MemStreamMaxLength = int.MaxValue;

	/// <summary>Gets a value indicating whether the current stream supports reading.</summary>
	/// <returns>true if the stream is open.</returns>
	/// <filterpriority>2</filterpriority>
	public override bool CanRead => _isOpen;

	/// <summary>Gets a value indicating whether the current stream supports seeking.</summary>
	/// <returns>true if the stream is open.</returns>
	/// <filterpriority>2</filterpriority>
	public override bool CanSeek => _isOpen;

	/// <summary>Gets a value indicating whether the current stream supports writing.</summary>
	/// <returns>true if the stream supports writing; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public override bool CanWrite => _writable;

	/// <summary>Gets or sets the number of bytes allocated for this stream.</summary>
	/// <returns>The length of the usable portion of the buffer for the stream.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">A capacity is set that is negative or less than the current length of the stream. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The current stream is closed. </exception>
	/// <exception cref="T:System.NotSupportedException">set is invoked on a stream whose capacity cannot be modified. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual int Capacity
	{
		get
		{
			if (!_isOpen)
			{
				__Error.StreamIsClosed();
			}
			return _capacity - _origin;
		}
		set
		{
			if (value < Length)
			{
				throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("capacity was less than the current size."));
			}
			if (!_isOpen)
			{
				__Error.StreamIsClosed();
			}
			if (!_expandable && value != Capacity)
			{
				__Error.MemoryStreamNotExpandable();
			}
			if (!_expandable || value == _capacity)
			{
				return;
			}
			if (value > 0)
			{
				byte[] array = new byte[value];
				if (_length > 0)
				{
					Buffer.InternalBlockCopy(_buffer, 0, array, 0, _length);
				}
				_buffer = array;
			}
			else
			{
				_buffer = null;
			}
			_capacity = value;
		}
	}

	/// <summary>Gets the length of the stream in bytes.</summary>
	/// <returns>The length of the stream in bytes.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <filterpriority>2</filterpriority>
	public override long Length
	{
		get
		{
			if (!_isOpen)
			{
				__Error.StreamIsClosed();
			}
			return _length - _origin;
		}
	}

	/// <summary>Gets or sets the current position within the stream.</summary>
	/// <returns>The current position within the stream.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The position is set to a negative value or a value greater than <see cref="F:System.Int32.MaxValue" />. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <filterpriority>2</filterpriority>
	public override long Position
	{
		get
		{
			if (!_isOpen)
			{
				__Error.StreamIsClosed();
			}
			return _position - _origin;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("Non-negative number required."));
			}
			if (!_isOpen)
			{
				__Error.StreamIsClosed();
			}
			if (value > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("Stream length must be non-negative and less than 2^31 - 1 - origin."));
			}
			_position = _origin + (int)value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.MemoryStream" /> class with an expandable capacity initialized to zero.</summary>
	public MemoryStream()
		: this(0)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.MemoryStream" /> class with an expandable capacity initialized as specified.</summary>
	/// <param name="capacity">The initial size of the internal array in bytes. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is negative. </exception>
	public MemoryStream(int capacity)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("Capacity must be positive."));
		}
		_buffer = new byte[capacity];
		_capacity = capacity;
		_expandable = true;
		_writable = true;
		_exposable = true;
		_origin = 0;
		_isOpen = true;
	}

	/// <summary>Initializes a new non-resizable instance of the <see cref="T:System.IO.MemoryStream" /> class based on the specified byte array.</summary>
	/// <param name="buffer">The array of unsigned bytes from which to create the current stream. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	public MemoryStream(byte[] buffer)
		: this(buffer, writable: true)
	{
	}

	/// <summary>Initializes a new non-resizable instance of the <see cref="T:System.IO.MemoryStream" /> class based on the specified byte array with the <see cref="P:System.IO.MemoryStream.CanWrite" /> property set as specified.</summary>
	/// <param name="buffer">The array of unsigned bytes from which to create this stream. </param>
	/// <param name="writable">The setting of the <see cref="P:System.IO.MemoryStream.CanWrite" /> property, which determines whether the stream supports writing. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	public MemoryStream(byte[] buffer, bool writable)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", Environment.GetResourceString("Buffer cannot be null."));
		}
		_buffer = buffer;
		_length = (_capacity = buffer.Length);
		_writable = writable;
		_exposable = false;
		_origin = 0;
		_isOpen = true;
	}

	/// <summary>Initializes a new non-resizable instance of the <see cref="T:System.IO.MemoryStream" /> class based on the specified region (index) of a byte array.</summary>
	/// <param name="buffer">The array of unsigned bytes from which to create this stream. </param>
	/// <param name="index">The index into <paramref name="buffer" /> at which the stream begins. </param>
	/// <param name="count">The length of the stream in bytes. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="index" /> is less than <paramref name="count" />.</exception>
	public MemoryStream(byte[] buffer, int index, int count)
		: this(buffer, index, count, writable: true, publiclyVisible: false)
	{
	}

	/// <summary>Initializes a new non-resizable instance of the <see cref="T:System.IO.MemoryStream" /> class based on the specified region of a byte array, with the <see cref="P:System.IO.MemoryStream.CanWrite" /> property set as specified.</summary>
	/// <param name="buffer">The array of unsigned bytes from which to create this stream. </param>
	/// <param name="index">The index in <paramref name="buffer" /> at which the stream begins. </param>
	/// <param name="count">The length of the stream in bytes. </param>
	/// <param name="writable">The setting of the <see cref="P:System.IO.MemoryStream.CanWrite" /> property, which determines whether the stream supports writing. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> are negative. </exception>
	/// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="index" /> is less than <paramref name="count" />.</exception>
	public MemoryStream(byte[] buffer, int index, int count, bool writable)
		: this(buffer, index, count, writable, publiclyVisible: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.MemoryStream" /> class based on the specified region of a byte array, with the <see cref="P:System.IO.MemoryStream.CanWrite" /> property set as specified, and the ability to call <see cref="M:System.IO.MemoryStream.GetBuffer" /> set as specified.</summary>
	/// <param name="buffer">The array of unsigned bytes from which to create this stream. </param>
	/// <param name="index">The index into <paramref name="buffer" /> at which the stream begins. </param>
	/// <param name="count">The length of the stream in bytes. </param>
	/// <param name="writable">The setting of the <see cref="P:System.IO.MemoryStream.CanWrite" /> property, which determines whether the stream supports writing. </param>
	/// <param name="publiclyVisible">true to enable <see cref="M:System.IO.MemoryStream.GetBuffer" />, which returns the unsigned byte array from which the stream was created; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="index" /> is less than <paramref name="count" />. </exception>
	public MemoryStream(byte[] buffer, int index, int count, bool writable, bool publiclyVisible)
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
		_buffer = buffer;
		_origin = (_position = index);
		_length = (_capacity = index + count);
		_writable = writable;
		_exposable = publiclyVisible;
		_expandable = false;
		_isOpen = true;
	}

	private void EnsureWriteable()
	{
		if (!CanWrite)
		{
			__Error.WriteNotSupported();
		}
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.MemoryStream" /> class and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing)
			{
				_isOpen = false;
				_writable = false;
				_expandable = false;
				_lastReadTask = null;
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	private bool EnsureCapacity(int value)
	{
		if (value < 0)
		{
			throw new IOException(Environment.GetResourceString("Stream was too long."));
		}
		if (value > _capacity)
		{
			int num = value;
			if (num < 256)
			{
				num = 256;
			}
			if (num < _capacity * 2)
			{
				num = _capacity * 2;
			}
			if ((uint)(_capacity * 2) > 2147483591u)
			{
				num = ((value > 2147483591) ? value : 2147483591);
			}
			Capacity = num;
			return true;
		}
		return false;
	}

	/// <summary>Overrides the <see cref="M:System.IO.Stream.Flush" /> method so that no action is performed.</summary>
	/// <filterpriority>2</filterpriority>
	public override void Flush()
	{
	}

	/// <summary>Asynchronously clears all buffers for this stream, and monitors cancellation requests.</summary>
	/// <returns>A task that represents the asynchronous flush operation.</returns>
	/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public override Task FlushAsync(CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCancellation(cancellationToken);
		}
		try
		{
			Flush();
			return Task.CompletedTask;
		}
		catch (Exception exception)
		{
			return Task.FromException(exception);
		}
	}

	/// <summary>Returns the array of unsigned bytes from which this stream was created.</summary>
	/// <returns>The byte array from which this stream was created, or the underlying array if a byte array was not provided to the <see cref="T:System.IO.MemoryStream" /> constructor during construction of the current instance.</returns>
	/// <exception cref="T:System.UnauthorizedAccessException">The MemoryStream instance was not created with a publicly visible buffer. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual byte[] GetBuffer()
	{
		if (!_exposable)
		{
			throw new UnauthorizedAccessException(Environment.GetResourceString("MemoryStream's internal buffer cannot be accessed."));
		}
		return _buffer;
	}

	public virtual bool TryGetBuffer(out ArraySegment<byte> buffer)
	{
		if (!_exposable)
		{
			buffer = default(ArraySegment<byte>);
			return false;
		}
		buffer = new ArraySegment<byte>(_buffer, _origin, _length - _origin);
		return true;
	}

	internal byte[] InternalGetBuffer()
	{
		return _buffer;
	}

	[FriendAccessAllowed]
	internal void InternalGetOriginAndLength(out int origin, out int length)
	{
		if (!_isOpen)
		{
			__Error.StreamIsClosed();
		}
		origin = _origin;
		length = _length;
	}

	internal int InternalGetPosition()
	{
		if (!_isOpen)
		{
			__Error.StreamIsClosed();
		}
		return _position;
	}

	internal int InternalReadInt32()
	{
		if (!_isOpen)
		{
			__Error.StreamIsClosed();
		}
		int num = (_position += 4);
		if (num > _length)
		{
			_position = _length;
			__Error.EndOfFile();
		}
		return _buffer[num - 4] | (_buffer[num - 3] << 8) | (_buffer[num - 2] << 16) | (_buffer[num - 1] << 24);
	}

	internal int InternalEmulateRead(int count)
	{
		if (!_isOpen)
		{
			__Error.StreamIsClosed();
		}
		int num = _length - _position;
		if (num > count)
		{
			num = count;
		}
		if (num < 0)
		{
			num = 0;
		}
		_position += num;
		return num;
	}

	/// <summary>Reads a block of bytes from the current stream and writes the data to a buffer.</summary>
	/// <returns>The total number of bytes written into the buffer. This can be less than the number of bytes requested if that number of bytes are not currently available, or zero if the end of the stream is reached before any bytes are read.</returns>
	/// <param name="buffer">When this method returns, contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the characters read from the current stream. </param>
	/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing data from the current stream.</param>
	/// <param name="count">The maximum number of bytes to read. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="offset" /> subtracted from the buffer length is less than <paramref name="count" />. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The current stream instance is closed. </exception>
	/// <filterpriority>2</filterpriority>
	public override int Read([In][Out] byte[] buffer, int offset, int count)
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
		if (!_isOpen)
		{
			__Error.StreamIsClosed();
		}
		int num = _length - _position;
		if (num > count)
		{
			num = count;
		}
		if (num <= 0)
		{
			return 0;
		}
		if (num <= 8)
		{
			int num2 = num;
			while (--num2 >= 0)
			{
				buffer[offset + num2] = _buffer[_position + num2];
			}
		}
		else
		{
			Buffer.InternalBlockCopy(_buffer, _position, buffer, offset, num);
		}
		_position += num;
		return num;
	}

	/// <summary>Asynchronously reads a sequence of bytes from the current stream, advances the position within the stream by the number of bytes read, and monitors cancellation requests.</summary>
	/// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached. </returns>
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
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
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
		try
		{
			int num = Read(buffer, offset, count);
			Task<int> lastReadTask = _lastReadTask;
			return (lastReadTask != null && lastReadTask.Result == num) ? lastReadTask : (_lastReadTask = Task.FromResult(num));
		}
		catch (OperationCanceledException exception)
		{
			return Task.FromCancellation<int>(exception);
		}
		catch (Exception exception2)
		{
			return Task.FromException<int>(exception2);
		}
	}

	/// <summary>Reads a byte from the current stream.</summary>
	/// <returns>The byte cast to a <see cref="T:System.Int32" />, or -1 if the end of the stream has been reached.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The current stream instance is closed. </exception>
	/// <filterpriority>2</filterpriority>
	public override int ReadByte()
	{
		if (!_isOpen)
		{
			__Error.StreamIsClosed();
		}
		if (_position >= _length)
		{
			return -1;
		}
		return _buffer[_position++];
	}

	/// <summary>Asynchronously reads all the bytes from the current stream and writes them to another stream, using a specified buffer size and cancellation token.</summary>
	/// <returns>A task that represents the asynchronous copy operation.</returns>
	/// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
	/// <param name="bufferSize">The size, in bytes, of the buffer. This value must be greater than zero.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destination" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="buffersize" /> is negative or zero.</exception>
	/// <exception cref="T:System.ObjectDisposedException">Either the current stream or the destination stream is disposed.</exception>
	/// <exception cref="T:System.NotSupportedException">The current stream does not support reading, or the destination stream does not support writing.</exception>
	public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (bufferSize <= 0)
		{
			throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("Positive number required."));
		}
		if (!CanRead && !CanWrite)
		{
			throw new ObjectDisposedException(null, Environment.GetResourceString("Cannot access a closed Stream."));
		}
		if (!destination.CanRead && !destination.CanWrite)
		{
			throw new ObjectDisposedException("destination", Environment.GetResourceString("Cannot access a closed Stream."));
		}
		if (!CanRead)
		{
			throw new NotSupportedException(Environment.GetResourceString("Stream does not support reading."));
		}
		if (!destination.CanWrite)
		{
			throw new NotSupportedException(Environment.GetResourceString("Stream does not support writing."));
		}
		if (GetType() != typeof(MemoryStream))
		{
			return base.CopyToAsync(destination, bufferSize, cancellationToken);
		}
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCancellation(cancellationToken);
		}
		int position = _position;
		int count = InternalEmulateRead(_length - _position);
		if (!(destination is MemoryStream memoryStream))
		{
			return destination.WriteAsync(_buffer, position, count, cancellationToken);
		}
		try
		{
			memoryStream.Write(_buffer, position, count);
			return Task.CompletedTask;
		}
		catch (Exception exception)
		{
			return Task.FromException(exception);
		}
	}

	/// <summary>Sets the position within the current stream to the specified value.</summary>
	/// <returns>The new position within the stream, calculated by combining the initial reference point and the offset.</returns>
	/// <param name="offset">The new position within the stream. This is relative to the <paramref name="loc" /> parameter, and can be positive or negative. </param>
	/// <param name="loc">A value of type <see cref="T:System.IO.SeekOrigin" />, which acts as the seek reference point. </param>
	/// <exception cref="T:System.IO.IOException">Seeking is attempted before the beginning of the stream. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is greater than <see cref="F:System.Int32.MaxValue" />. </exception>
	/// <exception cref="T:System.ArgumentException">There is an invalid <see cref="T:System.IO.SeekOrigin" />. -or-<paramref name="offset" /> caused an arithmetic overflow.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The current stream instance is closed. </exception>
	/// <filterpriority>2</filterpriority>
	public override long Seek(long offset, SeekOrigin loc)
	{
		if (!_isOpen)
		{
			__Error.StreamIsClosed();
		}
		if (offset > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("Stream length must be non-negative and less than 2^31 - 1 - origin."));
		}
		switch (loc)
		{
		case SeekOrigin.Begin:
		{
			int num3 = _origin + (int)offset;
			if (offset < 0 || num3 < _origin)
			{
				throw new IOException(Environment.GetResourceString("An attempt was made to move the position before the beginning of the stream."));
			}
			_position = num3;
			break;
		}
		case SeekOrigin.Current:
		{
			int num2 = _position + (int)offset;
			if (_position + offset < _origin || num2 < _origin)
			{
				throw new IOException(Environment.GetResourceString("An attempt was made to move the position before the beginning of the stream."));
			}
			_position = num2;
			break;
		}
		case SeekOrigin.End:
		{
			int num = _length + (int)offset;
			if (_length + offset < _origin || num < _origin)
			{
				throw new IOException(Environment.GetResourceString("An attempt was made to move the position before the beginning of the stream."));
			}
			_position = num;
			break;
		}
		default:
			throw new ArgumentException(Environment.GetResourceString("Invalid seek origin."));
		}
		return _position;
	}

	/// <summary>Sets the length of the current stream to the specified value.</summary>
	/// <param name="value">The value at which to set the length. </param>
	/// <exception cref="T:System.NotSupportedException">The current stream is not resizable and <paramref name="value" /> is larger than the current capacity.-or- The current stream does not support writing. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="value" /> is negative or is greater than the maximum length of the <see cref="T:System.IO.MemoryStream" />, where the maximum length is(<see cref="F:System.Int32.MaxValue" /> - origin), and origin is the index into the underlying buffer at which the stream starts. </exception>
	/// <filterpriority>2</filterpriority>
	public override void SetLength(long value)
	{
		if (value < 0 || value > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("Stream length must be non-negative and less than 2^31 - 1 - origin."));
		}
		EnsureWriteable();
		if (value > int.MaxValue - _origin)
		{
			throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("Stream length must be non-negative and less than 2^31 - 1 - origin."));
		}
		int num = _origin + (int)value;
		if (!EnsureCapacity(num) && num > _length)
		{
			Array.Clear(_buffer, _length, num - _length);
		}
		_length = num;
		if (_position > num)
		{
			_position = num;
		}
	}

	/// <summary>Writes the stream contents to a byte array, regardless of the <see cref="P:System.IO.MemoryStream.Position" /> property.</summary>
	/// <returns>A new byte array.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual byte[] ToArray()
	{
		if (_length - _origin == 0)
		{
			return EmptyArray<byte>.Value;
		}
		byte[] array = new byte[_length - _origin];
		Buffer.InternalBlockCopy(_buffer, _origin, array, 0, _length - _origin);
		return array;
	}

	/// <summary>Writes a block of bytes to the current stream using data read from a buffer.</summary>
	/// <param name="buffer">The buffer to write data from. </param>
	/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
	/// <param name="count">The maximum number of bytes to write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support writing. For additional information see <see cref="P:System.IO.Stream.CanWrite" />.-or- The current position is closer than <paramref name="count" /> bytes to the end of the stream, and the capacity cannot be modified. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="offset" /> subtracted from the buffer length is less than <paramref name="count" />. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="count" /> are negative. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The current stream instance is closed. </exception>
	/// <filterpriority>2</filterpriority>
	public override void Write(byte[] buffer, int offset, int count)
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
		if (!_isOpen)
		{
			__Error.StreamIsClosed();
		}
		EnsureWriteable();
		int num = _position + count;
		if (num < 0)
		{
			throw new IOException(Environment.GetResourceString("Stream was too long."));
		}
		if (num > _length)
		{
			bool flag = _position > _length;
			if (num > _capacity && EnsureCapacity(num))
			{
				flag = false;
			}
			if (flag)
			{
				Array.Clear(_buffer, _length, num - _length);
			}
			_length = num;
		}
		if (count <= 8 && buffer != _buffer)
		{
			int num2 = count;
			while (--num2 >= 0)
			{
				_buffer[_position + num2] = buffer[offset + num2];
			}
		}
		else
		{
			Buffer.InternalBlockCopy(buffer, offset, _buffer, _position, count);
		}
		_position = num;
	}

	/// <summary>Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream by the number of bytes written, and monitors cancellation requests.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="buffer">The buffer to write data from.</param>
	/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> from which to begin copying bytes to the stream.</param>
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
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
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
			return Task.FromCancellation(cancellationToken);
		}
		try
		{
			Write(buffer, offset, count);
			return Task.CompletedTask;
		}
		catch (OperationCanceledException exception)
		{
			return Task.FromCancellation<VoidTaskResult>(exception);
		}
		catch (Exception exception2)
		{
			return Task.FromException(exception2);
		}
	}

	/// <summary>Writes a byte to the current stream at the current position.</summary>
	/// <param name="value">The byte to write. </param>
	/// <exception cref="T:System.NotSupportedException">The stream does not support writing. For additional information see <see cref="P:System.IO.Stream.CanWrite" />.-or- The current position is at the end of the stream, and the capacity cannot be modified. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The current stream is closed. </exception>
	/// <filterpriority>2</filterpriority>
	public override void WriteByte(byte value)
	{
		if (!_isOpen)
		{
			__Error.StreamIsClosed();
		}
		EnsureWriteable();
		if (_position >= _length)
		{
			int num = _position + 1;
			bool flag = _position > _length;
			if (num >= _capacity && EnsureCapacity(num))
			{
				flag = false;
			}
			if (flag)
			{
				Array.Clear(_buffer, _length, _position - _length);
			}
			_length = num;
		}
		_buffer[_position++] = value;
	}

	/// <summary>Writes the entire contents of this memory stream to another stream.</summary>
	/// <param name="stream">The stream to write this memory stream to. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The current or target stream is closed. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual void WriteTo(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream", Environment.GetResourceString("Stream cannot be null."));
		}
		if (!_isOpen)
		{
			__Error.StreamIsClosed();
		}
		stream.Write(_buffer, _origin, _length - _origin);
	}
}
