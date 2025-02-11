using System;
using System.IO;
using System.IO.Packaging;
using System.Windows;
using System.Windows.Navigation;

namespace MS.Internal.AppModel;

internal class ContentFilePart : PackagePart
{
	private string _fullPath;

	internal ContentFilePart(Package container, Uri uri)
		: base(container, uri)
	{
		Invariant.Assert(Application.ResourceAssembly != null, "If the entry assembly is null no ContentFileParts should be created");
		_fullPath = null;
	}

	protected override Stream GetStreamCore(FileMode mode, FileAccess access)
	{
		if (_fullPath == null)
		{
			string entryAssemblyLocation = GetEntryAssemblyLocation();
			BaseUriHelper.GetAssemblyNameAndPart(base.Uri, out var partName, out var _, out var _, out var _);
			_fullPath = Path.Combine(Path.GetDirectoryName(entryAssemblyLocation), partName);
		}
		return CriticalOpenFile(_fullPath) ?? throw new IOException(SR.Format(SR.UnableToLocateResource, base.Uri.ToString()));
	}

	protected override string GetContentTypeCore()
	{
		return MimeTypeMapper.GetMimeTypeFromUri(new Uri(base.Uri.ToString(), UriKind.RelativeOrAbsolute)).ToString();
	}

	private string GetEntryAssemblyLocation()
	{
		string result = null;
		try
		{
			result = Application.ResourceAssembly.Location;
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
		}
		return result;
	}

	private Stream CriticalOpenFile(string filename)
	{
		return File.Open(filename, FileMode.Open, FileAccess.Read, ResourceContainer.FileShare);
	}
}
