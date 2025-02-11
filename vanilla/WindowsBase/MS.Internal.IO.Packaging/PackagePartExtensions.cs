using System.IO;
using System.IO.Packaging;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal static class PackagePartExtensions
{
	internal static Stream GetSeekableStream(this PackagePart packPart)
	{
		return packPart.GetSeekableStream(FileMode.OpenOrCreate, packPart.Package.FileOpenAccess);
	}

	internal static Stream GetSeekableStream(this PackagePart packPart, FileMode mode)
	{
		return packPart.GetSeekableStream(mode, packPart.Package.FileOpenAccess);
	}

	internal static Stream GetSeekableStream(this PackagePart packPart, FileMode mode, FileAccess access)
	{
		Stream stream = packPart.GetStream(mode, access);
		if (stream.CanSeek)
		{
			return stream;
		}
		using (stream)
		{
			MemoryStream memoryStream = new MemoryStream((int)stream.Length);
			stream.CopyTo(memoryStream);
			memoryStream.Position = 0L;
			return memoryStream;
		}
	}
}
