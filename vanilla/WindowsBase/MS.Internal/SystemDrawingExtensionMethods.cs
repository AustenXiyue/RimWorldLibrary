using System.IO;

namespace MS.Internal;

internal abstract class SystemDrawingExtensionMethods
{
	internal abstract bool IsBitmap(object data);

	internal abstract bool IsImage(object data);

	internal abstract bool IsMetafile(object data);

	internal abstract nint GetHandleFromMetafile(object data);

	internal abstract object GetMetafileFromHemf(nint hMetafile);

	internal abstract object GetBitmap(object data);

	internal abstract nint GetHBitmap(object data, out int width, out int height);

	internal abstract nint GetHBitmapFromBitmap(object data);

	internal abstract nint ConvertMetafileToHBitmap(nint handle);

	internal abstract Stream GetCommentFromGifStream(Stream stream);

	internal abstract void SaveMetafileToImageStream(MemoryStream metafileStream, Stream imageStream);

	internal abstract object GetBitmapFromBitmapSource(object source);
}
