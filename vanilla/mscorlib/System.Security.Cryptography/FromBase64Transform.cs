using System.Runtime.InteropServices;
using System.Text;

namespace System.Security.Cryptography;

/// <summary>Converts a <see cref="T:System.Security.Cryptography.CryptoStream" /> from base 64.</summary>
[ComVisible(true)]
public class FromBase64Transform : ICryptoTransform, IDisposable
{
	private byte[] _inputBuffer = new byte[4];

	private int _inputIndex;

	private FromBase64TransformMode _whitespaces;

	/// <summary>Gets the input block size.</summary>
	/// <returns>The size of the input data blocks in bytes.</returns>
	public int InputBlockSize => 1;

	/// <summary>Gets the output block size.</summary>
	/// <returns>The size of the output data blocks in bytes.</returns>
	public int OutputBlockSize => 3;

	/// <summary>Gets a value that indicates whether multiple blocks can be transformed.</summary>
	/// <returns>Always false.</returns>
	public bool CanTransformMultipleBlocks => false;

	/// <summary>Gets a value indicating whether the current transform can be reused.</summary>
	/// <returns>Always true.</returns>
	public virtual bool CanReuseTransform => true;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.FromBase64Transform" /> class.</summary>
	public FromBase64Transform()
		: this(FromBase64TransformMode.IgnoreWhiteSpaces)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.FromBase64Transform" /> class with the specified transformation mode.</summary>
	/// <param name="whitespaces">One of the <see cref="T:System.Security.Cryptography.FromBase64Transform" /> values. </param>
	public FromBase64Transform(FromBase64TransformMode whitespaces)
	{
		_whitespaces = whitespaces;
		_inputIndex = 0;
	}

	/// <summary>Converts the specified region of the input byte array from base 64 and copies the result to the specified region of the output byte array.</summary>
	/// <returns>The number of bytes written.</returns>
	/// <param name="inputBuffer">The input to compute from base 64. </param>
	/// <param name="inputOffset">The offset into the input byte array from which to begin using data. </param>
	/// <param name="inputCount">The number of bytes in the input byte array to use as data. </param>
	/// <param name="outputBuffer">The output to which to write the result. </param>
	/// <param name="outputOffset">The offset into the output byte array from which to begin writing data. </param>
	/// <exception cref="T:System.ObjectDisposedException">The current <see cref="T:System.Security.Cryptography.FromBase64Transform" /> object has already been disposed. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="inputCount" /> uses an invalid value.-or-<paramref name="inputBuffer" /> has an invalid offset length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="inputOffset" /> is out of range. This parameter requires a non-negative number.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="inputBuffer" /> is null.</exception>
	public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
	{
		if (inputBuffer == null)
		{
			throw new ArgumentNullException("inputBuffer");
		}
		if (inputOffset < 0)
		{
			throw new ArgumentOutOfRangeException("inputOffset", Environment.GetResourceString("Non-negative number required."));
		}
		if (inputCount < 0 || inputCount > inputBuffer.Length)
		{
			throw new ArgumentException(Environment.GetResourceString("Value was invalid."));
		}
		if (inputBuffer.Length - inputCount < inputOffset)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		if (_inputBuffer == null)
		{
			throw new ObjectDisposedException(null, Environment.GetResourceString("Cannot access a disposed object."));
		}
		byte[] array = new byte[inputCount];
		int num;
		if (_whitespaces == FromBase64TransformMode.IgnoreWhiteSpaces)
		{
			array = DiscardWhiteSpaces(inputBuffer, inputOffset, inputCount);
			num = array.Length;
		}
		else
		{
			Buffer.InternalBlockCopy(inputBuffer, inputOffset, array, 0, inputCount);
			num = inputCount;
		}
		if (num + _inputIndex < 4)
		{
			Buffer.InternalBlockCopy(array, 0, _inputBuffer, _inputIndex, num);
			_inputIndex += num;
			return 0;
		}
		int num2 = (num + _inputIndex) / 4;
		byte[] array2 = new byte[_inputIndex + num];
		Buffer.InternalBlockCopy(_inputBuffer, 0, array2, 0, _inputIndex);
		Buffer.InternalBlockCopy(array, 0, array2, _inputIndex, num);
		_inputIndex = (num + _inputIndex) % 4;
		Buffer.InternalBlockCopy(array, num - _inputIndex, _inputBuffer, 0, _inputIndex);
		byte[] array3 = Convert.FromBase64CharArray(Encoding.ASCII.GetChars(array2, 0, 4 * num2), 0, 4 * num2);
		Buffer.BlockCopy(array3, 0, outputBuffer, outputOffset, array3.Length);
		return array3.Length;
	}

	/// <summary>Converts the specified region of the specified byte array from base 64.</summary>
	/// <returns>The computed conversion.</returns>
	/// <param name="inputBuffer">The input to convert from base 64. </param>
	/// <param name="inputOffset">The offset into the byte array from which to begin using data. </param>
	/// <param name="inputCount">The number of bytes in the byte array to use as data. </param>
	/// <exception cref="T:System.ObjectDisposedException">The current <see cref="T:System.Security.Cryptography.FromBase64Transform" /> object has already been disposed. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="inputBuffer" /> has an invalid offset length.-or-<paramref name="inputCount" /> has an invalid value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="inputOffset" /> is out of range. This parameter requires a non-negative number.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="inputBuffer" /> is null.</exception>
	public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
	{
		if (inputBuffer == null)
		{
			throw new ArgumentNullException("inputBuffer");
		}
		if (inputOffset < 0)
		{
			throw new ArgumentOutOfRangeException("inputOffset", Environment.GetResourceString("Non-negative number required."));
		}
		if (inputCount < 0 || inputCount > inputBuffer.Length)
		{
			throw new ArgumentException(Environment.GetResourceString("Value was invalid."));
		}
		if (inputBuffer.Length - inputCount < inputOffset)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		if (_inputBuffer == null)
		{
			throw new ObjectDisposedException(null, Environment.GetResourceString("Cannot access a disposed object."));
		}
		byte[] array = new byte[inputCount];
		int num;
		if (_whitespaces == FromBase64TransformMode.IgnoreWhiteSpaces)
		{
			array = DiscardWhiteSpaces(inputBuffer, inputOffset, inputCount);
			num = array.Length;
		}
		else
		{
			Buffer.InternalBlockCopy(inputBuffer, inputOffset, array, 0, inputCount);
			num = inputCount;
		}
		if (num + _inputIndex < 4)
		{
			Reset();
			return EmptyArray<byte>.Value;
		}
		int num2 = (num + _inputIndex) / 4;
		byte[] array2 = new byte[_inputIndex + num];
		Buffer.InternalBlockCopy(_inputBuffer, 0, array2, 0, _inputIndex);
		Buffer.InternalBlockCopy(array, 0, array2, _inputIndex, num);
		_inputIndex = (num + _inputIndex) % 4;
		Buffer.InternalBlockCopy(array, num - _inputIndex, _inputBuffer, 0, _inputIndex);
		byte[] result = Convert.FromBase64CharArray(Encoding.ASCII.GetChars(array2, 0, 4 * num2), 0, 4 * num2);
		Reset();
		return result;
	}

	private byte[] DiscardWhiteSpaces(byte[] inputBuffer, int inputOffset, int inputCount)
	{
		int num = 0;
		for (int i = 0; i < inputCount; i++)
		{
			if (char.IsWhiteSpace((char)inputBuffer[inputOffset + i]))
			{
				num++;
			}
		}
		byte[] array = new byte[inputCount - num];
		num = 0;
		for (int i = 0; i < inputCount; i++)
		{
			if (!char.IsWhiteSpace((char)inputBuffer[inputOffset + i]))
			{
				array[num++] = inputBuffer[inputOffset + i];
			}
		}
		return array;
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.Security.Cryptography.FromBase64Transform" /> class.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Reset()
	{
		_inputIndex = 0;
	}

	/// <summary>Releases all resources used by the <see cref="T:System.Security.Cryptography.FromBase64Transform" />.</summary>
	public void Clear()
	{
		Dispose();
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Security.Cryptography.FromBase64Transform" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (_inputBuffer != null)
			{
				Array.Clear(_inputBuffer, 0, _inputBuffer.Length);
			}
			_inputBuffer = null;
			_inputIndex = 0;
		}
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Security.Cryptography.FromBase64Transform" />.</summary>
	~FromBase64Transform()
	{
		Dispose(disposing: false);
	}
}
