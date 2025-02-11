using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32.PresentationCore;

namespace System.Windows.Media;

/// <summary>Defines a pixel format for images and pixel-based surfaces. </summary>
[Serializable]
[TypeConverter(typeof(PixelFormatConverter))]
public struct PixelFormat : IEquatable<PixelFormat>
{
	[NonSerialized]
	private PixelFormatFlags _flags;

	[NonSerialized]
	private PixelFormatEnum _format;

	[NonSerialized]
	private uint _bitsPerPixel;

	[NonSerialized]
	private MS.Internal.SecurityCriticalDataForSet<Guid> _guidFormat;

	[NonSerialized]
	private static readonly Guid WICPixelFormatPhotonFirst = new Guid(1876804388, 19971, 19454, 177, 133, 61, 119, 118, 141, 201, 29);

	[NonSerialized]
	private static readonly Guid WICPixelFormatPhotonLast = new Guid(1876804388, 19971, 19454, 177, 133, 61, 119, 118, 141, 201, 66);

	private PixelFormatFlags FormatFlags => _flags;

	/// <summary> Gets the number of bits-per-pixel (bpp) for this <see cref="T:System.Windows.Media.PixelFormat" />. </summary>
	/// <returns>The number of bits-per-pixel (bpp) for this <see cref="T:System.Windows.Media.PixelFormat" />.  </returns>
	public int BitsPerPixel => InternalBitsPerPixel;

	/// <summary>Gets a collection of bit masks associated with the <see cref="T:System.Windows.Media.PixelFormat" />.</summary>
	/// <returns>The collection of bit masks and shifts associated with the <see cref="T:System.Windows.Media.PixelFormat" />.</returns>
	public unsafe IList<PixelFormatChannelMask> Masks
	{
		get
		{
			nint ptr = CreatePixelFormatInfo();
			uint uiChannelCount = 0u;
			PixelFormatChannelMask[] array = null;
			uint cbActual = 0u;
			try
			{
				HRESULT.Check(UnsafeNativeMethods.WICPixelFormatInfo.GetChannelCount(ptr, out uiChannelCount));
				array = new PixelFormatChannelMask[uiChannelCount];
				for (uint num = 0u; num < uiChannelCount; num++)
				{
					HRESULT.Check(UnsafeNativeMethods.WICPixelFormatInfo.GetChannelMask(ptr, num, 0u, null, out cbActual));
					byte[] array2 = new byte[cbActual];
					fixed (byte* pbMaskBuffer = array2)
					{
						HRESULT.Check(UnsafeNativeMethods.WICPixelFormatInfo.GetChannelMask(ptr, num, cbActual, pbMaskBuffer, out cbActual));
					}
					array[num] = new PixelFormatChannelMask(array2);
				}
			}
			finally
			{
				if (ptr != IntPtr.Zero)
				{
					UnsafeNativeMethods.MILUnknown.ReleaseInterface(ref ptr);
				}
			}
			return new PartialList<PixelFormatChannelMask>(array);
		}
	}

	internal int InternalBitsPerPixel
	{
		get
		{
			if (_bitsPerPixel == 0)
			{
				uint uiBitsPerPixel = 0u;
				nint ptr = CreatePixelFormatInfo();
				try
				{
					HRESULT.Check(UnsafeNativeMethods.WICPixelFormatInfo.GetBitsPerPixel(ptr, out uiBitsPerPixel));
				}
				finally
				{
					if (ptr != IntPtr.Zero)
					{
						UnsafeNativeMethods.MILUnknown.ReleaseInterface(ref ptr);
					}
				}
				_bitsPerPixel = uiBitsPerPixel;
			}
			return (int)_bitsPerPixel;
		}
	}

	internal bool HasAlpha
	{
		get
		{
			if ((FormatFlags & PixelFormatFlags.ChannelOrderABGR) == 0 && (FormatFlags & PixelFormatFlags.ChannelOrderARGB) == 0)
			{
				return (FormatFlags & PixelFormatFlags.NChannelAlpha) != 0;
			}
			return true;
		}
	}

	internal bool Palettized => (FormatFlags & PixelFormatFlags.Palettized) != 0;

	internal PixelFormatEnum Format => _format;

	internal Guid Guid => _guidFormat.Value;

	internal unsafe PixelFormat(Guid guidPixelFormat)
	{
		Guid wICPixelFormatDontCare = WICPixelFormatGUIDs.WICPixelFormatDontCare;
		byte* ptr = (byte*)(&guidPixelFormat);
		byte* ptr2 = (byte*)(&wICPixelFormatDontCare);
		int num = 15;
		bool flag = true;
		for (int i = 0; i < num; i++)
		{
			if (ptr[i] != ptr2[i])
			{
				flag = false;
				break;
			}
		}
		if (flag && ptr[num] <= 28)
		{
			_format = (PixelFormatEnum)ptr[num];
		}
		else
		{
			_format = PixelFormatEnum.Default;
		}
		_flags = GetPixelFormatFlagsFromEnum(_format) | GetPixelFormatFlagsFromGuid(guidPixelFormat);
		_bitsPerPixel = GetBitsPerPixelFromEnum(_format);
		_guidFormat = new MS.Internal.SecurityCriticalDataForSet<Guid>(guidPixelFormat);
	}

	internal PixelFormat(PixelFormatEnum format)
	{
		_format = format;
		_flags = GetPixelFormatFlagsFromEnum(format);
		_bitsPerPixel = GetBitsPerPixelFromEnum(format);
		_guidFormat = new MS.Internal.SecurityCriticalDataForSet<Guid>(GetGuidFromFormat(format));
	}

	internal PixelFormat(string pixelFormatString)
	{
		PixelFormatEnum pixelFormatEnum = PixelFormatEnum.Default;
		if (pixelFormatString == null)
		{
			throw new ArgumentNullException("pixelFormatString");
		}
		pixelFormatEnum = (_format = pixelFormatString.ToUpper(CultureInfo.InvariantCulture) switch
		{
			"DEFAULT" => PixelFormatEnum.Default, 
			"EXTENDED" => PixelFormatEnum.Default, 
			"INDEXED1" => PixelFormatEnum.Indexed1, 
			"INDEXED2" => PixelFormatEnum.Indexed2, 
			"INDEXED4" => PixelFormatEnum.Indexed4, 
			"INDEXED8" => PixelFormatEnum.Indexed8, 
			"BLACKWHITE" => PixelFormatEnum.BlackWhite, 
			"GRAY2" => PixelFormatEnum.Gray2, 
			"GRAY4" => PixelFormatEnum.Gray4, 
			"GRAY8" => PixelFormatEnum.Gray8, 
			"BGR555" => PixelFormatEnum.Bgr555, 
			"BGR565" => PixelFormatEnum.Bgr565, 
			"BGR24" => PixelFormatEnum.Bgr24, 
			"RGB24" => PixelFormatEnum.Rgb24, 
			"BGR101010" => PixelFormatEnum.Bgr101010, 
			"BGR32" => PixelFormatEnum.Bgr32, 
			"BGRA32" => PixelFormatEnum.Bgra32, 
			"PBGRA32" => PixelFormatEnum.Pbgra32, 
			"RGB48" => PixelFormatEnum.Rgb48, 
			"RGBA64" => PixelFormatEnum.Rgba64, 
			"PRGBA64" => PixelFormatEnum.Prgba64, 
			"GRAY16" => PixelFormatEnum.Gray16, 
			"GRAY32FLOAT" => PixelFormatEnum.Gray32Float, 
			"RGB128FLOAT" => PixelFormatEnum.Rgb128Float, 
			"RGBA128FLOAT" => PixelFormatEnum.Rgba128Float, 
			"PRGBA128FLOAT" => PixelFormatEnum.Prgba128Float, 
			"CMYK32" => PixelFormatEnum.Cmyk32, 
			_ => throw new ArgumentException(SR.Format(SR.Image_BadPixelFormat, pixelFormatString), "pixelFormatString"), 
		});
		_flags = GetPixelFormatFlagsFromEnum(pixelFormatEnum);
		_bitsPerPixel = GetBitsPerPixelFromEnum(pixelFormatEnum);
		_guidFormat = new MS.Internal.SecurityCriticalDataForSet<Guid>(GetGuidFromFormat(pixelFormatEnum));
	}

	private static Guid GetGuidFromFormat(PixelFormatEnum format)
	{
		return format switch
		{
			PixelFormatEnum.Default => WICPixelFormatGUIDs.WICPixelFormatDontCare, 
			PixelFormatEnum.Indexed1 => WICPixelFormatGUIDs.WICPixelFormat1bppIndexed, 
			PixelFormatEnum.Indexed2 => WICPixelFormatGUIDs.WICPixelFormat2bppIndexed, 
			PixelFormatEnum.Indexed4 => WICPixelFormatGUIDs.WICPixelFormat4bppIndexed, 
			PixelFormatEnum.Indexed8 => WICPixelFormatGUIDs.WICPixelFormat8bppIndexed, 
			PixelFormatEnum.BlackWhite => WICPixelFormatGUIDs.WICPixelFormatBlackWhite, 
			PixelFormatEnum.Gray2 => WICPixelFormatGUIDs.WICPixelFormat2bppGray, 
			PixelFormatEnum.Gray4 => WICPixelFormatGUIDs.WICPixelFormat4bppGray, 
			PixelFormatEnum.Gray8 => WICPixelFormatGUIDs.WICPixelFormat8bppGray, 
			PixelFormatEnum.Bgr555 => WICPixelFormatGUIDs.WICPixelFormat16bppBGR555, 
			PixelFormatEnum.Bgr565 => WICPixelFormatGUIDs.WICPixelFormat16bppBGR565, 
			PixelFormatEnum.Bgr24 => WICPixelFormatGUIDs.WICPixelFormat24bppBGR, 
			PixelFormatEnum.Rgb24 => WICPixelFormatGUIDs.WICPixelFormat24bppRGB, 
			PixelFormatEnum.Bgr101010 => WICPixelFormatGUIDs.WICPixelFormat32bppBGR101010, 
			PixelFormatEnum.Bgr32 => WICPixelFormatGUIDs.WICPixelFormat32bppBGR, 
			PixelFormatEnum.Bgra32 => WICPixelFormatGUIDs.WICPixelFormat32bppBGRA, 
			PixelFormatEnum.Pbgra32 => WICPixelFormatGUIDs.WICPixelFormat32bppPBGRA, 
			PixelFormatEnum.Rgb48 => WICPixelFormatGUIDs.WICPixelFormat48bppRGB, 
			PixelFormatEnum.Rgba64 => WICPixelFormatGUIDs.WICPixelFormat64bppRGBA, 
			PixelFormatEnum.Prgba64 => WICPixelFormatGUIDs.WICPixelFormat64bppPRGBA, 
			PixelFormatEnum.Gray16 => WICPixelFormatGUIDs.WICPixelFormat16bppGray, 
			PixelFormatEnum.Gray32Float => WICPixelFormatGUIDs.WICPixelFormat32bppGrayFloat, 
			PixelFormatEnum.Rgb128Float => WICPixelFormatGUIDs.WICPixelFormat128bppRGBFloat, 
			PixelFormatEnum.Rgba128Float => WICPixelFormatGUIDs.WICPixelFormat128bppRGBAFloat, 
			PixelFormatEnum.Prgba128Float => WICPixelFormatGUIDs.WICPixelFormat128bppPRGBAFloat, 
			PixelFormatEnum.Cmyk32 => WICPixelFormatGUIDs.WICPixelFormat32bppCMYK, 
			_ => throw new ArgumentException(SR.Format(SR.Image_BadPixelFormat, format), "format"), 
		};
	}

	/// <summary> Compares two <see cref="T:System.Windows.Media.PixelFormat" /> instances for equality. </summary>
	/// <returns>true if the two <see cref="T:System.Windows.Media.PixelFormat" /> objects are equal; otherwise, false.</returns>
	/// <param name="left">The first <see cref="T:System.Windows.Media.PixelFormat" /> to compare.</param>
	/// <param name="right">The second <see cref="T:System.Windows.Media.PixelFormat" /> to compare.</param>
	public static bool operator ==(PixelFormat left, PixelFormat right)
	{
		return left.Guid == right.Guid;
	}

	/// <summary> Compares two <see cref="T:System.Windows.Media.PixelFormat" /> instances for inequality.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.PixelFormat" /> objects are not equal; otherwise, false.</returns>
	/// <param name="left">The first <see cref="T:System.Windows.Media.PixelFormat" /> to compare.</param>
	/// <param name="right">The second <see cref="T:System.Windows.Media.PixelFormat" /> to compare.</param>
	public static bool operator !=(PixelFormat left, PixelFormat right)
	{
		return left.Guid != right.Guid;
	}

	/// <summary> Determines whether the specified <see cref="T:System.Windows.Media.PixelFormat" /> instances are considered equal.</summary>
	/// <returns>true if the two parameters are equal; otherwise, false.</returns>
	/// <param name="left">The first <see cref="T:System.Windows.Media.PixelFormat" /> objects to compare for equality.</param>
	/// <param name="right">The second <see cref="T:System.Windows.Media.PixelFormat" /> object to compare for equality.</param>
	public static bool Equals(PixelFormat left, PixelFormat right)
	{
		return left.Guid == right.Guid;
	}

	/// <summary>Determines whether the pixel format equals the given <see cref="T:System.Windows.Media.PixelFormat" />.</summary>
	/// <returns>true if the pixel formats are equal; otherwise, false.</returns>
	/// <param name="pixelFormat">The pixel format to compare.</param>
	public bool Equals(PixelFormat pixelFormat)
	{
		return this == pixelFormat;
	}

	/// <summary> Determines whether the specified object is equal to the current object. </summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.PixelFormat" /> is equal to <paramref name="obj" />; otherwise, false.</returns>
	/// <param name="obj">The Object to compare with the current Object.</param>
	public override bool Equals(object obj)
	{
		if (obj == null || !(obj is PixelFormat))
		{
			return false;
		}
		return this == (PixelFormat)obj;
	}

	/// <summary>Creates a hash code from this pixel format's <see cref="P:System.Windows.Media.PixelFormat.Masks" /> value.</summary>
	/// <returns>The pixel format's hash code.</returns>
	public override int GetHashCode()
	{
		return Guid.GetHashCode();
	}

	internal nint CreatePixelFormatInfo()
	{
		nint ppIComponentInfo = IntPtr.Zero;
		nint ppvObject = IntPtr.Zero;
		using FactoryMaker factoryMaker = new FactoryMaker();
		try
		{
			Guid clsidComponent = Guid;
			int num = UnsafeNativeMethods.WICImagingFactory.CreateComponentInfo(factoryMaker.ImagingFactoryPtr, ref clsidComponent, out ppIComponentInfo);
			if (num == -2003292277 || num == -2003292336)
			{
				throw new NotSupportedException(SR.Image_NoPixelFormatFound);
			}
			HRESULT.Check(num);
			Guid guid = MILGuidData.IID_IWICPixelFormatInfo;
			HRESULT.Check(UnsafeNativeMethods.MILUnknown.QueryInterface(ppIComponentInfo, ref guid, out ppvObject));
			return ppvObject;
		}
		finally
		{
			if (ppIComponentInfo != IntPtr.Zero)
			{
				UnsafeNativeMethods.MILUnknown.ReleaseInterface(ref ppIComponentInfo);
			}
		}
	}

	/// <summary> Creates a string representation of this <see cref="T:System.Windows.Media.PixelFormat" />.</summary>
	/// <returns>A <see cref="T:System.String" /> containing a representation of the <see cref="T:System.Windows.Media.PixelFormat" />.</returns>
	public override string ToString()
	{
		return _format.ToString();
	}

	internal static PixelFormat GetPixelFormat(SafeMILHandle bitmapSource)
	{
		Guid pPixelFormatEnum = WICPixelFormatGUIDs.WICPixelFormatDontCare;
		HRESULT.Check(UnsafeNativeMethods.WICBitmapSource.GetPixelFormat(bitmapSource, out pPixelFormatEnum));
		return new PixelFormat(pPixelFormatEnum);
	}

	internal static PixelFormat GetPixelFormat(Guid pixelFormatGuid)
	{
		return GetPixelFormat((PixelFormatEnum)pixelFormatGuid.ToByteArray()[^1]);
	}

	internal static PixelFormat GetPixelFormat(PixelFormatEnum pixelFormatEnum)
	{
		return pixelFormatEnum switch
		{
			PixelFormatEnum.Indexed1 => PixelFormats.Indexed1, 
			PixelFormatEnum.Indexed2 => PixelFormats.Indexed2, 
			PixelFormatEnum.Indexed4 => PixelFormats.Indexed4, 
			PixelFormatEnum.Indexed8 => PixelFormats.Indexed8, 
			PixelFormatEnum.BlackWhite => PixelFormats.BlackWhite, 
			PixelFormatEnum.Gray2 => PixelFormats.Gray2, 
			PixelFormatEnum.Gray4 => PixelFormats.Gray4, 
			PixelFormatEnum.Gray8 => PixelFormats.Gray8, 
			PixelFormatEnum.Bgr555 => PixelFormats.Bgr555, 
			PixelFormatEnum.Bgr565 => PixelFormats.Bgr565, 
			PixelFormatEnum.Bgr101010 => PixelFormats.Bgr101010, 
			PixelFormatEnum.Bgr24 => PixelFormats.Bgr24, 
			PixelFormatEnum.Rgb24 => PixelFormats.Rgb24, 
			PixelFormatEnum.Bgr32 => PixelFormats.Bgr32, 
			PixelFormatEnum.Bgra32 => PixelFormats.Bgra32, 
			PixelFormatEnum.Pbgra32 => PixelFormats.Pbgra32, 
			PixelFormatEnum.Rgb48 => PixelFormats.Rgb48, 
			PixelFormatEnum.Rgba64 => PixelFormats.Rgba64, 
			PixelFormatEnum.Prgba64 => PixelFormats.Prgba64, 
			PixelFormatEnum.Gray16 => PixelFormats.Gray16, 
			PixelFormatEnum.Gray32Float => PixelFormats.Gray32Float, 
			PixelFormatEnum.Rgb128Float => PixelFormats.Rgb128Float, 
			PixelFormatEnum.Rgba128Float => PixelFormats.Rgba128Float, 
			PixelFormatEnum.Prgba128Float => PixelFormats.Prgba128Float, 
			PixelFormatEnum.Cmyk32 => PixelFormats.Cmyk32, 
			_ => PixelFormats.Default, 
		};
	}

	private static PixelFormatFlags GetPixelFormatFlagsFromGuid(Guid pixelFormatGuid)
	{
		PixelFormatFlags result = PixelFormatFlags.BitsPerPixelUndefined;
		if (pixelFormatGuid.CompareTo(WICPixelFormatPhotonFirst) >= 0 && pixelFormatGuid.CompareTo(WICPixelFormatPhotonLast) <= 0)
		{
			switch (pixelFormatGuid.ToByteArray()[15])
			{
			case 29:
				result = PixelFormatFlags.IsScRGB | PixelFormatFlags.ChannelOrderARGB;
				break;
			case 30:
				result = PixelFormatFlags.IsScRGB | PixelFormatFlags.ChannelOrderARGB;
				break;
			case 31:
				result = PixelFormatFlags.IsCMYK;
				break;
			case 32:
				result = PixelFormatFlags.IsNChannel;
				break;
			case 33:
				result = PixelFormatFlags.IsNChannel;
				break;
			case 34:
				result = PixelFormatFlags.IsNChannel;
				break;
			case 35:
				result = PixelFormatFlags.IsNChannel;
				break;
			case 36:
				result = PixelFormatFlags.IsNChannel;
				break;
			case 37:
				result = PixelFormatFlags.IsNChannel;
				break;
			case 38:
				result = PixelFormatFlags.IsNChannel;
				break;
			case 39:
				result = PixelFormatFlags.IsNChannel;
				break;
			case 40:
				result = PixelFormatFlags.IsNChannel;
				break;
			case 41:
				result = PixelFormatFlags.IsNChannel;
				break;
			case 42:
				result = PixelFormatFlags.IsNChannel;
				break;
			case 43:
				result = PixelFormatFlags.IsNChannel;
				break;
			case 44:
				result = PixelFormatFlags.IsCMYK | PixelFormatFlags.NChannelAlpha;
				break;
			case 45:
				result = PixelFormatFlags.IsCMYK | PixelFormatFlags.NChannelAlpha;
				break;
			case 46:
				result = PixelFormatFlags.NChannelAlpha | PixelFormatFlags.IsNChannel;
				break;
			case 47:
				result = PixelFormatFlags.NChannelAlpha | PixelFormatFlags.IsNChannel;
				break;
			case 48:
				result = PixelFormatFlags.NChannelAlpha | PixelFormatFlags.IsNChannel;
				break;
			case 49:
				result = PixelFormatFlags.NChannelAlpha | PixelFormatFlags.IsNChannel;
				break;
			case 50:
				result = PixelFormatFlags.NChannelAlpha | PixelFormatFlags.IsNChannel;
				break;
			case 51:
				result = PixelFormatFlags.NChannelAlpha | PixelFormatFlags.IsNChannel;
				break;
			case 52:
				result = PixelFormatFlags.NChannelAlpha | PixelFormatFlags.IsNChannel;
				break;
			case 53:
				result = PixelFormatFlags.NChannelAlpha | PixelFormatFlags.IsNChannel;
				break;
			case 54:
				result = PixelFormatFlags.NChannelAlpha | PixelFormatFlags.IsNChannel;
				break;
			case 55:
				result = PixelFormatFlags.NChannelAlpha | PixelFormatFlags.IsNChannel;
				break;
			case 56:
				result = PixelFormatFlags.NChannelAlpha | PixelFormatFlags.IsNChannel;
				break;
			case 57:
				result = PixelFormatFlags.NChannelAlpha | PixelFormatFlags.IsNChannel;
				break;
			case 58:
				result = PixelFormatFlags.IsScRGB | PixelFormatFlags.ChannelOrderARGB;
				break;
			case 59:
				result = PixelFormatFlags.IsScRGB | PixelFormatFlags.ChannelOrderRGB;
				break;
			case 61:
				result = PixelFormatFlags.IsScRGB | PixelFormatFlags.ChannelOrderRGB;
				break;
			case 62:
				result = PixelFormatFlags.IsGray | PixelFormatFlags.IsScRGB;
				break;
			case 63:
				result = PixelFormatFlags.IsGray | PixelFormatFlags.IsScRGB;
				break;
			case 64:
				result = PixelFormatFlags.IsScRGB | PixelFormatFlags.ChannelOrderRGB;
				break;
			case 65:
				result = PixelFormatFlags.IsScRGB | PixelFormatFlags.ChannelOrderRGB;
				break;
			case 66:
				result = PixelFormatFlags.IsScRGB | PixelFormatFlags.ChannelOrderRGB;
				break;
			}
		}
		return result;
	}

	private static PixelFormatFlags GetPixelFormatFlagsFromEnum(PixelFormatEnum pixelFormatEnum)
	{
		return pixelFormatEnum switch
		{
			PixelFormatEnum.Default => PixelFormatFlags.BitsPerPixelUndefined, 
			PixelFormatEnum.Indexed1 => PixelFormatFlags.BitsPerPixel1 | PixelFormatFlags.Palettized, 
			PixelFormatEnum.Indexed2 => PixelFormatFlags.BitsPerPixel2 | PixelFormatFlags.Palettized, 
			PixelFormatEnum.Indexed4 => PixelFormatFlags.BitsPerPixel4 | PixelFormatFlags.Palettized, 
			PixelFormatEnum.Indexed8 => PixelFormatFlags.BitsPerPixel8 | PixelFormatFlags.Palettized, 
			PixelFormatEnum.BlackWhite => PixelFormatFlags.BitsPerPixel1 | PixelFormatFlags.IsGray, 
			PixelFormatEnum.Gray2 => PixelFormatFlags.BitsPerPixel2 | PixelFormatFlags.IsGray, 
			PixelFormatEnum.Gray4 => PixelFormatFlags.BitsPerPixel4 | PixelFormatFlags.IsGray, 
			PixelFormatEnum.Gray8 => PixelFormatFlags.BitsPerPixel8 | PixelFormatFlags.IsGray, 
			PixelFormatEnum.Bgr555 => PixelFormatFlags.BitsPerPixel16 | PixelFormatFlags.IsSRGB | PixelFormatFlags.ChannelOrderBGR, 
			PixelFormatEnum.Bgr565 => PixelFormatFlags.BitsPerPixel16 | PixelFormatFlags.IsSRGB | PixelFormatFlags.ChannelOrderBGR, 
			PixelFormatEnum.Bgr101010 => PixelFormatFlags.BitsPerPixel32 | PixelFormatFlags.IsSRGB | PixelFormatFlags.ChannelOrderBGR, 
			PixelFormatEnum.Bgr24 => PixelFormatFlags.BitsPerPixel24 | PixelFormatFlags.IsSRGB | PixelFormatFlags.ChannelOrderBGR, 
			PixelFormatEnum.Rgb24 => PixelFormatFlags.BitsPerPixel24 | PixelFormatFlags.IsSRGB | PixelFormatFlags.ChannelOrderRGB, 
			PixelFormatEnum.Bgr32 => PixelFormatFlags.BitsPerPixel32 | PixelFormatFlags.IsSRGB | PixelFormatFlags.ChannelOrderBGR, 
			PixelFormatEnum.Bgra32 => PixelFormatFlags.BitsPerPixel32 | PixelFormatFlags.IsSRGB | PixelFormatFlags.ChannelOrderABGR, 
			PixelFormatEnum.Pbgra32 => PixelFormatFlags.BitsPerPixel32 | PixelFormatFlags.IsSRGB | PixelFormatFlags.Premultiplied | PixelFormatFlags.ChannelOrderABGR, 
			PixelFormatEnum.Rgb48 => PixelFormatFlags.BitsPerPixel48 | PixelFormatFlags.IsSRGB | PixelFormatFlags.ChannelOrderRGB, 
			PixelFormatEnum.Rgba64 => PixelFormatFlags.BitsPerPixel64 | PixelFormatFlags.IsSRGB | PixelFormatFlags.ChannelOrderARGB, 
			PixelFormatEnum.Prgba64 => PixelFormatFlags.BitsPerPixel64 | PixelFormatFlags.IsSRGB | PixelFormatFlags.Premultiplied | PixelFormatFlags.ChannelOrderARGB, 
			PixelFormatEnum.Gray16 => PixelFormatFlags.BitsPerPixel16 | PixelFormatFlags.IsGray | PixelFormatFlags.IsSRGB, 
			PixelFormatEnum.Gray32Float => PixelFormatFlags.BitsPerPixel32 | PixelFormatFlags.IsGray | PixelFormatFlags.IsScRGB, 
			PixelFormatEnum.Rgb128Float => PixelFormatFlags.BitsPerPixel128 | PixelFormatFlags.IsScRGB | PixelFormatFlags.ChannelOrderRGB, 
			PixelFormatEnum.Rgba128Float => PixelFormatFlags.BitsPerPixel128 | PixelFormatFlags.IsScRGB | PixelFormatFlags.ChannelOrderARGB, 
			PixelFormatEnum.Prgba128Float => PixelFormatFlags.BitsPerPixel128 | PixelFormatFlags.IsScRGB | PixelFormatFlags.Premultiplied | PixelFormatFlags.ChannelOrderARGB, 
			PixelFormatEnum.Cmyk32 => PixelFormatFlags.BitsPerPixel32 | PixelFormatFlags.IsCMYK, 
			_ => PixelFormatFlags.BitsPerPixelUndefined, 
		};
	}

	private static uint GetBitsPerPixelFromEnum(PixelFormatEnum pixelFormatEnum)
	{
		switch (pixelFormatEnum)
		{
		case PixelFormatEnum.Default:
			return 0u;
		case PixelFormatEnum.Indexed1:
			return 1u;
		case PixelFormatEnum.Indexed2:
			return 2u;
		case PixelFormatEnum.Indexed4:
			return 4u;
		case PixelFormatEnum.Indexed8:
			return 8u;
		case PixelFormatEnum.BlackWhite:
			return 1u;
		case PixelFormatEnum.Gray2:
			return 2u;
		case PixelFormatEnum.Gray4:
			return 4u;
		case PixelFormatEnum.Gray8:
			return 8u;
		case PixelFormatEnum.Bgr555:
		case PixelFormatEnum.Bgr565:
			return 16u;
		case PixelFormatEnum.Bgr101010:
			return 32u;
		case PixelFormatEnum.Bgr24:
		case PixelFormatEnum.Rgb24:
			return 24u;
		case PixelFormatEnum.Bgr32:
		case PixelFormatEnum.Bgra32:
		case PixelFormatEnum.Pbgra32:
			return 32u;
		case PixelFormatEnum.Rgb48:
			return 48u;
		case PixelFormatEnum.Rgba64:
		case PixelFormatEnum.Prgba64:
			return 64u;
		case PixelFormatEnum.Gray16:
			return 16u;
		case PixelFormatEnum.Gray32Float:
			return 32u;
		case PixelFormatEnum.Rgba128Float:
		case PixelFormatEnum.Prgba128Float:
		case PixelFormatEnum.Rgb128Float:
			return 128u;
		case PixelFormatEnum.Cmyk32:
			return 32u;
		default:
			return 0u;
		}
	}
}
