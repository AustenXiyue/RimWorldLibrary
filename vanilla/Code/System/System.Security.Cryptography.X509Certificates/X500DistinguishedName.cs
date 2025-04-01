using System.Text;
using Mono.Security;
using Mono.Security.X509;

namespace System.Security.Cryptography.X509Certificates;

/// <summary>Represents the distinguished name of an X509 certificate. This class cannot be inherited.</summary>
[System.MonoTODO("Some X500DistinguishedNameFlags options aren't supported, like DoNotUsePlusSign, DoNotUseQuotes and ForceUTF8Encoding")]
public sealed class X500DistinguishedName : AsnEncodedData
{
	private const X500DistinguishedNameFlags AllFlags = X500DistinguishedNameFlags.Reversed | X500DistinguishedNameFlags.UseSemicolons | X500DistinguishedNameFlags.DoNotUsePlusSign | X500DistinguishedNameFlags.DoNotUseQuotes | X500DistinguishedNameFlags.UseCommas | X500DistinguishedNameFlags.UseNewLines | X500DistinguishedNameFlags.UseUTF8Encoding | X500DistinguishedNameFlags.UseT61Encoding | X500DistinguishedNameFlags.ForceUTF8Encoding;

	private string name;

	private byte[] canonEncoding;

	internal byte[] CanonicalEncoding => canonEncoding;

	/// <summary>Gets the comma-delimited distinguished name from an X500 certificate.</summary>
	/// <returns>The comma-delimited distinguished name of the X509 certificate.</returns>
	public string Name => name;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X500DistinguishedName" /> class using the specified <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object.</summary>
	/// <param name="encodedDistinguishedName">An <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object that represents the distinguished name.</param>
	public X500DistinguishedName(AsnEncodedData encodedDistinguishedName)
	{
		if (encodedDistinguishedName == null)
		{
			throw new ArgumentNullException("encodedDistinguishedName");
		}
		base.RawData = encodedDistinguishedName.RawData;
		if (base.RawData.Length != 0)
		{
			DecodeRawData();
		}
		else
		{
			name = string.Empty;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X500DistinguishedName" /> class using information from the specified byte array.</summary>
	/// <param name="encodedDistinguishedName">A byte array that contains distinguished name information.</param>
	public X500DistinguishedName(byte[] encodedDistinguishedName)
	{
		if (encodedDistinguishedName == null)
		{
			throw new ArgumentNullException("encodedDistinguishedName");
		}
		base.Oid = new Oid();
		base.RawData = encodedDistinguishedName;
		if (encodedDistinguishedName.Length != 0)
		{
			DecodeRawData();
		}
		else
		{
			name = string.Empty;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X500DistinguishedName" /> class using information from the specified string.</summary>
	/// <param name="distinguishedName">A string that represents the distinguished name.</param>
	public X500DistinguishedName(string distinguishedName)
		: this(distinguishedName, X500DistinguishedNameFlags.Reversed)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X500DistinguishedName" /> class using the specified string and <see cref="T:System.Security.Cryptography.X509Certificates.X500DistinguishedNameFlags" /> flag.</summary>
	/// <param name="distinguishedName">A string that represents the distinguished name.</param>
	/// <param name="flag">A bitwise combination of the enumeration values that specify the characteristics of the distinguished name.</param>
	public X500DistinguishedName(string distinguishedName, X500DistinguishedNameFlags flag)
	{
		if (distinguishedName == null)
		{
			throw new ArgumentNullException("distinguishedName");
		}
		if (flag != 0 && (flag & (X500DistinguishedNameFlags.Reversed | X500DistinguishedNameFlags.UseSemicolons | X500DistinguishedNameFlags.DoNotUsePlusSign | X500DistinguishedNameFlags.DoNotUseQuotes | X500DistinguishedNameFlags.UseCommas | X500DistinguishedNameFlags.UseNewLines | X500DistinguishedNameFlags.UseUTF8Encoding | X500DistinguishedNameFlags.UseT61Encoding | X500DistinguishedNameFlags.ForceUTF8Encoding)) == 0)
		{
			throw new ArgumentException("flag");
		}
		base.Oid = new Oid();
		if (distinguishedName.Length == 0)
		{
			base.RawData = new byte[2] { 48, 0 };
			DecodeRawData();
			return;
		}
		ASN1 aSN = X501.FromString(distinguishedName);
		if ((flag & X500DistinguishedNameFlags.Reversed) != 0)
		{
			ASN1 aSN2 = new ASN1(48);
			for (int num = aSN.Count - 1; num >= 0; num--)
			{
				aSN2.Add(aSN[num]);
			}
			aSN = aSN2;
		}
		base.RawData = aSN.GetBytes();
		if (flag == X500DistinguishedNameFlags.None)
		{
			name = distinguishedName;
		}
		else
		{
			name = Decode(flag);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X500DistinguishedName" /> class using the specified <see cref="T:System.Security.Cryptography.X509Certificates.X500DistinguishedName" /> object.</summary>
	/// <param name="distinguishedName">An <see cref="T:System.Security.Cryptography.X509Certificates.X500DistinguishedName" /> object.</param>
	public X500DistinguishedName(X500DistinguishedName distinguishedName)
	{
		if (distinguishedName == null)
		{
			throw new ArgumentNullException("distinguishedName");
		}
		base.Oid = new Oid();
		base.RawData = distinguishedName.RawData;
		name = distinguishedName.name;
	}

	internal X500DistinguishedName(byte[] encoded, byte[] canonEncoding, string name)
	{
		this.canonEncoding = canonEncoding;
		this.name = name;
		base.Oid = new Oid();
		base.RawData = encoded;
	}

	/// <summary>Decodes a distinguished name using the characteristics specified by the <paramref name="flag" /> parameter.</summary>
	/// <returns>The decoded distinguished name.</returns>
	/// <param name="flag">A bitwise combination of the enumeration values that specify the characteristics of the distinguished name.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate has an invalid name.</exception>
	public string Decode(X500DistinguishedNameFlags flag)
	{
		if (flag != 0 && (flag & (X500DistinguishedNameFlags.Reversed | X500DistinguishedNameFlags.UseSemicolons | X500DistinguishedNameFlags.DoNotUsePlusSign | X500DistinguishedNameFlags.DoNotUseQuotes | X500DistinguishedNameFlags.UseCommas | X500DistinguishedNameFlags.UseNewLines | X500DistinguishedNameFlags.UseUTF8Encoding | X500DistinguishedNameFlags.UseT61Encoding | X500DistinguishedNameFlags.ForceUTF8Encoding)) == 0)
		{
			throw new ArgumentException("flag");
		}
		if (base.RawData.Length == 0)
		{
			return string.Empty;
		}
		bool flag2 = (flag & X500DistinguishedNameFlags.Reversed) != 0;
		bool flag3 = (flag & X500DistinguishedNameFlags.DoNotUseQuotes) == 0;
		string separator = GetSeparator(flag);
		return X501.ToString(new ASN1(base.RawData), flag2, separator, flag3);
	}

	/// <summary>Returns a formatted version of an X500 distinguished name for printing or for output to a text window or to a console.</summary>
	/// <returns>A formatted string that represents the X500 distinguished name.</returns>
	/// <param name="multiLine">true if the return string should contain carriage returns; otherwise, false.</param>
	public override string Format(bool multiLine)
	{
		if (multiLine)
		{
			string text = Decode(X500DistinguishedNameFlags.UseNewLines);
			if (text.Length > 0)
			{
				return text + Environment.NewLine;
			}
			return text;
		}
		return Decode(X500DistinguishedNameFlags.UseCommas);
	}

	private static string GetSeparator(X500DistinguishedNameFlags flag)
	{
		if ((flag & X500DistinguishedNameFlags.UseSemicolons) != 0)
		{
			return "; ";
		}
		if ((flag & X500DistinguishedNameFlags.UseCommas) != 0)
		{
			return ", ";
		}
		if ((flag & X500DistinguishedNameFlags.UseNewLines) != 0)
		{
			return Environment.NewLine;
		}
		return ", ";
	}

	private void DecodeRawData()
	{
		if (base.RawData == null || base.RawData.Length < 3)
		{
			name = string.Empty;
			return;
		}
		ASN1 aSN = new ASN1(base.RawData);
		name = X501.ToString(aSN, reversed: true, ", ", quotes: true);
	}

	private static string Canonize(string s)
	{
		int i = s.IndexOf('=') + 1;
		StringBuilder stringBuilder = new StringBuilder(s.Substring(0, i));
		for (; i < s.Length && char.IsWhiteSpace(s, i); i++)
		{
		}
		s = s.TrimEnd();
		bool flag = false;
		for (; i < s.Length; i++)
		{
			if (flag)
			{
				flag = char.IsWhiteSpace(s, i);
				if (flag)
				{
					continue;
				}
			}
			if (char.IsWhiteSpace(s, i))
			{
				flag = true;
			}
			stringBuilder.Append(char.ToUpperInvariant(s[i]));
		}
		return stringBuilder.ToString();
	}

	internal static bool AreEqual(X500DistinguishedName name1, X500DistinguishedName name2)
	{
		if (name1 == null)
		{
			return name2 == null;
		}
		if (name2 == null)
		{
			return false;
		}
		if (name1.canonEncoding != null && name2.canonEncoding != null)
		{
			if (name1.canonEncoding.Length != name2.canonEncoding.Length)
			{
				return false;
			}
			for (int i = 0; i < name1.canonEncoding.Length; i++)
			{
				if (name1.canonEncoding[i] != name2.canonEncoding[i])
				{
					return false;
				}
			}
			return true;
		}
		X500DistinguishedNameFlags flag = X500DistinguishedNameFlags.DoNotUseQuotes | X500DistinguishedNameFlags.UseNewLines;
		string[] separator = new string[1] { Environment.NewLine };
		string[] array = name1.Decode(flag).Split(separator, StringSplitOptions.RemoveEmptyEntries);
		string[] array2 = name2.Decode(flag).Split(separator, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length != array2.Length)
		{
			return false;
		}
		for (int j = 0; j < array.Length; j++)
		{
			if (Canonize(array[j]) != Canonize(array2[j]))
			{
				return false;
			}
		}
		return true;
	}
}
