using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> types to UI Automation.</summary>
public class ScrollBarAutomationPeer : RangeBaseAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.ScrollBarAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ScrollBarAutomationPeer" />.</param>
	public ScrollBarAutomationPeer(ScrollBar owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ScrollBarAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains the word "ScrollBar".</returns>
	protected override string GetClassNameCore()
	{
		return "ScrollBar";
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ScrollBarAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>
	///   <see cref="F:System.Windows.Automation.Peers.AutomationControlType.ScrollBar" />.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.ScrollBar;
	}

	/// <summary>Gets a value that indicates whether the element that is associated with this automation peer contains data that is presented to the user. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsContentElement" />.</summary>
	/// <returns>false in all cases.</returns>
	protected override bool IsContentElementCore()
	{
		return false;
	}

	/// <summary>Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClickablePoint" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Point" /> that has values of <see cref="F:System.Double.NaN" />, <see cref="F:System.Double.NaN" />; the only clickable points in a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> are the child elements.</returns>
	protected override Point GetClickablePointCore()
	{
		return new Point(double.NaN, double.NaN);
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ScrollBarAutomationPeer" /> is laid out in a specific direction. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetOrientation" />.</summary>
	/// <returns>
	///   <see cref="F:System.Windows.Automation.Peers.AutomationOrientation.Horizontal" /> or <see cref="F:System.Windows.Automation.Peers.AutomationOrientation.Vertical" />, depending on the orientation of the <see cref="T:System.Windows.Controls.Primitives.ScrollBar" />.</returns>
	protected override AutomationOrientation GetOrientationCore()
	{
		if (((ScrollBar)base.Owner).Orientation != 0)
		{
			return AutomationOrientation.Vertical;
		}
		return AutomationOrientation.Horizontal;
	}

	internal override void SetValueCore(double val)
	{
		double horizontalPercent = -1.0;
		double verticalPercent = -1.0;
		ScrollBar scrollBar = base.Owner as ScrollBar;
		if (!(scrollBar.TemplatedParent is ScrollViewer scrollViewer))
		{
			base.SetValueCore(val);
			return;
		}
		if (scrollBar.Orientation == Orientation.Horizontal)
		{
			horizontalPercent = val / (scrollViewer.ExtentWidth - scrollViewer.ViewportWidth) * 100.0;
		}
		else
		{
			verticalPercent = val / (scrollViewer.ExtentHeight - scrollViewer.ViewportHeight) * 100.0;
		}
		((IScrollProvider)(UIElementAutomationPeer.FromElement(scrollViewer) as ScrollViewerAutomationPeer)).SetScrollPercent(horizontalPercent, verticalPercent);
	}
}
