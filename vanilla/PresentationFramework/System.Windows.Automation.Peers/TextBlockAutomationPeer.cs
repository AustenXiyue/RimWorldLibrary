using System.Collections.Generic;
using System.Windows.Controls;
using MS.Internal.Documents;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.TextBlock" /> types to UI Automation.</summary>
public class TextBlockAutomationPeer : FrameworkElementAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.TextBlockAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.TextBlock" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TextBlockAutomationPeer" />.</param>
	public TextBlockAutomationPeer(TextBlock owner)
		: base(owner)
	{
	}

	/// <summary>Gets the collection of child elements of the <see cref="T:System.Windows.Controls.TextBlock" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TextBlockAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.</summary>
	/// <returns>A collection of child elements, or null if the <see cref="T:System.Windows.Controls.TextBlock" /> is empty.</returns>
	protected override List<AutomationPeer> GetChildrenCore()
	{
		List<AutomationPeer> result = null;
		TextBlock textBlock = (TextBlock)base.Owner;
		if (textBlock.HasComplexContent)
		{
			result = TextContainerHelper.GetAutomationPeersFromRange(textBlock.TextContainer.Start, textBlock.TextContainer.End, null);
		}
		return result;
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.TextBlock" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TextBlockAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>
	///   <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Text" />
	/// </returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Text;
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.TextBlock" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TextBlockAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains the word "TextBlock".</returns>
	protected override string GetClassNameCore()
	{
		return "TextBlock";
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Controls.TextBlock" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TextBlockAutomationPeer" /> is something that the end user would understand as being interactive or as contributing to the logical structure of the control in the GUI. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsControlElement" />.</summary>
	/// <returns>false if the element is part of a template; otherwise true.</returns>
	protected override bool IsControlElementCore()
	{
		DependencyObject templatedParent = ((TextBlock)base.Owner).TemplatedParent;
		if (templatedParent == null || templatedParent is ContentPresenter)
		{
			return base.IsControlElementCore();
		}
		return false;
	}
}
