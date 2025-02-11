using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.DocumentViewer" /> types to UI Automation.</summary>
public class DocumentViewerAutomationPeer : DocumentViewerBaseAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.DocumentViewerAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.DocumentViewer" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.DocumentViewerAutomationPeer" />.</param>
	public DocumentViewerAutomationPeer(DocumentViewer owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.DocumentViewer" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.DocumentViewerAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "DocumentViewer".</returns>
	protected override string GetClassNameCore()
	{
		return "DocumentViewer";
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Controls.DocumentViewer" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.DocumentViewerAutomationPeer" />. </summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.ScrollItem" />, this method returns the <see cref="T:System.Windows.Automation.Peers.ScrollViewerAutomationPeer" /> for this <see cref="T:System.Windows.Automation.Peers.DocumentViewerAutomationPeer" />. This method also sets <see cref="T:System.Windows.Automation.Peers.DocumentViewerAutomationPeer" /> as the <see cref="P:System.Windows.Automation.Peers.AutomationPeer.EventsSource" />. </returns>
	/// <param name="patternInterface">One of the enumeration values.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		object result = null;
		if (patternInterface == PatternInterface.Scroll)
		{
			DocumentViewer documentViewer = (DocumentViewer)base.Owner;
			if (documentViewer.ScrollViewer != null)
			{
				AutomationPeer automationPeer = UIElementAutomationPeer.CreatePeerForElement(documentViewer.ScrollViewer);
				if (automationPeer != null && automationPeer is IScrollProvider)
				{
					automationPeer.EventsSource = this;
					result = automationPeer;
				}
			}
		}
		else
		{
			result = base.GetPattern(patternInterface);
		}
		return result;
	}
}
