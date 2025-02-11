using System;

namespace MS.Internal.PtsHost;

internal sealed class DtrList
{
	private DirtyTextRange[] _dtrs;

	private const int _defaultCapacity = 4;

	private int _count;

	internal int Length => _count;

	internal DirtyTextRange this[int index] => _dtrs[index];

	internal DtrList()
	{
		_dtrs = new DirtyTextRange[4];
		_count = 0;
	}

	internal void Merge(DirtyTextRange dtr)
	{
		bool flag = false;
		int i = 0;
		int num = dtr.StartIndex;
		if (_count > 0)
		{
			for (; i < _count; i++)
			{
				if (num < _dtrs[i].StartIndex)
				{
					if (num + dtr.PositionsRemoved > _dtrs[i].StartIndex)
					{
						flag = true;
					}
					break;
				}
				if (num <= _dtrs[i].StartIndex + _dtrs[i].PositionsAdded)
				{
					flag = true;
					break;
				}
				num -= _dtrs[i].PositionsAdded - _dtrs[i].PositionsRemoved;
			}
			dtr.StartIndex = num;
		}
		if (i < _count)
		{
			if (flag)
			{
				if (dtr.StartIndex < _dtrs[i].StartIndex)
				{
					int num2 = _dtrs[i].StartIndex - dtr.StartIndex;
					int num3 = Math.Min(_dtrs[i].PositionsAdded, dtr.PositionsRemoved - num2);
					_dtrs[i].StartIndex = dtr.StartIndex;
					_dtrs[i].PositionsAdded += dtr.PositionsAdded - num3;
					_dtrs[i].PositionsRemoved += dtr.PositionsRemoved - num3;
				}
				else
				{
					int num4 = dtr.StartIndex - _dtrs[i].StartIndex;
					int num5 = Math.Min(dtr.PositionsRemoved, _dtrs[i].PositionsAdded - num4);
					_dtrs[i].PositionsAdded += dtr.PositionsAdded - num5;
					_dtrs[i].PositionsRemoved += dtr.PositionsRemoved - num5;
				}
				_dtrs[i].FromHighlightLayer &= dtr.FromHighlightLayer;
			}
			else
			{
				if (_count == _dtrs.Length)
				{
					Resize();
				}
				Array.Copy(_dtrs, i, _dtrs, i + 1, _count - i);
				_dtrs[i] = dtr;
				_count++;
			}
			MergeWithNext(i);
		}
		else
		{
			if (_count == _dtrs.Length)
			{
				Resize();
			}
			_dtrs[_count] = dtr;
			_count++;
		}
	}

	internal DirtyTextRange GetMergedRange()
	{
		if (_count > 0)
		{
			DirtyTextRange dirtyTextRange = _dtrs[0];
			int startIndex = dirtyTextRange.StartIndex;
			int positionsAdded = dirtyTextRange.PositionsAdded;
			int positionsRemoved = dirtyTextRange.PositionsRemoved;
			bool flag = dirtyTextRange.FromHighlightLayer;
			for (int i = 1; i < _count; i++)
			{
				dirtyTextRange = _dtrs[i];
				int num = dirtyTextRange.StartIndex - startIndex;
				positionsAdded = num + dirtyTextRange.PositionsAdded;
				positionsRemoved = num + dirtyTextRange.PositionsRemoved;
				flag &= dirtyTextRange.FromHighlightLayer;
				startIndex = dirtyTextRange.StartIndex;
			}
			return new DirtyTextRange(_dtrs[0].StartIndex, positionsAdded, positionsRemoved, flag);
		}
		return new DirtyTextRange(0, 0, 0);
	}

	internal DtrList DtrsFromRange(int dcpNew, int cchOld)
	{
		DtrList dtrList = null;
		int i = 0;
		int num;
		for (num = 0; i < _count && dcpNew > _dtrs[i].StartIndex + num + _dtrs[i].PositionsAdded; i++)
		{
			num += _dtrs[i].PositionsAdded - _dtrs[i].PositionsRemoved;
		}
		int j = i;
		for (; i < _count; i++)
		{
			if (dcpNew - num + cchOld <= _dtrs[i].StartIndex + _dtrs[i].PositionsRemoved)
			{
				if (dcpNew - num + cchOld < _dtrs[i].StartIndex)
				{
					i--;
				}
				break;
			}
		}
		int num2 = ((i < _count) ? i : (_count - 1));
		if (num2 >= j)
		{
			dtrList = new DtrList();
			for (; num2 >= j; j++)
			{
				DirtyTextRange dtr = _dtrs[j];
				dtr.StartIndex += num;
				dtrList.Append(dtr);
			}
		}
		return dtrList;
	}

	private void MergeWithNext(int index)
	{
		while (index + 1 < _count)
		{
			DirtyTextRange dirtyTextRange = _dtrs[index + 1];
			if (dirtyTextRange.StartIndex <= _dtrs[index].StartIndex + _dtrs[index].PositionsRemoved)
			{
				_dtrs[index].PositionsAdded += dirtyTextRange.PositionsAdded;
				_dtrs[index].PositionsRemoved += dirtyTextRange.PositionsRemoved;
				_dtrs[index].FromHighlightLayer &= dirtyTextRange.FromHighlightLayer;
				for (int i = index + 2; i < _count; i++)
				{
					_dtrs[i - 1] = _dtrs[i];
				}
				_count--;
				continue;
			}
			break;
		}
	}

	private void Append(DirtyTextRange dtr)
	{
		if (_count == _dtrs.Length)
		{
			Resize();
		}
		_dtrs[_count] = dtr;
		_count++;
	}

	private void Resize()
	{
		DirtyTextRange[] array = new DirtyTextRange[_dtrs.Length * 2];
		Array.Copy(_dtrs, array, _dtrs.Length);
		_dtrs = array;
	}
}
