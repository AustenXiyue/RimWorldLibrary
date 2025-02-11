using System.Collections.Generic;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

/// <summary>Defines the available color palette for a supported image type.</summary>
public sealed class BitmapPalette : DispatcherObject
{
	private struct ImagePaletteColor
	{
		public byte B;

		public byte G;

		public byte R;

		public byte A;
	}

	private SafeMILHandle _palette;

	private IList<Color> _colors = new PartialList<Color>(new List<Color>());

	/// <summary>Get the colors defined in a palette.</summary>
	/// <returns>The list of colors defined in a palette.</returns>
	public IList<Color> Colors => _colors;

	internal SafeMILHandle InternalPalette
	{
		get
		{
			if (_palette == null || _palette.IsInvalid)
			{
				_palette = CreateInternalPalette();
			}
			return _palette;
		}
	}

	private BitmapPalette()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.BitmapPalette" /> class with the specified colors.</summary>
	/// <param name="colors">The colors to add to the custom palette.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="colors" /> parameter is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <paramref name="colors" /> parameter is less than 1 or greater than 256.</exception>
	public BitmapPalette(IList<Color> colors)
	{
		if (colors == null)
		{
			throw new ArgumentNullException("colors");
		}
		int count = colors.Count;
		if (count < 1 || count > 256)
		{
			throw new InvalidOperationException(SR.Format(SR.Image_PaletteZeroColors, null));
		}
		Color[] array = new Color[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = colors[i];
		}
		_colors = new PartialList<Color>(array);
		_palette = CreateInternalPalette();
		UpdateUnmanaged();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.BitmapPalette" /> class based on the specified <see cref="T:System.Windows.Media.Imaging.BitmapSource" />. The new <see cref="T:System.Windows.Media.Imaging.BitmapPalette" /> is limited to a specified maximum color count.</summary>
	/// <param name="bitmapSource">The source bitmap from which the palette is read or constructed.</param>
	/// <param name="maxColorCount">The maximum number of colors the new <see cref="T:System.Windows.Media.Imaging.BitmapPalette" /> can use.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="bitmapSource" /> parameter is null.</exception>
	public BitmapPalette(BitmapSource bitmapSource, int maxColorCount)
	{
		if (bitmapSource == null)
		{
			throw new ArgumentNullException("bitmapSource");
		}
		SafeMILHandle wicSourceHandle = bitmapSource.WicSourceHandle;
		_palette = CreateInternalPalette();
		lock (bitmapSource.SyncObject)
		{
			HRESULT.Check(UnsafeNativeMethods.WICPalette.InitializeFromBitmap(_palette, wicSourceHandle, maxColorCount, fAddTransparentColor: false));
		}
		UpdateManaged();
	}

	internal BitmapPalette(WICPaletteType paletteType, bool addtransparentColor)
	{
		if ((uint)(paletteType - 2) > 10u)
		{
			throw new ArgumentException(SR.Format(SR.Image_PaletteFixedType, paletteType));
		}
		_palette = CreateInternalPalette();
		HRESULT.Check(UnsafeNativeMethods.WICPalette.InitializePredefined(_palette, paletteType, addtransparentColor));
		UpdateManaged();
	}

	internal BitmapPalette(SafeMILHandle unmanagedPalette)
	{
		_palette = unmanagedPalette;
		UpdateManaged();
	}

	internal static BitmapPalette CreateFromBitmapSource(BitmapSource source)
	{
		SafeMILHandle wicSourceHandle = source.WicSourceHandle;
		SafeMILHandle safeMILHandle = CreateInternalPalette();
		lock (source.SyncObject)
		{
			if (UnsafeNativeMethods.WICBitmapSource.CopyPalette(wicSourceHandle, safeMILHandle) != 0)
			{
				return null;
			}
		}
		HRESULT.Check(UnsafeNativeMethods.WICPalette.GetType(safeMILHandle, out var pePaletteType));
		HRESULT.Check(UnsafeNativeMethods.WICPalette.HasAlpha(safeMILHandle, out var pfHasAlpha));
		if (pePaletteType == WICPaletteType.WICPaletteTypeCustom || pePaletteType == WICPaletteType.WICPaletteTypeOptimal)
		{
			return new BitmapPalette(safeMILHandle);
		}
		return BitmapPalettes.FromMILPaletteType(pePaletteType, pfHasAlpha);
	}

	internal static bool DoesPaletteHaveAlpha(BitmapPalette palette)
	{
		if (palette != null)
		{
			foreach (Color color in palette.Colors)
			{
				if (color.A != byte.MaxValue)
				{
					return true;
				}
			}
		}
		return false;
	}

	internal static SafeMILHandle CreateInternalPalette()
	{
		SafeMILHandle ppIPalette = null;
		using FactoryMaker factoryMaker = new FactoryMaker();
		HRESULT.Check(UnsafeNativeMethods.WICImagingFactory.CreatePalette(factoryMaker.ImagingFactoryPtr, out ppIPalette));
		return ppIPalette;
	}

	private unsafe void UpdateUnmanaged()
	{
		int num = Math.Min(256, _colors.Count);
		ImagePaletteColor[] array = new ImagePaletteColor[num];
		for (int i = 0; i < num; i++)
		{
			Color color = _colors[i];
			array[i].B = color.B;
			array[i].G = color.G;
			array[i].R = color.R;
			array[i].A = color.A;
		}
		fixed (ImagePaletteColor* ptr = array)
		{
			void* pColors = ptr;
			HRESULT.Check(UnsafeNativeMethods.WICPalette.InitializeCustom(_palette, (nint)pColors, num));
		}
	}

	private unsafe void UpdateManaged()
	{
		int pColorCount = 0;
		int pcActualCount = 0;
		HRESULT.Check(UnsafeNativeMethods.WICPalette.GetColorCount(_palette, out pColorCount));
		List<Color> list = new List<Color>();
		if (pColorCount < 1 || pColorCount > 256)
		{
			throw new InvalidOperationException(SR.Format(SR.Image_PaletteZeroColors, null));
		}
		ImagePaletteColor[] array = new ImagePaletteColor[pColorCount];
		fixed (ImagePaletteColor* ptr = array)
		{
			void* pColors = ptr;
			HRESULT.Check(UnsafeNativeMethods.WICPalette.GetColors(_palette, pColorCount, (nint)pColors, out pcActualCount));
		}
		for (int i = 0; i < pColorCount; i++)
		{
			ImagePaletteColor imagePaletteColor = array[i];
			list.Add(Color.FromArgb(imagePaletteColor.A, imagePaletteColor.R, imagePaletteColor.G, imagePaletteColor.B));
		}
		_colors = new PartialList<Color>(list);
	}
}
