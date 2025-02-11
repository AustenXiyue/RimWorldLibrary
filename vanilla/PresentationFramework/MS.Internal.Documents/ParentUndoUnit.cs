using System;
using System.Collections;
using System.Windows;

namespace MS.Internal.Documents;

internal class ParentUndoUnit : IParentUndoUnit, IUndoUnit
{
	private string _description;

	private bool _locked;

	private IParentUndoUnit _openedUnit;

	private IUndoUnit _lastUnit;

	private Stack _units;

	private object _container;

	public string Description
	{
		get
		{
			return _description;
		}
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}
			_description = value;
		}
	}

	public IParentUndoUnit OpenedUnit => _openedUnit;

	public IUndoUnit LastUnit => _lastUnit;

	public virtual bool Locked
	{
		get
		{
			return _locked;
		}
		protected set
		{
			_locked = value;
		}
	}

	public object Container
	{
		get
		{
			return _container;
		}
		set
		{
			if (!(value is IParentUndoUnit) && !(value is UndoManager))
			{
				throw new Exception(SR.UndoContainerTypeMismatch);
			}
			_container = value;
		}
	}

	protected IParentUndoUnit DeepestOpenUnit
	{
		get
		{
			IParentUndoUnit openedUnit = _openedUnit;
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

	protected object TopContainer
	{
		get
		{
			object obj = this;
			while (obj is IParentUndoUnit && ((IParentUndoUnit)obj).Container != null)
			{
				obj = ((IParentUndoUnit)obj).Container;
			}
			return obj;
		}
	}

	protected Stack Units => _units;

	public ParentUndoUnit(string description)
	{
		Init(description);
	}

	public virtual void Open(IParentUndoUnit newUnit)
	{
		if (newUnit == null)
		{
			throw new ArgumentNullException("newUnit");
		}
		IParentUndoUnit deepestOpenUnit = DeepestOpenUnit;
		if (deepestOpenUnit == null)
		{
			if (IsInParentUnitChain(newUnit))
			{
				throw new InvalidOperationException(SR.UndoUnitCantBeOpenedTwice);
			}
			_openedUnit = newUnit;
			if (newUnit != null)
			{
				newUnit.Container = this;
			}
		}
		else
		{
			if (newUnit != null)
			{
				newUnit.Container = deepestOpenUnit;
			}
			deepestOpenUnit.Open(newUnit);
		}
	}

	public virtual void Close(UndoCloseAction closeAction)
	{
		Close(OpenedUnit, closeAction);
	}

	public virtual void Close(IParentUndoUnit unit, UndoCloseAction closeAction)
	{
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
			IParentUndoUnit parentUndoUnit = this;
			while (parentUndoUnit.OpenedUnit != null && parentUndoUnit.OpenedUnit != unit)
			{
				parentUndoUnit = parentUndoUnit.OpenedUnit;
			}
			if (parentUndoUnit.OpenedUnit == null)
			{
				throw new ArgumentException(SR.UndoUnitNotFound, "unit");
			}
			if (parentUndoUnit != this)
			{
				parentUndoUnit.Close(closeAction);
				return;
			}
		}
		UndoManager undoManager = TopContainer as UndoManager;
		if (closeAction != 0)
		{
			if (undoManager != null)
			{
				undoManager.IsEnabled = false;
			}
			if (OpenedUnit.OpenedUnit != null)
			{
				OpenedUnit.Close(closeAction);
			}
			if (closeAction == UndoCloseAction.Rollback)
			{
				OpenedUnit.Do();
			}
			_openedUnit = null;
			if (TopContainer is UndoManager)
			{
				((UndoManager)TopContainer).OnNextDiscard();
			}
			else
			{
				((IParentUndoUnit)TopContainer).OnNextDiscard();
			}
			if (undoManager != null)
			{
				undoManager.IsEnabled = true;
			}
		}
		else
		{
			if (OpenedUnit.OpenedUnit != null)
			{
				OpenedUnit.Close(UndoCloseAction.Commit);
			}
			IParentUndoUnit openedUnit = OpenedUnit;
			_openedUnit = null;
			Add(openedUnit);
			SetLastUnit(openedUnit);
		}
	}

	public virtual void Add(IUndoUnit unit)
	{
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
		if (IsInParentUnitChain(unit))
		{
			throw new InvalidOperationException(SR.UndoUnitCantBeAddedTwice);
		}
		if (Locked)
		{
			throw new InvalidOperationException(SR.UndoUnitLocked);
		}
		if (!Merge(unit))
		{
			_units.Push(unit);
			if (LastUnit is IParentUndoUnit)
			{
				((IParentUndoUnit)LastUnit).OnNextAdd();
			}
			SetLastUnit(unit);
		}
	}

	public virtual void Clear()
	{
		if (Locked)
		{
			throw new InvalidOperationException(SR.UndoUnitLocked);
		}
		_units.Clear();
		SetOpenedUnit(null);
		SetLastUnit(null);
	}

	public virtual void OnNextAdd()
	{
		_locked = true;
		foreach (IUndoUnit unit in _units)
		{
			if (unit is IParentUndoUnit)
			{
				((IParentUndoUnit)unit).OnNextAdd();
			}
		}
	}

	public virtual void OnNextDiscard()
	{
		_locked = false;
		IParentUndoUnit parentUndoUnit = this;
		foreach (IUndoUnit unit in _units)
		{
			if (unit is IParentUndoUnit)
			{
				parentUndoUnit = unit as IParentUndoUnit;
			}
		}
		if (parentUndoUnit != this)
		{
			parentUndoUnit.OnNextDiscard();
		}
	}

	public virtual void Do()
	{
		IParentUndoUnit unit = CreateParentUndoUnitForSelf();
		UndoManager undoManager = TopContainer as UndoManager;
		if (undoManager != null && undoManager.IsEnabled)
		{
			undoManager.Open(unit);
		}
		while (_units.Count > 0)
		{
			(_units.Pop() as IUndoUnit).Do();
		}
		if (undoManager != null && undoManager.IsEnabled)
		{
			undoManager.Close(unit, UndoCloseAction.Commit);
		}
	}

	public virtual bool Merge(IUndoUnit unit)
	{
		Invariant.Assert(unit != null);
		return false;
	}

	protected void Init(string description)
	{
		if (description == null)
		{
			description = string.Empty;
		}
		_description = description;
		_locked = false;
		_openedUnit = null;
		_units = new Stack(2);
		_container = null;
	}

	protected void SetOpenedUnit(IParentUndoUnit value)
	{
		_openedUnit = value;
	}

	protected void SetLastUnit(IUndoUnit value)
	{
		_lastUnit = value;
	}

	protected virtual IParentUndoUnit CreateParentUndoUnitForSelf()
	{
		return new ParentUndoUnit(Description);
	}

	private bool IsInParentUnitChain(IUndoUnit unit)
	{
		if (unit is IParentUndoUnit)
		{
			IParentUndoUnit parentUndoUnit = this;
			do
			{
				if (parentUndoUnit == unit)
				{
					return true;
				}
				parentUndoUnit = parentUndoUnit.Container as IParentUndoUnit;
			}
			while (parentUndoUnit != null);
		}
		return false;
	}
}
