using System.Windows.Input;

namespace MS.Internal.Ink;

internal interface IStylusEditing
{
	void AddStylusPoints(StylusPointCollection stylusPoints, bool userInitiated);
}
