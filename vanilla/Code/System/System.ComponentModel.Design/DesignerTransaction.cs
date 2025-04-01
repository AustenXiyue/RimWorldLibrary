using System.Security.Permissions;

namespace System.ComponentModel.Design;

/// <summary>Provides a way to group a series of design-time actions to improve performance and enable most types of changes to be undone.</summary>
[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public abstract class DesignerTransaction : IDisposable
{
	private bool committed;

	private bool canceled;

	private bool suppressedFinalization;

	private string desc;

	/// <summary>Gets a value indicating whether the transaction was canceled.</summary>
	/// <returns>true if the transaction was canceled; otherwise, false.</returns>
	public bool Canceled => canceled;

	/// <summary>Gets a value indicating whether the transaction was committed.</summary>
	/// <returns>true if the transaction was committed; otherwise, false.</returns>
	public bool Committed => committed;

	/// <summary>Gets a description for the transaction.</summary>
	/// <returns>A description for the transaction.</returns>
	public string Description => desc;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.DesignerTransaction" /> class with no description.</summary>
	protected DesignerTransaction()
		: this("")
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.DesignerTransaction" /> class using the specified transaction description.</summary>
	/// <param name="description">A description for this transaction. </param>
	protected DesignerTransaction(string description)
	{
		desc = description;
	}

	/// <summary>Cancels the transaction and attempts to roll back the changes made by the events of the transaction.</summary>
	public void Cancel()
	{
		if (!canceled && !committed)
		{
			canceled = true;
			GC.SuppressFinalize(this);
			suppressedFinalization = true;
			OnCancel();
		}
	}

	/// <summary>Commits this transaction.</summary>
	public void Commit()
	{
		if (!committed && !canceled)
		{
			committed = true;
			GC.SuppressFinalize(this);
			suppressedFinalization = true;
			OnCommit();
		}
	}

	/// <summary>Raises the Cancel event.</summary>
	protected abstract void OnCancel();

	/// <summary>Performs the actual work of committing a transaction.</summary>
	protected abstract void OnCommit();

	/// <summary>Releases the resources associated with this object. This override commits this transaction if it was not already committed.</summary>
	~DesignerTransaction()
	{
		Dispose(disposing: false);
	}

	/// <summary>Releases all resources used by the <see cref="T:System.ComponentModel.Design.DesignerTransaction" />. </summary>
	void IDisposable.Dispose()
	{
		Dispose(disposing: true);
		if (!suppressedFinalization)
		{
			GC.SuppressFinalize(this);
		}
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.ComponentModel.Design.DesignerTransaction" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		Cancel();
	}
}
