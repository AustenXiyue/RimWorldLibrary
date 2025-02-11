using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using MS.Win32;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Interop.HwndSource" /> types to UI Automation.</summary>
public class GenericRootAutomationPeer : UIElementAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.GenericRootAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.GenericRootAutomationPeer" />.</param>
	public GenericRootAutomationPeer(UIElement owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.GenericRootAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "Pane".</returns>
	protected override string GetClassNameCore()
	{
		return "Pane";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.GenericRootAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Pane" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Pane;
	}

	/// <summary>Gets the text label of the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.GenericRootAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetName" />.</summary>
	/// <returns>The string that contains the label.</returns>
	protected override string GetNameCore()
	{
		string text = base.GetNameCore();
		if (text == string.Empty)
		{
			nint hwnd = base.Hwnd;
			if (hwnd != IntPtr.Zero)
			{
				try
				{
					StringBuilder stringBuilder = new StringBuilder(512);
					MS.Win32.UnsafeNativeMethods.GetWindowText(new HandleRef(null, hwnd), stringBuilder, stringBuilder.Capacity);
					text = stringBuilder.ToString();
				}
				catch (Win32Exception)
				{
				}
				if (text == null)
				{
					text = string.Empty;
				}
			}
		}
		return text;
	}

	/// <summary>Gets the <see cref="T:System.Windows.Rect" /> that represents the bounding rectangle of the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.GenericRootAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetBoundingRectangle" />.</summary>
	/// <returns>The bounding rectangle.</returns>
	protected override Rect GetBoundingRectangleCore()
	{
		Rect result = new Rect(0.0, 0.0, 0.0, 0.0);
		nint hwnd = base.Hwnd;
		if (hwnd != IntPtr.Zero)
		{
			MS.Win32.NativeMethods.RECT rect = new MS.Win32.NativeMethods.RECT(0, 0, 0, 0);
			try
			{
				SafeNativeMethods.GetWindowRect(new HandleRef(null, hwnd), ref rect);
			}
			catch (Win32Exception)
			{
			}
			result = new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
		}
		return result;
	}
}
