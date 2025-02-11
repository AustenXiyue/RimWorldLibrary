using System.Windows.Media.Composition;

namespace System.Windows.Media;

internal interface ICompositionTarget : IDisposable
{
	void Render(bool inResize, DUCE.Channel channel);

	void AddRefOnChannel(DUCE.Channel channel, DUCE.Channel outOfBandChannel);

	void ReleaseOnChannel(DUCE.Channel channel, DUCE.Channel outOfBandChannel);
}
