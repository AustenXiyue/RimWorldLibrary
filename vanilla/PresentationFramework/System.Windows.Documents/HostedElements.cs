using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Windows.Documents;

internal class HostedElements : IEnumerator<IInputElement>, IEnumerator, IDisposable
{
	private ReadOnlyCollection<TextSegment> _textSegments;

	private TextPointer _currentPosition;

	private int _currentTextSegment;

	object IEnumerator.Current => Current;

	public IInputElement Current
	{
		get
		{
			if (_textSegments == null)
			{
				throw new InvalidOperationException(SR.EnumeratorCollectionDisposed);
			}
			if (_currentPosition == null)
			{
				throw new InvalidOperationException(SR.EnumeratorNotStarted);
			}
			IInputElement result = null;
			switch (_currentPosition.GetPointerContext(LogicalDirection.Forward))
			{
			case TextPointerContext.ElementStart:
				result = _currentPosition.GetAdjacentElementFromOuterPosition(LogicalDirection.Forward);
				break;
			case TextPointerContext.EmbeddedElement:
				result = (IInputElement)_currentPosition.GetAdjacentElement(LogicalDirection.Forward);
				break;
			}
			return result;
		}
	}

	internal HostedElements(ReadOnlyCollection<TextSegment> textSegments)
	{
		_textSegments = textSegments;
		_currentPosition = null;
		_currentTextSegment = 0;
	}

	void IDisposable.Dispose()
	{
		_textSegments = null;
		GC.SuppressFinalize(this);
	}

	public bool MoveNext()
	{
		if (_textSegments == null)
		{
			throw new ObjectDisposedException("HostedElements");
		}
		if (_textSegments.Count == 0)
		{
			return false;
		}
		if (_currentPosition == null)
		{
			if (!(_textSegments[0].Start is TextPointer))
			{
				_currentPosition = null;
				return false;
			}
			_currentPosition = new TextPointer(_textSegments[0].Start as TextPointer);
		}
		else if (_currentTextSegment < _textSegments.Count)
		{
			_currentPosition.MoveToNextContextPosition(LogicalDirection.Forward);
		}
		while (_currentTextSegment < _textSegments.Count)
		{
			while (((ITextPointer)_currentPosition).CompareTo(_textSegments[_currentTextSegment].End) < 0)
			{
				if (_currentPosition.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementStart || _currentPosition.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.EmbeddedElement)
				{
					return true;
				}
				_currentPosition.MoveToNextContextPosition(LogicalDirection.Forward);
			}
			_currentTextSegment++;
			if (_currentTextSegment < _textSegments.Count)
			{
				if (!(_textSegments[_currentTextSegment].Start is TextPointer))
				{
					_currentPosition = null;
					return false;
				}
				_currentPosition = new TextPointer(_textSegments[_currentTextSegment].Start as TextPointer);
			}
		}
		return false;
	}

	void IEnumerator.Reset()
	{
		throw new NotImplementedException();
	}
}
