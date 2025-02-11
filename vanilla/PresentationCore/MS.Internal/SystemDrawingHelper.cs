using System;
using System.IO;

namespace MS.Internal;

internal static class SystemDrawingHelper
{
	internal static bool IsBitmap(object data)
	{
		return AssemblyHelper.ExtensionsForSystemDrawing()?.IsBitmap(data) ?? false;
	}

	internal static bool IsImage(object data)
	{
		return AssemblyHelper.ExtensionsForSystemDrawing()?.IsImage(data) ?? false;
	}

	internal static bool IsMetafile(object data)
	{
		return AssemblyHelper.ExtensionsForSystemDrawing()?.IsMetafile(data) ?? false;
	}

	internal static nint GetHandleFromMetafile(object data)
	{
		return AssemblyHelper.ExtensionsForSystemDrawing()?.GetHandleFromMetafile(data) ?? IntPtr.Zero;
	}

	internal static object GetMetafileFromHemf(nint hMetafile)
	{
		return AssemblyHelper.ExtensionsForSystemDrawing(force: true)?.GetMetafileFromHemf(hMetafile);
	}

	internal static object GetBitmap(object data)
	{
		return AssemblyHelper.ExtensionsForSystemDrawing(force: true)?.GetBitmap(data);
	}

	internal static nint GetHBitmap(object data, out int width, out int height)
	{
		SystemDrawingExtensionMethods systemDrawingExtensionMethods = AssemblyHelper.ExtensionsForSystemDrawing(force: true);
		if (systemDrawingExtensionMethods != null)
		{
			return systemDrawingExtensionMethods.GetHBitmap(data, out width, out height);
		}
		width = (height = 0);
		return IntPtr.Zero;
	}

	internal static nint GetHBitmapFromBitmap(object data)
	{
		return AssemblyHelper.ExtensionsForSystemDrawing()?.GetHBitmapFromBitmap(data) ?? IntPtr.Zero;
	}

	internal static nint ConvertMetafileToHBitmap(nint handle)
	{
		return AssemblyHelper.ExtensionsForSystemDrawing(force: true)?.ConvertMetafileToHBitmap(handle) ?? IntPtr.Zero;
	}

	internal static Stream GetCommentFromGifStream(Stream stream)
	{
		return AssemblyHelper.ExtensionsForSystemDrawing(force: true)?.GetCommentFromGifStream(stream);
	}

	internal static void SaveMetafileToImageStream(MemoryStream metafileStream, Stream imageStream)
	{
		AssemblyHelper.ExtensionsForSystemDrawing(force: true)?.SaveMetafileToImageStream(metafileStream, imageStream);
	}

	internal static object GetBitmapFromBitmapSource(object source)
	{
		return AssemblyHelper.ExtensionsForSystemDrawing(force: true)?.GetBitmapFromBitmapSource(source);
	}
}
