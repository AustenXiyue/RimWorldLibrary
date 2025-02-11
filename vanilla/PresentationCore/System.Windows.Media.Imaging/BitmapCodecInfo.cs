using System.Text;
using MS.Internal;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

/// <summary>Provides information about an imaging codec.</summary>
public abstract class BitmapCodecInfo
{
	private bool _isBuiltIn;

	private SafeMILHandle _codecInfoHandle;

	/// <summary>Gets a value that identifies the container format for the codec.</summary>
	/// <returns>The container format of the codec.</returns>
	public virtual Guid ContainerFormat
	{
		get
		{
			EnsureBuiltIn();
			HRESULT.Check(UnsafeNativeMethods.WICBitmapCodecInfo.GetContainerFormat(_codecInfoHandle, out var pguidContainerFormat));
			return pguidContainerFormat;
		}
	}

	/// <summary>Gets a value that identifies the author of the codec.</summary>
	/// <returns>The author of the codec.</returns>
	public virtual string Author
	{
		get
		{
			EnsureBuiltIn();
			StringBuilder stringBuilder = null;
			uint pcchActual = 0u;
			HRESULT.Check(UnsafeNativeMethods.WICComponentInfo.GetAuthor(_codecInfoHandle, 0u, stringBuilder, out pcchActual));
			if (pcchActual != 0)
			{
				stringBuilder = new StringBuilder((int)pcchActual);
				HRESULT.Check(UnsafeNativeMethods.WICComponentInfo.GetAuthor(_codecInfoHandle, pcchActual, stringBuilder, out pcchActual));
			}
			if (stringBuilder != null)
			{
				return stringBuilder.ToString();
			}
			return string.Empty;
		}
	}

	/// <summary>Gets a value that identifies the version of the codec.</summary>
	/// <returns>The version of the codec.</returns>
	public virtual Version Version
	{
		get
		{
			EnsureBuiltIn();
			StringBuilder stringBuilder = null;
			uint pcchActual = 0u;
			HRESULT.Check(UnsafeNativeMethods.WICComponentInfo.GetVersion(_codecInfoHandle, 0u, stringBuilder, out pcchActual));
			if (pcchActual != 0)
			{
				stringBuilder = new StringBuilder((int)pcchActual);
				HRESULT.Check(UnsafeNativeMethods.WICComponentInfo.GetVersion(_codecInfoHandle, pcchActual, stringBuilder, out pcchActual));
			}
			if (stringBuilder != null)
			{
				return new Version(stringBuilder.ToString());
			}
			return new Version();
		}
	}

	/// <summary>Gets a value that identifies the specification version of the codec.</summary>
	/// <returns>The specification version of the codec.</returns>
	public virtual Version SpecificationVersion
	{
		get
		{
			EnsureBuiltIn();
			StringBuilder stringBuilder = null;
			uint pcchActual = 0u;
			HRESULT.Check(UnsafeNativeMethods.WICComponentInfo.GetSpecVersion(_codecInfoHandle, 0u, stringBuilder, out pcchActual));
			if (pcchActual != 0)
			{
				stringBuilder = new StringBuilder((int)pcchActual);
				HRESULT.Check(UnsafeNativeMethods.WICComponentInfo.GetSpecVersion(_codecInfoHandle, pcchActual, stringBuilder, out pcchActual));
			}
			if (stringBuilder != null)
			{
				return new Version(stringBuilder.ToString());
			}
			return new Version();
		}
	}

	/// <summary>Gets a value that represents the friendly name of the codec.</summary>
	/// <returns>The friendly name of the codec.</returns>
	public virtual string FriendlyName
	{
		get
		{
			EnsureBuiltIn();
			StringBuilder stringBuilder = null;
			uint pcchActual = 0u;
			HRESULT.Check(UnsafeNativeMethods.WICComponentInfo.GetFriendlyName(_codecInfoHandle, 0u, stringBuilder, out pcchActual));
			if (pcchActual != 0)
			{
				stringBuilder = new StringBuilder((int)pcchActual);
				HRESULT.Check(UnsafeNativeMethods.WICComponentInfo.GetFriendlyName(_codecInfoHandle, pcchActual, stringBuilder, out pcchActual));
			}
			if (stringBuilder != null)
			{
				return stringBuilder.ToString();
			}
			return string.Empty;
		}
	}

	/// <summary>Gets a value that identifies the device manufacturer of the codec.</summary>
	/// <returns>The device manufacturer of the codec.</returns>
	public virtual string DeviceManufacturer
	{
		get
		{
			EnsureBuiltIn();
			StringBuilder stringBuilder = null;
			uint pcchActual = 0u;
			HRESULT.Check(UnsafeNativeMethods.WICBitmapCodecInfo.GetDeviceManufacturer(_codecInfoHandle, 0u, stringBuilder, out pcchActual));
			if (pcchActual != 0)
			{
				stringBuilder = new StringBuilder((int)pcchActual);
				HRESULT.Check(UnsafeNativeMethods.WICBitmapCodecInfo.GetDeviceManufacturer(_codecInfoHandle, pcchActual, stringBuilder, out pcchActual));
			}
			if (stringBuilder != null)
			{
				return stringBuilder.ToString();
			}
			return string.Empty;
		}
	}

	/// <summary>Gets a value that identifies the device models of the codec.</summary>
	/// <returns>The device model of the codec.</returns>
	public virtual string DeviceModels
	{
		get
		{
			EnsureBuiltIn();
			StringBuilder stringBuilder = null;
			uint pcchActual = 0u;
			HRESULT.Check(UnsafeNativeMethods.WICBitmapCodecInfo.GetDeviceModels(_codecInfoHandle, 0u, stringBuilder, out pcchActual));
			if (pcchActual != 0)
			{
				stringBuilder = new StringBuilder((int)pcchActual);
				HRESULT.Check(UnsafeNativeMethods.WICBitmapCodecInfo.GetDeviceModels(_codecInfoHandle, pcchActual, stringBuilder, out pcchActual));
			}
			if (stringBuilder != null)
			{
				return stringBuilder.ToString();
			}
			return string.Empty;
		}
	}

	/// <summary>Gets a value that identifies the Multipurpose Internet Mail Extensions (MIME) associated with the codec.</summary>
	/// <returns>The Multipurpose Internet Mail Extensions (MIME) types associated with the codec.</returns>
	public virtual string MimeTypes
	{
		get
		{
			EnsureBuiltIn();
			StringBuilder stringBuilder = null;
			uint pcchActual = 0u;
			HRESULT.Check(UnsafeNativeMethods.WICBitmapCodecInfo.GetMimeTypes(_codecInfoHandle, 0u, stringBuilder, out pcchActual));
			if (pcchActual != 0)
			{
				stringBuilder = new StringBuilder((int)pcchActual);
				HRESULT.Check(UnsafeNativeMethods.WICBitmapCodecInfo.GetMimeTypes(_codecInfoHandle, pcchActual, stringBuilder, out pcchActual));
			}
			if (stringBuilder != null)
			{
				return stringBuilder.ToString();
			}
			return string.Empty;
		}
	}

	/// <summary>Gets a value that identifies the file extensions associated with the codec.</summary>
	/// <returns>The file extensions associated with the codec.</returns>
	public virtual string FileExtensions
	{
		get
		{
			EnsureBuiltIn();
			StringBuilder stringBuilder = null;
			uint pcchActual = 0u;
			HRESULT.Check(UnsafeNativeMethods.WICBitmapCodecInfo.GetFileExtensions(_codecInfoHandle, 0u, stringBuilder, out pcchActual));
			if (pcchActual != 0)
			{
				stringBuilder = new StringBuilder((int)pcchActual);
				HRESULT.Check(UnsafeNativeMethods.WICBitmapCodecInfo.GetFileExtensions(_codecInfoHandle, pcchActual, stringBuilder, out pcchActual));
			}
			if (stringBuilder != null)
			{
				return stringBuilder.ToString();
			}
			return string.Empty;
		}
	}

	/// <summary>Gets a value that indicates whether the codec supports animation.</summary>
	/// <returns>true if the codec supports animation; otherwise, false.</returns>
	public virtual bool SupportsAnimation
	{
		get
		{
			EnsureBuiltIn();
			HRESULT.Check(UnsafeNativeMethods.WICBitmapCodecInfo.DoesSupportAnimation(_codecInfoHandle, out var pfSupportAnimation));
			return pfSupportAnimation;
		}
	}

	/// <summary>Gets a value that indicates whether the codec supports lossless of images.</summary>
	/// <returns>true if the codec supports lossless of images; otherwise, false.</returns>
	public virtual bool SupportsLossless
	{
		get
		{
			EnsureBuiltIn();
			HRESULT.Check(UnsafeNativeMethods.WICBitmapCodecInfo.DoesSupportLossless(_codecInfoHandle, out var pfSupportLossless));
			return pfSupportLossless;
		}
	}

	/// <summary>Gets a value that identifies whether the codec supports multiple frames.</summary>
	/// <returns>true if the codec supports multiple frames; otherwise, false.</returns>
	public virtual bool SupportsMultipleFrames
	{
		get
		{
			EnsureBuiltIn();
			HRESULT.Check(UnsafeNativeMethods.WICBitmapCodecInfo.DoesSupportMultiframe(_codecInfoHandle, out var pfSupportMultiframe));
			return pfSupportMultiframe;
		}
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.Media.Imaging.BitmapCodecInfo" />.</summary>
	protected BitmapCodecInfo()
	{
	}

	internal BitmapCodecInfo(SafeMILHandle codecInfoHandle)
	{
		_isBuiltIn = true;
		_codecInfoHandle = codecInfoHandle;
	}

	private void EnsureBuiltIn()
	{
		if (!_isBuiltIn)
		{
			throw new NotImplementedException();
		}
	}
}
