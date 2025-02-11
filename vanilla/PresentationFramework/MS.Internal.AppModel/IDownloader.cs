using System;
using System.Windows.Navigation;

namespace MS.Internal.AppModel;

internal interface IDownloader
{
	NavigationService Downloader { get; }

	event EventHandler ContentRendered;
}
