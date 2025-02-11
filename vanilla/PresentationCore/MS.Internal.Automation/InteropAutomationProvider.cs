using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using MS.Internal.PresentationCore;

namespace MS.Internal.Automation;

[FriendAccessAllowed]
internal class InteropAutomationProvider : IRawElementProviderFragmentRoot, IRawElementProviderFragment, IRawElementProviderSimple
{
	private HostedWindowWrapper _wrapper;

	private AutomationPeer _parent;

	ProviderOptions IRawElementProviderSimple.ProviderOptions => ProviderOptions.ServerSideProvider | ProviderOptions.OverrideProvider;

	IRawElementProviderSimple IRawElementProviderSimple.HostRawElementProvider => AutomationInteropProvider.HostProviderFromHandle(_wrapper.Handle);

	Rect IRawElementProviderFragment.BoundingRectangle => Rect.Empty;

	IRawElementProviderFragmentRoot IRawElementProviderFragment.FragmentRoot => null;

	internal InteropAutomationProvider(HostedWindowWrapper wrapper, AutomationPeer parent)
	{
		if (wrapper == null)
		{
			throw new ArgumentNullException("wrapper");
		}
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		_wrapper = wrapper;
		_parent = parent;
	}

	object IRawElementProviderSimple.GetPatternProvider(int patternId)
	{
		return null;
	}

	object IRawElementProviderSimple.GetPropertyValue(int propertyId)
	{
		return null;
	}

	IRawElementProviderFragment IRawElementProviderFragment.Navigate(NavigateDirection direction)
	{
		if (direction == NavigateDirection.Parent)
		{
			return (IRawElementProviderFragment)_parent.ProviderFromPeer(_parent);
		}
		return null;
	}

	int[] IRawElementProviderFragment.GetRuntimeId()
	{
		return null;
	}

	IRawElementProviderSimple[] IRawElementProviderFragment.GetEmbeddedFragmentRoots()
	{
		return null;
	}

	void IRawElementProviderFragment.SetFocus()
	{
		throw new NotSupportedException();
	}

	IRawElementProviderFragment IRawElementProviderFragmentRoot.ElementProviderFromPoint(double x, double y)
	{
		return null;
	}

	IRawElementProviderFragment IRawElementProviderFragmentRoot.GetFocus()
	{
		return null;
	}
}
