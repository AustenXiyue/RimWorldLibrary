using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace MS.Internal.Documents;

internal sealed class TextContentRange
{
	private int _cpFirst;

	private int _cpLast;

	private int _size;

	private int[] _ranges;

	private ITextContainer _textContainer;

	internal ITextPointer StartPosition
	{
		get
		{
			ITextPointer result = null;
			if (_textContainer != null)
			{
				result = _textContainer.CreatePointerAtOffset(IsSimple ? _cpFirst : _ranges[0], LogicalDirection.Forward);
			}
			return result;
		}
	}

	internal ITextPointer EndPosition
	{
		get
		{
			ITextPointer result = null;
			if (_textContainer != null)
			{
				result = _textContainer.CreatePointerAtOffset(IsSimple ? _cpLast : _ranges[(_size - 1) * 2 + 1], LogicalDirection.Backward);
			}
			return result;
		}
	}

	private bool IsSimple => _size == 0;

	internal TextContentRange()
	{
	}

	internal TextContentRange(int cpFirst, int cpLast, ITextContainer textContainer)
	{
		Invariant.Assert(cpFirst <= cpLast);
		Invariant.Assert(cpFirst >= 0);
		Invariant.Assert(textContainer != null);
		Invariant.Assert(cpLast <= textContainer.SymbolCount);
		_cpFirst = cpFirst;
		_cpLast = cpLast;
		_size = 0;
		_ranges = null;
		_textContainer = textContainer;
	}

	internal void Merge(TextContentRange other)
	{
		Invariant.Assert(other != null);
		if (other._textContainer == null)
		{
			return;
		}
		if (_textContainer == null)
		{
			_cpFirst = other._cpFirst;
			_cpLast = other._cpLast;
			_textContainer = other._textContainer;
			_size = other._size;
			if (_size != 0)
			{
				Invariant.Assert(other._ranges != null);
				Invariant.Assert(other._ranges.Length >= other._size * 2);
				_ranges = new int[_size * 2];
				for (int i = 0; i < _ranges.Length; i++)
				{
					_ranges[i] = other._ranges[i];
				}
			}
		}
		else
		{
			Invariant.Assert(_textContainer == other._textContainer);
			if (other.IsSimple)
			{
				Merge(other._cpFirst, other._cpLast);
			}
			else
			{
				for (int j = 0; j < other._size; j++)
				{
					Merge(other._ranges[j * 2], other._ranges[j * 2 + 1]);
				}
			}
		}
		Normalize();
	}

	internal ReadOnlyCollection<TextSegment> GetTextSegments()
	{
		List<TextSegment> list;
		if (_textContainer == null)
		{
			list = new List<TextSegment>();
		}
		else if (IsSimple)
		{
			list = new List<TextSegment>(1);
			list.Add(new TextSegment(_textContainer.CreatePointerAtOffset(_cpFirst, LogicalDirection.Forward), _textContainer.CreatePointerAtOffset(_cpLast, LogicalDirection.Backward), preserveLogicalDirection: true));
		}
		else
		{
			list = new List<TextSegment>(_size);
			for (int i = 0; i < _size; i++)
			{
				list.Add(new TextSegment(_textContainer.CreatePointerAtOffset(_ranges[i * 2], LogicalDirection.Forward), _textContainer.CreatePointerAtOffset(_ranges[i * 2 + 1], LogicalDirection.Backward), preserveLogicalDirection: true));
			}
		}
		return new ReadOnlyCollection<TextSegment>(list);
	}

	internal bool Contains(ITextPointer position, bool strict)
	{
		bool result = false;
		int offset = position.Offset;
		if (IsSimple)
		{
			if (offset >= _cpFirst && offset <= _cpLast)
			{
				result = true;
				if (strict && _cpFirst != _cpLast && ((offset == _cpFirst && position.LogicalDirection == LogicalDirection.Backward) || (offset == _cpLast && position.LogicalDirection == LogicalDirection.Forward)))
				{
					result = false;
				}
			}
		}
		else
		{
			for (int i = 0; i < _size; i++)
			{
				if (offset >= _ranges[i * 2] && offset <= _ranges[i * 2 + 1])
				{
					result = true;
					if (strict && ((offset == _ranges[i * 2] && position.LogicalDirection == LogicalDirection.Backward) || (offset == _ranges[i * 2 + 1] && position.LogicalDirection == LogicalDirection.Forward)))
					{
						result = false;
					}
					break;
				}
			}
		}
		return result;
	}

	private void Merge(int cpFirst, int cpLast)
	{
		if (IsSimple)
		{
			if (cpFirst > _cpLast || cpLast < _cpFirst)
			{
				_size = 2;
				_ranges = new int[8];
				if (cpFirst > _cpLast)
				{
					_ranges[0] = _cpFirst;
					_ranges[1] = _cpLast;
					_ranges[2] = cpFirst;
					_ranges[3] = cpLast;
				}
				else
				{
					_ranges[0] = cpFirst;
					_ranges[1] = cpLast;
					_ranges[2] = _cpFirst;
					_ranges[3] = _cpLast;
				}
			}
			else
			{
				_cpFirst = Math.Min(_cpFirst, cpFirst);
				_cpLast = Math.Max(_cpLast, cpLast);
			}
			return;
		}
		int i;
		for (i = 0; i < _size; i++)
		{
			if (cpLast < _ranges[i * 2])
			{
				EnsureSize();
				for (int num = _size * 2 - 1; num >= i * 2; num--)
				{
					_ranges[num + 2] = _ranges[num];
				}
				_ranges[i * 2] = cpFirst;
				_ranges[i * 2 + 1] = cpLast;
				_size++;
				break;
			}
			if (cpFirst <= _ranges[i * 2 + 1])
			{
				_ranges[i * 2] = Math.Min(_ranges[i * 2], cpFirst);
				_ranges[i * 2 + 1] = Math.Max(_ranges[i * 2 + 1], cpLast);
				while (MergeWithNext(i))
				{
				}
				break;
			}
		}
		if (i >= _size)
		{
			EnsureSize();
			_ranges[_size * 2] = cpFirst;
			_ranges[_size * 2 + 1] = cpLast;
			_size++;
		}
	}

	private bool MergeWithNext(int pos)
	{
		if (pos < _size - 1 && _ranges[pos * 2 + 1] >= _ranges[(pos + 1) * 2])
		{
			_ranges[pos * 2 + 1] = Math.Max(_ranges[pos * 2 + 1], _ranges[(pos + 1) * 2 + 1]);
			for (int i = (pos + 1) * 2; i < (_size - 1) * 2; i++)
			{
				_ranges[i] = _ranges[i + 2];
			}
			_size--;
			return true;
		}
		return false;
	}

	private void EnsureSize()
	{
		Invariant.Assert(_size > 0);
		Invariant.Assert(_ranges != null);
		if (_ranges.Length < (_size + 1) * 2)
		{
			int[] array = new int[_ranges.Length * 2];
			for (int i = 0; i < _size * 2; i++)
			{
				array[i] = _ranges[i];
			}
			_ranges = array;
		}
	}

	private void Normalize()
	{
		if (_size == 1)
		{
			_cpFirst = _ranges[0];
			_cpLast = _ranges[1];
			_size = 0;
			_ranges = null;
		}
	}
}
