using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Automation.Provider;
using System.Windows.Documents;
using MS.Internal.Automation;

namespace System.Windows.Automation.Peers;

/// <summary>Represents a base class for exposing <see cref="T:System.Windows.Automation.TextPattern" /> types to UI Automation.</summary>
public abstract class ContentTextAutomationPeer : FrameworkContentElementAutomationPeer
{
	/// <summary>Provides initialization for base class values when called by the constructor of a derived class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Automation.TextPattern" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentTextAutomationPeer" />.</param>
	protected ContentTextAutomationPeer(FrameworkContentElement owner)
		: base(owner)
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public virtual void RaiseActiveTextPositionChangedEvent(TextPointer rangeStart, TextPointer rangeEnd)
	{
		if (EventMap.HasRegisteredEvent(AutomationEvents.ActiveTextPositionChanged))
		{
			IRawElementProviderSimple rawElementProviderSimple = ProviderFromPeer(this);
			if (rawElementProviderSimple != null)
			{
				ActiveTextPositionChangedEventArgs e = new ActiveTextPositionChangedEventArgs(TextRangeFromTextPointers(rangeStart, rangeEnd));
				AutomationInteropProvider.RaiseAutomationEvent(AutomationElementIdentifiers.ActiveTextPositionChangedEvent, rawElementProviderSimple, e);
			}
		}
	}

	private ITextRangeProvider TextRangeFromTextPointers(TextPointer rangeStart, TextPointer rangeEnd)
	{
		return (GetPattern(PatternInterface.Text) as TextAdaptor)?.TextRangeFromTextPointers(rangeStart, rangeEnd);
	}

	internal new IRawElementProviderSimple ProviderFromPeer(AutomationPeer peer)
	{
		return base.ProviderFromPeer(peer);
	}

	internal DependencyObject ElementFromProvider(IRawElementProviderSimple provider)
	{
		DependencyObject result = null;
		AutomationPeer automationPeer = PeerFromProvider(provider);
		if (automationPeer is UIElementAutomationPeer)
		{
			result = ((UIElementAutomationPeer)automationPeer).Owner;
		}
		else if (automationPeer is ContentElementAutomationPeer)
		{
			result = ((ContentElementAutomationPeer)automationPeer).Owner;
		}
		return result;
	}

	internal abstract List<AutomationPeer> GetAutomationPeersFromRange(ITextPointer start, ITextPointer end);
}
