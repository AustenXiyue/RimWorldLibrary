using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace System.Windows.Automation.Peers;

/// <summary>Represents a base class for exposing elements derived from <see cref="T:System.Windows.Controls.Primitives.ButtonBase" /> to UI Automation.</summary>
public abstract class ButtonBaseAutomationPeer : FrameworkElementAutomationPeer
{
	/// <summary>Provides initialization for base class values when called by the constructor of a derived class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.Primitives.ButtonBase" /> that is associated with this peer.</param>
	protected ButtonBaseAutomationPeer(ButtonBase owner)
		: base(owner)
	{
	}

	/// <summary>Gets the accelerator key for the element associated with this <see cref="T:System.Windows.Automation.Peers.ButtonBaseAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAcceleratorKey" />.</summary>
	/// <returns>A string containing the accelerator key.</returns>
	protected override string GetAcceleratorKeyCore()
	{
		string text = base.GetAcceleratorKeyCore();
		if (text == string.Empty && ((ButtonBase)base.Owner).Command is RoutedUICommand routedUICommand && !string.IsNullOrEmpty(routedUICommand.Text))
		{
			text = routedUICommand.Text;
		}
		return text;
	}

	/// <summary>Gets the <see cref="P:System.Windows.Automation.AutomationProperties.AutomationId" /> for the element associated with this <see cref="T:System.Windows.Automation.Peers.ButtonBaseAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationId" />.</summary>
	/// <returns>The string that contains the <see cref="P:System.Windows.Automation.AutomationProperties.AutomationId" />.</returns>
	protected override string GetAutomationIdCore()
	{
		string text = base.GetAutomationIdCore();
		if (string.IsNullOrEmpty(text) && ((ButtonBase)base.Owner).Command is RoutedCommand { Name: var name } && !string.IsNullOrEmpty(name))
		{
			text = name;
		}
		return text ?? string.Empty;
	}

	/// <summary>Gets the name of the class of the element associated with this <see cref="T:System.Windows.Automation.Peers.ButtonBaseAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetName" />.</summary>
	/// <returns>A string that contains the class name, minus the accelerator key.</returns>
	protected override string GetNameCore()
	{
		string text = base.GetNameCore();
		ButtonBase buttonBase = (ButtonBase)base.Owner;
		if (!string.IsNullOrEmpty(text))
		{
			if (buttonBase.Content is string)
			{
				text = AccessText.RemoveAccessKeyMarker(text);
			}
		}
		else if (buttonBase.Command is RoutedUICommand routedUICommand && !string.IsNullOrEmpty(routedUICommand.Text))
		{
			text = routedUICommand.Text;
		}
		return text;
	}
}
