using System.Windows.Input;
using MS.Internal.PresentationCore;

namespace System.Windows.Interop;

internal class HwndSourceKeyboardInputSite : IKeyboardInputSite
{
	private HwndSource _source;

	private IKeyboardInputSink _sink;

	private UIElement _sinkElement;

	IKeyboardInputSink IKeyboardInputSite.Sink => _sink;

	public HwndSourceKeyboardInputSite(HwndSource source, IKeyboardInputSink sink)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (sink == null)
		{
			throw new ArgumentNullException("sink");
		}
		if (!(sink is UIElement))
		{
			throw new ArgumentException(SR.KeyboardSinkMustBeAnElement, "sink");
		}
		_source = source;
		_sink = sink;
		_sink.KeyboardInputSite = this;
		_sinkElement = sink as UIElement;
	}

	void IKeyboardInputSite.Unregister()
	{
		CriticalUnregister();
	}

	internal void CriticalUnregister()
	{
		if (_source != null && _sink != null)
		{
			_source.CriticalUnregisterKeyboardInputSink(this);
			_sink.KeyboardInputSite = null;
		}
		_source = null;
		_sink = null;
	}

	bool IKeyboardInputSite.OnNoMoreTabStops(TraversalRequest request)
	{
		bool result = false;
		if (_sinkElement != null)
		{
			result = _sinkElement.MoveFocus(request);
		}
		return result;
	}
}
