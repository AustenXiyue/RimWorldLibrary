using System.Windows.Input.StylusPointer;
using System.Windows.Input.StylusWisp;

namespace System.Windows.Input.StylusPlugIns;

internal abstract class StylusPlugInCollectionBase
{
	internal StylusPlugInCollection Wrapper { get; private set; }

	internal abstract bool IsActiveForInput { get; }

	internal abstract object SyncRoot { get; }

	internal static StylusPlugInCollectionBase Create(StylusPlugInCollection wrapper)
	{
		StylusPlugInCollectionBase stylusPlugInCollectionBase = ((!StylusLogic.IsPointerStackEnabled) ? ((StylusPlugInCollectionBase)new WispStylusPlugInCollection()) : ((StylusPlugInCollectionBase)new PointerStylusPlugInCollection()));
		stylusPlugInCollectionBase.Wrapper = wrapper;
		return stylusPlugInCollectionBase;
	}

	internal abstract void UpdateState(UIElement element);

	internal abstract void Unhook();
}
