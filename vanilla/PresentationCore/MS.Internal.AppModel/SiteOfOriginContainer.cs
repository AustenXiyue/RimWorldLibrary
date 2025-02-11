using System;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Windows.Navigation;
using MS.Internal.PresentationCore;

namespace MS.Internal.AppModel;

internal class SiteOfOriginContainer : Package
{
	internal static BooleanSwitch _traceSwitch = new BooleanSwitch("SiteOfOrigin", "SiteOfOriginContainer and SiteOfOriginPart trace messages");

	private static MS.Internal.SecurityCriticalDataForSet<Uri> _browserSource;

	private static MS.Internal.SecurityCriticalDataForSet<Uri>? _siteOfOriginForClickOnceApp;

	internal static Uri SiteOfOrigin
	{
		[FriendAccessAllowed]
		get
		{
			Uri uri = SiteOfOriginForClickOnceApp;
			if (uri == null)
			{
				uri = BaseUriHelper.FixFileUri(new Uri(AppDomain.CurrentDomain.BaseDirectory));
			}
			return uri;
		}
	}

	internal static Uri SiteOfOriginForClickOnceApp
	{
		get
		{
			if (!_siteOfOriginForClickOnceApp.HasValue)
			{
				_siteOfOriginForClickOnceApp = new MS.Internal.SecurityCriticalDataForSet<Uri>(null);
			}
			Invariant.Assert(_siteOfOriginForClickOnceApp.HasValue);
			return _siteOfOriginForClickOnceApp.Value.Value;
		}
	}

	internal static Uri BrowserSource
	{
		get
		{
			return _browserSource.Value;
		}
		set
		{
			_browserSource.Value = value;
		}
	}

	internal static bool TraceSwitchEnabled
	{
		get
		{
			return _traceSwitch.Enabled;
		}
		set
		{
			_traceSwitch.Enabled = value;
		}
	}

	internal SiteOfOriginContainer()
		: base(FileAccess.Read)
	{
	}

	public override bool PartExists(Uri uri)
	{
		return true;
	}

	protected override PackagePart GetPartCore(Uri uri)
	{
		return new SiteOfOriginPart(this, uri);
	}

	protected override PackagePart CreatePartCore(Uri uri, string contentType, CompressionOption compressionOption)
	{
		return null;
	}

	protected override void DeletePartCore(Uri uri)
	{
		throw new NotSupportedException();
	}

	protected override PackagePart[] GetPartsCore()
	{
		throw new NotSupportedException();
	}

	protected override void FlushCore()
	{
		throw new NotSupportedException();
	}
}
