using System;
using System.Windows;
using MS.Internal.PtsHost.UnsafeNativeMethods;

namespace MS.Internal.PtsTable;

internal struct CalculatedColumn
{
	[Flags]
	private enum Flags
	{
		ValidWidth = 1,
		ValidAutofit = 2
	}

	private GridLength _userWidth;

	private double _durWidth;

	private double _durMinWidth;

	private double _durMaxWidth;

	private double _urOffset;

	private Flags _flags;

	internal int PtsWidthChanged => PTS.FromBoolean(!CheckFlags(Flags.ValidWidth));

	internal double DurMinWidth => _durMinWidth;

	internal double DurMaxWidth => _durMaxWidth;

	internal GridLength UserWidth
	{
		get
		{
			return _userWidth;
		}
		set
		{
			if (_userWidth != value)
			{
				SetFlags(value: false, Flags.ValidAutofit);
			}
			_userWidth = value;
		}
	}

	internal double DurWidth
	{
		get
		{
			return _durWidth;
		}
		set
		{
			if (!DoubleUtil.AreClose(_durWidth, value))
			{
				SetFlags(value: false, Flags.ValidWidth);
			}
			_durWidth = value;
		}
	}

	internal double UrOffset
	{
		get
		{
			return _urOffset;
		}
		set
		{
			_urOffset = value;
		}
	}

	internal void ValidateAuto(double durMinWidth, double durMaxWidth)
	{
		_durMinWidth = durMinWidth;
		_durMaxWidth = durMaxWidth;
		SetFlags(value: true, Flags.ValidAutofit);
	}

	private void SetFlags(bool value, Flags flags)
	{
		_flags = (value ? (_flags | flags) : (_flags & ~flags));
	}

	private bool CheckFlags(Flags flags)
	{
		return (_flags & flags) == flags;
	}
}
