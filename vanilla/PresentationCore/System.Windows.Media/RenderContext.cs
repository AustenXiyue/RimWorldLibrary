using System.Windows.Media.Composition;

namespace System.Windows.Media;

internal sealed class RenderContext
{
	private DUCE.Channel _channel;

	private DUCE.ResourceHandle _root;

	internal DUCE.Channel Channel => _channel;

	internal DUCE.ResourceHandle Root => _root;

	internal RenderContext()
	{
	}

	internal void Initialize(DUCE.Channel channel, DUCE.ResourceHandle root)
	{
		_channel = channel;
		_root = root;
	}
}
