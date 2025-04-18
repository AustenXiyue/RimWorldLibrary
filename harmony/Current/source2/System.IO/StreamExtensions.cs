namespace System.IO;

internal static class StreamExtensions
{
	public static void CopyTo(this Stream src, Stream destination)
	{
		System.ThrowHelper.ThrowIfArgumentNull(src, "src");
		src.CopyTo(destination);
	}

	public static void CopyTo(this Stream src, Stream destination, int bufferSize)
	{
		System.ThrowHelper.ThrowIfArgumentNull(src, "src");
		src.CopyTo(destination, bufferSize);
	}
}
