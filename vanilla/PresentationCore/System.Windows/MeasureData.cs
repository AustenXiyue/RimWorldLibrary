using MS.Internal;

namespace System.Windows;

internal class MeasureData
{
	private Size _availableSize;

	private Rect _viewport;

	public bool HasViewport => Viewport != Rect.Empty;

	public Size AvailableSize
	{
		get
		{
			return _availableSize;
		}
		set
		{
			_availableSize = value;
		}
	}

	public Rect Viewport
	{
		get
		{
			return _viewport;
		}
		set
		{
			_viewport = value;
		}
	}

	public MeasureData(Size availableSize, Rect viewport)
	{
		_availableSize = availableSize;
		_viewport = viewport;
	}

	public MeasureData(MeasureData data)
		: this(data.AvailableSize, data.Viewport)
	{
	}

	public bool IsCloseTo(MeasureData other)
	{
		if (other == null)
		{
			return false;
		}
		return DoubleUtil.AreClose(AvailableSize, other.AvailableSize) & DoubleUtil.AreClose(Viewport, other.Viewport);
	}
}
