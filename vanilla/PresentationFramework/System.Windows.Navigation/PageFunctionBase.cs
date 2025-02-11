using System.Windows.Controls;
using MS.Internal.AppModel;

namespace System.Windows.Navigation;

/// <summary>An abstract base class that is the parent of all page function classes.</summary>
public abstract class PageFunctionBase : Page
{
	private Guid _pageFunctionId;

	private Guid _parentPageFunctionId;

	private bool _fRemoveFromJournal = true;

	private bool _resume;

	private ReturnEventSaver _saverInfo;

	private FinishEventHandler _finish;

	private Delegate _returnHandler;

	/// <summary>Gets or sets a value that indicates whether the page function should not be added to navigation history.</summary>
	/// <returns>A <see cref="T:System.Boolean" /> that indicates whether a page function should not be added to navigation history. The default value is false.</returns>
	public bool RemoveFromJournal
	{
		get
		{
			return _fRemoveFromJournal;
		}
		set
		{
			_fRemoveFromJournal = value;
		}
	}

	internal Guid PageFunctionId
	{
		get
		{
			return _pageFunctionId;
		}
		set
		{
			_pageFunctionId = value;
		}
	}

	internal Guid ParentPageFunctionId
	{
		get
		{
			return _parentPageFunctionId;
		}
		set
		{
			_parentPageFunctionId = value;
		}
	}

	internal Delegate _Return => _returnHandler;

	internal bool _Resume
	{
		get
		{
			return _resume;
		}
		set
		{
			_resume = value;
		}
	}

	internal ReturnEventSaver _Saver
	{
		get
		{
			return _saverInfo;
		}
		set
		{
			_saverInfo = value;
		}
	}

	internal FinishEventHandler FinishHandler
	{
		get
		{
			return _finish;
		}
		set
		{
			_finish = value;
		}
	}

	internal event EventToRaiseTypedEvent RaiseTypedEvent;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Navigation.PageFunctionBase" /> type.</summary>
	protected PageFunctionBase()
	{
		PageFunctionId = Guid.NewGuid();
		ParentPageFunctionId = Guid.Empty;
	}

	/// <summary>Override this method to initialize a <see cref="T:System.Windows.Navigation.PageFunction`1" /> when it is navigated to for the first time.</summary>
	protected virtual void Start()
	{
	}

	internal void CallStart()
	{
		Start();
	}

	internal void _OnReturnUnTyped(object o)
	{
		if (_finish != null)
		{
			_finish(this, o);
		}
	}

	internal void _AddEventHandler(Delegate d)
	{
		if (d.Target is PageFunctionBase pageFunctionBase)
		{
			ParentPageFunctionId = pageFunctionBase.PageFunctionId;
		}
		_returnHandler = Delegate.Combine(_returnHandler, d);
	}

	internal void _RemoveEventHandler(Delegate d)
	{
		_returnHandler = Delegate.Remove(_returnHandler, d);
	}

	internal void _DetachEvents()
	{
		_returnHandler = null;
	}

	internal void _OnFinish(object returnEventArgs)
	{
		RaiseTypedEventArgs args = new RaiseTypedEventArgs(_returnHandler, returnEventArgs);
		this.RaiseTypedEvent(this, args);
	}
}
