using System.Runtime.InteropServices;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct UriHolder
{
	internal Uri BaseUri;

	internal Uri OriginalUri;
}
