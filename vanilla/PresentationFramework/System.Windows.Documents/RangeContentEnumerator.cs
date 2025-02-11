using System.Collections;
using MS.Internal;

namespace System.Windows.Documents;

internal class RangeContentEnumerator : IEnumerator
{
	private readonly TextPointer _start;

	private readonly TextPointer _end;

	private readonly uint _generation;

	private TextPointer _navigator;

	private object _currentCache;

	private char[] _buffer;

	public object Current
	{
		get
		{
			if (_navigator == null)
			{
				throw new InvalidOperationException(SR.EnumeratorNotStarted);
			}
			if (_currentCache != null)
			{
				return _currentCache;
			}
			if (_navigator.CompareTo(_end) >= 0)
			{
				throw new InvalidOperationException(SR.EnumeratorReachedEnd);
			}
			if (_generation != _start.TextContainer.Generation && !IsLogicalChildrenIterationInProgress)
			{
				throw new InvalidOperationException(SR.EnumeratorVersionChanged);
			}
			switch (_navigator.GetPointerContext(LogicalDirection.Forward))
			{
			case TextPointerContext.Text:
			{
				int num = 0;
				do
				{
					int textRunLength = _navigator.GetTextRunLength(LogicalDirection.Forward);
					EnsureBufferCapacity(num + textRunLength);
					_navigator.GetTextInRun(LogicalDirection.Forward, _buffer, num, textRunLength);
					num += textRunLength;
					_navigator.MoveToNextContextPosition(LogicalDirection.Forward);
				}
				while (_navigator.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text);
				_currentCache = new string(_buffer, 0, num);
				break;
			}
			case TextPointerContext.EmbeddedElement:
				_currentCache = _navigator.GetAdjacentElement(LogicalDirection.Forward);
				_navigator.MoveToNextContextPosition(LogicalDirection.Forward);
				break;
			case TextPointerContext.ElementStart:
				_navigator.MoveToNextContextPosition(LogicalDirection.Forward);
				_currentCache = _navigator.Parent;
				_navigator.MoveToElementEdge(ElementEdge.AfterEnd);
				break;
			default:
				Invariant.Assert(condition: false, "Unexpected run type!");
				_currentCache = null;
				break;
			}
			return _currentCache;
		}
	}

	private bool IsLogicalChildrenIterationInProgress
	{
		get
		{
			for (DependencyObject parent = _start.Parent; parent != null; parent = LogicalTreeHelper.GetParent(parent))
			{
				if (parent is FrameworkElement frameworkElement)
				{
					if (frameworkElement.IsLogicalChildrenIterationInProgress)
					{
						return true;
					}
				}
				else if (parent is FrameworkContentElement { IsLogicalChildrenIterationInProgress: not false })
				{
					return true;
				}
			}
			return false;
		}
	}

	internal RangeContentEnumerator(TextPointer start, TextPointer end)
	{
		Invariant.Assert((start != null && end != null) || (start == null && end == null), "If start is null end should be null!");
		_start = start;
		_end = end;
		if (_start != null)
		{
			_generation = _start.TextContainer.Generation;
		}
	}

	public bool MoveNext()
	{
		if (_start != null && _generation != _start.TextContainer.Generation && !IsLogicalChildrenIterationInProgress)
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
		}
		else if (_currentCache == null)
		{
			switch (_navigator.GetPointerContext(LogicalDirection.Forward))
			{
			case TextPointerContext.Text:
				do
				{
					_navigator.MoveToNextContextPosition(LogicalDirection.Forward);
				}
				while (_navigator.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text && _navigator.CompareTo(_end) < 0);
				break;
			case TextPointerContext.EmbeddedElement:
				_navigator.MoveToNextContextPosition(LogicalDirection.Forward);
				break;
			case TextPointerContext.ElementStart:
				_navigator.MoveToNextContextPosition(LogicalDirection.Forward);
				_navigator.MoveToPosition(((TextElement)_navigator.Parent).ElementEnd);
				break;
			default:
				Invariant.Assert(condition: false, "Unexpected run type!");
				break;
			}
		}
		_currentCache = null;
		return _navigator.CompareTo(_end) < 0;
	}

	public void Reset()
	{
		if (_start != null && _generation != _start.TextContainer.Generation)
		{
			throw new InvalidOperationException(SR.EnumeratorVersionChanged);
		}
		_navigator = null;
	}

	private void EnsureBufferCapacity(int size)
	{
		if (_buffer == null)
		{
			_buffer = new char[size];
		}
		else if (_buffer.Length < size)
		{
			char[] array = new char[Math.Max(2 * _buffer.Length, size)];
			_buffer.CopyTo(array, 0);
			_buffer = array;
		}
	}
}
