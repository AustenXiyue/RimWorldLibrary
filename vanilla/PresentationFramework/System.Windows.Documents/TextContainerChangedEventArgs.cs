using System.Collections.Generic;
using System.Windows.Controls;

namespace System.Windows.Documents;

internal class TextContainerChangedEventArgs : EventArgs
{
	private bool _hasContentAddedOrRemoved;

	private bool _hasLocalPropertyValueChange;

	private SortedList<int, TextChange> _changes;

	internal bool HasContentAddedOrRemoved => _hasContentAddedOrRemoved;

	internal bool HasLocalPropertyValueChange => _hasLocalPropertyValueChange;

	internal SortedList<int, TextChange> Changes => _changes;

	internal TextContainerChangedEventArgs()
	{
		_changes = new SortedList<int, TextChange>();
	}

	internal void SetLocalPropertyValueChanged()
	{
		_hasLocalPropertyValueChange = true;
	}

	internal void AddChange(PrecursorTextChangeType textChange, int offset, int length, bool collectTextChanges)
	{
		if (textChange == PrecursorTextChangeType.ContentAdded || textChange == PrecursorTextChangeType.ElementAdded || textChange == PrecursorTextChangeType.ContentRemoved || textChange == PrecursorTextChangeType.ElementExtracted)
		{
			_hasContentAddedOrRemoved = true;
		}
		if (collectTextChanges)
		{
			switch (textChange)
			{
			case PrecursorTextChangeType.ElementAdded:
				AddChangeToList(textChange, offset, 1);
				AddChangeToList(textChange, offset + length - 1, 1);
				break;
			case PrecursorTextChangeType.ElementExtracted:
				AddChangeToList(textChange, offset + length - 1, 1);
				AddChangeToList(textChange, offset, 1);
				break;
			case PrecursorTextChangeType.PropertyModified:
				break;
			default:
				AddChangeToList(textChange, offset, length);
				break;
			}
		}
	}

	private void AddChangeToList(PrecursorTextChangeType textChange, int offset, int length)
	{
		int num = 0;
		TextChange textChange2 = null;
		bool flag = false;
		int num2 = Changes.IndexOfKey(offset);
		if (num2 != -1)
		{
			textChange2 = Changes.Values[num2];
		}
		else
		{
			textChange2 = new TextChange();
			textChange2.Offset = offset;
			Changes.Add(offset, textChange2);
			num2 = Changes.IndexOfKey(offset);
		}
		switch (textChange)
		{
		case PrecursorTextChangeType.ContentAdded:
		case PrecursorTextChangeType.ElementAdded:
			textChange2.AddedLength += length;
			num = length;
			break;
		case PrecursorTextChangeType.ContentRemoved:
		case PrecursorTextChangeType.ElementExtracted:
			textChange2.RemovedLength += Math.Max(0, length - textChange2.AddedLength);
			textChange2.AddedLength = Math.Max(0, textChange2.AddedLength - length);
			num = -length;
			flag = true;
			break;
		}
		int num3;
		if (num2 > 0 && textChange != PrecursorTextChangeType.PropertyModified)
		{
			num3 = num2 - 1;
			TextChange textChange3 = null;
			while (num3 >= 0)
			{
				TextChange textChange4 = Changes.Values[num3];
				if (textChange4.Offset + textChange4.AddedLength >= offset && MergeTextChangeLeft(textChange4, textChange2, flag, length))
				{
					textChange3 = textChange4;
				}
				num3--;
			}
			if (textChange3 != null)
			{
				textChange2 = textChange3;
			}
			num2 = Changes.IndexOfKey(textChange2.Offset);
		}
		num3 = num2 + 1;
		if (flag && num3 < Changes.Count)
		{
			while (num3 < Changes.Count && Changes.Values[num3].Offset <= offset + length)
			{
				MergeTextChangeRight(Changes.Values[num3], textChange2, offset, length);
			}
			num2 = Changes.IndexOfKey(textChange2.Offset);
		}
		if (num != 0)
		{
			SortedList<int, TextChange> sortedList = new SortedList<int, TextChange>(Changes.Count);
			for (num3 = 0; num3 < Changes.Count; num3++)
			{
				TextChange textChange5 = Changes.Values[num3];
				if (num3 > num2)
				{
					textChange5.Offset += num;
				}
				sortedList.Add(textChange5.Offset, textChange5);
			}
			_changes = sortedList;
		}
		DeleteChangeIfEmpty(textChange2);
	}

	private void DeleteChangeIfEmpty(TextChange change)
	{
		if (change.AddedLength == 0 && change.RemovedLength == 0)
		{
			Changes.Remove(change.Offset);
		}
	}

	private bool MergeTextChangeLeft(TextChange oldChange, TextChange newChange, bool isDeletion, int length)
	{
		if (oldChange.Offset + oldChange.AddedLength >= newChange.Offset)
		{
			if (isDeletion)
			{
				int num = Math.Min(oldChange.AddedLength - (newChange.Offset - oldChange.Offset), newChange.RemovedLength);
				oldChange.AddedLength -= num;
				oldChange.RemovedLength += length - num;
			}
			else
			{
				oldChange.AddedLength += length;
			}
			Changes.Remove(newChange.Offset);
			return true;
		}
		return false;
	}

	private void MergeTextChangeRight(TextChange oldChange, TextChange newChange, int offset, int length)
	{
		int num = ((oldChange.AddedLength > 0) ? (offset + length - oldChange.Offset) : 0);
		if (num >= oldChange.AddedLength)
		{
			newChange.RemovedLength += oldChange.RemovedLength - oldChange.AddedLength;
			Changes.Remove(oldChange.Offset);
		}
		else
		{
			newChange.RemovedLength += oldChange.RemovedLength - num;
			newChange.AddedLength += oldChange.AddedLength - num;
			Changes.Remove(oldChange.Offset);
		}
	}
}
