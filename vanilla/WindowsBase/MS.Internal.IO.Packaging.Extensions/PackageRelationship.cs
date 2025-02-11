using System;
using System.IO.Packaging;

namespace MS.Internal.IO.Packaging.Extensions;

internal class PackageRelationship
{
	public static Uri ContainerRelationshipPartName => System.IO.Packaging.PackUriHelper.CreatePartUri(new Uri("/_rels/.rels", UriKind.Relative));
}
