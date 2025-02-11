using System.Windows.Media.Composition;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

internal class BitmapVisualManager : DispatcherObject
{
	private RenderTargetBitmap _bitmapTarget;

	private BitmapVisualManager()
	{
	}

	public BitmapVisualManager(RenderTargetBitmap bitmapTarget)
	{
		if (bitmapTarget == null)
		{
			throw new ArgumentNullException("bitmapTarget");
		}
		if (bitmapTarget.IsFrozen)
		{
			throw new ArgumentException(SR.Format(SR.Image_CantBeFrozen, null));
		}
		_bitmapTarget = bitmapTarget;
	}

	public void Render(Visual visual)
	{
		Render(visual, Matrix.Identity, Rect.Empty);
	}

	internal void Render(Visual visual, Matrix worldTransform, Rect windowClip)
	{
		if (visual == null)
		{
			throw new ArgumentNullException("visual");
		}
		if (_bitmapTarget.IsFrozen)
		{
			throw new ArgumentException(SR.Image_CantBeFrozen);
		}
		int pixelWidth = _bitmapTarget.PixelWidth;
		int pixelHeight = _bitmapTarget.PixelHeight;
		double num = _bitmapTarget.DpiX;
		double num2 = _bitmapTarget.DpiY;
		if (pixelWidth > 0 && pixelHeight > 0)
		{
			if (num <= 0.0 || num2 <= 0.0)
			{
				num = 96.0;
				num2 = 96.0;
			}
			SafeMILHandle mILRenderTarget = _bitmapTarget.MILRenderTarget;
			nint ppvObject = IntPtr.Zero;
			try
			{
				MediaContext currentMediaContext = MediaContext.CurrentMediaContext;
				DUCE.Channel channel = currentMediaContext.AllocateSyncChannel();
				Guid guid = MILGuidData.IID_IMILRenderTargetBitmap;
				HRESULT.Check(UnsafeNativeMethods.MILUnknown.QueryInterface(mILRenderTarget, ref guid, out ppvObject));
				Renderer.Render(ppvObject, channel, visual, pixelWidth, pixelHeight, num, num2, worldTransform, windowClip);
				currentMediaContext.ReleaseSyncChannel(channel);
			}
			finally
			{
				UnsafeNativeMethods.MILUnknown.ReleaseInterface(ref ppvObject);
			}
			_bitmapTarget.RenderTargetContentsChanged();
		}
	}
}
