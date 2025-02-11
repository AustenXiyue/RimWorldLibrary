using System.Collections;
using MS.Internal;

namespace System.Windows.Documents;

internal class SpellerStatusTable
{
	internal enum RunType
	{
		Clean,
		Dirty,
		Error
	}

	private class Run
	{
		private ITextPointer _position;

		private RunType _runType;

		internal ITextPointer Position
		{
			get
			{
				return _position;
			}
			set
			{
				_position = value;
			}
		}

		internal RunType RunType
		{
			get
			{
				return _runType;
			}
			set
			{
				_runType = value;
			}
		}

		internal Run(ITextPointer position, RunType runType)
		{
			_position = position.GetFrozenPointer(LogicalDirection.Backward);
			_runType = runType;
		}
	}

	private readonly SpellerHighlightLayer _highlightLayer;

	private readonly ArrayList _runList;

	internal SpellerStatusTable(ITextPointer textContainerStart, SpellerHighlightLayer highlightLayer)
	{
		_highlightLayer = highlightLayer;
		_runList = new ArrayList(1);
		_runList.Add(new Run(textContainerStart, RunType.Dirty));
	}

	internal void OnTextChange(TextContainerChangeEventArgs e)
	{
		if (e.TextChange == TextChangeType.ContentAdded)
		{
			OnContentAdded(e);
		}
		else if (e.TextChange == TextChangeType.ContentRemoved)
		{
			OnContentRemoved(e.ITextPosition);
		}
		else
		{
			ITextPointer textPointer = e.ITextPosition.CreatePointer(e.Count);
			textPointer.Freeze();
			MarkDirtyRange(e.ITextPosition, textPointer);
		}
		DebugAssertRunList();
	}

	internal void GetFirstDirtyRange(ITextPointer searchStart, out ITextPointer start, out ITextPointer end)
	{
		start = null;
		end = null;
		for (int i = FindIndex(searchStart.CreateStaticPointer(), LogicalDirection.Forward); i >= 0 && i < _runList.Count; i++)
		{
			Run run = GetRun(i);
			if (run.RunType == RunType.Dirty)
			{
				start = TextPointerBase.Max(searchStart, run.Position);
				end = GetRunEndPositionDynamic(i);
				break;
			}
		}
	}

	internal void MarkCleanRange(ITextPointer start, ITextPointer end)
	{
		MarkRange(start, end, RunType.Clean);
		DebugAssertRunList();
	}

	internal void MarkDirtyRange(ITextPointer start, ITextPointer end)
	{
		MarkRange(start, end, RunType.Dirty);
		DebugAssertRunList();
	}

	internal void MarkErrorRange(ITextPointer start, ITextPointer end)
	{
		int num = FindIndex(start.CreateStaticPointer(), LogicalDirection.Forward);
		Run run = GetRun(num);
		Invariant.Assert(run.RunType == RunType.Clean);
		Invariant.Assert(run.Position.CompareTo(start) <= 0);
		Invariant.Assert(GetRunEndPosition(num).CompareTo(end) >= 0);
		if (run.Position.CompareTo(start) == 0)
		{
			run.RunType = RunType.Error;
		}
		else
		{
			_runList.Insert(num + 1, new Run(start, RunType.Error));
			num++;
		}
		if (GetRunEndPosition(num).CompareTo(end) > 0)
		{
			_runList.Insert(num + 1, new Run(end, RunType.Clean));
		}
		_highlightLayer.FireChangedEvent(start, end);
		DebugAssertRunList();
	}

	internal bool IsRunType(StaticTextPointer textPosition, LogicalDirection direction, RunType runType)
	{
		int num = FindIndex(textPosition, direction);
		if (num < 0)
		{
			return false;
		}
		return GetRun(num).RunType == runType;
	}

	internal StaticTextPointer GetNextErrorTransition(StaticTextPointer textPosition, LogicalDirection direction)
	{
		StaticTextPointer staticTextPointer = StaticTextPointer.Null;
		int num = FindIndex(textPosition, direction);
		if (num != -1)
		{
			if (direction == LogicalDirection.Forward)
			{
				if (IsErrorRun(num))
				{
					staticTextPointer = GetRunEndPosition(num);
				}
				else
				{
					for (int i = num + 1; i < _runList.Count; i++)
					{
						if (IsErrorRun(i))
						{
							staticTextPointer = GetRun(i).Position.CreateStaticPointer();
							break;
						}
					}
				}
			}
			else if (IsErrorRun(num))
			{
				staticTextPointer = GetRun(num).Position.CreateStaticPointer();
			}
			else
			{
				for (int i = num - 1; i > 0; i--)
				{
					if (IsErrorRun(i))
					{
						staticTextPointer = GetRunEndPosition(i);
						break;
					}
				}
			}
		}
		Invariant.Assert(staticTextPointer.IsNull || textPosition.CompareTo(staticTextPointer) != 0);
		return staticTextPointer;
	}

	internal bool GetError(StaticTextPointer textPosition, LogicalDirection direction, out ITextPointer start, out ITextPointer end)
	{
		start = null;
		end = null;
		int errorIndex = GetErrorIndex(textPosition, direction);
		if (errorIndex >= 0)
		{
			start = GetRun(errorIndex).Position;
			end = GetRunEndPositionDynamic(errorIndex);
		}
		return start != null;
	}

	internal bool GetRun(StaticTextPointer position, LogicalDirection direction, out RunType runType, out StaticTextPointer end)
	{
		int num = FindIndex(position, direction);
		runType = RunType.Clean;
		end = StaticTextPointer.Null;
		if (num < 0)
		{
			return false;
		}
		Run run = GetRun(num);
		runType = run.RunType;
		end = ((direction == LogicalDirection.Forward) ? GetRunEndPosition(num) : run.Position.CreateStaticPointer());
		return true;
	}

	private int GetErrorIndex(StaticTextPointer textPosition, LogicalDirection direction)
	{
		int num = FindIndex(textPosition, direction);
		if (num >= 0)
		{
			Run run = GetRun(num);
			if (run.RunType == RunType.Clean || run.RunType == RunType.Dirty)
			{
				num = -1;
			}
		}
		return num;
	}

	private int FindIndex(StaticTextPointer position, LogicalDirection direction)
	{
		int num = -1;
		int num2 = 0;
		int num3 = _runList.Count;
		while (num2 < num3)
		{
			num = (num2 + num3) / 2;
			Run run = GetRun(num);
			if ((direction == LogicalDirection.Forward && position.CompareTo(run.Position) < 0) || (direction == LogicalDirection.Backward && position.CompareTo(run.Position) <= 0))
			{
				num3 = num;
				continue;
			}
			if ((direction != LogicalDirection.Forward || position.CompareTo(GetRunEndPosition(num)) < 0) && (direction != 0 || position.CompareTo(GetRunEndPosition(num)) <= 0))
			{
				break;
			}
			num2 = num + 1;
		}
		if (num2 >= num3)
		{
			num = -1;
		}
		return num;
	}

	private void MarkRange(ITextPointer start, ITextPointer end, RunType runType)
	{
		if (start.CompareTo(end) == 0)
		{
			return;
		}
		Invariant.Assert(runType == RunType.Clean || runType == RunType.Dirty);
		int num = FindIndex(start.CreateStaticPointer(), LogicalDirection.Forward);
		int num2 = FindIndex(end.CreateStaticPointer(), LogicalDirection.Backward);
		Invariant.Assert(num >= 0);
		Invariant.Assert(num2 >= 0);
		if (num + 1 < num2)
		{
			for (int i = num + 1; i < num2; i++)
			{
				NotifyHighlightLayerBeforeRunChange(i);
			}
			_runList.RemoveRange(num + 1, num2 - num - 1);
			num2 = num + 1;
		}
		if (num == num2)
		{
			AddRun(num, start, end, runType);
			return;
		}
		Invariant.Assert(num == num2 - 1);
		AddRun(num, start, end, runType);
		num2 = FindIndex(end.CreateStaticPointer(), LogicalDirection.Backward);
		Invariant.Assert(num2 >= 0);
		AddRun(num2, start, end, runType);
	}

	private void AddRun(int index, ITextPointer start, ITextPointer end, RunType runType)
	{
		Invariant.Assert(runType == RunType.Clean || runType == RunType.Dirty);
		Invariant.Assert(start.CompareTo(end) < 0);
		RunType runType2 = ((runType == RunType.Clean) ? RunType.Dirty : RunType.Clean);
		Run run = GetRun(index);
		if (run.RunType == runType)
		{
			TryToMergeRunWithNeighbors(index);
		}
		else if (run.RunType == runType2)
		{
			if (run.Position.CompareTo(start) >= 0)
			{
				if (GetRunEndPosition(index).CompareTo(end) <= 0)
				{
					run.RunType = runType;
					TryToMergeRunWithNeighbors(index);
				}
				else if (index > 0 && GetRun(index - 1).RunType == runType)
				{
					run.Position = end;
				}
				else
				{
					run.RunType = runType;
					Run value = new Run(end, runType2);
					_runList.Insert(index + 1, value);
				}
			}
			else if (GetRunEndPosition(index).CompareTo(end) <= 0)
			{
				if (index < _runList.Count - 1 && GetRun(index + 1).RunType == runType)
				{
					GetRun(index + 1).Position = start;
					return;
				}
				Run value = new Run(start, runType);
				_runList.Insert(index + 1, value);
			}
			else
			{
				Run value = new Run(start, runType);
				_runList.Insert(index + 1, value);
				value = new Run(end, runType2);
				_runList.Insert(index + 2, value);
			}
		}
		else
		{
			run.RunType = runType;
			ITextPointer position = run.Position;
			ITextPointer runEndPositionDynamic = GetRunEndPositionDynamic(index);
			TryToMergeRunWithNeighbors(index);
			_highlightLayer.FireChangedEvent(position, runEndPositionDynamic);
		}
	}

	private void TryToMergeRunWithNeighbors(int index)
	{
		Run run = GetRun(index);
		if (index > 0 && GetRun(index - 1).RunType == run.RunType)
		{
			_runList.RemoveAt(index);
			index--;
		}
		if (index < _runList.Count - 1 && GetRun(index + 1).RunType == run.RunType)
		{
			_runList.RemoveAt(index + 1);
		}
	}

	private void OnContentAdded(TextContainerChangeEventArgs e)
	{
		ITextPointer textPointer = ((e.ITextPosition.Offset <= 0) ? e.ITextPosition : e.ITextPosition.CreatePointer(-1));
		textPointer.Freeze();
		ITextPointer textPointer2 = ((e.ITextPosition.Offset + e.Count >= e.ITextPosition.TextContainer.SymbolCount - 1) ? e.ITextPosition.CreatePointer(e.Count) : e.ITextPosition.CreatePointer(e.Count + 1));
		textPointer2.Freeze();
		MarkRange(textPointer, textPointer2, RunType.Dirty);
	}

	private void OnContentRemoved(ITextPointer position)
	{
		int num = FindIndex(position.CreateStaticPointer(), LogicalDirection.Backward);
		if (num == -1)
		{
			num = 0;
		}
		Run run = GetRun(num);
		if (run.RunType != RunType.Dirty)
		{
			NotifyHighlightLayerBeforeRunChange(num);
			run.RunType = RunType.Dirty;
			if (num > 0 && GetRun(num - 1).RunType == RunType.Dirty)
			{
				_runList.RemoveAt(num);
				num--;
			}
		}
		num++;
		int i;
		for (i = num; i < _runList.Count; i++)
		{
			ITextPointer position2 = GetRun(i).Position;
			if (position2.CompareTo(position) > 0 && position2.CompareTo(GetRunEndPosition(i)) != 0)
			{
				break;
			}
		}
		_runList.RemoveRange(num, i - num);
		if (num < _runList.Count)
		{
			NotifyHighlightLayerBeforeRunChange(num);
			_runList.RemoveAt(num);
			if (num < _runList.Count && GetRun(num).RunType == RunType.Dirty)
			{
				_runList.RemoveAt(num);
			}
		}
	}

	private void NotifyHighlightLayerBeforeRunChange(int index)
	{
		if (IsErrorRun(index))
		{
			ITextPointer position = GetRun(index).Position;
			ITextPointer runEndPositionDynamic = GetRunEndPositionDynamic(index);
			if (position.CompareTo(runEndPositionDynamic) != 0)
			{
				_highlightLayer.FireChangedEvent(position, runEndPositionDynamic);
			}
		}
	}

	private void DebugAssertRunList()
	{
		Invariant.Assert(_runList.Count >= 1, "Run list should never be empty!");
		if (!Invariant.Strict)
		{
			return;
		}
		RunType runType = RunType.Clean;
		for (int i = 0; i < _runList.Count; i++)
		{
			Run run = GetRun(i);
			if (_runList.Count == 1)
			{
				Invariant.Assert(run.Position.CompareTo(run.Position.TextContainer.Start) == 0);
			}
			else
			{
				Invariant.Assert(run.Position.CompareTo(GetRunEndPosition(i)) <= 0, "Found negative width run!");
			}
			Invariant.Assert(i == 0 || GetRunEndPosition(i - 1).CompareTo(run.Position) <= 0, "Found overlapping runs!");
			if (!IsErrorRun(i))
			{
				Invariant.Assert(i == 0 || runType != run.RunType, "Found consecutive dirty/dirt or clean/clean runs!");
			}
			runType = run.RunType;
		}
	}

	private Run GetRun(int index)
	{
		return (Run)_runList[index];
	}

	private ITextPointer GetRunEndPositionDynamic(int index)
	{
		return GetRunEndPosition(index).CreateDynamicTextPointer(LogicalDirection.Forward);
	}

	private StaticTextPointer GetRunEndPosition(int index)
	{
		if (index + 1 < _runList.Count)
		{
			return GetRun(index + 1).Position.CreateStaticPointer();
		}
		ITextContainer textContainer = GetRun(index).Position.TextContainer;
		return textContainer.CreateStaticPointerAtOffset(textContainer.SymbolCount);
	}

	private bool IsErrorRun(int index)
	{
		Run run = GetRun(index);
		if (run.RunType != 0)
		{
			return run.RunType != RunType.Dirty;
		}
		return false;
	}
}
