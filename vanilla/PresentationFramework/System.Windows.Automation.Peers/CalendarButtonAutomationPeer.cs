using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.Primitives.CalendarButton" /> types to UI Automation.</summary>
public sealed class CalendarButtonAutomationPeer : FrameworkElementAutomationPeer
{
	private bool IsDayButton => base.Owner is CalendarDayButton;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.CalendarButtonAutomationPeer" /> class. </summary>
	/// <param name="owner">The element associated with this automation peer.</param>
	public CalendarButtonAutomationPeer(Button owner)
		: base(owner)
	{
	}

	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Button;
	}

	protected override string GetClassNameCore()
	{
		return base.Owner.GetType().Name;
	}

	protected override string GetLocalizedControlTypeCore()
	{
		if (!IsDayButton)
		{
			return SR.CalendarAutomationPeer_CalendarButtonLocalizedControlType;
		}
		return SR.CalendarAutomationPeer_DayButtonLocalizedControlType;
	}
}
