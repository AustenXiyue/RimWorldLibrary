namespace System.Windows.Media.Imaging;

/// <summary>Provides data for the <see cref="E:System.Windows.Media.Imaging.BitmapSource.DownloadProgress" /> and <see cref="E:System.Windows.Media.Imaging.BitmapDecoder.DownloadProgress" /> events.</summary>
public class DownloadProgressEventArgs : EventArgs
{
	private int _percentComplete;

	/// <summary>Gets a value that represents the download progress of a bitmap, expressed as a percentage. </summary>
	/// <returns>The progress, expressed as a percentage, to which a bitmap has been downloaded. The returned value will be between 1 and 100.</returns>
	public int Progress => _percentComplete;

	internal DownloadProgressEventArgs(int percentComplete)
	{
		_percentComplete = percentComplete;
	}
}
