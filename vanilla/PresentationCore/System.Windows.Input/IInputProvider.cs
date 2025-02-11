using System.Windows.Media;

namespace System.Windows.Input;

internal interface IInputProvider
{
	bool ProvidesInputForRootVisual(Visual v);

	void NotifyDeactivate();
}
