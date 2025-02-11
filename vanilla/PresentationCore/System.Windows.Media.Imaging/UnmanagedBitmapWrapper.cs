namespace System.Windows.Media.Imaging;

internal sealed class UnmanagedBitmapWrapper : BitmapSource
{
	public UnmanagedBitmapWrapper(BitmapSourceSafeMILHandle bitmapSource)
		: base(useVirtuals: true)
	{
		_bitmapInit.BeginInit();
		base.WicSourceHandle = bitmapSource;
		_bitmapInit.EndInit();
		UpdateCachedSettings();
	}

	internal UnmanagedBitmapWrapper(bool initialize)
		: base(useVirtuals: true)
	{
		if (initialize)
		{
			_bitmapInit.BeginInit();
			_bitmapInit.EndInit();
		}
	}

	protected override Freezable CreateInstanceCore()
	{
		return new UnmanagedBitmapWrapper(initialize: false);
	}

	private void CopyCommon(UnmanagedBitmapWrapper sourceBitmap)
	{
		_bitmapInit.BeginInit();
		_bitmapInit.EndInit();
	}

	protected override void CloneCore(Freezable sourceFreezable)
	{
		UnmanagedBitmapWrapper sourceBitmap = (UnmanagedBitmapWrapper)sourceFreezable;
		base.CloneCore(sourceFreezable);
		CopyCommon(sourceBitmap);
	}

	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		UnmanagedBitmapWrapper sourceBitmap = (UnmanagedBitmapWrapper)sourceFreezable;
		base.CloneCurrentValueCore(sourceFreezable);
		CopyCommon(sourceBitmap);
	}

	protected override void GetAsFrozenCore(Freezable sourceFreezable)
	{
		UnmanagedBitmapWrapper sourceBitmap = (UnmanagedBitmapWrapper)sourceFreezable;
		base.GetAsFrozenCore(sourceFreezable);
		CopyCommon(sourceBitmap);
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
	{
		UnmanagedBitmapWrapper sourceBitmap = (UnmanagedBitmapWrapper)sourceFreezable;
		base.GetCurrentValueAsFrozenCore(sourceFreezable);
		CopyCommon(sourceBitmap);
	}
}
