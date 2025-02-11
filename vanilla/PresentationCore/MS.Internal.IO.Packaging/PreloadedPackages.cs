using System;
using System.Collections.Specialized;
using System.IO.Packaging;
using MS.Internal.PresentationCore;

namespace MS.Internal.IO.Packaging;

[FriendAccessAllowed]
internal static class PreloadedPackages
{
	private class PackageThreadSafePair
	{
		private readonly Package _package;

		private readonly bool _threadSafe;

		internal Package Package => _package;

		internal bool ThreadSafe => _threadSafe;

		internal PackageThreadSafePair(Package package, bool threadSafe)
		{
			Invariant.Assert(package != null);
			_package = package;
			_threadSafe = threadSafe;
		}
	}

	private static HybridDictionary _packagePairs;

	private static readonly object _globalLock;

	static PreloadedPackages()
	{
		_globalLock = new object();
	}

	internal static Package GetPackage(Uri uri)
	{
		bool threadSafe;
		return GetPackage(uri, out threadSafe);
	}

	internal static Package GetPackage(Uri uri, out bool threadSafe)
	{
		ValidateUriKey(uri);
		lock (_globalLock)
		{
			Package result = null;
			threadSafe = false;
			if (_packagePairs != null && _packagePairs[uri] is PackageThreadSafePair packageThreadSafePair)
			{
				result = packageThreadSafePair.Package;
				threadSafe = packageThreadSafePair.ThreadSafe;
			}
			return result;
		}
	}

	internal static void AddPackage(Uri uri, Package package)
	{
		AddPackage(uri, package, threadSafe: false);
	}

	internal static void AddPackage(Uri uri, Package package, bool threadSafe)
	{
		ValidateUriKey(uri);
		lock (_globalLock)
		{
			if (_packagePairs == null)
			{
				_packagePairs = new HybridDictionary(3);
			}
			_packagePairs.Add(uri, new PackageThreadSafePair(package, threadSafe));
		}
	}

	internal static void RemovePackage(Uri uri)
	{
		ValidateUriKey(uri);
		lock (_globalLock)
		{
			if (_packagePairs != null)
			{
				_packagePairs.Remove(uri);
			}
		}
	}

	internal static void Clear()
	{
		lock (_globalLock)
		{
			_packagePairs = null;
		}
	}

	private static void ValidateUriKey(Uri uri)
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
