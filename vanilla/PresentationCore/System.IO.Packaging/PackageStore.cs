using System.Collections.Specialized;
using System.Windows.Navigation;
using MS.Internal.PresentationCore;

namespace System.IO.Packaging;

/// <summary>Represents a collection of application-specific <see cref="T:System.IO.Packaging.Package" /> instances used in combination with <see cref="T:System.IO.Packaging.PackWebRequest" />.</summary>
public static class PackageStore
{
	private static HybridDictionary _packages;

	private static readonly object _globalLock;

	static PackageStore()
	{
		_globalLock = new object();
	}

	/// <summary>Returns the <see cref="T:System.IO.Packaging.Package" /> with a specified URI from the store.</summary>
	/// <returns>The package with a specified <paramref name="packageUri" />; or null, if a package with the specified <paramref name="packageUri" /> is not in the store.</returns>
	/// <param name="uri">The uniform resource identifier (URI) of the package to return.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="packageUri" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="packageUri" /> is an invalid package URI.</exception>
	public static Package GetPackage(Uri uri)
	{
		ValidatePackageUri(uri);
		lock (_globalLock)
		{
			Package result = null;
			if (_packages != null && _packages.Contains(uri))
			{
				result = (Package)_packages[uri];
			}
			return result;
		}
	}

	/// <summary>Adds a <see cref="T:System.IO.Packaging.Package" /> to the store.</summary>
	/// <param name="uri">The key URI of the <paramref name="package" /> to compare in a <see cref="T:System.IO.Packaging.PackWebRequest" />.</param>
	/// <param name="package">The package to add to the store.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="package" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="packageUri" /> is an invalid package URI.</exception>
	/// <exception cref="T:System.InvalidOperationException">A package with the specified <paramref name="packageUri" /> is already in the store.</exception>
	public static void AddPackage(Uri uri, Package package)
	{
		ValidatePackageUri(uri);
		Uri firstPackUri = PackUriHelper.Create(uri);
		if (PackUriHelper.ComparePackUri(firstPackUri, BaseUriHelper.PackAppBaseUri) == 0 || PackUriHelper.ComparePackUri(firstPackUri, BaseUriHelper.SiteOfOriginBaseUri) == 0)
		{
			throw new ArgumentException(SR.NotAllowedPackageUri, "uri");
		}
		if (package == null)
		{
			throw new ArgumentNullException("package");
		}
		lock (_globalLock)
		{
			if (_packages == null)
			{
				_packages = new HybridDictionary(2);
			}
			if (_packages.Contains(uri))
			{
				throw new InvalidOperationException(SR.PackageAlreadyExists);
			}
			_packages.Add(uri, package);
		}
	}

	/// <summary>Removes the <see cref="T:System.IO.Packaging.Package" /> with a specified URI from the store.</summary>
	/// <param name="uri">The uniform resource identifier (URI) of the package to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="packageUri" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="packageUri" /> is an invalid package URI.</exception>
	public static void RemovePackage(Uri uri)
	{
		ValidatePackageUri(uri);
		lock (_globalLock)
		{
			if (_packages != null)
			{
				_packages.Remove(uri);
			}
		}
	}

	private static void ValidatePackageUri(Uri uri)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		if (!uri.IsAbsoluteUri)
		{
			throw new ArgumentException(SR.UriMustBeAbsolute, "uri");
		}
	}
}
