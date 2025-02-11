using System;
using System.IO;
using System.IO.Packaging;
using System.Resources;
using System.Windows;
using MS.Internal.Resources;

namespace MS.Internal.AppModel;

internal class ResourcePart : PackagePart
{
	private MS.Internal.SecurityCriticalDataForSet<ResourceManagerWrapper> _rmWrapper;

	private bool _ensureResourceIsCalled;

	private string _name;

	private readonly object _globalLock = new object();

	public ResourcePart(Package container, Uri uri, string name, ResourceManagerWrapper rmWrapper)
		: base(container, uri)
	{
		if (rmWrapper == null)
		{
			throw new ArgumentNullException("rmWrapper");
		}
		_rmWrapper.Value = rmWrapper;
		_name = name;
	}

	protected override Stream GetStreamCore(FileMode mode, FileAccess access)
	{
		Stream stream = null;
		stream = EnsureResourceLocationSet();
		if (stream == null)
		{
			stream = _rmWrapper.Value.GetStream(_name);
			if (stream == null)
			{
				throw new IOException(SR.Format(SR.UnableToLocateResource, _name));
			}
		}
		ContentType contentType = new ContentType(base.ContentType);
		if (MimeTypeMapper.BamlMime.AreTypeAndSubTypeEqual(contentType))
		{
			stream = new BamlStream(stream, _rmWrapper.Value.Assembly);
		}
		return stream;
	}

	protected override string GetContentTypeCore()
	{
		EnsureResourceLocationSet();
		return MimeTypeMapper.GetMimeTypeFromUri(new Uri(_name, UriKind.RelativeOrAbsolute)).ToString();
	}

	private Stream EnsureResourceLocationSet()
	{
		Stream stream = null;
		lock (_globalLock)
		{
			if (_ensureResourceIsCalled)
			{
				return null;
			}
			_ensureResourceIsCalled = true;
			try
			{
				if (string.Compare(Path.GetExtension(_name), ".baml", StringComparison.OrdinalIgnoreCase) == 0)
				{
					throw new IOException(SR.Format(SR.UnableToLocateResource, _name));
				}
				if (string.Compare(Path.GetExtension(_name), ".xaml", StringComparison.OrdinalIgnoreCase) == 0)
				{
					string name = Path.ChangeExtension(_name, ".baml");
					stream = _rmWrapper.Value.GetStream(name);
					if (stream != null)
					{
						_name = name;
						return stream;
					}
				}
			}
			catch (MissingManifestResourceException)
			{
			}
		}
		return null;
	}
}
