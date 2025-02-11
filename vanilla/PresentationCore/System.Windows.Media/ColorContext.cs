using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32;
using MS.Win32.PresentationCore;

namespace System.Windows.Media;

/// <summary>Represents the International Color Consortium (ICC) or Image Color Management (ICM) color profile that is associated with a bitmap image.</summary>
public class ColorContext
{
	internal delegate int GetColorContextsDelegate(ref uint numContexts, nint[] colorContextPtrs);

	private struct AbbreviatedPROFILEHEADER
	{
		public uint phSize;

		public uint phCMMType;

		public uint phVersion;

		public uint phClass;

		public MS.Win32.NativeMethods.ColorSpace phDataColorSpace;

		public uint phConnectionSpace;

		public uint phDateTime_0;

		public uint phDateTime_1;

		public uint phDateTime_2;

		public uint phSignature;

		public uint phPlatform;

		public uint phProfileFlags;

		public uint phManufacturer;

		public uint phModel;

		public uint phAttributes_0;

		public uint phAttributes_1;

		public uint phRenderingIntent;

		public uint phIlluminant_0;

		public uint phIlluminant_1;

		public uint phIlluminant_2;

		public uint phCreator;
	}

	internal enum StandardColorSpace
	{
		Unknown = 0,
		Srgb = 1,
		ScRgb = 2,
		Rgb = 3,
		Cmyk = 4,
		Gray = 6,
		Multichannel = 7
	}

	private ColorContextHelper _colorContextHelper;

	private StandardColorSpace _colorSpaceFamily;

	private int _numChannels;

	private SecurityCriticalData<Uri> _profileUri;

	private MS.Internal.SecurityCriticalDataForSet<bool> _isProfileUriNotFromUser;

	private AbbreviatedPROFILEHEADER _profileHeader;

	private SafeMILHandle _colorContextHandle;

	private const int _bufferSizeIncrement = 1048576;

	private const int _maximumColorContextLength = 33554432;

	private static readonly MS.Win32.NativeMethods.COLORTYPE[] _colorTypeFromChannels = new MS.Win32.NativeMethods.COLORTYPE[9]
	{
		MS.Win32.NativeMethods.COLORTYPE.COLOR_UNDEFINED,
		MS.Win32.NativeMethods.COLORTYPE.COLOR_UNDEFINED,
		MS.Win32.NativeMethods.COLORTYPE.COLOR_UNDEFINED,
		MS.Win32.NativeMethods.COLORTYPE.COLOR_3_CHANNEL,
		MS.Win32.NativeMethods.COLORTYPE.COLOR_CMYK,
		MS.Win32.NativeMethods.COLORTYPE.COLOR_5_CHANNEL,
		MS.Win32.NativeMethods.COLORTYPE.COLOR_6_CHANNEL,
		MS.Win32.NativeMethods.COLORTYPE.COLOR_7_CHANNEL,
		MS.Win32.NativeMethods.COLORTYPE.COLOR_8_CHANNEL
	};

	private const string _colorProfileResources = "ColorProfiles";

	private const string _sRGBProfileName = "sRGB_icm";

	/// <summary>Gets a <see cref="T:System.Uri" /> that represents the location of a International Color Consortium (ICC) or Image Color Management (ICM) color profile.</summary>
	/// <returns>A <see cref="T:System.Uri" /> that represents the location of a International Color Consortium (ICC) or Image Color Management (ICM) color profile.</returns>
	public Uri ProfileUri
	{
		get
		{
			Uri value = _profileUri.Value;
			if (_isProfileUriNotFromUser.Value)
			{
				Invariant.Assert(value.IsFile);
			}
			return value;
		}
	}

	internal SafeProfileHandle ProfileHandle => _colorContextHelper.ProfileHandle;

	internal SafeMILHandle ColorContextHandle => _colorContextHandle;

	internal int NumChannels
	{
		get
		{
			if (_colorContextHelper.IsInvalid)
			{
				return 3;
			}
			return _numChannels;
		}
	}

	internal uint ColorType => (uint)_colorTypeFromChannels[NumChannels];

	internal StandardColorSpace ColorSpaceFamily
	{
		get
		{
			if (_colorContextHelper.IsInvalid)
			{
				return StandardColorSpace.Srgb;
			}
			return _colorSpaceFamily;
		}
	}

	internal bool IsValid => !_colorContextHelper.IsInvalid;

	private ColorContext(SafeMILHandle colorContextHandle)
	{
		_colorContextHandle = colorContextHandle;
		if (HRESULT.Failed(UnsafeNativeMethods.IWICColorContext.GetType(_colorContextHandle, out var pType)))
		{
			return;
		}
		switch (pType)
		{
		case UnsafeNativeMethods.IWICColorContext.WICColorContextType.WICColorContextProfile:
		{
			if (HRESULT.Succeeded(UnsafeNativeMethods.IWICColorContext.GetProfileBytes(_colorContextHandle, 0u, null, out var pcbActual)) && pcbActual != 0)
			{
				byte[] array = new byte[pcbActual];
				if (!HRESULT.Failed(UnsafeNativeMethods.IWICColorContext.GetProfileBytes(_colorContextHandle, pcbActual, array, out pcbActual)))
				{
					FromRawBytes(array, (int)pcbActual, dontThrowException: true);
				}
			}
			break;
		}
		case UnsafeNativeMethods.IWICColorContext.WICColorContextType.WICColorContextExifColorSpace:
		{
			if (HRESULT.Failed(UnsafeNativeMethods.IWICColorContext.GetExifColorSpace(_colorContextHandle, out var pValue)))
			{
				break;
			}
			if (pValue == 1 || pValue == 65535)
			{
				byte[] array2 = (byte[])new ResourceManager("ColorProfiles", Assembly.GetAssembly(typeof(ColorContext))).GetObject("sRGB_icm");
				using (FactoryMaker factoryMaker = new FactoryMaker())
				{
					_colorContextHandle.Dispose();
					_colorContextHandle = null;
					if (HRESULT.Failed(UnsafeNativeMethods.WICCodec.CreateColorContext(factoryMaker.ImagingFactoryPtr, out _colorContextHandle)) || HRESULT.Failed(UnsafeNativeMethods.IWICColorContext.InitializeFromMemory(_colorContextHandle, array2, (uint)array2.Length)))
					{
						break;
					}
				}
				FromRawBytes(array2, array2.Length, dontThrowException: true);
			}
			else if (Invariant.Strict)
			{
				Invariant.Assert(condition: false, string.Format(CultureInfo.InvariantCulture, "IWICColorContext::GetExifColorSpace returned {0}.", pValue));
			}
			break;
		}
		default:
			if (Invariant.Strict)
			{
				Invariant.Assert(condition: false, "IWICColorContext::GetType() returned WICColorContextUninitialized.");
			}
			break;
		}
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.Media.ColorContext" /> with the International Color Consortium (ICC) or Image Color Management (ICM) color profile located at a given <see cref="T:System.Uri" />.</summary>
	/// <param name="profileUri">The <see cref="T:System.Uri" /> that identifies the location of the desired color profile.</param>
	public ColorContext(Uri profileUri)
	{
		Initialize(profileUri, isStandardProfileUriNotFromUser: false);
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.Media.ColorContext" /> with the standard color profile (sRGB or RGB ) that most closely matches the supplied <see cref="T:System.Windows.Media.PixelFormat" />.</summary>
	/// <param name="pixelFormat">The <see cref="T:System.Windows.Media.PixelFormat" /> from which the <see cref="T:System.Windows.Media.ColorContext" /> is derived.</param>
	public ColorContext(PixelFormat pixelFormat)
	{
		switch (pixelFormat.Format)
		{
		case PixelFormatEnum.BlackWhite:
		case PixelFormatEnum.Gray2:
		case PixelFormatEnum.Gray4:
		case PixelFormatEnum.Gray8:
		case PixelFormatEnum.Gray32Float:
		case PixelFormatEnum.Rgba64:
		case PixelFormatEnum.Prgba64:
		case PixelFormatEnum.Rgba128Float:
		case PixelFormatEnum.Prgba128Float:
		case PixelFormatEnum.Cmyk32:
			throw new NotSupportedException();
		}
		Initialize(GetStandardColorSpaceProfile(), isStandardProfileUriNotFromUser: true);
	}

	/// <summary>Returns a readable <see cref="T:System.IO.Stream" /> of raw color profile data.</summary>
	/// <returns>A readable <see cref="T:System.IO.Stream" /> of raw color profile data.</returns>
	public Stream OpenProfileStream()
	{
		if (_colorContextHelper.IsInvalid)
		{
			throw new NullReferenceException();
		}
		uint bufferSize = 0u;
		_colorContextHelper.GetColorProfileFromHandle(null, ref bufferSize);
		byte[] buffer = new byte[bufferSize];
		_colorContextHelper.GetColorProfileFromHandle(buffer, ref bufferSize);
		return new MemoryStream(buffer);
	}

	internal static IList<ColorContext> GetColorContextsHelper(GetColorContextsDelegate getColorContexts)
	{
		uint numContexts = 0u;
		List<ColorContext> list = null;
		int num = getColorContexts(ref numContexts, null);
		if (num != -2003292287)
		{
			HRESULT.Check(num);
		}
		if (numContexts != 0)
		{
			SafeMILHandle[] array = new SafeMILHandle[numContexts];
			using (FactoryMaker factoryMaker = new FactoryMaker())
			{
				for (uint num2 = 0u; num2 < numContexts; num2++)
				{
					HRESULT.Check(UnsafeNativeMethods.WICCodec.CreateColorContext(factoryMaker.ImagingFactoryPtr, out array[num2]));
				}
			}
			nint[] array2 = new nint[numContexts];
			for (uint num3 = 0u; num3 < numContexts; num3++)
			{
				array2[num3] = array[num3].DangerousGetHandle();
			}
			HRESULT.Check(getColorContexts(ref numContexts, array2));
			list = new List<ColorContext>((int)numContexts);
			for (uint num4 = 0u; num4 < numContexts; num4++)
			{
				list.Add(new ColorContext(array[num4]));
			}
		}
		return list;
	}

	/// <summary>Determines whether an <see cref="T:System.Object" /> is equal to an instance of <see cref="T:System.Windows.Media.ColorContext" />.</summary>
	/// <returns>true if the supplied <see cref="T:System.Object" /> is equal to this instance of <see cref="T:System.Windows.Media.ColorContext" />; otherwise, false.</returns>
	/// <param name="obj">Identifies the <see cref="T:System.Object" /> to compare for equality.</param>
	public override bool Equals(object obj)
	{
		return obj as ColorContext == this;
	}

	/// <summary>Gets the hash code for this instance of <see cref="T:System.Windows.Media.ColorContext" />. </summary>
	/// <returns>An <see cref="T:System.Int32" /> that represents the hash code for the object.</returns>
	public override int GetHashCode()
	{
		return (int)_profileHeader.phDateTime_2;
	}

	/// <summary>Operates on two instances of <see cref="T:System.Windows.Media.ColorContext" /> to determine equality.</summary>
	/// <returns>true if the instances of <see cref="T:System.Windows.Media.ColorContext" /> are equal; otherwise, false.</returns>
	/// <param name="context1">The first instance of <see cref="T:System.Windows.Media.ColorContext" /> to compare.</param>
	/// <param name="context2">The second instance of <see cref="T:System.Windows.Media.ColorContext" /> to compare.</param>
	public static bool operator ==(ColorContext context1, ColorContext context2)
	{
		if ((object)context1 == null && (object)context2 == null)
		{
			return true;
		}
		if ((object)context1 != null && (object)context2 != null)
		{
			if (context1._profileHeader.phSize == context2._profileHeader.phSize && context1._profileHeader.phCMMType == context2._profileHeader.phCMMType && context1._profileHeader.phVersion == context2._profileHeader.phVersion && context1._profileHeader.phClass == context2._profileHeader.phClass && context1._profileHeader.phDataColorSpace == context2._profileHeader.phDataColorSpace && context1._profileHeader.phConnectionSpace == context2._profileHeader.phConnectionSpace && context1._profileHeader.phDateTime_0 == context2._profileHeader.phDateTime_0 && context1._profileHeader.phDateTime_1 == context2._profileHeader.phDateTime_1 && context1._profileHeader.phDateTime_2 == context2._profileHeader.phDateTime_2 && context1._profileHeader.phSignature == context2._profileHeader.phSignature && context1._profileHeader.phPlatform == context2._profileHeader.phPlatform && context1._profileHeader.phProfileFlags == context2._profileHeader.phProfileFlags && context1._profileHeader.phManufacturer == context2._profileHeader.phManufacturer && context1._profileHeader.phModel == context2._profileHeader.phModel && context1._profileHeader.phAttributes_0 == context2._profileHeader.phAttributes_0 && context1._profileHeader.phAttributes_1 == context2._profileHeader.phAttributes_1 && context1._profileHeader.phRenderingIntent == context2._profileHeader.phRenderingIntent && context1._profileHeader.phIlluminant_0 == context2._profileHeader.phIlluminant_0 && context1._profileHeader.phIlluminant_1 == context2._profileHeader.phIlluminant_1 && context1._profileHeader.phIlluminant_2 == context2._profileHeader.phIlluminant_2)
			{
				return context1._profileHeader.phCreator == context2._profileHeader.phCreator;
			}
			return false;
		}
		return false;
	}

	/// <summary>Operates on two instances of <see cref="T:System.Windows.Media.ColorContext" /> to determine that they are not equal.</summary>
	/// <returns>true if the instances of <see cref="T:System.Windows.Media.ColorContext" /> are not equal; otherwise, false.</returns>
	/// <param name="context1">The first instance of <see cref="T:System.Windows.Media.ColorContext" /> to compare.</param>
	/// <param name="context2">The second instance of <see cref="T:System.Windows.Media.ColorContext" /> to compare.</param>
	public static bool operator !=(ColorContext context1, ColorContext context2)
	{
		return !(context1 == context2);
	}

	private void Initialize(Uri profileUri, bool isStandardProfileUriNotFromUser)
	{
		bool flag = false;
		if (profileUri == null)
		{
			throw new ArgumentNullException("profileUri");
		}
		if (!profileUri.IsAbsoluteUri)
		{
			throw new ArgumentException(SR.UriNotAbsolute, "profileUri");
		}
		_profileUri = new SecurityCriticalData<Uri>(profileUri);
		_isProfileUriNotFromUser = new MS.Internal.SecurityCriticalDataForSet<bool>(isStandardProfileUriNotFromUser);
		Stream stream = null;
		try
		{
			stream = WpfWebRequestHelper.CreateRequestAndGetResponseStream(profileUri);
		}
		catch (WebException)
		{
			if (isStandardProfileUriNotFromUser)
			{
				flag = true;
			}
		}
		if (stream == null)
		{
			if (!flag)
			{
				Invariant.Assert(!isStandardProfileUriNotFromUser);
				throw new FileNotFoundException(SR.Format(SR.FileNotFoundExceptionWithFileName, profileUri.AbsolutePath), profileUri.AbsolutePath);
			}
			stream = new MemoryStream((byte[])new ResourceManager("ColorProfiles", Assembly.GetAssembly(typeof(ColorContext))).GetObject("sRGB_icm"));
		}
		FromStream(stream, profileUri.AbsolutePath);
	}

	private static Uri GetStandardColorSpaceProfile()
	{
		uint dwProfileID = 1934772034u;
		uint pdwSize = 260u;
		StringBuilder stringBuilder = new StringBuilder(260);
		HRESULT.Check(UnsafeNativeMethods.Mscms.GetStandardColorSpaceProfile(IntPtr.Zero, dwProfileID, stringBuilder, out pdwSize));
		string text = stringBuilder.ToString();
		if (!Uri.TryCreate(text, UriKind.Absolute, out Uri result))
		{
			pdwSize = 260u;
			HRESULT.Check(UnsafeNativeMethods.Mscms.GetColorDirectory(IntPtr.Zero, stringBuilder, out pdwSize));
			return new Uri(Path.Combine(stringBuilder.ToString(), text));
		}
		return result;
	}

	private void FromStream(Stream stm, string filename)
	{
		int num = 1048576;
		if (stm.CanSeek)
		{
			num = (int)stm.Length + 1;
		}
		byte[] array = new byte[num];
		int num2 = 0;
		while (num < 33554432)
		{
			num2 += stm.Read(array, num2, num - num2);
			if (num2 < num)
			{
				FromRawBytes(array, num2, dontThrowException: false);
				using FactoryMaker factoryMaker = new FactoryMaker();
				HRESULT.Check(UnsafeNativeMethods.WICCodec.CreateColorContext(factoryMaker.ImagingFactoryPtr, out _colorContextHandle));
				HRESULT.Check(UnsafeNativeMethods.IWICColorContext.InitializeFromMemory(_colorContextHandle, array, (uint)num2));
				return;
			}
			num += 1048576;
			byte[] array2 = new byte[num];
			array.CopyTo(array2, 0);
			array = array2;
		}
		throw new ArgumentException(SR.ColorContext_FileTooLarge, filename);
	}

	private unsafe void FromRawBytes(byte[] data, int dataLength, bool dontThrowException)
	{
		Invariant.Assert(dataLength <= data.Length);
		Invariant.Assert(dataLength >= 0);
		fixed (byte* ptr = data)
		{
			void* pProfileData = ptr;
			MS.Win32.UnsafeNativeMethods.PROFILE profile = default(MS.Win32.UnsafeNativeMethods.PROFILE);
			profile.dwType = MS.Win32.NativeMethods.ProfileType.PROFILE_MEMBUFFER;
			profile.pProfileData = pProfileData;
			profile.cbDataSize = (uint)dataLength;
			_colorContextHelper.OpenColorProfile(ref profile);
			if (_colorContextHelper.IsInvalid)
			{
				if (dontThrowException)
				{
					return;
				}
				HRESULT.Check(Marshal.GetHRForLastWin32Error());
			}
		}
		if (!_colorContextHelper.GetColorProfileHeader(out var header))
		{
			if (dontThrowException)
			{
				return;
			}
			HRESULT.Check(Marshal.GetHRForLastWin32Error());
		}
		_profileHeader.phSize = header.phSize;
		_profileHeader.phCMMType = header.phCMMType;
		_profileHeader.phVersion = header.phVersion;
		_profileHeader.phClass = header.phClass;
		_profileHeader.phDataColorSpace = header.phDataColorSpace;
		_profileHeader.phConnectionSpace = header.phConnectionSpace;
		_profileHeader.phDateTime_0 = header.phDateTime_0;
		_profileHeader.phDateTime_1 = header.phDateTime_1;
		_profileHeader.phDateTime_2 = header.phDateTime_2;
		_profileHeader.phSignature = header.phSignature;
		_profileHeader.phPlatform = header.phPlatform;
		_profileHeader.phProfileFlags = header.phProfileFlags;
		_profileHeader.phManufacturer = header.phManufacturer;
		_profileHeader.phModel = header.phModel;
		_profileHeader.phAttributes_0 = header.phAttributes_0;
		_profileHeader.phAttributes_1 = header.phAttributes_1;
		_profileHeader.phRenderingIntent = header.phRenderingIntent;
		_profileHeader.phIlluminant_0 = header.phIlluminant_0;
		_profileHeader.phIlluminant_1 = header.phIlluminant_1;
		_profileHeader.phIlluminant_2 = header.phIlluminant_2;
		_profileHeader.phCreator = header.phCreator;
		switch (_profileHeader.phDataColorSpace)
		{
		case MS.Win32.NativeMethods.ColorSpace.SPACE_CMY:
		case MS.Win32.NativeMethods.ColorSpace.SPACE_HLS:
		case MS.Win32.NativeMethods.ColorSpace.SPACE_HSV:
		case MS.Win32.NativeMethods.ColorSpace.SPACE_Lab:
		case MS.Win32.NativeMethods.ColorSpace.SPACE_Luv:
		case MS.Win32.NativeMethods.ColorSpace.SPACE_XYZ:
		case MS.Win32.NativeMethods.ColorSpace.SPACE_YCbCr:
		case MS.Win32.NativeMethods.ColorSpace.SPACE_Yxy:
			_numChannels = 3;
			_colorSpaceFamily = StandardColorSpace.Unknown;
			break;
		case MS.Win32.NativeMethods.ColorSpace.SPACE_RGB:
			_colorSpaceFamily = StandardColorSpace.Rgb;
			_numChannels = 3;
			break;
		case MS.Win32.NativeMethods.ColorSpace.SPACE_GRAY:
			_colorSpaceFamily = StandardColorSpace.Gray;
			_numChannels = 1;
			break;
		case MS.Win32.NativeMethods.ColorSpace.SPACE_CMYK:
			_colorSpaceFamily = StandardColorSpace.Cmyk;
			_numChannels = 4;
			break;
		case MS.Win32.NativeMethods.ColorSpace.SPACE_2_CHANNEL:
			_colorSpaceFamily = StandardColorSpace.Multichannel;
			_numChannels = 2;
			break;
		case MS.Win32.NativeMethods.ColorSpace.SPACE_3_CHANNEL:
			_colorSpaceFamily = StandardColorSpace.Multichannel;
			_numChannels = 3;
			break;
		case MS.Win32.NativeMethods.ColorSpace.SPACE_4_CHANNEL:
			_colorSpaceFamily = StandardColorSpace.Multichannel;
			_numChannels = 4;
			break;
		case MS.Win32.NativeMethods.ColorSpace.SPACE_5_CHANNEL:
			_numChannels = 5;
			_colorSpaceFamily = StandardColorSpace.Multichannel;
			break;
		case MS.Win32.NativeMethods.ColorSpace.SPACE_6_CHANNEL:
			_numChannels = 6;
			_colorSpaceFamily = StandardColorSpace.Multichannel;
			break;
		case MS.Win32.NativeMethods.ColorSpace.SPACE_7_CHANNEL:
			_numChannels = 7;
			_colorSpaceFamily = StandardColorSpace.Multichannel;
			break;
		case MS.Win32.NativeMethods.ColorSpace.SPACE_8_CHANNEL:
			_numChannels = 8;
			_colorSpaceFamily = StandardColorSpace.Multichannel;
			break;
		case MS.Win32.NativeMethods.ColorSpace.SPACE_9_CHANNEL:
			_numChannels = 9;
			_colorSpaceFamily = StandardColorSpace.Multichannel;
			break;
		case MS.Win32.NativeMethods.ColorSpace.SPACE_A_CHANNEL:
			_numChannels = 10;
			_colorSpaceFamily = StandardColorSpace.Multichannel;
			break;
		case MS.Win32.NativeMethods.ColorSpace.SPACE_B_CHANNEL:
			_numChannels = 11;
			_colorSpaceFamily = StandardColorSpace.Multichannel;
			break;
		case MS.Win32.NativeMethods.ColorSpace.SPACE_C_CHANNEL:
			_numChannels = 12;
			_colorSpaceFamily = StandardColorSpace.Multichannel;
			break;
		case MS.Win32.NativeMethods.ColorSpace.SPACE_D_CHANNEL:
			_numChannels = 13;
			_colorSpaceFamily = StandardColorSpace.Multichannel;
			break;
		case MS.Win32.NativeMethods.ColorSpace.SPACE_E_CHANNEL:
			_numChannels = 14;
			_colorSpaceFamily = StandardColorSpace.Multichannel;
			break;
		case MS.Win32.NativeMethods.ColorSpace.SPACE_F_CHANNEL:
			_numChannels = 15;
			_colorSpaceFamily = StandardColorSpace.Multichannel;
			break;
		default:
			_numChannels = 0;
			_colorSpaceFamily = StandardColorSpace.Unknown;
			break;
		}
	}
}
