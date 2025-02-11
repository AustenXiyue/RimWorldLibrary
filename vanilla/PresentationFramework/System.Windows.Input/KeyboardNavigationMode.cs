namespace System.Windows.Input;

/// <summary>Specifies the possible values for changes in focus when logical and directional navigation occurs.</summary>
public enum KeyboardNavigationMode
{
	/// <summary>Each element receives keyboard focus, as long as it is a navigation stop.  Navigation leaves the containing element when an edge is reached.</summary>
	Continue,
	/// <summary>The container and all of its child elements as a whole receive focus only once. Either the first tree child or the or the last focused element in the group receives focus</summary>
	Once,
	/// <summary>Depending on the direction of the navigation, the focus returns to the first or the last item when the end or the beginning of the container is reached.  Focus cannot leave the container using logical navigation.</summary>
	Cycle,
	/// <summary>No keyboard navigation is allowed inside this container.</summary>
	None,
	/// <summary>Depending on the direction of the navigation, focus returns to the first or the last item when the end or the beginning of the container is reached, but does not move past the beginning or end of the container.</summary>
	Contained,
	/// <summary>Tab Indexes are considered on local subtree only inside this container and behave like <see cref="F:System.Windows.Input.KeyboardNavigationMode.Continue" /> after that.</summary>
	Local
}
