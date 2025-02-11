using System;

namespace MS.Internal.AppModel;

[Flags]
internal enum HostingFlags
{
	hfHostedInIE = 1,
	hfHostedInWebOC = 2,
	hfHostedInIEorWebOC = 3,
	hfHostedInMozilla = 4,
	hfHostedInFrame = 8,
	hfIsBrowserLowIntegrityProcess = 0x10,
	hfInDebugMode = 0x20
}
