using System.IO.Packaging;

namespace MS.Internal.IO.Packaging.Extensions;

internal static class PackagePartExtensions
{
	public static ContentType ValidatedContentType(this PackagePart packagePart)
	{
		return new ContentType(packagePart.ContentType);
	}
}
