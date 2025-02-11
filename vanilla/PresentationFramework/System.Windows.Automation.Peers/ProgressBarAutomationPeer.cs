using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.ProgressBar" /> types to UI Automation.</summary>
public class ProgressBarAutomationPeer : RangeBaseAutomationPeer, IRangeValueProvider
{
	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the value is read-only; otherwise false.</returns>
	bool IRangeValueProvider.IsReadOnly => true;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The large-change value.</returns>
	double IRangeValueProvider.LargeChange => double.NaN;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The small-change value.</returns>
	double IRangeValueProvider.SmallChange => double.NaN;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.ProgressBarAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.ProgressBar" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ProgressBarAutomationPeer" />.</param>
	public ProgressBarAutomationPeer(ProgressBar owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.ProgressBar" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ProgressBarAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "ProgressBar".</returns>
	protected override string GetClassNameCore()
	{
		return "ProgressBar";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.ProgressBar" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ProgressBarAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.ProgressBar" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.ProgressBar;
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Controls.ProgressBar" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ProgressBarAutomationPeer" />.</summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.RangeValue" /> and <see cref="P:System.Windows.Controls.ProgressBar.IsIndeterminate" /> is true, this method returns the current instance of the <see cref="T:System.Windows.Automation.Peers.ProgressBarAutomationPeer" />; otherwise, this method returns null.</returns>
	/// <param name="patternInterface">A value in the enumeration.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		if (patternInterface == PatternInterface.RangeValue && ((ProgressBar)base.Owner).IsIndeterminate)
		{
			return null;
		}
		return base.GetPattern(patternInterface);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="val"> The value to set.</param>
	void IRangeValueProvider.SetValue(double val)
	{
		throw new InvalidOperationException(SR.ProgressBarReadOnly);
	}
}
