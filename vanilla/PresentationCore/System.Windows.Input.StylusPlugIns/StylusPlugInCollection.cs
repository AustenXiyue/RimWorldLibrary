using System.Collections.ObjectModel;
using System.Windows.Media;
using MS.Internal.PresentationCore;

namespace System.Windows.Input.StylusPlugIns;

/// <summary>Represent a collection of <see cref="T:System.Windows.Input.StylusPlugIns.StylusPlugIn" /> objects.</summary>
public sealed class StylusPlugInCollection : Collection<StylusPlugIn>
{
	private StylusPlugInCollectionBase _stylusPlugInCollectionImpl;

	private UIElement _element;

	private Rect _rc;

	private GeneralTransform _viewToElement;

	private Transform _lastRenderTransform;

	private DependencyPropertyChangedEventHandler _isEnabledChangedEventHandler;

	private DependencyPropertyChangedEventHandler _isVisibleChangedEventHandler;

	private DependencyPropertyChangedEventHandler _isHitTestVisibleChangedEventHandler;

	private EventHandler _renderTransformChangedEventHandler;

	private SourceChangedEventHandler _sourceChangedEventHandler;

	private EventHandler _layoutChangedEventHandler;

	internal UIElement Element => _element;

	internal GeneralTransform ViewToElement => _viewToElement;

	internal Rect Rect => _rc;

	internal bool IsActiveForInput => _stylusPlugInCollectionImpl.IsActiveForInput;

	internal object SyncRoot => _stylusPlugInCollectionImpl.SyncRoot;

	protected override void InsertItem(int index, StylusPlugIn plugIn)
	{
		_element.VerifyAccess();
		if (plugIn == null)
		{
			throw new ArgumentNullException("plugIn", SR.Stylus_PlugInIsNull);
		}
		if (IndexOf(plugIn) != -1)
		{
			throw new ArgumentException(SR.Stylus_PlugInIsDuplicated, "plugIn");
		}
		ExecuteWithPotentialDispatcherDisable(delegate
		{
			if (_stylusPlugInCollectionImpl.IsActiveForInput)
			{
				ExecuteWithPotentialLock(delegate
				{
					base.InsertItem(index, plugIn);
					plugIn.Added(this);
				});
				return;
			}
			EnsureEventsHooked();
			base.InsertItem(index, plugIn);
			try
			{
				plugIn.Added(this);
			}
			finally
			{
				_stylusPlugInCollectionImpl.UpdateState(_element);
			}
		});
	}

	protected override void ClearItems()
	{
		_element.VerifyAccess();
		if (base.Count == 0)
		{
			return;
		}
		ExecuteWithPotentialDispatcherDisable(delegate
		{
			if (_stylusPlugInCollectionImpl.IsActiveForInput)
			{
				ExecuteWithPotentialLock(delegate
				{
					while (base.Count > 0)
					{
						RemoveItem(0);
					}
				});
			}
			else
			{
				while (base.Count > 0)
				{
					RemoveItem(0);
				}
			}
		});
	}

	protected override void RemoveItem(int index)
	{
		_element.VerifyAccess();
		ExecuteWithPotentialDispatcherDisable(delegate
		{
			if (_stylusPlugInCollectionImpl.IsActiveForInput)
			{
				ExecuteWithPotentialLock(delegate
				{
					StylusPlugIn stylusPlugIn = base[index];
					base.RemoveItem(index);
					try
					{
						EnsureEventsUnhooked();
					}
					finally
					{
						stylusPlugIn.Removed();
					}
				});
				return;
			}
			StylusPlugIn stylusPlugIn2 = base[index];
			base.RemoveItem(index);
			try
			{
				EnsureEventsUnhooked();
			}
			finally
			{
				stylusPlugIn2.Removed();
			}
		});
	}

	protected override void SetItem(int index, StylusPlugIn plugIn)
	{
		_element.VerifyAccess();
		if (plugIn == null)
		{
			throw new ArgumentNullException("plugIn", SR.Stylus_PlugInIsNull);
		}
		if (IndexOf(plugIn) != -1)
		{
			throw new ArgumentException(SR.Stylus_PlugInIsDuplicated, "plugIn");
		}
		ExecuteWithPotentialDispatcherDisable(delegate
		{
			if (_stylusPlugInCollectionImpl.IsActiveForInput)
			{
				ExecuteWithPotentialLock(delegate
				{
					StylusPlugIn stylusPlugIn = base[index];
					base.SetItem(index, plugIn);
					try
					{
						stylusPlugIn.Removed();
					}
					finally
					{
						plugIn.Added(this);
					}
				});
				return;
			}
			StylusPlugIn stylusPlugIn2 = base[index];
			base.SetItem(index, plugIn);
			try
			{
				stylusPlugIn2.Removed();
			}
			finally
			{
				plugIn.Added(this);
			}
		});
	}

	internal StylusPlugInCollection(UIElement element)
	{
		_stylusPlugInCollectionImpl = StylusPlugInCollectionBase.Create(this);
		_element = element;
		_isEnabledChangedEventHandler = OnIsEnabledChanged;
		_isVisibleChangedEventHandler = OnIsVisibleChanged;
		_isHitTestVisibleChangedEventHandler = OnIsHitTestVisibleChanged;
		_sourceChangedEventHandler = OnSourceChanged;
		_layoutChangedEventHandler = OnLayoutUpdated;
	}

	internal void UpdateRect()
	{
		if (_element.IsArrangeValid && _element.IsEnabled && _element.IsVisible && _element.IsHitTestVisible)
		{
			_rc = new Rect(default(Point), _element.RenderSize);
			Visual containingVisual2D = VisualTreeHelper.GetContainingVisual2D(InputElement.GetRootVisual(_element));
			try
			{
				_viewToElement = containingVisual2D.TransformToDescendant(_element);
			}
			catch (InvalidOperationException)
			{
				_rc = default(Rect);
				_viewToElement = Transform.Identity;
			}
		}
		else
		{
			_rc = default(Rect);
		}
		if (_viewToElement == null)
		{
			_viewToElement = Transform.Identity;
		}
	}

	internal bool IsHit(Point pt)
	{
		Point result = pt;
		_viewToElement.TryTransform(result, out result);
		return _rc.Contains(result);
	}

	internal void FireEnterLeave(bool isEnter, RawStylusInput rawStylusInput, bool confirmed)
	{
		if (_stylusPlugInCollectionImpl.IsActiveForInput)
		{
			ExecuteWithPotentialLock(delegate
			{
				for (int i = 0; i < base.Count; i++)
				{
					base[i].StylusEnterLeave(isEnter, rawStylusInput, confirmed);
				}
			});
		}
		else
		{
			for (int j = 0; j < base.Count; j++)
			{
				base[j].StylusEnterLeave(isEnter, rawStylusInput, confirmed);
			}
		}
	}

	internal void FireRawStylusInput(RawStylusInput args)
	{
		try
		{
			if (_stylusPlugInCollectionImpl.IsActiveForInput)
			{
				ExecuteWithPotentialLock(delegate
				{
					for (int i = 0; i < base.Count; i++)
					{
						StylusPlugIn stylusPlugIn = base[i];
						args.CurrentNotifyPlugIn = stylusPlugIn;
						stylusPlugIn.RawStylusInput(args);
					}
				});
			}
			else
			{
				for (int j = 0; j < base.Count; j++)
				{
					StylusPlugIn stylusPlugIn2 = base[j];
					args.CurrentNotifyPlugIn = stylusPlugIn2;
					stylusPlugIn2.RawStylusInput(args);
				}
			}
		}
		finally
		{
			args.CurrentNotifyPlugIn = null;
		}
	}

	internal void OnLayoutUpdated(object sender, EventArgs e)
	{
		if (_stylusPlugInCollectionImpl.IsActiveForInput)
		{
			ExecuteWithPotentialDispatcherDisable(delegate
			{
				ExecuteWithPotentialLock(delegate
				{
					UpdateRect();
				});
			});
		}
		else
		{
			UpdateRect();
		}
		if (_lastRenderTransform != _element.RenderTransform)
		{
			if (_renderTransformChangedEventHandler != null)
			{
				_lastRenderTransform.Changed -= _renderTransformChangedEventHandler;
				_renderTransformChangedEventHandler = null;
			}
			_lastRenderTransform = _element.RenderTransform;
		}
		if (_lastRenderTransform == null)
		{
			return;
		}
		if (_lastRenderTransform.IsFrozen)
		{
			if (_renderTransformChangedEventHandler != null)
			{
				_renderTransformChangedEventHandler = null;
			}
		}
		else if (_renderTransformChangedEventHandler == null)
		{
			_renderTransformChangedEventHandler = OnRenderTransformChanged;
			_lastRenderTransform.Changed += _renderTransformChangedEventHandler;
		}
	}

	internal void ExecuteWithPotentialLock(Action action)
	{
		if (_stylusPlugInCollectionImpl.SyncRoot != null)
		{
			lock (_stylusPlugInCollectionImpl.SyncRoot)
			{
				action();
				return;
			}
		}
		action();
	}

	internal void ExecuteWithPotentialDispatcherDisable(Action action)
	{
		if (_stylusPlugInCollectionImpl.SyncRoot != null)
		{
			using (_element.Dispatcher.DisableProcessing())
			{
				action();
				return;
			}
		}
		action();
	}

	private void EnsureEventsHooked()
	{
		if (base.Count == 0)
		{
			UpdateRect();
			_element.IsEnabledChanged += _isEnabledChangedEventHandler;
			_element.IsVisibleChanged += _isVisibleChangedEventHandler;
			_element.IsHitTestVisibleChanged += _isHitTestVisibleChangedEventHandler;
			PresentationSource.AddSourceChangedHandler(_element, _sourceChangedEventHandler);
			_element.LayoutUpdated += _layoutChangedEventHandler;
			if (_element.RenderTransform != null && !_element.RenderTransform.IsFrozen && _renderTransformChangedEventHandler == null)
			{
				_renderTransformChangedEventHandler = OnRenderTransformChanged;
				_element.RenderTransform.Changed += _renderTransformChangedEventHandler;
			}
		}
	}

	private void EnsureEventsUnhooked()
	{
		if (base.Count == 0)
		{
			_element.IsEnabledChanged -= _isEnabledChangedEventHandler;
			_element.IsVisibleChanged -= _isVisibleChangedEventHandler;
			_element.IsHitTestVisibleChanged -= _isHitTestVisibleChangedEventHandler;
			if (_renderTransformChangedEventHandler != null)
			{
				_element.RenderTransform.Changed -= _renderTransformChangedEventHandler;
			}
			PresentationSource.RemoveSourceChangedHandler(_element, _sourceChangedEventHandler);
			_element.LayoutUpdated -= _layoutChangedEventHandler;
			ExecuteWithPotentialDispatcherDisable(delegate
			{
				_stylusPlugInCollectionImpl.Unhook();
			});
		}
	}

	private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		_stylusPlugInCollectionImpl.UpdateState(_element);
	}

	private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		_stylusPlugInCollectionImpl.UpdateState(_element);
	}

	private void OnIsHitTestVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		_stylusPlugInCollectionImpl.UpdateState(_element);
	}

	private void OnRenderTransformChanged(object sender, EventArgs e)
	{
		OnLayoutUpdated(sender, e);
	}

	private void OnSourceChanged(object sender, SourceChangedEventArgs e)
	{
		_stylusPlugInCollectionImpl.UpdateState(_element);
	}
}
