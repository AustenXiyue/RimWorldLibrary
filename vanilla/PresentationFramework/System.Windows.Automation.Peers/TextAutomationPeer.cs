using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Automation.Provider;
using System.Windows.Documents;
using MS.Internal.Automation;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Automation.TextPattern" /> types to UI Automation.</summary>
public abstract class TextAutomationPeer : FrameworkElementAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.TextAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Automation.TextPattern" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TextAutomationPeer" />.</param>
	protected TextAutomationPeer(FrameworkElement owner)
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

	/// <summary>Gets the text label of the element that is associated with this <see cref="T:System.Windows.Automation.Peers.TextAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetName" />.</summary>
	/// <returns>The value of <see cref="P:System.Windows.Automation.AutomationProperties.Name" /> or <see cref="P:System.Windows.Automation.AutomationProperties.LabeledBy" /> if either is set; otherwise this method returns an empty string. </returns>
	protected override string GetNameCore()
	{
		string name = AutomationProperties.GetName(base.Owner);
		if (string.IsNullOrEmpty(name))
		{
			AutomationPeer labeledByCore = GetLabeledByCore();
			if (labeledByCore != null)
			{
				name = labeledByCore.GetName();
			}
		}
		return name ?? string.Empty;
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
