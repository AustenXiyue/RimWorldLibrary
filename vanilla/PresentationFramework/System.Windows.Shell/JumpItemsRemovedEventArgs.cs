using System.Collections.Generic;

namespace System.Windows.Shell;

/// <summary>Provides data for the <see cref="E:System.Windows.Shell.JumpList.JumpItemsRemovedByUser" /> event.</summary>
public sealed class JumpItemsRemovedEventArgs : EventArgs
{
	/// <summary>Gets the list of Jump List items that have been removed by the user since the <see cref="M:System.Windows.Shell.JumpList.Apply" /> method was last called.</summary>
	/// <returns>The list of Jump List items that have been removed by the user since the <see cref="M:System.Windows.Shell.JumpList.Apply" /> method was last called.</returns>
	public IList<JumpItem> RemovedItems { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Shell.JumpItemsRemovedEventArgs" /> class.</summary>
	public JumpItemsRemovedEventArgs()
		: this(null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Shell.JumpItemsRemovedEventArgs" /> class that has the specified parameters.</summary>
	/// <param name="removedItems">The list of Jump List items that have been removed by the user since <see cref="M:System.Windows.Shell.JumpList.Apply" /> was last called.</param>
	public JumpItemsRemovedEventArgs(IList<JumpItem> removedItems)
	{
		if (removedItems != null)
		{
			RemovedItems = new List<JumpItem>(removedItems).AsReadOnly();
		}
		else
		{
			RemovedItems = new List<JumpItem>().AsReadOnly();
		}
	}
}
