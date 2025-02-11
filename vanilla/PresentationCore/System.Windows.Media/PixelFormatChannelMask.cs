using System.Collections.Generic;
using MS.Internal;

namespace System.Windows.Media;

/// <summary>Defines the bit mask and shift for a specific pixel formats </summary>
public struct PixelFormatChannelMask
{
	private byte[] _mask;

	/// <summary>Gets the bitmask for a color channel. The value will never be greater then 0xffffffff </summary>
	/// <returns>The bitmask for a color channel.</returns>
	public IList<byte> Mask
	{
		get
		{
			if (_mask == null)
			{
				return null;
			}
			return new PartialList<byte>((byte[])_mask.Clone());
		}
	}

	internal PixelFormatChannelMask(byte[] mask)
	{
		_mask = mask;
	}

	/// <summary> Compares two <see cref="T:System.Windows.Media.PixelFormatChannelMask" /> instances for equality.</summary>
	/// <returns>true if the two masks are equal; otherwise, false.</returns>
	/// <param name="left">The first mask to compare.</param>
	/// <param name="right">The second mask to compare.</param>
	public static bool operator ==(PixelFormatChannelMask left, PixelFormatChannelMask right)
	{
		return Equals(left, right);
	}

	/// <summary>Determines if two pixel format channel masks are equal.</summary>
	/// <returns>true if the masks are equal; otherwise, false.</returns>
	/// <param name="left">The first mask to compare.</param>
	/// <param name="right">The second mask to compare.</param>
	public static bool Equals(PixelFormatChannelMask left, PixelFormatChannelMask right)
	{
		int num = ((left._mask != null) ? left._mask.Length : 0);
		int num2 = ((right._mask != null) ? right._mask.Length : 0);
		if (num != num2)
		{
			return false;
		}
		for (int i = 0; i < num; i++)
		{
			if (left._mask[i] != right._mask[i])
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.PixelFormatChannelMask" /> instances for inequality.</summary>
	/// <returns>true if the two <see cref="T:System.Windows.Media.PixelFormatChannelMask" /> objects are not equal; otherwise, false.</returns>
	/// <param name="left">The first mask to compare.</param>
	/// <param name="right">The second mask to compare.</param>
	public static bool operator !=(PixelFormatChannelMask left, PixelFormatChannelMask right)
	{
		return !(left == right);
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.PixelFormatChannelMask" /> is equal to <paramref name="obj" />; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare with the current mask.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is PixelFormatChannelMask))
		{
			return false;
		}
		return this == (PixelFormatChannelMask)obj;
	}

	/// <summary>Retrieves a hash code for the mask.</summary>
	/// <returns>A mask's hash code.</returns>
	public override int GetHashCode()
	{
		int num = 0;
		if (_mask != null)
		{
			int i = 0;
			for (int num2 = _mask.Length; i < num2; i++)
			{
				num += _mask[i] * 256 * i;
			}
		}
		return num;
	}
}
