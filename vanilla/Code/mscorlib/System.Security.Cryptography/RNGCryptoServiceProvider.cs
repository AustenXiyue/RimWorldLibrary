using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Security.Cryptography;

/// <summary>Implements a cryptographic Random Number Generator (RNG) using the implementation provided by the cryptographic service provider (CSP). This class cannot be inherited.</summary>
[ComVisible(true)]
public sealed class RNGCryptoServiceProvider : RandomNumberGenerator
{
	private static object _lock;

	private IntPtr _handle;

	static RNGCryptoServiceProvider()
	{
		if (RngOpen())
		{
			_lock = new object();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.RNGCryptoServiceProvider" /> class.</summary>
	public RNGCryptoServiceProvider()
	{
		_handle = RngInitialize(null);
		Check();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.RNGCryptoServiceProvider" /> class.</summary>
	/// <param name="rgb">A byte array. This value is ignored.</param>
	public RNGCryptoServiceProvider(byte[] rgb)
	{
		_handle = RngInitialize(rgb);
		Check();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.RNGCryptoServiceProvider" /> class with the specified parameters.</summary>
	/// <param name="cspParams">The parameters to pass to the cryptographic service provider (CSP). </param>
	public RNGCryptoServiceProvider(CspParameters cspParams)
	{
		_handle = RngInitialize(null);
		Check();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.RNGCryptoServiceProvider" /> class.</summary>
	/// <param name="str">The string input. This parameter is ignored.</param>
	public RNGCryptoServiceProvider(string str)
	{
		if (str == null)
		{
			_handle = RngInitialize(null);
		}
		else
		{
			_handle = RngInitialize(Encoding.UTF8.GetBytes(str));
		}
		Check();
	}

	private void Check()
	{
		if (_handle == IntPtr.Zero)
		{
			throw new CryptographicException(Locale.GetText("Couldn't access random source."));
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool RngOpen();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern IntPtr RngInitialize(byte[] seed);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern IntPtr RngGetBytes(IntPtr handle, byte[] data);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void RngClose(IntPtr handle);

	/// <summary>Fills an array of bytes with a cryptographically strong sequence of random values.</summary>
	/// <param name="data">The array to fill with a cryptographically strong sequence of random values. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> is null.</exception>
	public override void GetBytes(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (_lock == null)
		{
			_handle = RngGetBytes(_handle, data);
		}
		else
		{
			lock (_lock)
			{
				_handle = RngGetBytes(_handle, data);
			}
		}
		Check();
	}

	/// <summary>Fills an array of bytes with a cryptographically strong sequence of random nonzero values.</summary>
	/// <param name="data">The array to fill with a cryptographically strong sequence of random nonzero values. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> is null.</exception>
	public override void GetNonZeroBytes(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		byte[] array = new byte[data.Length * 2];
		int num = 0;
		while (num < data.Length)
		{
			_handle = RngGetBytes(_handle, array);
			Check();
			for (int i = 0; i < array.Length; i++)
			{
				if (num == data.Length)
				{
					break;
				}
				if (array[i] != 0)
				{
					data[num++] = array[i];
				}
			}
		}
	}

	~RNGCryptoServiceProvider()
	{
		if (_handle != IntPtr.Zero)
		{
			RngClose(_handle);
			_handle = IntPtr.Zero;
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}
}
