using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Automation;
using MS.Internal.PresentationCore;
using MS.Win32;

namespace System.Windows.Automation.Peers;

/// <summary>Provides a base class that exposes an element to UI Automation. </summary>
public abstract class AutomationPeer : DispatcherObject
{
	private enum HeadingLevel
	{
		None = 80050,
		Level1,
		Level2,
		Level3,
		Level4,
		Level5,
		Level6,
		Level7,
		Level8,
		Level9
	}

	private delegate object WrapObject(AutomationPeer peer, object iface);

	private class PatternInfo
	{
		internal int Id;

		internal WrapObject WrapObject;

		internal PatternInterface PatternInterface;

		internal PatternInfo(int id, WrapObject wrapObject, PatternInterface patternInterface)
		{
			Id = id;
			WrapObject = wrapObject;
			PatternInterface = patternInterface;
		}
	}

	private delegate object GetProperty(AutomationPeer peer);

	private class PeerRecord
	{
		private AutomationPeer _eventsSource;

		private AutomationPeer _iterationParent;

		public AutomationPeer EventsSource
		{
			get
			{
				return _eventsSource;
			}
			set
			{
				_eventsSource = value;
			}
		}

		public AutomationPeer IterationParent
		{
			get
			{
				return _iterationParent;
			}
			set
			{
				_iterationParent = value;
			}
		}
	}

	private static Hashtable s_patternInfo;

	private static Hashtable s_propertyInfo;

	private int _index = -1;

	private nint _hwnd;

	private List<AutomationPeer> _children;

	private AutomationPeer _parent;

	private object _eventsSourceOrPeerRecord;

	private Rect _boundingRectangle;

	private string _itemStatus;

	private string _name;

	private bool _isOffscreen;

	private bool _isEnabled;

	private bool _invalidated;

	private bool _ancestorsInvalid;

	private bool _childrenValid;

	private bool _addedToEventList;

	private bool _publicCallInProgress;

	private bool _publicSetFocusInProgress;

	private bool _isInteropPeer;

	private bool _hasIterationParent;

	private WeakReference _elementProxyWeakReference;

	private static DispatcherOperationCallback _updatePeer;

	/// <summary>Gets a value that indicates whether the element that is associated with this <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> hosts hwnds in Windows Presentation Foundation (WPF).</summary>
	/// <returns>true if the element that is associated with this <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> hosts hwnds in Windows Presentation Foundation (WPF); otherwise, false.</returns>
	protected internal virtual bool IsHwndHost => false;

	private AutomationPeer IterationParent
	{
		get
		{
			if (_hasIterationParent)
			{
				return ((PeerRecord)_eventsSourceOrPeerRecord).IterationParent;
			}
			return _parent;
		}
		set
		{
			if (value == _parent)
			{
				if (_hasIterationParent)
				{
					_eventsSourceOrPeerRecord = EventsSource;
					_hasIterationParent = false;
				}
			}
			else if (!_hasIterationParent)
			{
				PeerRecord eventsSourceOrPeerRecord = new PeerRecord
				{
					EventsSource = EventsSource,
					IterationParent = value
				};
				_eventsSourceOrPeerRecord = eventsSourceOrPeerRecord;
				_hasIterationParent = true;
			}
			else
			{
				((PeerRecord)_eventsSourceOrPeerRecord).IterationParent = value;
			}
		}
	}

	/// <summary>Gets or sets an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> that is reported to the automation client as a source for all the events that come from this <see cref="T:System.Windows.Automation.Peers.AutomationPeer" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> that is the source of events.</returns>
	public AutomationPeer EventsSource
	{
		get
		{
			if (_hasIterationParent)
			{
				return ((PeerRecord)_eventsSourceOrPeerRecord).EventsSource;
			}
			return (AutomationPeer)_eventsSourceOrPeerRecord;
		}
		set
		{
			if (!_hasIterationParent)
			{
				_eventsSourceOrPeerRecord = value;
			}
			else
			{
				((PeerRecord)_eventsSourceOrPeerRecord).EventsSource = value;
			}
		}
	}

	internal nint Hwnd
	{
		get
		{
			return _hwnd;
		}
		set
		{
			_hwnd = value;
		}
	}

	internal virtual bool AncestorsInvalid
	{
		get
		{
			return _ancestorsInvalid;
		}
		set
		{
			_ancestorsInvalid = value;
		}
	}

	internal bool ChildrenValid
	{
		get
		{
			return _childrenValid;
		}
		set
		{
			_childrenValid = value;
		}
	}

	internal bool IsInteropPeer
	{
		get
		{
			return _isInteropPeer;
		}
		set
		{
			_isInteropPeer = value;
		}
	}

	internal int Index => _index;

	internal List<AutomationPeer> Children => _children;

	internal WeakReference ElementProxyWeakReference
	{
		get
		{
			return _elementProxyWeakReference;
		}
		set
		{
			if (value.Target is ElementProxy)
			{
				_elementProxyWeakReference = value;
			}
		}
	}

	internal bool IncludeInvisibleElementsInControlView => AccessibilitySwitches.UseNetFx472CompatibleAccessibilityFeatures;

	static AutomationPeer()
	{
		_updatePeer = UpdatePeer;
		using (Dispatcher.CurrentDispatcher.DisableProcessing())
		{
			Initialize();
		}
	}

	/// <summary>When overridden in a derived class, is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.</summary>
	/// <returns>The collection of child elements.</returns>
	protected abstract List<AutomationPeer> GetChildrenCore();

	/// <summary>When overridden in a derived class, gets the control pattern that is associated with the specified <see cref="T:System.Windows.Automation.Peers.PatternInterface" />.</summary>
	/// <returns>The object that implements the pattern interface; null if this peer does not support this interface.</returns>
	/// <param name="patternInterface">A value from the <see cref="T:System.Windows.Automation.Peers.PatternInterface" /> enumeration.</param>
	public abstract object GetPattern(PatternInterface patternInterface);

	/// <summary>Triggers recalculation of the main properties of the <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> and raises the <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged" /> notification to the Automation Client if the properties changed. </summary>
	public void InvalidatePeer()
	{
		if (!_invalidated)
		{
			base.Dispatcher.BeginInvoke(DispatcherPriority.Background, _updatePeer, this);
			_invalidated = true;
		}
	}

	/// <summary>Gets a value that indicates whether UI Automation is listening for the specified event. </summary>
	/// <returns>A boolean that indicates whether UI Automation is listening for the event.</returns>
	/// <param name="eventId">One of the enumeration values.</param>
	public static bool ListenerExists(AutomationEvents eventId)
	{
		return EventMap.HasRegisteredEvent(eventId);
	}

	/// <summary>Raises an automation event.</summary>
	/// <param name="eventId">The event identifier.</param>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public void RaiseAutomationEvent(AutomationEvents eventId)
	{
		AutomationEvent registeredEvent = EventMap.GetRegisteredEvent(eventId);
		if (registeredEvent != null)
		{
			IRawElementProviderSimple rawElementProviderSimple = ProviderFromPeer(this);
			if (rawElementProviderSimple != null)
			{
				AutomationInteropProvider.RaiseAutomationEvent(registeredEvent, rawElementProviderSimple, new AutomationEventArgs(registeredEvent));
			}
		}
	}

	/// <summary>Raises an event to notify the automation client of a changed property value.</summary>
	/// <param name="property">The property that changed.</param>
	/// <param name="oldValue">The previous value of the property.</param>
	/// <param name="newValue">The new value of the property.</param>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public void RaisePropertyChangedEvent(AutomationProperty property, object oldValue, object newValue)
	{
		if (AutomationInteropProvider.ClientsAreListening)
		{
			RaisePropertyChangedInternal(ProviderFromPeer(this), property, oldValue, newValue);
		}
	}

	/// <summary>Called by the <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> to raise the <see cref="F:System.Windows.Automation.AutomationElement.AsyncContentLoadedEvent" /> event.</summary>
	/// <param name="args">The event data.</param>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public void RaiseAsyncContentLoadedEvent(AsyncContentLoadedEventArgs args)
	{
		if (args == null)
		{
			throw new ArgumentNullException("args");
		}
		if (EventMap.HasRegisteredEvent(AutomationEvents.AsyncContentLoaded))
		{
			IRawElementProviderSimple rawElementProviderSimple = ProviderFromPeer(this);
			if (rawElementProviderSimple != null)
			{
				AutomationInteropProvider.RaiseAutomationEvent(AutomationElementIdentifiers.AsyncContentLoadedEvent, rawElementProviderSimple, args);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void RaiseNotificationEvent(AutomationNotificationKind notificationKind, AutomationNotificationProcessing notificationProcessing, string displayString, string activityId)
	{
		if (EventMap.HasRegisteredEvent(AutomationEvents.Notification))
		{
			IRawElementProviderSimple rawElementProviderSimple = ProviderFromPeer(this);
			if (rawElementProviderSimple != null)
			{
				AutomationInteropProvider.RaiseAutomationEvent(AutomationElementIdentifiers.NotificationEvent, rawElementProviderSimple, new NotificationEventArgs(notificationKind, notificationProcessing, displayString, activityId));
			}
		}
	}

	internal static void RaiseFocusChangedEventHelper(IInputElement newFocus)
	{
		if (EventMap.HasRegisteredEvent(AutomationEvents.AutomationFocusChanged))
		{
			AutomationPeerFromInputElement(newFocus)?.RaiseAutomationEvent(AutomationEvents.AutomationFocusChanged);
		}
	}

	internal static AutomationPeer AutomationPeerFromInputElement(IInputElement focusedElement)
	{
		AutomationPeer automationPeer = null;
		if (focusedElement is UIElement element)
		{
			automationPeer = UIElementAutomationPeer.CreatePeerForElement(element);
		}
		else if (focusedElement is ContentElement element2)
		{
			automationPeer = ContentElementAutomationPeer.CreatePeerForElement(element2);
		}
		else if (focusedElement is UIElement3D element3)
		{
			automationPeer = UIElement3DAutomationPeer.CreatePeerForElement(element3);
		}
		if (automationPeer != null)
		{
			automationPeer.ValidateConnected(automationPeer);
			if (automationPeer.EventsSource != null)
			{
				automationPeer = automationPeer.EventsSource;
			}
		}
		return automationPeer;
	}

	internal AutomationPeer ValidateConnected(AutomationPeer connectedPeer)
	{
		if (connectedPeer == null)
		{
			throw new ArgumentNullException("connectedPeer");
		}
		if (_parent != null && _hwnd != IntPtr.Zero)
		{
			return this;
		}
		if (connectedPeer._hwnd != IntPtr.Zero)
		{
			while (connectedPeer._parent != null)
			{
				connectedPeer = connectedPeer._parent;
			}
			if (connectedPeer == this || isDescendantOf(connectedPeer))
			{
				return this;
			}
		}
		ContextLayoutManager contextLayoutManager = ContextLayoutManager.From(base.Dispatcher);
		if (contextLayoutManager != null && contextLayoutManager.AutomationSyncUpdateCounter == 0)
		{
			AutomationPeer[] automationRoots = contextLayoutManager.GetAutomationRoots();
			foreach (AutomationPeer automationPeer in automationRoots)
			{
				if (automationPeer != null && (automationPeer == this || isDescendantOf(automationPeer)))
				{
					return this;
				}
			}
		}
		return null;
	}

	internal bool TrySetParentInfo(AutomationPeer peer)
	{
		Invariant.Assert(peer != null);
		if (peer._hwnd == IntPtr.Zero)
		{
			return false;
		}
		_hwnd = peer._hwnd;
		_parent = peer;
		return true;
	}

	internal virtual bool IsDataItemAutomationPeer()
	{
		return false;
	}

	internal virtual bool IgnoreUpdatePeer()
	{
		return false;
	}

	internal virtual void AddToParentProxyWeakRefCache()
	{
	}

	private bool isDescendantOf(AutomationPeer parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		List<AutomationPeer> children = parent.GetChildren();
		if (children == null)
		{
			return false;
		}
		int count = children.Count;
		for (int i = 0; i < count; i++)
		{
			AutomationPeer automationPeer = children[i];
			if (automationPeer == this || isDescendantOf(automationPeer))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Tells UI Automation where in the UI Automation tree to place the hwnd being hosted by a Windows Presentation Foundation (WPF) element.</summary>
	/// <returns>This method returns the hosted hwnd to UI Automation for controls that host hwnd objects.</returns>
	protected virtual HostedWindowWrapper GetHostRawElementProviderCore()
	{
		HostedWindowWrapper result = null;
		if (GetParent() == null)
		{
			result = HostedWindowWrapper.CreateInternal(Hwnd);
		}
		return result;
	}

	internal HostedWindowWrapper GetHostRawElementProvider()
	{
		return GetHostRawElementProviderCore();
	}

	/// <summary>When overridden in a derived class, is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetBoundingRectangle" />.</summary>
	/// <returns>The bounding rectangle.</returns>
	protected abstract Rect GetBoundingRectangleCore();

	/// <summary>When overridden in a derived class, is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsOffscreen" />.</summary>
	/// <returns>true if the element is not on the screen; otherwise, false.</returns>
	protected abstract bool IsOffscreenCore();

	/// <summary>When overridden in a derived class, is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetOrientation" />.</summary>
	/// <returns>The orientation of the control.</returns>
	protected abstract AutomationOrientation GetOrientationCore();

	/// <summary>When overridden in a derived class, is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetItemType" />.</summary>
	/// <returns>The kind of item.</returns>
	protected abstract string GetItemTypeCore();

	/// <summary>When overridden in a derived class, is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>The class name.</returns>
	protected abstract string GetClassNameCore();

	/// <summary>When overridden in a derived class, is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetItemStatus" />.</summary>
	/// <returns>The status.</returns>
	protected abstract string GetItemStatusCore();

	/// <summary>When overridden in a derived class, is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsRequiredForForm" />.</summary>
	/// <returns>true if the element is must be completed; otherwise, false.</returns>
	protected abstract bool IsRequiredForFormCore();

	/// <summary>When overridden in a derived class, is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsKeyboardFocusable" />.</summary>
	/// <returns>true if the element can accept keyboard focus; otherwise, false.</returns>
	protected abstract bool IsKeyboardFocusableCore();

	/// <summary>When overridden in a derived class, is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.HasKeyboardFocus" />.</summary>
	/// <returns>true if the element has keyboard focus; otherwise, false.</returns>
	protected abstract bool HasKeyboardFocusCore();

	/// <summary>When overridden in a derived class, is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsEnabled" />.</summary>
	/// <returns>true if the automation peer can receive and send events; otherwise, false.</returns>
	protected abstract bool IsEnabledCore();

	/// <summary>When overridden in a derived class, is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsPassword" />.</summary>
	/// <returns>true if the element contains sensitive content; otherwise, false.</returns>
	protected abstract bool IsPasswordCore();

	/// <summary>When overridden in a derived class, is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationId" />.</summary>
	/// <returns>The string that contains the identifier.</returns>
	protected abstract string GetAutomationIdCore();

	/// <summary>When overridden in a derived class, is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetName" />.</summary>
	/// <returns>The string that contains the label.</returns>
	protected abstract string GetNameCore();

	/// <summary>When overridden in a derived class, is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The control type.</returns>
	protected abstract AutomationControlType GetAutomationControlTypeCore();

	/// <summary>When overridden in a derived class, is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetLocalizedControlType" />.</summary>
	/// <returns>The type of the control.</returns>
	protected virtual string GetLocalizedControlTypeCore()
	{
		return GetControlType().LocalizedControlType;
	}

	/// <summary>When overridden in a derived class, is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsContentElement" />.</summary>
	/// <returns>true if the element is a content element; otherwise, false.</returns>
	protected abstract bool IsContentElementCore();

	/// <summary>When overridden in a derived class, is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsControlElement" />.</summary>
	/// <returns>true if the element is a control; otherwise, false.</returns>
	protected abstract bool IsControlElementCore();

	protected virtual bool IsDialogCore()
	{
		return false;
	}

	/// <summary>When overridden in a derived class, is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetLabeledBy" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Automation.Peers.LabelAutomationPeer" /> for the element that is targeted by the <see cref="T:System.Windows.Controls.Label" />.</returns>
	protected abstract AutomationPeer GetLabeledByCore();

	/// <summary>When overridden in a derived class, is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetHelpText" />.</summary>
	/// <returns>The help text.</returns>
	protected abstract string GetHelpTextCore();

	/// <summary>When overridden in a derived class, is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAcceleratorKey" />.</summary>
	/// <returns>The accelerator key.</returns>
	protected abstract string GetAcceleratorKeyCore();

	/// <summary>When overridden in a derived class, is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAccessKey" />.</summary>
	/// <returns>The string that contains the access key.</returns>
	protected abstract string GetAccessKeyCore();

	/// <summary>When overridden in a derived class, is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClickablePoint" />.</summary>
	/// <returns>A point within the clickable area of the element.</returns>
	protected abstract Point GetClickablePointCore();

	/// <summary>When overridden in a derived class, is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.SetFocus" />.</summary>
	protected abstract void SetFocusCore();

	protected virtual AutomationLiveSetting GetLiveSettingCore()
	{
		return AutomationLiveSetting.Off;
	}

	protected virtual List<AutomationPeer> GetControlledPeersCore()
	{
		return null;
	}

	protected virtual int GetSizeOfSetCore()
	{
		return -1;
	}

	protected virtual int GetPositionInSetCore()
	{
		return -1;
	}

	protected virtual AutomationHeadingLevel GetHeadingLevelCore()
	{
		return AutomationHeadingLevel.None;
	}

	internal virtual Rect GetVisibleBoundingRectCore()
	{
		return GetBoundingRectangle();
	}

	/// <summary>Gets the <see cref="T:System.Windows.Rect" /> object that represents the screen coordinates of the element that is associated with the automation peer.</summary>
	/// <returns>The bounding rectangle.</returns>
	public Rect GetBoundingRectangle()
	{
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			_boundingRectangle = GetBoundingRectangleCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
		return _boundingRectangle;
	}

	/// <summary>Gets a value that indicates whether an element is off the screen.</summary>
	/// <returns>true if the element is not on the screen; otherwise, false.</returns>
	public bool IsOffscreen()
	{
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			_isOffscreen = IsOffscreenCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
		return _isOffscreen;
	}

	/// <summary>Gets a value that indicates the explicit control orientation, if any.</summary>
	/// <returns>The orientation of the control.</returns>
	public AutomationOrientation GetOrientation()
	{
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			return GetOrientationCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
	}

	/// <summary>Gets a string that describes what kind of item an object represents. </summary>
	/// <returns>The kind of item.</returns>
	public string GetItemType()
	{
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			return GetItemTypeCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
	}

	/// <summary>Gets a name that is used with <see cref="T:System.Windows.Automation.Peers.AutomationControlType" />, to differentiate the control that is represented by this <see cref="T:System.Windows.Automation.Peers.AutomationPeer" />. </summary>
	/// <returns>The class name. </returns>
	public string GetClassName()
	{
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			return GetClassNameCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
	}

	/// <summary>Gets text that conveys the visual status of the element that is associated with this automation peer. </summary>
	/// <returns>The status.</returns>
	public string GetItemStatus()
	{
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			_itemStatus = GetItemStatusCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
		return _itemStatus;
	}

	/// <summary>Gets a value that indicates whether the element that is associated with this peer must be completed on a form.</summary>
	/// <returns>true if the element must be completed; otherwise, false.</returns>
	public bool IsRequiredForForm()
	{
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			return IsRequiredForFormCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
	}

	/// <summary>Gets a value that indicates whether the element can accept keyboard focus.</summary>
	/// <returns>true if the element can accept keyboard focus; otherwise, false.</returns>
	public bool IsKeyboardFocusable()
	{
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			return IsKeyboardFocusableCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
	}

	/// <summary>Gets a value that indicates whether the element that is associated with this automation peer currently has keyboard focus.</summary>
	/// <returns>true if the element has keyboard focus; otherwise, false.</returns>
	public bool HasKeyboardFocus()
	{
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			return HasKeyboardFocusCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
	}

	/// <summary>Gets a value that indicates whether the element associated with this automation peer supports interaction.</summary>
	/// <returns>true if the element supports interaction; otherwise, false.</returns>
	public bool IsEnabled()
	{
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			_isEnabled = IsEnabledCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
		return _isEnabled;
	}

	/// <summary>Gets a value that indicates whether the element contains sensitive content.</summary>
	/// <returns>true if the element contains sensitive content such as a password; otherwise, false.</returns>
	public bool IsPassword()
	{
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			return IsPasswordCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
	}

	/// <summary>Gets the <see cref="P:System.Windows.Automation.AutomationProperties.AutomationId" /> of the element that is associated with the automation peer.</summary>
	/// <returns>The identifier.</returns>
	public string GetAutomationId()
	{
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			return GetAutomationIdCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
	}

	/// <summary>Gets text that describes the element that is associated with this automation peer.</summary>
	/// <returns>The name.</returns>
	public string GetName()
	{
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			_name = GetNameCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
		return _name;
	}

	/// <summary>Gets the control type for the element that is associated with the UI Automation peer.</summary>
	/// <returns>The control type.</returns>
	public AutomationControlType GetAutomationControlType()
	{
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			return GetAutomationControlTypeCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
	}

	/// <summary>Gets a human-readable localized string that represents the <see cref="T:System.Windows.Automation.Peers.AutomationControlType" /> value for the control that is associated with this automation peer.</summary>
	/// <returns>The type of the control.</returns>
	public string GetLocalizedControlType()
	{
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			return GetLocalizedControlTypeCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
	}

	/// <summary>Gets a value that indicates whether the element that is associated with this automation peer contains data that is presented to the user.</summary>
	/// <returns>true if the element is a content element; otherwise, false.</returns>
	public bool IsContentElement()
	{
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			return IsControlElementPrivate() && IsContentElementCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
	}

	/// <summary>Gets a value that indicates whether the element is understood by the user as interactive or as contributing to the logical structure of the control in the GUI.</summary>
	/// <returns>true if the element is a control; otherwise, false.</returns>
	public bool IsControlElement()
	{
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			return IsControlElementPrivate();
		}
		finally
		{
			_publicCallInProgress = false;
		}
	}

	private bool IsControlElementPrivate()
	{
		return IsControlElementCore();
	}

	/// <summary>Gets the <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for the <see cref="T:System.Windows.Controls.Label" /> that is targeted to the element. </summary>
	/// <returns>The <see cref="T:System.Windows.Automation.Peers.LabelAutomationPeer" /> for the element that is targeted by the <see cref="T:System.Windows.Controls.Label" />.</returns>
	public AutomationPeer GetLabeledBy()
	{
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			return GetLabeledByCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
	}

	/// <summary>Gets text that describes the functionality of the control that is associated with the automation peer. </summary>
	/// <returns>The help text.</returns>
	public string GetHelpText()
	{
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			return GetHelpTextCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
	}

	/// <summary>Gets the accelerator key combinations for the element that is associated with the UI Automation peer. </summary>
	/// <returns>The accelerator key.</returns>
	public string GetAcceleratorKey()
	{
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			return GetAcceleratorKeyCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
	}

	/// <summary>Gets the access key for the element that is associated with the automation peer.</summary>
	/// <returns>The string that contains the access key.</returns>
	public string GetAccessKey()
	{
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			return GetAccessKeyCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.Point" /> on the element that is associated with the automation peer that responds to a mouse click. </summary>
	/// <returns>A point in the clickable area of the element.</returns>
	public Point GetClickablePoint()
	{
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			if (IsOffscreenCore())
			{
				return new Point(double.NaN, double.NaN);
			}
			return GetClickablePointCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
	}

	/// <summary>Sets the keyboard focus on the element that is associated with this automation peer.</summary>
	public void SetFocus()
	{
		if (_publicSetFocusInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicSetFocusInProgress = true;
			SetFocusCore();
		}
		finally
		{
			_publicSetFocusInProgress = false;
		}
	}

	public AutomationLiveSetting GetLiveSetting()
	{
		AutomationLiveSetting automationLiveSetting = AutomationLiveSetting.Off;
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			return GetLiveSettingCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
	}

	public List<AutomationPeer> GetControlledPeers()
	{
		List<AutomationPeer> list = null;
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			return GetControlledPeersCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
	}

	private IRawElementProviderSimple[] GetControllerForProviderArray()
	{
		List<AutomationPeer> controlledPeers = GetControlledPeers();
		IRawElementProviderSimple[] array = null;
		if (controlledPeers != null)
		{
			array = new IRawElementProviderSimple[controlledPeers.Count];
			for (int i = 0; i < controlledPeers.Count; i++)
			{
				array[i] = ProviderFromPeer(controlledPeers[i]);
			}
		}
		return array;
	}

	public int GetSizeOfSet()
	{
		int num = -1;
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			return GetSizeOfSetCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
	}

	public AutomationHeadingLevel GetHeadingLevel()
	{
		AutomationHeadingLevel automationHeadingLevel = AutomationHeadingLevel.None;
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			return GetHeadingLevelCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
	}

	private static HeadingLevel ConvertHeadingLevelToId(AutomationHeadingLevel value)
	{
		return value switch
		{
			AutomationHeadingLevel.None => HeadingLevel.None, 
			AutomationHeadingLevel.Level1 => HeadingLevel.Level1, 
			AutomationHeadingLevel.Level2 => HeadingLevel.Level2, 
			AutomationHeadingLevel.Level3 => HeadingLevel.Level3, 
			AutomationHeadingLevel.Level4 => HeadingLevel.Level4, 
			AutomationHeadingLevel.Level5 => HeadingLevel.Level5, 
			AutomationHeadingLevel.Level6 => HeadingLevel.Level6, 
			AutomationHeadingLevel.Level7 => HeadingLevel.Level7, 
			AutomationHeadingLevel.Level8 => HeadingLevel.Level8, 
			AutomationHeadingLevel.Level9 => HeadingLevel.Level9, 
			_ => HeadingLevel.None, 
		};
	}

	public bool IsDialog()
	{
		bool flag = false;
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			return IsDialogCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
	}

	public int GetPositionInSet()
	{
		int num = -1;
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			return GetPositionInSetCore();
		}
		finally
		{
			_publicCallInProgress = false;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> that is the parent of this <see cref="T:System.Windows.Automation.Peers.AutomationPeer" />.</summary>
	/// <returns>The parent automation peer.</returns>
	public AutomationPeer GetParent()
	{
		return _parent;
	}

	/// <summary>Gets the collection of <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" /> elements that are represented in the UI Automation tree as immediate child elements of the automation peer.</summary>
	/// <returns>The collection of child elements.</returns>
	public List<AutomationPeer> GetChildren()
	{
		if (_publicCallInProgress)
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
		try
		{
			_publicCallInProgress = true;
			EnsureChildren();
		}
		finally
		{
			_publicCallInProgress = false;
		}
		return _children;
	}

	/// <summary>Synchronously resets the tree of child elements by calling <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildrenCore" />. </summary>
	public void ResetChildrenCache()
	{
		using (UpdateChildren())
		{
		}
	}

	internal int[] GetRuntimeId()
	{
		return new int[3]
		{
			7,
			SafeNativeMethods.GetCurrentProcessId(),
			GetHashCode()
		};
	}

	internal string GetFrameworkId()
	{
		return "WPF";
	}

	internal AutomationPeer GetFirstChild()
	{
		AutomationPeer automationPeer = null;
		EnsureChildren();
		if (_children != null && _children.Count > 0)
		{
			automationPeer = _children[0];
			automationPeer.ChooseIterationParent(this);
		}
		return automationPeer;
	}

	private void EnsureChildren()
	{
		if (_childrenValid && !_ancestorsInvalid)
		{
			return;
		}
		_children = GetChildrenCore();
		if (_children != null)
		{
			int count = _children.Count;
			for (int i = 0; i < count; i++)
			{
				_children[i]._parent = this;
				_children[i]._index = i;
				_children[i]._hwnd = _hwnd;
			}
		}
		_childrenValid = true;
	}

	internal void ForceEnsureChildren()
	{
		_childrenValid = false;
		EnsureChildren();
	}

	internal AutomationPeer GetLastChild()
	{
		AutomationPeer automationPeer = null;
		EnsureChildren();
		if (_children != null && _children.Count > 0)
		{
			automationPeer = _children[_children.Count - 1];
			automationPeer.ChooseIterationParent(this);
		}
		return automationPeer;
	}

	[FriendAccessAllowed]
	internal virtual InteropAutomationProvider GetInteropChild()
	{
		return null;
	}

	internal AutomationPeer GetNextSibling()
	{
		AutomationPeer automationPeer = null;
		AutomationPeer iterationParent = IterationParent;
		if (iterationParent != null)
		{
			iterationParent.EnsureChildren();
			if (iterationParent._children != null && _index >= 0 && _index + 1 < iterationParent._children.Count && iterationParent._children[_index] == this)
			{
				automationPeer = iterationParent._children[_index + 1];
				automationPeer.IterationParent = iterationParent;
			}
		}
		return automationPeer;
	}

	internal AutomationPeer GetPreviousSibling()
	{
		AutomationPeer automationPeer = null;
		AutomationPeer iterationParent = IterationParent;
		if (iterationParent != null)
		{
			iterationParent.EnsureChildren();
			if (iterationParent._children != null && _index - 1 >= 0 && _index < iterationParent._children.Count && iterationParent._children[_index] == this)
			{
				automationPeer = iterationParent._children[_index - 1];
				automationPeer.IterationParent = iterationParent;
			}
		}
		return automationPeer;
	}

	private void ChooseIterationParent(AutomationPeer caller)
	{
		AutomationPeer iterationParent;
		if (_parent == caller || _parent == null)
		{
			iterationParent = _parent;
		}
		else
		{
			_parent.EnsureChildren();
			iterationParent = ((_parent._children == null || _parent._children.Count == caller._children.Count) ? _parent : caller);
		}
		IterationParent = iterationParent;
	}

	internal ControlType GetControlType()
	{
		ControlType result = null;
		switch (GetAutomationControlTypeCore())
		{
		case AutomationControlType.Button:
			result = ControlType.Button;
			break;
		case AutomationControlType.Calendar:
			result = ControlType.Calendar;
			break;
		case AutomationControlType.CheckBox:
			result = ControlType.CheckBox;
			break;
		case AutomationControlType.ComboBox:
			result = ControlType.ComboBox;
			break;
		case AutomationControlType.Edit:
			result = ControlType.Edit;
			break;
		case AutomationControlType.Hyperlink:
			result = ControlType.Hyperlink;
			break;
		case AutomationControlType.Image:
			result = ControlType.Image;
			break;
		case AutomationControlType.ListItem:
			result = ControlType.ListItem;
			break;
		case AutomationControlType.List:
			result = ControlType.List;
			break;
		case AutomationControlType.Menu:
			result = ControlType.Menu;
			break;
		case AutomationControlType.MenuBar:
			result = ControlType.MenuBar;
			break;
		case AutomationControlType.MenuItem:
			result = ControlType.MenuItem;
			break;
		case AutomationControlType.ProgressBar:
			result = ControlType.ProgressBar;
			break;
		case AutomationControlType.RadioButton:
			result = ControlType.RadioButton;
			break;
		case AutomationControlType.ScrollBar:
			result = ControlType.ScrollBar;
			break;
		case AutomationControlType.Slider:
			result = ControlType.Slider;
			break;
		case AutomationControlType.Spinner:
			result = ControlType.Spinner;
			break;
		case AutomationControlType.StatusBar:
			result = ControlType.StatusBar;
			break;
		case AutomationControlType.Tab:
			result = ControlType.Tab;
			break;
		case AutomationControlType.TabItem:
			result = ControlType.TabItem;
			break;
		case AutomationControlType.Text:
			result = ControlType.Text;
			break;
		case AutomationControlType.ToolBar:
			result = ControlType.ToolBar;
			break;
		case AutomationControlType.ToolTip:
			result = ControlType.ToolTip;
			break;
		case AutomationControlType.Tree:
			result = ControlType.Tree;
			break;
		case AutomationControlType.TreeItem:
			result = ControlType.TreeItem;
			break;
		case AutomationControlType.Custom:
			result = ControlType.Custom;
			break;
		case AutomationControlType.Group:
			result = ControlType.Group;
			break;
		case AutomationControlType.Thumb:
			result = ControlType.Thumb;
			break;
		case AutomationControlType.DataGrid:
			result = ControlType.DataGrid;
			break;
		case AutomationControlType.DataItem:
			result = ControlType.DataItem;
			break;
		case AutomationControlType.Document:
			result = ControlType.Document;
			break;
		case AutomationControlType.SplitButton:
			result = ControlType.SplitButton;
			break;
		case AutomationControlType.Window:
			result = ControlType.Window;
			break;
		case AutomationControlType.Pane:
			result = ControlType.Pane;
			break;
		case AutomationControlType.Header:
			result = ControlType.Header;
			break;
		case AutomationControlType.HeaderItem:
			result = ControlType.HeaderItem;
			break;
		case AutomationControlType.Table:
			result = ControlType.Table;
			break;
		case AutomationControlType.TitleBar:
			result = ControlType.TitleBar;
			break;
		case AutomationControlType.Separator:
			result = ControlType.Separator;
			break;
		}
		return result;
	}

	/// <summary>Gets an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> from the specified point.</summary>
	/// <returns>The <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> at the specified point.</returns>
	/// <param name="point">The position on the screen to get the <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> from.</param>
	public AutomationPeer GetPeerFromPoint(Point point)
	{
		return GetPeerFromPointCore(point);
	}

	/// <summary>When overridden in a derived class, is called from <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetPeerFromPoint(System.Windows.Point)" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> at the specified point.</returns>
	/// <param name="point">The position on the screen to get the <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> from.</param>
	protected virtual AutomationPeer GetPeerFromPointCore(Point point)
	{
		AutomationPeer automationPeer = null;
		if (!IsOffscreen())
		{
			List<AutomationPeer> children = GetChildren();
			if (children != null)
			{
				int num = children.Count - 1;
				while (num >= 0 && automationPeer == null)
				{
					automationPeer = children[num].GetPeerFromPoint(point);
					num--;
				}
			}
			if (automationPeer == null && GetVisibleBoundingRect().Contains(point))
			{
				automationPeer = this;
			}
		}
		return automationPeer;
	}

	internal Rect GetVisibleBoundingRect()
	{
		return GetVisibleBoundingRectCore();
	}

	/// <summary>Gets the <see cref="T:System.Windows.Automation.Provider.IRawElementProviderSimple" /> for the specified <see cref="T:System.Windows.Automation.Peers.AutomationPeer" />.</summary>
	/// <returns>The proxy.</returns>
	/// <param name="peer">The automation peer.</param>
	protected internal IRawElementProviderSimple ProviderFromPeer(AutomationPeer peer)
	{
		AutomationPeer referencePeer = this;
		AutomationPeer eventsSource;
		if (peer == this && (eventsSource = EventsSource) != null)
		{
			referencePeer = (peer = eventsSource);
		}
		return ElementProxy.StaticWrap(peer, referencePeer);
	}

	private IRawElementProviderSimple ProviderFromPeerNoDelegation(AutomationPeer peer)
	{
		return ElementProxy.StaticWrap(peer, this);
	}

	/// <summary>Gets an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for the specified <see cref="T:System.Windows.Automation.Provider.IRawElementProviderSimple" /> proxy.</summary>
	/// <returns>An <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for the specified <see cref="T:System.Windows.Automation.Provider.IRawElementProviderSimple" /> proxy.</returns>
	/// <param name="provider">The class that implements <see cref="T:System.Windows.Automation.Provider.IRawElementProviderSimple" />.</param>
	protected AutomationPeer PeerFromProvider(IRawElementProviderSimple provider)
	{
		if (provider is ElementProxy elementProxy)
		{
			return elementProxy.Peer;
		}
		return null;
	}

	internal void FireAutomationEvents()
	{
		UpdateSubtree();
	}

	private void RaisePropertyChangedInternal(IRawElementProviderSimple provider, AutomationProperty propertyId, object oldValue, object newValue)
	{
		if (provider != null && EventMap.HasRegisteredEvent(AutomationEvents.PropertyChanged))
		{
			AutomationPropertyChangedEventArgs e = new AutomationPropertyChangedEventArgs(propertyId, oldValue, newValue);
			AutomationInteropProvider.RaiseAutomationPropertyChangedEvent(provider, e);
		}
	}

	internal void UpdateChildrenInternal(int invalidateLimit)
	{
		List<AutomationPeer> children = _children;
		List<AutomationPeer> list = null;
		HashSet<AutomationPeer> hashSet = null;
		_childrenValid = false;
		EnsureChildren();
		if (!EventMap.HasRegisteredEvent(AutomationEvents.StructureChanged))
		{
			return;
		}
		if (children != null)
		{
			hashSet = new HashSet<AutomationPeer>();
			int count = children.Count;
			for (int i = 0; i < count; i++)
			{
				hashSet.Add(children[i]);
			}
		}
		int num = 0;
		if (_children != null)
		{
			int count2 = _children.Count;
			for (int j = 0; j < count2; j++)
			{
				AutomationPeer item = _children[j];
				if (hashSet != null && hashSet.Contains(item))
				{
					hashSet.Remove(item);
					continue;
				}
				if (list == null)
				{
					list = new List<AutomationPeer>();
				}
				num++;
				if (num <= invalidateLimit)
				{
					list.Add(item);
				}
			}
		}
		int num2 = hashSet?.Count ?? 0;
		if (num2 + num > invalidateLimit)
		{
			StructureChangeType structureChangeType = ((num == 0) ? StructureChangeType.ChildrenBulkRemoved : ((num2 != 0) ? StructureChangeType.ChildrenInvalidated : StructureChangeType.ChildrenBulkAdded));
			IRawElementProviderSimple rawElementProviderSimple = ProviderFromPeerNoDelegation(this);
			if (rawElementProviderSimple != null)
			{
				int[] runtimeId = GetRuntimeId();
				AutomationInteropProvider.RaiseStructureChangedEvent(rawElementProviderSimple, new StructureChangedEventArgs(structureChangeType, runtimeId));
			}
			return;
		}
		if (num2 > 0)
		{
			IRawElementProviderSimple rawElementProviderSimple2 = ProviderFromPeerNoDelegation(this);
			if (rawElementProviderSimple2 != null)
			{
				foreach (AutomationPeer item2 in hashSet)
				{
					int[] runtimeId2 = item2.GetRuntimeId();
					AutomationInteropProvider.RaiseStructureChangedEvent(rawElementProviderSimple2, new StructureChangedEventArgs(StructureChangeType.ChildRemoved, runtimeId2));
				}
			}
		}
		if (num <= 0)
		{
			return;
		}
		foreach (AutomationPeer item3 in list)
		{
			IRawElementProviderSimple rawElementProviderSimple3 = ProviderFromPeerNoDelegation(item3);
			if (rawElementProviderSimple3 != null)
			{
				int[] runtimeId3 = item3.GetRuntimeId();
				AutomationInteropProvider.RaiseStructureChangedEvent(rawElementProviderSimple3, new StructureChangedEventArgs(StructureChangeType.ChildAdded, runtimeId3));
			}
		}
	}

	internal virtual IDisposable UpdateChildren()
	{
		UpdateChildrenInternal(20);
		return null;
	}

	[FriendAccessAllowed]
	internal void UpdateSubtree()
	{
		ContextLayoutManager contextLayoutManager = ContextLayoutManager.From(base.Dispatcher);
		if (contextLayoutManager == null)
		{
			return;
		}
		contextLayoutManager.AutomationSyncUpdateCounter++;
		try
		{
			IRawElementProviderSimple rawElementProviderSimple = null;
			bool flag = EventMap.HasRegisteredEvent(AutomationEvents.PropertyChanged);
			bool flag2 = EventMap.HasRegisteredEvent(AutomationEvents.StructureChanged);
			if (flag)
			{
				string itemStatusCore = GetItemStatusCore();
				if (itemStatusCore != _itemStatus)
				{
					if (rawElementProviderSimple == null)
					{
						rawElementProviderSimple = ProviderFromPeerNoDelegation(this);
					}
					RaisePropertyChangedInternal(rawElementProviderSimple, AutomationElementIdentifiers.ItemStatusProperty, _itemStatus, itemStatusCore);
					_itemStatus = itemStatusCore;
				}
				string nameCore = GetNameCore();
				if (nameCore != _name)
				{
					if (rawElementProviderSimple == null)
					{
						rawElementProviderSimple = ProviderFromPeerNoDelegation(this);
					}
					RaisePropertyChangedInternal(rawElementProviderSimple, AutomationElementIdentifiers.NameProperty, _name, nameCore);
					_name = nameCore;
				}
				bool flag3 = IsOffscreenCore();
				if (flag3 != _isOffscreen)
				{
					if (rawElementProviderSimple == null)
					{
						rawElementProviderSimple = ProviderFromPeerNoDelegation(this);
					}
					RaisePropertyChangedInternal(rawElementProviderSimple, AutomationElementIdentifiers.IsOffscreenProperty, _isOffscreen, flag3);
					_isOffscreen = flag3;
				}
				bool flag4 = IsEnabledCore();
				if (flag4 != _isEnabled)
				{
					if (rawElementProviderSimple == null)
					{
						rawElementProviderSimple = ProviderFromPeerNoDelegation(this);
					}
					RaisePropertyChangedInternal(rawElementProviderSimple, AutomationElementIdentifiers.IsEnabledProperty, _isEnabled, flag4);
					_isEnabled = flag4;
				}
			}
			bool num;
			if (!_childrenValid)
			{
				num = flag2 || flag;
			}
			else
			{
				if (AncestorsInvalid)
				{
					goto IL_0161;
				}
				num = ControlType.Custom == GetControlType();
			}
			if (num)
			{
				goto IL_0161;
			}
			goto IL_019c;
			IL_019c:
			AncestorsInvalid = false;
			_invalidated = false;
			return;
			IL_0161:
			using (UpdateChildren())
			{
				AncestorsInvalid = false;
				for (AutomationPeer automationPeer = GetFirstChild(); automationPeer != null; automationPeer = automationPeer.GetNextSibling())
				{
					automationPeer.UpdateSubtree();
				}
			}
			goto IL_019c;
		}
		finally
		{
			contextLayoutManager.AutomationSyncUpdateCounter--;
		}
	}

	internal void InvalidateAncestorsRecursive()
	{
		if (!AncestorsInvalid)
		{
			AncestorsInvalid = true;
			if (EventsSource != null)
			{
				EventsSource.InvalidateAncestorsRecursive();
			}
			if (_parent != null)
			{
				_parent.InvalidateAncestorsRecursive();
			}
		}
	}

	private static object UpdatePeer(object arg)
	{
		AutomationPeer automationPeer = (AutomationPeer)arg;
		if (!automationPeer.IgnoreUpdatePeer())
		{
			automationPeer.UpdateSubtree();
		}
		return null;
	}

	internal void AddToAutomationEventList()
	{
		if (!_addedToEventList)
		{
			ContextLayoutManager.From(base.Dispatcher).AutomationEvents.Add(this);
			_addedToEventList = true;
		}
	}

	internal object GetWrappedPattern(int patternId)
	{
		object result = null;
		PatternInfo patternInfo = (PatternInfo)s_patternInfo[patternId];
		if (patternInfo != null)
		{
			object pattern = GetPattern(patternInfo.PatternInterface);
			if (pattern != null)
			{
				result = patternInfo.WrapObject(this, pattern);
			}
		}
		return result;
	}

	internal object GetPropertyValue(int propertyId)
	{
		object obj = null;
		GetProperty getProperty = (GetProperty)s_propertyInfo[propertyId];
		if (getProperty != null)
		{
			obj = getProperty(this);
			if (AutomationElementIdentifiers.HeadingLevelProperty != null && propertyId == AutomationElementIdentifiers.HeadingLevelProperty.Id)
			{
				obj = ConvertHeadingLevelToId((AutomationHeadingLevel)obj);
			}
		}
		return obj;
	}

	private static void Initialize()
	{
		s_patternInfo = new Hashtable();
		s_patternInfo[InvokePatternIdentifiers.Pattern.Id] = new PatternInfo(InvokePatternIdentifiers.Pattern.Id, InvokeProviderWrapper.Wrap, PatternInterface.Invoke);
		s_patternInfo[SelectionPatternIdentifiers.Pattern.Id] = new PatternInfo(SelectionPatternIdentifiers.Pattern.Id, SelectionProviderWrapper.Wrap, PatternInterface.Selection);
		s_patternInfo[ValuePatternIdentifiers.Pattern.Id] = new PatternInfo(ValuePatternIdentifiers.Pattern.Id, ValueProviderWrapper.Wrap, PatternInterface.Value);
		s_patternInfo[RangeValuePatternIdentifiers.Pattern.Id] = new PatternInfo(RangeValuePatternIdentifiers.Pattern.Id, RangeValueProviderWrapper.Wrap, PatternInterface.RangeValue);
		s_patternInfo[ScrollPatternIdentifiers.Pattern.Id] = new PatternInfo(ScrollPatternIdentifiers.Pattern.Id, ScrollProviderWrapper.Wrap, PatternInterface.Scroll);
		s_patternInfo[ScrollItemPatternIdentifiers.Pattern.Id] = new PatternInfo(ScrollItemPatternIdentifiers.Pattern.Id, ScrollItemProviderWrapper.Wrap, PatternInterface.ScrollItem);
		s_patternInfo[ExpandCollapsePatternIdentifiers.Pattern.Id] = new PatternInfo(ExpandCollapsePatternIdentifiers.Pattern.Id, ExpandCollapseProviderWrapper.Wrap, PatternInterface.ExpandCollapse);
		s_patternInfo[GridPatternIdentifiers.Pattern.Id] = new PatternInfo(GridPatternIdentifiers.Pattern.Id, GridProviderWrapper.Wrap, PatternInterface.Grid);
		s_patternInfo[GridItemPatternIdentifiers.Pattern.Id] = new PatternInfo(GridItemPatternIdentifiers.Pattern.Id, GridItemProviderWrapper.Wrap, PatternInterface.GridItem);
		s_patternInfo[MultipleViewPatternIdentifiers.Pattern.Id] = new PatternInfo(MultipleViewPatternIdentifiers.Pattern.Id, MultipleViewProviderWrapper.Wrap, PatternInterface.MultipleView);
		s_patternInfo[WindowPatternIdentifiers.Pattern.Id] = new PatternInfo(WindowPatternIdentifiers.Pattern.Id, WindowProviderWrapper.Wrap, PatternInterface.Window);
		s_patternInfo[SelectionItemPatternIdentifiers.Pattern.Id] = new PatternInfo(SelectionItemPatternIdentifiers.Pattern.Id, SelectionItemProviderWrapper.Wrap, PatternInterface.SelectionItem);
		s_patternInfo[DockPatternIdentifiers.Pattern.Id] = new PatternInfo(DockPatternIdentifiers.Pattern.Id, DockProviderWrapper.Wrap, PatternInterface.Dock);
		s_patternInfo[TablePatternIdentifiers.Pattern.Id] = new PatternInfo(TablePatternIdentifiers.Pattern.Id, TableProviderWrapper.Wrap, PatternInterface.Table);
		s_patternInfo[TableItemPatternIdentifiers.Pattern.Id] = new PatternInfo(TableItemPatternIdentifiers.Pattern.Id, TableItemProviderWrapper.Wrap, PatternInterface.TableItem);
		s_patternInfo[TogglePatternIdentifiers.Pattern.Id] = new PatternInfo(TogglePatternIdentifiers.Pattern.Id, ToggleProviderWrapper.Wrap, PatternInterface.Toggle);
		s_patternInfo[TransformPatternIdentifiers.Pattern.Id] = new PatternInfo(TransformPatternIdentifiers.Pattern.Id, TransformProviderWrapper.Wrap, PatternInterface.Transform);
		s_patternInfo[TextPatternIdentifiers.Pattern.Id] = new PatternInfo(TextPatternIdentifiers.Pattern.Id, TextProviderWrapper.Wrap, PatternInterface.Text);
		if (VirtualizedItemPatternIdentifiers.Pattern != null)
		{
			s_patternInfo[VirtualizedItemPatternIdentifiers.Pattern.Id] = new PatternInfo(VirtualizedItemPatternIdentifiers.Pattern.Id, VirtualizedItemProviderWrapper.Wrap, PatternInterface.VirtualizedItem);
		}
		if (ItemContainerPatternIdentifiers.Pattern != null)
		{
			s_patternInfo[ItemContainerPatternIdentifiers.Pattern.Id] = new PatternInfo(ItemContainerPatternIdentifiers.Pattern.Id, ItemContainerProviderWrapper.Wrap, PatternInterface.ItemContainer);
		}
		if (SynchronizedInputPatternIdentifiers.Pattern != null)
		{
			s_patternInfo[SynchronizedInputPatternIdentifiers.Pattern.Id] = new PatternInfo(SynchronizedInputPatternIdentifiers.Pattern.Id, SynchronizedInputProviderWrapper.Wrap, PatternInterface.SynchronizedInput);
		}
		s_propertyInfo = new Hashtable();
		s_propertyInfo[AutomationElementIdentifiers.IsControlElementProperty.Id] = new GetProperty(IsControlElement);
		s_propertyInfo[AutomationElementIdentifiers.ControlTypeProperty.Id] = new GetProperty(GetControlType);
		s_propertyInfo[AutomationElementIdentifiers.IsContentElementProperty.Id] = new GetProperty(IsContentElement);
		s_propertyInfo[AutomationElementIdentifiers.LabeledByProperty.Id] = new GetProperty(GetLabeledBy);
		s_propertyInfo[AutomationElementIdentifiers.NativeWindowHandleProperty.Id] = new GetProperty(GetNativeWindowHandle);
		s_propertyInfo[AutomationElementIdentifiers.AutomationIdProperty.Id] = new GetProperty(GetAutomationId);
		s_propertyInfo[AutomationElementIdentifiers.ItemTypeProperty.Id] = new GetProperty(GetItemType);
		s_propertyInfo[AutomationElementIdentifiers.IsPasswordProperty.Id] = new GetProperty(IsPassword);
		s_propertyInfo[AutomationElementIdentifiers.LocalizedControlTypeProperty.Id] = new GetProperty(GetLocalizedControlType);
		s_propertyInfo[AutomationElementIdentifiers.NameProperty.Id] = new GetProperty(GetName);
		s_propertyInfo[AutomationElementIdentifiers.AcceleratorKeyProperty.Id] = new GetProperty(GetAcceleratorKey);
		s_propertyInfo[AutomationElementIdentifiers.AccessKeyProperty.Id] = new GetProperty(GetAccessKey);
		s_propertyInfo[AutomationElementIdentifiers.HasKeyboardFocusProperty.Id] = new GetProperty(HasKeyboardFocus);
		s_propertyInfo[AutomationElementIdentifiers.IsKeyboardFocusableProperty.Id] = new GetProperty(IsKeyboardFocusable);
		s_propertyInfo[AutomationElementIdentifiers.IsEnabledProperty.Id] = new GetProperty(IsEnabled);
		s_propertyInfo[AutomationElementIdentifiers.BoundingRectangleProperty.Id] = new GetProperty(GetBoundingRectangle);
		s_propertyInfo[AutomationElementIdentifiers.ProcessIdProperty.Id] = new GetProperty(GetCurrentProcessId);
		s_propertyInfo[AutomationElementIdentifiers.RuntimeIdProperty.Id] = new GetProperty(GetRuntimeId);
		s_propertyInfo[AutomationElementIdentifiers.ClassNameProperty.Id] = new GetProperty(GetClassName);
		s_propertyInfo[AutomationElementIdentifiers.HelpTextProperty.Id] = new GetProperty(GetHelpText);
		s_propertyInfo[AutomationElementIdentifiers.ClickablePointProperty.Id] = new GetProperty(GetClickablePoint);
		s_propertyInfo[AutomationElementIdentifiers.CultureProperty.Id] = new GetProperty(GetCultureInfo);
		s_propertyInfo[AutomationElementIdentifiers.IsOffscreenProperty.Id] = new GetProperty(IsOffscreen);
		s_propertyInfo[AutomationElementIdentifiers.OrientationProperty.Id] = new GetProperty(GetOrientation);
		s_propertyInfo[AutomationElementIdentifiers.FrameworkIdProperty.Id] = new GetProperty(GetFrameworkId);
		s_propertyInfo[AutomationElementIdentifiers.IsRequiredForFormProperty.Id] = new GetProperty(IsRequiredForForm);
		s_propertyInfo[AutomationElementIdentifiers.ItemStatusProperty.Id] = new GetProperty(GetItemStatus);
		if (!AccessibilitySwitches.UseNetFx47CompatibleAccessibilityFeatures && AutomationElementIdentifiers.LiveSettingProperty != null)
		{
			s_propertyInfo[AutomationElementIdentifiers.LiveSettingProperty.Id] = new GetProperty(GetLiveSetting);
		}
		if (!AccessibilitySwitches.UseNetFx472CompatibleAccessibilityFeatures && AutomationElementIdentifiers.ControllerForProperty != null)
		{
			s_propertyInfo[AutomationElementIdentifiers.ControllerForProperty.Id] = new GetProperty(GetControllerFor);
		}
		if (!AccessibilitySwitches.UseNetFx472CompatibleAccessibilityFeatures && AutomationElementIdentifiers.SizeOfSetProperty != null)
		{
			s_propertyInfo[AutomationElementIdentifiers.SizeOfSetProperty.Id] = new GetProperty(GetSizeOfSet);
		}
		if (!AccessibilitySwitches.UseNetFx472CompatibleAccessibilityFeatures && AutomationElementIdentifiers.PositionInSetProperty != null)
		{
			s_propertyInfo[AutomationElementIdentifiers.PositionInSetProperty.Id] = new GetProperty(GetPositionInSet);
		}
		if (AutomationElementIdentifiers.HeadingLevelProperty != null)
		{
			s_propertyInfo[AutomationElementIdentifiers.HeadingLevelProperty.Id] = new GetProperty(GetHeadingLevel);
		}
		if (AutomationElementIdentifiers.IsDialogProperty != null)
		{
			s_propertyInfo[AutomationElementIdentifiers.IsDialogProperty.Id] = new GetProperty(IsDialog);
		}
	}

	private static object IsControlElement(AutomationPeer peer)
	{
		return peer.IsControlElement();
	}

	private static object GetControlType(AutomationPeer peer)
	{
		return peer.GetControlType().Id;
	}

	private static object IsContentElement(AutomationPeer peer)
	{
		return peer.IsContentElement();
	}

	private static object GetLabeledBy(AutomationPeer peer)
	{
		return ElementProxy.StaticWrap(peer.GetLabeledBy(), peer);
	}

	private static object GetNativeWindowHandle(AutomationPeer peer)
	{
		return null;
	}

	private static object GetAutomationId(AutomationPeer peer)
	{
		return peer.GetAutomationId();
	}

	private static object GetItemType(AutomationPeer peer)
	{
		return peer.GetItemType();
	}

	private static object IsPassword(AutomationPeer peer)
	{
		return peer.IsPassword();
	}

	private static object GetLocalizedControlType(AutomationPeer peer)
	{
		return peer.GetLocalizedControlType();
	}

	private static object GetName(AutomationPeer peer)
	{
		return peer.GetName();
	}

	private static object GetAcceleratorKey(AutomationPeer peer)
	{
		return peer.GetAcceleratorKey();
	}

	private static object GetAccessKey(AutomationPeer peer)
	{
		return peer.GetAccessKey();
	}

	private static object HasKeyboardFocus(AutomationPeer peer)
	{
		return peer.HasKeyboardFocus();
	}

	private static object IsKeyboardFocusable(AutomationPeer peer)
	{
		return peer.IsKeyboardFocusable();
	}

	private static object IsEnabled(AutomationPeer peer)
	{
		return peer.IsEnabled();
	}

	private static object GetBoundingRectangle(AutomationPeer peer)
	{
		return peer.GetBoundingRectangle();
	}

	private static object GetCurrentProcessId(AutomationPeer peer)
	{
		return SafeNativeMethods.GetCurrentProcessId();
	}

	private static object GetRuntimeId(AutomationPeer peer)
	{
		return peer.GetRuntimeId();
	}

	private static object GetClassName(AutomationPeer peer)
	{
		return peer.GetClassName();
	}

	private static object GetHelpText(AutomationPeer peer)
	{
		return peer.GetHelpText();
	}

	private static object GetClickablePoint(AutomationPeer peer)
	{
		Point clickablePoint = peer.GetClickablePoint();
		return new double[2] { clickablePoint.X, clickablePoint.Y };
	}

	private static object GetCultureInfo(AutomationPeer peer)
	{
		return null;
	}

	private static object IsOffscreen(AutomationPeer peer)
	{
		return peer.IsOffscreen();
	}

	private static object GetOrientation(AutomationPeer peer)
	{
		return peer.GetOrientation();
	}

	private static object GetFrameworkId(AutomationPeer peer)
	{
		return peer.GetFrameworkId();
	}

	private static object IsRequiredForForm(AutomationPeer peer)
	{
		return peer.IsRequiredForForm();
	}

	private static object GetItemStatus(AutomationPeer peer)
	{
		return peer.GetItemStatus();
	}

	private static object GetLiveSetting(AutomationPeer peer)
	{
		return peer.GetLiveSetting();
	}

	private static object GetControllerFor(AutomationPeer peer)
	{
		return peer.GetControllerForProviderArray();
	}

	private static object GetSizeOfSet(AutomationPeer peer)
	{
		return peer.GetSizeOfSet();
	}

	private static object GetPositionInSet(AutomationPeer peer)
	{
		return peer.GetPositionInSet();
	}

	private static object GetHeadingLevel(AutomationPeer peer)
	{
		return peer.GetHeadingLevel();
	}

	private static object IsDialog(AutomationPeer peer)
	{
		return peer.IsDialog();
	}

	/// <summary>Provides initialization for base class values when they are called by the constructor of a derived class.</summary>
	protected AutomationPeer()
	{
	}
}
