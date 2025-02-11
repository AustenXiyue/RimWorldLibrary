using System;

namespace MS.Internal;

internal static class MILGuidData
{
	internal static readonly Guid IID_IMILRenderTargetBitmap;

	internal static readonly Guid IID_IWICPalette;

	internal static readonly Guid IID_IWICBitmapSource;

	internal static readonly Guid IID_IWICFormatConverter;

	internal static readonly Guid IID_IWICBitmapScaler;

	internal static readonly Guid IID_IWICBitmapClipper;

	internal static readonly Guid IID_IWICBitmapFlipRotator;

	internal static readonly Guid IID_IWICBitmap;

	internal static readonly Guid IID_IWICBitmapEncoder;

	internal static readonly Guid IID_IWICBitmapFrameEncode;

	internal static readonly Guid IID_IWICBitmapDecoder;

	internal static readonly Guid IID_IWICBitmapFrameDecode;

	internal static readonly Guid IID_IWICMetadataQueryReader;

	internal static readonly Guid IID_IWICMetadataQueryWriter;

	internal static readonly Guid IID_IWICMetadataReader;

	internal static readonly Guid IID_IWICMetadataWriter;

	internal static readonly Guid IID_IWICPixelFormatInfo;

	internal static readonly Guid IID_IWICImagingFactory;

	internal static readonly Guid CLSID_WICBmpDecoder;

	internal static readonly Guid CLSID_WICPngDecoder;

	internal static readonly Guid CLSID_WICIcoDecoder;

	internal static readonly Guid CLSID_WICJpegDecoder;

	internal static readonly Guid CLSID_WICGifDecoder;

	internal static readonly Guid CLSID_WICTiffDecoder;

	internal static readonly Guid CLSID_WICWmpDecoder;

	internal static readonly Guid CLSID_WICBmpEncoder;

	internal static readonly Guid CLSID_WICPngEncoder;

	internal static readonly Guid CLSID_WICJpegEncoder;

	internal static readonly Guid CLSID_WICGifEncoder;

	internal static readonly Guid CLSID_WICTiffEncoder;

	internal static readonly Guid CLSID_WICWmpEncoder;

	internal static readonly Guid GUID_ContainerFormatBmp;

	internal static readonly Guid GUID_ContainerFormatIco;

	internal static readonly Guid GUID_ContainerFormatGif;

	internal static readonly Guid GUID_ContainerFormatJpeg;

	internal static readonly Guid GUID_ContainerFormatPng;

	internal static readonly Guid GUID_ContainerFormatTiff;

	internal static readonly Guid GUID_ContainerFormatWmp;

	internal static readonly byte[] GUID_VendorMicrosoft;

	static MILGuidData()
	{
		IID_IMILRenderTargetBitmap = new Guid(513u, 43250, 18551, 186, 10, 253, 43, 102, 69, 251, 148);
		IID_IWICPalette = new Guid(64u, 43250, 18551, 186, 10, 253, 43, 102, 69, 251, 148);
		IID_IWICBitmapSource = new Guid(288u, 43250, 18551, 186, 10, 253, 43, 102, 69, 251, 148);
		IID_IWICFormatConverter = new Guid(769u, 43250, 18551, 186, 10, 253, 43, 102, 69, 251, 148);
		IID_IWICBitmapScaler = new Guid(770u, 43250, 18551, 186, 10, 253, 43, 102, 69, 251, 148);
		IID_IWICBitmapClipper = new Guid(3841707779u, 8765, 20097, 147, 51, 214, 53, 85, 109, 209, 181);
		IID_IWICBitmapFlipRotator = new Guid(1342800719, 11626, 16846, 158, 27, 23, 197, 175, 247, 167, 130);
		IID_IWICBitmap = new Guid(289u, 43250, 18551, 186, 10, 253, 43, 102, 69, 251, 148);
		IID_IWICBitmapEncoder = new Guid(259u, 43250, 18551, 186, 10, 253, 43, 102, 69, 251, 148);
		IID_IWICBitmapFrameEncode = new Guid(261u, 43250, 18551, 186, 10, 253, 43, 102, 69, 251, 148);
		IID_IWICBitmapDecoder = new Guid(2665343463u, 36334, 18410, 153, 223, 230, 250, 242, 237, 68, 191);
		IID_IWICBitmapFrameDecode = new Guid(991330587, 27203, 20169, 168, 19, 61, 147, 12, 19, 185, 64);
		IID_IWICMetadataQueryReader = new Guid(815306344u, 57801, 17815, 179, 149, 69, 142, 237, 184, 8, 223);
		IID_IWICMetadataQueryWriter = new Guid(2803988762u, 3567, 19718, 189, 145, 33, 24, 191, 29, 177, 11);
		IID_IWICMetadataReader = new Guid(2449800857u, 55548, 20437, 160, 1, 149, 54, 176, 103, 168, 153);
		IID_IWICMetadataWriter = new Guid(4152585750u, 15328, 18187, 134, 187, 22, 13, 10, 236, 215, 222);
		IID_IWICPixelFormatInfo = new Guid(3907888641u, 15688, 17178, 171, 68, 105, 5, 155, 232, 139, 190);
		IID_IWICImagingFactory = new Guid(3965634729u, 50069, 17172, 156, 119, 84, 215, 169, 53, byte.MaxValue, 112);
		CLSID_WICBmpDecoder = new Guid(1799757922, 31935, 16397, 159, 219, 129, 61, 209, 15, 39, 120);
		CLSID_WICPngDecoder = new Guid(949920123, 20600, 19678, 182, 239, 37, 193, 81, 117, 199, 81);
		CLSID_WICIcoDecoder = new Guid(3323722975u, 11791, 19117, 168, 215, 224, 107, 175, 235, 205, 254);
		CLSID_WICJpegDecoder = new Guid(2488706176u, 59531, 17386, 158, 115, 11, 45, 155, 113, 177, 202);
		CLSID_WICGifDecoder = new Guid(941480508u, 40169, 18484, 162, 62, 31, 152, 248, 252, 82, 190);
		CLSID_WICTiffDecoder = new Guid(3041822169u, 65059, 18847, 139, 136, 106, 206, 167, 19, 117, 43);
		CLSID_WICWmpDecoder = new Guid(2725047350u, 9036, 18768, 174, 22, 227, 74, 172, 231, 29, 13);
		CLSID_WICBmpEncoder = new Guid(1774095284u, 54893, 18376, 134, 90, 237, 21, 137, 67, 55, 130);
		CLSID_WICPngEncoder = new Guid(664050025u, 34666, 16855, 148, 71, 86, 143, 106, 53, 164, 220);
		CLSID_WICJpegEncoder = new Guid(439678401, 19034, 18140, 182, 68, 31, 69, 103, 231, 166, 118);
		CLSID_WICGifEncoder = new Guid(290411928, 2850, 16544, 134, 161, 200, 62, 164, 149, 173, 189);
		CLSID_WICTiffEncoder = new Guid(20037136, 8193, 19551, 169, 176, 204, 136, 250, 182, 76, 232);
		CLSID_WICWmpEncoder = new Guid(2890720203u, 57793, 17613, 130, 21, 90, 22, 101, 80, 158, 194);
		GUID_ContainerFormatBmp = new Guid(183621758u, 64766, 16776, 189, 235, 167, 144, 100, 113, 203, 227);
		GUID_ContainerFormatIco = new Guid(2745721028u, 13199, 19479, 145, 154, 251, 164, 181, 98, 143, 33);
		GUID_ContainerFormatGif = new Guid(529159681, 32077, 19645, 156, 130, 27, 200, 212, 238, 185, 165);
		GUID_ContainerFormatJpeg = new Guid(434415018, 22114, 20421, 160, 192, 23, 88, 2, 142, 16, 87);
		GUID_ContainerFormatPng = new Guid(461175540, 28991, 18236, 187, 205, 97, 55, 66, 95, 174, 175);
		GUID_ContainerFormatTiff = new Guid(373017648u, 58089, 20235, 150, 29, 163, 233, 253, 183, 136, 163);
		GUID_ContainerFormatWmp = new Guid(1470332074, 13946, 17728, 145, 107, 241, 131, 197, 9, 58, 75);
		GUID_VendorMicrosoft = new byte[16]
		{
			202, 73, 231, 240, 239, 237, 137, 69, 167, 58,
			238, 14, 98, 106, 42, 43
		};
	}
}
