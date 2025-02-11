using System.Collections;
using System.Collections.Generic;
using MS.Internal;

namespace System.Windows.Documents;

internal class TextElementEnumerator<TextElementType> : IEnumerator<TextElementType>, IEnumerator, IDisposable where TextElementType : TextElement
{
	private readonly TextPointer _start;

	private readonly TextPointer _end;

	private readonly uint _generation;

	private TextPointer _navigator;

	private TextElementType _current;

	object IEnumerator.Current => Current;

	public TextElementType Current
	{
		get
		{
			if (_navigator == null)
			{
				throw new InvalidOperationException(SR.EnumeratorNotStarted);
			}
			if (_current == null)
			{
				throw new InvalidOperationException(SR.EnumeratorReachedEnd);
			}
			return _current;
		}
	}

	internal TextElementEnumerator(TextPointer start, TextPointer end)
	{
		Invariant.Assert((start != null && end != null) || (start == null && end == null), "If start is null end should be null!");
		_start = start;
		_end = end;
		if (_start != null)
		{
			_generation = _start.TextContainer.Generation;
		}
	}

	public void Dispose()
	{
		_current = null;
		_navigator = null;
		GC.SuppressFinalize(this);
	}

	public bool MoveNext()
	{
		if (_start != null && _generation != _start.TextContainer.Generation)
		{
			throw new InvalidOperationException(SR.EnumeratorVersionChanged);
		}
		if (_start == null || _start.CompareTo(_end) == 0)
		{
			return false;
		}
		if (_navigator != null && _navigator.CompareTo(_end) >= 0)
		{
			return false;
		}
		if (_navigator == null)
		{
			_navigator = new TextPointer(_start);
			_navigator.MoveToNextContextPosition(LogicalDirection.Forward);
		}
		else
		{
			Invariant.Assert(_navigator.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart, "Unexpected run type in TextElementEnumerator");
			_navigator.MoveToElementEdge(ElementEdge.AfterEnd);
			_navigator.MoveToNextContextPosition(LogicalDirection.Forward);
		}
		if (_navigator.CompareTo(_end) < 0)
		{
			_current = (TextElementType)_navigator.Parent;
		}
		else
		{
			_current = null;
		}
		return _current != null;
	}

	public void Reset()
	{
		if (_start != null && _generation != _start.TextContainer.Generation)
		{
			throw new InvalidOperationException(SR.EnumeratorVersionChanged);
		}
		_navigator = null;
		_current = null;
	}
}
