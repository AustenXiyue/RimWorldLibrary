using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace System.IO;

/// <summary>Implements a <see cref="T:System.IO.TextReader" /> that reads characters from a byte stream in a particular encoding.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
public class StreamReader : TextReader
{
	private class NullStreamReader : StreamReader
	{
		public override Stream BaseStream => Stream.Null;

		public override Encoding CurrentEncoding => Encoding.Unicode;

		internal NullStreamReader()
		{
			Init(Stream.Null);
		}

		protected override void Dispose(bool disposing)
		{
		}

		public override int Peek()
		{
			return -1;
		}

		public override int Read()
		{
			return -1;
		}

		public override int Read(char[] buffer, int index, int count)
		{
			return 0;
		}

		public override string ReadLine()
		{
			return null;
		}

		public override string ReadToEnd()
		{
			return string.Empty;
		}

		internal override int ReadBuffer()
		{
			return 0;
		}
	}

	/// <summary>A <see cref="T:System.IO.StreamReader" /> object around an empty stream.</summary>
	/// <filterpriority>1</filterpriority>
	public new static readonly StreamReader Null = new NullStreamReader();

	private const int DefaultFileStreamBufferSize = 4096;

	private const int MinBufferSize = 128;

	private Stream stream;

	private Encoding encoding;

	private Decoder decoder;

	private byte[] byteBuffer;

	private char[] charBuffer;

	private byte[] _preamble;

	private int charPos;

	private int charLen;

	private int byteLen;

	private int bytePos;

	private int _maxCharsPerBuffer;

	private bool _detectEncoding;

	private bool _checkPreamble;

	private bool _isBlocked;

	private bool _closable;

	[NonSerialized]
	private volatile Task _asyncReadTask;

	internal static int DefaultBufferSize => 1024;

	/// <summary>Gets the current character encoding that the current <see cref="T:System.IO.StreamReader" /> object is using.</summary>
	/// <returns>The current character encoding used by the current reader. The value can be different after the first call to any <see cref="Overload:System.IO.StreamReader.Read" /> method of <see cref="T:System.IO.StreamReader" />, since encoding autodetection is not done until the first call to a <see cref="Overload:System.IO.StreamReader.Read" /> method.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual Encoding CurrentEncoding => encoding;

	/// <summary>Returns the underlying stream.</summary>
	/// <returns>The underlying stream.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual Stream BaseStream => stream;

	internal bool LeaveOpen => !_closable;

	/// <summary>Gets a value that indicates whether the current stream position is at the end of the stream.</summary>
	/// <returns>true if the current stream position is at the end of the stream; otherwise false.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The underlying stream has been disposed.</exception>
	/// <filterpriority>1</filterpriority>
	public bool EndOfStream
	{
		get
		{
			if (stream == null)
			{
				__Error.ReaderClosed();
			}
			CheckAsyncTaskInProgress();
			if (charPos < charLen)
			{
				return false;
			}
			return ReadBuffer() == 0;
		}
	}

	private int CharLen_Prop
	{
		get
		{
			return charLen;
		}
		set
		{
			charLen = value;
		}
	}

	private int CharPos_Prop
	{
		get
		{
			return charPos;
		}
		set
		{
			charPos = value;
		}
	}

	private int ByteLen_Prop
	{
		get
		{
			return byteLen;
		}
		set
		{
			byteLen = value;
		}
	}

	private int BytePos_Prop
	{
		get
		{
			return bytePos;
		}
		set
		{
			bytePos = value;
		}
	}

	private byte[] Preamble_Prop => _preamble;

	private bool CheckPreamble_Prop => _checkPreamble;

	private Decoder Decoder_Prop => decoder;

	private bool DetectEncoding_Prop => _detectEncoding;

	private char[] CharBuffer_Prop => charBuffer;

	private byte[] ByteBuffer_Prop => byteBuffer;

	private bool IsBlocked_Prop
	{
		get
		{
			return _isBlocked;
		}
		set
		{
			_isBlocked = value;
		}
	}

	private Stream Stream_Prop => stream;

	private int MaxCharsPerBuffer_Prop => _maxCharsPerBuffer;

	private void CheckAsyncTaskInProgress()
	{
		Task asyncReadTask = _asyncReadTask;
		if (asyncReadTask != null && !asyncReadTask.IsCompleted)
		{
			throw new InvalidOperationException(Environment.GetResourceString("The stream is currently in use by a previous operation on the stream."));
		}
	}

	internal StreamReader()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReader" /> class for the specified stream.</summary>
	/// <param name="stream">The stream to be read. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stream" /> does not support reading. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null. </exception>
	public StreamReader(Stream stream)
		: this(stream, detectEncodingFromByteOrderMarks: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReader" /> class for the specified stream, with the specified byte order mark detection option.</summary>
	/// <param name="stream">The stream to be read. </param>
	/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stream" /> does not support reading. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null. </exception>
	public StreamReader(Stream stream, bool detectEncodingFromByteOrderMarks)
		: this(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks, DefaultBufferSize, leaveOpen: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReader" /> class for the specified stream, with the specified character encoding.</summary>
	/// <param name="stream">The stream to be read. </param>
	/// <param name="encoding">The character encoding to use. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stream" /> does not support reading. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> or <paramref name="encoding" /> is null. </exception>
	public StreamReader(Stream stream, Encoding encoding)
		: this(stream, encoding, detectEncodingFromByteOrderMarks: true, DefaultBufferSize, leaveOpen: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReader" /> class for the specified stream, with the specified character encoding and byte order mark detection option.</summary>
	/// <param name="stream">The stream to be read. </param>
	/// <param name="encoding">The character encoding to use. </param>
	/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stream" /> does not support reading. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> or <paramref name="encoding" /> is null. </exception>
	public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks)
		: this(stream, encoding, detectEncodingFromByteOrderMarks, DefaultBufferSize, leaveOpen: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReader" /> class for the specified stream, with the specified character encoding, byte order mark detection option, and buffer size.</summary>
	/// <param name="stream">The stream to be read. </param>
	/// <param name="encoding">The character encoding to use. </param>
	/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file. </param>
	/// <param name="bufferSize">The minimum buffer size. </param>
	/// <exception cref="T:System.ArgumentException">The stream does not support reading. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> or <paramref name="encoding" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="bufferSize" /> is less than or equal to zero. </exception>
	public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
		: this(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, leaveOpen: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReader" /> class for the specified stream based on the specified character encoding, byte order mark detection option, and buffer size, and optionally leaves the stream open.</summary>
	/// <param name="stream">The stream to read.</param>
	/// <param name="encoding">The character encoding to use.</param>
	/// <param name="detectEncodingFromByteOrderMarks">true to look for byte order marks at the beginning of the file; otherwise, false.</param>
	/// <param name="bufferSize">The minimum buffer size.</param>
	/// <param name="leaveOpen">true to leave the stream open after the <see cref="T:System.IO.StreamReader" /> object is disposed; otherwise, false.</param>
	public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool leaveOpen)
	{
		if (stream == null || encoding == null)
		{
			throw new ArgumentNullException((stream == null) ? "stream" : "encoding");
		}
		if (!stream.CanRead)
		{
			throw new ArgumentException(Environment.GetResourceString("Stream was not readable."));
		}
		if (bufferSize <= 0)
		{
			throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("Positive number required."));
		}
		Init(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, leaveOpen);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReader" /> class for the specified file name.</summary>
	/// <param name="path">The complete file path to be read. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""). </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive. </exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="path" /> includes an incorrect or invalid syntax for file name, directory name, or volume label. </exception>
	public StreamReader(string path)
		: this(path, detectEncodingFromByteOrderMarks: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReader" /> class for the specified file name, with the specified byte order mark detection option.</summary>
	/// <param name="path">The complete file path to be read. </param>
	/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""). </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive. </exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="path" /> includes an incorrect or invalid syntax for file name, directory name, or volume label. </exception>
	public StreamReader(string path, bool detectEncodingFromByteOrderMarks)
		: this(path, Encoding.UTF8, detectEncodingFromByteOrderMarks, DefaultBufferSize)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReader" /> class for the specified file name, with the specified character encoding.</summary>
	/// <param name="path">The complete file path to be read. </param>
	/// <param name="encoding">The character encoding to use. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""). </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> or <paramref name="encoding" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> includes an incorrect or invalid syntax for file name, directory name, or volume label. </exception>
	public StreamReader(string path, Encoding encoding)
		: this(path, encoding, detectEncodingFromByteOrderMarks: true, DefaultBufferSize)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReader" /> class for the specified file name, with the specified character encoding and byte order mark detection option.</summary>
	/// <param name="path">The complete file path to be read. </param>
	/// <param name="encoding">The character encoding to use. </param>
	/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""). </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> or <paramref name="encoding" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> includes an incorrect or invalid syntax for file name, directory name, or volume label. </exception>
	public StreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks)
		: this(path, encoding, detectEncodingFromByteOrderMarks, DefaultBufferSize)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReader" /> class for the specified file name, with the specified character encoding, byte order mark detection option, and buffer size.</summary>
	/// <param name="path">The complete file path to be read. </param>
	/// <param name="encoding">The character encoding to use. </param>
	/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file. </param>
	/// <param name="bufferSize">The minimum buffer size, in number of 16-bit characters. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""). </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> or <paramref name="encoding" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> includes an incorrect or invalid syntax for file name, directory name, or volume label. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="buffersize" /> is less than or equal to zero. </exception>
	[SecuritySafeCritical]
	public StreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
		: this(path, encoding, detectEncodingFromByteOrderMarks, bufferSize, checkHost: true)
	{
	}

	[SecurityCritical]
	internal StreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool checkHost)
	{
		if (path == null || encoding == null)
		{
			throw new ArgumentNullException((path == null) ? "path" : "encoding");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException(Environment.GetResourceString("Empty path name is not legal."));
		}
		if (bufferSize <= 0)
		{
			throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("Positive number required."));
		}
		Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan, Path.GetFileName(path), bFromProxy: false, useLongPath: false, checkHost);
		Init(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, leaveOpen: false);
	}

	private void Init(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool leaveOpen)
	{
		this.stream = stream;
		this.encoding = encoding;
		decoder = encoding.GetDecoder();
		if (bufferSize < 128)
		{
			bufferSize = 128;
		}
		byteBuffer = new byte[bufferSize];
		_maxCharsPerBuffer = encoding.GetMaxCharCount(bufferSize);
		charBuffer = new char[_maxCharsPerBuffer];
		byteLen = 0;
		bytePos = 0;
		_detectEncoding = detectEncodingFromByteOrderMarks;
		_preamble = encoding.GetPreamble();
		_checkPreamble = _preamble.Length != 0;
		_isBlocked = false;
		_closable = !leaveOpen;
	}

	internal void Init(Stream stream)
	{
		this.stream = stream;
		_closable = true;
	}

	/// <summary>Closes the <see cref="T:System.IO.StreamReader" /> object and the underlying stream, and releases any system resources associated with the reader.</summary>
	/// <filterpriority>1</filterpriority>
	public override void Close()
	{
		Dispose(disposing: true);
	}

	/// <summary>Closes the underlying stream, releases the unmanaged resources used by the <see cref="T:System.IO.StreamReader" />, and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!LeaveOpen && disposing && stream != null)
			{
				stream.Close();
			}
		}
		finally
		{
			if (!LeaveOpen && stream != null)
			{
				stream = null;
				encoding = null;
				decoder = null;
				byteBuffer = null;
				charBuffer = null;
				charPos = 0;
				charLen = 0;
				base.Dispose(disposing);
			}
		}
	}

	/// <summary>Clears the internal buffer.</summary>
	/// <filterpriority>2</filterpriority>
	public void DiscardBufferedData()
	{
		CheckAsyncTaskInProgress();
		byteLen = 0;
		charLen = 0;
		charPos = 0;
		if (encoding != null)
		{
			decoder = encoding.GetDecoder();
		}
		_isBlocked = false;
	}

	/// <summary>Returns the next available character but does not consume it.</summary>
	/// <returns>An integer representing the next character to be read, or -1 if there are no characters to be read or if the stream does not support seeking.</returns>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public override int Peek()
	{
		if (stream == null)
		{
			__Error.ReaderClosed();
		}
		CheckAsyncTaskInProgress();
		if (charPos == charLen && (_isBlocked || ReadBuffer() == 0))
		{
			return -1;
		}
		return charBuffer[charPos];
	}

	internal bool DataAvailable()
	{
		return charPos < charLen;
	}

	/// <summary>Reads the next character from the input stream and advances the character position by one character.</summary>
	/// <returns>The next character from the input stream represented as an <see cref="T:System.Int32" /> object, or -1 if no more characters are available.</returns>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public override int Read()
	{
		if (stream == null)
		{
			__Error.ReaderClosed();
		}
		CheckAsyncTaskInProgress();
		if (charPos == charLen && ReadBuffer() == 0)
		{
			return -1;
		}
		char result = charBuffer[charPos];
		charPos++;
		return result;
	}

	/// <summary>Reads a specified maximum of characters from the current stream into a buffer, beginning at the specified index.</summary>
	/// <returns>The number of characters that have been read, or 0 if at the end of the stream and no data was read. The number will be less than or equal to the <paramref name="count" /> parameter, depending on whether the data is available within the stream.</returns>
	/// <param name="buffer">When this method returns, contains the specified character array with the values between <paramref name="index" /> and (<paramref name="index + count - 1" />) replaced by the characters read from the current source. </param>
	/// <param name="index">The index of <paramref name="buffer" /> at which to begin writing. </param>
	/// <param name="count">The maximum number of characters to read. </param>
	/// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="index" /> is less than <paramref name="count" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs, such as the stream is closed. </exception>
	/// <filterpriority>1</filterpriority>
	public override int Read([In][Out] char[] buffer, int index, int count)
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
		if (stream == null)
		{
			__Error.ReaderClosed();
		}
		CheckAsyncTaskInProgress();
		int num = 0;
		bool readToUserBuffer = false;
		while (count > 0)
		{
			int num2 = charLen - charPos;
			if (num2 == 0)
			{
				num2 = ReadBuffer(buffer, index + num, count, out readToUserBuffer);
			}
			if (num2 == 0)
			{
				break;
			}
			if (num2 > count)
			{
				num2 = count;
			}
			if (!readToUserBuffer)
			{
				Buffer.InternalBlockCopy(charBuffer, charPos * 2, buffer, (index + num) * 2, num2 * 2);
				charPos += num2;
			}
			num += num2;
			count -= num2;
			if (_isBlocked)
			{
				break;
			}
		}
		return num;
	}

	/// <summary>Reads all characters from the current position to the end of the stream.</summary>
	/// <returns>The rest of the stream as a string, from the current position to the end. If the current position is at the end of the stream, returns an empty string ("").</returns>
	/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public override string ReadToEnd()
	{
		if (stream == null)
		{
			__Error.ReaderClosed();
		}
		CheckAsyncTaskInProgress();
		StringBuilder stringBuilder = new StringBuilder(charLen - charPos);
		do
		{
			stringBuilder.Append(charBuffer, charPos, charLen - charPos);
			charPos = charLen;
			ReadBuffer();
		}
		while (charLen > 0);
		return stringBuilder.ToString();
	}

	/// <summary>Reads a specified maximum number of characters from the current stream and writes the data to a buffer, beginning at the specified index.</summary>
	/// <returns>The number of characters that have been read. The number will be less than or equal to <paramref name="count" />, depending on whether all input characters have been read.</returns>
	/// <param name="buffer">When this method returns, contains the specified character array with the values between <paramref name="index" /> and (<paramref name="index + count - 1" />) replaced by the characters read from the current source.</param>
	/// <param name="index">The position in <paramref name="buffer" /> at which to begin writing.</param>
	/// <param name="count">The maximum number of characters to read.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="index" /> is less than <paramref name="count" />. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.StreamReader" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	public override int ReadBlock([In][Out] char[] buffer, int index, int count)
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
		if (stream == null)
		{
			__Error.ReaderClosed();
		}
		CheckAsyncTaskInProgress();
		return base.ReadBlock(buffer, index, count);
	}

	private void CompressBuffer(int n)
	{
		Buffer.InternalBlockCopy(byteBuffer, n, byteBuffer, 0, byteLen - n);
		byteLen -= n;
	}

	private void DetectEncoding()
	{
		if (byteLen < 2)
		{
			return;
		}
		_detectEncoding = false;
		bool flag = false;
		if (byteBuffer[0] == 254 && byteBuffer[1] == byte.MaxValue)
		{
			encoding = new UnicodeEncoding(bigEndian: true, byteOrderMark: true);
			CompressBuffer(2);
			flag = true;
		}
		else if (byteBuffer[0] == byte.MaxValue && byteBuffer[1] == 254)
		{
			if (byteLen < 4 || byteBuffer[2] != 0 || byteBuffer[3] != 0)
			{
				encoding = new UnicodeEncoding(bigEndian: false, byteOrderMark: true);
				CompressBuffer(2);
				flag = true;
			}
			else
			{
				encoding = new UTF32Encoding(bigEndian: false, byteOrderMark: true);
				CompressBuffer(4);
				flag = true;
			}
		}
		else if (byteLen >= 3 && byteBuffer[0] == 239 && byteBuffer[1] == 187 && byteBuffer[2] == 191)
		{
			encoding = Encoding.UTF8;
			CompressBuffer(3);
			flag = true;
		}
		else if (byteLen >= 4 && byteBuffer[0] == 0 && byteBuffer[1] == 0 && byteBuffer[2] == 254 && byteBuffer[3] == byte.MaxValue)
		{
			encoding = new UTF32Encoding(bigEndian: true, byteOrderMark: true);
			CompressBuffer(4);
			flag = true;
		}
		else if (byteLen == 2)
		{
			_detectEncoding = true;
		}
		if (flag)
		{
			decoder = encoding.GetDecoder();
			_maxCharsPerBuffer = encoding.GetMaxCharCount(byteBuffer.Length);
			charBuffer = new char[_maxCharsPerBuffer];
		}
	}

	private bool IsPreamble()
	{
		if (!_checkPreamble)
		{
			return _checkPreamble;
		}
		int num = ((byteLen >= _preamble.Length) ? (_preamble.Length - bytePos) : (byteLen - bytePos));
		int num2 = 0;
		while (num2 < num)
		{
			if (byteBuffer[bytePos] != _preamble[bytePos])
			{
				bytePos = 0;
				_checkPreamble = false;
				break;
			}
			num2++;
			bytePos++;
		}
		if (_checkPreamble && bytePos == _preamble.Length)
		{
			CompressBuffer(_preamble.Length);
			bytePos = 0;
			_checkPreamble = false;
			_detectEncoding = false;
		}
		return _checkPreamble;
	}

	internal virtual int ReadBuffer()
	{
		charLen = 0;
		charPos = 0;
		if (!_checkPreamble)
		{
			byteLen = 0;
		}
		do
		{
			if (_checkPreamble)
			{
				int num = stream.Read(byteBuffer, bytePos, byteBuffer.Length - bytePos);
				if (num == 0)
				{
					if (byteLen > 0)
					{
						charLen += decoder.GetChars(byteBuffer, 0, byteLen, charBuffer, charLen);
						bytePos = (byteLen = 0);
					}
					return charLen;
				}
				byteLen += num;
			}
			else
			{
				byteLen = stream.Read(byteBuffer, 0, byteBuffer.Length);
				if (byteLen == 0)
				{
					return charLen;
				}
			}
			_isBlocked = byteLen < byteBuffer.Length;
			if (!IsPreamble())
			{
				if (_detectEncoding && byteLen >= 2)
				{
					DetectEncoding();
				}
				charLen += decoder.GetChars(byteBuffer, 0, byteLen, charBuffer, charLen);
			}
		}
		while (charLen == 0);
		return charLen;
	}

	private int ReadBuffer(char[] userBuffer, int userOffset, int desiredChars, out bool readToUserBuffer)
	{
		charLen = 0;
		charPos = 0;
		if (!_checkPreamble)
		{
			byteLen = 0;
		}
		int num = 0;
		readToUserBuffer = desiredChars >= _maxCharsPerBuffer;
		do
		{
			if (_checkPreamble)
			{
				int num2 = stream.Read(byteBuffer, bytePos, byteBuffer.Length - bytePos);
				if (num2 == 0)
				{
					if (byteLen > 0)
					{
						if (readToUserBuffer)
						{
							num = decoder.GetChars(byteBuffer, 0, byteLen, userBuffer, userOffset + num);
							charLen = 0;
						}
						else
						{
							num = decoder.GetChars(byteBuffer, 0, byteLen, charBuffer, num);
							charLen += num;
						}
					}
					return num;
				}
				byteLen += num2;
			}
			else
			{
				byteLen = stream.Read(byteBuffer, 0, byteBuffer.Length);
				if (byteLen == 0)
				{
					break;
				}
			}
			_isBlocked = byteLen < byteBuffer.Length;
			if (!IsPreamble())
			{
				if (_detectEncoding && byteLen >= 2)
				{
					DetectEncoding();
					readToUserBuffer = desiredChars >= _maxCharsPerBuffer;
				}
				charPos = 0;
				if (readToUserBuffer)
				{
					num += decoder.GetChars(byteBuffer, 0, byteLen, userBuffer, userOffset + num);
					charLen = 0;
				}
				else
				{
					num = decoder.GetChars(byteBuffer, 0, byteLen, charBuffer, num);
					charLen += num;
				}
			}
		}
		while (num == 0);
		_isBlocked &= num < desiredChars;
		return num;
	}

	/// <summary>Reads a line of characters from the current stream and returns the data as a string.</summary>
	/// <returns>The next line from the input stream, or null if the end of the input stream is reached.</returns>
	/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public override string ReadLine()
	{
		if (stream == null)
		{
			__Error.ReaderClosed();
		}
		CheckAsyncTaskInProgress();
		if (charPos == charLen && ReadBuffer() == 0)
		{
			return null;
		}
		StringBuilder stringBuilder = null;
		do
		{
			int num = charPos;
			do
			{
				char c = charBuffer[num];
				if (c == '\r' || c == '\n')
				{
					string result;
					if (stringBuilder != null)
					{
						stringBuilder.Append(charBuffer, charPos, num - charPos);
						result = stringBuilder.ToString();
					}
					else
					{
						result = new string(charBuffer, charPos, num - charPos);
					}
					charPos = num + 1;
					if (c == '\r' && (charPos < charLen || ReadBuffer() > 0) && charBuffer[charPos] == '\n')
					{
						charPos++;
					}
					return result;
				}
				num++;
			}
			while (num < charLen);
			num = charLen - charPos;
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder(num + 80);
			}
			stringBuilder.Append(charBuffer, charPos, num);
		}
		while (ReadBuffer() > 0);
		return stringBuilder.ToString();
	}

	/// <summary>Reads a line of characters asynchronously from the current stream and returns the data as a string.</summary>
	/// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains the next line from the stream, or is null if all the characters have been read.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The number of characters in the next line is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The reader is currently in use by a previous read operation. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public override Task<string> ReadLineAsync()
	{
		if (GetType() != typeof(StreamReader))
		{
			return base.ReadLineAsync();
		}
		if (stream == null)
		{
			__Error.ReaderClosed();
		}
		CheckAsyncTaskInProgress();
		return (Task<string>)(_asyncReadTask = ReadLineAsyncInternal());
	}

	private async Task<string> ReadLineAsyncInternal()
	{
		bool flag = CharPos_Prop == CharLen_Prop;
		if (flag)
		{
			flag = await ReadBufferAsync().ConfigureAwait(continueOnCapturedContext: false) == 0;
		}
		if (flag)
		{
			return null;
		}
		StringBuilder sb = null;
		do
		{
			char[] tmpCharBuffer = CharBuffer_Prop;
			int tmpCharLen = CharLen_Prop;
			int tmpCharPos = CharPos_Prop;
			int i = tmpCharPos;
			do
			{
				char c = tmpCharBuffer[i];
				if (c == '\r' || c == '\n')
				{
					string s;
					if (sb != null)
					{
						sb.Append(tmpCharBuffer, tmpCharPos, i - tmpCharPos);
						s = sb.ToString();
					}
					else
					{
						s = new string(tmpCharBuffer, tmpCharPos, i - tmpCharPos);
					}
					StreamReader streamReader = this;
					int charPos_Prop;
					tmpCharPos = (charPos_Prop = i + 1);
					streamReader.CharPos_Prop = charPos_Prop;
					flag = c == '\r';
					if (flag)
					{
						bool flag2 = tmpCharPos < tmpCharLen;
						if (!flag2)
						{
							flag2 = await ReadBufferAsync().ConfigureAwait(continueOnCapturedContext: false) > 0;
						}
						flag = flag2;
					}
					if (flag)
					{
						tmpCharPos = CharPos_Prop;
						if (CharBuffer_Prop[tmpCharPos] == '\n')
						{
							StreamReader streamReader2 = this;
							charPos_Prop = tmpCharPos + 1;
							streamReader2.CharPos_Prop = charPos_Prop;
						}
					}
					return s;
				}
				i++;
			}
			while (i < tmpCharLen);
			i = tmpCharLen - tmpCharPos;
			if (sb == null)
			{
				sb = new StringBuilder(i + 80);
			}
			sb.Append(tmpCharBuffer, tmpCharPos, i);
		}
		while (await ReadBufferAsync().ConfigureAwait(continueOnCapturedContext: false) > 0);
		return sb.ToString();
	}

	/// <summary>Reads all characters from the current position to the end of the stream asynchronously and returns them as one string.</summary>
	/// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains a string with the characters from the current position to the end of the stream.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The number of characters is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The reader is currently in use by a previous read operation. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public override Task<string> ReadToEndAsync()
	{
		if (GetType() != typeof(StreamReader))
		{
			return base.ReadToEndAsync();
		}
		if (stream == null)
		{
			__Error.ReaderClosed();
		}
		CheckAsyncTaskInProgress();
		return (Task<string>)(_asyncReadTask = ReadToEndAsyncInternal());
	}

	private async Task<string> ReadToEndAsyncInternal()
	{
		StringBuilder sb = new StringBuilder(CharLen_Prop - CharPos_Prop);
		do
		{
			int charPos_Prop = CharPos_Prop;
			sb.Append(CharBuffer_Prop, charPos_Prop, CharLen_Prop - charPos_Prop);
			CharPos_Prop = CharLen_Prop;
			await ReadBufferAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		while (CharLen_Prop > 0);
		return sb.ToString();
	}

	/// <summary>Reads a specified maximum number of characters from the current stream asynchronously and writes the data to a buffer, beginning at the specified index. </summary>
	/// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached.</returns>
	/// <param name="buffer">When this method returns, contains the specified character array with the values between <paramref name="index" /> and (<paramref name="index" /> + <paramref name="count" /> - 1) replaced by the characters read from the current source.</param>
	/// <param name="index">The position in <paramref name="buffer" /> at which to begin writing.</param>
	/// <param name="count">The maximum number of characters to read. If the end of the stream is reached before the specified number of characters is written into the buffer, the current method returns.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative.</exception>
	/// <exception cref="T:System.ArgumentException">The sum of <paramref name="index" /> and <paramref name="count" /> is larger than the buffer length.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The reader is currently in use by a previous read operation. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
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
		if (GetType() != typeof(StreamReader))
		{
			return base.ReadAsync(buffer, index, count);
		}
		if (stream == null)
		{
			__Error.ReaderClosed();
		}
		CheckAsyncTaskInProgress();
		return (Task<int>)(_asyncReadTask = ReadAsyncInternal(buffer, index, count));
	}

	internal override async Task<int> ReadAsyncInternal(char[] buffer, int index, int count)
	{
		bool flag = CharPos_Prop == CharLen_Prop;
		if (flag)
		{
			flag = await ReadBufferAsync().ConfigureAwait(continueOnCapturedContext: false) == 0;
		}
		if (flag)
		{
			return 0;
		}
		int charsRead = 0;
		bool readToUserBuffer = false;
		byte[] tmpByteBuffer = ByteBuffer_Prop;
		Stream tmpStream = Stream_Prop;
		while (count > 0)
		{
			int n = CharLen_Prop - CharPos_Prop;
			if (n == 0)
			{
				CharLen_Prop = 0;
				CharPos_Prop = 0;
				if (!CheckPreamble_Prop)
				{
					ByteLen_Prop = 0;
				}
				readToUserBuffer = count >= MaxCharsPerBuffer_Prop;
				do
				{
					if (CheckPreamble_Prop)
					{
						int bytePos_Prop = BytePos_Prop;
						int num = await tmpStream.ReadAsync(tmpByteBuffer, bytePos_Prop, tmpByteBuffer.Length - bytePos_Prop).ConfigureAwait(continueOnCapturedContext: false);
						if (num == 0)
						{
							if (ByteLen_Prop > 0)
							{
								if (readToUserBuffer)
								{
									n = Decoder_Prop.GetChars(tmpByteBuffer, 0, ByteLen_Prop, buffer, index + charsRead);
									CharLen_Prop = 0;
								}
								else
								{
									n = Decoder_Prop.GetChars(tmpByteBuffer, 0, ByteLen_Prop, CharBuffer_Prop, 0);
									CharLen_Prop += n;
								}
							}
							IsBlocked_Prop = true;
							break;
						}
						ByteLen_Prop += num;
					}
					else
					{
						ByteLen_Prop = await tmpStream.ReadAsync(tmpByteBuffer, 0, tmpByteBuffer.Length).ConfigureAwait(continueOnCapturedContext: false);
						if (ByteLen_Prop == 0)
						{
							IsBlocked_Prop = true;
							break;
						}
					}
					IsBlocked_Prop = ByteLen_Prop < tmpByteBuffer.Length;
					if (!IsPreamble())
					{
						if (DetectEncoding_Prop && ByteLen_Prop >= 2)
						{
							DetectEncoding();
							readToUserBuffer = count >= MaxCharsPerBuffer_Prop;
						}
						CharPos_Prop = 0;
						if (readToUserBuffer)
						{
							n += Decoder_Prop.GetChars(tmpByteBuffer, 0, ByteLen_Prop, buffer, index + charsRead);
							CharLen_Prop = 0;
						}
						else
						{
							n = Decoder_Prop.GetChars(tmpByteBuffer, 0, ByteLen_Prop, CharBuffer_Prop, 0);
							CharLen_Prop += n;
						}
					}
				}
				while (n == 0);
				if (n == 0)
				{
					break;
				}
			}
			if (n > count)
			{
				n = count;
			}
			if (!readToUserBuffer)
			{
				Buffer.InternalBlockCopy(CharBuffer_Prop, CharPos_Prop * 2, buffer, (index + charsRead) * 2, n * 2);
				CharPos_Prop += n;
			}
			charsRead += n;
			count -= n;
			if (IsBlocked_Prop)
			{
				break;
			}
		}
		return charsRead;
	}

	/// <summary>Reads a specified maximum number of characters from the current stream asynchronously and writes the data to a buffer, beginning at the specified index.</summary>
	/// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached.</returns>
	/// <param name="buffer">When this method returns, contains the specified character array with the values between <paramref name="index" /> and (<paramref name="index" /> + <paramref name="count" /> - 1) replaced by the characters read from the current source.</param>
	/// <param name="index">The position in <paramref name="buffer" /> at which to begin writing.</param>
	/// <param name="count">The maximum number of characters to read. If the end of the stream is reached before the specified number of characters is written into the buffer, the method returns.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative.</exception>
	/// <exception cref="T:System.ArgumentException">The sum of <paramref name="index" /> and <paramref name="count" /> is larger than the buffer length.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The reader is currently in use by a previous read operation. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
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
		if (GetType() != typeof(StreamReader))
		{
			return base.ReadBlockAsync(buffer, index, count);
		}
		if (stream == null)
		{
			__Error.ReaderClosed();
		}
		CheckAsyncTaskInProgress();
		return (Task<int>)(_asyncReadTask = base.ReadBlockAsync(buffer, index, count));
	}

	private async Task<int> ReadBufferAsync()
	{
		CharLen_Prop = 0;
		CharPos_Prop = 0;
		byte[] tmpByteBuffer = ByteBuffer_Prop;
		Stream tmpStream = Stream_Prop;
		if (!CheckPreamble_Prop)
		{
			ByteLen_Prop = 0;
		}
		do
		{
			if (CheckPreamble_Prop)
			{
				int bytePos_Prop = BytePos_Prop;
				int num = await tmpStream.ReadAsync(tmpByteBuffer, bytePos_Prop, tmpByteBuffer.Length - bytePos_Prop).ConfigureAwait(continueOnCapturedContext: false);
				if (num == 0)
				{
					if (ByteLen_Prop > 0)
					{
						CharLen_Prop += Decoder_Prop.GetChars(tmpByteBuffer, 0, ByteLen_Prop, CharBuffer_Prop, CharLen_Prop);
						BytePos_Prop = 0;
						ByteLen_Prop = 0;
					}
					return CharLen_Prop;
				}
				ByteLen_Prop += num;
			}
			else
			{
				ByteLen_Prop = await tmpStream.ReadAsync(tmpByteBuffer, 0, tmpByteBuffer.Length).ConfigureAwait(continueOnCapturedContext: false);
				if (ByteLen_Prop == 0)
				{
					return CharLen_Prop;
				}
			}
			IsBlocked_Prop = ByteLen_Prop < tmpByteBuffer.Length;
			if (!IsPreamble())
			{
				if (DetectEncoding_Prop && ByteLen_Prop >= 2)
				{
					DetectEncoding();
				}
				CharLen_Prop += Decoder_Prop.GetChars(tmpByteBuffer, 0, ByteLen_Prop, CharBuffer_Prop, CharLen_Prop);
			}
		}
		while (CharLen_Prop == 0);
		return CharLen_Prop;
	}
}
