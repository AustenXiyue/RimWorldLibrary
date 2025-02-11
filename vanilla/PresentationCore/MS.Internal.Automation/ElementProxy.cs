using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Input;

namespace MS.Internal.Automation;

internal class ElementProxy : IRawElementProviderFragmentRoot, IRawElementProviderFragment, IRawElementProviderSimple, IRawElementProviderAdviseEvents
{
	internal enum ReferenceType
	{
		Strong,
		Weak
	}

	private static ReferenceType _automationInteropReferenceType = ReferenceType.Weak;

	private static bool _shouldCheckInTheRegistry = true;

	private readonly object _peer;

	public ProviderOptions ProviderOptions
	{
		get
		{
			AutomationPeer peer = Peer;
			if (peer == null)
			{
				return ProviderOptions.ServerSideProvider;
			}
			return (ProviderOptions)ElementUtil.Invoke(peer, (object state) => ((ElementProxy)state).InContextGetProviderOptions(), this);
		}
	}

	public IRawElementProviderSimple HostRawElementProvider
	{
		get
		{
			IRawElementProviderSimple result = null;
			HostedWindowWrapper hostedWindowWrapper = null;
			AutomationPeer peer = Peer;
			if (peer == null)
			{
				return null;
			}
			hostedWindowWrapper = (HostedWindowWrapper)ElementUtil.Invoke(peer, InContextGetHostRawElementProvider, null);
			if (hostedWindowWrapper != null)
			{
				result = GetHostHelper(hostedWindowWrapper);
			}
			return result;
		}
	}

	public Rect BoundingRectangle => (Rect)ElementUtil.Invoke(Peer ?? throw new ElementNotAvailableException(), (object state) => ((ElementProxy)state).InContextBoundingRectangle(), this);

	public IRawElementProviderFragmentRoot FragmentRoot
	{
		get
		{
			AutomationPeer peer = Peer;
			if (peer == null)
			{
				return null;
			}
			return (IRawElementProviderFragmentRoot)ElementUtil.Invoke(peer, (object state) => ((ElementProxy)state).InContextFragmentRoot(), this);
		}
	}

	internal AutomationPeer Peer
	{
		get
		{
			if (_peer is WeakReference)
			{
				return (AutomationPeer)((WeakReference)_peer).Target;
			}
			return (AutomationPeer)_peer;
		}
	}

	internal static ReferenceType AutomationInteropReferenceType
	{
		get
		{
			if (_shouldCheckInTheRegistry)
			{
				if (RegistryKeys.ReadLocalMachineBool("Software\\Microsoft\\.NETFramework\\Windows Presentation Foundation\\Features", "AutomationWeakReferenceDisallow"))
				{
					_automationInteropReferenceType = ReferenceType.Strong;
				}
				_shouldCheckInTheRegistry = false;
			}
			return _automationInteropReferenceType;
		}
	}

	private ElementProxy(AutomationPeer peer)
	{
		if (AutomationInteropReferenceType == ReferenceType.Weak && (peer is UIElementAutomationPeer || peer is ContentElementAutomationPeer || peer is UIElement3DAutomationPeer))
		{
			_peer = new WeakReference(peer);
		}
		else
		{
			_peer = peer;
		}
	}

	public object GetPatternProvider(int pattern)
	{
		return ElementUtil.Invoke(Peer ?? throw new ElementNotAvailableException(), InContextGetPatternProvider, pattern);
	}

	public object GetPropertyValue(int property)
	{
		return ElementUtil.Invoke(Peer ?? throw new ElementNotAvailableException(), InContextGetPropertyValue, property);
	}

	private IRawElementProviderSimple GetHostHelper(HostedWindowWrapper hwndWrapper)
	{
		return AutomationInteropProvider.HostProviderFromHandle(hwndWrapper.Handle);
	}

	public IRawElementProviderFragment Navigate(NavigateDirection direction)
	{
		AutomationPeer peer = Peer;
		if (peer == null)
		{
			return null;
		}
		return (IRawElementProviderFragment)ElementUtil.Invoke(peer, InContextNavigate, direction);
	}

	public int[] GetRuntimeId()
	{
		return (int[])ElementUtil.Invoke(Peer ?? throw new ElementNotAvailableException(), (object state) => ((ElementProxy)state).InContextGetRuntimeId(), this);
	}

	public IRawElementProviderSimple[] GetEmbeddedFragmentRoots()
	{
		return null;
	}

	public void SetFocus()
	{
		ElementUtil.Invoke(Peer ?? throw new ElementNotAvailableException(), (object state) => ((ElementProxy)state).InContextSetFocus(), this);
	}

	public IRawElementProviderFragment ElementProviderFromPoint(double x, double y)
	{
		AutomationPeer peer = Peer;
		if (peer == null)
		{
			return null;
		}
		return (IRawElementProviderFragment)ElementUtil.Invoke(peer, InContextElementProviderFromPoint, new Point(x, y));
	}

	public IRawElementProviderFragment GetFocus()
	{
		AutomationPeer peer = Peer;
		if (peer == null)
		{
			return null;
		}
		return (IRawElementProviderFragment)ElementUtil.Invoke(peer, (object state) => ((ElementProxy)state).InContextGetFocus(), this);
	}

	public void AdviseEventAdded(int eventID, int[] properties)
	{
		EventMap.AddEvent(eventID);
	}

	public void AdviseEventRemoved(int eventID, int[] properties)
	{
		EventMap.RemoveEvent(eventID);
	}

	internal static ElementProxy StaticWrap(AutomationPeer peer, AutomationPeer referencePeer)
	{
		ElementProxy elementProxy = null;
		if (peer != null)
		{
			peer = peer.ValidateConnected(referencePeer);
			if (peer != null)
			{
				if (peer.ElementProxyWeakReference != null)
				{
					elementProxy = peer.ElementProxyWeakReference.Target as ElementProxy;
				}
				if (elementProxy == null)
				{
					elementProxy = new ElementProxy(peer);
					peer.ElementProxyWeakReference = new WeakReference(elementProxy);
				}
				if (elementProxy != null && peer.IsDataItemAutomationPeer())
				{
					peer.AddToParentProxyWeakRefCache();
				}
			}
		}
		return elementProxy;
	}

	private object InContextElementProviderFromPoint(object arg)
	{
		Point point = (Point)arg;
		AutomationPeer peer = Peer;
		if (peer == null)
		{
			return null;
		}
		return StaticWrap(peer.GetPeerFromPoint(point), peer);
	}

	private object InContextGetFocus()
	{
		AutomationPeer peer = Peer;
		if (peer == null)
		{
			return null;
		}
		return StaticWrap(AutomationPeer.AutomationPeerFromInputElement(Keyboard.FocusedElement), peer);
	}

	private object InContextGetPatternProvider(object arg)
	{
		return (Peer ?? throw new ElementNotAvailableException()).GetWrappedPattern((int)arg);
	}

	private object InContextNavigate(object arg)
	{
		NavigateDirection navigateDirection = (NavigateDirection)arg;
		AutomationPeer peer = Peer;
		if (peer == null)
		{
			return null;
		}
		AutomationPeer peer2;
		switch (navigateDirection)
		{
		case NavigateDirection.Parent:
			peer2 = peer.GetParent();
			break;
		case NavigateDirection.FirstChild:
			if (!peer.IsInteropPeer)
			{
				peer2 = peer.GetFirstChild();
				break;
			}
			return peer.GetInteropChild();
		case NavigateDirection.LastChild:
			if (!peer.IsInteropPeer)
			{
				peer2 = peer.GetLastChild();
				break;
			}
			return peer.GetInteropChild();
		case NavigateDirection.NextSibling:
			peer2 = peer.GetNextSibling();
			break;
		case NavigateDirection.PreviousSibling:
			peer2 = peer.GetPreviousSibling();
			break;
		default:
			peer2 = null;
			break;
		}
		return StaticWrap(peer2, peer);
	}

	private object InContextGetProviderOptions()
	{
		ProviderOptions providerOptions = ProviderOptions.ServerSideProvider;
		AutomationPeer peer = Peer;
		if (peer == null)
		{
			return providerOptions;
		}
		if (peer.IsHwndHost)
		{
			providerOptions |= ProviderOptions.OverrideProvider;
		}
		return providerOptions;
	}

	private object InContextGetPropertyValue(object arg)
	{
		return (Peer ?? throw new ElementNotAvailableException()).GetPropertyValue((int)arg);
	}

	private object InContextGetHostRawElementProvider(object unused)
	{
		return Peer?.GetHostRawElementProvider();
	}

	private object InContextGetRuntimeId()
	{
		return (Peer ?? throw new ElementNotAvailableException()).GetRuntimeId();
	}

	private object InContextBoundingRectangle()
	{
		return (Peer ?? throw new ElementNotAvailableException()).GetBoundingRectangle();
	}

	private object InContextSetFocus()
	{
		(Peer ?? throw new ElementNotAvailableException()).SetFocus();
		return null;
	}

	private object InContextFragmentRoot()
	{
		AutomationPeer peer = Peer;
		AutomationPeer automationPeer = peer;
		if (automationPeer == null)
		{
			return null;
		}
		while (true)
		{
			AutomationPeer parent = automationPeer.GetParent();
			if (parent == null)
			{
				break;
			}
			automationPeer = parent;
		}
		return StaticWrap(automationPeer, peer);
	}
}
