using System.Runtime.InteropServices;

namespace System.Security.Cryptography;

/// <summary>Computes a Message Authentication Code (MAC) using <see cref="T:System.Security.Cryptography.TripleDES" /> for the input data <see cref="T:System.Security.Cryptography.CryptoStream" />.</summary>
[ComVisible(true)]
public class MACTripleDES : KeyedHashAlgorithm
{
	private ICryptoTransform m_encryptor;

	private CryptoStream _cs;

	private TailStream _ts;

	private const int m_bitsPerByte = 8;

	private int m_bytesPerBlock;

	private TripleDES des;

	/// <summary>Gets or sets the padding mode used in the hashing algorithm.</summary>
	/// <returns>The padding mode used in the hashing algorithm.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The property cannot be set because the padding mode is invalid.</exception>
	[ComVisible(false)]
	public PaddingMode Padding
	{
		get
		{
			return des.Padding;
		}
		set
		{
			if (value < PaddingMode.None || PaddingMode.ISO10126 < value)
			{
				throw new CryptographicException(Environment.GetResourceString("Specified padding mode is not valid for this algorithm."));
			}
			des.Padding = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.MACTripleDES" /> class.</summary>
	public MACTripleDES()
	{
		KeyValue = new byte[24];
		Utils.StaticRandomNumberGenerator.GetBytes(KeyValue);
		des = TripleDES.Create();
		HashSizeValue = des.BlockSize;
		m_bytesPerBlock = des.BlockSize / 8;
		des.IV = new byte[m_bytesPerBlock];
		des.Padding = PaddingMode.Zeros;
		m_encryptor = null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.MACTripleDES" /> class with the specified key data.</summary>
	/// <param name="rgbKey">The secret key for <see cref="T:System.Security.Cryptography.MACTripleDES" /> encryption. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rgbKey" /> parameter is null. </exception>
	public MACTripleDES(byte[] rgbKey)
		: this("System.Security.Cryptography.TripleDES", rgbKey)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.MACTripleDES" /> class with the specified key data and using the specified implementation of <see cref="T:System.Security.Cryptography.TripleDES" />.</summary>
	/// <param name="strTripleDES">The name of the <see cref="T:System.Security.Cryptography.TripleDES" /> implementation to use. </param>
	/// <param name="rgbKey">The secret key for <see cref="T:System.Security.Cryptography.MACTripleDES" /> encryption. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rgbKey" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicUnexpectedOperationException">The <paramref name="strTripleDES" /> parameter is not a valid name of a <see cref="T:System.Security.Cryptography.TripleDES" /> implementation. </exception>
	public MACTripleDES(string strTripleDES, byte[] rgbKey)
	{
		if (rgbKey == null)
		{
			throw new ArgumentNullException("rgbKey");
		}
		if (strTripleDES == null)
		{
			des = TripleDES.Create();
		}
		else
		{
			des = TripleDES.Create(strTripleDES);
		}
		HashSizeValue = des.BlockSize;
		KeyValue = (byte[])rgbKey.Clone();
		m_bytesPerBlock = des.BlockSize / 8;
		des.IV = new byte[m_bytesPerBlock];
		des.Padding = PaddingMode.Zeros;
		m_encryptor = null;
	}

	/// <summary>Initializes an instance of <see cref="T:System.Security.Cryptography.MACTripleDES" />.</summary>
	public override void Initialize()
	{
		m_encryptor = null;
	}

	/// <summary>Routes data written to the object into the <see cref="T:System.Security.Cryptography.TripleDES" /> encryptor for computing the Message Authentication Code (MAC).</summary>
	/// <param name="rgbData">The input data. </param>
	/// <param name="ibStart">The offset into the byte array from which to begin using data. </param>
	/// <param name="cbSize">The number of bytes in the array to use as data. </param>
	protected override void HashCore(byte[] rgbData, int ibStart, int cbSize)
	{
		if (m_encryptor == null)
		{
			des.Key = Key;
			m_encryptor = des.CreateEncryptor();
			_ts = new TailStream(des.BlockSize / 8);
			_cs = new CryptoStream(_ts, m_encryptor, CryptoStreamMode.Write);
		}
		_cs.Write(rgbData, ibStart, cbSize);
	}

	/// <summary>Returns the computed Message Authentication Code (MAC) after all data is written to the object.</summary>
	/// <returns>The computed MAC.</returns>
	protected override byte[] HashFinal()
	{
		if (m_encryptor == null)
		{
			des.Key = Key;
			m_encryptor = des.CreateEncryptor();
			_ts = new TailStream(des.BlockSize / 8);
			_cs = new CryptoStream(_ts, m_encryptor, CryptoStreamMode.Write);
		}
		_cs.FlushFinalBlock();
		return _ts.Buffer;
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Security.Cryptography.MACTripleDES" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (des != null)
			{
				des.Clear();
			}
			if (m_encryptor != null)
			{
				m_encryptor.Dispose();
			}
			if (_cs != null)
			{
				_cs.Clear();
			}
			if (_ts != null)
			{
				_ts.Clear();
			}
		}
		base.Dispose(disposing);
	}
}
