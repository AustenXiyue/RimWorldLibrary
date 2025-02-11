using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;

namespace MS.Internal.Documents;

internal class UndoManager
{
	private static readonly DependencyProperty UndoManagerInstanceProperty = DependencyProperty.RegisterAttached("UndoManagerInstance", typeof(UndoManager), typeof(UndoManager), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

	private DependencyObject _scope;

	private IParentUndoUnit _openedUnit;

	private IUndoUnit _lastUnit;

	private List<IUndoUnit> _undoStack;

	private Stack _redoStack;

	private UndoState _state;

	private bool _isEnabled;

	private IParentUndoUnit _lastReopenedUnit;

	private int _topUndoIndex;

	private int _bottomUndoIndex;

	private int _undoLimit;

	private int _minUndoStackCount;

	private bool _imeSupportModeEnabled;

	private const int _undoLimitDefaultValue = 100;

	internal bool IsImeSupportModeEnabled
	{
		get
		{
			return _imeSupportModeEnabled;
		}
		set
		{
			if (value == _imeSupportModeEnabled)
			{
				return;
			}
			if (value)
			{
				if (_bottomUndoIndex != 0 && _topUndoIndex >= 0)
				{
					List<IUndoUnit> list = new List<IUndoUnit>(UndoCount);
					if (_bottomUndoIndex > _topUndoIndex)
					{
						for (int i = _bottomUndoIndex; i < UndoLimit; i++)
						{
							list.Add(_undoStack[i]);
						}
						_bottomUndoIndex = 0;
					}
					for (int i = _bottomUndoIndex; i <= _topUndoIndex; i++)
					{
						list.Add(_undoStack[i]);
					}
					_undoStack = list;
					_bottomUndoIndex = 0;
					_topUndoIndex = list.Count - 1;
				}
				_imeSupportModeEnabled = value;
				return;
			}
			_imeSupportModeEnabled = value;
			if (!IsEnabled)
			{
				DoClear();
			}
			else if (UndoLimit >= 0 && _topUndoIndex >= UndoLimit)
			{
				List<IUndoUnit> list2 = new List<IUndoUnit>(UndoLimit);
				for (int j = _topUndoIndex + 1 - UndoLimit; j <= _topUndoIndex; j++)
				{
					list2.Add(_undoStack[j]);
				}
				_undoStack = list2;
				_bottomUndoIndex = 0;
				_topUndoIndex = UndoLimit - 1;
			}
		}
	}

	internal int UndoLimit
	{
		get
		{
			if (!_imeSupportModeEnabled)
			{
				return _undoLimit;
			}
			return -1;
		}
		set
		{
			_undoLimit = value;
			if (!_imeSupportModeEnabled)
			{
				DoClear();
			}
		}
	}

	internal UndoState State => _state;

	internal bool IsEnabled
	{
		get
		{
			if (!_imeSupportModeEnabled)
			{
				if (_isEnabled)
				{
					return _undoLimit != 0;
				}
				return false;
			}
			return true;
		}
		set
		{
			_isEnabled = value;
		}
	}

	internal IParentUndoUnit OpenedUnit => _openedUnit;

	internal IUndoUnit LastUnit => _lastUnit;

	internal IParentUndoUnit LastReopenedUnit => _lastReopenedUnit;

	internal int UndoCount
	{
		get
		{
			if (UndoStack.Count == 0 || _topUndoIndex < 0)
			{
				return 0;
			}
			if (_topUndoIndex == _bottomUndoIndex - 1 && PeekUndoStack() == null)
			{
				return 0;
			}
			if (_topUndoIndex >= _bottomUndoIndex)
			{
				return _topUndoIndex - _bottomUndoIndex + 1;
			}
			return _topUndoIndex + (UndoLimit - _bottomUndoIndex) + 1;
		}
	}

	internal int RedoCount => RedoStack.Count;

	internal static int UndoLimitDefaultValue => 100;

	internal int MinUndoStackCount
	{
		get
		{
			return _minUndoStackCount;
		}
		set
		{
			_minUndoStackCount = value;
		}
	}

	protected IParentUndoUnit DeepestOpenUnit
	{
		get
		{
			IParentUndoUnit openedUnit = OpenedUnit;
			if (openedUnit != null)
			{
				while (openedUnit.OpenedUnit != null)
				{
					openedUnit = openedUnit.OpenedUnit;
				}
			}
			return openedUnit;
		}
	}

	protected List<IUndoUnit> UndoStack => _undoStack;

	protected Stack RedoStack => _redoStack;

	internal UndoManager()
	{
		_scope = null;
		_state = UndoState.Normal;
		_isEnabled = false;
		_undoStack = new List<IUndoUnit>(4);
		_redoStack = new Stack(2);
		_topUndoIndex = -1;
		_bottomUndoIndex = 0;
		_undoLimit = 100;
	}

	internal static void AttachUndoManager(DependencyObject scope, UndoManager undoManager)
	{
		if (scope == null)
		{
			throw new ArgumentNullException("scope");
		}
		if (undoManager == null)
		{
			throw new ArgumentNullException("undoManager");
		}
		if (undoManager != null && undoManager._scope != null)
		{
			throw new InvalidOperationException(SR.UndoManagerAlreadyAttached);
		}
		DetachUndoManager(scope);
		scope.SetValue(UndoManagerInstanceProperty, undoManager);
		if (undoManager != null)
		{
			undoManager._scope = scope;
		}
		undoManager.IsEnabled = true;
	}

	internal static void DetachUndoManager(DependencyObject scope)
	{
		if (scope == null)
		{
			throw new ArgumentNullException("scope");
		}
		if (scope.ReadLocalValue(UndoManagerInstanceProperty) is UndoManager undoManager)
		{
			undoManager.IsEnabled = false;
			scope.ClearValue(UndoManagerInstanceProperty);
			if (undoManager != null)
			{
				undoManager._scope = null;
			}
		}
	}

	internal static UndoManager GetUndoManager(DependencyObject target)
	{
		if (target == null)
		{
			return null;
		}
		return target.GetValue(UndoManagerInstanceProperty) as UndoManager;
	}

	internal void Open(IParentUndoUnit unit)
	{
		if (!IsEnabled)
		{
			throw new InvalidOperationException(SR.UndoServiceDisabled);
		}
		if (unit == null)
		{
			throw new ArgumentNullException("unit");
		}
		IParentUndoUnit deepestOpenUnit = DeepestOpenUnit;
		if (deepestOpenUnit == unit)
		{
			throw new InvalidOperationException(SR.UndoUnitCantBeOpenedTwice);
		}
		if (deepestOpenUnit == null)
		{
			if (unit != LastUnit)
			{
				Add(unit);
				SetLastUnit(unit);
			}
			SetOpenedUnit(unit);
			unit.Container = this;
		}
		else
		{
			unit.Container = deepestOpenUnit;
			deepestOpenUnit.Open(unit);
		}
	}

	internal void Reopen(IParentUndoUnit unit)
	{
		if (!IsEnabled)
		{
			throw new InvalidOperationException(SR.UndoServiceDisabled);
		}
		if (unit == null)
		{
			throw new ArgumentNullException("unit");
		}
		if (OpenedUnit != null)
		{
			throw new InvalidOperationException(SR.UndoUnitAlreadyOpen);
		}
		switch (State)
		{
		case UndoState.Normal:
		case UndoState.Redo:
			if (UndoCount == 0 || PeekUndoStack() != unit)
			{
				throw new InvalidOperationException(SR.UndoUnitNotOnTopOfStack);
			}
			break;
		case UndoState.Undo:
			if (RedoStack.Count == 0 || (IParentUndoUnit)RedoStack.Peek() != unit)
			{
				throw new InvalidOperationException(SR.UndoUnitNotOnTopOfStack);
			}
			break;
		}
		if (unit.Locked)
		{
			throw new InvalidOperationException(SR.UndoUnitLocked);
		}
		Open(unit);
		_lastReopenedUnit = unit;
	}

	internal void Close(UndoCloseAction closeAction)
	{
		Close(OpenedUnit, closeAction);
	}

	internal void Close(IParentUndoUnit unit, UndoCloseAction closeAction)
	{
		if (!IsEnabled)
		{
			throw new InvalidOperationException(SR.UndoServiceDisabled);
		}
		if (unit == null)
		{
			throw new ArgumentNullException("unit");
		}
		if (OpenedUnit == null)
		{
			throw new InvalidOperationException(SR.UndoNoOpenUnit);
		}
		if (OpenedUnit != unit)
		{
			IParentUndoUnit openedUnit = OpenedUnit;
			while (openedUnit.OpenedUnit != null && openedUnit.OpenedUnit != unit)
			{
				openedUnit = openedUnit.OpenedUnit;
			}
			if (openedUnit.OpenedUnit == null)
			{
				throw new ArgumentException(SR.UndoUnitNotFound, "unit");
			}
			openedUnit.Close(closeAction);
		}
		else if (closeAction != 0)
		{
			SetState(UndoState.Rollback);
			if (unit.OpenedUnit != null)
			{
				unit.Close(closeAction);
			}
			if (closeAction == UndoCloseAction.Rollback)
			{
				unit.Do();
			}
			PopUndoStack();
			SetOpenedUnit(null);
			OnNextDiscard();
			SetLastUnit((_topUndoIndex == -1) ? null : PeekUndoStack());
			SetState(UndoState.Normal);
		}
		else
		{
			if (unit.OpenedUnit != null)
			{
				unit.Close(UndoCloseAction.Commit);
			}
			if (State != UndoState.Redo && State != UndoState.Undo && RedoStack.Count > 0)
			{
				RedoStack.Clear();
			}
			SetOpenedUnit(null);
		}
	}

	internal void Add(IUndoUnit unit)
	{
		if (!IsEnabled)
		{
			throw new InvalidOperationException(SR.UndoServiceDisabled);
		}
		if (unit == null)
		{
			throw new ArgumentNullException("unit");
		}
		IParentUndoUnit deepestOpenUnit = DeepestOpenUnit;
		if (deepestOpenUnit != null)
		{
			deepestOpenUnit.Add(unit);
			return;
		}
		if (unit is IParentUndoUnit)
		{
			((IParentUndoUnit)unit).Container = this;
			if (LastUnit is IParentUndoUnit)
			{
				((IParentUndoUnit)LastUnit).OnNextAdd();
			}
			SetLastUnit(unit);
			if (State == UndoState.Normal || State == UndoState.Redo)
			{
				if (++_topUndoIndex == UndoLimit)
				{
					_topUndoIndex = 0;
				}
				if ((_topUndoIndex >= UndoStack.Count || PeekUndoStack() != null) && (UndoLimit == -1 || UndoStack.Count < UndoLimit))
				{
					UndoStack.Add(unit);
					return;
				}
				if (PeekUndoStack() != null && ++_bottomUndoIndex == UndoLimit)
				{
					_bottomUndoIndex = 0;
				}
				UndoStack[_topUndoIndex] = unit;
			}
			else if (State == UndoState.Undo)
			{
				RedoStack.Push(unit);
			}
			else
			{
				_ = State;
				_ = 3;
			}
			return;
		}
		throw new InvalidOperationException(SR.UndoNoOpenParentUnit);
	}

	internal void Clear()
	{
		if (!IsEnabled)
		{
			throw new InvalidOperationException(SR.UndoServiceDisabled);
		}
		if (!_imeSupportModeEnabled)
		{
			DoClear();
		}
	}

	internal void Undo(int count)
	{
		if (!IsEnabled)
		{
			throw new InvalidOperationException(SR.UndoServiceDisabled);
		}
		if (count > UndoCount || count <= 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (State != 0)
		{
			throw new InvalidOperationException(SR.UndoNotInNormalState);
		}
		if (OpenedUnit != null)
		{
			throw new InvalidOperationException(SR.UndoUnitOpen);
		}
		Invariant.Assert(UndoCount > _minUndoStackCount);
		SetState(UndoState.Undo);
		bool flag = true;
		try
		{
			while (count > 0)
			{
				PopUndoStack().Do();
				count--;
			}
			flag = false;
		}
		finally
		{
			if (flag)
			{
				Clear();
			}
		}
		SetState(UndoState.Normal);
	}

	internal void Redo(int count)
	{
		if (!IsEnabled)
		{
			throw new InvalidOperationException(SR.UndoServiceDisabled);
		}
		if (count > RedoStack.Count || count <= 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (State != 0)
		{
			throw new InvalidOperationException(SR.UndoNotInNormalState);
		}
		if (OpenedUnit != null)
		{
			throw new InvalidOperationException(SR.UndoUnitOpen);
		}
		SetState(UndoState.Redo);
		bool flag = true;
		try
		{
			while (count > 0)
			{
				((IUndoUnit)RedoStack.Pop()).Do();
				count--;
			}
			flag = false;
		}
		finally
		{
			if (flag)
			{
				Clear();
			}
		}
		SetState(UndoState.Normal);
	}

	internal virtual void OnNextDiscard()
	{
		if (UndoCount > 0)
		{
			((IParentUndoUnit)PeekUndoStack()).OnNextDiscard();
		}
	}

	internal IUndoUnit PeekUndoStack()
	{
		if (_topUndoIndex < 0 || _topUndoIndex == UndoStack.Count)
		{
			return null;
		}
		return UndoStack[_topUndoIndex];
	}

	internal Stack SetRedoStack(Stack value)
	{
		Stack redoStack = _redoStack;
		if (value == null)
		{
			value = new Stack(2);
		}
		_redoStack = value;
		return redoStack;
	}

	internal IUndoUnit GetUndoUnit(int index)
	{
		Invariant.Assert(index < UndoCount);
		Invariant.Assert(index >= 0);
		Invariant.Assert(_bottomUndoIndex == 0);
		Invariant.Assert(_imeSupportModeEnabled);
		return _undoStack[index];
	}

	internal void RemoveUndoRange(int index, int count)
	{
		Invariant.Assert(index >= 0);
		Invariant.Assert(count >= 0);
		Invariant.Assert(count + index <= UndoCount);
		Invariant.Assert(_bottomUndoIndex == 0);
		Invariant.Assert(_imeSupportModeEnabled);
		for (int i = index + count; i <= _topUndoIndex; i++)
		{
			_undoStack[i - count] = _undoStack[i];
		}
		for (int i = _topUndoIndex - (count - 1); i <= _topUndoIndex; i++)
		{
			_undoStack[i] = null;
		}
		_topUndoIndex -= count;
	}

	protected void SetState(UndoState value)
	{
		_state = value;
	}

	protected void SetOpenedUnit(IParentUndoUnit value)
	{
		_openedUnit = value;
	}

	protected void SetLastUnit(IUndoUnit value)
	{
		_lastUnit = value;
	}

	private void DoClear()
	{
		Invariant.Assert(!_imeSupportModeEnabled);
		if (UndoStack.Count > 0)
		{
			UndoStack.Clear();
			UndoStack.TrimExcess();
		}
		if (RedoStack.Count > 0)
		{
			RedoStack.Clear();
		}
		SetLastUnit(null);
		SetOpenedUnit(null);
		_topUndoIndex = -1;
		_bottomUndoIndex = 0;
	}

	private IUndoUnit PopUndoStack()
	{
		int num = UndoCount - 1;
		IUndoUnit result = UndoStack[_topUndoIndex];
		UndoStack[_topUndoIndex--] = null;
		if (_topUndoIndex < 0 && num > 0)
		{
			Invariant.Assert(UndoLimit > 0);
			_topUndoIndex = UndoLimit - 1;
		}
		return result;
	}
}
