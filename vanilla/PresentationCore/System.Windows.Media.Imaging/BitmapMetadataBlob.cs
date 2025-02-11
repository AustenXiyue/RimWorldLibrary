namespace System.Windows.Media.Imaging;

/// <summary>Provides a placeholder for metadata items that cannot be converted from C# to an underlying data type that persists metadata. The blob is converted into an array of bytes to preserve the content.</summary>
public class BitmapMetadataBlob
{
	private byte[] _blob;

	/// <summary>Initializes an instance of <see cref="T:System.Windows.Media.Imaging.BitmapMetadataBlob" /> and converts the metadata it holds into an array of bytes to persist its content.</summary>
	/// <param name="blob">Placeholder metadata.</param>
	public BitmapMetadataBlob(byte[] blob)
	{
		_blob = blob;
	}

	/// <summary>Returns an array of bytes that represents the value of a <see cref="T:System.Windows.Media.Imaging.BitmapMetadataBlob" />.</summary>
	/// <returns>An array of bytes.</returns>
	public byte[] GetBlobValue()
	{
		return (byte[])_blob.Clone();
	}

	internal byte[] InternalGetBlobValue()
	{
		return _blob;
	}
}
