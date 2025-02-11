using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using MS.Internal;
using MS.Win32;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Window" /> types to UI Automation.</summary>
public class WindowAutomationPeer : FrameworkElementAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.WindowAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Window" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.WindowAutomationPeer" />.</param>
	public WindowAutomationPeer(Window owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Window" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.WindowAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains the word "Window".</returns>
	protected override string GetClassNameCore()
	{
		return "Window";
	}

	/// <summary>Gets the text label of the <see cref="T:System.Windows.Window" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetName" />.</summary>
	/// <returns>A string that contains the <see cref="P:System.Windows.Automation.AutomationProperties.Name" /> or  <see cref="P:System.Windows.FrameworkElement.Name" /> of the <see cref="T:System.Windows.Window" />, or <see cref="F:System.String.Empty" /> if the name is null.</returns>
	protected override string GetNameCore()
	{
		string text = base.GetNameCore();
		if (text.Length == 0)
		{
			Window window = (Window)base.Owner;
			if (!window.IsSourceWindowNull)
			{
				try
				{
					StringBuilder stringBuilder = new StringBuilder(512);
					MS.Win32.UnsafeNativeMethods.GetWindowText(new HandleRef(null, window.CriticalHandle), stringBuilder, stringBuilder.Capacity);
					text = stringBuilder.ToString();
				}
				catch (Win32Exception)
				{
					text = window.Title;
				}
				if (text == null)
				{
					text = "";
				}
			}
		}
		return text;
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Window" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.WindowAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Window" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Window;
	}

	/// <summary>Gets the <see cref="T:System.Windows.Rect" /> representing the bounding rectangle of the <see cref="T:System.Windows.Window" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.WindowAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetBoundingRectangle" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Rect" /> that represents the screen coordinates of the <see cref="T:System.Windows.Window" />.</returns>
	protected override Rect GetBoundingRectangleCore()
	{
		Window window = (Window)base.Owner;
		Rect result = new Rect(0.0, 0.0, 0.0, 0.0);
		if (!window.IsSourceWindowNull)
		{
			MS.Win32.NativeMethods.RECT rect = new MS.Win32.NativeMethods.RECT(0, 0, 0, 0);
			nint criticalHandle = window.CriticalHandle;
			if (criticalHandle != IntPtr.Zero)
			{
				try
				{
					SafeNativeMethods.GetWindowRect(new HandleRef(null, criticalHandle), ref rect);
				}
				catch (Win32Exception)
				{
				}
			}
			result = new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
		}
		return result;
	}

	protected override bool IsDialogCore()
	{
		Window window = (Window)base.Owner;
		if (Helper.IsDefaultValue(AutomationProperties.IsDialogProperty, window))
		{
			return window.IsShowingAsDialog;
		}
		return AutomationProperties.GetIsDialog(window);
	}
}
