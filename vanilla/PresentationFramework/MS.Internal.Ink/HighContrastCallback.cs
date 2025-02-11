using System.Windows.Media;
using System.Windows.Threading;

namespace MS.Internal.Ink;

internal abstract class HighContrastCallback
{
	internal abstract Dispatcher Dispatcher { get; }

	internal abstract void TurnHighContrastOn(Color highContrastColor);

	internal abstract void TurnHighContrastOff();
}
