using System.Collections;
using System.Windows.Media;

namespace System.Windows.Documents;

internal class ColorTable : ArrayList
{
	private bool _inProgress;

	internal byte NewRed
	{
		set
		{
			ColorTableEntry inProgressEntry = GetInProgressEntry();
			if (inProgressEntry != null)
			{
				inProgressEntry.Red = value;
			}
		}
	}

	internal byte NewGreen
	{
		set
		{
			ColorTableEntry inProgressEntry = GetInProgressEntry();
			if (inProgressEntry != null)
			{
				inProgressEntry.Green = value;
			}
		}
	}

	internal byte NewBlue
	{
		set
		{
			ColorTableEntry inProgressEntry = GetInProgressEntry();
			if (inProgressEntry != null)
			{
				inProgressEntry.Blue = value;
			}
		}
	}

	internal ColorTable()
		: base(20)
	{
		_inProgress = false;
	}

	internal Color ColorAt(int index)
	{
		if (index >= 0 && index < Count)
		{
			return EntryAt(index).Color;
		}
		return Color.FromArgb(byte.MaxValue, 0, 0, 0);
	}

	internal void FinishColor()
	{
		if (_inProgress)
		{
			_inProgress = false;
			return;
		}
		int index = AddColor(Color.FromArgb(byte.MaxValue, 0, 0, 0));
		EntryAt(index).IsAuto = true;
	}

	internal int AddColor(Color color)
	{
		for (int i = 0; i < Count; i++)
		{
			if (ColorAt(i) == color)
			{
				return i;
			}
		}
		ColorTableEntry colorTableEntry = new ColorTableEntry();
		colorTableEntry.Color = color;
		Add(colorTableEntry);
		return Count - 1;
	}

	internal ColorTableEntry EntryAt(int index)
	{
		if (index >= 0 && index < Count)
		{
			return (ColorTableEntry)this[index];
		}
		return null;
	}

	private ColorTableEntry GetInProgressEntry()
	{
		if (_inProgress)
		{
			return EntryAt(Count - 1);
		}
		_inProgress = true;
		ColorTableEntry colorTableEntry = new ColorTableEntry();
		Add(colorTableEntry);
		return colorTableEntry;
	}
}
