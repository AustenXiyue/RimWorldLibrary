namespace System.ComponentModel;

/// <summary>Provides functionality to commit or rollback changes to an object that is used as a data source.</summary>
public interface IEditableObject
{
	/// <summary>Begins an edit on an object.</summary>
	void BeginEdit();

	/// <summary>Pushes changes since the last <see cref="M:System.ComponentModel.IEditableObject.BeginEdit" /> or <see cref="M:System.ComponentModel.IBindingList.AddNew" /> call into the underlying object.</summary>
	void EndEdit();

	/// <summary>Discards changes since the last <see cref="M:System.ComponentModel.IEditableObject.BeginEdit" /> call.</summary>
	void CancelEdit();
}
