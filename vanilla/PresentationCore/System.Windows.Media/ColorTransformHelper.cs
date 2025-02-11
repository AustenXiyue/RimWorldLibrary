using System.Runtime.InteropServices;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32.PresentationCore;

namespace System.Windows.Media;

internal class ColorTransformHelper
{
	private ColorTransformHandle _transformHandle;

	private const uint INTENT_PERCEPTUAL = 0u;

	private const uint INTENT_RELATIVE_COLORIMETRIC = 1u;

	private const uint INTENT_SATURATION = 2u;

	private const uint INTENT_ABSOLUTE_COLORIMETRIC = 3u;

	private const uint PROOF_MODE = 1u;

	private const uint NORMAL_MODE = 2u;

	private const uint BEST_MODE = 3u;

	private const uint ENABLE_GAMUT_CHECKING = 65536u;

	private const uint USE_RELATIVE_COLORIMETRIC = 131072u;

	private const uint FAST_TRANSLATE = 262144u;

	internal ColorTransformHelper()
	{
	}

	internal void CreateTransform(SafeProfileHandle sourceProfile, SafeProfileHandle destinationProfile)
	{
		if (sourceProfile == null || sourceProfile.IsInvalid)
		{
			throw new ArgumentNullException("sourceProfile");
		}
		if (destinationProfile == null || destinationProfile.IsInvalid)
		{
			throw new ArgumentNullException("destinationProfile");
		}
		nint[] array = new nint[2];
		bool success = true;
		sourceProfile.DangerousAddRef(ref success);
		destinationProfile.DangerousAddRef(ref success);
		try
		{
			array[0] = sourceProfile.DangerousGetHandle();
			array[1] = destinationProfile.DangerousGetHandle();
			uint[] array2 = new uint[2];
			_transformHandle = UnsafeNativeMethods.Mscms.CreateMultiProfileTransform(array, (uint)array.Length, array2, (uint)array2.Length, 131075u, 0u);
		}
		finally
		{
			sourceProfile.DangerousRelease();
			destinationProfile.DangerousRelease();
		}
		if (_transformHandle == null || _transformHandle.IsInvalid)
		{
			HRESULT.Check(Marshal.GetHRForLastWin32Error());
		}
	}

	internal void TranslateColors(nint paInputColors, uint numColors, uint inputColorType, nint paOutputColors, uint outputColorType)
	{
		if (_transformHandle == null || _transformHandle.IsInvalid)
		{
			throw new InvalidOperationException(SR.Image_ColorTransformInvalid);
		}
		HRESULT.Check(UnsafeNativeMethods.Mscms.TranslateColors(_transformHandle, paInputColors, numColors, inputColorType, paOutputColors, outputColorType));
	}
}
