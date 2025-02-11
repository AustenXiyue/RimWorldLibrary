using System;

namespace Standard;

internal struct RECT
{
	private int _left;

	private int _top;

	private int _right;

	private int _bottom;

	public int Left
	{
		get
		{
			return _left;
		}
		set
		{
			_left = value;
		}
	}

	public int Right
	{
		get
		{
			return _right;
		}
		set
		{
			_right = value;
		}
	}

	public int Top
	{
		get
		{
			return _top;
		}
		set
		{
			_top = value;
		}
	}

	public int Bottom
	{
		get
		{
			return _bottom;
		}
		set
		{
			_bottom = value;
		}
	}

	public int Width => _right - _left;

	public int Height => _bottom - _top;

	public POINT Position
	{
		get
		{
			POINT result = default(POINT);
			result.x = _left;
			result.y = _top;
			return result;
		}
	}

	public SIZE Size
	{
		get
		{
			SIZE result = default(SIZE);
			result.cx = Width;
			result.cy = Height;
			return result;
		}
	}

	public void Offset(int dx, int dy)
	{
		_left += dx;
		_top += dy;
		_right += dx;
		_bottom += dy;
	}

	public static RECT Union(RECT rect1, RECT rect2)
	{
		RECT result = default(RECT);
		result.Left = Math.Min(rect1.Left, rect2.Left);
		result.Top = Math.Min(rect1.Top, rect2.Top);
		result.Right = Math.Max(rect1.Right, rect2.Right);
		result.Bottom = Math.Max(rect1.Bottom, rect2.Bottom);
		return result;
	}

	public override bool Equals(object obj)
	{
		try
		{
			RECT rECT = (RECT)obj;
			return rECT._bottom == _bottom && rECT._left == _left && rECT._right == _right && rECT._top == _top;
		}
		catch (InvalidCastException)
		{
			return false;
		}
	}

	public override int GetHashCode()
	{
		return ((_left << 16) | Utility.LOWORD(_right)) ^ ((_top << 16) | Utility.LOWORD(_bottom));
	}
}
