using System.Runtime.InteropServices;
using MS.Win32;

namespace System.Windows.Input;

internal class DefaultTextStoreTextComposition : TextComposition
{
	internal DefaultTextStoreTextComposition(InputManager inputManager, IInputElement source, string text, TextCompositionAutoComplete autoComplete)
		: base(inputManager, source, text, autoComplete)
	{
	}

	public override void Complete()
	{
		MS.Win32.UnsafeNativeMethods.ITfContext transitoryContext = GetTransitoryContext();
		MS.Win32.UnsafeNativeMethods.ITfContextOwnerCompositionServices tfContextOwnerCompositionServices = transitoryContext as MS.Win32.UnsafeNativeMethods.ITfContextOwnerCompositionServices;
		MS.Win32.UnsafeNativeMethods.ITfCompositionView composition = GetComposition(transitoryContext);
		if (composition != null)
		{
			tfContextOwnerCompositionServices.TerminateComposition(composition);
			Marshal.ReleaseComObject(composition);
		}
		Marshal.ReleaseComObject(transitoryContext);
	}

	private MS.Win32.UnsafeNativeMethods.ITfContext GetTransitoryContext()
	{
		MS.Win32.UnsafeNativeMethods.ITfDocumentMgr transitoryDocumentManager = DefaultTextStore.Current.TransitoryDocumentManager;
		transitoryDocumentManager.GetBase(out var context);
		Marshal.ReleaseComObject(transitoryDocumentManager);
		return context;
	}

	private MS.Win32.UnsafeNativeMethods.ITfCompositionView GetComposition(MS.Win32.UnsafeNativeMethods.ITfContext context)
	{
		MS.Win32.UnsafeNativeMethods.ITfCompositionView[] array = new MS.Win32.UnsafeNativeMethods.ITfCompositionView[1];
		((MS.Win32.UnsafeNativeMethods.ITfContextComposition)context).EnumCompositions(out var enumView);
		enumView.Next(1, array, out var _);
		Marshal.ReleaseComObject(enumView);
		return array[0];
	}
}
