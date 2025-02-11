using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.GridViewColumnHeader" /> types to UI Automation.</summary>
public class GridViewColumnHeaderAutomationPeer : FrameworkElementAutomationPeer, IInvokeProvider, ITransformProvider
{
	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the element can be moved; otherwise false.</returns>
	bool ITransformProvider.CanMove => false;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the element can be resized; otherwise false.</returns>
	bool ITransformProvider.CanResize => true;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the element can rotate; otherwise false.</returns>
	bool ITransformProvider.CanRotate => false;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.GridViewColumnHeaderAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.GridViewColumnHeader" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.GridViewColumnHeaderAutomationPeer" />.</param>
	public GridViewColumnHeaderAutomationPeer(GridViewColumnHeader owner)
		: base(owner)
	{
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.GridViewColumnHeader" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.GridViewColumnHeaderAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.HeaderItem" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.HeaderItem;
	}

	/// <summary>Gets a value that indicates whether the element that is associated with this automation peer contains data that is presented to the user. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsContentElement" />.</summary>
	/// <returns>false in all cases.</returns>
	protected override bool IsContentElementCore()
	{
		return false;
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.GridViewColumnHeader" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.GridViewColumnHeaderAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "GridViewColumnHeader".</returns>
	protected override string GetClassNameCore()
	{
		return "GridViewColumnHeader";
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Controls.GridViewColumnHeader" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.GridViewColumnHeaderAutomationPeer" />. </summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.Transform" /> or <see cref="F:System.Windows.Automation.Peers.PatternInterface.Invoke" />, this method returns a this pointer; otherwise this method returns null.</returns>
	/// <param name="patternInterface">One of the enumeration values.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		if (patternInterface == PatternInterface.Invoke || patternInterface == PatternInterface.Transform)
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
		((GridViewColumnHeader)base.Owner).AutomationClick();
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="x"> Absolute screen coordinates of the left side of the control.</param>
	/// <param name="y"> Absolute screen coordinates of the top of the control.</param>
	void ITransformProvider.Move(double x, double y)
	{
		throw new InvalidOperationException(SR.UIA_OperationCannotBePerformed);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="width"> The new width of the window, in pixels.</param>
	/// <param name="height"> The new height of the window, in pixels.</param>
	void ITransformProvider.Resize(double width, double height)
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		if (width < 0.0)
		{
			throw new ArgumentOutOfRangeException("width");
		}
		if (height < 0.0)
		{
			throw new ArgumentOutOfRangeException("height");
		}
		if (base.Owner is GridViewColumnHeader gridViewColumnHeader)
		{
			if (gridViewColumnHeader.Column != null)
			{
				gridViewColumnHeader.Column.Width = width;
			}
			gridViewColumnHeader.Height = height;
		}
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="degrees"> The number of degrees to rotate the control.</param>
	void ITransformProvider.Rotate(double degrees)
	{
		throw new InvalidOperationException(SR.UIA_OperationCannotBePerformed);
	}
}
