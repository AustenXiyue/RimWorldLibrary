using System.Windows.Media;

namespace MS.Internal.AppModel;

internal interface INavigatorImpl
{
	void OnSourceUpdatedFromNavService(bool journalOrCancel);

	Visual FindRootViewer();
}
