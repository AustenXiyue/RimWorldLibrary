using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Threading;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.Button" /> types to UI Automation.</summary>
public class ButtonAutomationPeer : ButtonBaseAutomationPeer, IInvokeProvider
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.ButtonAutomationPeer" /> class.</summary>
	/// <param name="owner">The element associated with this automation peer.</param>
	public ButtonAutomationPeer(Button owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the control that is associated with this UI Automation peer.</summary>
	/// <returns>A string that contains "Button".</returns>
	protected override string GetClassNameCore()
	{
		return "Button";
	}

	/// <summary>Gets the control type of the element that is associated with the UI Automation peer.</summary>
	/// <returns>
	///   <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Button" />.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Button;
	}

	/// <summary>Gets the object that supports the specified control pattern of the element that is associated with this automation peer.</summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.Invoke" />, this method returns a this pointer, otherwise this method returns null.</returns>
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
		base.Dispatcher.BeginInvoke(DispatcherPriority.Input, (DispatcherOperationCallback)delegate
		{
			((Button)base.Owner).AutomationButtonBaseClick();
			return (object)null;
		}, null);
	}
}
