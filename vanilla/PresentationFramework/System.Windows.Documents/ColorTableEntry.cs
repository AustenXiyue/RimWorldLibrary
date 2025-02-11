using System.Windows.Media;

namespace System.Windows.Documents;

internal class ColorTableEntry
{
	private Color _color;

	private bool _bAuto;

	internal Color Color
	{
		get
		{
			return _color;
		}
		set
		{
			_color = value;
		}
	}

	internal bool IsAuto
	{
		get
		{
			return _bAuto;
		}
		set
		{
			_bAuto = value;
		}
	}

	internal byte Red
	{
		set
		{
			_color = Color.FromArgb(byte.MaxValue, value, _color.G, _color.B);
		}
	}

	internal byte Green
	{
		set
		{
			_color = Color.FromArgb(byte.MaxValue, _color.R, value, _color.B);
		}
	}

	internal byte Blue
	{
		set
		{
			_color = Color.FromArgb(byte.MaxValue, _color.R, _color.G, value);
		}
	}

	internal ColorTableEntry()
	{
		_color = Color.FromArgb(byte.MaxValue, 0, 0, 0);
		_bAuto = false;
	}
}
