using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.Primitives.DocumentViewerBase" /> types to UI Automation.</summary>
public class DocumentViewerBaseAutomationPeer : FrameworkElementAutomationPeer
{
	private DocumentAutomationPeer _documentPeer;

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Automation.Peers.DocumentViewerBaseAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.Primitives.DocumentViewerBase" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.DocumentViewerBaseAutomationPeer" />.</param>
	public DocumentViewerBaseAutomationPeer(DocumentViewerBase owner)
		: base(owner)
	{
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Controls.Primitives.DocumentViewerBase" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.DocumentViewerBaseAutomationPeer" />. </summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.Text" />, this method returns an <see cref="T:System.Windows.Automation.Provider.ITextProvider" /> reference. </returns>
	/// <param name="patternInterface">One of the enumeration values.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		object result = null;
		if (patternInterface == PatternInterface.Text)
		{
			GetChildren();
			if (_documentPeer != null)
			{
				_documentPeer.EventsSource = this;
				result = _documentPeer.GetPattern(patternInterface);
			}
		}
		else
		{
			result = base.GetPattern(patternInterface);
		}
		return result;
	}

	/// <summary>Gets the collection of child elements of the <see cref="T:System.Windows.Controls.Primitives.DocumentViewerBase" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.DocumentViewerBaseAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.</summary>
	/// <returns>The collection of child elements.</returns>
	protected override List<AutomationPeer> GetChildrenCore()
	{
		List<AutomationPeer> list = base.GetChildrenCore();
		AutomationPeer documentAutomationPeer = GetDocumentAutomationPeer();
		if (_documentPeer != documentAutomationPeer)
		{
			if (_documentPeer != null)
			{
				_documentPeer.OnDisconnected();
			}
			_documentPeer = documentAutomationPeer as DocumentAutomationPeer;
		}
		if (documentAutomationPeer != null)
		{
			if (list == null)
			{
				list = new List<AutomationPeer>();
			}
			list.Add(documentAutomationPeer);
		}
		return list;
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.Primitives.DocumentViewerBase" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.DocumentViewerBaseAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Document" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Document;
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.Primitives.DocumentViewerBase" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.DocumentViewerBaseAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "DocumentViewer".</returns>
	protected override string GetClassNameCore()
	{
		return "DocumentViewer";
	}

	private AutomationPeer GetDocumentAutomationPeer()
	{
		AutomationPeer result = null;
		IDocumentPaginatorSource document = ((DocumentViewerBase)base.Owner).Document;
		if (document != null)
		{
			if (document is UIElement)
			{
				result = UIElementAutomationPeer.CreatePeerForElement((UIElement)document);
			}
			else if (document is ContentElement)
			{
				result = ContentElementAutomationPeer.CreatePeerForElement((ContentElement)document);
			}
		}
		return result;
	}
}
