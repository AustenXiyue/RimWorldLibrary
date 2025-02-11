using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents;
using MS.Internal.Automation;
using MS.Internal.Documents;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.RichTextBox" /> types to UI Automation.</summary>
public class RichTextBoxAutomationPeer : TextAutomationPeer
{
	private TextAdaptor _textPattern;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.RichTextBoxAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.RichTextBox" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.RichTextBoxAutomationPeer" />.</param>
	public RichTextBoxAutomationPeer(RichTextBox owner)
		: base(owner)
	{
		_textPattern = new TextAdaptor(this, owner.TextContainer);
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.RichTextBox" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.RichTextBoxAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "RichTextBox".</returns>
	protected override string GetClassNameCore()
	{
		return "RichTextBox";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.RichTextBox" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.RichTextBoxAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Document" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Document;
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Controls.RichTextBox" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.RichTextBoxAutomationPeer" />.</summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.Text" />, this method returns an <see cref="T:System.Windows.Automation.Provider.ITextProvider" /> reference. If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.Scroll" />, this method returns a new <see cref="T:System.Windows.Automation.Peers.ScrollViewerAutomationPeer" />.  </returns>
	/// <param name="patternInterface">A value in the enumeration.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		object obj = null;
		RichTextBox richTextBox = (RichTextBox)base.Owner;
		switch (patternInterface)
		{
		case PatternInterface.Text:
			if (_textPattern == null)
			{
				_textPattern = new TextAdaptor(this, richTextBox.TextContainer);
			}
			return _textPattern;
		case PatternInterface.Scroll:
			if (richTextBox.ScrollViewer != null)
			{
				obj = richTextBox.ScrollViewer.CreateAutomationPeer();
				((AutomationPeer)obj).EventsSource = this;
			}
			break;
		default:
			obj = base.GetPattern(patternInterface);
			break;
		}
		return obj;
	}

	/// <summary>Gets the collection of child elements for <see cref="T:System.Windows.Controls.RichTextBox" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.RichTextBoxAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.</summary>
	/// <returns>The collection of child elements.</returns>
	protected override List<AutomationPeer> GetChildrenCore()
	{
		RichTextBox richTextBox = (RichTextBox)base.Owner;
		return TextContainerHelper.GetAutomationPeersFromRange(richTextBox.TextContainer.Start, richTextBox.TextContainer.End, null);
	}

	internal override List<AutomationPeer> GetAutomationPeersFromRange(ITextPointer start, ITextPointer end)
	{
		GetChildren();
		RichTextBox richTextBox = (RichTextBox)base.Owner;
		return TextContainerHelper.GetAutomationPeersFromRange(start, end, richTextBox.TextContainer.Start);
	}
}
