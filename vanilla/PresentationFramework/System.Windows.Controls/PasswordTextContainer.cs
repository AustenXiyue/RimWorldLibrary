using System.Collections;
using System.Security;
using System.Windows.Documents;
using MS.Internal;
using MS.Internal.Documents;

namespace System.Windows.Controls;

internal sealed class PasswordTextContainer : ITextContainer
{
	private readonly PasswordBox _passwordBox;

	private SecureString _password;

	private ArrayList _positionList;

	private Highlights _highlights;

	private int _changeBlockLevel;

	private TextContainerChangedEventArgs _changes;

	private ITextView _textview;

	private bool _isReadOnly;

	private EventHandler Changing;

	private TextContainerChangeEventHandler Change;

	private TextContainerChangedEventHandler Changed;

	private ITextSelection _textSelection;

	bool ITextContainer.IsReadOnly => false;

	ITextPointer ITextContainer.Start => Start;

	ITextPointer ITextContainer.End => End;

	uint ITextContainer.Generation => 0u;

	Highlights ITextContainer.Highlights
	{
		get
		{
			if (_highlights == null)
			{
				_highlights = new Highlights(this);
			}
			return _highlights;
		}
	}

	DependencyObject ITextContainer.Parent => _passwordBox;

	ITextSelection ITextContainer.TextSelection
	{
		get
		{
			return _textSelection;
		}
		set
		{
			_textSelection = value;
		}
	}

	UndoManager ITextContainer.UndoManager => null;

	ITextView ITextContainer.TextView
	{
		get
		{
			return TextView;
		}
		set
		{
			TextView = value;
		}
	}

	internal ITextView TextView
	{
		get
		{
			return _textview;
		}
		set
		{
			_textview = value;
		}
	}

	int ITextContainer.SymbolCount => SymbolCount;

	int ITextContainer.IMECharCount => SymbolCount;

	internal ITextPointer Start => new PasswordTextPointer(this, LogicalDirection.Backward, 0);

	internal ITextPointer End => new PasswordTextPointer(this, LogicalDirection.Forward, SymbolCount);

	internal int SymbolCount => _password.Length;

	internal char PasswordChar => PasswordBox.PasswordChar;

	internal PasswordBox PasswordBox => _passwordBox;

	event EventHandler ITextContainer.Changing
	{
		add
		{
			Changing = (EventHandler)Delegate.Combine(Changing, value);
		}
		remove
		{
			Changing = (EventHandler)Delegate.Remove(Changing, value);
		}
	}

	event TextContainerChangeEventHandler ITextContainer.Change
	{
		add
		{
			Change = (TextContainerChangeEventHandler)Delegate.Combine(Change, value);
		}
		remove
		{
			Change = (TextContainerChangeEventHandler)Delegate.Remove(Change, value);
		}
	}

	event TextContainerChangedEventHandler ITextContainer.Changed
	{
		add
		{
			Changed = (TextContainerChangedEventHandler)Delegate.Combine(Changed, value);
		}
		remove
		{
			Changed = (TextContainerChangedEventHandler)Delegate.Remove(Changed, value);
		}
	}

	internal PasswordTextContainer(PasswordBox passwordBox)
	{
		_passwordBox = passwordBox;
		_password = new SecureString();
	}

	internal void InsertText(ITextPointer position, string textData)
	{
		BeginChange();
		try
		{
			int offset = ((PasswordTextPointer)position).Offset;
			for (int i = 0; i < textData.Length; i++)
			{
				_password.InsertAt(offset + i, textData[i]);
			}
			OnPasswordChange(offset, textData.Length);
		}
		finally
		{
			EndChange();
		}
	}

	internal void DeleteContent(ITextPointer startPosition, ITextPointer endPosition)
	{
		BeginChange();
		try
		{
			int offset = ((PasswordTextPointer)startPosition).Offset;
			int offset2 = ((PasswordTextPointer)endPosition).Offset;
			for (int i = 0; i < offset2 - offset; i++)
			{
				_password.RemoveAt(offset);
			}
			OnPasswordChange(offset, offset - offset2);
		}
		finally
		{
			EndChange();
		}
	}

	internal void BeginChange()
	{
		_changeBlockLevel++;
	}

	internal void EndChange()
	{
		EndChange(skipEvents: false);
	}

	internal void EndChange(bool skipEvents)
	{
		Invariant.Assert(_changeBlockLevel > 0, "Unmatched EndChange call!");
		_changeBlockLevel--;
		if (_changeBlockLevel == 0 && _changes != null)
		{
			TextContainerChangedEventArgs changes = _changes;
			_changes = null;
			if (Changed != null && !skipEvents)
			{
				Changed(this, changes);
			}
		}
	}

	void ITextContainer.BeginChange()
	{
		BeginChange();
	}

	void ITextContainer.BeginChangeNoUndo()
	{
		((ITextContainer)this).BeginChange();
	}

	void ITextContainer.EndChange()
	{
		EndChange(skipEvents: false);
	}

	void ITextContainer.EndChange(bool skipEvents)
	{
		EndChange(skipEvents);
	}

	ITextPointer ITextContainer.CreatePointerAtOffset(int offset, LogicalDirection direction)
	{
		return new PasswordTextPointer(this, direction, offset);
	}

	ITextPointer ITextContainer.CreatePointerAtCharOffset(int charOffset, LogicalDirection direction)
	{
		return ((ITextContainer)this).CreatePointerAtOffset(charOffset, direction);
	}

	ITextPointer ITextContainer.CreateDynamicTextPointer(StaticTextPointer position, LogicalDirection direction)
	{
		return ((ITextPointer)position.Handle0).CreatePointer(direction);
	}

	StaticTextPointer ITextContainer.CreateStaticPointerAtOffset(int offset)
	{
		return new StaticTextPointer(this, ((ITextContainer)this).CreatePointerAtOffset(offset, LogicalDirection.Forward));
	}

	TextPointerContext ITextContainer.GetPointerContext(StaticTextPointer pointer, LogicalDirection direction)
	{
		return ((ITextPointer)pointer.Handle0).GetPointerContext(direction);
	}

	int ITextContainer.GetOffsetToPosition(StaticTextPointer position1, StaticTextPointer position2)
	{
		return ((ITextPointer)position1.Handle0).GetOffsetToPosition((ITextPointer)position2.Handle0);
	}

	int ITextContainer.GetTextInRun(StaticTextPointer position, LogicalDirection direction, char[] textBuffer, int startIndex, int count)
	{
		return ((ITextPointer)position.Handle0).GetTextInRun(direction, textBuffer, startIndex, count);
	}

	object ITextContainer.GetAdjacentElement(StaticTextPointer position, LogicalDirection direction)
	{
		return ((ITextPointer)position.Handle0).GetAdjacentElement(direction);
	}

	DependencyObject ITextContainer.GetParent(StaticTextPointer position)
	{
		return null;
	}

	StaticTextPointer ITextContainer.CreatePointer(StaticTextPointer position, int offset)
	{
		return new StaticTextPointer(this, ((ITextPointer)position.Handle0).CreatePointer(offset));
	}

	StaticTextPointer ITextContainer.GetNextContextPosition(StaticTextPointer position, LogicalDirection direction)
	{
		return new StaticTextPointer(this, ((ITextPointer)position.Handle0).GetNextContextPosition(direction));
	}

	int ITextContainer.CompareTo(StaticTextPointer position1, StaticTextPointer position2)
	{
		return ((ITextPointer)position1.Handle0).CompareTo((ITextPointer)position2.Handle0);
	}

	int ITextContainer.CompareTo(StaticTextPointer position1, ITextPointer position2)
	{
		return ((ITextPointer)position1.Handle0).CompareTo(position2);
	}

	object ITextContainer.GetValue(StaticTextPointer position, DependencyProperty formattingProperty)
	{
		return ((ITextPointer)position.Handle0).GetValue(formattingProperty);
	}

	internal void AddPosition(PasswordTextPointer position)
	{
		RemoveUnreferencedPositions();
		if (_positionList == null)
		{
			_positionList = new ArrayList();
		}
		int index = FindIndex(position.Offset, position.LogicalDirection);
		_positionList.Insert(index, new WeakReference(position));
		DebugAssertPositionList();
	}

	internal void RemovePosition(PasswordTextPointer searchPosition)
	{
		Invariant.Assert(_positionList != null);
		int i;
		for (i = 0; i < _positionList.Count; i++)
		{
			if (GetPointerAtIndex(i) == searchPosition)
			{
				_positionList.RemoveAt(i);
				i = -1;
				break;
			}
		}
		Invariant.Assert(i == -1, "Couldn't find position to remove!");
	}

	internal SecureString GetPasswordCopy()
	{
		return _password.Copy();
	}

	internal void SetPassword(SecureString value)
	{
		int symbolCount = SymbolCount;
		_password.Clear();
		OnPasswordChange(0, -symbolCount);
		_password = ((value == null) ? new SecureString() : value.Copy());
		OnPasswordChange(0, SymbolCount);
	}

	private void AddChange(ITextPointer startPosition, int symbolCount, PrecursorTextChangeType precursorTextChange)
	{
		Invariant.Assert(_changeBlockLevel > 0, "All public APIs must call BeginChange!");
		Invariant.Assert(!_isReadOnly, "Illegal to modify PasswordTextContainer inside Change event scope!");
		if (Changing != null)
		{
			Changing(this, EventArgs.Empty);
		}
		if (_changes == null)
		{
			_changes = new TextContainerChangedEventArgs();
		}
		_changes.AddChange(precursorTextChange, startPosition.Offset, symbolCount, collectTextChanges: false);
		if (Change != null)
		{
			Invariant.Assert(precursorTextChange == PrecursorTextChangeType.ContentAdded || precursorTextChange == PrecursorTextChangeType.ContentRemoved);
			TextChangeType textChange = ((precursorTextChange != 0) ? TextChangeType.ContentRemoved : TextChangeType.ContentAdded);
			_isReadOnly = true;
			try
			{
				Change(this, new TextContainerChangeEventArgs(startPosition, symbolCount, symbolCount, textChange));
			}
			finally
			{
				_isReadOnly = false;
			}
		}
	}

	private void OnPasswordChange(int offset, int delta)
	{
		if (delta != 0)
		{
			UpdatePositionList(offset, delta);
			PasswordTextPointer startPosition = new PasswordTextPointer(this, LogicalDirection.Forward, offset);
			int symbolCount;
			PrecursorTextChangeType precursorTextChange;
			if (delta > 0)
			{
				symbolCount = delta;
				precursorTextChange = PrecursorTextChangeType.ContentAdded;
			}
			else
			{
				symbolCount = -delta;
				precursorTextChange = PrecursorTextChangeType.ContentRemoved;
			}
			AddChange(startPosition, symbolCount, precursorTextChange);
		}
	}

	private void UpdatePositionList(int offset, int delta)
	{
		if (_positionList == null)
		{
			return;
		}
		RemoveUnreferencedPositions();
		int i = FindIndex(offset, LogicalDirection.Forward);
		if (delta < 0)
		{
			int num = -1;
			for (; i < _positionList.Count; i++)
			{
				PasswordTextPointer pointerAtIndex = GetPointerAtIndex(i);
				if (pointerAtIndex == null)
				{
					continue;
				}
				if (pointerAtIndex.Offset > offset + -delta)
				{
					break;
				}
				pointerAtIndex.Offset = offset;
				if (pointerAtIndex.LogicalDirection == LogicalDirection.Backward)
				{
					if (num >= 0)
					{
						WeakReference value = (WeakReference)_positionList[num];
						_positionList[num] = _positionList[i];
						_positionList[i] = value;
						num++;
					}
				}
				else if (num == -1)
				{
					num = i;
				}
			}
		}
		for (; i < _positionList.Count; i++)
		{
			PasswordTextPointer pointerAtIndex = GetPointerAtIndex(i);
			if (pointerAtIndex != null)
			{
				pointerAtIndex.Offset += delta;
			}
		}
		DebugAssertPositionList();
	}

	private void RemoveUnreferencedPositions()
	{
		if (_positionList == null)
		{
			return;
		}
		for (int num = _positionList.Count - 1; num >= 0; num--)
		{
			if (GetPointerAtIndex(num) == null)
			{
				_positionList.RemoveAt(num);
			}
		}
	}

	private int FindIndex(int offset, LogicalDirection gravity)
	{
		Invariant.Assert(_positionList != null);
		int i;
		for (i = 0; i < _positionList.Count; i++)
		{
			PasswordTextPointer pointerAtIndex = GetPointerAtIndex(i);
			if (pointerAtIndex != null && ((pointerAtIndex.Offset == offset && (pointerAtIndex.LogicalDirection == gravity || gravity == LogicalDirection.Backward)) || pointerAtIndex.Offset > offset))
			{
				break;
			}
		}
		return i;
	}

	private void DebugAssertPositionList()
	{
		if (!Invariant.Strict)
		{
			return;
		}
		int num = -1;
		LogicalDirection logicalDirection = LogicalDirection.Backward;
		for (int i = 0; i < _positionList.Count; i++)
		{
			PasswordTextPointer pointerAtIndex = GetPointerAtIndex(i);
			if (pointerAtIndex != null)
			{
				Invariant.Assert(pointerAtIndex.Offset >= 0 && pointerAtIndex.Offset <= _password.Length);
				Invariant.Assert(num <= pointerAtIndex.Offset);
				if (i > 0 && pointerAtIndex.LogicalDirection == LogicalDirection.Backward && num == pointerAtIndex.Offset)
				{
					Invariant.Assert(logicalDirection != LogicalDirection.Forward);
				}
				num = pointerAtIndex.Offset;
				logicalDirection = pointerAtIndex.LogicalDirection;
			}
		}
	}

	private PasswordTextPointer GetPointerAtIndex(int index)
	{
		Invariant.Assert(_positionList != null);
		WeakReference obj = (WeakReference)_positionList[index];
		Invariant.Assert(obj != null);
		object target = obj.Target;
		if (target != null && !(target is PasswordTextPointer))
		{
			Invariant.Assert(condition: false, "Unexpected type: " + target.GetType());
		}
		return (PasswordTextPointer)target;
	}
}
