using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace System.IO;

/// <summary>Implements a <see cref="T:System.IO.TextReader" /> that reads from a string.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public class StringReader : TextReader
{
	private string _s;

	private int _pos;

	private int _length;

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StringReader" /> class that reads from the specified string.</summary>
	/// <param name="s">The string to which the <see cref="T:System.IO.StringReader" /> should be initialized. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="s" /> parameter is null. </exception>
	public StringReader(string s)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		_s = s;
		_length = s?.Length ?? 0;
	}

	/// <summary>Closes the <see cref="T:System.IO.StringReader" />.</summary>
	/// <filterpriority>2</filterpriority>
	public override void Close()
	{
		Dispose(disposing: true);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.StringReader" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected override void Dispose(bool disposing)
	{
		_s = null;
		_pos = 0;
		_length = 0;
		base.Dispose(disposing);
	}

	/// <summary>Returns the next available character but does not consume it.</summary>
	/// <returns>An integer representing the next character to be read, or -1 if no more characters are available or the stream does not support seeking.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The current reader is closed. </exception>
	/// <filterpriority>2</filterpriority>
	public override int Peek()
	{
		if (_s == null)
		{
			__Error.ReaderClosed();
		}
		if (_pos == _length)
		{
			return -1;
		}
		return _s[_pos];
	}

	/// <summary>Reads the next character from the input string and advances the character position by one character.</summary>
	/// <returns>The next character from the underlying string, or -1 if no more characters are available.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The current reader is closed. </exception>
	/// <filterpriority>2</filterpriority>
	public override int Read()
	{
		if (_s == null)
		{
			__Error.ReaderClosed();
		}
		if (_pos == _length)
		{
			return -1;
		}
		return _s[_pos++];
	}

	/// <summary>Reads a block of characters from the input string and advances the character position by <paramref name="count" />.</summary>
	/// <returns>The total number of characters read into the buffer. This can be less than the number of characters requested if that many characters are not currently available, or zero if the end of the underlying string has been reached.</returns>
	/// <param name="buffer">When this method returns, contains the specified character array with the values between <paramref name="index" /> and (<paramref name="index" /> + <paramref name="count" /> - 1) replaced by the characters read from the current source. </param>
	/// <param name="index">The starting index in the buffer. </param>
	/// <param name="count">The number of characters to read. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="index" /> is less than <paramref name="count" />. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The current reader is closed. </exception>
	/// <filterpriority>2</filterpriority>
	public override int Read([In][Out] char[] buffer, int index, int count)
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
		if (_s == null)
		{
			__Error.ReaderClosed();
		}
		int num = _length - _pos;
		if (num > 0)
		{
			if (num > count)
			{
				num = count;
			}
			_s.CopyTo(_pos, buffer, index, num);
			_pos += num;
		}
		return num;
	}

	/// <summary>Reads all characters from the current position to the end of the string and returns them as a single string.</summary>
	/// <returns>The content from the current position to the end of the underlying string.</returns>
	/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The current reader is closed. </exception>
	/// <filterpriority>2</filterpriority>
	public override string ReadToEnd()
	{
		if (_s == null)
		{
			__Error.ReaderClosed();
		}
		string result = ((_pos != 0) ? _s.Substring(_pos, _length - _pos) : _s);
		_pos = _length;
		return result;
	}

	/// <summary>Reads a line of characters from the current string and returns the data as a string.</summary>
	/// <returns>The next line from the current string, or null if the end of the string is reached.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The current reader is closed. </exception>
	/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string. </exception>
	/// <filterpriority>2</filterpriority>
	public override string ReadLine()
	{
		if (_s == null)
		{
			__Error.ReaderClosed();
		}
		int i;
		for (i = _pos; i < _length; i++)
		{
			char c = _s[i];
			if (c == '\r' || c == '\n')
			{
				string result = _s.Substring(_pos, i - _pos);
				_pos = i + 1;
				if (c == '\r' && _pos < _length && _s[_pos] == '\n')
				{
					_pos++;
				}
				return result;
			}
		}
		if (i > _pos)
		{
			string result2 = _s.Substring(_pos, i - _pos);
			_pos = i;
			return result2;
		}
		return null;
	}

	/// <summary>Reads a line of characters asynchronously from the current string and returns the data as a string.</summary>
	/// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains the next line from the string reader, or is null if all the characters have been read.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The number of characters in the next line is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The string reader has been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The reader is currently in use by a previous read operation. </exception>
	[ComVisible(false)]
	public override Task<string> ReadLineAsync()
	{
		return Task.FromResult(ReadLine());
	}

	/// <summary>Reads all characters from the current position to the end of the string asynchronously and returns them as a single string.</summary>
	/// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains a string with the characters from the current position to the end of the string. </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The number of characters is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The string reader has been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The reader is currently in use by a previous read operation. </exception>
	[ComVisible(false)]
	public override Task<string> ReadToEndAsync()
	{
		return Task.FromResult(ReadToEnd());
	}

	/// <summary>Reads a specified maximum number of characters from the current string asynchronously and writes the data to a buffer, beginning at the specified index.</summary>
	/// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the string has been reached.</returns>
	/// <param name="buffer">When this method returns, contains the specified character array with the values between <paramref name="index" /> and (<paramref name="index" /> + <paramref name="count" /> - 1) replaced by the characters read from the current source.</param>
	/// <param name="index">The position in <paramref name="buffer" /> at which to begin writing.</param>
	/// <param name="count">The maximum number of characters to read. If the end of the string is reached before the specified number of characters is written into the buffer, the method returns.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative.</exception>
	/// <exception cref="T:System.ArgumentException">The sum of <paramref name="index" /> and <paramref name="count" /> is larger than the buffer length.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The string reader has been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The reader is currently in use by a previous read operation. </exception>
	[ComVisible(false)]
	public override Task<int> ReadBlockAsync(char[] buffer, int index, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", Environment.GetResourceString("Buffer cannot be null."));
		}
		if (index < 0 || count < 0)
		{
			throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("Non-negative number required."));
		}
		if (buffer.Length - index < count)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		return Task.FromResult(ReadBlock(buffer, index, count));
	}

	/// <summary>Reads a specified maximum number of characters from the current string asynchronously and writes the data to a buffer, beginning at the specified index. </summary>
	/// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the string has been reached.</returns>
	/// <param name="buffer">When this method returns, contains the specified character array with the values between <paramref name="index" /> and (<paramref name="index" /> + <paramref name="count" /> - 1) replaced by the characters read from the current source.</param>
	/// <param name="index">The position in <paramref name="buffer" /> at which to begin writing.</param>
	/// <param name="count">The maximum number of characters to read. If the end of the string is reached before the specified number of characters is written into the buffer, the method returns.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative.</exception>
	/// <exception cref="T:System.ArgumentException">The sum of <paramref name="index" /> and <paramref name="count" /> is larger than the buffer length.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The string reader has been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The reader is currently in use by a previous read operation. </exception>
	[ComVisible(false)]
	public override Task<int> ReadAsync(char[] buffer, int index, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", Environment.GetResourceString("Buffer cannot be null."));
		}
		if (index < 0 || count < 0)
		{
			throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("Non-negative number required."));
		}
		if (buffer.Length - index < count)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		return Task.FromResult(Read(buffer, index, count));
	}
}
