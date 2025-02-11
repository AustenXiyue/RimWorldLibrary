using System.Collections;
using System.Collections.Generic;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Animation;

[FriendAccessAllowed]
internal struct TimelineTreeEnumerator
{
	private Timeline _rootTimeline;

	private SubtreeFlag _flags;

	private Stack _indexStack;

	private Stack<Timeline> _timelineStack;

	internal Timeline Current => _timelineStack.Peek();

	internal TimelineTreeEnumerator(Timeline root, bool processRoot)
	{
		_rootTimeline = root;
		_flags = ((!processRoot) ? SubtreeFlag.Reset : (SubtreeFlag.Reset | SubtreeFlag.ProcessRoot));
		_indexStack = new Stack(9);
		_timelineStack = new Stack<Timeline>(10);
	}

	internal void SkipSubtree()
	{
		_flags |= SubtreeFlag.SkipSubtree;
	}

	public bool MoveNext()
	{
		if ((_flags & SubtreeFlag.Reset) != 0)
		{
			_flags &= ~SubtreeFlag.Reset;
			_timelineStack.Push(_rootTimeline);
			if ((_flags & SubtreeFlag.ProcessRoot) == 0)
			{
				MoveNext();
			}
		}
		else if (_timelineStack.Count > 0)
		{
			TimelineGroup timelineGroup = _timelineStack.Peek() as TimelineGroup;
			TimelineCollection children;
			if ((_flags & SubtreeFlag.SkipSubtree) == 0 && timelineGroup != null && (children = timelineGroup.Children) != null && children.Count > 0)
			{
				_timelineStack.Push(children[0]);
				_indexStack.Push(0);
			}
			else
			{
				_flags &= ~SubtreeFlag.SkipSubtree;
				_timelineStack.Pop();
				while (_timelineStack.Count > 0)
				{
					timelineGroup = _timelineStack.Peek() as TimelineGroup;
					children = timelineGroup.Children;
					int num = (int)_indexStack.Pop() + 1;
					if (num < children.Count)
					{
						_timelineStack.Push(children[num]);
						_indexStack.Push(num);
						break;
					}
					_timelineStack.Pop();
				}
			}
		}
		return _timelineStack.Count > 0;
	}
}
