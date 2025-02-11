using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO;

/// <summary>Represents a writer that can write a sequential series of characters. This class is abstract.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public abstract class TextWriter : MarshalByRefObject, IDisposable
{
	[Serializable]
	private sealed class NullTextWriter : TextWriter
	{
		public override Encoding Encoding => Encoding.Default;

		internal NullTextWriter()
			: base(CultureInfo.InvariantCulture)
		{
		}

		public override void Write(char[] buffer, int index, int count)
		{
		}

		public override void Write(string value)
		{
		}

		public override void WriteLine()
		{
		}

		public override void WriteLine(string value)
		{
		}

		public override void WriteLine(object value)
		{
		}
	}

	[Serializable]
	internal sealed class SyncTextWriter : TextWriter, IDisposable
	{
		private TextWriter _out;

		public override Encoding Encoding => _out.Encoding;

		public override IFormatProvider FormatProvider => _out.FormatProvider;

		public override string NewLine
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			get
			{
				return _out.NewLine;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				_out.NewLine = value;
			}
		}

		internal SyncTextWriter(TextWriter t)
			: base(t.FormatProvider)
		{
			_out = t;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void Close()
		{
			_out.Close();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				((IDisposable)_out).Dispose();
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void Flush()
		{
			_out.Flush();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void Write(char value)
		{
			_out.Write(value);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void Write(char[] buffer)
		{
			_out.Write(buffer);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void Write(char[] buffer, int index, int count)
		{
			_out.Write(buffer, index, count);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void Write(bool value)
		{
			_out.Write(value);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void Write(int value)
		{
			_out.Write(value);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void Write(uint value)
		{
			_out.Write(value);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void Write(long value)
		{
			_out.Write(value);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void Write(ulong value)
		{
			_out.Write(value);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void Write(float value)
		{
			_out.Write(value);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void Write(double value)
		{
			_out.Write(value);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void Write(decimal value)
		{
			_out.Write(value);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void Write(string value)
		{
			_out.Write(value);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void Write(object value)
		{
			_out.Write(value);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void Write(string format, object arg0)
		{
			_out.Write(format, arg0);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void Write(string format, object arg0, object arg1)
		{
			_out.Write(format, arg0, arg1);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void Write(string format, object arg0, object arg1, object arg2)
		{
			_out.Write(format, arg0, arg1, arg2);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void Write(string format, params object[] arg)
		{
			_out.Write(format, arg);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void WriteLine()
		{
			_out.WriteLine();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void WriteLine(char value)
		{
			_out.WriteLine(value);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void WriteLine(decimal value)
		{
			_out.WriteLine(value);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void WriteLine(char[] buffer)
		{
			_out.WriteLine(buffer);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void WriteLine(char[] buffer, int index, int count)
		{
			_out.WriteLine(buffer, index, count);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void WriteLine(bool value)
		{
			_out.WriteLine(value);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void WriteLine(int value)
		{
			_out.WriteLine(value);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void WriteLine(uint value)
		{
			_out.WriteLine(value);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void WriteLine(long value)
		{
			_out.WriteLine(value);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void WriteLine(ulong value)
		{
			_out.WriteLine(value);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void WriteLine(float value)
		{
			_out.WriteLine(value);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void WriteLine(double value)
		{
			_out.WriteLine(value);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void WriteLine(string value)
		{
			_out.WriteLine(value);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void WriteLine(object value)
		{
			_out.WriteLine(value);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void WriteLine(string format, object arg0)
		{
			_out.WriteLine(format, arg0);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void WriteLine(string format, object arg0, object arg1)
		{
			_out.WriteLine(format, arg0, arg1);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void WriteLine(string format, object arg0, object arg1, object arg2)
		{
			_out.WriteLine(format, arg0, arg1, arg2);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void WriteLine(string format, params object[] arg)
		{
			_out.WriteLine(format, arg);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		[ComVisible(false)]
		public override Task WriteAsync(char value)
		{
			Write(value);
			return Task.CompletedTask;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		[ComVisible(false)]
		public override Task WriteAsync(string value)
		{
			Write(value);
			return Task.CompletedTask;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		[ComVisible(false)]
		public override Task WriteAsync(char[] buffer, int index, int count)
		{
			Write(buffer, index, count);
			return Task.CompletedTask;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		[ComVisible(false)]
		public override Task WriteLineAsync(char value)
		{
			WriteLine(value);
			return Task.CompletedTask;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		[ComVisible(false)]
		public override Task WriteLineAsync(string value)
		{
			WriteLine(value);
			return Task.CompletedTask;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		[ComVisible(false)]
		public override Task WriteLineAsync(char[] buffer, int index, int count)
		{
			WriteLine(buffer, index, count);
			return Task.CompletedTask;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		[ComVisible(false)]
		public override Task FlushAsync()
		{
			Flush();
			return Task.CompletedTask;
		}
	}

	/// <summary>Provides a TextWriter with no backing store that can be written to, but not read from.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly TextWriter Null = new NullTextWriter();

	[NonSerialized]
	private static Action<object> _WriteCharDelegate = delegate(object state)
	{
		Tuple<TextWriter, char> tuple = (Tuple<TextWriter, char>)state;
		tuple.Item1.Write(tuple.Item2);
	};

	[NonSerialized]
	private static Action<object> _WriteStringDelegate = delegate(object state)
	{
		Tuple<TextWriter, string> tuple2 = (Tuple<TextWriter, string>)state;
		tuple2.Item1.Write(tuple2.Item2);
	};

	[NonSerialized]
	private static Action<object> _WriteCharArrayRangeDelegate = delegate(object state)
	{
		Tuple<TextWriter, char[], int, int> tuple3 = (Tuple<TextWriter, char[], int, int>)state;
		tuple3.Item1.Write(tuple3.Item2, tuple3.Item3, tuple3.Item4);
	};

	[NonSerialized]
	private static Action<object> _WriteLineCharDelegate = delegate(object state)
	{
		Tuple<TextWriter, char> tuple4 = (Tuple<TextWriter, char>)state;
		tuple4.Item1.WriteLine(tuple4.Item2);
	};

	[NonSerialized]
	private static Action<object> _WriteLineStringDelegate = delegate(object state)
	{
		Tuple<TextWriter, string> tuple5 = (Tuple<TextWriter, string>)state;
		tuple5.Item1.WriteLine(tuple5.Item2);
	};

	[NonSerialized]
	private static Action<object> _WriteLineCharArrayRangeDelegate = delegate(object state)
	{
		Tuple<TextWriter, char[], int, int> tuple6 = (Tuple<TextWriter, char[], int, int>)state;
		tuple6.Item1.WriteLine(tuple6.Item2, tuple6.Item3, tuple6.Item4);
	};

	[NonSerialized]
	private static Action<object> _FlushDelegate = delegate(object state)
	{
		((TextWriter)state).Flush();
	};

	/// <summary>Stores the newline characters used for this TextWriter.</summary>
	protected char[] CoreNewLine = InitialNewLine.ToCharArray();

	private IFormatProvider InternalFormatProvider;

	private static string InitialNewLine => Environment.NewLine;

	/// <summary>Gets an object that controls formatting.</summary>
	/// <returns>An <see cref="T:System.IFormatProvider" /> object for a specific culture, or the formatting of the current culture if no other culture is specified.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual IFormatProvider FormatProvider
	{
		get
		{
			if (InternalFormatProvider == null)
			{
				return Thread.CurrentThread.CurrentCulture;
			}
			return InternalFormatProvider;
		}
	}

	/// <summary>When overridden in a derived class, returns the character encoding in which the output is written.</summary>
	/// <returns>The character encoding in which the output is written.</returns>
	/// <filterpriority>1</filterpriority>
	public abstract Encoding Encoding { get; }

	/// <summary>Gets or sets the line terminator string used by the current TextWriter.</summary>
	/// <returns>The line terminator string for the current TextWriter.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual string NewLine
	{
		get
		{
			return new string(CoreNewLine);
		}
		set
		{
			if (value == null)
			{
				value = InitialNewLine;
			}
			CoreNewLine = value.ToCharArray();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.TextWriter" /> class.</summary>
	protected TextWriter()
	{
		InternalFormatProvider = null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.TextWriter" /> class with the specified format provider.</summary>
	/// <param name="formatProvider">An <see cref="T:System.IFormatProvider" /> object that controls formatting. </param>
	protected TextWriter(IFormatProvider formatProvider)
	{
		InternalFormatProvider = formatProvider;
	}

	/// <summary>Closes the current writer and releases any system resources associated with the writer.</summary>
	/// <filterpriority>1</filterpriority>
	public virtual void Close()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.TextWriter" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
	}

	/// <summary>Releases all resources used by the <see cref="T:System.IO.TextWriter" /> object.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Clears all buffers for the current writer and causes any buffered data to be written to the underlying device.</summary>
	/// <filterpriority>1</filterpriority>
	public virtual void Flush()
	{
	}

	/// <summary>Creates a thread-safe wrapper around the specified TextWriter.</summary>
	/// <returns>A thread-safe wrapper.</returns>
	/// <param name="writer">The TextWriter to synchronize. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="writer" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
	public static TextWriter Synchronized(TextWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (writer is SyncTextWriter)
		{
			return writer;
		}
		return new SyncTextWriter(writer);
	}

	/// <summary>Writes a character to the text string or stream.</summary>
	/// <param name="value">The character to write to the text stream. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(char value)
	{
	}

	/// <summary>Writes a character array to the text string or stream.</summary>
	/// <param name="buffer">The character array to write to the text stream. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(char[] buffer)
	{
		if (buffer != null)
		{
			Write(buffer, 0, buffer.Length);
		}
	}

	/// <summary>Writes a subarray of characters to the text string or stream.</summary>
	/// <param name="buffer">The character array to write data from. </param>
	/// <param name="index">The character position in the buffer at which to start retrieving data. </param>
	/// <param name="count">The number of characters to write. </param>
	/// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="index" /> is less than <paramref name="count" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="buffer" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(char[] buffer, int index, int count)
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
		for (int i = 0; i < count; i++)
		{
			Write(buffer[index + i]);
		}
	}

	/// <summary>Writes the text representation of a Boolean value to the text string or stream.</summary>
	/// <param name="value">The Boolean value to write. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(bool value)
	{
		Write(value ? "True" : "False");
	}

	/// <summary>Writes the text representation of a 4-byte signed integer to the text string or stream.</summary>
	/// <param name="value">The 4-byte signed integer to write. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(int value)
	{
		Write(value.ToString(FormatProvider));
	}

	/// <summary>Writes the text representation of a 4-byte unsigned integer to the text string or stream.</summary>
	/// <param name="value">The 4-byte unsigned integer to write. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public virtual void Write(uint value)
	{
		Write(value.ToString(FormatProvider));
	}

	/// <summary>Writes the text representation of an 8-byte signed integer to the text string or stream.</summary>
	/// <param name="value">The 8-byte signed integer to write. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(long value)
	{
		Write(value.ToString(FormatProvider));
	}

	/// <summary>Writes the text representation of an 8-byte unsigned integer to the text string or stream.</summary>
	/// <param name="value">The 8-byte unsigned integer to write. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public virtual void Write(ulong value)
	{
		Write(value.ToString(FormatProvider));
	}

	/// <summary>Writes the text representation of a 4-byte floating-point value to the text string or stream.</summary>
	/// <param name="value">The 4-byte floating-point value to write. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(float value)
	{
		Write(value.ToString(FormatProvider));
	}

	/// <summary>Writes the text representation of an 8-byte floating-point value to the text string or stream.</summary>
	/// <param name="value">The 8-byte floating-point value to write. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(double value)
	{
		Write(value.ToString(FormatProvider));
	}

	/// <summary>Writes the text representation of a decimal value to the text string or stream.</summary>
	/// <param name="value">The decimal value to write. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(decimal value)
	{
		Write(value.ToString(FormatProvider));
	}

	/// <summary>Writes a string to the text string or stream.</summary>
	/// <param name="value">The string to write. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(string value)
	{
		if (value != null)
		{
			Write(value.ToCharArray());
		}
	}

	/// <summary>Writes the text representation of an object to the text string or stream by calling the ToString method on that object.</summary>
	/// <param name="value">The object to write. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(object value)
	{
		if (value != null)
		{
			if (value is IFormattable formattable)
			{
				Write(formattable.ToString(null, FormatProvider));
			}
			else
			{
				Write(value.ToString());
			}
		}
	}

	/// <summary>Writes a formatted string to the text string or stream, using the same semantics as the <see cref="M:System.String.Format(System.String,System.Object)" /> method.</summary>
	/// <param name="format">A composite format string (see Remarks). </param>
	/// <param name="arg0">The object to format and write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is not a valid composite format string.-or- The index of a format item is less than 0 (zero), or greater than or equal to the number of objects to be formatted (which, for this method overload, is one). </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(string format, object arg0)
	{
		Write(string.Format(FormatProvider, format, arg0));
	}

	/// <summary>Writes a formatted string to the text string or stream, using the same semantics as the <see cref="M:System.String.Format(System.String,System.Object,System.Object)" /> method.</summary>
	/// <param name="format">A composite format string (see Remarks). </param>
	/// <param name="arg0">The first object to format and write. </param>
	/// <param name="arg1">The second object to format and write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is not a valid composite format string.-or- The index of a format item is less than 0 (zero) or greater than or equal to the number of objects to be formatted (which, for this method overload, is two). </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(string format, object arg0, object arg1)
	{
		Write(string.Format(FormatProvider, format, arg0, arg1));
	}

	/// <summary>Writes a formatted string to the text string or stream, using the same semantics as the <see cref="M:System.String.Format(System.String,System.Object,System.Object,System.Object)" /> method.</summary>
	/// <param name="format">A composite format string (see Remarks). </param>
	/// <param name="arg0">The first object to format and write. </param>
	/// <param name="arg1">The second object to format and write. </param>
	/// <param name="arg2">The third object to format and write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is not a valid composite format string.-or- The index of a format item is less than 0 (zero), or greater than or equal to the number of objects to be formatted (which, for this method overload, is three). </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(string format, object arg0, object arg1, object arg2)
	{
		Write(string.Format(FormatProvider, format, arg0, arg1, arg2));
	}

	/// <summary>Writes a formatted string to the text string or stream, using the same semantics as the <see cref="M:System.String.Format(System.String,System.Object[])" /> method.</summary>
	/// <param name="format">A composite format string (see Remarks). </param>
	/// <param name="arg">An object array that contains zero or more objects to format and write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> or <paramref name="arg" /> is null. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is not a valid composite format string.-or- The index of a format item is less than 0 (zero), or greater than or equal to the length of the <paramref name="arg" /> array. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(string format, params object[] arg)
	{
		Write(string.Format(FormatProvider, format, arg));
	}

	/// <summary>Writes a line terminator to the text string or stream.</summary>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void WriteLine()
	{
		Write(CoreNewLine);
	}

	/// <summary>Writes a character followed by a line terminator to the text string or stream.</summary>
	/// <param name="value">The character to write to the text stream. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void WriteLine(char value)
	{
		Write(value);
		WriteLine();
	}

	/// <summary>Writes an array of characters followed by a line terminator to the text string or stream.</summary>
	/// <param name="buffer">The character array from which data is read. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void WriteLine(char[] buffer)
	{
		Write(buffer);
		WriteLine();
	}

	/// <summary>Writes a subarray of characters followed by a line terminator to the text string or stream.</summary>
	/// <param name="buffer">The character array from which data is read. </param>
	/// <param name="index">The character position in <paramref name="buffer" /> at which to start reading data. </param>
	/// <param name="count">The maximum number of characters to write. </param>
	/// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="index" /> is less than <paramref name="count" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="buffer" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void WriteLine(char[] buffer, int index, int count)
	{
		Write(buffer, index, count);
		WriteLine();
	}

	/// <summary>Writes the text representation of a Boolean value followed by a line terminator to the text string or stream.</summary>
	/// <param name="value">The Boolean value to write. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void WriteLine(bool value)
	{
		Write(value);
		WriteLine();
	}

	/// <summary>Writes the text representation of a 4-byte signed integer followed by a line terminator to the text string or stream.</summary>
	/// <param name="value">The 4-byte signed integer to write. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void WriteLine(int value)
	{
		Write(value);
		WriteLine();
	}

	/// <summary>Writes the text representation of a 4-byte unsigned integer followed by a line terminator to the text string or stream.</summary>
	/// <param name="value">The 4-byte unsigned integer to write. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public virtual void WriteLine(uint value)
	{
		Write(value);
		WriteLine();
	}

	/// <summary>Writes the text representation of an 8-byte signed integer followed by a line terminator to the text string or stream.</summary>
	/// <param name="value">The 8-byte signed integer to write. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void WriteLine(long value)
	{
		Write(value);
		WriteLine();
	}

	/// <summary>Writes the text representation of an 8-byte unsigned integer followed by a line terminator to the text string or stream.</summary>
	/// <param name="value">The 8-byte unsigned integer to write. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public virtual void WriteLine(ulong value)
	{
		Write(value);
		WriteLine();
	}

	/// <summary>Writes the text representation of a 4-byte floating-point value followed by a line terminator to the text string or stream.</summary>
	/// <param name="value">The 4-byte floating-point value to write. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void WriteLine(float value)
	{
		Write(value);
		WriteLine();
	}

	/// <summary>Writes the text representation of a 8-byte floating-point value followed by a line terminator to the text string or stream.</summary>
	/// <param name="value">The 8-byte floating-point value to write. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void WriteLine(double value)
	{
		Write(value);
		WriteLine();
	}

	/// <summary>Writes the text representation of a decimal value followed by a line terminator to the text string or stream.</summary>
	/// <param name="value">The decimal value to write. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void WriteLine(decimal value)
	{
		Write(value);
		WriteLine();
	}

	/// <summary>Writes a string followed by a line terminator to the text string or stream.</summary>
	/// <param name="value">The string to write. If <paramref name="value" /> is null, only the line terminator is written. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void WriteLine(string value)
	{
		if (value == null)
		{
			WriteLine();
			return;
		}
		int length = value.Length;
		int num = CoreNewLine.Length;
		char[] array = new char[length + num];
		value.CopyTo(0, array, 0, length);
		switch (num)
		{
		case 2:
			array[length] = CoreNewLine[0];
			array[length + 1] = CoreNewLine[1];
			break;
		case 1:
			array[length] = CoreNewLine[0];
			break;
		default:
			Buffer.InternalBlockCopy(CoreNewLine, 0, array, length * 2, num * 2);
			break;
		}
		Write(array, 0, length + num);
	}

	/// <summary>Writes the text representation of an object by calling the ToString method on that object, followed by a line terminator to the text string or stream.</summary>
	/// <param name="value">The object to write. If <paramref name="value" /> is null, only the line terminator is written. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void WriteLine(object value)
	{
		if (value == null)
		{
			WriteLine();
		}
		else if (value is IFormattable formattable)
		{
			WriteLine(formattable.ToString(null, FormatProvider));
		}
		else
		{
			WriteLine(value.ToString());
		}
	}

	/// <summary>Writes a formatted string and a new line to the text string or stream, using the same semantics as the <see cref="M:System.String.Format(System.String,System.Object)" /> method.</summary>
	/// <param name="format">A composite format string (see Remarks).</param>
	/// <param name="arg0">The object to format and write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is not a valid composite format string.-or- The index of a format item is less than 0 (zero), or greater than or equal to the number of objects to be formatted (which, for this method overload, is one). </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void WriteLine(string format, object arg0)
	{
		WriteLine(string.Format(FormatProvider, format, arg0));
	}

	/// <summary>Writes a formatted string and a new line to the text string or stream, using the same semantics as the <see cref="M:System.String.Format(System.String,System.Object,System.Object)" /> method.</summary>
	/// <param name="format">A composite format string (see Remarks).</param>
	/// <param name="arg0">The first object to format and write. </param>
	/// <param name="arg1">The second object to format and write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is not a valid composite format string.-or- The index of a format item is less than 0 (zero), or greater than or equal to the number of objects to be formatted (which, for this method overload, is two). </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void WriteLine(string format, object arg0, object arg1)
	{
		WriteLine(string.Format(FormatProvider, format, arg0, arg1));
	}

	/// <summary>Writes out a formatted string and a new line, using the same semantics as <see cref="M:System.String.Format(System.String,System.Object)" />.</summary>
	/// <param name="format">A composite format string (see Remarks).</param>
	/// <param name="arg0">The first object to format and write. </param>
	/// <param name="arg1">The second object to format and write. </param>
	/// <param name="arg2">The third object to format and write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is not a valid composite format string.-or- The index of a format item is less than 0 (zero), or greater than or equal to the number of objects to be formatted (which, for this method overload, is three). </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void WriteLine(string format, object arg0, object arg1, object arg2)
	{
		WriteLine(string.Format(FormatProvider, format, arg0, arg1, arg2));
	}

	/// <summary>Writes out a formatted string and a new line, using the same semantics as <see cref="M:System.String.Format(System.String,System.Object)" />.</summary>
	/// <param name="format">A composite format string (see Remarks).</param>
	/// <param name="arg">An object array that contains zero or more objects to format and write. </param>
	/// <exception cref="T:System.ArgumentNullException">A string or object is passed in as null. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is not a valid composite format string.-or- The index of a format item is less than 0 (zero), or greater than or equal to the length of the <paramref name="arg" /> array. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void WriteLine(string format, params object[] arg)
	{
		WriteLine(string.Format(FormatProvider, format, arg));
	}

	/// <summary>Writes a character to the text string or stream asynchronously.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="value">The character to write to the text stream.</param>
	/// <exception cref="T:System.ObjectDisposedException">The text writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The text writer is currently in use by a previous write operation. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public virtual Task WriteAsync(char value)
	{
		Tuple<TextWriter, char> state = new Tuple<TextWriter, char>(this, value);
		return Task.Factory.StartNew(_WriteCharDelegate, state, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
	}

	/// <summary>Writes a string to the text string or stream asynchronously.</summary>
	/// <returns>A task that represents the asynchronous write operation. </returns>
	/// <param name="value">The string to write. If <paramref name="value" /> is null, nothing is written to the text stream.</param>
	/// <exception cref="T:System.ObjectDisposedException">The text writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The text writer is currently in use by a previous write operation. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public virtual Task WriteAsync(string value)
	{
		Tuple<TextWriter, string> state = new Tuple<TextWriter, string>(this, value);
		return Task.Factory.StartNew(_WriteStringDelegate, state, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
	}

	/// <summary>Writes a character array to the text string or stream asynchronously.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="buffer">The character array to write to the text stream. If <paramref name="buffer" /> is null, nothing is written.</param>
	/// <exception cref="T:System.ObjectDisposedException">The text writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The text writer is currently in use by a previous write operation. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task WriteAsync(char[] buffer)
	{
		if (buffer == null)
		{
			return Task.CompletedTask;
		}
		return WriteAsync(buffer, 0, buffer.Length);
	}

	/// <summary>Writes a subarray of characters to the text string or stream asynchronously. </summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="buffer">The character array to write data from. </param>
	/// <param name="index">The character position in the buffer at which to start retrieving data. </param>
	/// <param name="count">The number of characters to write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="index" /> plus <paramref name="count" /> is greater than the buffer length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The text writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The text writer is currently in use by a previous write operation. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public virtual Task WriteAsync(char[] buffer, int index, int count)
	{
		Tuple<TextWriter, char[], int, int> state = new Tuple<TextWriter, char[], int, int>(this, buffer, index, count);
		return Task.Factory.StartNew(_WriteCharArrayRangeDelegate, state, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
	}

	/// <summary>Writes a character followed by a line terminator asynchronously to the text string or stream.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="value">The character to write to the text stream.</param>
	/// <exception cref="T:System.ObjectDisposedException">The text writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The text writer is currently in use by a previous write operation. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public virtual Task WriteLineAsync(char value)
	{
		Tuple<TextWriter, char> state = new Tuple<TextWriter, char>(this, value);
		return Task.Factory.StartNew(_WriteLineCharDelegate, state, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
	}

	/// <summary>Writes a string followed by a line terminator asynchronously to the text string or stream. </summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="value">The string to write. If the value is null, only a line terminator is written. </param>
	/// <exception cref="T:System.ObjectDisposedException">The text writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The text writer is currently in use by a previous write operation. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public virtual Task WriteLineAsync(string value)
	{
		Tuple<TextWriter, string> state = new Tuple<TextWriter, string>(this, value);
		return Task.Factory.StartNew(_WriteLineStringDelegate, state, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
	}

	/// <summary>Writes an array of characters followed by a line terminator asynchronously to the text string or stream.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="buffer">The character array to write to the text stream. If the character array is null, only the line terminator is written. </param>
	/// <exception cref="T:System.ObjectDisposedException">The text writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The text writer is currently in use by a previous write operation. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task WriteLineAsync(char[] buffer)
	{
		if (buffer == null)
		{
			return Task.CompletedTask;
		}
		return WriteLineAsync(buffer, 0, buffer.Length);
	}

	/// <summary>Writes a subarray of characters followed by a line terminator asynchronously to the text string or stream.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="buffer">The character array to write data from. </param>
	/// <param name="index">The character position in the buffer at which to start retrieving data. </param>
	/// <param name="count">The number of characters to write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="index" /> plus <paramref name="count" /> is greater than the buffer length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The text writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The text writer is currently in use by a previous write operation. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public virtual Task WriteLineAsync(char[] buffer, int index, int count)
	{
		Tuple<TextWriter, char[], int, int> state = new Tuple<TextWriter, char[], int, int>(this, buffer, index, count);
		return Task.Factory.StartNew(_WriteLineCharArrayRangeDelegate, state, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
	}

	/// <summary>Writes a line terminator asynchronously to the text string or stream.</summary>
	/// <returns>A task that represents the asynchronous write operation. </returns>
	/// <exception cref="T:System.ObjectDisposedException">The text writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The text writer is currently in use by a previous write operation. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public virtual Task WriteLineAsync()
	{
		return WriteAsync(CoreNewLine);
	}

	/// <summary>Asynchronously clears all buffers for the current writer and causes any buffered data to be written to the underlying device. </summary>
	/// <returns>A task that represents the asynchronous flush operation. </returns>
	/// <exception cref="T:System.ObjectDisposedException">The text writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The writer is currently in use by a previous write operation. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public virtual Task FlushAsync()
	{
		return Task.Factory.StartNew(_FlushDelegate, this, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
	}
}
