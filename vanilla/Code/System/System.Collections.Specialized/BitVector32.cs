using System.Text;

namespace System.Collections.Specialized;

/// <summary>Provides a simple structure that stores Boolean values and small integers in 32 bits of memory.</summary>
public struct BitVector32
{
	/// <summary>Represents a section of the vector that can contain an integer number.</summary>
	public struct Section
	{
		private readonly short mask;

		private readonly short offset;

		/// <summary>Gets a mask that isolates this section within the <see cref="T:System.Collections.Specialized.BitVector32" />.</summary>
		/// <returns>A mask that isolates this section within the <see cref="T:System.Collections.Specialized.BitVector32" />.</returns>
		public short Mask => mask;

		/// <summary>Gets the offset of this section from the start of the <see cref="T:System.Collections.Specialized.BitVector32" />.</summary>
		/// <returns>The offset of this section from the start of the <see cref="T:System.Collections.Specialized.BitVector32" />.</returns>
		public short Offset => offset;

		internal Section(short mask, short offset)
		{
			this.mask = mask;
			this.offset = offset;
		}

		/// <summary>Determines whether the specified object is the same as the current <see cref="T:System.Collections.Specialized.BitVector32.Section" /> object.</summary>
		/// <returns>true if the specified object is the same as the current <see cref="T:System.Collections.Specialized.BitVector32.Section" /> object; otherwise, false.</returns>
		/// <param name="o">The object to compare with the current <see cref="T:System.Collections.Specialized.BitVector32.Section" />.</param>
		public override bool Equals(object o)
		{
			if (o is Section)
			{
				return Equals((Section)o);
			}
			return false;
		}

		/// <summary>Determines whether the specified <see cref="T:System.Collections.Specialized.BitVector32.Section" /> object is the same as the current <see cref="T:System.Collections.Specialized.BitVector32.Section" /> object.</summary>
		/// <returns>true if the <paramref name="obj" /> parameter is the same as the current <see cref="T:System.Collections.Specialized.BitVector32.Section" /> object; otherwise false.</returns>
		/// <param name="obj">The <see cref="T:System.Collections.Specialized.BitVector32.Section" /> object to compare with the current <see cref="T:System.Collections.Specialized.BitVector32.Section" /> object.</param>
		public bool Equals(Section obj)
		{
			if (obj.mask == mask)
			{
				return obj.offset == offset;
			}
			return false;
		}

		/// <summary>Determines whether two specified <see cref="T:System.Collections.Specialized.BitVector32.Section" /> objects are equal.</summary>
		/// <returns>true if the <paramref name="a" /> and <paramref name="b" /> parameters represent the same <see cref="T:System.Collections.Specialized.BitVector32.Section" /> object, otherwise, false.</returns>
		/// <param name="a">A <see cref="T:System.Collections.Specialized.BitVector32.Section" /> object.</param>
		/// <param name="b">A <see cref="T:System.Collections.Specialized.BitVector32.Section" /> object.</param>
		public static bool operator ==(Section a, Section b)
		{
			return a.Equals(b);
		}

		/// <summary>Determines whether two <see cref="T:System.Collections.Specialized.BitVector32.Section" /> objects have different values.</summary>
		/// <returns>true if the <paramref name="a" /> and <paramref name="b" /> parameters represent different <see cref="T:System.Collections.Specialized.BitVector32.Section" /> objects; otherwise, false.</returns>
		/// <param name="a">A <see cref="T:System.Collections.Specialized.BitVector32.Section" /> object.</param>
		/// <param name="b">A <see cref="T:System.Collections.Specialized.BitVector32.Section" /> object.</param>
		public static bool operator !=(Section a, Section b)
		{
			return !(a == b);
		}

		/// <summary>Serves as a hash function for the current <see cref="T:System.Collections.Specialized.BitVector32.Section" />, suitable for hashing algorithms and data structures, such as a hash table.</summary>
		/// <returns>A hash code for the current <see cref="T:System.Collections.Specialized.BitVector32.Section" />.</returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>Returns a string that represents the specified <see cref="T:System.Collections.Specialized.BitVector32.Section" />.</summary>
		/// <returns>A string that represents the specified <see cref="T:System.Collections.Specialized.BitVector32.Section" />.</returns>
		/// <param name="value">The <see cref="T:System.Collections.Specialized.BitVector32.Section" /> to represent.</param>
		public static string ToString(Section value)
		{
			return "Section{0x" + Convert.ToString(value.Mask, 16) + ", 0x" + Convert.ToString(value.Offset, 16) + "}";
		}

		/// <summary>Returns a string that represents the current <see cref="T:System.Collections.Specialized.BitVector32.Section" />.</summary>
		/// <returns>A string that represents the current <see cref="T:System.Collections.Specialized.BitVector32.Section" />.</returns>
		public override string ToString()
		{
			return ToString(this);
		}
	}

	private uint data;

	/// <summary>Gets or sets the state of the bit flag indicated by the specified mask.</summary>
	/// <returns>true if the specified bit flag is on (1); otherwise, false.</returns>
	/// <param name="bit">A mask that indicates the bit to get or set. </param>
	public bool this[int bit]
	{
		get
		{
			return (data & bit) == (uint)bit;
		}
		set
		{
			if (value)
			{
				data |= (uint)bit;
			}
			else
			{
				data &= (uint)(~bit);
			}
		}
	}

	/// <summary>Gets or sets the value stored in the specified <see cref="T:System.Collections.Specialized.BitVector32.Section" />.</summary>
	/// <returns>The value stored in the specified <see cref="T:System.Collections.Specialized.BitVector32.Section" />.</returns>
	/// <param name="section">A <see cref="T:System.Collections.Specialized.BitVector32.Section" /> that contains the value to get or set. </param>
	public int this[Section section]
	{
		get
		{
			return (int)((data & (uint)(section.Mask << (int)section.Offset)) >> (int)section.Offset);
		}
		set
		{
			value <<= (int)section.Offset;
			int num = (0xFFFF & section.Mask) << (int)section.Offset;
			data = (data & (uint)(~num)) | (uint)(value & num);
		}
	}

	/// <summary>Gets the value of the <see cref="T:System.Collections.Specialized.BitVector32" /> as an integer.</summary>
	/// <returns>The value of the <see cref="T:System.Collections.Specialized.BitVector32" /> as an integer.</returns>
	public int Data => (int)data;

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.BitVector32" /> structure containing the data represented in an integer.</summary>
	/// <param name="data">An integer representing the data of the new <see cref="T:System.Collections.Specialized.BitVector32" />. </param>
	public BitVector32(int data)
	{
		this.data = (uint)data;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.BitVector32" /> structure containing the data represented in an existing <see cref="T:System.Collections.Specialized.BitVector32" /> structure.</summary>
	/// <param name="value">A <see cref="T:System.Collections.Specialized.BitVector32" /> structure that contains the data to copy. </param>
	public BitVector32(BitVector32 value)
	{
		data = value.data;
	}

	private static short CountBitsSet(short mask)
	{
		short num = 0;
		while ((mask & 1) != 0)
		{
			num++;
			mask >>= 1;
		}
		return num;
	}

	/// <summary>Creates the first mask in a series of masks that can be used to retrieve individual bits in a <see cref="T:System.Collections.Specialized.BitVector32" /> that is set up as bit flags.</summary>
	/// <returns>A mask that isolates the first bit flag in the <see cref="T:System.Collections.Specialized.BitVector32" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static int CreateMask()
	{
		return CreateMask(0);
	}

	/// <summary>Creates an additional mask following the specified mask in a series of masks that can be used to retrieve individual bits in a <see cref="T:System.Collections.Specialized.BitVector32" /> that is set up as bit flags.</summary>
	/// <returns>A mask that isolates the bit flag following the one that <paramref name="previous" /> points to in <see cref="T:System.Collections.Specialized.BitVector32" />.</returns>
	/// <param name="previous">The mask that indicates the previous bit flag. </param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="previous" /> indicates the last bit flag in the <see cref="T:System.Collections.Specialized.BitVector32" />. </exception>
	public static int CreateMask(int previous)
	{
		return previous switch
		{
			0 => 1, 
			int.MinValue => throw new InvalidOperationException(global::SR.GetString("Bit vector is full.")), 
			_ => previous << 1, 
		};
	}

	private static short CreateMaskFromHighValue(short highValue)
	{
		short num = 16;
		while ((highValue & 0x8000) == 0)
		{
			num--;
			highValue <<= 1;
		}
		ushort num2 = 0;
		while (num > 0)
		{
			num--;
			num2 <<= 1;
			num2 |= 1;
		}
		return (short)num2;
	}

	/// <summary>Creates the first <see cref="T:System.Collections.Specialized.BitVector32.Section" /> in a series of sections that contain small integers.</summary>
	/// <returns>A <see cref="T:System.Collections.Specialized.BitVector32.Section" /> that can hold a number from zero to <paramref name="maxValue" />.</returns>
	/// <param name="maxValue">A 16-bit signed integer that specifies the maximum value for the new <see cref="T:System.Collections.Specialized.BitVector32.Section" />. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="maxValue" /> is less than 1. </exception>
	public static Section CreateSection(short maxValue)
	{
		return CreateSectionHelper(maxValue, 0, 0);
	}

	/// <summary>Creates a new <see cref="T:System.Collections.Specialized.BitVector32.Section" /> following the specified <see cref="T:System.Collections.Specialized.BitVector32.Section" /> in a series of sections that contain small integers.</summary>
	/// <returns>A <see cref="T:System.Collections.Specialized.BitVector32.Section" /> that can hold a number from zero to <paramref name="maxValue" />.</returns>
	/// <param name="maxValue">A 16-bit signed integer that specifies the maximum value for the new <see cref="T:System.Collections.Specialized.BitVector32.Section" />. </param>
	/// <param name="previous">The previous <see cref="T:System.Collections.Specialized.BitVector32.Section" /> in the <see cref="T:System.Collections.Specialized.BitVector32" />. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="maxValue" /> is less than 1. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="previous" /> includes the final bit in the <see cref="T:System.Collections.Specialized.BitVector32" />.-or- <paramref name="maxValue" /> is greater than the highest value that can be represented by the number of bits after <paramref name="previous" />. </exception>
	public static Section CreateSection(short maxValue, Section previous)
	{
		return CreateSectionHelper(maxValue, previous.Mask, previous.Offset);
	}

	private static Section CreateSectionHelper(short maxValue, short priorMask, short priorOffset)
	{
		if (maxValue < 1)
		{
			throw new ArgumentException(global::SR.GetString("Argument {0} should be larger than {1}.", "maxValue", 0), "maxValue");
		}
		short num = (short)(priorOffset + CountBitsSet(priorMask));
		if (num >= 32)
		{
			throw new InvalidOperationException(global::SR.GetString("Bit vector is full."));
		}
		return new Section(CreateMaskFromHighValue(maxValue), num);
	}

	/// <summary>Determines whether the specified object is equal to the <see cref="T:System.Collections.Specialized.BitVector32" />.</summary>
	/// <returns>true if the specified object is equal to the <see cref="T:System.Collections.Specialized.BitVector32" />; otherwise, false.</returns>
	/// <param name="o">The object to compare with the current <see cref="T:System.Collections.Specialized.BitVector32" />. </param>
	public override bool Equals(object o)
	{
		if (!(o is BitVector32))
		{
			return false;
		}
		return data == ((BitVector32)o).data;
	}

	/// <summary>Serves as a hash function for the <see cref="T:System.Collections.Specialized.BitVector32" />.</summary>
	/// <returns>A hash code for the <see cref="T:System.Collections.Specialized.BitVector32" />.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Returns a string that represents the specified <see cref="T:System.Collections.Specialized.BitVector32" />.</summary>
	/// <returns>A string that represents the specified <see cref="T:System.Collections.Specialized.BitVector32" />.</returns>
	/// <param name="value">The <see cref="T:System.Collections.Specialized.BitVector32" /> to represent. </param>
	public static string ToString(BitVector32 value)
	{
		StringBuilder stringBuilder = new StringBuilder(45);
		stringBuilder.Append("BitVector32{");
		int num = (int)value.data;
		for (int i = 0; i < 32; i++)
		{
			if ((num & 0x80000000u) != 0L)
			{
				stringBuilder.Append("1");
			}
			else
			{
				stringBuilder.Append("0");
			}
			num <<= 1;
		}
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}

	/// <summary>Returns a string that represents the current <see cref="T:System.Collections.Specialized.BitVector32" />.</summary>
	/// <returns>A string that represents the current <see cref="T:System.Collections.Specialized.BitVector32" />.</returns>
	public override string ToString()
	{
		return ToString(this);
	}
}
