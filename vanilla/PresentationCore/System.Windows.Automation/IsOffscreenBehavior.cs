namespace System.Windows.Automation;

/// <summary>Specifies how the <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsOffscreen" /> property is determined.</summary>
public enum IsOffscreenBehavior
{
	/// <summary>
	///   <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsOffscreen" /> is calculated based on the <see cref="P:System.Windows.UIElement.IsVisible" /> property.</summary>
	Default,
	/// <summary>
	///   <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsOffscreen" /> is false.</summary>
	Onscreen,
	/// <summary>
	///   <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsOffscreen" /> is true.</summary>
	Offscreen,
	/// <summary>
	///   <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsOffscreen" /> is calculated based on clip regions.</summary>
	FromClip
}
