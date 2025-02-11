using System.Windows.Automation.Provider;
using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.Primitives.RepeatButton" /> types to UI Automation.</summary>
public class RepeatButtonAutomationPeer : ButtonBaseAutomationPeer, IInvokeProvider
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.RepeatButtonAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.Primitives.RepeatButton" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.RepeatButtonAutomationPeer" />.</param>
	public RepeatButtonAutomationPeer(RepeatButton owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "RepeatButton".</returns>
	protected override string GetClassNameCore()
	{
		return "RepeatButton";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.Primitives.RepeatButton" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.RepeatButtonAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Button" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Button;
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />.</summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.Invoke" />, this method returns a reference to the current instance of the <see cref="T:System.Windows.Automation.Peers.RepeatButtonAutomationPeer" />; otherwise, null.</returns>
	/// <param name="patternInterface">A value in the enumeration.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		if (patternInterface == PatternInterface.Invoke)
		{
			return this;
		}
		return base.GetPattern(patternInterface);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void IInvokeProvider.Invoke()
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		((RepeatButton)base.Owner).AutomationButtonBaseClick();
	}
}
