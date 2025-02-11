using System.Windows.Interop;

namespace MS.Internal.AppModel;

internal interface IProgressPage2 : IProgressPage
{
	void ShowProgressMessage(string message);
}
