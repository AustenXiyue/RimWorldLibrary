using System.Windows.Media.Composition;

namespace System.Windows.Media;

internal interface ICyclicBrush
{
	void FireOnChanged();

	void RenderForCyclicBrush(DUCE.Channel channel, bool skipChannelCheck);
}
