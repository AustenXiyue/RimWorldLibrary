using System.Runtime.CompilerServices;
using System.Windows.Navigation;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Navigation.NavigationWindow" /> types to UI Automation.</summary>
public class NavigationWindowAutomationPeer : WindowAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.NavigationWindowAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Navigation.NavigationWindow" /> associated with this <see cref="T:System.Windows.Automation.Peers.NavigationWindowAutomationPeer" />.</param>
	public NavigationWindowAutomationPeer(NavigationWindow owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "NavigationWindow".</returns>
	protected override string GetClassNameCore()
	{
		return "NavigationWindow";
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void RaiseAsyncContentLoadedEvent(AutomationPeer peer, long bytesRead, long maxBytes)
	{
		double percentComplete = 0.0;
		AsyncContentLoadedState asyncContentState = AsyncContentLoadedState.Beginning;
		if (bytesRead > 0)
		{
			if (bytesRead < maxBytes)
			{
				percentComplete = ((maxBytes > 0) ? ((double)bytesRead * 100.0 / (double)maxBytes) : 0.0);
				asyncContentState = AsyncContentLoadedState.Progress;
			}
			else
			{
				percentComplete = 100.0;
				asyncContentState = AsyncContentLoadedState.Completed;
			}
		}
		peer.RaiseAsyncContentLoadedEvent(new AsyncContentLoadedEventArgs(asyncContentState, percentComplete));
	}
}
