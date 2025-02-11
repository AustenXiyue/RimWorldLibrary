using System.Windows.Automation.Provider;
using System.Windows.Documents;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Documents.Hyperlink" /> types to UI Automation.</summary>
public class HyperlinkAutomationPeer : TextElementAutomationPeer, IInvokeProvider
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.HyperlinkAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Documents.Hyperlink" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.HyperlinkAutomationPeer" />.</param>
	public HyperlinkAutomationPeer(Hyperlink owner)
		: base(owner)
	{
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Documents.Hyperlink" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.HyperlinkAutomationPeer" />.</summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.Invoke" />, this method returns a this pointer; otherwise, this method returns null.</returns>
	/// <param name="patternInterface">A value in the enumeration.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		if (patternInterface == PatternInterface.Invoke)
		{
			return this;
		}
		return base.GetPattern(patternInterface);
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Documents.Hyperlink" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.HyperlinkAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Hyperlink" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Hyperlink;
	}

	/// <summary>Gets the text label of the <see cref="T:System.Windows.Documents.Hyperlink" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.HyperlinkAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetName" />.</summary>
	/// <returns>The string that contains the label.</returns>
	protected override string GetNameCore()
	{
		string text = base.GetNameCore();
		if (text.Length == 0)
		{
			Hyperlink hyperlink = (Hyperlink)base.Owner;
			if (hyperlink.Text != null)
			{
				text = hyperlink.Text;
			}
		}
		return text;
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Documents.Hyperlink" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.HyperlinkAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "Hyperlink".</returns>
	protected override string GetClassNameCore()
	{
		return "Hyperlink";
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Documents.Hyperlink" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.HyperlinkAutomationPeer" /> is understood by the end user as interactive the user might understand the <see cref="T:System.Windows.Documents.Hyperlink" /> as contributing to the logical structure of the control in the GUI. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsControlElement" />.</summary>
	/// <returns>true.</returns>
	protected override bool IsControlElementCore()
	{
		if (!base.IncludeInvisibleElementsInControlView)
		{
			return base.IsTextViewVisible == true;
		}
		return true;
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void IInvokeProvider.Invoke()
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		((Hyperlink)base.Owner).DoClick();
	}
}
