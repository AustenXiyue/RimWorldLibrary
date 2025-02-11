using System.Text;

namespace System.Net.NetworkInformation;

/// <summary>Provides the Media Access Control (MAC) address for a network interface (adapter).</summary>
public class PhysicalAddress
{
	private byte[] address;

	private bool changed = true;

	private int hash;

	/// <summary>Returns a new <see cref="T:System.Net.NetworkInformation.PhysicalAddress" /> instance with a zero length address. This field is read-only.</summary>
	public static readonly PhysicalAddress None = new PhysicalAddress(new byte[0]);

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.NetworkInformation.PhysicalAddress" /> class. </summary>
	/// <param name="address">A <see cref="T:System.Byte" /> array containing the address.</param>
	public PhysicalAddress(byte[] address)
	{
		this.address = address;
	}

	/// <summary>Returns the hash value of a physical address.</summary>
	/// <returns>An integer hash value.</returns>
	public override int GetHashCode()
	{
		if (changed)
		{
			changed = false;
			hash = 0;
			int num = address.Length & -4;
			int i;
			for (i = 0; i < num; i += 4)
			{
				hash ^= address[i] | (address[i + 1] << 8) | (address[i + 2] << 16) | (address[i + 3] << 24);
			}
			if ((address.Length & 3) != 0)
			{
				int num2 = 0;
				int num3 = 0;
				for (; i < address.Length; i++)
				{
					num2 |= address[i] << num3;
					num3 += 8;
				}
				hash ^= num2;
			}
		}
		return hash;
	}

	/// <summary>Compares two <see cref="T:System.Net.NetworkInformation.PhysicalAddress" /> instances.</summary>
	/// <returns>true if this instance and the specified instance contain the same address; otherwise false.</returns>
	/// <param name="comparand">The <see cref="T:System.Net.NetworkInformation.PhysicalAddress" />  to compare to the current instance.</param>
	public override bool Equals(object comparand)
	{
		if (!(comparand is PhysicalAddress physicalAddress))
		{
			return false;
		}
		if (address.Length != physicalAddress.address.Length)
		{
			return false;
		}
		for (int i = 0; i < physicalAddress.address.Length; i++)
		{
			if (address[i] != physicalAddress.address[i])
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Returns the <see cref="T:System.String" /> representation of the address of this instance.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the address contained in this instance.</returns>
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		byte[] array = address;
		foreach (byte b in array)
		{
			int num = (b >> 4) & 0xF;
			for (int j = 0; j < 2; j++)
			{
				if (num < 10)
				{
					stringBuilder.Append((char)(num + 48));
				}
				else
				{
					stringBuilder.Append((char)(num + 55));
				}
				num = b & 0xF;
			}
		}
		return stringBuilder.ToString();
	}

	/// <summary>Returns the address of the current instance.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the address.</returns>
	public byte[] GetAddressBytes()
	{
		byte[] array = new byte[address.Length];
		Buffer.BlockCopy(address, 0, array, 0, address.Length);
		return array;
	}

	/// <summary>Parses the specified <see cref="T:System.String" /> and stores its contents as the address bytes of the <see cref="T:System.Net.NetworkInformation.PhysicalAddress" /> returned by this method.</summary>
	/// <returns>A <see cref="T:System.Net.NetworkInformation.PhysicalAddress" /> instance with the specified address.</returns>
	/// <param name="address">A <see cref="T:System.String" /> containing the address that will be used to initialize the <see cref="T:System.Net.NetworkInformation.PhysicalAddress" /> instance returned by this method.</param>
	/// <exception cref="T:System.FormatException">The <paramref name="address" /> parameter contains an illegal hardware address. This exception also occurs if the <paramref name="address" /> parameter contains a string in the incorrect format.</exception>
	public static PhysicalAddress Parse(string address)
	{
		int num = 0;
		bool flag = false;
		byte[] array = null;
		if (address == null)
		{
			return None;
		}
		if (address.IndexOf('-') >= 0)
		{
			flag = true;
			array = new byte[(address.Length + 1) / 3];
		}
		else
		{
			if (address.Length % 2 > 0)
			{
				throw new FormatException(global::SR.GetString("An invalid physical address was specified."));
			}
			array = new byte[address.Length / 2];
		}
		int num2 = 0;
		for (int i = 0; i < address.Length; i++)
		{
			int num3 = address[i];
			if (num3 >= 48 && num3 <= 57)
			{
				num3 -= 48;
			}
			else
			{
				if (num3 < 65 || num3 > 70)
				{
					if (num3 == 45)
					{
						if (num == 2)
						{
							num = 0;
							continue;
						}
						throw new FormatException(global::SR.GetString("An invalid physical address was specified."));
					}
					throw new FormatException(global::SR.GetString("An invalid physical address was specified."));
				}
				num3 -= 55;
			}
			if (flag && num >= 2)
			{
				throw new FormatException(global::SR.GetString("An invalid physical address was specified."));
			}
			if (num % 2 == 0)
			{
				array[num2] = (byte)(num3 << 4);
			}
			else
			{
				array[num2++] |= (byte)num3;
			}
			num++;
		}
		if (num < 2)
		{
			throw new FormatException(global::SR.GetString("An invalid physical address was specified."));
		}
		return new PhysicalAddress(array);
	}
}
