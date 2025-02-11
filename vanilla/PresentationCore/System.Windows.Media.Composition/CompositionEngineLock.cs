using System.Runtime.InteropServices;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Composition;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct CompositionEngineLock : IDisposable
{
	internal static CompositionEngineLock Acquire()
	{
		UnsafeNativeMethods.MilCoreApi.EnterCompositionEngineLock();
		return default(CompositionEngineLock);
	}

	public void Dispose()
	{
		UnsafeNativeMethods.MilCoreApi.ExitCompositionEngineLock();
	}
}
