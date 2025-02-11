using System.Windows;

namespace MS.Internal.Ink;

internal abstract class ClipboardData
{
	internal bool CopyToDataObject(IDataObject dataObject)
	{
		if (CanCopy())
		{
			DoCopy(dataObject);
			return true;
		}
		return false;
	}

	internal void PasteFromDataObject(IDataObject dataObject)
	{
		if (CanPaste(dataObject))
		{
			DoPaste(dataObject);
		}
	}

	internal abstract bool CanPaste(IDataObject dataObject);

	protected abstract bool CanCopy();

	protected abstract void DoCopy(IDataObject dataObject);

	protected abstract void DoPaste(IDataObject dataObject);
}
