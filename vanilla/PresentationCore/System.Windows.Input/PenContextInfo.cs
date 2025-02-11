using MS.Internal;
using MS.Win32.Penimc;

namespace System.Windows.Input;

internal struct PenContextInfo
{
	public SecurityCriticalDataClass<IPimcContext3> PimcContext;

	public SecurityCriticalDataClass<nint> CommHandle;

	public int ContextId;

	public uint WispContextKey;
}
