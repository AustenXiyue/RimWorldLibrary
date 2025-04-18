using System;

namespace MonoMod.Core.Platforms;

internal interface INativeExceptionHelper
{
	GetExceptionSlot GetExceptionSlot { get; }

	IntPtr CreateNativeToManagedHelper(IntPtr target, out IDisposable? handle);

	IntPtr CreateManagedToNativeHelper(IntPtr target, out IDisposable? handle);
}
