using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Documents;
using MS.Internal.Documents;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" /> types to UI Automation.</summary>
public class FlowDocumentScrollViewerAutomationPeer : FrameworkElementAutomationPeer
{
	private DocumentAutomationPeer _documentPeer;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.FlowDocumentScrollViewerAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.FlowDocumentScrollViewerAutomationPeer" />.</param>
	public FlowDocumentScrollViewerAutomationPeer(FlowDocumentScrollViewer owner)
		: base(owner)
	{
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.FlowDocumentScrollViewerAutomationPeer" />. </summary>
	/// <returns>An object that supports the control pattern if <paramref name="patternInterface" /> is a supported value; otherwise, null. </returns>
	/// <param name="patternInterface">One of the enumeration values.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		object result = null;
		switch (patternInterface)
		{
		case PatternInterface.Scroll:
		{
			FlowDocumentScrollViewer flowDocumentScrollViewer = (FlowDocumentScrollViewer)base.Owner;
			if (flowDocumentScrollViewer.ScrollViewer != null)
			{
				AutomationPeer automationPeer = UIElementAutomationPeer.CreatePeerForElement(flowDocumentScrollViewer.ScrollViewer);
				if (automationPeer != null && automationPeer is IScrollProvider)
				{
					automationPeer.EventsSource = this;
					result = automationPeer;
				}
			}
			break;
		}
		case PatternInterface.Text:
			GetChildren();
			if (_documentPeer != null)
			{
				_documentPeer.EventsSource = this;
				result = _documentPeer.GetPattern(patternInterface);
			}
			break;
		case PatternInterface.SynchronizedInput:
			result = base.GetPattern(patternInterface);
			break;
		}
		return result;
	}

	/// <summary>Gets the collection of child elements of the <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.FlowDocumentScrollViewerAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.</summary>
	/// <returns>The collection of child elements.</returns>
	protected override List<AutomationPeer> GetChildrenCore()
	{
		List<AutomationPeer> list = base.GetChildrenCore();
		if (!(base.Owner is IFlowDocumentViewer))
		{
			FlowDocument document = ((FlowDocumentScrollViewer)base.Owner).Document;
			if (document != null)
			{
				AutomationPeer automationPeer = ContentElementAutomationPeer.CreatePeerForElement(document);
				if (_documentPeer != automationPeer)
				{
					if (_documentPeer != null)
					{
						_documentPeer.OnDisconnected();
					}
					_documentPeer = automationPeer as DocumentAutomationPeer;
				}
				if (automationPeer != null)
				{
					if (list == null)
					{
						list = new List<AutomationPeer>();
					}
					list.Add(automationPeer);
				}
			}
		}
		return list;
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.FlowDocumentScrollViewerAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Document" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Document;
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.FlowDocumentScrollViewerAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "FlowDocumentScrollViewer".</returns>
	protected override string GetClassNameCore()
	{
		return "FlowDocumentScrollViewer";
	}
}
