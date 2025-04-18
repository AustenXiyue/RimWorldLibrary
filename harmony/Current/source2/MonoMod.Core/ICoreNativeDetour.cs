using System;

namespace MonoMod.Core;

internal interface ICoreNativeDetour : ICoreDetourBase, IDisposable
{
	IntPtr Source { get; }

	IntPtr Target { get; }

	bool HasOrigEntrypoint { get; }

	IntPtr OrigEntrypoint { get; }
}
