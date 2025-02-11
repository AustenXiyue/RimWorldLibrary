using System.Net;
using MS.Internal;
using MS.Internal.IO.Packaging;
using MS.Internal.PresentationCore;

namespace System.IO.Packaging;

/// <summary>Represents the class that is invoked when an instance of a pack URI <see cref="T:System.IO.Packaging.PackWebRequest" /> is created.    </summary>
public sealed class PackWebRequestFactory : IWebRequestCreate
{
	private static PackWebRequestFactory _factorySingleton;

	static PackWebRequestFactory()
	{
		_factorySingleton = new PackWebRequestFactory();
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.  Use the type-safe <see cref="T:System.IO.Packaging.PackUriHelper" /> method instead. </summary>
	/// <returns>The pack URI Web request.</returns>
	/// <param name="uri">The URI to create the Web request.</param>
	WebRequest IWebRequestCreate.Create(Uri uri)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		if (!uri.IsAbsoluteUri)
		{
			throw new ArgumentException(SR.UriMustBeAbsolute, "uri");
		}
		if (!string.Equals(uri.Scheme, PackUriHelper.UriSchemePack, StringComparison.Ordinal))
		{
			throw new ArgumentException(SR.Format(SR.UriSchemeMismatch, PackUriHelper.UriSchemePack), "uri");
		}
		Uri packageUri = PackUriHelper.GetPackageUri(uri);
		Uri partUri = PackUriHelper.GetPartUri(uri);
		if (partUri != null)
		{
			bool threadSafe;
			Package package = PreloadedPackages.GetPackage(packageUri, out threadSafe);
			bool respectCachePolicy = false;
			if (package == null)
			{
				threadSafe = false;
				respectCachePolicy = true;
				package = PackageStore.GetPackage(packageUri);
			}
			if (package != null)
			{
				return new PackWebRequest(uri, packageUri, partUri, package, respectCachePolicy, threadSafe);
			}
		}
		return new PackWebRequest(uri, packageUri, partUri);
	}

	[FriendAccessAllowed]
	internal static WebRequest CreateWebRequest(Uri uri)
	{
		if (string.Equals(uri.Scheme, PackUriHelper.UriSchemePack, StringComparison.Ordinal))
		{
			return ((IWebRequestCreate)_factorySingleton).Create(uri);
		}
		return WpfWebRequestHelper.CreateRequest(uri);
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.IO.Packaging.PackWebRequestFactory" /> class. </summary>
	public PackWebRequestFactory()
	{
	}
}
