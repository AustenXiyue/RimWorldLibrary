using System;

namespace MS.Internal.Xaml.Context;

internal abstract class XamlFrame
{
	private int _depth;

	private MS.Internal.Xaml.Context.XamlFrame _previous;

	public int Depth => _depth;

	public MS.Internal.Xaml.Context.XamlFrame Previous
	{
		get
		{
			return _previous;
		}
		set
		{
			_previous = value;
			_depth = ((_previous != null) ? (_previous._depth + 1) : 0);
		}
	}

	protected XamlFrame()
	{
		_depth = -1;
	}

	protected XamlFrame(MS.Internal.Xaml.Context.XamlFrame source)
	{
		_depth = source._depth;
	}

	public virtual MS.Internal.Xaml.Context.XamlFrame Clone()
	{
		throw new NotImplementedException();
	}

	public abstract void Reset();
}
