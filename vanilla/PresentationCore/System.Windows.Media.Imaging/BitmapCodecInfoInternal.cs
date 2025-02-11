namespace System.Windows.Media.Imaging;

internal class BitmapCodecInfoInternal : BitmapCodecInfo
{
	private BitmapCodecInfoInternal()
	{
	}

	internal BitmapCodecInfoInternal(SafeMILHandle codecInfoHandle)
		: base(codecInfoHandle)
	{
	}
}
