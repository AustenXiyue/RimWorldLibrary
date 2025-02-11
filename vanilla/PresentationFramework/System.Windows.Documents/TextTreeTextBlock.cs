using MS.Internal;

namespace System.Windows.Documents;

internal class TextTreeTextBlock : SplayTreeNode
{
	private int _leftSymbolCount;

	private SplayTreeNode _parentNode;

	private TextTreeTextBlock _leftChildNode;

	private TextTreeTextBlock _rightChildNode;

	private char[] _text;

	private int _gapOffset;

	private int _gapSize;

	internal const int MaxBlockSize = 4096;

	internal override SplayTreeNode ParentNode
	{
		get
		{
			return _parentNode;
		}
		set
		{
			_parentNode = value;
		}
	}

	internal override SplayTreeNode ContainedNode
	{
		get
		{
			return null;
		}
		set
		{
			Invariant.Assert(condition: false, "Can't set ContainedNode on a TextTreeTextBlock!");
		}
	}

	internal override int LeftSymbolCount
	{
		get
		{
			return _leftSymbolCount;
		}
		set
		{
			_leftSymbolCount = value;
		}
	}

	internal override int LeftCharCount
	{
		get
		{
			return 0;
		}
		set
		{
			Invariant.Assert(value == 0);
		}
	}

	internal override SplayTreeNode LeftChildNode
	{
		get
		{
			return _leftChildNode;
		}
		set
		{
			_leftChildNode = (TextTreeTextBlock)value;
		}
	}

	internal override SplayTreeNode RightChildNode
	{
		get
		{
			return _rightChildNode;
		}
		set
		{
			_rightChildNode = (TextTreeTextBlock)value;
		}
	}

	internal override uint Generation
	{
		get
		{
			return 0u;
		}
		set
		{
			Invariant.Assert(condition: false, "TextTreeTextBlock does not track Generation!");
		}
	}

	internal override int SymbolOffsetCache
	{
		get
		{
			return -1;
		}
		set
		{
			Invariant.Assert(condition: false, "TextTreeTextBlock does not track SymbolOffsetCache!");
		}
	}

	internal override int SymbolCount
	{
		get
		{
			return Count;
		}
		set
		{
			Invariant.Assert(condition: false, "Can't set SymbolCount on TextTreeTextBlock!");
		}
	}

	internal override int IMECharCount
	{
		get
		{
			return 0;
		}
		set
		{
			Invariant.Assert(value == 0);
		}
	}

	internal int Count => _text.Length - _gapSize;

	internal int FreeCapacity => _gapSize;

	internal int GapOffset => _gapOffset;

	internal TextTreeTextBlock(int size)
	{
		Invariant.Assert(size > 0);
		Invariant.Assert(size <= 4096);
		_text = new char[size];
		_gapSize = size;
	}

	internal int InsertText(int logicalOffset, object text, int textStartIndex, int textEndIndex)
	{
		Invariant.Assert(text is string || text is char[], "Bad text parameter!");
		Invariant.Assert(textStartIndex <= textEndIndex, "Bad start/end index!");
		Splay();
		int num = textEndIndex - textStartIndex;
		if (_text.Length < 4096 && num > _gapSize)
		{
			char[] array = new char[Math.Min(Count + num, 4096)];
			Array.Copy(_text, 0, array, 0, _gapOffset);
			int num2 = _text.Length - (_gapOffset + _gapSize);
			Array.Copy(_text, _gapOffset + _gapSize, array, array.Length - num2, num2);
			_gapSize += array.Length - _text.Length;
			_text = array;
		}
		if (logicalOffset != _gapOffset)
		{
			MoveGap(logicalOffset);
		}
		num = Math.Min(num, _gapSize);
		if (text is string text2)
		{
			text2.CopyTo(textStartIndex, _text, logicalOffset, num);
		}
		else
		{
			Array.Copy((char[])text, textStartIndex, _text, logicalOffset, num);
		}
		_gapOffset += num;
		_gapSize -= num;
		return num;
	}

	internal TextTreeTextBlock SplitBlock()
	{
		Invariant.Assert(_gapSize == 0, "Splitting non-full block!");
		Invariant.Assert(_text.Length == 4096, "Splitting non-max sized block!");
		TextTreeTextBlock textTreeTextBlock = new TextTreeTextBlock(4096);
		bool insertBefore;
		if (_gapOffset < 2048)
		{
			Array.Copy(_text, 0, textTreeTextBlock._text, 0, _gapOffset);
			textTreeTextBlock._gapOffset = _gapOffset;
			textTreeTextBlock._gapSize = 4096 - _gapOffset;
			_gapSize += _gapOffset;
			_gapOffset = 0;
			insertBefore = true;
		}
		else
		{
			Array.Copy(_text, _gapOffset, textTreeTextBlock._text, _gapOffset, 4096 - _gapOffset);
			Invariant.Assert(textTreeTextBlock._gapOffset == 0);
			textTreeTextBlock._gapSize = _gapOffset;
			_gapSize = 4096 - _gapOffset;
			insertBefore = false;
		}
		textTreeTextBlock.InsertAtNode(this, insertBefore);
		return textTreeTextBlock;
	}

	internal void RemoveText(int logicalOffset, int count)
	{
		Invariant.Assert(logicalOffset >= 0);
		Invariant.Assert(count >= 0);
		Invariant.Assert(logicalOffset + count <= Count, "Removing too much text!");
		int num = count;
		int count2 = Count;
		Splay();
		if (logicalOffset < _gapOffset)
		{
			if (logicalOffset + count < _gapOffset)
			{
				MoveGap(logicalOffset + count);
			}
			int num2 = ((logicalOffset + count == _gapOffset) ? count : (_gapOffset - logicalOffset));
			_gapOffset -= num2;
			_gapSize += num2;
			logicalOffset = _gapOffset;
			count -= num2;
		}
		logicalOffset += _gapSize;
		if (logicalOffset > _gapOffset + _gapSize)
		{
			MoveGap(logicalOffset - _gapSize);
		}
		_gapSize += count;
		Invariant.Assert(_gapOffset + _gapSize <= _text.Length);
		Invariant.Assert(count2 == Count + num);
	}

	internal int ReadText(int logicalOffset, int count, char[] chars, int charsStartIndex)
	{
		int num = count;
		if (logicalOffset < _gapOffset)
		{
			int num2 = Math.Min(count, _gapOffset - logicalOffset);
			Array.Copy(_text, logicalOffset, chars, charsStartIndex, num2);
			count -= num2;
			charsStartIndex += num2;
			logicalOffset = _gapOffset;
		}
		if (count > 0)
		{
			logicalOffset += _gapSize;
			int num2 = Math.Min(count, _text.Length - logicalOffset);
			Array.Copy(_text, logicalOffset, chars, charsStartIndex, num2);
			count -= num2;
		}
		return num - count;
	}

	private void MoveGap(int offset)
	{
		int sourceIndex;
		int destinationIndex;
		int length;
		if (offset < _gapOffset)
		{
			sourceIndex = offset;
			destinationIndex = offset + _gapSize;
			length = _gapOffset - offset;
		}
		else
		{
			sourceIndex = _gapOffset + _gapSize;
			destinationIndex = _gapOffset;
			length = offset - _gapOffset;
		}
		Array.Copy(_text, sourceIndex, _text, destinationIndex, length);
		_gapOffset = offset;
	}
}
