using System.Collections.Generic;

namespace System.Windows.Shell;

/// <summary>Provides data for the <see cref="E:System.Windows.Shell.JumpList.JumpItemsRejected" /> event.</summary>
public sealed class JumpItemsRejectedEventArgs : EventArgs
{
	/// <summary>Gets the list of Jump List items that could not be added to the Jump List by the Windows shell.</summary>
	/// <returns>The list of Jump List items that could not be added to the Jump List by the Windows shell.</returns>
	public IList<JumpItem> RejectedItems { get; private set; }

	/// <summary>Gets the list of reasons why the rejected Jump List items could not be added to the Jump List.</summary>
	/// <returns>The list of reasons why the rejected Jump List items could not be added to the Jump List.</returns>
	public IList<JumpItemRejectionReason> RejectionReasons { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Shell.JumpItemsRejectedEventArgs" /> class.</summary>
	public JumpItemsRejectedEventArgs()
		: this(null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Shell.JumpItemsRejectedEventArgs" /> class that has the specified parameters.</summary>
	/// <param name="rejectedItems">The list of Jump List items that could not be added to the Jump List by the Windows shell.</param>
	/// <param name="reasons">The list of reasons why the rejected Jump List items could not be added to the Jump List.</param>
	/// <exception cref="T:System.ArgumentException">The count of <paramref name="rejectedItems " />does not equal the count of rejection <paramref name="reasons" />.</exception>
	public JumpItemsRejectedEventArgs(IList<JumpItem> rejectedItems, IList<JumpItemRejectionReason> reasons)
	{
		if ((rejectedItems == null && reasons != null) || (reasons == null && rejectedItems != null) || (rejectedItems != null && reasons != null && rejectedItems.Count != reasons.Count))
		{
			throw new ArgumentException(SR.JumpItemsRejectedEventArgs_CountMismatch);
		}
		if (rejectedItems != null)
		{
			RejectedItems = new List<JumpItem>(rejectedItems).AsReadOnly();
			RejectionReasons = new List<JumpItemRejectionReason>(reasons).AsReadOnly();
		}
		else
		{
			RejectedItems = new List<JumpItem>().AsReadOnly();
			RejectionReasons = new List<JumpItemRejectionReason>().AsReadOnly();
		}
	}
}
