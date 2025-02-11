using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.Primitives.DocumentPageView" /> types to UI Automation.</summary>
public class DocumentPageViewAutomationPeer : FrameworkElementAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.DocumentPageViewAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.Primitives.DocumentPageView" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.DocumentPageViewAutomationPeer" />.</param>
	public DocumentPageViewAutomationPeer(DocumentPageView owner)
		: base(owner)
	{
	}

	/// <summary>Gets the collection of child elements of the <see cref="T:System.Windows.Controls.Primitives.DocumentPageView" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.DocumentPageViewAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.</summary>
	/// <returns>The collection of child elements of the <see cref="T:System.Windows.Controls.Primitives.DocumentPageView" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.DocumentPageViewAutomationPeer" />.</returns>
	protected override List<AutomationPeer> GetChildrenCore()
	{
		return null;
	}

	/// <summary>Gets the string that uniquely identifies the <see cref="T:System.Windows.Controls.Primitives.DocumentPageView" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.DocumentPageViewAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationId" />.</summary>
	/// <returns>A string that contains the automation identifier.</returns>
	protected override string GetAutomationIdCore()
	{
		string result = string.Empty;
		DocumentPageView documentPageView = (DocumentPageView)base.Owner;
		if (!string.IsNullOrEmpty(documentPageView.Name))
		{
			result = documentPageView.Name;
		}
		else if (documentPageView.PageNumber >= 0 && documentPageView.PageNumber < int.MaxValue)
		{
			result = string.Format(CultureInfo.InvariantCulture, "DocumentPage{0}", documentPageView.PageNumber + 1);
		}
		return result;
	}
}
