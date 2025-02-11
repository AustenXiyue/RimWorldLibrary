using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.GridSplitter" /> types to UI Automation.</summary>
public class GridSplitterAutomationPeer : ThumbAutomationPeer, ITransformProvider
{
	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the element can be moved; otherwise false.</returns>
	bool ITransformProvider.CanMove => true;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the element can be resized; otherwise false.</returns>
	bool ITransformProvider.CanResize => false;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the element can rotate; otherwise false.</returns>
	bool ITransformProvider.CanRotate => false;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.GridSplitterAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.GridSplitter" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.GridSplitterAutomationPeer" />.</param>
	public GridSplitterAutomationPeer(GridSplitter owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.GridSplitter" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.GridSplitterAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "GridSplitter".</returns>
	protected override string GetClassNameCore()
	{
		return "GridSplitter";
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Automation.Peers.GridSplitterAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />. </summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.Transform" />, this method returns a this pointer; otherwise this method returns null.</returns>
	/// <param name="patternInterface">A value in the enumeration.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		if (patternInterface == PatternInterface.Transform)
		{
			return this;
		}
		return base.GetPattern(patternInterface);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="x"> Absolute screen coordinates of the left side of the control.</param>
	/// <param name="y"> Absolute screen coordinates of the top of the control.</param>
	void ITransformProvider.Move(double x, double y)
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		if (double.IsInfinity(x) || double.IsNaN(x))
		{
			throw new ArgumentOutOfRangeException("x");
		}
		if (double.IsInfinity(y) || double.IsNaN(y))
		{
			throw new ArgumentOutOfRangeException("y");
		}
		((GridSplitter)base.Owner).KeyboardMoveSplitter(x, y);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="width"> The new width of the window, in pixels.</param>
	/// <param name="height"> The new height of the window, in pixels.</param>
	void ITransformProvider.Resize(double width, double height)
	{
		throw new InvalidOperationException(SR.UIA_OperationCannotBePerformed);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="degrees">The number of degrees to rotate the control.</param>
	void ITransformProvider.Rotate(double degrees)
	{
		throw new InvalidOperationException(SR.UIA_OperationCannotBePerformed);
	}
}
